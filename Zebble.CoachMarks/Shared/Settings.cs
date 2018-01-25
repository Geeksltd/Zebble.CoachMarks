using System;

namespace Zebble
{
    public class CoachMarksSettings
    {
        Buttons AllButtons = Buttons.Skip | Buttons.Back | Buttons.Next;

        Buttons topButtons = Buttons.Skip;
        Buttons bottomButtons = Buttons.Next | Buttons.Back;

        public bool MoveOnByTime { get; set; } = false;

        public TimeSpan Delay { get; set; } = new TimeSpan(0, 0, 3);

        public int ElementPadding { get; set; } = 5;

        public bool DisableRealEvents { get; set; } = true;

        public Buttons TopButtons
        {
            get => topButtons;
            set
            {
                bottomButtons &= GetFilter(value);
                topButtons = value;
            }
        }

        public Buttons BottomButtons
        {
            get => bottomButtons;
            set
            {
                topButtons &= GetFilter(value);
                bottomButtons = value;
            }
        }


        Buttons GetFilter(Buttons buttons) => (AllButtons & buttons) ^ AllButtons;

        [Flags]
        public enum Buttons
        {
            None = 0,
            Back = 1,
            Next = 2,
            Skip = 4
        }
    }
}