// XmlExceptionCode.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:47:23 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Xml {


	/// <summary>
	/// </summary>
	public enum XmlExceptionCode {

		/// <summary>
		/// </summary>
		Success = 0,

		/// <summary>
		/// </summary>
		UnclosedQuote = 1,

		/// <summary>
		/// </summary>
		UnexpectedEOF = 2,

		/// <summary>
		/// </summary>
		BadStartNameChar = 3,

		/// <summary>
		/// </summary>
		BadNameChar = 4,

		/// <summary>
		/// </summary>
		BadComment = 5,

		/// <summary>
		/// </summary>
		BadDecimalEntity = 6,

		/// <summary>
		/// </summary>
		BadHexEntity = 7,

		/// <summary>
		/// </summary>
		NumEntityOverflow = 8,

		/// <summary>
		/// </summary>
		MissingByteOrderMark = 9,

		/// <summary>
		/// </summary>
		UnknownEncoding = 10,

		/// <summary>
		/// </summary>
		InternalError = 11,

		/// <summary>
		/// </summary>
		UnexpectedWS = 12,

		/// <summary>
		/// </summary>
		UnexpectedChar = 13,

		/// <summary>
		/// </summary>
		NoInput = 14,

		/// <summary>
		/// </summary>
		NoHandler = 15,

		/// <summary>
		/// </summary>
		UnexpectedToken = 16,

		/// <summary>
		/// </summary>
		NotImplemented = 17,

		/// <summary>
		/// </summary>
		TagMismatch = 18,

		/// <summary>
		/// </summary>
		UnexpectedTag = 19,

		/// <summary>
		/// </summary>
		BadColon = 20,

		/// <summary>
		/// </summary>
		UnknownNs = 21,

		/// <summary>
		/// </summary>
		ReservedNs = 22,

		/// <summary>
		/// </summary>
		BadAttributeChar = 23,

		/// <summary>
		/// </summary>
		MissingRoot = 24,

		/// <summary>
		/// </summary>
		MultipleRoots = 25,

		/// <summary>
		/// </summary>
		BadElementData = 26,

		/// <summary>
		/// </summary>
		InvalidRootData = 27,

		/// <summary>
		/// </summary>
		XmlDeclNotFirst = 28,

		/// <summary>
		/// </summary>
		InvalidAttributeValue = 29,

		/// <summary>
		/// </summary>
		InvalidXmlDecl = 30,

		/// <summary>
		/// </summary>
		BadXmlDeclCase = 31,

		/// <summary>
		/// </summary>
		InvalidNodeType = 32,

		/// <summary>
		/// </summary>
		InvalidPIName = 33,

		/// <summary>
		/// </summary>
		InvalidXmlSpace = 34,

		/// <summary>
		/// </summary>
		InvalidXmlLang = 35,

		/// <summary>
		/// </summary>
		InvalidVersionNumber = 36,

		/// <summary>
		/// </summary>
		DupAttributeName = 37,

		/// <summary>
		/// </summary>
		BadDTDLocation = 38,

		/// <summary>
		/// </summary>
		UnexpectedElement = 39,

		/// <summary>
		/// </summary>
		TagNotInTheSameEntity = 40,

		/// <summary>
		/// </summary>
		PartialContentNodeTypeNotSupported = 41,

		/// <summary>
		/// </summary>
		InvalidPartialContentData = 42,

		/// <summary>
		/// </summary>
		TwoDTDsProvided = 43,

		/// <summary>
		/// </summary>
		CanNotBindToReservedNamespace = 44,

		/// <summary>
		/// </summary>
		TextDeclPosition = 45,

		/// <summary>
		/// </summary>
		DupElementDecl = 46,

		/// <summary>
		/// </summary>
		DupIDAttribute = 47,

		/// <summary>
		/// </summary>
		DupNotation = 48,

		/// <summary>
		/// </summary>
		DupEntity = 49,

		/// <summary>
		/// </summary>
		DupParEntity = 50,

		/// <summary>
		/// </summary>
		DupID = 51,

		/// <summary>
		/// </summary>
		UndeclaredElement = 52,

		/// <summary>
		/// </summary>
		UndeclaredAttribute = 53,

		/// <summary>
		/// </summary>
		UndeclaredNotation = 54,

		/// <summary>
		/// </summary>
		UndeclaredID = 55,

		/// <summary>
		/// </summary>
		UndeclaredParEntity = 56,

		/// <summary>
		/// </summary>
		UndeclaredEntity = 57,

		/// <summary>
		/// </summary>
		InvalidCondSect = 58,

		/// <summary>
		/// </summary>
		InvalidParEntityRef = 59,

		/// <summary>
		/// </summary>
		InvalidContentModel = 60,

		/// <summary>
		/// </summary>
		InvalidAttType = 61,

		/// <summary>
		/// </summary>
		InvalidTextDecl = 62,

		/// <summary>
		/// </summary>
		InvalidContent = 63,

		/// <summary>
		/// </summary>
		InvalidContentExpecting = 64,

		/// <summary>
		/// </summary>
		ParEntityRefNesting = 65,

		/// <summary>
		/// </summary>
		ReservedNsDecl = 66,

		/// <summary>
		/// </summary>
		RecursiveParEntity = 67,

		/// <summary>
		/// </summary>
		RootMatchDocType = 68,

		/// <summary>
		/// </summary>
		EmptyContent = 69,

		/// <summary>
		/// </summary>
		IncompleteContent = 70,

		/// <summary>
		/// </summary>
		UnparsedEntity = 71,

		/// <summary>
		/// </summary>
		SchemaRootExpected = 72,

		/// <summary>
		/// </summary>
		UnsupportedAttribute = 73,

		/// <summary>
		/// </summary>
		UnsupportedElement = 74,

		/// <summary>
		/// </summary>
		MissAttribute = 75,

		/// <summary>
		/// </summary>
		AnnotationLocation = 76,

		/// <summary>
		/// </summary>
		DataTypeTextOnly = 77,

		/// <summary>
		/// </summary>
		UnknownContent = 78,

		/// <summary>
		/// </summary>
		UnknownModel = 79,

		/// <summary>
		/// </summary>
		UnknownOrder = 80,

		/// <summary>
		/// </summary>
		MixedMany = 81,

		/// <summary>
		/// </summary>
		GroupDisabled = 82,

		/// <summary>
		/// </summary>
		DupConstraint = 83,

		/// <summary>
		/// </summary>
		MissDtvaluesAttribute = 84,

		/// <summary>
		/// </summary>
		MissDtvalue = 85,

		/// <summary>
		/// </summary>
		DupDtType = 86,

		/// <summary>
		/// </summary>
		DupAttribute = 87,

		/// <summary>
		/// </summary>
		UnknownDtType = 88,

		/// <summary>
		/// </summary>
		RequireEnumeration = 89,

		/// <summary>
		/// </summary>
		DefaultIDValue = 90,

		/// <summary>
		/// </summary>
		ElementNotAllowed = 91,

		/// <summary>
		/// </summary>
		ElementMissing = 92,

		/// <summary>
		/// </summary>
		ManyMaxOccurs = 93,

		/// <summary>
		/// </summary>
		UnknownRequired = 94,

		/// <summary>
		/// </summary>
		MaxOccursInvalid = 95,

		/// <summary>
		/// </summary>
		MinOccursInvalid = 96,

		/// <summary>
		/// </summary>
		DtMaxLenghtInvalid = 97,

		/// <summary>
		/// </summary>
		DtMinLenghtInvalid = 98,

		/// <summary>
		/// </summary>
		DupDtMaxLenght = 99,

		/// <summary>
		/// </summary>
		DupDtMinLenght = 100,

		/// <summary>
		/// </summary>
		DtMinMaxLength = 101,

		/// <summary>
		/// </summary>
		InvalidAttributeDefault = 102,

		/// <summary>
		/// </summary>
		DupElement = 103,

		/// <summary>
		/// </summary>
		InvalidValue = 104,

		/// <summary>
		/// </summary>
		RecursiveGenEntity = 105,

		/// <summary>
		/// </summary>
		ExternalEntityInAttValue = 106,

		/// <summary>
		/// </summary>
		UnparsedEntityRef = 107,

		/// <summary>
		/// </summary>
		MissRequiredAttribute = 108,

		/// <summary>
		/// </summary>
		FixedAttributeValue = 109,

		/// <summary>
		/// </summary>
		AttributeValueDataType = 110,

		/// <summary>
		/// </summary>
		IncludeLocation = 111,

		/// <summary>
		/// </summary>
		ImportLocation = 112,

		/// <summary>
		/// </summary>
		NoParticle = 113,

		/// <summary>
		/// </summary>
		InvalidProcessContentValue = 114,

		/// <summary>
		/// </summary>
		InvalidDerivedByValue = 115,

		/// <summary>
		/// </summary>
		InvalidBlockValue = 116,

		/// <summary>
		/// </summary>
		InvalidBlockDefaultValue = 117,

		/// <summary>
		/// </summary>
		InvalidFinalValue = 118,

		/// <summary>
		/// </summary>
		InvalidFinalDefaultValue = 119,

		/// <summary>
		/// </summary>
		DupAttributeValue = 120,

		/// <summary>
		/// </summary>
		InvalidID = 121,

		/// <summary>
		/// </summary>
		DupSimpleType = 122,

		/// <summary>
		/// </summary>
		DupComplexType = 123,

		/// <summary>
		/// </summary>
		InvalidContentValue = 124,

		/// <summary>
		/// </summary>
		DupGroup = 125,

		/// <summary>
		/// </summary>
		DefaultFixedAttributes = 126,

		/// <summary>
		/// </summary>
		DupAttributeGroup = 127,

		/// <summary>
		/// </summary>
		DerivedByNotAllowed = 128,

		/// <summary>
		/// </summary>
		DupXsdElement = 129,

		/// <summary>
		/// </summary>
		ForbiddenAttribute = 130,

		/// <summary>
		/// </summary>
		NoElementContent = 131,

		/// <summary>
		/// </summary>
		ElementRef = 132,

		/// <summary>
		/// </summary>
		TypeMutualExclusive = 133,

		/// <summary>
		/// </summary>
		ElementNameRef = 134,

		/// <summary>
		/// </summary>
		AttributeNameRef = 135,

		/// <summary>
		/// </summary>
		TextNotAllowed = 136,

		/// <summary>
		/// </summary>
		UndeclaredEquivClass = 137,

		/// <summary>
		/// </summary>
		UndeclaredType = 138,

		/// <summary>
		/// </summary>
		AttListPresence = 139,

		/// <summary>
		/// </summary>
		EmptySchemaCollection = 140,

		/// <summary>
		/// </summary>
		NotInSchemaCollection = 141,

		/// <summary>
		/// </summary>
		NotSameNameTable = 142,

		/// <summary>
		/// </summary>
		InvalidCharacter = 143,

		/// <summary>
		/// </summary>
		ValidDateElement = 144,

		/// <summary>
		/// </summary>
		NotationValue = 145,

		/// <summary>
		/// </summary>
		EnumerationValue = 146,

		/// <summary>
		/// </summary>
		EmptyAttributeValue = 147,

		/// <summary>
		/// </summary>
		InvalidName = 148,

		/// <summary>
		/// </summary>
		NoColonEntityName = 149,

		/// <summary>
		/// </summary>
		MultipleAttributeValue = 150,

		/// <summary>
		/// </summary>
		UnexpectedCDataEnd = 151,

		/// <summary>
		/// </summary>
		ResolveURL = 152,

		/// <summary>
		/// </summary>
		NullXmlResolver = 153,

		/// <summary>
		/// </summary>
		ColonInLocalName = 154,

		/// <summary>
		/// </summary>
		XmlLangNmtoken = 155,

		/// <summary>
		/// </summary>
		InvalidLanguageID = 156,

		/// <summary>
		/// </summary>
		XmlSpace = 157,

		/// <summary>
		/// </summary>
		ExpectDtdMarkup = 158,

		/// <summary>
		/// </summary>
		ExpectedExternalOrPublicID = 159,

		/// <summary>
		/// </summary>
		ExpectedExternalIdOrEntityValue = 160,

		/// <summary>
		/// </summary>
		ExpectAttType = 161,

		/// <summary>
		/// </summary>
		ExpectIgnoreOrInclude = 162,

		/// <summary>
		/// </summary>
		ExpectSubOrClose = 163,

		/// <summary>
		/// </summary>
		ExpectExternalOrClose = 164,

		/// <summary>
		/// </summary>
		ExpectOp = 165,

		/// <summary>
		/// </summary>
		ExpectPcData = 166,

		/// <summary>
		/// </summary>
		InvalidXsdAttributeValue = 167,

		/// <summary>
		/// </summary>
		ElementValueDataType = 168,

		/// <summary>
		/// </summary>
		AttributeRef = 169,

		/// <summary>
		/// </summary>
		AttributeComplexType = 170,

		/// <summary>
		/// </summary>
		NonDeterministic = 171,

		/// <summary>
		/// </summary>
		StandAlone = 172,

		/// <summary>
		/// </summary>
		InvalidSchema = 173,

		/// <summary>
		/// </summary>
		SimpleFromComplex = 174,

		/// <summary>
		/// </summary>
		ListFromList = 175,

		/// <summary>
		/// </summary>
		DerivedByBaseName = 176,

		/// <summary>
		/// </summary>
		InvalidXmlDocument = 177,

		/// <summary>
		/// </summary>
		ParticlesNotAllowed = 178,

		/// <summary>
		/// </summary>
		FacetsNotAllowed = 179,

		/// <summary>
		/// </summary>
		AttributesNotAllowed = 180,

		/// <summary>
		/// </summary>
		ComplexSimpleRestriction = 181,

		/// <summary>
		/// </summary>
		ComplexSimpleContent = 182,

		/// <summary>
		/// </summary>
		XmlNsAttribute = 183,

		/// <summary>
		/// </summary>
		GlobalAttributeFixedDefault = 184,

		/// <summary>
		/// </summary>
		XsiTargetNamespace = 185,

		/// <summary>
		/// </summary>
		BlockEquivClass = 186,

		/// <summary>
		/// </summary>
		ElementDefault = 187,

		/// <summary>
		/// </summary>
		UndeclaredModelGroup = 188,

		/// <summary>
		/// </summary>
		UndeclaredAttributeGroup = 189,

		/// <summary>
		/// </summary>
		FragmentID = 190,

		/// <summary>
		/// </summary>
		AllElement = 191,

		/// <summary>
		/// </summary>
		MaxOccursInAll = 192,

		/// <summary>
		/// </summary>
		MisMatchTargetNamespace = 193,

		/// <summary>
		/// </summary>
		AttributeFollowCompositor = 194,

		/// <summary>
		/// </summary>
		XsiTypeNotFound = 195,

		/// <summary>
		/// </summary>
		ListFromNonatomic = 196,

		/// <summary>
		/// </summary>
		DupLengthFacet = 197,

		/// <summary>
		/// </summary>
		DupMinLengthFacet = 198,

		/// <summary>
		/// </summary>
		DupMaxLengthFacet = 199,

		/// <summary>
		/// </summary>
		DupPatternFacet = 200,

		/// <summary>
		/// </summary>
		DupWhiteSpaceFacet = 201,

		/// <summary>
		/// </summary>
		DupMaxInclusiveFacet = 202,

		/// <summary>
		/// </summary>
		DupMaxExclusiveFacet = 203,

		/// <summary>
		/// </summary>
		DupMinInclusiveFacet = 204,

		/// <summary>
		/// </summary>
		DupMinExclusiveFacet = 205,

		/// <summary>
		/// </summary>
		DupPrecisionFacet = 206,

		/// <summary>
		/// </summary>
		DupScaleFacet = 207,

		/// <summary>
		/// </summary>
		DupEncodingFacet = 208,

		/// <summary>
		/// </summary>
		DupDurationFacet = 209,

		/// <summary>
		/// </summary>
		DupPeriodFacet = 210,

		/// <summary>
		/// </summary>
		LengthFacetProhibited = 211,

		/// <summary>
		/// </summary>
		MinLengthFacetProhibited = 212,

		/// <summary>
		/// </summary>
		MaxLengthFacetProhibited = 213,

		/// <summary>
		/// </summary>
		PatternFacetProhibited = 214,

		/// <summary>
		/// </summary>
		EnumerationFacetProhibited = 215,

		/// <summary>
		/// </summary>
		WhiteSpaceFacetProhibited = 216,

		/// <summary>
		/// </summary>
		MaxInclusiveFacetProhibited = 217,

		/// <summary>
		/// </summary>
		MaxExclusiveFacetProhibited = 218,

		/// <summary>
		/// </summary>
		MinInclusiveFacetProhibited = 219,

		/// <summary>
		/// </summary>
		MinExclusiveFacetProhibited = 220,

		/// <summary>
		/// </summary>
		PrecisionFacetProhibited = 221,

		/// <summary>
		/// </summary>
		ScaleFacetProhibited = 222,

		/// <summary>
		/// </summary>
		EncodingFacetProhibited = 223,

		/// <summary>
		/// </summary>
		DurationFacetProhibited = 224,

		/// <summary>
		/// </summary>
		PeriodFacetProhibited = 225,

		/// <summary>
		/// </summary>
		NegativeLength = 226,

		/// <summary>
		/// </summary>
		NegativeMinLength = 227,

		/// <summary>
		/// </summary>
		NegativeMaxLength = 228,

		/// <summary>
		/// </summary>
		FacetOnlyForAtomic = 229,

		/// <summary>
		/// </summary>
		InvalidEncoding = 230,

		/// <summary>
		/// </summary>
		InvalidWhiteSpace = 231,

		/// <summary>
		/// </summary>
		UnknownFacet = 232,

		/// <summary>
		/// </summary>
		LengthAndMinMax = 233,

		/// <summary>
		/// </summary>
		MinLengthGtMaxLength = 234,

		/// <summary>
		/// </summary>
		ScaleGtPrecision = 235,

		/// <summary>
		/// </summary>
		LengthConstraintFailed = 236,

		/// <summary>
		/// </summary>
		MinLengthConstraintFailed = 237,

		/// <summary>
		/// </summary>
		MaxLengthConstraintFailed = 238,

		/// <summary>
		/// </summary>
		PatternConstraintFailed = 239,

		/// <summary>
		/// </summary>
		EnumerationConstraintFailed = 240,

		/// <summary>
		/// </summary>
		MaxInclusiveConstraintFailed = 241,

		/// <summary>
		/// </summary>
		MaxExclusiveConstraintFailed = 242,

		/// <summary>
		/// </summary>
		MinInclusiveConstraintFailed = 243,

		/// <summary>
		/// </summary>
		MinExclusiveConstraintFailed = 244,

		/// <summary>
		/// </summary>
		PrecisionConstraintFailed = 245,

		/// <summary>
		/// </summary>
		ScaleConstraintFailed = 246,

		/// <summary>
		/// </summary>
		EncodingConstraintFailed = 247,

		/// <summary>
		/// </summary>
		DurationConstraintFailed = 248,

		/// <summary>
		/// </summary>
		PeriodConstraintFailed = 249,

		/// <summary>
		/// </summary>
		UnionFailed = 250,

		/// <summary>
		/// </summary>
		EncodingRequired = 251,

		/// <summary>
		/// </summary>
		MoreFacetsRequired = 252,

		/// <summary>
		/// </summary>
		DupNotationAttribute = 253,

		/// <summary>
		/// </summary>
		NotationAttributeOnEmptyElement = 254,

		/// <summary>
		/// </summary>
		Last = 255,
	} // XmlExceptionCode

} // System.Xml
