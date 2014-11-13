//------------------------------------------------------------------------------
// <copyright file="SoapDocumentMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Web.Services.Description;

    /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SoapDocumentMethodAttribute : System.Attribute {
        string action;
        string requestName;
        string responseName;
        string requestNamespace;
        string responseNamespace;
        bool oneWay;
        SoapBindingUse use = SoapBindingUse.Default;
        SoapParameterStyle style = SoapParameterStyle.Default;
        string binding;
        
        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.SoapDocumentMethodAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapDocumentMethodAttribute() {
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.SoapDocumentMethodAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapDocumentMethodAttribute(string action) {
            this.action = action;
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.Action"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Action {
            get { return action; }
            set { action = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool OneWay {
            get { return oneWay; }
            set { oneWay = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.RequestNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string RequestNamespace {
            get { return requestNamespace; }
            set { requestNamespace = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.ResponseNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ResponseNamespace {
            get { return responseNamespace; }
            set { responseNamespace = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.RequestElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string RequestElementName {
            get { return requestName == null ? string.Empty : requestName; }
            set { requestName = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.ResponseElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ResponseElementName {
            get { return responseName == null ? string.Empty : responseName; }
            set { responseName = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.Use"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.ParameterStyle"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapParameterStyle ParameterStyle {
            get { return style; }
            set { style = value; }
        }

        /// <include file='doc\SoapDocumentMethodAttribute.uex' path='docs/doc[@for="SoapDocumentMethodAttribute.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Binding {
            get { return binding == null ? string.Empty : binding; }
            set { binding = value; }
        }
    }

}
