#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.Unity
{
	internal static class Debug
	{
		public static void CheckAndThrow(UnityTls.unitytls_errorstate errorState, string context)
		{
			if (errorState.code != UnityTls.unitytls_error_code.UNITYTLS_SUCCESS) {
				string message = string.Format("{0} - error code: {1}", context, errorState.code);
				throw new TlsException(AlertDescription.InternalError, message);
			}
		}

		public static AlertDescription VerifyResultToAlertDescription(UnityTls.unitytls_x509verify_result verifyResult, AlertDescription defaultAlert = AlertDescription.InternalError)
		{
			if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_EXPIRED))
				return AlertDescription.CertificateExpired;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_REVOKED))
				return AlertDescription.CertificateRevoked;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_CN_MISMATCH))
				return AlertDescription.UnknownCA;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_NOT_TRUSTED))
				return AlertDescription.CertificateUnknown;

			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR1))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR2))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR2))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR3))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR4))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR5))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR6))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR7))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag(UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR8))
				return AlertDescription.UserCancelled;

			return defaultAlert;
		}

		public static void CheckAndThrow(UnityTls.unitytls_errorstate errorState, UnityTls.unitytls_x509verify_result verifyResult, string context, AlertDescription defaultAlert = AlertDescription.InternalError)
		{
			if (verifyResult != UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS) {
				AlertDescription alert = VerifyResultToAlertDescription(verifyResult, defaultAlert);
				string message = string.Format("{0} - error code: {1}, verify result: {2}", context, errorState.code, verifyResult);
				throw new TlsException(alert, message);
			}
		}
	}
}
#endif