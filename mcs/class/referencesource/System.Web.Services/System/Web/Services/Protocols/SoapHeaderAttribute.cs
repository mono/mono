//------------------------------------------------------------------------------
// <copyright file="SoapHeaderAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;

    /// <include file='doc\SoapHeaderAttribute.uex' path='docs/doc[@for="SoapHeaderAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SoapHeaderAttribute : System.Attribute {
        string memberName;
        SoapHeaderDirection direction = SoapHeaderDirection.In;
        bool required = true;

        /// <include file='doc\SoapHeaderAttribute.uex' path='docs/doc[@for="SoapHeaderAttribute.SoapHeaderAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderAttribute(string memberName) {
            this.memberName = memberName;
        }

        /// <include file='doc\SoapHeaderAttribute.uex' path='docs/doc[@for="SoapHeaderAttribute.MemberName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string MemberName {
            get { return memberName == null ? string.Empty : memberName; }
            set { memberName = value; }
        }

        /// <include file='doc\SoapHeaderAttribute.uex' path='docs/doc[@for="SoapHeaderAttribute.Direction"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderDirection Direction {
            get { return direction; }
            set { direction = value; }
        }

        /// <include file='doc\SoapHeaderAttribute.uex' path='docs/doc[@for="SoapHeaderAttribute.Required"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("This property will be removed from a future version. The presence of a particular header in a SOAP message is no longer enforced", false)]
        public bool Required {
            get { return required; }
            set { required = value; }
        }
    }
}
