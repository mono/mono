//------------------------------------------------------------------------------
// <copyright file="SoapHeaders.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;
    using System.Security.Permissions;

    /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class SoapHeaderCollection : CollectionBase {
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeader this[int index] {
            get { return (SoapHeader)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(SoapHeader header) {
            return List.Add(header);
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, SoapHeader header) {
            List.Insert(index, header);
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(SoapHeader header) {
            return List.IndexOf(header);
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(SoapHeader header) {
            return List.Contains(header);
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(SoapHeader header) {
            List.Remove(header);
        }
        
        /// <include file='doc\SoapHeaders.uex' path='docs/doc[@for="SoapHeaderCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(SoapHeader[] array, int index) {
            List.CopyTo(array, index);
        }
        
    }
}
