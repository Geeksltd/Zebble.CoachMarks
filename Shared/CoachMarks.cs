using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zebble
{
    public partial class CoachMarks : Canvas
    {
        List<Step> stepsList = new List<Step>();

        Canvas Overlay;
        Stack TopButtons;
        Stack BottomButtons;
        Button NextButton;
        Button SkipButton;
        Button BackButton;

        TaskCompletionSource<bool> OnPopOverClosed;
        TaskCompletionSource<bool> OnNextTapped;
        TaskCompletionSource<bool> OnSkipTapped;
        TaskCompletionSource<bool> OnBackTapped;

        CancellationToken CancellationToken;
        bool SkipTapped;
        int Index;

        View Element;
        View ElementParent;
        View ElementHolder;
        View ElementInnerHolder;
        View EventBlocker;
        PopOver PopOver;

        public readonly AsyncEvent NextButtonTapped = new AsyncEvent();
        public readonly AsyncEvent SkipButtonTapped = new AsyncEvent();
        public readonly AsyncEvent BackButtonTapped = new AsyncEvent();

        public CoachMarksSettings Settings { get; }

        public bool IsCoaching;

        public CoachMarks() : this(new CoachMarksSettings()) { }

        public CoachMarks(CoachMarksSettings settings)
        {
            Settings = settings;

            Overlay = new Canvas { CssClass = "coach-marks-background" };

            // Buttons
            NextButton = new Button { Text = "Next", CssClass = "next" }
                .On(x => x.Tapped, () => NextButtonTapped.Raise());

            SkipButton = new Button { Text = "Skip", CssClass = "skip" }
                .On(x => x.Tapped, () => SkipButtonTapped.Raise());

            BackButton = new Button { Text = "Back", CssClass = "back" }
                .On(x => x.Tapped, () => BackButtonTapped.Raise());

            // Buttons stacks
            TopButtons = new Stack { CssClass = "coach-marks-buttons top" };
            BottomButtons = new Stack { CssClass = "coach-marks-buttons bottom" };
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

        public async Task Show(CancellationToken cancellationToken)
        {
            if (IsCoaching)
                throw new InvalidOperationException("Coaching is under process.");

            CancellationToken = cancellationToken;
            SkipTapped = false;
            
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
                    await ShowStep(step);

                    if (ShouldItTerminate()) return;

                    // Wait for next, skip or time delay (If applicable) to continue.
                    if (Settings.MoveOnByTime)
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnBackTapped.Task, OnPopOverClosed.Task, Task.Delay(Settings.Delay));
                    else
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnBackTapped.Task, OnPopOverClosed.Task);

                    if (ShouldItTerminate()) return;

                    // Hide the step by showing it text and element.
                    await HideStep();
                }
            }
            finally
            {
                await Task.WhenAll(RemoveButtons(), HideStep());
            }
        }

        public void Hide()
        {
            SkipTapped = true;
            OnSkipTapped?.TrySetResult(result: true);
        }

        void FixButtonsConditions()
        {
            BackButton.Visible = Index > 0;
            NextButton.Text = stepsList.Count - 1 == Index ? "Finish" : "Next";
        }

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;
        
        async Task HideStep()
        {
            await Task.WhenAll(ShrinkTheHolder(), PopOver.Hide());

            await ChangeParent(Element, ElementParent, Element.ActualY, Element.ActualX);

            await Task.WhenAll(ElementHolder.RemoveSelf(), ElementInnerHolder.RemoveSelf(), EventBlocker.RemoveSelf());
        }

        async Task ShowStep(Step step)
        {
            Element = step.Element;

            ElementParent = Element.Parent ?? throw new InvalidOperationException();

            async Task showPopOver(){ PopOver = await Element.PopOver(step.Text); }

            await CreateHolder();            
            await ChangeParent(Element, ElementInnerHolder);

            await Task.WhenAll(ExpandHolder(), showPopOver());

            OnPopOverClosed = new TaskCompletionSource<bool>();
            OnNextTapped = new TaskCompletionSource<bool>();
            OnSkipTapped = new TaskCompletionSource<bool>();
            OnBackTapped = new TaskCompletionSource<bool>();

            PopOver.On(x => x.OnHide, () =>
            {
                if (!OnPopOverClosed.Task.IsCompleted)
                    OnPopOverClosed.SetResult(result: true);
            });

            await PopOver.BringToFront();
        }
        
        async Task CreateHolder() {
            // Adding the ElementHolder
            await Root.Add(ElementHolder = GetCanvasForElement(Settings.ElementPadding));
            ElementHolder.BackgroundColor = Colors.White;
            ElementHolder.Opacity(0);

            await Root.Add(ElementInnerHolder = GetCanvasForElement());
            
            await ElementHolder.BringToFront();
            await ElementInnerHolder.BringToFront();

            if(Settings.DisableRealEvents)
            {
                await Root.Add(EventBlocker = GetCanvasForElement());
                await EventBlocker.BringToFront();
            }
        }

        Canvas GetCanvasForElement(int radiusMax = 0)
        {
            var result = new Canvas { CssClass = "coach-marks-element-holder".OnlyWhen(radiusMax > 0) };

            result.X(Element.CalculateAbsoluteX());
            result.Y(Element.CalculateAbsoluteY());

            result.Height(Element.ActualHeight);
            result.Width(Element.ActualWidth);

            Func<float, float> getBorderRadius = (a) => a + Settings.ElementPadding.LimitMax(radiusMax);

            result.BorderRadius(
                topLeft: getBorderRadius(Element.Border.RadiusTopLeft),
                topRight: getBorderRadius(Element.Border.RadiusTopRight),
                bottomLeft: getBorderRadius(Element.Border.RadiusBottomLeft),
                bottomRight: getBorderRadius(Element.Border.RadiusBottomRight)
                );

            return result;
        }

        async Task ExpandHolder()
        {
            Func<float, float> getScale = a => (a + Settings.ElementPadding * 2) / a;

            await ElementHolder.Animate(new Animation
            {
                Easing = AnimationEasing.EaseIn,
                EasingFactor = EasingFactor.Cubic,
                Change = () =>
                {
                    ElementHolder.ScaleX(getScale(ElementHolder.ActualWidth));
                    ElementHolder.ScaleY(getScale(ElementHolder.ActualHeight));
                    ElementHolder.Opacity(1);
                },
                Duration = Animation.FadeDuration
            });
        }

        async Task ShrinkTheHolder()
        {
            await ElementHolder.Animate(new Animation
            {
                Easing = AnimationEasing.EaseIn,
                EasingFactor = EasingFactor.Cubic,
                Change = () => {
                    ElementHolder.ScaleX(1);
                    ElementHolder.ScaleY(1);
                    ElementHolder.Opacity(0);
                },
                Duration = Animation.FadeDuration
            });
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
            await Root.Add(ElementHolder = new Canvas { BackgroundColor = Colors.White, Visible = false });
        }

        async Task AddButtons(Stack stack, CoachMarksSettings.Buttons buttons)
        {
            bool Has(CoachMarksSettings.Buttons b) => (buttons & b) == b;

            if (Has(CoachMarksSettings.Buttons.Skip))
                await stack.Add(SkipButton);

            if (Has(CoachMarksSettings.Buttons.Back))
                await stack.Add(BackButton);

            if (Has(CoachMarksSettings.Buttons.Next))
                await stack.Add(NextButton);
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
