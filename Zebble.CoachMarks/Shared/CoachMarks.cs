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
                await RemoveBackground();

                await HideStep();
                await ElementHolder.RemoveSelf();
            }
        }

        bool ShouldItTerminate() => CancellationToken.IsCancellationRequested || SkipTapped;

        async Task HideStep()
        {
            await ChangeParent(Element, ElementParent, Element.ActualY, Element.ActualX);
            
            await PopOver.Hide();
        }

        async Task ShowStep(Step step)
        {
            Element = step.Element;

            ElementParent = Element.Parent ?? throw new InvalidOperationException();

            await Task.WhenAll(
                Fade(),
                Move(),
                ChangeParent(Element, ElementHolder, Setting.ElementPadding, Setting.ElementPadding),
                ShowUp());

            OnPopOverClosed = new TaskCompletionSource<bool>();
            OnNextTapped = new TaskCompletionSource<bool>();
            OnSkipTapped = new TaskCompletionSource<bool>();

            PopOver = (await Element.ShowPopOver(step.Text)).On(x => x.OnHide, () =>
            {
                if (!OnPopOverClosed.Task.IsCompleted)
                    OnPopOverClosed.SetResult(result: true);
            });

            await PopOver.BringToFront();
        }

        async Task ShowUp()
        {
            await Task.Delay(Animation.FadeDuration);

            await ElementHolder.Animate(new Animation
            {
                Easing = AnimationEasing.EaseOut,
                EasingFactor = EasingFactor.Cubic,
                Change = () =>
                {
                    ElementHolder.Opacity = 1;
                },
                Duration = HalfDuration
            });
        }

        TimeSpan HalfDuration => ((int)(Animation.FadeDuration.TotalMilliseconds / 2)).Milliseconds();

        async Task Move() {
            await Task.Delay(HalfDuration);

            await ElementHolder.Animate(new Animation
            {
                Easing = AnimationEasing.EaseIn,
                EasingFactor = EasingFactor.Cubic,
                Change = () => {
                    ElementHolder.Opacity = 0.25f;
                    ElementHolder.X(Element.CalculateAbsoluteX() - Setting.ElementPadding);
                    ElementHolder.Y(Element.CalculateAbsoluteY() - Setting.ElementPadding);

                    ElementHolder.Height(Element.ActualHeight + Setting.ElementPadding * 2);
                    ElementHolder.Width(Element.ActualWidth + Setting.ElementPadding * 2);

                    ElementHolder.BorderRadius(
                        topLeft: Element.Border.RadiusTopLeft + Setting.ElementPadding,
                        topRight: Element.Border.RadiusTopRight + Setting.ElementPadding,
                        bottomLeft: Element.Border.RadiusBottomLeft + Setting.ElementPadding,
                        bottomRight: Element.Border.RadiusBottomRight + Setting.ElementPadding
                        );
                },
                Duration = HalfDuration
            });
        }
        async Task Fade()
        {
            await ElementHolder.Animate(new Animation
             {
                 Easing = AnimationEasing.EaseIn,
                 EasingFactor = EasingFactor.Cubic,
                 Change = () => {
                     ElementHolder.Opacity = 0.25f;
                 },
                 Duration = HalfDuration
             });            

            await ElementHolder.BringToFront();
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

            // Adding the ElementHolder
            await View.Root.Add(ElementHolder = new Canvas { BackgroundColor = Colors.White, Opacity = 0 });
        }
    }
}
