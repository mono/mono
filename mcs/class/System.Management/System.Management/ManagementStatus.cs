//
// System.Management.ManagementStatus
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace System.Management
{
	public enum ManagementStatus
	{
		AccessDenied = -2147217405,
		AggregatingByObject = -2147217315,
		AlreadyExists = -2147217383,
		AmendedObject = -2147217306,
		BackupRestoreWinmgmtRunning = -2147217312,
		BufferTooSmall = -2147217348,
		CallCanceled = -2147217358,
		CannotBeAbstract = -2147217307,
		CannotBeKey = -2147217377,
		CannotBeSingleton = -2147217364,
		CannotChangeIndexInheritance = -2147217328,
		CannotChangeKeyInheritance = -2147217335,
		CircularReference = -2147217337,
		ClassHasChildren = -2147217371,
		ClassHasInstances = -2147217370,
		ClientTooSlow = -2147217305,
		CriticalError = -2147217398,
		Different = 262147,
		DuplicateObjects = 262152,
		Failed = -2147217407,
		False = 1,
		IllegalNull = -2147217368,
		IllegalOperation = -2147217378,
		IncompleteClass = -2147217376,
		InitializationFailure = -2147217388,
		InvalidCimType = -2147217363,
		InvalidClass = -2147217392,
		InvalidContext = -2147217401,
		InvalidDuplicateParameter = -2147217341,
		InvalidFlavor = -2147217338,
		InvalidMethod = -2147217362,
		InvalidMethodParameters = -2147217361,
		InvalidNamespace = -2147217394,
		InvalidObject = -2147217393,
		InvalidObjectPath = -2147217350,
		InvalidOperation = -2147217386,
		InvalidOperator = -2147217309,
		InvalidParameter = -2147217400,
		InvalidParameterID = -2147217353,
		InvalidProperty = -2147217359,
		InvalidPropertyType = -2147217366,
		InvalidProviderRegistration = -2147217390,
		InvalidQualifier = -2147217342,
		InvalidQualifierType = -2147217367,
		InvalidQuery = -2147217385,
		InvalidQueryType = -2147217384,
		InvalidStream = -2147217397,
		InvalidSuperclass = -2147217395,
		InvalidSyntax = -2147217375,
		LocalCredentials = -2147217308,
		MarshalInvalidSignature = -2147217343,
		MarshalVersionMismatch = -2147217344,
		MethodDisabled = -2147217322,
		MethodNotImplemented = -2147217323,
		MissingAggregationList = -2147217317,
		MissingGroupWithin = -2147217318,
		MissingParameterID = -2147217354,
		NoError = 0,
		NoMoreData = 262149,
		NonconsecutiveParameterIDs = -2147217352,
		NondecoratedObject = -2147217374,
		NotAvailable = -2147217399,
		NotEventClass = -2147217319,
		NotFound = -2147217406,
		NotSupported = -2147217396,
		OperationCanceled = 262150,
		OutOfDiskSpace = -2147217349,
		OutOfMemory = -2147217402,
		OverrideNotAllowed = -2147217382,
		ParameterIDOnRetval = -2147217351,
		PartialResults = 262160,
		Pending = 262151,
		PrivilegeNotHeld = -2147217310,
		PropagatedMethod = -2147217356,
		PropagatedProperty = -2147217380,
		PropagatedQualifier = -2147217381,
		PropertyNotAnObject = -2147217316,
		ProviderFailure = -2147217404,
		ProviderLoadFailure = -2147217389,
		ProviderNotCapable = -2147217372,
		ProviderNotFound = -2147217391,
		QueryNotImplemented = -2147217369,
		QueueOverflow = -2147217311,
		ReadOnly = -2147217373,
		RefresherBusy = -2147217321,
		RegistrationTooBroad = -2147213311,
		RegistrationTooPrecise = -2147213310,
		ResetToDefault = 262146,
		ServerTooBusy = -2147217339,
		ShuttingDown = -2147217357,
		SystemProperty = -2147217360,
		Timedout = 262148,
		TooManyProperties = -2147217327,
		TooMuchData = -2147217340,
		TransportFailure = -2147217387,
		TypeMismatch = -2147217403,
		Unexpected = -2147217379,
		UninterpretableProviderQuery = -2147217313,
		UnknownObjectType = -2147217346,
		UnknownPacketType = -2147217345,
		UnparsableQuery = -2147217320,
		UnsupportedClassUpdate = -2147217336,
		UnsupportedParameter = -2147217355,
		UnsupportedPutExtension = -2147217347,
		UpdateOverrideNotAllowed = -2147217325,
		UpdatePropagatedMethod = -2147217324,
		UpdateTypeMismatch = -2147217326,
		ValueOutOfRange = -2147217365,
	}
}

