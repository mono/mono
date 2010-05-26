/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Web.UI;

    internal class ViewTypeParserFilter : PageParserFilter {

        private string _viewBaseType;
        private DirectiveType _directiveType = DirectiveType.Unknown;
        private bool _viewTypeControlAdded;

        public override void PreprocessDirective(string directiveName, IDictionary attributes) {
            base.PreprocessDirective(directiveName, attributes);

            string defaultBaseType = null;

            // If we recognize the directive, keep track of what it was. If we don't recognize
            // the directive then just stop.
            switch (directiveName) {
                case "page":
                    _directiveType = DirectiveType.Page;
                    defaultBaseType = typeof(ViewPage).FullName;
                    break;
                case "control":
                    _directiveType = DirectiveType.UserControl;
                    defaultBaseType = typeof(ViewUserControl).FullName;
                    break;
                case "master":
                    _directiveType = DirectiveType.Master;
                    defaultBaseType = typeof(ViewMasterPage).FullName;
                    break;
            }

            if (_directiveType == DirectiveType.Unknown) {
                // If we're processing an unknown directive (e.g. a register directive), stop processing
                return;
            }

            // Look for an inherit attribute
            string inherits = (string)attributes["inherits"];
            if (!String.IsNullOrEmpty(inherits)) {
                // If it doesn't look like a generic type, don't do anything special,
                // and let the parser do its normal processing
                if (IsGenericTypeString(inherits)) {
                    // Remove the inherits attribute so the parser doesn't blow up
                    attributes["inherits"] = defaultBaseType;

                    // Remember the full type string so we can later give it to the ControlBuilder
                    _viewBaseType = inherits;
                }
            }
        }

        private static bool IsGenericTypeString(string typeName) {
            // Detect C# and VB generic syntax
            // REVIEW: what about other languages?
            return typeName.IndexOfAny(new char[] { '<', '(' }) >= 0;
        }

        public override void ParseComplete(ControlBuilder rootBuilder) {
            base.ParseComplete(rootBuilder);

            // If it's our page ControlBuilder, give it the base type string
            ViewPageControlBuilder pageBuilder = rootBuilder as ViewPageControlBuilder;
            if (pageBuilder != null) {
                pageBuilder.PageBaseType = _viewBaseType;
            }
            ViewUserControlControlBuilder userControlBuilder = rootBuilder as ViewUserControlControlBuilder;
            if (userControlBuilder != null) {
                userControlBuilder.UserControlBaseType = _viewBaseType;
            }
        }

        public override bool ProcessCodeConstruct(CodeConstructType codeType, string code) {
            if (!_viewTypeControlAdded &&
                _viewBaseType != null &&
                _directiveType == DirectiveType.Master) {

                // If we're dealing with a master page that needs to have its base type set, do it here.
                // It's done by adding the ViewType control, which has a builder that sets the base type.

                // The code currently assumes that the file in question contains a code snippet, since
                // that's the item we key off of in order to know when to add the ViewType control.

                Hashtable attribs = new Hashtable();
                attribs["typename"] = _viewBaseType;
                AddControl(typeof(ViewType), attribs);
                _viewTypeControlAdded = true;
            }

            return base.ProcessCodeConstruct(codeType, code);
        }

        // Everything else in this class is unrelated to our 'inherits' handling.
        // Since PageParserFilter blocks everything by default, we need to unblock it

        public override bool AllowCode {
            get {
                return true;
            }
        }

        public override bool AllowBaseType(Type baseType) {
            return true;
        }

        public override bool AllowControl(Type controlType, ControlBuilder builder) {
            return true;
        }

        public override bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType) {
            return true;
        }

        public override bool AllowServerSideInclude(string includeVirtualPath) {
            return true;
        }

        public override int NumberOfControlsAllowed {
            get {
                return -1;
            }
        }

        public override int NumberOfDirectDependenciesAllowed {
            get {
                return -1;
            }
        }

        public override int TotalNumberOfDependenciesAllowed {
            get {
                return -1;
            }
        }

        private enum DirectiveType {
            Unknown,
            Page,
            UserControl,
            Master,
        }
    }
}
