using System;                       // InvalidOperationException
using System.Windows;               // UIElement, etc.
using MS.Internal;                  // PointUtil
using System.Windows.Media;         // Matrix

namespace DRT
{
    // base class for a DRT application that uses the PointUtil class
    // from Avalon shared dev source
    public abstract partial class DrtBase
    {
        #region Input
        /// <summary>
        /// Move the mouse to the specified position within the given element.  x and y are
        /// coordinates within the element, (0,0) is upper left, (1,1) is lower right.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool MoveMouse(UIElement target, double x, double y)
        {
            // This code is paraphrased from Popup.cs.

            PresentationSource source = PresentationSource.FromVisual(target);
            if (source == null) return false;

            // Transform (0,0) from the target element up to the root.
            Point ptTarget = new Point(0,0);
            GeneralTransform transform;
            try
            {
                transform = target.TransformToAncestor(source.RootVisual);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine( "  MoveMouse to ({0},{1}) of {2} failed",
                                   x, y, Identify( target ) );
                // if visuals are not connected...
                return false;
            }
            Point ptRoot;
            transform.TryTransform(ptTarget, out ptRoot);
            Point ptClient = PointUtil.RootToClient(ptRoot, source);
            Point ptScreen = PointUtil.ClientToScreen(ptClient, source);

            // Get the width of the target element in pixels.
            Point size = source.CompositionTarget.TransformToDevice.Transform(new Point(target.RenderSize.Width, target.RenderSize.Height));

            Point moveToPoint = new Point(ptScreen.X + size.X * x, ptScreen.Y + size.Y * y);

            MoveMouse(moveToPoint);

            return true;
        }
        #endregion
    }
}

