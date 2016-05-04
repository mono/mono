// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {
    
    using System;
    using System.Runtime.ConstrainedExecution;
    
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class UnhandledExceptionEventArgs : EventArgs {
        private Object _Exception;
        private bool _IsTerminating;

        public UnhandledExceptionEventArgs(Object exception, bool isTerminating) {
            _Exception = exception;
            _IsTerminating = isTerminating;
        }
        public Object ExceptionObject { 
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get { return _Exception; }
        }
        public bool IsTerminating {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get { return _IsTerminating; }
        }
    }
}
