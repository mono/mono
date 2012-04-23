namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;

    internal class ViewTypeParserFilter : PageParserFilter {

        private static Dictionary<string, Type> _directiveBaseTypeMappings = new Dictionary<string, Type> {
            { "page",    typeof(ViewPage)        },
            { "control", typeof(ViewUserControl) },
            { "master",  typeof(ViewMasterPage)  },
        };

        private string _inherits;

        [SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule", Justification = "System.Web.Mvc is SecurityTransparent and requires medium trust to run, so this downstream link demand is fine")]
        public ViewTypeParserFilter() {
        }

        [SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule", Justification = "System.Web.Mvc is SecurityTransparent and requires medium trust to run, so this downstream link demand is fine")]
        public override void PreprocessDirective(string directiveName, IDictionary attributes) {
            base.PreprocessDirective(directiveName, attributes);

            Type baseType;
            if (_directiveBaseTypeMappings.TryGetValue(directiveName, out baseType)) {
                string inheritsAttribute = attributes["inherits"] as string;

                // Since the ASP.NET page parser doesn't understand native generic syntax, we
                // need to swap out whatever the user provided with the default base type for
                // the given directive (page vs. control vs. master). We stash the old value
                // and swap it back in inside the control builder. Our "is this generic?"
                // check here really only works for C# and VB.NET, since we're checking for
                // < or ( in the type name.
                //
                // We only change generic directives, because doing so breaks back-compat
                // for property setters on @Page, @Control, and @Master directives. The user
                // can work around this breaking behavior by using a non-generic inherits
                // directive, or by using the CLR syntax for generic type names.

                if (inheritsAttribute != null && inheritsAttribute.IndexOfAny(new[] { '<', '(' }) > 0) {
                    attributes["inherits"] = baseType.FullName;
                    _inherits = inheritsAttribute;
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule", Justification = "System.Web.Mvc is SecurityTransparent and requires medium trust to run, so this downstream link demand is fine")]
        public override void ParseComplete(ControlBuilder rootBuilder) {
            base.ParseComplete(rootBuilder);

            IMvcControlBuilder builder = rootBuilder as IMvcControlBuilder;
            if (builder != null) {
                builder.Inherits = _inherits;
            }
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
    }
}
