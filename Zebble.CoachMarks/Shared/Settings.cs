using System;
using System.Collections.Generic;
using System.Text;

namespace Zebble
{
    partial class CoachMarks
    {
        public class Settings
        {
            List<Step> stepsList = new List<Step>();

            public View MarksRoot { get; set; }

            public bool MoveOnByTime { get; set; } = false;

            public TimeSpan Delay { get; set; } = new TimeSpan(0, 0, 3);

            public int ElementPadding { get; set; } = 5;

            public bool DisableRealEvents { get; set; } = false;

            public IEnumerable<Step> Steps => stepsList.ToArray();

            public bool CanSkip { get; set; } = true;

            public Step CreateStep(string text, string elementId, bool isNextEnabled = true)
            {
                var step = new Step
                {
                    Text = text,
                    ElementId = elementId,
                    IsNextEnabled = isNextEnabled
                };

                stepsList.Add(step);

                return step;
            }
        }
    }
}