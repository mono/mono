//------------------------------------------------------------------------------
// <copyright file="SchemaInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">[....]</owner>                                                              
//------------------------------------------------------------------------------

using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
    
namespace System.Xml.Schema {

#if !SILVERLIGHT
    internal enum AttributeMatchState {
        AttributeFound,
        AnyIdAttributeFound,
        UndeclaredElementAndAttribute,
        UndeclaredAttribute,
        AnyAttributeLax,
        AnyAttributeSkip,
        ProhibitedAnyAttribute,
        ProhibitedAttribute,
        AttributeNameMismatch,
        ValidateAttributeInvalidCall,
    }
#endif

    internal class SchemaInfo : IDtdInfo {
        Dictionary<XmlQualifiedName, SchemaElementDecl> elementDecls = new Dictionary<XmlQualifiedName, SchemaElementDecl>();
        Dictionary<XmlQualifiedName, SchemaElementDecl> undeclaredElementDecls = new Dictionary<XmlQualifiedName, SchemaElementDecl>();

        Dictionary<XmlQualifiedName, SchemaEntity> generalEntities;
        Dictionary<XmlQualifiedName, SchemaEntity> parameterEntities;

        XmlQualifiedName docTypeName = XmlQualifiedName.Empty;
        string internalDtdSubset = string.Empty;
        bool hasNonCDataAttributes = false;
        bool hasDefaultAttributes = false;

#if !SILVERLIGHT
        Dictionary<string, bool> targetNamespaces = new Dictionary<string, bool>();
        Dictionary<XmlQualifiedName, SchemaAttDef> attributeDecls = new Dictionary<XmlQualifiedName, SchemaAttDef>();
        int errorCount;
        SchemaType schemaType;
        Dictionary<XmlQualifiedName, SchemaElementDecl> elementDeclsByType = new Dictionary<XmlQualifiedName, SchemaElementDecl>();
        Dictionary<string, SchemaNotation> notations;
#endif


        internal SchemaInfo() {
#if !SILVERLIGHT
            schemaType = SchemaType.None;
#endif
        }

        public XmlQualifiedName DocTypeName {
            get { return docTypeName; }
            set { docTypeName = value; }
        }

