using System.Linq;

namespace Zebble
{
    partial class CoachMarks
    {
        public class Step
        {
            View element;
            
            public string Text { get; set; }

            public string ElementId { get; }

            public View Element
            {
                get
                {
                    if (element == null)
                    {
                        element = View.Root.AllDescendents().FirstOrDefault(v => v.Id == ElementId);
                    }

                    return element;
                }
            }

            internal Step(string elementId)
            {
                ElementId = elementId;
            }

            internal Step(View element)
            {
                this.element = element;
                ElementId = element.Id;
            }
        }
    }
}
