//
// Resource strings referenced by the code.
//
// Copyright 2014 Xamarin Inc
//
// Use the following script to extract strings from .NET strings.resx:
//
// var d = XDocument.Load ("Strings.resx");
// foreach (var j in d.XPathSelectElements ("/root/data")){ var v = j.XPathSelectElement ("value"); Console.WriteLine ("\tpublic const string {0}=\"{1}\";", j.Attribute ("name").Value, v.Value); }
//
partial class SR
{
	public static object GetObject (string name)
	{
		// The PropertyGrid code in WinForms and the corresponding tests
		// rely on PropertyCategoryDefault returning the correct value.
		// Handle this special case until we have proper resource lookup. 
		if (name == "PropertyCategoryDefault")
			return "Misc";

		return name;
	}

	public const string AlternationCantCapture="Alternation conditions do not capture and cannot be named.";
	public const string AlternationCantHaveComment="Alternation conditions cannot be comments.";
	public const string Arg_InvalidArrayType="Target array type is not compatible with the type of items in the collection.";
	public const string Arg_RankMultiDimNotSupported="Only single dimensional arrays are supported for the requested action.";
	public const string BadClassInCharRange="Cannot include class {0} in character range.";
	public const string BeginIndexNotNegative="Start index cannot be less than 0 or greater than input length.";
	public const string CapnumNotZero="Capture number cannot be zero.";
	public const string CaptureGroupOutOfRange="Capture group numbers must be less than or equal to Int32.MaxValue.";
	public const string CountTooSmall="Count cannot be less than -1.";
	public const string EnumNotStarted="Enumeration has either not started or has already finished.";
	public const string IllegalCondition="Illegal conditional (?(...)) expression.";
	public const string IllegalEndEscape="Illegal \\ at end of pattern.";
	public const string IllegalRange="Illegal {x,y} with x > y.";
	public const string IncompleteSlashP="Incomplete \\{X} character escape.";
	public const string InternalError="Internal error in ScanRegex.";
	public const string InvalidGroupName="Invalid group name: Group names must begin with a word character.";
	public const string LengthNotNegative="Length cannot be less than 0 or exceed input length.";
	public const string MakeException="parsing '{0}' - {1}";
	public const string MalformedNameRef="Malformed \\k<...> named back reference.";
	public const string MalformedReference="(?({0}) ) malformed.";
	public const string MalformedSlashP="Malformed \\p{X} character escape.";
	public const string MissingControl="Missing control character.";
	public const string NestedQuantify="Nested quantifier {0}.";
	public const string NoResultOnFailed="Result cannot be called on a failed Match.";
	public const string NotEnoughParens="Not enough )'s.";
	public const string OnlyAllowedOnce="This operation is only allowed once per object.";
	public const string QuantifyAfterNothing="Quantifier {x,y} following nothing.";
	public const string RegexMatchTimeoutException_Occurred="The RegEx engine has timed out while trying to match a pattern to an input string. This can occur for many reasons, including very large inputs or excessive backtracking caused by nested quantifiers, back-references and other factors.";
	public const string ReplacementError="Replacement pattern error.";
	public const string ReversedCharRange="[x-y] range in reverse order.";
	public const string SubtractionMustBeLast="A subtraction must be the last element in a character class.";
	public const string TooFewHex="Insufficient hexadecimal digits.";
	public const string TooManyAlternates="Too many | in (?()|).";
	public const string TooManyParens="Too many )'s.";
	public const string UndefinedBackref="Reference to undefined group number {0}.";
	public const string UndefinedNameRef="Reference to undefined group name {0}.";
	public const string UndefinedReference="(?({0}) ) reference to undefined group.";
	public const string UnexpectedOpcode="Unexpected opcode in regular expression generation: {0}.";
	public const string UnimplementedState="Unimplemented state.";
	public const string UnknownProperty="Unknown property '{0}'.";
	public const string UnrecognizedControl="Unrecognized control character.";
	public const string UnrecognizedEscape="Unrecognized escape sequence {0}.";
	public const string UnrecognizedGrouping="Unrecognized grouping construct.";
	public const string UnterminatedBracket="Unterminated [] set.";
	public const string UnterminatedComment="Unterminated (?#...) comment.";
	
