using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zebble
{
    class BackgroundControl : Canvas
    {
        CoachMarksSettings Settings;

        Canvas Overlay;

        Stack TopButtons;
        Stack BottomButtons;

        Button NextButton;
        Button SkipButton;
        Button BackButton;

        public readonly AsyncEvent NextButtonTapped = new AsyncEvent();
        public readonly AsyncEvent SkipButtonTapped = new AsyncEvent();
        public readonly AsyncEvent BackButtonTapped = new AsyncEvent();

        public string NextButtonText
        {
            get => NextButton.Text;
            set => NextButton.Text = value;
        }

        public bool BackButtonVisible
        {
            get => BackButton.Visible;
            set => BackButton.Visible = value;
        }

        public BackgroundControl(CoachMarksSettings settings)
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


            // Buttons` stacks
            TopButtons = new Stack { CssClass = "coach-marks-buttons top" };
            BottomButtons = new Stack { CssClass = "coach-marks-buttons bottom" };
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            await Add(Overlay);
            await Add(TopButtons);
            await Add(BottomButtons);

            await AddButtons(TopButtons, Settings.TopButtons);
            await AddButtons(BottomButtons, Settings.BottomButtons);
        }

        async Task AddButtons(Stack stack, CoachMarksSettings.Buttons buttons)
        {
            Func<CoachMarksSettings.Buttons, bool> has = b => (buttons & b) == b;

            if (has(CoachMarksSettings.Buttons.Skip))
                await stack.Add(SkipButton);

            if (has(CoachMarksSettings.Buttons.Back))
                await stack.Add(BackButton);

            if (has(CoachMarksSettings.Buttons.Next))
                await stack.Add(NextButton);
        }
    }
}
