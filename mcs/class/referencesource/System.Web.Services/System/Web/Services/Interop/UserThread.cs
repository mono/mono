namespace System.Web.Services.Interop {
    using System;
    using System.Threading;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class UserThread {
        internal int pSidBuffer; // byte * to buffer of size dwSidLen.
        internal int dwSidLen;
        internal int dwTid;

        internal UserThread() {
            pSidBuffer = 0;
            dwSidLen = 0;
            dwTid = 0;
        }

        public override bool Equals(object obj) {
            if (! (obj is UserThread)) {
                return false;
            }

            UserThread ut = (UserThread) obj;

            if (ut.dwTid == this.dwTid &&
                ut.pSidBuffer == this.pSidBuffer &&
                ut.dwSidLen == this.dwSidLen) {
                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}    
