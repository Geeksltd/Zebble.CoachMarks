using Android.Widget;
using System.Threading.Tasks;
using Zebble.AndroidOS;
using AndroidViews = Android.Views;

namespace Zebble
{
    partial class CoachMarks
    {
        async Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            var native = view.Native();
            
            var nativeParent = native.Parent;
            var newNativeParent = newParent.Native();
            
            if (nativeParent is IScrollView scrollview)
                scrollview.GetContainer().RemoveView(native);
            else if (nativeParent is AndroidViews.ViewGroup viewGroup)
                viewGroup.RemoveView(native);

            if (newNativeParent is IScrollView newScrollview)
                newScrollview.GetContainer().AddView(native);
            else if (newNativeParent is AndroidViews.ViewGroup newViewGroup)
                newViewGroup.AddView(native);

            native.LayoutParameters = new FrameLayout.LayoutParams(native.LayoutParameters)
            {
                LeftMargin = Scaler.ToDevice(left),
                TopMargin = Scaler.ToDevice(top),
            };
        }
    }
}
