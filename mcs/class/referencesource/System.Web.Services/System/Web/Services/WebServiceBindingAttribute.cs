//------------------------------------------------------------------------------
// <copyright file="WebServiceBindingAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {

    using System;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Runtime.InteropServices;

    /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class WebServiceBindingAttribute : Attribute {
        string name;
        string ns;
        string location;
        WsiProfiles claims = WsiProfiles.None;
        bool emitClaims;

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.WebServiceBindingAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebServiceBindingAttribute() {
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.WebServiceBindingAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebServiceBindingAttribute(string name) {
            this.name = name;
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.WebServiceBindingAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebServiceBindingAttribute(string name, string ns) {
            this.name = name;
            this.ns = ns;
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.WebServiceBindingAttribute3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebServiceBindingAttribute(string name, string ns, string location) {
            this.name = name;
            this.ns = ns;
            this.location = location;
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.ConformsTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WsiProfiles ConformsTo {
            get { return claims; }
            set { claims = value; }
        }

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.EmitConformanceClaims"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool EmitConformanceClaims {
            get { return emitClaims; }
            set { emitClaims = value; }
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.Location"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Location {
            get { return location == null ? string.Empty : location; }
            set { location = value; }
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Name {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }

        /// <include file='doc\WebServiceBindingAttribute.uex' path='docs/doc[@for="WebServiceBindingAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }
    }

    internal class WebServiceBindingReflector {
        private WebServiceBindingReflector() { }
        internal static WebServiceBindingAttribute GetAttribute(Type type) {
            for (; type != null; type = type.BaseType) {
                object[] attrs = type.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                if (attrs.Length == 0) continue;
                if (attrs.Length > 1) throw new ArgumentException(Res.GetString(Res.OnlyOneWebServiceBindingAttributeMayBeSpecified1, type.FullName), "type");
                return (WebServiceBindingAttribute)attrs[0];
            }
            return null;
        }

        internal static WebServiceBindingAttribute GetAttribute(LogicalMethodInfo methodInfo, string binding) {
            if (methodInfo.Binding != null) {
                if (binding.Length > 0 && methodInfo.Binding.Name != binding)
                    throw new InvalidOperationException(Res.GetString(Res.WebInvalidBindingName, binding, methodInfo.Binding.Name));
                return methodInfo.Binding;
            }
            Type type = methodInfo.DeclaringType;
            object[] attrs = type.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
            WebServiceBindingAttribute webAttr = null;
            foreach (WebServiceBindingAttribute attr in attrs) {
                if (attr.Name == binding) {
                    if (webAttr != null)
                        throw new ArgumentException(Res.GetString(Res.MultipleBindingsWithSameName2, type.FullName, binding, "methodInfo"));
                    webAttr = attr;
                }
            }
            if (webAttr == null && binding != null && binding.Length > 0)
                throw new ArgumentException(Res.GetString(Res.TypeIsMissingWebServiceBindingAttributeThat2, type.FullName, binding), "methodInfo");
            return webAttr;
        }
    }
}
