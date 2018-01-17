using System;
using System.Collections.Generic;
using System.Text;

namespace Zebble
{
    partial class CoachMarks
    {
        public class Step
        {
            public bool IsNextEnabled { get; set; } = true;

            public string Text { get; set; }

            public string ElementId { get; set; }

            internal Step() { /**/ }
        }
    }
}
