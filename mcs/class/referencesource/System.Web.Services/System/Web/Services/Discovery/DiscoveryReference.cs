//------------------------------------------------------------------------------
// <copyright file="DiscoveryReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.Xml.Serialization;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Collections;
    using System.Diagnostics;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class DiscoveryReference {

        private DiscoveryClientProtocol clientProtocol;

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.ClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public DiscoveryClientProtocol ClientProtocol {
            get { return clientProtocol; }
            set { clientProtocol = value; }
        }

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.DefaultFilename"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public virtual string DefaultFilename {
            get {
                return FilenameFromUrl(Url);
            }
        }

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.WriteDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void WriteDocument(object document, Stream stream);
        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.ReadDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract object ReadDocument(Stream stream);

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public abstract string Url {
            get;
            set;
        }

        internal virtual void LoadExternals(Hashtable loadedExternals) {
        }

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.FilenameFromUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string FilenameFromUrl(string url) {
            // get everything after the last /, not including the one at the end of the string
            int lastSlash = url.LastIndexOf('/', url.Length - 1);
            if (lastSlash >= 0) url = url.Substring(lastSlash + 1);

            // get everything up to the first dot (the filename)
            int firstDot = url.IndexOf('.');
            if (firstDot >= 0) url = url.Substring(0, firstDot);

            // make sure we don't include the question mark and stuff that follows it
            int question = url.IndexOf('?');
            if (question >= 0) url = url.Substring(0, question);
            if (url == null || url.Length == 0)
                return "item";
            return MakeValidFilename(url);
        }

        private static bool FindChar(char ch, char[] chars) {
            for (int i = 0; i < chars.Length; i++) {
                if (ch == chars[i])
                    return true;
            }
            return false;
        }

        internal static string MakeValidFilename(string filename) {
            if (filename == null)
                return null;

            StringBuilder sb = new StringBuilder(filename.Length);
            for (int i = 0; i < filename.Length; i++) {
                char c = filename[i];
                if (!FindChar(c, Path.InvalidPathChars))
                    sb.Append(c);
            }
            string name = sb.ToString();
            if (name.Length == 0)
                name = "item";

            return Path.GetFileName(name);
        }

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.Resolve"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Resolve() {
            if (ClientProtocol == null)
                throw new InvalidOperationException(Res.GetString(Res.WebResolveMissingClientProtocol));

            if (ClientProtocol.Documents[Url] != null)
                return;
            if (ClientProtocol.InlinedSchemas[Url] != null)
                return;

            string newUrl = Url;
            string oldUrl = Url;
            string contentType = null;
            Stream stream = ClientProtocol.Download(ref newUrl, ref contentType);
            if (ClientProtocol.Documents[newUrl] != null) {
                Url = newUrl;
                return;
            }
            try {
                Url = newUrl;
                Resolve(contentType, stream);
            }
            catch {
                Url = oldUrl;
                throw;
            }
            finally {
                stream.Close();
            }
        }

        internal Exception AttemptResolve(string contentType, Stream stream) {
            try {
                Resolve(contentType, stream);
                return null;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "AttemptResolve", e);
                return e;
            }
        }

        /// <include file='doc\DiscoveryReference.uex' path='docs/doc[@for="DiscoveryReference.Resolve1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected internal abstract void Resolve(string contentType, Stream stream);

        internal static string UriToString(string baseUrl, string relUrl) {
            return (new Uri(new Uri(baseUrl), relUrl)).GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
        }
    }
}
