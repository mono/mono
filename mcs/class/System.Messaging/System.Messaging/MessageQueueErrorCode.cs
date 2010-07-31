//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) 2003 Peter Van Isacker
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
using System;

namespace System.Messaging
{
	[Serializable]
	public enum MessageQueueErrorCode
	{
		AccessDenied = -1072824283,
		BadSecurityContext = -1072824267,
		Base = -1072824320,
		BufferOverflow = -1072824294,
		CannotCreateCertificateStore = -1072824209,
		CannotCreateHashEx = -1072824191,
		CannotCreateOnGlobalCatalog = -1072824201,
		CannotGetDistinguishedName = -1072824194,
		CannotGrantAddGuid = -1072824206,
		CannotHashDataEx = -1072824193,
		CannotImpersonateClient = -1072824284,
		CannotJoinDomain = -1072824202,
		CannotLoadMsmqOcm = -1072824205,
		CannotOpenCertificateStore = -1072824208,
		CannotSetCryptographicSecurityDescriptor = -1072824212,
		CannotSignDataEx = -1072824192,
		CertificateNotProvided = -1072824211,
		ComputerDoesNotSupportEncryption = -1072824269,
		CorruptedInternalCertificate = -1072824275,
		CorruptedPersonalCertStore = -1072824271,
		CorruptedQueueWasDeleted = -1072824216,
		CorruptedSecurityData = -1072824272,
		CouldNotGetAccountInfo = -1072824265,
		CouldNotGetUserSid = -1072824266,
		DeleteConnectedNetworkInUse = -1072824248,
		DependentClientLicenseOverflow = -1072824217,
		DsError = -1072824253,
		DsIsFull = -1072824254,
		DtcConnect = -1072824244,
		EncryptionProviderNotSupported = -1072824213,
		FailVerifySignatureEx = -1072824190,
		FormatNameBufferTooSmall = -1072824289,
		Generic = -1072824319,
		GuidNotMatching = -1072824200,
		IllegalContext = -1072824229,
		IllegalCriteriaColumns = -1072824264,
		IllegalCursorAction = -1072824292,
		IllegalEnterpriseOperation = -1072824207,
		IllegalFormatName = -1072824290,
		IllegalMessageProperties = -1072824255,
		IllegalOperation = -1072824220,
		IllegalPrivateProperties = -1072824197,
		IllegalPropertyId = -1072824263,
		IllegalPropertySize = -1072824261,
		IllegalPropertyValue = -1072824296,
		IllegalPropertyVt = -1072824295,
		IllegalQueuePathName = -1072824300,
		IllegalQueueProperties = -1072824259,
		IllegalRelation = -1072824262,
		IllegalRestrictionPropertyId = -1072824260,
		IllegalSecurityDescriptor = -1072824287,
		IllegalSort = -1072824304,
		IllegalSortPropertyId = -1072824228,
		IllegalUser = -1072824303,
		InsufficientProperties = -1072824257,
		InsufficientResources = -1072824281,
		InvalidCertificate = -1072824276,
		InvalidHandle = -1072824313,
		InvalidOwner = -1072824252,
		InvalidParameter = -1072824314,
		IOTimeout = -1072824293,
		LabelBufferTooSmall = -1072824226,
		MachineExists = -1072824256,
		MachineNotFound = -1072824307,
		MessageAlreadyReceived = -1072824291,
		MessageStorageFailed = -1072824278,
		MissingConnectorType = -1072824235,
		MqisReadOnlyMode = -1072824224,
		MqisServerEmpty = -1072824225,
		NoDs = -1072824301,
		NoEntryPointMsmqOcm = -1072824204,
		NoGlobalCatalogInDomain = -1072824196,
		NoInternalUserCertificate = -1072824273,
		NoMsmqServersOnDc = -1072824203,
		NoMsmqServersOnGlobalCatalog = -1072824195,
		NoResponseFromObjectServer = -1072824247,
		ObjectServerNotAvailable = -1072824246,
		OperationCanceled = -1072824312,
		PrivilegeNotHeld = -1072824282,
		Property = -1072824318,
		PropertyNotAllowed = -1072824258,
		ProviderNameBufferTooSmall = -1072824221,
		PublicKeyDoesNotExist = -1072824198,
		PublicKeyNotFound = -1072824199,
		QDnsPropertyNotSupported = -1072824210,
		QueueDeleted = -1072824230,
		QueueExists = -1072824315,
		QueueNotAvailable = -1072824245,
		QueueNotFound = -1072824317,
		RemoteMachineNotAvailable = -1072824215,
		ResultBufferTooSmall = -1072824250,
		SecurityDescriptorBufferTooSmall = -1072824285,
		SenderCertificateBufferTooSmall = -1072824277,
		SenderIdBufferTooSmall = -1072824286,
		ServiceNotAvailable = -1072824309,
		SharingViolation = -1072824311,
		SignatureBufferTooSmall = -1072824222,
		StaleHandle = -1072824234,
		SymmetricKeyBufferTooSmall = -1072824223,
		TransactionEnlist = -1072824232,
		TransactionImport = -1072824242,
		TransactionSequence = -1072824239,
		TransactionUsage = -1072824240,
		UnsupportedAccessMode = -1072824251,
		UnsupportedFormatNameOperation = -1072824288,
		UnsupportedOperation = -1072824214,
		UserBufferTooSmall = -1072824280,
		WksCantServeClient = -1072824218,
		WriteNotAllowed = -1072824219
	}
}
