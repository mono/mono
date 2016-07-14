using System;

namespace Microsoft.Win32
{
    static class NativeMethods
    {
        public const int E_ABORT = unchecked ((int)0x80004004);

        public static bool CloseProcess (IntPtr handle)
        {
        	// TODO:
        	return true;
        }
    }
}