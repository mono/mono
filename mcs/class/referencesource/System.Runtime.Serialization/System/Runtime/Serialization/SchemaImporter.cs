//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Runtime.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;
    using SchemaObjectDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, SchemaObjectInfo>;
    using System.Runtime.Serialization.Diagnostics;

    class SchemaImporter
    {
        DataContractSet dataContractSet;
        XmlSchemaSet schemaSet;
        ICollection<XmlQualifiedName> typeNames;
        ICollection<XmlSchemaElement> elements;
        XmlQualifiedName[] elementTypeNames;
        bool importXmlDataType;
        SchemaObjectDictionary schemaObjects;
        List<XmlSchemaRedefine> redefineList;
        bool needToImportKnownTypesForObject;

        [Fx.Tag.SecurityNote(Critical = "Static field used to store serialization schema elements from future versions."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static Hashtable serializationSchemaElements;

        internal SchemaImporter(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames, ICollection<XmlSchemaElement> elements, XmlQualifiedName[] elementTypeNames, DataContractSet dataContractSet, bool importXmlDataType)
        {
            this.dataContractSet = dataContractSet;
            this.schemaSet = schemas;
            this.typeNames = typeNames;
            this.elements = elements;
            this.elementTypeNames = elementTypeNames;
            this.importXmlDataType = importXmlDataType;
        }

        internal void Import()
        {
            if (!schemaSet.Contains(Globals.SerializationNamespace))
            {
                StringReader reader = new StringReader(Globals.SerializationSchema);
                XmlSchema schema = XmlSchema.Read(new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit }, null);
                if (schema == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CouldNotReadSerializationSchema, Globals.SerializationNamespace)));
                schemaSet.Add(schema);
            }

            try
            {
                CompileSchemaSet(schemaSet);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportInvalidSchemas), ex));
            }

            if (typeNames == null)
            {
                ICollection schemaList = schemaSet.Schemas();
                foreach (object schemaObj in schemaList)
                {
                    if (schemaObj == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportNullSchema)));

                    XmlSchema schema = (XmlSchema)schemaObj;
                    if (schema.TargetNamespace != Globals.SerializationNamespace
                        && schema.TargetNamespace != Globals.SchemaNamespace)
                    {
                        foreach (XmlSchemaObject typeObj in schema.SchemaTypes.Values)
                        {
                            ImportType((XmlSchemaType)typeObj);
                        }
                        foreach (XmlSchemaElement element in schema.Elements.Values)
                        {
                            if (element.SchemaType != null)
                                ImportAnonymousGlobalElement(element, element.QualifiedName, schema.TargetNamespace);
                        }
                    }
                }
            }
            else
            {
                foreach (XmlQualifiedName typeName in typeNames)
                {
                    if (typeName == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportNullDataContractName)));
                    ImportType(typeName);
                }

                if (elements != null)
                {
                    int i = 0;
                    foreach (XmlSchemaElement element in elements)
                    {
                        XmlQualifiedName typeName = element.SchemaTypeName;
                        if (typeName != null && typeName.Name.Length > 0)
                        {
                            elementTypeNames[i++] = ImportType(typeName).StableName;
                        }
                        else
                        {
                            XmlSchema schema = SchemaHelper.GetSchemaWithGlobalElementDeclaration(element, schemaSet);
                            if (schema == null)
                            {
                                elementTypeNames[i++] = ImportAnonymousElement(element, element.QualifiedName).StableName;
                            }
                            else
                            {
                                elementTypeNames[i++] = ImportAnonymousGlobalElement(element, element.QualifiedName, schema.TargetNamespace).StableName;
                            }
                        }
                    }
                }
            }
            ImportKnownTypesForObject();
        }

        internal static void CompileSchemaSet(XmlSchemaSet schemaSet)
        {
            if (schemaSet.Contains(XmlSchema.Namespace))
                schemaSet.Compile();
            else
            {
                // Add base XSD schema with top level element named "schema"
                XmlSchema xsdSchema = new XmlSchema();
                xsdSchema.TargetNamespace = XmlSchema.Namespace;
                XmlSchemaElement element = new XmlSchemaElement();
                element.Name = Globals.SchemaLocalName;
                element.SchemaType = new XmlSchemaComplexType();
                xsdSchema.Items.Add(element);
                schemaSet.Add(xsdSchema);
                schemaSet.Compile();
            }
        }

        SchemaObjectDictionary SchemaObjects
        {
            get
            {
                if (schemaObjects == null)
                    schemaObjects = CreateSchemaObjects();
                return schemaObjects;
            }
        }

        List<XmlSchemaRedefine> RedefineList
        {
            get
            {
                if (redefineList == null)
                    redefineList = CreateRedefineList();
                return redefineList;
            }
        }

        void ImportKnownTypes(XmlQualifiedName typeName)
        {
            SchemaObjectInfo schemaObjectInfo;
            if (SchemaObjects.TryGetValue(typeName, out schemaObjectInfo))
            {
                List<XmlSchemaType> knownTypes = schemaObjectInfo.knownTypes;
                if (knownTypes != null)
                {
                    foreach (XmlSchemaType knownType in knownTypes)
                        ImportType(knownType);
                }
            }
        }

        internal static bool IsObjectContract(DataContract dataContract)
        {
            Dictionary<Type, object> previousCollectionTypes = new Dictionary<Type, object>();
            while (dataContract is CollectionDataContract)
            {
                if (dataContract.OriginalUnderlyingType == null)
                {
                    dataContract = ((CollectionDataContract)dataContract).ItemContract;
                    continue;
                }

                if (!previousCollectionTypes.ContainsKey(dataContract.OriginalUnderlyingType))
                {
                    previousCollectionTypes.Add(dataContract.OriginalUnderlyingType, dataContract.OriginalUnderlyingType);
                    dataContract = ((CollectionDataContract)dataContract).ItemContract;
                }
                else
                {
                    break;
                }
            }

            return dataContract is PrimitiveDataContract && ((PrimitiveDataContract)dataContract).UnderlyingType == Globals.TypeOfObject;
        }

        void ImportKnownTypesForObject()
        {
            if (!needToImportKnownTypesForObject)
                return;
            needToImportKnownTypesForObject = false;
            if (dataContractSet.KnownTypesForObject == null)
            {
                SchemaObjectInfo schemaObjectInfo;
                if (SchemaObjects.TryGetValue(SchemaExporter.AnytypeQualifiedName, out schemaObjectInfo))
                {
                    List<XmlSchemaType> knownTypes = schemaObjectInfo.knownTypes;
                    if (knownTypes != null)
                    {
                        DataContractDictionary knownDataContracts = new DataContractDictionary();
                        foreach (XmlSchemaType knownType in knownTypes)
                        {
                            // Expected: will throw exception if schema set contains types that are not supported 
                            DataContract dataContract = ImportType(knownType);
                            DataContract existingContract;
                            if (!knownDataContracts.TryGetValue(dataContract.StableName, out existingContract))
                            {
                                knownDataContracts.Add(dataContract.StableName, dataContract);
                            }
                        }
                        dataContractSet.KnownTypesForObject = knownDataContracts;
                    }
                }
            }
        }

        internal SchemaObjectDictionary CreateSchemaObjects()
        {
            SchemaObjectDictionary schemaObjects = new SchemaObjectDictionary();
            ICollection schemaList = schemaSet.Schemas();
            List<XmlSchemaType> knownTypesForObject = new List<XmlSchemaType>();
            schemaObjects.Add(SchemaExporter.AnytypeQualifiedName, new SchemaObjectInfo(null, null, null, knownTypesForObject));

            foreach (XmlSchema schema in schemaList)
            {
                if (schema.TargetNamespace != Globals.SerializationNamespace)
                {
                    foreach (XmlSchemaObject schemaObj in schema.SchemaTypes.Values)
                    {
                        XmlSchemaType schemaType = schemaObj as XmlSchemaType;
                        if (schemaType != null)
                        {
                            knownTypesForObject.Add(schemaType);

                            XmlQualifiedName currentTypeName = new XmlQualifiedName(schemaType.Name, schema.TargetNamespace);
                            SchemaObjectInfo schemaObjectInfo;
                            if (schemaObjects.TryGetValue(currentTypeName, out schemaObjectInfo))
                            {
                                schemaObjectInfo.type = schemaType;
                                schemaObjectInfo.schema = schema;
                            }
                            else
                            {
                                schemaObjects.Add(currentTypeName, new SchemaObjectInfo(schemaType, null, schema, null));
                            }

                            XmlQualifiedName baseTypeName = GetBaseTypeName(schemaType);
                            if (baseTypeName != null)
                            {
                                SchemaObjectInfo baseTypeInfo;
                                if (schemaObjects.TryGetValue(baseTypeName, out baseTypeInfo))
                                {
                                    if (baseTypeInfo.knownTypes == null)
                                    {
                                        baseTypeInfo.knownTypes = new List<XmlSchemaType>();
                                    }
                                }
                                else
                                {
                                    baseTypeInfo = new SchemaObjectInfo(null, null, null, new List<XmlSchemaType>());
                                    schemaObjects.Add(baseTypeName, baseTypeInfo);
                                }
                                baseTypeInfo.knownTypes.Add(schemaType);
                            }
                        }
                    }
                    foreach (XmlSchemaObject schemaObj in schema.Elements.Values)
                    {
                        XmlSchemaElement schemaElement = schemaObj as XmlSchemaElement;
                        if (schemaElement != null)
                        {
                            XmlQualifiedName currentElementName = new XmlQualifiedName(schemaElement.Name, schema.TargetNamespace);
                            SchemaObjectInfo schemaObjectInfo;
                            if (schemaObjects.TryGetValue(currentElementName, out schemaObjectInfo))
                            {
                                schemaObjectInfo.element = schemaElement;
                                schemaObjectInfo.schema = schema;
                            }
                            else
                            {
                                schemaObjects.Add(currentElementName, new SchemaObjectInfo(null, schemaElement, schema, null));
                            }
                        }
                    }
                }
            }
            return schemaObjects;
        }

        XmlQualifiedName GetBaseTypeName(XmlSchemaType type)
        {
            XmlQualifiedName baseTypeName = null;
            XmlSchemaComplexType complexType = type as XmlSchemaComplexType;
            if (complexType != null)
            {
                if (complexType.ContentModel != null)
                {
                    XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;
                    if (complexContent != null)
                    {
                        XmlSchemaComplexContentExtension extension = complexContent.Content as XmlSchemaComplexContentExtension;
                        if (extension != null)
                            baseTypeName = extension.BaseTypeName;
                    }
                }
            }
            return baseTypeName;
        }

        List<XmlSchemaRedefine> CreateRedefineList()
        {
            List<XmlSchemaRedefine> list = new List<XmlSchemaRedefine>();

            ICollection schemaList = schemaSet.Schemas();
            foreach (object schemaObj in schemaList)
            {
                XmlSchema schema = schemaObj as XmlSchema;
                if (schema == null)
                    continue;
                foreach (XmlSchemaExternal ext in schema.Includes)
                {
                    XmlSchemaRedefine redefine = ext as XmlSchemaRedefine;
                    if (redefine != null)
                        list.Add(redefine);
                }
            }

            return list;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on XmlDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        DataContract ImportAnonymousGlobalElement(XmlSchemaElement element, XmlQualifiedName typeQName, string ns)
        {
            DataContract contract = ImportAnonymousElement(element, typeQName);
            XmlDataContract xmlDataContract = contract as XmlDataContract;
            if (xmlDataContract != null)
            {
                xmlDataContract.SetTopLevelElementName(new XmlQualifiedName(element.Name, ns));
                xmlDataContract.IsTopLevelElementNullable = element.IsNillable;
            }
            return contract;
        }

        DataContract ImportAnonymousElement(XmlSchemaElement element, XmlQualifiedName typeQName)
        {
            if (SchemaHelper.GetSchemaType(SchemaObjects, typeQName) != null)
            {
                for (int i = 1;; i++)
                {
                    typeQName = new XmlQualifiedName(typeQName.Name + i.ToString(NumberFormatInfo.InvariantInfo), typeQName.Namespace);
                    if (SchemaHelper.GetSchemaType(SchemaObjects, typeQName) == null)
                        break;
                    if (i == Int32.MaxValue)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.CannotComputeUniqueName, element.Name)));
                }
            }
            if (element.SchemaType == null)
                return ImportType(SchemaExporter.AnytypeQualifiedName);
            else
                return ImportType(element.SchemaType, typeQName, true/*isAnonymous*/);
        }

        DataContract ImportType(XmlQualifiedName typeName)
        {
            DataContract dataContract = DataContract.GetBuiltInDataContract(typeName.Name, typeName.Namespace);
            if (dataContract == null)
            {
                XmlSchemaType type = SchemaHelper.GetSchemaType(SchemaObjects, typeName);
                if (type == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SpecifiedTypeNotFoundInSchema, typeName.Name, typeName.Namespace)));
                dataContract = ImportType(type);
            }
            if (IsObjectContract(dataContract))
                needToImportKnownTypesForObject = true;
            return dataContract;
        }

        DataContract ImportType(XmlSchemaType type)
        {
            return ImportType(type, type.QualifiedName, false/*isAnonymous*/);
        }


        DataContract ImportType(XmlSchemaType type, XmlQualifiedName typeName, bool isAnonymous)
        {
            DataContract dataContract = dataContractSet[typeName];
            if (dataContract != null)
                return dataContract;

            InvalidDataContractException invalidContractException;
            try
            {
                foreach (XmlSchemaRedefine redefine in RedefineList)
                {
                    if (redefine.SchemaTypes[typeName] != null)
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.RedefineNotSupported));
                }

                if (type is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType)type;
                    XmlSchemaSimpleTypeContent content = simpleType.Content;
                    if (content is XmlSchemaSimpleTypeUnion)
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.SimpleTypeUnionNotSupported));
                    else if (content is XmlSchemaSimpleTypeList)
                        dataContract = ImportFlagsEnum(typeName, (XmlSchemaSimpleTypeList)content, simpleType.Annotation);
                    else if (content is XmlSchemaSimpleTypeRestriction)
                    {
                        XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)content;
                        if (CheckIfEnum(restriction))
                        {
                            dataContract = ImportEnum(typeName, restriction, false /*isFlags*/, simpleType.Annotation);
                        }
                        else
                        {
                            dataContract = ImportSimpleTypeRestriction(typeName, restriction);
                            if (dataContract.IsBuiltInDataContract && !isAnonymous)
                            {
                                dataContractSet.InternalAdd(typeName, dataContract);
                            }
                        }
                    }
                }
                else if (type is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType complexType = (XmlSchemaComplexType)type;
                    if (complexType.ContentModel == null)
                    {
                        CheckComplexType(typeName, complexType);
                        dataContract = ImportType(typeName, complexType.Particle, complexType.Attributes, complexType.AnyAttribute, null /* baseTypeName */, complexType.Annotation);
                    }
                    else
                    {
                        XmlSchemaContentModel contentModel = complexType.ContentModel;
                        if (contentModel is XmlSchemaSimpleContent)
                            ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.SimpleContentNotSupported));
                        else if (contentModel is XmlSchemaComplexContent)
                        {
                            XmlSchemaComplexContent complexContent = (XmlSchemaComplexContent)contentModel;
                            if (complexContent.IsMixed)
                                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.MixedContentNotSupported));

                            if (complexContent.Content is XmlSchemaComplexContentExtension)
                            {
                                XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension)complexContent.Content;
                                dataContract = ImportType(typeName, extension.Particle, extension.Attributes, extension.AnyAttribute, extension.BaseTypeName, complexType.Annotation);
                            }
                            else if (complexContent.Content is XmlSchemaComplexContentRestriction)
                            {
                                XmlSchemaComplexContentRestriction restriction = (XmlSchemaComplexContentRestriction)complexContent.Content;
                                XmlQualifiedName baseTypeName = restriction.BaseTypeName;
                                if (baseTypeName == SchemaExporter.AnytypeQualifiedName)
                                    dataContract = ImportType(typeName, restriction.Particle, restriction.Attributes, restriction.AnyAttribute, null /* baseTypeName */, complexType.Annotation);
                                else
                                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ComplexTypeRestrictionNotSupported));
                            }
                        }
                    }
                }

                if (dataContract == null)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, String.Empty);
                if (type.QualifiedName != XmlQualifiedName.Empty)
                    ImportTopLevelElement(typeName);
                ImportDataContractExtension(type, dataContract);
                ImportGenericInfo(type, dataContract);
                ImportKnownTypes(typeName);

                return dataContract;
            }
            catch (InvalidDataContractException e)
            {
                invalidContractException = e;
            }

            // Execution gets to this point if InvalidDataContractException was thrown
            if (importXmlDataType)
            {
                RemoveFailedContract(typeName);
                return ImportXmlDataType(typeName, type, isAnonymous);
            }
            Type referencedType;
            if (dataContractSet.TryGetReferencedType(typeName, dataContract, out referencedType)
                || (string.IsNullOrEmpty(type.Name) && dataContractSet.TryGetReferencedType(ImportActualType(type.Annotation, typeName, typeName), dataContract, out referencedType)))
            {
                if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(referencedType))
                {
                    RemoveFailedContract(typeName);
                    return ImportXmlDataType(typeName, type, isAnonymous);
                }
            }
            XmlDataContract specialContract = ImportSpecialXmlDataType(type, isAnonymous);
            if (specialContract != null)
            {
                this.dataContractSet.Remove(typeName);
                return specialContract;
            }

            throw invalidContractException;
        }

        private void RemoveFailedContract(XmlQualifiedName typeName)
        {
            ClassDataContract oldContract = this.dataContractSet[typeName] as ClassDataContract;
            this.dataContractSet.Remove(typeName);
            if (oldContract != null)
            {
                ClassDataContract ancestorDataContract = oldContract.BaseContract;
                while (ancestorDataContract != null)
                {
                    ancestorDataContract.KnownDataContracts.Remove(typeName);
                    ancestorDataContract = ancestorDataContract.BaseContract;
                }
                if (dataContractSet.KnownTypesForObject != null)
                    dataContractSet.KnownTypesForObject.Remove(typeName);
            }
        }

        bool CheckIfEnum(XmlSchemaSimpleTypeRestriction restriction)
        {
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                if (!(facet is XmlSchemaEnumerationFacet))
                    return false;
            }

            XmlQualifiedName expectedBase = SchemaExporter.StringQualifiedName;
            if (restriction.BaseTypeName != XmlQualifiedName.Empty)
            {
                return ((restriction.BaseTypeName == expectedBase && restriction.Facets.Count > 0) || ImportType(restriction.BaseTypeName) is EnumDataContract);
            }
            else if (restriction.BaseType != null)
            {
                DataContract baseContract = ImportType(restriction.BaseType);
                return (baseContract.StableName == expectedBase || baseContract is EnumDataContract);
            }

            return false;
        }

        bool CheckIfCollection(XmlSchemaSequence rootSequence)
        {
            if (rootSequence.Items == null || rootSequence.Items.Count == 0)
                return false;
            RemoveOptionalUnknownSerializationElements(rootSequence.Items);
            if (rootSequence.Items.Count != 1)
                return false;

            XmlSchemaObject o = rootSequence.Items[0];
            if (!(o is XmlSchemaElement))
                return false;

            XmlSchemaElement localElement = (XmlSchemaElement)o;
            return (localElement.MaxOccursString == Globals.OccursUnbounded || localElement.MaxOccurs > 1);
        }

        bool CheckIfISerializable(XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
        {
            if (rootSequence.Items == null || rootSequence.Items.Count == 0)
                return false;

            RemoveOptionalUnknownSerializationElements(rootSequence.Items);

            if (attributes == null || attributes.Count == 0)
                return false;

            return (rootSequence.Items.Count == 1 && rootSequence.Items[0] is XmlSchemaAny);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes critical static fields.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        void RemoveOptionalUnknownSerializationElements(XmlSchemaObjectCollection items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                XmlSchemaElement element = items[i] as XmlSchemaElement;
                if (element != null && element.RefName != null &&
                   element.RefName.Namespace == Globals.SerializationNamespace &&
                   element.MinOccurs == 0)
                {
                    if (serializationSchemaElements == null)
                    {
                        XmlSchema serializationSchema = XmlSchema.Read(XmlReader.Create(new StringReader(Globals.SerializationSchema)), null);
                        serializationSchemaElements = new Hashtable();
                        foreach (XmlSchemaObject schemaObject in serializationSchema.Items)
                        {
                            XmlSchemaElement schemaElement = schemaObject as XmlSchemaElement;
                            if (schemaElement != null)
                                serializationSchemaElements.Add(schemaElement.Name, schemaElement);
                        }
                    }
                    if (!serializationSchemaElements.ContainsKey(element.RefName.Name))
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        DataContract ImportType(XmlQualifiedName typeName, XmlSchemaParticle rootParticle, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation)
        {
            DataContract dataContract = null;
            bool isDerived = (baseTypeName != null);

            bool isReference;
            ImportAttributes(typeName, attributes, anyAttribute, out isReference);

            if (rootParticle == null)
                dataContract = ImportClass(typeName, new XmlSchemaSequence(), baseTypeName, annotation, isReference);
            else if (!(rootParticle is XmlSchemaSequence))
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.RootParticleMustBeSequence));
            else
            {
                XmlSchemaSequence rootSequence = (XmlSchemaSequence)rootParticle;
                if (rootSequence.MinOccurs != 1)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.RootSequenceMustBeRequired));
                if (rootSequence.MaxOccurs != 1)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.RootSequenceMaxOccursMustBe));

                if (!isDerived && CheckIfCollection(rootSequence))
                    dataContract = ImportCollection(typeName, rootSequence, attributes, annotation, isReference);
                else if (CheckIfISerializable(rootSequence, attributes))
                    dataContract = ImportISerializable(typeName, rootSequence, baseTypeName, attributes, annotation);
                else
                    dataContract = ImportClass(typeName, rootSequence, baseTypeName, annotation, isReference);
            }
            return dataContract;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on ClassDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        ClassDataContract ImportClass(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation, bool isReference)
        {
            ClassDataContract dataContract = new ClassDataContract();
            dataContract.StableName = typeName;
            AddDataContract(dataContract);

            dataContract.IsValueType = IsValueType(typeName, annotation);
            dataContract.IsReference = isReference;
            if (baseTypeName != null)
            {
                ImportBaseContract(baseTypeName, dataContract);
                if (dataContract.BaseContract.IsISerializable)
                {
                    if (IsISerializableDerived(typeName, rootSequence))
                        dataContract.IsISerializable = true;
                    else
                        ThrowTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, SR.GetString(SR.DerivedTypeNotISerializable, baseTypeName.Name, baseTypeName.Namespace));
                }
                if (dataContract.BaseContract.IsReference)
                {
                    dataContract.IsReference = true;
                }
            }

            if (!dataContract.IsISerializable)
            {
                dataContract.Members = new List<DataMember>();
                RemoveOptionalUnknownSerializationElements(rootSequence.Items);
                for (int memberIndex = 0; memberIndex < rootSequence.Items.Count; memberIndex++)
                {
                    XmlSchemaElement element = rootSequence.Items[memberIndex] as XmlSchemaElement;
                    if (element == null)
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.MustContainOnlyLocalElements));
                    ImportClassMember(element, dataContract);
                }
            }

            return dataContract;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on XmlDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        DataContract ImportXmlDataType(XmlQualifiedName typeName, XmlSchemaType xsdType, bool isAnonymous)
        {
            DataContract dataContract = dataContractSet[typeName];
            if (dataContract != null)
                return dataContract;

            XmlDataContract xmlDataContract = ImportSpecialXmlDataType(xsdType, isAnonymous);
            if (xmlDataContract != null)
                return xmlDataContract;

            xmlDataContract = new XmlDataContract();
            xmlDataContract.StableName = typeName;
            xmlDataContract.IsValueType = false;
            AddDataContract(xmlDataContract);
            if (xsdType != null)
            {
                ImportDataContractExtension(xsdType, xmlDataContract);
                xmlDataContract.IsValueType = IsValueType(typeName, xsdType.Annotation);
                xmlDataContract.IsTypeDefinedOnImport = true;
                xmlDataContract.XsdType = isAnonymous ? xsdType : null;
                xmlDataContract.HasRoot = !IsXmlAnyElementType(xsdType as XmlSchemaComplexType);
            }
            else
            {
                //Value type can be used by both nillable and non-nillable elements but reference type cannot be used by non nillable elements
                xmlDataContract.IsValueType = true;
                xmlDataContract.IsTypeDefinedOnImport = false;
                xmlDataContract.HasRoot = true;
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.Trace(TraceEventType.Verbose, TraceCode.XsdImportAnnotationFailed,
                        SR.GetString(SR.TraceCodeXsdImportAnnotationFailed), new StringTraceRecord("Type", typeName.Namespace + ":" + typeName.Name));
                }
            }
            if (!isAnonymous)
            {
                bool isNullable;
                xmlDataContract.SetTopLevelElementName(SchemaHelper.GetGlobalElementDeclaration(schemaSet, typeName, out isNullable));
                xmlDataContract.IsTopLevelElementNullable = isNullable;
            }
            return xmlDataContract;
        }

        private XmlDataContract ImportSpecialXmlDataType(XmlSchemaType xsdType, bool isAnonymous)
        {
            if (!isAnonymous)
                return null;
            XmlSchemaComplexType complexType = xsdType as XmlSchemaComplexType;
            if (complexType == null)
                return null;
            if (IsXmlAnyElementType(complexType))
            {
                //check if the type is XElement
                XmlQualifiedName xlinqTypeName = new XmlQualifiedName("XElement", "http://schemas.datacontract.org/2004/07/System.Xml.Linq");
                Type referencedType;
                if (dataContractSet.TryGetReferencedType(xlinqTypeName, null, out referencedType)
                    && Globals.TypeOfIXmlSerializable.IsAssignableFrom(referencedType))
                {
                    XmlDataContract xmlDataContract = new XmlDataContract(referencedType);
                    AddDataContract(xmlDataContract);
                    return xmlDataContract;
                }
                //otherwise, assume XmlElement
                return (XmlDataContract)DataContract.GetBuiltInDataContract(Globals.TypeOfXmlElement);
            }
            if (IsXmlAnyType(complexType))
                return (XmlDataContract)DataContract.GetBuiltInDataContract(Globals.TypeOfXmlNodeArray);
            return null;
        }

        bool IsXmlAnyElementType(XmlSchemaComplexType xsdType)
        {
            if (xsdType == null)
                return false;

            XmlSchemaSequence sequence = xsdType.Particle as XmlSchemaSequence;
            if (sequence == null)
                return false;

            if (sequence.Items == null || sequence.Items.Count != 1)
                return false;

            XmlSchemaAny any = sequence.Items[0] as XmlSchemaAny;
            if (any == null || any.Namespace != null)
                return false;

            if (xsdType.AnyAttribute != null || (xsdType.Attributes != null && xsdType.Attributes.Count > 0))
                return false;

            return true;
        }

        bool IsXmlAnyType(XmlSchemaComplexType xsdType)
        {
            if (xsdType == null)
                return false;

            XmlSchemaSequence sequence = xsdType.Particle as XmlSchemaSequence;
            if (sequence == null)
                return false;

            if (sequence.Items == null || sequence.Items.Count != 1)
                return false;

            XmlSchemaAny any = sequence.Items[0] as XmlSchemaAny;
            if (any == null || any.Namespace != null)
                return false;

            if (any.MaxOccurs != Decimal.MaxValue)
                return false;

            if (xsdType.AnyAttribute == null || xsdType.Attributes.Count > 0)
                return false;

            return true;
        }

        bool IsValueType(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
        {
            string isValueTypeInnerText = GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsValueTypeName));
            if (isValueTypeInnerText != null)
            {
                try
                {
                    return XmlConvert.ToBoolean(isValueTypeInnerText);
                }
                catch (FormatException fe)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.IsValueTypeFormattedIncorrectly, isValueTypeInnerText, fe.Message));
                }
            }
            return false;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on ClassDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        ClassDataContract ImportISerializable(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation)
        {
            ClassDataContract dataContract = new ClassDataContract();
            dataContract.StableName = typeName;
            dataContract.IsISerializable = true;
            AddDataContract(dataContract);

            dataContract.IsValueType = IsValueType(typeName, annotation);
            if (baseTypeName == null)
                CheckISerializableBase(typeName, rootSequence, attributes);
            else
            {
                ImportBaseContract(baseTypeName, dataContract);
                if (!dataContract.BaseContract.IsISerializable)
                    ThrowISerializableTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, SR.GetString(SR.BaseTypeNotISerializable, baseTypeName.Name, baseTypeName.Namespace));
                if (!IsISerializableDerived(typeName, rootSequence))
                    ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableDerivedContainsOneOrMoreItems));
            }

            return dataContract;
        }

        void CheckISerializableBase(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
        {
            if (rootSequence == null)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableDoesNotContainAny));

            if (rootSequence.Items == null || rootSequence.Items.Count < 1)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableDoesNotContainAny));
            else if (rootSequence.Items.Count > 1)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableContainsMoreThanOneItems));

            XmlSchemaObject o = rootSequence.Items[0];
            if (!(o is XmlSchemaAny))
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableDoesNotContainAny));

            XmlSchemaAny wildcard = (XmlSchemaAny)o;
            XmlSchemaAny iSerializableWildcardElement = SchemaExporter.ISerializableWildcardElement;
            if (wildcard.MinOccurs != iSerializableWildcardElement.MinOccurs)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableWildcardMinOccursMustBe, iSerializableWildcardElement.MinOccurs));
            if (wildcard.MaxOccursString != iSerializableWildcardElement.MaxOccursString)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableWildcardMaxOccursMustBe, iSerializableWildcardElement.MaxOccursString));
            if (wildcard.Namespace != iSerializableWildcardElement.Namespace)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableWildcardNamespaceInvalid, iSerializableWildcardElement.Namespace));
            if (wildcard.ProcessContents != iSerializableWildcardElement.ProcessContents)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableWildcardProcessContentsInvalid, iSerializableWildcardElement.ProcessContents));

            XmlQualifiedName factoryTypeAttributeRefName = SchemaExporter.ISerializableFactoryTypeAttribute.RefName;
            bool containsFactoryTypeAttribute = false;
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    o = attributes[i];
                    if (o is XmlSchemaAttribute)
                    {
                        if (((XmlSchemaAttribute)o).RefName == factoryTypeAttributeRefName)
                        {
                            containsFactoryTypeAttribute = true;
                            break;
                        }
                    }
                }
            }
            if (!containsFactoryTypeAttribute)
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ISerializableMustRefFactoryTypeAttribute, factoryTypeAttributeRefName.Name, factoryTypeAttributeRefName.Namespace));
        }

        bool IsISerializableDerived(XmlQualifiedName typeName, XmlSchemaSequence rootSequence)
        {
            return (rootSequence == null || rootSequence.Items == null || rootSequence.Items.Count == 0);
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical BaseContract property on ClassDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        void ImportBaseContract(XmlQualifiedName baseTypeName, ClassDataContract dataContract)
        {
            ClassDataContract baseContract = ImportType(baseTypeName) as ClassDataContract;
            if (baseContract == null)
                ThrowTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, SR.GetString(dataContract.IsISerializable ? SR.InvalidISerializableDerivation : SR.InvalidClassDerivation, baseTypeName.Name, baseTypeName.Namespace));

            // Note: code ignores IsValueType annotation if derived type exists
            if (baseContract.IsValueType)
                baseContract.IsValueType = false;

            ClassDataContract ancestorDataContract = baseContract;
            while (ancestorDataContract != null)
            {
                DataContractDictionary knownDataContracts = ancestorDataContract.KnownDataContracts;
                if (knownDataContracts == null)
                {
                    knownDataContracts = new DataContractDictionary();
                    ancestorDataContract.KnownDataContracts = knownDataContracts;
                }
                knownDataContracts.Add(dataContract.StableName, dataContract);
                ancestorDataContract = ancestorDataContract.BaseContract;
            }

            dataContract.BaseContract = baseContract;
        }

        void ImportTopLevelElement(XmlQualifiedName typeName)
        {
            XmlSchemaElement topLevelElement = SchemaHelper.GetSchemaElement(SchemaObjects, typeName);
            // Top level element of same name is not required, but is validated if it is present 
            if (topLevelElement == null)
                return;
            else
            {
                XmlQualifiedName elementTypeName = topLevelElement.SchemaTypeName;
                if (elementTypeName.IsEmpty)
                {
                    if (topLevelElement.SchemaType != null)
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.AnonymousTypeNotSupported, typeName.Name, typeName.Namespace));
                    else
                        elementTypeName = SchemaExporter.AnytypeQualifiedName;
                }
                if (elementTypeName != typeName)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.TopLevelElementRepresentsDifferentType, topLevelElement.SchemaTypeName.Name, topLevelElement.SchemaTypeName.Namespace));
                CheckIfElementUsesUnsupportedConstructs(typeName, topLevelElement);
            }
        }

        void ImportClassMember(XmlSchemaElement element, ClassDataContract dataContract)
        {
            XmlQualifiedName typeName = dataContract.StableName;

            if (element.MinOccurs > 1)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ElementMinOccursMustBe, element.Name));
            if (element.MaxOccurs != 1)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ElementMaxOccursMustBe, element.Name));

            DataContract memberTypeContract = null;
            string memberName = element.Name;
            bool memberIsRequired = (element.MinOccurs > 0);
            bool memberIsNullable = element.IsNillable;
            bool memberEmitDefaultValue;
            int memberOrder = 0;

            XmlSchemaForm elementForm = (element.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(SchemaObjects, schemaSet, typeName).ElementFormDefault : element.Form;
            if (elementForm != XmlSchemaForm.Qualified)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.FormMustBeQualified, element.Name));
            CheckIfElementUsesUnsupportedConstructs(typeName, element);

            if (element.SchemaTypeName.IsEmpty)
            {
                if (element.SchemaType != null)
                    memberTypeContract = ImportAnonymousElement(element, new XmlQualifiedName(String.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", typeName.Name, element.Name), typeName.Namespace));
                else if (!element.RefName.IsEmpty)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ElementRefOnLocalElementNotSupported, element.RefName.Name, element.RefName.Namespace));
                else
                    memberTypeContract = ImportType(SchemaExporter.AnytypeQualifiedName);
            }
            else
            {
                XmlQualifiedName memberTypeName = ImportActualType(element.Annotation, element.SchemaTypeName, typeName);
                memberTypeContract = ImportType(memberTypeName);
                if (IsObjectContract(memberTypeContract))
                    needToImportKnownTypesForObject = true;
            }
            bool? emitDefaultValueFromAnnotation = ImportEmitDefaultValue(element.Annotation, typeName);
            if (!memberTypeContract.IsValueType && !memberIsNullable)
            {
                if (emitDefaultValueFromAnnotation != null && emitDefaultValueFromAnnotation.Value)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidEmitDefaultAnnotation, memberName, typeName.Name, typeName.Namespace)));
                memberEmitDefaultValue = false;
            }
            else
                memberEmitDefaultValue = emitDefaultValueFromAnnotation != null ? emitDefaultValueFromAnnotation.Value : Globals.DefaultEmitDefaultValue;

            int prevMemberIndex = dataContract.Members.Count - 1;
            if (prevMemberIndex >= 0)
            {
                DataMember prevMember = dataContract.Members[prevMemberIndex];
                if (prevMember.Order > Globals.DefaultOrder)
                    memberOrder = dataContract.Members.Count;
                DataMember currentMember = new DataMember(memberTypeContract, memberName, memberIsNullable, memberIsRequired, memberEmitDefaultValue, memberOrder);
                int compare = ClassDataContract.DataMemberComparer.Singleton.Compare(prevMember, currentMember);
                if (compare == 0)
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.CannotHaveDuplicateElementNames, memberName));
                else if (compare > 0)
                    memberOrder = dataContract.Members.Count;
            }
            DataMember dataMember = new DataMember(memberTypeContract, memberName, memberIsNullable, memberIsRequired, memberEmitDefaultValue, memberOrder);

            XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
            dataContractSet.SetSurrogateData(dataMember, ImportSurrogateData(ImportAnnotation(element.Annotation, surrogateDataAnnotationName), surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace));

            dataContract.Members.Add(dataMember);
        }

        private bool? ImportEmitDefaultValue(XmlSchemaAnnotation annotation, XmlQualifiedName typeName)
        {
            XmlElement defaultValueElement = ImportAnnotation(annotation, SchemaExporter.DefaultValueAnnotation);
            if (defaultValueElement == null)
                return null;
            XmlNode emitDefaultValueAttribute = defaultValueElement.Attributes.GetNamedItem(Globals.EmitDefaultValueAttribute);
            string emitDefaultValueString = (emitDefaultValueAttribute == null) ? null : emitDefaultValueAttribute.Value;
            if (emitDefaultValueString == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.AnnotationAttributeNotFound, SchemaExporter.DefaultValueAnnotation.Name, typeName.Name, typeName.Namespace, Globals.EmitDefaultValueAttribute)));
            return XmlConvert.ToBoolean(emitDefaultValueString);
        }

        internal static XmlQualifiedName ImportActualType(XmlSchemaAnnotation annotation, XmlQualifiedName defaultTypeName, XmlQualifiedName typeName)
        {
            XmlElement actualTypeElement = ImportAnnotation(annotation, SchemaExporter.ActualTypeAnnotationName);
            if (actualTypeElement == null)
                return defaultTypeName;

            XmlNode nameAttribute = actualTypeElement.Attributes.GetNamedItem(Globals.ActualTypeNameAttribute);
            string name = (nameAttribute == null) ? null : nameAttribute.Value;
            if (name == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.AnnotationAttributeNotFound, SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, Globals.ActualTypeNameAttribute)));
            XmlNode nsAttribute = actualTypeElement.Attributes.GetNamedItem(Globals.ActualTypeNamespaceAttribute);
            string ns = (nsAttribute == null) ? null : nsAttribute.Value;
            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.AnnotationAttributeNotFound, SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, Globals.ActualTypeNamespaceAttribute)));
            return new XmlQualifiedName(name, ns);
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on CollectionDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        CollectionDataContract ImportCollection(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation, bool isReference)
        {
            CollectionDataContract dataContract = new CollectionDataContract(CollectionKind.Array);
            dataContract.StableName = typeName;
            AddDataContract(dataContract);

            dataContract.IsReference = isReference;

            // CheckIfCollection has already checked if sequence contains exactly one item with maxOccurs="unbounded" or maxOccurs > 1 
            XmlSchemaElement element = (XmlSchemaElement)rootSequence.Items[0];

            dataContract.IsItemTypeNullable = element.IsNillable;
            dataContract.ItemName = element.Name;

            XmlSchemaForm elementForm = (element.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(SchemaObjects, schemaSet, typeName).ElementFormDefault : element.Form;
            if (elementForm != XmlSchemaForm.Qualified)
                ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ArrayItemFormMustBe, element.Name));
            CheckIfElementUsesUnsupportedConstructs(typeName, element);

            if (element.SchemaTypeName.IsEmpty)
            {
                if (element.SchemaType != null)
                {
                    XmlQualifiedName shortName = new XmlQualifiedName(element.Name, typeName.Namespace);
                    DataContract contract = dataContractSet[shortName];
                    if (contract == null)
                    {
                        dataContract.ItemContract = ImportAnonymousElement(element, shortName);
                    }
                    else
                    {
                        XmlQualifiedName fullName = new XmlQualifiedName(String.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", typeName.Name, element.Name), typeName.Namespace);
                        dataContract.ItemContract = ImportAnonymousElement(element, fullName);
                    }
                }
                else if (!element.RefName.IsEmpty)
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.ElementRefOnLocalElementNotSupported, element.RefName.Name, element.RefName.Namespace));
                else
                    dataContract.ItemContract = ImportType(SchemaExporter.AnytypeQualifiedName);
            }
            else
            {
                dataContract.ItemContract = ImportType(element.SchemaTypeName);
            }

            if (IsDictionary(typeName, annotation))
            {
                ClassDataContract keyValueContract = dataContract.ItemContract as ClassDataContract;
                DataMember key = null, value = null;
                if (keyValueContract == null || keyValueContract.Members == null || keyValueContract.Members.Count != 2
                    || !(key = keyValueContract.Members[0]).IsRequired || !(value = keyValueContract.Members[1]).IsRequired)
                {
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.InvalidKeyValueType, element.Name));
                }
                if (keyValueContract.Namespace != dataContract.Namespace)
                {
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.InvalidKeyValueTypeNamespace, element.Name, keyValueContract.Namespace));
                }
                keyValueContract.IsValueType = true;
                dataContract.KeyName = key.Name;
                dataContract.ValueName = value.Name;
                if (element.SchemaType != null)
                {
                    dataContractSet.Remove(keyValueContract.StableName);

                    GenericInfo genericInfo = new GenericInfo(DataContract.GetStableName(Globals.TypeOfKeyValue), Globals.TypeOfKeyValue.FullName);
                    genericInfo.Add(GetGenericInfoForDataMember(key));
                    genericInfo.Add(GetGenericInfoForDataMember(value));
                    genericInfo.AddToLevel(0, 2);
                    dataContract.ItemContract.StableName = new XmlQualifiedName(genericInfo.GetExpandedStableName().Name, typeName.Namespace);
                }
            }

            return dataContract;
        }

        GenericInfo GetGenericInfoForDataMember(DataMember dataMember)
        {
            GenericInfo genericInfo = null;
            if (dataMember.MemberTypeContract.IsValueType && dataMember.IsNullable)
            {
                genericInfo = new GenericInfo(DataContract.GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable.FullName);
                genericInfo.Add(new GenericInfo(dataMember.MemberTypeContract.StableName, null));
            }
            else
            {
                genericInfo = new GenericInfo(dataMember.MemberTypeContract.StableName, null);
            }

            return genericInfo;
        }

        bool IsDictionary(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
        {
            string isDictionaryInnerText = GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsDictionaryAnnotationName));
            if (isDictionaryInnerText != null)
            {
                try
                {
                    return XmlConvert.ToBoolean(isDictionaryInnerText);
                }
                catch (FormatException fe)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.IsDictionaryFormattedIncorrectly, isDictionaryInnerText, fe.Message));
                }
            }
            return false;
        }

        EnumDataContract ImportFlagsEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeList list, XmlSchemaAnnotation annotation)
        {
            XmlSchemaSimpleType anonymousType = list.ItemType;
            if (anonymousType == null)
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumListMustContainAnonymousType));

            XmlSchemaSimpleTypeContent content = anonymousType.Content;
            if (content is XmlSchemaSimpleTypeUnion)
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumUnionInAnonymousTypeNotSupported));
            else if (content is XmlSchemaSimpleTypeList)
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumListInAnonymousTypeNotSupported));
            else if (content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)content;
                if (CheckIfEnum(restriction))
                    return ImportEnum(typeName, restriction, true /*isFlags*/, annotation);
                else
                    ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumRestrictionInvalid));
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on EnumDataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        EnumDataContract ImportEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction, bool isFlags, XmlSchemaAnnotation annotation)
        {
            EnumDataContract dataContract = new EnumDataContract();
            dataContract.StableName = typeName;
            dataContract.BaseContractName = ImportActualType(annotation, SchemaExporter.DefaultEnumBaseTypeName, typeName);
            dataContract.IsFlags = isFlags;
            AddDataContract(dataContract);

            // CheckIfEnum has already checked if baseType of restriction is string 
            dataContract.Values = new List<long>();
            dataContract.Members = new List<DataMember>();
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                XmlSchemaEnumerationFacet enumFacet = facet as XmlSchemaEnumerationFacet;
                if (enumFacet == null)
                    ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumOnlyEnumerationFacetsSupported));
                if (enumFacet.Value == null)
                    ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.EnumEnumerationFacetsMustHaveValue));

                string valueInnerText = GetInnerText(typeName, ImportAnnotation(enumFacet.Annotation, SchemaExporter.EnumerationValueAnnotationName));
                if (valueInnerText == null)
                    dataContract.Values.Add(SchemaExporter.GetDefaultEnumValue(isFlags, dataContract.Members.Count));
                else
                    dataContract.Values.Add(dataContract.GetEnumValueFromString(valueInnerText));
                DataMember dataMember = new DataMember(enumFacet.Value);
                dataContract.Members.Add(dataMember);
            }
            return dataContract;
        }

        DataContract ImportSimpleTypeRestriction(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction)
        {
            DataContract dataContract = null;

            if (!restriction.BaseTypeName.IsEmpty)
                dataContract = ImportType(restriction.BaseTypeName);
            else if (restriction.BaseType != null)
                dataContract = ImportType(restriction.BaseType);
            else
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.SimpleTypeRestrictionDoesNotSpecifyBase));

            return dataContract;
        }

        void ImportDataContractExtension(XmlSchemaType type, DataContract dataContract)
        {
            if (type.Annotation == null || type.Annotation.Items == null)
                return;
            foreach (XmlSchemaObject schemaObject in type.Annotation.Items)
            {
                XmlSchemaAppInfo appInfo = schemaObject as XmlSchemaAppInfo;
                if (appInfo == null)
                    continue;
                if (appInfo.Markup != null)
                {
                    foreach (XmlNode xmlNode in appInfo.Markup)
                    {
                        XmlElement typeElement = xmlNode as XmlElement;
                        XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
                        if (typeElement != null && typeElement.NamespaceURI == surrogateDataAnnotationName.Namespace && typeElement.LocalName == surrogateDataAnnotationName.Name)
                        {
                            object surrogateData = ImportSurrogateData(typeElement, surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace);
                            dataContractSet.SetSurrogateData(dataContract, surrogateData);
                        }
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on DataContract.",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        void ImportGenericInfo(XmlSchemaType type, DataContract dataContract)
        {
            if (type.Annotation == null || type.Annotation.Items == null)
                return;
            foreach (XmlSchemaObject schemaObject in type.Annotation.Items)
            {
                XmlSchemaAppInfo appInfo = schemaObject as XmlSchemaAppInfo;
                if (appInfo == null)
                    continue;
                if (appInfo.Markup != null)
                {
                    foreach (XmlNode xmlNode in appInfo.Markup)
                    {
                        XmlElement typeElement = xmlNode as XmlElement;
                        if (typeElement != null && typeElement.NamespaceURI == Globals.SerializationNamespace)
                        {
                            if (typeElement.LocalName == Globals.GenericTypeLocalName)
                                dataContract.GenericInfo = ImportGenericInfo(typeElement, type);
                        }
                    }
                }
            }
        }

        GenericInfo ImportGenericInfo(XmlElement typeElement, XmlSchemaType type)
        {
            XmlNode nameAttribute = typeElement.Attributes.GetNamedItem(Globals.GenericNameAttribute);
            string name = (nameAttribute == null) ? null : nameAttribute.Value;
            if (name == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationAttributeNotFound, type.Name, Globals.GenericNameAttribute)));
            XmlNode nsAttribute = typeElement.Attributes.GetNamedItem(Globals.GenericNamespaceAttribute);
            string ns = (nsAttribute == null) ? null : nsAttribute.Value;
            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationAttributeNotFound, type.Name, Globals.GenericNamespaceAttribute)));
            if (typeElement.ChildNodes.Count > 0) //Generic Type
                name = DataContract.EncodeLocalName(name);

            int currentLevel = 0;
            GenericInfo genInfo = new GenericInfo(new XmlQualifiedName(name, ns), type.Name);
            foreach (XmlNode childNode in typeElement.ChildNodes)
            {
                XmlElement argumentElement = childNode as XmlElement;
                if (argumentElement == null)
                    continue;
                if (argumentElement.LocalName != Globals.GenericParameterLocalName ||
                    argumentElement.NamespaceURI != Globals.SerializationNamespace)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationHasInvalidElement, argumentElement.LocalName, argumentElement.NamespaceURI, type.Name)));
                XmlNode nestedLevelAttribute = argumentElement.Attributes.GetNamedItem(Globals.GenericParameterNestedLevelAttribute);
                int argumentLevel = 0;
                if (nestedLevelAttribute != null)
                {
                    if (!Int32.TryParse(nestedLevelAttribute.Value, out argumentLevel))
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationHasInvalidAttributeValue, argumentElement.LocalName, argumentElement.NamespaceURI, type.Name, nestedLevelAttribute.Value, nestedLevelAttribute.LocalName, Globals.TypeOfInt.Name)));
                }
                if (argumentLevel < currentLevel)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationForNestedLevelMustBeIncreasing, argumentElement.LocalName, argumentElement.NamespaceURI, type.Name)));
                genInfo.Add(ImportGenericInfo(argumentElement, type));
                genInfo.AddToLevel(argumentLevel, 1);
                currentLevel = argumentLevel;
            }

            XmlNode typeNestedLevelsAttribute = typeElement.Attributes.GetNamedItem(Globals.GenericParameterNestedLevelAttribute);
            if (typeNestedLevelsAttribute != null)
            {
                int nestedLevels = 0;
                if (!Int32.TryParse(typeNestedLevelsAttribute.Value, out nestedLevels))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericAnnotationHasInvalidAttributeValue, typeElement.LocalName, typeElement.NamespaceURI, type.Name, typeNestedLevelsAttribute.Value, typeNestedLevelsAttribute.LocalName, Globals.TypeOfInt.Name)));
                if ((nestedLevels - 1) > currentLevel)
                    genInfo.AddToLevel(nestedLevels - 1, 0);
            }
            return genInfo;
        }

        object ImportSurrogateData(XmlElement typeElement, string name, string ns)
        {
            if (dataContractSet.DataContractSurrogate != null && typeElement != null)
            {
                Collection<Type> knownTypes = new Collection<Type>();
                DataContractSurrogateCaller.GetKnownCustomDataTypes(dataContractSet.DataContractSurrogate, knownTypes);
                DataContractSerializer serializer = new DataContractSerializer(Globals.TypeOfObject, name, ns, knownTypes,
                    Int32.MaxValue, false /*ignoreExtensionDataObject*/, true /*preserveObjectReferences*/, null /*dataContractSurrogate*/);
                return serializer.ReadObject(new XmlNodeReader(typeElement));
            }
            return null;
        }

        void CheckComplexType(XmlQualifiedName typeName, XmlSchemaComplexType type)
        {
            if (type.IsAbstract)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.AbstractTypeNotSupported));
            if (type.IsMixed)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.MixedContentNotSupported));
        }

        void CheckIfElementUsesUnsupportedConstructs(XmlQualifiedName typeName, XmlSchemaElement element)
        {
            if (element.IsAbstract)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.AbstractElementNotSupported, element.Name));
            if (element.DefaultValue != null)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.DefaultOnElementNotSupported, element.Name));
            if (element.FixedValue != null)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.FixedOnElementNotSupported, element.Name));
            if (!element.SubstitutionGroup.IsEmpty)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.SubstitutionGroupOnElementNotSupported, element.Name));
        }

        void ImportAttributes(XmlQualifiedName typeName, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, out bool isReference)
        {
            if (anyAttribute != null)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.AnyAttributeNotSupported));

            isReference = false;
            if (attributes != null)
            {
                bool foundId = false, foundRef = false;
                for (int i = 0; i < attributes.Count; i++)
                {
                    XmlSchemaObject o = attributes[i];
                    if (o is XmlSchemaAttribute)
                    {
                        XmlSchemaAttribute attribute = (XmlSchemaAttribute)o;
                        if (attribute.Use == XmlSchemaUse.Prohibited)
                            continue;
                        if (TryCheckIfAttribute(typeName, attribute, Globals.IdQualifiedName, ref foundId))
                            continue;
                        if (TryCheckIfAttribute(typeName, attribute, Globals.RefQualifiedName, ref foundRef))
                            continue;
                        if (attribute.RefName.IsEmpty || attribute.RefName.Namespace != Globals.SerializationNamespace || attribute.Use == XmlSchemaUse.Required)
                            ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.TypeShouldNotContainAttributes, Globals.SerializationNamespace));
                    }
                }
                isReference = (foundId && foundRef);
            }
        }

        bool TryCheckIfAttribute(XmlQualifiedName typeName, XmlSchemaAttribute attribute, XmlQualifiedName refName, ref bool foundAttribute)
        {
            if (attribute.RefName != refName)
                return false;
            if (foundAttribute)
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.CannotHaveDuplicateAttributeNames, refName.Name));
            foundAttribute = true;
            return true;
        }

        void AddDataContract(DataContract dataContract)
        {
            dataContractSet.Add(dataContract.StableName, dataContract);
        }

        string GetInnerText(XmlQualifiedName typeName, XmlElement xmlElement)
        {
            if (xmlElement != null)
            {
                XmlNode child = xmlElement.FirstChild;
                while (child != null)
                {
                    if (child.NodeType == XmlNodeType.Element)
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString(SR.InvalidAnnotationExpectingText, xmlElement.LocalName, xmlElement.NamespaceURI, child.LocalName, child.NamespaceURI));
                    child = child.NextSibling;
                }
                return xmlElement.InnerText;
            }
            return null;
        }

        static XmlElement ImportAnnotation(XmlSchemaAnnotation annotation, XmlQualifiedName annotationQualifiedName)
        {
            if (annotation != null && annotation.Items != null && annotation.Items.Count > 0 && annotation.Items[0] is XmlSchemaAppInfo)
            {
                XmlSchemaAppInfo appInfo = (XmlSchemaAppInfo)annotation.Items[0];
                XmlNode[] markup = appInfo.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlElement annotationElement = markup[i] as XmlElement;
                        if (annotationElement != null && annotationElement.LocalName == annotationQualifiedName.Name && annotationElement.NamespaceURI == annotationQualifiedName.Namespace)
                            return annotationElement;
                    }
                }
            }
            return null;
        }

        static void ThrowTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(SR.GetString(SR.TypeCannotBeImported, name, ns, message));
        }

        static void ThrowArrayTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(SR.GetString(SR.ArrayTypeCannotBeImported, name, ns, message));
        }

        static void ThrowEnumTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(SR.GetString(SR.EnumTypeCannotBeImported, name, ns, message));
        }

        static void ThrowISerializableTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(SR.GetString(SR.ISerializableTypeCannotBeImported, name, ns, message));
        }

        static void ThrowTypeCannotBeImportedException(string message)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TypeCannotBeImportedHowToFix, message)));
        }
    }

}


