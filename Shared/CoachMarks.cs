using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zebble
{
    public partial class CoachMarks : Canvas
    {
        List<Step> stepsList = new List<Step>();
        CancellationToken CancellationToken;
        bool SkipTapped;
        int Index;

        CoachMarksOverlay Overlay;
        TopButtonContainer TopButtons;
        BottomButtonContainer BottomButtons;
        NextButton Next;
        SkipButton Skip;
        BackButton Back;

        CurrentStep CurrentStep;

        TaskCompletionSource<bool> OnNextTapped;
        TaskCompletionSource<bool> OnSkipTapped;
        TaskCompletionSource<bool> OnBackTapped;

        public readonly AsyncEvent NextButtonTapped = new AsyncEvent();
        public readonly AsyncEvent SkipButtonTapped = new AsyncEvent();
        public readonly AsyncEvent BackButtonTapped = new AsyncEvent();

        public CoachMarksSettings Settings { get; }

        public bool IsCoaching;

        public CoachMarks() : this(new CoachMarksSettings()) { }

        public CoachMarks(CoachMarksSettings settings)
        {
            Settings = settings;

            Overlay = new CoachMarksOverlay();

            // Buttons
            Next = new NextButton(NextButtonTapped);
            Skip = new SkipButton(SkipButtonTapped);
            Back = new BackButton(BackButtonTapped);

            // Buttons stacks
            TopButtons = new TopButtonContainer();
            BottomButtons = new BottomButtonContainer();
        }

        public void CreateStep(string text, string elementId)
        {
            stepsList.Add(new Step(elementId)
            {
                Text = text
            });
        }

        public void CreateStep(string text, View element)
        {
            stepsList.Add(new Step(element)
            {
                Text = text
            });
        }

        public Task Show() => Show(CancellationToken.None);

        public Task Show(CancellationToken cancellationToken) =>
            Thread.UI.Run(() => DoShow(cancellationToken));

        async Task DoShow(CancellationToken cancellationToken)
        {
            if (IsCoaching)
                throw new InvalidOperationException("Coaching is under process.");

            CancellationToken = cancellationToken;
            SkipTapped = false;
            CurrentStep currentStep = null;

            try
            {
                // Create and show background which contains the next and skip button
                await AddButtons(cancellationToken);

                for (Index = 0; Index < stepsList.Count; Index++)
                {
                    var step = stepsList[Index];

                    FixButtonsConditions();

                    if (ShouldItTerminate()) return;

                    // Show the step by showing it text and element.
                    currentStep = await ShowStep(step);

                    if (ShouldItTerminate()) return;

                    // Wait for next, skip or time delay (If applicable) to continue.
                    if (Settings.MoveOnByTime)
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnBackTapped.Task, currentStep.OnPopOverClosed.Task, Task.Delay(Settings.Delay));
                    else
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnBackTapped.Task, currentStep.OnPopOverClosed.Task);

                    if (ShouldItTerminate()) return;

                    // Hide the step by showing it text and element.
                    await HideStep(currentStep);
                }
            }
            finally
            {
                await Task.WhenAll(RemoveButtons(), HideStep(currentStep));
            }
        }

        public Task Hide() => Thread.UI.Run(() => DoHide());

        void DoHide()
        {
            SkipTapped = true;
            OnSkipTapped?.TrySetResult(result: true);
        }

        void FixButtonsConditions()
        {
            Back.Visible = Index > 0;
            Next.Text = stepsList.Count - 1 == Index ? "Finish" : "Next";
        }

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;
        
        async Task HideStep(CurrentStep currentStep)
        {
            if (currentStep == null) return;

            await currentStep.Hide();
        }

        async Task<CurrentStep> ShowStep(Step step)
        {
            OnNextTapped = new TaskCompletionSource<bool>();
            OnSkipTapped = new TaskCompletionSource<bool>();
            OnBackTapped = new TaskCompletionSource<bool>();

            var currentStep = new CurrentStep(step.Element, step.Text, Settings);

            await currentStep.Show();

            return currentStep;
        }

        async Task AddButtons(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            IsCoaching = true;

            await Add(Overlay);
            await Add(TopButtons);
            await Add(BottomButtons);

            await AddButtons(TopButtons, Settings.TopButtons);
            await AddButtons(BottomButtons, Settings.BottomButtons);

            this.On(x => x.NextButtonTapped, () => OnNextTapped?.TrySetResult(result: true))
                .On(x => x.SkipButtonTapped, () => Hide())
                .On(x => x.BackButtonTapped, () =>
                {
                    Index -= 2;
                    OnBackTapped?.TrySetResult(result: true);
                });

            await Root.Add(this);
            await BringToFront();
        }

        async Task AddButtons(Stack stack, CoachMarksSettings.Buttons buttons)
        {
            if (buttons.HasFlag(CoachMarksSettings.Buttons.Skip))
                await stack.Add(Skip);

            if (buttons.HasFlag(CoachMarksSettings.Buttons.Back))
                await stack.Add(Back);

            if (buttons.HasFlag(CoachMarksSettings.Buttons.Next))
                await stack.Add(Next);
        }

        async Task RemoveButtons()
        {
            await Remove(Overlay);
            await Remove(TopButtons);
            await Remove(BottomButtons);

            IsCoaching = false;
        }
    }
}
