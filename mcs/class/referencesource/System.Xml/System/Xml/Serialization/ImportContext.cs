//------------------------------------------------------------------------------
// <copyright file="ImportContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Specialized;

    public class ImportContext {
        bool shareTypes;
        SchemaObjectCache cache; // cached schema top-level items
        Hashtable mappings; // XmlSchema -> SerializableMapping, XmlSchemaSimpleType -> EnumMapping, XmlSchemaComplexType -> StructMapping
        Hashtable elements; // XmlSchemaElement -> ElementAccessor
        CodeIdentifiers typeIdentifiers;

        /// <include file='doc\ImportContext.uex' path='docs/doc[@for="ImportContext.ImportContext"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ImportContext(CodeIdentifiers identifiers, bool shareTypes) {
            this.typeIdentifiers = identifiers;
            this.shareTypes = shareTypes;
        }
        internal ImportContext() : this(null, false) {}

        internal SchemaObjectCache Cache {
            get {
                if (cache == null)
                    cache = new SchemaObjectCache();
                return cache; 
            }
        }

        internal Hashtable Elements {
            get { 
                if (elements == null)
                    elements = new Hashtable();
                return elements;
            }
        }

        internal Hashtable Mappings {
            get { 
                if (mappings == null)
                    mappings = new Hashtable();
                return mappings;
            }
        }

        /// <include file='doc\ImportContext.uex' path='docs/doc[@for="ImportContext.TypeIdentifiers"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeIdentifiers TypeIdentifiers {
            get { 
                if (typeIdentifiers == null)
                    typeIdentifiers = new CodeIdentifiers();
                return typeIdentifiers; 
            }
        }

        /// <include file='doc\ImportContext.uex' path='docs/doc[@for="ImportContext.ShareTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool ShareTypes {
            get { return shareTypes; }
        }

        /// <include file='doc\ImportContext.uex' path='docs/doc[@for="ImportContext.Warnings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StringCollection Warnings {
            get { return Cache.Warnings; }
        }
    }

    internal class SchemaObjectCache {
        Hashtable graph;
        Hashtable hash;
        Hashtable objectCache;
        StringCollection warnings;
        // 
        internal Hashtable looks = new Hashtable();
        Hashtable Graph {
            get {
                if (graph == null) 
                    graph = new Hashtable();
                return graph;
            }
        }

        Hashtable Hash {
            get {
                if (hash == null) 
                    hash = new Hashtable();
                return hash;
            }
        }

        Hashtable ObjectCache {
            get {
                if (objectCache == null) 
                    objectCache = new Hashtable();
                return objectCache;
            }
        }

        internal StringCollection Warnings {
            get {
                if (warnings == null) 
                    warnings = new StringCollection();
                return warnings;
            }
        }
      
        internal XmlSchemaObject AddItem(XmlSchemaObject item, XmlQualifiedName qname, XmlSchemas schemas) {
            if (item == null)
                return null;
            if (qname == null || qname.IsEmpty) 
                return null;

            string key = item.GetType().Name + ":" + qname.ToString();
            ArrayList list = (ArrayList)ObjectCache[key];
            if (list == null) {
                list = new ArrayList();
                ObjectCache[key] = list;
            }

            for (int i = 0; i < list.Count; i++) {
                XmlSchemaObject cachedItem = (XmlSchemaObject)list[i];
                if (cachedItem == item)
                    return cachedItem;

                if (Match(cachedItem, item, true)) {
                    return cachedItem;
                }
                else {
                    Warnings.Add(Res.GetString(Res.XmlMismatchSchemaObjects, item.GetType().Name, qname.Name, qname.Namespace));
                    Warnings.Add("DEBUG:Cached item key:\r\n" + (string)looks[cachedItem] + "\r\nnew item key:\r\n" + (string)looks[item]);
                }
            }
            // no match found we need to insert the new type in the cache
            list.Add(item);
            return item;
        }

        internal bool Match(XmlSchemaObject o1, XmlSchemaObject o2, bool shareTypes) {
            if (o1 == o2)
                return true;
            if (o1.GetType() != o2.GetType())
                return false;
            if (Hash[o1] == null)
                Hash[o1] = GetHash(o1);
            int hash1 = (int)Hash[o1];
            int hash2 = GetHash(o2);
            if (hash1 != hash2)
                return false;

            if (shareTypes)
                return CompositeHash(o1, hash1) == CompositeHash(o2, hash2);
            return true;
        }

        private ArrayList GetDependencies(XmlSchemaObject o, ArrayList deps, Hashtable refs) {
            if (refs[o] == null) {
                refs[o] = o;
                deps.Add(o);
                ArrayList list = Graph[o] as ArrayList;
                if (list != null) {
                    for (int i = 0; i < list.Count; i++) {
                        GetDependencies((XmlSchemaObject)list[i], deps, refs);
                    }
                }
            }
            return deps;
        }

        private int CompositeHash(XmlSchemaObject o, int hash) {
            ArrayList list = GetDependencies(o, new ArrayList(), new Hashtable());
            double tmp = 0;
            for (int i = 0; i < list.Count; i++) {
                object cachedHash = Hash[list[i]];
                if (cachedHash is int) {
                    tmp += (int)cachedHash/list.Count;
                }
            }
            return (int)tmp;
        }

        internal void GenerateSchemaGraph(XmlSchemas schemas) {
            SchemaGraph graph = new SchemaGraph(Graph, schemas);
            ArrayList items = graph.GetItems();

            for (int i = 0; i < items.Count; i++) {
                GetHash((XmlSchemaObject)items[i]);
            }
        }

        private int GetHash(XmlSchemaObject o) {
            object hash = Hash[o];
            if (hash != null) {
                if (hash is XmlSchemaObject) {
                }
                else {
                    return (int)hash;
                }
            }
            // new object, generate the hash
            string hashString = ToString(o, new SchemaObjectWriter());
            looks[o] = hashString;
            int code = hashString.GetHashCode();
            Hash[o] = code;
            return code;
        }

        string ToString(XmlSchemaObject o, SchemaObjectWriter writer) {
            return writer.WriteXmlSchemaObject(o);
        }
    }

    internal class SchemaGraph {
        ArrayList empty = new ArrayList();
        XmlSchemas schemas;
        Hashtable scope;
        int items;

        internal SchemaGraph(Hashtable scope, XmlSchemas schemas) {
            this.scope = scope;
            schemas.Compile(null, false);
            this.schemas = schemas;
            items = 0;
            foreach(XmlSchema s in schemas) {
                items += s.Items.Count;
                foreach (XmlSchemaObject item in s.Items) {
                    Depends(item);
                }
            }
        }

        internal ArrayList GetItems() {
            return new ArrayList(scope.Keys);
        }

        internal void AddRef(ArrayList list, XmlSchemaObject o) {
            if (o == null)
                return;
            if (schemas.IsReference(o))
                return;
            if (o.Parent is XmlSchema) {
                string ns = ((XmlSchema)o.Parent).TargetNamespace;
                if (ns == XmlSchema.Namespace)
                    return;
                if (list.Contains(o))
                    return;
                list.Add(o);
            }
        }

        internal ArrayList Depends(XmlSchemaObject item) {

            if (item.Parent is XmlSchema) {
                if (scope[item] != null)
                    return (ArrayList)scope[item];

                ArrayList refs = new ArrayList();
                Depends(item, refs);
                scope.Add(item, refs);
                return refs;
            }
            return empty;
        }

        internal void Depends(XmlSchemaObject item, ArrayList refs) {
            if (item == null || scope[item] != null)
                return;

            Type t = item.GetType();
            if (typeof(XmlSchemaType).IsAssignableFrom(t)) {
                XmlQualifiedName baseName = XmlQualifiedName.Empty;
                XmlSchemaType baseType = null;
                XmlSchemaParticle particle = null;
                XmlSchemaObjectCollection attributes = null;

                if (item is XmlSchemaComplexType) {
                    XmlSchemaComplexType ct = (XmlSchemaComplexType)item;
                    if (ct.ContentModel != null) {
                        XmlSchemaContent content = ct.ContentModel.Content;
                        if (content is XmlSchemaComplexContentRestriction) {
                            baseName = ((XmlSchemaComplexContentRestriction)content).BaseTypeName;
                            attributes = ((XmlSchemaComplexContentRestriction)content).Attributes;
                        }
                        else if (content is XmlSchemaSimpleContentRestriction) {
                            XmlSchemaSimpleContentRestriction restriction = (XmlSchemaSimpleContentRestriction)content;
                            if (restriction.BaseType != null)
                                baseType = restriction.BaseType;
                            else
                                baseName = restriction.BaseTypeName;
                            attributes = restriction.Attributes;
                        }
                        else if (content is XmlSchemaComplexContentExtension) {
                            XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension)content;
                            attributes = extension.Attributes;
                            particle = extension.Particle;
                            baseName = extension.BaseTypeName;
                        }
                        else if (content is XmlSchemaSimpleContentExtension) {
                            XmlSchemaSimpleContentExtension extension = (XmlSchemaSimpleContentExtension)content;
                            attributes = extension.Attributes;
                            baseName = extension.BaseTypeName;
                        }
                    }
                    else {
                        attributes = ct.Attributes;
                        particle = ct.Particle;
                    }
                    if (particle is XmlSchemaGroupRef) {
                        XmlSchemaGroupRef refGroup = (XmlSchemaGroupRef)particle;
                        particle = ((XmlSchemaGroup)schemas.Find(refGroup.RefName, typeof(XmlSchemaGroup), false)).Particle;
                    }
                    else if (particle is XmlSchemaGroupBase) {
                        particle = (XmlSchemaGroupBase)particle;
                    }
                }
                else if (item is XmlSchemaSimpleType) {
                    XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType)item;
                    XmlSchemaSimpleTypeContent content = simpleType.Content;
                    if (content is XmlSchemaSimpleTypeRestriction) {
                        baseType = ((XmlSchemaSimpleTypeRestriction)content).BaseType;
                        baseName = ((XmlSchemaSimpleTypeRestriction)content).BaseTypeName;
                    }
                    else if (content is XmlSchemaSimpleTypeList) {
                        XmlSchemaSimpleTypeList list  = (XmlSchemaSimpleTypeList)content;
                        if (list.ItemTypeName != null && !list.ItemTypeName.IsEmpty)
                            baseName = list.ItemTypeName;
                        if (list.ItemType != null) {
                            baseType = list.ItemType;
                        }
                    }
                    else if (content is XmlSchemaSimpleTypeRestriction) {
                        baseName = ((XmlSchemaSimpleTypeRestriction)content).BaseTypeName;
                    }
                    else if (t == typeof(XmlSchemaSimpleTypeUnion)) {
                        XmlQualifiedName[] memberTypes = ((XmlSchemaSimpleTypeUnion)item).MemberTypes;
                        
                        if (memberTypes != null) {
                            for (int i = 0; i < memberTypes.Length; i++) {
                                XmlSchemaType type = (XmlSchemaType)schemas.Find(memberTypes[i], typeof(XmlSchemaType), false);
                                AddRef(refs, type);
                            }
                        }
                    }
                }
                if (baseType == null && !baseName.IsEmpty && baseName.Namespace != XmlSchema.Namespace)
                    baseType = (XmlSchemaType)schemas.Find(baseName, typeof(XmlSchemaType), false);

                if (baseType != null) {
                    AddRef(refs, baseType);
                }
                if (particle != null) {
                    Depends(particle, refs);
                }
                if (attributes != null) {
                    for (int i = 0; i < attributes.Count; i++) {
                        Depends(attributes[i], refs);
                    }
                }
            }
            else if (t == typeof(XmlSchemaElement)) {
                XmlSchemaElement el = (XmlSchemaElement)item;
                if (!el.SubstitutionGroup.IsEmpty) {
                    if (el.SubstitutionGroup.Namespace != XmlSchema.Namespace) {
                        XmlSchemaElement head = (XmlSchemaElement)schemas.Find(el.SubstitutionGroup, typeof(XmlSchemaElement), false);
                        AddRef(refs, head);
                    }
                }
                if (!el.RefName.IsEmpty) {
                    el = (XmlSchemaElement)schemas.Find(el.RefName, typeof(XmlSchemaElement), false);
                    AddRef(refs, el);
                }
                else if (!el.SchemaTypeName.IsEmpty) {
                    XmlSchemaType type = (XmlSchemaType)schemas.Find(el.SchemaTypeName, typeof(XmlSchemaType), false);
                    AddRef(refs, type);
                }
                else {
                    Depends(el.SchemaType, refs);
                }
            }
            else if (t == typeof(XmlSchemaGroup)) {
                Depends(((XmlSchemaGroup)item).Particle);
            }
            else if (t == typeof(XmlSchemaGroupRef)) {
                XmlSchemaGroup group = (XmlSchemaGroup)schemas.Find(((XmlSchemaGroupRef)item).RefName, typeof(XmlSchemaGroup), false);
                AddRef(refs, group);
            }
            else if (typeof(XmlSchemaGroupBase).IsAssignableFrom(t)) {
                foreach (XmlSchemaObject o in ((XmlSchemaGroupBase)item).Items) {
                    Depends(o, refs);
                }
            }
            else if (t == typeof(XmlSchemaAttributeGroupRef)) {
                XmlSchemaAttributeGroup group = (XmlSchemaAttributeGroup)schemas.Find(((XmlSchemaAttributeGroupRef)item).RefName, typeof(XmlSchemaAttributeGroup), false);
                AddRef(refs, group);
            }
            else if (t == typeof(XmlSchemaAttributeGroup)) {
                foreach (XmlSchemaObject o in ((XmlSchemaAttributeGroup)item).Attributes) {
                    Depends(o, refs);
                }
            }
            else if (t == typeof(XmlSchemaAttribute)) {
                XmlSchemaAttribute at = (XmlSchemaAttribute)item;
                if (!at.RefName.IsEmpty) {
                    at = (XmlSchemaAttribute)schemas.Find(at.RefName, typeof(XmlSchemaAttribute), false);
                    AddRef(refs, at);
                }
                else if (!at.SchemaTypeName.IsEmpty) {
                    XmlSchemaType type = (XmlSchemaType)schemas.Find(at.SchemaTypeName, typeof(XmlSchemaType), false);
                    AddRef(refs, type);
                }
                else {
                    Depends(at.SchemaType, refs);
                }
            }
            if (typeof(XmlSchemaAnnotated).IsAssignableFrom(t)) {
                XmlAttribute[] attrs = (XmlAttribute[])((XmlSchemaAnnotated)item).UnhandledAttributes;

                if (attrs != null) {
                    for (int i = 0; i < attrs.Length; i++) {
                        XmlAttribute attribute = attrs[i];
                        if (attribute.LocalName == Wsdl.ArrayType && attribute.NamespaceURI == Wsdl.Namespace) {
                            string dims;
                            XmlQualifiedName qname = TypeScope.ParseWsdlArrayType(attribute.Value, out dims, item);
                            XmlSchemaType type = (XmlSchemaType)schemas.Find(qname, typeof(XmlSchemaType), false);
                            AddRef(refs, type);
                        }
                    }
                }
            }
        }
    }
}

