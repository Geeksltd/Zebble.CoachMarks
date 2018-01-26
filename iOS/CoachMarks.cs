using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebble
{
    partial class CoachMarks
    {
        async Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            await Task.Delay(Animation.FadeDuration);
            var parent = view.Parent;

            var native = view.Native();

            native.RemoveFromSuperview();

            newParent.Native().Add(native);
            
            native.Frame = new CGRect(left, top, view.ActualWidth, view.ActualHeight);
        }
    }
}
