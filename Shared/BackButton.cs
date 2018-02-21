using System.Threading.Tasks;

namespace Zebble
{
    partial class CoachMarks
    {
        public class BackButton : Button
        {
            readonly AsyncEvent buttonTapped;

            public BackButton(AsyncEvent buttonTapped)
            {
                this.buttonTapped = buttonTapped;
            }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();

                Text = "Back";
                Tapped.Handle(() => buttonTapped.Raise());
            }
        }
    }
}
