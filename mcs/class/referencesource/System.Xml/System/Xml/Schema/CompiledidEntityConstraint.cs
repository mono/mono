//------------------------------------------------------------------------------
// <copyright file="CompiledIdentityConstraint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Text;
    using System.Collections;
    using System.Diagnostics;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;

    internal class CompiledIdentityConstraint {
        internal XmlQualifiedName name = XmlQualifiedName.Empty;    
        private ConstraintRole role;
        private Asttree selector;
        private Asttree[] fields;
        internal XmlQualifiedName refer = XmlQualifiedName.Empty;

        public enum ConstraintRole {
            Unique,
            Key,
            Keyref
        }

        public ConstraintRole Role {
            get { return this.role; }
        }

        public Asttree Selector {
            get { return this.selector; }
        }

        public Asttree[] Fields {
            get { return this.fields; }
        }

        public static readonly CompiledIdentityConstraint Empty = new CompiledIdentityConstraint();

        private CompiledIdentityConstraint() {}

        public CompiledIdentityConstraint(XmlSchemaIdentityConstraint constraint, XmlNamespaceManager nsmgr) {
            this.name = constraint.QualifiedName;

            //public Asttree (string xPath, bool isField, XmlNamespaceManager nsmgr)
            try {
                this.selector = new Asttree(constraint.Selector.XPath, false, nsmgr);
            }
            catch (XmlSchemaException e) {
                e.SetSource(constraint.Selector);
                throw e;
            }
            XmlSchemaObjectCollection fields = constraint.Fields;
            Debug.Assert(fields.Count > 0);
            this.fields = new Asttree[fields.Count];
            for(int idxField = 0; idxField < fields.Count; idxField ++) {
                try {
                    this.fields[idxField] = new Asttree(((XmlSchemaXPath)fields[idxField]).XPath, true, nsmgr);
                }
                catch (XmlSchemaException e) {
                    e.SetSource(constraint.Fields[idxField]);
                    throw e;
                }
            }
            if (constraint is XmlSchemaUnique) {
                this.role = ConstraintRole.Unique;
            } 
            else if (constraint is XmlSchemaKey) {
                this.role = ConstraintRole.Key;
            } 
            else {             // XmlSchemaKeyref
                this.role = ConstraintRole.Keyref;
                this.refer = ((XmlSchemaKeyref)constraint).Refer; 
            }
        }
    }

}


