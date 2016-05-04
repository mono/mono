//------------------------------------------------------------------------------
// <copyright file="XmlReflectionImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.Reflection;
    using System;
    using System.Xml.Schema;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.CodeDom.Compiler;
    using System.Threading;
    using System.Diagnostics;

    /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter"]/*' />
    ///<internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlReflectionImporter {
        TypeScope typeScope;
        XmlAttributeOverrides attributeOverrides;
        XmlAttributes defaultAttributes = new XmlAttributes();
        NameTable types = new NameTable();      // xmltypename + xmlns -> Mapping
        NameTable nullables = new NameTable();  // xmltypename + xmlns -> NullableMapping
        NameTable elements = new NameTable();   // xmlelementname + xmlns -> ElementAccessor
        NameTable xsdAttributes;   // xmlattributetname + xmlns -> AttributeAccessor
        Hashtable specials;   // type -> SpecialMapping
        Hashtable anonymous = new Hashtable();   // type -> AnonymousMapping
        NameTable serializables;  // type name --> new SerializableMapping
        StructMapping root;
        string defaultNs;
        ModelScope modelScope;
        int arrayNestingLevel;
        XmlArrayItemAttributes savedArrayItemAttributes;
        string savedArrayNamespace;
        int choiceNum = 1;

        enum ImportContext {
            Text,
            Attribute,
            Element
        }
        
        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.XmlReflectionImporter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlReflectionImporter() : this(null, null) {
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.XmlReflectionImporter1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlReflectionImporter(string defaultNamespace) : this(null, defaultNamespace) {
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.XmlReflectionImporter2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides) : this(attributeOverrides, null) {
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.XmlReflectionImporter3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides, string defaultNamespace) {
            if (defaultNamespace == null)
                defaultNamespace = String.Empty;
            if (attributeOverrides == null)
                attributeOverrides = new XmlAttributeOverrides();
            this.attributeOverrides = attributeOverrides;
            this.defaultNs = defaultNamespace;
            this.typeScope = new TypeScope();
            this.modelScope = new ModelScope(this.typeScope);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.IncludeTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void IncludeTypes(ICustomAttributeProvider provider) {
            IncludeTypes(provider, new RecursionLimiter());
        }

        void IncludeTypes(ICustomAttributeProvider provider, RecursionLimiter limiter) {
            object[] attrs = provider.GetCustomAttributes(typeof(XmlIncludeAttribute), false);
            for (int i = 0; i < attrs.Length; i++) {
                Type type = ((XmlIncludeAttribute)attrs[i]).Type;
                IncludeType(type, limiter);
            }
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.IncludeType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void IncludeType(Type type) {
            IncludeType(type, new RecursionLimiter());
        }

        void IncludeType(Type type, RecursionLimiter limiter) {
            int previousNestingLevel = arrayNestingLevel;
            XmlArrayItemAttributes previousArrayItemAttributes = savedArrayItemAttributes;
            string previousArrayNamespace = savedArrayNamespace;
            arrayNestingLevel = 0;
            savedArrayItemAttributes = null;
            savedArrayNamespace = null;
           
            TypeMapping mapping = ImportTypeMapping(modelScope.GetTypeModel(type), defaultNs, ImportContext.Element, string.Empty, null, limiter);
            if (mapping.IsAnonymousType && !mapping.TypeDesc.IsSpecial) {
                //XmlAnonymousInclude=Cannot include anonymous type '{0}'.
                throw new InvalidOperationException(Res.GetString(Res.XmlAnonymousInclude, type.FullName));
            }
            arrayNestingLevel = previousNestingLevel;
            savedArrayItemAttributes = previousArrayItemAttributes;
            savedArrayNamespace = previousArrayNamespace;
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportTypeMapping"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeMapping ImportTypeMapping(Type type) {
            return ImportTypeMapping(type, null, null);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportTypeMapping1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeMapping ImportTypeMapping(Type type, string defaultNamespace) {
            return ImportTypeMapping(type, null, defaultNamespace);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportTypeMapping2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root) {
            return ImportTypeMapping(type, root, null);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportTypeMapping3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root, string defaultNamespace) {
            if (type == null)
                throw new ArgumentNullException("type");
            XmlTypeMapping xmlMapping = new XmlTypeMapping(typeScope, ImportElement(modelScope.GetTypeModel(type), root, defaultNamespace, new RecursionLimiter()));
            xmlMapping.SetKeyInternal(XmlMapping.GenerateKey(type, root, defaultNamespace));
            xmlMapping.GenerateSerializer = true;
            return xmlMapping;
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportMembersMapping"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement) {
            return ImportMembersMapping(elementName, ns, members, hasWrapperElement, false);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportMembersMapping1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc) {
            return ImportMembersMapping(elementName, ns, members, hasWrapperElement, rpc, false);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportMembersMapping2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// 
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel) {
            return ImportMembersMapping(elementName, ns, members, hasWrapperElement, rpc, openModel, XmlMappingAccess.Read | XmlMappingAccess.Write);
        }

        /// <include file='doc\XmlReflectionImporter.uex' path='docs/doc[@for="XmlReflectionImporter.ImportMembersMapping3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// 
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel, XmlMappingAccess access) {
            ElementAccessor element = new ElementAccessor();
            element.Name = elementName == null || elementName.Length == 0 ? elementName : XmlConvert.EncodeLocalName(elementName);
            element.Namespace = ns;

            MembersMapping membersMapping = ImportMembersMapping(members, ns, hasWrapperElement, rpc, openModel, new RecursionLimiter());
            element.Mapping = membersMapping;
            element.Form = XmlSchemaForm.Qualified;   // elements within soap:body are always qualified
            if (!rpc) {
                if (hasWrapperElement)
                    element = (ElementAccessor)ReconcileAccessor(element, this.elements);
                else {
                    foreach (MemberMapping mapping in membersMapping.Members) {
                        if (mapping.Elements != null && mapping.Elements.Length > 0) {
                            mapping.Elements[0] = (ElementAccessor)ReconcileAccessor(mapping.Elements[0], this.elements);
                        }
                    }
                }
            }
            XmlMembersMapping xmlMapping = new XmlMembersMapping(typeScope, element, access);
            xmlMapping.GenerateSerializer = true;
            return xmlMapping;
        }

        XmlAttributes GetAttributes(Type type, bool canBeSimpleType) {
            XmlAttributes attrs = attributeOverrides[type];
            if (attrs != null) return attrs;
            if (canBeSimpleType && TypeScope.IsKnownType(type)) {
                return defaultAttributes;
            }
            return new XmlAttributes(type);
        }

        XmlAttributes GetAttributes(MemberInfo memberInfo) {
            XmlAttributes attrs = attributeOverrides[memberInfo.DeclaringType, memberInfo.Name];
            if (attrs != null) return attrs;
            return new XmlAttributes(memberInfo);
        }

        ElementAccessor ImportElement(TypeModel model, XmlRootAttribute root, string defaultNamespace, RecursionLimiter limiter) {
            XmlAttributes a = GetAttributes(model.Type, true);

            if (root == null)
                root = a.XmlRoot;
            string ns = root == null ? null : root.Namespace;
            if (ns == null) ns = defaultNamespace;
            if (ns == null) ns = this.defaultNs;

            arrayNestingLevel = -1;
            savedArrayItemAttributes = null;
            savedArrayNamespace = null;
            ElementAccessor element = CreateElementAccessor(ImportTypeMapping(model, ns, ImportContext.Element, string.Empty, a, limiter), ns);

            if (root != null) {
                if (root.ElementName.Length > 0)
                    element.Name = XmlConvert.EncodeLocalName(root.ElementName);
                if (root.IsNullableSpecified && !root.IsNullable && model.TypeDesc.IsOptionalValue)
                    //XmlInvalidNotNullable=IsNullable may not be set to 'false' for a Nullable<{0}> type. Consider using '{0}' type or removing the IsNullable property from the XmlElement attribute.
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidNotNullable, model.TypeDesc.BaseTypeDesc.FullName, "XmlRoot"));
                element.IsNullable = root.IsNullableSpecified ? root.IsNullable : model.TypeDesc.IsNullable || model.TypeDesc.IsOptionalValue;
                CheckNullable(element.IsNullable, model.TypeDesc, element.Mapping);
            }
            else
                element.IsNullable = model.TypeDesc.IsNullable || model.TypeDesc.IsOptionalValue;
            element.Form = XmlSchemaForm.Qualified;
            return (ElementAccessor)ReconcileAccessor(element, this.elements);
        }

        static string GetMappingName(Mapping mapping) {
            if (mapping is MembersMapping)
                return "(method)";
            else if (mapping is TypeMapping)
                return ((TypeMapping)mapping).TypeDesc.FullName;
            else
                throw new ArgumentException(Res.GetString(Res.XmlInternalError), "mapping");
        }
        
        ElementAccessor ReconcileLocalAccessor(ElementAccessor accessor, string ns) {
            if (accessor.Namespace == ns) return accessor;
            return (ElementAccessor)ReconcileAccessor(accessor, this.elements);
        }

        Accessor ReconcileAccessor(Accessor accessor, NameTable accessors) {
            if (accessor.Any && accessor.Name.Length == 0)
                 return accessor;

            Accessor existing = (Accessor)accessors[accessor.Name, accessor.Namespace];
            if (existing == null) {
                accessor.IsTopLevelInSchema = true;
                accessors.Add(accessor.Name, accessor.Namespace, accessor);
                return accessor;
            }

            if (existing.Mapping == accessor.Mapping) 
                return existing;

            if (!(accessor.Mapping is MembersMapping) && !(existing.Mapping is MembersMapping)) {
                if (accessor.Mapping.TypeDesc == existing.Mapping.TypeDesc
                    || (existing.Mapping is NullableMapping && accessor.Mapping.TypeDesc == ((NullableMapping)existing.Mapping).BaseMapping.TypeDesc)
                    || (accessor.Mapping is NullableMapping && ((NullableMapping)accessor.Mapping).BaseMapping.TypeDesc == existing.Mapping.TypeDesc))
                {
                    // need to compare default values
                    string value1 = Convert.ToString(accessor.Default, CultureInfo.InvariantCulture);
                    string value2 = Convert.ToString(existing.Default, CultureInfo.InvariantCulture);
                    if (value1 == value2) {
                        return existing;
                    }
                    throw new InvalidOperationException(Res.GetString(Res.XmlCannotReconcileAccessorDefault, accessor.Name, accessor.Namespace, value1, value2));
                }
            }

            if (accessor.Mapping is MembersMapping || existing.Mapping is MembersMapping)
                throw new InvalidOperationException(Res.GetString(Res.XmlMethodTypeNameConflict, accessor.Name, accessor.Namespace));

            if (accessor.Mapping is ArrayMapping) {
                if (!(existing.Mapping is ArrayMapping)) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlCannotReconcileAccessor, accessor.Name, accessor.Namespace, GetMappingName(existing.Mapping), GetMappingName(accessor.Mapping)));
                }
                ArrayMapping mapping = (ArrayMapping)accessor.Mapping;
                ArrayMapping existingMapping = mapping.IsAnonymousType ? null : (ArrayMapping)types[existing.Mapping.TypeName, existing.Mapping.Namespace];
                ArrayMapping first = existingMapping;
                while (existingMapping != null) {
                    if (existingMapping == accessor.Mapping)
                        return existing;
                    existingMapping = existingMapping.Next;
                }
                mapping.Next = first;
                if (!mapping.IsAnonymousType)
                    types[existing.Mapping.TypeName, existing.Mapping.Namespace] = mapping;
                return existing;
            }
            if (accessor is AttributeAccessor)
                throw new InvalidOperationException(Res.GetString(Res.XmlCannotReconcileAttributeAccessor, accessor.Name, accessor.Namespace, GetMappingName(existing.Mapping), GetMappingName(accessor.Mapping)));
            else
                throw new InvalidOperationException(Res.GetString(Res.XmlCannotReconcileAccessor, accessor.Name, accessor.Namespace, GetMappingName(existing.Mapping), GetMappingName(accessor.Mapping)));
        }

        Exception CreateReflectionException(string context, Exception e) {
            return new InvalidOperationException(Res.GetString(Res.XmlReflectionError, context), e);
        }

        Exception CreateTypeReflectionException(string context, Exception e) {
            return new InvalidOperationException(Res.GetString(Res.XmlTypeReflectionError, context), e);
        }

        Exception CreateMemberReflectionException(FieldModel model, Exception e) {
            return new InvalidOperationException(Res.GetString(model.IsProperty ? Res.XmlPropertyReflectionError : Res.XmlFieldReflectionError, model.Name), e);
        }

        TypeMapping ImportTypeMapping(TypeModel model, string ns, ImportContext context, string dataType, XmlAttributes a, RecursionLimiter limiter) {
            return ImportTypeMapping(model, ns, context, dataType, a, false, false, limiter);
        }

        TypeMapping ImportTypeMapping(TypeModel model, string ns, ImportContext context, string dataType, XmlAttributes a, bool repeats, bool openModel, RecursionLimiter limiter) {
            try {
                if (dataType.Length > 0) {
                    TypeDesc modelTypeDesc = TypeScope.IsOptionalValue(model.Type) ? model.TypeDesc.BaseTypeDesc : model.TypeDesc;
                    if (!modelTypeDesc.IsPrimitive) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidDataTypeUsage, dataType, "XmlElementAttribute.DataType"));
                    }
                    TypeDesc td = typeScope.GetTypeDesc(dataType, XmlSchema.Namespace);
                    if (td == null) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidXsdDataType, dataType, "XmlElementAttribute.DataType", new XmlQualifiedName(dataType, XmlSchema.Namespace).ToString()));
                    }
                    if (modelTypeDesc.FullName != td.FullName) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlDataTypeMismatch, dataType, "XmlElementAttribute.DataType", modelTypeDesc.FullName));
                    }
                }

                if (a == null)
                    a = GetAttributes(model.Type, false);
                
                if ((a.XmlFlags & ~(XmlAttributeFlags.Type | XmlAttributeFlags.Root)) != 0)
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidTypeAttributes, model.Type.FullName));

                switch (model.TypeDesc.Kind) {
                case TypeKind.Enum: 
                    return ImportEnumMapping((EnumModel)model, ns, repeats);
                case TypeKind.Primitive:
                    if (a.XmlFlags != 0) throw InvalidAttributeUseException(model.Type);
                    return ImportPrimitiveMapping((PrimitiveModel)model, context, dataType, repeats);
                case TypeKind.Array:
                case TypeKind.Collection:
                case TypeKind.Enumerable:
                    //if (a.XmlFlags != 0) throw InvalidAttributeUseException(model.Type);
                    if (context != ImportContext.Element) throw UnsupportedException(model.TypeDesc, context);
                    arrayNestingLevel++;
                    ArrayMapping arrayMapping = ImportArrayLikeMapping((ArrayModel)model, ns, limiter);
                    arrayNestingLevel--;
                    return arrayMapping;
                case TypeKind.Root:
                case TypeKind.Class:
                case TypeKind.Struct:
                    if (context != ImportContext.Element) throw UnsupportedException(model.TypeDesc, context);
                    if (model.TypeDesc.IsOptionalValue) {
                        TypeDesc valueTypeDesc = string.IsNullOrEmpty(dataType) ? model.TypeDesc.BaseTypeDesc : typeScope.GetTypeDesc(dataType, XmlSchema.Namespace);
                        string xsdTypeName = valueTypeDesc.DataType == null ? valueTypeDesc.Name : valueTypeDesc.DataType.Name;
                        TypeMapping baseMapping = GetTypeMapping(xsdTypeName, ns, valueTypeDesc, types, null);
                        if (baseMapping == null)
                            baseMapping = ImportTypeMapping(modelScope.GetTypeModel(model.TypeDesc.BaseTypeDesc.Type), ns, context, dataType, null, repeats, openModel, limiter);
                        return CreateNullableMapping(baseMapping, model.TypeDesc.Type);
                    }
                    else {
                        return ImportStructLikeMapping((StructModel)model, ns, openModel, a, limiter);
                    }
                default:
                    if (model.TypeDesc.Kind == TypeKind.Serializable) {
                        // We allow XmlRoot attribute on IXmlSerializable, but not others
                        if ((a.XmlFlags & ~XmlAttributeFlags.Root) != 0) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlSerializableAttributes, model.TypeDesc.FullName, typeof(XmlSchemaProviderAttribute).Name));
                        }
                    }
                    else {
                        if (a.XmlFlags != 0) throw InvalidAttributeUseException(model.Type);
                    }
                    if (model.TypeDesc.IsSpecial)
                        return ImportSpecialMapping(model.Type, model.TypeDesc, ns, context, limiter);
                    throw UnsupportedException(model.TypeDesc, context);
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                throw CreateTypeReflectionException(model.TypeDesc.FullName, e);
            }
        }

        internal static MethodInfo GetMethodFromSchemaProvider(XmlSchemaProviderAttribute provider, Type type) {
            if (provider.IsAny) {
                // do not validate the schema provider method for wildcard types.
                return null;
            }
            else if (provider.MethodName == null) {
                throw new ArgumentNullException("MethodName");
            }
            if (!CodeGenerator.IsValidLanguageIndependentIdentifier(provider.MethodName))
                throw new ArgumentException(Res.GetString(Res.XmlGetSchemaMethodName, provider.MethodName), "MethodName");

            MethodInfo getMethod = getMethod = type.GetMethod(provider.MethodName, /* BindingFlags.DeclaredOnly | */ BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof(XmlSchemaSet)}, null);
            if (getMethod == null)
                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaMethodMissing, provider.MethodName, typeof(XmlSchemaSet).Name, type.FullName));

            if (!(typeof(XmlQualifiedName).IsAssignableFrom(getMethod.ReturnType)) && !(typeof(XmlSchemaType).IsAssignableFrom(getMethod.ReturnType)))
                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaMethodReturnType, type.Name, provider.MethodName, typeof(XmlSchemaProviderAttribute).Name, typeof(XmlQualifiedName).FullName, typeof(XmlSchemaType).FullName));

            return getMethod;
        }
       
        SpecialMapping ImportSpecialMapping(Type type, TypeDesc typeDesc, string ns, ImportContext context, RecursionLimiter limiter) {
            if (specials == null)
                specials = new Hashtable();
            SpecialMapping mapping = (SpecialMapping)specials[type];
            if (mapping != null) {
                CheckContext(mapping.TypeDesc, context);
                return mapping;
            }
            if (typeDesc.Kind == TypeKind.Serializable) {
                // 
                SerializableMapping serializableMapping = null;

                // get the schema method info
                object[] attrs = type.GetCustomAttributes(typeof(XmlSchemaProviderAttribute), false);

                if (attrs.Length > 0) {
                    // new IXmlSerializable
                    XmlSchemaProviderAttribute provider = (XmlSchemaProviderAttribute)attrs[0];
                    MethodInfo method = GetMethodFromSchemaProvider(provider, type);
                    serializableMapping = new SerializableMapping(method, provider.IsAny, ns);
                    XmlQualifiedName qname = serializableMapping.XsiType;
                    if (qname != null && !qname.IsEmpty) {
                        if (serializables == null)
                            serializables = new NameTable();
                        SerializableMapping existingMapping = (SerializableMapping)serializables[qname];
                        if (existingMapping != null) {
                            if (existingMapping.Type == null) {
                                serializableMapping = existingMapping;
                            }
                            else if (existingMapping.Type != type) {
                                SerializableMapping next = existingMapping.Next;
                                existingMapping.Next = serializableMapping;
                                serializableMapping.Next = next;
                            }
                        }
                        else {
                            XmlSchemaType xsdType = serializableMapping.XsdType;
                            if (xsdType != null)
                                SetBase(serializableMapping, xsdType.DerivedFrom);
                            serializables[qname] = serializableMapping;
                        }
                        serializableMapping.TypeName = qname.Name;
                        serializableMapping.Namespace = qname.Namespace;
                    }
                    serializableMapping.TypeDesc = typeDesc;
                    serializableMapping.Type = type;
                    IncludeTypes(type);
                }
                else {
                    // old IXmlSerializable
                    serializableMapping = new SerializableMapping();
                    serializableMapping.TypeDesc = typeDesc;
                    serializableMapping.Type = type;
                }
                mapping = serializableMapping;
            }
            else {
                mapping = new SpecialMapping();
                mapping.TypeDesc = typeDesc;
            }
            CheckContext(typeDesc, context);
            specials.Add(type, mapping);
            typeScope.AddTypeMapping(mapping);
            return mapping;
        }

        internal static void ValidationCallbackWithErrorCode (object sender, ValidationEventArgs args) {
            // 
            if (args.Severity == XmlSeverityType.Error)
                throw new InvalidOperationException(Res.GetString(Res.XmlSerializableSchemaError, typeof(IXmlSerializable).Name, args.Message));
        }

        internal void SetBase(SerializableMapping mapping, XmlQualifiedName baseQname) {

            if (baseQname.IsEmpty) return;
            if (baseQname.Namespace == XmlSchema.Namespace) return;
            XmlSchemaSet schemas = mapping.Schemas;
            ArrayList srcSchemas = (ArrayList)schemas.Schemas(baseQname.Namespace);

            if (srcSchemas.Count == 0) {
                throw new InvalidOperationException(Res.GetString(Res.XmlMissingSchema, baseQname.Namespace));
            }
            if (srcSchemas.Count > 1) {
                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaInclude, baseQname.Namespace, typeof(IXmlSerializable).Name, "GetSchema"));
            }
            XmlSchema s = (XmlSchema)srcSchemas[0];

            XmlSchemaType t = (XmlSchemaType)s.SchemaTypes[baseQname];
            t = t.Redefined != null ? t.Redefined : t;

            if (serializables[baseQname] == null) {
                SerializableMapping baseMapping = new SerializableMapping(baseQname, schemas);
                SetBase(baseMapping, t.DerivedFrom);
                serializables.Add(baseQname, baseMapping);
            }
            mapping.SetBaseMapping((SerializableMapping)serializables[baseQname]);
        }

        static string GetContextName(ImportContext context) {
            switch (context) {
                case ImportContext.Element: return "element";
                case ImportContext.Attribute: return "attribute";
                case ImportContext.Text: return "text";
                default:
                    throw new ArgumentException(Res.GetString(Res.XmlInternalError), "context");
            }
        }

        static Exception InvalidAttributeUseException(Type type) {
            return new InvalidOperationException(Res.GetString(Res.XmlInvalidAttributeUse, type.FullName));
        }

        static Exception UnsupportedException(TypeDesc typeDesc, ImportContext context) {
            return new InvalidOperationException(Res.GetString(Res.XmlIllegalTypeContext, typeDesc.FullName, GetContextName(context)));
        }

        StructMapping CreateRootMapping() {
            TypeDesc typeDesc = typeScope.GetTypeDesc(typeof(object));
            StructMapping mapping = new StructMapping();
            mapping.TypeDesc = typeDesc;
            mapping.TypeName = Soap.UrType;
            mapping.Namespace = XmlSchema.Namespace;
            mapping.Members = new MemberMapping[0];
            mapping.IncludeInSchema = false;
            return mapping;
        }

        NullableMapping CreateNullableMapping(TypeMapping baseMapping, Type type) {
            TypeDesc typeDesc = baseMapping.TypeDesc.GetNullableTypeDesc(type);
            TypeMapping existingMapping;
            if (!baseMapping.IsAnonymousType)
            {
                existingMapping = (TypeMapping)nullables[baseMapping.TypeName, baseMapping.Namespace];
            }
            else
            {
                existingMapping = (TypeMapping)anonymous[type];
            }

            NullableMapping mapping;
            if (existingMapping != null) {
                if (existingMapping is NullableMapping) {
                    mapping = (NullableMapping)existingMapping;
                    if (mapping.BaseMapping is PrimitiveMapping && baseMapping is PrimitiveMapping)
                        return mapping;
                    else if (mapping.BaseMapping == baseMapping) {
                        return mapping;
                    }
                    else {
                        throw new InvalidOperationException(Res.GetString(Res.XmlTypesDuplicate, typeDesc.FullName, existingMapping.TypeDesc.FullName, typeDesc.Name, existingMapping.Namespace));
                    }
                }
                else {
                    throw new InvalidOperationException(Res.GetString(Res.XmlTypesDuplicate, typeDesc.FullName, existingMapping.TypeDesc.FullName, typeDesc.Name, existingMapping.Namespace));
                }
            }
            mapping = new NullableMapping();
            mapping.BaseMapping = baseMapping;
            mapping.TypeDesc = typeDesc;
            mapping.TypeName = baseMapping.TypeName;
            mapping.Namespace = baseMapping.Namespace;
            mapping.IncludeInSchema = baseMapping.IncludeInSchema;
            if (!baseMapping.IsAnonymousType)
            {
            nullables.Add(baseMapping.TypeName, baseMapping.Namespace, mapping);
            }
            else
            {
                anonymous[type] = mapping;
            }
            
            typeScope.AddTypeMapping(mapping);
            return mapping;
        }

        StructMapping GetRootMapping() {
            if (root == null) {
                root = CreateRootMapping();
                typeScope.AddTypeMapping(root);
            }
            return root;
        }

        TypeMapping GetTypeMapping(string typeName, string ns, TypeDesc typeDesc, NameTable typeLib, Type type) {
            TypeMapping mapping ;
            if (typeName == null || typeName.Length == 0)
                mapping = type == null ? null : (TypeMapping)anonymous[type];
            else
                mapping = (TypeMapping)typeLib[typeName, ns];

            if (mapping == null) return null;
            if (!mapping.IsAnonymousType && mapping.TypeDesc != typeDesc) 
                throw new InvalidOperationException(Res.GetString(Res.XmlTypesDuplicate, typeDesc.FullName, mapping.TypeDesc.FullName, typeName, ns));
            return mapping;
        }

        StructMapping ImportStructLikeMapping(StructModel model, string ns, bool openModel, XmlAttributes a, RecursionLimiter limiter) {
            if (model.TypeDesc.Kind == TypeKind.Root) return GetRootMapping();
            if (a == null)
                a = GetAttributes(model.Type, false);

            string typeNs = ns;
            if (a.XmlType != null && a.XmlType.Namespace != null)
                typeNs = a.XmlType.Namespace;
            else if (a.XmlRoot != null && a.XmlRoot.Namespace != null)
                typeNs = a.XmlRoot.Namespace;

            string typeName = IsAnonymousType(a, ns) ? null : XsdTypeName(model.Type, a, model.TypeDesc.Name);
            typeName = XmlConvert.EncodeLocalName(typeName);

            StructMapping mapping = (StructMapping)GetTypeMapping(typeName, typeNs, model.TypeDesc, types, model.Type);
            if (mapping == null) {
                mapping = new StructMapping();
                mapping.TypeDesc = model.TypeDesc;
                mapping.Namespace = typeNs;
                mapping.TypeName = typeName;
                if (!mapping.IsAnonymousType)
                    types.Add(typeName, typeNs, mapping);
                else
                    anonymous[model.Type] = mapping;
                if (a.XmlType != null) {
                    mapping.IncludeInSchema = a.XmlType.IncludeInSchema;
                }

                if (limiter.IsExceededLimit) {
                    limiter.DeferredWorkItems.Add(new ImportStructWorkItem(model, mapping));
                    return mapping;
                }

                limiter.Depth++;

                InitializeStructMembers(mapping, model, openModel, typeName, limiter);
                while (limiter.DeferredWorkItems.Count > 0) {
                    int index = limiter.DeferredWorkItems.Count - 1;
                    ImportStructWorkItem item = limiter.DeferredWorkItems[index];
                    if (InitializeStructMembers(item.Mapping, item.Model, openModel, typeName, limiter)) {
                        //
                        // if InitializeStructMembers returns true, then there were *no* chages to the DeferredWorkItems
                        //
#if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (index != limiter.DeferredWorkItems.Count - 1)
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "DeferredWorkItems.Count have changed"));
                    if (item != limiter.DeferredWorkItems[index])
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "DeferredWorkItems.Top have changed"));
#endif
                        // Remove the last work item
                        limiter.DeferredWorkItems.RemoveAt(index);
                    }
                }
                limiter.Depth--;
            }
            return mapping;
        }

        bool InitializeStructMembers(StructMapping mapping, StructModel model, bool openModel, string typeName, RecursionLimiter limiter) {
            if (mapping.IsFullyInitialized)
                return true;

                if (model.TypeDesc.BaseTypeDesc != null) {
                    TypeModel baseModel = modelScope.GetTypeModel(model.Type.BaseType, false);
                    if (!(baseModel is StructModel)) {
                        //XmlUnsupportedInheritance=Using '{0}' as a base type for a class is not supported by XmlSerializer.
                         throw new NotSupportedException(Res.GetString(Res.XmlUnsupportedInheritance, model.Type.BaseType.FullName));
                    }
                StructMapping baseMapping = ImportStructLikeMapping((StructModel)baseModel, mapping.Namespace, openModel, null, limiter);
                // check to see if the import of the baseMapping was deffered
                int baseIndex = limiter.DeferredWorkItems.IndexOf(baseMapping);
                if (baseIndex < 0) {
                    mapping.BaseMapping = baseMapping;

                    ICollection values = mapping.BaseMapping.LocalAttributes.Values;
                    foreach (AttributeAccessor attribute in values) {
                        AddUniqueAccessor(mapping.LocalAttributes, attribute);
                    }
                    if (!mapping.BaseMapping.HasExplicitSequence()) {
                        values = mapping.BaseMapping.LocalElements.Values;
                        foreach (ElementAccessor e in values) {
                            AddUniqueAccessor(mapping.LocalElements, e);
                        }
                    }
                }
                else {
                    // the import of the baseMapping was deffered, make sure that the derived mappings is deffered as well
                    if (!limiter.DeferredWorkItems.Contains(mapping)) {
                        limiter.DeferredWorkItems.Add(new ImportStructWorkItem(model, mapping));
                    }
                    // make sure that baseMapping get processed before the derived
                    int top = limiter.DeferredWorkItems.Count - 1;
                    if (baseIndex < top) {
                        ImportStructWorkItem baseMappingWorkItem = limiter.DeferredWorkItems[baseIndex];
                        limiter.DeferredWorkItems[baseIndex] = limiter.DeferredWorkItems[top];
                        limiter.DeferredWorkItems[top] = baseMappingWorkItem;
                    }
                    return false;
                }
            }
                ArrayList members = new ArrayList();
                TextAccessor textAccesor = null;
                bool hasElements = false;
                bool isSequence = false;
                
                foreach (MemberInfo memberInfo in model.GetMemberInfos()) {
                    if ((memberInfo.MemberType & (MemberTypes.Field | MemberTypes.Property)) == 0)
                        continue;
                    XmlAttributes memberAttrs = GetAttributes(memberInfo);
                    if (memberAttrs.XmlIgnore) continue;
                    FieldModel fieldModel = model.GetFieldModel(memberInfo);
                    if (fieldModel == null) continue;
                    try {
                    MemberMapping member = ImportFieldMapping(model, fieldModel, memberAttrs, mapping.Namespace, limiter);
                        if (member == null) continue;
                        if (mapping.BaseMapping != null) {
                            if (mapping.BaseMapping.Declares(member, mapping.TypeName)) continue;
                        }
                        isSequence |= member.IsSequence;
                        // add All memeber accessors to the scope accessors
                        AddUniqueAccessor(member, mapping.LocalElements, mapping.LocalAttributes, isSequence);

                        if (member.Text != null) {
                            if (!member.Text.Mapping.TypeDesc.CanBeTextValue && member.Text.Mapping.IsList)
                                throw new InvalidOperationException(Res.GetString(Res.XmlIllegalTypedTextAttribute, typeName, member.Text.Name, member.Text.Mapping.TypeDesc.FullName));
                            if (textAccesor != null) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlIllegalMultipleText, model.Type.FullName));
                            }
                            textAccesor = member.Text;
                        }
                        if (member.Xmlns != null) {
                            if (mapping.XmlnsMember != null)
                                throw new InvalidOperationException(Res.GetString(Res.XmlMultipleXmlns, model.Type.FullName));
                            mapping.XmlnsMember = member;
                        }
                        if (member.Elements != null && member.Elements.Length != 0) {
                            hasElements = true;
                        }
                        members.Add(member);
                    }
                    catch (Exception e) {
                        if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                            throw;
                        }
                        throw CreateMemberReflectionException(fieldModel, e);
                    }
                }
                mapping.SetContentModel(textAccesor, hasElements);
                if (isSequence) {
                    Hashtable ids = new Hashtable();
                    for (int i = 0; i < members.Count; i++) {
                        MemberMapping member = (MemberMapping)members[i];
                        if (!member.IsParticle)
                            continue;
                        if (member.IsSequence) {
                            if (ids[member.SequenceId] != null) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceUnique, member.SequenceId.ToString(CultureInfo.InvariantCulture), "Order", member.Name));
                            }
                            ids[member.SequenceId] = member;
                        }
                        else {
                            throw new InvalidOperationException(Res.GetString(Res.XmlSequenceInconsistent, "Order", member.Name));
                        }
                    }
                    members.Sort(new MemberMappingComparer());
                }
                mapping.Members = (MemberMapping[])members.ToArray(typeof(MemberMapping));

                if (mapping.BaseMapping == null) mapping.BaseMapping = GetRootMapping();

                if (mapping.XmlnsMember != null && mapping.BaseMapping.HasXmlnsMember)
                    throw new InvalidOperationException(Res.GetString(Res.XmlMultipleXmlns, model.Type.FullName));

            IncludeTypes(model.Type, limiter);
                typeScope.AddTypeMapping(mapping);
            if (openModel)
                mapping.IsOpenModel = true;
            return true;
        }

        static bool IsAnonymousType(XmlAttributes a, string contextNs) {
            if (a.XmlType != null && a.XmlType.AnonymousType) {
                //
                // check to see if the anonymous type is used in the original context
                // only treat it as Anonymous, if the referencing element's namespace
                // matches the original referencing element, otherwise revert to
                // non-Anonymous handling for backward compatibility.
                //
                string originalNs = a.XmlType.Namespace;
                return string.IsNullOrEmpty(originalNs) || originalNs == contextNs;
            }
            return false;
        }

        internal string XsdTypeName(Type type) {
            if (type == typeof(object)) return Soap.UrType;
            TypeDesc typeDesc = typeScope.GetTypeDesc(type);
            if (typeDesc.IsPrimitive && typeDesc.DataType != null && typeDesc.DataType.Name != null && typeDesc.DataType.Name.Length > 0)
                return typeDesc.DataType.Name;
            return XsdTypeName(type, GetAttributes(type, false), typeDesc.Name);
        }

        internal string XsdTypeName(Type type, XmlAttributes a, string name) {
            string typeName = name;
            if (a.XmlType != null && a.XmlType.TypeName.Length > 0)
                typeName = a.XmlType.TypeName;

            if (type.IsGenericType && typeName.IndexOf('{') >= 0) {
                Type genType = type.GetGenericTypeDefinition();
                Type[] names = genType.GetGenericArguments();
                Type[] types = type.GetGenericArguments();

                for (int i = 0; i < names.Length; i++) {
                    string argument = "{" + names[i] + "}";
                    if (typeName.Contains(argument)) {
                        typeName = typeName.Replace(argument, XsdTypeName(types[i]));
                        if (typeName.IndexOf('{') < 0) {
                            break;
                        }
                    }
                }
            }
            // 
            return typeName;
        }

        private static int CountAtLevel(XmlArrayItemAttributes attributes, int level) {
            int sum = 0;
            for (int i = 0; i < attributes.Count; i++)
                if (attributes[i].NestingLevel == level) sum++;
            return sum;
        }

        void SetArrayMappingType(ArrayMapping mapping, string defaultNs, Type type) {
            XmlAttributes a = GetAttributes(type, false);
            bool isAnonymous = IsAnonymousType(a, defaultNs);
            if (isAnonymous) {
                mapping.TypeName = null;
                mapping.Namespace = defaultNs;
                return;
            }
            string name;
            string ns;
            TypeMapping itemTypeMapping;
            ElementAccessor element = null;

            if (mapping.Elements.Length == 1) {
                element = mapping.Elements[0];
                itemTypeMapping = element.Mapping;
            }
            else {
                itemTypeMapping = null;
            }

            bool generateTypeName = true;
            if (a.XmlType != null) {
                ns = a.XmlType.Namespace;
                name = XsdTypeName(type, a, a.XmlType.TypeName);
                name = XmlConvert.EncodeLocalName(name);
                generateTypeName = name == null;
            }
            else if (itemTypeMapping is EnumMapping) {
                ns = itemTypeMapping.Namespace;
                name = itemTypeMapping.DefaultElementName;
            }
            else if (itemTypeMapping is PrimitiveMapping) {
                ns = defaultNs;
                name = itemTypeMapping.TypeDesc.DataType.Name;
            }
            else if (itemTypeMapping is StructMapping && itemTypeMapping.TypeDesc.IsRoot) {
                ns = defaultNs;
                name = Soap.UrType;
            }
            else if (itemTypeMapping != null) {
                ns = itemTypeMapping.Namespace == XmlSchema.Namespace ? defaultNs : itemTypeMapping.Namespace;
                name = itemTypeMapping.DefaultElementName;
            }
            else {
                ns = defaultNs;
                name = "Choice" + (choiceNum++);
            }

            if (name == null)
                name = "Any";

            if (element != null)
                ns = element.Namespace;

            if (ns == null)
                ns = defaultNs;

            string uniqueName = name = generateTypeName ? "ArrayOf" + CodeIdentifier.MakePascal(name) : name;
            int i = 1;
            TypeMapping existingMapping = (TypeMapping)types[uniqueName, ns];
            while (existingMapping != null) {
                if (existingMapping is ArrayMapping) {
                    ArrayMapping arrayMapping = (ArrayMapping)existingMapping;
                    if (AccessorMapping.ElementsMatch(arrayMapping.Elements, mapping.Elements)) {
                        break;
                    }
                }
                // need to re-name the mapping
                uniqueName = name + i.ToString(CultureInfo.InvariantCulture);
                existingMapping = (TypeMapping)types[uniqueName, ns];
                i++;
            }
            mapping.TypeName = uniqueName;
            mapping.Namespace = ns;
        }

        ArrayMapping ImportArrayLikeMapping(ArrayModel model, string ns, RecursionLimiter limiter) {
            ArrayMapping mapping = new ArrayMapping();
            mapping.TypeDesc = model.TypeDesc;

            if (savedArrayItemAttributes == null)
                savedArrayItemAttributes = new XmlArrayItemAttributes();
            if (CountAtLevel(savedArrayItemAttributes, arrayNestingLevel) == 0)
                savedArrayItemAttributes.Add(CreateArrayItemAttribute(typeScope.GetTypeDesc(model.Element.Type), arrayNestingLevel));
            CreateArrayElementsFromAttributes(mapping, savedArrayItemAttributes, model.Element.Type, savedArrayNamespace == null ? ns : savedArrayNamespace, limiter);
            SetArrayMappingType(mapping, ns, model.Type);

            // reconcile accessors now that we have the ArrayMapping namespace
            for (int i = 0; i < mapping.Elements.Length; i++) {
                mapping.Elements[i] = ReconcileLocalAccessor(mapping.Elements[i], mapping.Namespace);
            }

            IncludeTypes(model.Type);

            // in the case of an ArrayMapping we can have more that one mapping correspond to a type
            // examples of that are ArrayList and object[] both will map tp ArrayOfur-type
            // so we create a link list for all mappings of the same XSD type
            ArrayMapping existingMapping = (ArrayMapping)types[mapping.TypeName, mapping.Namespace];
            if (existingMapping != null) {
                ArrayMapping first = existingMapping;
                while (existingMapping != null) {
                    if (existingMapping.TypeDesc == model.TypeDesc)
                        return existingMapping;
                    existingMapping = existingMapping.Next;
                }
                mapping.Next = first;
                if (!mapping.IsAnonymousType)
                    types[mapping.TypeName, mapping.Namespace] = mapping;
                else
                    anonymous[model.Type] = mapping;
                return mapping;
            }
            typeScope.AddTypeMapping(mapping);
            if (!mapping.IsAnonymousType)
                types.Add(mapping.TypeName, mapping.Namespace, mapping);
            else
                anonymous[model.Type] = mapping;
            return mapping;
        }

        void CheckContext(TypeDesc typeDesc, ImportContext context) {
            switch (context) {
                case ImportContext.Element:
                    if (typeDesc.CanBeElementValue) return;
                    break;
                case ImportContext.Attribute:
                    if (typeDesc.CanBeAttributeValue) return;
                    break;
                case ImportContext.Text:
                    if (typeDesc.CanBeTextValue || typeDesc.IsEnum || typeDesc.IsPrimitive)
                        return;
                    break;
                default:
                    throw new ArgumentException(Res.GetString(Res.XmlInternalError), "context");
            }
            throw UnsupportedException(typeDesc, context);
        }
        
        PrimitiveMapping ImportPrimitiveMapping(PrimitiveModel model, ImportContext context, string dataType, bool repeats) {
            PrimitiveMapping mapping = new PrimitiveMapping();
            if (dataType.Length > 0) {
                mapping.TypeDesc = typeScope.GetTypeDesc(dataType, XmlSchema.Namespace);
                if (mapping.TypeDesc == null) {
                    // try it as a non-Xsd type
                    mapping.TypeDesc = typeScope.GetTypeDesc(dataType, UrtTypes.Namespace);
                    if (mapping.TypeDesc == null) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlUdeclaredXsdType, dataType));
                    }
                }
            }
            else {
                mapping.TypeDesc = model.TypeDesc;
            }
            mapping.TypeName = mapping.TypeDesc.DataType.Name;
            mapping.Namespace = mapping.TypeDesc.IsXsdType ? XmlSchema.Namespace : UrtTypes.Namespace;
            mapping.IsList = repeats;
            CheckContext(mapping.TypeDesc, context);
            return mapping;
        }
        
        EnumMapping ImportEnumMapping(EnumModel model, string ns, bool repeats) {
            XmlAttributes a = GetAttributes(model.Type, false);
            string typeNs = ns;
            if (a.XmlType != null && a.XmlType.Namespace != null)
                typeNs = a.XmlType.Namespace;

            string typeName = IsAnonymousType(a, ns) ? null : XsdTypeName(model.Type, a, model.TypeDesc.Name);
            typeName = XmlConvert.EncodeLocalName(typeName);

            EnumMapping mapping = (EnumMapping)GetTypeMapping(typeName, typeNs, model.TypeDesc, types, model.Type);
            if (mapping == null) {
                mapping = new EnumMapping();
                mapping.TypeDesc = model.TypeDesc;
                mapping.TypeName = typeName;
                mapping.Namespace = typeNs;
                mapping.IsFlags =  model.Type.IsDefined(typeof(FlagsAttribute), false);
                if (mapping.IsFlags && repeats)
                    throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttributeFlagsArray, model.TypeDesc.FullName));
                mapping.IsList = repeats;
                mapping.IncludeInSchema = a.XmlType == null ? true : a.XmlType.IncludeInSchema;
                if (!mapping.IsAnonymousType)
                    types.Add(typeName, typeNs, mapping);
                else
                    anonymous[model.Type] = mapping;
                ArrayList constants = new ArrayList();                
                for (int i = 0; i < model.Constants.Length; i++) {
                    ConstantMapping constant = ImportConstantMapping(model.Constants[i]);
                    if (constant != null) constants.Add(constant);
                }
                if (constants.Count == 0) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlNoSerializableMembers, model.TypeDesc.FullName));
                }
                mapping.Constants = (ConstantMapping[])constants.ToArray(typeof(ConstantMapping));
                typeScope.AddTypeMapping(mapping);
            }
            return mapping;
        }
        
        ConstantMapping ImportConstantMapping(ConstantModel model) {
            XmlAttributes a = GetAttributes(model.FieldInfo);
            if (a.XmlIgnore) return null;
            if ((a.XmlFlags & ~XmlAttributeFlags.Enum) != 0)
                throw new InvalidOperationException(Res.GetString(Res.XmlInvalidConstantAttribute));
            if (a.XmlEnum == null)
                a.XmlEnum = new XmlEnumAttribute();

            ConstantMapping constant = new ConstantMapping();
            constant.XmlName = a.XmlEnum.Name == null ? model.Name : a.XmlEnum.Name;
            constant.Name = model.Name;
            constant.Value = model.Value;
            return constant;
        }
        
        MembersMapping ImportMembersMapping(XmlReflectionMember[] xmlReflectionMembers, string ns, bool hasWrapperElement, bool rpc, bool openModel, RecursionLimiter limiter) {
            MembersMapping members = new MembersMapping();
            members.TypeDesc = typeScope.GetTypeDesc(typeof(object[]));
            MemberMapping[] mappings = new MemberMapping[xmlReflectionMembers.Length];
            NameTable elements = new NameTable();
            NameTable attributes = new NameTable();
            TextAccessor textAccessor = null;
            bool isSequence = false;

            for (int i = 0; i < mappings.Length; i++) {
                try {
                    MemberMapping mapping = ImportMemberMapping(xmlReflectionMembers[i], ns, xmlReflectionMembers, rpc, openModel, limiter);
                    if (!hasWrapperElement) {
                        if (mapping.Attribute != null) {
                            if (rpc) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitAttributeAttributes));
                            }
                            else {
                                throw new InvalidOperationException(Res.GetString(Res.XmlInvalidAttributeType, "XmlAttribute"));
                            }
                        }
                    }
                    if (rpc && xmlReflectionMembers[i].IsReturnValue) {
                        if (i > 0) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidReturnPosition));
                        mapping.IsReturnValue = true;
                    }
                    mappings[i] = mapping;
                    isSequence |= mapping.IsSequence;
                    if (!xmlReflectionMembers[i].XmlAttributes.XmlIgnore) {
                        // add All memeber accessors to the scope accessors
                        AddUniqueAccessor(mapping, elements, attributes, isSequence);
                    }

                    mappings[i] = mapping;
                    if (mapping.Text != null) {
                        if (textAccessor != null) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalMultipleTextMembers));
                        }
                        textAccessor = mapping.Text;
                    }

                    if (mapping.Xmlns != null) {
                        if (members.XmlnsMember != null)
                            throw new InvalidOperationException(Res.GetString(Res.XmlMultipleXmlnsMembers));
                        members.XmlnsMember = mapping;
                    }
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw CreateReflectionException(xmlReflectionMembers[i].MemberName, e);
                }
            }
            if (isSequence) {
                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMembers, "Order"));
            }
            members.Members = mappings;
            members.HasWrapperElement = hasWrapperElement;
            return members;
        }

        MemberMapping ImportMemberMapping(XmlReflectionMember xmlReflectionMember, string ns, XmlReflectionMember[] xmlReflectionMembers, bool rpc, bool openModel, RecursionLimiter limiter) {
            XmlSchemaForm form = rpc ? XmlSchemaForm.Unqualified : XmlSchemaForm.Qualified;
            XmlAttributes a = xmlReflectionMember.XmlAttributes;
            TypeDesc typeDesc = typeScope.GetTypeDesc(xmlReflectionMember.MemberType);

            if (a.XmlFlags == 0) {
                if (typeDesc.IsArrayLike) {
                    XmlArrayAttribute xmlArray = CreateArrayAttribute(typeDesc);
                    xmlArray.ElementName = xmlReflectionMember.MemberName;
                    xmlArray.Namespace = rpc ? null : ns;
                    xmlArray.Form = form;
                    a.XmlArray = xmlArray;
                }
                else {
                    XmlElementAttribute xmlElement = CreateElementAttribute(typeDesc);
                    // If there is no metadata specified on a parameter, then see if someone used
                    // an XmlRoot attribute on the struct or class.
                    if (typeDesc.IsStructLike) {
                        XmlAttributes structAttrs = new XmlAttributes(xmlReflectionMember.MemberType);
                        if (structAttrs.XmlRoot != null) {
                            if (structAttrs.XmlRoot.ElementName.Length > 0)
                                xmlElement.ElementName = structAttrs.XmlRoot.ElementName;
                            if (rpc) {
                                xmlElement.Namespace = null;
                                if (structAttrs.XmlRoot.IsNullableSpecified)
                                    xmlElement.IsNullable = structAttrs.XmlRoot.IsNullable;
                            }
                            else {
                                xmlElement.Namespace = structAttrs.XmlRoot.Namespace;
                                xmlElement.IsNullable = structAttrs.XmlRoot.IsNullable;
                            }
                        }
                    }
                    if (xmlElement.ElementName.Length == 0)
                        xmlElement.ElementName = xmlReflectionMember.MemberName;
                    if (xmlElement.Namespace == null && !rpc)
                        xmlElement.Namespace = ns;
                    xmlElement.Form = form;
                    a.XmlElements.Add(xmlElement);
                }
            }
            else if (a.XmlRoot != null) {
                CheckNullable(a.XmlRoot.IsNullable, typeDesc, null);
            }
            MemberMapping member = new MemberMapping();
            member.Name = xmlReflectionMember.MemberName;
            bool checkSpecified = FindSpecifiedMember(xmlReflectionMember.MemberName, xmlReflectionMembers) != null;
            FieldModel model = new FieldModel(xmlReflectionMember.MemberName, xmlReflectionMember.MemberType, typeScope.GetTypeDesc(xmlReflectionMember.MemberType), checkSpecified, false);
            member.CheckShouldPersist = model.CheckShouldPersist;
            member.CheckSpecified = model.CheckSpecified;
            member.ReadOnly = model.ReadOnly; // || !model.FieldTypeDesc.HasDefaultConstructor;

            Type choiceIdentifierType = null;
            if (a.XmlChoiceIdentifier != null) {
                choiceIdentifierType = GetChoiceIdentifierType(a.XmlChoiceIdentifier, xmlReflectionMembers, typeDesc.IsArrayLike, model.Name);
            }
            ImportAccessorMapping(member, model, a, ns, choiceIdentifierType, rpc, openModel, limiter);
            if (xmlReflectionMember.OverrideIsNullable && member.Elements.Length > 0)
                member.Elements[0].IsNullable = false;
            return member;
        }
        
        internal static XmlReflectionMember FindSpecifiedMember(string memberName, XmlReflectionMember[] reflectionMembers) {
            for (int i = 0; i < reflectionMembers.Length; i++)
                if (string.Compare(reflectionMembers[i].MemberName, memberName + "Specified", StringComparison.Ordinal) == 0)
                    return reflectionMembers[i];
            return null;
        }

        MemberMapping ImportFieldMapping(StructModel parent, FieldModel model, XmlAttributes a, string ns, RecursionLimiter limiter) {
            MemberMapping member = new MemberMapping();
            member.Name = model.Name;
            member.CheckShouldPersist = model.CheckShouldPersist;
            member.CheckSpecified = model.CheckSpecified;
            member.MemberInfo = model.MemberInfo;
            member.CheckSpecifiedMemberInfo = model.CheckSpecifiedMemberInfo;
            member.CheckShouldPersistMethodInfo = model.CheckShouldPersistMethodInfo;
            member.ReadOnly = model.ReadOnly; // || !model.FieldTypeDesc.HasDefaultConstructor;
            Type choiceIdentifierType = null;
            if (a.XmlChoiceIdentifier != null) {
                choiceIdentifierType = GetChoiceIdentifierType(a.XmlChoiceIdentifier, parent, model.FieldTypeDesc.IsArrayLike, model.Name);
            }
            ImportAccessorMapping(member, model, a, ns, choiceIdentifierType, false, false, limiter);
            return member;
        }

        Type CheckChoiceIdentifierType(Type type, bool isArrayLike, string identifierName, string memberName) {
            if (type.IsArray) {
                if (!isArrayLike) {
                    // Inconsistent type of the choice identifier '{0}'.  Please use {1}.
                    throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentifierType, identifierName, memberName, type.GetElementType().FullName));
                }
                type = type.GetElementType();
            }
            else if (isArrayLike) {
                // Inconsistent type of the choice identifier '{0}'.  Please use {1}.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentifierArrayType, identifierName, memberName, type.FullName));
            }

            if (!type.IsEnum) {
                // Choice identifier '{0}' must be an enum.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentifierTypeEnum, identifierName));
            }
            return type;
        }

        Type GetChoiceIdentifierType(XmlChoiceIdentifierAttribute choice, XmlReflectionMember[] xmlReflectionMembers, bool isArrayLike, string accessorName) {
            for (int i = 0; i < xmlReflectionMembers.Length; i++) {
                if (choice.MemberName == xmlReflectionMembers[i].MemberName) {
                    return CheckChoiceIdentifierType(xmlReflectionMembers[i].MemberType, isArrayLike, choice.MemberName, accessorName);
                }
            }
            // Missing '{0}' needed for serialization of choice '{1}'.
            throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferMemberMissing, choice.MemberName, accessorName));
        }

        Type GetChoiceIdentifierType(XmlChoiceIdentifierAttribute choice, StructModel structModel, bool isArrayLike, string accessorName) {
            // check that the choice field exists

            MemberInfo[] infos = structModel.Type.GetMember(choice.MemberName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (infos == null || infos.Length == 0) {
                // if we can not find the choice identifier between fields, check proerties
                PropertyInfo info = structModel.Type.GetProperty(choice.MemberName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                
                if (info == null) {
                    // Missing '{0}' needed for serialization of choice '{1}'.
                    throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferMemberMissing, choice.MemberName, accessorName));
                }
                infos = new MemberInfo[] { info };
            }
            else if (infos.Length > 1) {
                // Ambiguous choice identifer: there are several members named '{0}'.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferAmbiguous, choice.MemberName));
            }

            FieldModel member = structModel.GetFieldModel(infos[0]);
            if (member == null) {
                // Missing '{0}' needed for serialization of choice '{1}'.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferMemberMissing, choice.MemberName, accessorName));
            }
            choice.MemberInfo = member.MemberInfo;
            Type enumType = member.FieldType;
            enumType = CheckChoiceIdentifierType(enumType, isArrayLike, choice.MemberName, accessorName);
            return enumType;
        }

        void CreateArrayElementsFromAttributes(ArrayMapping arrayMapping, XmlArrayItemAttributes attributes, Type arrayElementType, string arrayElementNs, RecursionLimiter limiter) {
            NameTable arrayItemElements = new NameTable();   // xmlelementname + xmlns -> ElementAccessor

            for (int i = 0; attributes != null && i < attributes.Count; i++) {
                XmlArrayItemAttribute xmlArrayItem = attributes[i];
                if (xmlArrayItem.NestingLevel != arrayNestingLevel)
                    continue;
                Type targetType = xmlArrayItem.Type != null ? xmlArrayItem.Type : arrayElementType;
                TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                ElementAccessor arrayItemElement = new ElementAccessor();
                arrayItemElement.Namespace = xmlArrayItem.Namespace == null ? arrayElementNs : xmlArrayItem.Namespace;
                arrayItemElement.Mapping = ImportTypeMapping(modelScope.GetTypeModel(targetType), arrayItemElement.Namespace, ImportContext.Element, xmlArrayItem.DataType, null, limiter);
                arrayItemElement.Name = xmlArrayItem.ElementName.Length == 0 ? arrayItemElement.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(xmlArrayItem.ElementName);
                arrayItemElement.IsNullable = xmlArrayItem.IsNullableSpecified ? xmlArrayItem.IsNullable : targetTypeDesc.IsNullable || targetTypeDesc.IsOptionalValue;
                arrayItemElement.Form = xmlArrayItem.Form == XmlSchemaForm.None ? XmlSchemaForm.Qualified : xmlArrayItem.Form;
                CheckForm(arrayItemElement.Form, arrayElementNs != arrayItemElement.Namespace);
                CheckNullable(arrayItemElement.IsNullable, targetTypeDesc, arrayItemElement.Mapping);
                AddUniqueAccessor(arrayItemElements, arrayItemElement);
            }
            arrayMapping.Elements = (ElementAccessor[])arrayItemElements.ToArray(typeof(ElementAccessor));
        }

        void ImportAccessorMapping(MemberMapping accessor, FieldModel model, XmlAttributes a, string ns, Type choiceIdentifierType, bool rpc, bool openModel, RecursionLimiter limiter) {
            XmlSchemaForm elementFormDefault = XmlSchemaForm.Qualified;
            int previousNestingLevel = arrayNestingLevel;
            int sequenceId = -1;
            XmlArrayItemAttributes previousArrayItemAttributes = savedArrayItemAttributes;
            string previousArrayNamespace = savedArrayNamespace;
            arrayNestingLevel = 0;
            savedArrayItemAttributes = null;
            savedArrayNamespace = null;
            Type accessorType = model.FieldType;
            string accessorName = model.Name;
            ArrayList elementList = new ArrayList();
            NameTable elements = new NameTable();
            accessor.TypeDesc = typeScope.GetTypeDesc(accessorType);
            XmlAttributeFlags flags = a.XmlFlags;
            accessor.Ignore = a.XmlIgnore;
            if (rpc)
                CheckTopLevelAttributes(a, accessorName);
            else
                CheckAmbiguousChoice(a, accessorType, accessorName);

            XmlAttributeFlags elemFlags = XmlAttributeFlags.Elements | XmlAttributeFlags.Text | XmlAttributeFlags.AnyElements | XmlAttributeFlags.ChoiceIdentifier;
            XmlAttributeFlags attrFlags = XmlAttributeFlags.Attribute | XmlAttributeFlags.AnyAttribute;
            XmlAttributeFlags arrayFlags = XmlAttributeFlags.Array | XmlAttributeFlags.ArrayItems;

            // special case for byte[]. It can be a primitive (base64Binary or hexBinary), or it can
            // be an array of bytes. Our default is primitive; specify [XmlArray] to get array behavior.
            if ((flags & arrayFlags) != 0 && accessorType == typeof(byte[]))
                accessor.TypeDesc = typeScope.GetArrayTypeDesc(accessorType);

            if (a.XmlChoiceIdentifier != null) {
                accessor.ChoiceIdentifier = new ChoiceIdentifierAccessor();
                accessor.ChoiceIdentifier.MemberName = a.XmlChoiceIdentifier.MemberName;
                accessor.ChoiceIdentifier.MemberInfo = a.XmlChoiceIdentifier.MemberInfo;
                accessor.ChoiceIdentifier.Mapping = ImportTypeMapping(modelScope.GetTypeModel(choiceIdentifierType), ns, ImportContext.Element, String.Empty, null, limiter);
                CheckChoiceIdentifierMapping((EnumMapping)accessor.ChoiceIdentifier.Mapping);
            }

            if (accessor.TypeDesc.IsArrayLike) {
                Type arrayElementType = TypeScope.GetArrayElementType(accessorType, model.FieldTypeDesc.FullName + "." + model.Name);

                if ((flags & attrFlags) != 0) {
                    if ((flags & attrFlags) != flags) 
                        throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttributesArrayAttribute));

                    if (a.XmlAttribute != null && !accessor.TypeDesc.ArrayElementTypeDesc.IsPrimitive && !accessor.TypeDesc.ArrayElementTypeDesc.IsEnum) {

                        if (accessor.TypeDesc.ArrayElementTypeDesc.Kind == TypeKind.Serializable) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttrOrTextInterface, accessorName, accessor.TypeDesc.ArrayElementTypeDesc.FullName, typeof(IXmlSerializable).Name));
                        }
                        else {
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttrOrText, accessorName, accessor.TypeDesc.ArrayElementTypeDesc.FullName));
                        }
                    }

                    bool isList = a.XmlAttribute != null && (accessor.TypeDesc.ArrayElementTypeDesc.IsPrimitive || accessor.TypeDesc.ArrayElementTypeDesc.IsEnum);

                    if (a.XmlAnyAttribute != null) {
                        a.XmlAttribute = new XmlAttributeAttribute();
                    }

                    AttributeAccessor attribute = new AttributeAccessor();
                    Type targetType = a.XmlAttribute.Type == null ? arrayElementType : a.XmlAttribute.Type;
                    TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                    attribute.Name = Accessor.EscapeQName(a.XmlAttribute.AttributeName.Length == 0 ? accessorName : a.XmlAttribute.AttributeName);
                    attribute.Namespace = a.XmlAttribute.Namespace == null ? ns : a.XmlAttribute.Namespace;
                    attribute.Form = a.XmlAttribute.Form;
                    if (attribute.Form == XmlSchemaForm.None && ns != attribute.Namespace) {
                        attribute.Form = XmlSchemaForm.Qualified;
                    }
                    attribute.CheckSpecial();
                    CheckForm(attribute.Form, ns != attribute.Namespace);
                    attribute.Mapping = ImportTypeMapping(modelScope.GetTypeModel(targetType), ns, ImportContext.Attribute, a.XmlAttribute.DataType, null, isList, false, limiter);
                    attribute.IsList = isList;
                    attribute.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                    attribute.Any = (a.XmlAnyAttribute != null);
                    if (attribute.Form == XmlSchemaForm.Qualified && attribute.Namespace != ns) {
                        if (xsdAttributes == null)
                            xsdAttributes = new NameTable();
                        attribute = (AttributeAccessor)ReconcileAccessor(attribute, xsdAttributes);
                    }
                    accessor.Attribute = attribute;

                }
                else if ((flags & elemFlags) != 0) {
                    if ((flags & elemFlags) != flags)
                        throw new InvalidOperationException(Res.GetString(Res.XmlIllegalElementsArrayAttribute));
                    
                    if (a.XmlText != null) {
                        TextAccessor text = new TextAccessor();
                        Type targetType = a.XmlText.Type == null ? arrayElementType : a.XmlText.Type;
                        TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                        text.Name = accessorName; // unused except to make more helpful error messages
                        text.Mapping = ImportTypeMapping(modelScope.GetTypeModel(targetType), ns, ImportContext.Text, a.XmlText.DataType, null, true, false, limiter);
                        if (!(text.Mapping is SpecialMapping) && targetTypeDesc != typeScope.GetTypeDesc(typeof(string)))
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalArrayTextAttribute, accessorName));

                        accessor.Text = text;
                    }
                    if (a.XmlText == null && a.XmlElements.Count == 0 && a.XmlAnyElements.Count == 0)
                        a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                    
                    for (int i = 0; i < a.XmlElements.Count; i++) {
                        XmlElementAttribute xmlElement = a.XmlElements[i];
                        Type targetType = xmlElement.Type == null ? arrayElementType : xmlElement.Type;
                        TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                        TypeModel typeModel = modelScope.GetTypeModel(targetType);
                        ElementAccessor element = new ElementAccessor();
                        element.Namespace = rpc ? null : xmlElement.Namespace == null ? ns : xmlElement.Namespace;
                        element.Mapping = ImportTypeMapping(typeModel, rpc ? ns : element.Namespace, ImportContext.Element, xmlElement.DataType, null, limiter);
                        if (a.XmlElements.Count == 1) {
                            element.Name = XmlConvert.EncodeLocalName(xmlElement.ElementName.Length == 0 ? accessorName : xmlElement.ElementName);
                            //element.IsUnbounded = element.Mapping is ArrayMapping;
                        }
                        else {
                            element.Name = xmlElement.ElementName.Length == 0 ? element.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(xmlElement.ElementName);
                        }
                        element.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        if (xmlElement.IsNullableSpecified && !xmlElement.IsNullable && typeModel.TypeDesc.IsOptionalValue)
                            //XmlInvalidNotNullable=IsNullable may not be set to 'false' for a Nullable<{0}> type. Consider using '{0}' type or removing the IsNullable property from the XmlElement attribute.
                            throw new InvalidOperationException(Res.GetString(Res.XmlInvalidNotNullable, typeModel.TypeDesc.BaseTypeDesc.FullName, "XmlElement"));
                        element.IsNullable = xmlElement.IsNullableSpecified ? xmlElement.IsNullable : typeModel.TypeDesc.IsOptionalValue;
                        element.Form = rpc ? XmlSchemaForm.Unqualified : xmlElement.Form == XmlSchemaForm.None ? elementFormDefault : xmlElement.Form;

                        CheckNullable(element.IsNullable, targetTypeDesc, element.Mapping);
                        if (!rpc) {
                            CheckForm(element.Form, ns != element.Namespace);
                            element = ReconcileLocalAccessor(element, ns);
                        }
                        if (xmlElement.Order != -1) {
                            if (xmlElement.Order != sequenceId &&  sequenceId != -1)
                                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMatch, "Order"));
                            sequenceId = xmlElement.Order;
                        }
                        AddUniqueAccessor(elements, element);
                        elementList.Add(element);
                    }
                    NameTable anys = new NameTable();
                    for (int i = 0; i < a.XmlAnyElements.Count; i++) {
                        XmlAnyElementAttribute xmlAnyElement = a.XmlAnyElements[i];
                        Type targetType = typeof(IXmlSerializable).IsAssignableFrom(arrayElementType) ? arrayElementType : typeof(XmlNode).IsAssignableFrom(arrayElementType) ? arrayElementType : typeof(XmlElement);
                        if (!arrayElementType.IsAssignableFrom(targetType))
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAnyElement, arrayElementType.FullName));
                        string anyName = xmlAnyElement.Name.Length == 0 ? xmlAnyElement.Name : XmlConvert.EncodeLocalName(xmlAnyElement.Name);
                        string anyNs = xmlAnyElement.NamespaceSpecified ? xmlAnyElement.Namespace : null;
                        if (anys[anyName, anyNs] != null) {
                            // ignore duplicate anys
                            continue;
                        }
                        anys[anyName, anyNs] = xmlAnyElement;
                        if (elements[anyName, (anyNs == null ? ns : anyNs)] != null) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlAnyElementDuplicate, accessorName, xmlAnyElement.Name, xmlAnyElement.Namespace == null ? "null" : xmlAnyElement.Namespace));
                        }
                        ElementAccessor element = new ElementAccessor();
                        element.Name = anyName;
                        element.Namespace = anyNs == null ? ns : anyNs;
                        element.Any = true;
                        element.AnyNamespaces = anyNs;
                        TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                        TypeModel typeModel = modelScope.GetTypeModel(targetType);
                        if (element.Name.Length > 0)
                            typeModel.TypeDesc.IsMixed = true;
                        element.Mapping = ImportTypeMapping(typeModel, element.Namespace, ImportContext.Element, String.Empty, null, limiter);
                        element.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        element.IsNullable = false;
                        element.Form = elementFormDefault;

                        CheckNullable(element.IsNullable, targetTypeDesc, element.Mapping);
                        if (!rpc) {
                            CheckForm(element.Form, ns != element.Namespace);
                            element = ReconcileLocalAccessor(element, ns);
                        }
                        elements.Add(element.Name, element.Namespace, element);
                        elementList.Add(element);
                        if (xmlAnyElement.Order != -1) {
                            if (xmlAnyElement.Order != sequenceId &&  sequenceId != -1)
                                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMatch, "Order"));
                            sequenceId = xmlAnyElement.Order;
                        }
                    }
                }
                else {
                    if ((flags & arrayFlags) != 0) {
                        if ((flags & arrayFlags) != flags)
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalArrayArrayAttribute));
                    }
                    
                    TypeDesc arrayElementTypeDesc = typeScope.GetTypeDesc(arrayElementType);
                    if (a.XmlArray == null)
                        a.XmlArray = CreateArrayAttribute(accessor.TypeDesc);
                    if (CountAtLevel(a.XmlArrayItems, arrayNestingLevel) == 0)
                        a.XmlArrayItems.Add(CreateArrayItemAttribute(arrayElementTypeDesc, arrayNestingLevel));
                    ElementAccessor arrayElement = new ElementAccessor();
                    arrayElement.Name = XmlConvert.EncodeLocalName(a.XmlArray.ElementName.Length == 0 ? accessorName : a.XmlArray.ElementName);
                    arrayElement.Namespace = rpc ? null : a.XmlArray.Namespace == null ? ns : a.XmlArray.Namespace;
                    savedArrayItemAttributes = a.XmlArrayItems;
                    savedArrayNamespace = arrayElement.Namespace;
                    ArrayMapping arrayMapping = ImportArrayLikeMapping(modelScope.GetArrayModel(accessorType), ns, limiter);
                    arrayElement.Mapping = arrayMapping;
                    arrayElement.IsNullable = a.XmlArray.IsNullable;
                    arrayElement.Form = rpc ? XmlSchemaForm.Unqualified : a.XmlArray.Form == XmlSchemaForm.None ? elementFormDefault : a.XmlArray.Form;
                    sequenceId = a.XmlArray.Order;
                    CheckNullable(arrayElement.IsNullable, accessor.TypeDesc, arrayElement.Mapping);
                    if (!rpc) {
                        CheckForm(arrayElement.Form, ns != arrayElement.Namespace);
                        arrayElement = ReconcileLocalAccessor(arrayElement, ns);
                    }
                    savedArrayItemAttributes = null;
                    savedArrayNamespace = null;
                    
                    AddUniqueAccessor(elements, arrayElement);
                    elementList.Add(arrayElement);
                }
            }
            else if (!accessor.TypeDesc.IsVoid) {
                XmlAttributeFlags allFlags = XmlAttributeFlags.Elements | XmlAttributeFlags.Text | XmlAttributeFlags.Attribute | XmlAttributeFlags.AnyElements | XmlAttributeFlags.ChoiceIdentifier | XmlAttributeFlags.XmlnsDeclarations;
                if ((flags & allFlags) != flags)
                    throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttribute));

                if (accessor.TypeDesc.IsPrimitive || accessor.TypeDesc.IsEnum) {
                    if (a.XmlAnyElements.Count > 0) throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAnyElement, accessor.TypeDesc.FullName));

                    if (a.XmlAttribute != null) {
                        if (a.XmlElements.Count > 0) throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttribute));
                        if (a.XmlAttribute.Type != null) throw new InvalidOperationException(Res.GetString(Res.XmlIllegalType, "XmlAttribute"));
                        AttributeAccessor attribute = new AttributeAccessor();
                        attribute.Name = Accessor.EscapeQName(a.XmlAttribute.AttributeName.Length == 0 ? accessorName : a.XmlAttribute.AttributeName);
                        attribute.Namespace = a.XmlAttribute.Namespace == null ? ns : a.XmlAttribute.Namespace;
                        attribute.Form = a.XmlAttribute.Form;
                        if (attribute.Form == XmlSchemaForm.None && ns != attribute.Namespace) {
                            attribute.Form = XmlSchemaForm.Qualified;
                        }
                        attribute.CheckSpecial();
                        CheckForm(attribute.Form, ns != attribute.Namespace);
                        attribute.Mapping = ImportTypeMapping(modelScope.GetTypeModel(accessorType), ns, ImportContext.Attribute, a.XmlAttribute.DataType, null, limiter);
                        attribute.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        attribute.Any = a.XmlAnyAttribute != null;
                        if (attribute.Form == XmlSchemaForm.Qualified && attribute.Namespace != ns) {
                            if (xsdAttributes == null)
                                xsdAttributes = new NameTable();
                            attribute = (AttributeAccessor)ReconcileAccessor(attribute, xsdAttributes);
                        }
                        accessor.Attribute = attribute;
                    }
                    else {
                        if (a.XmlText != null) {
                            if (a.XmlText.Type != null && a.XmlText.Type != accessorType) 
                                throw new InvalidOperationException(Res.GetString(Res.XmlIllegalType, "XmlText"));
                            TextAccessor text = new TextAccessor();
                            text.Name = accessorName; // unused except to make more helpful error messages
                            text.Mapping = ImportTypeMapping(modelScope.GetTypeModel(accessorType), ns, ImportContext.Text, a.XmlText.DataType, null, limiter);
                            accessor.Text = text;
                        }
                        else if (a.XmlElements.Count == 0) {
                            a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                        }

                        for (int i = 0; i < a.XmlElements.Count; i++) {
                            XmlElementAttribute xmlElement = a.XmlElements[i];
                            if (xmlElement.Type != null) {
                                if (typeScope.GetTypeDesc(xmlElement.Type) != accessor.TypeDesc)
                                    throw new InvalidOperationException(Res.GetString(Res.XmlIllegalType, "XmlElement"));
                            }
                            ElementAccessor element = new ElementAccessor();
                            element.Name = XmlConvert.EncodeLocalName(xmlElement.ElementName.Length == 0 ? accessorName : xmlElement.ElementName);
                            element.Namespace = rpc ? null : xmlElement.Namespace == null ? ns : xmlElement.Namespace;
                            TypeModel typeModel = modelScope.GetTypeModel(accessorType);
                            element.Mapping = ImportTypeMapping(typeModel, rpc ? ns : element.Namespace, ImportContext.Element, xmlElement.DataType, null, limiter);
                            if (element.Mapping.TypeDesc.Kind == TypeKind.Node) {
                                element.Any = true;
                            }
                            element.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                            if (xmlElement.IsNullableSpecified && !xmlElement.IsNullable && typeModel.TypeDesc.IsOptionalValue)
                                //XmlInvalidNotNullable=IsNullable may not be set to 'false' for a Nullable<{0}> type. Consider using '{0}' type or removing the IsNullable property from the XmlElement attribute.
                                throw new InvalidOperationException(Res.GetString(Res.XmlInvalidNotNullable, typeModel.TypeDesc.BaseTypeDesc.FullName, "XmlElement"));
                            element.IsNullable = xmlElement.IsNullableSpecified ? xmlElement.IsNullable : typeModel.TypeDesc.IsOptionalValue;
                            element.Form = rpc ? XmlSchemaForm.Unqualified : xmlElement.Form == XmlSchemaForm.None ? elementFormDefault : xmlElement.Form;

                            CheckNullable(element.IsNullable, accessor.TypeDesc, element.Mapping);
                            if (!rpc) {
                                CheckForm(element.Form, ns != element.Namespace);
                                element = ReconcileLocalAccessor(element, ns);
                            }
                            if (xmlElement.Order != -1) {
                                if (xmlElement.Order != sequenceId &&  sequenceId != -1)
                                    throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMatch, "Order"));
                                sequenceId = xmlElement.Order;
                            }
                            AddUniqueAccessor(elements, element);
                            elementList.Add(element);
                        }
                    }
                }
                else if (a.Xmlns) {
                    if (flags != XmlAttributeFlags.XmlnsDeclarations)
                        throw new InvalidOperationException(Res.GetString(Res.XmlSoleXmlnsAttribute));
                    
                    if (accessorType != typeof(XmlSerializerNamespaces)) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlXmlnsInvalidType, accessorName, accessorType.FullName, typeof(XmlSerializerNamespaces).FullName));
                    }
                    accessor.Xmlns = new XmlnsAccessor();
                    accessor.Ignore = true;
                }
                else  {
                    if (a.XmlAttribute != null || a.XmlText != null) {
                        if (accessor.TypeDesc.Kind == TypeKind.Serializable) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttrOrTextInterface, accessorName, accessor.TypeDesc.FullName, typeof(IXmlSerializable).Name));
                        }
                        else {
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAttrOrText, accessorName, accessor.TypeDesc));
                        }
                    }
                    if (a.XmlElements.Count == 0 && a.XmlAnyElements.Count == 0)
                        a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                    for (int i = 0; i < a.XmlElements.Count; i++) {
                        XmlElementAttribute xmlElement = a.XmlElements[i];
                        Type targetType = xmlElement.Type == null ? accessorType : xmlElement.Type;
                        TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                        ElementAccessor element = new ElementAccessor();
                        TypeModel typeModel = modelScope.GetTypeModel(targetType);
                        element.Namespace = rpc ? null : xmlElement.Namespace == null ? ns : xmlElement.Namespace;
                        element.Mapping = ImportTypeMapping(typeModel, rpc ? ns : element.Namespace, ImportContext.Element, xmlElement.DataType, null, false, openModel, limiter);
                        if (a.XmlElements.Count == 1) {
                            element.Name = XmlConvert.EncodeLocalName(xmlElement.ElementName.Length == 0 ? accessorName : xmlElement.ElementName);
                        }
                        else {
                            element.Name = xmlElement.ElementName.Length == 0 ? element.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(xmlElement.ElementName);
                        }
                        element.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        if (xmlElement.IsNullableSpecified && !xmlElement.IsNullable && typeModel.TypeDesc.IsOptionalValue)
                            //XmlInvalidNotNullable=IsNullable may not be set to 'false' for a Nullable<{0}> type. Consider using '{0}' type or removing the IsNullable property from the XmlElement attribute.
                            throw new InvalidOperationException(Res.GetString(Res.XmlInvalidNotNullable, typeModel.TypeDesc.BaseTypeDesc.FullName, "XmlElement"));
                        element.IsNullable = xmlElement.IsNullableSpecified ? xmlElement.IsNullable : typeModel.TypeDesc.IsOptionalValue;
                        element.Form = rpc ? XmlSchemaForm.Unqualified : xmlElement.Form == XmlSchemaForm.None ? elementFormDefault : xmlElement.Form;
                        CheckNullable(element.IsNullable, targetTypeDesc, element.Mapping);

                        if (!rpc) {
                            CheckForm(element.Form, ns != element.Namespace);
                            element = ReconcileLocalAccessor(element, ns);
                        }
                        if (xmlElement.Order != -1) {
                            if (xmlElement.Order != sequenceId &&  sequenceId != -1)
                                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMatch, "Order"));
                            sequenceId = xmlElement.Order;
                        }
                        AddUniqueAccessor(elements, element);
                        elementList.Add(element);
                    }
                    NameTable anys = new NameTable();
                    for (int i = 0; i < a.XmlAnyElements.Count; i++)
                    {
                        XmlAnyElementAttribute xmlAnyElement = a.XmlAnyElements[i];
                        Type targetType = typeof(IXmlSerializable).IsAssignableFrom(accessorType) ? accessorType : typeof(XmlNode).IsAssignableFrom(accessorType) ? accessorType : typeof(XmlElement);
                        if (!accessorType.IsAssignableFrom(targetType))
                            throw new InvalidOperationException(Res.GetString(Res.XmlIllegalAnyElement, accessorType.FullName));

                        string anyName = xmlAnyElement.Name.Length == 0 ? xmlAnyElement.Name : XmlConvert.EncodeLocalName(xmlAnyElement.Name);
                        string anyNs = xmlAnyElement.NamespaceSpecified ? xmlAnyElement.Namespace : null;
                        if (anys[anyName, anyNs] != null)
                        {
                            // ignore duplicate anys
                            continue;
                        }
                        anys[anyName, anyNs] = xmlAnyElement;
                        if (elements[anyName, (anyNs == null ? ns : anyNs)] != null)
                        {
                            throw new InvalidOperationException(Res.GetString(Res.XmlAnyElementDuplicate, accessorName, xmlAnyElement.Name, xmlAnyElement.Namespace == null ? "null" : xmlAnyElement.Namespace));
                        }
                        ElementAccessor element = new ElementAccessor();
                        element.Name = anyName;
                        element.Namespace = anyNs == null ? ns : anyNs;
                        element.Any = true;
                        element.AnyNamespaces = anyNs;
                        TypeDesc targetTypeDesc = typeScope.GetTypeDesc(targetType);
                        TypeModel typeModel = modelScope.GetTypeModel(targetType);

                        if (element.Name.Length > 0)
                            typeModel.TypeDesc.IsMixed = true;
                        element.Mapping = ImportTypeMapping(typeModel, element.Namespace, ImportContext.Element, String.Empty, null, false, openModel, limiter);
                        element.Default = GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        element.IsNullable = false;
                        element.Form = elementFormDefault;
                        CheckNullable(element.IsNullable, targetTypeDesc, element.Mapping);
                        if (!rpc) {
                            CheckForm(element.Form, ns != element.Namespace);
                            element = ReconcileLocalAccessor(element, ns);
                        }
                        if (xmlAnyElement.Order != -1) {
                            if (xmlAnyElement.Order != sequenceId &&  sequenceId != -1)
                                throw new InvalidOperationException(Res.GetString(Res.XmlSequenceMatch, "Order"));
                            sequenceId = xmlAnyElement.Order;
                        }
                        elements.Add(element.Name, element.Namespace, element);
                        elementList.Add(element);
                    }
                }
            }
            accessor.Elements = (ElementAccessor[])elementList.ToArray(typeof(ElementAccessor));
            accessor.SequenceId = sequenceId;
            
            if (rpc)
            {
                if (accessor.TypeDesc.IsArrayLike && accessor.Elements.Length > 0 && !(accessor.Elements[0].Mapping is ArrayMapping))
                    throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitArrayElement, accessor.Elements[0].Name));

                if (accessor.Xmlns != null)
                    throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitXmlns, accessor.Name));
            }

            if (accessor.ChoiceIdentifier != null) {
                // find the enum value corresponding to each element
                accessor.ChoiceIdentifier.MemberIds = new string[accessor.Elements.Length];
                for (int i = 0; i < accessor.Elements.Length; i++) {
                    bool found = false;
                    ElementAccessor element = accessor.Elements[i];
                    EnumMapping choiceMapping = (EnumMapping)accessor.ChoiceIdentifier.Mapping;
                    for (int j = 0; j < choiceMapping.Constants.Length; j++) {
                        string xmlName = choiceMapping.Constants[j].XmlName;

                        if (element.Any && element.Name.Length == 0) {
                            string anyNs = element.AnyNamespaces == null ? "##any" : element.AnyNamespaces;
                            if (xmlName.Substring(0, xmlName.Length-1) == anyNs) {
                                accessor.ChoiceIdentifier.MemberIds[i] = choiceMapping.Constants[j].Name;
                                found = true;
                                break;
                            }
                            continue;
                        }
                        int colon = xmlName.LastIndexOf(':');
                        string choiceNs = colon < 0 ? choiceMapping.Namespace : xmlName.Substring(0, colon);
                        string choiceName = colon < 0 ? xmlName : xmlName.Substring(colon+1);

                        if (element.Name == choiceName) {
                            if ((element.Form == XmlSchemaForm.Unqualified && string.IsNullOrEmpty(choiceNs)) || element.Namespace == choiceNs) {
                                accessor.ChoiceIdentifier.MemberIds[i] = choiceMapping.Constants[j].Name;
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found) {
                        if (element.Any && element.Name.Length == 0) {
                            // Type {0} is missing enumeration value '##any' for XmlAnyElementAttribute.
                            throw new InvalidOperationException(Res.GetString(Res.XmlChoiceMissingAnyValue, accessor.ChoiceIdentifier.Mapping.TypeDesc.FullName));
                        }
                        else {
                            string id = element.Namespace != null && element.Namespace.Length > 0 ? element.Namespace + ":" + element.Name : element.Name;
                            // Type {0} is missing value for '{1}'.
                            throw new InvalidOperationException(Res.GetString(Res.XmlChoiceMissingValue, accessor.ChoiceIdentifier.Mapping.TypeDesc.FullName, id, element.Name, element.Namespace));
                        }
                    }
                }
            }
            arrayNestingLevel = previousNestingLevel;
            savedArrayItemAttributes = previousArrayItemAttributes;
            savedArrayNamespace = previousArrayNamespace;
        }

        
        void CheckTopLevelAttributes(XmlAttributes a, string accessorName) {
            XmlAttributeFlags flags = a.XmlFlags;

            if ((flags & (XmlAttributeFlags.Attribute | XmlAttributeFlags.AnyAttribute)) != 0) 
                throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitAttributeAttributes));

            if ((flags & (XmlAttributeFlags.Text | XmlAttributeFlags.AnyElements | XmlAttributeFlags.ChoiceIdentifier)) != 0) 
                throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitAttributes));

            if (a.XmlElements != null && a.XmlElements.Count > 0) {
                if (a.XmlElements.Count > 1) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitElements));
                }
                XmlElementAttribute xmlElement = a.XmlElements[0];
                if (xmlElement.Namespace != null) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitElementNamespace, "Namespace", xmlElement.Namespace));
                }
                if (xmlElement.IsNullable) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitElementNullable, "IsNullable", "true"));
                }
            }
            if (a.XmlArray != null && a.XmlArray.Namespace != null) {
                throw new InvalidOperationException(Res.GetString(Res.XmlRpcLitElementNamespace, "Namespace", a.XmlArray.Namespace));
            }
        }

        void CheckAmbiguousChoice(XmlAttributes a, Type accessorType, string accessorName) {
            Hashtable choiceTypes = new Hashtable();

            XmlElementAttributes elements = a.XmlElements;
            if (elements != null && elements.Count >= 2 && a.XmlChoiceIdentifier == null) {
                for (int i = 0; i < elements.Count; i++) {
                    Type type = elements[i].Type == null ? accessorType : elements[i].Type;
                    if (choiceTypes.Contains(type)) {
                        // You need to add {0} to the '{1}'.
                        throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferMissing, typeof(XmlChoiceIdentifierAttribute).Name, accessorName));
                    }
                    else {
                        choiceTypes.Add(type, false);
                    }
                }
            }
            if (choiceTypes.Contains(typeof(XmlElement)) && a.XmlAnyElements.Count > 0) {
                // You need to add {0} to the '{1}'.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentiferMissing, typeof(XmlChoiceIdentifierAttribute).Name, accessorName));
            }

            XmlArrayItemAttributes items = a.XmlArrayItems;
            if (items != null && items.Count >= 2) {
                NameTable arrayTypes = new NameTable();

                for (int i = 0; i < items.Count; i++) {
                    Type type = items[i].Type == null ? accessorType : items[i].Type;
                    string ns = items[i].NestingLevel.ToString(CultureInfo.InvariantCulture);
                    XmlArrayItemAttribute item = (XmlArrayItemAttribute)arrayTypes[type.FullName, ns];
                    if (item != null) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlArrayItemAmbiguousTypes, accessorName, item.ElementName, items[i].ElementName, typeof(XmlElementAttribute).Name, typeof(XmlChoiceIdentifierAttribute).Name, accessorName));
                    }
                    else {
                        arrayTypes[type.FullName, ns] =  items[i];
                    }
                }
            }
        }

        void CheckChoiceIdentifierMapping(EnumMapping choiceMapping) {
            NameTable ids = new NameTable();
            for (int i = 0; i < choiceMapping.Constants.Length; i++) {
                string choiceId = choiceMapping.Constants[i].XmlName;
                int colon = choiceId.LastIndexOf(':');
                string choiceName = colon < 0 ? choiceId : choiceId.Substring(colon+1);
                string choiceNs = colon < 0 ? "" : choiceId.Substring(0, colon);

                if (ids[choiceName, choiceNs] != null) {
                    // Enum values in the XmlChoiceIdentifier '{0}' have to be unique.  Value '{1}' already present.
                    throw new InvalidOperationException(Res.GetString(Res.XmlChoiceIdDuplicate, choiceMapping.TypeName, choiceId));
                }
                ids.Add(choiceName, choiceNs, choiceMapping.Constants[i]);
            }
        }

        object GetDefaultValue(TypeDesc fieldTypeDesc, Type t, XmlAttributes a) {
            if (a.XmlDefaultValue == null || a.XmlDefaultValue == DBNull.Value) return null;
            if (!(fieldTypeDesc.Kind == TypeKind.Primitive || fieldTypeDesc.Kind == TypeKind.Enum))  {
                //throw new InvalidOperationException(Res.GetString(Res.XmlIllegalDefault));
                a.XmlDefaultValue = null;
                return a.XmlDefaultValue;
            }
            // for enums validate and return a string representation
            if (fieldTypeDesc.Kind == TypeKind.Enum) {
                string strValue = Enum.Format(t, a.XmlDefaultValue, "G").Replace(",", " ");
                string numValue = Enum.Format(t, a.XmlDefaultValue, "D");
                if (strValue == numValue) // means enum value wasn't recognized
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidDefaultValue, strValue, a.XmlDefaultValue.GetType().FullName));
                return strValue;
            }
            return a.XmlDefaultValue;
        }

        static XmlArrayItemAttribute CreateArrayItemAttribute(TypeDesc typeDesc, int nestingLevel) {
            XmlArrayItemAttribute xmlArrayItem = new XmlArrayItemAttribute();
            xmlArrayItem.NestingLevel = nestingLevel;
            return xmlArrayItem;
        }

        static XmlArrayAttribute CreateArrayAttribute(TypeDesc typeDesc) {
            XmlArrayAttribute xmlArrayItem = new XmlArrayAttribute();
            return xmlArrayItem;
        }

        static XmlElementAttribute CreateElementAttribute(TypeDesc typeDesc) {
            XmlElementAttribute xmlElement = new XmlElementAttribute();
            xmlElement.IsNullable = typeDesc.IsOptionalValue;
            return xmlElement;
        }

        static void AddUniqueAccessor(INameScope scope, Accessor accessor) {
            Accessor existing = (Accessor)scope[accessor.Name, accessor.Namespace];
            if (existing != null) {
                if (accessor is ElementAccessor) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlDuplicateElementName, existing.Name, existing.Namespace));
                }
                else {
                    #if DEBUG
                    if (!(accessor is AttributeAccessor))
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "Bad accessor type " + accessor.GetType().FullName));
                    #endif
                    throw new InvalidOperationException(Res.GetString(Res.XmlDuplicateAttributeName, existing.Name, existing.Namespace));
                }
            }
            else {
                scope[accessor.Name, accessor.Namespace] = accessor;
            }
        }

        static void AddUniqueAccessor(MemberMapping member, INameScope elements, INameScope attributes, bool isSequence) {
            if (member.Attribute != null) {
                AddUniqueAccessor(attributes, member.Attribute);
            }
            else if (!isSequence && member.Elements != null && member.Elements.Length > 0) {
                for (int i = 0; i < member.Elements.Length; i++) {
                    AddUniqueAccessor(elements, member.Elements[i]);
                }
            }
        }

        static void CheckForm(XmlSchemaForm form, bool isQualified) {
            if (isQualified && form == XmlSchemaForm.Unqualified) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidFormUnqualified));
        }

        static void CheckNullable(bool isNullable, TypeDesc typeDesc, TypeMapping mapping) {
            if (mapping is NullableMapping) return;
            if (mapping is SerializableMapping) return;
            if (isNullable && !typeDesc.IsNullable) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidIsNullable, typeDesc.FullName));
        }

        static ElementAccessor CreateElementAccessor(TypeMapping mapping, string ns) {
            ElementAccessor element = new ElementAccessor();
            bool isAny = mapping.TypeDesc.Kind == TypeKind.Node;
            if (!isAny && mapping is SerializableMapping) {
                isAny = ((SerializableMapping)mapping).IsAny;
            }
            if (isAny) {
                element.Any = true;
            }
            else {
                element.Name = mapping.DefaultElementName;
                element.Namespace = ns;
            }
            element.Mapping = mapping;
            return element;
        }

        // will create a shallow type mapping for a top-level type
        internal static XmlTypeMapping GetTopLevelMapping(Type type, string defaultNamespace) {
            XmlAttributes a = new XmlAttributes(type);
            TypeDesc typeDesc = new TypeScope().GetTypeDesc(type);
            ElementAccessor element = new ElementAccessor();

            if (typeDesc.Kind == TypeKind.Node) {
                element.Any = true;
            }
            else {
                string ns = a.XmlRoot == null ? defaultNamespace : a.XmlRoot.Namespace;
                string typeName = string.Empty;
                if (a.XmlType != null)
                    typeName = a.XmlType.TypeName;
                if (typeName.Length == 0) 
                    typeName = type.Name;

                element.Name = XmlConvert.EncodeLocalName(typeName);
                element.Namespace = ns;
            }
            XmlTypeMapping mapping = new XmlTypeMapping(null, element);
            mapping.SetKeyInternal(XmlMapping.GenerateKey(type, a.XmlRoot, defaultNamespace));
            return mapping;
        }
    }
    internal class ImportStructWorkItem {
        StructModel model;
        StructMapping mapping;

        internal ImportStructWorkItem(StructModel model, StructMapping mapping) {
            this.model = model;
            this.mapping = mapping;
        }

        internal StructModel Model { get { return model; } }
        internal StructMapping Mapping { get { return mapping; } }
    }

    internal class WorkItems {
        ArrayList list = new ArrayList();

        internal ImportStructWorkItem this[int index] {
            get {
                return (ImportStructWorkItem)list[index];
            }
            set {
                list[index] = value;
            }
        }

        internal int Count {
            get {
                return list.Count;
            }
        }

        internal void Add(ImportStructWorkItem item) {
            list.Add(item);
        }

        internal bool Contains(StructMapping mapping) {
            return IndexOf(mapping) >= 0;
        }

        internal int IndexOf(StructMapping mapping) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Mapping == mapping)
                    return i;
            }
            return -1;
        }

        internal void RemoveAt(int index) {
            list.RemoveAt(index);
        }
    }

    internal class RecursionLimiter {
        int maxDepth;
        int depth;
        WorkItems deferredWorkItems;

        internal RecursionLimiter() {
            this.depth = 0;
            this.maxDepth = DiagnosticsSwitches.NonRecursiveTypeLoading.Enabled ? 1 : int.MaxValue;
        }

        internal bool IsExceededLimit { get { return this.depth > this.maxDepth; } }
        internal int Depth { get { return this.depth; } set { this.depth = value; } }

        internal WorkItems DeferredWorkItems {
            get {
                if (deferredWorkItems == null) {
                    deferredWorkItems = new WorkItems();
                }
                return deferredWorkItems;
            }
        }

    }



}
