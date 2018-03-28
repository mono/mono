//------------------------------------------------------------------------------
// <copyright file="ADVF.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;

    /// <devdoc>
    /// </devdoc>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1712:DoNotPrefixEnumValuesWithTypeName")]
    public enum ADVF {
        ADVF_NODATA	            = 1,
        ADVF_PRIMEFIRST	        = 2,
        ADVF_ONLYONCE	        = 4,
        ADVF_DATAONSTOP	        = 64,
        ADVFCACHE_NOHANDLER	    = 8,
        ADVFCACHE_FORCEBUILTIN	= 16,
        ADVFCACHE_ONSAVE	    = 32
    }

    // Note: ADVF_ONLYONCE and ADVF_PRIMEFIRST values conform with objidl.dll but are backwards from 
    // the Platform SDK documentation as of 07/21/2003.
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/com/htm/oen_a2z_8jxi.asp.
    // See VSWhidbey bug#96162.
}


