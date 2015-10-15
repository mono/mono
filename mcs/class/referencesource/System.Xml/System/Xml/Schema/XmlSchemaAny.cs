//------------------------------------------------------------------------------
// <copyright file="XmlSchemaAny.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">Microsoft</owner>                                                               
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Text;

    /// <include file='doc\XmlSchemaAny.uex' path='docs/doc[@for="XmlSchemaAny"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaAny : XmlSchemaParticle {
        string ns;
        XmlSchemaContentProcessing processContents = XmlSchemaContentProcessing.None;
        NamespaceList namespaceList;
        
        /// <include file='doc\XmlSchemaAny.uex' path='docs/doc[@for="XmlSchemaAny.Namespaces"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("namespace")]
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlSchemaAny.uex' path='docs/doc[@for="XmlSchemaAny.ProcessContents"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("processContents"), DefaultValue(XmlSchemaContentProcessing.None)]
        public XmlSchemaContentProcessing ProcessContents {
            get { return processContents; }
            set { processContents = value; }
        }

        [XmlIgnore]
        internal NamespaceList NamespaceList {
            get { return namespaceList; }
        }

        [XmlIgnore]
        internal string ResolvedNamespace {
            get { 
                if (ns == null || ns.Length == 0) {
                    return "##any";
                }
                return ns;
            }
        }

        [XmlIgnore]
        internal XmlSchemaContentProcessing ProcessContentsCorrect {
            get { return processContents == XmlSchemaContentProcessing.None ? XmlSchemaContentProcessing.Strict : processContents; }
        }
        
        internal override string NameString {
            get {
                switch (namespaceList.Type) {
                    case NamespaceList.ListType.Any:
                        return "##any:*";
                    
                    case NamespaceList.ListType.Other:
                        return "##other:*";

                    case NamespaceList.ListType.Set:
                        StringBuilder sb = new StringBuilder();
                        int i = 1;
                        foreach(string wildcardNS in namespaceList.Enumerate) {
                            sb.Append(wildcardNS + ":*");
                            if (i < namespaceList.Enumerate.Count) {
                                sb.Append(" ");
                            }
                            i++;
                        }
                        return sb.ToString();

                    default:
                        return string.Empty;
                }
            }
        }

        internal void BuildNamespaceList(string targetNamespace) {
            if (ns != null) { //If namespace="" default to namespace="##any"
                namespaceList = new NamespaceList(ns, targetNamespace);
            }
            else {
                namespaceList = new NamespaceList();
            }
        }

        internal void BuildNamespaceListV1Compat(string targetNamespace) {
            if (ns != null) {
                namespaceList = new NamespaceListV1Compat(ns, targetNamespace);
            }
            else {
                namespaceList = new NamespaceList(); //This is only ##any, hence base class is sufficient
            }
        }

        internal bool Allows(XmlQualifiedName qname) {
            return namespaceList.Allows(qname.Namespace);
        }

    }

}
