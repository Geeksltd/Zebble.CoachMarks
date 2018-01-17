using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zebble
{
    //public partial class CoachMarks2
    //{

    //    BackgroundControl Background;
    //    View MarksRoot;
    //    TaskCompletionSource<bool> OnPopOverClosed;
    //    TaskCompletionSource<bool> OnNextTapped;
    //    TaskCompletionSource<bool> OnSkipTapped;
    //    bool SkipTapped;
        
    //    public bool IsCoaching => Background != null;

    //    public Task Coach(Settings settings) => Coach(settings, CancellationToken.None);

    //    public async Task Coach(Settings settings, CancellationToken cancellationToken)
    //    {
    //        if (IsCoaching)
    //            throw new InvalidOperationException("Coaching is under process.");

    //        MarksRoot = settings.MarksRoot ?? View.Root;
    //        SkipTapped = false;

    //        int? zIndex = null;
    //        View currentStepElement = null;
    //        PopOver currentStepUserHelp = null;

    //        try
    //        {
    //            // Create and show background which contains the next and skip button
    //            await ShowBackround(cancellationToken);

    //            foreach (var step in settings.Steps)
    //            {
    //                if (ShouldItTerminate(cancellationToken)) return;

    //                // Show the step by showing it text and element.
    //                var temp = await ShowStep(step);

    //                currentStepElement = temp.Item1;
    //                currentStepUserHelp = temp.Item2;
    //                zIndex = temp.Item3;

    //                if (ShouldItTerminate(cancellationToken)) return;

    //                // Wait for next, skip or time delay (If applicable) to continue.
    //                if (settings.MoveOnByTime)
    //                    await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnPopOverClosed.Task, Task.Delay(settings.Delay));
    //                else
    //                    await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnPopOverClosed.Task);

    //                if (ShouldItTerminate(cancellationToken)) return;

    //                // Hide the step by showing it text and element.
    //                await HideStep(currentStepElement, currentStepUserHelp, zIndex);
    //            }
    //        }
    //        finally
    //        {
    //            await RemoveBackground();
    //            if (zIndex.HasValue)
    //            {
    //                currentStepElement.ZIndex(zIndex.Value);
    //                await currentStepUserHelp?.Hide();
    //            }
    //        }
    //    }

    //    bool ShouldItTerminate(CancellationToken cancellationToken)
    //    {
    //        return cancellationToken.IsCancellationRequested || SkipTapped;
    //    }

    //    async Task HideStep(View currentStepElement, PopOver currentStepUserHelp, int? defaultZIndex)
    //    {
    //        if (defaultZIndex.HasValue)
    //            currentStepElement.ZIndex(defaultZIndex.Value);

    //        await currentStepUserHelp?.Hide();
    //    }

    //    async Task<Tuple<View, PopOver, int>> ShowStep(Step step)
    //    {
    //        var element = MarksRoot.AllDescendents().FirstOrDefault(v => v.Id == step.ElementId);
            
    //        var zIndex = element.ZIndex;
            
    //        await element.BringToFront();
            
    //        OnPopOverClosed = new TaskCompletionSource<bool>();
    //        OnNextTapped = new TaskCompletionSource<bool>();
    //        OnSkipTapped = new TaskCompletionSource<bool>();

    //        var helpOverlay = await element.ShowPopOver(step.Text);

    //        helpOverlay.On(x => x.OnHide, () =>
    //        {
    //            if (!OnPopOverClosed.Task.IsCompleted)
    //                OnPopOverClosed.SetResult(result: true);
    //        });

    //        await helpOverlay.BringToFront();

    //        return Tuple.Create(element, helpOverlay, zIndex);
    //    }

    //    async Task RemoveBackground()
    //    {
    //        await Background.RemoveSelf();
    //        Background = null;
    //    }

    //    async Task ShowBackround(CancellationToken cancellationToken)
    //    {
    //        if (cancellationToken.IsCancellationRequested) return;

    //        Background = new BackgroundControl(MarksRoot);
            
    //        Background.Y(0);
    //        Background.X(0);
    //        Background.Height.BindTo(MarksRoot.Height);
    //        Background.Width.BindTo(MarksRoot.Width);

    //        Background.On(x => x.RightButtonTapped, () => OnNextTapped?.SetResult(result: true))
    //            .On(x => x.LeftButtonTapped, () =>
    //            {
    //                SkipTapped = true;
    //                OnSkipTapped?.SetResult(result: true);
    //            });

    //        await MarksRoot.Add(Background);
    //        await Background.BringToFront();
    //    }
    //}

    public partial class CoachMarks
    {
        const string BACKGROUND_CSS_CLASS = "coach-marks-background";

        bool SkipTapped;
        Settings CurrentSettings;
        CancellationToken CancellationToken;

        TaskCompletionSource<bool> OnPopOverClosed;
        TaskCompletionSource<bool> OnNextTapped;
        TaskCompletionSource<bool> OnSkipTapped;

        PopOver PopOver;
        int? ElementZIndex;
        View Element;

        Stack ButtonsContainer;
        Canvas TopBackground;
        Canvas MiddleBackground;
        Canvas BottomBackground;

        public bool IsCoaching { get; private set; }

        public Task Coach(Settings settings) => Coach(settings, CancellationToken.None);

        public async Task Coach(Settings settings, CancellationToken cancellationToken)
        {
            if (IsCoaching)
                throw new InvalidOperationException("Coaching is under process.");

            IsCoaching = true;
            SkipTapped = false;
            CurrentSettings = settings;
            CancellationToken = cancellationToken;

            try
            {
                await Initialize();

                foreach (var step in settings.Steps)
                {
                    if (ShouldItTerminate()) return;

                    // Show the step by showing it text and element.
                    await ShowStep(step);
                    
                    if (ShouldItTerminate()) return;

                    // Wait for next, skip or time delay (If applicable) to continue.
                    if (settings.MoveOnByTime)
                        await Task.WhenAny(OnNextTapped?.Task, OnSkipTapped?.Task, OnPopOverClosed?.Task, Task.Delay(settings.Delay));
                    else
                        await Task.WhenAny(OnNextTapped?.Task, OnSkipTapped?.Task, OnPopOverClosed?.Task);

                    if (ShouldItTerminate()) return;

                    // Hide the step by showing it text and element.
                    await HideStep();
                }
            }
            finally
            {
                IsCoaching = false;
                await Clear();
            }
        }

        async Task Initialize()
        {
            ButtonsContainer = new Stack { CssClass = "coach-marks-buttons", Direction = RepeatDirection.Horizontal };
            TopBackground = new Canvas { CssClass = BACKGROUND_CSS_CLASS };
            BottomBackground = new Canvas { CssClass = BACKGROUND_CSS_CLASS };

            await View.Root.Add(TopBackground.Y(0));
            await View.Root.Add(BottomBackground);
            await View.Root.Add(ButtonsContainer);

            if (CurrentSettings.CanSkip)
                await ButtonsContainer.Add(new Button { Text = "Skip", CssClass = "skip" }
                .On(b => b.Tapped, () =>
                {
                    OnSkipTapped?.TrySetResult(result: true);
                    SkipTapped = true;
                }));
            
            await ButtonsContainer.Add(new Button { Text = "Next", CssClass = "next" }
                .On(b => b.Tapped, () => OnNextTapped?.TrySetResult(result: true)));
        }

        Task HideStep()
        {
            if(ElementZIndex.HasValue)
                Element.ZIndex(ElementZIndex.Value);
            ElementZIndex = null;

            return PopOver.Hide();
        }

        async Task ShowStep(Step step)
        {
            Element = View.Root.AllDescendents().FirstOrDefault(v => v.Id == step.ElementId);
            var parent = Element.Parent ?? throw new InvalidOperationException($"It is not possible to add a step for '{step.ElementId}'.");
            ElementZIndex = Element.ZIndex;

            // Rearrange backgrounds
            var parentAbsY = parent.CalculateAbsoluteY();
            var parentHeight = parent.Height.CurrentValue;

            if (MiddleBackground != null)
                await MiddleBackground.RemoveSelf();

            MiddleBackground = new Canvas { CssClass = BACKGROUND_CSS_CLASS };

            MiddleBackground.Y(0);
            MiddleBackground.Height(parentHeight);

            TopBackground.Height(parentAbsY);

            BottomBackground.Y(parentAbsY + parentHeight);
            BottomBackground.Height(View.Root.Height.CurrentValue - parentAbsY - parentHeight);

            await parent.Add(MiddleBackground);

            // Refresh event handlers
            OnPopOverClosed = new TaskCompletionSource<bool>();
            OnNextTapped = new TaskCompletionSource<bool>();
            OnSkipTapped = new TaskCompletionSource<bool>();

            PopOver = (await Element.ShowPopOver(step.Text))
                .On(x => x.OnHide, () =>
                {
                    if (!OnPopOverClosed.Task.IsCompleted)
                        OnPopOverClosed.SetResult(result: true);
                });

            // Reset Z indexes
            await TopBackground.BringToFront();
            await MiddleBackground.BringToFront();
            await BottomBackground.BringToFront();
            await ButtonsContainer.BringToFront();
            await PopOver.BringToFront();
        }

        async Task Clear()
        {
            await HideStep();
            
            await ButtonsContainer.RemoveSelf();
            await TopBackground.RemoveSelf();
            await MiddleBackground.RemoveSelf();
            await BottomBackground.RemoveSelf();
        }

        Task WhenAny(params Task[] tasks) => Task.WhenAny(tasks.Except(t => t == null).ToArray());

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;
    }
}
