using System;
using System.Threading.Tasks;
using controls = Windows.UI.Xaml.Controls;
using xaml = Windows.UI.Xaml;
using Olive;

namespace Zebble
{
    partial class CurrentStep
    {
        async Task ChangeParent(View view, View newParent, float top = 0, float left = 0)
        {
            RemoveParent(view.Native());

            AddToNativeParent(newParent, view.Native(), top, left);
        }

        void RemoveParent(xaml.FrameworkElement native)
        {
            var parent = native.Parent as xaml.FrameworkElement;
            if (parent is controls.Panel panel)
                panel.Children.Remove(native);
            else
                Log.For(this).Error(null, $"The item is not removed., type: {parent.GetType()}");
        }

        void AddToNativeParent(View parent, xaml.UIElement nativeChild, float top, float left)
        {
            var nativeParent = parent?.Native() as xaml.UIElement;

            if (nativeParent is controls.Border wrapper) nativeParent = wrapper.Child;
            
            // High concurrency. Already disposed:
            if (parent == null || parent.IsDisposed || nativeParent == null || nativeChild == null) return;
            
            try
            {
                if (nativeParent is controls.Panel panel)
                    panel.Children.Add(nativeChild);
                else if (nativeParent is controls.ScrollViewer scroller)
                    (scroller.Content as controls.Panel)?.Children.Add(nativeChild);
                else throw new Exception(nativeParent.GetType().Name +
                        " is not a supported container for rendering.");
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                /*No logging is needed. Is this a strange random UWP bug ?*/
            }
                    
            controls.Canvas.SetLeft(nativeChild, left);
            controls.Canvas.SetTop(nativeChild, top);
        }
    }
}
