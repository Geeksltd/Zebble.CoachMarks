using System.Threading.Tasks;

namespace Zebble
{
    public class NextButton : Button
    {
        readonly AsyncEvent buttonTapped;

        public NextButton(AsyncEvent buttonTapped)
        {
            this.buttonTapped = buttonTapped;
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            Text = "Next";
            Tapped.Handle(() => buttonTapped.Raise());
        }
    }
}
