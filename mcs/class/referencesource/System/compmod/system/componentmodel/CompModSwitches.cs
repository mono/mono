//------------------------------------------------------------------------------
// <copyright file="CompModSwitches.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System.Configuration.Assemblies;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Threading;

    /// <internalonly/>    
    [HostProtection(SharedState = true)]
    internal static class CompModSwitches
    {
        
        private static volatile BooleanSwitch commonDesignerServices;
        private static volatile TraceSwitch eventLog;
                
        public static BooleanSwitch CommonDesignerServices {
            get {
                if (commonDesignerServices == null) {
                    commonDesignerServices = new BooleanSwitch("CommonDesignerServices", "Assert if any common designer service is not found.");
                }
                return commonDesignerServices;
            }
        }   
        
        public static TraceSwitch EventLog {
            get {
                if (eventLog == null) {
                    eventLog = new TraceSwitch("EventLog", "Enable tracing for the EventLog component.");
                }
                return eventLog;
            }
        }
                                                                                                                                                                               
    }
}

