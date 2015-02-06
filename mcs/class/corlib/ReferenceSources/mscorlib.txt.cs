/*

Extracted from https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/mscorlib.txt

	var res = File.ReadAllLines (input_file).Select (l => {
		if (l.Length == 0 || l.StartsWith ("#"))
			return l;

		if (l.StartsWith (";"))
			return "// " + l;

		return "case \"" + l.Replace ("\"", "\\\"").Replace ("= ", "=").Replace (" =", "=").Replace ("=", "\": return \"") + "\";";
	});

*/

#define INCLUDE_RUNTIME
#define INCLUDE_DEBUG
#define FEATURE_MACL
#define FEATURE_IDENTITY_REFERENCE
#define FEATURE_COMINTEROP
#define FEATURE_CRYPTO
#define FEATURE_REMOTING
#define FEATURE_SERIALIZATION

static class Messages
{
	public static string GetMessage (string id) {
		switch (id) {

// ;==++==
// ;
// ;   Copyright (c) Microsoft Corporation.  All rights reserved.
// ;
// ;==--==
// ;
// ; These are the managed resources for mscorlib.dll.
// ; See those first three bytes in the file?  This is in UTF-8.  Leave the
// ; Unicode byte order mark (U+FEFF) written in UTF-8 at the start of this file.

// ; For resource info, see the ResourceManager documentation and the ResGen tool,
// ; which is a managed app using ResourceWriter.
// ; ResGen now supports C++ & C# style #ifdef's, like #ifndef FOO and #if BAR

// ; The naming scheme is: [Namespace.] ExceptionName _ Reason
// ; We'll suppress "System." where possible.
// ; Examples:
// ; Argument_Null
// ; Reflection.TargetInvokation_someReason

// ; Usage Notes:
// ; * Keep exceptions in alphabetical order by package
// ; * A single space may exist on either side of the equal sign.
// ; * Follow the naming conventions.
// ; * Any lines starting with a '#' or ';' are ignored
// ; * Equal signs aren't legal characters for keys, but may occur in values.
// ; * Correctly punctuate all sentences. Most resources should end in a period.
// ;       Remember, your mother will probably read some of these messages.
// ; * You may use " (quote), \n and \t. Use \\ for a single '\' character.
// ; * String inserts work.  i.e., BadNumber_File = Wrong number in file "{0}".

// ; Real words, used by code like Environment.StackTrace
#if INCLUDE_RUNTIME
case "Word_At": return "at";
case "StackTrace_InFileLineNumber": return "in {0}:line {1}";
case "UnknownError_Num": return "Unknown error \"{0}\".";
case "AllocatedFrom": return "Allocated from:";

// ; Note this one is special, used as a divider between stack traces!
case "Exception_EndOfInnerExceptionStack": return "--- End of inner exception stack trace ---";
case "Exception_WasThrown": return "Exception of type '{0}' was thrown.";

// ; The following are used in the implementation of ExceptionDispatchInfo
case "Exception_EndStackTraceFromPreviousThrow": return "--- End of stack trace from previous location where exception was thrown ---";

case "Arg_ParamName_Name": return "Parameter name: {0}";
case "ArgumentOutOfRange_ActualValue": return "Actual value was {0}.";

#endif // INCLUDE_RUNTIME

#if !FEATURE_CORECLR
case "UnknownError": return "Unknown error.";
#endif // !FEATURE_CORECLR

#if INCLUDE_DEBUG

// ; For code contracts
case "AssumptionFailed": return "Assumption failed.";
case "AssumptionFailed_Cnd": return "Assumption failed: {0}";
case "AssertionFailed": return "Assertion failed.";
case "AssertionFailed_Cnd": return "Assertion failed: {0}";
case "PreconditionFailed": return "Precondition failed.";
case "PreconditionFailed_Cnd": return "Precondition failed: {0}";
case "PostconditionFailed": return "Postcondition failed.";
case "PostconditionFailed_Cnd": return "Postcondition failed: {0}";
case "PostconditionOnExceptionFailed": return "Postcondition failed after throwing an exception.";
case "PostconditionOnExceptionFailed_Cnd": return "Postcondition failed after throwing an exception: {0}";
case "InvariantFailed": return "Invariant failed.";
case "InvariantFailed_Cnd": return "Invariant failed: {0}";
#if PLATFORM_UNIX
case "StackTrace_Stack": return "Stack trace: \n{0}";
#endif
#if !PLATFORM_UNIX
case "StackTrace_Stack": return "Stack trace: \r\n{0}";
#endif // PLATFORM_UNIX
case "MustUseCCRewrite": return "An assembly (probably \"{1}\") must be rewritten using the code contracts binary rewriter (CCRewrite) because it is calling Contract.{0} and the CONTRACTS_FULL symbol is defined.  Remove any explicit definitions of the CONTRACTS_FULL symbol from your project and rebuild.";

// ; Access Control
#if FEATURE_MACL
case "AccessControl_MustSpecifyContainerAcl": return "The named parameter must be a container ACL.";
case "AccessControl_MustSpecifyLeafObjectAcl": return "The named parameter must be a non-container ACL.";
case "AccessControl_AclTooLong": return "Length of the access control list exceed the allowed maximum.";
case "AccessControl_MustSpecifyDirectoryObjectAcl": return "The named parameter must be a directory-object ACL.";
case "AccessControl_MustSpecifyNonDirectoryObjectAcl": return "The named parameter must be a non-directory-object ACL.";
case "AccessControl_InvalidSecurityDescriptorRevision": return "Security descriptor with revision other than '1' are not legal.";
case "AccessControl_InvalidSecurityDescriptorSelfRelativeForm": return "Security descriptor must be in the self-relative form.";
case "AccessControl_NoAssociatedSecurity": return "Unable to perform a security operation on an object that has no associated security. This can happen when trying to get an ACL of an anonymous kernel object.";
case "AccessControl_InvalidHandle": return "The supplied handle is invalid. This can happen when trying to set an ACL on an anonymous kernel object.";
case "AccessControl_UnexpectedError": return "Method failed with unexpected error code {0}.";
case "AccessControl_InvalidSidInSDDLString": return "The SDDL string contains an invalid sid or a sid that cannot be translated.";
case "AccessControl_InvalidOwner": return "The security identifier is not allowed to be the owner of this object.";
case "AccessControl_InvalidGroup": return "The security identifier is not allowed to be the primary group of this object.";
case "AccessControl_InvalidAccessRuleType": return "The access rule is not the correct type.";
case "AccessControl_InvalidAuditRuleType": return "The audit rule is not the correct type.";
#endif  // FEATURE_MACL

// ; Identity Reference Library
#if FEATURE_IDENTITY_REFERENCE
case "IdentityReference_IdentityNotMapped": return "Some or all identity references could not be translated.";
case "IdentityReference_MustBeIdentityReference": return "The targetType parameter must be of IdentityReference type.";
case "IdentityReference_AccountNameTooLong": return "Account name is too long.";
case "IdentityReference_DomainNameTooLong": return "Domain name is too long.";
case "IdentityReference_InvalidNumberOfSubauthorities": return "The number of sub-authorities must not exceed {0}.";
case "IdentityReference_IdentifierAuthorityTooLarge": return "The size of the identifier authority must not exceed 6 bytes.";
case "IdentityReference_InvalidSidRevision": return "SIDs with revision other than '1' are not supported.";
case "IdentityReference_CannotCreateLogonIdsSid": return "Well-known SIDs of type LogonIdsSid cannot be created.";
case "IdentityReference_DomainSidRequired": return "The domainSid parameter must be specified for creating well-known SID of type {0}.";
case "IdentityReference_NotAWindowsDomain": return "The domainSid parameter is not a valid Windows domain SID.";
#endif // FEATURE_IDENTITY_REFERENCE

// ; AccessException
case "Acc_CreateGeneric": return "Cannot create a type for which Type.ContainsGenericParameters is true.";
case "Acc_CreateAbst": return "Cannot create an abstract class.";
case "Acc_CreateInterface": return "Cannot create an instance of an interface.";
case "Acc_NotClassInit": return "Type initializer was not callable.";
case "Acc_CreateGenericEx": return "Cannot create an instance of {0} because Type.ContainsGenericParameters is true.";
case "Acc_CreateArgIterator": return "Cannot dynamically create an instance of ArgIterator.";
case "Acc_CreateAbstEx": return "Cannot create an instance of {0} because it is an abstract class.";
case "Acc_CreateInterfaceEx": return "Cannot create an instance of {0} because it is an interface.";
case "Acc_CreateVoid": return "Cannot dynamically create an instance of System.Void.";
case "Acc_ReadOnly": return "Cannot set a constant field.";
case "Acc_RvaStatic": return "SkipVerification permission is needed to modify an image-based (RVA) static field.";
case "Access_Void": return "Cannot create an instance of void.";

// ; ArgumentException
case "Arg_TypedReference_Null": return "The TypedReference must be initialized.";
case "Argument_AddingDuplicate__": return "Item has already been added. Key in dictionary: '{0}'  Key being added: '{1}'";
case "Argument_AddingDuplicate": return "An item with the same key has already been added.";
case "Argument_MethodDeclaringTypeGenericLcg": return "Method '{0}' has a generic declaring type '{1}'. Explicitly provide the declaring type to GetTokenFor. ";
case "Argument_MethodDeclaringTypeGeneric": return "Cannot resolve method {0} because the declaring type of the method handle {1} is generic. Explicitly provide the declaring type to GetMethodFromHandle. ";
case "Argument_FieldDeclaringTypeGeneric": return "Cannot resolve field {0} because the declaring type of the field handle {1} is generic. Explicitly provide the declaring type to GetFieldFromHandle.";
case "Argument_ApplicationTrustShouldHaveIdentity": return "An ApplicationTrust must have an application identity before it can be persisted.";
case "Argument_ConversionOverflow": return "Conversion buffer overflow.";
case "Argument_CodepageNotSupported": return "{0} is not a supported code page.";
case "Argument_CultureNotSupported": return "Culture is not supported.";
case "Argument_CultureInvalidIdentifier": return "{0} is an invalid culture identifier.";
case "Argument_OneOfCulturesNotSupported": return "Culture name {0} or {1} is not supported.";
case "Argument_CultureIetfNotSupported": return "Culture IETF Name {0} is not a recognized IETF name.";
case "Argument_CultureIsNeutral": return "Culture ID {0} (0x{0:X4}) is a neutral culture; a region cannot be created from it.";
case "Argument_InvalidNeutralRegionName": return "The region name {0} should not correspond to neutral culture; a specific culture name is required.";
case "Argument_InvalidGenericInstArray": return "Generic arguments must be provided for each generic parameter and each generic argument must be a RuntimeType.";
case "Argument_GenericArgsCount": return "The number of generic arguments provided doesn't equal the arity of the generic type definition.";
case "Argument_CultureInvalidFormat": return "Culture '{0}' is a neutral culture. It cannot be used in formatting and parsing and therefore cannot be set as the thread's current culture.";
case "Argument_CompareOptionOrdinal": return "CompareOption.Ordinal cannot be used with other options.";
case "Argument_CustomCultureCannotBePassedByNumber": return "Customized cultures cannot be passed by LCID, only by name.";
case "Argument_EncodingConversionOverflowChars": return "The output char buffer is too small to contain the decoded characters, encoding '{0}' fallback '{1}'.";
case "Argument_EncodingConversionOverflowBytes": return "The output byte buffer is too small to contain the encoded data, encoding '{0}' fallback '{1}'.";
case "Argument_EncoderFallbackNotEmpty": return "Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.";
case "Argument_EmptyFileName": return "Empty file name is not legal.";
case "Argument_EmptyPath": return "Empty path name is not legal.";
case "Argument_EmptyName": return "Empty name is not legal.";
case "Argument_ImplementIComparable": return "At least one object must implement IComparable.";
case "Argument_InvalidType": return "The type of arguments passed into generic comparer methods is invalid.";
case "Argument_InvalidTypeForCA": return "Cannot build type parameter for custom attribute with a type that does not support the AssemblyQualifiedName property. The type instance supplied was of type '{0}'.";
case "Argument_IllegalEnvVarName": return "Environment variable name cannot contain equal character.";
case "Argument_IllegalAppId": return "Application identity does not have same number of components as manifest paths.";
case "Argument_IllegalAppBase": return "The application base specified is not valid.";
case "Argument_UnableToParseManifest": return "Unexpected error while parsing the specified manifest.";
case "Argument_IllegalAppIdMismatch": return "Application identity does not match identities in manifests.";
case "Argument_InvalidAppId": return "Invalid identity: no deployment or application identity specified.";
case "Argument_InvalidGenericArg": return "The generic type parameter was not valid";
case "Argument_InvalidArrayLength": return "Length of the array must be {0}.";
case "Argument_InvalidArrayType": return "Target array type is not compatible with the type of items in the collection.";
case "Argument_InvalidAppendMode": return "Append access can be requested only in write-only mode.";
case "Argument_InvalidEnumValue": return "The value '{0}' is not valid for this usage of the type {1}.";
case "Argument_EnumIsNotIntOrShort": return "The underlying type of enum argument must be Int32 or Int16.";
case "Argument_InvalidEnum": return "The Enum type should contain one and only one instance field.";
case "Argument_InvalidKeyStore": return "'{0}' is not a valid KeyStore name. ";
case "Argument_InvalidFileMode&AccessCombo": return "Combining FileMode: {0} with FileAccess: {1} is invalid.";
case "Argument_InvalidFileMode&RightsCombo": return "Combining FileMode: {0} with FileSystemRights: {1} is invalid.";
case "Argument_InvalidFileModeTruncate&RightsCombo": return "Combining FileMode: {0} with FileSystemRights: {1} is invalid. FileMode.Truncate is valid only when used with FileSystemRights.Write.";
case "Argument_InvalidFlag": return "Value of flags is invalid.";
case "Argument_InvalidAnyFlag": return "No flags can be set.";
case "Argument_InvalidHandle": return "The handle is invalid.";
case "Argument_InvalidRegistryKeyPermissionCheck": return "The specified RegistryKeyPermissionCheck value is invalid.";
case "Argument_InvalidRegistryOptionsCheck": return "The specified RegistryOptions value is invalid.";
case "Argument_InvalidRegistryViewCheck": return "The specified RegistryView value is invalid.";
case "Argument_InvalidSubPath": return "The directory specified, '{0}', is not a subdirectory of '{1}'.";
case "Argument_NoRegionInvariantCulture": return "There is no region associated with the Invariant Culture (Culture ID: 0x7F).";
case "Argument_ResultCalendarRange": return "The result is out of the supported range for this calendar. The result should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive.";
case "Argument_ResultIslamicCalendarRange": return "The date is out of the supported range for the Islamic calendar. The date should be greater than July 18th, 622 AD (Gregorian date).";
case "Argument_NeverValidGenericArgument": return "The type '{0}' may not be used as a type argument.";
case "Argument_NotEnoughGenArguments": return "The type or method has {1} generic parameter(s), but {0} generic argument(s) were provided. A generic argument must be provided for each generic parameter.";
case "Argument_NullFullTrustAssembly": return "A null StrongName was found in the full trust assembly list.";
case "Argument_GenConstraintViolation": return "GenericArguments[{0}], '{1}', on '{2}' violates the constraint of type '{3}'.";
case "Argument_InvalidToken": return "Token {0:x} is not valid in the scope of module {1}.";
case "Argument_InvalidTypeToken": return "Token {0:x} is not a valid Type token.";
case "Argument_ResolveType": return "Token {0:x} is not a valid Type token in the scope of module {1}.";
case "Argument_ResolveMethod": return "Token {0:x} is not a valid MethodBase token in the scope of module {1}.";
case "Argument_ResolveField": return "Token {0:x} is not a valid FieldInfo token in the scope of module {1}.";
case "Argument_ResolveMember": return "Token {0:x} is not a valid MemberInfo token in the scope of module {1}.";
case "Argument_ResolveString": return "Token {0:x} is not a valid string token in the scope of module {1}.";
case "Argument_ResolveModuleType": return "Token {0} resolves to the special module type representing this module.";
case "Argument_ResolveMethodHandle": return "Type handle '{0}' and method handle with declaring type '{1}' are incompatible. Get RuntimeMethodHandle and declaring RuntimeTypeHandle off the same MethodBase.";
case "Argument_ResolveFieldHandle": return "Type handle '{0}' and field handle with declaring type '{1}' are incompatible. Get RuntimeFieldHandle and declaring RuntimeTypeHandle off the same FieldInfo.";
case "Argument_ResourceScopeWrongDirection": return "Resource type in the ResourceScope enum is going from a more restrictive resource type to a more general one.  From: \"{0}\"  To: \"{1}\"";
case "Argument_BadResourceScopeTypeBits": return "Unknown value for the ResourceScope: {0}  Too many resource type bits may be set.";
case "Argument_BadResourceScopeVisibilityBits": return "Unknown value for the ResourceScope: {0}  Too many resource visibility bits may be set.";
case "Argument_WaitHandleNameTooLong": return "The name can be no more than 260 characters in length.";
case "Argument_EnumTypeDoesNotMatch": return "The argument type, '{0}', is not the same as the enum type '{1}'.";
case "InvalidOperation_MethodBuilderBaked": return "The signature of the MethodBuilder can no longer be modified because an operation on the MethodBuilder caused the methodDef token to be created. For example, a call to SetCustomAttribute requires the methodDef token to emit the CustomAttribute token.";
case "InvalidOperation_GenericParametersAlreadySet": return "The generic parameters are already defined on this MethodBuilder.";
case "Arg_AccessException": return "Cannot access member.";
case "Arg_AppDomainUnloadedException": return "Attempted to access an unloaded AppDomain.";
case "Arg_ApplicationException": return "Error in the application.";
case "Arg_ArgumentOutOfRangeException": return "Specified argument was out of the range of valid values.";
case "Arg_ArithmeticException": return "Overflow or underflow in the arithmetic operation.";
case "Arg_ArrayLengthsDiffer": return "Array lengths must be the same.";
case "Arg_ArrayPlusOffTooSmall": return "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
case "Arg_ArrayTypeMismatchException": return "Attempted to access an element as a type incompatible with the array.";
case "Arg_BadImageFormatException": return "Format of the executable (.exe) or library (.dll) is invalid.";
case "Argument_BadImageFormatExceptionResolve": return "A BadImageFormatException has been thrown while parsing the signature. This is likely due to lack of a generic context. Ensure genericTypeArguments and genericMethodArguments are provided and contain enough context.";
case "Arg_BufferTooSmall": return "Not enough space available in the buffer.";
case "Arg_CATypeResolutionFailed": return "Failed to resolve type from string \"{0}\" which was embedded in custom attribute blob.";
case "Arg_CannotHaveNegativeValue": return "String cannot contain a minus sign if the base is not 10.";
case "Arg_CannotUnloadAppDomainException": return "Attempt to unload the AppDomain failed.";
case "Arg_CannotMixComparisonInfrastructure": return "The usage of IKeyComparer and IHashCodeProvider/IComparer interfaces cannot be mixed; use one or the other.";
case "Arg_ContextMarshalException": return "Attempted to marshal an object across a context boundary.";
case "Arg_DataMisalignedException": return "A datatype misalignment was detected in a load or store instruction.";
case "Arg_DevicesNotSupported": return "FileStream will not open Win32 devices such as disk partitions and tape drives. Avoid use of \"\\\\.\\\" in the path.";
case "Arg_DuplicateWaitObjectException": return "Duplicate objects in argument.";
case "Arg_EntryPointNotFoundException": return "Entry point was not found.";
case "Arg_DllNotFoundException": return "Dll was not found.";
case "Arg_ExecutionEngineException": return "Internal error in the runtime.";
case "Arg_FieldAccessException": return "Attempted to access a field that is not accessible by the caller.";
case "Arg_FileIsDirectory_Name": return "The target file \"{0}\" is a directory, not a file.";
case "Arg_FormatException": return "One of the identified items was in an invalid format.";
case "Arg_IndexOutOfRangeException": return "Index was outside the bounds of the array.";
case "Arg_InsufficientExecutionStackException": return "Insufficient stack to continue executing the program safely. This can happen from having too many functions on the call stack or function on the stack using too much stack space.";
case "Arg_InvalidCastException": return "Specified cast is not valid.";
case "Arg_InvalidOperationException": return "Operation is not valid due to the current state of the object.";
case "Arg_CorruptedCustomCultureFile": return "The file of the custom culture {0} is corrupt. Try to unregister this culture.";
case "Arg_InvokeMember": return "InvokeMember can be used only for COM objects.";
case "Arg_InvalidNeutralResourcesLanguage_Asm_Culture": return "The NeutralResourcesLanguageAttribute on the assembly \"{0}\" specifies an invalid culture name: \"{1}\".";
case "Arg_InvalidNeutralResourcesLanguage_FallbackLoc": return "The NeutralResourcesLanguageAttribute specifies an invalid or unrecognized ultimate resource fallback location: \"{0}\".";
case "Arg_InvalidSatelliteContract_Asm_Ver": return "Satellite contract version attribute on the assembly '{0}' specifies an invalid version: {1}.";
case "Arg_MethodAccessException": return "Attempt to access the method failed.";
case "Arg_MethodAccessException_WithMethodName": return "Attempt to access the method \"{0}\" on type \"{1}\" failed.";
case "Arg_MethodAccessException_WithCaller": return "Attempt by security transparent method '{0}' to access security critical method '{1}' failed.";
case "Arg_MissingFieldException": return "Attempted to access a non-existing field.";
case "Arg_MissingMemberException": return "Attempted to access a missing member.";
case "Arg_MissingMethodException": return "Attempted to access a missing method.";
case "Arg_MulticastNotSupportedException": return "Attempted to add multiple callbacks to a delegate that does not support multicast.";
case "Arg_NotFiniteNumberException": return "Number encountered was not a finite quantity.";
case "Arg_NotSupportedException": return "Specified method is not supported.";
case "Arg_UnboundGenParam": return "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.";
case "Arg_UnboundGenField": return "Late bound operations cannot be performed on fields with types for which Type.ContainsGenericParameters is true.";
case "Arg_NotGenericParameter": return "Method may only be called on a Type for which Type.IsGenericParameter is true.";
case "Arg_GenericParameter": return "Method must be called on a Type for which Type.IsGenericParameter is false.";
case "Arg_NotGenericTypeDefinition": return "{0} is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.";
case "Arg_NotGenericMethodDefinition": return "{0} is not a GenericMethodDefinition. MakeGenericMethod may only be called on a method for which MethodBase.IsGenericMethodDefinition is true.";
case "Arg_BadLiteralFormat": return "Encountered an invalid type for a default value.";
case "Arg_MissingActivationArguments": return "The AppDomainSetup must specify the activation arguments for this call.";
case "Argument_BadParameterTypeForCAB": return "Cannot emit a CustomAttribute with argument of type {0}.";
case "Argument_InvalidMemberForNamedArgument": return "The member must be either a field or a property.";
case "Argument_InvalidTypeName": return "The name of the type is invalid.";

// ; Note - don't change the NullReferenceException default message. This was
// ; negotiated carefully with the VB team to avoid saying "null" or "nothing".
case "Arg_NullReferenceException": return "Object reference not set to an instance of an object.";

case "Arg_AccessViolationException": return "Attempted to read or write protected memory. This is often an indication that other memory is corrupt.";
case "Arg_OverflowException": return "Arithmetic operation resulted in an overflow.";
case "Arg_PathGlobalRoot": return "Paths that begin with \\\\?\\GlobalRoot are internal to the kernel and should not be opened by managed applications.";
case "Arg_PathIllegal": return "The path is not of a legal form.";
case "Arg_PathIllegalUNC": return "The UNC path should be of the form \\\\server\\share.";
case "Arg_RankException": return "Attempted to operate on an array with the incorrect number of dimensions.";
case "Arg_RankMultiDimNotSupported": return "Only single dimensional arrays are supported for the requested action.";
case "Arg_NonZeroLowerBound": return "The lower bound of target array must be zero.";
case "Arg_RegSubKeyValueAbsent": return "No value exists with that name.";
case "Arg_ResourceFileUnsupportedVersion": return "The ResourceReader class does not know how to read this version of .resources files. Expected version: {0}  This file: {1}";
case "Arg_ResourceNameNotExist": return "The specified resource name \"{0}\" does not exist in the resource file.";
case "Arg_SecurityException": return "Security error.";
case "Arg_SerializationException": return "Serialization error.";
case "Arg_StackOverflowException": return "Operation caused a stack overflow.";
case "Arg_SurrogatesNotAllowedAsSingleChar": return "Unicode surrogate characters must be written out as pairs together in the same call, not individually. Consider passing in a character array instead.";
case "Arg_SynchronizationLockException": return "Object synchronization method was called from an unsynchronized block of code.";
case "Arg_RWLockRestoreException": return "ReaderWriterLock.RestoreLock was called without releasing all locks acquired since the call to ReleaseLock.";
case "Arg_SystemException": return "System error.";
case "Arg_TimeoutException": return "The operation has timed out.";
case "Arg_UnauthorizedAccessException": return "Attempted to perform an unauthorized operation.";
case "Arg_ArgumentException": return "Value does not fall within the expected range.";
case "Arg_DirectoryNotFoundException": return "Attempted to access a path that is not on the disk.";
case "Arg_DriveNotFoundException": return "Attempted to access a drive that is not available.";
case "Arg_EndOfStreamException": return "Attempted to read past the end of the stream.";
case "Arg_HexStyleNotSupported": return "The number style AllowHexSpecifier is not supported on floating point data types.";
case "Arg_IOException": return "I/O error occurred.";
case "Arg_InvalidHexStyle": return "With the AllowHexSpecifier bit set in the enum bit field, the only other valid bits that can be combined into the enum value must be a subset of those in HexNumber.";
case "Arg_KeyNotFound": return "The given key was not present in the dictionary.";
case "Argument_InvalidNumberStyles": return "An undefined NumberStyles value is being used.";
case "Argument_InvalidDateTimeStyles": return "An undefined DateTimeStyles value is being used.";
case "Argument_InvalidTimeSpanStyles": return "An undefined TimeSpanStyles value is being used.";
case "Argument_DateTimeOffsetInvalidDateTimeStyles": return "The DateTimeStyles value 'NoCurrentDateDefault' is not allowed when parsing DateTimeOffset.";
case "Argument_NativeResourceAlreadyDefined": return "Native resource has already been defined.";
case "Argument_BadObjRef": return "Invalid ObjRef provided to '{0}'.";
case "Argument_InvalidCultureName": return "Culture name '{0}' is not supported.";
case "Argument_NameTooLong": return "The name '{0}' is too long to be a Culture or Region name, which is limited to {1} characters.";
case "Argument_NameContainsInvalidCharacters": return "The name '{0}' contains characters that are not valid for a Culture or Region.";
case "Argument_InvalidRegionName": return "Region name '{0}' is not supported.";
case "Argument_CannotCreateTypedReference": return "Cannot use function evaluation to create a TypedReference object.";
case "Arg_ArrayZeroError": return "Array must not be of length zero.";
case "Arg_BogusIComparer": return "Unable to sort because the IComparer.Compare() method returns inconsistent results. Either a value does not compare equal to itself, or one value repeatedly compared to another value yields different results. IComparer: '{0}'.";
case "Arg_CreatInstAccess": return "Cannot specify both CreateInstance and another access type.";
case "Arg_CryptographyException": return "Error occurred during a cryptographic operation.";
case "Arg_DateTimeRange": return "Combination of arguments to the DateTime constructor is out of the legal range.";
case "Arg_DecBitCtor": return "Decimal byte array constructor requires an array of length four containing valid decimal bytes.";
case "Arg_DlgtTargMeth": return "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type.";
case "Arg_DlgtTypeMis": return "Delegates must be of the same type.";
case "Arg_DlgtNullInst": return "Delegate to an instance method cannot have null 'this'.";
case "Arg_DllInitFailure": return "One machine may not have remote administration enabled, or both machines may not be running the remote registry service.";
case "Arg_EmptyArray": return "Array may not be empty.";
case "Arg_EmptyOrNullArray": return "Array may not be empty or null.";
case "Arg_EmptyCollection": return "Collection must not be empty.";
case "Arg_EmptyOrNullString": return "String may not be empty or null.";
case "Argument_ItemNotExist": return "The specified item does not exist in this KeyedCollection.";
case "Argument_EncodingNotSupported": return "'{0}' is not a supported encoding name. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.";
case "Argument_FallbackBufferNotEmpty": return "Cannot change fallback when buffer is not empty. Previous Convert() call left data in the fallback buffer.";
case "Argument_InvalidCodePageConversionIndex": return "Unable to translate Unicode character \\u{0:X4} at index {1} to specified code page.";
case "Argument_InvalidCodePageBytesIndex": return "Unable to translate bytes {0} at index {1} from specified code page to Unicode.";
case "Argument_RecursiveFallback": return "Recursive fallback not allowed for character \\u{0:X4}.";
case "Argument_RecursiveFallbackBytes": return "Recursive fallback not allowed for bytes {0}.";
case "Arg_EnumAndObjectMustBeSameType": return "Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.";
case "Arg_EnumIllegalVal": return "Illegal enum value: {0}.";
case "Arg_EnumNotSingleFlag": return "Must set exactly one flag.";
case "Arg_EnumAtLeastOneFlag": return "Must set at least one flag.";
case "Arg_EnumUnderlyingTypeAndObjectMustBeSameType": return "Enum underlying type and the object must be same type or object must be a String. Type passed in was '{0}'; the enum underlying type was '{1}'.";
case "Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType": return "Enum underlying type and the object must be same type or object. Type passed in was '{0}'; the enum underlying type was '{1}'.";
case "Arg_EnumMustHaveUnderlyingValueField": return "All enums must have an underlying value__ field.";
case "Arg_COMAccess": return "Must specify property Set or Get or method call for a COM Object.";
case "Arg_COMPropSetPut": return "Only one of the following binding flags can be set: BindingFlags.SetProperty, BindingFlags.PutDispProperty,  BindingFlags.PutRefDispProperty.";
case "Arg_FldSetGet": return "Cannot specify both Get and Set on a field.";
case "Arg_PropSetGet": return "Cannot specify both Get and Set on a property.";
case "Arg_CannotBeNaN": return "TimeSpan does not accept floating point Not-a-Number values.";
case "Arg_FldGetPropSet": return "Cannot specify both GetField and SetProperty.";
case "Arg_FldSetPropGet": return "Cannot specify both SetField and GetProperty.";
case "Arg_FldSetInvoke": return "Cannot specify Set on a Field and Invoke on a method.";
case "Arg_FldGetArgErr": return "No arguments can be provided to Get a field value.";
case "Arg_FldSetArgErr": return "Only the field value can be specified to set a field value.";
case "Arg_GetMethNotFnd": return "Property Get method was not found.";
case "Arg_GuidArrayCtor": return "Byte array for GUID must be exactly {0} bytes long.";
case "Arg_HandleNotAsync": return "Handle does not support asynchronous operations. The parameters to the FileStream constructor may need to be changed to indicate that the handle was opened synchronously (that is, it was not opened for overlapped I/O).";
case "Arg_HandleNotSync": return "Handle does not support synchronous operations. The parameters to the FileStream constructor may need to be changed to indicate that the handle was opened asynchronously (that is, it was opened explicitly for overlapped I/O).";
case "Arg_HTCapacityOverflow": return "Hashtable's capacity overflowed and went negative. Check load factor, capacity and the current size of the table.";
case "Arg_IndexMustBeInt": return "All indexes must be of type Int32.";
case "Arg_InvalidConsoleColor": return "The ConsoleColor enum value was not defined on that enum. Please use a defined color from the enum.";
case "Arg_InvalidFileAttrs": return "Invalid File or Directory attributes value.";
case "Arg_InvalidHandle": return "Invalid handle.";
case "Arg_InvalidTypeInSignature": return "The signature Type array contains some invalid type (i.e. null, void)";
case "Arg_InvalidTypeInRetType": return "The return Type contains some invalid type (i.e. null, ByRef)";
case "Arg_EHClauseNotFilter": return "This ExceptionHandlingClause is not a filter.";
case "Arg_EHClauseNotClause": return "This ExceptionHandlingClause is not a clause.";
case "Arg_ReflectionOnlyCA": return "It is illegal to reflect on the custom attributes of a Type loaded via ReflectionOnlyGetType (see Assembly.ReflectionOnly) -- use CustomAttributeData instead.";
case "Arg_ReflectionOnlyInvoke": return "It is illegal to invoke a method on a Type loaded via ReflectionOnlyGetType.";
case "Arg_ReflectionOnlyField": return "It is illegal to get or set the value on a field on a Type loaded via ReflectionOnlyGetType.";
case "Arg_MemberInfoNullModule": return "The Module object containing the member cannot be null.";
case "Arg_ParameterInfoNullMember": return "The MemberInfo object defining the parameter cannot be null.";
case "Arg_ParameterInfoNullModule": return "The Module object containing the parameter cannot be null.";
case "Arg_AssemblyNullModule": return "The manifest module of the assembly cannot be null.";
case "Arg_LongerThanSrcArray": return "Source array was not long enough. Check srcIndex and length, and the array's lower bounds.";
case "Arg_LongerThanDestArray": return "Destination array was not long enough. Check destIndex and length, and the array's lower bounds.";
case "Arg_LowerBoundsMustMatch": return "The arrays' lower bounds must be identical.";
case "Arg_MustBeBoolean": return "Object must be of type Boolean.";
case "Arg_MustBeByte": return "Object must be of type Byte.";
case "Arg_MustBeChar": return "Object must be of type Char.";
case "Arg_MustBeDateTime": return "Object must be of type DateTime.";
case "Arg_MustBeDateTimeOffset": return "Object must be of type DateTimeOffset.";
case "Arg_MustBeDecimal": return "Object must be of type Decimal.";
case "Arg_MustBeDelegate": return "Type must derive from Delegate.";
case "Arg_MustBeDouble": return "Object must be of type Double.";
case "Arg_MustBeDriveLetterOrRootDir": return "Object must be a root directory (\"C:\\\") or a drive letter (\"C\").";
case "Arg_MustBeEnum": return "Type provided must be an Enum.";
case "Arg_MustBeEnumBaseTypeOrEnum": return "The value passed in must be an enum base or an underlying type for an enum, such as an Int32.";
case "Arg_MustBeGuid": return "Object must be of type GUID.";
case "Arg_MustBeIdentityReferenceType": return "Type must be an IdentityReference, such as NTAccount or SecurityIdentifier.";
case "Arg_MustBeInterface": return "Type passed must be an interface.";
case "Arg_MustBeInt16": return "Object must be of type Int16.";
case "Arg_MustBeInt32": return "Object must be of type Int32.";
case "Arg_MustBeInt64": return "Object must be of type Int64.";
case "Arg_MustBePrimArray": return "Object must be an array of primitives.";
case "Arg_MustBePointer": return "Type must be a Pointer.";
case "Arg_MustBeStatic": return "Method must be a static method.";
case "Arg_MustBeString": return "Object must be of type String.";
case "Arg_MustBeStringPtrNotAtom": return "The pointer passed in as a String must not be in the bottom 64K of the process's address space.";
case "Arg_MustBeSByte": return "Object must be of type SByte.";
case "Arg_MustBeSingle": return "Object must be of type Single.";
case "Arg_MustBeTimeSpan": return "Object must be of type TimeSpan.";
case "Arg_MustBeType": return "Type must be a type provided by the runtime.";
case "Arg_MustBeUInt16": return "Object must be of type UInt16.";
case "Arg_MustBeUInt32": return "Object must be of type UInt32.";
case "Arg_MustBeUInt64": return "Object must be of type UInt64.";
case "Arg_MustBeVersion": return "Object must be of type Version.";
case "Arg_MustBeTrue": return "Argument must be true.";
case "Arg_MustAllBeRuntimeType": return "At least one type argument is not a runtime type.";
case "Arg_NamedParamNull": return "Named parameter value must not be null.";
case "Arg_NamedParamTooBig": return "Named parameter array cannot be bigger than argument array.";
case "Arg_Need1DArray": return "Array was not a one-dimensional array.";
case "Arg_Need2DArray": return "Array was not a two-dimensional array.";
case "Arg_Need3DArray": return "Array was not a three-dimensional array.";
case "Arg_NeedAtLeast1Rank": return "Must provide at least one rank.";
case "Arg_NoDefCTor": return "No parameterless constructor defined for this object.";
case "Arg_BitArrayTypeUnsupported": return "Only supported array types for CopyTo on BitArrays are Boolean[], Int32[] and Byte[].";
case "Arg_DivideByZero": return "Attempted to divide by zero.";
case "Arg_NoAccessSpec": return "Must specify binding flags describing the invoke operation required (BindingFlags.InvokeMethod CreateInstance GetField SetField GetProperty SetProperty).";
case "Arg_NoStaticVirtual": return "Method cannot be both static and virtual.";
case "Arg_NotFoundIFace": return "Interface not found.";
case "Arg_ObjObjEx": return "Object of type '{0}' cannot be converted to type '{1}'.";
case "Arg_ObjObj": return "Object type cannot be converted to target type.";
case "Arg_FieldDeclTarget": return "Field '{0}' defined on type '{1}' is not a field on the target object which is of type '{2}'.";
case "Arg_OleAutDateInvalid": return "Not a legal OleAut date.";
case "Arg_OleAutDateScale": return "OleAut date did not convert to a DateTime correctly.";
case "Arg_PlatformNotSupported": return "Operation is not supported on this platform.";
case "Arg_PlatformSecureString": return "SecureString is only supported on Windows 2000 SP3 and higher platforms.";
case "Arg_ParmCnt": return "Parameter count mismatch.";
case "Arg_ParmArraySize": return "Must specify one or more parameters.";
case "Arg_Path2IsRooted": return "Second path fragment must not be a drive or UNC name.";
case "Arg_PathIsVolume": return "Path must not be a drive.";
case "Arg_PrimWiden": return "Cannot widen from source type to target type either because the source type is a not a primitive type or the conversion cannot be accomplished.";
case "Arg_NullIndex": return "Arrays indexes must be set to an object instance.";
case "Arg_VarMissNull": return "Missing parameter does not have a default value.";
case "Arg_PropSetInvoke": return "Cannot specify Set on a property and Invoke on a method.";
case "Arg_PropNotFound": return "Could not find the specified property.";
case "Arg_RankIndices": return "Indices length does not match the array rank.";
case "Arg_RanksAndBounds": return "Number of lengths and lowerBounds must match.";
case "Arg_RegSubKeyAbsent": return "Cannot delete a subkey tree because the subkey does not exist.";
case "Arg_RemoveArgNotFound": return "Cannot remove the specified item because it was not found in the specified Collection.";
case "Arg_RegKeyDelHive": return "Cannot delete a registry hive's subtree.";
case "Arg_RegKeyNoRemoteConnect": return "No remote connection to '{0}' while trying to read the registry.";
case "Arg_RegKeyOutOfRange": return "Registry HKEY was out of the legal range.";
case "Arg_RegKeyNotFound": return "The specified registry key does not exist.";
case "Arg_RegKeyStrLenBug": return "Registry key names should not be greater than 255 characters.";
case "Arg_RegValStrLenBug": return "Registry value names should not be greater than 16,383 characters.";
case "Arg_RegBadKeyKind": return "The specified RegistryValueKind is an invalid value.";
case "Arg_RegGetOverflowBug": return "RegistryKey.GetValue does not allow a String that has a length greater than Int32.MaxValue.";
case "Arg_RegSetMismatchedKind": return "The type of the value object did not match the specified RegistryValueKind or the object could not be properly converted.";
case "Arg_RegSetBadArrType": return "RegistryKey.SetValue does not support arrays of type '{0}'. Only Byte[] and String[] are supported.";
case "Arg_RegSetStrArrNull": return "RegistryKey.SetValue does not allow a String[] that contains a null String reference.";
case "Arg_RegInvalidKeyName": return "Registry key name must start with a valid base key name.";
case "Arg_ResMgrNotResSet": return "Type parameter must refer to a subclass of ResourceSet.";
case "Arg_SetMethNotFnd": return "Property set method not found.";
case "Arg_TypeRefPrimitve": return "TypedReferences cannot be redefined as primitives.";
case "Arg_UnknownTypeCode": return "Unknown TypeCode value.";
case "Arg_VersionString": return "Version string portion was too short or too long.";
case "Arg_NoITypeInfo": return "Specified TypeInfo was invalid because it did not support the ITypeInfo interface.";
case "Arg_NoITypeLib": return "Specified TypeLib was invalid because it did not support the ITypeLib interface.";
case "Arg_NoImporterCallback": return "Specified type library importer callback was invalid because it did not support the ITypeLibImporterNotifySink interface.";
case "Arg_ImporterLoadFailure": return "The type library importer encountered an error during type verification. Try importing without class members.";
case "Arg_InvalidBase": return "Invalid Base.";
case "Arg_EnumValueNotFound": return "Requested value '{0}' was not found.";
case "Arg_EnumLitValueNotFound": return "Literal value was not found.";
case "Arg_MustContainEnumInfo": return "Must specify valid information for parsing in the string.";
case "Arg_InvalidSearchPattern": return "Search pattern cannot contain \"..\" to move up directories and can be contained only internally in file/directory names, as in \"a..b\".";
case "Arg_NegativeArgCount": return "Argument count must not be negative.";
case "Arg_InvalidAccessEntry": return "Specified access entry is invalid because it is unrestricted. The global flags should be specified instead.";
case "Arg_InvalidFileName": return "Specified file name was invalid.";
case "Arg_InvalidFileExtension": return "Specified file extension was not a valid extension.";
case "Arg_COMException": return "Error HRESULT E_FAIL has been returned from a call to a COM component.";
case "Arg_ExternalException": return "External component has thrown an exception.";
case "Arg_InvalidComObjectException": return "Attempt has been made to use a COM object that does not have a backing class factory.";
case "Arg_InvalidOleVariantTypeException": return "Specified OLE variant was invalid.";
case "Arg_MarshalDirectiveException": return "Marshaling directives are invalid.";
case "Arg_MarshalAsAnyRestriction": return "AsAny cannot be used on return types, ByRef parameters, ArrayWithOffset, or parameters passed from unmanaged to managed.";
case "Arg_NDirectBadObject": return "No PInvoke conversion exists for value passed to Object-typed parameter.";
case "Arg_SafeArrayTypeMismatchException": return "Specified array was not of the expected type.";
case "Arg_VTableCallsNotSupportedException": return "Attempted to make an early bound call on a COM dispatch-only interface.";
case "Arg_SafeArrayRankMismatchException": return "Specified array was not of the expected rank.";
case "Arg_AmbiguousMatchException": return "Ambiguous match found.";
case "Arg_CustomAttributeFormatException": return "Binary format of the specified custom attribute was invalid.";
case "Arg_InvalidFilterCriteriaException": return "Specified filter criteria was invalid.";
case "Arg_TypeLoadNullStr": return "A null or zero length string does not represent a valid Type.";
case "Arg_TargetInvocationException": return "Exception has been thrown by the target of an invocation.";
case "Arg_TargetParameterCountException": return "Number of parameters specified does not match the expected number.";
case "Arg_TypeAccessException": return "Attempt to access the type failed.";
case "Arg_TypeLoadException": return "Failure has occurred while loading a type.";
case "Arg_TypeUnloadedException": return "Type had been unloaded.";
case "Arg_ThreadStateException": return "Thread was in an invalid state for the operation being executed.";
case "Arg_ThreadStartException": return "Thread failed to start.";
case "Arg_WrongAsyncResult": return "IAsyncResult object did not come from the corresponding async method on this type.";
case "Arg_WrongType": return "The value \"{0}\" is not of type \"{1}\" and cannot be used in this generic collection.";
case "Argument_InvalidArgumentForComparison": return "Type of argument is not compatible with the generic comparer.";
case "Argument_ALSInvalidCapacity": return "Specified capacity must not be less than the current capacity.";
case "Argument_ALSInvalidSlot": return "Specified slot number was invalid.";
case "Argument_IdnIllegalName": return "Decoded string is not a valid IDN name.";
case "Argument_IdnBadBidi": return "Left to right characters may not be mixed with right to left characters in IDN labels.";
case "Argument_IdnBadLabelSize": return "IDN labels must be between 1 and 63 characters long.";
case "Argument_IdnBadNameSize": return "IDN names must be between 1 and {0} characters long.";
case "Argument_IdnBadPunycode": return "Invalid IDN encoded string.";
case "Argument_IdnBadStd3": return "Label contains character '{0}' not allowed with UseStd3AsciiRules";
case "Arg_InvalidANSIString": return "The ANSI string passed in could not be converted from the default ANSI code page to Unicode.";
case "Arg_InvalidUTF8String": return "The UTF8 string passed in could not be converted to Unicode.";
case "Argument_InvalidCharSequence": return "Invalid Unicode code point found at index {0}.";
case "Argument_InvalidCharSequenceNoIndex": return "String contains invalid Unicode code points.";
case "Argument_InvalidCalendar": return "Not a valid calendar for the given culture.";
case "Argument_InvalidNormalizationForm": return "Invalid or unsupported normalization form.";
case "Argument_InvalidPathChars": return "Illegal characters in path.";
case "Argument_InvalidOffLen": return "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
case "Argument_InvalidSeekOrigin": return "Invalid seek origin.";
case "Argument_SeekOverflow": return "The specified seek offset '{0}' would result in a negative Stream position.";
case "Argument_InvalidUnity": return "Invalid Unity type.";
case "Argument_LongEnvVarName": return "Environment variable name cannot contain 1024 or more characters.";
case "Argument_LongEnvVarValue": return "Environment variable name or value is too long.";
case "Argument_StringFirstCharIsZero": return "The first char in the string is the null character.";
case "Argument_OnlyMscorlib": return "Only mscorlib's assembly is valid.";
case "Argument_PathEmpty": return "Path cannot be the empty string or all whitespace.";
case "Argument_PathFormatNotSupported": return "The given path's format is not supported.";
case "Argument_PathUriFormatNotSupported": return "URI formats are not supported.";
case "Argument_TypeNameTooLong": return "Type name was too long. The fully qualified type name must be less than 1,024 characters.";
case "Argument_StreamNotReadable": return "Stream was not readable.";
case "Argument_StreamNotWritable": return "Stream was not writable.";
case "Argument_InvalidNumberOfMembers": return "MemberData contains an invalid number of members.";
case "Argument_InvalidValue": return "Value was invalid.";
case "Argument_InvalidKey": return "Key was invalid.";
case "Argument_MinMaxValue": return "'{0}' cannot be greater than {1}.";
case "Argument_InvalidGroupSize": return "Every element in the value array should be between one and nine, except for the last element, which can be zero.";
case "Argument_MustHaveAttributeBaseClass": return "Type passed in must be derived from System.Attribute or System.Attribute itself.";
case "Argument_NoUninitializedStrings": return "Uninitialized Strings cannot be created.";
case "Argument_UnequalMembers": return "Supplied MemberInfo does not match the expected type.";
case "Argument_BadFormatSpecifier": return "Format specifier was invalid.";
case "Argument_InvalidHighSurrogate": return "Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.";
case "Argument_InvalidLowSurrogate": return "Found a low surrogate char without a preceding high surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.";
case "Argument_UnmatchingSymScope": return "Non-matching symbol scope.";
case "Argument_NotInExceptionBlock": return "Not currently in an exception block.";
case "Argument_BadExceptionCodeGen": return "Incorrect code generation for exception block.";
case "Argument_NotExceptionType": return "Does not extend Exception.";
case "Argument_DuplicateResourceName": return "Duplicate resource name within an assembly.";
case "Argument_BadPersistableModuleInTransientAssembly": return "Cannot have a persistable module in a transient assembly.";
case "Argument_InvalidPermissionState": return "Invalid permission state.";
case "Argument_UnrestrictedIdentityPermission": return "Identity permissions cannot be unrestricted.";
case "Argument_WrongType": return "Operation on type '{0}' attempted with target of incorrect type.";
case "Argument_IllegalZone": return "Illegal security permission zone specified.";
case "Argument_HasToBeArrayClass": return "Must be an array type.";
case "Argument_InvalidDirectory": return "Invalid directory, '{0}'.";
case "Argument_DataLengthDifferent": return "Parameters 'members' and 'data' must have the same length.";
case "Argument_SigIsFinalized": return "Completed signature cannot be modified.";
case "Argument_ArraysInvalid": return "Array or pointer types are not valid.";
case "Argument_GenericsInvalid": return "Generic types are not valid.";
case "Argument_LargeInteger": return "Integer or token was too large to be encoded.";
case "Argument_BadSigFormat": return "Incorrect signature format.";
case "Argument_UnmatchedMethodForLocal": return "Local passed in does not belong to this ILGenerator.";
case "Argument_DuplicateName": return "Tried to add NamedPermissionSet with non-unique name.";
case "Argument_InvalidXMLElement": return "Invalid XML. Missing required tag <{0}> for type '{1}'.";
case "Argument_InvalidXMLMissingAttr": return "Invalid XML. Missing required attribute '{0}'.";
case "Argument_CannotGetTypeTokenForByRef": return "Cannot get TypeToken for a ByRef type.";
case "Argument_NotASimpleNativeType": return "The UnmanagedType passed to DefineUnmanagedMarshal is not a simple type. None of the following values may be used: UnmanagedType.ByValTStr, UnmanagedType.SafeArray, UnmanagedType.ByValArray, UnmanagedType.LPArray, UnmanagedType.CustomMarshaler.";
case "Argument_NotACustomMarshaler": return "Not a custom marshal.";
case "Argument_NoUnmanagedElementCount": return "Unmanaged marshal does not have ElementCount.";
case "Argument_NoNestedMarshal": return "Only LPArray or SafeArray has nested unmanaged marshal.";
case "Argument_InvalidXML": return "Invalid Xml.";
case "Argument_NoUnderlyingCCW": return "The object has no underlying COM data associated with it.";
case "Argument_BadFieldType": return "Bad field type in defining field.";
case "Argument_InvalidXMLBadVersion": return "Invalid Xml - can only parse elements of version one.";
case "Argument_NotAPermissionElement": return "'elem' was not a permission element.";
case "Argument_NPMSInvalidName": return "Name can be neither null nor empty.";
case "Argument_InvalidElementTag": return "Invalid element tag '{0}'.";
case "Argument_InvalidElementText": return "Invalid element text '{0}'.";
case "Argument_InvalidElementName": return "Invalid element name '{0}'.";
case "Argument_InvalidElementValue": return "Invalid element value '{0}'.";
case "Argument_AttributeNamesMustBeUnique": return "Attribute names must be unique.";
#if FEATURE_CAS_POLICY
case "Argument_UninitializedCertificate": return "Uninitialized certificate object.";
case "Argument_MembershipConditionElement": return "Element must be a <IMembershipCondition> element.";
case "Argument_ReservedNPMS": return "Cannot remove or modify reserved permissions set '{0}'.";
case "Argument_NPMSInUse": return "Permission set '{0}' was in use and could not be deleted.";
case "Argument_StrongNameGetPublicKey": return "Unable to obtain public key for StrongNameKeyPair.";
case "Argument_SiteCannotBeNull": return "Site name must be specified.";
case "Argument_BlobCannotBeNull": return "Public key must be specified.";
case "Argument_ZoneCannotBeNull": return "Zone must be specified.";
case "Argument_UrlCannotBeNull": return "URL must be specified.";
case "Argument_NoNPMS": return "Unable to find a permission set with the provided name.";
case "Argument_FailedCodeGroup": return "Failed to create a code group of type '{0}'.";
case "Argument_CodeGroupChildrenMustBeCodeGroups": return "All objects in the input list must have a parent type of 'CodeGroup'.";
#endif // FEATURE_CAS_POLICY
#if FEATURE_IMPERSONATION
case "Argument_InvalidPrivilegeName": return "Privilege '{0}' is not valid on this system.";
case "Argument_TokenZero": return "Token cannot be zero.";
case "Argument_InvalidImpersonationToken": return "Invalid token for impersonation - it cannot be duplicated.";
case "Argument_ImpersonateUser": return "Unable to impersonate user.";
#endif // FEATURE_IMPERSONATION
case "Argument_InvalidHexFormat": return "Improperly formatted hex string.";
case "Argument_InvalidSite": return "Invalid site.";
case "Argument_InterfaceMap": return "'this' type cannot be an interface itself.";
case "Argument_ArrayGetInterfaceMap": return "Interface maps for generic interfaces on arrays cannot be retrived.";
case "Argument_InvalidName": return "Invalid name.";
case "Argument_InvalidDirectoryOnUrl": return "Invalid directory on URL.";
case "Argument_InvalidUrl": return "Invalid URL.";
case "Argument_InvalidKindOfTypeForCA": return "This type cannot be represented as a custom attribute.";
case "Argument_MustSupplyContainer": return "When supplying a FieldInfo for fixing up a nested type, a valid ID for that containing object must also be supplied.";
case "Argument_MustSupplyParent": return "When supplying the ID of a containing object, the FieldInfo that identifies the current field within that object must also be supplied.";
case "Argument_NoClass": return "Element does not specify a class.";
case "Argument_WrongElementType": return "'{0}' element required.";
case "Argument_UnableToGeneratePermissionSet": return "Unable to generate permission set; input XML may be malformed.";
case "Argument_NoEra": return "No Era was supplied.";
case "Argument_AssemblyAlreadyFullTrust": return "Assembly was already fully trusted.";
case "Argument_AssemblyNotFullTrust": return "Assembly was not fully trusted.";
case "Argument_AssemblyWinMD": return "Assembly must not be a Windows Runtime assembly.";
case "Argument_MemberAndArray": return "Cannot supply both a MemberInfo and an Array to indicate the parent of a value type.";
case "Argument_ObjNotComObject": return "The object's type must be __ComObject or derived from __ComObject.";
case "Argument_ObjIsWinRTObject": return "The object's type must not be a Windows Runtime type.";
case "Argument_TypeNotComObject": return "The type must be __ComObject or be derived from __ComObject.";
case "Argument_TypeIsWinRTType": return "The type must not be a Windows Runtime type.";
case "Argument_CantCallSecObjFunc": return "Cannot evaluate a security function.";
case "Argument_StructMustNotBeValueClass": return "The structure must not be a value class.";
case "Argument_NoSpecificCulture": return "Please select a specific culture, such as zh-CN, zh-HK, zh-TW, zh-MO, zh-SG.";
case "Argument_InvalidResourceCultureName": return "The given culture name '{0}' cannot be used to locate a resource file. Resource filenames must consist of only letters, numbers, hyphens or underscores.";
case "Argument_InvalidParamInfo": return "Invalid type for ParameterInfo member in Attribute class.";
case "Argument_EmptyDecString": return "Decimal separator cannot be the empty string.";
case "Argument_OffsetOfFieldNotFound": return "Field passed in is not a marshaled member of the type '{0}'.";
case "Argument_EmptyStrongName": return "StrongName cannot have an empty string for the assembly name.";
case "Argument_NotSerializable": return "Argument passed in is not serializable.";
case "Argument_EmptyApplicationName": return "ApplicationId cannot have an empty string for the name.";
case "Argument_NoDomainManager": return "The domain manager specified by the host could not be instantiated.";
case "Argument_NoMain": return "Main entry point not defined.";
case "Argument_InvalidDateTimeKind": return "Invalid DateTimeKind value.";
case "Argument_ConflictingDateTimeStyles": return "The DateTimeStyles values AssumeLocal and AssumeUniversal cannot be used together.";
case "Argument_ConflictingDateTimeRoundtripStyles": return "The DateTimeStyles value RoundtripKind cannot be used with the values AssumeLocal, AssumeUniversal or AdjustToUniversal.";
case "Argument_InvalidDigitSubstitution": return "The DigitSubstitution property must be of a valid member of the DigitShapes enumeration. Valid entries include Context, NativeNational or None.";
case "Argument_InvalidNativeDigitCount": return "The NativeDigits array must contain exactly ten members.";
case "Argument_InvalidNativeDigitValue": return "Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit.";
case "ArgumentException_InvalidAceBinaryForm": return "The binary form of an ACE object is invalid.";
case "ArgumentException_InvalidAclBinaryForm": return "The binary form of an ACL object is invalid.";
case "ArgumentException_InvalidSDSddlForm": return "The SDDL form of a security descriptor object is invalid.";
case "Argument_InvalidSafeHandle": return "The SafeHandle is invalid.";
case "Argument_CannotPrepareAbstract": return "Abstract methods cannot be prepared.";
case "Argument_ArrayTooLarge": return "The input array length must not exceed Int32.MaxValue / {0}. Otherwise BitArray.Length would exceed Int32.MaxValue.";
case "Argument_RelativeUrlMembershipCondition": return "UrlMembershipCondition requires an absolute URL.";
case "Argument_EmptyWaithandleArray": return "Waithandle array may not be empty.";
case "Argument_InvalidSafeBufferOffLen": return "Offset and length were greater than the size of the SafeBuffer.";
case "Argument_NotEnoughBytesToRead": return "There are not enough bytes remaining in the accessor to read at this position.";
case "Argument_NotEnoughBytesToWrite": return "There are not enough bytes remaining in the accessor to write at this position.";
case "Argument_OffsetAndLengthOutOfBounds": return "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
case "Argument_OffsetAndCapacityOutOfBounds": return "Offset and capacity were greater than the size of the view.";
case "Argument_UnmanagedMemAccessorWrapAround": return "The UnmanagedMemoryAccessor capacity and offset would wrap around the high end of the address space.";
case "Argument_UnrecognizedLoaderOptimization": return "Unrecognized LOADER_OPTIMIZATION property value.  Supported values may include \"SingleDomain\", \"MultiDomain\", \"MultiDomainHost\", and \"NotSpecified\".";
case "ArgumentException_NotAllCustomSortingFuncsDefined": return "Implementations of all the NLS functions must be provided.";
case "ArgumentException_MinSortingVersion": return "The runtime does not support a version of \"{0}\" less than {1}.";

// ;
// ; =====================================================
// ; Reflection Emit resource strings
case "Arugment_EmitMixedContext1": return "Type '{0}' was loaded in the ReflectionOnly context but the AssemblyBuilder was not created as AssemblyBuilderAccess.ReflectionOnly.";
case "Arugment_EmitMixedContext2": return "Type '{0}' was not loaded in the ReflectionOnly context but the AssemblyBuilder was created as AssemblyBuilderAccess.ReflectionOnly.";
case "Argument_BadSizeForData": return "Data size must be > 0 and < 0x3f0000";
case "Argument_InvalidLabel": return "Invalid Label.";
case "Argument_RedefinedLabel": return "Label multiply defined.";
case "Argument_UnclosedExceptionBlock": return "The IL Generator cannot be used while there are unclosed exceptions.";
case "Argument_MissingDefaultConstructor": return "was missing default constructor.";
case "Argument_TooManyFinallyClause": return "Exception blocks may have at most one finally clause.";
case "Argument_NotInTheSameModuleBuilder": return "The argument passed in was not from the same ModuleBuilder.";
case "Argument_BadCurrentLocalVariable": return "Bad current local variable for setting symbol information.";
case "Argument_DuplicateModuleName": return "Duplicate dynamic module name within an assembly.";
case "Argument_DuplicateTypeName": return "Duplicate type name within an assembly.";
case "Argument_InvalidAssemblyName": return "Assembly names may not begin with whitespace or contain the characters '/', or '\\' or ':'.";
case "Argument_InvalidGenericInstantiation": return "The given generic instantiation was invalid.";
case "Argument_DuplicatedFileName": return "Duplicate file names.";
case "Argument_GlobalFunctionHasToBeStatic": return "Global members must be static.";
case "Argument_BadPInvokeOnInterface": return "PInvoke methods cannot exist on interfaces.";
case "Argument_BadPInvokeMethod": return "PInvoke methods must be static and native and cannot be abstract.";
case "Argument_MethodRedefined": return "Method has been already defined.";
case "Argument_BadTypeAttrAbstractNFinal": return "Bad type attributes. A type cannot be both abstract and final.";
case "Argument_BadTypeAttrNestedVisibilityOnNonNestedType": return "Bad type attributes. Nested visibility flag set on a non-nested type.";
case "Argument_BadTypeAttrNonNestedVisibilityNestedType": return "Bad type attributes. Non-nested visibility flag set on a nested type.";
case "Argument_BadTypeAttrInvalidLayout": return "Bad type attributes. Invalid layout attribute specified.";
case "Argument_BadTypeAttrReservedBitsSet": return "Bad type attributes. Reserved bits set on the type.";
case "Argument_BadFieldSig": return "Field signatures do not have return types.";
case "Argument_ShouldOnlySetVisibilityFlags": return "Should only set visibility flags when creating EnumBuilder.";
case "Argument_BadNestedTypeFlags": return "Visibility of interfaces must be one of the following: NestedAssembly, NestedFamANDAssem, NestedFamily, NestedFamORAssem, NestedPrivate or NestedPublic.";
case "Argument_ShouldNotSpecifyExceptionType": return "Should not specify exception type for catch clause for filter block.";
case "Argument_BadLabel": return "Bad label in ILGenerator.";
case "Argument_BadLabelContent": return "Bad label content in ILGenerator.";
case "Argument_EmitWriteLineType": return "EmitWriteLine does not support this field or local type.";
case "Argument_ConstantNull": return "Null is not a valid constant value for this type.";
case "Argument_ConstantDoesntMatch": return "Constant does not match the defined type.";
case "Argument_ConstantNotSupported": return "{0} is not a supported constant type.";
case "Argument_BadConstructor": return "Cannot have private or static constructor.";
case "Argument_BadConstructorCallConv": return "Constructor must have standard calling convention.";
case "Argument_BadPropertyForConstructorBuilder": return "Property must be on the same type of the given ConstructorInfo.";
case "Argument_NotAWritableProperty": return "Not a writable property.";
case "Argument_BadFieldForConstructorBuilder": return "Field must be on the same type of the given ConstructorInfo.";
case "Argument_BadAttributeOnInterfaceMethod": return "Interface method must be abstract and virtual.";
case "ArgumentException_BadMethodImplBody": return "MethodOverride's body must be from this type.";
case "Argument_BadParameterCountsForConstructor": return "Parameter count does not match passed in argument value count.";
case "Argument_BadParameterTypeForConstructor": return "Passed in argument value at index {0} does not match the parameter type.";
case "Argument_BadTypeInCustomAttribute": return "An invalid type was used as a custom attribute constructor argument, field or property.";
case "Argument_DateTimeBadBinaryData": return "The binary data must result in a DateTime with ticks between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";
case "Argument_VerStringTooLong": return "The unmanaged Version information is too large to persist.";
case "Argument_UnknownUnmanagedCallConv": return "Unknown unmanaged calling convention for function signature.";
case "Argument_BadConstantValue": return "Bad default value.";
case "Argument_IllegalName": return "Illegal name.";
case "Argument_cvtres_NotFound": return "Cannot find cvtres.exe";
case "Argument_BadCAForUnmngRSC": return "Bad '{0}' while generating unmanaged resource information.";
case "Argument_MustBeInterfaceMethod": return "The MemberInfo must be an interface method.";
case "Argument_CORDBBadVarArgCallConv": return "Cannot evaluate a VarArgs function.";
case "Argument_CORDBBadMethod": return "Cannot find the method on the object instance.";
case "Argument_InvalidOpCodeOnDynamicMethod": return "Ldtoken, Ldftn and Ldvirtftn OpCodes cannot target DynamicMethods.";
case "Argument_InvalidTypeForDynamicMethod": return "Invalid type owner for DynamicMethod.";
case "Argument_NeedGenericMethodDefinition": return "Method must represent a generic method definition on a generic type definition.";
case "Argument_MethodNeedGenericDeclaringType": return "The specified method cannot be dynamic or global and must be declared on a generic type definition.";
case "Argument_ConstructorNeedGenericDeclaringType": return "The specified constructor must be declared on a generic type definition.";
case "Argument_FieldNeedGenericDeclaringType": return "The specified field must be declared on a generic type definition.";
case "Argument_InvalidMethodDeclaringType": return "The specified method must be declared on the generic type definition of the specified type.";
case "Argument_InvalidConstructorDeclaringType": return "The specified constructor must be declared on the generic type definition of the specified type.";
case "Argument_InvalidFieldDeclaringType": return "The specified field must be declared on the generic type definition of the specified type.";
case "Argument_NeedNonGenericType": return "The specified Type must not be a generic type definition.";
case "Argument_MustBeTypeBuilder": return "'type' must contain a TypeBuilder as a generic argument.";
case "Argument_CannotSetParentToInterface": return "Cannot set parent to an interface.";
case "Argument_MismatchedArrays": return "Two arrays, {0} and {1}, must be of  the same size.";
case "Argument_NeedNonGenericObject": return "The specified object must not be an instance of a generic type.";
case "Argument_NeedStructWithNoRefs": return "The specified Type must be a struct containing no references.";
case "Argument_NotMethodCallOpcode": return "The specified opcode cannot be passed to EmitCall.";

// ; =====================================================
// ;
case "Argument_ModuleAlreadyLoaded": return "The specified module has already been loaded.";
case "Argument_MustHaveLayoutOrBeBlittable": return "The specified structure must be blittable or have layout information.";
case "Argument_NotSimpleFileName": return "The filename must not include a path specification.";
case "Argument_TypeMustBeVisibleFromCom": return "The specified type must be visible from COM.";
case "Argument_TypeMustBeComCreatable": return "The type must be creatable from COM.";
case "Argument_TypeMustNotBeComImport": return "The type must not be imported from COM.";
case "Argument_PolicyFileDoesNotExist": return "The requested policy file does not exist.";
case "Argument_NonNullObjAndCtx": return "Either obj or ctx must be null.";
case "Argument_NoModuleFileExtension": return "Module file name '{0}' must have file extension.";
case "Argument_TypeDoesNotContainMethod": return "Type does not contain the given method.";
case "Argument_StringZeroLength": return "String cannot be of zero length.";
case "Argument_MustBeString": return "String is too long or has invalid contents.";
case "Argument_AbsolutePathRequired": return "Absolute path information is required.";
case "Argument_ManifestFileDoesNotExist": return "The specified manifest file does not exist.";
case "Argument_MustBeRuntimeType": return "Type must be a runtime Type object.";
case "Argument_TypeNotValid": return "The Type object is not valid.";
case "Argument_MustBeRuntimeMethodInfo": return "MethodInfo must be a runtime MethodInfo object.";
case "Argument_MustBeRuntimeFieldInfo": return "FieldInfo must be a runtime FieldInfo object.";
case "Argument_InvalidFieldInfo": return "The FieldInfo object is not valid.";
case "Argument_InvalidConstructorInfo": return "The ConstructorInfo object is not valid.";
case "Argument_MustBeRuntimeAssembly": return "Assembly must be a runtime Assembly object.";
case "Argument_MustBeRuntimeModule": return "Module must be a runtime Module object.";
case "Argument_MustBeRuntimeParameterInfo": return "ParameterInfo must be a runtime ParameterInfo object.";
case "Argument_InvalidParameterInfo": return "The ParameterInfo object is not valid.";
case "Argument_MustBeRuntimeReflectionObject": return "The object must be a runtime Reflection object.";
case "Argument_InvalidMarshalByRefObject": return "The MarshalByRefObject is not valid.";
case "Argument_TypedReferenceInvalidField": return "Field in TypedReferences cannot be static or init only.";
case "Argument_HandleLeak": return "Cannot pass a GCHandle across AppDomains.";
case "Argument_ArgumentZero": return "Argument cannot be zero.";
case "Argument_ImproperType": return "Improper types in collection.";
case "Argument_NotAMembershipCondition": return "The type does not implement IMembershipCondition";
case "Argument_NotAPermissionType": return "The type does not implement IPermission";
case "Argument_NotACodeGroupType": return "The type does not inherit from CodeGroup";
case "Argument_NotATP": return "Type must be a TransparentProxy";
case "Argument_AlreadyACCW": return "The object already has a CCW associated with it.";
case "Argument_OffsetLocalMismatch": return "The UTC Offset of the local dateTime parameter does not match the offset argument.";
case "Argument_OffsetUtcMismatch": return "The UTC Offset for Utc DateTime instances must be 0.";
case "Argument_UTCOutOfRange": return "The UTC time represented when the offset is applied must be between year 0 and 10,000.";
case "Argument_OffsetOutOfRange": return "Offset must be within plus or minus 14 hours.";
case "Argument_OffsetPrecision": return "Offset must be specified in whole minutes.";
case "Argument_FlagNotSupported": return "One or more flags are not supported.";
case "Argument_MustBeFalse": return "Argument must be initialized to false";
case "Argument_ToExclusiveLessThanFromExclusive": return "fromInclusive must be less than or equal to toExclusive.";
case "Argument_FrameworkNameTooShort": return "FrameworkName cannot have less than two components or more than three components.";
case "Argument_FrameworkNameInvalid": return "FrameworkName is invalid.";
case "Argument_FrameworkNameMissingVersion": return "FrameworkName version component is missing.";
#if FEATURE_COMINTEROP
case "Argument_TypeNotActivatableViaWindowsRuntime": return "Type '{0}' does not have an activation factory because it is not activatable by Windows Runtime.";
case "Argument_WinRTSystemRuntimeType": return "Cannot marshal type '{0}' to Windows Runtime. Only 'System.RuntimeType' is supported.";
case "Argument_Unexpected_TypeSource": return "Unexpected TypeKind when marshaling Windows.Foundation.TypeName. ";
#endif // FEATURE_COMINTEROP

// ; ArgumentNullException
case "ArgumentNull_Array": return "Array cannot be null.";
case "ArgumentNull_ArrayValue": return "Found a null value within an array.";
case "ArgumentNull_ArrayElement": return "At least one element in the specified array was null.";
case "ArgumentNull_Assembly": return "Assembly cannot be null.";
case "ArgumentNull_AssemblyName": return "AssemblyName cannot be null.";
case "ArgumentNull_AssemblyNameName": return "AssemblyName.Name cannot be null or an empty string.";
case "ArgumentNull_Buffer": return "Buffer cannot be null.";
case "ArgumentNull_Collection": return "Collection cannot be null.";
case "ArgumentNull_CultureInfo": return "CultureInfo cannot be null.";
case "ArgumentNull_Dictionary": return "Dictionary cannot be null.";
case "ArgumentNull_FileName": return "File name cannot be null.";
case "ArgumentNull_Key": return "Key cannot be null.";
case "ArgumentNull_Graph": return "Object Graph cannot be null.";
case "ArgumentNull_Path": return "Path cannot be null.";
case "ArgumentNull_Stream": return "Stream cannot be null.";
case "ArgumentNull_String": return "String reference not set to an instance of a String.";
case "ArgumentNull_Type": return "Type cannot be null.";
case "ArgumentNull_Obj": return "Object cannot be null.";
case "ArgumentNull_GUID": return "GUID cannot be null.";
case "ArgumentNull_NullMember": return "Member at position {0} was null.";
case "ArgumentNull_Generic": return "Value cannot be null.";
case "ArgumentNull_WithParamName": return "Parameter '{0}' cannot be null.";
case "ArgumentNull_Child": return "Cannot have a null child.";
case "ArgumentNull_SafeHandle": return "SafeHandle cannot be null.";
case "ArgumentNull_CriticalHandle": return "CriticalHandle cannot be null.";
case "ArgumentNull_TypedRefType": return "Type in TypedReference cannot be null.";
case "ArgumentNull_ApplicationTrust": return "The application trust cannot be null.";
case "ArgumentNull_TypeRequiredByResourceScope": return "The type parameter cannot be null when scoping the resource's visibility to Private or Assembly.";
case "ArgumentNull_Waithandles": return "The waitHandles parameter cannot be null.";

// ; ArgumentOutOfRangeException
case "ArgumentOutOfRange_AddressSpace": return "The number of bytes cannot exceed the virtual address space on a 32 bit machine.";
case "ArgumentOutOfRange_ArrayLB": return "Number was less than the array's lower bound in the first dimension.";
case "ArgumentOutOfRange_ArrayLBAndLength": return "Higher indices will exceed Int32.MaxValue because of large lower bound and/or length.";
case "ArgumentOutOfRange_ArrayLength": return "The length of the array must be between {0} and {1}, inclusive.";
case "ArgumentOutOfRange_ArrayLengthMultiple": return "The length of the array must be a multiple of {0}.";
case "ArgumentOutOfRange_ArrayListInsert": return "Insertion index was out of range. Must be non-negative and less than or equal to size.";
case "ArgumentOutOfRange_ArrayTooSmall": return "Destination array is not long enough to copy all the required data. Check array length and offset.";
case "ArgumentOutOfRange_BeepFrequency": return "Console.Beep's frequency must be between {0} and {1}.";
case "ArgumentOutOfRange_BiggerThanCollection": return "Larger than collection size.";
case "ArgumentOutOfRange_Bounds_Lower_Upper": return "Argument must be between {0} and {1}.";
case "ArgumentOutOfRange_Count": return "Count must be positive and count must refer to a location within the string/array/collection.";
case "ArgumentOutOfRange_CalendarRange": return "Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive.";
case "ArgumentOutOfRange_ConsoleBufferBoundaries": return "The value must be greater than or equal to zero and less than the console's buffer size in that dimension.";
case "ArgumentOutOfRange_ConsoleBufferLessThanWindowSize": return "The console buffer size must not be less than the current size and position of the console window, nor greater than or equal to Int16.MaxValue.";
case "ArgumentOutOfRange_ConsoleWindowBufferSize": return "The new console window size would force the console buffer size to be too large.";
case "ArgumentOutOfRange_ConsoleTitleTooLong": return "The console title is too long.";
case "ArgumentOutOfRange_ConsoleWindowPos": return "The window position must be set such that the current window size fits within the console's buffer, and the numbers must not be negative.";
case "ArgumentOutOfRange_ConsoleWindowSize_Size": return "The value must be less than the console's current maximum window size of {0} in that dimension. Note that this value depends on screen resolution and the console font.";
case "ArgumentOutOfRange_ConsoleKey": return "Console key values must be between 0 and 255.";
case "ArgumentOutOfRange_CursorSize": return "The cursor size is invalid. It must be a percentage between 1 and 100.";
case "ArgumentOutOfRange_BadYearMonthDay": return "Year, Month, and Day parameters describe an un-representable DateTime.";
case "ArgumentOutOfRange_BadHourMinuteSecond": return "Hour, Minute, and Second parameters describe an un-representable DateTime.";
case "ArgumentOutOfRange_DateArithmetic": return "The added or subtracted value results in an un-representable DateTime.";
case "ArgumentOutOfRange_DateTimeBadMonths": return "Months value must be between +/-120000.";
case "ArgumentOutOfRange_DateTimeBadYears": return "Years value must be between +/-10000.";
case "ArgumentOutOfRange_DateTimeBadTicks": return "Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";
case "ArgumentOutOfRange_Day": return "Day must be between 1 and {0} for month {1}.";
case "ArgumentOutOfRange_DecimalRound": return "Decimal can only round to between 0 and 28 digits of precision.";
case "ArgumentOutOfRange_DecimalScale": return "Decimal's scale value must be between 0 and 28, inclusive.";
case "ArgumentOutOfRange_Era": return "Time value was out of era range.";
case "ArgumentOutOfRange_Enum": return "Enum value was out of legal range.";
case "ArgumentOutOfRange_FileLengthTooBig": return "Specified file length was too large for the file system.";
case "ArgumentOutOfRange_FileTimeInvalid": return "Not a valid Win32 FileTime.";
case "ArgumentOutOfRange_GetByteCountOverflow": return "Too many characters. The resulting number of bytes is larger than what can be returned as an int.";
case "ArgumentOutOfRange_GetCharCountOverflow": return "Too many bytes. The resulting number of chars is larger than what can be returned as an int.";
case "ArgumentOutOfRange_HashtableLoadFactor": return "Load factor needs to be between 0.1 and 1.0.";
case "ArgumentOutOfRange_HugeArrayNotSupported": return "Arrays larger than 2GB are not supported.";
case "ArgumentOutOfRange_InvalidHighSurrogate": return "A valid high surrogate character is between 0xd800 and 0xdbff, inclusive.";
case "ArgumentOutOfRange_InvalidLowSurrogate": return "A valid low surrogate character is between 0xdc00 and 0xdfff, inclusive.";
case "ArgumentOutOfRange_InvalidEraValue": return "Era value was not valid.";
case "ArgumentOutOfRange_InvalidUserDefinedAceType": return "User-defined ACEs must not have a well-known ACE type.";
case "ArgumentOutOfRange_InvalidUTF32": return "A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).";
case "ArgumentOutOfRange_Index": return "Index was out of range. Must be non-negative and less than the size of the collection.";
case "ArgumentOutOfRange_IndexString": return "Index was out of range. Must be non-negative and less than the length of the string.";
case "ArgumentOutOfRange_StreamLength": return "Stream length must be non-negative and less than 2^31 - 1 - origin.";
case "ArgumentOutOfRange_LessEqualToIntegerMaxVal": return "Argument must be less than or equal to 2^31 - 1 milliseconds.";
case "ArgumentOutOfRange_Month": return "Month must be between one and twelve.";
case "ArgumentOutOfRange_MustBeNonNegInt32": return "Value must be non-negative and less than or equal to Int32.MaxValue.";
case "ArgumentOutOfRange_NeedNonNegNum": return "Non-negative number required.";
case "ArgumentOutOfRange_NeedNonNegOrNegative1": return "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.";
case "ArgumentOutOfRange_NeedPosNum": return "Positive number required.";
case "ArgumentOutOfRange_NegativeCapacity": return "Capacity must be positive.";
case "ArgumentOutOfRange_NegativeCount": return "Count cannot be less than zero.";
case "ArgumentOutOfRange_NegativeLength": return "Length cannot be less than zero.";
case "ArgumentOutOfRange_NegFileSize": return "Length must be non-negative.";
case "ArgumentOutOfRange_ObjectID": return "objectID cannot be less than or equal to zero.";
case "ArgumentOutOfRange_SmallCapacity": return "capacity was less than the current size.";
case "ArgumentOutOfRange_QueueGrowFactor": return "Queue grow factor must be between {0} and {1}.";
case "ArgumentOutOfRange_RoundingDigits": return "Rounding digits must be between 0 and 15, inclusive.";
case "ArgumentOutOfRange_StartIndex": return "StartIndex cannot be less than zero.";
case "ArgumentOutOfRange_MustBePositive": return "'{0}' must be greater than zero.";
case "ArgumentOutOfRange_MustBeNonNegNum": return "'{0}' must be non-negative.";
case "ArgumentOutOfRange_LengthGreaterThanCapacity": return "The length cannot be greater than the capacity.";
case "ArgumentOutOfRange_ListInsert": return "Index must be within the bounds of the List.";
case "ArgumentOutOfRange_StartIndexLessThanLength": return "startIndex must be less than length of string.";
case "ArgumentOutOfRange_StartIndexLargerThanLength": return "startIndex cannot be larger than length of string.";
case "ArgumentOutOfRange_EndIndexStartIndex": return "endIndex cannot be greater than startIndex.";
case "ArgumentOutOfRange_IndexCount": return "Index and count must refer to a location within the string.";
case "ArgumentOutOfRange_IndexCountBuffer": return "Index and count must refer to a location within the buffer.";
case "ArgumentOutOfRange_IndexLength": return "Index and length must refer to a location within the string.";
case "ArgumentOutOfRange_InvalidThreshold": return "The specified threshold for creating dictionary is out of range.";
case "ArgumentOutOfRange_Capacity": return "Capacity exceeds maximum capacity.";
case "ArgumentOutOfRange_Length": return "The specified length exceeds maximum capacity of SecureString.";
case "ArgumentOutOfRange_LengthTooLarge": return "The specified length exceeds the maximum value of {0}.";
case "ArgumentOutOfRange_SmallMaxCapacity": return "MaxCapacity must be one or greater.";
case "ArgumentOutOfRange_GenericPositive": return "Value must be positive.";
case "ArgumentOutOfRange_Range": return "Valid values are between {0} and {1}, inclusive.";
case "ArgumentOutOfRange_AddValue": return "Value to add was out of range.";
case "ArgumentOutOfRange_OffsetLength": return "Offset and length must refer to a position in the string.";
case "ArgumentOutOfRange_OffsetOut": return "Either offset did not refer to a position in the string, or there is an insufficient length of destination character array.";
case "ArgumentOutOfRange_PartialWCHAR": return "Pointer startIndex and length do not refer to a valid string.";
case "ArgumentOutOfRange_ParamSequence": return "The specified parameter index is not in range.";
case "ArgumentOutOfRange_Version": return "Version's parameters must be greater than or equal to zero.";
case "ArgumentOutOfRange_TimeoutTooLarge": return "Time-out interval must be less than 2^32-2.";
case "ArgumentOutOfRange_UIntPtrMax-1": return "The length of the buffer must be less than the maximum UIntPtr value for your platform.";
case "ArgumentOutOfRange_UnmanagedMemStreamLength": return "UnmanagedMemoryStream length must be non-negative and less than 2^63 - 1 - baseAddress.";
case "ArgumentOutOfRange_UnmanagedMemStreamWrapAround": return "The UnmanagedMemoryStream capacity would wrap around the high end of the address space.";
case "ArgumentOutOfRange_PeriodTooLarge": return "Period must be less than 2^32-2.";
case "ArgumentOutOfRange_Year": return "Year must be between 1 and 9999.";
case "ArgumentOutOfRange_BinaryReaderFillBuffer": return "The number of bytes requested does not fit into BinaryReader's internal buffer.";
case "ArgumentOutOfRange_PositionLessThanCapacityRequired": return "The position may not be greater or equal to the capacity of the accessor.";

// ; ArithmeticException
case "Arithmetic_NaN": return "Function does not accept floating point Not-a-Number values.";

// ; ArrayTypeMismatchException
case "ArrayTypeMismatch_CantAssignType": return "Source array type cannot be assigned to destination array type.";
case "ArrayTypeMismatch_ConstrainedCopy": return "Array.ConstrainedCopy will only work on array types that are provably compatible, without any form of boxing, unboxing, widening, or casting of each array element.  Change the array types (i.e., copy a Derived[] to a Base[]), or use a mitigation strategy in the CER for Array.Copy's less powerful reliability contract, such as cloning the array or throwing away the potentially corrupt destination array.";

// ; BadImageFormatException
case "BadImageFormat_ResType&SerBlobMismatch": return "The type serialized in the .resources file was not the same type that the .resources file said it contained. Expected '{0}' but read '{1}'.";
case "BadImageFormat_ResourcesIndexTooLong": return "Corrupt .resources file. String for name index '{0}' extends past the end of the file.";
case "BadImageFormat_ResourcesNameTooLong": return "Corrupt .resources file. Resource name extends past the end of the file.";
case "BadImageFormat_ResourcesNameInvalidOffset": return "Corrupt .resources file. Invalid offset '{0}' into name section.";
case "BadImageFormat_ResourcesHeaderCorrupted": return "Corrupt .resources file. Unable to read resources from this file because of invalid header information. Try regenerating the .resources file.";
case "BadImageFormat_ResourceNameCorrupted": return "Corrupt .resources file. A resource name extends past the end of the stream.";
case "BadImageFormat_ResourceNameCorrupted_NameIndex": return "Corrupt .resources file. The resource name for name index {0} extends past the end of the stream.";
case "BadImageFormat_ResourceDataLengthInvalid": return "Corrupt .resources file.  The specified data length '{0}' is not a valid position in the stream.";
case "BadImageFormat_TypeMismatch": return "Corrupt .resources file.  The specified type doesn't match the available data in the stream.";
case "BadImageFormat_InvalidType": return "Corrupt .resources file.  The specified type doesn't exist.";
case "BadImageFormat_ResourcesIndexInvalid": return "Corrupt .resources file. The resource index '{0}' is outside the valid range.";
case "BadImageFormat_StreamPositionInvalid": return "Corrupt .resources file.  The specified position '{0}' is not a valid position in the stream.";
case "BadImageFormat_ResourcesDataInvalidOffset": return "Corrupt .resources file. Invalid offset '{0}' into data section.";
case "BadImageFormat_NegativeStringLength": return "Corrupt .resources file. String length must be non-negative.";
case "BadImageFormat_ParameterSignatureMismatch": return "The parameters and the signature of the method don't match.";

// ; Cryptography
// ; These strings still appear in bcl.small but should go away eventually
case "Cryptography_CSSM_Error": return "Error 0x{0} from the operating system security framework: '{1}'.";
case "Cryptography_CSSM_Error_Unknown": return "Error 0x{0} from the operating system security framework.";
case "Cryptography_InvalidDSASignatureSize": return "Length of the DSA signature was not 40 bytes.";
case "Cryptography_InvalidHandle": return "{0} is an invalid handle.";
case "Cryptography_InvalidOID": return "Object identifier (OID) is unknown.";
case "Cryptography_OAEPDecoding": return "Error occurred while decoding OAEP padding.";
case "Cryptography_PasswordDerivedBytes_InvalidIV": return "The Initialization vector should have the same length as the algorithm block size in bytes.";
case "Cryptography_SSE_InvalidDataSize": return "Length of the data to encrypt is invalid.";
case "Cryptography_X509_ExportFailed": return "The certificate export operation failed.";
case "Cryptography_X509_InvalidContentType": return "Invalid content type.";
case "Cryptography_CryptoStream_FlushFinalBlockTwice": return "FlushFinalBlock() method was called twice on a CryptoStream. It can only be called once.";
case "Cryptography_HashKeySet": return "Hash key cannot be changed after the first write to the stream.";
case "Cryptography_HashNotYetFinalized": return "Hash must be finalized before the hash value is retrieved.";
case "Cryptography_InsufficientBuffer": return "Input buffer contains insufficient data.";
case "Cryptography_InvalidBlockSize": return "Specified block size is not valid for this algorithm.";
case "Cryptography_InvalidCipherMode": return "Specified cipher mode is not valid for this algorithm.";
case "Cryptography_InvalidIVSize": return "Specified initialization vector (IV) does not match the block size for this algorithm.";
case "Cryptography_InvalidKeySize": return "Specified key is not a valid size for this algorithm.";
case "Cryptography_PasswordDerivedBytes_FewBytesSalt": return "Salt is not at least eight bytes.";
case "Cryptography_PKCS7_InvalidPadding": return "Padding is invalid and cannot be removed.";
case "Cryptography_UnknownHashAlgorithm": return "'{0}' is not a known hash algorithm.";
case "Cryptography_LegacyNetCF_UnknownError": return "Unknown Error '{0}'.";
case "Cryptography_LegacyNetCF_CSP_CouldNotAcquire": return "CryptoAPI cryptographic service provider (CSP) for this implementation could not be acquired.";

#if FEATURE_CRYPTO
case "Cryptography_Config_EncodedOIDError": return "Encoded OID length is too large (greater than 0x7f bytes).";
case "Cryptography_CSP_AlgKeySizeNotAvailable": return "Algorithm implementation does not support a key size of {0}.";
case "Cryptography_CSP_AlgorithmNotAvailable": return "Cryptographic service provider (CSP) could not be found for this algorithm.";
case "Cryptography_CSP_CFBSizeNotSupported": return "Feedback size for the cipher feedback mode (CFB) must be 8 bits.";
case "Cryptography_CSP_NotFound": return "The requested key container was not found.";
case "Cryptography_CSP_NoPrivateKey": return "Object contains only the public half of a key pair. A private key must also be provided.";
case "Cryptography_CSP_OFBNotSupported": return "Output feedback mode (OFB) is not supported by this implementation.";
case "Cryptography_CSP_WrongKeySpec": return "The specified cryptographic service provider (CSP) does not support this key algorithm.";
case "Cryptography_HashNameSet": return "Hash name cannot be changed after the first write to the stream.";
case "Cryptography_InvalidHashSize": return "{0} algorithm hash size is {1} bytes.";
case "Cryptography_InvalidKey_Weak": return "Specified key is a known weak key for '{0}' and cannot be used.";
case "Cryptography_InvalidKey_SemiWeak": return "Specified key is a known semi-weak key for '{0}' and cannot be used.";
case "Cryptography_InvalidKeyParameter": return "Parameter '{0}' is not a valid key parameter.";
case "Cryptography_InvalidFeedbackSize": return "Specified feedback size is invalid.";
case "Cryptography_InvalidOperation": return "This operation is not supported for this class.";
case "Cryptography_InvalidPaddingMode": return "Specified padding mode is not valid for this algorithm.";
case "Cryptography_InvalidFromXmlString": return "Input string does not contain a valid encoding of the '{0}' '{1}' parameter.";
case "Cryptography_MissingKey": return "No asymmetric key object has been associated with this formatter object.";
case "Cryptography_MissingOID": return "Required object identifier (OID) cannot be found.";
case "Cryptography_NotInteractive": return "The current session is not interactive.";
case "Cryptography_NonCompliantFIPSAlgorithm": return "This implementation is not part of the Windows Platform FIPS validated cryptographic algorithms.";
case "Cryptography_Padding_Win2KEnhOnly": return "Direct Encryption and decryption using RSA are not available on this platform.";
case "Cryptography_Padding_EncDataTooBig": return "The data to be encrypted exceeds the maximum for this modulus of {0} bytes.";
case "Cryptography_Padding_DecDataTooBig": return "The data to be decrypted exceeds the maximum for this modulus of {0} bytes.";
case "Cryptography_PasswordDerivedBytes_ValuesFixed": return "Value of '{0}' cannot be changed after the bytes have been retrieved.";
case "Cryptography_PasswordDerivedBytes_TooManyBytes": return "Requested number of bytes exceeds the maximum.";
case "Cryptography_PasswordDerivedBytes_InvalidAlgorithm": return "Algorithm is unavailable or is not supported for this operation.";
case "Cryptography_PKCS1Decoding": return "Error occurred while decoding PKCS1 padding.";
case "Cryptography_RC2_EKSKS": return "EffectiveKeySize value must be at least as large as the KeySize value.";
case "Cryptography_RC2_EKSKS2": return "EffectiveKeySize must be the same as KeySize in this implementation.";
case "Cryptography_RC2_EKS40": return "EffectiveKeySize value must be at least 40 bits.";
case "Cryptography_SSD_InvalidDataSize": return "Length of the data to decrypt is invalid.";
case "Cryptography_AddNullOrEmptyName": return "CryptoConfig cannot add a mapping for a null or empty name.";
case "Cryptography_AlgorithmTypesMustBeVisible": return "Algorithms added to CryptoConfig must be accessable from outside their assembly.";
#endif  // FEATURE_CRYPTO

// ; EventSource
case "EventSource_ToString": return "EventSource({0}, {1})";
case "EventSource_EventSourceGuidInUse": return "An instance of EventSource with Guid {0} already exists.";
case "EventSource_KeywordNeedPowerOfTwo": return "Value {0} for keyword {1} needs to be a power of 2.";
case "EventSource_UndefinedKeyword": return "Use of undefined keyword value {0} for event {1}.";
case "EventSource_UnsupportedEventTypeInManifest": return "Unsupported type {0} in event source.";
case "EventSource_ListenerNotFound": return "Listener not found.";
case "EventSource_ListenerCreatedInsideCallback": return "Creating an EventListener inside a EventListener callback.";
case "EventSource_AttributeOnNonVoid": return "Event attribute placed on method {0} which does not return 'void'.";
case "EventSource_NeedPositiveId": return "Event IDs must be positive integers.";
case "EventSource_ReservedOpcode": return "Opcode values less than 11 are reserved for system use.";
case "EventSource_ReservedKeywords": return "Keywords values larger than 0x0000100000000000 are reserved for system use";
case "EventSource_PayloadTooBig": return "The payload for a single event is too large.";
case "EventSource_NoFreeBuffers": return "No Free Buffers available from the operating system (e.g. event rate too fast).";
case "EventSource_NullInput": return "Null passed as a event argument.";
case "EventSource_TooManyArgs": return "Too many arguments.";
case "EventSource_SessionIdError": return "Bit position in AllKeywords ({0}) must equal the command argument named \"EtwSessionKeyword\" ({1}).";
case "EventSource_EnumKindMismatch": return "The type of {0} is not expected in {1}.";
case "EventSource_MismatchIdToWriteEvent": return "Event {0} is givien event ID {1} but {2} was passed to WriteEvent.";
case "EventSource_EventIdReused": return "Event {0} has ID {1} which is already in use.";
case "EventSource_EventNameReused": return "Event name {0} used more than once.  If you wish to overload a method, the overloaded method should have a NonEvent attribute.";
case "EventSource_UndefinedChannel": return "Use of undefined channel value {0} for event {1}.";
case "EventSource_UndefinedOpcode": return "Use of undefined opcode value {0} for event {1}.";
case "ArgumentOutOfRange_MaxArgExceeded": return "The total number of parameters must not exceed {0}.";
case "ArgumentOutOfRange_MaxStringsExceeded": return "The number of String parameters must not exceed {0}.";
case "ArgumentOutOfRange_NeedValidId": return "The ID parameter must be in the range {0} through {1}.";
case "EventSource_NeedGuid": return "The Guid of an EventSource must be non zero.";
case "EventSource_NeedName": return "The name of an EventSource must not be null.";
case "EventSource_EtwAlreadyRegistered": return "The provider has already been registered with the operating system.";
case "EventSource_ListenerWriteFailure": return "An error occurred when writing to a listener.";
case "EventSource_TypeMustDeriveFromEventSource": return "Event source types must derive from EventSource.";
case "EventSource_TypeMustBeSealedOrAbstract": return "Event source types must be sealed or abstract.";
case "EventSource_TaskOpcodePairReused": return "Event {0} (with ID {1}) has the same task/opcode pair as event {2} (with ID {3}).";
case "EventSource_EventMustHaveTaskIfNonDefaultOpcode": return "Event {0} (with ID {1}) has a non-default opcode but not a task.";
case "EventSource_EventNameDoesNotEqualTaskPlusOpcode": return "Event {0} (with ID {1}) has a name that is not the concatenation of its task name and opcode.";
case "EventSource_PeriodIllegalInProviderName": return "Period character ('.') is illegal in an ETW provider name ({0}).";
case "EventSource_IllegalOpcodeValue": return "Opcode {0} has a value of {1} which is outside the legal range (11-238).";
case "EventSource_OpcodeCollision": return "Opcodes {0} and {1} are defined with the same value ({2}).";
case "EventSource_IllegalTaskValue": return "Task {0} has a value of {1} which is outside the legal range (1-65535).";
case "EventSource_TaskCollision": return "Tasks {0} and {1} are defined with the same value ({2}).";
case "EventSource_IllegalKeywordsValue": return "Keyword {0} has a value of {1} which is outside the legal range (0-0x0000080000000000).";
case "EventSource_KeywordCollision": return "Keywords {0} and {1} are defined with the same value ({2}).";
case "EventSource_EventChannelOutOfRange": return "Channel {0} has a value of [1} which is outside the legal range (16-254).";
case "EventSource_ChannelTypeDoesNotMatchEventChannelValue": return "Channel {0} does not match event channel value {1}.";
case "EventSource_MaxChannelExceeded": return "Attempt to define more than the maximum limit of 8 channels for a provider.";
case "EventSource_DuplicateStringKey": return "Multiple definitions for string \"{0}\".";
case "EventSource_EventWithAdminChannelMustHaveMessage": return "Event {0} specifies an Admin channel {1}. It must specify a Message property.";
case "EventSource_UnsupportedMessageProperty": return "Event {0} specifies an illegal or unsupported formatting message (\"{1}\").";
case "EventSource_AbstractMustNotDeclareKTOC": return "Abstract event source must not declare {0} nested type.";
case "EventSource_AbstractMustNotDeclareEventMethods": return "Abstract event source must not declare event methods ({0} with ID {1}).";
case "EventSource_EventMustNotBeExplicitImplementation": return "Event method {0} (with ID {1}) is an explicit interface method implementation. Re-write method as implicit implementation.";
case "EventSource_EventParametersMismatch": return "Event {0} was called with {1} argument(s) , but it is defined with {2} paramenter(s).";
case "EventSource_InvalidCommand": return "Invalid command value.";
case "EventSource_InvalidEventFormat": return "Can't specify both etw event format flags.";
case "EventSource_AddScalarOutOfRange": return "Getting out of bounds during scalar addition. ";
case "EventSource_PinArrayOutOfRange": return " Pins are out of range.";
case "EventSource_DataDescriptorsOutOfRange": return "Data descriptors are out of range.";
case "EventSource_NotSupportedArrayOfNil": return "Arrays of Nil are not supported.";
case "EventSource_NotSupportedArrayOfBinary": return "Arrays of Binary are not supported.";
case "EventSource_NotSupportedArrayOfNullTerminatedString": return "Arrays of null-terminated string are not supported.";
case "EventSource_TooManyFields": return "Too many fields in structure.";
case "EventSource_RecursiveTypeDefinition": return "Recursive type definition is not supported.";
case "EventSource_NotSupportedEnumType": return "Enum type {0} underlying type {1} is not supported for serialization.";
case "EventSource_NonCompliantTypeError": return "The API supports only anonymous types or types decorated with the EventDataAttribute. Non-compliant type: {0} dataType.";
case "EventSource_NotSupportedNestedArraysEnums": return "Nested arrays/enumerables are not supported.";
case "EventSource_IncorrentlyAuthoredTypeInfo": return "Incorrectly-authored TypeInfo - a type should be serialized as one field or as one group";
case "EventSource_NotSupportedCustomSerializedData": return "Enumerables of custom-serialized data are not supported";
case "EventSource_StopsFollowStarts": return "An event with stop suffix must follow a corresponding event with a start suffix.";

// ; ExecutionEngineException
case "ExecutionEngine_InvalidAttribute": return "Attribute cannot have multiple definitions.";
case "ExecutionEngine_MissingSecurityDescriptor": return "Unable to retrieve security descriptor for this frame.";

// ;;ExecutionContext
case "ExecutionContext_UndoFailed": return "Undo operation on a component context threw an exception";
case "ExecutionContext_ExceptionInAsyncLocalNotification": return "An exception was not handled in an AsyncLocal<T> notification callback.";


// ; FieldAccessException
case "FieldAccess_InitOnly": return "InitOnly (aka ReadOnly) fields can only be initialized in the type/instance constructor.";

// ; FormatException
case "Format_AttributeUsage": return "Duplicate AttributeUsageAttribute found on attribute type {0}.";
case "Format_Bad7BitInt32": return "Too many bytes in what should have been a 7 bit encoded Int32.";
case "Format_BadBase": return "Invalid digits for the specified base.";
case "Format_BadBase64Char": return "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. ";
case "Format_BadBase64CharArrayLength": return "Invalid length for a Base-64 char array or string.";
case "Format_BadBoolean": return "String was not recognized as a valid Boolean.";
case "Format_BadDateTime": return "String was not recognized as a valid DateTime.";
case "Format_BadDateTimeCalendar": return "The DateTime represented by the string is not supported in calendar {0}.";
case "Format_BadDayOfWeek": return "String was not recognized as a valid DateTime because the day of week was incorrect.";
case "Format_DateOutOfRange": return "The DateTime represented by the string is out of range.";
case "Format_BadDatePattern": return "Could not determine the order of year, month, and date from '{0}'.";
case "Format_BadFormatSpecifier": return "Format specifier was invalid.";
case "Format_BadTimeSpan": return "String was not recognized as a valid TimeSpan.";
case "Format_BadQuote": return "Cannot find a matching quote character for the character '{0}'.";
case "Format_EmptyInputString": return "Input string was either empty or contained only whitespace.";
case "Format_ExtraJunkAtEnd": return "Additional non-parsable characters are at the end of the string.";
case "Format_GuidBrace": return "Expected {0xdddddddd, etc}.";
case "Format_GuidComma": return "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).";
case "Format_GuidBraceAfterLastNumber": return "Could not find a brace, or the length between the previous token and the brace was zero (i.e., '0x,'etc.).";
case "Format_GuidDashes": return "Dashes are in the wrong position for GUID parsing.";
case "Format_GuidEndBrace": return "Could not find the ending brace.";
case "Format_GuidHexPrefix": return "Expected hex 0x in '{0}'.";
case "Format_GuidInvLen": return "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";
case "Format_GuidInvalidChar": return "Guid string should only contain hexadecimal characters.";
case "Format_GuidUnrecognized": return "Unrecognized Guid format.";
case "Format_InvalidEnumFormatSpecification": return "Format String can be only \"G\", \"g\", \"X\", \"x\", \"F\", \"f\", \"D\" or \"d\".";
case "Format_InvalidGuidFormatSpecification": return "Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\".";
case "Format_InvalidString": return "Input string was not in a correct format.";
case "Format_IndexOutOfRange": return "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";
case "Format_UnknowDateTimeWord": return "The string was not recognized as a valid DateTime. There is an unknown word starting at index {0}.";
case "Format_NeedSingleChar": return "String must be exactly one character long.";
case "Format_NoParsibleDigits": return "Could not find any recognizable digits.";
case "Format_RepeatDateTimePattern": return "DateTime pattern '{0}' appears more than once with different values.";
case "Format_StringZeroLength": return "String cannot have zero length.";
case "Format_TwoTimeZoneSpecifiers": return "The String being parsed cannot contain two TimeZone specifiers.";
case "Format_UTCOutOfRange": return "The UTC representation of the date falls outside the year range 1-9999.";
case "Format_OffsetOutOfRange": return "The time zone offset must be within plus or minus 14 hours.";
case "Format_MissingIncompleteDate": return "There must be at least a partial date with a year present in the input.";

// ; IndexOutOfRangeException
case "IndexOutOfRange_ArrayRankIndex": return "Array does not have that many dimensions.";
case "IndexOutOfRange_IORaceCondition": return "Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods. This also applies to classes like StreamWriter and StreamReader.";
case "IndexOutOfRange_UMSPosition": return "Unmanaged memory stream position was beyond the capacity of the stream.";

// ; InsufficientMemoryException
case "InsufficientMemory_MemFailPoint": return "Insufficient available memory to meet the expected demands of an operation at this time.  Please try again later.";
case "InsufficientMemory_MemFailPoint_TooBig": return "Insufficient memory to meet the expected demands of an operation, and this system is likely to never satisfy this request.  If this is a 32 bit system, consider booting in 3 GB mode.";
case "InsufficientMemory_MemFailPoint_VAFrag": return "Insufficient available memory to meet the expected demands of an operation at this time, possibly due to virtual address space fragmentation.  Please try again later.";


// ; InvalidCastException
case "InvalidCast_DBNull": return "Object cannot be cast to DBNull.";
case "InvalidCast_DownCastArrayElement": return "At least one element in the source array could not be cast down to the destination array type.";
case "InvalidCast_Empty": return "Object cannot be cast to Empty.";
case "InvalidCast_FromDBNull": return "Object cannot be cast from DBNull to other types.";
case "InvalidCast_FromTo": return "Invalid cast from '{0}' to '{1}'.";
case "InvalidCast_IConvertible": return "Object must implement IConvertible.";
case "InvalidCast_OATypeMismatch": return "OleAut reported a type mismatch.";
case "InvalidCast_StoreArrayElement": return "Object cannot be stored in an array of this type.";
case "InvalidCast_CannotCoerceByRefVariant": return "Object cannot be coerced to the original type of the ByRef VARIANT it was obtained from.";
case "InvalidCast_CannotCastNullToValueType": return "Null object cannot be converted to a value type.";
#if FEATURE_COMINTEROP
case "InvalidCast_WinRTIPropertyValueElement": return "Object in an IPropertyValue is of type '{0}', which cannot be converted to a '{1}'.";
case "InvalidCast_WinRTIPropertyValueCoersion": return "Object in an IPropertyValue is of type '{0}' with value '{1}', which cannot be converted to a '{2}'.";
case "InvalidCast_WinRTIPropertyValueArrayCoersion": return "Object in an IPropertyValue is of type '{0}' which cannot be convereted to a '{1}' due to array element '{2}': {3}.";
#endif // FEATURE_COMINTEROP

// ; InvalidOperationException
case "InvalidOperation_ActivationArgsAppTrustMismatch": return "The activation arguments and application trust for the AppDomain must correspond to the same application identity.";
case "InvalidOperation_AddContextFrozen": return "Attempted to add properties to a frozen context.";
case "InvalidOperation_AppDomainSandboxAPINeedsExplicitAppBase": return "This API requires the ApplicationBase to be specified explicitly in the AppDomainSetup parameter.";
case "InvalidOperation_CantCancelCtrlBreak": return "Applications may not prevent control-break from terminating their process.";
case "InvalidOperation_CalledTwice": return "The method cannot be called twice on the same instance.";
case "InvalidOperation_CollectionCorrupted": return "A prior operation on this collection was interrupted by an exception. Collection's state is no longer trusted.";
case "InvalidOperation_CriticalTransparentAreMutuallyExclusive": return "SecurityTransparent and SecurityCritical attributes cannot be applied to the assembly scope at the same time.";
case "InvalidOperation_SubclassedObject": return "Cannot set sub-classed {0} object to {1} object.";
case "InvalidOperation_ExceptionStateCrossAppDomain": return "Thread.ExceptionState cannot access an ExceptionState from a different AppDomain.";
case "InvalidOperation_DebuggerLaunchFailed": return "Debugger unable to launch.";
case "InvalidOperation_ApartmentStateSwitchFailed": return "Failed to set the specified COM apartment state.";
case "InvalidOperation_EmptyQueue": return "Queue empty.";
case "InvalidOperation_EmptyStack": return "Stack empty.";
case "InvalidOperation_CannotRemoveFromStackOrQueue": return "Removal is an invalid operation for Stack or Queue.";
case "InvalidOperation_EnumEnded": return "Enumeration already finished.";
case "InvalidOperation_EnumFailedVersion": return "Collection was modified; enumeration operation may not execute.";
case "InvalidOperation_EnumNotStarted": return "Enumeration has not started. Call MoveNext.";
case "InvalidOperation_EnumOpCantHappen": return "Enumeration has either not started or has already finished.";
case "InvalidOperation_ModifyRONumFmtInfo": return "Unable to modify a read-only NumberFormatInfo object.";
#if FEATURE_CAS_POLICY
case "InvalidOperation_ModifyROPermSet": return "ReadOnlyPermissionSet objects may not be modified.";
#endif // FEATURE_CAS_POLICY
case "InvalidOperation_MustBeSameThread": return "This operation must take place on the same thread on which the object was created.";
case "InvalidOperation_MustRevertPrivilege": return "Must revert the privilege prior to attempting this operation.";
case "InvalidOperation_ReadOnly": return "Instance is read-only.";
case "InvalidOperation_RegRemoveSubKey": return "Registry key has subkeys and recursive removes are not supported by this method.";
case "InvalidOperation_IComparerFailed": return "Failed to compare two elements in the array.";
case "InvalidOperation_InternalState": return "Invalid internal state.";
case "InvalidOperation_DuplicatePropertyName": return "Another property by this name already exists.";
case "InvalidOperation_NotCurrentDomain": return "You can only define a dynamic assembly on the current AppDomain.";
case "InvalidOperation_ContextAlreadyFrozen": return "Context is already frozen.";
case "InvalidOperation_WriteOnce": return "This property has already been set and cannot be modified.";
case "InvalidOperation_MethodBaked": return "Type definition of the method is complete.";
case "InvalidOperation_MethodHasBody": return "Method already has a body.";
case "InvalidOperation_ModificationOfNonCanonicalAcl": return "This access control list is not in canonical form and therefore cannot be modified.";
case "InvalidOperation_Method": return "This method is not supported by the current object.";
case "InvalidOperation_NotADebugModule": return "Not a debug ModuleBuilder.";
case "InvalidOperation_NoMultiModuleAssembly": return "You cannot have more than one dynamic module in each dynamic assembly in this version of the runtime.";
case "InvalidOperation_OpenLocalVariableScope": return "Local variable scope was not properly closed.";
case "InvalidOperation_SetVolumeLabelFailed": return "Volume labels can only be set for writable local volumes.";
case "InvalidOperation_SetData": return "An additional permission should not be supplied for setting loader information.";
case "InvalidOperation_SetData_OnlyOnce": return "SetData can only be used to set the value of a given name once.";
case "InvalidOperation_SetData_OnlyLocationURI": return "SetData cannot be used to set the value for '{0}'.";
case "InvalidOperation_TypeHasBeenCreated": return "Unable to change after type has been created.";
case "InvalidOperation_TypeNotCreated": return "Type has not been created.";
case "InvalidOperation_NoUnderlyingTypeOnEnum": return "Underlying type information on enumeration is not specified.";
case "InvalidOperation_ResMgrBadResSet_Type": return "'{0}': ResourceSet derived classes must provide a constructor that takes a String file name and a constructor that takes a Stream.";
case "InvalidOperation_AssemblyHasBeenSaved": return "Assembly '{0}' has been saved.";
case "InvalidOperation_ModuleHasBeenSaved": return "Module '{0}' has been saved.";
case "InvalidOperation_CannotAlterAssembly": return "Unable to alter assembly information.";
case "InvalidOperation_BadTransientModuleReference": return "Unable to make a reference to a transient module from a non-transient module.";
case "InvalidOperation_BadILGeneratorUsage": return "ILGenerator usage is invalid.";
case "InvalidOperation_BadInstructionOrIndexOutOfBound": return "MSIL instruction is invalid or index is out of bounds.";
case "InvalidOperation_ShouldNotHaveMethodBody": return "Method body should not exist.";
case "InvalidOperation_EntryMethodNotDefinedInAssembly": return "Entry method is not defined in the same assembly.";
case "InvalidOperation_CantSaveTransientAssembly": return "Cannot save a transient assembly.";
case "InvalidOperation_BadResourceContainer": return "Unable to add resource to transient module or transient assembly.";
case "InvalidOperation_CantInstantiateAbstractClass": return "Instances of abstract classes cannot be created.";
case "InvalidOperation_CantInstantiateFunctionPointer": return "Instances of function pointers cannot be created.";
case "InvalidOperation_BadTypeAttributesNotAbstract": return "Type must be declared abstract if any of its methods are abstract.";
case "InvalidOperation_BadInterfaceNotAbstract": return "Interface must be declared abstract.";
case "InvalidOperation_ConstructorNotAllowedOnInterface": return "Interface cannot have constructors.";
case "InvalidOperation_BadMethodBody": return "Method '{0}' cannot have a method body.";
case "InvalidOperation_MetaDataError": return "Metadata operation failed.";
case "InvalidOperation_BadEmptyMethodBody": return "Method '{0}' does not have a method body.";
case "InvalidOperation_EndInvokeCalledMultiple": return "EndInvoke can only be called once for each asynchronous operation.";
case "InvalidOperation_EndReadCalledMultiple": return "EndRead can only be called once for each asynchronous operation.";
case "InvalidOperation_EndWriteCalledMultiple": return "EndWrite can only be called once for each asynchronous operation.";
case "InvalidOperation_AsmLoadedForReflectionOnly": return "Assembly has been loaded as ReflectionOnly. This API requires an assembly capable of execution.";
case "InvalidOperation_NoAsmName": return "Assembly does not have an assembly name. In order to be registered for use by COM, an assembly must have a valid assembly name.";
case "InvalidOperation_NoAsmCodeBase": return "Assembly does not have a code base.";
case "InvalidOperation_HandleIsNotInitialized": return "Handle is not initialized.";
case "InvalidOperation_HandleIsNotPinned": return "Handle is not pinned.";
case "InvalidOperation_SlotHasBeenFreed": return "LocalDataStoreSlot storage has been freed.";
case "InvalidOperation_GlobalsHaveBeenCreated": return "Type definition of the global function has been completed.";
case "InvalidOperation_NotAVarArgCallingConvention": return "Calling convention must be VarArgs.";
case "InvalidOperation_CannotImportGlobalFromDifferentModule": return "Unable to import a global method or field from a different module.";
case "InvalidOperation_NonStaticComRegFunction": return "COM register function must be static.";
case "InvalidOperation_NonStaticComUnRegFunction": return "COM unregister function must be static.";
case "InvalidOperation_InvalidComRegFunctionSig": return "COM register function must have a System.Type parameter and a void return type.";
case "InvalidOperation_InvalidComUnRegFunctionSig": return "COM unregister function must have a System.Type parameter and a void return type.";
case "InvalidOperation_MultipleComRegFunctions": return "Type '{0}' has more than one COM registration function.";
case "InvalidOperation_MultipleComUnRegFunctions": return "Type '{0}' has more than one COM unregistration function.";
case "InvalidOperation_MustCallInitialize": return "You must call Initialize on this object instance before using it.";
case "InvalidOperation_MustLockForReadOrWrite": return "Object must be locked for read or write.";
case "InvalidOperation_MustLockForWrite": return "Object must be locked for read.";
case "InvalidOperation_NoValue": return "Nullable object must have a value.";
case "InvalidOperation_ResourceNotStream_Name": return "Resource '{0}' was not a Stream - call GetObject instead.";
case "InvalidOperation_ResourceNotString_Name": return "Resource '{0}' was not a String - call GetObject instead.";
case "InvalidOperation_ResourceNotString_Type": return "Resource was of type '{0}' instead of String - call GetObject instead.";
case "InvalidOperation_ResourceWriterSaved": return "The resource writer has already been closed and cannot be edited.";
case "InvalidOperation_UnderlyingArrayListChanged": return "This range in the underlying list is invalid. A possible cause is that elements were removed.";
case "InvalidOperation_AnonymousCannotImpersonate": return "An anonymous identity cannot perform an impersonation.";
case "InvalidOperation_DefaultConstructorILGen": return "Unable to access ILGenerator on a constructor created with DefineDefaultConstructor.";
case "InvalidOperation_DefaultConstructorDefineBody": return "The method body of the default constructor cannot be changed.";
case "InvalidOperation_ComputerName": return "Computer name could not be obtained.";
case "InvalidOperation_MismatchedAsyncResult": return "The IAsyncResult object provided does not match this delegate.";
case "InvalidOperation_PIAMustBeStrongNamed": return "Primary interop assemblies must be strongly named.";
case "InvalidOperation_HashInsertFailed": return "Hashtable insert failed. Load factor too high. The most common cause is multiple threads writing to the Hashtable simultaneously.";
case "InvalidOperation_UnknownEnumType": return "Unknown enum type.";
case "InvalidOperation_GetVersion": return "OSVersion's call to GetVersionEx failed.";
case "InvalidOperation_DateTimeParsing": return "Internal Error in DateTime and Calendar operations.";
case "InvalidOperation_UserDomainName": return "UserDomainName native call failed.";
case "InvalidOperation_WaitOnTransparentProxy": return "Cannot wait on a transparent proxy.";
case "InvalidOperation_NoPublicAddMethod": return "Cannot add the event handler since no public add method exists for the event.";
case "InvalidOperation_NoPublicRemoveMethod": return "Cannot remove the event handler since no public remove method exists for the event.";
case "InvalidOperation_NotSupportedOnWinRTEvent": return "Adding or removing event handlers dynamically is not supported on WinRT events.";
case "InvalidOperation_ConsoleKeyAvailableOnFile": return "Cannot see if a key has been pressed when either application does not have a console or when console input has been redirected from a file. Try Console.In.Peek.";
case "InvalidOperation_ConsoleReadKeyOnFile": return "Cannot read keys when either application does not have a console or when console input has been redirected from a file. Try Console.Read.";
case "InvalidOperation_ThreadWrongThreadStart": return "The thread was created with a ThreadStart delegate that does not accept a parameter.";
case "InvalidOperation_ThreadAPIsNotSupported": return "Use CompressedStack.(Capture/Run) or ExecutionContext.(Capture/Run) APIs instead.";
case "InvalidOperation_NotNewCaptureContext": return "Cannot apply a context that has been marshaled across AppDomains, that was not acquired through a Capture operation or that has already been the argument to a Set call.";
case "InvalidOperation_NullContext": return "Cannot call Set on a null context";
case "InvalidOperation_CannotCopyUsedContext": return "Only newly captured contexts can be copied";
case "InvalidOperation_CannotUseSwitcherOtherThread": return "Undo operation must be performed on the thread where the corresponding context was Set.";
case "InvalidOperation_SwitcherCtxMismatch": return "The Undo operation encountered a context that is different from what was applied in the corresponding Set operation. The possible cause is that a context was Set on the thread and not reverted(undone).";
case "InvalidOperation_CannotOverrideSetWithoutRevert": return "Must override both HostExecutionContextManager.SetHostExecutionContext and HostExecutionContextManager.Revert.";
case "InvalidOperation_CannotUseAFCOtherThread": return "AsyncFlowControl object must be used on the thread where it was created.";
case "InvalidOperation_CannotRestoreUnsupressedFlow": return "Cannot restore context flow when it is not suppressed.";
case "InvalidOperation_CannotSupressFlowMultipleTimes": return "Context flow is already suppressed.";
case "InvalidOperation_CannotUseAFCMultiple": return "AsyncFlowControl object can be used only once to call Undo().";
case "InvalidOperation_AsyncFlowCtrlCtxMismatch": return "AsyncFlowControl objects can be used to restore flow only on the Context that had its flow suppressed.";
case "InvalidOperation_TimeoutsNotSupported": return "Timeouts are not supported on this stream.";
case "InvalidOperation_Overlapped_Pack": return "Cannot pack a packed Overlapped again.";
case "InvalidOperation_OnlyValidForDS": return "Adding ACEs with Object Flags and Object GUIDs is only valid for directory-object ACLs.";
case "InvalidOperation_WrongAsyncResultOrEndReadCalledMultiple": return "Either the IAsyncResult object did not come from the corresponding async method on this type, or EndRead was called multiple times with the same IAsyncResult.";
case "InvalidOperation_WrongAsyncResultOrEndWriteCalledMultiple": return "Either the IAsyncResult object did not come from the corresponding async method on this type, or EndWrite was called multiple times with the same IAsyncResult.";
case "InvalidOperation_WrongAsyncResultOrEndCalledMultiple": return "Either the IAsyncResult object did not come from the corresponding async method on this type, or the End method was called multiple times with the same IAsyncResult.";
case "InvalidOperation_NoSecurityDescriptor": return "The object does not contain a security descriptor.";
case "InvalidOperation_NotAllowedInReflectionOnly": return "The requested operation is invalid in the ReflectionOnly context.";
case "InvalidOperation_NotAllowedInDynamicMethod": return "The requested operation is invalid for DynamicMethod.";
case "InvalidOperation_PropertyInfoNotAvailable": return "This API does not support PropertyInfo tokens.";
case "InvalidOperation_EventInfoNotAvailable": return "This API does not support EventInfo tokens.";
case "InvalidOperation_UnexpectedWin32Error": return "Unexpected error when calling an operating system function.  The returned error code is 0x{0:x}.";
case "InvalidOperation_AssertTransparentCode": return "Cannot perform CAS Asserts in Security Transparent methods";
case "InvalidOperation_NullModuleHandle": return "The requested operation is invalid when called on a null ModuleHandle.";
case "InvalidOperation_NotWithConcurrentGC": return "This API is not available when the concurrent GC is enabled.";
case "InvalidOperation_WithoutARM": return "This API is not available when AppDomain Resource Monitoring is not turned on.";
case "InvalidOperation_NotGenericType": return "This operation is only valid on generic types.";
case "InvalidOperation_TypeCannotBeBoxed": return "The given type cannot be boxed.";
case "InvalidOperation_HostModifiedSecurityState": return "The security state of an AppDomain was modified by an AppDomainManager configured with the NoSecurityChanges flag.";
case "InvalidOperation_StrongNameKeyPairRequired": return "A strong name key pair is required to emit a strong-named dynamic assembly.";
#if FEATURE_COMINTEROP
case "InvalidOperation_EventTokenTableRequiresDelegate": return "Type '{0}' is not a delegate type.  EventTokenTable may only be used with delegate types.";
#endif // FEATURE_COMINTEROP
case "InvalidOperation_NullArray": return "The underlying array is null.";
// ;system.security.claims
case "InvalidOperation_ClaimCannotBeRemoved": return "The Claim '{0}' was not able to be removed.  It is either not part of this Identity or it is a claim that is owned by the Principal that contains this Identity. For example, the Principal will own the claim when creating a GenericPrincipal with roles. The roles will be exposed through the Identity that is passed in the constructor, but not actually owned by the Identity.  Similar logic exists for a RolePrincipal.";
case "InvalidOperationException_ActorGraphCircular": return "Actor cannot be set so that circular directed graph will exist chaining the subjects together.";
case "InvalidOperation_AsyncIOInProgress": return "The stream is currently in use by a previous operation on the stream.";
case "InvalidOperation_APIInvalidForCurrentContext": return "The API '{0}' cannot be used on the current platform.";

// ; InvalidProgramException
case "InvalidProgram_Default": return "Common Language Runtime detected an invalid program.";

// ; Isolated Storage
#if FEATURE_ISOSTORE
case "IsolatedStorage_AssemblyMissingIdentity": return "Unable to determine assembly of the caller.";
case "IsolatedStorage_ApplicationMissingIdentity": return "Unable to determine application identity of the caller.";
case "IsolatedStorage_DomainMissingIdentity": return "Unable to determine domain of the caller.";
case "IsolatedStorage_AssemblyGrantSet": return "Unable to determine granted permission for assembly.";
case "IsolatedStorage_DomainGrantSet": return "Unable to determine granted permission for domain.";
case "IsolatedStorage_ApplicationGrantSet": return "Unable to determine granted permission for application.";
case "IsolatedStorage_Init": return "Initialization failed.";
case "IsolatedStorage_ApplicationNoEvidence": return "Unable to determine identity of application.";
case "IsolatedStorage_AssemblyNoEvidence": return "Unable to determine identity of assembly.";
case "IsolatedStorage_DomainNoEvidence": return "Unable to determine the identity of domain.";
case "IsolatedStorage_DeleteDirectories": return "Unable to delete; directory or files in the directory could be in use.";
case "IsolatedStorage_DeleteFile": return "Unable to delete file.";
case "IsolatedStorage_CreateDirectory": return "Unable to create directory.";
case "IsolatedStorage_DeleteDirectory": return "Unable to delete, directory not empty or does not exist.";
case "IsolatedStorage_Operation_ISFS": return "Operation not permitted on IsolatedStorageFileStream.";
case "IsolatedStorage_Operation": return "Operation not permitted.";
case "IsolatedStorage_Path": return "Path must be a valid file name.";
case "IsolatedStorage_FileOpenMode": return "Invalid mode, see System.IO.FileMode.";
case "IsolatedStorage_SeekOrigin": return "Invalid origin, see System.IO.SeekOrigin.";
case "IsolatedStorage_Scope_U_R_M": return "Invalid scope, expected User, User|Roaming or Machine.";
case "IsolatedStorage_Scope_Invalid": return "Invalid scope.";
case "IsolatedStorage_Exception": return "An error occurred while accessing IsolatedStorage.";
case "IsolatedStorage_QuotaIsUndefined": return "{0} is not defined for this store. An operation was performed that requires access to {0}. Stores obtained using enumeration APIs do not have a well-defined {0}, since partial evidence is used to open the store.";
case "IsolatedStorage_CurrentSizeUndefined": return "Current size cannot be determined for this store.";
case "IsolatedStorage_DomainUndefined": return "Domain cannot be determined on an Assembly or Application store.";
case "IsolatedStorage_ApplicationUndefined": return "Application cannot be determined on an Assembly or Domain store.";
case "IsolatedStorage_AssemblyUndefined": return "Assembly cannot be determined for an Application store.";
case "IsolatedStorage_StoreNotOpen": return "Store must be open for this operation.";
case "IsolatedStorage_OldQuotaLarger": return "The new quota must be larger than the old quota.";
case "IsolatedStorage_UsageWillExceedQuota": return "There is not enough free space to perform the operation.";
case "IsolatedStorage_NotValidOnDesktop": return "The Site scope is currently not supported.";
case "IsolatedStorage_OnlyIncreaseUserApplicationStore": return "Increasing the quota of this scope is not supported.  Only the user application scope�s quota can be increased.";
#endif  // FEATURE_ISOSTORE

// ; Verification Exception
case "Verification_Exception": return "Operation could destabilize the runtime.";

// ; IL stub marshaler exceptions
case "Marshaler_StringTooLong": return "Marshaler restriction: Excessively long string.";

// ; Missing (General)
case "MissingConstructor_Name": return "Constructor on type '{0}' not found.";
case "MissingField": return "Field not found.";
case "MissingField_Name": return "Field '{0}' not found.";
case "MissingMember": return "Member not found.";
case "MissingMember_Name": return "Member '{0}' not found.";
case "MissingMethod_Name": return "Method '{0}' not found.";
case "MissingModule": return "Module '{0}' not found.";
case "MissingType": return "Type '{0}' not found.";

// ; MissingManifestResourceException
case "Arg_MissingManifestResourceException": return "Unable to find manifest resource.";
case "MissingManifestResource_LooselyLinked": return "Could not find a manifest resource entry called \"{0}\" in assembly \"{1}\". Please check spelling, capitalization, and build rules to ensure \"{0}\" is being linked into the assembly.";
case "MissingManifestResource_NoNeutralAsm": return "Could not find any resources appropriate for the specified culture or the neutral culture.  Make sure \"{0}\" was correctly embedded or linked into assembly \"{1}\" at compile time, or that all the satellite assemblies required are loadable and fully signed.";
case "MissingManifestResource_NoNeutralDisk": return "Could not find any resources appropriate for the specified culture (or the neutral culture) on disk.";
case "MissingManifestResource_MultipleBlobs": return "A case-insensitive lookup for resource file \"{0}\" in assembly \"{1}\" found multiple entries. Remove the duplicates or specify the exact case.";
#if !FEATURE_CORECLR
case "MissingManifestResource_ResWFileNotLoaded": return "Unable to load resources for resource file \"{0}\" in package \"{1}\".";
case "MissingManifestResource_NoPRIresources": return "Unable to open Package Resource Index.";
#endif

// ; MissingMember
case "MissingMemberTypeRef": return "FieldInfo does not match the target Type.";
case "MissingMemberNestErr": return "TypedReference can only be made on nested value Types.";

// ; MissingSatelliteAssemblyException
case "MissingSatelliteAssembly_Default": return "Resource lookup fell back to the ultimate fallback resources in a satellite assembly, but that satellite either was not found or could not be loaded. Please consider reinstalling or repairing the application.";
case "MissingSatelliteAssembly_Culture_Name": return "The satellite assembly named \"{1}\" for fallback culture \"{0}\" either could not be found or could not be loaded. This is generally a setup problem. Please consider reinstalling or repairing the application.";

// ; MulticastNotSupportedException
case "Multicast_Combine": return "Delegates that are not of type MulticastDelegate may not be combined.";

// ; NotImplementedException
case "Arg_NotImplementedException": return "The method or operation is not implemented.";
case "NotImplemented_ResourcesLongerThan2^63": return "Resource files longer than 2^63 bytes are not currently implemented.";

// ; NotSupportedException
case "NotSupported_NYI": return "This feature is not currently implemented.";
case "NotSupported_AbstractNonCLS": return "This non-CLS method is not implemented.";
case "NotSupported_ChangeType": return "ChangeType operation is not supported.";
case "NotSupported_ContainsStackPtr": return "Cannot create boxed TypedReference, ArgIterator, or RuntimeArgumentHandle Objects.";
case "NotSupported_ContainsStackPtr[]": return "Cannot create arrays of TypedReference, ArgIterator, ByRef, or RuntimeArgumentHandle Objects.";
case "NotSupported_OpenType": return "Cannot create arrays of open type. ";
case "NotSupported_DBNullSerial": return "Only one DBNull instance may exist, and calls to DBNull deserialization methods are not allowed.";
case "NotSupported_DelegateSerHolderSerial": return "DelegateSerializationHolder objects are designed to represent a delegate during serialization and are not serializable themselves.";
case "NotSupported_DelegateCreationFromPT": return "Application code cannot use Activator.CreateInstance to create types that derive from System.Delegate. Delegate.CreateDelegate can be used instead.";
case "NotSupported_EncryptionNeedsNTFS": return "File encryption support only works on NTFS partitions.";
case "NotSupported_FileStreamOnNonFiles": return "FileStream was asked to open a device that was not a file. For support for devices like 'com1:' or 'lpt1:', call CreateFile, then use the FileStream constructors that take an OS handle as an IntPtr.";
case "NotSupported_FixedSizeCollection": return "Collection was of a fixed size.";
case "NotSupported_KeyCollectionSet": return "Mutating a key collection derived from a dictionary is not allowed.";
case "NotSupported_ValueCollectionSet": return "Mutating a value collection derived from a dictionary is not allowed.";
case "NotSupported_MemStreamNotExpandable": return "Memory stream is not expandable.";
case "NotSupported_ObsoleteResourcesFile": return "Found an obsolete .resources file in assembly '{0}'. Rebuild that .resources file then rebuild that assembly.";
case "NotSupported_OleAutBadVarType": return "The given Variant type is not supported by this OleAut function.";
case "NotSupported_PopulateData": return "This Surrogate does not support PopulateData().";
case "NotSupported_ReadOnlyCollection": return "Collection is read-only.";
case "NotSupported_RangeCollection": return "The specified operation is not supported on Ranges.";
case "NotSupported_SortedListNestedWrite": return "This operation is not supported on SortedList nested types because they require modifying the original SortedList.";
case "NotSupported_SubclassOverride": return "Derived classes must provide an implementation.";
case "NotSupported_TypeCannotDeserialized": return "Direct deserialization of type '{0}' is not supported.";
case "NotSupported_UnreadableStream": return "Stream does not support reading.";
case "NotSupported_UnseekableStream": return "Stream does not support seeking.";
case "NotSupported_UnwritableStream": return "Stream does not support writing.";
case "NotSupported_CannotWriteToBufferedStreamIfReadBufferCannotBeFlushed": return "Cannot write to a BufferedStream while the read buffer is not empty if the underlying stream is not seekable. Ensure that the stream underlying this BufferedStream can seek or avoid interleaving read and write operations on this BufferedStream.";
case "NotSupported_Method": return "Method is not supported.";
case "NotSupported_Constructor": return "Object cannot be created through this constructor.";
case "NotSupported_DynamicModule": return "The invoked member is not supported in a dynamic module.";
case "NotSupported_TypeNotYetCreated": return "The invoked member is not supported before the type is created.";
case "NotSupported_SymbolMethod": return "Not supported in an array method of a type definition that is not complete.";
case "NotSupported_NotDynamicModule": return "The MethodRental.SwapMethodBody method can only be called to swap the method body of a method in a dynamic module.";
case "NotSupported_DynamicAssembly": return "The invoked member is not supported in a dynamic assembly.";
case "NotSupported_NotAllTypesAreBaked": return "Type '{0}' was not completed.";
case "NotSupported_CannotSaveModuleIndividually": return "Unable to save a ModuleBuilder if it was created underneath an AssemblyBuilder. Call Save on the AssemblyBuilder instead.";
case "NotSupported_MaxWaitHandles": return "The number of WaitHandles must be less than or equal to 64.";
case "NotSupported_IllegalOneByteBranch": return "Illegal one-byte branch at position: {0}. Requested branch was: {1}.";
case "NotSupported_OutputStreamUsingTypeBuilder": return "Output streams do not support TypeBuilders.";
case "NotSupported_ValueClassCM": return "Custom marshalers for value types are not currently supported.";
case "NotSupported_Void[]": return "Arrays of System.Void are not supported.";
case "NotSupported_NoParentDefaultConstructor": return "Parent does not have a default constructor. The default constructor must be explicitly defined.";
case "NotSupported_NonReflectedType": return "Not supported in a non-reflected type.";
case "NotSupported_GlobalFunctionNotBaked": return "The type definition of the global function is not completed.";
case "NotSupported_SecurityPermissionUnion": return "Union is not implemented.";
case "NotSupported_UnitySerHolder": return "The UnitySerializationHolder object is designed to transmit information about other types and is not serializable itself.";
case "NotSupported_UnknownTypeCode": return "TypeCode '{0}' was not valid.";
case "NotSupported_WaitAllSTAThread": return "WaitAll for multiple handles on a STA thread is not supported.";
case "NotSupported_SignalAndWaitSTAThread": return "SignalAndWait on a STA thread is not supported.";
case "NotSupported_CreateInstanceWithTypeBuilder": return "CreateInstance cannot be used with an object of type TypeBuilder.";
case "NotSupported_NonUrlAttrOnMBR": return "UrlAttribute is the only attribute supported for MarshalByRefObject.";
case "NotSupported_ActivAttrOnNonMBR": return "Activation Attributes are not supported for types not deriving from MarshalByRefObject.";
case "NotSupported_ActivForCom": return "Activation Attributes not supported for COM Objects.";
case "NotSupported_NoCodepageData": return "No data is available for encoding {0}. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.";
case "NotSupported_CodePage50229": return "The ISO-2022-CN Encoding (Code page 50229) is not supported.";
case "NotSupported_DynamicAssemblyNoRunAccess": return "Cannot execute code on a dynamic assembly without run access.";
case "NotSupported_IDispInvokeDefaultMemberWithNamedArgs": return "Invoking default method with named arguments is not supported.";
case "NotSupported_Type": return "Type is not supported.";
case "NotSupported_GetMethod": return "The 'get' method is not supported on this property.";
case "NotSupported_SetMethod": return "The 'set' method is not supported on this property.";
case "NotSupported_DeclarativeUnion": return "Declarative unionizing of these permissions is not supported.";
case "NotSupported_StringComparison": return "The string comparison type passed in is currently not supported.";
case "NotSupported_WrongResourceReader_Type": return "This .resources file should not be read with this reader. The resource reader type is \"{0}\".";
case "NotSupported_MustBeModuleBuilder": return "Module argument must be a ModuleBuilder.";
case "NotSupported_CallToVarArg": return "Vararg calling convention not supported.";
case "NotSupported_TooManyArgs": return "Stack size too deep. Possibly too many arguments.";
case "NotSupported_DeclSecVarArg": return "Assert, Deny, and PermitOnly are not supported on methods with a Vararg calling convention.";
case "NotSupported_AmbiguousIdentity": return "The operation is ambiguous because the permission represents multiple identities.";
case "NotSupported_DynamicMethodFlags": return "Wrong MethodAttributes or CallingConventions for DynamicMethod. Only public, static, standard supported";
case "NotSupported_GlobalMethodSerialization": return "Serialization of global methods (including implicit serialization via the use of asynchronous delegates) is not supported.";
case "NotSupported_InComparableType": return "A type must implement IComparable<T> or IComparable to support comparison.";
case "NotSupported_ManagedActivation": return "Cannot create uninitialized instances of types requiring managed activation.";
case "NotSupported_ByRefReturn": return "ByRef return value not supported in reflection invocation.";
case "NotSupported_DelegateMarshalToWrongDomain": return "Delegates cannot be marshaled from native code into a domain other than their home domain.";
case "NotSupported_ResourceObjectSerialization": return "Cannot read resources that depend on serialization.";
case "NotSupported_One": return "The arithmetic type '{0}' cannot represent the number one.";
case "NotSupported_Zero": return "The arithmetic type '{0}' cannot represent the number zero.";
case "NotSupported_MaxValue": return "The arithmetic type '{0}' does not have a maximum value.";
case "NotSupported_MinValue": return "The arithmetic type '{0}' does not have a minimum value.";
case "NotSupported_PositiveInfinity": return "The arithmetic type '{0}' cannot represent positive infinity.";
case "NotSupported_NegativeInfinity": return "The arithmetic type '{0}' cannot represent negative infinity.";
case "NotSupported_UmsSafeBuffer": return "This operation is not supported for an UnmanagedMemoryStream created from a SafeBuffer.";
case "NotSupported_Reading": return "Accessor does not support reading.";
case "NotSupported_Writing": return "Accessor does not support writing.";
case "NotSupported_UnsafePointer": return "This accessor was created with a SafeBuffer; use the SafeBuffer to gain access to the pointer.";
case "NotSupported_CollectibleCOM": return "COM Interop is not supported for collectible types.";
case "NotSupported_CollectibleAssemblyResolve": return "Resolving to a collectible assembly is not supported.";
case "NotSupported_CollectibleBoundNonCollectible": return "A non-collectible assembly may not reference a collectible assembly.";
case "NotSupported_CollectibleDelegateMarshal": return "Delegate marshaling for types within collectible assemblies is not supported.";
#if FEATURE_WINDOWSPHONE
case "NotSupported_UserDllImport": return "DllImport cannot be used on user-defined methods.";
case "NotSupported_UserCOM": return "COM Interop is not supported for user-defined types.";
#endif //FEATURE_WINDOWSPHONE
#if FEATURE_CAS_POLICY
case "NotSupported_RequiresCasPolicyExplicit": return "This method explicitly uses CAS policy, which has been obsoleted by the .NET Framework. In order to enable CAS policy for compatibility reasons, please use the NetFx40_LegacySecurityPolicy configuration switch. Please see http://go.microsoft.com/fwlink/?LinkID": return "155570 for more information.";
case "NotSupported_RequiresCasPolicyImplicit": return "This method implicitly uses CAS policy, which has been obsoleted by the .NET Framework. In order to enable CAS policy for compatibility reasons, please use the NetFx40_LegacySecurityPolicy configuration switch. Please see http://go.microsoft.com/fwlink/?LinkID": return "155570 for more information.";
case "NotSupported_CasDeny": return "The Deny stack modifier has been obsoleted by the .NET Framework.  Please see http://go.microsoft.com/fwlink/?LinkId": return "155571 for more information.";
case "NotSupported_SecurityContextSourceAppDomainInHeterogenous": return "SecurityContextSource.CurrentAppDomain is not supported in heterogenous AppDomains.";
#endif // FEATURE_CAS_POLICY
#if FEATURE_APPX
case "NotSupported_AppX": return "{0} is not supported in AppX.";
case "LoadOfFxAssemblyNotSupported_AppX": return "{0} of .NET Framework assemblies is not supported in AppX.";
#endif
#if FEATURE_COMINTEROP
case "NotSupported_WinRT_PartialTrust": return "Windows Runtime is not supported in partial trust.";
#endif // FEATURE_COMINTEROP
// ; ReflectionTypeLoadException
case "ReflectionTypeLoad_LoadFailed": return "Unable to load one or more of the requested types. Retrieve the LoaderExceptions property for more information.";
#if !FEATURE_CORECLR
case "NotSupported_NoTypeInfo": return "Cannot resolve {0} to a TypeInfo object.";
#endif
#if FEATURE_COMINTEROP
case "NotSupported_PIAInAppxProcess": return "A Primary Interop Assembly is not supported in AppX.";
#endif
#if FEATURE_WINDOWSPHONE
case "NotSupported_WindowsPhone": return "{0} is not supported on Windows Phone.";
case "NotSupported_AssemblyLoadCodeBase": return "Assembly.Load with a Codebase is not supported on Windows Phone.";
#endif

// ; TypeLoadException
case "TypeLoad_ResolveType": return "Could not resolve type '{0}'.";
case "TypeLoad_ResolveTypeFromAssembly": return "Could not resolve type '{0}' in assembly '{1}'.";
case "TypeLoad_ResolveNestedType": return "Could not resolve nested type '{0}' in type \"{1}'.";
case "FileNotFound_ResolveAssembly": return "Could not resolve assembly '{0}'.";

// ; NullReferenceException
case "NullReference_This": return "The pointer for this method was null.";

// ; ObjectDisposedException
case "ObjectDisposed_Generic": return "Cannot access a disposed object.";
case "ObjectDisposed_FileClosed": return "Cannot access a closed file.";
case "ObjectDisposed_ObjectName_Name": return "Object name: '{0}'.";
case "ObjectDisposed_ReaderClosed": return "Cannot read from a closed TextReader.";
case "ObjectDisposed_ResourceSet": return "Cannot access a closed resource set.";
case "ObjectDisposed_RegKeyClosed": return "Cannot access a closed registry key.";
case "ObjectDisposed_StreamClosed": return "Cannot access a closed Stream.";
case "ObjectDisposed_WriterClosed": return "Cannot write to a closed TextWriter.";
case "ObjectDisposed_ViewAccessorClosed": return "Cannot access a closed accessor.";

// ; OperationCanceledException
case "OperationCanceled": return "The operation was canceled.";

// ; OutOfMemoryException
case "OutOfMemory_GCHandleMDA": return "The GCHandle MDA has run out of available cookies.";

// ; OverflowException
case "Overflow_Byte": return "Value was either too large or too small for an unsigned byte.";
case "Overflow_Char": return "Value was either too large or too small for a character.";
case "Overflow_Currency": return "Value was either too large or too small for a Currency.";
case "Overflow_Decimal": return "Value was either too large or too small for a Decimal.";
case "Overflow_Int16": return "Value was either too large or too small for an Int16.";
case "Overflow_Int32": return "Value was either too large or too small for an Int32.";
case "Overflow_Int64": return "Value was either too large or too small for an Int64.";
case "Overflow_NegateTwosCompNum": return "Negating the minimum value of a twos complement number is invalid.";
case "Overflow_NegativeUnsigned": return "The string was being parsed as an unsigned number and could not have a negative sign.";
case "Overflow_SByte": return "Value was either too large or too small for a signed byte.";
case "Overflow_Single": return "Value was either too large or too small for a Single.";
case "Overflow_Double": return "Value was either too large or too small for a Double.";
case "Overflow_TimeSpanTooLong": return "TimeSpan overflowed because the duration is too long.";
case "Overflow_TimeSpanElementTooLarge": return "The TimeSpan could not be parsed because at least one of the numeric components is out of range or contains too many digits.";
case "Overflow_Duration": return "The duration cannot be returned for TimeSpan.MinValue because the absolute value of TimeSpan.MinValue exceeds the value of TimeSpan.MaxValue.";
case "Overflow_UInt16": return "Value was either too large or too small for a UInt16.";
case "Overflow_UInt32": return "Value was either too large or too small for a UInt32.";
case "Overflow_UInt64": return "Value was either too large or too small for a UInt64.";

// ; PlatformNotsupportedException
case "PlatformNotSupported_RequiresLonghorn": return "This operation is only supported on Windows Vista and above.";
case "PlatformNotSupported_RequiresNT": return "This operation is only supported on Windows 2000, Windows XP, and higher.";
case "PlatformNotSupported_RequiresW2kSP3": return "This operation is only supported on Windows 2000 SP3 or later operating systems.";
#if FEATURE_COMINTEROP
case "PlatformNotSupported_WinRT": return "Windows Runtime is not supported on this operating system.";
#endif // FEATURE_COMINTEROP

// ; PolicyException
// ; This still appears in bcl.small but should go away eventually
case "Policy_Default": return "Error occurred while performing a policy operation.";
case "Policy_CannotLoadSemiTrustAssembliesDuringInit": return "All assemblies loaded as part of AppDomain initialization must be fully trusted.";
#if FEATURE_IMPERSONATION
case "Policy_PrincipalTwice": return "Default principal object cannot be set twice.";
#endif // FEATURE_IMPERSONATION
#if FEATURE_CAS_POLICY
case "Policy_PolicyAlreadySet": return "Policy for this domain cannot be set twice.";
case "Policy_NoExecutionPermission": return "Execution permission cannot be acquired.";
case "Policy_NoRequiredPermission": return "Required permissions cannot be acquired.";
case "Policy_MultipleExclusive": return "More than one exclusive group is not allowed.";
case "Policy_RecoverNotFileBased": return "PolicyLevel object not based on a file cannot be recovered.";
case "Policy_RecoverNoConfigFile": return "No old configuration file exists to recover.";
case "Policy_UnableToSave": return "Policy level '{0}' could not be saved: {1}.";
case "Policy_BadXml": return "Policy configuration XML is invalid. The required tag '{0}' is missing.";
case "Policy_NonFullTrustAssembly": return "Policy references an assembly not in the full trust assemblies list.";
case "Policy_MissingActivationContextInAppEvidence": return "The application evidence does not contain a Fusion activation context.";
case "Policy_NoTrustManager": return "A trust manager could not be loaded for this application.";
case "Policy_GrantSetDoesNotMatchDomain": return "An assembly was provided an invalid grant set by runtime host '{0}'. In a homogenous AppDomain, the only valid grant sets are FullTrust and the AppDomain's sandbox grant set.";
#endif  // FEATURE_CAS_POLICY
case "Policy_SaveNotFileBased": return "PolicyLevel object not based on a file cannot be saved.";
case "Policy_AppTrustMustGrantAppRequest": return "ApplicationTrust grant set does not contain ActivationContext's minimum request set.";

case "Error_SecurityPolicyFileParse": return "Error occurred while parsing the '{0}' policy level. The default policy level was used instead.";
case "Error_SecurityPolicyFileParseEx": return "Error '{1}' occurred while parsing the '{0}' policy level. The default policy level was used instead.";

#if FEATURE_CAS_POLICY
case "Policy_EvidenceMustBeSerializable": return "Objects used as evidence must be serializable.";
case "Policy_DuplicateEvidence": return "The evidence collection already contains evidence of type '{0}'. Multiple pieces of the same type of evidence are not allowed.";
case "Policy_IncorrectHostEvidence": return "Runtime host '{0}' returned evidence of type '{1}' from a request for evidence of type '{2}'.";
case "Policy_NullHostEvidence": return "Runtime host '{0}' returned null when asked for assembly evidence for assembly '{1}'.";
case "Policy_NullHostGrantSet": return "Runtime host '{0}' returned a null grant set from ResolvePolicy.";
#endif // FEATURE_CAS_POLICY

// ; Policy codegroup and permission set names and descriptions
#if FEATURE_CAS_POLICY
case "Policy_AllCode_Name": return "All_Code";
case "Policy_AllCode_DescriptionFullTrust": return "Code group grants all code full trust and forms the root of the code group tree.";
case "Policy_AllCode_DescriptionNothing": return "Code group grants no permissions and forms the root of the code group tree.";
case "Policy_MyComputer_Name": return "My_Computer_Zone";
case "Policy_MyComputer_Description": return "Code group grants full trust to all code originating on the local computer";
case "Policy_Intranet_Name": return "LocalIntranet_Zone";
case "Policy_Intranet_Description": return "Code group grants the intranet permission set to code from the intranet zone. This permission set grants intranet code the right to use isolated storage, full UI access, some capability to do reflection, and limited access to environment variables.";
case "Policy_IntranetNet_Name": return "Intranet_Same_Site_Access";
case "Policy_IntranetNet_Description": return "All intranet code gets the right to connect back to the site of its origin.";
case "Policy_IntranetFile_Name": return "Intranet_Same_Directory_Access";
case "Policy_IntranetFile_Description": return "All intranet code gets the right to read from its install directory.";
case "Policy_Internet_Name": return "Internet_Zone";
case "Policy_Internet_Description": return "Code group grants code from the Internet zone the Internet permission set. This permission set grants Internet code the right to use isolated storage and limited UI access.";
case "Policy_InternetNet_Name": return "Internet_Same_Site_Access";
case "Policy_InternetNet_Description": return "All Internet code gets the right to connect back to the site of its origin.";
case "Policy_Trusted_Name": return "Trusted_Zone";
case "Policy_Trusted_Description": return "Code from a trusted zone is granted the Internet permission set. This permission set grants the right to use isolated storage and limited UI access.";
case "Policy_TrustedNet_Name": return "Trusted_Same_Site_Access";
case "Policy_TrustedNet_Description": return "All Trusted Code gets the right to connect back to the site of its origin.";
case "Policy_Untrusted_Name": return "Restricted_Zone";
case "Policy_Untrusted_Description": return "Code coming from a restricted zone does not receive any permissions.";
case "Policy_Microsoft_Name": return "Microsoft_Strong_Name";
case "Policy_Microsoft_Description": return "Code group grants full trust to code signed with the Microsoft strong name.";
case "Policy_Ecma_Name": return "ECMA_Strong_Name";
case "Policy_Ecma_Description": return "Code group grants full trust to code signed with the ECMA strong name.";

// ; Policy permission set descriptions
case "Policy_PS_FullTrust": return "Allows full access to all resources";
case "Policy_PS_Everything": return "Allows unrestricted access to all resources covered by built-in permissions";
case "Policy_PS_Nothing": return "Denies all resources, including the right to execute";
case "Policy_PS_Execution": return "Permits execution";
case "Policy_PS_SkipVerification": return "Grants right to bypass the verification";
case "Policy_PS_Internet": return "Default rights given to Internet applications";
case "Policy_PS_LocalIntranet": return "Default rights given to applications on the local intranet";

// ; default Policy level names
case "Policy_PL_Enterprise": return "Enterprise";
case "Policy_PL_Machine": return "Machine";
case "Policy_PL_User": return "User";
case "Policy_PL_AppDomain": return "AppDomain";
#endif  // FEATURE_CAS_POLICY

// ; RankException
case "Rank_MultiDimNotSupported": return "Only single dimension arrays are supported here.";
case "Rank_MustMatch": return "The specified arrays must have the same number of dimensions.";

// ; TypeInitializationException
case "TypeInitialization_Default": return "Type constructor threw an exception.";
case "TypeInitialization_Type": return "The type initializer for '{0}' threw an exception.";

// ; TypeLoadException


// ;
// ; Reflection exceptions
// ;
case "RtType.InvalidCaller": return "Caller is not a friend.";

// ;CustomAttributeFormatException
case "RFLCT.InvalidPropFail": return "'{0}' property specified was not found.";
case "RFLCT.InvalidFieldFail": return "'{0}' field specified was not found.";

// ;InvalidFilterCriteriaException
case "RFLCT.FltCritString": return "A String must be provided for the filter criteria.";
case "RFLCT.FltCritInt": return "An Int32 must be provided for the filter criteria.";

// ; TargetException
case "RFLCT.Targ_ITargMismatch": return "Object does not match target type.";
case "RFLCT.Targ_StatMethReqTarg": return "Non-static method requires a target.";
case "RFLCT.Targ_StatFldReqTarg": return "Non-static field requires a target.";

// ;AmbiguousMatchException
case "RFLCT.Ambiguous": return "Ambiguous match found.";
case "RFLCT.AmbigCust": return "Multiple custom attributes of the same type found.";

// ;
// ; Remoting exceptions
// ;
case "Remoting_AppDomainUnloaded_ThreadUnwound": return "The application domain in which the thread was running has been unloaded.";
case "Remoting_AppDomainUnloaded": return "The target application domain has been unloaded.";
case "Remoting_CantRemotePointerType": return "Pointer types cannot be passed in a remote call.";
case "Remoting_TypeCantBeRemoted": return "The given type cannot be passed in a remote call.";
case "Remoting_Delegate_TooManyTargets": return "The delegate must have only one target.";
case "Remoting_InvalidContext": return "The context is not valid.";
case "Remoting_InvalidValueTypeFieldAccess": return "An attempt was made to calculate the address of a value type field on a remote object. This was likely caused by an attempt to directly get or set the value of a field within this embedded value type. Avoid this and instead provide and use access methods for each field in the object that will be accessed remotely.";
case "Remoting_Message_BadRetValOrOutArg": return "Bad return value or out-argument inside the return message.";
case "Remoting_NonPublicOrStaticCantBeCalledRemotely": return "Permission denied: cannot call non-public or static methods remotely.";
case "Remoting_Proxy_ProxyTypeIsNotMBR": return "classToProxy argument must derive from MarshalByRef type.";
case "Remoting_TP_NonNull": return "The transparent proxy field of a real proxy must be null.";
#if FEATURE_REMOTING
case "Remoting_Activation_BadAttribute": return "Activation attribute does not implement the IContextAttribute interface.";
case "Remoting_Activation_BadObject": return "Proxy Attribute returned an incompatible object when constructing an instance of type {0}.";
case "Remoting_Activation_MBR_ProxyAttribute": return "Proxy Attributes are supported on ContextBound types only.";
case "Remoting_Activation_ConnectFailed": return "An attempt to connect to the remote activator failed with exception '{0}'.";
case "Remoting_Activation_Failed": return "Activation failed due to an unknown reason.";
case "Remoting_Activation_InconsistentState": return "Inconsistent state during activation; there may be two proxies for the same object.";
case "Remoting_Activation_MissingRemoteAppEntry": return "Cannot find an entry for remote application '{0}'.";
case "Remoting_Activation_NullReturnValue": return "Return value of construction call was null.";
case "Remoting_Activation_NullFromInternalUnmarshal": return "InternalUnmarshal of returned ObjRef from activation call returned null.";
case "Remoting_Activation_WellKnownCTOR": return "Cannot run a non-default constructor when connecting to well-known objects.";
case "Remoting_Activation_PermissionDenied": return "Type '{0}' is not registered for activation.";
case "Remoting_Activation_PropertyUnhappy": return "A context property did not approve the candidate context for activating the object.";
case "Remoting_Activation_AsyncUnsupported": return "Async Activation not supported.";
case "Remoting_AmbiguousCTOR": return "Cannot resolve the invocation to the correct constructor.";
case "Remoting_AmbiguousMethod": return "Cannot resolve the invocation to the correct method.";
case "Remoting_AppDomains_NYI": return "This feature is not yet supported for cross-application domain.";
case "Remoting_AppDomainsCantBeCalledRemotely": return "Permission denied: cannot call methods on the AppDomain class remotely.";
case "Remoting_AssemblyLoadFailed": return "Cannot load assembly '{0}'.";
case "Remoting_Attribute_UseAttributeNotsettable": return "UseAttribute not allowed in SoapTypeAttribute.";
case "Remoting_BadType": return "Cannot load type '{0}'.";
case "Remoting_BadField": return "Remoting cannot find field '{0}' on type '{1}'.";
case "Remoting_BadInternalState_ActivationFailure": return "Invalid internal state: Activation service failed to initialize.";
case "Remoting_BadInternalState_ProxySameAppDomain": return "Invalid internal state: A marshal by ref object should not have a proxy in its own AppDomain.";
case "Remoting_BadInternalState_FailEnvoySink": return "Invalid internal state: Failed to create an envoy sink for the object.";
case "Remoting_CantDisconnectClientProxy": return "Cannot call disconnect on a proxy.";
case "Remoting_CantInvokeIRemoteDispatch": return "Cannot invoke methods on IRemoteDispatch.";
case "Remoting_ChannelNameAlreadyRegistered": return "The channel '{0}' is already registered.";
case "Remoting_ChannelNotRegistered": return "The channel '{0}' is not registered with remoting services.";
case "Remoting_Channel_PopOnEmptySinkStack": return "Tried to pop data from an empty channel sink stack.";
case "Remoting_Channel_PopFromSinkStackWithoutPush": return "A channel sink tried to pop data from the stack without first pushing data onto the stack.";
case "Remoting_Channel_StoreOnEmptySinkStack": return "A channel sink called the Store method when the sink stack was empty.";
case "Remoting_Channel_StoreOnSinkStackWithoutPush": return "A channel sink called the Store method on the sink stack without first pushing data onto the stack.";
case "Remoting_Channel_CantCallAPRWhenStackEmpty": return "Cannot call the AsyncProcessResponse method on the previous channel sink because the stack is empty.";
case "Remoting_Channel_CantCallFRSWhenStackEmtpy": return "Called FlipRememberedStack() when stack was not null.";
case "Remoting_Channel_CantCallGetResponseStreamWhenStackEmpty": return "Cannot call the GetResponseStream method on the previous channel sink because the stack is empty.";
case "Remoting_Channel_DispatchSinkMessageMissing": return "No message was deserialized prior to calling the DispatchChannelSink.";
case "Remoting_Channel_DispatchSinkWantsNullRequestStream": return "The request stream should be null when the DispatchChannelSink is called. ";
case "Remoting_Channel_CannotBeSecured": return "Channel {0} cannot be secured. Please consider using a channel that implements ISecurableChannel";
case "Remoting_Config_ChannelMissingCtor": return "To be used from a .config file, the channel type '{0}' must have a constructor of the form '{1}'";
case "Remoting_Config_SinkProviderMissingCtor": return "To be used from a .config file, the sink provider type '{0}' must have a constructor of the form '{1}'";
case "Remoting_Config_SinkProviderNotFormatter": return "A sink provider of type '{0}' is incorrectly labeled as a 'formatter'.";
case "Remoting_Config_ConfigurationFailure": return "Remoting configuration failed with the exception '{0}'.";
case "Remoting_Config_InvalidTimeFormat": return "Invalid time format '{0}'. Examples of valid time formats include 7D, 10H, 5M, 30S, or 20MS.";
case "Remoting_Config_AppNameSet": return "The remoting application name, '{0}', had already been set.";
case "Remoting_Config_ErrorsModeSet": return "The remoting custom errors mode had already been set.";
case "Remoting_Config_CantRedirectActivationOfWellKnownService": return "Attempt to redirect activation for type '{0}, {1}'. This is not allowed since either a well-known service type has already been registered with that type or that type has been registered has a activated service type.";
case "Remoting_Config_CantUseRedirectedTypeForWellKnownService": return "Attempt to register a well-known or activated service type of type '{0}, {1}'. This is not allowed since the type has already been redirected to activate elsewhere.";
case "Remoting_Config_InvalidChannelType": return "'{0}' does not implement IChannelReceiver or IChannelSender. All channels must implement one of these interfaces.";
case "Remoting_Config_InvalidSinkProviderType": return "Unable to use '{0}' as a channel sink provider. It does not implement '{1}'.";
case "Remoting_Config_MissingWellKnownModeAttribute": return "Well-known service entries must contain a 'mode' attribute with a value of 'Singleton' or 'SingleCall'.";
case "Remoting_Config_MissingTypeAttribute": return "'{0}' entries must contain a '{1}' attribute of the form 'typeName, assemblyName'.";
case "Remoting_Config_MissingXmlTypeAttribute": return "'{0}' entries must contain a '{1}' attribute of the form 'xmlTypeName, xmlTypeNamespace'.";
case "Remoting_Config_NoAppName": return "Improper remoting configuration: missing ApplicationName property.";
case "Remoting_Config_NonTemplateIdAttribute": return "Only '{0}' templates can have an 'id' attribute.";
case "Remoting_Config_PreloadRequiresTypeOrAssembly": return "Preload entries require a type or assembly attribute.";
case "Remoting_Config_ProviderNeedsElementName": return "Sink providers must have an element name of 'formatter' or 'provider'.";
case "Remoting_Config_RequiredXmlAttribute": return "'{0}' entries require a '{1}' attribute.";
case "Remoting_Config_ReadFailure": return ".Config file '{0}' cannot be read successfully due to exception '{1}'.";
case "Remoting_Config_NodeMustBeUnique": return "There can be only one '{0}' node in the '{1}' section of a config file.";
case "Remoting_Config_TemplateCannotReferenceTemplate": return "A '{0}' template cannot reference another '{0}' template.";
case "Remoting_Config_TypeAlreadyRedirected": return "Attempt to redirect activation of type '{0}, {1}' which is already redirected.";
case "Remoting_Config_UnknownValue": return "Unknown value {1} was found on the {0} node.";
case "Remoting_Config_UnableToResolveTemplate": return "Cannot resolve '{0}' template reference: '{1}'.";
case "Remoting_Config_VersionPresent": return "Version information is present in the assembly name '{0}' which is not allowed for '{1}' entries.";
case "Remoting_Contexts_BadProperty": return "A property that contributed a bad sink to the chain was found.";
case "Remoting_Contexts_NoProperty": return "A property with the name '{0}' was not found.";
case "Remoting_Contexts_ContextNotFrozenForCallBack": return "Context should be frozen before calling the DoCallBack method.";
case "Remoting_Default": return "Unknown remoting error.";
case "Remoting_HandlerNotRegistered": return "The tracking handler of type '{0}' is not registered with Remoting Services.";
case "Remoting_InvalidMsg": return "Invalid Message Object.";
case "Remoting_InvalidCallingType": return "Attempted to call a method declared on type '{0}' on an object which exposes '{1}'.";
case "Remoting_InvalidRequestedType": return "The server object type cannot be cast to the requested type '{0}'.";
case "Remoting_InternalError": return "Server encountered an internal error. For more information, turn off customErrors in the server's .config file.";
case "Remoting_Lifetime_ILeaseReturn": return "Expected a return object of type ILease, but received '{0}'.";
case "Remoting_Lifetime_InitialStateInitialLeaseTime": return "InitialLeaseTime property can only be set when the lease is in initial state. The state is '{0}'.";
case "Remoting_Lifetime_InitialStateRenewOnCall": return "RenewOnCallTime property can only be set when the lease is in initial state. The state is '{0}'.";
case "Remoting_Lifetime_InitialStateSponsorshipTimeout": return "SponsorshipTimeout property can only be set when the lease is in initial state. State is '{0}'.";
case "Remoting_Lifetime_SetOnce": return "'{0}' can only be set once within an AppDomain.";
case "Remoting_Message_ArgMismatch": return "{2} arguments were passed to '{0}::{1}'. {3} arguments were expected by this method.";
case "Remoting_Message_BadAsyncResult": return "The async result object is null or of an unexpected type.";
case "Remoting_Message_BadType": return "The method was called with a Message of an unexpected type.";
case "Remoting_Message_CoercionFailed": return "The argument type '{0}' cannot be converted into parameter type '{1}'.";
case "Remoting_Message_MissingArgValue": return "Expecting an instance of type '{0}' at pos {1} in the args array.";
case "Remoting_Message_BadSerialization": return "Invalid or malformed serialization information for the message object.";
case "Remoting_NoIdentityEntry": return "No remoting information was found for this object.";
case "Remoting_NotRemotableByReference": return "Trying to create a proxy to an unbound type.";
case "Remoting_NullMessage": return "The method was called with a null message.";
case "Remoting_Proxy_BadType": return "The proxy is of an unsupported type.";
case "Remoting_ResetURI": return "Attempt to reset the URI for an object from '{0}' to '{1}'.";
case "Remoting_ServerObjectNotFound": return "The server object for URI '{0}' is not registered with the remoting infrastructure (it may have been disconnected).";
case "Remoting_SetObjectUriForMarshal__ObjectNeedsToBeLocal": return "SetObjectUriForMarshal method should only be called for MarshalByRefObjects that exist in the current AppDomain.";
case "Remoting_SetObjectUriForMarshal__UriExists": return "SetObjectUriForMarshal method has already been called on this object or the object has already been marshaled.";
case "Remoting_Proxy_BadReturnType": return "Return argument has an invalid type.";
case "Remoting_Proxy_ReturnValueTypeCannotBeNull": return "ByRef value type parameter cannot be null.";
case "Remoting_Proxy_BadReturnTypeForActivation": return "Bad return type for activation call via Invoke: must be of type IConstructionReturnMessage.";
case "Remoting_Proxy_BadTypeForActivation": return "Type mismatch between proxy type '{0}' and activation type '{1}'.";
case "Remoting_Proxy_ExpectedOriginalMessage": return "The message passed to Invoke should be passed to PropagateOutParameters.";
case "Remoting_Proxy_InvalidCall": return "Trying to call proxy while constructor call is in progress.";
case "Remoting_Proxy_InvalidState": return "Channel sink does not exist. Failed to dispatch async call.";
case "Remoting_Proxy_NoChannelSink": return "This remoting proxy has no channel sink which means either the server has no registered server channels that are listening, or this application has no suitable client channel to talk to the server.";
case "Remoting_Proxy_InvalidCallType": return "Only the synchronous call type is supported for messages that are not of type Message.";
case "Remoting_Proxy_WrongContext": return " ExecuteMessage can be called only from the native context of the object.";
case "Remoting_SOAPInteropxsdInvalid": return "Soap Parse error, xsd:type '{0}' invalid {1}";
case "Remoting_SOAPQNameNamespace": return "SoapQName missing a Namespace value '{0}'.";
case "Remoting_ThreadAffinity_InvalidFlag": return "The specified flag '{0}' does not have one of the valid values.";
case "Remoting_TrackingHandlerAlreadyRegistered": return "The handler has already been registered with TrackingServices.";
case "Remoting_URIClash": return "Found two different objects associated with the same URI, '{0}'.";
case "Remoting_URIExists": return "The remoted object already has an associated URI.";
case "Remoting_URIToProxy": return "Trying to associate the URI with a proxy.";
case "Remoting_WellKnown_MustBeMBR": return "Attempted to create well-known object of type '{0}'. Well-known objects must derive from the MarshalByRefObject class.";
case "Remoting_WellKnown_CtorCantMarshal": return "'{0}': A well-known object cannot marshal itself in its constructor, or perform any action that would cause it to be marshaled (such as passing the 'this' pointer as a parameter to a remote method).";
case "Remoting_WellKnown_CantDirectlyConnect": return "Attempt to connect to a server using its object URI: '{0}'. A valid, complete URL must be used.";
case "Remoting_Connect_CantCreateChannelSink": return "Cannot create channel sink to connect to URL '{0}'. An appropriate channel has probably not been registered.";
case "Remoting_UnexpectedNullTP": return "Failed to create a transparent proxy. If a custom RealProxy is being used ensure it sets the proxy type.";
// ; The following remoting exception messages appear in native resources too (mscorrc.rc)
case "Remoting_Disconnected": return "Object '{0}' has been disconnected or does not exist at the server.";
case "Remoting_Message_MethodMissing": return "The method '{0}' was not found on the interface/type '{1}'.";
#endif  // FEATURE_REMOTING

// ; Resources exceptions
// ;
case "Resources_StreamNotValid": return "Stream is not a valid resource file.";
case "ResourceReaderIsClosed": return "ResourceReader is closed.";

// ; RuntimeWrappedException
case "RuntimeWrappedException": return "An object that does not derive from System.Exception has been wrapped in a RuntimeWrappedException.";

// ; UnauthorizedAccessException
case "UnauthorizedAccess_MemStreamBuffer": return "MemoryStream's internal buffer cannot be accessed.";
case "UnauthorizedAccess_IODenied_Path": return "Access to the path '{0}' is denied.";
case "UnauthorizedAccess_IODenied_NoPathName": return "Access to the path is denied.";
case "UnauthorizedAccess_RegistryKeyGeneric_Key": return "Access to the registry key '{0}' is denied.";
case "UnauthorizedAccess_RegistryNoWrite": return "Cannot write to the registry key.";
case "UnauthorizedAccess_SystemDomain": return "Cannot execute an assembly in the system domain.";

// ;
// ; Security exceptions
// ;

// ;SecurityException
// ; These still appear in bcl.small but should go away eventually
case "Security_Generic": return "Request for the permission of type '{0}' failed.";
case "Security_GenericNoType": return "Request failed.";
case "Security_NoAPTCA": return "That assembly does not allow partially trusted callers.";
case "Security_RegistryPermission": return "Requested registry access is not allowed.";
case "Security_MustRevertOverride": return "Stack walk modifier must be reverted before another modification of the same type can be performed.";
#if FEATURE_CAS_POLICY
case "Security_CannotGenerateHash": return "Hash for the assembly cannot be generated.";
case "Security_CannotGetRawData": return "Assembly bytes could not be retrieved.";
case "Security_PrincipalPermission": return "Request for principal permission failed.";
case "Security_Action": return "The action that failed was:";
case "Security_TypeFirstPermThatFailed": return "The type of the first permission that failed was:";
case "Security_FirstPermThatFailed": return "The first permission that failed was:";
case "Security_Demanded": return "The demand was for:";
case "Security_GrantedSet": return "The granted set of the failing assembly was:";
case "Security_RefusedSet": return "The refused set of the failing assembly was:";
case "Security_Denied": return "The denied permissions were:";
case "Security_PermitOnly": return "The only permitted permissions were:";
case "Security_Assembly": return "The assembly or AppDomain that failed was:";
case "Security_Method": return "The method that caused the failure was:";
case "Security_Zone": return "The Zone of the assembly that failed was:";
case "Security_Url": return "The Url of the assembly that failed was:";
case "Security_AnonymouslyHostedDynamicMethodCheckFailed": return "The demand failed due to the code access security information captured during the creation of an anonymously hosted dynamic method. In order for this operation to succeed, ensure that the demand would have succeeded at the time the method was created. See http://go.microsoft.com/fwlink/?LinkId": return "288746 for more information.";
#endif  // FEATURE_CAS_POLICY

// ;
// ; HostProtection exceptions
// ;

case "HostProtection_HostProtection": return "Attempted to perform an operation that was forbidden by the CLR host.";
case "HostProtection_ProtectedResources": return "The protected resources (only available with full trust) were:";
case "HostProtection_DemandedResources": return "The demanded resources were:";

// ;
// ; IO exceptions
// ;

// ; EOFException
case "IO.EOF_ReadBeyondEOF": return "Unable to read beyond the end of the stream.";

// ; FileNotFoundException
case "IO.FileNotFound": return "Unable to find the specified file.";
case "IO.FileNotFound_FileName": return "Could not find file '{0}'.";
case "IO.FileName_Name": return "File name: '{0}'";
case "IO.FileLoad": return "Could not load the specified file.";

// ; IOException
case "IO.IO_AlreadyExists_Name": return "Cannot create \"{0}\" because a file or directory with the same name already exists.";
case "IO.IO_BindHandleFailed": return "BindHandle for ThreadPool failed on this handle.";
case "IO.IO_FileExists_Name": return "The file '{0}' already exists.";
case "IO.IO_FileStreamHandlePosition": return "The OS handle's position is not what FileStream expected. Do not use a handle simultaneously in one FileStream and in Win32 code or another FileStream. This may cause data loss.";
case "IO.IO_FileTooLong2GB": return "The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size.";
case "IO.IO_FileTooLongOrHandleNotSync": return "IO operation will not work. Most likely the file will become too long or the handle was not opened to support synchronous IO operations.";
case "IO.IO_FixedCapacity": return "Unable to expand length of this stream beyond its capacity.";
case "IO.IO_InvalidStringLen_Len": return "BinaryReader encountered an invalid string length of {0} characters.";
case "IO.IO_NoConsole": return "There is no console.";
case "IO.IO_NoPermissionToDirectoryName": return "<Path discovery permission to the specified directory was denied.>";
case "IO.IO_SeekBeforeBegin": return "An attempt was made to move the position before the beginning of the stream.";
case "IO.IO_SeekAppendOverwrite": return "Unable seek backward to overwrite data that previously existed in a file opened in Append mode.";
case "IO.IO_SetLengthAppendTruncate": return "Unable to truncate data that previously existed in a file opened in Append mode.";
case "IO.IO_SharingViolation_File": return "The process cannot access the file '{0}' because it is being used by another process.";
case "IO.IO_SharingViolation_NoFileName": return "The process cannot access the file because it is being used by another process.";
case "IO.IO_StreamTooLong": return "Stream was too long.";
case "IO.IO_CannotCreateDirectory": return "The specified directory '{0}' cannot be created.";
case "IO.IO_SourceDestMustBeDifferent": return "Source and destination path must be different.";
case "IO.IO_SourceDestMustHaveSameRoot": return "Source and destination path must have identical roots. Move will not work across volumes.";

// ; DirectoryNotFoundException
case "IO.DriveNotFound_Drive": return "Could not find the drive '{0}'. The drive might not be ready or might not be mapped.";
case "IO.PathNotFound_Path": return "Could not find a part of the path '{0}'.";
case "IO.PathNotFound_NoPathName": return "Could not find a part of the path.";

// ; PathTooLongException
case "IO.PathTooLong": return "The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.";

#if FEATURE_CORECLR
// ; SecurityException
case "FileSecurityState_OperationNotPermitted": return "File operation not permitted. Access to path '{0}' is denied.";
#endif

// ; PrivilegeNotHeldException
case "PrivilegeNotHeld_Default": return "The process does not possess some privilege required for this operation.";
case "PrivilegeNotHeld_Named": return "The process does not possess the '{0}' privilege which is required for this operation.";

// ; General strings used in the IO package
case "IO_UnknownFileName": return "[Unknown]";
case "IO_StreamWriterBufferedDataLost": return "A StreamWriter was not closed and all buffered data within that StreamWriter was not flushed to the underlying stream.  (This was detected when the StreamWriter was finalized with data in its buffer.)  A portion of the data was lost.  Consider one of calling Close(), Flush(), setting the StreamWriter's AutoFlush property to true, or allocating the StreamWriter with a \"using\" statement.  Stream type: {0}\r\nFile name: {1}\r\nAllocated from:\r\n {2}";
case "IO_StreamWriterBufferedDataLostCaptureAllocatedFromCallstackNotEnabled": return "callstack information is not captured by default for performance reasons. Please enable captureAllocatedCallStack config switch for streamWriterBufferedDataLost MDA (refer to MSDN MDA documentation for how to do this).  ";

// ;
// ; Serialization Exceptions
// ;
#if FEATURE_SERIALIZATION
// ; SerializationException
case "Serialization_NoID": return "Object has never been assigned an objectID.";
case "Serialization_UnknownMemberInfo": return "Only FieldInfo, PropertyInfo, and SerializationMemberInfo are recognized.";
case "Serialization_UnableToFixup": return "Cannot perform fixup.";
case "Serialization_NoType": return "Object does not specify a type.";
case "Serialization_ValueTypeFixup": return "ValueType fixup on Arrays is not implemented.";
case "Serialization_PartialValueTypeFixup": return "Fixing up a partially available ValueType chain is not implemented.";
case "Serialization_InvalidData": return "An error occurred while deserializing the object.  The serialized data is corrupt.";
case "Serialization_InvalidID": return "Object specifies an invalid ID.";
case "Serialization_InvalidPtrValue": return "An IntPtr or UIntPtr with an eight byte value cannot be deserialized on a machine with a four byte word size.";
case "Serialization_DuplicateSelector": return "Selector is already on the list of checked selectors.";
case "Serialization_MemberTypeNotRecognized": return "Unknown member type.";
case "Serialization_NoBaseType": return "Object does not specify a base type.";
case "Serialization_ArrayNoLength": return "Array does not specify a length.";
case "Serialization_CannotGetType": return "Cannot get the type '{0}'.";
case "Serialization_AssemblyNotFound": return "Unable to find assembly '{0}'.";
case "Serialization_ArrayInvalidLength": return "Array specifies an invalid length.";
case "Serialization_MalformedArray": return "The array information in the stream is invalid.";
case "Serialization_InsufficientState": return "Insufficient state to return the real object.";
case "Serialization_InvalidFieldState": return "Object fields may not be properly initialized.";
case "Serialization_MissField": return "Field {0} is missing.";
case "Serialization_MultipleMembers": return "Cannot resolve multiple members with the same name.";
case "Serialization_NullSignature": return "The method signature cannot be null.";
case "Serialization_ObjectUsedBeforeDeserCallback": return "An object was used before its deserialization callback ran, which may break higher-level consistency guarantees in the application.";
case "Serialization_UnknownMember": return "Cannot get the member '{0}'.";
case "Serialization_RegisterTwice": return "An object cannot be registered twice.";
case "Serialization_IdTooSmall": return "Object IDs must be greater than zero.";
case "Serialization_NotFound": return "Member '{0}' was not found.";
case "Serialization_InsufficientDeserializationState": return "Insufficient state to deserialize the object. Missing field '{0}'. More information is needed.";
case "Serialization_UnableToFindModule": return "The given module {0} cannot be found within the assembly {1}.";
case "Serialization_TooManyReferences": return "The implementation of the IObjectReference interface returns too many nested references to other objects that implement IObjectReference.";
case "Serialization_NotISer": return "The given object does not implement the ISerializable interface.";
case "Serialization_InvalidOnDeser": return "OnDeserialization method was called while the object was not being deserialized.";
case "Serialization_MissingKeys": return "The Keys for this Hashtable are missing.";
case "Serialization_MissingKeyValuePairs": return "The KeyValuePairs for this Dictionary are missing.";
case "Serialization_MissingValues": return "The values for this dictionary are missing.";
case "Serialization_NullKey": return "One of the serialized keys is null.";
case "Serialization_KeyValueDifferentSizes": return "The keys and values arrays have different sizes.";
case "Serialization_SurrogateCycleInArgument": return "Selector contained a cycle.";
case "Serialization_SurrogateCycle": return "Adding selector will introduce a cycle.";
case "Serialization_NeverSeen": return "A fixup is registered to the object with ID {0}, but the object does not appear in the graph.";
case "Serialization_IORIncomplete": return "The object with ID {0} implements the IObjectReference interface for which all dependencies cannot be resolved. The likely cause is two instances of IObjectReference that have a mutual dependency on each other.";
case "Serialization_NotCyclicallyReferenceableSurrogate": return "{0}.SetObjectData returns a value that is neither null nor equal to the first parameter. Such Surrogates cannot be part of cyclical reference.";
case "Serialization_ObjectNotSupplied": return "The object with ID {0} was referenced in a fixup but does not exist.";
case "Serialization_TooManyElements": return "The internal array cannot expand to greater than Int32.MaxValue elements.";
case "Serialization_SameNameTwice": return "Cannot add the same member twice to a SerializationInfo object.";
case "Serialization_InvalidType": return "Only system-provided types can be passed to the GetUninitializedObject method. '{0}' is not a valid instance of a type.";
case "Serialization_MissingObject": return "The object with ID {0} was referenced in a fixup but has not been registered.";
case "Serialization_InvalidFixupType": return "A member fixup was registered for an object which implements ISerializable or has a surrogate. In this situation, a delayed fixup must be used.";
case "Serialization_InvalidFixupDiscovered": return "A fixup on an object implementing ISerializable or having a surrogate was discovered for an object which does not have a SerializationInfo available.";
case "Serialization_InvalidFormat": return "The input stream is not a valid binary format. The starting contents (in bytes) are: {0} ...";
case "Serialization_ParentChildIdentical": return "The ID of the containing object cannot be the same as the object ID.";
case "Serialization_IncorrectNumberOfFixups": return "The ObjectManager found an invalid number of fixups. This usually indicates a problem in the Formatter.";
case "Serialization_BadParameterInfo": return "Non existent ParameterInfo. Position bigger than member's parameters length.";
case "Serialization_NoParameterInfo": return "Serialized member does not have a ParameterInfo.";
case "Serialization_StringBuilderMaxCapacity": return "The serialized MaxCapacity property of StringBuilder must be positive and greater than or equal to the String length.";
case "Serialization_StringBuilderCapacity": return "The serialized Capacity property of StringBuilder must be positive, less than or equal to MaxCapacity and greater than or equal to the String length.";
case "Serialization_InvalidDelegateType": return "Cannot serialize delegates over unmanaged function pointers, dynamic methods or methods outside the delegate creator's assembly.";
case "Serialization_OptionalFieldVersionValue": return "Version value must be positive.";
case "Serialization_MissingDateTimeData": return "Invalid serialized DateTime data. Unable to find 'ticks' or 'dateData'.";
case "Serialization_DateTimeTicksOutOfRange": return "Invalid serialized DateTime data. Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks. ";
// ; The following serialization exception messages appear in native resources too (mscorrc.rc)
case "Serialization_NonSerType": return "Type '{0}' in Assembly '{1}' is not marked as serializable.";
case "Serialization_ConstructorNotFound": return "The constructor to deserialize an object of type '{0}' was not found.";

// ; SerializationException used by Formatters
case "Serialization_ArrayType": return "Invalid array type '{0}'.";
case "Serialization_ArrayTypeObject": return "Array element type is Object, 'dt' attribute is null.";
case "Serialization_Assembly": return "No assembly information is available for object on the wire, '{0}'.";
case "Serialization_AssemblyId": return "No assembly ID for object type '{0}'.";
case "Serialization_BinaryHeader": return "Binary stream '{0}' does not contain a valid BinaryHeader. Possible causes are invalid stream or object version change between serialization and deserialization.";
case "Serialization_CrossAppDomainError": return "Cross-AppDomain BinaryFormatter error; expected '{0}' but received '{1}'.";
case "Serialization_CorruptedStream": return "Invalid BinaryFormatter stream.";
case "Serialization_HeaderReflection": return "Header reflection error: number of value members: {0}.";
case "Serialization_ISerializableTypes": return "Types not available for ISerializable object '{0}'.";
case "Serialization_ISerializableMemberInfo": return "MemberInfo requested for ISerializable type.";
case "Serialization_MBRAsMBV": return "Type {0} must be marshaled by reference in this context.";
case "Serialization_Map": return "No map for object '{0}'.";
case "Serialization_MemberInfo": return "MemberInfo cannot be obtained for ISerialized Object '{0}'.";
case "Serialization_Method": return "Invalid MethodCall or MethodReturn stream format.";
case "Serialization_MissingMember": return "Member '{0}' in class '{1}' is not present in the serialized stream and is not marked with {2}.";
case "Serialization_NoMemberInfo": return "No MemberInfo for Object {0}.";
case "Serialization_ObjNoID": return "Object {0} has never been assigned an objectID.";
case "Serialization_ObjectTypeEnum": return "Invalid ObjectTypeEnum {0}.";
case "Serialization_ParseError": return "Parse error. Current element is not compatible with the next element, {0}.";
case "Serialization_SerMemberInfo": return "MemberInfo type {0} cannot be serialized.";
case "Serialization_Stream": return "Attempting to deserialize an empty stream.";
case "Serialization_StreamEnd": return "End of Stream encountered before parsing was completed.";
case "Serialization_TopObject": return "No top object.";
case "Serialization_TopObjectInstantiate": return "Top object cannot be instantiated for element '{0}'.";
case "Serialization_TypeCode": return "Invalid type code in stream '{0}'.";
case "Serialization_TypeExpected": return "Invalid expected type.";
case "Serialization_TypeMissing": return "Type is missing for member of type Object '{0}'.";
case "Serialization_TypeRead": return "Invalid read type request '{0}'.";
case "Serialization_TypeSecurity": return "Type {0} and the types derived from it (such as {1}) are not permitted to be deserialized at this security level.";
case "Serialization_TypeWrite": return "Invalid write type request '{0}'.";
case "Serialization_XMLElement": return "Invalid element '{0}'.";
case "Serialization_Security": return "Because of security restrictions, the type {0} cannot be accessed.";
case "Serialization_TypeLoadFailure": return "Unable to load type {0} required for deserialization.";
case "Serialization_RequireFullTrust": return "A type '{0}' that is defined in a partially trusted assembly cannot be type forwarded from an assembly with a different Public Key Token or without a public key token. To fix this, please either turn on unsafeTypeForwarding flag in the configuration file or remove the TypeForwardedFrom attribute.";
// ; The following serialization exception messages appear in native resources too (mscorrc.rc)
case "Serialization_TypeResolved": return "Type is not resolved for member '{0}'.";
case "Serialization_MemberOutOfRange": return "The deserialized value of the member \"{0}\" in the class \"{1}\" is out of range.";
#endif  // FEATURE_SERIALIZATION

// ;
// ; StringBuilder Exceptions
// ;
case "Arg_LongerThanSrcString": return "Source string was not long enough. Check sourceIndex and count.";


// ;
// ; System.Threading
// ;

// ;
// ; Thread Exceptions
// ;
case "ThreadState_NoAbortRequested": return "Unable to reset abort because no abort was requested.";
case "Threading.WaitHandleTooManyPosts": return "The WaitHandle cannot be signaled because it would exceed its maximum count.";
// ;
// ; WaitHandleCannotBeOpenedException
// ;
case "Threading.WaitHandleCannotBeOpenedException": return "No handle of the given name exists.";
case "Threading.WaitHandleCannotBeOpenedException_InvalidHandle": return "A WaitHandle with system-wide name '{0}' cannot be created. A WaitHandle of a different type might have the same name.";

// ;
// ; AbandonedMutexException
// ;
case "Threading.AbandonedMutexException": return "The wait completed due to an abandoned mutex.";

// ; AggregateException
case "AggregateException_ctor_DefaultMessage": return "One or more errors occurred.";
case "AggregateException_ctor_InnerExceptionNull": return "An element of innerExceptions was null.";
case "AggregateException_DeserializationFailure": return "The serialization stream contains no inner exceptions.";
case "AggregateException_ToString": return "{0}{1}---> (Inner Exception #{2}) {3}{4}{5}";

// ; Cancellation
case "CancellationToken_CreateLinkedToken_TokensIsEmpty": return "No tokens were supplied.";
case "CancellationTokenSource_Disposed": return "The CancellationTokenSource has been disposed.";
case "CancellationToken_SourceDisposed": return "The CancellationTokenSource associated with this CancellationToken has been disposed.";

// ; Exceptions shared by all concurrent collection
case "ConcurrentCollection_SyncRoot_NotSupported": return "The SyncRoot property may not be used for the synchronization of concurrent collections.";

// ; Exceptions shared by ConcurrentStack and ConcurrentQueue
case "ConcurrentStackQueue_OnDeserialization_NoData": return "The serialization stream contains no elements.";

// ; ConcurrentStack<T>
case "ConcurrentStack_PushPopRange_StartOutOfRange": return "The startIndex argument must be greater than or equal to zero.";
case "ConcurrentStack_PushPopRange_CountOutOfRange": return "The count argument must be greater than or equal to zero.";
case "ConcurrentStack_PushPopRange_InvalidCount": return "The sum of the startIndex and count arguments must be less than or equal to the collection's Count.";

// ; ConcurrentDictionary<TKey, TValue>
case "ConcurrentDictionary_ItemKeyIsNull": return "TKey is a reference type and item.Key is null.";
case "ConcurrentDictionary_SourceContainsDuplicateKeys": return "The source argument contains duplicate keys.";
case "ConcurrentDictionary_IndexIsNegative": return "The index argument is less than zero.";
case "ConcurrentDictionary_ConcurrencyLevelMustBePositive": return "The concurrencyLevel argument must be positive.";
case "ConcurrentDictionary_CapacityMustNotBeNegative": return "The capacity argument must be greater than or equal to zero.";
case "ConcurrentDictionary_ArrayNotLargeEnough": return "The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.";
case "ConcurrentDictionary_ArrayIncorrectType": return "The array is multidimensional, or the type parameter for the set cannot be cast automatically to the type of the destination array.";
case "ConcurrentDictionary_KeyAlreadyExisted": return "The key already existed in the dictionary.";
case "ConcurrentDictionary_TypeOfKeyIncorrect": return "The key was of an incorrect type for this dictionary.";
case "ConcurrentDictionary_TypeOfValueIncorrect": return "The value was of an incorrect type for this dictionary.";

// ; Partitioner
case "Partitioner_DynamicPartitionsNotSupported": return "Dynamic partitions are not supported by this partitioner.";

// ; OrderablePartitioner
case "OrderablePartitioner_GetPartitions_WrongNumberOfPartitions": return "GetPartitions returned an incorrect number of partitions.";

// ; PartitionerStatic
case "PartitionerStatic_CurrentCalledBeforeMoveNext": return "MoveNext must be called at least once before calling Current.";
case "PartitionerStatic_CanNotCallGetEnumeratorAfterSourceHasBeenDisposed": return "Can not call GetEnumerator on partitions after the source enumerable is disposed";

// ; CDSCollectionETWBCLProvider events
case "event_ConcurrentStack_FastPushFailed": return "Push to ConcurrentStack spun {0} time(s).";
case "event_ConcurrentStack_FastPopFailed": return "Pop from ConcurrentStack spun {0} time(s).";
case "event_ConcurrentDictionary_AcquiringAllLocks": return "ConcurrentDictionary acquiring all locks on {0} bucket(s).";
case "event_ConcurrentBag_TryTakeSteals": return "ConcurrentBag stealing in TryTake.";
case "event_ConcurrentBag_TryPeekSteals": return "ConcurrentBag stealing in TryPeek.";

// ; CountdownEvent
case "CountdownEvent_Decrement_BelowZero": return "Invalid attempt made to decrement the event's count below zero.";
case "CountdownEvent_Increment_AlreadyZero": return "The event is already signaled and cannot be incremented.";
case "CountdownEvent_Increment_AlreadyMax": return "The increment operation would cause the CurrentCount to overflow.";

// ; Parallel
case "Parallel_Invoke_ActionNull": return "One of the actions was null.";
case "Parallel_ForEach_OrderedPartitionerKeysNotNormalized": return "This method requires the use of an OrderedPartitioner with the KeysNormalized property set to true.";
case "Parallel_ForEach_PartitionerNotDynamic": return "The Partitioner used here must support dynamic partitioning.";
case "Parallel_ForEach_PartitionerReturnedNull": return "The Partitioner used here returned a null partitioner source.";
case "Parallel_ForEach_NullEnumerator": return "The Partitioner source returned a null enumerator.";

// ; SemaphyoreFullException
case "Threading_SemaphoreFullException": return "Adding the specified count to the semaphore would cause it to exceed its maximum count.";

// ; Lazy
case "Lazy_ctor_ValueSelectorNull": return "The valueSelector argument is null.";
case "Lazy_ctor_InfoNull": return "The info argument is null.";
case "Lazy_ctor_deserialization_ValueInvalid": return "The Value cannot be null.";
case "Lazy_ctor_ModeInvalid": return "The mode argument specifies an invalid value.";
case "Lazy_CreateValue_NoParameterlessCtorForT": return "The lazily-initialized type does not have a public, parameterless constructor.";
case "Lazy_StaticInit_InvalidOperation": return "ValueFactory returned null.";
case "Lazy_Value_RecursiveCallsToValue": return "ValueFactory attempted to access the Value property of this instance.";
case "Lazy_ToString_ValueNotCreated": return "Value is not created.";


// ;ThreadLocal
case "ThreadLocal_Value_RecursiveCallsToValue": return "ValueFactory attempted to access the Value property of this instance.";
case "ThreadLocal_Disposed": return "The ThreadLocal object has been disposed.";
case "ThreadLocal_ValuesNotAvailable": return "The ThreadLocal object is not tracking values. To use the Values property, use a ThreadLocal constructor that accepts the trackAllValues parameter and set the parameter to true.";

// ; SemaphoreSlim
case "SemaphoreSlim_ctor_InitialCountWrong": return "The initialCount argument must be non-negative and less than or equal to the maximumCount.";
case "SemaphoreSlim_ctor_MaxCountWrong": return "The maximumCount argument must be a positive number. If a maximum is not required, use the constructor without a maxCount parameter.";
case "SemaphoreSlim_Wait_TimeoutWrong": return "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.";
case "SemaphoreSlim_Release_CountWrong": return "The releaseCount argument must be greater than zero.";
case "SemaphoreSlim_Disposed": return "The semaphore has been disposed.";

// ; ManualResetEventSlim
case "ManualResetEventSlim_ctor_SpinCountOutOfRange": return "The spinCount argument must be in the range 0 to {0}, inclusive.";
case "ManualResetEventSlim_ctor_TooManyWaiters": return "There are too many threads currently waiting on the event. A maximum of {0} waiting threads are supported.";
case "ManualResetEventSlim_Disposed": return "The event has been disposed.";

// ; SpinLock
case "SpinLock_TryEnter_ArgumentOutOfRange": return "The timeout must be a value between -1 and Int32.MaxValue, inclusive.";
case "SpinLock_TryEnter_LockRecursionException": return "The calling thread already holds the lock.";
case "SpinLock_TryReliableEnter_ArgumentException": return "The tookLock argument must be set to false before calling this method.";
case "SpinLock_Exit_SynchronizationLockException": return "The calling thread does not hold the lock.";
case "SpinLock_IsHeldByCurrentThread": return "Thread tracking is disabled.";

// ; SpinWait
case "SpinWait_SpinUntil_TimeoutWrong": return "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.";
case "SpinWait_SpinUntil_ArgumentNull": return "The condition argument is null.";

// ; CdsSyncEtwBCLProvider events
case "event_SpinLock_FastPathFailed": return "SpinLock beginning to spin.";
case "event_SpinWait_NextSpinWillYield": return "Next spin will yield.";
case "event_Barrier_PhaseFinished": return "Barrier finishing phase {1}.";

// ;
// ; System.Threading.Tasks
// ;

// ; AsyncMethodBuilder
case "AsyncMethodBuilder_InstanceNotInitialized": return "The builder was not properly initialized.";

// ; TaskAwaiter and YieldAwaitable
case "AwaitableAwaiter_InstanceNotInitialized": return "The awaitable or awaiter was not properly initialized.";
case "TaskAwaiter_TaskNotCompleted": return "The awaited task has not yet completed.";

// ; Task<T>
case "TaskT_SetException_HasAnInitializer": return "A task's Exception may only be set directly if the task was created without a function.";
case "TaskT_TransitionToFinal_AlreadyCompleted": return "An attempt was made to transition a task to a final state when it had already completed.";
case "TaskT_ctor_SelfReplicating": return "It is invalid to specify TaskCreationOptions.SelfReplicating for a Task<TResult>.";
case "TaskT_DebuggerNoResult": return "{Not yet computed}";

// ; Task
case "Task_ctor_LRandSR": return "(Internal)An attempt was made to create a LongRunning SelfReplicating task.";
case "Task_ThrowIfDisposed": return "The task has been disposed.";
case "Task_Dispose_NotCompleted": return "A task may only be disposed if it is in a completion state (RanToCompletion, Faulted or Canceled).";
case "Task_Start_Promise": return "Start may not be called on a promise-style task.";
case "Task_Start_AlreadyStarted": return "Start may not be called on a task that was already started.";
case "Task_Start_TaskCompleted": return "Start may not be called on a task that has completed.";
case "Task_Start_ContinuationTask": return "Start may not be called on a continuation task.";
case "Task_RunSynchronously_AlreadyStarted": return "RunSynchronously may not be called on a task that was already started.";
case "Task_RunSynchronously_TaskCompleted": return "RunSynchronously may not be called on a task that has already completed.";
case "Task_RunSynchronously_Promise": return "RunSynchronously may not be called on a task not bound to a delegate, such as the task returned from an asynchronous method.";
case "Task_RunSynchronously_Continuation": return "RunSynchronously may not be called on a continuation task.";
case "Task_ContinueWith_NotOnAnything": return "The specified TaskContinuationOptions excluded all continuation kinds.";
case "Task_ContinueWith_ESandLR": return "The specified TaskContinuationOptions combined LongRunning and ExecuteSynchronously.  Synchronous continuations should not be long running.";
case "Task_MultiTaskContinuation_NullTask": return "The tasks argument included a null value.";
case "Task_MultiTaskContinuation_FireOptions": return "It is invalid to exclude specific continuation kinds for continuations off of multiple tasks.";
case "Task_MultiTaskContinuation_EmptyTaskList": return "The tasks argument contains no tasks.";
case "Task_FromAsync_TaskManagerShutDown": return "FromAsync was called with a TaskManager that had already shut down.";
case "Task_FromAsync_SelfReplicating": return "It is invalid to specify TaskCreationOptions.SelfReplicating in calls to FromAsync.";
case "Task_FromAsync_LongRunning": return "It is invalid to specify TaskCreationOptions.LongRunning in calls to FromAsync.";
case "Task_FromAsync_PreferFairness": return "It is invalid to specify TaskCreationOptions.PreferFairness in calls to FromAsync.";
case "Task_WaitMulti_NullTask": return "The tasks array included at least one null element.";
case "Task_Delay_InvalidMillisecondsDelay": return "The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.";
case "Task_Delay_InvalidDelay": return "The value needs to translate in milliseconds to -1 (signifying an infinite timeout), 0 or a positive integer less than or equal to Int32.MaxValue.";

// ; TaskCanceledException
case "TaskCanceledException_ctor_DefaultMessage": return "A task was canceled.";

// ;TaskCompletionSource<T>
case "TaskCompletionSourceT_TrySetException_NullException": return "The exceptions collection included at least one null element.";
case "TaskCompletionSourceT_TrySetException_NoExceptions": return "The exceptions collection was empty.";

// ;TaskExceptionHolder
case "TaskExceptionHolder_UnknownExceptionType": return "(Internal)Expected an Exception or an IEnumerable<Exception>";
case "TaskExceptionHolder_UnhandledException": return "A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.";

// ; TaskScheduler
case "TaskScheduler_ExecuteTask_TaskAlreadyExecuted": return "ExecuteTask may not be called for a task which was already executed.";
case "TaskScheduler_ExecuteTask_WrongTaskScheduler": return "ExecuteTask may not be called for a task which was previously queued to a different TaskScheduler.";
case "TaskScheduler_InconsistentStateAfterTryExecuteTaskInline": return "The TryExecuteTaskInline call to the underlying scheduler succeeded, but the task body was not invoked.";
case "TaskScheduler_FromCurrentSynchronizationContext_NoCurrent": return "The current SynchronizationContext may not be used as a TaskScheduler.";

// ; TaskSchedulerException
case "TaskSchedulerException_ctor_DefaultMessage": return "An exception was thrown by a TaskScheduler.";

// ;
// ; ParallelState ( used in Parallel.For(), Parallel.ForEach() )
case "ParallelState_Break_InvalidOperationException_BreakAfterStop": return "Break was called after Stop was called.";
case "ParallelState_Stop_InvalidOperationException_StopAfterBreak": return "Stop was called after Break was called.";
case "ParallelState_NotSupportedException_UnsupportedMethod": return "This method is not supported.";

// ;
// ; TPLETWProvider events
case "event_ParallelLoopBegin": return "Beginning {3} loop {2} from Task {1}.";
case "event_ParallelLoopEnd": return "Ending loop {2} after {3} iterations.";
case "event_ParallelInvokeBegin": return "Beginning ParallelInvoke {2} from Task {1} for {4} actions.";
case "event_ParallelInvokeEnd": return "Ending ParallelInvoke {2}.";
case "event_ParallelFork": return "Task {1} entering fork/join {2}.";
case "event_ParallelJoin": return "Task {1} leaving fork/join {2}.";
case "event_TaskScheduled": return "Task {2} scheduled to TaskScheduler {0}.";
case "event_TaskStarted": return "Task {2} executing.";
case "event_TaskCompleted": return "Task {2} completed.";
case "event_TaskWaitBegin": return "Beginning wait ({3}) on Task {2}.";
case "event_TaskWaitEnd": return "Ending wait on Task {2}.";


// ;
// ; Weak Reference Exception
// ;
case "WeakReference_NoLongerValid": return "The weak reference is no longer valid.";


// ;
// ; Interop Exceptions
// ;
case "Interop.COM_TypeMismatch": return "Type mismatch between source and destination types.";
case "Interop_Marshal_Unmappable_Char": return "Cannot marshal: Encountered unmappable character.";

#if FEATURE_COMINTEROP_WINRT_DESKTOP_HOST
case "WinRTHostDomainName": return "Windows Runtime Object Host Domain for '{0}'";
#endif

// ;
// ; Loader Exceptions
// ;
case "Loader_InvalidPath": return "Relative path must be a string that contains the substring, \"..\", or does not contain a root directory.";
case "Loader_Name": return "Name:";
case "Loader_NoContextPolicies": return "There are no context policies.";
case "Loader_ContextPolicies": return "Context Policies:";

// ;
// ; AppDomain Exceptions
case "AppDomain_RequireApplicationName": return "ApplicationName must be set before the DynamicBase can be set.";
case "AppDomain_AppBaseNotSet": return "The ApplicationBase must be set before retrieving this property.";

#if FEATURE_HOST_ASSEMBLY_RESOLVER
case "AppDomain_BindingModelIsLocked": return "Binding model is already locked for the AppDomain and cannot be reset.";
case "Argument_CustomAssemblyLoadContextRequestedNameMismatch": return "Resolved assembly's simple name should be the same as of the requested assembly.";
#endif // FEATURE_HOST_ASSEMBLY_RESOLVER
// ;
// ; XMLSyntaxExceptions
case "XMLSyntax_UnexpectedEndOfFile": return "Unexpected end of file.";
case "XMLSyntax_ExpectedCloseBracket": return "Expected > character.";
case "XMLSyntax_ExpectedSlashOrString": return "Expected / character or string.";
case "XMLSyntax_UnexpectedCloseBracket": return "Unexpected > character.";
case "XMLSyntax_SyntaxError": return "Invalid syntax on line {0}.";
case "XMLSyntax_SyntaxErrorEx": return "Invalid syntax on line {0} - '{1}'.";
case "XMLSyntax_InvalidSyntax": return "Invalid syntax.";
case "XML_Syntax_InvalidSyntaxInFile": return "Invalid XML in file '{0}' near element '{1}'.";
case "XMLSyntax_InvalidSyntaxSatAssemTag": return "Invalid XML in file \"{0}\" near element \"{1}\". The <satelliteassemblies> section only supports <assembly> tags.";
case "XMLSyntax_InvalidSyntaxSatAssemTagBadAttr": return "Invalid XML in file \"{0}\" near \"{1}\" and \"{2}\". In the <satelliteassemblies> section, the <assembly> tag must have exactly 1 attribute called 'name', whose value is a fully-qualified assembly name.";
case "XMLSyntax_InvalidSyntaxSatAssemTagNoAttr": return "Invalid XML in file \"{0}\". In the <satelliteassemblies> section, the <assembly> tag must have exactly 1 attribute called 'name', whose value is a fully-qualified assembly name.";

// ; CodeGroup
#if FEATURE_CAS_POLICY
case "NetCodeGroup_PermissionSet": return "Same site Web";
case "MergeLogic_Union": return "Union";
case "MergeLogic_FirstMatch": return "First Match";
case "FileCodeGroup_PermissionSet": return "Same directory FileIO - '{0}'";
#endif // FEATURE_CAS_POLICY

// ; MembershipConditions
case "StrongName_ToString": return "StrongName - {0}{1}{2}";
case "StrongName_Name": return "name = {0}";
case "StrongName_Version": return "version = {0}";
case "Site_ToString": return "Site";
case "Publisher_ToString": return "Publisher";
case "Hash_ToString": return "Hash - {0} = {1}";
case "ApplicationDirectory_ToString": return "ApplicationDirectory";
case "Zone_ToString": return "Zone - {0}";
case "All_ToString": return "All code";
case "Url_ToString": return "Url";
case "GAC_ToString": return "GAC";
#if FEATURE_CAS_POLICY
case "Site_ToStringArg": return "Site - {0}";
case "Publisher_ToStringArg": return "Publisher - {0}";
case "Url_ToStringArg": return "Url - {0}";
#endif // FEATURE_CAS_POLICY


// ; Interop non exception strings.
case "TypeLibConverter_ImportedTypeLibProductName": return "Assembly imported from type library '{0}'.";

// ;
// ; begin System.TimeZoneInfo ArgumentException's
// ;
case "Argument_AdjustmentRulesNoNulls": return "The AdjustmentRule array cannot contain null elements.";
case "Argument_AdjustmentRulesOutOfOrder": return "The elements of the AdjustmentRule array must be in chronological order and must not overlap.";
case "Argument_AdjustmentRulesAmbiguousOverlap": return "The elements of the AdjustmentRule array must not contain ambiguous time periods that extend beyond the DateStart or DateEnd properties of the element.";
case "Argument_AdjustmentRulesrDaylightSavingTimeOverlap": return "The elements of the AdjustmentRule array must not contain Daylight Saving Time periods that overlap adjacent elements in such a way as to cause invalid or ambiguous time periods.";
case "Argument_AdjustmentRulesrDaylightSavingTimeOverlapNonRuleRange": return "The elements of the AdjustmentRule array must not contain Daylight Saving Time periods that overlap the DateStart or DateEnd properties in such a way as to cause invalid or ambiguous time periods.";
case "Argument_AdjustmentRulesInvalidOverlap": return "The elements of the AdjustmentRule array must not contain invalid time periods that extend beyond the DateStart or DateEnd properties of the element. ";
case "Argument_ConvertMismatch": return "The conversion could not be completed because the supplied DateTime did not have the Kind property set correctly.  For example, when the Kind property is DateTimeKind.Local, the source time zone must be TimeZoneInfo.Local.";
case "Argument_DateTimeHasTimeOfDay": return "The supplied DateTime includes a TimeOfDay setting.   This is not supported.";
case "Argument_DateTimeIsInvalid": return "The supplied DateTime represents an invalid time.  For example, when the clock is adjusted forward, any time in the period that is skipped is invalid.";
case "Argument_DateTimeIsNotAmbiguous": return "The supplied DateTime is not in an ambiguous time range.";
case "Argument_DateTimeOffsetIsNotAmbiguous": return "The supplied DateTimeOffset is not in an ambiguous time range.";
case "Argument_DateTimeKindMustBeUnspecified": return "The supplied DateTime must have the Kind property set to DateTimeKind.Unspecified.";
case "Argument_DateTimeHasTicks": return "The supplied DateTime must have the Year, Month, and Day properties set to 1.  The time cannot be specified more precisely than whole milliseconds.";
case "Argument_InvalidId": return "The specified ID parameter '{0}' is not supported.";
case "Argument_InvalidSerializedString": return "The specified serialized string '{0}' is not supported.";
case "Argument_InvalidREG_TZI_FORMAT": return "The REG_TZI_FORMAT structure is corrupt.";
case "Argument_OutOfOrderDateTimes": return "The DateStart property must come before the DateEnd property.";
case "Argument_TimeSpanHasSeconds": return "The TimeSpan parameter cannot be specified more precisely than whole minutes.";
case "Argument_TimeZoneInfoBadTZif": return "The tzfile does not begin with the magic characters 'TZif'.  Please verify that the file is not corrupt.";
case "Argument_TimeZoneInfoInvalidTZif": return "The TZif data structure is corrupt.";
case "Argument_TransitionTimesAreIdentical": return "The DaylightTransitionStart property must not equal the DaylightTransitionEnd property.";
// ;
// ; begin System.TimeZoneInfo ArgumentOutOfRangeException's
// ;
case "ArgumentOutOfRange_DayParam": return "The Day parameter must be in the range 1 through 31.";
case "ArgumentOutOfRange_DayOfWeek": return "The DayOfWeek enumeration must be in the range 0 through 6.";
case "ArgumentOutOfRange_MonthParam": return "The Month parameter must be in the range 1 through 12.";
case "ArgumentOutOfRange_UtcOffset": return "The TimeSpan parameter must be within plus or minus 14.0 hours.";
case "ArgumentOutOfRange_UtcOffsetAndDaylightDelta": return "The sum of the BaseUtcOffset and DaylightDelta properties must within plus or minus 14.0 hours.";
case "ArgumentOutOfRange_Week": return "The Week parameter must be in the range 1 through 5.";
// ;
// ; begin System.TimeZoneInfo InvalidTimeZoneException's
// ;
case "InvalidTimeZone_InvalidRegistryData": return "The time zone ID '{0}' was found on the local computer, but the registry information was corrupt.";
case "InvalidTimeZone_InvalidWin32APIData": return "The Local time zone was found on the local computer, but the data was corrupt.";
// ;
// ; begin System.TimeZoneInfo SecurityException's
// ;
case "Security_CannotReadRegistryData": return "The time zone ID '{0}' was found on the local computer, but the application does not have permission to read the registry information.";
// ;
// ; begin System.TimeZoneInfo SerializationException's
// ;
case "Serialization_CorruptField": return "The value of the field '{0}' is invalid.  The serialized data is corrupt.  ";
case "Serialization_InvalidEscapeSequence": return "The serialized data contained an invalid escape sequence '\\{0}'.";
// ;
// ; begin System.TimeZoneInfo TimeZoneNotFoundException's
// ;
case "TimeZoneNotFound_MissingRegistryData": return "The time zone ID '{0}' was not found on the local computer.";
// ;
// ; end System.TimeZoneInfo
// ;


// ; Tuple
case "ArgumentException_TupleIncorrectType": return "Argument must be of type {0}.";
case "ArgumentException_TupleNonIComparableElement": return "The tuple contains an element of type {0} which does not implement the IComparable interface.";
case "ArgumentException_TupleLastArgumentNotATuple": return "The last element of an eight element tuple must be a Tuple.";
case "ArgumentException_OtherNotArrayOfCorrectLength": return "Object is not a array with the same number of elements as the array to compare it to.";

// ; WinRT collection adapters
case "Argument_IndexOutOfArrayBounds": return "The specified index is out of bounds of the specified array.";
case "Argument_InsufficientSpaceToCopyCollection": return "The specified space is not sufficient to copy the elements from this Collection.";
case "ArgumentOutOfRange_IndexLargerThanMaxValue": return "This collection cannot work with indices larger than Int32.MaxValue - 1 (0x7FFFFFFF - 1).";
case "ArgumentOutOfRange_IndexOutOfRange": return "The specified index is outside the current index range of this collection.";
case "InvalidOperation_CollectionBackingListTooLarge": return "The collection backing this List contains too many elements.";
case "InvalidOperation_CollectionBackingDictionaryTooLarge": return "The collection backing this Dictionary contains too many elements.";
case "InvalidOperation_CannotRemoveLastFromEmptyCollection": return "Cannot remove the last element from an empty collection.";

// ; Globalization resources
// ;------------------

#if !FEATURE_CORECLR
case "Globalization.LegacyModifier": return " Legacy";

// ;
// ;Total items: 809
// ;
case "Globalization.ci_": return "Invariant Language (Invariant Country)";
case "Globalization.ci_aa": return "Afar";
case "Globalization.ci_aa-DJ": return "Afar (Djibouti)";
case "Globalization.ci_aa-ER": return "Afar (Eritrea)";
case "Globalization.ci_aa-ET": return "Afar (Ethiopia)";
case "Globalization.ci_af": return "Afrikaans";
case "Globalization.ci_af-NA": return "Afrikaans (Namibia)";
case "Globalization.ci_af-ZA": return "Afrikaans (South Africa)";
case "Globalization.ci_agq": return "Aghem";
case "Globalization.ci_agq-CM": return "Aghem (Cameroon)";
case "Globalization.ci_ak": return "Akan";
case "Globalization.ci_ak-GH": return "Akan (Ghana)";
case "Globalization.ci_am": return "Amharic";
case "Globalization.ci_am-ET": return "Amharic (Ethiopia)";
case "Globalization.ci_ar": return "Arabic";
case "Globalization.ci_ar-001": return "Arabic (World)";
case "Globalization.ci_ar-AE": return "Arabic (U.A.E.)";
case "Globalization.ci_ar-BH": return "Arabic (Bahrain)";
case "Globalization.ci_ar-DJ": return "Arabic (Djibouti)";
case "Globalization.ci_ar-DZ": return "Arabic (Algeria)";
case "Globalization.ci_ar-EG": return "Arabic (Egypt)";
case "Globalization.ci_ar-ER": return "Arabic (Eritrea)";
case "Globalization.ci_ar-IL": return "Arabic (Israel)";
case "Globalization.ci_ar-IQ": return "Arabic (Iraq)";
case "Globalization.ci_ar-JO": return "Arabic (Jordan)";
case "Globalization.ci_ar-KM": return "Arabic (Comoros)";
case "Globalization.ci_ar-KW": return "Arabic (Kuwait)";
case "Globalization.ci_ar-LB": return "Arabic (Lebanon)";
case "Globalization.ci_ar-LY": return "Arabic (Libya)";
case "Globalization.ci_ar-MA": return "Arabic (Morocco)";
case "Globalization.ci_ar-MR": return "Arabic (Mauritania)";
case "Globalization.ci_ar-OM": return "Arabic (Oman)";
case "Globalization.ci_ar-PS": return "Arabic (Palestinian Authority)";
case "Globalization.ci_ar-QA": return "Arabic (Qatar)";
case "Globalization.ci_ar-SA": return "Arabic (Saudi Arabia)";
case "Globalization.ci_ar-SD": return "Arabic (Sudan)";
case "Globalization.ci_ar-SO": return "Arabic (Somalia)";
case "Globalization.ci_ar-SS": return "Arabic (South Sudan)";
case "Globalization.ci_ar-SY": return "Arabic (Syria)";
case "Globalization.ci_ar-TD": return "Arabic (Chad)";
case "Globalization.ci_ar-TN": return "Arabic (Tunisia)";
case "Globalization.ci_ar-YE": return "Arabic (Yemen)";
case "Globalization.ci_arn": return "Mapudungun";
case "Globalization.ci_arn-CL": return "Mapudungun (Chile)";
case "Globalization.ci_as": return "Assamese";
case "Globalization.ci_as-IN": return "Assamese (India)";
case "Globalization.ci_asa": return "Asu";
case "Globalization.ci_asa-TZ": return "Asu (Tanzania)";
case "Globalization.ci_ast": return "Asturian";
case "Globalization.ci_ast-ES": return "Asturian (Spain)";
case "Globalization.ci_az": return "Azerbaijani";
case "Globalization.ci_az-Cyrl": return "Azerbaijani (Cyrillic)";
case "Globalization.ci_az-Cyrl-AZ": return "Azerbaijani (Cyrillic, Azerbaijan)";
case "Globalization.ci_az-Latn": return "Azerbaijani (Latin)";
case "Globalization.ci_az-Latn-AZ": return "Azerbaijani (Latin, Azerbaijan)";
case "Globalization.ci_ba": return "Bashkir";
case "Globalization.ci_ba-RU": return "Bashkir (Russia)";
case "Globalization.ci_bas": return "Basaa";
case "Globalization.ci_bas-CM": return "Basaa (Cameroon)";
case "Globalization.ci_be": return "Belarusian";
case "Globalization.ci_be-BY": return "Belarusian (Belarus)";
case "Globalization.ci_bem": return "Bemba";
case "Globalization.ci_bem-ZM": return "Bemba (Zambia)";
case "Globalization.ci_bez": return "Bena";
case "Globalization.ci_bez-TZ": return "Bena (Tanzania)";
case "Globalization.ci_bg": return "Bulgarian";
case "Globalization.ci_bg-BG": return "Bulgarian (Bulgaria)";
case "Globalization.ci_bm": return "Bambara";
case "Globalization.ci_bm-Latn": return "Bambara (Latin)";
case "Globalization.ci_bm-Latn-ML": return "Bambara (Latin, Mali)";
case "Globalization.ci_bm-ML": return "Bamanankan (Latin, Mali)";
case "Globalization.ci_bn": return "Bangla";
case "Globalization.ci_bn-BD": return "Bangla (Bangladesh)";
case "Globalization.ci_bn-IN": return "Bangla (India)";
case "Globalization.ci_bo": return "Tibetan";
case "Globalization.ci_bo-CN": return "Tibetan (PRC)";
case "Globalization.ci_bo-IN": return "Tibetan (India)";
case "Globalization.ci_br": return "Breton";
case "Globalization.ci_br-FR": return "Breton (France)";
case "Globalization.ci_brx": return "Bodo";
case "Globalization.ci_brx-IN": return "Bodo (India)";
case "Globalization.ci_bs": return "Bosnian";
case "Globalization.ci_bs-Cyrl": return "Bosnian (Cyrillic)";
case "Globalization.ci_bs-Cyrl-BA": return "Bosnian (Cyrillic, Bosnia and Herzegovina)";
case "Globalization.ci_bs-Latn": return "Bosnian (Latin)";
case "Globalization.ci_bs-Latn-BA": return "Bosnian (Latin, Bosnia and Herzegovina)";
case "Globalization.ci_byn": return "Blin";
case "Globalization.ci_byn-ER": return "Blin (Eritrea)";
case "Globalization.ci_ca": return "Catalan";
case "Globalization.ci_ca-AD": return "Catalan (Andorra)";
case "Globalization.ci_ca-ES": return "Catalan (Catalan)";
case "Globalization.ci_ca-ES-valencia": return "Valencian (Spain)";
case "Globalization.ci_ca-FR": return "Catalan (France)";
case "Globalization.ci_ca-IT": return "Catalan (Italy)";
case "Globalization.ci_cgg": return "Chiga";
case "Globalization.ci_cgg-UG": return "Chiga (Uganda)";
case "Globalization.ci_chr": return "Cherokee";
case "Globalization.ci_chr-Cher": return "Cherokee (Cherokee)";
case "Globalization.ci_chr-Cher-US": return "Cherokee (Cherokee)";
case "Globalization.ci_co": return "Corsican";
case "Globalization.ci_co-FR": return "Corsican (France)";
case "Globalization.ci_cs": return "Czech";
case "Globalization.ci_cs-CZ": return "Czech (Czech Republic)";
case "Globalization.ci_cy": return "Welsh";
case "Globalization.ci_cy-GB": return "Welsh (United Kingdom)";
case "Globalization.ci_da": return "Danish";
case "Globalization.ci_da-DK": return "Danish (Denmark)";
case "Globalization.ci_da-GL": return "Danish (Greenland)";
case "Globalization.ci_dav": return "Taita";
case "Globalization.ci_dav-KE": return "Taita (Kenya)";
case "Globalization.ci_de": return "German";
case "Globalization.ci_de-AT": return "German (Austria)";
case "Globalization.ci_de-BE": return "German (Belgium)";
case "Globalization.ci_de-CH": return "German (Switzerland)";
case "Globalization.ci_de-DE": return "German (Germany)";
case "Globalization.ci_de-DE_phoneb": return "German (Germany)";
case "Globalization.ci_de-LI": return "German (Liechtenstein)";
case "Globalization.ci_de-LU": return "German (Luxembourg)";
case "Globalization.ci_dje": return "Zarma";
case "Globalization.ci_dje-NE": return "Zarma (Niger)";
case "Globalization.ci_dsb": return "Lower Sorbian";
case "Globalization.ci_dsb-DE": return "Lower Sorbian (Germany)";
case "Globalization.ci_dua": return "Duala";
case "Globalization.ci_dua-CM": return "Duala (Cameroon)";
case "Globalization.ci_dv": return "Divehi";
case "Globalization.ci_dv-MV": return "Divehi (Maldives)";
case "Globalization.ci_dyo": return "Jola-Fonyi";
case "Globalization.ci_dyo-SN": return "Jola-Fonyi (Senegal)";
case "Globalization.ci_dz": return "Dzongkha";
case "Globalization.ci_dz-BT": return "Dzongkha (Bhutan)";
case "Globalization.ci_ebu": return "Embu";
case "Globalization.ci_ebu-KE": return "Embu (Kenya)";
case "Globalization.ci_ee": return "Ewe";
case "Globalization.ci_ee-GH": return "Ewe (Ghana)";
case "Globalization.ci_ee-TG": return "Ewe (Togo)";
case "Globalization.ci_el": return "Greek";
case "Globalization.ci_el-CY": return "Greek (Cyprus)";
case "Globalization.ci_el-GR": return "Greek (Greece)";
case "Globalization.ci_en": return "English";
case "Globalization.ci_en-001": return "English (World)";
case "Globalization.ci_en-029": return "English (Caribbean)";
case "Globalization.ci_en-150": return "English (Europe)";
case "Globalization.ci_en-AG": return "English (Antigua and Barbuda)";
case "Globalization.ci_en-AI": return "English (Anguilla)";
case "Globalization.ci_en-AS": return "English (American Samoa)";
case "Globalization.ci_en-AU": return "English (Australia)";
case "Globalization.ci_en-BB": return "English (Barbados)";
case "Globalization.ci_en-BE": return "English (Belgium)";
case "Globalization.ci_en-BM": return "English (Bermuda)";
case "Globalization.ci_en-BS": return "English (Bahamas)";
case "Globalization.ci_en-BW": return "English (Botswana)";
case "Globalization.ci_en-BZ": return "English (Belize)";
case "Globalization.ci_en-CA": return "English (Canada)";
case "Globalization.ci_en-CC": return "English (Cocos [Keeling] Islands)";
case "Globalization.ci_en-CK": return "English (Cook Islands)";
case "Globalization.ci_en-CM": return "English (Cameroon)";
case "Globalization.ci_en-CX": return "English (Christmas Island)";
case "Globalization.ci_en-DM": return "English (Dominica)";
case "Globalization.ci_en-ER": return "English (Eritrea)";
case "Globalization.ci_en-FJ": return "English (Fiji)";
case "Globalization.ci_en-FK": return "English (Falkland Islands)";
case "Globalization.ci_en-FM": return "English (Micronesia)";
case "Globalization.ci_en-GB": return "English (United Kingdom)";
case "Globalization.ci_en-GD": return "English (Grenada)";
case "Globalization.ci_en-GG": return "English (Guernsey)";
case "Globalization.ci_en-GH": return "English (Ghana)";
case "Globalization.ci_en-GI": return "English (Gibraltar)";
case "Globalization.ci_en-GM": return "English (Gambia)";
case "Globalization.ci_en-GU": return "English (Guam)";
case "Globalization.ci_en-GY": return "English (Guyana)";
case "Globalization.ci_en-HK": return "English (Hong Kong SAR)";
case "Globalization.ci_en-IE": return "English (Ireland)";
case "Globalization.ci_en-IM": return "English (Isle of Man)";
case "Globalization.ci_en-IN": return "English (India)";
case "Globalization.ci_en-IO": return "English (British Indian Ocean Territory)";
case "Globalization.ci_en-JE": return "English (Jersey)";
case "Globalization.ci_en-JM": return "English (Jamaica)";
case "Globalization.ci_en-KE": return "English (Kenya)";
case "Globalization.ci_en-KI": return "English (Kiribati)";
case "Globalization.ci_en-KN": return "English (Saint Kitts and Nevis)";
case "Globalization.ci_en-KY": return "English (Cayman Islands)";
case "Globalization.ci_en-LC": return "English (Saint Lucia)";
case "Globalization.ci_en-LR": return "English (Liberia)";
case "Globalization.ci_en-LS": return "English (Lesotho)";
case "Globalization.ci_en-MG": return "English (Madagascar)";
case "Globalization.ci_en-MH": return "English (Marshall Islands)";
case "Globalization.ci_en-MO": return "English (Macao SAR)";
case "Globalization.ci_en-MP": return "English (Northern Mariana Islands)";
case "Globalization.ci_en-MS": return "English (Montserrat)";
case "Globalization.ci_en-MT": return "English (Malta)";
case "Globalization.ci_en-MU": return "English (Mauritius)";
case "Globalization.ci_en-MW": return "English (Malawi)";
case "Globalization.ci_en-MY": return "English (Malaysia)";
case "Globalization.ci_en-NA": return "English (Namibia)";
case "Globalization.ci_en-NF": return "English (Norfolk Island)";
case "Globalization.ci_en-NG": return "English (Nigeria)";
case "Globalization.ci_en-NR": return "English (Nauru)";
case "Globalization.ci_en-NU": return "English (Niue)";
case "Globalization.ci_en-NZ": return "English (New Zealand)";
case "Globalization.ci_en-PG": return "English (Papua New Guinea)";
case "Globalization.ci_en-PH": return "English (Republic of the Philippines)";
case "Globalization.ci_en-PK": return "English (Pakistan)";
case "Globalization.ci_en-PN": return "English (Pitcairn Islands)";
case "Globalization.ci_en-PR": return "English (Puerto Rico)";
case "Globalization.ci_en-PW": return "English (Palau)";
case "Globalization.ci_en-RW": return "English (Rwanda)";
case "Globalization.ci_en-SB": return "English (Solomon Islands)";
case "Globalization.ci_en-SC": return "English (Seychelles)";
case "Globalization.ci_en-SD": return "English (Sudan)";
case "Globalization.ci_en-SG": return "English (Singapore)";
case "Globalization.ci_en-SH": return "English (St Helena, Ascension, Tristan da Cunha)";
case "Globalization.ci_en-SL": return "English (Sierra Leone)";
case "Globalization.ci_en-SS": return "English (South Sudan)";
case "Globalization.ci_en-SX": return "English (Sint Maarten)";
case "Globalization.ci_en-SZ": return "English (Swaziland)";
case "Globalization.ci_en-TC": return "English (Turks and Caicos Islands)";
case "Globalization.ci_en-TK": return "English (Tokelau)";
case "Globalization.ci_en-TO": return "English (Tonga)";
case "Globalization.ci_en-TT": return "English (Trinidad and Tobago)";
case "Globalization.ci_en-TV": return "English (Tuvalu)";
case "Globalization.ci_en-TZ": return "English (Tanzania)";
case "Globalization.ci_en-UG": return "English (Uganda)";
case "Globalization.ci_en-UM": return "English (US Minor Outlying Islands)";
case "Globalization.ci_en-US": return "English (United States)";
case "Globalization.ci_en-VC": return "English (Saint Vincent and the Grenadines)";
case "Globalization.ci_en-VG": return "English (British Virgin Islands)";
case "Globalization.ci_en-VI": return "English (US Virgin Islands)";
case "Globalization.ci_en-VU": return "English (Vanuatu)";
case "Globalization.ci_en-WS": return "English (Samoa)";
case "Globalization.ci_en-ZA": return "English (South Africa)";
case "Globalization.ci_en-ZM": return "English (Zambia)";
case "Globalization.ci_en-ZW": return "English (Zimbabwe)";
case "Globalization.ci_eo": return "Esperanto";
case "Globalization.ci_eo-001": return "Esperanto (World)";
case "Globalization.ci_es": return "Spanish";
case "Globalization.ci_es-419": return "Spanish (Latin America)";
case "Globalization.ci_es-AR": return "Spanish (Argentina)";
case "Globalization.ci_es-BO": return "Spanish (Bolivia)";
case "Globalization.ci_es-CL": return "Spanish (Chile)";
case "Globalization.ci_es-CO": return "Spanish (Colombia)";
case "Globalization.ci_es-CR": return "Spanish (Costa Rica)";
case "Globalization.ci_es-CU": return "Spanish (Cuba)";
case "Globalization.ci_es-DO": return "Spanish (Dominican Republic)";
case "Globalization.ci_es-EC": return "Spanish (Ecuador)";
case "Globalization.ci_es-ES": return "Spanish (Spain)";
case "Globalization.ci_es-ES_tradnl": return "Spanish (Spain)";
case "Globalization.ci_es-GQ": return "Spanish (Equatorial Guinea)";
case "Globalization.ci_es-GT": return "Spanish (Guatemala)";
case "Globalization.ci_es-HN": return "Spanish (Honduras)";
case "Globalization.ci_es-MX": return "Spanish (Mexico)";
case "Globalization.ci_es-NI": return "Spanish (Nicaragua)";
case "Globalization.ci_es-PA": return "Spanish (Panama)";
case "Globalization.ci_es-PE": return "Spanish (Peru)";
case "Globalization.ci_es-PH": return "Spanish (Philippines)";
case "Globalization.ci_es-PR": return "Spanish (Puerto Rico)";
case "Globalization.ci_es-PY": return "Spanish (Paraguay)";
case "Globalization.ci_es-SV": return "Spanish (El Salvador)";
case "Globalization.ci_es-US": return "Spanish (United States)";
case "Globalization.ci_es-UY": return "Spanish (Uruguay)";
case "Globalization.ci_es-VE": return "Spanish (Bolivarian Republic of Venezuela)";
case "Globalization.ci_et": return "Estonian";
case "Globalization.ci_et-EE": return "Estonian (Estonia)";
case "Globalization.ci_eu": return "Basque";
case "Globalization.ci_eu-ES": return "Basque (Basque)";
case "Globalization.ci_ewo": return "Ewondo";
case "Globalization.ci_ewo-CM": return "Ewondo (Cameroon)";
case "Globalization.ci_fa": return "Persian";
case "Globalization.ci_fa-AF": return "Persian (Afghanistan)";
case "Globalization.ci_fa-IR": return "Persian";
case "Globalization.ci_ff": return "Fulah";
case "Globalization.ci_ff-CM": return "Fulah (Cameroon)";
case "Globalization.ci_ff-GN": return "Fulah (Guinea)";
case "Globalization.ci_ff-Latn": return "Fulah (Latin)";
case "Globalization.ci_ff-Latn-SN": return "Fulah (Latin, Senegal)";
case "Globalization.ci_ff-MR": return "Fulah (Mauritania)";
case "Globalization.ci_fi": return "Finnish";
case "Globalization.ci_fi-FI": return "Finnish (Finland)";
case "Globalization.ci_fil": return "Filipino";
case "Globalization.ci_fil-PH": return "Filipino (Philippines)";
case "Globalization.ci_fo": return "Faroese";
case "Globalization.ci_fo-FO": return "Faroese (Faroe Islands)";
case "Globalization.ci_fr": return "French";
case "Globalization.ci_fr-BE": return "French (Belgium)";
case "Globalization.ci_fr-BF": return "French (Burkina Faso)";
case "Globalization.ci_fr-BI": return "French (Burundi)";
case "Globalization.ci_fr-BJ": return "French (Benin)";
case "Globalization.ci_fr-BL": return "French (Saint Barth�lemy)";
case "Globalization.ci_fr-CA": return "French (Canada)";
case "Globalization.ci_fr-CD": return "French (Congo DRC)";
case "Globalization.ci_fr-CF": return "French (Central African Republic)";
case "Globalization.ci_fr-CG": return "French (Congo)";
case "Globalization.ci_fr-CH": return "French (Switzerland)";
case "Globalization.ci_fr-CI": return "French (C�te d�Ivoire)";
case "Globalization.ci_fr-CM": return "French (Cameroon)";
case "Globalization.ci_fr-DJ": return "French (Djibouti)";
case "Globalization.ci_fr-DZ": return "French (Algeria)";
case "Globalization.ci_fr-FR": return "French (France)";
case "Globalization.ci_fr-GA": return "French (Gabon)";
case "Globalization.ci_fr-GF": return "French (French Guiana)";
case "Globalization.ci_fr-GN": return "French (Guinea)";
case "Globalization.ci_fr-GP": return "French (Guadeloupe)";
case "Globalization.ci_fr-GQ": return "French (Equatorial Guinea)";
case "Globalization.ci_fr-HT": return "French (Haiti)";
case "Globalization.ci_fr-KM": return "French (Comoros)";
case "Globalization.ci_fr-LU": return "French (Luxembourg)";
case "Globalization.ci_fr-MA": return "French (Morocco)";
case "Globalization.ci_fr-MC": return "French (Monaco)";
case "Globalization.ci_fr-MF": return "French (Saint Martin)";
case "Globalization.ci_fr-MG": return "French (Madagascar)";
case "Globalization.ci_fr-ML": return "French (Mali)";
case "Globalization.ci_fr-MQ": return "French (Martinique)";
case "Globalization.ci_fr-MR": return "French (Mauritania)";
case "Globalization.ci_fr-MU": return "French (Mauritius)";
case "Globalization.ci_fr-NC": return "French (New Caledonia)";
case "Globalization.ci_fr-NE": return "French (Niger)";
case "Globalization.ci_fr-PF": return "French (French Polynesia)";
case "Globalization.ci_fr-PM": return "French (Saint Pierre and Miquelon)";
case "Globalization.ci_fr-RE": return "French (Reunion)";
case "Globalization.ci_fr-RW": return "French (Rwanda)";
case "Globalization.ci_fr-SC": return "French (Seychelles)";
case "Globalization.ci_fr-SN": return "French (Senegal)";
case "Globalization.ci_fr-SY": return "French (Syria)";
case "Globalization.ci_fr-TD": return "French (Chad)";
case "Globalization.ci_fr-TG": return "French (Togo)";
case "Globalization.ci_fr-TN": return "French (Tunisia)";
case "Globalization.ci_fr-VU": return "French (Vanuatu)";
case "Globalization.ci_fr-WF": return "French (Wallis and Futuna)";
case "Globalization.ci_fr-YT": return "French (Mayotte)";
case "Globalization.ci_fur": return "Friulian";
case "Globalization.ci_fur-IT": return "Friulian (Italy)";
case "Globalization.ci_fy": return "Frisian";
case "Globalization.ci_fy-NL": return "Frisian (Netherlands)";
case "Globalization.ci_ga": return "Irish";
case "Globalization.ci_ga-IE": return "Irish (Ireland)";
case "Globalization.ci_gd": return "Scottish Gaelic";
case "Globalization.ci_gd-GB": return "Scottish Gaelic (United Kingdom)";
case "Globalization.ci_gl": return "Galician";
case "Globalization.ci_gl-ES": return "Galician (Galician)";
case "Globalization.ci_gn": return "Guarani";
case "Globalization.ci_gn-PY": return "Guarani (Paraguay)";
case "Globalization.ci_gsw": return "Alsatian";
case "Globalization.ci_gsw-CH": return "Alsatian (Switzerland)";
case "Globalization.ci_gsw-FR": return "Alsatian (France)";
case "Globalization.ci_gsw-LI": return "Alsatian (Liechtenstein)";
case "Globalization.ci_gu": return "Gujarati";
case "Globalization.ci_gu-IN": return "Gujarati (India)";
case "Globalization.ci_guz": return "Gusii";
case "Globalization.ci_guz-KE": return "Gusii (Kenya)";
case "Globalization.ci_gv": return "Manx";
case "Globalization.ci_gv-IM": return "Manx (Isle of Man)";
case "Globalization.ci_ha": return "Hausa";
case "Globalization.ci_ha-Latn": return "Hausa (Latin)";
case "Globalization.ci_ha-Latn-GH": return "Hausa (Latin, Ghana)";
case "Globalization.ci_ha-Latn-NE": return "Hausa (Latin, Niger)";
case "Globalization.ci_ha-Latn-NG": return "Hausa (Latin, Nigeria)";
case "Globalization.ci_haw": return "Hawaiian";
case "Globalization.ci_haw-US": return "Hawaiian (United States)";
case "Globalization.ci_he": return "Hebrew";
case "Globalization.ci_he-IL": return "Hebrew (Israel)";
case "Globalization.ci_hi": return "Hindi";
case "Globalization.ci_hi-IN": return "Hindi (India)";
case "Globalization.ci_hr": return "Croatian";
case "Globalization.ci_hr-BA": return "Croatian (Latin, Bosnia and Herzegovina)";
case "Globalization.ci_hr-HR": return "Croatian (Croatia)";
case "Globalization.ci_hsb": return "Upper Sorbian";
case "Globalization.ci_hsb-DE": return "Upper Sorbian (Germany)";
case "Globalization.ci_hu": return "Hungarian";
case "Globalization.ci_hu-HU": return "Hungarian (Hungary)";
case "Globalization.ci_hu-HU_technl": return "Hungarian (Hungary)";
case "Globalization.ci_hy": return "Armenian";
case "Globalization.ci_hy-AM": return "Armenian (Armenia)";
case "Globalization.ci_ia": return "Interlingua";
case "Globalization.ci_ia-001": return "Interlingua (World)";
case "Globalization.ci_ia-FR": return "Interlingua (France)";
case "Globalization.ci_id": return "Indonesian";
case "Globalization.ci_id-ID": return "Indonesian (Indonesia)";
case "Globalization.ci_ig": return "Igbo";
case "Globalization.ci_ig-NG": return "Igbo (Nigeria)";
case "Globalization.ci_ii": return "Yi";
case "Globalization.ci_ii-CN": return "Yi (PRC)";
case "Globalization.ci_is": return "Icelandic";
case "Globalization.ci_is-IS": return "Icelandic (Iceland)";
case "Globalization.ci_it": return "Italian";
case "Globalization.ci_it-CH": return "Italian (Switzerland)";
case "Globalization.ci_it-IT": return "Italian (Italy)";
case "Globalization.ci_it-SM": return "Italian (San Marino)";
case "Globalization.ci_iu": return "Inuktitut";
case "Globalization.ci_iu-Cans": return "Inuktitut (Syllabics)";
case "Globalization.ci_iu-Cans-CA": return "Inuktitut (Syllabics, Canada)";
case "Globalization.ci_iu-Latn": return "Inuktitut (Latin)";
case "Globalization.ci_iu-Latn-CA": return "Inuktitut (Latin, Canada)";
case "Globalization.ci_ja": return "Japanese";
case "Globalization.ci_ja-JP": return "Japanese (Japan)";
case "Globalization.ci_ja-JP_radstr": return "Japanese (Japan)";
case "Globalization.ci_jgo": return "Ngomba";
case "Globalization.ci_jgo-CM": return "Ngomba (Cameroon)";
case "Globalization.ci_jmc": return "Machame";
case "Globalization.ci_jmc-TZ": return "Machame (Tanzania)";
case "Globalization.ci_jv": return "Javanese";
case "Globalization.ci_jv-Latn": return "Javanese";
case "Globalization.ci_jv-Latn-ID": return "Javanese (Indonesia)";
case "Globalization.ci_ka": return "Georgian";
case "Globalization.ci_ka-GE": return "Georgian (Georgia)";
case "Globalization.ci_ka-GE_modern": return "Georgian (Georgia)";
case "Globalization.ci_kab": return "Kabyle";
case "Globalization.ci_kab-DZ": return "Kabyle (Algeria)";
case "Globalization.ci_kam": return "Kamba";
case "Globalization.ci_kam-KE": return "Kamba (Kenya)";
case "Globalization.ci_kde": return "Makonde";
case "Globalization.ci_kde-TZ": return "Makonde (Tanzania)";
case "Globalization.ci_kea": return "Kabuverdianu";
case "Globalization.ci_kea-CV": return "Kabuverdianu (Cabo Verde)";
case "Globalization.ci_khq": return "Koyra Chiini";
case "Globalization.ci_khq-ML": return "Koyra Chiini (Mali)";
case "Globalization.ci_ki": return "Kikuyu";
case "Globalization.ci_ki-KE": return "Kikuyu (Kenya)";
case "Globalization.ci_kk": return "Kazakh";
case "Globalization.ci_kk-KZ": return "Kazakh (Kazakhstan)";
case "Globalization.ci_kkj": return "Kako";
case "Globalization.ci_kkj-CM": return "Kako (Cameroon)";
case "Globalization.ci_kl": return "Greenlandic";
case "Globalization.ci_kl-GL": return "Greenlandic (Greenland)";
case "Globalization.ci_kln": return "Kalenjin";
case "Globalization.ci_kln-KE": return "Kalenjin (Kenya)";
case "Globalization.ci_km": return "Khmer";
case "Globalization.ci_km-KH": return "Khmer (Cambodia)";
case "Globalization.ci_kn": return "Kannada";
case "Globalization.ci_kn-IN": return "Kannada (India)";
case "Globalization.ci_ko": return "Korean";
case "Globalization.ci_ko-KR": return "Korean (Korea)";
case "Globalization.ci_kok": return "Konkani";
case "Globalization.ci_kok-IN": return "Konkani (India)";
case "Globalization.ci_ks": return "Kashmiri";
case "Globalization.ci_ks-Arab": return "Kashmiri (Perso-Arabic)";
case "Globalization.ci_ks-Arab-IN": return "Kashmiri (Perso-Arabic)";
case "Globalization.ci_ksb": return "Shambala";
case "Globalization.ci_ksb-TZ": return "Shambala (Tanzania)";
case "Globalization.ci_ksf": return "Bafia";
case "Globalization.ci_ksf-CM": return "Bafia (Cameroon)";
case "Globalization.ci_ksh": return "Colognian";
case "Globalization.ci_ksh-DE": return "Ripuarian (Germany)";
case "Globalization.ci_ku": return "Central Kurdish";
case "Globalization.ci_ku-Arab": return "Central Kurdish (Arabic)";
case "Globalization.ci_ku-Arab-IQ": return "Central Kurdish (Iraq)";
case "Globalization.ci_kw": return "Cornish";
case "Globalization.ci_kw-GB": return "Cornish (United Kingdom)";
case "Globalization.ci_ky": return "Kyrgyz";
case "Globalization.ci_ky-KG": return "Kyrgyz (Kyrgyzstan)";
case "Globalization.ci_lag": return "Langi";
case "Globalization.ci_lag-TZ": return "Langi (Tanzania)";
case "Globalization.ci_lb": return "Luxembourgish";
case "Globalization.ci_lb-LU": return "Luxembourgish (Luxembourg)";
case "Globalization.ci_lg": return "Ganda";
case "Globalization.ci_lg-UG": return "Ganda (Uganda)";
case "Globalization.ci_lkt": return "Lakota";
case "Globalization.ci_lkt-US": return "Lakota (United States)";
case "Globalization.ci_ln": return "Lingala";
case "Globalization.ci_ln-AO": return "Lingala (Angola)";
case "Globalization.ci_ln-CD": return "Lingala (Congo DRC)";
case "Globalization.ci_ln-CF": return "Lingala (Central African Republic)";
case "Globalization.ci_ln-CG": return "Lingala (Congo)";
case "Globalization.ci_lo": return "Lao";
case "Globalization.ci_lo-LA": return "Lao (Lao P.D.R.)";
case "Globalization.ci_lt": return "Lithuanian";
case "Globalization.ci_lt-LT": return "Lithuanian (Lithuania)";
case "Globalization.ci_lu": return "Luba-Katanga";
case "Globalization.ci_lu-CD": return "Luba-Katanga (Congo DRC)";
case "Globalization.ci_luo": return "Luo";
case "Globalization.ci_luo-KE": return "Luo (Kenya)";
case "Globalization.ci_luy": return "Luyia";
case "Globalization.ci_luy-KE": return "Luyia (Kenya)";
case "Globalization.ci_lv": return "Latvian";
case "Globalization.ci_lv-LV": return "Latvian (Latvia)";
case "Globalization.ci_mas": return "Masai";
case "Globalization.ci_mas-KE": return "Masai (Kenya)";
case "Globalization.ci_mas-TZ": return "Masai (Tanzania)";
case "Globalization.ci_mer": return "Meru";
case "Globalization.ci_mer-KE": return "Meru (Kenya)";
case "Globalization.ci_mfe": return "Morisyen";
case "Globalization.ci_mfe-MU": return "Morisyen (Mauritius)";
case "Globalization.ci_mg": return "Malagasy";
case "Globalization.ci_mg-MG": return "Malagasy (Madagascar)";
case "Globalization.ci_mgh": return "Makhuwa-Meetto";
case "Globalization.ci_mgh-MZ": return "Makhuwa-Meetto (Mozambique)";
case "Globalization.ci_mgo": return "Meta'";
case "Globalization.ci_mgo-CM": return "Meta' (Cameroon)";
case "Globalization.ci_mi": return "Maori";
case "Globalization.ci_mi-NZ": return "Maori (New Zealand)";
case "Globalization.ci_mk": return "Macedonian (FYROM)";
case "Globalization.ci_mk-MK": return "Macedonian (Former Yugoslav Republic of Macedonia)";
case "Globalization.ci_ml": return "Malayalam";
case "Globalization.ci_ml-IN": return "Malayalam (India)";
case "Globalization.ci_mn": return "Mongolian";
case "Globalization.ci_mn-Cyrl": return "Mongolian (Cyrillic)";
case "Globalization.ci_mn-MN": return "Mongolian (Cyrillic, Mongolia)";
case "Globalization.ci_mn-Mong": return "Mongolian (Traditional Mongolian)";
case "Globalization.ci_mn-Mong-CN": return "Mongolian (Traditional Mongolian, PRC)";
case "Globalization.ci_mn-Mong-MN": return "Mongolian (Traditional Mongolian, Mongolia)";
case "Globalization.ci_moh": return "Mohawk";
case "Globalization.ci_moh-CA": return "Mohawk (Mohawk)";
case "Globalization.ci_mr": return "Marathi";
case "Globalization.ci_mr-IN": return "Marathi (India)";
case "Globalization.ci_ms": return "Malay";
case "Globalization.ci_ms-BN": return "Malay (Brunei Darussalam)";
case "Globalization.ci_ms-MY": return "Malay (Malaysia)";
case "Globalization.ci_ms-SG": return "Malay (Latin, Singapore)";
case "Globalization.ci_mt": return "Maltese";
case "Globalization.ci_mt-MT": return "Maltese (Malta)";
case "Globalization.ci_mua": return "Mundang";
case "Globalization.ci_mua-CM": return "Mundang (Cameroon)";
case "Globalization.ci_my": return "Burmese";
case "Globalization.ci_my-MM": return "Burmese (Myanmar)";
case "Globalization.ci_naq": return "Nama";
case "Globalization.ci_naq-NA": return "Nama (Namibia)";
case "Globalization.ci_nb": return "Norwegian (Bokm�l)";
case "Globalization.ci_nb-NO": return "Norwegian, Bokm�l (Norway)";
case "Globalization.ci_nb-SJ": return "Norwegian, Bokm�l (Svalbard and Jan Mayen)";
case "Globalization.ci_nd": return "North Ndebele";
case "Globalization.ci_nd-ZW": return "North Ndebele (Zimbabwe)";
case "Globalization.ci_ne": return "Nepali";
case "Globalization.ci_ne-IN": return "Nepali (India)";
case "Globalization.ci_ne-NP": return "Nepali (Nepal)";
case "Globalization.ci_nl": return "Dutch";
case "Globalization.ci_nl-AW": return "Dutch (Aruba)";
case "Globalization.ci_nl-BE": return "Dutch (Belgium)";
case "Globalization.ci_nl-BQ": return "Dutch (Bonaire, Sint Eustatius and Saba)";
case "Globalization.ci_nl-CW": return "Dutch (Cura�ao)";
case "Globalization.ci_nl-NL": return "Dutch (Netherlands)";
case "Globalization.ci_nl-SR": return "Dutch (Suriname)";
case "Globalization.ci_nl-SX": return "Dutch (Sint Maarten)";
case "Globalization.ci_nmg": return "Kwasio";
case "Globalization.ci_nmg-CM": return "Kwasio (Cameroon)";
case "Globalization.ci_nn": return "Norwegian (Nynorsk)";
case "Globalization.ci_nn-NO": return "Norwegian, Nynorsk (Norway)";
case "Globalization.ci_nnh": return "Ngiemboon";
case "Globalization.ci_nnh-CM": return "Ngiemboon (Cameroon)";
case "Globalization.ci_no": return "Norwegian";
case "Globalization.ci_nqo": return "N'ko";
case "Globalization.ci_nqo-GN": return "N'ko (Guinea)";
case "Globalization.ci_nr": return "South Ndebele";
case "Globalization.ci_nr-ZA": return "South Ndebele (South Africa)";
case "Globalization.ci_nso": return "Sesotho sa Leboa";
case "Globalization.ci_nso-ZA": return "Sesotho sa Leboa (South Africa)";
case "Globalization.ci_nus": return "Nuer";
case "Globalization.ci_nus-SD": return "Nuer (Sudan)";
case "Globalization.ci_nyn": return "Nyankole";
case "Globalization.ci_nyn-UG": return "Nyankole (Uganda)";
case "Globalization.ci_oc": return "Occitan";
case "Globalization.ci_oc-FR": return "Occitan (France)";
case "Globalization.ci_om": return "Oromo";
case "Globalization.ci_om-ET": return "Oromo (Ethiopia)";
case "Globalization.ci_om-KE": return "Oromo (Kenya)";
case "Globalization.ci_or": return "Odia";
case "Globalization.ci_or-IN": return "Odia (India)";
case "Globalization.ci_os": return "Ossetic";
case "Globalization.ci_os-GE": return "Ossetian (Cyrillic, Georgia)";
case "Globalization.ci_os-RU": return "Ossetian (Cyrillic, Russia)";
case "Globalization.ci_pa": return "Punjabi";
case "Globalization.ci_pa-Arab": return "Punjabi (Arabic)";
case "Globalization.ci_pa-Arab-PK": return "Punjabi (Islamic Republic of Pakistan)";
case "Globalization.ci_pa-IN": return "Punjabi (India)";
case "Globalization.ci_pl": return "Polish";
case "Globalization.ci_pl-PL": return "Polish (Poland)";
case "Globalization.ci_prs": return "Dari";
case "Globalization.ci_prs-AF": return "Dari (Afghanistan)";
case "Globalization.ci_ps": return "Pashto";
case "Globalization.ci_ps-AF": return "Pashto (Afghanistan)";
case "Globalization.ci_pt": return "Portuguese";
case "Globalization.ci_pt-AO": return "Portuguese (Angola)";
case "Globalization.ci_pt-BR": return "Portuguese (Brazil)";
case "Globalization.ci_pt-CV": return "Portuguese (Cabo Verde)";
case "Globalization.ci_pt-GW": return "Portuguese (Guinea-Bissau)";
case "Globalization.ci_pt-MO": return "Portuguese (Macao SAR)";
case "Globalization.ci_pt-MZ": return "Portuguese (Mozambique)";
case "Globalization.ci_pt-PT": return "Portuguese (Portugal)";
case "Globalization.ci_pt-ST": return "Portuguese (S�o Tom� and Pr�ncipe)";
case "Globalization.ci_pt-TL": return "Portuguese (Timor-Leste)";
case "Globalization.ci_qps-ploc": return "Pseudo Language (Pseudo)";
case "Globalization.ci_qps-ploca": return "Pseudo Language (Pseudo Asia)";
case "Globalization.ci_qps-plocm": return "Pseudo Language (Pseudo Mirrored)";
case "Globalization.ci_qu": return "Quechua";
case "Globalization.ci_qu-BO": return "Quechua (Bolivia)";
case "Globalization.ci_qu-EC": return "Quechua (Ecuador)";
case "Globalization.ci_qu-PE": return "Quechua (Peru)";
case "Globalization.ci_quc": return "K'iche'";
case "Globalization.ci_quc-Latn": return "K'iche'";
case "Globalization.ci_quc-Latn-GT": return "K'iche' (Guatemala)";
case "Globalization.ci_qut": return "K'iche";
case "Globalization.ci_qut-GT": return "K'iche (Guatemala)";
case "Globalization.ci_quz": return "Quechua";
case "Globalization.ci_quz-BO": return "Quechua (Bolivia)";
case "Globalization.ci_quz-EC": return "Quechua (Ecuador)";
case "Globalization.ci_quz-PE": return "Quechua (Peru)";
case "Globalization.ci_rm": return "Romansh";
case "Globalization.ci_rm-CH": return "Romansh (Switzerland)";
case "Globalization.ci_rn": return "Rundi";
case "Globalization.ci_rn-BI": return "Rundi (Burundi)";
case "Globalization.ci_ro": return "Romanian";
case "Globalization.ci_ro-MD": return "Romanian (Moldova)";
case "Globalization.ci_ro-RO": return "Romanian (Romania)";
case "Globalization.ci_rof": return "Rombo";
case "Globalization.ci_rof-TZ": return "Rombo (Tanzania)";
case "Globalization.ci_ru": return "Russian";
case "Globalization.ci_ru-BY": return "Russian (Belarus)";
case "Globalization.ci_ru-KG": return "Russian (Kyrgyzstan)";
case "Globalization.ci_ru-KZ": return "Russian (Kazakhstan)";
case "Globalization.ci_ru-MD": return "Russian (Moldova)";
case "Globalization.ci_ru-RU": return "Russian (Russia)";
case "Globalization.ci_ru-UA": return "Russian (Ukraine)";
case "Globalization.ci_rw": return "Kinyarwanda";
case "Globalization.ci_rw-RW": return "Kinyarwanda (Rwanda)";
case "Globalization.ci_rwk": return "Rwa";
case "Globalization.ci_rwk-TZ": return "Rwa (Tanzania)";
case "Globalization.ci_sa": return "Sanskrit";
case "Globalization.ci_sa-IN": return "Sanskrit (India)";
case "Globalization.ci_sah": return "Sakha";
case "Globalization.ci_sah-RU": return "Sakha (Russia)";
case "Globalization.ci_saq": return "Samburu";
case "Globalization.ci_saq-KE": return "Samburu (Kenya)";
case "Globalization.ci_sbp": return "Sangu";
case "Globalization.ci_sbp-TZ": return "Sangu (Tanzania)";
case "Globalization.ci_sd": return "Sindhi";
case "Globalization.ci_sd-Arab": return "Sindhi (Arabic)";
case "Globalization.ci_sd-Arab-PK": return "Sindhi (Islamic Republic of Pakistan)";
case "Globalization.ci_se": return "Sami (Northern)";
case "Globalization.ci_se-FI": return "Sami, Northern (Finland)";
case "Globalization.ci_se-NO": return "Sami, Northern (Norway)";
case "Globalization.ci_se-SE": return "Sami, Northern (Sweden)";
case "Globalization.ci_seh": return "Sena";
case "Globalization.ci_seh-MZ": return "Sena (Mozambique)";
case "Globalization.ci_ses": return "Koyraboro Senni";
case "Globalization.ci_ses-ML": return "Koyraboro Senni (Mali)";
case "Globalization.ci_sg": return "Sango";
case "Globalization.ci_sg-CF": return "Sango (Central African Republic)";
case "Globalization.ci_shi": return "Tachelhit";
case "Globalization.ci_shi-Latn": return "Tachelhit (Latin)";
case "Globalization.ci_shi-Latn-MA": return "Tachelhit (Latin, Morocco)";
case "Globalization.ci_shi-Tfng": return "Tachelhit (Tifinagh)";
case "Globalization.ci_shi-Tfng-MA": return "Tachelhit (Tifinagh, Morocco)";
case "Globalization.ci_si": return "Sinhala";
case "Globalization.ci_si-LK": return "Sinhala (Sri Lanka)";
case "Globalization.ci_sk": return "Slovak";
case "Globalization.ci_sk-SK": return "Slovak (Slovakia)";
case "Globalization.ci_sl": return "Slovenian";
case "Globalization.ci_sl-SI": return "Slovenian (Slovenia)";
case "Globalization.ci_sma": return "Sami (Southern)";
case "Globalization.ci_sma-NO": return "Sami, Southern (Norway)";
case "Globalization.ci_sma-SE": return "Sami, Southern (Sweden)";
case "Globalization.ci_smj": return "Sami (Lule)";
case "Globalization.ci_smj-NO": return "Sami, Lule (Norway)";
case "Globalization.ci_smj-SE": return "Sami, Lule (Sweden)";
case "Globalization.ci_smn": return "Sami (Inari)";
case "Globalization.ci_smn-FI": return "Sami, Inari (Finland)";
case "Globalization.ci_sms": return "Sami (Skolt)";
case "Globalization.ci_sms-FI": return "Sami, Skolt (Finland)";
case "Globalization.ci_sn": return "Shona";
case "Globalization.ci_sn-Latn": return "Shona (Latin)";
case "Globalization.ci_sn-Latn-ZW": return "Shona (Latin, Zimbabwe)";
case "Globalization.ci_so": return "Somali";
case "Globalization.ci_so-DJ": return "Somali (Djibouti)";
case "Globalization.ci_so-ET": return "Somali (Ethiopia)";
case "Globalization.ci_so-KE": return "Somali (Kenya)";
case "Globalization.ci_so-SO": return "Somali (Somalia)";
case "Globalization.ci_sq": return "Albanian";
case "Globalization.ci_sq-AL": return "Albanian (Albania)";
case "Globalization.ci_sq-MK": return "Albanian (Macedonia, FYRO)";
case "Globalization.ci_sr": return "Serbian";
case "Globalization.ci_sr-Cyrl": return "Serbian (Cyrillic)";
case "Globalization.ci_sr-Cyrl-BA": return "Serbian (Cyrillic, Bosnia and Herzegovina)";
case "Globalization.ci_sr-Cyrl-CS": return "Serbian (Cyrillic, Serbia and Montenegro (Former))";
case "Globalization.ci_sr-Cyrl-ME": return "Serbian (Cyrillic, Montenegro)";
case "Globalization.ci_sr-Cyrl-RS": return "Serbian (Cyrillic, Serbia)";
case "Globalization.ci_sr-Latn": return "Serbian (Latin)";
case "Globalization.ci_sr-Latn-BA": return "Serbian (Latin, Bosnia and Herzegovina)";
case "Globalization.ci_sr-Latn-CS": return "Serbian (Latin, Serbia and Montenegro (Former))";
case "Globalization.ci_sr-Latn-ME": return "Serbian (Latin, Montenegro)";
case "Globalization.ci_sr-Latn-RS": return "Serbian (Latin, Serbia)";
case "Globalization.ci_ss": return "Swati";
case "Globalization.ci_ss-SZ": return "Swati (Swaziland)";
case "Globalization.ci_ss-ZA": return "Swati (South Africa)";
case "Globalization.ci_ssy": return "Saho";
case "Globalization.ci_ssy-ER": return "Saho (Eritrea)";
case "Globalization.ci_st": return "Southern Sotho";
case "Globalization.ci_st-LS": return "Sesotho (Lesotho)";
case "Globalization.ci_st-ZA": return "Southern Sotho (South Africa)";
case "Globalization.ci_sv": return "Swedish";
case "Globalization.ci_sv-AX": return "Swedish (�land Islands)";
case "Globalization.ci_sv-FI": return "Swedish (Finland)";
case "Globalization.ci_sv-SE": return "Swedish (Sweden)";
case "Globalization.ci_sw": return "Kiswahili";
case "Globalization.ci_sw-KE": return "Kiswahili (Kenya)";
case "Globalization.ci_sw-TZ": return "Kiswahili (Tanzania)";
case "Globalization.ci_sw-UG": return "Kiswahili (Uganda)";
case "Globalization.ci_swc": return "Congo Swahili";
case "Globalization.ci_swc-CD": return "Congo Swahili (Congo DRC)";
case "Globalization.ci_syr": return "Syriac";
case "Globalization.ci_syr-SY": return "Syriac (Syria)";
case "Globalization.ci_ta": return "Tamil";
case "Globalization.ci_ta-IN": return "Tamil (India)";
case "Globalization.ci_ta-LK": return "Tamil (Sri Lanka)";
case "Globalization.ci_ta-MY": return "Tamil (Malaysia)";
case "Globalization.ci_ta-SG": return "Tamil (Singapore)";
case "Globalization.ci_te": return "Telugu";
case "Globalization.ci_te-IN": return "Telugu (India)";
case "Globalization.ci_teo": return "Teso";
case "Globalization.ci_teo-KE": return "Teso (Kenya)";
case "Globalization.ci_teo-UG": return "Teso (Uganda)";
case "Globalization.ci_tg": return "Tajik";
case "Globalization.ci_tg-Cyrl": return "Tajik (Cyrillic)";
case "Globalization.ci_tg-Cyrl-TJ": return "Tajik (Cyrillic, Tajikistan)";
case "Globalization.ci_th": return "Thai";
case "Globalization.ci_th-TH": return "Thai (Thailand)";
case "Globalization.ci_ti": return "Tigrinya";
case "Globalization.ci_ti-ER": return "Tigrinya (Eritrea)";
case "Globalization.ci_ti-ET": return "Tigrinya (Ethiopia)";
case "Globalization.ci_tig": return "Tigre";
case "Globalization.ci_tig-ER": return "Tigre (Eritrea)";
case "Globalization.ci_tk": return "Turkmen";
case "Globalization.ci_tk-TM": return "Turkmen (Turkmenistan)";
case "Globalization.ci_tn": return "Setswana";
case "Globalization.ci_tn-BW": return "Setswana (Botswana)";
case "Globalization.ci_tn-ZA": return "Setswana (South Africa)";
case "Globalization.ci_to": return "Tongan";
case "Globalization.ci_to-TO": return "Tongan (Tonga)";
case "Globalization.ci_tr": return "Turkish";
case "Globalization.ci_tr-CY": return "Turkish (Cyprus)";
case "Globalization.ci_tr-TR": return "Turkish (Turkey)";
case "Globalization.ci_ts": return "Tsonga";
case "Globalization.ci_ts-ZA": return "Tsonga (South Africa)";
case "Globalization.ci_tt": return "Tatar";
case "Globalization.ci_tt-RU": return "Tatar (Russia)";
case "Globalization.ci_twq": return "Tasawaq";
case "Globalization.ci_twq-NE": return "Tasawaq (Niger)";
case "Globalization.ci_tzm": return "Tamazight";
case "Globalization.ci_tzm-Latn": return "Tamazight (Latin)";
case "Globalization.ci_tzm-Latn-DZ": return "Tamazight (Latin, Algeria)";
case "Globalization.ci_tzm-Latn-MA": return "Central Atlas Tamazight (Latin, Morocco)";
case "Globalization.ci_tzm-Tfng": return "Tamazight (Tifinagh)";
case "Globalization.ci_tzm-Tfng-MA": return "Central Atlas Tamazight (Tifinagh, Morocco)";
case "Globalization.ci_ug": return "Uyghur";
case "Globalization.ci_ug-CN": return "Uyghur (PRC)";
case "Globalization.ci_uk": return "Ukrainian";
case "Globalization.ci_uk-UA": return "Ukrainian (Ukraine)";
case "Globalization.ci_ur": return "Urdu";
case "Globalization.ci_ur-IN": return "Urdu (India)";
case "Globalization.ci_ur-PK": return "Urdu (Islamic Republic of Pakistan)";
case "Globalization.ci_uz": return "Uzbek";
case "Globalization.ci_uz-Arab": return "Uzbek (Perso-Arabic)";
case "Globalization.ci_uz-Arab-AF": return "Uzbek (Perso-Arabic, Afghanistan)";
case "Globalization.ci_uz-Cyrl": return "Uzbek (Cyrillic)";
case "Globalization.ci_uz-Cyrl-UZ": return "Uzbek (Cyrillic, Uzbekistan)";
case "Globalization.ci_uz-Latn": return "Uzbek (Latin)";
case "Globalization.ci_uz-Latn-UZ": return "Uzbek (Latin, Uzbekistan)";
case "Globalization.ci_vai": return "Vai";
case "Globalization.ci_vai-Latn": return "Vai (Latin)";
case "Globalization.ci_vai-Latn-LR": return "Vai (Latin, Liberia)";
case "Globalization.ci_vai-Vaii": return "Vai (Vai)";
case "Globalization.ci_vai-Vaii-LR": return "Vai (Vai, Liberia)";
case "Globalization.ci_ve": return "Venda";
case "Globalization.ci_ve-ZA": return "Venda (South Africa)";
case "Globalization.ci_vi": return "Vietnamese";
case "Globalization.ci_vi-VN": return "Vietnamese (Vietnam)";
case "Globalization.ci_vo": return "Volap�k";
case "Globalization.ci_vo-001": return "Volap�k (World)";
case "Globalization.ci_vun": return "Vunjo";
case "Globalization.ci_vun-TZ": return "Vunjo (Tanzania)";
case "Globalization.ci_wae": return "Walser";
case "Globalization.ci_wae-CH": return "Walser (Switzerland)";
case "Globalization.ci_wal": return "Wolaytta";
case "Globalization.ci_wal-ET": return "Wolaytta (Ethiopia)";
case "Globalization.ci_wo": return "Wolof";
case "Globalization.ci_wo-SN": return "Wolof (Senegal)";
case "Globalization.ci_x-IV": return "Invariant Language (Invariant Country)";
case "Globalization.ci_x-IV_mathan": return "Invariant Language (Invariant Country)";
case "Globalization.ci_xh": return "isiXhosa";
case "Globalization.ci_xh-ZA": return "isiXhosa (South Africa)";
case "Globalization.ci_xog": return "Soga";
case "Globalization.ci_xog-UG": return "Soga (Uganda)";
case "Globalization.ci_yav": return "Yangben";
case "Globalization.ci_yav-CM": return "Yangben (Cameroon)";
case "Globalization.ci_yi": return "Yiddish";
case "Globalization.ci_yi-001": return "Yiddish (World)";
case "Globalization.ci_yo": return "Yoruba";
case "Globalization.ci_yo-BJ": return "Yoruba (Benin)";
case "Globalization.ci_yo-NG": return "Yoruba (Nigeria)";
case "Globalization.ci_zgh": return "Standard Moroccan Tamazight";
case "Globalization.ci_zgh-Tfng": return "Standard Moroccan Tamazight (Tifinagh)";
case "Globalization.ci_zgh-Tfng-MA": return "Standard Moroccan Tamazight (Tifinagh, Morocco)";
case "Globalization.ci_zh": return "Chinese";
case "Globalization.ci_zh-CHS": return "Chinese (Simplified) Legacy";
case "Globalization.ci_zh-CHT": return "Chinese (Traditional) Legacy";
case "Globalization.ci_zh-CN": return "Chinese (Simplified, PRC)";
case "Globalization.ci_zh-CN_stroke": return "Chinese (Simplified, PRC)";
case "Globalization.ci_zh-Hans": return "Chinese (Simplified)";
case "Globalization.ci_zh-Hant": return "Chinese (Traditional)";
case "Globalization.ci_zh-HK": return "Chinese (Traditional, Hong Kong S.A.R.)";
case "Globalization.ci_zh-HK_radstr": return "Chinese (Traditional, Hong Kong S.A.R.)";
case "Globalization.ci_zh-MO": return "Chinese (Traditional, Macao S.A.R.)";
case "Globalization.ci_zh-MO_radstr": return "Chinese (Traditional, Macao S.A.R.)";
case "Globalization.ci_zh-MO_stroke": return "Chinese (Traditional, Macao S.A.R.)";
case "Globalization.ci_zh-SG": return "Chinese (Simplified, Singapore)";
case "Globalization.ci_zh-SG_stroke": return "Chinese (Simplified, Singapore)";
case "Globalization.ci_zh-TW": return "Chinese (Traditional, Taiwan)";
case "Globalization.ci_zh-TW_pronun": return "Chinese (Traditional, Taiwan)";
case "Globalization.ci_zh-TW_radstr": return "Chinese (Traditional, Taiwan)";
case "Globalization.ci_zu": return "isiZulu";
case "Globalization.ci_zu-ZA": return "isiZulu (South Africa)";
// ;------------------
// ;
// ;Total items: 129
// ;
case "Globalization.ri_029": return "Caribbean";
case "Globalization.ri_AE": return "U.A.E.";
case "Globalization.ri_AF": return "Afghanistan";
case "Globalization.ri_AL": return "Albania";
case "Globalization.ri_AM": return "Armenia";
case "Globalization.ri_AR": return "Argentina";
case "Globalization.ri_AT": return "Austria";
case "Globalization.ri_AU": return "Australia";
case "Globalization.ri_AZ": return "Azerbaijan";
case "Globalization.ri_BA": return "Bosnia and Herzegovina";
case "Globalization.ri_BD": return "Bangladesh";
case "Globalization.ri_BE": return "Belgium";
case "Globalization.ri_BG": return "Bulgaria";
case "Globalization.ri_BH": return "Bahrain";
case "Globalization.ri_BN": return "Brunei Darussalam";
case "Globalization.ri_BO": return "Bolivia";
case "Globalization.ri_BR": return "Brazil";
case "Globalization.ri_BY": return "Belarus";
case "Globalization.ri_BZ": return "Belize";
case "Globalization.ri_CA": return "Canada";
case "Globalization.ri_CH": return "Switzerland";
case "Globalization.ri_CL": return "Chile";
case "Globalization.ri_CN": return "People's Republic of China";
case "Globalization.ri_CO": return "Colombia";
case "Globalization.ri_CR": return "Costa Rica";
case "Globalization.ri_CS": return "Serbia and Montenegro (Former)";
case "Globalization.ri_CZ": return "Czech Republic";
case "Globalization.ri_DE": return "Germany";
case "Globalization.ri_DK": return "Denmark";
case "Globalization.ri_DO": return "Dominican Republic";
case "Globalization.ri_DZ": return "Algeria";
case "Globalization.ri_EC": return "Ecuador";
case "Globalization.ri_EE": return "Estonia";
case "Globalization.ri_EG": return "Egypt";
case "Globalization.ri_ER": return "Eritrea";
case "Globalization.ri_ES": return "Spain";
case "Globalization.ri_ET": return "Ethiopia";
case "Globalization.ri_FI": return "Finland";
case "Globalization.ri_FO": return "Faroe Islands";
case "Globalization.ri_FR": return "France";
case "Globalization.ri_GB": return "United Kingdom";
case "Globalization.ri_GE": return "Georgia";
case "Globalization.ri_GL": return "Greenland";
case "Globalization.ri_GR": return "Greece";
case "Globalization.ri_GT": return "Guatemala";
case "Globalization.ri_HK": return "Hong Kong S.A.R.";
case "Globalization.ri_HN": return "Honduras";
case "Globalization.ri_HR": return "Croatia";
case "Globalization.ri_HU": return "Hungary";
case "Globalization.ri_ID": return "Indonesia";
case "Globalization.ri_IE": return "Ireland";
case "Globalization.ri_IL": return "Israel";
case "Globalization.ri_IN": return "India";
case "Globalization.ri_IQ": return "Iraq";
case "Globalization.ri_IR": return "Iran";
case "Globalization.ri_IS": return "Iceland";
case "Globalization.ri_IT": return "Italy";
case "Globalization.ri_IV": return "Invariant Country";
case "Globalization.ri_JM": return "Jamaica";
case "Globalization.ri_JO": return "Jordan";
case "Globalization.ri_JP": return "Japan";
case "Globalization.ri_KE": return "Kenya";
case "Globalization.ri_KG": return "Kyrgyzstan";
case "Globalization.ri_KH": return "Cambodia";
case "Globalization.ri_KR": return "Korea";
case "Globalization.ri_KW": return "Kuwait";
case "Globalization.ri_KZ": return "Kazakhstan";
case "Globalization.ri_LA": return "Lao P.D.R.";
case "Globalization.ri_LB": return "Lebanon";
case "Globalization.ri_LI": return "Liechtenstein";
case "Globalization.ri_LK": return "Sri Lanka";
case "Globalization.ri_LT": return "Lithuania";
case "Globalization.ri_LU": return "Luxembourg";
case "Globalization.ri_LV": return "Latvia";
case "Globalization.ri_LY": return "Libya";
case "Globalization.ri_MA": return "Morocco";
case "Globalization.ri_MC": return "Principality of Monaco";
case "Globalization.ri_ME": return "Montenegro";
case "Globalization.ri_MK": return "Macedonia (FYROM)";
case "Globalization.ri_MN": return "Mongolia";
case "Globalization.ri_MO": return "Macao S.A.R.";
case "Globalization.ri_MT": return "Malta";
case "Globalization.ri_MV": return "Maldives";
case "Globalization.ri_MX": return "Mexico";
case "Globalization.ri_MY": return "Malaysia";
case "Globalization.ri_NG": return "Nigeria";
case "Globalization.ri_NI": return "Nicaragua";
case "Globalization.ri_NL": return "Netherlands";
case "Globalization.ri_NO": return "Norway";
case "Globalization.ri_NP": return "Nepal";
case "Globalization.ri_NZ": return "New Zealand";
case "Globalization.ri_OM": return "Oman";
case "Globalization.ri_PA": return "Panama";
case "Globalization.ri_PE": return "Peru";
case "Globalization.ri_PH": return "Philippines";
case "Globalization.ri_PK": return "Islamic Republic of Pakistan";
case "Globalization.ri_PL": return "Poland";
case "Globalization.ri_PR": return "Puerto Rico";
case "Globalization.ri_PT": return "Portugal";
case "Globalization.ri_PY": return "Paraguay";
case "Globalization.ri_QA": return "Qatar";
case "Globalization.ri_RO": return "Romania";
case "Globalization.ri_RS": return "Serbia";
case "Globalization.ri_RU": return "Russia";
case "Globalization.ri_RW": return "Rwanda";
case "Globalization.ri_SA": return "Saudi Arabia";
case "Globalization.ri_SE": return "Sweden";
case "Globalization.ri_SG": return "Singapore";
case "Globalization.ri_SI": return "Slovenia";
case "Globalization.ri_SK": return "Slovakia";
case "Globalization.ri_SN": return "Senegal";
case "Globalization.ri_SV": return "El Salvador";
case "Globalization.ri_SY": return "Syria";
case "Globalization.ri_TH": return "Thailand";
case "Globalization.ri_TJ": return "Tajikistan";
case "Globalization.ri_TM": return "Turkmenistan";
case "Globalization.ri_TN": return "Tunisia";
case "Globalization.ri_TR": return "Turkey";
case "Globalization.ri_TT": return "Trinidad and Tobago";
case "Globalization.ri_TW": return "Taiwan";
case "Globalization.ri_UA": return "Ukraine";
case "Globalization.ri_US": return "United States";
case "Globalization.ri_UY": return "Uruguay";
case "Globalization.ri_UZ": return "Uzbekistan";
case "Globalization.ri_VE": return "Bolivarian Republic of Venezuela";
case "Globalization.ri_VN": return "Vietnam";
case "Globalization.ri_YE": return "Yemen";
case "Globalization.ri_ZA": return "South Africa";
case "Globalization.ri_ZW": return "Zimbabwe";
#endif //!FEATURE_CORECLR

// ;------------------
// ; Encoding names:
// ;
// ;Total items: 147
// ;
case "Globalization.cp_1200": return "Unicode";
case "Globalization.cp_1201": return "Unicode (Big-Endian)";
case "Globalization.cp_65001": return "Unicode (UTF-8)";
case "Globalization.cp_65000": return "Unicode (UTF-7)";
case "Globalization.cp_12000": return "Unicode (UTF-32)";
case "Globalization.cp_12001": return "Unicode (UTF-32 Big-Endian)";
case "Globalization.cp_20127": return "US-ASCII";
case "Globalization.cp_28591": return "Western European (ISO)";

#if FEATURE_NON_UNICODE_CODE_PAGES
case "Globalization.cp_37": return "IBM EBCDIC (US-Canada)";
case "Globalization.cp_437": return "OEM United States";
case "Globalization.cp_500": return "IBM EBCDIC (International)";
case "Globalization.cp_708": return "Arabic (ASMO 708)";
case "Globalization.cp_720": return "Arabic (DOS)";
case "Globalization.cp_737": return "Greek (DOS)";
case "Globalization.cp_775": return "Baltic (DOS)";
case "Globalization.cp_850": return "Western European (DOS)";
case "Globalization.cp_852": return "Central European (DOS)";
case "Globalization.cp_855": return "OEM Cyrillic";
case "Globalization.cp_857": return "Turkish (DOS)";
case "Globalization.cp_858": return "OEM Multilingual Latin I";
case "Globalization.cp_860": return "Portuguese (DOS)";
case "Globalization.cp_861": return "Icelandic (DOS)";
case "Globalization.cp_862": return "Hebrew (DOS)";
case "Globalization.cp_863": return "French Canadian (DOS)";
case "Globalization.cp_864": return "Arabic (864)";
case "Globalization.cp_865": return "Nordic (DOS)";
case "Globalization.cp_866": return "Cyrillic (DOS)";
case "Globalization.cp_869": return "Greek, Modern (DOS)";
case "Globalization.cp_870": return "IBM EBCDIC (Multilingual Latin-2)";
case "Globalization.cp_874": return "Thai (Windows)";
case "Globalization.cp_875": return "IBM EBCDIC (Greek Modern)";
case "Globalization.cp_932": return "Japanese (Shift-JIS)";
case "Globalization.cp_936": return "Chinese Simplified (GB2312)";
case "Globalization.cp_949": return "Korean";
case "Globalization.cp_950": return "Chinese Traditional (Big5)";
case "Globalization.cp_1026": return "IBM EBCDIC (Turkish Latin-5)";
case "Globalization.cp_1047": return "IBM Latin-1";
case "Globalization.cp_1140": return "IBM EBCDIC (US-Canada-Euro)";
case "Globalization.cp_1141": return "IBM EBCDIC (Germany-Euro)";
case "Globalization.cp_1142": return "IBM EBCDIC (Denmark-Norway-Euro)";
case "Globalization.cp_1143": return "IBM EBCDIC (Finland-Sweden-Euro)";
case "Globalization.cp_1144": return "IBM EBCDIC (Italy-Euro)";
case "Globalization.cp_1145": return "IBM EBCDIC (Spain-Euro)";
case "Globalization.cp_1146": return "IBM EBCDIC (UK-Euro)";
case "Globalization.cp_1147": return "IBM EBCDIC (France-Euro)";
case "Globalization.cp_1148": return "IBM EBCDIC (International-Euro)";
case "Globalization.cp_1149": return "IBM EBCDIC (Icelandic-Euro)";
case "Globalization.cp_1250": return "Central European (Windows)";
case "Globalization.cp_1251": return "Cyrillic (Windows)";
case "Globalization.cp_1252": return "Western European (Windows)";
case "Globalization.cp_1253": return "Greek (Windows)";
case "Globalization.cp_1254": return "Turkish (Windows)";
case "Globalization.cp_1255": return "Hebrew (Windows)";
case "Globalization.cp_1256": return "Arabic (Windows)";
case "Globalization.cp_1257": return "Baltic (Windows)";
case "Globalization.cp_1258": return "Vietnamese (Windows)";
case "Globalization.cp_1361": return "Korean (Johab)";
case "Globalization.cp_10000": return "Western European (Mac)";
case "Globalization.cp_10001": return "Japanese (Mac)";
case "Globalization.cp_10002": return "Chinese Traditional (Mac)";
case "Globalization.cp_10003": return "Korean (Mac)";
case "Globalization.cp_10004": return "Arabic (Mac)";
case "Globalization.cp_10005": return "Hebrew (Mac)";
case "Globalization.cp_10006": return "Greek (Mac)";
case "Globalization.cp_10007": return "Cyrillic (Mac)";
case "Globalization.cp_10008": return "Chinese Simplified (Mac)";
case "Globalization.cp_10010": return "Romanian (Mac)";
case "Globalization.cp_10017": return "Ukrainian (Mac)";
case "Globalization.cp_10021": return "Thai (Mac)";
case "Globalization.cp_10029": return "Central European (Mac)";
case "Globalization.cp_10079": return "Icelandic (Mac)";
case "Globalization.cp_10081": return "Turkish (Mac)";
case "Globalization.cp_10082": return "Croatian (Mac)";
case "Globalization.cp_20000": return "Chinese Traditional (CNS)";
case "Globalization.cp_20001": return "TCA Taiwan";
case "Globalization.cp_20002": return "Chinese Traditional (Eten)";
case "Globalization.cp_20003": return "IBM5550 Taiwan";
case "Globalization.cp_20004": return "TeleText Taiwan";
case "Globalization.cp_20005": return "Wang Taiwan";
case "Globalization.cp_20105": return "Western European (IA5)";
case "Globalization.cp_20106": return "German (IA5)";
case "Globalization.cp_20107": return "Swedish (IA5)";
case "Globalization.cp_20108": return "Norwegian (IA5)";
case "Globalization.cp_20261": return "T.61";
case "Globalization.cp_20269": return "ISO-6937";
case "Globalization.cp_20273": return "IBM EBCDIC (Germany)";
case "Globalization.cp_20277": return "IBM EBCDIC (Denmark-Norway)";
case "Globalization.cp_20278": return "IBM EBCDIC (Finland-Sweden)";
case "Globalization.cp_20280": return "IBM EBCDIC (Italy)";
case "Globalization.cp_20284": return "IBM EBCDIC (Spain)";
case "Globalization.cp_20285": return "IBM EBCDIC (UK)";
case "Globalization.cp_20290": return "IBM EBCDIC (Japanese katakana)";
case "Globalization.cp_20297": return "IBM EBCDIC (France)";
case "Globalization.cp_20420": return "IBM EBCDIC (Arabic)";
case "Globalization.cp_20423": return "IBM EBCDIC (Greek)";
case "Globalization.cp_20424": return "IBM EBCDIC (Hebrew)";
case "Globalization.cp_20833": return "IBM EBCDIC (Korean Extended)";
case "Globalization.cp_20838": return "IBM EBCDIC (Thai)";
case "Globalization.cp_20866": return "Cyrillic (KOI8-R)";
case "Globalization.cp_20871": return "IBM EBCDIC (Icelandic)";
case "Globalization.cp_20880": return "IBM EBCDIC (Cyrillic Russian)";
case "Globalization.cp_20905": return "IBM EBCDIC (Turkish)";
case "Globalization.cp_20924": return "IBM Latin-1";
case "Globalization.cp_20932": return "Japanese (JIS 0208-1990 and 0212-1990)";
case "Globalization.cp_20936": return "Chinese Simplified (GB2312-80)";
case "Globalization.cp_20949": return "Korean Wansung";
case "Globalization.cp_21025": return "IBM EBCDIC (Cyrillic Serbian-Bulgarian)";
case "Globalization.cp_21027": return "Ext Alpha Lowercase";
case "Globalization.cp_21866": return "Cyrillic (KOI8-U)";
case "Globalization.cp_28592": return "Central European (ISO)";
case "Globalization.cp_28593": return "Latin 3 (ISO)";
case "Globalization.cp_28594": return "Baltic (ISO)";
case "Globalization.cp_28595": return "Cyrillic (ISO)";
case "Globalization.cp_28596": return "Arabic (ISO)";
case "Globalization.cp_28597": return "Greek (ISO)";
case "Globalization.cp_28598": return "Hebrew (ISO-Visual)";
case "Globalization.cp_28599": return "Turkish (ISO)";
case "Globalization.cp_28603": return "Estonian (ISO)";
case "Globalization.cp_28605": return "Latin 9 (ISO)";
case "Globalization.cp_29001": return "Europa";
case "Globalization.cp_38598": return "Hebrew (ISO-Logical)";
case "Globalization.cp_50000": return "User Defined";
case "Globalization.cp_50220": return "Japanese (JIS)";
case "Globalization.cp_50221": return "Japanese (JIS-Allow 1 byte Kana)";
case "Globalization.cp_50222": return "Japanese (JIS-Allow 1 byte Kana - SO/SI)";
case "Globalization.cp_50225": return "Korean (ISO)";
case "Globalization.cp_50227": return "Chinese Simplified (ISO-2022)";
case "Globalization.cp_50229": return "Chinese Traditional (ISO-2022)";
case "Globalization.cp_50930": return "IBM EBCDIC (Japanese and Japanese Katakana)";
case "Globalization.cp_50931": return "IBM EBCDIC (Japanese and US-Canada)";
case "Globalization.cp_50933": return "IBM EBCDIC (Korean and Korean Extended)";
case "Globalization.cp_50935": return "IBM EBCDIC (Simplified Chinese)";
case "Globalization.cp_50937": return "IBM EBCDIC (Traditional Chinese)";
case "Globalization.cp_50939": return "IBM EBCDIC (Japanese and Japanese-Latin)";
case "Globalization.cp_51932": return "Japanese (EUC)";
case "Globalization.cp_51936": return "Chinese Simplified (EUC)";
case "Globalization.cp_51949": return "Korean (EUC)";
case "Globalization.cp_52936": return "Chinese Simplified (HZ)";
case "Globalization.cp_54936": return "Chinese Simplified (GB18030)";
case "Globalization.cp_57002": return "ISCII Devanagari";
case "Globalization.cp_57003": return "ISCII Bengali";
case "Globalization.cp_57004": return "ISCII Tamil";
case "Globalization.cp_57005": return "ISCII Telugu";
case "Globalization.cp_57006": return "ISCII Assamese";
case "Globalization.cp_57007": return "ISCII Oriya";
case "Globalization.cp_57008": return "ISCII Kannada";
case "Globalization.cp_57009": return "ISCII Malayalam";
case "Globalization.cp_57010": return "ISCII Gujarati";
case "Globalization.cp_57011": return "ISCII Punjabi";
#endif // FEATURE_NON_UNICODE_CODE_PAGES
#endif // INCLUDE_DEBUG

// ;------------------

}

return id;

} }