        internal string InternalDtdSubset {
            get { return internalDtdSubset; }
            set { internalDtdSubset = value; }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> ElementDecls {
            get { return elementDecls; }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> UndeclaredElementDecls {
            get { return undeclaredElementDecls; }
        }

        internal Dictionary<XmlQualifiedName, SchemaEntity> GeneralEntities {
            get {
                if (this.generalEntities == null) {
                    this.generalEntities = new Dictionary<XmlQualifiedName, SchemaEntity>();
                }
                return this.generalEntities;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaEntity> ParameterEntities {
            get {
                if (this.parameterEntities == null) {
                    this.parameterEntities = new Dictionary<XmlQualifiedName, SchemaEntity>();
                }
                return this.parameterEntities;
            }
        }

#if !SILVERLIGHT
        internal SchemaType SchemaType {
            get { return schemaType;}
            set { schemaType = value;}
        }

        internal Dictionary<string, bool> TargetNamespaces {
            get { return targetNamespaces; }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> ElementDeclsByType {
            get { return elementDeclsByType; }
        }

        internal Dictionary<XmlQualifiedName, SchemaAttDef> AttributeDecls {
            get { return attributeDecls; }
        }

        internal Dictionary<string, SchemaNotation> Notations {
            get {
                if (this.notations == null) {
                    this.notations = new Dictionary<string, SchemaNotation>();
                }
                return this.notations; 
            }
        }

        internal int ErrorCount {
            get { return errorCount; }
            set { errorCount = value; }
        }

        internal SchemaElementDecl GetElementDecl(XmlQualifiedName qname) {
            SchemaElementDecl elemDecl;
            if (elementDecls.TryGetValue(qname, out elemDecl)) {
                return elemDecl;
            }
            return null;
        }
        
        internal SchemaElementDecl GetTypeDecl(XmlQualifiedName qname) {
            SchemaElementDecl elemDecl;
            if (elementDeclsByType.TryGetValue(qname, out elemDecl)) {
                return elemDecl;
            }
            return null;
        }

        
        internal XmlSchemaElement GetElement(XmlQualifiedName qname) {
            SchemaElementDecl ed = GetElementDecl(qname);
            if (ed != null) {
                return ed.SchemaElement;
            }
            return null;
        }
        
        internal XmlSchemaAttribute GetAttribute(XmlQualifiedName qname) {
            SchemaAttDef attdef = (SchemaAttDef)attributeDecls[qname];
            if (attdef != null) {
                return attdef.SchemaAttribute;
            }
            return null;
        }
        
        internal XmlSchemaElement GetType(XmlQualifiedName qname) {
            SchemaElementDecl ed = GetElementDecl(qname);
            if (ed != null) {
                return ed.SchemaElement;
            }
            return null;
        }

        internal bool HasSchema(string ns) {
            return targetNamespaces.ContainsKey(ns);
        }
        
        internal bool Contains(string ns) {
            return targetNamespaces.ContainsKey(ns);
        }

        internal SchemaAttDef GetAttributeXdr(SchemaElementDecl ed, XmlQualifiedName qname) {
            SchemaAttDef attdef = null;
            if (ed != null) {
                attdef = ed.GetAttDef(qname);;
                if (attdef == null) {
                    if (!ed.ContentValidator.IsOpen || qname.Namespace.Length == 0) {
                        throw new XmlSchemaException(Res.Sch_UndeclaredAttribute, qname.ToString());
                    }
                    if (!attributeDecls.TryGetValue(qname, out attdef) && targetNamespaces.ContainsKey(qname.Namespace)) {
                        throw new XmlSchemaException(Res.Sch_UndeclaredAttribute, qname.ToString());
                    }
                }
            }
            return attdef;
        }


        internal SchemaAttDef GetAttributeXsd(SchemaElementDecl ed, XmlQualifiedName qname, XmlSchemaObject partialValidationType, out AttributeMatchState attributeMatchState) {
            SchemaAttDef attdef = null;
            attributeMatchState = AttributeMatchState.UndeclaredAttribute;
            if (ed != null) {
                attdef = ed.GetAttDef(qname);
                if (attdef != null) {
                    attributeMatchState = AttributeMatchState.AttributeFound;
                    return attdef;
                }
                XmlSchemaAnyAttribute any = ed.AnyAttribute;
                if (any != null) {
                    if (!any.NamespaceList.Allows(qname)) {
                        attributeMatchState = AttributeMatchState.ProhibitedAnyAttribute;
                    }
                    else if (any.ProcessContentsCorrect != XmlSchemaContentProcessing.Skip) {
                        if (attributeDecls.TryGetValue(qname, out attdef)) {
                            if (attdef.Datatype.TypeCode == XmlTypeCode.Id) { //anyAttribute match whose type is ID
                                attributeMatchState = AttributeMatchState.AnyIdAttributeFound;
                            }
                            else {
                                attributeMatchState = AttributeMatchState.AttributeFound;
                            }
                        }
                        else if (any.ProcessContentsCorrect == XmlSchemaContentProcessing.Lax) {
                            attributeMatchState = AttributeMatchState.AnyAttributeLax;
                        }
                    }
                    else {
                        attributeMatchState = AttributeMatchState.AnyAttributeSkip;
                    }
                }
                else if (ed.ProhibitedAttributes.ContainsKey(qname)) {
                    attributeMatchState = AttributeMatchState.ProhibitedAttribute;
                }
            }
            else if (partialValidationType != null) {
                XmlSchemaAttribute attr = partialValidationType as XmlSchemaAttribute;
                if (attr != null) {
                    if (qname.Equals(attr.QualifiedName)) {
                        attdef = attr.AttDef;
                        attributeMatchState = AttributeMatchState.AttributeFound;
                    }
                    else {
                        attributeMatchState = AttributeMatchState.AttributeNameMismatch;
                    }
                }
                else {
                    attributeMatchState = AttributeMatchState.ValidateAttributeInvalidCall;
                }
            }
            else {
                if (attributeDecls.TryGetValue(qname, out attdef)) {
                    attributeMatchState = AttributeMatchState.AttributeFound;
                }
                else {
                    attributeMatchState = AttributeMatchState.UndeclaredElementAndAttribute;
                }
            }
            return attdef;
        }

        internal SchemaAttDef GetAttributeXsd(SchemaElementDecl ed, XmlQualifiedName qname, ref bool skip) {
            AttributeMatchState attributeMatchState;

            SchemaAttDef attDef = GetAttributeXsd(ed, qname, null, out attributeMatchState);
            switch(attributeMatchState) {
                case AttributeMatchState.UndeclaredAttribute:
                    throw new XmlSchemaException(Res.Sch_UndeclaredAttribute, qname.ToString());

                case AttributeMatchState.ProhibitedAnyAttribute:
                case AttributeMatchState.ProhibitedAttribute:
                    throw new XmlSchemaException(Res.Sch_ProhibitedAttribute, qname.ToString());

                case AttributeMatchState.AttributeFound:
                case AttributeMatchState.AnyIdAttributeFound:
                case AttributeMatchState.AnyAttributeLax:
                case AttributeMatchState.UndeclaredElementAndAttribute:
                    break;

                case AttributeMatchState.AnyAttributeSkip:
                    skip = true;
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            return attDef;
        }
            
        internal void Add(SchemaInfo sinfo, ValidationEventHandler eventhandler) {
            if (schemaType == SchemaType.None) {
                schemaType = sinfo.SchemaType;
            }
            else if (schemaType != sinfo.SchemaType) {
                if (eventhandler != null) {
                    eventhandler(this, new ValidationEventArgs(new XmlSchemaException(Res.Sch_MixSchemaTypes, string.Empty)));
                }
                return;
            }

            foreach(string tns in sinfo.TargetNamespaces.Keys) {
                if (!targetNamespaces.ContainsKey(tns)) {
                    targetNamespaces.Add(tns, true);
                }
            }

            foreach(KeyValuePair<XmlQualifiedName, SchemaElementDecl> entry in sinfo.elementDecls) {
                if (!elementDecls.ContainsKey(entry.Key)) {
                    elementDecls.Add(entry.Key, entry.Value);
                }
            }
            foreach(KeyValuePair<XmlQualifiedName, SchemaElementDecl> entry in sinfo.elementDeclsByType) {
                if (!elementDeclsByType.ContainsKey(entry.Key)) {
                    elementDeclsByType.Add(entry.Key, entry.Value);
                }   
            }
            foreach (SchemaAttDef attdef in sinfo.AttributeDecls.Values) {
                if (!attributeDecls.ContainsKey(attdef.Name)) {
                    attributeDecls.Add(attdef.Name, attdef);
                }
            }
            foreach (SchemaNotation notation in sinfo.Notations.Values) {
                if (!Notations.ContainsKey(notation.Name.Name)) {
                    Notations.Add(notation.Name.Name, notation);
                }
            }

        }
#endif

        internal void Finish() {
            Dictionary<XmlQualifiedName, SchemaElementDecl> elements = elementDecls;
            for ( int i = 0; i < 2; i++ ) {
                foreach ( SchemaElementDecl e in elements.Values ) {
                    if ( e.HasNonCDataAttribute ) {
                        hasNonCDataAttributes = true;
                    }
                    if ( e.DefaultAttDefs != null ) {
                        hasDefaultAttributes = true;
                    }
                }
                elements = undeclaredElementDecls;
            }
        }
//
// IDtdInfo interface
//
#region IDtdInfo Members
        bool IDtdInfo.HasDefaultAttributes {
            get {
                return hasDefaultAttributes;
            }
        }

        bool IDtdInfo.HasNonCDataAttributes {
            get {
                return hasNonCDataAttributes;
            }
        }

        IDtdAttributeListInfo IDtdInfo.LookupAttributeList(string prefix, string localName) {
            XmlQualifiedName qname = new XmlQualifiedName(prefix, localName);
            SchemaElementDecl elementDecl;
            if (!elementDecls.TryGetValue(qname, out elementDecl)) {
                undeclaredElementDecls.TryGetValue(qname, out elementDecl);
            }
            return elementDecl;
        }

        IEnumerable<IDtdAttributeListInfo> IDtdInfo.GetAttributeLists() {
            foreach (SchemaElementDecl elemDecl in elementDecls.Values) {
                IDtdAttributeListInfo eleDeclAsAttList = (IDtdAttributeListInfo)elemDecl;
                yield return eleDeclAsAttList;
            }
        }

        IDtdEntityInfo IDtdInfo.LookupEntity(string name) {
            if (generalEntities == null) {
                return null;
            }
            XmlQualifiedName qname = new XmlQualifiedName(name);
            SchemaEntity entity;
            if (generalEntities.TryGetValue(qname, out entity)) {
                return entity;
            }
            return null;
        }

        XmlQualifiedName IDtdInfo.Name {
            get { return docTypeName; }
        }

        string IDtdInfo.InternalDtdSubset {
            get { return internalDtdSubset; }
        }
#endregion
    }
}
