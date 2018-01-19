using System;
using System.Collections.Generic;
using System.Text;

namespace Zebble
{
    partial class CoachMarks
    {
        public class Settings
        {
            Buttons AllButtons = Buttons.Skip | Buttons.Back | Buttons.Next;

            Buttons topButtons = Buttons.Skip;
            Buttons bottomButtons = Buttons.Next | Buttons.Back;

            List<Step> stepsList = new List<Step>();
            
            public bool MoveOnByTime { get; set; } = false;

            public TimeSpan Delay { get; set; } = new TimeSpan(0, 0, 3);

            public int ElementPadding { get; set; } = 5;

            public bool DisableRealEvents { get; set; } = false;

            public Step[] Steps => stepsList.ToArray();

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

            public Buttons GetFilter(Buttons buttons) => (AllButtons & buttons) ^ AllButtons;

            public Step CreateStep(string text, string elementId, bool isNextEnabled = true)
            {
                var result = new Step
                {
                    Text = text,
                    ElementId = elementId,
                    IsNextEnabled = isNextEnabled
                };

                stepsList.Add(result);

                return result;
            }
        }

        [Flags]
        public enum Buttons
        {
            Back = 1,
            Next = 2,
            Skip = 4
        }
    }
}