using System.Globalization;

namespace System.Runtime.Serialization
{
	static partial class SR
	{
		internal static string GetString(string name, params object[] args)
		{
			return GetString (CultureInfo.InvariantCulture, name, args);
		}

		internal static string GetString(CultureInfo culture, string name, params object[] args)
		{
			return string.Format (culture, name, args);
		}

		internal static string GetString(string name)
		{
			return name;
		}

		internal static string GetString(CultureInfo culture, string name)
		{
			return name;
		}

#region MissingInStrings.txt

//
// This was retrieved as follows:
//
// 1. mcs ReferenceSources/SR.cs -t:library -out:existing.dll
// 2. mcs {https://raw.githubusercontent.com/mono/mono/wip-serialization-halfway/mcs/class/System.Runtime.Serialization/ReferenceSource/SR.cs} -t:library -out:full.dll
// 3. csharp -e "System.IO.File.WriteAllLines ("existing.txt", System.Reflection.Assembly.ReflectionOnlyLoadFrom ("existing.dll").GetTypes ().SelectMany (t => t.GetFields ()).Select (f => f.Name).ToArray ())"
// 4. csharp -e "System.IO.File.WriteAllLines ("full.txt", System.Reflection.Assembly.ReflectionOnlyLoadFrom ("full.dll").GetTypes ().SelectMany (t => t.GetFields ()).Select (f => f.Name).ToArray ())"
// 5. csharp
//	var existing = System.IO.File.ReadAllLines ("existing.txt");
//	var full = System.IO.File.ReadAllLines ("full.txt");
//  var missing = full.Where (f => !existing.Contains (f));
//  System.IO.File.WriteAllLines ("missing.cs", missing.Select (m => "public const string " + m + " = @\"" + m + "\";").ToArray ())
// 6. copy missing.cs contents here.
//

public const string AbstractElementNotSupported = @"Abstract element '{0}' is not supported.";
public const string AbstractTypeNotSupported = @"Abstract type is not supported";
public const string AmbiguousReferencedCollectionTypes1 = @"Ambiguous collection types were referenced: {0}";
public const string AmbiguousReferencedCollectionTypes3 = @"In '{0}' element in '{1}' namespace, ambiguous collection types were referenced: {2}";
public const string AmbiguousReferencedTypes1 = @"Ambiguous types were referenced: {0}";
public const string AmbiguousReferencedTypes3 = @"In '{0}' element in '{1}' namespace, ambiguous types were referenced: {2}";
public const string AnnotationAttributeNotFound = @"Annotation attribute was not found: default value annotation is '{0}', type is '{1}' in '{2}' namespace, emit default value is {3}.";
public const string AnonymousTypeNotSupported = @"Anonymous type is not supported. Type is '{0}' in '{1}' namespace.";
public const string AnyAttributeNotSupported = @"XML Schema 'any' attribute is not supported";
public const string ArrayItemFormMustBe = @"For array item, element 'form' must be {0}.";
public const string ArraySizeAttributeIncorrect = @"Array size attribute is incorrect; must be between {0} and {1}.";

public const string ArrayTypeCannotBeImported = @"Array type cannot be imported for '{0}' in '{1}' namespace: {2}.";
public const string AssemblyNotFound = @"Assembly '{0}' was not found.";
public const string AttributeNotFound = @"Attribute was not found for CLR type '{1}' in namespace '{0}'. XML reader node is on {2}, '{4}' node in '{3}' namespace.";
public const string BaseTypeNotISerializable = @"Base type '{0}' in '{1}' namespace is not ISerializable.";
public const string CannotComputeUniqueName = @"Cannot compute unique name for '{0}'.";
public const string CannotDeriveFromSealedReferenceType = @"Cannod drive from sealed reference type '{2}', for '{0}' element in '{1}' namespace.";
public const string CannotDeserializeForwardedType = @"Cannot deserialize forwarded type '{0}'.";
public const string CannotExportNullAssembly = @"Cannot export null assembly.";
public const string CannotExportNullKnownType = @"Cannot export null known type.";
public const string CannotExportNullType = @"Cannot export null type.";
public const string CannotHaveDuplicateAttributeNames = @"Cannot have duplicate attribute names '{0}'.";
public const string CannotHaveDuplicateElementNames = @"Cannot have duplicate element names '{0}'.";
public const string CannotImportInvalidSchemas = @"Cannot import invalid schemas.";
public const string CannotImportNullDataContractName = @"Cannot import data contract with null name.";
public const string CannotImportNullSchema = @"Cannot import from schema list that contains null.";
public const string CannotSetMembersForReferencedType = @"Cannot set members for already referenced type. Base type is '{0}'.";
public const string CannotSetNamespaceForReferencedType = @"Cannot set namespace for already referenced type. Base type is '{0}'.";
public const string CannotUseGenericTypeAsBase = @"For '{0}' in '{1}' namespace, generic type cannot be referenced as the base type.";
public const string ChangingFullTypeNameNotSupported = @"Changing full type name is not supported. Serialization type name: '{0}', data contract type name: '{1}'.";
public const string CircularTypeReference = @"Circular type reference was found for '{0}' in '{1}' namespace.";
public const string ClassDataContractReturnedForGetOnlyCollection = @"For '{0}' type, class data contract was returned for get-only collection.";
public const string CLRNamespaceMappedMultipleTimes = @"CLR namespace is mapped multiple times. Current data contract namespace is '{0}', found '{1}' for CLR namespace '{2}'.";
public const string ClrTypeNotFound = @"CLR type '{1}' in assembly '{0}' is not found.";
public const string CollectionAssignedToIncompatibleInterface = @"Collection of type '{0}' is assigned to an incompatible interface '{1}'";
public const string ComplexTypeRestrictionNotSupported = @"XML schema complexType restriction is not supported.";
public const string ConfigDataContractSerializerSectionLoadError = @"Failed to load configuration section for dataContractSerializer.";
public const string ConfigIndexOutOfRange = @"For type '{0}', configuration index is out of range.";
public const string ConfigMustOnlyAddParamsWithType = @"Configuration parameter element must only add params with type."; // huh? the code doesn't make a lot of sense to me...
public const string ConfigMustOnlySetTypeOrIndex = @"Configuration parameter element can set only one of either type or index.";
public const string ConfigMustSetTypeOrIndex = @"Configuration parameter element must set either type or index.";
public const string CouldNotReadSerializationSchema = @"Could not read serialization schema for '{0}' namespace.";
public const string DefaultOnElementNotSupported = @"On element '{0}', default value is not supported.";
public const string DerivedTypeNotISerializable = @"On type '{0}' in '{1}' namespace, derived type is not ISerializable.";
public const string DupContractInDataContractSet = @"Duplicate contract in data contract set was found, for '{0}' in '{1}' namespace.";
public const string DuplicateExtensionDataSetMethod = @"Duplicate extension data set method was found, for method '{0}', existing method is '{1}', on data contract type '{2}'.";
public const string DupTypeContractInDataContractSet = @"Duplicate type contract in data contract set. Type name '{0}', for data contract '{1}' in '{2}' namespace.";
public const string ElementMaxOccursMustBe = @"On element '{0}', schema element maxOccurs must be 1.";
public const string ElementMinOccursMustBe = @"On element '{0}', schema element minOccurs must be less or equal to 1.";
public const string ElementRefOnLocalElementNotSupported = @"For local element, ref is not supported. The referenced name is '{0}' in '{1}' namespace.";
public const string EnumEnumerationFacetsMustHaveValue = @"Schema enumeration facet must have values.";
public const string EnumListInAnonymousTypeNotSupported = @"Enum list in anonymous type is not supported.";
public const string EnumListMustContainAnonymousType = @"Enum list must contain an anonymous type.";
public const string EnumOnlyEnumerationFacetsSupported = @"For schema facets, only enumeration is supported.";
public const string EnumRestrictionInvalid = @"For simpleType restriction, only enum is supported and this type could not be convert to enum.";
public const string EnumTypeCannotBeImported = @"For '{0}' in '{1}' namespace, enum type cannot be imported: {2}";
public const string EnumTypeNotSupportedByDataContractJsonSerializer = @"Enum type is not supported by DataContractJsonSerializer. The underlying type is '{0}'.";
public const string EnumUnionInAnonymousTypeNotSupported = @"Enum union in anonymous type is not supported.";
public const string ExtensionDataSetMustReturnVoid = @"For type '{0}' method '{1}', extension data set method must return void.";
public const string ExtensionDataSetParameterInvalid = @"For type '{0}' method '{1}', extension data set method has invalid type of parameter '{2}'.";
public const string FactoryObjectContainsSelfReference = @"Factory object contains a reference to self. Old object is '{0}', new object is '{1}'.";
public const string FactoryTypeNotISerializable = @"For data contract '{1}', factory type '{0}' is not ISerializable.";
public const string FixedOnElementNotSupported = @"On schema element '{0}', fixed value is not supported.";
public const string FlushBufferAlreadyInUse = @"Flush buffer is already in use.";
public const string FormMustBeQualified = @"On schema element '{0}', form must be qualified.";
public const string GenericAnnotationAttributeNotFound = @"On type '{0}' Generic annotation attribute '{1}' was not found.";
public const string GenericAnnotationForNestedLevelMustBeIncreasing = @"On type '{2}', generic annotation for nested level must be increasing. Argument element is '{0}' in '{1}' namespace.";
public const string GenericAnnotationHasInvalidAttributeValue = @"On type '{2}', generic annotation has invalid attribute value '{3}'. Argument element is '{0}' in '{1}' namespace. Nested level attribute attribute name is '{4}'. Type is '{5}'."; // dunno if this makes sense...
public const string GenericAnnotationHasInvalidElement = @"On type '{2}', generic annotation has invalid element. Argument element is '{0}' in '{1}' namespace.";
public const string GenericTypeNameMismatch = @"Generic type name mismatch. Expected '{0}' in '{1}' namespace, got '{2}' in '{3}' namespace instead.";
public const string GenericTypeNotExportable = @"Generic type '{0}' is not exportable.";
public const string GetOnlyCollectionMustHaveAddMethod = @"On type '{0}', get-only collection must have an Add method.";
public const string GetRealObjectReturnedNull = @"On the surrogate data contract for '{0}', GetRealObject method returned null.";
public const string InvalidAnnotationExpectingText = @"For annotation element '{0}' in namespace '{1}', expected text but got element '{2}' in '{3}' namespace.";
public const string InvalidAssemblyFormat = @"'{0}': invalid assembly format.";
public const string InvalidCharacterEncountered = @"Encountered an invalid character '{0}'.";
public const string InvalidClassDerivation = @"Invalid class derivation from '{0}' in '{1}' namespace.";
public const string InvalidClrNameGeneratedForISerializable = @"Invalid CLR name '{2}' is generated for ISerializable type '{0}' in '{1}' namespace.";
public const string InvalidClrNamespaceGeneratedForISerializable = @"Invalid CLR namespace '{3}' is generated for ISerializable type '{0}' in '{1}' namespace. Data contract namespace from the URI would be generated as '{2}'.";
public const string InvalidDataNode = @"Invalid data node for '{0}' type.";
public const string InvalidEmitDefaultAnnotation = @"Invalid EmilDefault annotation for '{0}' in type '{1}' in '{2}' namespace.";
public const string InvalidEnumBaseType = @"Invalid enum base type is specified for type '{0}' in '{1}' namespace, element name is '{2}' in '{3}' namespace.";
public const string InvalidISerializableDerivation = @"Invalid ISerializable derivation from '{0}' in '{1}' namespace.";
public const string InvalidKeyValueType = @"'{0}' is an invalid key value type.";
public const string InvalidKeyValueTypeNamespace = @"'{0}' in '{1}' namespace is an invalid key value type.";
public const string InvalidReturnSchemaOnGetSchemaMethod = @"On type '{0}', the return value from GetSchema method was invalid.";
public const string InvalidStateInExtensionDataReader = @"Invalid state in extension data reader.";
public const string InvalidXmlDeserializingExtensionData = @"Invalid XML while deserializing extension data.";
public const string IsAnyNotSupportedByNetDataContractSerializer = @"For type '{0}', IsAny is not supported by NetDataContractSerializer.";
public const string IsDictionaryFormattedIncorrectly = @"IsDictionary formatted value '{0}' is incorrect: {1}";
public const string ISerializableAssemblyNameSetToZero = @"ISerializable AssemblyName is set to ""0"" for type '{0}'.";
public const string ISerializableCannotHaveDataContract = @"ISerializable type '{0}' cannot have DataContract.";
public const string ISerializableContainsMoreThanOneItems = @"ISerializable cannot contain more than one item.";
public const string ISerializableDerivedContainsOneOrMoreItems = @"Type derived from ISerializable cannot contain more than one item.";
public const string ISerializableDoesNotContainAny = @"ISerializable does not contain any element.";
public const string ISerializableMustRefFactoryTypeAttribute = @"ISerializable must have ref attribute that points to its factory type.";
public const string ISerializableTypeCannotBeImported = @"ISerializable type '{0}' in '{1}' namespace cannot be imported: {2}";
public const string ISerializableWildcardMaxOccursMustBe = @"ISerializable wildcard maxOccurs must be '{0}'.";
public const string ISerializableWildcardMinOccursMustBe = @"ISerializable wildcard maxOccurs must be '{0}'.";
public const string ISerializableWildcardNamespaceInvalid = @"ISerializable wildcard namespace is invalid: '{0}'.";
public const string ISerializableWildcardProcessContentsInvalid = @"ISerializable wildcard processContents is invalid: '{0}'.";
public const string IsReferenceGetOnlyCollectionsNotSupported = @"On type '{1}', attribute '{0}' points to get-only collection, which is not supported.";
public const string IsValueTypeFormattedIncorrectly = @"IsValueType is formatted incorrectly as '{0}': {1}";
public const string JsonAttributeAlreadyWritten = @"JSON attribute '{0}' is already written.";
public const string JsonAttributeMustHaveElement = @"JSON attribute must have an owner element.";
public const string JsonCannotWriteStandaloneTextAfterQuotedText = @"JSON writer cannot write standalone text after quoted text.";
public const string JsonCannotWriteTextAfterNonTextAttribute = @"JSON writer cannot write text after non-text attribute. Data type is '{0}'.";
public const string JsonDateTimeOutOfRange = @"JSON DateTime is out of range.";
public const string JsonDuplicateMemberInInput = @"Duplicate member '{0}' is found in JSON input.";
public const string JsonDuplicateMemberNames = @"Duplicate member, including '{1}', is found in JSON input, in type '{0}'.";
public const string JsonEncodingNotSupported = @"JSON Encoding is not supported.";
public const string JsonEncounteredUnexpectedCharacter = @"Encountered an unexpected character '{0}' in JSON.";
public const string JsonEndElementNoOpenNodes = @"Encountered an end element while there was no open element in JSON writer.";
public const string JsonExpectedEncoding = @"Expected encoding '{0}', got '{1}' instead.";
public const string JsonInvalidBytes = @"Invalid bytes in JSON.";
public const string JsonInvalidDataTypeSpecifiedForServerType = @"The specified data type is invalid for server type. Type: '{0}', specified data type: '{1}', server type: '{2}', object '{3}'."; // I wonder if this makes sense...
public const string JsonInvalidDateTimeString = @"Invalid JSON dateTime string is specified: original value '{0}', start guide writer: {1}, end guard writer: {2}.";
public const string JsonInvalidFFFE = @"FFFE in JSON is invalid.";
public const string JsonInvalidItemNameForArrayElement = @"Invalid JSON item name '{0}' for array element (item element is '{1}' in JSON).";
public const string JsonInvalidLocalNameEmpty = @"Empty string is invalid as a local name.";
public const string JsonInvalidMethodBetweenStartEndAttribute = @"Invalid method call state between start and end attribute.";
public const string JsonInvalidRootElementName = @"Invalid root element name '{0}' (root element is '{1}' in JSON).";
public const string JsonInvalidStartElementCall = @"Invalid call to JSON WriteStartElement method.";
public const string JsonInvalidWriteState = @"Invalid write state {1} for '{0}' method.";
public const string JsonMethodNotSupported = @"Method {0} is not supported in JSON.";
public const string JsonMultipleRootElementsNotAllowedOnWriter = @"Multiple root element is not allowed on JSON writer.";
public const string JsonMustSpecifyDataType = @"On JSON writer data type '{0}' must be specified. Object string is '{1}', server type string is '{2}'.";
public const string JsonMustUseWriteStringForWritingAttributeValues = @"On JSON writer WriteString must be used for writing attribute values.";
public const string JsonNamespaceMustBeEmpty = @"JSON namespace is specified as '{0}' but it must be empty.";
public const string JsonNestedArraysNotSupported = @"Nested array is not supported in JSON: '{0}'";
public const string JsonNodeTypeArrayOrObjectNotSpecified = @"Either Object or Array of JSON node type must be specified.";
public const string JsonNoMatchingStartAttribute = @"WriteEndAttribute was called while there is no open attribute.";
public const string JsonOffsetExceedsBufferSize = @"On JSON writer, offset exceeded buffer size {0}.";
public const string JsonOneRequiredMemberNotFound = @"Required member {1} in type '{0}' is not found.";
public const string JsonOnlyWhitespace = @"Only whitespace characters are allowed for {1} method. The specified value is '{0}'";
public const string JsonOpenAttributeMustBeClosedFirst = @"JSON attribute must be closed first before calling {0} method.";
public const string JsonPrefixMustBeNullOrEmpty = @"JSON prefix must be null or empty. '{0}' is specified instead.";
public const string JsonRequiredMembersNotFound = @"Required members {0} in type '{1}' are not found.";
public const string JsonServerTypeSpecifiedForInvalidDataType = @"Server type is specified for invalid data type in JSON. Server type: '{0}', type: '{1}', dataType: '{2}', object: '{3}'.";
public const string JsonSizeExceedsRemainingBufferSpace = @"JSON size exceeded remaining buffer space, by {0} byte(s).";
public const string JsonTypeNotSupportedByDataContractJsonSerializer = @"Type '{0}' is not suppotred by DataContractJsonSerializer.";
public const string JsonUnexpectedAttributeLocalName = @"Unexpected attribute local name '{0}'.";
public const string JsonUnexpectedAttributeValue = @"Unexpected attribute value '{0}'.";
public const string JsonUnexpectedEndOfFile = @"Unexpected end of file in JSON.";
public const string JsonUnsupportedForIsReference = @"Unsupported value for IsReference for type '{0}', IsReference value is {1}.";
public const string JsonWriteArrayNotSupported = @"JSON WriteArray is not supported.";
public const string JsonWriterClosed = @"JSON writer is already closed.";
public const string JsonXmlInvalidDeclaration = @"Attempt to write invalid XML declration.";
public const string JsonXmlProcessingInstructionNotSupported = @"processing instruction is not supported in JSON writer.";
public const string KeyTypeCannotBeParsedInSimpleDictionary = @"Key type '{1}' for collection type '{0}' cannot be parsed in simple dictionary.";
public const string KnownTypeConfigGenericParamMismatch = @"Generic parameter count do not match between known type and configuration. Type is '{0}', known type has {1} parameters, configuration has {2} parameters.";
public const string KnownTypeConfigIndexOutOfBounds = @"For known type configuration, index is out of bound. Root type: '{0}' has {1} type arguments, and index was {2}.";
public const string KnownTypeConfigIndexOutOfBoundsZero = @"For known type configuration, index is out of bound. Root type: '{0}' has {1} type arguments, and index was {2}.";
public const string KnownTypeConfigObject = @"Known type configuration specifies System.Object.";
public const string MaxMimePartsExceeded = @"MIME parts number exceeded the maximum settings. Must be less than {0}. Specified as '{1}'.";
public const string MimeContentTypeHeaderInvalid = @"MIME content type header is invalid.";
public const string MimeHeaderInvalidCharacter = @"MIME header has an invalid character ('{0}', {1} in hexadecimal value).";
public const string MimeMessageGetContentStreamCalledAlready = @"On MimeMessage, GetContentStream method is already called.";
public const string MimeReaderHeaderAlreadyExists = @"MIME header '{0}' already exists.";
public const string MimeReaderMalformedHeader = @"Malformed MIME header.";
public const string MimeReaderResetCalledBeforeEOF = @"On MimeReader, Reset method is called before EOF.";
public const string MimeReaderTruncated = @"MIME parts are truncated.";
public const string MimeVersionHeaderInvalid = @"MIME version header is invalid.";
public const string MimeWriterInvalidStateForClose = @"MIME writer is at invalid state for closing.";
public const string MimeWriterInvalidStateForContent = @"MIME writer is at invalid state for content.";
public const string MimeWriterInvalidStateForHeader = @"MIME writer is at invalid state for header.";
public const string MimeWriterInvalidStateForStartPart = @"MIME writer is at invalid state for starting a part.";
public const string MimeWriterInvalidStateForStartPreface = @"MIME writer is at invalid state for starting preface.";
public const string MissingSchemaType = @"Schema type '{0}' is missing and required for '{1}' type.";
public const string MixedContentNotSupported = @"Mixed content is not supported.";
public const string MtomBoundaryInvalid = @"MIME boundary is invalid: '{0}'.";
public const string MtomBufferQuotaExceeded = @"MTOM buffer quota exceeded. The maximum size is {0}.";
public const string MtomContentTransferEncodingNotPresent = @"MTOM content transfer encoding is not present. ContentTransferEncoding header is '{0}'.";
public const string MtomContentTransferEncodingNotSupported = @"MTOM content transfer encoding value is not supported. Raw value is '{0}', '{1}' in 7bit encoding, '{2}' in 8bit encoding, and '{3}' in binary.";
public const string MtomContentTypeInvalid = @"MTOM content type is invalid.";
public const string MtomDataMustNotContainXopInclude = @"MTOM data must not contain xop:Include element. '{0}' element in '{1}' namespace.";
public const string MtomExceededMaxSizeInBytes = @"MTOM exceeded max size in bytes. The maximum size is {0}.";
public const string MtomInvalidCIDUri = @"Invalid MTOM CID URI: '{0}'.";
public const string MtomInvalidEmptyURI = @"empty URI is invalid for MTOM MIME part.";
public const string MtomInvalidStartUri = @"Invalid MTOM start URI: '{0}'.";
public const string MtomInvalidTransferEncodingForMimePart = @"Invalid transfer encoding for MIME part: '{0}', in binary: '{1}'.";
public const string MtomMessageContentTypeNotFound = @"MTOM message content type was not found.";
public const string MtomMessageInvalidContent = @"MTOM message content is invalid.";
public const string MtomMessageInvalidContentInMimePart = @"MTOM message content in MIME part is invalid.";
public const string MtomMessageInvalidMimeVersion = @"MTOM message has invalid MIME version. Expected '{1}', got '{0}' instead.";
public const string MtomMessageNotApplicationXopXml = @"MTOM msssage type is not '{0}'.";
public const string MtomMessageNotMultipart = @"MTOM message is not multipart: media type should be '{0}', media subtype should be '{1}'.";
public const string MtomMessageRequiredParamNotSpecified = @"Required MTOM parameter '{0}' is not specified.";
public const string MtomMimePartReferencedMoreThanOnce = @"Specified MIME part '{0}' is referenced more than once.";
public const string MtomPartNotFound = @"MTOM part with URI '{0}' is not found.";
public const string MtomRootContentTypeNotFound = @"MTOM root content type is not found.";
public const string MtomRootNotApplicationXopXml = @"MTOM root should have media type '{0}' and subtype '{1}'.";
public const string MtomRootPartNotFound = @"MTOM root part is not found.";
public const string MtomRootRequiredParamNotSpecified = @"Required MTOM root parameter '{0}' is not specified.";
public const string MtomRootUnexpectedCharset = @"Unexpected charset on MTOM root. Expected '{1}', got '{0}' instead.";
public const string MtomRootUnexpectedType = @"Unexpected type on MTOM root. Expected '{1}', got '{0}' instead.";
public const string MtomXopIncludeHrefNotSpecified = @"xop Include element did not specify '{0}' attribute.";
public const string MtomXopIncludeInvalidXopAttributes = @"xop Include element has invalid attribute: '{0}' in '{1}' namespace.";
public const string MtomXopIncludeInvalidXopElement = @"xop Include element has invalid element: '{0}' in '{1}' namespace.";
public const string MustContainOnlyLocalElements = @"Only local elements can be imported.";
public const string NoAsyncWritePending = @"No async write operation is pending.";
public const string NonOptionalFieldMemberOnIsReferenceSerializableType = @"For type '{0}', non-optional field member '{1}' is on the Serializable type that has IsReference as {2}.";
public const string OnlyDataContractTypesCanHaveExtensionData = @"On '{0}' type, only DataContract types can have extension data.";
public const string PartialTrustISerializableNoPublicConstructor = @"Partial trust access required for the constructor on the ISerializable type '{0}'";
public const string QueryGeneratorPathToMemberNotFound = @"The path to member was not found for XPath query generator.";
public const string ReadNotSupportedOnStream = @"Read operation is not supported on the Stream.";
public const string ReadOnlyClassDeserialization = @"Error on deserializing read-only members in the class: {0}";
public const string ReadOnlyCollectionDeserialization = @"Error on deserializing read-only collection: {0}";
public const string RecursiveCollectionType = @"Type '{0}' involves recursive collection.";
public const string RedefineNotSupported = @"XML Schema 'redefine' is not supported.";
public const string ReferencedBaseTypeDoesNotExist = @"Referenced base type does not exist. Data contract name: '{0}' in '{1}' namespace, expected type: '{2}' in '{3}' namespace. Collection can be '{4}' or '{5}'."; // is it the expected message? I'm quite unsure.
public const string ReferencedCollectionTypesCannotContainNull = @"Referenced collection types cannot contain null.";
public const string ReferencedTypeDoesNotMatch = @"Referenced type '{0}' does not match the expected type '{1}' in '{2}' namespace.";
public const string ReferencedTypeMatchingMessage = @"Reference type matches.";
public const string ReferencedTypeNotMatchingMessage = @"Reference type does not match.";
public const string ReferencedTypesCannotContainNull = @"Referenced types cannot contain null.";
public const string RequiresClassDataContractToSetIsISerializable = @"To set IsISerializable, class data cotnract is required.";
public const string RootParticleMustBeSequence = @"Root particle must be sequence to be imported.";
public const string RootSequenceMaxOccursMustBe = @"On root sequence, maxOccurs must be 1.";
public const string RootSequenceMustBeRequired = @"Root sequence must have an item and minOccurs must be 1.";
public const string SeekNotSupportedOnStream = @"Seek operation is not supported on this Stream.";
public const string SerializationInfo_ConstructorNotFound = @"Constructor that takes SerializationInfo and StreamingContext is not found for '{0}'.";
public const string SimpleContentNotSupported = @"Simple content is not supported.";
public const string SimpleTypeRestrictionDoesNotSpecifyBase = @"This simpleType restriction does not specify the base type.";
public const string SimpleTypeUnionNotSupported = @"simpleType union is not supported.";
public const string SpecifiedTypeNotFoundInSchema = @"Specified type '{0}' in '{1}' namespace is not found in the schemas.";
public const string SubstitutionGroupOnElementNotSupported = @"substitutionGroups on elements are not supported.";
public const string SurrogatesWithGetOnlyCollectionsNotSupported = @"Surrogates with get-only collections are not supported. Type '{1}' contains '{2}' which is of '{0}' type.";
public const string SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser = @"Surrogates with get-only collections are not supported. Found on type '{0}'.";
public const string TopLevelElementRepresentsDifferentType = @"Top-level element represents a different type. Expected '{0}' type in '{1}' namespace.";
public const string TraceCodeElementIgnored = @"Element ignored";
public const string TraceCodeFactoryTypeNotFound = @"Factory type not found";
public const string TraceCodeObjectWithLargeDepth = @"Object with large depth";
public const string TraceCodeReadObjectBegin = @"ReadObject begins";
public const string TraceCodeReadObjectEnd = @"ReadObject ends";
public const string TraceCodeWriteObjectBegin = @"WriteObject begins";
public const string TraceCodeWriteObjectContentBegin = @"WriteObjectContent begins";
public const string TraceCodeWriteObjectContentEnd = @"WriteObjectContent ends";
public const string TraceCodeWriteObjectEnd = @"WriteObject ends";
public const string TraceCodeXsdExportAnnotationFailed = @"XSD export annotation failed";
public const string TraceCodeXsdExportBegin = @"XSD export begins";
public const string TraceCodeXsdExportDupItems = @"XSD export duplicate items";
public const string TraceCodeXsdExportEnd = @"XSD export ends";
public const string TraceCodeXsdExportError = @"XSD export error";
public const string TraceCodeXsdImportAnnotationFailed = @"XSD import annotation failed";
public const string TraceCodeXsdImportBegin = @"XSD import begins";
public const string TraceCodeXsdImportEnd = @"XSD import ends";
public const string TraceCodeXsdImportError = @"XSD import error";
public const string TypeCannotBeForwardedFrom = @"Type '{0}' in assembly '{1}' cannot be forwarded from assembly '{2}'.";
public const string TypeCannotBeImported = @"Type '{0}' in '{1}' namespace cannot be imported: {2}";
public const string TypeCannotBeImportedHowToFix = @"Type cannot be imported: {0}"; // I cannot see where HowToFix is given from...
public const string TypeHasNotBeenImported = @"Type '{0}' in '{1}' namespace has not been imported.";
public const string TypeMustBeIXmlSerializable = @"Type '{0}' must be IXmlSerializable. Contract type: '{1}', contract name: '{2}' in '{3}' namespace.";
public const string TypeShouldNotContainAttributes = @"Type should not contain attributes. Serialization namespace: '{0}'.";
public const string UnknownXmlType = @"Unknown XML type: '{0}'.";
public const string WriteBufferOverflow = @"Write buffer overflow.";
public const string WriteNotSupportedOnStream = @"Write operation is not supported on this '{0}' Stream.";
public const string XmlCanonicalizationNotStarted = @"XML canonicalization was not started.";
public const string XmlCanonicalizationStarted = @"XML canonicalization started";
public const string XmlMaxArrayLengthOrMaxItemsQuotaExceeded = @"XML max array length or max items quota exceeded. It must be less than {0}.";
public const string XmlMaxBytesPerReadExceeded = @"XML max bytes per read exceeded. It must be less than {0}.";
public const string XmlMaxDepthExceeded = @"XML max depth exceeded. It must be less than {0}.";
public const string XmlMaxStringContentLengthExceeded = @"XML max string content length exceeded. It must be less than {0}.";
public const string XmlObjectAssignedToIncompatibleInterface = @"Object of type '{0}' is assigned to an incompatible interface '{1}'.";

#endregion

	// CoreFX
	public const string PlatformNotSupported_SchemaImporter = "The implementation of the function requires System.Runtime.Serialization.SchemaImporter which is not supported on this platform.";
	public const string PlatformNotSupported_IDataContractSurrogate = "The implementation of the function requires System.Runtime.Serialization.IDataContractSurrogate which is not supported on this platform.";

	internal static string Format(string resourceFormats)
	{
		return resourceFormats;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		return string.Format (CultureInfo.InvariantCulture, resourceFormat, p1);
	}
}
}
