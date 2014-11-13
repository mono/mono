//------------------------------------------------------------------------------
// <copyright file="XMLSchema.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Data.Common;
    using System.Xml;
    using System.Xml.Schema;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;

    internal class XMLSchema {

        internal static TypeConverter GetConverter(Type type) { 
            HostProtectionAttribute protAttrib = new HostProtectionAttribute();
            protAttrib.SharedState = true;

            CodeAccessPermission permission = (CodeAccessPermission)protAttrib.CreatePermission();
            permission.Assert(); 
            try {
                return TypeDescriptor.GetConverter(type);
            }
            finally {
                CodeAccessPermission.RevertAssert(); 
            }
        }

        internal static void SetProperties(Object instance, XmlAttributeCollection attrs) {
            // This is called from both XSD and XDR schemas. 
            // Do we realy need it in XSD ???
            for (int i = 0; i < attrs.Count; i++) {
                if (attrs[i].NamespaceURI == Keywords.MSDNS) {
                    string name  = attrs[i].LocalName;
                    string value = attrs[i].Value;

                    if (name == "DefaultValue" || name == "RemotingFormat")
                        continue;
// Webdata 97925, skipp expressions, we will handle them after SetProperties (in xdrschema)
                    if (name == "Expression" &&  instance is DataColumn)
                        continue;

                    PropertyDescriptor pd = TypeDescriptor.GetProperties(instance)[name];
                    if (pd != null) {
                        // Standard property
                        Type type = pd.PropertyType;

                        TypeConverter converter = XMLSchema.GetConverter(type);
                        object propValue;
                        if (converter.CanConvertFrom(typeof(string))) {
                            propValue = converter.ConvertFromString(value);
                        }else if (type == typeof(Type)) {
                            propValue = DataStorage.GetType(value);
                        }else if (type == typeof(CultureInfo)) {
                            propValue = new CultureInfo(value);
                        }else {
                            throw ExceptionBuilder.CannotConvert(value,type.FullName);
                        }
                        pd.SetValue(instance, propValue);
                    }
                }
            }
        }// SetProperties

        internal static bool FEqualIdentity(XmlNode node, String name, String ns) {
            if (node != null && node.LocalName == name && node.NamespaceURI == ns)
                return true;

            return false;
        }

        internal static bool GetBooleanAttribute(XmlElement element, String attrName, String attrNS, bool defVal) {
            string value = element.GetAttribute(attrName, attrNS);
            if (value == null || value.Length == 0) {
                return defVal;
            }
            if ((value == Keywords.TRUE) || (value == Keywords.ONE_DIGIT)){
                return true;
            }
            if ((value == Keywords.FALSE) || (value == Keywords.ZERO_DIGIT)){
                return false;
            }
            // Error processing:
            throw ExceptionBuilder.InvalidAttributeValue(attrName, value);
        }

        internal static string GenUniqueColumnName(string proposedName, DataTable table) {

            if (table.Columns.IndexOf(proposedName) >= 0) {
                for (int i=0; i <= table.Columns.Count; i++) {
                    string tempName = proposedName + "_" + (i).ToString(CultureInfo.InvariantCulture);
                    if (table.Columns.IndexOf(tempName) >= 0) {
                        continue;
                    }
                    else {
                        return tempName;
                    }
                }
            }
            return proposedName;
        }
     }

    internal sealed class ConstraintTable {
        public DataTable table;
        public XmlSchemaIdentityConstraint constraint;
        public ConstraintTable(DataTable t, XmlSchemaIdentityConstraint c) {
            table = t;
            constraint = c;
        }
    }

    internal sealed class XSDSchema :  XMLSchema 
    {
        XmlSchemaSet _schemaSet = null;    
        XmlSchemaElement dsElement = null;
        DataSet _ds = null;
        String _schemaName = null;
        private ArrayList ColumnExpressions;
        private Hashtable ConstraintNodes;
        private ArrayList RefTables;
        private ArrayList complexTypes;
        XmlSchemaObjectCollection annotations;
        XmlSchemaObjectCollection elements;
        Hashtable attributes;
        Hashtable elementsTable;
        Hashtable attributeGroups;
        Hashtable schemaTypes;
        Hashtable expressions;
        Dictionary <DataTable, List<DataTable>> tableDictionary;

        Hashtable udSimpleTypes;

        Hashtable existingSimpleTypeMap;
        
        private bool fromInference = false;
        
        internal bool FromInference {
            get {
                return fromInference;
            }
            set {
                fromInference = value;
            }
        }        

        private void CollectElementsAnnotations(XmlSchema schema){
            ArrayList schemaList = new ArrayList();
            CollectElementsAnnotations(schema, schemaList);
            schemaList.Clear();
        }
        
        private void CollectElementsAnnotations(XmlSchema schema, ArrayList schemaList){
            if (schemaList.Contains(schema)) {
                return;
            }
            schemaList.Add(schema);

            foreach(object item in schema.Items) {
                if (item is XmlSchemaAnnotation) {
                    annotations.Add((XmlSchemaAnnotation)item);
                }
                if (item is XmlSchemaElement) {
                    XmlSchemaElement elem = (XmlSchemaElement)item;
                    elements.Add(elem);
                    elementsTable[elem.QualifiedName] = elem;
                }
                if (item is XmlSchemaAttribute) {
                    XmlSchemaAttribute attr = (XmlSchemaAttribute)item;
                    attributes[attr.QualifiedName] = attr;
                }
                if (item is XmlSchemaAttributeGroup) {
                    XmlSchemaAttributeGroup attr = (XmlSchemaAttributeGroup)item;
                    attributeGroups[attr.QualifiedName] = attr;
                }
                if (item is XmlSchemaType) {
                    string MSDATATargetNamespace = null;
                    if (item is XmlSchemaSimpleType) {
                        MSDATATargetNamespace = XSDSchema.GetMsdataAttribute((XmlSchemaType)item, Keywords.TARGETNAMESPACE);
                    }
                    
                    XmlSchemaType type = (XmlSchemaType)item;
                    schemaTypes[type.QualifiedName] = type;

// Webdata 92054 if we have a User Defined simple type, cache it so later we may need for mapping
// meanwhile more convinient solution would be to directly use schemaTypes, but it would be more complex to handle
                    XmlSchemaSimpleType xmlSimpleType = (item as XmlSchemaSimpleType );
                    if (xmlSimpleType != null) { 
                        if (udSimpleTypes == null) {
                            udSimpleTypes = new Hashtable();
                        }

                        udSimpleTypes[type.QualifiedName.ToString()] = xmlSimpleType;
                        DataColumn dc = (DataColumn)existingSimpleTypeMap[type.QualifiedName.ToString()];
                        // Assumption is that our simple type qualified name ihas the same output as XmlSchemaSimpleType type.QualifiedName.ToString()
                        SimpleType tmpSimpleType = (dc != null)? dc.SimpleType: null;
// 
                        if (tmpSimpleType != null) {
                            SimpleType tmpDataSimpleType = new SimpleType(xmlSimpleType);
                            string errorStr = tmpSimpleType.HasConflictingDefinition(tmpDataSimpleType);
                            if (errorStr.Length != 0) {
                                throw ExceptionBuilder.InvalidDuplicateNamedSimpleTypeDelaration(tmpDataSimpleType.SimpleTypeQualifiedName, errorStr);
                            }
                        }
                        
                    }                    
                }
            }
            foreach(XmlSchemaExternal include in schema.Includes) {
                if (include is XmlSchemaImport)
                    continue;
                if (include.Schema != null) {
                    CollectElementsAnnotations(include.Schema, schemaList);
                }
            }
        }

        internal static string QualifiedName(string name) {
            int iStart = name.IndexOf(':');
            if (iStart == -1)
                return Keywords.XSD_PREFIXCOLON + name;
            else
                return name;
        }

        internal static void SetProperties(Object instance, XmlAttribute[] attrs) {
            // This is called from both XSD and XDR schemas. 
            // Do we realy need it in XSD ???
            if (attrs == null)
                return;
            for (int i = 0; i < attrs.Length; i++) {
                if (attrs[i].NamespaceURI == Keywords.MSDNS) {
                    string name  = attrs[i].LocalName;
                    string value = attrs[i].Value;

                    if (name == "DefaultValue" || name == "Ordinal" || name == "Locale" || name == "RemotingFormat")
                        continue;

                    if (name == "Expression" &&  instance is DataColumn) // Webdata 97925: we will handle columnexpressions at HandleColumnExpression
                        continue;

                    if (name == "DataType") {
                        DataColumn col = instance as DataColumn;
                        if (col != null) {
                            col.DataType = DataStorage.GetType(value);
                        }

                        continue;
                    }

                    PropertyDescriptor pd = TypeDescriptor.GetProperties(instance)[name];
                    if (pd != null) {
                        // Standard property
                        Type type = pd.PropertyType;

                        TypeConverter converter = XMLSchema.GetConverter(type);
                        object propValue;
                        if (converter.CanConvertFrom(typeof(string))) {
                            propValue = converter.ConvertFromString(value);
                        }else if (type == typeof(Type)) {
                            propValue = Type.GetType(value);
                        }else if (type == typeof(CultureInfo)) {
                            propValue = new CultureInfo(value);
                        }else {
                            throw ExceptionBuilder.CannotConvert(value,type.FullName);
                        }
                        pd.SetValue(instance, propValue);
                    }
                }
            }
        }// SetProperties

        private static void SetExtProperties(Object instance, XmlAttribute[] attrs) {
            PropertyCollection props = null;
            if (attrs == null)
                return;
            for (int i = 0; i < attrs.Length; i++) {
                if (attrs[i].NamespaceURI == Keywords.MSPROPNS) {
                    if(props == null) {
                        object val = TypeDescriptor.GetProperties(instance)["ExtendedProperties"].GetValue(instance);
                        Debug.Assert(val is PropertyCollection, "We can set values only for classes that have ExtendedProperties");
                        props = (PropertyCollection) val;
                    }
                    string propName = XmlConvert.DecodeName(attrs[i].LocalName);

                    if (instance is ForeignKeyConstraint) {
                        if (propName.StartsWith(Keywords.MSD_FK_PREFIX, StringComparison.Ordinal)) 
                            propName = propName.Substring(3);
                        else 
                            continue; 
                    }
                    if ((instance is DataRelation) && (propName.StartsWith(Keywords.MSD_REL_PREFIX, StringComparison.Ordinal))) {
                            propName = propName.Substring(4);
                    }
                    else if ((instance is DataRelation) && (propName.StartsWith(Keywords.MSD_FK_PREFIX, StringComparison.Ordinal))) {
                        continue;
                    }

                    props.Add(propName, attrs[i].Value);
                }
            }
        }// SetExtProperties

        private void HandleColumnExpression(Object instance, XmlAttribute[] attrs) {
            if (attrs == null)
                return;
            DataColumn dc = instance as DataColumn;
            Debug.Assert(dc != null, "HandleColumnExpression is supposed to be called for DataColumn");
            if (dc != null ) {
                for (int i = 0; i < attrs.Length; i++) {
                    if (attrs[i].NamespaceURI == Keywords.MSDNS) {
                        if (attrs[i].LocalName == "Expression"){
                            if (this.expressions == null)
                                this.expressions = new Hashtable();
                            this.expressions[dc] = attrs[i].Value;
                            ColumnExpressions.Add(dc);
                            break;
                        }
                    }
                }
            }
        }
        
        internal static String GetMsdataAttribute(XmlSchemaAnnotated node, String ln) {
            XmlAttribute[]   nodeAttributes = node.UnhandledAttributes;
            if (nodeAttributes!=null)
                for(int i=0; i<nodeAttributes.Length;i++)
                    if (nodeAttributes[i].LocalName == ln && nodeAttributes[i].NamespaceURI == Keywords.MSDNS)
                        return nodeAttributes[i].Value;
            return null;
        }
                
        private static void SetExtProperties(Object instance, XmlAttributeCollection attrs) {
            PropertyCollection props = null;
            for (int i = 0; i < attrs.Count; i++) {
                if (attrs[i].NamespaceURI == Keywords.MSPROPNS) {
                    if(props == null) {
                        object val = TypeDescriptor.GetProperties(instance)["ExtendedProperties"].GetValue(instance);
                        Debug.Assert(val is PropertyCollection, "We can set values only for classes that have ExtendedProperties");
                        props = (PropertyCollection) val;
                    }
                    string propName = XmlConvert.DecodeName(attrs[i].LocalName);
                    props.Add(propName, attrs[i].Value);
                }
            }
        }// SetExtProperties

        internal void HandleRefTableProperties(ArrayList RefTables, XmlSchemaElement element) {
            string typeName = GetInstanceName(element);
            DataTable table = _ds.Tables.GetTable(XmlConvert.DecodeName(typeName), element.QualifiedName.Namespace);
            Debug.Assert(table != null, "ref table should have been already created");

            SetProperties(table, element.UnhandledAttributes);
            SetExtProperties(table, element.UnhandledAttributes);
        }
        
        internal void HandleRelation(XmlElement node, bool fNested) {
            string          strName;
            string          parentName;
            string          childName;
            string []       parentNames;
            string []       childNames;
            string          value;
            bool            fCreateConstraints = false; //if we have a relation,
                                                        //we do not have constraints
            DataRelationCollection rels = _ds.Relations;
            DataRelation    relation;
            DataColumn []   parentKey;
            DataColumn []   childKey;
            DataTable       parent;
            DataTable       child;
            int             keyLength;

            strName = XmlConvert.DecodeName(node.GetAttribute(Keywords.NAME));
            for (int i = 0; i < rels.Count; ++i) {
                if (0 == String.Compare(rels[i].RelationName, strName, StringComparison.Ordinal)) 
                    return;
            }

            parentName = node.GetAttribute(Keywords.MSD_PARENT, Keywords.MSDNS);
            if (parentName == null || parentName.Length==0)
                throw ExceptionBuilder.RelationParentNameMissing(strName);
            parentName = XmlConvert.DecodeName(parentName);
            
            childName = node.GetAttribute(Keywords.MSD_CHILD, Keywords.MSDNS);
            if (childName == null || childName.Length==0)
                throw ExceptionBuilder.RelationChildNameMissing(strName);
            childName = XmlConvert.DecodeName(childName);

            value = node.GetAttribute(Keywords.MSD_PARENTKEY, Keywords.MSDNS);
            if (value == null || value.Length==0)
                throw ExceptionBuilder.RelationTableKeyMissing(strName);

            parentNames = value.TrimEnd(null).Split(new char[] {Keywords.MSD_KEYFIELDSEP, Keywords.MSD_KEYFIELDOLDSEP});
            value = node.GetAttribute(Keywords.MSD_CHILDKEY, Keywords.MSDNS);
            if (value == null || value.Length==0)
                throw ExceptionBuilder.RelationChildKeyMissing(strName);

            childNames = value.TrimEnd(null).Split(new char[] {Keywords.MSD_KEYFIELDSEP, Keywords.MSD_KEYFIELDOLDSEP});

            keyLength = parentNames.Length;
            if (keyLength != childNames.Length)
                throw ExceptionBuilder.MismatchKeyLength();

            parentKey = new DataColumn[keyLength];
            childKey = new DataColumn[keyLength];

            string parentNs = node.GetAttribute(Keywords.MSD_PARENTTABLENS, Keywords.MSDNS);
            string childNs = node.GetAttribute(Keywords.MSD_CHILDTABLENS, Keywords.MSDNS);
            
            parent = _ds.Tables.GetTableSmart(parentName,parentNs);

            if (parent == null)
                throw ExceptionBuilder.ElementTypeNotFound(parentName);

            child =  _ds.Tables.GetTableSmart(childName,childNs);

            if (child == null)
                throw ExceptionBuilder.ElementTypeNotFound(childName);

            for (int i = 0; i < keyLength; i++) {
                parentKey[i] = parent.Columns[XmlConvert.DecodeName(parentNames[i])];
                if (parentKey[i] == null)
                    throw ExceptionBuilder.ElementTypeNotFound(parentNames[i]);
                childKey[i] = child.Columns[XmlConvert.DecodeName(childNames[i])];
                if (childKey[i] == null)
                    throw ExceptionBuilder.ElementTypeNotFound(childNames[i]);
            }
            relation = new DataRelation(strName, parentKey, childKey, fCreateConstraints);
            relation.Nested = fNested;
            SetExtProperties(relation, node.Attributes);
            _ds.Relations.Add(relation);
            if (FromInference && relation.Nested) {
                tableDictionary[relation.ParentTable].Add(relation.ChildTable);
            }
        }

        private bool HasAttributes(XmlSchemaObjectCollection attributes){
            foreach (XmlSchemaObject so in attributes) {
                if (so is XmlSchemaAttribute) {
                   return true;
                }
                if (so is XmlSchemaAttributeGroup) {
                   return true;
                }
                if (so is XmlSchemaAttributeGroupRef) {
                   return true;
                }
            }
            return false;
        }

        private bool IsDatasetParticle(XmlSchemaParticle pt){
            XmlSchemaObjectCollection items = GetParticleItems(pt);
    
            if (items == null)
                return false; // empty element, threat it as table
            
            bool isChoice = this.FromInference && (pt is XmlSchemaChoice);// currently we add this support for choice, just for inference


            foreach (XmlSchemaAnnotated el in items){
                if (el is XmlSchemaElement) {

                    // pushing max occur of choice element to its imidiate children of type xs:elements
                    if (isChoice && pt.MaxOccurs > Decimal.One && (((XmlSchemaElement)el).SchemaType is XmlSchemaComplexType)) // we know frominference condition
                        ((XmlSchemaElement)el).MaxOccurs = pt.MaxOccurs;

            if (((XmlSchemaElement)el).RefName.Name.Length != 0) {
                if (!FromInference || (((XmlSchemaElement)el).MaxOccurs != Decimal.One &&  !(((XmlSchemaElement)el).SchemaType is XmlSchemaComplexType)))
                    continue;
            }


                    if (!IsTable ((XmlSchemaElement)el))
                        return false;

                    continue;
                }
            
                if (el is XmlSchemaParticle) {
                    if (!IsDatasetParticle((XmlSchemaParticle)el))
                        return false;
                }
            } 

            return true;
        }
        
        private int DatasetElementCount(XmlSchemaObjectCollection elements) {
            int nCount = 0;
            foreach(XmlSchemaElement XmlElement in elements) {
                if (GetBooleanAttribute(XmlElement, Keywords.MSD_ISDATASET,  /*default:*/ false)) {
                    nCount++;
                }
            }
            return nCount;
        }

        private XmlSchemaElement FindDatasetElement(XmlSchemaObjectCollection elements) {
            foreach(XmlSchemaElement XmlElement in elements) {
                if (GetBooleanAttribute(XmlElement, Keywords.MSD_ISDATASET,  /*default:*/ false)) 
                    return XmlElement;
            }
            if ((elements.Count == 1) || (this.FromInference && elements.Count > 0)) { //let's see if this element looks like a DataSet

                XmlSchemaElement node = (XmlSchemaElement)elements[0];
                if (!GetBooleanAttribute(node, Keywords.MSD_ISDATASET,  /*default:*/ true)) 
                    return null;

                XmlSchemaComplexType ct = node.SchemaType as XmlSchemaComplexType;
                if (ct == null)
                    return null;

                while (ct != null) {
                    if (HasAttributes(ct.Attributes))
                        return null;

                     if (ct.ContentModel is XmlSchemaSimpleContent){
                         XmlSchemaAnnotated cContent = ((XmlSchemaSimpleContent) (ct.ContentModel)).Content;
                         if (cContent is XmlSchemaSimpleContentExtension) {
                              XmlSchemaSimpleContentExtension ccExtension = ((XmlSchemaSimpleContentExtension) cContent );
                             if (HasAttributes(ccExtension.Attributes))
                                return null;
                          } else {
                              XmlSchemaSimpleContentRestriction ccRestriction = ((XmlSchemaSimpleContentRestriction) cContent );
                             if (HasAttributes(ccRestriction.Attributes))
                                return null;
                          }
                     }
                     
                    
                    XmlSchemaParticle particle = GetParticle(ct);
                    if (particle != null) {
                        if (!IsDatasetParticle(particle))
                            return null; // it's a table
                        } 

                    if (ct.BaseXmlSchemaType is XmlSchemaComplexType) 
                        ct = (XmlSchemaComplexType)ct.BaseXmlSchemaType;
                    else
                        break;
                }


                //if we are here there all elements are tables
                return node;
            }
            return null;
        }

        public void LoadSchema(XmlSchemaSet schemaSet , DataTable dt) {
            if (dt.DataSet != null)
                LoadSchema(schemaSet, dt.DataSet);
        }
        public void LoadSchema(XmlSchemaSet schemaSet , DataSet ds) { //Element schemaRoot, DataSet ds) {
            ConstraintNodes = new Hashtable();
            RefTables = new ArrayList();
            ColumnExpressions = new ArrayList();
            complexTypes = new ArrayList();
            bool setRootNStoDataSet = false;
            bool newDataSet = (ds.Tables.Count == 0);

            if (schemaSet == null) // 
                return;
            _schemaSet = schemaSet;
            _ds = ds;
            ds.fIsSchemaLoading = true;

            foreach (XmlSchema schemaRoot in schemaSet.Schemas()) {
                _schemaName = schemaRoot.Id;

                if (_schemaName == null ||_schemaName.Length == 0) {
                    _schemaName = "NewDataSet";
                }
                ds.DataSetName = XmlConvert.DecodeName(_schemaName);
                string ns = schemaRoot.TargetNamespace;
                if (ds.namespaceURI == null || ds.namespaceURI.Length == 0) {// set just one time, for backward compatibility
                    ds.namespaceURI = (ns== null)? string.Empty : ns;           // see fx\Data\XDO\ReadXml\SchemaM2.xml for more info
                }
                break; // we just need to take Name and NS from first schema [V1.0 & v1.1 semantics]
            }
            
            annotations = new XmlSchemaObjectCollection();
            elements = new XmlSchemaObjectCollection();
            elementsTable = new Hashtable();
            attributes = new Hashtable();
            attributeGroups = new Hashtable();
            schemaTypes = new Hashtable();
            tableDictionary = new Dictionary <DataTable, List<DataTable>>() ;

            existingSimpleTypeMap = new Hashtable();
            foreach(DataTable dt in ds.Tables){
                foreach(DataColumn dc in dt.Columns) {
                    if (dc.SimpleType!= null && dc.SimpleType.Name != null && dc.SimpleType.Name.Length != 0) {
                        existingSimpleTypeMap[dc.SimpleType.SimpleTypeQualifiedName] = dc;
//                        existingSimpleTypeMap[dc.SimpleType.SimpleTypeQualifiedName] = dc.SimpleType;
                    }
                }
            }



            foreach (XmlSchema schemaRoot in schemaSet.Schemas())
                CollectElementsAnnotations(schemaRoot);



            dsElement = FindDatasetElement(elements);
            if (dsElement != null) {
                string mainName = GetStringAttribute (dsElement, Keywords.MSD_MAINDATATABLE, "");
                if (null != mainName) {
                    ds.MainTableName = XmlConvert.DecodeName (mainName);
                }
            }
            else {
                if (this.FromInference) {
                    ds.fTopLevelTable = true; // Backward compatability: for inference, if we do not read DataSet element
                }
                // we should not write it also
                setRootNStoDataSet = true;
                //incase of Root is not mapped to DataSet and is mapped to DataTable instead; to be backward compatable
                // we need to set the Namespace of Root to DataSet's namespace also(it would be NS of First DataTable in collection)
            }

            List<XmlQualifiedName> qnames = new List<XmlQualifiedName>();
                
            if (ds != null && ds.UseDataSetSchemaOnly) {
                int dataSetElementCount = DatasetElementCount(elements);
                if (dataSetElementCount == 0) {
                    throw ExceptionBuilder.IsDataSetAttributeMissingInSchema();
                }
                else if (dataSetElementCount > 1) {
                    throw ExceptionBuilder.TooManyIsDataSetAtributeInSchema();
                }              

                XmlSchemaComplexType ct = (XmlSchemaComplexType) FindTypeNode(dsElement);
                if (ct.Particle != null) {
                    XmlSchemaObjectCollection items = GetParticleItems(ct.Particle);
                    if (items != null) {
                        foreach (XmlSchemaAnnotated el in items){
                            XmlSchemaElement sel = el as XmlSchemaElement;
                            if (null != sel) {
                                if(sel.RefName.Name.Length != 0 ) {
                                    qnames.Add(sel.QualifiedName);
                                }
                            }
                        }
                    }
                }
                
            }

            // Walk all the top level Element tags.  
            foreach (XmlSchemaElement element in elements) {
                if (element == dsElement)
                    continue;
                if (ds != null && ds.UseDataSetSchemaOnly && dsElement != null) {
                    if (dsElement.Parent != element.Parent) {
                        if (!qnames.Contains(element.QualifiedName)) {
                            continue;
                        }
                    }
                }

                String typeName = GetInstanceName(element);
                if (RefTables.Contains(element.QualifiedName.Namespace +":"+ typeName)) {
                    HandleRefTableProperties(RefTables, element);
                    continue;
                }

                DataTable table = HandleTable(element);
            } 
            
            if (dsElement!=null)
                HandleDataSet(dsElement, newDataSet);

            foreach (XmlSchemaAnnotation annotation in annotations) {
                HandleRelations(annotation, false);
            }

//just add Expressions, at this point and if ColumnExpressions.Count > 0, this.expressions should not be null 
            for(int i=0; i<ColumnExpressions.Count; i++) {
                DataColumn dc = ((DataColumn)(ColumnExpressions[i]));
                dc.Expression = (string) this.expressions[dc];
            }             

            foreach (DataTable dt in ds.Tables) {
                if (dt.NestedParentRelations.Length == 0 && dt.Namespace == ds.Namespace) {
                    DataRelationCollection childRelations = dt.ChildRelations;// WebData 113251 if we nulling NS of parent table,
                    for (int j = 0; j < childRelations.Count; j++) {   //  we need to do the same thing for nested child tables as they
                        if (childRelations[j].Nested && dt.Namespace == childRelations[j].ChildTable.Namespace) { // take NS from Parent table
                            childRelations[j].ChildTable.tableNamespace = null;
                        }
                    }
                    dt.tableNamespace = null;
                }
            }

           DataTable tmpTable = ds.Tables[ds.DataSetName, ds.Namespace];
           if ( tmpTable  != null) // this fix is done to support round-trip problem in case if there is one table with same name and NS
               tmpTable.fNestedInDataset = true;


            // this fix is for backward compatability with old inference engine
            if (this.FromInference && ds.Tables.Count == 0 && String.Compare(ds.DataSetName, "NewDataSet", StringComparison.Ordinal) == 0)
                ds.DataSetName = XmlConvert.DecodeName(((XmlSchemaElement)elements[0]).Name);

            
            ds.fIsSchemaLoading = false; //reactivate column computations


            //for backward compatability; we need to set NS of Root Element to DataSet, if root already does not mapped to dataSet
            if (setRootNStoDataSet) { 
                if (ds.Tables.Count > 0) { // if there is table, take first one's NS
                    ds.Namespace = ds.Tables[0].Namespace;
                    ds.Prefix = ds.Tables[0].Prefix;
                }
                else {// otherwise, take TargetNS from first schema
                    Debug.Assert(schemaSet.Count == 1, "there should be one schema");
                    foreach (XmlSchema schemaRoot in schemaSet.Schemas()) { // we should have 1 schema
                        ds.Namespace = schemaRoot.TargetNamespace;
                    }
                }
            }
        }

        private void HandleRelations(XmlSchemaAnnotation ann, bool fNested) {
            foreach (object __items in ann.Items)
            if (__items is XmlSchemaAppInfo) {
                XmlNode[] relations = ((XmlSchemaAppInfo) __items).Markup;
                for (int i = 0; i<relations.Length; i++)
                if (FEqualIdentity(relations[i], Keywords.MSD_RELATION, Keywords.MSDNS))
                    HandleRelation((XmlElement)relations[i], fNested);
            }
        }
        
        internal XmlSchemaObjectCollection GetParticleItems(XmlSchemaParticle pt){
            if (pt is XmlSchemaSequence)
                return ((XmlSchemaSequence)pt).Items;
            if (pt is XmlSchemaAll)
                return ((XmlSchemaAll)pt).Items;
            if (pt is XmlSchemaChoice)
                return ((XmlSchemaChoice)pt).Items;
            if (pt is XmlSchemaAny)
                return null;
            // the code below is a little hack for the SOM behavior        
            if (pt is XmlSchemaElement) {
                XmlSchemaObjectCollection Items = new XmlSchemaObjectCollection();
                Items.Add(pt);
                return Items;
            }
            if (pt is XmlSchemaGroupRef)
                return GetParticleItems( ((XmlSchemaGroupRef)pt).Particle );
            // should never get here.
            return null;

        }

        internal void HandleParticle(XmlSchemaParticle pt, DataTable table, ArrayList tableChildren, bool isBase){
            XmlSchemaObjectCollection items = GetParticleItems(pt);
            
            if (items == null)
                return;

            foreach (XmlSchemaAnnotated item in items){
                XmlSchemaElement el = item as XmlSchemaElement;
                if (el != null) {
                    if (this.FromInference && pt is XmlSchemaChoice && pt.MaxOccurs > Decimal.One && (el.SchemaType is XmlSchemaComplexType))
                        ((XmlSchemaElement)el).MaxOccurs = pt.MaxOccurs;
                   

                    DataTable child = null;
                    // to decide if element is our table, we need to match both name and ns 
                    // 286043 - SQL BU Defect Tracking
                    if (((el.Name == null) && (el.RefName.Name == table.EncodedTableName && el.RefName.Namespace == table.Namespace)) ||
                        (IsTable(el) && el.Name == table.TableName)) {
                        if (this.FromInference) {
                            child = HandleTable ((XmlSchemaElement)el);
                            Debug.Assert(child == table, "table not the same");
                        }
                        else {
                            child = table;
                        }
                    }
                    else {
                        child = HandleTable ((XmlSchemaElement)el);
                        if (child == null && this.FromInference && el.Name == table.TableName) {
                            child = table;
                        }
                    }
                    
                    if (child==null) {
                        if (!this.FromInference || el.Name != table.TableName) {// check is required to support 1.1 inference behavior
                            HandleElementColumn((XmlSchemaElement)el, table, isBase); 
                        }
                    }
                    else {
                        DataRelation relation = null;
                        if (el.Annotation != null)
                            HandleRelations(el.Annotation, true);

                        DataRelationCollection childRelations = table.ChildRelations;
                        for (int j = 0; j < childRelations.Count; j++) {
                            if (!childRelations[j].Nested)
                                continue;

                            if (child == childRelations[j].ChildTable)
                                relation = childRelations[j];
                        }

                        if (relation == null) {
                            tableChildren.Add(child);// how about prefix for this?
                            if(this.FromInference && table.UKColumnPositionForInference == -1) { // this is done for Inference
                                int ukColumnPosition = -1;
                                foreach(DataColumn dc in table.Columns) {
                                    if (dc.ColumnMapping == MappingType.Element)
                                        ukColumnPosition++;
                                }
                                table.UKColumnPositionForInference = ukColumnPosition + 1; // since it starts from 
                            }
                        }
                    }
                } 
                else {
                    HandleParticle((XmlSchemaParticle)item, table, tableChildren, isBase);
                }

            }
            return ;
        }

        internal void HandleAttributes(XmlSchemaObjectCollection attributes, DataTable table, bool isBase) {
        foreach (XmlSchemaObject so in attributes) {
                if (so is XmlSchemaAttribute) {
                    HandleAttributeColumn((XmlSchemaAttribute) so, table, isBase);
                }
                else {  // XmlSchemaAttributeGroupRef
                    XmlSchemaAttributeGroupRef groupRef =  so as XmlSchemaAttributeGroupRef;
                    XmlSchemaAttributeGroup schemaGroup = attributeGroups[groupRef.RefName] as XmlSchemaAttributeGroup;
                    if (schemaGroup!=null) {
                        HandleAttributeGroup(schemaGroup, table, isBase);
                    }
                }
            }
        }

       private void HandleAttributeGroup(XmlSchemaAttributeGroup attributeGroup, DataTable table, bool isBase) {
           foreach (XmlSchemaObject obj in attributeGroup.Attributes) {
               if (obj is XmlSchemaAttribute) {
                   HandleAttributeColumn((XmlSchemaAttribute) obj, table, isBase);
               }
               else { // XmlSchemaAttributeGroupRef
                   XmlSchemaAttributeGroupRef attributeGroupRef = (XmlSchemaAttributeGroupRef)obj;
                   XmlSchemaAttributeGroup attributeGroupResolved;
                   if (attributeGroup.RedefinedAttributeGroup != null && attributeGroupRef.RefName == new XmlQualifiedName(attributeGroup.Name, attributeGroupRef.RefName.Namespace)) {
                       attributeGroupResolved = (XmlSchemaAttributeGroup)attributeGroup.RedefinedAttributeGroup;
                   }
                   else {
                       attributeGroupResolved = (XmlSchemaAttributeGroup)attributeGroups[attributeGroupRef.RefName];
                   }
                   if (attributeGroupResolved != null) {
                       HandleAttributeGroup(attributeGroupResolved, table, isBase);
                   }
               }
           }
       }
        internal void HandleComplexType(XmlSchemaComplexType ct, DataTable table, ArrayList tableChildren,  bool isNillable){
            if (complexTypes.Contains(ct))
                throw ExceptionBuilder.CircularComplexType(ct.Name);
            bool isBase = false;
            complexTypes.Add(ct);


            if (ct.ContentModel != null) {
                /*
                HandleParticle(ct.CompiledParticle, table, tableChildren, isBase);
                foreach (XmlSchemaAttribute s in ct.Attributes){
                    HandleAttributeColumn(s, table, isBase);
                }
                */

                
                if (ct.ContentModel is XmlSchemaComplexContent) {
                    XmlSchemaAnnotated cContent = ((XmlSchemaComplexContent) (ct.ContentModel)).Content;
                    if (cContent is XmlSchemaComplexContentExtension) {
                        XmlSchemaComplexContentExtension ccExtension = ((XmlSchemaComplexContentExtension) cContent );
                        if (!(ct.BaseXmlSchemaType is XmlSchemaComplexType && this.FromInference))
                            HandleAttributes(ccExtension.Attributes, table, isBase);

                        if (ct.BaseXmlSchemaType is XmlSchemaComplexType) {
                            HandleComplexType((XmlSchemaComplexType)ct.BaseXmlSchemaType, table, tableChildren, isNillable);
                        }
                        else {
                            Debug.Assert(ct.BaseXmlSchemaType is XmlSchemaSimpleType,  "Expected SimpleType or ComplexType");
                            if (ccExtension.BaseTypeName.Namespace != Keywords.XSDNS){ 
                                // this is UDSimpleType, pass Qualified name of type
                                HandleSimpleContentColumn(ccExtension.BaseTypeName.ToString(), table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                            }
                            else { // it is built in type
                                HandleSimpleContentColumn(ccExtension.BaseTypeName.Name, table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                            }
                        }
                        if (ccExtension.Particle != null)
                            HandleParticle(ccExtension.Particle, table, tableChildren, isBase);
                        if (ct.BaseXmlSchemaType is XmlSchemaComplexType && this.FromInference)
                            HandleAttributes(ccExtension.Attributes, table, isBase);
                    } else {
                        Debug.Assert(cContent is XmlSchemaComplexContentRestriction, "Expected complexContent extension or restriction");
                        XmlSchemaComplexContentRestriction ccRestriction = ((XmlSchemaComplexContentRestriction) cContent );
                            if (!this.FromInference)
                        HandleAttributes(ccRestriction.Attributes, table, isBase);
                        if (ccRestriction.Particle != null)
                            HandleParticle(ccRestriction.Particle, table, tableChildren, isBase);
                        if (this.FromInference)
                            HandleAttributes(ccRestriction.Attributes, table, isBase);
                    }
                } else {
                    Debug.Assert(ct.ContentModel is XmlSchemaSimpleContent, "expected simpleContent or complexContent");
                    XmlSchemaAnnotated cContent = ((XmlSchemaSimpleContent) (ct.ContentModel)).Content;
                    if (cContent is XmlSchemaSimpleContentExtension) {
                        XmlSchemaSimpleContentExtension ccExtension = ((XmlSchemaSimpleContentExtension) cContent );
                            HandleAttributes(ccExtension.Attributes, table, isBase);
                        if (ct.BaseXmlSchemaType is XmlSchemaComplexType) {
                            HandleComplexType((XmlSchemaComplexType)ct.BaseXmlSchemaType, table, tableChildren, isNillable);
                        } 
                        else {
                            Debug.Assert(ct.BaseXmlSchemaType is XmlSchemaSimpleType,  "Expected SimpleType or ComplexType");
                            HandleSimpleTypeSimpleContentColumn((XmlSchemaSimpleType)ct.BaseXmlSchemaType, ccExtension.BaseTypeName.Name, table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                        }
                    } else {
                        Debug.Assert(cContent is XmlSchemaSimpleContentRestriction, "Expected SimpleContent extension or restriction");
                        XmlSchemaSimpleContentRestriction ccRestriction = ((XmlSchemaSimpleContentRestriction) cContent );
                        HandleAttributes(ccRestriction.Attributes, table, isBase);
                    }

                }

            }
            else {
                isBase = true;
                if (!this.FromInference)
                    HandleAttributes(ct.Attributes, table, isBase);
                if (ct.Particle != null)
                    HandleParticle(ct.Particle, table, tableChildren, isBase);
                if (this.FromInference) {
                    HandleAttributes(ct.Attributes, table, isBase);
                    if (isNillable) // this is for backward compatability to support xsi:Nill=true
                        HandleSimpleContentColumn("string", table, isBase, null, isNillable);
                }
            }

            complexTypes.Remove(ct);
        }       

        internal XmlSchemaParticle GetParticle(XmlSchemaComplexType ct){
            if (ct.ContentModel != null) {
                if (ct.ContentModel is XmlSchemaComplexContent) {
                    XmlSchemaAnnotated cContent = ((XmlSchemaComplexContent) (ct.ContentModel)).Content;
                    if (cContent is XmlSchemaComplexContentExtension) {
                        return ((XmlSchemaComplexContentExtension) cContent ).Particle;
                    } else {
                        Debug.Assert(cContent is XmlSchemaComplexContentRestriction, "Expected complexContent extension or restriction");
                        return ((XmlSchemaComplexContentRestriction) cContent ).Particle;
                    }
                } else {
                    Debug.Assert(ct.ContentModel is XmlSchemaSimpleContent, "expected simpleContent or complexContent");
                    return null;

                }
 
            }
            else {
                return ct.Particle;
            }

        }

        internal DataColumn FindField(DataTable table, string field) {
            bool attribute = false;
            String colName = field;
 
            if (field.StartsWith("@", StringComparison.Ordinal)) {
                attribute = true;
                colName = field.Substring(1);
            }

            String [] split = colName.Split(':');
            colName = split [split.Length - 1];

            colName = XmlConvert.DecodeName(colName);
            DataColumn col = table.Columns[colName];
            if (col == null )
                throw ExceptionBuilder.InvalidField(field);

            bool _attribute = (col.ColumnMapping == MappingType.Attribute) || (col.ColumnMapping == MappingType.Hidden);
    
            if  (_attribute != attribute)
                throw ExceptionBuilder.InvalidField(field);
                
            return col;
        }

        internal DataColumn[] BuildKey(XmlSchemaIdentityConstraint keyNode, DataTable table){
            ArrayList keyColumns = new ArrayList();
         
            foreach (XmlSchemaXPath node in keyNode.Fields) {
                keyColumns.Add(FindField(table, node.XPath));
            }
         
            DataColumn [] key = new DataColumn[keyColumns.Count];
            keyColumns.CopyTo(key, 0);
          
            return key;
        } 
          
        internal bool GetBooleanAttribute(XmlSchemaAnnotated element, String attrName, bool defVal) {
            string value = GetMsdataAttribute(element, attrName);
            if (value == null || value.Length == 0) {
                return defVal;
            }
            if ((value == Keywords.TRUE) || (value == Keywords.ONE_DIGIT)){
                return true;
            }
            if ((value == Keywords.FALSE) || (value == Keywords.ZERO_DIGIT)){
                return false;
            }            
            // Error processing:
            throw ExceptionBuilder.InvalidAttributeValue(attrName, value);
        }

        internal String GetStringAttribute(XmlSchemaAnnotated element, String attrName, String defVal) {
            string value = GetMsdataAttribute(element, attrName);
            if (value == null || value.Length == 0) {
                return defVal;
            }
            return value;
        }
                    
        /*
        <key name="fk">
            <selector>../Customers</selector>
            <field>ID</field>
        </key>
        <keyref refer="fk">
            <selector>.</selector>
            <field>CustID</field>
        </keyref>
        */

        internal static AcceptRejectRule TranslateAcceptRejectRule( string strRule ) {
            if (strRule == "Cascade")
                return AcceptRejectRule.Cascade;
            else if (strRule == "None")
                return AcceptRejectRule.None;
            else
                return ForeignKeyConstraint.AcceptRejectRule_Default;
        }

        internal static Rule TranslateRule( string strRule ) {
            if (strRule == "Cascade")
                return Rule.Cascade;
            else if (strRule == "None")
                return Rule.None;
            else if (strRule == "SetDefault")
                return Rule.SetDefault;
            else if (strRule == "SetNull")
                return Rule.SetNull;
            else
                return ForeignKeyConstraint.Rule_Default;
        }

        internal void HandleKeyref(XmlSchemaKeyref keyref) {
            string refer = XmlConvert.DecodeName(keyref.Refer.Name); // check here!!!
            string name = XmlConvert.DecodeName(keyref.Name);
            name = GetStringAttribute( keyref, "ConstraintName", /*default:*/ name);
            
            // we do not process key defined outside the current node
            
            String tableName = GetTableName(keyref);

            string tableNs = GetMsdataAttribute(keyref,Keywords.MSD_TABLENS);
            
            DataTable table =  _ds.Tables.GetTableSmart(tableName,tableNs);
            
            if (table == null)
                return;

            if (refer == null || refer.Length == 0)
                throw ExceptionBuilder.MissingRefer(name);
            
            ConstraintTable key = (ConstraintTable) ConstraintNodes[refer];

            if (key == null) {
                throw ExceptionBuilder.InvalidKey(name);
            }

            DataColumn[] pKey = BuildKey(key.constraint, key.table);
            DataColumn[] fKey = BuildKey(keyref, table);

            ForeignKeyConstraint fkc = null;

            if (GetBooleanAttribute(keyref, Keywords.MSD_CONSTRAINTONLY,  /*default:*/ false)) {
                int iExisting = fKey[0].Table.Constraints.InternalIndexOf(name);
                if (iExisting > -1) {
                    if (fKey[0].Table.Constraints[iExisting].ConstraintName != name)
                        iExisting = -1;
                }
       
                if (iExisting < 0) {
                    fkc = new ForeignKeyConstraint( name, pKey, fKey );
                    fKey[0].Table.Constraints.Add(fkc);
                }
            }
            else {
                string relName = XmlConvert.DecodeName(GetStringAttribute( keyref, Keywords.MSD_RELATIONNAME, keyref.Name));

                if (relName == null || relName.Length == 0)
                    relName = name;

                int iExisting = fKey[0].Table.DataSet.Relations.InternalIndexOf(relName);
                if (iExisting > -1) {
                    if (fKey[0].Table.DataSet.Relations[iExisting].RelationName != relName)
                        iExisting = -1;
                }
                DataRelation relation = null;
                if (iExisting < 0) {
                    relation = new DataRelation(relName, pKey, fKey);
                    SetExtProperties(relation, keyref.UnhandledAttributes);
                    pKey[0].Table.DataSet.Relations.Add(relation);

                    if (FromInference && relation.Nested) {
                        if (tableDictionary.ContainsKey(relation.ParentTable)) {
                            tableDictionary[relation.ParentTable].Add(relation.ChildTable);
                        }
                    }
                    
                    fkc = relation.ChildKeyConstraint;
                    fkc.ConstraintName = name;
                } 
                else {
                    relation = fKey[0].Table.DataSet.Relations[iExisting];
                }
                if (GetBooleanAttribute(keyref, Keywords.MSD_ISNESTED,  /*default:*/ false)) {
                    relation.Nested = true;
                }

            }

            string acceptRejectRule = GetMsdataAttribute(keyref, Keywords.MSD_ACCEPTREJECTRULE);
            string updateRule       = GetMsdataAttribute(keyref, Keywords.MSD_UPDATERULE);
            string deleteRule       = GetMsdataAttribute(keyref, Keywords.MSD_DELETERULE);

            if (fkc != null) {
                if (acceptRejectRule != null)
                    fkc.AcceptRejectRule = TranslateAcceptRejectRule(acceptRejectRule);

                if (updateRule != null)
                    fkc.UpdateRule = TranslateRule(updateRule);

                if (deleteRule != null)
                    fkc.DeleteRule = TranslateRule(deleteRule);

                SetExtProperties(fkc, keyref.UnhandledAttributes);
            }
        }


        internal void HandleConstraint(XmlSchemaIdentityConstraint keyNode){
            String name = null;
            
            name = XmlConvert.DecodeName(keyNode.Name);
            if (name==null || name.Length==0)
                throw ExceptionBuilder.MissingAttribute(Keywords.NAME);

            if (ConstraintNodes.ContainsKey(name))
                throw ExceptionBuilder.DuplicateConstraintRead(name);

            // we do not process key defined outside the current node
            String tableName = GetTableName(keyNode);
            string tableNs = GetMsdataAttribute(keyNode,Keywords.MSD_TABLENS);
            
            DataTable table =  _ds.Tables.GetTableSmart(tableName,tableNs);

            if (table == null)
                return;

            ConstraintNodes.Add(name, new ConstraintTable(table, keyNode));

            bool   fPrimaryKey = GetBooleanAttribute(keyNode, Keywords.MSD_PRIMARYKEY,  /*default:*/ false);
            name        = GetStringAttribute(keyNode, "ConstraintName", /*default:*/ name);



            DataColumn[] key = BuildKey(keyNode, table);

            if (0 < key.Length) {
                UniqueConstraint found = (UniqueConstraint) key[0].Table.Constraints.FindConstraint(new UniqueConstraint(name, key));

                if (found == null) {
                    key[0].Table.Constraints.Add(name, key, fPrimaryKey);
                    SetExtProperties(key[0].Table.Constraints[name], keyNode.UnhandledAttributes);
                }
                else {
                        key = found.ColumnsReference;
                        SetExtProperties(found, keyNode.UnhandledAttributes);
                        if (fPrimaryKey)
                            key[0].Table.PrimaryKey = key;
                    }
                if (keyNode is XmlSchemaKey) {
                    for (int i=0; i<key.Length; i++)
                        key[i].AllowDBNull = false;
                }
            }
        }

        internal DataTable InstantiateSimpleTable(XmlSchemaElement node) {
            DataTable table;
            String typeName = XmlConvert.DecodeName(GetInstanceName(node));
            String _TableUri;
            
            _TableUri = node.QualifiedName.Namespace;
            table = _ds.Tables.GetTable(typeName, _TableUri);

// 



/*            if (table!=null) {
                throw ExceptionBuilder.DuplicateDeclaration(typeName);
            }
*/
            if (!FromInference && table!=null) {
                throw ExceptionBuilder.DuplicateDeclaration(typeName);
            }


            if (table == null) {
                table = new DataTable( typeName);
                table.Namespace = _TableUri;
                // If msdata:targetNamespace node on element, then use it to override table.Namespace.
                table.Namespace = GetStringAttribute(node, "targetNamespace", _TableUri);
                
                if (!this.FromInference) {
                    table.MinOccurs = node.MinOccurs;
                    table.MaxOccurs = node.MaxOccurs;
                }
                else {
                    string prefix = GetPrefix(_TableUri);
                    if (prefix != null)
                        table.Prefix = prefix;
                }

                SetProperties(table, node.UnhandledAttributes);
                SetExtProperties(table, node.UnhandledAttributes);
            }


             XmlSchemaComplexType ct = node.SchemaType as XmlSchemaComplexType;
             // We assume node.ElementSchemaType.BaseSchemaType to be null for 
             //  <xs:element name="foo"/> and not null for <xs:element name="foo" type="xs:string"/>
             bool isSimpleContent = ((node.ElementSchemaType.BaseXmlSchemaType != null) ||(ct != null && ct.ContentModel is XmlSchemaSimpleContent)); 

             if (!FromInference ||(isSimpleContent && table.Columns.Count == 0 )) {// for inference backward compatability
                 HandleElementColumn(node, table, false);
                 string colName;
                 
                 if (this.FromInference) {
                     int i = 0;
                     colName = typeName + "_Text";
                     while (table.Columns[colName] != null)
                         colName = colName + i++;
                 }
                 else {
                     colName = typeName + "_Column";
                 }

                 table.Columns[0].ColumnName = colName;
                 table.Columns[0].ColumnMapping = MappingType.SimpleContent;
             }
             
             if (!this.FromInference ||_ds.Tables.GetTable(typeName, _TableUri) == null) { // for inference; special case: add table if doesnot exists in collection
                _ds.Tables.Add(table);
                if (this.FromInference) {
                    tableDictionary.Add(table, new List<DataTable>());
                }
             }

            // handle all the unique and key constraints related to this table

            if ((dsElement != null) && (dsElement.Constraints!=null)) {
                foreach (XmlSchemaIdentityConstraint key in dsElement.Constraints) {
                    if (key is XmlSchemaKeyref) 
                        continue;
                    if (GetTableName(key) == table.TableName)
                        HandleConstraint(key);
                }
            }
            table.fNestedInDataset = false;

            return (table);
        }


        internal string GetInstanceName(XmlSchemaAnnotated node) {
            string  instanceName = null;

            Debug.Assert( (node is XmlSchemaElement) || (node is XmlSchemaAttribute), "GetInstanceName should only be called on attribute or elements");

            if (node is XmlSchemaElement) {
                XmlSchemaElement el = (XmlSchemaElement) node;
                instanceName = el.Name != null ? el.Name : el.RefName.Name;
            }
            else if (node is XmlSchemaAttribute) {
                XmlSchemaAttribute el = (XmlSchemaAttribute) node;
                instanceName = el.Name != null ? el.Name : el.RefName.Name;
            }

            Debug.Assert((instanceName != null) && (instanceName.Length != 0), "instanceName cannot be null or empty. There's an error in the XSD compiler");

            return instanceName;
        }

        // Sequences of handling Elements, Attributes and Text-only column should be the same as in InferXmlSchema
        internal DataTable InstantiateTable(XmlSchemaElement node, XmlSchemaComplexType typeNode, bool isRef) {
            DataTable table;
            String typeName = GetInstanceName(node);
            ArrayList tableChildren = new ArrayList();
            
            String _TableUri;
            
            _TableUri = node.QualifiedName.Namespace;

            table = _ds.Tables.GetTable(XmlConvert.DecodeName(typeName), _TableUri);
// TOD: Do not do this fix
//            if (table == null && node.RefName.IsEmpty && !IsTopLevelElement(node) && _TableUri != null && _TableUri.Length > 0) { 
//                _TableUri = null;    // it means form="qualified", so child element inherits namespace. [....]
//            }
            
            if (!FromInference || (FromInference && table == null)) 
            {
                if (table!=null) 
                {
                    if (isRef)
                        return table;
                    else
                        throw ExceptionBuilder.DuplicateDeclaration(typeName);
                }
                
                if (isRef)
                    RefTables.Add(_TableUri+":"+typeName);

                table = new DataTable(XmlConvert.DecodeName(typeName) );
                table.TypeName = node.SchemaTypeName;

                table.Namespace = _TableUri;
                table.Namespace = GetStringAttribute(node, "targetNamespace", _TableUri);

                //table.Prefix = node.Prefix;
                String value= GetStringAttribute(typeNode, Keywords.MSD_CASESENSITIVE,  "") ;
                if (value.Length ==0)
                {
                    value= GetStringAttribute(node, Keywords.MSD_CASESENSITIVE,  "") ;
                }
                if (0 < value.Length) {
                    if ((value == Keywords.TRUE) || (value == "True"))
                        table.CaseSensitive = true;                
                    if ((value == Keywords.FALSE) || (value == "False"))
                        table.CaseSensitive = false;                
                }

                value = GetMsdataAttribute(node, Keywords.MSD_LOCALE);
                if (null != value) { // set by user
                    if (0 < value.Length) {
                        // <... msdata:Locale="en-US"/>
                        table.Locale = new CultureInfo(value);
                    }
                    else {
                        // everett bug behavior before <... msdata:Locale=""/> inherit from DataSet
                        table.Locale = CultureInfo.InvariantCulture;
                    }
                }
                
                // else inherit from DataSet, not set by user
                
                if (!this.FromInference) 
                {
                    table.MinOccurs = node.MinOccurs;
                    table.MaxOccurs = node.MaxOccurs;
                } 
                else 
                {
                    string prefix = GetPrefix(_TableUri);
                    if (prefix != null)
                        table.Prefix = prefix;
                }

                _ds.Tables.Add(table);
                if (FromInference) {
                    tableDictionary.Add(table, new List<DataTable>());
                }
            }
         
            HandleComplexType(typeNode, table, tableChildren, node.IsNillable);

            for (int i=0; i < table.Columns.Count ; i++)
                table.Columns[i].SetOrdinalInternal(i);

/*
            if (xmlContent == XmlContent.Mixed) {
                string textColumn = GenUniqueColumnName(table.TableName+ "_Text", table);
                table.XmlText = new DataColumn(textColumn, typeof(string), null, MappingType.Text);
            } */

            SetProperties(table, node.UnhandledAttributes);
            SetExtProperties(table, node.UnhandledAttributes);

            // handle all the unique and key constraints related to this table
            if ((dsElement != null) && (dsElement.Constraints!=null)) {
                foreach (XmlSchemaIdentityConstraint key in dsElement.Constraints) {
                    if (key is XmlSchemaKeyref) 
                        continue;
                    if (GetTableName(key) == table.TableName) {
                        // respect the NS if it is specified for key, otherwise just go with table name check
                        if( GetTableNamespace(key) == table.Namespace || GetTableNamespace(key) == null)
                            HandleConstraint(key);
/*                     if (GetTableNamespace(key) != null) {
                            if (GetTableNamespace(key) == table.Namespace)
                                HandleConstraint(key);
                        }
                        else {
                            HandleConstraint(key);
                        }
*/
                    }
                }
            }

            foreach(DataTable _tableChild in tableChildren) {
                if (_tableChild != table && table.Namespace == _tableChild.Namespace)
                    _tableChild.tableNamespace = null;

                if ((dsElement != null) && (dsElement.Constraints!=null)) {
                    foreach (XmlSchemaIdentityConstraint key in dsElement.Constraints) {
                        XmlSchemaKeyref keyref = key as XmlSchemaKeyref;
                        if (keyref == null)
                            continue;

                        bool isNested = GetBooleanAttribute(keyref, Keywords.MSD_ISNESTED,  /*default:*/ false);                                
                        if (!isNested)
                            continue;
                        if (GetTableName(keyref) == _tableChild.TableName) {
                            if (_tableChild.DataSet.Tables.InternalIndexOf(_tableChild.TableName) < -1) { // if we have multiple tables with the same name
                                if (GetTableNamespace(keyref) == _tableChild.Namespace) {
                                    HandleKeyref(keyref);
                                }
                            }
                            else {
                                HandleKeyref(keyref);
                            }
                        }
                    }
                } 

                DataRelation relation = null;

                DataRelationCollection childRelations = table.ChildRelations;
                for (int j = 0; j < childRelations.Count; j++) {
                    if (!childRelations[j].Nested)
                        continue;

                    if (_tableChild == childRelations[j].ChildTable)
                        relation = childRelations[j];
                }

                if (relation!=null)
                    continue;

                DataColumn parentKey;
                if (this.FromInference) {
                    int position = table.UKColumnPositionForInference;// we keep posiotion of unique key column here, for inference
                    if (position == -1)
                        foreach (DataColumn dc in table.Columns) {
                        if (dc.ColumnMapping == MappingType.Attribute) {
                            position = dc.Ordinal;
                            break;
                        }
                    }
                    parentKey = table.AddUniqueKey(position);

                }
                else {
                    parentKey = table.AddUniqueKey();
                }

                // foreign key in the child table
                DataColumn childKey = _tableChild.AddForeignKey(parentKey);

                // when we add  unique key, we do set prefix; but for Fk we do not do . So for backward compatability
                if (this.FromInference)
                    childKey.Prefix = _tableChild.Prefix;
//                    childKey.Prefix = GetPrefix(childKey.Namespace);

                // create relationship
                // setup relationship between parent and this table
                relation = new DataRelation(table.TableName + "_" + _tableChild.TableName, parentKey, childKey, true);
                relation.Nested = true;
                _tableChild.DataSet.Relations.Add(relation);
                if (FromInference && relation.Nested) {
                    if (tableDictionary.ContainsKey(relation.ParentTable)) {
                        tableDictionary[relation.ParentTable].Add(relation.ChildTable);
                    }                   
                }
            }

            return (table);
        }

        private sealed class NameType : IComparable {
            public readonly String   name;
            public readonly Type     type;
            public NameType(String n, Type t) { 
                name = n;
                type = t;
            }
            public int CompareTo(object obj) { return String.Compare(name, (string)obj, StringComparison.Ordinal); }
        };

        public static Type XsdtoClr(string xsdTypeName) {
#if DEBUG
            for(int i = 1; i < mapNameTypeXsd.Length; ++i) {
                Debug.Assert((mapNameTypeXsd[i-1].CompareTo(mapNameTypeXsd[i].name)) < 0, "incorrect sorting " + mapNameTypeXsd[i].name);
            }
#endif
            int index = Array.BinarySearch(mapNameTypeXsd, xsdTypeName);
            if (index < 0) {
                throw ExceptionBuilder.UndefinedDatatype(xsdTypeName);
            }
            return mapNameTypeXsd[index].type;
        }
        
        // XSD spec: http://www.w3.org/TR/xmlschema-2/
        //    April: http://www.w3.org/TR/2000/WD-xmlschema-2-20000407/datatypes.html
        //    Fabr:  http://www.w3.org/TR/2000/WD-xmlschema-2-20000225/
        private static readonly NameType[] mapNameTypeXsd = {
            new NameType("ENTITIES"            , typeof(string)  ), /* XSD Apr */
            new NameType("ENTITY"              , typeof(string)  ), /* XSD Apr */
            new NameType("ID"                  , typeof(string)  ), /* XSD Apr */
            new NameType("IDREF"               , typeof(string)  ), /* XSD Apr */
            new NameType("IDREFS"              , typeof(string)  ), /* XSD Apr */
            new NameType("NCName"              , typeof(string)  ), /* XSD Apr */
            new NameType("NMTOKEN"             , typeof(string)  ), /* XSD Apr */
            new NameType("NMTOKENS"            , typeof(string)  ), /* XSD Apr */
            new NameType("NOTATION"            , typeof(string)  ), /* XSD Apr */
            new NameType("Name"                , typeof(string)  ), /* XSD Apr */
            new NameType("QName"               , typeof(string)  ), /* XSD Apr */

            new NameType("anyType"             , typeof(System.Object)  ), /* XSD Apr */
            new NameType("anyURI"              , typeof(System.Uri)  ), /* XSD Apr */
            new NameType("base64Binary"        , typeof(Byte[])  ), /* XSD Apr : abstruct */
            new NameType("boolean"             , typeof(bool)    ), /* XSD Apr */
            new NameType("byte"                , typeof(SByte)   ), /* XSD Apr */
            new NameType("date"                , typeof(DateTime)), /* XSD Apr */
            new NameType("dateTime"            , typeof(DateTime)), /* XSD Apr */
            new NameType("decimal"              , typeof(decimal) ), /* XSD 2001 [....] */
            new NameType("double"              , typeof(double)  ), /* XSD Apr */
            new NameType("duration"            , typeof(TimeSpan)), /* XSD Apr */
            new NameType("float"               , typeof(Single)  ), /* XSD Apr */
            new NameType("gDay"                , typeof(DateTime)), /* XSD Apr */
            new NameType("gMonth"              , typeof(DateTime)), /* XSD Apr */
            new NameType("gMonthDay"           , typeof(DateTime)), /* XSD Apr */
            new NameType("gYear"               , typeof(DateTime)), /* XSD Apr */
            new NameType("gYearMonth"          , typeof(DateTime)), /* XSD Apr */
            new NameType("hexBinary"           , typeof(Byte[])  ), /* XSD Apr : abstruct */
            new NameType("int"                 , typeof(Int32)   ), /* XSD Apr */
            new NameType("integer"             , typeof(Int64)   ), /* XSD Apr */ // <xs:element name="" msdata:DataType="System.Numerics.BigInteger" type="xs:integer" minOccurs="0" />
            new NameType("language"            , typeof(string)  ), /* XSD Apr */
            new NameType("long"                , typeof(Int64)   ), /* XSD Apr */
            new NameType("negativeInteger"     , typeof(Int64)   ), /* XSD Apr */
            new NameType("nonNegativeInteger"  , typeof(UInt64)  ), /* XSD Apr */
            new NameType("nonPositiveInteger"  , typeof(Int64)   ), /* XSD Apr */
            new NameType("normalizedString"    , typeof(string)  ), /* XSD Apr */
            new NameType("positiveInteger"     , typeof(UInt64)  ), /* XSD Apr */
            new NameType("short"               , typeof(Int16)   ), /* XSD Apr */
            new NameType("string"              , typeof(string)  ), /* XSD Apr */
            new NameType("time"                , typeof(DateTime)), /* XSD Apr */
            new NameType("unsignedByte"        , typeof(Byte)    ), /* XSD Apr */
            new NameType("unsignedInt"         , typeof(UInt32)  ), /* XSD Apr */
            new NameType("unsignedLong"        , typeof(UInt64)  ), /* XSD Apr */
            new NameType("unsignedShort"       , typeof(UInt16)  ), /* XSD Apr */
        };

        private static NameType FindNameType(string name) {
#if DEBUG
            for(int i = 1; i < mapNameTypeXsd.Length; ++i) {
                Debug.Assert((mapNameTypeXsd[i-1].CompareTo(mapNameTypeXsd[i].name)) < 0, "incorrect sorting " + mapNameTypeXsd[i].name);
            }
#endif
            int index = Array.BinarySearch(mapNameTypeXsd, name);
            if (index < 0) {
                throw ExceptionBuilder.UndefinedDatatype(name);
            }
            return mapNameTypeXsd[index];
        }

        // input param dt is a "qName" for UDSimpleType else it assumes it's a XSD builtin simpleType
        private Type ParseDataType(string dt) {
            if (!IsXsdType(dt)) {
                if (udSimpleTypes != null) {
                    XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType) udSimpleTypes[dt];
                    if (simpleType == null) { // it is not named simple type, it is not  XSD type, it should be unsupported type like xs:token
                        throw ExceptionBuilder.UndefinedDatatype(dt);
                    }
                    SimpleType rootType = new SimpleType(simpleType);
                    while (rootType.BaseSimpleType != null) {
                        rootType = rootType.BaseSimpleType;
                    }

                    return ParseDataType(rootType.BaseType);
                }
            }
            NameType nt = FindNameType(dt);
            return nt.type;
        }
/*  later we may need such a function
        private Boolean IsUDSimpleType(string qname) {
            if (udSimpleTypes == null)
                return false;
            return (udSimpleTypes.Contains(qname));
        }
*/
        internal static Boolean IsXsdType(string name) {
#if DEBUG
            for(int i = 1; i < mapNameTypeXsd.Length; ++i) {
                Debug.Assert((mapNameTypeXsd[i-1].CompareTo(mapNameTypeXsd[i].name)) < 0, "incorrect sorting " + mapNameTypeXsd[i].name);
            }
#endif
            int index = Array.BinarySearch(mapNameTypeXsd, name);
            if (index < 0) {
#if DEBUG
                // Let's check that we realy don't have this name:
                foreach (NameType nt in mapNameTypeXsd) {
                    Debug.Assert(nt.name != name, "FindNameType('" + name + "') -- failed. Existed name not found");
                }
#endif
                return false;
            }
            Debug.Assert(mapNameTypeXsd[index].name == name, "FindNameType('" + name + "') -- failed. Wrong name found");
            return true;
        }


        internal XmlSchemaAnnotated FindTypeNode(XmlSchemaAnnotated node) {
            // this function is returning null
            // if the typeNode for node is in the XSD namespace.
            
            XmlSchemaAttribute attr = node as XmlSchemaAttribute;
            XmlSchemaElement el = node as XmlSchemaElement;
            bool isAttr = false;
            if (attr != null) {
                isAttr = true;
            }

            String _type = isAttr ? attr.SchemaTypeName.Name :  el.SchemaTypeName.Name;
            String _typeNs = isAttr ? attr.SchemaTypeName.Namespace :  el.SchemaTypeName.Namespace;
            if (_typeNs == Keywords.XSDNS)
                return null;
            XmlSchemaAnnotated typeNode;
            if (_type == null || _type.Length == 0) {
                _type = isAttr ? attr.RefName.Name :  el.RefName.Name;
                if (_type == null || _type.Length == 0) 
                    typeNode = (XmlSchemaAnnotated) (isAttr ? attr.SchemaType :  el.SchemaType);
                else 
                    typeNode = isAttr ? FindTypeNode((XmlSchemaAnnotated)attributes[attr.RefName]) :FindTypeNode((XmlSchemaAnnotated)elementsTable[el.RefName]);
            }
            else
                typeNode = (XmlSchemaAnnotated)schemaTypes[isAttr ? ((XmlSchemaAttribute)node).SchemaTypeName :  ((XmlSchemaElement)node).SchemaTypeName];
            return typeNode;
        }


    internal void HandleSimpleTypeSimpleContentColumn(XmlSchemaSimpleType typeNode, string strType, DataTable table, bool isBase, XmlAttribute[] attrs, bool isNillable){
            // disallow multiple simple content columns for the table
            if(FromInference && table.XmlText != null) { // backward compatability for inference
                return;
            }

            Type type = null;        
            SimpleType xsdType = null;
            
//            if (typeNode.QualifiedName.Namespace != Keywords.XSDNS) { // this means UDSimpleType
            if (typeNode.QualifiedName.Name != null && typeNode.QualifiedName.Name.Length != 0 && typeNode.QualifiedName.Namespace != Keywords.XSDNS) { // this means UDSimpleType
                xsdType = new SimpleType(typeNode);
                strType = typeNode.QualifiedName.ToString(); // use qualifed name
                type = ParseDataType(typeNode.QualifiedName.ToString());
            }
            else {// previous code V 1.1
                XmlSchemaSimpleType ancestor = typeNode.BaseXmlSchemaType as XmlSchemaSimpleType;
                if ((ancestor != null) && (ancestor.QualifiedName.Namespace != Keywords.XSDNS)) { 
                    xsdType = new SimpleType(typeNode);
                    SimpleType rootType = xsdType;

                    while (rootType.BaseSimpleType != null) {
                        rootType = rootType.BaseSimpleType;
                    }
                    type = ParseDataType(rootType.BaseType);
                    strType = xsdType.Name;
                }
                else {
                    type = ParseDataType(strType);
                }
            }

            
            DataColumn column;

            string colName;
            if (this.FromInference) {
                int i = 0;
                colName = table.TableName + "_Text";
                while (table.Columns[colName] != null) {
                    colName = colName + i++;
                }
            }
            else
                colName = table.TableName + "_text";

            string columnName = colName;
            bool isToAdd = true;
            if ((!isBase) && (table.Columns.Contains(columnName, true))){
                column = table.Columns[columnName];
                isToAdd = false;
            }
            else {
                column = new DataColumn(columnName, type, null, MappingType.SimpleContent);
            }

            SetProperties(column, attrs);
            HandleColumnExpression(column, attrs);
            SetExtProperties(column, attrs);

            String tmp = (-1).ToString(CultureInfo.CurrentCulture);
            string defValue = null;
            //try to see if attributes contain allownull
            column.AllowDBNull = isNillable;

            if(attrs!=null)
                for(int i=0; i< attrs.Length;i++) {
                    if ( attrs[i].LocalName == Keywords.MSD_ALLOWDBNULL &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        if ( attrs[i].Value == Keywords.FALSE) 
                            column.AllowDBNull = false;
                    if ( attrs[i].LocalName == Keywords.MSD_ORDINAL &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        tmp = attrs[i].Value;
                    if ( attrs[i].LocalName == Keywords.MSD_DEFAULTVALUE &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        defValue = attrs[i].Value;
                }
            int ordinal = (int)Convert.ChangeType(tmp, typeof(int), null);

            
            //SetExtProperties(column, attr.UnhandledAttributes);

            if ((column.Expression!=null)&&(column.Expression.Length!=0)) {
                ColumnExpressions.Add(column);
            } 

// Update XSD type to point to simple types actual namespace instead of normalized default namespace in case of remoting
            if (xsdType != null && xsdType.Name != null && xsdType.Name.Length > 0) {
                if (XSDSchema.GetMsdataAttribute(typeNode, Keywords.TARGETNAMESPACE)!= null) {
                  column.XmlDataType = xsdType.SimpleTypeQualifiedName;
                }
            }
            else {
                column.XmlDataType = strType;
            }
            column.SimpleType = xsdType;

            //column.Namespace = typeNode.SourceUri;
            if (isToAdd) {
                if (this.FromInference) {
                    column.Prefix = GetPrefix(table.Namespace);
                    column.AllowDBNull = true;
                }
                if(ordinal>-1 && ordinal<table.Columns.Count)
                    table.Columns.AddAt(ordinal, column);
                else
                    table.Columns.Add(column);
            }

            if (defValue != null)
                try {
                    column.DefaultValue = column.ConvertXmlToObject(defValue);
                }
                catch (System.FormatException) {
                    throw ExceptionBuilder.CannotConvert(defValue, type.FullName);
                }    


    }

    internal void HandleSimpleContentColumn(String strType, DataTable table, bool isBase, XmlAttribute[] attrs, bool isNillable){
// for Named Simple type support : We should not recieved anything here other than string.
// there can not be typed simple content
            // disallow multiple simple content columns for the table
            if(this.FromInference && table.XmlText != null) // backward compatability for inference
             return;
            
            Type type = null;            
            if (strType == null) {
                return;
            }
            type = ParseDataType(strType); // we pass it correctly when we call the method, no need to special check. 
            DataColumn column;


            string colName;
            if (this.FromInference) {
                int i = 0;
                colName = table.TableName + "_Text";
                while (table.Columns[colName] != null) {
                    colName = colName + i++;
                }
            }
            else
                colName = table.TableName + "_text";

            string columnName = colName;
            bool isToAdd = true;

            if ((!isBase) && (table.Columns.Contains(columnName, true))){
                column = table.Columns[columnName];
                isToAdd = false;
            }
            else {
                column = new DataColumn(columnName, type, null, MappingType.SimpleContent);
            }
            
            SetProperties(column, attrs);
            HandleColumnExpression(column, attrs);
            SetExtProperties(column, attrs);

            String tmp = (-1).ToString(CultureInfo.CurrentCulture);
            string defValue = null;
            //try to see if attributes contain allownull
            column.AllowDBNull = isNillable;

            if(attrs!=null)
                for(int i=0; i< attrs.Length;i++) {
                    if ( attrs[i].LocalName == Keywords.MSD_ALLOWDBNULL &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        if ( attrs[i].Value == Keywords.FALSE) 
                            column.AllowDBNull = false;
                    if ( attrs[i].LocalName == Keywords.MSD_ORDINAL &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        tmp = attrs[i].Value;
                    if ( attrs[i].LocalName == Keywords.MSD_DEFAULTVALUE &&  attrs[i].NamespaceURI == Keywords.MSDNS)
                        defValue = attrs[i].Value;
                }
            int ordinal = (int)Convert.ChangeType(tmp, typeof(int), null);

            
            //SetExtProperties(column, attr.UnhandledAttributes);

            if ((column.Expression!=null)&&(column.Expression.Length!=0)) {
                ColumnExpressions.Add(column);
            } 

            column.XmlDataType = strType;
            column.SimpleType = null;

            //column.Namespace = typeNode.SourceUri;
         if(this.FromInference)
             column.Prefix = GetPrefix(column.Namespace);
            if (isToAdd) {
                if (this.FromInference) // move this setting to SetProperties
                    column.AllowDBNull = true;
                if(ordinal>-1 && ordinal<table.Columns.Count)
                    table.Columns.AddAt(ordinal, column);
                else
                    table.Columns.Add(column);
            }

            if (defValue != null)
                try {
                    column.DefaultValue = column.ConvertXmlToObject(defValue);
                }
                catch (System.FormatException) {
                    throw ExceptionBuilder.CannotConvert(defValue, type.FullName);
                }    


        }

        internal void HandleAttributeColumn(XmlSchemaAttribute attrib, DataTable table, bool isBase){
            Type type = null;
            XmlSchemaAttribute attr = attrib.Name != null ? attrib : (XmlSchemaAttribute) attributes[attrib.RefName];


            XmlSchemaAnnotated typeNode = FindTypeNode(attr);
            String strType = null;
            SimpleType xsdType = null;

            if (typeNode == null) {
                strType = attr.SchemaTypeName.Name;
                if (Common.ADP.IsEmpty(strType)) {
                    strType = "";
                    type = typeof(string);
                }
                else {
                    if (attr.SchemaTypeName.Namespace != Keywords.XSDNS) // it is UD Simple Type, can it be?
//
                        type = ParseDataType(attr.SchemaTypeName.ToString());
                    else
                        type = ParseDataType(attr.SchemaTypeName.Name);
                }
            }
            else if (typeNode is XmlSchemaSimpleType) {
                // 
                XmlSchemaSimpleType node = typeNode as XmlSchemaSimpleType;
                xsdType = new SimpleType(node);
                if (node.QualifiedName.Name != null && node.QualifiedName.Name.Length != 0 && node.QualifiedName.Namespace != Keywords.XSDNS) { // this means UDSimpleType
                    strType = node.QualifiedName.ToString(); // use qualifed name
                    type = ParseDataType(node.QualifiedName.ToString());// search with QName
                }
                else {
                    type = ParseDataType(xsdType.BaseType);
                    strType = xsdType.Name;
                    if(xsdType.Length == 1 && type == typeof(string)) {
                        type = typeof(Char);
                    }
                }
            }
            else if (typeNode is XmlSchemaElement) {
                strType = ((XmlSchemaElement)typeNode).SchemaTypeName.Name;
                type = ParseDataType(strType);
            }
            else {
                if (typeNode.Id == null)
                    throw ExceptionBuilder.DatatypeNotDefined();
                else
                    throw ExceptionBuilder.UndefinedDatatype(typeNode.Id);
            }

            DataColumn column;
            string columnName = XmlConvert.DecodeName(GetInstanceName(attr));
            bool isToAdd = true;

            if ((!isBase || FromInference) && (table.Columns.Contains(columnName, true))) {
                column = table.Columns[columnName];
                isToAdd = false;

                if (FromInference) { // for backward compatability with old inference
            // throw eception if same column is being aded with different mapping
                    if (column.ColumnMapping != MappingType.Attribute)
                        throw ExceptionBuilder.ColumnTypeConflict(column.ColumnName);
                    // in previous inference , if we have incoming column with different NS, we think as different column and 
                    //while adding , since there is no NS concept for datacolumn, we used to throw exception
                    // simulate the same behavior.
                    if ((Common.ADP.IsEmpty(attrib.QualifiedName.Namespace) && Common.ADP.IsEmpty(column._columnUri)) || // backward compatability :SQL BU DT 310912
                        (string.Compare(attrib.QualifiedName.Namespace, column.Namespace, StringComparison.Ordinal) == 0))
                    {
                        return; // backward compatability
                    }
                    column = new DataColumn(columnName, type, null, MappingType.Attribute); // this is to fix issue with Exception we used to throw for old inference engine if column
                    //exists with different namespace; while adding it to columncollection
                    isToAdd = true;
                }

            }
            else {
                column = new DataColumn(columnName, type, null, MappingType.Attribute);
            }

            SetProperties(column, attr.UnhandledAttributes);
            HandleColumnExpression(column, attr.UnhandledAttributes);
            SetExtProperties(column, attr.UnhandledAttributes);

            if ((column.Expression!=null)&&(column.Expression.Length!=0)) {
                ColumnExpressions.Add(column);
            }

            if (xsdType != null && xsdType.Name != null && xsdType.Name.Length > 0) {
                if (XSDSchema.GetMsdataAttribute(typeNode, Keywords.TARGETNAMESPACE)!= null) {
                  column.XmlDataType = xsdType.SimpleTypeQualifiedName;
                }
            }
            else {
                column.XmlDataType = strType;
            }

            column.SimpleType = xsdType;

            column.AllowDBNull = !(attrib.Use == XmlSchemaUse.Required);
            column.Namespace = attrib.QualifiedName.Namespace;
            column.Namespace = GetStringAttribute(attrib, "targetNamespace", column.Namespace);

            if (isToAdd) {
                if (this.FromInference) { // move this setting to SetProperties
                    column.AllowDBNull = true;
                    column.Prefix = GetPrefix(column.Namespace);
                }
              table.Columns.Add(column);
            }

            if (attrib.Use == XmlSchemaUse.Prohibited) {
                column.ColumnMapping = MappingType.Hidden;
                
                column.AllowDBNull = GetBooleanAttribute(attr, Keywords.MSD_ALLOWDBNULL, true)  ;
                String defValue = GetMsdataAttribute(attr, Keywords.MSD_DEFAULTVALUE);
                if (defValue != null)
                    try {
                        column.DefaultValue = column.ConvertXmlToObject(defValue);
                    }
                    catch (System.FormatException) {
                        throw ExceptionBuilder.CannotConvert(defValue, type.FullName);
                    }    
            }


            // XDR [....] change
            string strDefault = (attrib.Use == XmlSchemaUse.Required) ? GetMsdataAttribute(attr, Keywords.MSD_DEFAULTVALUE) : attr.DefaultValue;
            if ((attr.Use == XmlSchemaUse.Optional) && (strDefault == null ))
                strDefault = attr.FixedValue;
            
            if (strDefault != null)
                try {
                    column.DefaultValue = column.ConvertXmlToObject(strDefault);
                }
                catch (System.FormatException) {
                    throw ExceptionBuilder.CannotConvert(strDefault, type.FullName);
                }    
        }

        internal void HandleElementColumn(XmlSchemaElement elem, DataTable table, bool isBase){
            Type type = null;
            XmlSchemaElement el = elem.Name != null ? elem : (XmlSchemaElement) elementsTable[elem.RefName];

            if (el == null) // it's possible due to some XSD compiler optimizations
                return; // do nothing
            
            XmlSchemaAnnotated typeNode = FindTypeNode(el);
            String strType = null;
            SimpleType xsdType = null;

            if (typeNode == null) {
                strType = el.SchemaTypeName.Name;
                if (Common.ADP.IsEmpty(strType)) {
                    strType = "";
                    type = typeof(string);
                }
                else {
                    type = ParseDataType(el.SchemaTypeName.Name);
                }
            }
            else if (typeNode is XmlSchemaSimpleType) {
                // 
                XmlSchemaSimpleType simpleTypeNode = typeNode as XmlSchemaSimpleType;
                xsdType = new SimpleType(simpleTypeNode);
             // ((XmlSchemaSimpleType)typeNode).Name != null && ((XmlSchemaSimpleType)typeNode).Name.Length != 0 check is for annonymos simple type, 
             // it should be  user defined  Named  simple type
                if (((XmlSchemaSimpleType)typeNode).Name != null && ((XmlSchemaSimpleType)typeNode).Name.Length != 0 && ((XmlSchemaSimpleType)typeNode).QualifiedName.Namespace != Keywords.XSDNS) {

                    string targetNamespace = XSDSchema.GetMsdataAttribute(typeNode, Keywords.TARGETNAMESPACE);
                    strType = ((XmlSchemaSimpleType)typeNode).QualifiedName.ToString(); // use qualifed name
                    type = ParseDataType(strType);
                }
                else {
                    simpleTypeNode = (xsdType.XmlBaseType!= null && xsdType.XmlBaseType.Namespace != Keywords.XSDNS)  ?
                                                schemaTypes[xsdType.XmlBaseType] as XmlSchemaSimpleType :
                                                null;
                    while (simpleTypeNode != null) {
                        xsdType.LoadTypeValues(simpleTypeNode);
                        simpleTypeNode = (xsdType.XmlBaseType!= null && xsdType.XmlBaseType.Namespace != Keywords.XSDNS)  ?
                                                    schemaTypes[xsdType.XmlBaseType] as XmlSchemaSimpleType :
                                                    null;
                   }

                    type = ParseDataType(xsdType.BaseType);
                    strType = xsdType.Name;

                    if(xsdType.Length == 1 && type == typeof(string)) {
                        type = typeof(Char);
                    }
                }
            }
            else if (typeNode is XmlSchemaElement) { // theoratically no named simpletype should come here
                strType = ((XmlSchemaElement)typeNode).SchemaTypeName.Name;
                type = ParseDataType(strType);
            }
            else if (typeNode is XmlSchemaComplexType) {
                if (ADP.IsEmpty(XSDSchema.GetMsdataAttribute(elem, Keywords.MSD_DATATYPE))) {
                    throw ExceptionBuilder.DatatypeNotDefined();
                }
                else {
                    type = typeof(object);
                }
            }
            else {
                if (typeNode.Id == null)
                    throw ExceptionBuilder.DatatypeNotDefined();
                else
                    throw ExceptionBuilder.UndefinedDatatype(typeNode.Id);
            }

            DataColumn column;
            string columnName = XmlConvert.DecodeName(GetInstanceName(el));
            bool isToAdd = true;

            if (((!isBase) || FromInference) && (table.Columns.Contains(columnName, true))) {
                column = table.Columns[columnName];
                isToAdd = false;
                
                if (FromInference) { // for backward compatability with old inference
                    if (column.ColumnMapping != MappingType.Element)
                        throw ExceptionBuilder.ColumnTypeConflict(column.ColumnName);
                    // in previous inference , if we have incoming column with different NS, we think as different column and 
                    //while adding , since there is no NS concept for datacolumn, we used to throw exception
                    // simulate the same behavior.
                    if ((Common.ADP.IsEmpty(elem.QualifiedName.Namespace) && Common.ADP.IsEmpty(column._columnUri)) || // backward compatability :SQL BU DT 310912
                        (string.Compare(elem.QualifiedName.Namespace, column.Namespace, StringComparison.Ordinal) == 0))
                    {
                        return; // backward compatability
                    }
                    column =   new DataColumn(columnName, type, null, MappingType.Element);// this is to fix issue with Exception we used to throw for old inference engine if column
                    //exists with different namespace; while adding it to columncollection
                    isToAdd = true;
                }
            }
            else {
                column = new DataColumn(columnName, type, null, MappingType.Element);
            }

            SetProperties(column, el.UnhandledAttributes);
            HandleColumnExpression(column, el.UnhandledAttributes);
            SetExtProperties(column, el.UnhandledAttributes);

            if (!Common.ADP.IsEmpty(column.Expression)) {
                ColumnExpressions.Add(column);
            }

// Update XSD type to point to simple types actual namespace instead of normalized default namespace in case of remoting
            if (xsdType != null && xsdType.Name != null && xsdType.Name.Length > 0) {
                if (XSDSchema.GetMsdataAttribute(typeNode, Keywords.TARGETNAMESPACE)!= null) {
                  column.XmlDataType = xsdType.SimpleTypeQualifiedName;
                }
            }
            else {
                column.XmlDataType = strType;
            }
            column.SimpleType = xsdType;            

            column.AllowDBNull = this.FromInference ||(elem.MinOccurs == 0 ) || elem.IsNillable;


            if (!elem.RefName.IsEmpty || elem.QualifiedName.Namespace != table.Namespace)
            { // if ref element (or in diferent NS) it is global element, so form MUST BE Qualified
                column.Namespace = elem.QualifiedName.Namespace;
                column.Namespace = GetStringAttribute(el, "targetNamespace", column.Namespace);
            }
            else { // it is local, hence check for 'form' on local element, if not specified, check for 'elemenfformdefault' on schema element
                if (elem.Form == XmlSchemaForm.Unqualified) {
                    column.Namespace = String.Empty;                    
                }
                else if (elem.Form == XmlSchemaForm.None) {
                    XmlSchemaObject e = (XmlSchemaObject)elem.Parent;
                    while (e.Parent != null) {
                        e = e.Parent;
                    }
                    if (((XmlSchema)e).ElementFormDefault == XmlSchemaForm.Unqualified) {
                        column.Namespace = String.Empty;
                    }
                }
                else {
                    column.Namespace = elem.QualifiedName.Namespace;
                    column.Namespace = GetStringAttribute(el, "targetNamespace", column.Namespace);
                }
            }

            String tmp = GetStringAttribute(elem, Keywords.MSD_ORDINAL, (-1).ToString(CultureInfo.CurrentCulture));
            int ordinal = (int)Convert.ChangeType(tmp, typeof(int), null);

            if(isToAdd) {
                if(ordinal>-1 && ordinal<table.Columns.Count)
                    table.Columns.AddAt(ordinal, column);
                else
                    table.Columns.Add(column);
            }

            if (column.Namespace == table.Namespace)
                column._columnUri = null; // to not raise a column change namespace again

            if(this.FromInference) {// search for prefix after adding to table, so NS has its final value, and 
                column.Prefix = GetPrefix(column.Namespace); // it can inherit its NS from DataTable, if it is null
            }

            string strDefault = el.DefaultValue;
            if (strDefault != null )
                try {
                    column.DefaultValue = column.ConvertXmlToObject(strDefault);
                }
                catch (System.FormatException) {

                    throw ExceptionBuilder.CannotConvert(strDefault, type.FullName);
                }    
        }

        internal void HandleDataSet(XmlSchemaElement node, bool isNewDataSet) {
            string dsName = node.Name;
            string dsNamespace = node.QualifiedName.Namespace;
            int initialTableCount = _ds.Tables.Count; // just use for inference backward compatablity

            List<DataTable> tableSequenceList = new List<DataTable>();

            String value = GetMsdataAttribute(node, Keywords.MSD_LOCALE);
            if (null != value) { // set by user
                if (0 != value.Length) {
                    // <... msdata:Locale="en-US"/>
                    _ds.Locale = new CultureInfo(value);
                }
                else {
                    // everett bug behavior before <... msdata:Locale=""/> becoming CultureInfo(0x409)
                    _ds.Locale = CultureInfo.InvariantCulture;
                }
            }
            else { // not set by user
                // MSD_LOCALE overrides MSD_USECURRENTLOCALE
                if (GetBooleanAttribute(node, Keywords.MSD_USECURRENTLOCALE, false)) {
                    _ds.SetLocaleValue(CultureInfo.CurrentCulture, false);
                }
                else {
                    // everett behavior before <... msdata:UseCurrentLocale="true"/>
                    _ds.SetLocaleValue(new CultureInfo(0x409), false);
                }
            }
            
            // reuse variable
            value =  GetMsdataAttribute(node, Keywords.MSD_DATASETNAME);
            if (value!=null && value.Length != 0) {
                dsName = value;
            }

            value =  GetMsdataAttribute(node, Keywords.MSD_DATASETNAMESPACE);
            if (value!=null && value.Length != 0) {
                dsNamespace = value;
            }

            SetProperties(_ds, node.UnhandledAttributes);
            SetExtProperties(_ds, node.UnhandledAttributes);


            if (dsName != null && dsName.Length != 0)
                _ds.DataSetName = XmlConvert.DecodeName(dsName);

 //            _ds.Namespace = node.QualifiedName.Namespace;
            _ds.Namespace = dsNamespace;

            if (this.FromInference)
                _ds.Prefix = GetPrefix(_ds.Namespace);

            XmlSchemaComplexType ct = (XmlSchemaComplexType) FindTypeNode(node);
            if (ct.Particle != null) {
                XmlSchemaObjectCollection items = GetParticleItems(ct.Particle);
            
                if (items == null) {
                    return;
                }

                foreach (XmlSchemaAnnotated el in items){
                    if (el is XmlSchemaElement) {
                        if(((XmlSchemaElement)el).RefName.Name.Length != 0) {
                            if (!FromInference) {
                                continue;
                            }
                            else {
                                DataTable tempTable = _ds.Tables.GetTable(XmlConvert.DecodeName(GetInstanceName((XmlSchemaElement)el)), node.QualifiedName.Namespace);
                                if (tempTable != null) {
                                    tableSequenceList.Add(tempTable); // if ref table is created, add it
                                }
                                bool isComplexTypeOrValidElementType = false;
                                if (node.ElementSchemaType != null || !(((XmlSchemaElement)el).SchemaType is XmlSchemaComplexType)) {
                                    isComplexTypeOrValidElementType = true;
                                }
 //                          bool isComplexTypeOrValidElementType = (node.ElementType != null || !(((XmlSchemaElement)el).SchemaType is XmlSchemaComplexType));
                                if ((((XmlSchemaElement)el).MaxOccurs != Decimal.One )  && (!isComplexTypeOrValidElementType)) {
                                    continue;
                                }
                            }
                        }

                        DataTable child = HandleTable ((XmlSchemaElement)el);
                        if (child!=null) {
                            child.fNestedInDataset = true;
                        }
                        if (FromInference) {
                            tableSequenceList.Add(child);
                        }
                    }
                    else if (el is XmlSchemaChoice){ // should we check for inference?
                        XmlSchemaObjectCollection choiceItems = ((XmlSchemaChoice)el).Items;
                        if (choiceItems == null)
                            continue;
                        foreach (XmlSchemaAnnotated choiceEl in choiceItems) {
                            if (choiceEl is XmlSchemaElement) {
                                if (((XmlSchemaParticle)el).MaxOccurs > Decimal.One && (((XmlSchemaElement)choiceEl).SchemaType is XmlSchemaComplexType)) // amir
                                ((XmlSchemaElement)choiceEl).MaxOccurs = ((XmlSchemaParticle)el).MaxOccurs;
                                if ((((XmlSchemaElement)choiceEl).RefName.Name.Length != 0) && (!FromInference && ((XmlSchemaElement)choiceEl).MaxOccurs != Decimal.One && !(((XmlSchemaElement)choiceEl).SchemaType is XmlSchemaComplexType)))
                                    continue;

                                DataTable child = HandleTable ((XmlSchemaElement)choiceEl);
                                if (FromInference) {
                                    tableSequenceList.Add(child);
                                }
                                if (child != null)
                                {
                                    child.fNestedInDataset = true;
                                }
                            }
                        }
                    }
                } 
            }

            // Handle the non-nested keyref constraints
            if (node.Constraints != null) {
                foreach (XmlSchemaIdentityConstraint key in node.Constraints) {
                        XmlSchemaKeyref keyref = key as XmlSchemaKeyref;
                        if (keyref == null)
                            continue;

                        bool isNested = GetBooleanAttribute(keyref, Keywords.MSD_ISNESTED,  /*default:*/ false);                                
                        if (isNested)
                            continue;

                        HandleKeyref(keyref);
                    }
            }
            if (FromInference && isNewDataSet) {
                List<DataTable> _tableList = new List<DataTable>(_ds.Tables.Count);
                foreach(DataTable dt in tableSequenceList) {
                    AddTablesToList(_tableList, dt);
                }
                _ds.Tables.ReplaceFromInference(_tableList); // replace the list with the one in correct order: BackWard compatability for inference
            }
        }

        private void AddTablesToList(List<DataTable> tableList, DataTable dt) { // kind of depth _first travarsal
            if (!tableList.Contains(dt)) {
                tableList.Add(dt);
                foreach(DataTable childTable in tableDictionary[dt]) {
                    AddTablesToList(tableList, childTable);
                }
            }
        }
        
        private string GetPrefix(string ns) {
            if (ns == null)
                return null;
            foreach (XmlSchema schemaRoot in _schemaSet.Schemas()) {
                XmlQualifiedName[] qualifiedNames = schemaRoot.Namespaces.ToArray();
                for(int i = 0; i < qualifiedNames.Length; i++) {
                    if (qualifiedNames[i].Namespace == ns)
                        return qualifiedNames[i].Name;
                }
            }
            return null;
        }

        private string GetNamespaceFromPrefix(string prefix) {
            if ((prefix == null) ||(prefix.Length == 0))
                return null;
            foreach (XmlSchema schemaRoot in _schemaSet.Schemas()) {
                XmlQualifiedName[] qualifiedNames = schemaRoot.Namespaces.ToArray();
                for(int i = 0; i < qualifiedNames.Length; i++) {
                    if (qualifiedNames[i].Name == prefix)
                        return qualifiedNames[i].Namespace;
                }
            }
            return null;
        }


        private String GetTableNamespace(XmlSchemaIdentityConstraint key) {
            string xpath = key.Selector.XPath;
            string [] split = xpath.Split('/');
            string prefix =string.Empty;
            
            string QualifiedTableName = split[split.Length - 1]; //get the last string after '/' and ':'
            
            if ((QualifiedTableName == null) || (QualifiedTableName.Length == 0))
                throw ExceptionBuilder.InvalidSelector(xpath);

            if (QualifiedTableName.IndexOf(':') != -1)
                prefix = QualifiedTableName.Substring(0, QualifiedTableName.IndexOf(':'));
            else
                return GetMsdataAttribute(key, Keywords.MSD_TABLENS);

            prefix  = XmlConvert.DecodeName(prefix );

            return GetNamespaceFromPrefix(prefix);
        }
        
        private String GetTableName(XmlSchemaIdentityConstraint key) {
            string xpath = key.Selector.XPath;
            string [] split = xpath.Split('/',':');
            String tableName = split[split.Length - 1]; //get the last string after '/' and ':'
            
            if ((tableName == null) || (tableName.Length == 0))
                throw ExceptionBuilder.InvalidSelector(xpath);
                
            tableName = XmlConvert.DecodeName(tableName);
            return tableName;            
        }
        
        internal bool IsTable(XmlSchemaElement node) {
            if (node.MaxOccurs == decimal.Zero)
                return false;

            XmlAttribute[] attribs = node.UnhandledAttributes;
            if (attribs != null) {
                for(int i = 0; i < attribs.Length; i++) {
                    XmlAttribute attrib = attribs[i];
                    if (attrib.LocalName == Keywords.MSD_DATATYPE &&
                        attrib.Prefix == Keywords.MSD &&
                        attrib.NamespaceURI == Keywords.MSDNS)
                        return false;
                }
            }

            Object typeNode = FindTypeNode(node);

            if ( (node.MaxOccurs > decimal.One) && typeNode == null ){
                return true;
            }
            

            if ((typeNode==null) || !(typeNode is XmlSchemaComplexType)) {
                return false;
            }

            XmlSchemaComplexType ctNode = (XmlSchemaComplexType) typeNode;

            if (ctNode.IsAbstract)
                throw ExceptionBuilder.CannotInstantiateAbstract(node.Name);

            return true;
        }

//        internal bool IsTopLevelElement (XmlSchemaElement node) {
//            return (elements.IndexOf(node) != -1);           
//        }
        internal DataTable HandleTable(XmlSchemaElement node) {
        
            if (!IsTable(node))
                return null;
            
            Object typeNode = FindTypeNode(node);

            if ( (node.MaxOccurs > decimal.One) && typeNode == null ){
                return InstantiateSimpleTable(node);
            }
            
            DataTable table = InstantiateTable(node,(XmlSchemaComplexType)typeNode, (node.RefName != null)); // this is wrong , correct check should be node.RefName.IsEmpty

            table.fNestedInDataset = false;
            return table;

        }

    }

    internal sealed class XmlIgnoreNamespaceReader : XmlNodeReader {
        private List<string> namespacesToIgnore;    
    // 
    // Constructor
    //
        internal XmlIgnoreNamespaceReader(XmlDocument xdoc, string[] namespacesToIgnore) : base(xdoc) {
            this.namespacesToIgnore = new List<string>(namespacesToIgnore);
        }

    //
    // XmlReader implementation
    //

        public override bool MoveToFirstAttribute() {
            if (base.MoveToFirstAttribute()) {
                if ( namespacesToIgnore.Contains(this.NamespaceURI) ||
                    (this.NamespaceURI == Keywords.XML_XMLNS && this.LocalName != "lang")) { //try next one
                    return MoveToNextAttribute();
                }
                else {
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute() {
            bool moved, flag;
            do {
                moved = false;
                flag = false;
                if (base.MoveToNextAttribute()) {
                    moved = true;

                    if ( namespacesToIgnore.Contains(this.NamespaceURI) ||
                        (this.NamespaceURI == Keywords.XML_XMLNS && this.LocalName != "lang")) {
                        flag = true;
                    }
                }
            } while(flag);
            return moved;
        }
    }
}