	public const string ArgumentNull_ArrayWithNullElements = "ArgumentNull_ArrayWithNullElements";
	public const string Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";
	public const string Arg_InsufficientSpace = "Arg_InsufficientSpace";
	public const string Arg_MultiRank = "Arg_MultiRank";
	public const string Arg_NonZeroLowerBound = "Arg_NonZeroLowerBound";
	public const string Arg_WrongType = "Arg_WrongType";
	public const string ArgumentOutOfRange_Index = "ArgumentOutOfRange_Index";
	public const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
	public const string ArgumentOutOfRange_NeedNonNegNumRequired = "ArgumentOutOfRange_NeedNonNegNumRequired";
	public const string ArgumentOutOfRange_SmallCapacity = "ArgumentOutOfRange_SmallCapacity";
	public const string Argument_AddingDuplicate = "Argument_AddingDuplicate";
	public const string Argument_ImplementIComparable = "Argument_ImplementIComparable";
	public const string Argument_InvalidOffLen = "Argument_InvalidOffLen";
	public const string ExternalLinkedListNode = "ExternalLinkedListNode";
	public const string IndexOutOfRange = "IndexOutOfRange";
	public const string InvalidOperation_CannotRemoveFromStackOrQueue = "InvalidOperation_CannotRemoveFromStackOrQueue";
	public const string InvalidOperation_EmptyCollection = "InvalidOperation_EmptyCollection";
	public const string InvalidOperation_EmptyQueue = "InvalidOperation_EmptyQueue";
	public const string InvalidOperation_EmptyStack = "InvalidOperation_EmptyStack";
	public const string InvalidOperation_EnumEnded = "InvalidOperation_EnumEnded";
	public const string InvalidOperation_EnumFailedVersion = "InvalidOperation_EnumFailedVersion";
	public const string InvalidOperation_EnumNotStarted = "InvalidOperation_EnumNotStarted";
	public const string InvalidOperation_EnumOpCantHappen = "InvalidOperation_EnumOpCantHappen";
	public const string Invalid_Array_Type = "Invalid_Array_Type";
	public const string LinkedListEmpty = "LinkedListEmpty";
	public const string LinkedListNodeIsAttached = "LinkedListNodeIsAttached";
	public const string NotSupported_KeyCollectionSet = "NotSupported_KeyCollectionSet";
	public const string NotSupported_SortedListNestedWrite = "NotSupported_SortedListNestedWrite";
	public const string NotSupported_ValueCollectionSet = "NotSupported_ValueCollectionSet";
	public const string Serialization_InvalidOnDeser = "Serialization_InvalidOnDeser";
	public const string Serialization_MismatchedCount = "Serialization_MismatchedCount";
	public const string Serialization_MissingValues = "Serialization_MissingValues";
	public const string IllegalDefaultRegexMatchTimeoutInAppDomain = "IllegalDefaultRegexMatchTimeoutInAppDomain";
	public const string InvalidNullEmptyArgument = "InvalidNullEmptyArgument";

	public const string BlockingCollection_Add_ConcurrentCompleteAdd = "BlockingCollection_Add_ConcurrentCompleteAdd";
	public const string BlockingCollection_Add_Failed = "BlockingCollection_Add_Failed";
	public const string BlockingCollection_CantAddAnyWhenCompleted = "BlockingCollection_CantAddAnyWhenCompleted";
	public const string BlockingCollection_CantTakeAnyWhenAllDone = "BlockingCollection_CantTakeAnyWhenAllDone";
	public const string BlockingCollection_CantTakeWhenDone = "BlockingCollection_CantTakeWhenDone";
	public const string BlockingCollection_Completed = "BlockingCollection_Completed";
	public const string BlockingCollection_CopyTo_IncorrectType = "BlockingCollection_CopyTo_IncorrectType";
	public const string BlockingCollection_CopyTo_MultiDim = "BlockingCollection_CopyTo_MultiDim";
	public const string BlockingCollection_CopyTo_NonNegative = "BlockingCollection_CopyTo_NonNegative";
	public const string BlockingCollection_CopyTo_TooManyElems = "BlockingCollection_CopyTo_TooManyElems";
	public const string BlockingCollection_Disposed = "BlockingCollection_Disposed";
	public const string BlockingCollection_Take_CollectionModified = "BlockingCollection_Take_CollectionModified";
	public const string BlockingCollection_TimeoutInvalid = "BlockingCollection_TimeoutInvalid";
	public const string BlockingCollection_ValidateCollectionsArray_DispElems = "BlockingCollection_ValidateCollectionsArray_DispElems";
	public const string BlockingCollection_ValidateCollectionsArray_LargeSize = "BlockingCollection_ValidateCollectionsArray_LargeSize";
	public const string BlockingCollection_ValidateCollectionsArray_NullElems = "BlockingCollection_ValidateCollectionsArray_NullElems";
	public const string BlockingCollection_ValidateCollectionsArray_ZeroSize = "BlockingCollection_ValidateCollectionsArray_ZeroSize";
	public const string BlockingCollection_ctor_BoundedCapacityRange = "BlockingCollection_ctor_BoundedCapacityRange";
	public const string BlockingCollection_ctor_CountMoreThanCapacity = "BlockingCollection_ctor_CountMoreThanCapacity";
	public const string Common_OperationCanceled = "Common_OperationCanceled";
	public const string ConcurrentBag_CopyTo_ArgumentNullException = "ConcurrentBag_CopyTo_ArgumentNullException";
	public const string ConcurrentBag_CopyTo_ArgumentOutOfRangeException = "ConcurrentBag_CopyTo_ArgumentOutOfRangeException";
	public const string ConcurrentBag_Ctor_ArgumentNullException = "ConcurrentBag_Ctor_ArgumentNullException";
	public const string ConcurrentCollection_SyncRoot_NotSupported = "ConcurrentCollection_SyncRoot_NotSupported";

