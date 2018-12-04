using System;
using System.Windows;
using System.Windows.Media;
using System.Security;
using System.Security.Permissions;
using MS.Win32;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace MS.Internal
{
    public static class PointUtil
    {        
        /// <summary>
        ///     Convert a point from "client" coordinate space of a window into
        ///     the coordinate space of the root element of the same window.
        /// </summary>
        /// <SecurityNote>
        ///    Critical: This code accesses presentationSource
        ///    TreatAsSafe: Transforming a Point is considered safe. 
        /// </SecurityNote>
        [SecuritySafeCritical]       
        public static Point ClientToRoot(Point pt, PresentationSource presentationSource)
        {
            // Convert from pixels into measure units.
            pt = presentationSource.CompositionTarget.TransformFromDevice.Transform(pt);

            // REVIEW:
            // We need to include the root element's transform until the MIL
            // team fixes their APIs to do this.
            pt = ApplyVisualTransform(pt, presentationSource.RootVisual, true);

            return pt;
        }

        /// <summary>
        ///     Convert a point from the coordinate space of a root element of
        ///     a window into the "client" coordinate space of the same window.
        /// </summary>
        /// <SecurityNote>
        ///    Critical: This code accesses presentationSource
        ///    TreatAsSafe: Transforming a point is considered safe. 
        /// </SecurityNote>
        [SecuritySafeCritical]              
        public static Point RootToClient(Point pt, PresentationSource presentationSource)
        {
            // REVIEW:
            // We need to include the root element's transform until the MIL
            // team fixes their APIs to do this.
            pt = ApplyVisualTransform(pt, presentationSource.RootVisual, false);

            // Convert from measure units into pixels.
            pt = presentationSource.CompositionTarget.TransformToDevice.Transform(pt);

            return pt;
        }
        
        /// <summary>
        ///     Convert a point from "above" the coordinate space of a
        ///     visual into the the coordinate space "below" the visual.
        /// </summary>
        public static Point ApplyVisualTransform(Point pt, Visual v, bool inverse)
        {
            // Notes:
            // 1) First of all the MIL should provide a way of transforming
            //    a point from the window to the root element.
            // 2) A visual can currently have two properties that affect
            //    its coordinate space:
            //    A) Transform - any matrix
            //    B) Offset - a simpification for just a 2D offset.
            // 3) In the future a Visual may have other properties that
            //    affect its coordinate space, which is why the MIL should
            //    provide this API in the first place.
            //
            // The following code was copied from the MIL's TransformToAncestor
            // method on 12/16/2005.
            //
            if(v != null)
            {
                Matrix m = GetVisualTransform(v);

                if (inverse)
                {
                    m.Invert();
                }

                pt = m.Transform(pt);
            }
            return pt;
        }

        /// <summary>
        ///     Gets the matrix that will convert a point 
        ///     from "above" the coordinate space of a visual
        ///     into the the coordinate space "below" the visual.
        /// </summary>
        internal static Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix m = Matrix.Identity;

                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix cm = transform.Value;
                    m = Matrix.Multiply(m, cm);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                m.Translate(offset.X, offset.Y);

                return m;
            }

            return Matrix.Identity;
        }

        /// <SecurityNote>
        /// SecurityCritical: This code causes eleveation to unmanaged code via call to GetWindowLong and UnsecureGetHandle
        /// SecurityTreatAsSafe: This data is ok to give out
        /// validate all code paths that lead to this.
        /// </SecurityNote>
        /// <summary>
        ///     Convert a point from "client" coordinate space of a window into
        ///     the coordinate space of the screen.
        /// </summary>
        [SecuritySafeCritical]
        public static Point ClientToScreen(Point ptClient, PresentationSource presentationSource)
        {
            // For now we only know how to use HwndSource.
            HwndSource inputSource = presentationSource as HwndSource;
            if(inputSource == null)
            {
                return ptClient;
            }
            
            // Convert the point to screen coordinates.
            NativeMethods.POINT ptScreen = new NativeMethods.POINT((int)ptClient.X, (int)ptClient.Y);

            // MITIGATION: AVALON_RTL_AND_WIN32RTL
            //
            // When a window is marked with the WS_EX_LAYOUTRTL style, Win32
            // mirrors the coordinates during the various translation APIs.
            //
            // Avalon also sets up mirroring transforms so that we properly
            // mirror the output since we render to DirectX, not a GDI DC.
            //
            // Unfortunately, this means that our coordinates are already mirrored
            // by Win32, and Avalon mirrors them again.  To work around this
            // problem, we un-mirror the coordinates from Win32 before hit-testing
            // in Avalon.
            //

            //
            // Assert for unamanaged code permission to get to the handle. 
            // Note that we can't use HwndSource.UnsecureHandle here - as this method is called cross-assembly
            // 
            HandleRef handleRef ;             
            new UIPermission(UIPermissionWindow.AllWindows).Assert(); // BlessedAssert: 
            try
            {
                handleRef = new HandleRef( inputSource, inputSource.Handle ); 
            }
            finally
            {
                UIPermission.RevertAssert(); 
            }
            
            int windowStyle = UnsafeNativeMethods.GetWindowLong( handleRef, NativeMethods.GWL_EXSTYLE);
            if ((windowStyle & NativeMethods.WS_EX_LAYOUTRTL) == NativeMethods.WS_EX_LAYOUTRTL)
            {
                NativeMethods.RECT rcClient = new NativeMethods.RECT();
                SafeNativeMethods.GetClientRect( handleRef , ref rcClient);
                ptScreen.x = rcClient.right - ptScreen.x;
            }

            UnsafeNativeMethods.ClientToScreen( handleRef , ptScreen);
            
            return new Point(ptScreen.x, ptScreen.y);
        }

        /// <summary>
        ///     Convert a point from the coordinate space of the screen into
        ///     the "client" coordinate space of a window.
        /// </summary>
        /// <SecurityNote>
        ///    Critical: This code accesses presentationSource
        ///    TreatAsSafe: Transforming a Point is considered safe. 
        /// </SecurityNote>
        [SecuritySafeCritical]         
        internal static Point ScreenToClient(Point ptScreen, PresentationSource presentationSource)
        {
            // For now we only know how to use HwndSource.
            HwndSource inputSource = presentationSource as HwndSource;
            if(inputSource == null)
            {
                return ptScreen;
            }

            HandleRef handleRef ;
            new UIPermission(UIPermissionWindow.AllWindows).Assert(); // BlessedAssert: 
            try
            {
                handleRef = new HandleRef( inputSource, inputSource.Handle ); 
            }
            finally
            {
                UIPermission.RevertAssert();
            }
            
            // Convert the point from screen coordinates back to client coordinates.
            NativeMethods.POINT ptClient = new NativeMethods.POINT((int)ptScreen.X, (int)ptScreen.Y);
            SafeNativeMethods.ScreenToClient(handleRef , ptClient);

            // MITIGATION: WIN32_AND_AVALON_RTL
            //
            // When a window is marked with the WS_EX_LAYOUTRTL style, Win32
            // mirrors the coordinates during the various translation APIs.
            //
            // Avalon also sets up mirroring transforms so that we properly
            // mirror the output since we render to DirectX, not a GDI DC.
            //
            // Unfortunately, this means that our coordinates are already mirrored
            // by Win32, and Avalon mirrors them again.  To work around this
            // problem, we un-mirror the coordinates from Win32 before hit-testing
            // in Avalon.
            //
            int windowStyle = SafeNativeMethods.GetWindowStyle( handleRef , true);
            if ((windowStyle & NativeMethods.WS_EX_LAYOUTRTL) == NativeMethods.WS_EX_LAYOUTRTL)
            {
                NativeMethods.RECT rcClient = new NativeMethods.RECT();
                SafeNativeMethods.GetClientRect(handleRef , ref rcClient);
                ptClient.x = rcClient.right - ptClient.x;
            }

            return new Point(ptClient.x, ptClient.y);
        }
    }
}


