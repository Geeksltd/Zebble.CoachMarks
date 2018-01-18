using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zebble
{
    class BackgroundControl : Canvas
    {
        const string COLOR_CODE = "#121212";
        const float OPACITY = 0.4F;

        Canvas Overlay;
        Stack Stack;
        Button LeftButton;
        Button RightButton;

        public readonly AsyncEvent LeftButtonTapped = new AsyncEvent();
        public readonly AsyncEvent RightButtonTapped = new AsyncEvent();

        public string RightButtonText
        {
            get => RightButton.Text;
            set => RightButton.Text = value;
        }
        public string LeftButtonText
        {
            get => LeftButton.Text;
            set => LeftButton.Text = value;
        }

        public BackgroundControl(string leftButtonText = "Skip", string rightButtonText = "Next")
        {
            Overlay = new Canvas
            {
                BackgroundColor = Color.Parse(COLOR_CODE),
                Opacity = OPACITY
            };

            Overlay.X(0);
            Overlay.Y(0);
            Overlay.Height.BindTo(Height);
            Overlay.Width.BindTo(Width);

            Add(Overlay);

            LeftButton = new Button
            {
                Text = leftButtonText,
                TextColor = Colors.Black,
                BackgroundColor = Colors.White
            };
            LeftButton.Css.TextAlignment = Alignment.Left;
            LeftButton.On(x => x.Tapped, () => LeftButtonTapped.Raise());

            RightButton = new Button
            {
                Text = rightButtonText,
                TextColor = Colors.White
            };
            RightButton.Css.TextAlignment = Alignment.Right;
            RightButton.On(x => x.Tapped, () => RightButtonTapped.Raise());

            Stack = new Stack
            {
                Direction = RepeatDirection.Horizontal
            };

            Stack.Css.Absolute = true;
            Stack.Width.BindTo(Root.Width);
            Stack.Y.BindTo(Root.Height, Stack.Height, (rh, sh) => rh - sh);

            Stack.Add(LeftButton);
            Stack.Add(RightButton);

            Add(Stack);
        }
    }
}
