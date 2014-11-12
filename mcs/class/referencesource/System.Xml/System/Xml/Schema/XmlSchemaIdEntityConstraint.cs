//------------------------------------------------------------------------------
// <copyright file="XmlSchemaIdentityConstraint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaIdentityConstraint"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaIdentityConstraint : XmlSchemaAnnotated {
        string name;        
        XmlSchemaXPath selector;
        XmlSchemaObjectCollection fields = new XmlSchemaObjectCollection();
		XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;
		CompiledIdentityConstraint compiledConstraint = null;

        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaIdentityConstraint.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("name")]
        public string Name { 
            get { return name; }
            set { name = value; }
        }

        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaIdentityConstraint.Selector"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("selector", typeof(XmlSchemaXPath))]
        public XmlSchemaXPath Selector {
            get { return selector; }
            set { selector = value; }
        }

        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaIdentityConstraint.Fields"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("field", typeof(XmlSchemaXPath))]
        public XmlSchemaObjectCollection Fields {
            get { return fields; }
        }
		
        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaIdentityConstraint.QualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
		[XmlIgnore]
		public XmlQualifiedName QualifiedName { 
			get { return qualifiedName; }
		}

		internal void  SetQualifiedName(XmlQualifiedName value) { 
			qualifiedName = value;
		}

		[XmlIgnore]
		internal CompiledIdentityConstraint CompiledConstraint {
			get { return compiledConstraint; }
			set { compiledConstraint = value; }
		}

        [XmlIgnore]
        internal override string NameAttribute {
            get { return Name; }
            set { Name = value; }
        }
    }

    /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaXPath"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaXPath : XmlSchemaAnnotated {
        string xpath;
        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaXPath.XPath"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("xpath"), DefaultValue("")]
        public string XPath {
            get { return xpath; }
            set { xpath = value; }
        }
    }

    /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaUnique"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaUnique : XmlSchemaIdentityConstraint {
    }

    /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaKey"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaKey : XmlSchemaIdentityConstraint {
    }

    /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaKeyref"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaKeyref : XmlSchemaIdentityConstraint {
        XmlQualifiedName refer = XmlQualifiedName.Empty; 

        /// <include file='doc\XmlSchemaIdentityConstraint.uex' path='docs/doc[@for="XmlSchemaKeyref.Refer"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("refer")]
        public XmlQualifiedName Refer { 
            get { return refer; }
            set { refer = (value == null ? XmlQualifiedName.Empty : value); }
        }
    }
}
