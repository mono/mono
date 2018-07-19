//------------------------------------------------------------------------------
// <copyright file="ApplicationFileParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

using System;
using System.Collections;
using System.IO;
using System.Web.Util;
using System.Web.Compilation;
using Debug=System.Web.Util.Debug;


/*
 * Parser for global.asax files
 */
internal sealed class ApplicationFileParser : TemplateParser {

    internal ApplicationFileParser() {}

    internal override Type DefaultBaseType { get { return PageParser.DefaultApplicationBaseType ?? typeof(System.Web.HttpApplication); } }

    internal override bool FApplicationFile { get { return true; } }

    internal const string defaultDirectiveName = "application";
    internal override string DefaultDirectiveName {
        get { return defaultDirectiveName; }
    }

    internal override void CheckObjectTagScope(ref ObjectTagScope scope) {

        // Map the default scope to AppInstance
        if (scope == ObjectTagScope.Default)
            scope = ObjectTagScope.AppInstance;

        // Check for invalid scopes
        if (scope == ObjectTagScope.Page) {
            throw new HttpException(
                SR.GetString(SR.Page_scope_in_global_asax));
        }
    }
}

}
