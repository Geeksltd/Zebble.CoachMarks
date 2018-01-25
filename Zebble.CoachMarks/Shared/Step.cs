using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zebble
{
    partial class CoachMarks
    {
        public class Step
        {
            View element;
            
            public string Text { get; set; }

            public string ElementId { get; set; }

            internal Step() { /**/ }

            internal View Element => element = element ?? View.Root.AllDescendents().FirstOrDefault(v => v.Id == ElementId);
        }
    }
}
