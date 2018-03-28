//------------------------------------------------------------------------------
// <copyright file="ExcludePathInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Xml.Serialization;
    
    /// <include file='doc\ExcludePathInfo.uex' path='docs/doc[@for="ExcludePathInfo"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ExcludePathInfo {
        private string path = null;

        /// <include file='doc\ExcludePathInfo.uex' path='docs/doc[@for="ExcludePathInfo.ExcludePathInfo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ExcludePathInfo() {
        }

        /// <include file='doc\ExcludePathInfo.uex' path='docs/doc[@for="ExcludePathInfo.ExcludePathInfo1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ExcludePathInfo(string path) {
            this.path = path;
        }

        /// <include file='doc\ExcludePathInfo.uex' path='docs/doc[@for="ExcludePathInfo.Path"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("path")]
        public string Path {
            get {
                return path;
            }
            set {
                path = value;
            }
        }
    }
}
