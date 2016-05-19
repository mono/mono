//------------------------------------------------------------------------------
// <copyright file="SchemaElementDecl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                               
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Collections.Generic;

    internal sealed class SchemaElementDecl : SchemaDeclBase, IDtdAttributeListInfo {
        Dictionary<XmlQualifiedName, SchemaAttDef> attdefs = new Dictionary<XmlQualifiedName, SchemaAttDef>();
        List<IDtdDefaultAttributeInfo> defaultAttdefs;
        bool isIdDeclared;
        bool hasNonCDataAttribute = false;

#if !SILVERLIGHT
        bool isAbstract = false;
        bool isNillable = false;
        bool hasRequiredAttribute = false;
        bool isNotationDeclared;
        Dictionary<XmlQualifiedName, XmlQualifiedName> prohibitedAttributes = new Dictionary<XmlQualifiedName, XmlQualifiedName>(); 
        ContentValidator contentValidator;
        XmlSchemaAnyAttribute anyAttribute;
        XmlSchemaDerivationMethod block;
        CompiledIdentityConstraint[] constraints;
        XmlSchemaElement schemaElement;

        internal static readonly SchemaElementDecl Empty = new SchemaElementDecl();
#endif

        //
// Constructor
//
#if !SILVERLIGHT
        internal SchemaElementDecl() {
        }
        
        internal SchemaElementDecl(XmlSchemaDatatype dtype) {
            Datatype = dtype;
            contentValidator = ContentValidator.TextOnly;
        }
#endif

        internal SchemaElementDecl(XmlQualifiedName name, String prefix) 
        : base(name, prefix) {
        }

//
// Static methods
//
#if !SILVERLIGHT
        internal static SchemaElementDecl CreateAnyTypeElementDecl() {
            SchemaElementDecl anyTypeElementDecl = new SchemaElementDecl();
            anyTypeElementDecl.Datatype = DatatypeImplementation.AnySimpleType.Datatype;
            return anyTypeElementDecl;
        }
#endif
        
//
// IDtdAttributeListInfo interface
//
#region IDtdAttributeListInfo Members

        string IDtdAttributeListInfo.Prefix {
            get { return ((SchemaElementDecl)this).Prefix; }
        }

        string IDtdAttributeListInfo.LocalName {
            get { return ((SchemaElementDecl)this).Name.Name; }
        }

        bool IDtdAttributeListInfo.HasNonCDataAttributes {
            get { return hasNonCDataAttribute; }
        }

        IDtdAttributeInfo IDtdAttributeListInfo.LookupAttribute(string prefix, string localName) {
            XmlQualifiedName qname = new XmlQualifiedName(localName, prefix);
            SchemaAttDef attDef;
            if (attdefs.TryGetValue(qname, out attDef)) {
                return attDef;
            }
            return null;
        }

        IEnumerable<IDtdDefaultAttributeInfo> IDtdAttributeListInfo.LookupDefaultAttributes() {
            return defaultAttdefs;
        }

        IDtdAttributeInfo IDtdAttributeListInfo.LookupIdAttribute() {
            foreach (SchemaAttDef attDef in attdefs.Values) {
                if (attDef.TokenizedType == XmlTokenizedType.ID) {
                    return (IDtdAttributeInfo)attDef;
                }
            }
            return null;
        }
#endregion

//
// SchemaElementDecl properties
//
        internal bool IsIdDeclared {
            get { return isIdDeclared;}
            set { isIdDeclared = value;}
        }

        internal bool HasNonCDataAttribute {
            get { return hasNonCDataAttribute; }
            set { hasNonCDataAttribute = value; }
        }

#if !SILVERLIGHT
        internal SchemaElementDecl Clone() {
            return (SchemaElementDecl) MemberwiseClone();
        }

        internal bool IsAbstract {
            get { return isAbstract;}
            set { isAbstract = value;}
        }

        internal bool IsNillable {
            get { return isNillable;}
            set { isNillable = value;}
        }

        internal XmlSchemaDerivationMethod Block {
             get { return block; }
             set { block = value; }
        }

        internal bool IsNotationDeclared {
            get { return isNotationDeclared; }
            set { isNotationDeclared = value; }
        }

        internal bool HasDefaultAttribute {
            get { return defaultAttdefs != null; }
        }

        internal bool HasRequiredAttribute {
            get { return hasRequiredAttribute; }
            set { hasRequiredAttribute = value; }
        }

        internal ContentValidator ContentValidator {
            get { return contentValidator;}
            set { contentValidator = value;}
        }

        internal XmlSchemaAnyAttribute AnyAttribute {
            get { return anyAttribute; }
            set { anyAttribute = value; }
        }

        internal CompiledIdentityConstraint[] Constraints {
            get { return constraints; }
            set { constraints = value; }
        }
        
        internal XmlSchemaElement SchemaElement {
            get { return schemaElement;}
            set { schemaElement = value;}
        }
#endif
        // add a new SchemaAttDef to the SchemaElementDecl
        internal void AddAttDef(SchemaAttDef attdef) {
            attdefs.Add(attdef.Name, attdef);
#if !SILVERLIGHT
            if (attdef.Presence == SchemaDeclBase.Use.Required || attdef.Presence == SchemaDeclBase.Use.RequiredFixed) {
                hasRequiredAttribute = true;
            }
#endif
            if (attdef.Presence == SchemaDeclBase.Use.Default || attdef.Presence == SchemaDeclBase.Use.Fixed) { //Not adding RequiredFixed here
                if (defaultAttdefs == null) {
                    defaultAttdefs = new List<IDtdDefaultAttributeInfo>();
                }
                defaultAttdefs.Add(attdef);
            }
        }
        
        /*
         * Retrieves the attribute definition of the named attribute.
         * @param name  The name of the attribute.
         * @return  an attribute definition object; returns null if it is not found.
         */
        internal SchemaAttDef GetAttDef(XmlQualifiedName qname) {
            SchemaAttDef attDef;
            if (attdefs.TryGetValue(qname, out attDef)) {
                return attDef;
            }
            return null;
        }

        internal IList<IDtdDefaultAttributeInfo> DefaultAttDefs {
            get { return defaultAttdefs; }
        }

#if !SILVERLIGHT
        internal Dictionary<XmlQualifiedName, SchemaAttDef> AttDefs {
            get { return attdefs; }
        }

        internal Dictionary<XmlQualifiedName, XmlQualifiedName> ProhibitedAttributes {
            get { return prohibitedAttributes; }
        }

        internal void CheckAttributes(Hashtable presence, bool standalone) {
            foreach(SchemaAttDef attdef in attdefs.Values) {
                if (presence[attdef.Name] == null) {
                    if (attdef.Presence == SchemaDeclBase.Use.Required) {
                        throw new XmlSchemaException(Res.Sch_MissRequiredAttribute, attdef.Name.ToString());
                    }
                    else if (standalone && attdef.IsDeclaredInExternal && (attdef.Presence == SchemaDeclBase.Use.Default || attdef.Presence == SchemaDeclBase.Use.Fixed)) {
                        throw new XmlSchemaException(Res.Sch_StandAlone, string.Empty);
                    }
                }
            }
        }
#endif
    }
}
