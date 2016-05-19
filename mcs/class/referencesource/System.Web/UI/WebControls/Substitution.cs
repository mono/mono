//------------------------------------------------------------------------------
// <copyright file="Substitution.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;


    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    [
    DefaultProperty("MethodName"),
    Designer("System.Web.UI.Design.WebControls.SubstitutionDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ]
    public class Substitution : Control {


        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public Substitution() {
        }


        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.Substitution_MethodNameDescr)
        ]
        public virtual string MethodName {
            get {
                string s = ViewState["MethodName"] as string;
                return s == null? String.Empty : s;
            }
            set {
                ViewState["MethodName"] = value;
            }
        }


        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        // SECURITY CODE 













        // VSWhidbey 253188: Permission assert to support page running below full trust
        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private HttpResponseSubstitutionCallback GetDelegate(Type targetType, string methodName) {
            return (HttpResponseSubstitutionCallback)Delegate.CreateDelegate(
                        typeof(HttpResponseSubstitutionCallback), targetType, methodName);
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // VSWhidbey 84748: Temp fix, throw if we are within a cached control
            Control parent = Parent;
            while (parent != null) {
                if (parent is BasePartialCachingControl) {
                    throw new HttpException(
                        SR.GetString(SR.Substitution_CannotBeInCachedControl));
                }
                parent = parent.Parent;
            }
        }


        protected internal override void Render(HtmlTextWriter writer) {
            RenderMarkup(writer);
        }

        internal void RenderMarkup(HtmlTextWriter writer) {
            if (MethodName.Length == 0) {
                return;
            }

            TemplateControl target = TemplateControl;
            if (target == null) {
                return;
            }

            // get the delegate to the method
            HttpResponseSubstitutionCallback callback = null;

            try {
                 callback = GetDelegate(target.GetType(), MethodName);
            }
            catch {
            }

            if (callback == null) {
                throw new HttpException(
                    SR.GetString(SR.Substitution_BadMethodName, MethodName));
            }

            // add the substitution to the response
            Page.Response.WriteSubstitution(callback);
        }
    }
}
