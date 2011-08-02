// CS1502:  The best overloaded method match for `System.Windows.Forms.X11Xim.XCreateIC(System.IntPtr, __arglist)' has some invalid arguments
// Line: 16

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
        internal class X11Xim
        {
                [DllImport ("libX11", EntryPoint="XCreateIC")]
                internal extern static IntPtr XCreateIC(IntPtr xim, __arglist);

                public static void Main ()
                {
                        XCreateIC (IntPtr.Zero, IntPtr.Zero);
                }
        }
}


