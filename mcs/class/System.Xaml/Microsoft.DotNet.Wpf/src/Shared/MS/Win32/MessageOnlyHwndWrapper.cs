
using System;
using System.Threading;
using System.Security ; 
using System.Runtime.InteropServices;

namespace MS.Win32
{
    // Specialized version of HwndWrapper for message-only windows.

    internal class MessageOnlyHwndWrapper : HwndWrapper
    {
        /// <SecurityNote>
	    ///    Critical: This code calls into base class which is critical
        /// </SecurityNote>
        [SecurityCritical]
        public MessageOnlyHwndWrapper() : base(0, 0, 0, 0, 0, 0, 0, "", NativeMethods.HWND_MESSAGE, null)
        {
        }
    }
}
