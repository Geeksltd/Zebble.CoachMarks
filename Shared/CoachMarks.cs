using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zebble
{
    public partial class CoachMarks
    {
        List<Step> stepsList = new List<Step>();
        BackgroundControl Background;

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

        public CoachMarksSettings Settings { get; }

        public bool IsCoaching => Background != null;

        public CoachMarks() : this(new CoachMarksSettings()) { }

        public CoachMarks(CoachMarksSettings settings) => Settings = settings;

        public Step CreateStep(string text, string elementId)
        {
            var result = new Step
            {
                Text = text,
                ElementId = elementId
            };

            stepsList.Add(result);

            return result;
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
                await ShowBackground(cancellationToken);

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
                await Task.WhenAll(
                    RemoveBackground(),
                    HideStep());
            }
        }

        public void Hide()
        {
            SkipTapped = true;
            OnSkipTapped?.TrySetResult(result: true);
        }

        void FixButtonsConditions()
        {
            Background.BackButtonVisible = Index > 0;
            Background.NextButtonText = stepsList.Count - 1 == Index ? "Finish" : "Next";
        }

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;

        async Task HideStep()
        {
            await Task.WhenAll(
                ShrinkTheHolder(),
                PopOver.Hide());

            await ChangeParent(Element, ElementParent, Element.ActualY, Element.ActualX);

            await Task.WhenAll(
                ElementHolder.RemoveSelf(),
                ElementInnerHolder.RemoveSelf(),
                EventBlocker.RemoveSelf()
                );
        }

        async Task ShowStep(Step step)
        {
            Element = step.Element;

            ElementParent = Element.Parent ?? throw new InvalidOperationException();

            async Task showPopOver(){ PopOver = await Element.PopOver(step.Text); }

            await CreateHolder();            
            await ChangeParent(Element, ElementInnerHolder);

            await Task.WhenAll(
                ExpandHolder(),
                showPopOver()
                );


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
            await View.Root.Add(ElementHolder = GetCanvasForElement(Settings.ElementPadding));
            ElementHolder.BackgroundColor = Colors.White;
            ElementHolder.Opacity(0);

            await View.Root.Add(ElementInnerHolder = GetCanvasForElement());
            
            await ElementHolder.BringToFront();
            await ElementInnerHolder.BringToFront();

            if(Settings.DisableRealEvents)
            {
                await View.Root.Add(EventBlocker = GetCanvasForElement());
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

            //ElementHolder.Opacity = 1;
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

        async Task RemoveBackground()
        {
            await Background.RemoveSelf();
            Background = null;
        }

        async Task ShowBackground(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Background = new BackgroundControl(Settings);

            Background
                .On(x => x.NextButtonTapped, () => OnNextTapped?.TrySetResult(result: true))
                .On(x => x.SkipButtonTapped, () => Hide())
                .On(x => x.BackButtonTapped, () =>
                {
                    Index -= 2;
                    OnBackTapped?.TrySetResult(result: true);
                });

            await View.Root.Add(Background);
            await Background.BringToFront();
            await View.Root.Add(ElementHolder = new Canvas { BackgroundColor = Colors.White, Visible = false });
        }
    }
}
