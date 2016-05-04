//------------------------------------------------------------------------------
// <copyright file="ElapsedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Timers {

    using System;
    using System.Diagnostics;
    
    public class ElapsedEventArgs : EventArgs {   
        private DateTime signalTime;
        
        internal ElapsedEventArgs(int low, int high) {        
            long fileTime = (long)((((ulong)high) << 32) | (((ulong)low) & 0xffffffff));
            this.signalTime = DateTime.FromFileTime(fileTime);                        
        }
    
        public DateTime SignalTime {
            get {
                return this.signalTime;
            }
        }
    }
}
