namespace System.Web.Services.Interop {
    using System;
    using System.Threading;
    using System.Runtime.InteropServices;
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CallId {        
        public string szMachine; 
        public int dwPid;        
        public IntPtr userThread;        
        public long addStackPointer;        
        public string szEntryPoint;
        public string szDestinationMachine;

        public CallId(string machine, int pid, IntPtr userThread, long stackPtr, string entryPoint, string destMachine) {
            this.szMachine = machine;
            this.dwPid = pid;
            this.userThread = userThread;
            this.addStackPointer = stackPtr;
            this.szEntryPoint = entryPoint;
            this.szDestinationMachine = destMachine;
        }

    }
}    
         
