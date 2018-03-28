//------------------------------------------------------------------------------
// <copyright file="SoapRpcMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Web.Services.Description;
    using System.Runtime.InteropServices;

    /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SoapRpcMethodAttribute : System.Attribute {
        string action;
        string requestName;
        string responseName;
        string requestNamespace;
        string responseNamespace;
        bool oneWay;
        string binding;
        SoapBindingUse use = SoapBindingUse.Encoded;
        
        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.SoapRpcMethodAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapRpcMethodAttribute() {
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.SoapRpcMethodAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapRpcMethodAttribute(string action) {
            this.action = action;
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.Action"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Action {
            get { return action; }
            set { action = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Binding {
            get { return binding == null ? string.Empty : binding; }
            set { binding = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool OneWay {
            get { return oneWay; }
            set { oneWay = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.RequestNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string RequestNamespace {
            get { return requestNamespace; }
            set { requestNamespace = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.ResponseNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ResponseNamespace {
            get { return responseNamespace; }
            set { responseNamespace = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.RequestElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string RequestElementName {
            get { return requestName == null ? string.Empty : requestName; }
            set { requestName = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.ResponseElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ResponseElementName {
            get { return responseName == null ? string.Empty : responseName; }
            set { responseName = value; }
        }

        /// <include file='doc\SoapRpcMethodAttribute.uex' path='docs/doc[@for="SoapRpcMethodAttribute.Use"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public SoapBindingUse Use {
            get { return use; }
            set { use = value; }
        }
    }

}
