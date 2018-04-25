//------------------------------------------------------------------------------
// <copyright file="PerformanceCounterPermissionAccess.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Flags]         
    public enum PerformanceCounterPermissionAccess {
        
        [Obsolete("This member has been deprecated.  Use System.Diagnostics.PerformanceCounter.PerformanceCounterPermissionAccess.Read instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Browse = 1,
        
#pragma warning disable 618        
        [Obsolete("This member has been deprecated.  Use System.Diagnostics.PerformanceCounter.PerformanceCounterPermissionAccess.Write instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Instrument = 2 | Browse,
#pragma warning restore 618        

        None = 0,

        Read = 1,
        
        Write = 2,

        Administer = 4 | Read | Write,
    }    
}  
  
