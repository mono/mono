// CS1501: No overload for method `XCreateIC' takes `1' arguments
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
                        XCreateIC (IntPtr.Zero);
                }
        }
}


