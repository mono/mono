//------------------------------------------------------------------------------
// <copyright file="WebReferenceCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System;
    using System.Net;
    using System.Web.Services.Description;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Web.Services.Protocols;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Threading;
    using System.CodeDom;
    using System.Web.Services.Discovery;

    /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class WebReferenceCollection : CollectionBase {
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebReference this[int index] {
            get { return (WebReference)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(WebReference webReference) {
            return List.Add(webReference);
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, WebReference webReference) {
            List.Insert(index, webReference);
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(WebReference webReference) {
            return List.IndexOf(webReference);
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(WebReference webReference) {
            return List.Contains(webReference);
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(WebReference webReference) {
            List.Remove(webReference);
        }
        
        /// <include file='doc\WebReferenceCollection.uex' path='docs/doc[@for="WebReferenceCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(WebReference[] array, int index) {
            List.CopyTo(array, index);
        }
    }
}
