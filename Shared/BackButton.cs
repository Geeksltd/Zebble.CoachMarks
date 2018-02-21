using System.Threading.Tasks;

namespace Zebble
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
