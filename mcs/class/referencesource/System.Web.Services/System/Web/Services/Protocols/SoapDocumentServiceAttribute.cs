//------------------------------------------------------------------------------
// <copyright file="SoapDocumentServiceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System;
    using System.Web.Services.Description;
    using System.Reflection;
    using System.Xml.Serialization;

    /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SoapDocumentServiceAttribute : Attribute {
        SoapBindingUse use = SoapBindingUse.Default;
        SoapParameterStyle paramStyle = SoapParameterStyle.Default;
        SoapServiceRoutingStyle routingStyle = SoapServiceRoutingStyle.SoapAction;

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.SoapDocumentServiceAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapDocumentServiceAttribute() {
        }

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.SoapDocumentServiceAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapDocumentServiceAttribute(SoapBindingUse use) {
            this.use = use;
        }

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.SoapDocumentServiceAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapDocumentServiceAttribute(SoapBindingUse use, SoapParameterStyle paramStyle) {
            this.use = use;
            this.paramStyle = paramStyle;
        }

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.Use"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.ParameterStyle"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapParameterStyle ParameterStyle {
            get { return paramStyle; }
            set { paramStyle = value; }
        }

        /// <include file='doc\SoapDocumentServiceAttribute.uex' path='docs/doc[@for="SoapDocumentServiceAttribute.RoutingStyle"]/*' />
        public SoapServiceRoutingStyle RoutingStyle {
            get { return routingStyle; }
            set { routingStyle = value; }
        }
    }

}
