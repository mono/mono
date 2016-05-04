//------------------------------------------------------------------------------
// <copyright file="UserControlParser.cs" company="Microsoft">
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
using System.Security;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Globalization;
using System.Web.Caching;


/*
 * Parser for declarative controls
 */
internal class UserControlParser : TemplateControlParser {

    private bool _fSharedPartialCaching;
    private string _provider;
    internal bool FSharedPartialCaching { get { return _fSharedPartialCaching ; } }
    internal string Provider { get { return _provider ; } }


    // Get default settings from config
    internal override void ProcessConfigSettings() {
        base.ProcessConfigSettings();

        ApplyBaseType();
    }

    // Get the default baseType from PagesConfig.
    internal virtual void ApplyBaseType() {
        if (PageParser.DefaultUserControlBaseType != null) {
            BaseType = PageParser.DefaultUserControlBaseType;
        }
        else if (PagesConfig != null && PagesConfig.UserControlBaseTypeInternal != null) {
            BaseType = PagesConfig.UserControlBaseTypeInternal;
        }
    }

    internal override Type DefaultBaseType { get { return typeof(System.Web.UI.UserControl); } }

    internal const string defaultDirectiveName = "control";
    internal override string DefaultDirectiveName {
        get { return defaultDirectiveName; }
    }

    internal override Type DefaultFileLevelBuilderType {
        get {
            return typeof(FileLevelUserControlBuilder);
        }
    }

    internal override RootBuilder CreateDefaultFileLevelBuilder() {

        return new FileLevelUserControlBuilder();
    }

    /*
     * Process the contents of the <%@ OutputCache ... %> directive
     */
    internal override void ProcessOutputCacheDirective(string directiveName, IDictionary directive) {
        string sqlDependency;

        Util.GetAndRemoveBooleanAttribute(directive, "shared", ref _fSharedPartialCaching);

        _provider = Util.GetAndRemoveNonEmptyAttribute(directive, "providerName");
        if (_provider == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
            _provider = null;
        }
        OutputCache.ThrowIfProviderNotFound(_provider);

        sqlDependency = Util.GetAndRemoveNonEmptyAttribute(directive, "sqldependency");

        if (sqlDependency != null) {
            // Validate the sqldependency attribute
            SqlCacheDependency.ValidateOutputCacheDependencyString(sqlDependency, false);
            OutputCacheParameters.SqlDependency = sqlDependency;
        }

        base.ProcessOutputCacheDirective(directiveName, directive);
    }

    internal override bool FVaryByParamsRequiredOnOutputCache {
        get { return OutputCacheParameters.VaryByControl == null; }
    }

    internal override string UnknownOutputCacheAttributeError {
        get { return SR.Attr_not_supported_in_ucdirective; }
    }
}

}
