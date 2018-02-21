using System;
using System.Threading.Tasks;

namespace Zebble
{
    partial class CurrentStep
    {
        string Text;
        CoachMarksSettings Settings;
        View Element;
        View ElementParent;
        View ElementHolder;
        View ElementInnerHolder;
        View EventBlocker;
        PopOver PopOver;

        public TaskCompletionSource<bool> OnPopOverClosed;
        
        public CurrentStep(View element, string text, CoachMarksSettings settings)
        {
            Text = text;
            Element = element;
            Settings = settings;
            ElementParent = Element.Parent ?? throw new InvalidOperationException();
        }

        public async Task Show()
        {
            async Task showPopOver() { PopOver = await Element.PopOver(Text); }

            await CreateHolder();
            await ChangeParent(Element, ElementInnerHolder);

            await Task.WhenAll(ExpandHolder(), showPopOver());

            OnPopOverClosed = new TaskCompletionSource<bool>();

            PopOver.On(x => x.OnHide, () =>
            {
                if (!OnPopOverClosed.Task.IsCompleted)
                    OnPopOverClosed.SetResult(result: true);
            });

            await PopOver.BringToFront();
        }

        public async Task Hide()
        {
            await Task.WhenAll(ShrinkTheHolder(), PopOver.Hide());

            await ChangeParent(Element, ElementParent, Element.ActualY, Element.ActualX);

            await Task.WhenAll(ElementHolder.RemoveSelf(), ElementInnerHolder.RemoveSelf(), EventBlocker.RemoveSelf());
        }

        async Task CreateHolder()
        {
            // Adding the ElementHolder
            await View.Root.Add(ElementHolder = GetCanvasForElement(Settings.ElementPadding));
            ElementHolder.BackgroundColor = Colors.White;
            ElementHolder.Opacity(0);

            await View.Root.Add(ElementInnerHolder = GetCanvasForElement());

            await ElementHolder.BringToFront();
            await ElementInnerHolder.BringToFront();

            if (Settings.DisableRealEvents)
            {
                await View.Root.Add(EventBlocker = GetCanvasForElement());
                await EventBlocker.BringToFront();
            }
        }

        Canvas GetCanvasForElement(int radiusMax = 0)
        {
            Canvas result;
            if (radiusMax > 0)
                result = new ElementHolder();
            else
                result = new Canvas();

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
    }
}
