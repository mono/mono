//------------------------------------------------------------------------------
// <copyright file="Mappings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.Reflection;
    using System.Collections;
    using System.Xml.Schema;
    using System;
    using System.Text;
    using System.ComponentModel;
    using System.Xml;
    using System.CodeDom.Compiler;

    // These classes represent a mapping between classes and a particular XML format.
    // There are two class of mapping information: accessors (such as elements and
    // attributes), and mappings (which specify the type of an accessor).

    internal abstract class Accessor {
        string name;
        object defaultValue = null;
        string ns;
        TypeMapping mapping;
        bool any;
        string anyNs;
        bool topLevelInSchema;
        bool isFixed;
        bool isOptional;
        XmlSchemaForm form = XmlSchemaForm.None;

        internal Accessor() { }

        internal TypeMapping Mapping {
            get { return mapping; }
            set { mapping = value; }
        }

        internal object Default {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        internal bool HasDefault {
            get { return defaultValue != null && defaultValue != DBNull.Value; }
        }

        internal virtual string Name {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }

        internal bool Any {
            get { return any; }
            set { any = value; }
        }

        internal string AnyNamespaces {
            get { return anyNs; }
            set { anyNs = value; }
        }

        internal string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        internal XmlSchemaForm Form {
            get { return form; }
            set { form = value; }
        }

        internal bool IsFixed {
            get { return isFixed; }
            set { isFixed = value; }
        }

        internal bool IsOptional {
            get { return isOptional; }
            set { isOptional = value; }
        }

        internal bool IsTopLevelInSchema {
            get { return topLevelInSchema; }
            set { topLevelInSchema = value; }
        }

        internal static string EscapeName(string name) {
            if (name == null || name.Length == 0) return name;
            return XmlConvert.EncodeLocalName(name);
        }

        internal static string EscapeQName(string name) {
            if (name == null || name.Length == 0) return name;
            int colon = name.LastIndexOf(':');
            if (colon < 0)
                return XmlConvert.EncodeLocalName(name);
            else {
                if (colon == 0 || colon == name.Length - 1)
                    throw new ArgumentException(Res.GetString(Res.Xml_InvalidNameChars, name), "name");
                return new XmlQualifiedName(XmlConvert.EncodeLocalName(name.Substring(colon + 1)), XmlConvert.EncodeLocalName(name.Substring(0, colon))).ToString();
            }
        }

        internal static string UnescapeName(string name) {
            return XmlConvert.DecodeName(name);
        }

        internal string ToString(string defaultNs) {
            if (Any) {
                return (Namespace == null ? "##any" : Namespace) + ":" + Name;
            }
            else {
                return Namespace == defaultNs ? Name : Namespace + ":" + Name;
            }
        }
    }

    internal class ElementAccessor : Accessor {
        bool nullable;
        bool isSoap;
        bool unbounded = false;

        internal bool IsSoap {
            get { return isSoap; }
            set { isSoap = value; }
        }

        internal bool IsNullable {
            get { return nullable; }
            set { nullable = value; }
        }

        internal bool IsUnbounded {
            get { return unbounded; }
            set { unbounded = value; }
        }

        internal ElementAccessor Clone() {
            ElementAccessor newAccessor = new ElementAccessor();
            newAccessor.nullable = this.nullable;
            newAccessor.IsTopLevelInSchema = this.IsTopLevelInSchema;
            newAccessor.Form = this.Form;
            newAccessor.isSoap = this.isSoap;
            newAccessor.Name = this.Name;
            newAccessor.Default = this.Default;
            newAccessor.Namespace = this.Namespace;
            newAccessor.Mapping = this.Mapping;
            newAccessor.Any = this.Any;

            return newAccessor;
        }
    }

    internal class ChoiceIdentifierAccessor : Accessor {
        string memberName;
        string[] memberIds;
        MemberInfo memberInfo;

        internal string MemberName {
            get { return memberName; }
            set { memberName = value; }
        }

        internal string[] MemberIds {
            get { return memberIds; }
            set { memberIds = value; }
        }

        internal MemberInfo MemberInfo {
            get { return memberInfo; }
            set { memberInfo = value; }
        }
    }

    internal class TextAccessor : Accessor {
    }

    internal class XmlnsAccessor : Accessor {
    }

    internal class AttributeAccessor : Accessor {
        bool isSpecial;
        bool isList;

        internal bool IsSpecialXmlNamespace {
            get { return isSpecial; }
        }

        internal bool IsList {
            get { return isList; }
            set { isList = value; }
        }

        internal void CheckSpecial() {
            int colon = Name.LastIndexOf(':');

            if (colon >= 0) {
                if (!Name.StartsWith("xml:", StringComparison.Ordinal)) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidNameChars, Name));
                }
                Name = Name.Substring("xml:".Length);
                Namespace = XmlReservedNs.NsXml;
                isSpecial = true;
            }
            else {
                if (Namespace == XmlReservedNs.NsXml) {
                    isSpecial = true;
                }
                else {
                    isSpecial = false;
                }
            }
            if (isSpecial) {
                Form = XmlSchemaForm.Qualified;
            }
        }
    }

    internal abstract class Mapping {
        bool isSoap;

        internal Mapping() { }

        protected Mapping(Mapping mapping)
        {
            this.isSoap = mapping.isSoap;
        }

        internal bool IsSoap {
            get { return isSoap; }
            set { isSoap = value; }
        }
    }

    internal abstract class TypeMapping : Mapping {
        TypeDesc typeDesc;
        string typeNs;
        string typeName;
        bool referencedByElement;
        bool referencedByTopLevelElement;
        bool includeInSchema = true;
        bool reference = false;

        internal bool ReferencedByTopLevelElement {
            get { return referencedByTopLevelElement; }
            set { referencedByTopLevelElement = value; }
        }

        internal bool ReferencedByElement {
            get { return referencedByElement || referencedByTopLevelElement; }
            set { referencedByElement = value; }
        }
        internal string Namespace {
            get { return typeNs; }
            set { typeNs = value; }
        }

        internal string TypeName {
            get { return typeName; }
            set { typeName = value; }
        }

        internal TypeDesc TypeDesc {
            get { return typeDesc; }
            set { typeDesc = value; }
        }

        internal bool IncludeInSchema {
            get { return includeInSchema; }
            set { includeInSchema = value; }
        }

        internal virtual bool IsList {
            get { return false; }
            set { }
        }

        internal bool IsReference {
            get { return reference; }
            set { reference = value; }
        }

        internal bool IsAnonymousType {
            get { return typeName == null || typeName.Length == 0; }
        }

        internal virtual string DefaultElementName {
            get { return IsAnonymousType ? XmlConvert.EncodeLocalName(typeDesc.Name) : typeName; }
        }
    }

    internal class PrimitiveMapping : TypeMapping {
        bool isList;

        internal override bool IsList {
            get { return isList; }
            set { isList = value; }
        }
    }

    internal class NullableMapping : TypeMapping {
        TypeMapping baseMapping;

        internal TypeMapping BaseMapping {
            get { return baseMapping; }
            set { baseMapping = value; }
        }

        internal override string DefaultElementName {
            get { return BaseMapping.DefaultElementName; }
        }
    }

    internal class ArrayMapping : TypeMapping {
        ElementAccessor[] elements;
        ElementAccessor[] sortedElements;
        ArrayMapping next;
        StructMapping topLevelMapping;

        internal ElementAccessor[] Elements {
            get { return elements; }
            set { elements = value; sortedElements = null; }
        }

        internal ElementAccessor[] ElementsSortedByDerivation {
            get {
                if (sortedElements != null)
                    return sortedElements;
                if (elements == null)
                    return null;
                sortedElements = new ElementAccessor[elements.Length];
                Array.Copy(elements, 0, sortedElements, 0, elements.Length);
                AccessorMapping.SortMostToLeastDerived(sortedElements);
                return sortedElements;
            }
        }


        internal ArrayMapping Next {
            get { return next; }
            set { next = value; }
        }

        internal StructMapping TopLevelMapping {
            get { return topLevelMapping; }
            set { topLevelMapping = value; }
        }
    }

    internal class EnumMapping : PrimitiveMapping {
        ConstantMapping[] constants;
        bool isFlags;

        internal bool IsFlags {
            get { return isFlags; }
            set { isFlags = value; }
        }

        internal ConstantMapping[] Constants {
            get { return constants; }
            set { constants = value; }
        }
    }

    internal class ConstantMapping : Mapping {
        string xmlName;
        string name;
        long value;

        internal string XmlName {
            get { return xmlName == null ? string.Empty : xmlName; }
            set { xmlName = value; }
        }

        internal string Name {
            get { return name == null ? string.Empty : name; }
            set { this.name = value; }
        }

        internal long Value {
            get { return value; }
            set { this.value = value; }
        }
    }

    internal class StructMapping : TypeMapping, INameScope {
        MemberMapping[] members;
        StructMapping baseMapping;
        StructMapping derivedMappings;
        StructMapping nextDerivedMapping;
        MemberMapping xmlnsMember = null;
        bool hasSimpleContent;
        bool openModel;
        bool isSequence;
        NameTable elements;
        NameTable attributes;
        CodeIdentifiers scope;

        internal StructMapping BaseMapping {
            get { return baseMapping; }
            set {
                baseMapping = value;
                if (!IsAnonymousType && baseMapping != null) {
                    nextDerivedMapping = baseMapping.derivedMappings;
                    baseMapping.derivedMappings = this;
                }
                if (value.isSequence && !isSequence) {
                    isSequence = true;
                    if (baseMapping.IsSequence) {
                        for (StructMapping derived = derivedMappings; derived != null; derived = derived.NextDerivedMapping) {
                            derived.SetSequence();
                        }
                    }
                }
            }
        }

        internal StructMapping DerivedMappings {
            get { return derivedMappings; }
        }

        internal bool IsFullyInitialized {
            get { return baseMapping != null && Members != null; }
        }

        internal NameTable LocalElements {
            get {
                if (elements == null)
                    elements = new NameTable();
                return elements;
            }
        }
        internal NameTable LocalAttributes {
            get {
                if (attributes == null)
                    attributes = new NameTable();
                return attributes;
            }
        }
        object INameScope.this[string name, string ns] {
            get {
                object named = LocalElements[name, ns];
                if (named != null)
                    return named;
                if (baseMapping != null)
                    return ((INameScope)baseMapping)[name, ns];
                return null;
            }
            set {
                LocalElements[name, ns] = value;
            }
        }
        internal StructMapping NextDerivedMapping {
            get { return nextDerivedMapping; }
        }

        internal bool HasSimpleContent {
            get { return hasSimpleContent; }
        }

        internal bool HasXmlnsMember {
            get {
                StructMapping mapping = this;
                while (mapping != null) {
                    if (mapping.XmlnsMember != null)
                        return true;
                    mapping = mapping.BaseMapping;
                }
                return false;
            }
        }

        internal MemberMapping[] Members {
            get { return members; }
            set { members = value; }
        }

        internal MemberMapping XmlnsMember {
            get { return xmlnsMember; }
            set { xmlnsMember = value; }
        }

        internal bool IsOpenModel {
            get { return openModel; }
            set { openModel = value; }
        }

        internal CodeIdentifiers Scope {
            get {
                if (scope == null)
                    scope = new CodeIdentifiers();
                return scope;
            }
            set { scope = value; }
        }

        internal MemberMapping FindDeclaringMapping(MemberMapping member, out StructMapping declaringMapping, string parent) {
            declaringMapping = null;
            if (BaseMapping != null) {
                MemberMapping baseMember = BaseMapping.FindDeclaringMapping(member, out declaringMapping, parent);
                if (baseMember != null) return baseMember;
            }
            if (members == null) return null;

            for (int i = 0; i < members.Length; i++) {
                if (members[i].Name == member.Name) {
                    if (members[i].TypeDesc != member.TypeDesc)
                        throw new InvalidOperationException(Res.GetString(Res.XmlHiddenMember, parent, member.Name, member.TypeDesc.FullName, this.TypeName, members[i].Name, members[i].TypeDesc.FullName));
                    else if (!members[i].Match(member)) {
                        throw new InvalidOperationException(Res.GetString(Res.XmlInvalidXmlOverride, parent, member.Name, this.TypeName, members[i].Name));
                    }
                    declaringMapping = this;
                    return members[i];
                }
            }
            return null;
        }
        internal bool Declares(MemberMapping member, string parent) {
            StructMapping m;
            return (FindDeclaringMapping(member, out m, parent) != null);
        }

        internal void SetContentModel(TextAccessor text, bool hasElements) {
            if (BaseMapping == null || BaseMapping.TypeDesc.IsRoot) {
                hasSimpleContent = !hasElements && text != null && !text.Mapping.IsList;
            }
            else if (BaseMapping.HasSimpleContent) {
                if (text != null || hasElements) {
                    // we can only extent a simleContent type with attributes
                    throw new InvalidOperationException(Res.GetString(Res.XmlIllegalSimpleContentExtension, TypeDesc.FullName, BaseMapping.TypeDesc.FullName));
                }
                else {
                    hasSimpleContent = true;
                }
            }
            else {
                hasSimpleContent = false;
            }
            if (!hasSimpleContent && text != null && !text.Mapping.TypeDesc.CanBeTextValue) {
                throw new InvalidOperationException(Res.GetString(Res.XmlIllegalTypedTextAttribute, TypeDesc.FullName, text.Name, text.Mapping.TypeDesc.FullName));
            }
        }

        internal bool HasElements {
            get { return elements != null && elements.Values.Count > 0; }
        }

        internal bool HasExplicitSequence() {
            if (members != null) {
                for (int i = 0; i < members.Length; i++) {
                    if (members[i].IsParticle && members[i].IsSequence) {
                        return true;
                    }
                }
            }
            return (baseMapping != null && baseMapping.HasExplicitSequence());
        }

        internal void SetSequence() {
            if (TypeDesc.IsRoot)
                return;

            StructMapping start = this;

            // find first mapping that does not have the sequence set
            while (!start.BaseMapping.IsSequence && start.BaseMapping != null && !start.BaseMapping.TypeDesc.IsRoot)
                start = start.BaseMapping;

            start.IsSequence = true;
            for (StructMapping derived = start.DerivedMappings; derived != null; derived = derived.NextDerivedMapping) {
                derived.SetSequence();
            }
        }

        internal bool IsSequence {
            get { return isSequence && !TypeDesc.IsRoot; }
            set { isSequence = value; }
        }
    }

    internal abstract class AccessorMapping : Mapping {
        TypeDesc typeDesc;
        AttributeAccessor attribute;
        ElementAccessor[] elements;
        ElementAccessor[] sortedElements;
        TextAccessor text;
        ChoiceIdentifierAccessor choiceIdentifier;
        XmlnsAccessor xmlns;
        bool ignore;
        
        internal AccessorMapping()
        { }

        protected AccessorMapping(AccessorMapping mapping)
            : base(mapping)
        {
            this.typeDesc = mapping.typeDesc;
            this.attribute = mapping.attribute;
            this.elements = mapping.elements;
            this.sortedElements = mapping.sortedElements;
            this.text = mapping.text;
            this.choiceIdentifier = mapping.choiceIdentifier;
            this.xmlns = mapping.xmlns;
            this.ignore = mapping.ignore;
        }

        internal bool IsAttribute {
            get { return attribute != null; }
        }

        internal bool IsText {
            get { return text != null && (elements == null || elements.Length == 0); }
        }

        internal bool IsParticle {
            get { return (elements != null && elements.Length > 0); }
        }

        internal TypeDesc TypeDesc {
            get { return typeDesc; }
            set { typeDesc = value; }
        }

        internal AttributeAccessor Attribute {
            get { return attribute; }
            set { attribute = value; }
        }

        internal ElementAccessor[] Elements {
            get { return elements; }
            set { elements = value; sortedElements = null; }
        }

        internal static void SortMostToLeastDerived(ElementAccessor[] elements) {
            Array.Sort(elements, new AccessorComparer());
        }

        internal class AccessorComparer : IComparer {
            public int Compare(object o1, object o2) {
                if (o1 == o2)
                    return 0;
                Accessor a1 = (Accessor)o1;
                Accessor a2 = (Accessor)o2;
                int w1 = a1.Mapping.TypeDesc.Weight;
                int w2 = a2.Mapping.TypeDesc.Weight;
                if (w1 == w2)
                    return 0;
                if (w1 < w2)
                    return 1;
                return -1;
            }
        }

        internal ElementAccessor[] ElementsSortedByDerivation {
            get {
                if (sortedElements != null)
                    return sortedElements;
                if (elements == null)
                    return null;
                sortedElements = new ElementAccessor[elements.Length];
                Array.Copy(elements, 0, sortedElements, 0, elements.Length);
                SortMostToLeastDerived(sortedElements);
                return sortedElements;
            }
        }

        internal TextAccessor Text {
            get { return text; }
            set { text = value; }
        }

        internal ChoiceIdentifierAccessor ChoiceIdentifier {
            get { return choiceIdentifier; }
            set { choiceIdentifier = value; }
        }

        internal XmlnsAccessor Xmlns {
            get { return xmlns; }
            set { xmlns = value; }
        }

        internal bool Ignore {
            get { return ignore; }
            set { ignore = value; }
        }

        internal Accessor Accessor {
            get {
                if (xmlns != null) return xmlns;
                if (attribute != null) return attribute;
                if (elements != null && elements.Length > 0) return elements[0];
                return text;
            }
        }

        static bool IsNeedNullableMember(ElementAccessor element) {
            if (element.Mapping is ArrayMapping) {
                ArrayMapping arrayMapping = (ArrayMapping)element.Mapping;
                if (arrayMapping.Elements != null && arrayMapping.Elements.Length == 1) {
                    return IsNeedNullableMember(arrayMapping.Elements[0]);
                }
                return false;
            }
            else {
                return element.IsNullable && element.Mapping.TypeDesc.IsValueType;
            }
        }

        internal bool IsNeedNullable {
            get {
                if (xmlns != null) return false;
                if (attribute != null) return false;
                if (elements != null && elements.Length == 1) {
                    return IsNeedNullableMember(elements[0]);
                }
                return false;
            }
        }

        internal static bool ElementsMatch(ElementAccessor[] a, ElementAccessor[] b) {
            if (a == null) {
                if (b == null)
                    return true;
                return false;
            }
            if (b == null)
                return false;
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++) {
                if (a[i].Name != b[i].Name || a[i].Namespace != b[i].Namespace || a[i].Form != b[i].Form || a[i].IsNullable != b[i].IsNullable)
                    return false;
            }
            return true;
        }

        internal bool Match(AccessorMapping mapping) {
            if (Elements != null && Elements.Length > 0) {
                if (!ElementsMatch(Elements, mapping.Elements)) {
                    return false;
                }
                if (Text == null) {
                    return (mapping.Text == null);
                }
            }
            if (Attribute != null) {
                if (mapping.Attribute == null)
                    return false;
                return (Attribute.Name == mapping.Attribute.Name && Attribute.Namespace == mapping.Attribute.Namespace && Attribute.Form == mapping.Attribute.Form);
            }
            if (Text != null) {
                return (mapping.Text != null);
            }
            return (mapping.Accessor == null);
        }
    }

    internal class MemberMappingComparer : IComparer {
        public int Compare(object o1, object o2) {
            MemberMapping m1 = (MemberMapping)o1;
            MemberMapping m2 = (MemberMapping)o2;

            bool m1Text = m1.IsText;
            if (m1Text) {
                if (m2.IsText)
                    return 0;
                return 1;
            }
            else if (m2.IsText)
                return -1;

            if (m1.SequenceId < 0 && m2.SequenceId < 0)
                return 0;
            if (m1.SequenceId < 0)
                return 1;
            if (m2.SequenceId < 0)
                return -1;
            if (m1.SequenceId < m2.SequenceId)
                return -1;
            if (m1.SequenceId > m2.SequenceId)
                return 1;
            return 0;
        }
    }

    internal class MemberMapping : AccessorMapping {
        string name;
        bool checkShouldPersist;
        SpecifiedAccessor checkSpecified;
        bool isReturnValue;
        bool readOnly = false;
        int sequenceId = -1;
        MemberInfo memberInfo;
        MemberInfo checkSpecifiedMemberInfo;
        MethodInfo checkShouldPersistMethodInfo;

        internal MemberMapping() { }
        
        MemberMapping(MemberMapping mapping)
            : base(mapping)
        {
            this.name = mapping.name;
            this.checkShouldPersist = mapping.checkShouldPersist;
            this.checkSpecified = mapping.checkSpecified;
            this.isReturnValue = mapping.isReturnValue;
            this.readOnly = mapping.readOnly;
            this.sequenceId = mapping.sequenceId;
            this.memberInfo = mapping.memberInfo;
            this.checkSpecifiedMemberInfo = mapping.checkSpecifiedMemberInfo;
            this.checkShouldPersistMethodInfo = mapping.checkShouldPersistMethodInfo;
        }

        internal bool CheckShouldPersist {
            get { return checkShouldPersist; }
            set { checkShouldPersist = value; }
        }

        internal SpecifiedAccessor CheckSpecified {
            get { return checkSpecified; }
            set { checkSpecified = value; }
        }

        internal string Name {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }

        internal MemberInfo MemberInfo {
            get { return memberInfo; }
            set { memberInfo = value; }
        }

        internal MemberInfo CheckSpecifiedMemberInfo {
            get { return checkSpecifiedMemberInfo; }
            set { checkSpecifiedMemberInfo = value; }
        }

        internal MethodInfo CheckShouldPersistMethodInfo {
            get { return checkShouldPersistMethodInfo; }
            set { checkShouldPersistMethodInfo = value; }
        }

        internal bool IsReturnValue {
            get { return isReturnValue; }
            set { isReturnValue = value; }
        }

        internal bool ReadOnly {
            get { return readOnly; }
            set { readOnly = value; }
        }

        internal bool IsSequence {
            get { return sequenceId >= 0; }
        }

        internal int SequenceId {
            get { return sequenceId; }
            set { sequenceId = value; }
        }

        string GetNullableType(TypeDesc td) {
            // SOAP encoded arrays not mapped to Nullable<T> since they always derive from soapenc:Array
            if (td.IsMappedType || (!td.IsValueType && (Elements[0].IsSoap || td.ArrayElementTypeDesc == null)))
                return td.FullName;
            if (td.ArrayElementTypeDesc != null) {
                return GetNullableType(td.ArrayElementTypeDesc) + "[]";
            }
            return "System.Nullable`1[" + td.FullName + "]";
        }

        internal MemberMapping Clone()
        {
            return new MemberMapping(this);
        }

        internal string GetTypeName(CodeDomProvider codeProvider) {
            if (IsNeedNullable && codeProvider.Supports(GeneratorSupport.GenericTypeReference)) {
                return GetNullableType(TypeDesc);
            }
            return TypeDesc.FullName;
        }
    }

    internal class MembersMapping : TypeMapping {
        MemberMapping[] members;
        bool hasWrapperElement = true;
        bool validateRpcWrapperElement;
        bool writeAccessors = true;
        MemberMapping xmlnsMember = null;

        internal MemberMapping[] Members {
            get { return members; }
            set { members = value; }
        }

        internal MemberMapping XmlnsMember {
            get { return xmlnsMember; }
            set { xmlnsMember = value; }
        }

        internal bool HasWrapperElement {
            get { return hasWrapperElement; }
            set { hasWrapperElement = value; }
        }

        internal bool ValidateRpcWrapperElement {
            get { return validateRpcWrapperElement; }
            set { validateRpcWrapperElement = value; }
        }

        internal bool WriteAccessors {
            get { return writeAccessors; }
            set { writeAccessors = value; }
        }
    }

    internal class SpecialMapping : TypeMapping {
        bool namedAny;

        internal bool NamedAny {
            get { return namedAny; }
            set { namedAny = value; }
        }
    }

    internal class SerializableMapping : SpecialMapping {
        XmlSchema schema;
        Type type;
        bool needSchema = true;

        // new implementation of the IXmlSerializable
        MethodInfo getSchemaMethod;
        XmlQualifiedName xsiType;
        XmlSchemaType xsdType;
        XmlSchemaSet schemas;
        bool any;
        string namespaces;

        SerializableMapping baseMapping;
        SerializableMapping derivedMappings;
        SerializableMapping nextDerivedMapping;
        SerializableMapping next; // all mappings with the same qname

        internal SerializableMapping() { }
        internal SerializableMapping(MethodInfo getSchemaMethod, bool any, string ns) {
            this.getSchemaMethod = getSchemaMethod;
            this.any = any;
            this.Namespace = ns;
            needSchema = getSchemaMethod != null;
        }

        internal SerializableMapping(XmlQualifiedName xsiType, XmlSchemaSet schemas) {
            this.xsiType = xsiType;
            this.schemas = schemas;
            this.TypeName = xsiType.Name;
            this.Namespace = xsiType.Namespace;
            needSchema = false;
        }

        internal void SetBaseMapping(SerializableMapping mapping) {
            baseMapping = mapping;
            if (baseMapping != null) {
                nextDerivedMapping = baseMapping.derivedMappings;
                baseMapping.derivedMappings = this;
                if (this == nextDerivedMapping) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlCircularDerivation, TypeDesc.FullName));
                }
            }
        }

        internal bool IsAny {
            get {
                if (any)
                    return true;
                if (getSchemaMethod == null)
                    return false;
                if (needSchema && typeof(XmlSchemaType).IsAssignableFrom(getSchemaMethod.ReturnType))
                    return false;
                RetrieveSerializableSchema();
                return any;
            }
        }

        internal string NamespaceList {
            get {
                RetrieveSerializableSchema();
                if (namespaces == null) {
                    if (schemas != null) {
                        StringBuilder anyNamespaces = new StringBuilder();
                        foreach (XmlSchema s in schemas.Schemas()) {
                            if (s.TargetNamespace != null && s.TargetNamespace.Length > 0) {
                                if (anyNamespaces.Length > 0)
                                    anyNamespaces.Append(" ");
                                anyNamespaces.Append(s.TargetNamespace);
                            }
                        }
                        namespaces = anyNamespaces.ToString();
                    }
                    else {
                        namespaces = string.Empty;
                    }
                }
                return namespaces;
            }
        }

        internal SerializableMapping DerivedMappings {
            get {
                return derivedMappings;
            }
        }

        internal SerializableMapping NextDerivedMapping {
            get {
                return nextDerivedMapping;
            }
        }

        internal SerializableMapping Next {
            get { return next; }
            set { next = value; }
        }

        internal Type Type {
            get { return type; }
            set { type = value; }
        }

        internal XmlSchemaSet Schemas {
            get {
                RetrieveSerializableSchema();
                return schemas;
            }
        }

        internal XmlSchema Schema {
            get {
                RetrieveSerializableSchema();
                return schema;
            }
        }

        internal XmlQualifiedName XsiType {
            get {
                if (!needSchema)
                    return xsiType;
                if (getSchemaMethod == null)
                    return null;
                if (typeof(XmlSchemaType).IsAssignableFrom(getSchemaMethod.ReturnType))
                    return null;
                RetrieveSerializableSchema();
                return xsiType;
            }
        }

        internal XmlSchemaType XsdType {
            get {
                RetrieveSerializableSchema();
                return xsdType;
            }
        }

        internal static void ValidationCallbackWithErrorCode(object sender, ValidationEventArgs args) {
            // 
            if (args.Severity == XmlSeverityType.Error)
                throw new InvalidOperationException(Res.GetString(Res.XmlSerializableSchemaError, typeof(IXmlSerializable).Name, args.Message));
        }

        internal void CheckDuplicateElement(XmlSchemaElement element, string elementNs) {
            if (element == null)
                return;

            // only check duplicate definitions for top-level element
            if (element.Parent == null || !(element.Parent is XmlSchema))
                return;

            XmlSchemaObjectTable elements = null;
            if (Schema != null && Schema.TargetNamespace == elementNs) {
                XmlSchemas.Preprocess(Schema);
                elements = Schema.Elements;
            }
            else if (Schemas != null) {
                elements = Schemas.GlobalElements;
            }
            else {
                return;
            }
            foreach (XmlSchemaElement e in elements.Values) {
                if (e.Name == element.Name && e.QualifiedName.Namespace == elementNs) {
                    if (Match(e, element))
                        return;
                    // XmlSerializableRootDupName=Cannot reconcile schema for '{0}'. Please use [XmlRoot] attribute to change name or namepace of the top-level element to avoid duplicate element declarations: element name='{1} namespace='{2}'.
                    throw new InvalidOperationException(Res.GetString(Res.XmlSerializableRootDupName, getSchemaMethod.DeclaringType.FullName, e.Name, elementNs));
                }
            }
        }

        bool Match(XmlSchemaElement e1, XmlSchemaElement e2) {
            if (e1.IsNillable != e2.IsNillable)
                return false;
            if (e1.RefName != e2.RefName)
                return false;
            if (e1.SchemaType != e2.SchemaType)
                return false;
            if (e1.SchemaTypeName != e2.SchemaTypeName)
                return false;
            if (e1.MinOccurs != e2.MinOccurs)
                return false;
            if (e1.MaxOccurs != e2.MaxOccurs)
                return false;
            if (e1.IsAbstract != e2.IsAbstract)
                return false;
            if (e1.DefaultValue != e2.DefaultValue)
                return false;
            if (e1.SubstitutionGroup != e2.SubstitutionGroup)
                return false;
            return true;
        }

        void RetrieveSerializableSchema() {
            if (needSchema) {
                needSchema = false;
                if (getSchemaMethod != null) {
                    // get the type info
                    if (schemas == null)
                        schemas = new XmlSchemaSet();
                    object typeInfo = getSchemaMethod.Invoke(null, new object[] { schemas });
                    xsiType = XmlQualifiedName.Empty;

                    if (typeInfo != null) {
                        if (typeof(XmlSchemaType).IsAssignableFrom(getSchemaMethod.ReturnType)) {
                            xsdType = (XmlSchemaType)typeInfo;
                            // check if type is named
                            xsiType = xsdType.QualifiedName;
                        }
                        else if (typeof(XmlQualifiedName).IsAssignableFrom(getSchemaMethod.ReturnType)) {
                            xsiType = (XmlQualifiedName)typeInfo;
                            if (xsiType.IsEmpty) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaEmptyTypeName, type.FullName, getSchemaMethod.Name));
                            }
                        }
                        else {
                            throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaMethodReturnType, type.Name, getSchemaMethod.Name, typeof(XmlSchemaProviderAttribute).Name, typeof(XmlQualifiedName).FullName));
                        }
                    }
                    else {
                        any = true;
                    }

                    // make sure that user-specified schemas are valid
                    schemas.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackWithErrorCode);
                    schemas.Compile();
                    // at this point we verified that the information returned by the IXmlSerializable is valid
                    // Now check to see if the type was referenced before:
                    // 
                    if (!xsiType.IsEmpty) {
                        // try to find the type in the schemas collection
                        if (xsiType.Namespace != XmlSchema.Namespace) {
                            ArrayList srcSchemas = (ArrayList)schemas.Schemas(xsiType.Namespace);

                            if (srcSchemas.Count == 0) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlMissingSchema, xsiType.Namespace));
                            }
                            if (srcSchemas.Count > 1) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaInclude, xsiType.Namespace, getSchemaMethod.DeclaringType.FullName, getSchemaMethod.Name));
                            }
                            XmlSchema s = (XmlSchema)srcSchemas[0];
                            if (s == null) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlMissingSchema, xsiType.Namespace));
                            }
                            xsdType = (XmlSchemaType)s.SchemaTypes[xsiType];
                            if (xsdType == null) {
                                throw new InvalidOperationException(Res.GetString(Res.XmlGetSchemaTypeMissing, getSchemaMethod.DeclaringType.FullName, getSchemaMethod.Name, xsiType.Name, xsiType.Namespace));
                            }
                            xsdType = xsdType.Redefined != null ? xsdType.Redefined : xsdType;
                        }
                    }
                }
                else {
                    IXmlSerializable serializable = (IXmlSerializable)Activator.CreateInstance(type);
                    schema = serializable.GetSchema();

                    if (schema != null) {
                        if (schema.Id == null || schema.Id.Length == 0) throw new InvalidOperationException(Res.GetString(Res.XmlSerializableNameMissing1, type.FullName));
                    }
                }
            }
        }
    }
}

