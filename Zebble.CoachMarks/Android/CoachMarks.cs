using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidViews = Android.Views;

namespace Zebble
{
    partial class CoachMarks
    {
        async Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            var native = view.Native();

            var parent = view.parent;
            var nativeParent = parent?.Native();
            var newNativeParent = newParent.Native();


            // High concurrency. Already disposed:
            if (view.IsDisposing || parent == null || parent.IsDisposed || newNativeParent == null) return;

            
            if (newNativeParent is AndroidOS.IScrollView scrollview)
            {
                scrollview.GetContainer().AddView(native);
            }
            else if (newNativeParent is AndroidViews.ViewGroup viewGroup)
            {
                if (native.Parent != null) viewGroup.RemoveView(native);
                viewGroup.AddView(native);
            }
        }
    }
}
