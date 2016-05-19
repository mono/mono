//------------------------------------------------------------------------------
// <copyright file="XmlSerializerNamespaces.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.Xml.Schema;
    using System;

    /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSerializerNamespaces {
        Hashtable namespaces = null;

        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.XmlSerializerNamespaces"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerNamespaces() {
        }


        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.XmlSerializerNamespaces1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerNamespaces(XmlSerializerNamespaces namespaces) {
            this.namespaces = (Hashtable)namespaces.Namespaces.Clone();
        }

        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.XmlSerializerNamespaces2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerNamespaces(XmlQualifiedName[] namespaces) {
            for (int i = 0; i < namespaces.Length; i++) {
                XmlQualifiedName qname = namespaces[i];
                Add(qname.Name, qname.Namespace);
            }
        }
        
        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(string prefix, string ns) {
            // parameter value check
            if (prefix != null && prefix.Length > 0)
                XmlConvert.VerifyNCName(prefix);

            if (ns != null && ns.Length > 0)
                XmlConvert.ToUri(ns);
            AddInternal(prefix, ns);
        }

        internal void AddInternal(string prefix, string ns) {
            Namespaces[prefix] = ns;
        }

        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.ToArray"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlQualifiedName[] ToArray() {
            if (NamespaceList == null)
                return new XmlQualifiedName[0];
            return (XmlQualifiedName[])NamespaceList.ToArray(typeof(XmlQualifiedName));
        }

        /// <include file='doc\XmlSerializerNamespaces.uex' path='docs/doc[@for="XmlSerializerNamespaces.Count"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Count {
            get { return Namespaces.Count; }
        }

        internal ArrayList NamespaceList {
            get {
                if (namespaces == null || namespaces.Count == 0)
                    return null;
                ArrayList namespaceList = new ArrayList();
                foreach(string key in Namespaces.Keys) {
                    namespaceList.Add(new XmlQualifiedName(key, (string)Namespaces[key]));
                }
                return namespaceList;
            }
        }

        internal Hashtable Namespaces {
            get {
                if (namespaces == null)
                    namespaces = new Hashtable();
                return namespaces;
            }
            set { namespaces = value; }
        }

        internal string LookupPrefix(string ns) {
            if (string.IsNullOrEmpty(ns))
                return null;
            if (namespaces == null || namespaces.Count == 0)
                return null;

            foreach(string prefix in namespaces.Keys) {
                if (!string.IsNullOrEmpty(prefix) && (string)namespaces[prefix] == ns) {
                    return prefix;
                }
            }
            return null;
        }
    }
}

