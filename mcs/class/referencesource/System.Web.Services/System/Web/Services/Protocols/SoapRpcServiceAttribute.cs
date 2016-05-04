//------------------------------------------------------------------------------
// <copyright file="SoapRpcServiceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Web.Services.Description;
    using System.Runtime.InteropServices;

    /// <include file='doc\SoapRpcServiceAttribute.uex' path='docs/doc[@for="SoapRpcServiceAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SoapRpcServiceAttribute : Attribute {
        SoapServiceRoutingStyle routingStyle = SoapServiceRoutingStyle.SoapAction;
        SoapBindingUse use = SoapBindingUse.Encoded;

        /// <include file='doc\SoapRpcServiceAttribute.uex' path='docs/doc[@for="SoapRpcServiceAttribute.SoapRpcServiceAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapRpcServiceAttribute() {
        }

        /// <include file='doc\SoapRpcServiceAttribute.uex' path='docs/doc[@for="SoapRpcServiceAttribute.RoutingStyle"]/*' />
        public SoapServiceRoutingStyle RoutingStyle {
            get { return routingStyle; }
            set { routingStyle = value; }
        }

        /// <include file='doc\SoapRpcServiceAttribute.uex' path='docs/doc[@for="SoapRpcServiceAttribute.Use"]/*' />
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
