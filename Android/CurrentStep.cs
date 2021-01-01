using System.Threading.Tasks;
using Android.Widget;
using Zebble.AndroidOS;
using Zebble.Device;
using AndroidViews = Android.Views;

namespace Zebble
{
    partial class CurrentStep
    {
        Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            var native = view.Native();
            
            var nativeParent = native.Parent;
            var newNativeParent = newParent.Native();
            
            if (nativeParent is IScrollView scrollView)
                scrollView.GetContainer().RemoveView(native);
            else if (nativeParent is AndroidViews.ViewGroup viewGroup)
                viewGroup.RemoveView(native);

            if (newNativeParent is IScrollView newScrollView)
                newScrollView.GetContainer().AddView(native);
            else if (newNativeParent is AndroidViews.ViewGroup newViewGroup)
                newViewGroup.AddView(native);

            native.LayoutParameters = new FrameLayout.LayoutParams(native.LayoutParameters)
            {
                LeftMargin = Scale.ToDevice(left),
                TopMargin = Scale.ToDevice(top),
            };
            
            return Task.CompletedTask;
        }
    }
}
