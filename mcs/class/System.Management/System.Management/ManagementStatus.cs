//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
		Failed = -2147217407,
		NotFound = -2147217406,
		AccessDenied = -2147217405,
		ProviderFailure = -2147217404,
		TypeMismatch = -2147217403,
		OutOfMemory = -2147217402,
		InvalidContext = -2147217401,
		InvalidParameter = -2147217400,
		NotAvailable = -2147217399,
		CriticalError = -2147217398,
		InvalidStream = -2147217397,
		NotSupported = -2147217396,
		InvalidSuperclass = -2147217395,
		InvalidNamespace = -2147217394,
		InvalidObject = -2147217393,
		InvalidClass = -2147217392,
		ProviderNotFound = -2147217391,
		InvalidProviderRegistration = -2147217390,
		ProviderLoadFailure = -2147217389,
		InitializationFailure = -2147217388,
		TransportFailure = -2147217387,
		InvalidOperation = -2147217386,
		InvalidQuery = -2147217385,
		InvalidQueryType = -2147217384,
		AlreadyExists = -2147217383,
		OverrideNotAllowed = -2147217382,
		PropagatedQualifier = -2147217381,
		PropagatedProperty = -2147217380,
		Unexpected = -2147217379,
		IllegalOperation = -2147217378,
		CannotBeKey = -2147217377,
		IncompleteClass = -2147217376,
		InvalidSyntax = -2147217375,
		NondecoratedObject = -2147217374,
		ReadOnly = -2147217373,
		ProviderNotCapable = -2147217372,
		ClassHasChildren = -2147217371,
		ClassHasInstances = -2147217370,
		QueryNotImplemented = -2147217369,
		IllegalNull = -2147217368,
		InvalidQualifierType = -2147217367,
		InvalidPropertyType = -2147217366,
		ValueOutOfRange = -2147217365,
		CannotBeSingleton = -2147217364,
		InvalidCimType = -2147217363,
		InvalidMethod = -2147217362,
		InvalidMethodParameters = -2147217361,
		SystemProperty = -2147217360,
		InvalidProperty = -2147217359,
		CallCanceled = -2147217358,
		ShuttingDown = -2147217357,
		PropagatedMethod = -2147217356,
		UnsupportedParameter = -2147217355,
		MissingParameterID = -2147217354,
		InvalidParameterID = -2147217353,
		NonconsecutiveParameterIDs = -2147217352,
		ParameterIDOnRetval = -2147217351,
		InvalidObjectPath = -2147217350,
		OutOfDiskSpace = -2147217349,
		BufferTooSmall = -2147217348,
		UnsupportedPutExtension = -2147217347,
		UnknownObjectType = -2147217346,
		UnknownPacketType = -2147217345,
		MarshalVersionMismatch = -2147217344,
		MarshalInvalidSignature = -2147217343,
		InvalidQualifier = -2147217342,
		InvalidDuplicateParameter = -2147217341,
		TooMuchData = -2147217340,
		ServerTooBusy = -2147217339,
		InvalidFlavor = -2147217338,
		CircularReference = -2147217337,
		UnsupportedClassUpdate = -2147217336,
		CannotChangeKeyInheritance = -2147217335,
		CannotChangeIndexInheritance = -2147217328,
		TooManyProperties = -2147217327,
		UpdateTypeMismatch = -2147217326,
		UpdateOverrideNotAllowed = -2147217325,
		UpdatePropagatedMethod = -2147217324,
		MethodNotImplemented = -2147217323,
		MethodDisabled = -2147217322,
		RefresherBusy = -2147217321,
		UnparsableQuery = -2147217320,
		NotEventClass = -2147217319,
		MissingGroupWithin = -2147217318,
		MissingAggregationList = -2147217317,
		PropertyNotAnObject = -2147217316,
		AggregatingByObject = -2147217315,
		UninterpretableProviderQuery = -2147217313,
		BackupRestoreWinmgmtRunning = -2147217312,
		QueueOverflow = -2147217311,
		PrivilegeNotHeld = -2147217310,
		InvalidOperator = -2147217309,
		LocalCredentials = -2147217308,
		CannotBeAbstract = -2147217307,
		AmendedObject = -2147217306,
		ClientTooSlow = -2147217305,
		RegistrationTooBroad = -2147213311,
		RegistrationTooPrecise = -2147213310,
		NoError = 0,
		False = 1,
		ResetToDefault = 262146,
		Different = 262147,
		Timedout = 262148,
		NoMoreData = 262149,
		OperationCanceled = 262150,
		Pending = 262151,
		DuplicateObjects = 262152,
		PartialResults = 262160
	}
}