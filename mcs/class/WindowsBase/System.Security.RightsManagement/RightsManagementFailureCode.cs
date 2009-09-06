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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;

namespace System.Security.RightsManagement {

	public enum RightsManagementFailureCode
	{
		ManifestPolicyViolation = -2147183860,

		InvalidLicense = -2147168512,
		InfoNotInLicense,
		InvalidLicenseSignature,

		EncryptionNotPermitted = -2147168508,
		RightNotGranted,
		InvalidVersion,
		InvalidEncodingType,
		InvalidNumericalValue,
		InvalidAlgorithmType,
		EnvironmentNotLoaded,
		EnvironmentCannotLoad,
		TooManyLoadedEnvironments,

		IncompatibleObjects = -2147168498,
		LibraryFail,
		EnablingPrincipalFailure,
		InfoNotPresent,
		BadGetInfoQuery,
		KeyTypeUnsupported,
		CryptoOperationUnsupported,
		ClockRollbackDetected,
		QueryReportsNoResults,
		UnexpectedException,
		BindValidityTimeViolated,
		BrokenCertChain,

		BindPolicyViolation = -2147168485,
		BindRevokedLicense,
		BindRevokedIssuer,
		BindRevokedPrincipal,
		BindRevokedResource,
		BindRevokedModule,
		BindContentNotInEndUseLicense,
		BindAccessPrincipalNotEnabling,
		BindAccessUnsatisfied,
		BindIndicatedPrincipalMissing,
		BindMachineNotFoundInGroupIdentity,
		LibraryUnsupportedPlugIn,
		BindRevocationListStale,
		BindNoApplicableRevocationList,

		InvalidHandle = -2147168468,

		BindIntervalTimeViolated = -2147168465,
		BindNoSatisfiedRightsGroup,
		BindSpecifiedWorkMissing,

		NoMoreData = -2147168461,
		LicenseAcquisitionFailed,
		IdMismatch,
		TooManyCertificates,
		NoDistributionPointUrlFound,
		AlreadyInProgress,
		GroupIdentityNotSet,
		RecordNotFound,
		NoConnect,
		NoLicense,
		NeedsMachineActivation,
		NeedsGroupIdentityActivation,

		ActivationFailed = -2147168448,
		Aborted,
		OutOfQuota,
		AuthenticationFailed,
		ServerError,
		InstallationFailed,
		HidCorrupted,
		InvalidServerResponse,
		ServiceNotFound,
		UseDefault,
		ServerNotFound,
		InvalidEmail,
		ValidityTimeViolation,
		OutdatedModule,
		NotSet,
		MetadataNotSet,
		RevocationInfoNotSet,
		InvalidTimeInfo,
		RightNotSet,
		LicenseBindingToWindowsIdentityFailed,
		InvalidIssuanceLicenseTemplate,
		InvalidKeyLength,

		ExpiredOfficialIssuanceLicenseTemplate = -2147168425,
		InvalidClientLicensorCertificate,
		HidInvalid,
		EmailNotVerified,
		ServiceMoved,
		ServiceGone,
		AdEntryNotFound,
		NotAChain,
		RequestDenied,
		DebuggerDetected,

		InvalidLockboxType = -2147168400,
		InvalidLockboxPath,
		InvalidRegistryPath,
		NoAesCryptoProvider,
		GlobalOptionAlreadySet,
		OwnerLicenseNotFound,

		Success = 0
	}

}