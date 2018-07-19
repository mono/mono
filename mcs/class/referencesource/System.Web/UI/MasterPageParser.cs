//------------------------------------------------------------------------------
// <copyright file="MasterPageParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET master page parser
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
    using System.Web;
    using System.Web.Util;

    /*
     * Parser for MasterPage
     */
    internal sealed class MasterPageParser : UserControlParser {

        internal override Type DefaultBaseType { get { return typeof(System.Web.UI.MasterPage); } }

        internal new const string defaultDirectiveName = "master";
        internal override string DefaultDirectiveName {
            get { return defaultDirectiveName; }
        }

        internal override Type DefaultFileLevelBuilderType {
            get {
                return typeof(FileLevelMasterPageControlBuilder);
            }
        }

        private Type _masterPageType;
        internal Type MasterPageType { get { return _masterPageType; } }
        
        private CaseInsensitiveStringSet _placeHolderList;
        internal CaseInsensitiveStringSet PlaceHolderList {
            get {
                if (_placeHolderList == null)
                    _placeHolderList = new CaseInsensitiveStringSet();

                return _placeHolderList;
            }
        }

        // Do not apply the basetype. Override this method
        // so the userControlbasetype do not affect masterpages.
        internal override void ApplyBaseType() {
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder() {
            return new FileLevelMasterPageControlBuilder();
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive) {

            if (StringUtil.EqualsIgnoreCase(directiveName, "masterType")) {
                if (_masterPageType != null) {
                    ProcessError(SR.GetString(SR.Only_one_directive_allowed, directiveName));
                    return;
                }

                _masterPageType = GetDirectiveType(directive, directiveName);
                Util.CheckAssignableType(typeof(MasterPage), _masterPageType);
            }
            // outputcaching is not allowed on masterpages.
            else if (StringUtil.EqualsIgnoreCase(directiveName, "outputcache")) {
                ProcessError(SR.GetString(SR.Directive_not_allowed, directiveName));
                return;
            }
            else {
                base.ProcessDirective(directiveName, directive);
            }
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name,
            string value, IDictionary parseData) {

            switch (name) {
                case "masterpagefile":
                    // Skip validity check for expression builder (e.g. <%$ ... %>)
                    if (IsExpressionBuilderValue(value)) return false;

                    if (value.Length > 0) {
                        // Add dependency on the Type by calling this method
                        Type type = GetReferencedType(value);
                        
                        Util.CheckAssignableType(typeof(MasterPage), type);
                    }

                    // Return false to let the generic attribute processing continue
                    return false;

                // outputcaching is not allowed on masterpages.
                case "outputcaching" : 

                    ProcessError(SR.GetString(SR.Attr_not_supported_in_directive,
                            name, DefaultDirectiveName));

                    // Return false to let the generic attribute processing continue
                    return false;

                default:
                    // We didn't handle the attribute.  Try the base class
                    return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
            }
        }
    }
}
