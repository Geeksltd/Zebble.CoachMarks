using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using controls = Windows.UI.Xaml.Controls;
using xaml = Windows.UI.Xaml;

namespace Zebble
{
    partial class CoachMarks
    {
        async Task ChangeParent(View view, View newParent, float top, float left)
        {
            var parent = view.Parent;

            var native = view.Native() as xaml.UIElement;
            var nativeParent = parent?.Native() as xaml.UIElement;            
        }

        //async Task AddToNativeParent(View zebbleParent, xaml.UIElement nativeChild)
        //{
        //    var parent = zebbleParent?.parent;
        //    var nativeParent = parent?.Native() as xaml.UIElement;

        //    if (nativeParent is controls.Border wrapper) nativeParent = wrapper.Child;

        //    //var nativeChild = NativeResult;

        //    // High concurrency. Already disposed:
        //    if (zebbleParent == null || zebbleParent.IsDisposing) return;
        //    if (parent == null || parent.IsDisposed || nativeParent == null || nativeChild == null) return;

        //    using (await parent.DomLock.LockAsync())
        //    {
        //        using (await zebbleParent.DomLock.LockAsync())
        //        {
        //            nativeChild.Visibility = xaml.Visibility.Collapsed;

        //            try
        //            {
        //                if (nativeParent is controls.Panel panel)
        //                    panel.Children.Add(nativeChild);
        //                else if (nativeParent is controls.ScrollViewer scroller)
        //                    (scroller.Content as controls.Panel)?.Children.Add(nativeChild);
        //                else throw new Exception(nativeParent.GetType().Name +
        //                        " is not a supported container for rendering.");
        //            }
        //            catch (System.Runtime.InteropServices.COMException)
        //            {
        //                /*No logging is needed. Is this a strange random UWP bug ?*/
        //            }

        //            var size = zebbleParent.GetNativeSize(nativeChild);
        //            NativeElement.Width = size.Width;
        //            NativeElement.Height = size.Height;
        //            (zebbleParent as Canvas)?.ApplyClip(NativeElement);
        //            controls.Canvas.SetLeft(nativeChild, zebbleParent.ActualX);
        //            controls.Canvas.SetTop(nativeChild, zebbleParent.ActualY);

        //            if (zebbleParent.ActualY == 0 && parent.CurrentChildren.IndexOf(zebbleParent) != 0)
        //            {
        //                for (var i = 1; i < 3; i++)
        //                {
        //                    await Task.Delay(10);
        //                    if (zebbleParent.ActualY != 0) break;
        //                }

        //                controls.Canvas.SetTop(nativeChild, zebbleParent.ActualY);
        //            }

        //            OnVisibilityChanged();
        //        }
        //    }

        //    zebbleParent.BroughtToFront.Raise().ContinueWith(x => zebbleParent.PushBackToZIndexOrder()).RunInParallel();
        //}
    }
}
