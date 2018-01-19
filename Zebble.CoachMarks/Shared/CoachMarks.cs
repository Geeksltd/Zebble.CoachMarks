using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zebble
{
    public partial class CoachMarks
    {
        Settings Setting;
        BackgroundControl Background;
        
        TaskCompletionSource<bool> OnPopOverClosed;
        TaskCompletionSource<bool> OnNextTapped;
        TaskCompletionSource<bool> OnSkipTapped;

        CancellationToken CancellationToken;
        bool SkipTapped;

        View Element;
        View ElementParent;
        View ElementHolder;
        View ElementInnerHolder;
        PopOver PopOver;

        public bool IsCoaching => Background != null;

        public Task Coach(Settings settings) => Coach(settings, CancellationToken.None);

        public async Task Coach(Settings settings, CancellationToken cancellationToken)
        {
            if (IsCoaching)
                throw new InvalidOperationException("Coaching is under process.");

            CancellationToken = cancellationToken;
            Setting = settings;
            SkipTapped = false;
            
            try
            {
                // Create and show background which contains the next and skip button
                await ShowBackround(cancellationToken);

                foreach (var step in settings.Steps)
                {
                    if (ShouldItTerminate()) return;

                    // Show the step by showing it text and element.
                    await ShowStep(step);

                    if (ShouldItTerminate()) return;

                    // Wait for next, skip or time delay (If applicable) to continue.
                    if (settings.MoveOnByTime)
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnPopOverClosed.Task, Task.Delay(settings.Delay));
                    else
                        await Task.WhenAny(OnNextTapped.Task, OnSkipTapped.Task, OnPopOverClosed.Task);

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

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;

        async Task HideStep()
        {
            await Task.WhenAll(
                ShrinkTheHolder(),
                PopOver.Hide());

            await ChangeParent(Element, ElementParent, Element.ActualY, Element.ActualX);

            await Task.WhenAll(
                ElementHolder.RemoveSelf(),
                ElementInnerHolder.RemoveSelf()
                );
        }

        async Task ShowStep(Step step)
        {
            Element = step.Element;

            ElementParent = Element.Parent ?? throw new InvalidOperationException();

            async Task showPopOver(){ PopOver = await Element.ShowPopOver(step.Text); }

            await CreateHolder();            
            await ChangeParent(Element, ElementInnerHolder);

            await Task.WhenAll(
                ExpandTheHolder(),
                showPopOver()
                );


            OnPopOverClosed = new TaskCompletionSource<bool>();
            OnNextTapped = new TaskCompletionSource<bool>();
            OnSkipTapped = new TaskCompletionSource<bool>();

            PopOver.On(x => x.OnHide, () =>
            {
                if (!OnPopOverClosed.Task.IsCompleted)
                    OnPopOverClosed.SetResult(result: true);
            });

            await PopOver.BringToFront();
        }
        
        async Task CreateHolder() {
            // Adding the ElementHolder
            await View.Root.Add(ElementHolder = GetCanvasForElement(Setting.ElementPadding));
            ElementHolder.BackgroundColor = Colors.White;
            ElementHolder.Opacity(0);

            await View.Root.Add(ElementInnerHolder = GetCanvasForElement());
            
            await ElementHolder.BringToFront();
            await ElementInnerHolder.BringToFront();
        }

        Canvas GetCanvasForElement(int radiusMax = 0)
        {
            var result = new Canvas();

            result.X(Element.CalculateAbsoluteX());
            result.Y(Element.CalculateAbsoluteY());

            result.Height(Element.ActualHeight);
            result.Width(Element.ActualWidth);

            Func<float, float> getBorderRadius = (a) => a + Setting.ElementPadding.LimitMax(radiusMax);

            result.BorderRadius(
                topLeft: getBorderRadius(Element.Border.RadiusTopLeft),
                topRight: getBorderRadius(Element.Border.RadiusTopRight),
                bottomLeft: getBorderRadius(Element.Border.RadiusBottomLeft),
                bottomRight: getBorderRadius(Element.Border.RadiusBottomRight)
                );

            return result;
        }

        async Task ExpandTheHolder()
        {
            Func<float, float> getScale = a => (a + Setting.ElementPadding * 2) / a;

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

        async Task ShowBackround(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Background = new BackgroundControl();

            Background.Y(0);
            Background.X(0);
            Background.Height.BindTo(View.Root.Height);
            Background.Width.BindTo(View.Root.Width);

            Background.On(x => x.RightButtonTapped, () => OnNextTapped?.SetResult(result: true))
                .On(x => x.LeftButtonTapped, () =>
                {
                    SkipTapped = true;
                    OnSkipTapped?.SetResult(result: true);
                });

            await View.Root.Add(Background);
            await Background.BringToFront();
            await View.Root.Add(ElementHolder = new Canvas { BackgroundColor = Colors.White, Visible = false });
        }
    }
}