	public const string ArrayConverterText = "{0} Array";
	public const string Async_AsyncEventArgs_Cancelled = "Async_AsyncEventArgs_Cancelled";
	public const string Async_AsyncEventArgs_Error = "Async_AsyncEventArgs_Error";
	public const string Async_AsyncEventArgs_UserState = "Async_AsyncEventArgs_UserState";
	public const string Async_ExceptionOccurred = "Async_ExceptionOccurred";
	public const string Async_NullDelegate = "Async_NullDelegate";
	public const string Async_OperationAlreadyCompleted = "Async_OperationAlreadyCompleted";
	public const string Async_OperationCancelled = "Async_OperationCancelled";
	public const string Async_ProgressChangedEventArgs_ProgressPercentage = "Async_ProgressChangedEventArgs_ProgressPercentage";
	public const string Async_ProgressChangedEventArgs_UserState = "Async_ProgressChangedEventArgs_UserState";
	public const string BackgroundWorker_CancellationPending = "BackgroundWorker_CancellationPending";
	public const string BackgroundWorker_Desc = "BackgroundWorker_Desc";
	public const string BackgroundWorker_DoWork = "BackgroundWorker_DoWork";
	public const string BackgroundWorker_DoWorkEventArgs_Argument = "BackgroundWorker_DoWorkEventArgs_Argument";
	public const string BackgroundWorker_DoWorkEventArgs_Result = "BackgroundWorker_DoWorkEventArgs_Result";
	public const string BackgroundWorker_IsBusy = "BackgroundWorker_IsBusy";
	public const string BackgroundWorker_ProgressChanged = "BackgroundWorker_ProgressChanged";
	public const string BackgroundWorker_RunWorkerCompleted = "BackgroundWorker_RunWorkerCompleted";
	public const string BackgroundWorker_WorkerAlreadyRunning = "BackgroundWorker_WorkerAlreadyRunning";
	public const string BackgroundWorker_WorkerDoesntReportProgress = "BackgroundWorker_WorkerDoesntReportProgress";
	public const string BackgroundWorker_WorkerDoesntSupportCancellation = "BackgroundWorker_WorkerDoesntSupportCancellation";
	public const string BackgroundWorker_WorkerReportsProgress = "BackgroundWorker_WorkerReportsProgress";
	public const string BackgroundWorker_WorkerSupportsCancellation = "BackgroundWorker_WorkerSupportsCancellation";
	public const string CHECKOUTCanceled = "CHECKOUTCanceled";
	public const string CantModifyListSortDescriptionCollection = "CantModifyListSortDescriptionCollection";
	public const string CollectionConverterText = "(Collection)";
	public const string ConvertFromException = "{0} cannot convert from {1}.";
	public const string ConvertInvalidPrimitive = "{0} is not a valid value for {1}.";
	public const string ConvertToException = "'{0}' is unable to convert '{1}' to '{2}'.";
	public const string CultureInfoConverterDefaultCultureString = "(Default)";
	public const string CultureInfoConverterInvalidCulture = "The {0} culture cannot be converted to a CultureInfo object on this computer.";
	public const string DuplicateComponentName = "Duplicate component name '{0}'.  Component names must be unique and case-insensitive.";
	public const string EnumConverterInvalidValue = "The value '{0}' is not a valid value for the enum '{1}'.";
	public const string ErrorBadExtenderType = "ErrorBadExtenderType";
	public const string ErrorInvalidEventHandler = "ErrorInvalidEventHandler";
	public const string ErrorInvalidEventType = "ErrorInvalidEventType";
	public const string ErrorInvalidPropertyType = "ErrorInvalidPropertyType";
	public const string ErrorInvalidServiceInstance = "ErrorInvalidServiceInstance";
	public const string ErrorMissingEventAccessors = "ErrorMissingEventAccessors";
	public const string ErrorMissingPropertyAccessors = "ErrorMissingPropertyAccessors";
	public const string ErrorPropertyAccessorException = "ErrorPropertyAccessorException";
	public const string ErrorServiceExists = "The service {0} already exists in the service container.";
	public const string ISupportInitializeDescr = "ISupportInitializeDescr";
	public const string InstanceCreationEditorDefaultText = "InstanceCreationEditorDefaultText";
	public const string InstanceDescriptorCannotBeStatic = "InstanceDescriptorCannotBeStatic";
	public const string InstanceDescriptorLengthMismatch = "InstanceDescriptorLengthMismatch";
	public const string InstanceDescriptorMustBeReadable = "InstanceDescriptorMustBeReadable";
	public const string InstanceDescriptorMustBeStatic = "InstanceDescriptorMustBeStatic";
	public const string InvalidArgument = "InvalidArgument";
	public const string InvalidEnumArgument = "The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.";
	public const string InvalidMemberName = "InvalidMemberName";
	public const string InvalidNullArgument = "InvalidNullArgument";
	public const string LicExceptionTypeAndInstance = "LicExceptionTypeAndInstance";
	public const string LicExceptionTypeOnly = "LicExceptionTypeOnly";
	public const string LicMgrAlreadyLocked = "LicMgrAlreadyLocked";
	public const string LicMgrContextCannotBeChanged = "LicMgrContextCannotBeChanged";
	public const string LicMgrDifferentUser = "LicMgrDifferentUser";
	public const string MaskedTextProviderInvalidCharError = "MaskedTextProviderInvalidCharError";
	public const string MaskedTextProviderMaskInvalidChar  = "MaskedTextProviderMaskInvalidChar ";
	public const string MaskedTextProviderMaskNullOrEmpty = "MaskedTextProviderMaskNullOrEmpty";
	public const string MaskedTextProviderPasswordAndPromptCharError = "MaskedTextProviderPasswordAndPromptCharError";
	public const string MemberRelationshipService_RelationshipNotSupported = "MemberRelationshipService_RelationshipNotSupported";
	public const string MetaExtenderName = "MetaExtenderName";
	public const string MultilineStringConverterText = "(Text)";
	public const string NullableConverterBadCtorArg = "NullableConverterBadCtorArg";
	public const string PropertyCategoryAsynchronous = "PropertyCategoryAsynchronous";
	public const string PropertyTabAttributeArrayLengthMismatch = "PropertyTabAttributeArrayLengthMismatch";
	public const string PropertyTabAttributeBadPropertyTabScope = "PropertyTabAttributeBadPropertyTabScope";
	public const string PropertyTabAttributeParamsBothNull = "PropertyTabAttributeParamsBothNull";
	public const string PropertyTabAttributeTypeLoadException = "PropertyTabAttributeTypeLoadException";
	public const string ToStringNull = "(null)";
	public const string ToolboxItemAttributeFailedGetType = "ToolboxItemAttributeFailedGetType";
	public const string TypeDescriptorAlreadyAssociated = "TypeDescriptorAlreadyAssociated";
	public const string TypeDescriptorArgsCountMismatch = "TypeDescriptorArgsCountMismatch";
	public const string TypeDescriptorExpectedElementType = "TypeDescriptorExpectedElementType";
	public const string TypeDescriptorProviderError = "TypeDescriptorProviderError";
	public const string TypeDescriptorSameAssociation = "TypeDescriptorSameAssociation";
	public const string TypeDescriptorUnsupportedRemoteObject = "TypeDescriptorUnsupportedRemoteObject";
	public const string toStringNone = "(none)";

	public const string Arg_EnumIllegalVal = "Arg_EnumIllegalVal";
	public const string Argument_InvalidClassAttribute = "Argument_InvalidClassAttribute";
	public const string Argument_InvalidPermissionState = "Argument_InvalidPermissionState";
	public const string Argument_WrongType = "Argument_WrongType";		
}
