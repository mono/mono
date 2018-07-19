//------------------------------------------------------------------------------
// <copyright file="IUserControlTypeResolutionService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;

    /// <devdoc>
    /// Implemented by the designer host to participate in user control type resolution.
    /// </devdoc>
    public interface IUserControlTypeResolutionService {


        Type GetType(string tagPrefix, string tagName);
    }
}
