//------------------------------------------------------------------------------
// <copyright file="TextWriterTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Diagnostics {
    using System;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.Win32;

    [HostProtection(Synchronization=true)]
    public class ConsoleTraceListener : TextWriterTraceListener {

        public ConsoleTraceListener() : base (Console.Out) {
        }

        public ConsoleTraceListener(bool useErrorStream) : base (useErrorStream ? Console.Error : Console.Out) {
        }

        public override void Close() {
            // No resources to clean up.
        }
    }
}
 

        
