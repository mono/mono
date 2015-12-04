//------------------------------------------------------------------------------
// <copyright file="Types.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    using System.IO; 
    using System.Reflection; 
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using System.Xml;
    using System.Text;
    using System.ComponentModel;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml.Serialization.Advanced;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Diagnostics;

    // These classes provide a higher level view on reflection specific to 
    // Xml serialization, for example:
    // - allowing one to talk about types w/o having them compiled yet
    // - abstracting collections & arrays
    // - abstracting classes, structs, interfaces
    // - knowing about XSD primitives
    // - dealing with Serializable and xmlelement
    // and lots of other little details

    internal enum TypeKind {
        Root,
        Primitive,
        Enum,
        Struct,
        Class,
        Array,
        Collection,
        Enumerable,
        Void,
        Node,
        Attribute,
        Serializable
    }

    internal enum TypeFlags {
        None = 0x0,
        Abstract = 0x1,
        Reference = 0x2,
        Special = 0x4,
        CanBeAttributeValue = 0x8,
        CanBeTextValue = 0x10,
        CanBeElementValue = 0x20,
        HasCustomFormatter = 0x40,
        AmbiguousDataType = 0x80,
        IgnoreDefault = 0x200,
        HasIsEmpty = 0x400,
        HasDefaultConstructor = 0x800,
        XmlEncodingNotRequired = 0x1000,
        UseReflection = 0x4000,
        CollapseWhitespace = 0x8000,
        OptionalValue = 0x10000,
        CtorInaccessible = 0x20000,
        UsePrivateImplementation = 0x40000,
        GenericInterface = 0x80000,
        Unsupported = 0x100000,
    }

    internal class TypeDesc {
        string name;
        string fullName;
        string cSharpName;
        TypeDesc arrayElementTypeDesc;
        TypeDesc arrayTypeDesc;
        TypeDesc nullableTypeDesc;
        TypeKind kind;
        XmlSchemaType dataType;
        Type type;
        TypeDesc baseTypeDesc;
        TypeFlags flags;
        string formatterName;
        bool isXsdType;
        bool isMixed;
        MappedTypeDesc extendedType;
        int weight;
        Exception exception;

        internal TypeDesc(string name, string fullName, XmlSchemaType dataType, TypeKind kind, TypeDesc baseTypeDesc, TypeFlags flags, string formatterName) {
            this.name = name.Replace('+', '.');
            this.fullName = fullName.Replace('+', '.');
            this.kind = kind;
            this.baseTypeDesc = baseTypeDesc;
            this.flags = flags;
            this.isXsdType = kind == TypeKind.Primitive;
            if (this.isXsdType)
                this.weight = 1;
            else if (kind == TypeKind.Enum)
                this.weight = 2;
            else if (this.kind == TypeKind.Root)
                this.weight = -1;
            else
                this.weight = baseTypeDesc == null ? 0 : baseTypeDesc.Weight + 1;
            this.dataType = dataType;
            this.formatterName = formatterName;
        }

        internal TypeDesc(string name, string fullName, XmlSchemaType dataType, TypeKind kind, TypeDesc baseTypeDesc, TypeFlags flags)
            : this(name, fullName, dataType, kind, baseTypeDesc, flags, null) { }

        internal TypeDesc(string name, string fullName, TypeKind kind, TypeDesc baseTypeDesc, TypeFlags flags)
            : this(name, fullName, (XmlSchemaType)null, kind, baseTypeDesc, flags, null) { }

        internal TypeDesc(Type type, bool isXsdType, XmlSchemaType dataType, string formatterName, TypeFlags flags)
            : this(type.Name, type.FullName, dataType, TypeKind.Primitive, (TypeDesc)null, flags, formatterName) {
            this.isXsdType = isXsdType;
            this.type = type;
        }
        internal TypeDesc(Type type, string name, string fullName, TypeKind kind, TypeDesc baseTypeDesc, TypeFlags flags, TypeDesc arrayElementTypeDesc)
            : this(name, fullName, null, kind, baseTypeDesc, flags, null) {

            this.arrayElementTypeDesc = arrayElementTypeDesc;
            this.type = type;
        }

        public override string ToString() {
            return fullName;
        }

        internal TypeFlags Flags {
            get { return flags; }
        }

        internal bool IsXsdType {
            get { return isXsdType; }
        }

        internal bool IsMappedType {
            get { return extendedType != null; }
        }

        internal MappedTypeDesc ExtendedType {
            get { return extendedType; }
        }

        internal string Name {
            get { return name; }
        }

        internal string FullName {
            get { return fullName; }
        }

        internal string CSharpName {
            get {
                if (cSharpName == null) {
                    cSharpName = type == null ? CodeIdentifier.GetCSharpName(fullName) : CodeIdentifier.GetCSharpName(type);
                }
                return cSharpName;
            }
        }

        internal XmlSchemaType DataType {
            get { return dataType; }
        }

        internal Type Type {
            get { return type; }
        }

        internal string FormatterName {
            get { return formatterName; }
        }

        internal TypeKind Kind {
            get { return kind; }
        }

        internal bool IsValueType {
            get { return (flags & TypeFlags.Reference) == 0; }
        }

        internal bool CanBeAttributeValue {
            get { return (flags & TypeFlags.CanBeAttributeValue) != 0; }
        }

        internal bool XmlEncodingNotRequired {
            get { return (flags & TypeFlags.XmlEncodingNotRequired) != 0; }
        }

        internal bool CanBeElementValue {
            get { return (flags & TypeFlags.CanBeElementValue) != 0; }
        }

        internal bool CanBeTextValue {
            get { return (flags & TypeFlags.CanBeTextValue) != 0; }
        }

        internal bool IsMixed {
            get { return isMixed || CanBeTextValue; }
            set { isMixed = value; }
        }

        internal bool IsSpecial {
            get { return (flags & TypeFlags.Special) != 0; }
        }

        internal bool IsAmbiguousDataType {
            get { return (flags & TypeFlags.AmbiguousDataType) != 0; }
        }

        internal bool HasCustomFormatter {
            get { return (flags & TypeFlags.HasCustomFormatter) != 0; }
        }

        internal bool HasDefaultSupport {
            get { return (flags & TypeFlags.IgnoreDefault) == 0; }
        }

        internal bool HasIsEmpty {
            get { return (flags & TypeFlags.HasIsEmpty) != 0; }
        }

        internal bool CollapseWhitespace {
            get { return (flags & TypeFlags.CollapseWhitespace) != 0; }
        }

        internal bool HasDefaultConstructor {
            get { return (flags & TypeFlags.HasDefaultConstructor) != 0; }
        }

        internal bool IsUnsupported {
            get { return (flags & TypeFlags.Unsupported) != 0; }
        }

        internal bool IsGenericInterface {
            get { return (flags & TypeFlags.GenericInterface) != 0; }
        }

        internal bool IsPrivateImplementation {
            get { return (flags & TypeFlags.UsePrivateImplementation) != 0; }
        }

        internal bool CannotNew {
            get { return !HasDefaultConstructor || ConstructorInaccessible; }
        }

        internal bool IsAbstract {
            get { return (flags & TypeFlags.Abstract) != 0; }
        }

        internal bool IsOptionalValue {
            get { return (flags & TypeFlags.OptionalValue) != 0; }
        }

        internal bool UseReflection {
            get { return (flags & TypeFlags.UseReflection) != 0; }
        }

        internal bool IsVoid {
            get { return kind == TypeKind.Void; }
        }

        internal bool IsClass {
            get { return kind == TypeKind.Class; }
        }

        internal bool IsStructLike {
            get { return kind == TypeKind.Struct || kind == TypeKind.Class; }
        }

        internal bool IsArrayLike {
            get { return kind == TypeKind.Array || kind == TypeKind.Collection || kind == TypeKind.Enumerable; }
        }

        internal bool IsCollection {
            get { return kind == TypeKind.Collection; }
        }

        internal bool IsEnumerable {
            get { return kind == TypeKind.Enumerable; }
        }

        internal bool IsArray {
            get { return kind == TypeKind.Array; }
        }

        internal bool IsPrimitive {
            get { return kind == TypeKind.Primitive; }
        }

        internal bool IsEnum {
            get { return kind == TypeKind.Enum; }
        }

        internal bool IsNullable {
            get { return !IsValueType; }
        }

        internal bool IsRoot {
            get { return kind == TypeKind.Root; }
        }

        internal bool ConstructorInaccessible {
            get { return (flags & TypeFlags.CtorInaccessible) != 0; }
        }

        internal Exception Exception {
            get { return exception; }
            set { exception = value; }
        }

        internal TypeDesc GetNullableTypeDesc(Type type) {
            if (IsOptionalValue)
                return this;

            if (nullableTypeDesc == null) {
                nullableTypeDesc = new TypeDesc("NullableOf" + this.name, "System.Nullable`1[" + this.fullName + "]", null, TypeKind.Struct, this, this.flags | TypeFlags.OptionalValue, this.formatterName);
                nullableTypeDesc.type = type;
            }

            return nullableTypeDesc;
        }
        internal void CheckSupported() {
            if (IsUnsupported) {
                if (Exception != null) {
                    throw Exception;
                }
                else {
                    throw new NotSupportedException(Res.GetString(Res.XmlSerializerUnsupportedType, FullName));
                }
            }
            if (baseTypeDesc != null)
                baseTypeDesc.CheckSupported();
            if (arrayElementTypeDesc != null)
                arrayElementTypeDesc.CheckSupported();
        }

        internal void CheckNeedConstructor() {
            if (!IsValueType && !IsAbstract && !HasDefaultConstructor) {
                flags |= TypeFlags.Unsupported;
                this.exception = new InvalidOperationException(Res.GetString(Res.XmlConstructorInaccessible, FullName));
            }
        }

        internal string ArrayLengthName {
            get { return kind == TypeKind.Array ? "Length" : "Count"; }
        }

        internal TypeDesc ArrayElementTypeDesc {
            get { return arrayElementTypeDesc; }
            set { arrayElementTypeDesc = value; }
        }

        internal int Weight {
            get { return weight; }
        }

        internal TypeDesc CreateArrayTypeDesc() {
            if (arrayTypeDesc == null)
                arrayTypeDesc = new TypeDesc(null, name + "[]", fullName + "[]", TypeKind.Array, null, TypeFlags.Reference | (flags & TypeFlags.UseReflection), this);
            return arrayTypeDesc;
        }

        internal TypeDesc CreateMappedTypeDesc(MappedTypeDesc extension) {
            TypeDesc newTypeDesc = new TypeDesc(extension.Name, extension.Name, null, this.kind, this.baseTypeDesc, this.flags, null);
            newTypeDesc.isXsdType = this.isXsdType;
            newTypeDesc.isMixed = this.isMixed;
            newTypeDesc.extendedType = extension;
            newTypeDesc.dataType = this.dataType;
            return newTypeDesc;
        }

        internal TypeDesc BaseTypeDesc {
            get { return baseTypeDesc; }
            set {
                baseTypeDesc = value;
                weight = baseTypeDesc == null ? 0 : baseTypeDesc.Weight + 1;
            }
        }

        internal bool IsDerivedFrom(TypeDesc baseTypeDesc) {
            TypeDesc typeDesc = this;
            while (typeDesc != null) {
                if (typeDesc == baseTypeDesc) return true;
                typeDesc = typeDesc.BaseTypeDesc;
            }
            return baseTypeDesc.IsRoot;
        }

        internal static TypeDesc FindCommonBaseTypeDesc(TypeDesc[] typeDescs) {
            if (typeDescs.Length == 0) return null;
            TypeDesc leastDerivedTypeDesc = null;
            int leastDerivedLevel = int.MaxValue;

            for (int i = 0; i < typeDescs.Length; i++) {
                int derivationLevel = typeDescs[i].Weight;
                if (derivationLevel < leastDerivedLevel) {
                    leastDerivedLevel = derivationLevel;
                    leastDerivedTypeDesc = typeDescs[i];
                }
            }
            while (leastDerivedTypeDesc != null) {
                int i;
                for (i = 0; i < typeDescs.Length; i++) {
                    if (!typeDescs[i].IsDerivedFrom(leastDerivedTypeDesc)) break;
                }
                if (i == typeDescs.Length) break;
                leastDerivedTypeDesc = leastDerivedTypeDesc.BaseTypeDesc;
            }
            return leastDerivedTypeDesc;
        }
    }

    internal class TypeScope {
        Hashtable typeDescs = new Hashtable();
        Hashtable arrayTypeDescs = new Hashtable();
        ArrayList typeMappings = new ArrayList();

        static Hashtable primitiveTypes = new Hashtable();
        static Hashtable primitiveDataTypes = new Hashtable();
        static NameTable primitiveNames = new NameTable();

        static string[] unsupportedTypes = new string[] {
            "anyURI",
            "duration",
            "ENTITY",
            "ENTITIES",
            "gDay",
            "gMonth",
            "gMonthDay",
            "gYear",
            "gYearMonth",
            "ID",
            "IDREF",
            "IDREFS",
            "integer",
            "language",
            "negativeInteger",
            "nonNegativeInteger",
            "nonPositiveInteger",
            //"normalizedString",
            "NOTATION",
            "positiveInteger",
            "token"
        };

        static TypeScope() {
            AddPrimitive(typeof(string), "string", "String", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            AddPrimitive(typeof(int), "int", "Int32", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(bool), "boolean", "Boolean", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(short), "short", "Int16", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(long), "long", "Int64", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(float), "float", "Single", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(double), "double", "Double", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(decimal), "decimal", "Decimal", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(DateTime), "dateTime", "DateTime", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(XmlQualifiedName), "QName", "XmlQualifiedName", TypeFlags.CanBeAttributeValue | TypeFlags.HasCustomFormatter | TypeFlags.HasIsEmpty | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired | TypeFlags.Reference);
            AddPrimitive(typeof(byte), "unsignedByte", "Byte", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(SByte), "byte", "SByte", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(UInt16), "unsignedShort", "UInt16", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(UInt32), "unsignedInt", "UInt32", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(UInt64), "unsignedLong", "UInt64", TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);

            // Types without direct mapping (ambigous)
            AddPrimitive(typeof(DateTime), "date", "Date", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(DateTime), "time", "Time", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);

            AddPrimitive(typeof(string), "Name", "XmlName", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NCName", "XmlNCName", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NMTOKEN", "XmlNmToken", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NMTOKENS", "XmlNmTokens", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);

            AddPrimitive(typeof(byte[]), "base64Binary", "ByteArrayBase64", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor);
            AddPrimitive(typeof(byte[]), "hexBinary", "ByteArrayHex", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor);
            // NOTE, [....]: byte[] can also be used to mean array of bytes. That datatype is not a primitive, so we
            // can't use the AmbiguousDataType mechanism. To get an array of bytes in literal XML, apply [XmlArray] or
            // [XmlArrayItem].

            XmlSchemaPatternFacet guidPattern = new XmlSchemaPatternFacet();
            guidPattern.Value = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

            AddNonXsdPrimitive(typeof(Guid), "guid", UrtTypes.Namespace, "Guid", new XmlQualifiedName("string", XmlSchema.Namespace), new XmlSchemaFacet[] { guidPattern }, TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired | TypeFlags.IgnoreDefault);
            AddNonXsdPrimitive(typeof(char), "char", UrtTypes.Namespace, "Char", new XmlQualifiedName("unsignedShort", XmlSchema.Namespace), new XmlSchemaFacet[0], TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.IgnoreDefault);

            AddSoapEncodedTypes(Soap.Encoding);

            // Unsuppoted types that we map to string, if in the future we decide 
            // to add support for them we would need to create custom formatters for them
            // normalizedString is the only one unsuported type that suppose to preserve whitesapce
            AddPrimitive(typeof(string), "normalizedString", "String", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            for (int i = 0; i < unsupportedTypes.Length; i++) {
                AddPrimitive(typeof(string), unsupportedTypes[i], "String", TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.CollapseWhitespace);
            }
        }

        internal static bool IsKnownType(Type type) {
            if (type == typeof(object))
                return true;
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.String: return true;
                case TypeCode.Int32: return true;
                case TypeCode.Boolean: return true;
                case TypeCode.Int16: return true;
                case TypeCode.Int64: return true;
                case TypeCode.Single: return true;
                case TypeCode.Double: return true;
                case TypeCode.Decimal: return true;
                case TypeCode.DateTime: return true;
                case TypeCode.Byte: return true;
                case TypeCode.SByte: return true;
                case TypeCode.UInt16: return true;
                case TypeCode.UInt32: return true;
                case TypeCode.UInt64: return true;
                case TypeCode.Char: return true;
                default:
                    if (type == typeof(XmlQualifiedName))
                        return true;
                    else if (type == typeof(byte[]))
                        return true;
                    else if (type == typeof(Guid))
                        return true;
                    else if (type == typeof(XmlNode[])) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        static void AddSoapEncodedTypes(string ns) {
            AddSoapEncodedPrimitive(typeof(string), "normalizedString", ns, "String", new XmlQualifiedName("normalizedString", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            for (int i = 0; i < unsupportedTypes.Length; i++) {
                AddSoapEncodedPrimitive(typeof(string), unsupportedTypes[i], ns, "String", new XmlQualifiedName(unsupportedTypes[i], XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.Reference | TypeFlags.CollapseWhitespace);
            }

            AddSoapEncodedPrimitive(typeof(string), "string", ns, "String", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(int), "int", ns, "Int32", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(bool), "boolean", ns, "Boolean", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(short), "short", ns, "Int16", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(long), "long", ns, "Int64", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(float), "float", ns, "Single", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(double), "double", ns, "Double", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(decimal), "decimal", ns, "Decimal", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(DateTime), "dateTime", ns, "DateTime", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(XmlQualifiedName), "QName", ns, "XmlQualifiedName", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.HasCustomFormatter | TypeFlags.HasIsEmpty | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(byte), "unsignedByte", ns, "Byte", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(SByte), "byte", ns, "SByte", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(UInt16), "unsignedShort", ns, "UInt16", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(UInt32), "unsignedInt", ns, "UInt32", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(UInt64), "unsignedLong", ns, "UInt64", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired);

            // Types without direct mapping (ambigous)
            AddSoapEncodedPrimitive(typeof(DateTime), "date", ns, "Date", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(DateTime), "time", ns, "Time", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);

            AddSoapEncodedPrimitive(typeof(string), "Name", ns, "XmlName", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NCName", ns, "XmlNCName", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NMTOKEN", ns, "XmlNmToken", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NMTOKENS", ns, "XmlNmTokens", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference);

            AddSoapEncodedPrimitive(typeof(byte[]), "base64Binary", ns, "ByteArrayBase64", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(byte[]), "hexBinary", ns, "ByteArrayHex", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.AmbiguousDataType | TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired);

            AddSoapEncodedPrimitive(typeof(string), "arrayCoordinate", ns, "String", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue);
            AddSoapEncodedPrimitive(typeof(byte[]), "base64", ns, "ByteArrayBase64", new XmlQualifiedName("base64Binary", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.IgnoreDefault | TypeFlags.Reference);
        }

        static void AddPrimitive(Type type, string dataTypeName, string formatterName, TypeFlags flags) {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType();
            dataType.Name = dataTypeName;
            TypeDesc typeDesc = new TypeDesc(type, true, dataType, formatterName, flags);
            if (primitiveTypes[type] == null)
                primitiveTypes.Add(type, typeDesc);
            primitiveDataTypes.Add(dataType, typeDesc);
            primitiveNames.Add(dataTypeName, XmlSchema.Namespace, typeDesc);
        }

        static void AddNonXsdPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, XmlSchemaFacet[] facets, TypeFlags flags) {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType();
            dataType.Name = dataTypeName;
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
            restriction.BaseTypeName = baseTypeName;
            foreach (XmlSchemaFacet facet in facets) {
                restriction.Facets.Add(facet);
            }
            dataType.Content = restriction;
            TypeDesc typeDesc = new TypeDesc(type, false, dataType, formatterName, flags);
            if (primitiveTypes[type] == null)
                primitiveTypes.Add(type, typeDesc);
            primitiveDataTypes.Add(dataType, typeDesc);
            primitiveNames.Add(dataTypeName, ns, typeDesc);
        }

        static void AddSoapEncodedPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, TypeFlags flags) {
            AddNonXsdPrimitive(type, dataTypeName, ns, formatterName, baseTypeName, new XmlSchemaFacet[0], flags);
        }

        internal TypeDesc GetTypeDesc(string name, string ns) {
            return GetTypeDesc(name, ns, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue);
        }

        internal TypeDesc GetTypeDesc(string name, string ns, TypeFlags flags) {
            TypeDesc typeDesc = (TypeDesc)primitiveNames[name, ns];
            if (typeDesc != null) {
                if ((typeDesc.Flags & flags) != 0) {
                    return typeDesc;
                }
            }
            return null;
        }

        internal TypeDesc GetTypeDesc(XmlSchemaSimpleType dataType) {
            return (TypeDesc)primitiveDataTypes[dataType];
        }

        internal TypeDesc GetTypeDesc(Type type) {
            return GetTypeDesc(type, null, true, true);
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source) {
            return GetTypeDesc(type, source, true, true);
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source, bool directReference) {
            return GetTypeDesc(type, source, directReference, true);
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source, bool directReference, bool throwOnError) {
            if (type.ContainsGenericParameters) {
                throw new InvalidOperationException(Res.GetString(Res.XmlUnsupportedOpenGenericType, type.ToString()));
            }
            TypeDesc typeDesc = (TypeDesc)primitiveTypes[type];
            if (typeDesc == null) {
                typeDesc = (TypeDesc)typeDescs[type];
                if (typeDesc == null) {
                    typeDesc = ImportTypeDesc(type, source, directReference);
                }
            }
            if (throwOnError)
                typeDesc.CheckSupported();


            return typeDesc;
        }

        internal TypeDesc GetArrayTypeDesc(Type type) {
            TypeDesc typeDesc = (TypeDesc)arrayTypeDescs[type];
            if (typeDesc == null) {
                typeDesc = GetTypeDesc(type);
                if (!typeDesc.IsArrayLike)
                    typeDesc = ImportTypeDesc(type, null, false);
                typeDesc.CheckSupported();
                arrayTypeDescs.Add(type, typeDesc);
            }
            return typeDesc;
        }

        internal TypeMapping GetTypeMappingFromTypeDesc(TypeDesc typeDesc) {
            foreach (TypeMapping typeMapping in TypeMappings) {
                if (typeMapping.TypeDesc == typeDesc)
                    return typeMapping;
            }
            return null;
        }

        internal Type GetTypeFromTypeDesc(TypeDesc typeDesc) {
            if (typeDesc.Type != null)
                return typeDesc.Type;
            foreach (DictionaryEntry de in typeDescs) {
                if (de.Value == typeDesc)
                    return de.Key as Type;
            }
            return null;
        }

        TypeDesc ImportTypeDesc(Type type, MemberInfo memberInfo, bool directReference) {
            TypeDesc typeDesc = null;
            TypeKind kind;
            Type arrayElementType = null;
            Type baseType = null;
            TypeFlags flags = 0;
            Exception exception = null;

            if (!type.IsPublic && !type.IsNestedPublic) {
                flags |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(Res.GetString(Res.XmlTypeInaccessible, type.FullName));
            }
            else if (directReference && (type.IsAbstract && type.IsSealed)) {
                flags |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(Res.GetString(Res.XmlTypeStatic, type.FullName));
            }

            if (DynamicAssemblies.IsTypeDynamic(type)) {
                flags |= TypeFlags.UseReflection;
            }
            if (!type.IsValueType)
                flags |= TypeFlags.Reference;

            if (type == typeof(object)) {
                kind = TypeKind.Root;
                flags |= TypeFlags.HasDefaultConstructor;
            }
            else if (type == typeof(ValueType)) {
                kind = TypeKind.Enum;
                flags |= TypeFlags.Unsupported;
                if (exception == null) {
                    exception = new NotSupportedException(Res.GetString(Res.XmlSerializerUnsupportedType, type.FullName));
                }
            }
            else if (type == typeof(void)) {
                kind = TypeKind.Void;
            }
            else if (typeof(IXmlSerializable).IsAssignableFrom(type)) {
                // 
                kind = TypeKind.Serializable;
                flags |= TypeFlags.Special | TypeFlags.CanBeElementValue;
                flags |= GetConstructorFlags(type, ref exception);
            }
            else if (type.IsArray) {
                kind = TypeKind.Array;
                if (type.GetArrayRank() > 1) {
                    flags |= TypeFlags.Unsupported;
                    if (exception == null) {
                        exception = new NotSupportedException(Res.GetString(Res.XmlUnsupportedRank, type.FullName));
                    }
                }
                arrayElementType = type.GetElementType();
                flags |= TypeFlags.HasDefaultConstructor;
            }
            else if (typeof(ICollection).IsAssignableFrom(type) && !IsArraySegment(type)) {
                kind = TypeKind.Collection;
                arrayElementType = GetCollectionElementType(type, memberInfo == null ? null : memberInfo.DeclaringType.FullName + "." + memberInfo.Name);
                flags |= GetConstructorFlags(type, ref exception);
            }
            else if (type == typeof(XmlQualifiedName)) {
                kind = TypeKind.Primitive;
            }
            else if (type.IsPrimitive) {
                kind = TypeKind.Primitive;
                flags |= TypeFlags.Unsupported;
                if (exception == null) {
                    exception = new NotSupportedException(Res.GetString(Res.XmlSerializerUnsupportedType, type.FullName));
                }
            }
            else if (type.IsEnum) {
                kind = TypeKind.Enum;
            }
            else if (type.IsValueType) {
                kind = TypeKind.Struct;
                if (IsOptionalValue(type)) {
                    baseType = type.GetGenericArguments()[0];
                    flags |= TypeFlags.OptionalValue;
                }
                else {
                    baseType = type.BaseType;
                }
                if (type.IsAbstract) flags |= TypeFlags.Abstract;
            }
            else if (type.IsClass) {
                if (type == typeof(XmlAttribute)) {
                    kind = TypeKind.Attribute;
                    flags |= TypeFlags.Special | TypeFlags.CanBeAttributeValue;
                }
                else if (typeof(XmlNode).IsAssignableFrom(type)) {
                    kind = TypeKind.Node;
                    baseType = type.BaseType;
                    flags |= TypeFlags.Special | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue;
                    if (typeof(XmlText).IsAssignableFrom(type))
                        flags &= ~TypeFlags.CanBeElementValue;
                    else if (typeof(XmlElement).IsAssignableFrom(type))
                        flags &= ~TypeFlags.CanBeTextValue;
                    else if (type.IsAssignableFrom(typeof(XmlAttribute)))
                        flags |= TypeFlags.CanBeAttributeValue;
                }
                else {
                    kind = TypeKind.Class;
                    baseType = type.BaseType;
                    if (type.IsAbstract)
                        flags |= TypeFlags.Abstract;
                }
            }
            else if (type.IsInterface) {
                kind = TypeKind.Void;
                flags |= TypeFlags.Unsupported;
                if (exception == null) {
                    if (memberInfo == null) {
                        exception = new NotSupportedException(Res.GetString(Res.XmlUnsupportedInterface, type.FullName));
                    }
                    else {
                        exception = new NotSupportedException(Res.GetString(Res.XmlUnsupportedInterfaceDetails, memberInfo.DeclaringType.FullName + "." + memberInfo.Name, type.FullName));
                    }
                }
            }
            else {
                kind = TypeKind.Void;
                flags |= TypeFlags.Unsupported;
                if (exception == null) {
                    exception = new NotSupportedException(Res.GetString(Res.XmlSerializerUnsupportedType, type.FullName));
                }
            }

            // check to see if the type has public default constructor for classes
            if (kind == TypeKind.Class && !type.IsAbstract) {
                flags |= GetConstructorFlags(type, ref exception);
            }
            // check if a struct-like type is enumerable
            if (kind == TypeKind.Struct || kind == TypeKind.Class) {
                if (typeof(IEnumerable).IsAssignableFrom(type) && !IsArraySegment(type)) {
                    arrayElementType = GetEnumeratorElementType(type, ref flags);
                    kind = TypeKind.Enumerable;

                    // GetEnumeratorElementType checks for the security attributes on the GetEnumerator(), Add() methods and Current property, 
                    // we need to check the MoveNext() and ctor methods for the security attribues
                    flags |= GetConstructorFlags(type, ref exception);
                }
            }
            // 
            typeDesc = new TypeDesc(type, CodeIdentifier.MakeValid(TypeName(type)), type.ToString(), kind, null, flags, null);
            typeDesc.Exception = exception;

            if (directReference && (typeDesc.IsClass || kind == TypeKind.Serializable))
                typeDesc.CheckNeedConstructor();

            if (typeDesc.IsUnsupported) {
                // return right away, do not check anything else
                return typeDesc;
            }
            typeDescs.Add(type, typeDesc);

            if (arrayElementType != null) {
                TypeDesc td = GetTypeDesc(arrayElementType, memberInfo, true, false);
                // explicitly disallow read-only elements, even if they are collections
                if (directReference && (td.IsCollection || td.IsEnumerable) && !td.IsPrimitive) {
                    td.CheckNeedConstructor();
                }
                typeDesc.ArrayElementTypeDesc = td;
            }
            if (baseType != null && baseType != typeof(object) && baseType != typeof(ValueType)) {
                typeDesc.BaseTypeDesc = GetTypeDesc(baseType, memberInfo, false, false);
            }
            if (type.IsNestedPublic) {
                for (Type t = type.DeclaringType; t != null && !t.ContainsGenericParameters && !(t.IsAbstract && t.IsSealed); t = t.DeclaringType)
                    GetTypeDesc(t, null, false);
            }
            return typeDesc;
        }

        private static bool IsArraySegment(Type t) {
            return t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(ArraySegment<>));
        }

        internal static bool IsOptionalValue(Type type) {
            if (type.IsGenericType) {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
                    return true;
            }
            return false;
        }
        // 
        /*
        static string GetHash(string str) {
            MD5 md5 = MD5.Create();
            string hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(str)), 0, 6).Replace("+", "_P").Replace("/", "_S");
            return hash;
        }
        */

        // 
        internal static string TypeName(Type t) {
            if (t.IsArray) {
                return "ArrayOf" + TypeName(t.GetElementType());
            }
            else if (t.IsGenericType) {
                StringBuilder typeName = new StringBuilder();
                StringBuilder ns = new StringBuilder();
                string name = t.Name;
                int arity = name.IndexOf("`", StringComparison.Ordinal);
                if (arity >= 0) {
                    name = name.Substring(0, arity);
                }
                typeName.Append(name);
                typeName.Append("Of");
                Type[] arguments = t.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++) {
                    typeName.Append(TypeName(arguments[i]));
                    ns.Append(arguments[i].Namespace);
                }
                // 
                /*
                if (ns.Length > 0) {
                    typeName.Append("_");
                    typeName.Append(GetHash(ns.ToString()));
                }
                */
                return typeName.ToString();
            }
            return t.Name;
        }

        internal static Type GetArrayElementType(Type type, string memberInfo) {
            if (type.IsArray)
                return type.GetElementType();
            else if (IsArraySegment(type))
                return null;
            else if (typeof(ICollection).IsAssignableFrom(type))
                return GetCollectionElementType(type, memberInfo);
            else if (typeof(IEnumerable).IsAssignableFrom(type)) {
                TypeFlags flags = TypeFlags.None;
                return GetEnumeratorElementType(type, ref flags);
            }
            else
                return null;
        }

        internal static MemberMapping[] GetAllMembers(StructMapping mapping) {
            if (mapping.BaseMapping == null)
                return mapping.Members;
            ArrayList list = new ArrayList();
            GetAllMembers(mapping, list);
            return (MemberMapping[])list.ToArray(typeof(MemberMapping));
        }

        internal static void GetAllMembers(StructMapping mapping, ArrayList list) {
            if (mapping.BaseMapping != null) {
                GetAllMembers(mapping.BaseMapping, list);
            }
            for (int i = 0; i < mapping.Members.Length; i++) {
                list.Add(mapping.Members[i]);
            }
        }

        internal static MemberMapping[] GetAllMembers(StructMapping mapping, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos) {
            MemberMapping[] mappings = GetAllMembers(mapping);
            PopulateMemberInfos(mapping, mappings, memberInfos);
            return mappings;
        }

        internal static MemberMapping[] GetSettableMembers(StructMapping structMapping) {
            ArrayList list = new ArrayList();
            GetSettableMembers(structMapping, list);
            return (MemberMapping[])list.ToArray(typeof(MemberMapping));
        }

        static void GetSettableMembers(StructMapping mapping, ArrayList list) {
            if (mapping.BaseMapping != null) {
                GetSettableMembers(mapping.BaseMapping, list);
            }

            if(mapping.Members != null) {
                foreach (MemberMapping memberMapping in mapping.Members) {                
                    MemberInfo memberInfo = memberMapping.MemberInfo;
                    if(memberInfo != null && memberInfo.MemberType == MemberTypes.Property) {
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                        if (propertyInfo != null && !CanWriteProperty(propertyInfo, memberMapping.TypeDesc)) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlReadOnlyPropertyError, propertyInfo.DeclaringType, propertyInfo.Name));
                        }
                    }
                    list.Add(memberMapping);                
                }
            }
        }

        static bool CanWriteProperty(PropertyInfo propertyInfo, TypeDesc typeDesc) {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(typeDesc != null);

            // If the property is a collection, we don't need a setter.
            if (typeDesc.Kind == TypeKind.Collection || typeDesc.Kind == TypeKind.Enumerable) {
                return true;
            }
            // Else the property needs a public setter.
            return propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic;
        }

        internal static MemberMapping[] GetSettableMembers(StructMapping mapping, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos) {
            MemberMapping[] mappings = GetSettableMembers(mapping);
            PopulateMemberInfos(mapping, mappings, memberInfos);
            return mappings;
        }

        static void PopulateMemberInfos(StructMapping structMapping, MemberMapping[] mappings, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos)
        {
            memberInfos.Clear();
            for (int i = 0; i < mappings.Length; ++i) {
                memberInfos[mappings[i].Name] = mappings[i].MemberInfo;
                if (mappings[i].ChoiceIdentifier != null)
                    memberInfos[mappings[i].ChoiceIdentifier.MemberName] = mappings[i].ChoiceIdentifier.MemberInfo;
                if (mappings[i].CheckSpecifiedMemberInfo != null)
                    memberInfos[mappings[i].Name + "Specified"] = mappings[i].CheckSpecifiedMemberInfo;
            }

            // The scenario here is that user has one base class A and one derived class B and wants to serialize/deserialize an object of B.
            // There's one virtual property defined in A and overrided by B. Without the replacing logic below, the code generated will always
            // try to access the property defined in A, rather than B.
            // The logic here is to:
            // 1) Check current members inside memberInfos dictionary and figure out whether there's any override or new properties defined in the derived class.
            //    If so, replace the one on the base class with the one on the derived class.
            // 2) Do the same thing for the memberMapping array. Note that we need to create a new copy of MemberMapping object since the old one could still be referenced
            //    by the StructMapping of the baseclass, so updating it directly could lead to other issues.
            Dictionary<string, MemberInfo> replaceList = null;
            MemberInfo replacedInfo = null;
            foreach (KeyValuePair<string, MemberInfo> pair in memberInfos)
            {
                if (ShouldBeReplaced(pair.Value, structMapping.TypeDesc.Type, out replacedInfo))
                {
                    if (replaceList == null)
                    {
                        replaceList = new Dictionary<string, MemberInfo>();
                    }

                    replaceList.Add(pair.Key, replacedInfo);
                }
            }

            if (replaceList != null)
            {
                foreach (KeyValuePair<string, MemberInfo> pair in replaceList)
                {
                    memberInfos[pair.Key] = pair.Value;
                }
                for (int i = 0; i < mappings.Length; i++)
                {
                    MemberInfo mi;
                    if (replaceList.TryGetValue(mappings[i].Name, out mi))
                    {
                        MemberMapping newMapping = mappings[i].Clone();
                        newMapping.MemberInfo = mi;
                        mappings[i] = newMapping;
                    }
                }
            }
        }

        static bool ShouldBeReplaced(MemberInfo memberInfoToBeReplaced, Type derivedType, out MemberInfo replacedInfo)
        {
            replacedInfo = memberInfoToBeReplaced;
            Type currentType = derivedType;
            Type typeToBeReplaced = memberInfoToBeReplaced.DeclaringType;

            if (typeToBeReplaced.IsAssignableFrom(currentType))
            {
                while (currentType != typeToBeReplaced)
                {
                    TypeInfo currentInfo = currentType.GetTypeInfo();

                    foreach (PropertyInfo info in currentInfo.DeclaredProperties)
                    {
                        if (info.Name == memberInfoToBeReplaced.Name)
                        {
                            // we have a new modifier situation: property names are the same but the declaring types are different
                            replacedInfo = info;
                            if (replacedInfo != memberInfoToBeReplaced)
                            {
                                return true;
                            }
                        }
                    }
                    foreach (FieldInfo info in currentInfo.DeclaredFields)
                    {
                        if (info.Name == memberInfoToBeReplaced.Name)
                        {
                            // we have a new modifier situation: field names are the same but the declaring types are different
                            replacedInfo = info;
                            if (replacedInfo != memberInfoToBeReplaced)
                            {
                                return true;
                            }
                        }
                    }

                    // we go one level down and try again
                    currentType = currentType.BaseType;
                }
            }

            return false;
        }

        static TypeFlags GetConstructorFlags(Type type, ref Exception exception) {
            ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
            if (ctor != null) {
                TypeFlags flags = TypeFlags.HasDefaultConstructor;
                if (!ctor.IsPublic)
                    flags |= TypeFlags.CtorInaccessible;
                else {
                    object[] attrs = ctor.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                    if (attrs != null && attrs.Length > 0) {
                        ObsoleteAttribute obsolete = (ObsoleteAttribute)attrs[0];
                        if (obsolete.IsError) {
                            flags |= TypeFlags.CtorInaccessible;
                        }
                    }
                }
                return flags;
            }
            return 0;
        }

        static Type GetEnumeratorElementType(Type type, ref TypeFlags flags) {
            if (typeof(IEnumerable).IsAssignableFrom(type)) {
                MethodInfo enumerator = type.GetMethod("GetEnumerator", new Type[0]);

                if (enumerator == null || !typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType)) {
                    // try generic implementation
                    enumerator = null;
                    foreach (MemberInfo member in type.GetMember("System.Collections.Generic.IEnumerable<*", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)) {
                        enumerator = member as MethodInfo;
                        if (enumerator != null && typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType)) {
                            // use the first one we find
                            flags |= TypeFlags.GenericInterface;
                            break;
                        }
                        else {
                            enumerator = null;
                        }
                    }
                    if (enumerator == null) {
                        // and finally private interface implementation
                        flags |= TypeFlags.UsePrivateImplementation;
                        enumerator = type.GetMethod("System.Collections.IEnumerable.GetEnumerator", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
                    }
                }
                if (enumerator == null || !typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType)) {
                    return null;
                }
                XmlAttributes methodAttrs = new XmlAttributes(enumerator);
                if (methodAttrs.XmlIgnore) return null;

                PropertyInfo p = enumerator.ReturnType.GetProperty("Current");
                Type currentType = (p == null ? typeof(object) : p.PropertyType);

                MethodInfo addMethod = type.GetMethod("Add", new Type[] { currentType });

                if (addMethod == null && currentType != typeof(object)) {
                    currentType = typeof(object);
                    addMethod = type.GetMethod("Add", new Type[] { currentType });
                }
                if (addMethod == null) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlNoAddMethod, type.FullName, currentType, "IEnumerable"));
                }
                return currentType;
            }
            else {
                return null;
            }
        }

        internal static PropertyInfo GetDefaultIndexer(Type type, string memberInfo) {
            if (typeof(IDictionary).IsAssignableFrom(type)) {
                if (memberInfo == null) {
                    throw new NotSupportedException(Res.GetString(Res.XmlUnsupportedIDictionary, type.FullName));
                }
                else {
                    throw new NotSupportedException(Res.GetString(Res.XmlUnsupportedIDictionaryDetails, memberInfo, type.FullName));
                }
            }

            MemberInfo[] defaultMembers = type.GetDefaultMembers();
            PropertyInfo indexer = null;
            if (defaultMembers != null && defaultMembers.Length > 0) {
                for (Type t = type; t != null; t = t.BaseType) {
                    for (int i = 0; i < defaultMembers.Length; i++) {
                        if (defaultMembers[i] is PropertyInfo) {
                            PropertyInfo defaultProp = (PropertyInfo)defaultMembers[i];
                            if (defaultProp.DeclaringType != t) continue;
                            if (!defaultProp.CanRead) continue;
                            MethodInfo getMethod = defaultProp.GetGetMethod();
                            ParameterInfo[] parameters = getMethod.GetParameters();
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int)) {
                                indexer = defaultProp;
                                break;
                            }
                        }
                    }
                    if (indexer != null) break;
                }
            }
            if (indexer == null) {
                throw new InvalidOperationException(Res.GetString(Res.XmlNoDefaultAccessors, type.FullName));
            }
            MethodInfo addMethod = type.GetMethod("Add", new Type[] { indexer.PropertyType });
            if (addMethod == null) {
                throw new InvalidOperationException(Res.GetString(Res.XmlNoAddMethod, type.FullName, indexer.PropertyType, "ICollection"));
            }
            return indexer;
        }
        static Type GetCollectionElementType(Type type, string memberInfo) {
            return GetDefaultIndexer(type, memberInfo).PropertyType;
        }

        static internal XmlQualifiedName ParseWsdlArrayType(string type, out string dims, XmlSchemaObject parent) {
            string ns;
            string name;

            int nsLen = type.LastIndexOf(':');

            if (nsLen <= 0) {
                ns = "";
            }
            else {
                ns = type.Substring(0, nsLen);
            }
            int nameLen = type.IndexOf('[', nsLen + 1);

            if (nameLen <= nsLen) {
                throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayTypeSyntax, type));
            }
            name = type.Substring(nsLen + 1, nameLen - nsLen - 1);
            dims = type.Substring(nameLen);

            // parent is not null only in the case when we used XmlSchema.Read(), 
            // in which case we need to fixup the wsdl:arayType attribute value
            while (parent != null) {
                if (parent.Namespaces != null) {
                    string wsdlNs = (string)parent.Namespaces.Namespaces[ns];
                    if (wsdlNs != null) {
                        ns = wsdlNs;
                        break;
                    }
                }
                parent = parent.Parent;
            }
            return new XmlQualifiedName(name, ns);
        }

        internal ICollection Types {
            get { return this.typeDescs.Keys; }
        }

        internal void AddTypeMapping(TypeMapping typeMapping) {
            typeMappings.Add(typeMapping);
        }

        internal ICollection TypeMappings {
            get { return typeMappings; }
        }
        internal static Hashtable PrimtiveTypes { get { return primitiveTypes; } }
    }

    internal class Soap {
        private Soap() { }
        internal const string Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
        internal const string UrType = "anyType";
        internal const string Array = "Array";
        internal const string ArrayType = "arrayType";
    }

    internal class Soap12 {
        private Soap12() { }
        internal const string Encoding = "http://www.w3.org/2003/05/soap-encoding";
        internal const string RpcNamespace = "http://www.w3.org/2003/05/soap-rpc";
        internal const string RpcResult = "result";
    }

    internal class Wsdl {
        private Wsdl() { }
        internal const string Namespace = "http://schemas.xmlsoap.org/wsdl/";
        internal const string ArrayType = "arrayType";
    }

    internal class UrtTypes {
        private UrtTypes() { }
        internal const string Namespace = "http://microsoft.com/wsdl/types/";
    }
}

