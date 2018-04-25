//------------------------------------------------------------------------------
// <copyright file="RequestValidationSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Describes the collection passed into a request validator
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Util {
    using System.Diagnostics.CodeAnalysis;

    public enum RequestValidationSource {
        QueryString,
        Form,
        Cookies,
        Files,
        RawUrl,
        Path,
        PathInfo,
        Headers
    }

}
