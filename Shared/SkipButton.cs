using System.Threading.Tasks;

namespace Zebble
{
    public class SkipButton : Button
    {
        readonly AsyncEvent buttonTapped;

        public SkipButton(AsyncEvent buttonTapped)
        {
            this.buttonTapped = buttonTapped;
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            Text = "Skip";
            Tapped.Handle(() => buttonTapped.Raise());
        }
    }
}
