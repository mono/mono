//------------------------------------------------------------------------------
// <copyright file="AsyncPreloadModeFlags.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * AsyncPreloadModeFlags preloads the request entity for form posts.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web.Configuration {
    using System;
    
    [Flags]
    public enum AsyncPreloadModeFlags {
        None            = 0x00,
        Form            = 0x01,
        FormMultiPart   = 0x02,
        NonForm         = 0x04,
        AllFormTypes    = Form | FormMultiPart,
	    All             = AllFormTypes | NonForm
    }
}
