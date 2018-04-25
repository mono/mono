//------------------------------------------------------------------------------
// <copyright file="DatatypeImplementation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System;
    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Serialization;

    /// <include file='doc\DatatypeImplementation.uex' path='docs/doc[@for="XmlSchemaDatatypeVariety"]/*' />
    public enum XmlSchemaDatatypeVariety {
        /// <include file='doc\DatatypeImplementation.uex' path='docs/doc[@for="XmlSchemaDatatypeVariety.Atomic"]/*' />
        Atomic,
        /// <include file='doc\DatatypeImplementation.uex' path='docs/doc[@for="XmlSchemaDatatypeVariety.List"]/*' />
        List,
        /// <include file='doc\DatatypeImplementation.uex' path='docs/doc[@for="XmlSchemaDatatypeVariety.Union"]/*' />
        Union
    }

    internal class XsdSimpleValue { //Wrapper to store XmlType and TypedValue together
        XmlSchemaSimpleType xmlType;
        object  typedValue;

        public XsdSimpleValue(XmlSchemaSimpleType st, object value) {
            xmlType = st;
            typedValue = value;
        }

        public XmlSchemaSimpleType XmlType {
            get {
                return xmlType;
            }
        }

        public object TypedValue {
            get {
                return typedValue;
            }
        }
    }


    [Flags]
    internal enum RestrictionFlags {
        Length              = 0x0001,
        MinLength           = 0x0002,
        MaxLength           = 0x0004,
        Pattern             = 0x0008,
        Enumeration         = 0x0010,
        WhiteSpace          = 0x0020,
        MaxInclusive        = 0x0040,
        MaxExclusive        = 0x0080,
        MinInclusive        = 0x0100,
        MinExclusive        = 0x0200,
        TotalDigits         = 0x0400,
        FractionDigits      = 0x0800,
    }

    internal enum XmlSchemaWhiteSpace {
        Preserve,
        Replace,
        Collapse,
    }

    internal class RestrictionFacets {
        internal int Length;
        internal int MinLength;
        internal int MaxLength;
        internal ArrayList Patterns;
        internal ArrayList Enumeration;
        internal XmlSchemaWhiteSpace WhiteSpace;
        internal object MaxInclusive;
        internal object MaxExclusive;
        internal object MinInclusive;
        internal object MinExclusive;
        internal int TotalDigits;
        internal int FractionDigits;
        internal RestrictionFlags Flags = 0;
        internal RestrictionFlags FixedFlags = 0;
    }

    internal abstract class DatatypeImplementation : XmlSchemaDatatype {
        private XmlSchemaDatatypeVariety variety = XmlSchemaDatatypeVariety.Atomic;
        private RestrictionFacets restriction = null;
        private DatatypeImplementation baseType = null;
        private XmlValueConverter valueConverter;
        private XmlSchemaType parentSchemaType;

        private static Hashtable builtinTypes = new Hashtable();
        private static XmlSchemaSimpleType[] enumToTypeCode = new XmlSchemaSimpleType[(int) XmlTypeCode.DayTimeDuration + 1];
        private static XmlSchemaSimpleType anySimpleType;
        private static XmlSchemaSimpleType anyAtomicType;
        private static XmlSchemaSimpleType untypedAtomicType;
        private static XmlSchemaSimpleType yearMonthDurationType;
        private static XmlSchemaSimpleType dayTimeDurationType;
        private static volatile XmlSchemaSimpleType normalizedStringTypeV1Compat;
        private static volatile XmlSchemaSimpleType tokenTypeV1Compat;

        private const int anySimpleTypeIndex = 11;

        internal static XmlQualifiedName QnAnySimpleType = new XmlQualifiedName("anySimpleType",XmlReservedNs.NsXs);
        internal static XmlQualifiedName QnAnyType = new XmlQualifiedName("anyType",XmlReservedNs.NsXs);

        //Create facet checkers
        internal static FacetsChecker stringFacetsChecker = new StringFacetsChecker();
        internal static FacetsChecker miscFacetsChecker = new MiscFacetsChecker();
        internal static FacetsChecker numeric2FacetsChecker = new Numeric2FacetsChecker();
        internal static FacetsChecker binaryFacetsChecker = new BinaryFacetsChecker();
        internal static FacetsChecker dateTimeFacetsChecker = new DateTimeFacetsChecker();
        internal static FacetsChecker durationFacetsChecker = new DurationFacetsChecker();
        internal static FacetsChecker listFacetsChecker = new ListFacetsChecker();
        internal static FacetsChecker qnameFacetsChecker = new QNameFacetsChecker();
        internal static FacetsChecker unionFacetsChecker = new UnionFacetsChecker();

        static DatatypeImplementation() {
            CreateBuiltinTypes();
        }

        internal static XmlSchemaSimpleType AnySimpleType { get { return anySimpleType; } }

        // Additional built-in XQuery simple types
        internal static XmlSchemaSimpleType AnyAtomicType { get { return anyAtomicType; } }
        internal static XmlSchemaSimpleType UntypedAtomicType { get { return untypedAtomicType; } }
        internal static XmlSchemaSimpleType YearMonthDurationType { get { return yearMonthDurationType; } }
        internal static XmlSchemaSimpleType DayTimeDurationType { get { return dayTimeDurationType; } }

        internal new static DatatypeImplementation FromXmlTokenizedType(XmlTokenizedType token) {
            return c_tokenizedTypes[(int)token];
        }

        internal new static DatatypeImplementation FromXmlTokenizedTypeXsd(XmlTokenizedType token) {
            return c_tokenizedTypesXsd[(int)token];
        }

        internal new static DatatypeImplementation FromXdrName(string name) {
            int i = Array.BinarySearch(c_XdrTypes, name, null);
            return i < 0 ? null : (DatatypeImplementation)c_XdrTypes[i];
        }

        private static DatatypeImplementation FromTypeName(string name) {
            int i = Array.BinarySearch(c_XsdTypes, name, null);
            return i < 0 ? null : (DatatypeImplementation)c_XsdTypes[i];
        }

        /// <summary>
        /// Begin the creation of an XmlSchemaSimpleType object that will be used to represent a static built-in type.
        /// Once StartBuiltinType has been called for all built-in types, FinishBuiltinType should be called in order
        /// to create links between the types.
        /// </summary>
        internal static XmlSchemaSimpleType StartBuiltinType(XmlQualifiedName qname, XmlSchemaDatatype dataType) {
            XmlSchemaSimpleType simpleType;
            Debug.Assert(qname != null && dataType != null);

            simpleType = new XmlSchemaSimpleType();
            simpleType.SetQualifiedName(qname);
            simpleType.SetDatatype(dataType);
            simpleType.ElementDecl = new SchemaElementDecl(dataType);
            simpleType.ElementDecl.SchemaType = simpleType;

            return simpleType;
        }

        /// <summary>
        /// Finish constructing built-in types by setting up derivation and list links.
        /// </summary>
        internal static void FinishBuiltinType(XmlSchemaSimpleType derivedType, XmlSchemaSimpleType baseType) {
            Debug.Assert(derivedType != null && baseType != null);

            // Create link from the derived type to the base type
            derivedType.SetBaseSchemaType(baseType);
            derivedType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
            if (derivedType.Datatype.Variety == XmlSchemaDatatypeVariety.Atomic) { //Content is restriction
                XmlSchemaSimpleTypeRestriction restContent = new XmlSchemaSimpleTypeRestriction();
                restContent.BaseTypeName = baseType.QualifiedName;
                derivedType.Content = restContent;
            }

            // Create link from a list type to its member type
            if (derivedType.Datatype.Variety == XmlSchemaDatatypeVariety.List) {
                XmlSchemaSimpleTypeList listContent = new XmlSchemaSimpleTypeList();
                derivedType.SetDerivedBy(XmlSchemaDerivationMethod.List);
                switch (derivedType.Datatype.TypeCode) {
                    case XmlTypeCode.NmToken:
                        listContent.ItemType = listContent.BaseItemType = enumToTypeCode[(int) XmlTypeCode.NmToken];
                        break;

                    case XmlTypeCode.Entity:
                        listContent.ItemType = listContent.BaseItemType = enumToTypeCode[(int) XmlTypeCode.Entity];
                        break;

                    case XmlTypeCode.Idref:
                        listContent.ItemType = listContent.BaseItemType = enumToTypeCode[(int) XmlTypeCode.Idref];
                        break;
                }
                derivedType.Content = listContent;
            }
        }

        internal static void CreateBuiltinTypes() {
            XmlQualifiedName qname;

            //Build anySimpleType
            SchemaDatatypeMap sdm = c_XsdTypes[anySimpleTypeIndex]; //anySimpleType
            qname = new XmlQualifiedName(sdm.Name, XmlReservedNs.NsXs);
            DatatypeImplementation dt = FromTypeName(qname.Name);
            anySimpleType = StartBuiltinType(qname, dt);
            dt.parentSchemaType = anySimpleType;
            builtinTypes.Add(qname, anySimpleType);

            // Start construction of each built-in Xsd type
            XmlSchemaSimpleType simpleType;
            for (int i = 0; i < c_XsdTypes.Length; i++) { //Create all types
                if (i == anySimpleTypeIndex) { //anySimpleType
                    continue;
                }
                sdm = c_XsdTypes[i];

                qname = new XmlQualifiedName(sdm.Name, XmlReservedNs.NsXs);
                dt = FromTypeName(qname.Name);
                simpleType = StartBuiltinType(qname, dt);
                dt.parentSchemaType = simpleType;

                builtinTypes.Add(qname, simpleType);
                if (dt.variety == XmlSchemaDatatypeVariety.Atomic) {
                    enumToTypeCode[(int)dt.TypeCode] = simpleType;
                }
            }

            // Finish construction of each built-in Xsd type
            for (int i = 0; i < c_XsdTypes.Length; i++) {
                if (i == anySimpleTypeIndex) { //anySimpleType
                    continue;
                }
                sdm = c_XsdTypes[i];
                XmlSchemaSimpleType derivedType = (XmlSchemaSimpleType) builtinTypes[new XmlQualifiedName(sdm.Name, XmlReservedNs.NsXs)];
                XmlSchemaSimpleType baseType;

                if (sdm.ParentIndex == anySimpleTypeIndex) {
                    FinishBuiltinType(derivedType, anySimpleType);
                }
                else { //derived types whose index > 0
                    baseType = (XmlSchemaSimpleType) builtinTypes[new XmlQualifiedName( ((SchemaDatatypeMap)(c_XsdTypes[sdm.ParentIndex])).Name, XmlReservedNs.NsXs)];
                    FinishBuiltinType(derivedType, baseType);
                }
            }

            // Construct xdt:anyAtomicType type (derived from xs:anySimpleType)
            qname = new XmlQualifiedName("anyAtomicType", XmlReservedNs.NsXQueryDataType);
            anyAtomicType = StartBuiltinType(qname, c_anyAtomicType);
            c_anyAtomicType.parentSchemaType = anyAtomicType;
            FinishBuiltinType(anyAtomicType, anySimpleType);
            builtinTypes.Add(qname, anyAtomicType);
            enumToTypeCode[(int)XmlTypeCode.AnyAtomicType] = anyAtomicType;

            // Construct xdt:untypedAtomic type (derived from xdt:anyAtomicType)
            qname = new XmlQualifiedName("untypedAtomic", XmlReservedNs.NsXQueryDataType);
            untypedAtomicType = StartBuiltinType(qname, c_untypedAtomicType);
            c_untypedAtomicType.parentSchemaType = untypedAtomicType;
            FinishBuiltinType(untypedAtomicType, anyAtomicType);
            builtinTypes.Add(qname, untypedAtomicType);
            enumToTypeCode[(int)XmlTypeCode.UntypedAtomic] = untypedAtomicType;

            // Construct xdt:yearMonthDuration type (derived from xs:duration)
            qname = new XmlQualifiedName("yearMonthDuration", XmlReservedNs.NsXQueryDataType);
            yearMonthDurationType = StartBuiltinType(qname, c_yearMonthDuration);
            c_yearMonthDuration.parentSchemaType = yearMonthDurationType;
            FinishBuiltinType(yearMonthDurationType, enumToTypeCode[(int) XmlTypeCode.Duration]);
            builtinTypes.Add(qname, yearMonthDurationType);
            enumToTypeCode[(int)XmlTypeCode.YearMonthDuration] = yearMonthDurationType;

            // Construct xdt:dayTimeDuration type (derived from xs:duration)
            qname = new XmlQualifiedName("dayTimeDuration", XmlReservedNs.NsXQueryDataType);
            dayTimeDurationType = StartBuiltinType(qname, c_dayTimeDuration);
            c_dayTimeDuration.parentSchemaType = dayTimeDurationType;
            FinishBuiltinType(dayTimeDurationType, enumToTypeCode[(int) XmlTypeCode.Duration]);
            builtinTypes.Add(qname, dayTimeDurationType);
            enumToTypeCode[(int)XmlTypeCode.DayTimeDuration] = dayTimeDurationType;
        }

        internal static XmlSchemaSimpleType GetSimpleTypeFromTypeCode(XmlTypeCode typeCode) {
            return enumToTypeCode[(int) typeCode];
        }

        internal static XmlSchemaSimpleType GetSimpleTypeFromXsdType(XmlQualifiedName qname) {
            return (XmlSchemaSimpleType)builtinTypes[qname];
        }

        internal static XmlSchemaSimpleType GetNormalizedStringTypeV1Compat() {
            if (normalizedStringTypeV1Compat == null) {
                XmlSchemaSimpleType correctType = GetSimpleTypeFromTypeCode(XmlTypeCode.NormalizedString);
                XmlSchemaSimpleType tempNormalizedStringTypeV1Compat = correctType.Clone() as XmlSchemaSimpleType;
                tempNormalizedStringTypeV1Compat.SetDatatype(c_normalizedStringV1Compat);
                tempNormalizedStringTypeV1Compat.ElementDecl = new SchemaElementDecl(c_normalizedStringV1Compat);
                tempNormalizedStringTypeV1Compat.ElementDecl.SchemaType = tempNormalizedStringTypeV1Compat;
                normalizedStringTypeV1Compat = tempNormalizedStringTypeV1Compat;
            }
            return normalizedStringTypeV1Compat;
        }

        internal static XmlSchemaSimpleType GetTokenTypeV1Compat() {
            if (tokenTypeV1Compat == null) {
                XmlSchemaSimpleType correctType = GetSimpleTypeFromTypeCode(XmlTypeCode.Token);
                XmlSchemaSimpleType tempTokenTypeV1Compat = correctType.Clone() as XmlSchemaSimpleType;
                tempTokenTypeV1Compat.SetDatatype(c_tokenV1Compat);
                tempTokenTypeV1Compat.ElementDecl = new SchemaElementDecl(c_tokenV1Compat);
                tempTokenTypeV1Compat.ElementDecl.SchemaType = tempTokenTypeV1Compat;
                tokenTypeV1Compat = tempTokenTypeV1Compat;
            }
            return tokenTypeV1Compat;
        }

        internal static XmlSchemaSimpleType[] GetBuiltInTypes() {
            return enumToTypeCode;
        }

        internal static XmlTypeCode GetPrimitiveTypeCode(XmlTypeCode typeCode) {
            XmlSchemaSimpleType currentType = enumToTypeCode[(int)typeCode];
            while (currentType.BaseXmlSchemaType != DatatypeImplementation.AnySimpleType) {
                currentType = currentType.BaseXmlSchemaType as XmlSchemaSimpleType;
                Debug.Assert(currentType != null);
            }
            return currentType.TypeCode;
        }

        internal override XmlSchemaDatatype DeriveByRestriction(XmlSchemaObjectCollection facets, XmlNameTable nameTable, XmlSchemaType schemaType) {
            DatatypeImplementation dt = (DatatypeImplementation)MemberwiseClone();
            dt.restriction = this.FacetsChecker.ConstructRestriction(this, facets, nameTable);
            dt.baseType = this;
            dt.parentSchemaType = schemaType;
            dt.valueConverter = null; //re-set old datatype's valueconverter
            return dt;
        }

        internal override XmlSchemaDatatype DeriveByList(XmlSchemaType schemaType) {
            return DeriveByList(0, schemaType);
        }

        internal XmlSchemaDatatype DeriveByList(int minSize, XmlSchemaType schemaType) {
            if (variety == XmlSchemaDatatypeVariety.List) {
                throw new XmlSchemaException(Res.Sch_ListFromNonatomic, string.Empty);
            }
            else if (variety == XmlSchemaDatatypeVariety.Union && !((Datatype_union)this).HasAtomicMembers()) {
                throw new XmlSchemaException(Res.Sch_ListFromNonatomic, string.Empty);
            }
            DatatypeImplementation dt = new Datatype_List(this, minSize);
            dt.variety = XmlSchemaDatatypeVariety.List;
            dt.restriction = null;
            dt.baseType = c_anySimpleType; //Base type of a union is anySimpleType
            dt.parentSchemaType = schemaType;
            return dt;
        }

        internal new static DatatypeImplementation DeriveByUnion(XmlSchemaSimpleType[] types, XmlSchemaType schemaType) {
            DatatypeImplementation dt = new Datatype_union(types);
            dt.baseType = c_anySimpleType; //Base type of a union is anySimpleType
            dt.variety = XmlSchemaDatatypeVariety.Union;
            dt.parentSchemaType = schemaType;
            return dt;
        }

        internal override void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller) {/*noop*/}

        public override bool IsDerivedFrom(XmlSchemaDatatype datatype) {
            if (datatype == null) {
                return false;
            }

            //Common case - Derived by restriction
            for(DatatypeImplementation dt = this; dt != null; dt = dt.baseType) {
                if (dt == datatype) {
                    return true;
                }
            }
            if (((DatatypeImplementation)datatype).baseType == null) { //Both are built-in types
                Type derivedType = this.GetType();
                Type baseType = datatype.GetType();
                return baseType == derivedType || derivedType.IsSubclassOf(baseType);
            }
            else if (datatype.Variety == XmlSchemaDatatypeVariety.Union && !datatype.HasLexicalFacets && !datatype.HasValueFacets && variety != XmlSchemaDatatypeVariety.Union) { //base type is union (not a restriction of union) and derived type is not union
                return ((Datatype_union)datatype).IsUnionBaseOf(this);
            }
            else if ((variety == XmlSchemaDatatypeVariety.Union || variety == XmlSchemaDatatypeVariety.List) && restriction == null) { //derived type is union (not a restriction)
                return (datatype == anySimpleType.Datatype);
            }
            return false;
        }

        internal override bool IsEqual(object o1, object o2) {
            //Debug.WriteLineIf(DiagnosticsSwitches.XmlSchema.TraceVerbose, string.Format("\t\tSchemaDatatype.IsEqual({0}, {1})", o1, o2));
            return Compare(o1, o2) == 0;
        }

        internal override bool IsComparable(XmlSchemaDatatype dtype) {
            XmlTypeCode thisCode = this.TypeCode;
            XmlTypeCode otherCode = dtype.TypeCode;

            if (thisCode == otherCode) { //They are both same built-in type or one is list and the other is list's itemType
                return true;
            }
            if (GetPrimitiveTypeCode(thisCode) == GetPrimitiveTypeCode(otherCode)) {
                return true;
            }
            if (this.IsDerivedFrom(dtype) || dtype.IsDerivedFrom(this)) { //One is union and the other is a member of the union
                return true;
            }
            return false;
        }

        internal virtual XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) { return null; }

        internal override FacetsChecker FacetsChecker { get { return miscFacetsChecker; } }

        internal override XmlValueConverter ValueConverter {
            get {
                if (valueConverter == null) {
                    valueConverter = CreateValueConverter(this.parentSchemaType);
                }
                return valueConverter;
            }
        }

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.None;}}

        public override Type ValueType { get { return typeof(string); }}

        public override XmlSchemaDatatypeVariety Variety { get { return variety;}}

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.None; } }

        internal override RestrictionFacets Restriction {
            get {
                return restriction;
            }
            set {
                restriction = value;
            }
        }
        internal override bool HasLexicalFacets {
            get {
                RestrictionFlags flags = restriction != null ? restriction.Flags : 0;
                if (flags != 0 && (flags & (RestrictionFlags.Pattern|RestrictionFlags.WhiteSpace|RestrictionFlags.TotalDigits|RestrictionFlags.FractionDigits)) != 0) {
                    return true;
                }
                return false;
            }
        }
        internal override bool HasValueFacets {
            get {
                RestrictionFlags flags = restriction != null ? restriction.Flags : 0;
                if (flags != 0 && (flags & (RestrictionFlags.Length | RestrictionFlags.MinLength | RestrictionFlags.MaxLength | RestrictionFlags.MaxExclusive | RestrictionFlags.MaxInclusive | RestrictionFlags.MinExclusive | RestrictionFlags.MinInclusive | RestrictionFlags.TotalDigits | RestrictionFlags.FractionDigits | RestrictionFlags.Enumeration)) != 0)
                {
                    return true;
                }
                return false;
            }
        }

        protected DatatypeImplementation Base { get { return baseType; }}

        internal abstract Type ListValueType { get; }

        internal abstract RestrictionFlags ValidRestrictionFlags { get; }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Preserve; } }

        internal override object ParseValue(string s, Type typDest, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            return ValueConverter.ChangeType(ParseValue(s, nameTable, nsmgr), typDest, nsmgr);
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            object typedValue;
            Exception exception = TryParseValue(s, nameTable, nsmgr, out typedValue);
            if (exception != null) {
                throw new XmlSchemaException(Res.Sch_InvalidValueDetailed, new string[] { s, GetTypeName(), exception.Message }, exception, null, 0, 0, null);
            }
            if (this.Variety == XmlSchemaDatatypeVariety.Union) {
                XsdSimpleValue simpleValue = typedValue as XsdSimpleValue;
                return simpleValue.TypedValue;
            }
            return typedValue;
        }

        internal override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, bool createAtomicValue) {
            if (createAtomicValue) {
                object typedValue;
                Exception exception = TryParseValue(s, nameTable, nsmgr, out typedValue);
                if (exception != null) {
                    throw new XmlSchemaException(Res.Sch_InvalidValueDetailed, new string[] { s, GetTypeName(), exception.Message }, exception, null, 0, 0, null);
                }
                return typedValue;
            }
            else {
                return ParseValue(s, nameTable, nsmgr);
            }
        }

        internal override Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue) {
            Exception exception = null;
            typedValue = null;
            if (value == null) {
                return new ArgumentNullException("value");
            }
            string s = value as string;
            if (s != null) {
                return TryParseValue(s, nameTable, namespaceResolver, out typedValue);
            }
            try {
                object valueToCheck = value;
                if (value.GetType() != this.ValueType) {
                    valueToCheck = this.ValueConverter.ChangeType(value, this.ValueType, namespaceResolver);
                }
                if (this.HasLexicalFacets) {
                    string s1 = (string)this.ValueConverter.ChangeType(value, typeof(System.String), namespaceResolver); //Using value here to avoid info loss
                    exception = this.FacetsChecker.CheckLexicalFacets(ref s1, this);
                    if (exception != null) goto Error;
                }
                if (this.HasValueFacets) {
                    exception = this.FacetsChecker.CheckValueFacets(valueToCheck, this);
                    if (exception != null) goto Error;
                }
                typedValue = valueToCheck;
                return null;
            }
            catch (FormatException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (InvalidCastException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (OverflowException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (ArgumentException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }

        Error:
           return exception;
        }

        internal string GetTypeName() {
            XmlSchemaType simpleType = this.parentSchemaType;
            string typeName;
            if (simpleType == null || simpleType.QualifiedName.IsEmpty) { //If no QName, get typecode, no line info since it is not pertinent without file name
                typeName = TypeCodeString;
            }
            else {
                typeName = simpleType.QualifiedName.ToString();
            }
            return typeName;
        }

        // XSD types
        static private readonly DatatypeImplementation c_anySimpleType       = new Datatype_anySimpleType();
        static private readonly DatatypeImplementation c_anyURI              = new Datatype_anyURI();
        static private readonly DatatypeImplementation c_base64Binary        = new Datatype_base64Binary();
        static private readonly DatatypeImplementation c_boolean             = new Datatype_boolean();
        static private readonly DatatypeImplementation c_byte                = new Datatype_byte();
        static private readonly DatatypeImplementation c_char                = new Datatype_char(); // XDR
        static private readonly DatatypeImplementation c_date                = new Datatype_date();
        static private readonly DatatypeImplementation c_dateTime            = new Datatype_dateTime();
        static private readonly DatatypeImplementation c_dateTimeNoTz        = new Datatype_dateTimeNoTimeZone(); // XDR
        static private readonly DatatypeImplementation c_dateTimeTz          = new Datatype_dateTimeTimeZone(); // XDR
        static private readonly DatatypeImplementation c_day                 = new Datatype_day();
        static private readonly DatatypeImplementation c_decimal             = new Datatype_decimal();
        static private readonly DatatypeImplementation c_double              = new Datatype_double();
        static private readonly DatatypeImplementation c_doubleXdr           = new Datatype_doubleXdr();     // XDR
        static private readonly DatatypeImplementation c_duration            = new Datatype_duration();
        static private readonly DatatypeImplementation c_ENTITY              = new Datatype_ENTITY();
        static private readonly DatatypeImplementation c_ENTITIES            = (DatatypeImplementation)c_ENTITY.DeriveByList(1, null);
        static private readonly DatatypeImplementation c_ENUMERATION         = new Datatype_ENUMERATION(); // XDR
        static private readonly DatatypeImplementation c_fixed               = new Datatype_fixed();
        static private readonly DatatypeImplementation c_float               = new Datatype_float();
        static private readonly DatatypeImplementation c_floatXdr            = new Datatype_floatXdr(); // XDR
        static private readonly DatatypeImplementation c_hexBinary           = new Datatype_hexBinary();
        static private readonly DatatypeImplementation c_ID                  = new Datatype_ID();
        static private readonly DatatypeImplementation c_IDREF               = new Datatype_IDREF();
        static private readonly DatatypeImplementation c_IDREFS              = (DatatypeImplementation)c_IDREF.DeriveByList(1, null);
        static private readonly DatatypeImplementation c_int                 = new Datatype_int();
        static private readonly DatatypeImplementation c_integer             = new Datatype_integer();
        static private readonly DatatypeImplementation c_language            = new Datatype_language();
        static private readonly DatatypeImplementation c_long                = new Datatype_long();
        static private readonly DatatypeImplementation c_month               = new Datatype_month();
        static private readonly DatatypeImplementation c_monthDay            = new Datatype_monthDay();
        static private readonly DatatypeImplementation c_Name                = new Datatype_Name();
        static private readonly DatatypeImplementation c_NCName              = new Datatype_NCName();
        static private readonly DatatypeImplementation c_negativeInteger     = new Datatype_negativeInteger();
        static private readonly DatatypeImplementation c_NMTOKEN             = new Datatype_NMTOKEN();
        static private readonly DatatypeImplementation c_NMTOKENS            = (DatatypeImplementation)c_NMTOKEN.DeriveByList(1, null);
        static private readonly DatatypeImplementation c_nonNegativeInteger  = new Datatype_nonNegativeInteger();
        static private readonly DatatypeImplementation c_nonPositiveInteger  = new Datatype_nonPositiveInteger();
        static private readonly DatatypeImplementation c_normalizedString    = new Datatype_normalizedString();
        static private readonly DatatypeImplementation c_NOTATION            = new Datatype_NOTATION();
        static private readonly DatatypeImplementation c_positiveInteger     = new Datatype_positiveInteger();
        static private readonly DatatypeImplementation c_QName               = new Datatype_QName();
        static private readonly DatatypeImplementation c_QNameXdr            = new Datatype_QNameXdr(); //XDR
        static private readonly DatatypeImplementation c_short               = new Datatype_short();
        static private readonly DatatypeImplementation c_string              = new Datatype_string();
        static private readonly DatatypeImplementation c_time                = new Datatype_time();
        static private readonly DatatypeImplementation c_timeNoTz            = new Datatype_timeNoTimeZone(); // XDR
        static private readonly DatatypeImplementation c_timeTz              = new Datatype_timeTimeZone(); // XDR
        static private readonly DatatypeImplementation c_token               = new Datatype_token();
        static private readonly DatatypeImplementation c_unsignedByte        = new Datatype_unsignedByte();
        static private readonly DatatypeImplementation c_unsignedInt         = new Datatype_unsignedInt();
        static private readonly DatatypeImplementation c_unsignedLong        = new Datatype_unsignedLong();
        static private readonly DatatypeImplementation c_unsignedShort       = new Datatype_unsignedShort();
        static private readonly DatatypeImplementation c_uuid                = new Datatype_uuid(); // XDR
        static private readonly DatatypeImplementation c_year                = new Datatype_year();
        static private readonly DatatypeImplementation c_yearMonth           = new Datatype_yearMonth();

        //V1 compat types
        static internal readonly DatatypeImplementation c_normalizedStringV1Compat = new Datatype_normalizedStringV1Compat();
        static internal readonly DatatypeImplementation c_tokenV1Compat = new Datatype_tokenV1Compat();

        // XQuery types
        static private readonly DatatypeImplementation c_anyAtomicType       = new Datatype_anyAtomicType();
        static private readonly DatatypeImplementation c_dayTimeDuration     = new Datatype_dayTimeDuration();
        static private readonly DatatypeImplementation c_untypedAtomicType   = new Datatype_untypedAtomicType();
        static private readonly DatatypeImplementation c_yearMonthDuration   = new Datatype_yearMonthDuration();


        private class SchemaDatatypeMap : IComparable {
            string name;
            DatatypeImplementation type;
            int parentIndex;

            internal SchemaDatatypeMap(string name, DatatypeImplementation type) {
                this.name = name;
                this.type = type;
            }

            internal SchemaDatatypeMap(string name, DatatypeImplementation type, int parentIndex) {
                this.name = name;
                this.type = type;
                this.parentIndex = parentIndex;
            }
            public static explicit operator DatatypeImplementation(SchemaDatatypeMap sdm) { return sdm.type; }

            public string Name {
                get {
                    return name;
                }
            }

            public int ParentIndex {
                get {
                    return parentIndex;
                }
            }

            public int CompareTo(object obj) { return string.Compare(name, (string)obj, StringComparison.Ordinal); }
        }

        private static readonly DatatypeImplementation[] c_tokenizedTypes = {
            c_string,               // CDATA
            c_ID,                   // ID
            c_IDREF,                // IDREF
            c_IDREFS,               // IDREFS
            c_ENTITY,               // ENTITY
            c_ENTITIES,             // ENTITIES
            c_NMTOKEN,              // NMTOKEN
            c_NMTOKENS,             // NMTOKENS
            c_NOTATION,             // NOTATION
            c_ENUMERATION,          // ENUMERATION
            c_QNameXdr,             // QName
            c_NCName,               // NCName
            null
        };

        private static readonly DatatypeImplementation[] c_tokenizedTypesXsd = {
            c_string,               // CDATA
            c_ID,                   // ID
            c_IDREF,                // IDREF
            c_IDREFS,               // IDREFS
            c_ENTITY,               // ENTITY
            c_ENTITIES,             // ENTITIES
            c_NMTOKEN,              // NMTOKEN
            c_NMTOKENS,             // NMTOKENS
            c_NOTATION,             // NOTATION
            c_ENUMERATION,          // ENUMERATION
            c_QName,                // QName
            c_NCName,               // NCName
            null
        };

        private static readonly SchemaDatatypeMap[] c_XdrTypes = {
            new SchemaDatatypeMap("bin.base64",          c_base64Binary),
            new SchemaDatatypeMap("bin.hex",             c_hexBinary),
            new SchemaDatatypeMap("boolean",             c_boolean),
            new SchemaDatatypeMap("char",                c_char),
            new SchemaDatatypeMap("date",                c_date),
            new SchemaDatatypeMap("dateTime",            c_dateTimeNoTz),
            new SchemaDatatypeMap("dateTime.tz",         c_dateTimeTz),
            new SchemaDatatypeMap("decimal",             c_decimal),
            new SchemaDatatypeMap("entities",            c_ENTITIES),
            new SchemaDatatypeMap("entity",              c_ENTITY),
            new SchemaDatatypeMap("enumeration",         c_ENUMERATION),
            new SchemaDatatypeMap("fixed.14.4",          c_fixed),
            new SchemaDatatypeMap("float",               c_doubleXdr),
            new SchemaDatatypeMap("float.ieee.754.32",   c_floatXdr),
            new SchemaDatatypeMap("float.ieee.754.64",   c_doubleXdr),
            new SchemaDatatypeMap("i1",                  c_byte),
            new SchemaDatatypeMap("i2",                  c_short),
            new SchemaDatatypeMap("i4",                  c_int),
            new SchemaDatatypeMap("i8",                  c_long),
            new SchemaDatatypeMap("id",                  c_ID),
            new SchemaDatatypeMap("idref",               c_IDREF),
            new SchemaDatatypeMap("idrefs",              c_IDREFS),
            new SchemaDatatypeMap("int",                 c_int),
            new SchemaDatatypeMap("nmtoken",             c_NMTOKEN),
            new SchemaDatatypeMap("nmtokens",            c_NMTOKENS),
            new SchemaDatatypeMap("notation",            c_NOTATION),
            new SchemaDatatypeMap("number",              c_doubleXdr),
            new SchemaDatatypeMap("r4",                  c_floatXdr),
            new SchemaDatatypeMap("r8",                  c_doubleXdr),
            new SchemaDatatypeMap("string",              c_string),
            new SchemaDatatypeMap("time",                c_timeNoTz),
            new SchemaDatatypeMap("time.tz",             c_timeTz),
            new SchemaDatatypeMap("ui1",                 c_unsignedByte),
            new SchemaDatatypeMap("ui2",                 c_unsignedShort),
            new SchemaDatatypeMap("ui4",                 c_unsignedInt),
            new SchemaDatatypeMap("ui8",                 c_unsignedLong),
            new SchemaDatatypeMap("uri",                 c_anyURI),
            new SchemaDatatypeMap("uuid",                c_uuid)
        };


        private static readonly SchemaDatatypeMap[] c_XsdTypes = {
            new SchemaDatatypeMap("ENTITIES",           c_ENTITIES, 11),
            new SchemaDatatypeMap("ENTITY",             c_ENTITY, 11),
            new SchemaDatatypeMap("ID",                 c_ID, 5),
            new SchemaDatatypeMap("IDREF",              c_IDREF, 5),
            new SchemaDatatypeMap("IDREFS",             c_IDREFS, 11),

            new SchemaDatatypeMap("NCName",             c_NCName, 9),
            new SchemaDatatypeMap("NMTOKEN",            c_NMTOKEN, 40),
            new SchemaDatatypeMap("NMTOKENS",           c_NMTOKENS, 11),
            new SchemaDatatypeMap("NOTATION",           c_NOTATION, 11),

            new SchemaDatatypeMap("Name",               c_Name, 40),
            new SchemaDatatypeMap("QName",              c_QName, 11), //-> 10

            new SchemaDatatypeMap("anySimpleType",      c_anySimpleType, -1),
            new SchemaDatatypeMap("anyURI",             c_anyURI, 11),
            new SchemaDatatypeMap("base64Binary",       c_base64Binary, 11),
            new SchemaDatatypeMap("boolean",            c_boolean, 11),
            new SchemaDatatypeMap("byte",               c_byte, 37),
            new SchemaDatatypeMap("date",               c_date, 11),
            new SchemaDatatypeMap("dateTime",           c_dateTime, 11),
            new SchemaDatatypeMap("decimal",            c_decimal, 11),
            new SchemaDatatypeMap("double",             c_double, 11),
            new SchemaDatatypeMap("duration",           c_duration, 11), //->20

            new SchemaDatatypeMap("float",              c_float, 11),
            new SchemaDatatypeMap("gDay",               c_day, 11),
            new SchemaDatatypeMap("gMonth",             c_month, 11),
            new SchemaDatatypeMap("gMonthDay",          c_monthDay, 11),
            new SchemaDatatypeMap("gYear",              c_year, 11),
            new SchemaDatatypeMap("gYearMonth",         c_yearMonth, 11),
            new SchemaDatatypeMap("hexBinary",          c_hexBinary, 11),
            new SchemaDatatypeMap("int",                c_int, 31),
            new SchemaDatatypeMap("integer",            c_integer, 18),
            new SchemaDatatypeMap("language",           c_language, 40), //->30
            new SchemaDatatypeMap("long",               c_long, 29),

            new SchemaDatatypeMap("negativeInteger",    c_negativeInteger, 34),

            new SchemaDatatypeMap("nonNegativeInteger", c_nonNegativeInteger, 29),
            new SchemaDatatypeMap("nonPositiveInteger", c_nonPositiveInteger, 29),
            new SchemaDatatypeMap("normalizedString",   c_normalizedString, 38),

            new SchemaDatatypeMap("positiveInteger",    c_positiveInteger, 33),

            new SchemaDatatypeMap("short",              c_short, 28),
            new SchemaDatatypeMap("string",             c_string, 11),
            new SchemaDatatypeMap("time",               c_time, 11),
            new SchemaDatatypeMap("token",              c_token, 35), //->40
            new SchemaDatatypeMap("unsignedByte",       c_unsignedByte, 44),
            new SchemaDatatypeMap("unsignedInt",        c_unsignedInt, 43),
            new SchemaDatatypeMap("unsignedLong",       c_unsignedLong, 33),
            new SchemaDatatypeMap("unsignedShort",      c_unsignedShort, 42),
        };

        protected int Compare(byte[] value1, byte[] value2) {
            int length = value1.Length;
            if (length != value2.Length) {
                return -1;
            }
            for (int i = 0; i < length; i ++) {
                if (value1[i] != value2[i]) {
                    return -1;
                }
            }
            return 0;
        }

#if Microsoft
        protected object GetValueToCheck(object value, IXmlNamespaceResolver nsmgr) {
            object valueToCheck = value;
            string resId;
            if (CanConvert(value, value.GetType(), this.ValueType, out resId)) {
                valueToCheck = this.ValueConverter.ChangeType(value, this.ValueType, nsmgr);
            }
            else {
                throw new XmlSchemaException(resId, string.Empty);
            }
            return valueToCheck;
        }
#endif

    }


    //List type
    internal class Datatype_List : Datatype_anySimpleType {
        DatatypeImplementation itemType;
        int minListSize;

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            XmlSchemaType listItemType = null;
            XmlSchemaSimpleType simpleType;
            XmlSchemaComplexType complexType;
            complexType = schemaType as XmlSchemaComplexType;

            if (complexType != null) {
                do {
                    simpleType = complexType.BaseXmlSchemaType as XmlSchemaSimpleType;
                    if (simpleType != null) {
                        break;
                    }
                    complexType = complexType.BaseXmlSchemaType as XmlSchemaComplexType;
                } while(complexType != null && complexType != XmlSchemaComplexType.AnyType);
            }
            else {
                simpleType = schemaType as XmlSchemaSimpleType;
            }
            if (simpleType != null) {
                do {
                    XmlSchemaSimpleTypeList listType = simpleType.Content as XmlSchemaSimpleTypeList;
                    if (listType != null) {
                        listItemType = listType.BaseItemType;
                        break;
                    }
                    simpleType = simpleType.BaseXmlSchemaType as XmlSchemaSimpleType;
                } while (simpleType != null && simpleType != DatatypeImplementation.AnySimpleType);
            }

            if (listItemType == null) { //Get built-in simple type for the typecode
                listItemType = DatatypeImplementation.GetSimpleTypeFromTypeCode(schemaType.Datatype.TypeCode);
            }

            return XmlListConverter.Create(listItemType.ValueConverter);
        }

        internal Datatype_List(DatatypeImplementation type) : this(type, 0) {
        }
        internal Datatype_List(DatatypeImplementation type, int minListSize) {
            this.itemType = type;
            this.minListSize = minListSize;
        }

        internal override int Compare(object value1, object value2) {
            System.Array arr1 = (System.Array)value1;
            System.Array arr2 = (System.Array)value2;

            Debug.Assert(arr1 != null && arr2 != null);
            int length = arr1.Length;
            if (length != arr2.Length) {
                return -1;
            }
            XmlAtomicValue[] atomicValues1 = arr1 as XmlAtomicValue[];
            if (atomicValues1 != null) {
                XmlAtomicValue[] atomicValues2 = arr2 as XmlAtomicValue[];
                Debug.Assert(atomicValues2 != null);
                XmlSchemaType xmlType1;
                for (int i =0; i < atomicValues1.Length; i++) {
                    xmlType1 = atomicValues1[i].XmlType;
                    if (xmlType1 != atomicValues2[i].XmlType || !xmlType1.Datatype.IsEqual(atomicValues1[i].TypedValue, atomicValues2[i].TypedValue) ) {
                        return -1;
                    }
                }
                return 0;
            }
            else {
                for (int i = 0; i < arr1.Length; i ++) {
                    if ( itemType.Compare(arr1.GetValue(i), arr2.GetValue(i)) != 0) {
                        return -1;
                    }
                }
                return 0;
            }
        }

        public override Type ValueType { get { return  ListValueType; }}

        public override XmlTokenizedType TokenizedType { get { return itemType.TokenizedType;}}

        internal override Type ListValueType { get { return itemType.ListValueType; }}

        internal override FacetsChecker FacetsChecker { get { return listFacetsChecker; } }

        public override XmlTypeCode TypeCode {
            get {
                return itemType.TypeCode;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|RestrictionFlags.MinLength|RestrictionFlags.MaxLength|RestrictionFlags.Enumeration|RestrictionFlags.WhiteSpace | RestrictionFlags.Pattern;
            }
        }
        internal DatatypeImplementation ItemType { get { return itemType; }}

        internal override Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue) {
            Exception exception;
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            string s = value as string;
            typedValue = null;
            if (s != null) {
                return TryParseValue(s, nameTable, namespaceResolver, out typedValue);
            }

            try {
                object valueToCheck = this.ValueConverter.ChangeType(value, this.ValueType, namespaceResolver);
                Array valuesToCheck = valueToCheck as Array;
                Debug.Assert(valuesToCheck != null);

                bool checkItemLexical = itemType.HasLexicalFacets;
                bool checkItemValue = itemType.HasValueFacets;
                object item;
                FacetsChecker itemFacetsChecker = itemType.FacetsChecker;
                XmlValueConverter itemValueConverter = itemType.ValueConverter;

                for (int i =0; i < valuesToCheck.Length; i++) {
                    item = valuesToCheck.GetValue(i);
                    if (checkItemLexical) {
                        string s1 = (string)itemValueConverter.ChangeType(item, typeof(System.String), namespaceResolver);
                        exception = itemFacetsChecker.CheckLexicalFacets(ref s1, itemType);
                        if (exception != null) goto Error;
                    }
                    if (checkItemValue) {
                        exception = itemFacetsChecker.CheckValueFacets(item, itemType);
                        if (exception != null) goto Error;
                    }
                }

                //Check facets on the list itself
                if (this.HasLexicalFacets) {
                    string s1 = (string)this.ValueConverter.ChangeType(valueToCheck, typeof(System.String), namespaceResolver);
                    exception = listFacetsChecker.CheckLexicalFacets(ref s1, this);
                    if (exception != null) goto Error;
                }
                if (this.HasValueFacets) {
                    exception = listFacetsChecker.CheckValueFacets(valueToCheck, this);
                    if (exception != null) goto Error;
                }
                typedValue = valueToCheck;
                return null;
            }
            catch (FormatException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (InvalidCastException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (OverflowException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (ArgumentException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }

        Error:
            return exception;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = listFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            ArrayList values = new ArrayList();
            object array;
            if (itemType.Variety == XmlSchemaDatatypeVariety.Union) {
                object unionTypedValue;
                string[] splitString = XmlConvert.SplitString(s);
                for (int i = 0; i < splitString.Length; ++i) {
                    //Parse items in list according to the itemType
                    exception = itemType.TryParseValue(splitString[i], nameTable, nsmgr, out unionTypedValue);
                    if (exception != null) goto Error;

                    XsdSimpleValue simpleValue = (XsdSimpleValue)unionTypedValue;
                    values.Add(new XmlAtomicValue(simpleValue.XmlType, simpleValue.TypedValue, nsmgr));
                }
                array = values.ToArray(typeof(XmlAtomicValue));
            }
            else { //Variety == List or Atomic
                string[] splitString = XmlConvert.SplitString(s);
                for (int i = 0; i < splitString.Length; ++i) {
                    exception = itemType.TryParseValue(splitString[i], nameTable, nsmgr, out typedValue);
                    if (exception != null) goto Error;

                    values.Add(typedValue);
                }
                array = values.ToArray(itemType.ValueType);
                Debug.Assert(array.GetType() == ListValueType);
            }
            if (values.Count < minListSize) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = listFacetsChecker.CheckValueFacets(array, this);
            if (exception != null) goto Error;

            typedValue = array;

            return null;

        Error:
            return exception;
        }

    }

    //Union datatype
    internal class Datatype_union : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(object);
        static readonly Type listValueType = typeof(object[]);
        XmlSchemaSimpleType[] types;

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlUnionConverter.Create(schemaType);
        }

        internal Datatype_union(XmlSchemaSimpleType[] types) {
            this.types = types;
        }

        internal override int Compare(object value1, object value2) {
            XsdSimpleValue simpleValue1 = value1 as XsdSimpleValue;
            XsdSimpleValue simpleValue2 = value2 as XsdSimpleValue;

            if (simpleValue1 == null || simpleValue2 == null) {
                return -1;
            }
            XmlSchemaType schemaType1 = simpleValue1.XmlType;
            XmlSchemaType schemaType2 = simpleValue2.XmlType;

            if (schemaType1 == schemaType2) {
                XmlSchemaDatatype datatype = schemaType1.Datatype;
                return datatype.Compare(simpleValue1.TypedValue, simpleValue2.TypedValue);
            }
            return -1;
        }

        public override Type ValueType { get { return atomicValueType; }}

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.AnyAtomicType; }}

        internal override FacetsChecker FacetsChecker { get { return unionFacetsChecker; } }

        internal override Type ListValueType { get { return listValueType; }}

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                    RestrictionFlags.Enumeration;
            }
        }

        internal XmlSchemaSimpleType[] BaseMemberTypes {
            get {
                return types;
            }
        }

        internal bool HasAtomicMembers() {
            for (int i = 0; i < types.Length; ++i) {
                if (types[i].Datatype.Variety == XmlSchemaDatatypeVariety.List) {
                    return false;
                }
            }
            return true;
        }

        internal bool IsUnionBaseOf(DatatypeImplementation derivedType) {
            for (int i = 0; i < types.Length; ++i) {
                if (derivedType.IsDerivedFrom(types[i].Datatype)) {
                    return true;
                }
            }
            return false;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            XmlSchemaSimpleType memberType = null;

            typedValue = null;

            exception = unionFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            //Parse string to CLR value
            for (int i = 0; i < types.Length; ++i) {
                exception = types[i].Datatype.TryParseValue(s, nameTable, nsmgr, out typedValue);
                if (exception == null) {
                    memberType = types[i];
                    break;
                }
            }
            if (memberType == null) {
                exception = new XmlSchemaException(Res.Sch_UnionFailedEx, s);
                goto Error;
            }

            typedValue = new XsdSimpleValue(memberType, typedValue);
            exception = unionFacetsChecker.CheckValueFacets(typedValue, this);
            if (exception != null) goto Error;

            return null;

        Error:
            return exception;
        }

        internal override Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            typedValue = null;
            string s = value as string;
            if (s != null) {
                return TryParseValue(s, nameTable, nsmgr, out typedValue);
            }

            object valueToCheck = null;
            XmlSchemaSimpleType memberType = null;
            for (int i = 0; i < types.Length; ++i) {
                if (types[i].Datatype.TryParseValue(value, nameTable, nsmgr, out valueToCheck) == null) { //no error
                    memberType = types[i];
                    break;
                }
            }
            if (valueToCheck == null) {
                exception = new XmlSchemaException(Res.Sch_UnionFailedEx, value.ToString());
                goto Error;
            }
            try {
                if (this.HasLexicalFacets) {
                    string s1 = (string)this.ValueConverter.ChangeType(valueToCheck, typeof(System.String), nsmgr); //Using value here to avoid info loss
                    exception = unionFacetsChecker.CheckLexicalFacets(ref s1, this);
                    if (exception != null) goto Error;
                }
                typedValue = new XsdSimpleValue(memberType, valueToCheck);
                if (this.HasValueFacets) {
                    exception = unionFacetsChecker.CheckValueFacets(typedValue, this);
                    if (exception != null) goto Error;
                }
                return null;
            }
            catch (FormatException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (InvalidCastException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (OverflowException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }
            catch (ArgumentException e) { //Catching for exceptions thrown by ValueConverter
                exception = e;
            }

        Error:
            return exception;
        }
    }


    // Primitive datatypes
    internal class Datatype_anySimpleType : DatatypeImplementation {
        static readonly Type atomicValueType = typeof(string);
        static readonly Type listValueType = typeof(string[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlUntypedConverter.Untyped;
        }

        internal override FacetsChecker FacetsChecker { get { return miscFacetsChecker; } }

        public override Type ValueType { get { return atomicValueType; }}

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.AnyAtomicType; }}

        internal override Type ListValueType { get { return listValueType; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.None;}}

        internal override RestrictionFlags ValidRestrictionFlags { get { return 0;}}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override int Compare(object value1, object value2) {
            //Changed StringComparison.CurrentCulture to StringComparison.Ordinal to handle zero-weight code points like the cyrillic E
            return String.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            typedValue = XmlComplianceUtil.NonCDataNormalize(s); //Whitespace facet is treated as collapse since thats the way it was in Everett
            return null;
        }
    }

    internal class Datatype_anyAtomicType : Datatype_anySimpleType {
        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlAnyConverter.AnyAtomic;
        }
        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Preserve; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.AnyAtomicType; } }

    }

    internal class Datatype_untypedAtomicType : Datatype_anyAtomicType {
        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlUntypedConverter.Untyped;
        }
        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Preserve; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.UntypedAtomic; } }

    }


    /*
      <xs:simpleType name="string" id="string">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality" value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
                    source="http://www.w3.org/TR/xmlschema-2/#string"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="preserve" id="string.preserve"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_string : Datatype_anySimpleType {
        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlStringConverter.Create(schemaType);
        }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Preserve; } }

        internal override FacetsChecker FacetsChecker { get { return stringFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.String; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.CDATA;}}

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = stringFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            exception = stringFacetsChecker.CheckValueFacets(s, this);
            if (exception != null) goto Error;

            typedValue = s;
            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="boolean" id="boolean">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality" value="finite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#boolean"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse" fixed="true"
            id="boolean.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_boolean : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(bool);
        static readonly Type listValueType = typeof(bool[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlBooleanConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return miscFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Boolean; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                       RestrictionFlags.WhiteSpace;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((bool)value1).CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            typedValue = null;

            exception = miscFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            bool boolValue;
            exception = XmlConvert.TryToBoolean(s, out boolValue);
            if (exception != null) goto Error;

            typedValue = boolValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="float" id="float">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="total"/>
            <hfp:hasProperty name="bounded" value="true"/>
            <hfp:hasProperty name="cardinality" value="finite"/>
            <hfp:hasProperty name="numeric" value="true"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#float"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse" fixed="true"
            id="float.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_float : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(float);
        static readonly Type listValueType = typeof(float[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlNumeric2Converter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return numeric2FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Float; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace|
                       RestrictionFlags.MinExclusive|
                       RestrictionFlags.MinInclusive|
                       RestrictionFlags.MaxExclusive|
                       RestrictionFlags.MaxInclusive;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((float)value1).CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric2FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            float singleValue;
            exception = XmlConvert.TryToSingle(s, out singleValue);
            if (exception != null) goto Error;

            exception = numeric2FacetsChecker.CheckValueFacets(singleValue, this);
            if (exception != null) goto Error;

            typedValue = singleValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="double" id="double">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="total"/>
            <hfp:hasProperty name="bounded" value="true"/>
            <hfp:hasProperty name="cardinality" value="finite"/>
            <hfp:hasProperty name="numeric" value="true"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#double"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="double.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_double : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(double);
        static readonly Type listValueType = typeof(double[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlNumeric2Converter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return numeric2FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Double; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace|
                       RestrictionFlags.MinExclusive|
                       RestrictionFlags.MinInclusive|
                       RestrictionFlags.MaxExclusive|
                       RestrictionFlags.MaxInclusive;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((double)value1).CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            typedValue = null;

            exception = numeric2FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            double doubleValue;
            exception = XmlConvert.TryToDouble(s, out doubleValue);
            if (exception != null) goto Error;

            exception = numeric2FacetsChecker.CheckValueFacets(doubleValue, this);
            if (exception != null) goto Error;

            typedValue = doubleValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="decimal" id="decimal">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="totalDigits"/>
            <hfp:hasFacet name="fractionDigits"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="total"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="true"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#decimal"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="decimal.whiteSpace"/>
        </xs:restriction>
       </xs:simpleType>
    */
    internal class Datatype_decimal : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(decimal);
        static readonly Type listValueType = typeof(decimal[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(decimal.MinValue, decimal.MaxValue);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlNumeric10Converter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Decimal; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.TotalDigits|
                       RestrictionFlags.FractionDigits|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace|
                       RestrictionFlags.MinExclusive|
                       RestrictionFlags.MinInclusive|
                       RestrictionFlags.MaxExclusive|
                       RestrictionFlags.MaxInclusive;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((decimal)value1).CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            decimal decimalValue;
            exception = XmlConvert.TryToDecimal(s, out decimalValue);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets(decimalValue, this);
            if (exception != null) goto Error;

            typedValue = decimalValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
       <xs:simpleType name="duration" id="duration">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#duration"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="duration.whiteSpace"/>
        </xs:restriction>
       </xs:simpleType>
    */
    internal class Datatype_duration : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(TimeSpan);
        static readonly Type listValueType = typeof(TimeSpan[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return durationFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Duration; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace|
                       RestrictionFlags.MinExclusive|
                       RestrictionFlags.MinInclusive|
                       RestrictionFlags.MaxExclusive|
                       RestrictionFlags.MaxInclusive;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((TimeSpan)value1).CompareTo(value2);
        }


        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            typedValue = null;

            if (s == null || s.Length == 0) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = durationFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            TimeSpan timeSpanValue;
            exception = XmlConvert.TryToTimeSpan(s, out timeSpanValue);
            if (exception != null) goto Error;

            exception = durationFacetsChecker.CheckValueFacets(timeSpanValue, this);
            if (exception != null) goto Error;

            typedValue = timeSpanValue;

            return null;

        Error:
            return exception;
        }
    }

    internal class Datatype_yearMonthDuration : Datatype_duration {

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            typedValue = null;

            if (s == null || s.Length == 0) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = durationFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            XsdDuration duration;
            exception = XsdDuration.TryParse(s, XsdDuration.DurationType.YearMonthDuration, out duration);
            if (exception != null) goto Error;

            TimeSpan timeSpanValue;

            exception = duration.TryToTimeSpan(XsdDuration.DurationType.YearMonthDuration, out timeSpanValue);
            if (exception != null) goto Error;

            exception = durationFacetsChecker.CheckValueFacets(timeSpanValue, this);
            if (exception != null) goto Error;

            typedValue = timeSpanValue;

            return null;

        Error:
            return exception;
        }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.YearMonthDuration; } }

    }

    internal class Datatype_dayTimeDuration : Datatype_duration {

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            if (s == null || s.Length == 0) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = durationFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            XsdDuration duration;
            exception = XsdDuration.TryParse(s, XsdDuration.DurationType.DayTimeDuration, out duration);
            if (exception != null) goto Error;

            TimeSpan timeSpanValue;
            exception = duration.TryToTimeSpan(XsdDuration.DurationType.DayTimeDuration, out timeSpanValue);
            if (exception != null) goto Error;

            exception = durationFacetsChecker.CheckValueFacets(timeSpanValue, this);
            if (exception != null) goto Error;

            typedValue = timeSpanValue;

            return null;

        Error:
            return exception;
        }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.DayTimeDuration; } }

    }

    internal class Datatype_dateTimeBase : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(DateTime);
        static readonly Type listValueType = typeof(DateTime[]);
        private XsdDateTimeFlags dateTimeFlags;

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlDateTimeConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return dateTimeFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.DateTime; }}

        internal Datatype_dateTimeBase() {
        }

        internal Datatype_dateTimeBase(XsdDateTimeFlags dateTimeFlags) {
            this.dateTimeFlags = dateTimeFlags;
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace|
                       RestrictionFlags.MinExclusive|
                       RestrictionFlags.MinInclusive|
                       RestrictionFlags.MaxExclusive|
                       RestrictionFlags.MaxInclusive;
            }
        }

        internal override int Compare(object value1, object value2) {
            DateTime dateTime1 = (DateTime)value1;
            DateTime dateTime2 = (DateTime)value2;
            if (dateTime1.Kind == DateTimeKind.Unspecified || dateTime2.Kind == DateTimeKind.Unspecified) { //If either of them are unspecified, do not convert zones
                return dateTime1.CompareTo(dateTime2);
            }
            dateTime1 = dateTime1.ToUniversalTime();
            return dateTime1.CompareTo(dateTime2.ToUniversalTime());
        }

         internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;
            typedValue = null;

            exception = dateTimeFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            XsdDateTime dateTime;
            if (!XsdDateTime.TryParse(s, dateTimeFlags, out dateTime)) {
                exception = new FormatException(Res.GetString(Res.XmlConvert_BadFormat, s, dateTimeFlags.ToString()));
                goto Error;
            }

            DateTime dateTimeValue = DateTime.MinValue;
            try {
                dateTimeValue = (DateTime)dateTime;
            }
            catch (ArgumentException e) {
                exception = e;
                goto Error;
            }

            exception = dateTimeFacetsChecker.CheckValueFacets(dateTimeValue, this);
            if (exception != null) goto Error;

            typedValue = dateTimeValue;

            return null;

        Error:
            return exception;
        }
    }

    internal class Datatype_dateTimeNoTimeZone : Datatype_dateTimeBase {
        internal Datatype_dateTimeNoTimeZone() : base(XsdDateTimeFlags.XdrDateTimeNoTz) { }
    }

    internal class Datatype_dateTimeTimeZone : Datatype_dateTimeBase {
        internal Datatype_dateTimeTimeZone() : base(XsdDateTimeFlags.XdrDateTime) {}
    }

    /*
      <xs:simpleType name="dateTime" id="dateTime">
       <xs:annotation>
        <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#dateTime"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="dateTime.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_dateTime : Datatype_dateTimeBase {
        internal Datatype_dateTime() : base(XsdDateTimeFlags.DateTime) {}
    }

    internal class Datatype_timeNoTimeZone : Datatype_dateTimeBase {
        internal Datatype_timeNoTimeZone() : base(XsdDateTimeFlags.XdrTimeNoTz) { }
    }

    internal class Datatype_timeTimeZone : Datatype_dateTimeBase {
        internal Datatype_timeTimeZone() : base(XsdDateTimeFlags.Time) { }
    }

    /*
      <xs:simpleType name="time" id="time">
        <xs:annotation>
        <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#time"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="time.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_time : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Time; }}

        internal Datatype_time() : base(XsdDateTimeFlags.Time) {}
    }

    /*
      <xs:simpleType name="date" id="date">
       <xs:annotation>
        <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#date"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="date.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_date : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Date; }}

        internal Datatype_date() : base(XsdDateTimeFlags.Date) { }
    }

    /*
      <xs:simpleType name="gYearMonth" id="gYearMonth">
       <xs:annotation>
        <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#gYearMonth"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="gYearMonth.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_yearMonth : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.GYearMonth; }}

        internal Datatype_yearMonth() : base(XsdDateTimeFlags.GYearMonth) { }
    }


    /*
      <xs:simpleType name="gYear" id="gYear">
        <xs:annotation>
        <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#gYear"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="gYear.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_year : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.GYear; }}

        internal Datatype_year() : base(XsdDateTimeFlags.GYear) { }
    }

    /*
     <xs:simpleType name="gMonthDay" id="gMonthDay">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
           <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#gMonthDay"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
             <xs:whiteSpace value="collapse" fixed="true"
                    id="gMonthDay.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_monthDay : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.GMonthDay; }}

        internal Datatype_monthDay() : base(XsdDateTimeFlags.GMonthDay) { }
    }

    /*
      <xs:simpleType name="gDay" id="gDay">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#gDay"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
             <xs:whiteSpace value="collapse"  fixed="true"
                    id="gDay.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_day : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.GDay; }}

        internal Datatype_day() : base(XsdDateTimeFlags.GDay) { }
    }


    /*
     <xs:simpleType name="gMonth" id="gMonth">
        <xs:annotation>
      <xs:appinfo>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasFacet name="maxInclusive"/>
            <hfp:hasFacet name="maxExclusive"/>
            <hfp:hasFacet name="minInclusive"/>
            <hfp:hasFacet name="minExclusive"/>
            <hfp:hasProperty name="ordered" value="partial"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#gMonth"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
             <xs:whiteSpace value="collapse"  fixed="true"
                    id="gMonth.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_month : Datatype_dateTimeBase {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.GMonth; }}

        internal Datatype_month() : base(XsdDateTimeFlags.GMonth) { }
    }

    /*
       <xs:simpleType name="hexBinary" id="hexBinary">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#binary"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse" fixed="true"
            id="hexBinary.whiteSpace"/>
        </xs:restriction>
       </xs:simpleType>
    */
    internal class Datatype_hexBinary : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(byte[]);
        static readonly Type listValueType = typeof(byte[][]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return binaryFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.HexBinary; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        internal override int Compare(object value1, object value2) {
            return Compare((byte[])value1, (byte[])value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = binaryFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            byte[] byteArrayValue = null;
            try {
                byteArrayValue = XmlConvert.FromBinHexString(s, false);
            }
            catch (ArgumentException e) {
                exception = e;
                goto Error;
            }
            catch (XmlException e) {
                exception = e;
                goto Error;
            }

            exception = binaryFacetsChecker.CheckValueFacets(byteArrayValue, this);
            if (exception != null) goto Error;

            typedValue = byteArrayValue;

            return null;

        Error:
            return exception;
        }
    }


    /*
     <xs:simpleType name="base64Binary" id="base64Binary">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
                    source="http://www.w3.org/TR/xmlschema-2/#base64Binary"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse" fixed="true"
            id="base64Binary.whiteSpace"/>
        </xs:restriction>
       </xs:simpleType>
    */
    internal class Datatype_base64Binary : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(byte[]);
        static readonly Type listValueType = typeof(byte[][]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return binaryFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Base64Binary; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        internal override int Compare(object value1, object value2) {
            return Compare((byte[])value1, (byte[])value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = binaryFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            byte[] byteArrayValue = null;
            try {
                byteArrayValue = Convert.FromBase64String(s);
            }
            catch (ArgumentException e) {
                exception = e;
                goto Error;
            }
            catch (FormatException e) {
                exception = e;
                goto Error;
            }

            exception = binaryFacetsChecker.CheckValueFacets(byteArrayValue, this);
            if (exception != null) goto Error;

            typedValue = byteArrayValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
       <xs:simpleType name="anyURI" id="anyURI">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#anyURI"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="anyURI.whiteSpace"/>
        </xs:restriction>
       </xs:simpleType>
    */
    internal class Datatype_anyURI : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(Uri);
        static readonly Type listValueType = typeof(Uri[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return stringFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.AnyUri; }}

        public override Type ValueType { get { return atomicValueType; }}

        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check validity of Uri
            }
        }
        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        internal override int Compare(object value1, object value2) {
            return ((Uri)value1).Equals((Uri)value2) ? 0 : -1;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = stringFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            Uri uri;
            exception = XmlConvert.TryToUri(s, out uri);
            if (exception != null) goto Error;

            string stringValue = uri.OriginalString;
            exception = ((StringFacetsChecker)stringFacetsChecker).CheckValueFacets(stringValue, this, false);
            if (exception != null) goto Error;

            typedValue = uri;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="QName" id="QName">
        <xs:annotation>
            <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#QName"/>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="QName.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_QName : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(XmlQualifiedName);
        static readonly Type listValueType = typeof(XmlQualifiedName[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return qnameFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.QName; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.QName;}}

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            if (s == null || s.Length == 0) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = qnameFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            XmlQualifiedName qname = null;
            try {
                string prefix;
                qname = XmlQualifiedName.Parse(s, nsmgr, out prefix);
            }
            catch (ArgumentException e) {
                exception = e;
                goto Error;
            }
            catch (XmlException e) {
                exception = e;
                goto Error;
            }

            exception = qnameFacetsChecker.CheckValueFacets(qname, this);
            if (exception != null) goto Error;

            typedValue = qname;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="normalizedString" id="normalizedString">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#normalizedString"/>
        </xs:annotation>
        <xs:restriction base="xs:string">
          <xs:whiteSpace value="replace"
            id="normalizedString.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_normalizedString : Datatype_string {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NormalizedString; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Replace; } }

        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check validity of NormalizedString
            }
        }
    }

    internal class Datatype_normalizedStringV1Compat : Datatype_string {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NormalizedString; }}
        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check validity of NormalizedString
            }
        }
    }

    /*
      <xs:simpleType name="token" id="token">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#token"/>
        </xs:annotation>
        <xs:restriction base="xs:normalizedString">
          <xs:whiteSpace value="collapse" id="token.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_token : Datatype_normalizedString {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Token; }}
        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }
    }

    internal class Datatype_tokenV1Compat : Datatype_normalizedStringV1Compat {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Token; }}
    }

    /*
      <xs:simpleType name="language" id="language">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#language"/>
        </xs:annotation>
        <xs:restriction base="xs:token">
          <xs:pattern
            value="([a-zA-Z]{2}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*"
                    id="language.pattern">
            <xs:annotation>
              <xs:documentation
                    source="http://www.w3.org/TR/REC-xml#NT-LanguageID">
                pattern specifies the content of section 2.12 of XML 1.0e2
                and RFC 1766
              </xs:documentation>
            </xs:annotation>
          </xs:pattern>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_language : Datatype_token {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Language; }}
    }

    /*
      <xs:simpleType name="NMTOKEN" id="NMTOKEN">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#NMTOKEN"/>
        </xs:annotation>
        <xs:restriction base="xs:token">
          <xs:pattern value="\c+" id="NMTOKEN.pattern">
            <xs:annotation>
              <xs:documentation
                    source="http://www.w3.org/TR/REC-xml#NT-Nmtoken">
                pattern matches production 7 from the XML spec
              </xs:documentation>
            </xs:annotation>
          </xs:pattern>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_NMTOKEN : Datatype_token {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NmToken; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.NMTOKEN;}}
    }

    /*
      <xs:simpleType name="Name" id="Name">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#Name"/>
        </xs:annotation>
        <xs:restriction base="xs:token">
          <xs:pattern value="\i\c*" id="Name.pattern">
            <xs:annotation>
              <xs:documentation
                            source="http://www.w3.org/TR/REC-xml#NT-Name">
                pattern matches production 5 from the XML spec
              </xs:documentation>
            </xs:annotation>
          </xs:pattern>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_Name : Datatype_token {
        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Name; }}
    }

    /*
      <xs:simpleType name="NCName" id="NCName">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#NCName"/>
        </xs:annotation>
        <xs:restriction base="xs:Name">
          <xs:pattern value="[\i-[:]][\c-[:]]*" id="NCName.pattern">
            <xs:annotation>
              <xs:documentation
                    source="http://www.w3.org/TR/REC-xml-names/#NT-NCName">
                pattern matches production 4 from the Namespaces in XML spec
              </xs:documentation>
            </xs:annotation>
          </xs:pattern>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_NCName : Datatype_Name {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NCName; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = stringFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            exception = stringFacetsChecker.CheckValueFacets(s, this);
            if (exception != null) goto Error;

            nameTable.Add(s);

            typedValue = s;
            return null;

        Error:
            return exception;
        }
    }

    /*
       <xs:simpleType name="ID" id="ID">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#ID"/>
        </xs:annotation>
        <xs:restriction base="xs:NCName"/>
       </xs:simpleType>
    */
    internal class Datatype_ID : Datatype_NCName {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Id; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.ID;}}
    }

    /*
       <xs:simpleType name="IDREF" id="IDREF">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#IDREF"/>
        </xs:annotation>
        <xs:restriction base="xs:NCName"/>
       </xs:simpleType>
    */
    internal class Datatype_IDREF : Datatype_NCName {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Idref; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.IDREF;}}
    }

    /*
       <xs:simpleType name="ENTITY" id="ENTITY">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#ENTITY"/>
        </xs:annotation>
        <xs:restriction base="xs:NCName"/>
       </xs:simpleType>
    */
    internal class Datatype_ENTITY : Datatype_NCName {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Entity; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.ENTITY;}}

    }

    /*
       <xs:simpleType name="NOTATION" id="NOTATION">
        <xs:annotation>
            <xs:appinfo>
            <hfp:hasFacet name="length"/>
            <hfp:hasFacet name="minLength"/>
            <hfp:hasFacet name="maxLength"/>
            <hfp:hasFacet name="pattern"/>
            <hfp:hasFacet name="enumeration"/>
            <hfp:hasFacet name="whiteSpace"/>
            <hfp:hasProperty name="ordered" value="false"/>
            <hfp:hasProperty name="bounded" value="false"/>
            <hfp:hasProperty name="cardinality"
                    value="countably infinite"/>
            <hfp:hasProperty name="numeric" value="false"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#NOTATION"/>
          <xs:documentation>
            NOTATION cannot be used directly in a schema; rather a type
            must be derived from it by specifying at least one enumeration
            facet whose value is the name of a NOTATION declared in the
            schema.
          </xs:documentation>
        </xs:annotation>
        <xs:restriction base="xs:anySimpleType">
          <xs:whiteSpace value="collapse"  fixed="true"
            id="NOTATION.whiteSpace"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_NOTATION : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(XmlQualifiedName);
        static readonly Type listValueType = typeof(XmlQualifiedName[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType) {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override FacetsChecker FacetsChecker { get { return qnameFacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Notation; }}

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.NOTATION;}}

        internal override RestrictionFlags ValidRestrictionFlags {
            get {
                return RestrictionFlags.Length|
                       RestrictionFlags.MinLength|
                       RestrictionFlags.MaxLength|
                       RestrictionFlags.Pattern|
                       RestrictionFlags.Enumeration|
                       RestrictionFlags.WhiteSpace;
            }
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get { return XmlSchemaWhiteSpace.Collapse; } }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            if (s == null || s.Length == 0) {
                return new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }

            exception = qnameFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            XmlQualifiedName qname = null;
            try {
                string prefix;
                qname = XmlQualifiedName.Parse(s, nsmgr, out prefix);
            }
            catch (ArgumentException e) {
                exception = e;
                goto Error;
            }
            catch (XmlException e) {
                exception = e;
                goto Error;
            }

            exception = qnameFacetsChecker.CheckValueFacets(qname, this);
            if (exception != null) goto Error;

            typedValue = qname;

            return null;

        Error:
            return exception;
        }

        internal override void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller) {

            // Only datatypes that are derived from NOTATION by specifying a value for enumeration can be used in a schema.
            // Furthermore, the value of all enumeration facets must match the name of a notation declared in the current schema.                    //
            for(Datatype_NOTATION dt = this; dt != null; dt = (Datatype_NOTATION)dt.Base) {
                if (dt.Restriction != null && (dt.Restriction.Flags & RestrictionFlags.Enumeration) != 0) {
                    for (int i = 0; i < dt.Restriction.Enumeration.Count; ++i) {
                        XmlQualifiedName notation = (XmlQualifiedName)dt.Restriction.Enumeration[i];
                        if (!notations.Contains(notation)) {
                            throw new XmlSchemaException(Res.Sch_NotationRequired, caller);
                        }
                    }
                    return;
                }
            }
            throw new XmlSchemaException(Res.Sch_NotationRequired, caller);
        }
    }

    /*
      <xs:simpleType name="integer" id="integer">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#integer"/>
        </xs:annotation>
        <xs:restriction base="xs:decimal">
          <xs:fractionDigits value="0" fixed="true" id="integer.fractionDigits"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_integer : Datatype_decimal {

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Integer; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            decimal decimalValue;
            exception = XmlConvert.TryToInteger(s, out decimalValue);
            if (exception != null) goto Error;

            exception = FacetsChecker.CheckValueFacets(decimalValue, this);
            if (exception != null) goto Error;

            typedValue = decimalValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="nonPositiveInteger" id="nonPostiveInteger">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#negativeInteger"/>
        </xs:annotation>
        <xs:restriction base="xs:nonPositiveInteger">
          <xs:maxInclusive value="-1" id="negativeInteger.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_nonPositiveInteger : Datatype_integer {
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(decimal.MinValue, decimal.Zero);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NonPositiveInteger; }}

        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check range
            }
        }
    }


    /*
      <xs:simpleType name="negativeInteger" id="negativeInteger">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#negativeInteger"/>
        </xs:annotation>
        <xs:restriction base="xs:nonPositiveInteger">
          <xs:maxInclusive value="-1" id="negativeInteger.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_negativeInteger : Datatype_nonPositiveInteger {
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(decimal.MinValue, decimal.MinusOne);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NegativeInteger; }}
    }


    /*
      <xs:simpleType name="long" id="long">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasProperty name="bounded" value="true"/>
            <hfp:hasProperty name="cardinality" value="finite"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#long"/>
        </xs:annotation>
        <xs:restriction base="xs:integer">
          <xs:minInclusive value="-9223372036854775808" id="long.minInclusive"/>
          <xs:maxInclusive value="9223372036854775807" id="long.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_long : Datatype_integer {
        static readonly Type atomicValueType = typeof(long);
        static readonly Type listValueType = typeof(long[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(long.MinValue, long.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check range
            }
        }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Long; }}

        internal override int Compare(object value1, object value2) {
            return ((long)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            long int64Value;
            exception = XmlConvert.TryToInt64(s, out int64Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets(int64Value, this);
            if (exception != null) goto Error;

            typedValue = int64Value;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="int" id="int">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#int"/>
        </xs:annotation>
        <xs:restriction base="xs:long">
          <xs:minInclusive value="-2147483648" id="int.minInclusive"/>
          <xs:maxInclusive value="2147483647" id="int.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_int : Datatype_long {
        static readonly Type atomicValueType = typeof(int);
        static readonly Type listValueType = typeof(int[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(int.MinValue, int.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Int; }}

        internal override int Compare(object value1, object value2) {
            return ((int)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            int int32Value;
            exception = XmlConvert.TryToInt32(s, out int32Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets(int32Value, this);
            if (exception != null) goto Error;

            typedValue = int32Value;

            return null;

        Error:
            return exception;
        }
    }


    /*
      <xs:simpleType name="short" id="short">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#short"/>
        </xs:annotation>
        <xs:restriction base="xs:int">
          <xs:minInclusive value="-32768" id="short.minInclusive"/>
          <xs:maxInclusive value="32767" id="short.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_short : Datatype_int {
        static readonly Type atomicValueType = typeof(short);
        static readonly Type listValueType = typeof(short[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(short.MinValue, short.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Short; }}

        internal override int Compare(object value1, object value2) {
            return ((short)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            short int16Value;
            exception = XmlConvert.TryToInt16(s, out int16Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets(int16Value, this);
            if (exception != null) goto Error;

            typedValue = int16Value;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="byte" id="byte">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#byte"/>
        </xs:annotation>
        <xs:restriction base="xs:short">
          <xs:minInclusive value="-128" id="byte.minInclusive"/>
          <xs:maxInclusive value="127" id="byte.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_byte : Datatype_short {
        static readonly Type atomicValueType = typeof(sbyte);
        static readonly Type listValueType = typeof(sbyte[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(sbyte.MinValue, sbyte.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.Byte; }}

        internal override int Compare(object value1, object value2) {
            return ((sbyte)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            sbyte sbyteValue;
            exception = XmlConvert.TryToSByte(s, out sbyteValue);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets((short)sbyteValue, this);
            if (exception != null) goto Error;

            typedValue = sbyteValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="nonNegativeInteger" id="nonNegativeInteger">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#nonNegativeInteger"/>
        </xs:annotation>
        <xs:restriction base="xs:integer">
          <xs:minInclusive value="0" id="nonNegativeInteger.minInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_nonNegativeInteger : Datatype_integer {
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(decimal.Zero,decimal.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.NonNegativeInteger; }}

        internal override bool HasValueFacets {
            get {
                return true; //Built-in facet to check range
            }
        }
    }

    /*
      <xs:simpleType name="unsignedLong" id="unsignedLong">
        <xs:annotation>
          <xs:appinfo>
            <hfp:hasProperty name="bounded" value="true"/>
            <hfp:hasProperty name="cardinality" value="finite"/>
          </xs:appinfo>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#unsignedLong"/>
        </xs:annotation>
        <xs:restriction base="xs:nonNegativeInteger">
          <xs:maxInclusive value="18446744073709551615"
            id="unsignedLong.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_unsignedLong : Datatype_nonNegativeInteger {
        static readonly Type atomicValueType = typeof(ulong);
        static readonly Type listValueType = typeof(ulong[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(ulong.MinValue, ulong.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.UnsignedLong; }}

        internal override int Compare(object value1, object value2) {
            return ((ulong)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            ulong uint64Value;
            exception = XmlConvert.TryToUInt64(s, out uint64Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets((decimal)uint64Value, this);
            if (exception != null) goto Error;

            typedValue = uint64Value;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="unsignedInt" id="unsignedInt">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#unsignedInt"/>
        </xs:annotation>
        <xs:restriction base="xs:unsignedLong">
          <xs:maxInclusive value="4294967295"
            id="unsignedInt.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_unsignedInt : Datatype_unsignedLong {
        static readonly Type atomicValueType = typeof(uint);
        static readonly Type listValueType = typeof(uint[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(uint.MinValue, uint.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.UnsignedInt; }}

        internal override int Compare(object value1, object value2) {
            return ((uint)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            uint uint32Value;
            exception = XmlConvert.TryToUInt32(s, out uint32Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets((long)uint32Value, this);
            if (exception != null) goto Error;

            typedValue = uint32Value;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="unsignedShort" id="unsignedShort">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#unsignedShort"/>
        </xs:annotation>
        <xs:restriction base="xs:unsignedInt">
          <xs:maxInclusive value="65535"
            id="unsignedShort.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_unsignedShort : Datatype_unsignedInt {
        static readonly Type atomicValueType = typeof(ushort);
        static readonly Type listValueType = typeof(ushort[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(ushort.MinValue, ushort.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.UnsignedShort; }}

        internal override int Compare(object value1, object value2) {
            return ((ushort)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            ushort uint16Value;
            exception = XmlConvert.TryToUInt16(s, out uint16Value);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets((int)uint16Value, this);
            if (exception != null) goto Error;

            typedValue = uint16Value;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="unsignedByte" id="unsignedBtype">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#unsignedByte"/>
        </xs:annotation>
        <xs:restriction base="xs:unsignedShort">
          <xs:maxInclusive value="255" id="unsignedByte.maxInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_unsignedByte : Datatype_unsignedShort {
        static readonly Type atomicValueType = typeof(byte);
        static readonly Type listValueType = typeof(byte[]);
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(byte.MinValue, byte.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.UnsignedByte; }}

        internal override int Compare(object value1, object value2) {
            return ((byte)value1).CompareTo(value2);
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception != null) goto Error;

            byte byteValue;
            exception = XmlConvert.TryToByte(s, out byteValue);
            if (exception != null) goto Error;

            exception = numeric10FacetsChecker.CheckValueFacets((short)byteValue, this);
            if (exception != null) goto Error;

            typedValue = byteValue;

            return null;

        Error:
            return exception;
        }
    }

    /*
      <xs:simpleType name="positiveInteger" id="positiveInteger">
        <xs:annotation>
          <xs:documentation
            source="http://www.w3.org/TR/xmlschema-2/#positiveInteger"/>
        </xs:annotation>
        <xs:restriction base="xs:nonNegativeInteger">
          <xs:minInclusive value="1" id="positiveInteger.minInclusive"/>
        </xs:restriction>
      </xs:simpleType>
    */
    internal class Datatype_positiveInteger : Datatype_nonNegativeInteger {
        static readonly FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(decimal.One, decimal.MaxValue);

        internal override FacetsChecker FacetsChecker { get { return numeric10FacetsChecker; } }

        public override XmlTypeCode TypeCode { get { return XmlTypeCode.PositiveInteger; }}
    }

    /*
        XDR
    */
    internal class Datatype_doubleXdr : Datatype_double {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
             double value;
            try {
               value = XmlConvert.ToDouble(s);
            }
            catch(Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
            if (double.IsInfinity(value) || double.IsNaN(value)) {
                throw new XmlSchemaException(Res.Sch_InvalidValue, s);
            }
            return value;
        }
    }

    internal class Datatype_floatXdr : Datatype_float {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            float value;
            try {
                value = XmlConvert.ToSingle(s);
            }
            catch(Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
            if (float.IsInfinity(value) || float.IsNaN(value)) {
                throw new XmlSchemaException(Res.Sch_InvalidValue, s);
            }
            return value;
        }
    }

    internal class Datatype_QNameXdr : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(XmlQualifiedName);
        static readonly Type listValueType = typeof(XmlQualifiedName[]);

        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.QName;}}

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            if (s == null || s.Length == 0) {
                throw new XmlSchemaException(Res.Sch_EmptyAttributeValue, string.Empty);
            }
            if (nsmgr == null) {
                throw new ArgumentNullException("nsmgr");
            }
            string prefix;
            try {
                return XmlQualifiedName.Parse(s.Trim(), nsmgr, out prefix);
            }
            catch (XmlSchemaException e) {
                throw e;
            }
            catch (Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
        }

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}
    }

    internal class Datatype_ENUMERATION : Datatype_NMTOKEN {
        public override XmlTokenizedType TokenizedType { get { return XmlTokenizedType.ENUMERATION;}}
    }

    internal class Datatype_char : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(char);
        static readonly Type listValueType = typeof(char[]);

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override RestrictionFlags ValidRestrictionFlags { get { return 0; }} //XDR only

        internal override int Compare(object value1, object value2) {
            // this should be culture sensitive - comparing values
            return ((char)value1).CompareTo(value2);
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            try {
                return XmlConvert.ToChar(s);
            }
            catch (XmlSchemaException e) {
                throw e;
            }
            catch (Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            char charValue;
            exception = XmlConvert.TryToChar(s, out charValue);
            if (exception != null) goto Error;

            typedValue = charValue;

            return null;

        Error:
            return exception;
        }
    }

    internal class Datatype_fixed : Datatype_decimal {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            Exception exception;

            try {
                Numeric10FacetsChecker facetsChecker = this.FacetsChecker as Numeric10FacetsChecker;
                decimal value = XmlConvert.ToDecimal(s);
                exception = facetsChecker.CheckTotalAndFractionDigits(value, 14 + 4, 4, true, true);
                if (exception != null) goto Error;

                return value;
            }
            catch (XmlSchemaException e) {
                throw e;
            }
            catch (Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
        Error:
            throw exception;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            decimal decimalValue;
            exception = XmlConvert.TryToDecimal(s, out decimalValue);
            if (exception != null) goto Error;

            Numeric10FacetsChecker facetsChecker = this.FacetsChecker as Numeric10FacetsChecker;
            exception = facetsChecker.CheckTotalAndFractionDigits(decimalValue, 14 + 4, 4, true, true);
            if (exception != null) goto Error;

            typedValue = decimalValue;

            return null;

        Error:
            return exception;
        }
    }

    internal class Datatype_uuid : Datatype_anySimpleType {
        static readonly Type atomicValueType = typeof(Guid);
        static readonly Type listValueType = typeof(Guid[]);

        public override Type ValueType { get { return atomicValueType; }}

        internal override Type ListValueType { get { return listValueType; }}

        internal override RestrictionFlags ValidRestrictionFlags { get { return 0; }}

        internal override int Compare(object value1, object value2) {
            return ((Guid)value1).Equals(value2) ? 0 : -1;
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr) {
            try {
                return XmlConvert.ToGuid(s);
            }
            catch (XmlSchemaException e) {
                throw e;
            }
            catch (Exception e) {
                throw new XmlSchemaException(Res.GetString(Res.Sch_InvalidValue, s), e);
            }
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue) {
            Exception exception;

            typedValue = null;

            Guid guid;
            exception = XmlConvert.TryToGuid(s, out guid);
            if (exception != null) goto Error;

            typedValue = guid;

            return null;

        Error:
            return exception;
        }
    }
}
