//
// This file was generated by resx2sr tool
//

partial class SR
{
	public const string BlockingCollection_Add_ConcurrentCompleteAdd = "CompleteAdding may not be used concurrently with additions to the collection.";
	public const string BlockingCollection_Add_Failed = "The underlying collection didn't accept the item.";
	public const string BlockingCollection_CantAddAnyWhenCompleted = "At least one of the specified collections is marked as complete with regards to additions.";
	public const string BlockingCollection_CantTakeAnyWhenAllDone = "All collections are marked as complete with regards to additions.";
	public const string BlockingCollection_CantTakeWhenDone = "The collection argument is empty and has been marked as complete with regards to additions.";
	public const string BlockingCollection_Completed = "The collection has been marked as complete with regards to additions.";
	public const string BlockingCollection_CopyTo_IncorrectType = "The array argument is of the incorrect type.";
	public const string BlockingCollection_CopyTo_MultiDim = "The array argument is multidimensional.";
	public const string BlockingCollection_CopyTo_NonNegative = "The index argument must be greater than or equal zero.";
	public const string Collection_CopyTo_TooManyElems = "The number of elements in the collection is greater than the available space from index to the end of the destination array.";
	public const string BlockingCollection_ctor_BoundedCapacityRange = "The boundedCapacity argument must be positive.";
	public const string BlockingCollection_ctor_CountMoreThanCapacity = "The collection argument contains more items than are allowed by the boundedCapacity.";
	public const string BlockingCollection_Disposed = "The collection has been disposed.";
	public const string BlockingCollection_Take_CollectionModified = "The underlying collection was modified from outside of the BlockingCollection<T>.";
	public const string BlockingCollection_TimeoutInvalid = "The specified timeout must represent a value between -1 and {0}, inclusive.";
	public const string BlockingCollection_ValidateCollectionsArray_DispElems = "The collections argument contains at least one disposed element.";
	public const string BlockingCollection_ValidateCollectionsArray_LargeSize = "The collections length is greater than the supported range for 32 bit machine.";
	public const string BlockingCollection_ValidateCollectionsArray_NullElems = "The collections argument contains at least one null element.";
	public const string BlockingCollection_ValidateCollectionsArray_ZeroSize = "The collections argument is a zero-length array.";
	public const string Common_OperationCanceled = "The operation was canceled.";
	public const string ConcurrentBag_Ctor_ArgumentNullException = "The collection argument is null.";
	public const string ConcurrentBag_CopyTo_ArgumentNullException = "The array argument is null.";
	public const string Collection_CopyTo_ArgumentOutOfRangeException = "The index argument must be greater than or equal zero.";
	public const string ConcurrentCollection_SyncRoot_NotSupported = "The SyncRoot property may not be used for the synchronization of concurrent collections.";
	public const string ConcurrentDictionary_ArrayIncorrectType = "The array is multidimensional, or the type parameter for the set cannot be cast automatically to the type of the destination array.";
	public const string ConcurrentDictionary_SourceContainsDuplicateKeys = "The source argument contains duplicate keys.";
	public const string ConcurrentDictionary_ConcurrencyLevelMustBePositive = "The concurrencyLevel argument must be positive.";
	public const string ConcurrentDictionary_CapacityMustNotBeNegative = "The capacity argument must be greater than or equal to zero.";
	public const string ConcurrentDictionary_IndexIsNegative = "The index argument is less than zero.";
	public const string ConcurrentDictionary_ArrayNotLargeEnough = "The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.";
	public const string ConcurrentDictionary_KeyAlreadyExisted = "The key already existed in the dictionary.";
	public const string ConcurrentDictionary_ItemKeyIsNull = "TKey is a reference type and item.Key is null.";
	public const string ConcurrentDictionary_TypeOfKeyIncorrect = "The key was of an incorrect type for this dictionary.";
	public const string ConcurrentDictionary_TypeOfValueIncorrect = "The value was of an incorrect type for this dictionary.";
	public const string ConcurrentStack_PushPopRange_CountOutOfRange = "The count argument must be greater than or equal to zero.";
	public const string ConcurrentStack_PushPopRange_InvalidCount = "The sum of the startIndex and count arguments must be less than or equal to the collection's Count.";
	public const string ConcurrentStack_PushPopRange_StartOutOfRange = "The startIndex argument must be greater than or equal to zero.";
	public const string Partitioner_DynamicPartitionsNotSupported = "Dynamic partitions are not supported by this partitioner.";
	public const string PartitionerStatic_CanNotCallGetEnumeratorAfterSourceHasBeenDisposed = "Can not call GetEnumerator on partitions after the source enumerable is disposed";
	public const string PartitionerStatic_CurrentCalledBeforeMoveNext = "MoveNext must be called at least once before calling Current.";
	public const string ConcurrentBag_Enumerator_EnumerationNotStartedOrAlreadyFinished = "Enumeration has either not started or has already finished.";
	public const string Arg_KeyNotFoundWithKey = "The given key '{0}' was not present in the dictionary.";
	public const string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";
	public const string Arg_WrongType = "The value '{0}' is not of type '{1}' and cannot be used in this generic collection.";
	public const string Arg_ArrayPlusOffTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
	public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
	public const string ArgumentOutOfRange_SmallCapacity = "capacity was less than the current size.";
	public const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
	public const string Argument_AddingDuplicate = "An item with the same key has already been added. Key: {0}";
	public const string InvalidOperation_EmptyQueue = "Queue empty.";
	public const string InvalidOperation_EnumOpCantHappen = "Enumeration has either not started or has already finished.";
	public const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
	public const string InvalidOperation_EmptyStack = "Stack empty.";
	public const string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";
	public const string InvalidOperation_EnumEnded = "Enumeration already finished.";
	public const string NotSupported_KeyCollectionSet = "Mutating a key collection derived from a dictionary is not allowed.";
	public const string NotSupported_ValueCollectionSet = "Mutating a value collection derived from a dictionary is not allowed.";
	public const string Arg_ArrayLengthsDiffer = "Array lengths must be the same.";
	public const string Arg_BitArrayTypeUnsupported = "Only supported array types for CopyTo on BitArrays are Boolean[], Int32[] and Byte[].";
	public const string Arg_HSCapacityOverflow = "HashSet capacity is too big.";
	public const string Arg_HTCapacityOverflow = "Hashtable's capacity overflowed and went negative. Check load factor, capacity and the current size of the table.";
	public const string Arg_InsufficientSpace = "Insufficient space in the target location to copy the information.";
	public const string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";
	public const string Argument_ArrayTooLarge = "The input array length must not exceed Int32.MaxValue / {0}. Otherwise BitArray.Length would exceed Int32.MaxValue.";
	public const string Argument_InvalidArrayType = "Target array type is not compatible with the type of items in the collection.";
	public const string ArgumentOutOfRange_BiggerThanCollection = "Must be less than or equal to the size of the collection.";
	public const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
	public const string ExternalLinkedListNode = "The LinkedList node does not belong to current LinkedList.";
	public const string LinkedListEmpty = "The LinkedList is empty.";
	public const string LinkedListNodeIsAttached = "The LinkedList node already belongs to a LinkedList.";
	public const string NotSupported_SortedListNestedWrite = "This operation is not supported on SortedList nested types because they require modifying the original SortedList.";
	public const string SortedSet_LowerValueGreaterThanUpperValue = "Must be less than or equal to upperValue.";
	public const string Serialization_InvalidOnDeser = "OnDeserialization method was called while the object was not being deserialized.";
	public const string Serialization_MismatchedCount = "The serialized Count information doesn't match the number of items.";
	public const string Serialization_MissingKeys = "The keys for this dictionary are missing.";
	public const string Serialization_MissingValues = "The values for this dictionary are missing.";
	public const string ArgumentException_BufferNotFromPool = "The buffer is not associated with this pool and may not be returned to it.";
	public const string net_uri_BadAuthority = "Invalid URI: The Authority/Host could not be parsed.";
	public const string net_uri_BadAuthorityTerminator = "Invalid URI: The Authority/Host cannot end with a backslash character ('\\\\').";
	public const string net_uri_BadFormat = "Invalid URI: The format of the URI could not be determined.";
	public const string net_uri_NeedFreshParser = "The URI parser instance passed into 'uriParser' parameter is already registered with the scheme name '{0}'.";
	public const string net_uri_AlreadyRegistered = "A URI scheme name '{0}' already has a registered custom parser.";
	public const string net_uri_BadHostName = "Invalid URI: The hostname could not be parsed.";
	public const string net_uri_BadPort = "Invalid URI: Invalid port specified.";
	public const string net_uri_BadScheme = "Invalid URI: The URI scheme is not valid.";
	public const string net_uri_BadString = "Invalid URI: There is an invalid sequence in the string.";
	public const string net_uri_BadUserPassword = "Invalid URI: The username:password construct is badly formed.";
	public const string net_uri_CannotCreateRelative = "A relative URI cannot be created because the 'uriString' parameter represents an absolute URI.";
	public const string net_uri_SchemeLimit = "Invalid URI: The Uri scheme is too long.";
	public const string net_uri_EmptyUri = "Invalid URI: The URI is empty.";
	public const string net_uri_InvalidUriKind = "The value '{0}' passed for the UriKind parameter is invalid.";
	public const string net_uri_MustRootedPath = "Invalid URI: A Dos path must be rooted, for example, 'c:\\\\'.";
	public const string net_uri_NotAbsolute = "This operation is not supported for a relative URI.";
	public const string net_uri_PortOutOfRange = "A derived type '{0}' has reported an invalid value for the Uri port '{1}'.";
	public const string net_uri_SizeLimit = "Invalid URI: The Uri string is too long.";
	public const string net_uri_UserDrivenParsing = "A derived type '{0}' is responsible for parsing this Uri instance. The base implementation must not be used.";
	public const string net_uri_NotJustSerialization = "UriComponents.SerializationInfoString must not be combined with other UriComponents.";
	public const string net_uri_BadUnicodeHostForIdn = "An invalid Unicode character by IDN standards was specified in the host.";
	public const string Argument_ExtraNotValid = "Extra portion of URI not valid.";
	public const string Argument_InvalidUriSubcomponent = "The subcomponent, {0}, of this uri is not valid.";
	public const string IO_EOF_ReadBeyondEOF = "Unable to read beyond the end of the stream.";
	public const string BaseStream_Invalid_Not_Open = "The BaseStream is only available when the port is open.";
	public const string PortNameEmpty_String = "The PortName cannot be empty.";
	public const string Port_not_open = "The port is closed.";
	public const string Port_already_open = "The port is already open.";
	public const string Cant_be_set_when_open = "'{0}' cannot be set while the port is open.";
	public const string Max_Baud = "The maximum baud rate for the device is {0}.";
	public const string In_Break_State = "The port is in the break state and cannot be written to.";
	public const string Write_timed_out = "The write timed out.";
	public const string CantSetRtsWithHandshaking = "RtsEnable cannot be accessed if Handshake is set to RequestToSend or RequestToSendXOnXOff.";
	public const string NotSupportedEncoding = "SerialPort does not support encoding '{0}'.  The supported encodings include ASCIIEncoding, UTF8Encoding, UnicodeEncoding, UTF32Encoding, and most single or double byte code pages.  For a complete list please see the documentation.";
	public const string Arg_InvalidSerialPort = "The given port name does not start with COM/com or does not resolve to a valid serial port.";
	public const string Arg_InvalidSerialPortExtended = "The given port name is invalid.  It may be a valid port, but not a serial port.";
	public const string ArgumentOutOfRange_Bounds_Lower_Upper = "Argument must be between {0} and {1}.";
	public const string ArgumentOutOfRange_Enum = "Enum value was out of legal range.";
	public const string ArgumentOutOfRange_NeedNonNegNumRequired = "Non-negative number required.";
	public const string ArgumentOutOfRange_NeedPosNum = "Positive number required.";
	public const string ArgumentOutOfRange_Timeout = "The timeout must be greater than or equal to -1.";
	public const string ArgumentOutOfRange_WriteTimeout = "The timeout must be either a positive number or -1.";
	public const string IndexOutOfRange_IORaceCondition = "Probable I/O race condition detected while copying memory.  The I/O package is not thread safe by default.  In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods.  This also applies to classes like StreamWriter and StreamReader.";
	public const string IO_OperationAborted = "The I/O operation has been aborted because of either a thread exit or an application request.";
	public const string NotSupported_UnseekableStream = "Stream does not support seeking.";
	public const string ObjectDisposed_StreamClosed = "Can not access a closed Stream.";
	public const string InvalidNullEmptyArgument = "Argument {0} cannot be null or zero-length.";
	public const string Arg_WrongAsyncResult = "IAsyncResult object did not come from the corresponding async method on this type.";
	public const string InvalidOperation_EndReadCalledMultiple = "EndRead can only be called once for each asynchronous operation.";
	public const string InvalidOperation_EndWriteCalledMultiple = "EndWrite can only be called once for each asynchronous operation.";
	public const string IO_PortNotFound = "The specified port does not exist.";
	public const string IO_PortNotFoundFileName = "The port '{0}' does not exist.";
	public const string UnauthorizedAccess_IODenied_NoPathName = "Access to the port is denied.";
	public const string IO_PathTooLong = "The specified port name is too long.  The port name must be less than 260 characters.";
	public const string IO_SharingViolation_NoFileName = "The process cannot access the port because it is being used by another process.";
	public const string IO_SharingViolation_File = "The process cannot access the port '{0}' because it is being used by another process.";
	public const string UnauthorizedAccess_IODenied_Path = "Access to the port '{0}' is denied.";
	public const string PlatformNotSupported_IOPorts = "System.IO.Ports is currently only supported on Windows.";
	public const string PlatformNotSupported_SerialPort_GetPortNames = "Enumeration of serial port names is not supported on the current platform.";
	public const string IO_PathTooLong_Path = "The specified port name '{0}' is too long.  The port name must be less than 260 characters.";
	public const string net_log_listener_delegate_exception = "Sending 500 response, AuthenticationSchemeSelectorDelegate threw an exception: {0}.";
	public const string net_log_listener_unsupported_authentication_scheme = "Received a request with an unsupported authentication scheme, Authorization:{0} SupportedSchemes:{1}.";
	public const string net_log_listener_unmatched_authentication_scheme = "Received a request with an unmatched or no authentication scheme. AuthenticationSchemes:{0}, Authorization:{1}.";
	public const string net_io_invalidasyncresult = "The IAsyncResult object was not returned from the corresponding asynchronous method on this class.";
	public const string net_io_invalidendcall = "{0} can only be called once for each asynchronous operation.";
	public const string net_listener_cannot_set_custom_cbt = "Custom channel bindings are not supported.";
	public const string net_listener_detach_error = "Can't detach Url group from request queue. Status code: {0}.";
	public const string net_listener_scheme = "Only Uri prefixes starting with 'http://' or 'https://' are supported.";
	public const string net_listener_host = "Only Uri prefixes with a valid hostname are supported.";
	public const string net_listener_not_supported = "The request is not supported.";
	public const string net_listener_mustcall = "Please call the {0} method before calling this method.";
	public const string net_listener_slash = "Only Uri prefixes ending in '/' are allowed.";
	public const string net_listener_already = "Failed to listen on prefix '{0}' because it conflicts with an existing registration on the machine.";
	public const string net_log_listener_no_cbt_disabled = "No channel binding check because extended protection is disabled.";
	public const string net_log_listener_no_cbt_http = "No channel binding check for requests without a secure channel.";
	public const string net_log_listener_no_cbt_trustedproxy = "No channel binding check for the trusted proxy scenario.";
	public const string net_log_listener_cbt = "Channel binding check enabled.";
	public const string net_log_listener_no_spn_kerberos = "No explicit service name check because Kerberos authentication already validates the service name.";
	public const string net_log_listener_no_spn_disabled = "No service name check because extended protection is disabled.";
	public const string net_log_listener_no_spn_cbt = "No service name check because the channel binding was already checked.";
	public const string net_log_listener_no_spn_whensupported = "No service name check because the client did not provide a service name and the server was configured for PolicyEnforcement.WhenSupported.";
	public const string net_log_listener_no_spn_loopback = "No service name check because the authentication was from a client on the local machine.";
	public const string net_log_listener_spn = "Client provided service name '{0}'.";
	public const string net_log_listener_spn_passed = "Service name check succeeded.";
	public const string net_log_listener_spn_failed = "Service name check failed.";
	public const string net_log_listener_spn_failed_always = "Service name check failed because the client did not provide a service name and the server was configured for PolicyEnforcement.Always.";
	public const string net_log_listener_spn_failed_empty = "No acceptable service names were configured!";
	public const string net_log_listener_spn_failed_dump = "Dumping acceptable service names:";
	public const string net_log_listener_spn_add = "Adding default service name '{0}' from prefix '{1}'.";
	public const string net_log_listener_spn_not_add = "No default service name added for prefix '{0}'.";
	public const string net_log_listener_spn_remove = "Removing default service name '{0}' from prefix '{1}'.";
	public const string net_log_listener_spn_not_remove = "No default service name removed for prefix '{0}'.";
	public const string net_listener_no_spns = "No service names could be determined from the registered prefixes. Either add prefixes from which default service names can be derived or specify an ExtendedProtectionPolicy object which contains an explicit list of service names.";
	public const string net_ssp_dont_support_cbt = "The Security Service Providers don't support extended protection. Please install the latest Security Service Providers update.";
	public const string net_PropertyNotImplementedException = "This property is not implemented by this class.";
	public const string net_array_too_small = "The target array is too small.";
	public const string net_listener_mustcompletecall = "The in-progress method {0} must be completed first.";
	public const string net_listener_invalid_cbt_type = "Querying the {0} Channel Binding is not supported.";
	public const string net_listener_callinprogress = "Cannot re-call {0} while a previous call is still in progress.";
	public const string net_log_listener_cant_create_uri = "Can't create Uri from string '{0}://{1}{2}{3}'.";
	public const string net_log_listener_cant_convert_raw_path = "Can't convert Uri path '{0}' using encoding '{1}'.";
	public const string net_log_listener_cant_convert_percent_value = "Can't convert percent encoded value '{0}'.";
	public const string net_log_listener_cant_convert_to_utf8 = "Can't convert string '{0}' into UTF-8 bytes: {1}";
	public const string net_log_listener_cant_convert_bytes = "Can't convert bytes '{0}' into UTF-16 characters: {1}";
	public const string net_invalidstatus = "The status code must be exactly three digits.";
	public const string net_WebHeaderInvalidControlChars = "Specified value has invalid Control characters.";
	public const string net_rspsubmitted = "This operation cannot be performed after the response has been submitted.";
	public const string net_nochunkuploadonhttp10 = "Chunked encoding upload is not supported on the HTTP/1.0 protocol.";
	public const string net_cookie_exists = "Cookie already exists.";
	public const string net_clsmall = "The Content-Length value must be greater than or equal to zero.";
	public const string net_wrongversion = "Only HTTP/1.0 and HTTP/1.1 version requests are currently supported.";
	public const string net_noseek = "This stream does not support seek operations.";
	public const string net_writeonlystream = "The stream does not support reading.";
	public const string net_entitytoobig = "Bytes to be written to the stream exceed the Content-Length bytes size specified.";
	public const string net_io_notenoughbyteswritten = "Cannot close stream until all bytes are written.";
	public const string net_listener_close_urlgroup_error = "Can't close Url group. Status code: {0}.";
	public const string net_WebSockets_NativeSendResponseHeaders = "An error occurred when sending the WebSocket HTTP upgrade response during the {0} operation. The HRESULT returned is '{1}'";
	public const string net_WebSockets_ClientAcceptingNoProtocols = "The WebSocket client did not request any protocols, but server attempted to accept '{0}' protocol(s). ";
	public const string net_WebSockets_AcceptUnsupportedProtocol = "The WebSocket client request requested '{0}' protocol(s), but server is only accepting '{1}' protocol(s).";
	public const string net_WebSockets_AcceptNotAWebSocket = "The {0} operation was called on an incoming request that did not specify a '{1}: {2}' header or the {2} header not contain '{3}'. {2} specified by the client was '{4}'.";
	public const string net_WebSockets_AcceptHeaderNotFound = "The {0} operation was called on an incoming WebSocket request without required '{1}' header. ";
	public const string net_WebSockets_AcceptUnsupportedWebSocketVersion = "The {0} operation was called on an incoming request with WebSocket version '{1}', expected '{2}'. ";
	public const string net_WebSockets_InvalidEmptySubProtocol = "Empty string is not a valid subprotocol value. Please use \\\"null\\\" to specify no value.";
	public const string net_WebSockets_InvalidCharInProtocolString = "The WebSocket protocol '{0}' is invalid because it contains the invalid character '{1}'.";
	public const string net_WebSockets_ReasonNotNull = "The close status description '{0}' is invalid. When using close status code '{1}' the description must be null.";
	public const string net_WebSockets_InvalidCloseStatusCode = "The close status code '{0}' is reserved for system use only and cannot be specified when calling this method.";
	public const string net_WebSockets_InvalidCloseStatusDescription = "The close status description '{0}' is too long. The UTF8-representation of the status description must not be longer than {1} bytes.";
	public const string net_WebSockets_ArgumentOutOfRange_TooSmall = "The argument must be a value greater than {0}.";
	public const string net_WebSockets_ArgumentOutOfRange_TooBig = "The value of the '{0}' parameter ({1}) must be less than or equal to {2}.";
	public const string net_WebSockets_UnsupportedPlatform = "The WebSocket protocol is not supported on this platform.";
	public const string net_readonlystream = "The stream does not support writing.";
	public const string net_WebSockets_InvalidState_ClosedOrAborted = "The '{0}' instance cannot be used for communication because it has been transitioned into the '{1}' state.";
	public const string net_WebSockets_ReceiveAsyncDisallowedAfterCloseAsync = "The WebSocket is in an invalid state for this operation. The '{0}' method has already been called before on this instance. Use '{1}' instead to keep being able to receive data but close the output channel.";
	public const string net_Websockets_AlreadyOneOutstandingOperation = "There is already one outstanding '{0}' call for this WebSocket instance. ReceiveAsync and SendAsync can be called simultaneously, but at most one outstanding operation for each of them is allowed at the same time.";
	public const string net_WebSockets_InvalidMessageType = "The received message type '{2}' is invalid after calling {0}. {0} should only be used if no more data is expected from the remote endpoint. Use '{1}' instead to keep being able to receive data but close the output channel.";
	public const string net_WebSockets_InvalidBufferType = "The buffer type '{0}' is invalid. Valid buffer types are: '{1}', '{2}', '{3}', '{4}', '{5}'.";
	public const string net_WebSockets_ArgumentOutOfRange_InternalBuffer = "The byte array must have a length of at least '{0}' bytes.  ";
	public const string net_WebSockets_Argument_InvalidMessageType = "The message type '{0}' is not allowed for the '{1}' operation. Valid message types are: '{2}, {3}'. To close the WebSocket, use the '{4}' operation instead. ";
	public const string net_securitypackagesupport = "The requested security package is not supported.";
	public const string net_log_operation_failed_with_error = "{0} failed with error {1}.";
	public const string net_MethodNotImplementedException = "This method is not implemented by this class.";
	public const string event_OperationReturnedSomething = "{0} returned {1}.";
	public const string net_invalid_enum = "The specified value is not valid in the '{0}' enumeration.";
	public const string net_auth_message_not_encrypted = "Protocol error: A received message contains a valid signature but it was not encrypted as required by the effective Protection Level.";
	public const string SSPIInvalidHandleType = "'{0}' is not a supported handle type.";
	public const string net_io_operation_aborted = "I/O operation aborted: '{0}'.";
	public const string net_invalid_path = "Invalid path.";
	public const string net_listener_auth_errors = "Authentication errors.";
	public const string net_listener_close = "Listener closed.";
	public const string net_invalid_port = "Invalid port in prefix.";
	public const string net_WebSockets_InvalidState = "The WebSocket is in an invalid state ('{0}') for this operation. Valid states are: '{1}'";
	public const string net_unknown_prefix = "The URI prefix is not recognized.";
	public const string net_reqsubmitted = "This operation cannot be performed after the request has been submitted.";
	public const string net_io_timeout_use_ge_zero = "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value >= 0.";
	public const string net_writestarted = "This property cannot be set after writing has started.";
	public const string net_badmethod = "Cannot set null or blank methods on request.";
	public const string net_servererror = "The remote server returned an error: ({0}) {1}.";
	public const string net_reqaborted = "The request was aborted: The request was canceled.";
	public const string net_OperationNotSupportedException = "This operation is not supported.";
	public const string net_nouploadonget = "Cannot send a content-body with this verb-type.";
	public const string net_repcall = "Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress.";
	public const string net_securityprotocolnotsupported = "The requested security protocol is not supported.";
	public const string net_requestaborted = "The request was aborted: {0}.";
	public const string net_webstatus_Timeout = "The operation has timed out.";
	public const string net_baddate = "The value of the date string in the header is invalid.";
	public const string net_connarg = "Keep-Alive and Close may not be set using this property.";
	public const string net_fromto = "The From parameter cannot be less than To.";
	public const string net_needchunked = "TransferEncoding requires the SendChunked property to be set to true.";
	public const string net_no100 = "100-Continue may not be set using this property.";
	public const string net_nochunked = "Chunked encoding must be set via the SendChunked property.";
	public const string net_nottoken = "The supplied string is not a valid HTTP token.";
	public const string net_rangetoosmall = "The From or To parameter cannot be less than 0.";
	public const string net_rangetype = "A different range specifier has already been added to this request.";
	public const string net_toosmall = "The specified value must be greater than 0.";
	public const string net_WebHeaderInvalidCRLFChars = "Specified value has invalid CRLF characters.";
	public const string net_WebHeaderInvalidHeaderChars = "Specified value has invalid HTTP Header characters.";
	public const string net_timeout = "The operation has timed out.";
	public const string net_completed_result = "This operation cannot be performed on a completed asynchronous result object.";
	public const string net_PropertyNotSupportedException = "This property is not supported by this class.";
	public const string net_InvalidStatusCode = "The server returned a status code outside the valid range of 100-599.";
	public const string net_io_timeout_use_gt_zero = "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0.";
	public const string net_ftp_servererror = "The remote server returned an error: {0}.";
	public const string net_ftp_active_address_different = "The data connection was made from an address that is different than the address to which the FTP connection was made.";
	public const string net_ftp_invalid_method_name = "FTP Method names cannot be null or empty.";
	public const string net_ftp_invalid_renameto = "The RenameTo filename cannot be null or empty.";
	public const string net_ftp_invalid_response_filename = "The server returned the filename ({0}) which is not valid.";
	public const string net_ftp_invalid_status_response = "The status response ({0}) is not expected in response to '{1}' command.";
	public const string net_ftp_invalid_uri = "The requested URI is invalid for this FTP command.";
	public const string net_ftp_no_defaultcreds = "Default credentials are not supported on an FTP request.";
	public const string net_ftp_response_invalid_format = "The response string '{0}' has invalid format.";
	public const string net_ftp_server_failed_passive = "The server failed the passive mode request with status response ({0}).";
	public const string net_ftp_unsupported_method = "This method is not supported.";
	public const string net_ftp_protocolerror = "The underlying connection was closed: The server committed a protocol violation";
	public const string net_ftp_receivefailure = "The underlying connection was closed: An unexpected error occurred on a receive";
	public const string net_webstatus_NameResolutionFailure = "The remote name could not be resolved";
	public const string net_webstatus_ConnectFailure = "Unable to connect to the remote server";
	public const string net_ftpstatuscode_ServiceNotAvailable = "Service not available, closing control connection.";
	public const string net_ftpstatuscode_CantOpenData = "Can't open data connection";
	public const string net_ftpstatuscode_ConnectionClosed = "Connection closed; transfer aborted";
	public const string net_ftpstatuscode_ActionNotTakenFileUnavailableOrBusy = "File unavailable (e.g., file busy)";
	public const string net_ftpstatuscode_ActionAbortedLocalProcessingError = "Local error in processing";
	public const string net_ftpstatuscode_ActionNotTakenInsufficientSpace = "Insufficient storage space in system";
	public const string net_ftpstatuscode_CommandSyntaxError = "Syntax error, command unrecognized";
	public const string net_ftpstatuscode_ArgumentSyntaxError = "Syntax error in parameters or arguments";
	public const string net_ftpstatuscode_CommandNotImplemented = "Command not implemented";
	public const string net_ftpstatuscode_BadCommandSequence = "Bad sequence of commands";
	public const string net_ftpstatuscode_NotLoggedIn = "Not logged in";
	public const string net_ftpstatuscode_AccountNeeded = "Need account for storing files";
	public const string net_ftpstatuscode_ActionNotTakenFileUnavailable = "File unavailable (e.g., file not found, no access)";
	public const string net_ftpstatuscode_ActionAbortedUnknownPageType = "Page type unknown";
	public const string net_ftpstatuscode_FileActionAborted = "Exceeded storage allocation (for current directory or data set)";
	public const string net_ftpstatuscode_ActionNotTakenFilenameNotAllowed = "File name not allowed";
	public const string net_invalid_host = "The specified value is not a valid Host header string.";
	public const string PlatformNotSupported_CompileToAssembly = "This platform does not support writing compiled regular expressions to an assembly.";
	public const string NotSupported_ReadOnlyCollection = "Collection is read-only.";
	public const string InvalidEmptyArgument = "Argument {0} cannot be zero-length.";
}
