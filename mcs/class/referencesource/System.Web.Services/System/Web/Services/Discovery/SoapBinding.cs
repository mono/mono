//------------------------------------------------------------------------------
// <copyright file="SoapBinding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Diagnostics;

    /// <include file='doc\SoapBinding.uex' path='docs/doc[@for="SoapBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlRoot("soap", Namespace = SoapBinding.Namespace)]
    public sealed class SoapBinding {

        /// <include file='doc\SoapBinding.uex' path='docs/doc[@for="SoapBinding.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = "http://schemas.xmlsoap.org/disco/soap/";

        private XmlQualifiedName binding;
        private string address = "";

        /// <include file='doc\SoapBinding.uex' path='docs/doc[@for="SoapBinding.Address"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("address")]
        public string Address {
            get { return address; }
            set {
                if (value == null)
                    address = "";
                else
                    address = value;
            }
        }

        /// <include file='doc\SoapBinding.uex' path='docs/doc[@for="SoapBinding.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("binding")]
        public XmlQualifiedName Binding {
            get { return binding; }
            set { binding = value; }
        }

    }

}
