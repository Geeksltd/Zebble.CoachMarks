using System.Threading.Tasks;
using CoreGraphics;

namespace Zebble
{
    partial class CurrentStep
    {
        async Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            await Task.Delay(Animation.FadeDuration);

            var native = view.Native();

            native.RemoveFromSuperview();

            newParent.Native().Add(native);
            
            native.Frame = new CGRect(left, top, view.ActualWidth, view.ActualHeight);
        }
    }
}
