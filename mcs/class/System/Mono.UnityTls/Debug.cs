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
		public static void CheckAndThrow (UnityTls.unitytls_errorstate errorState, string context, AlertDescription defaultAlert = AlertDescription.InternalError)
		{
			if (errorState.code == UnityTls.unitytls_error_code.UNITYTLS_SUCCESS)
				return;

			string message = string.Format ("{0} - error code: {1}", context, errorState.code);
			throw new TlsException (defaultAlert, message);
		}

		public static void CheckAndThrow(UnityTls.unitytls_errorstate errorState, UnityTls.unitytls_x509verify_result verifyResult, string context, AlertDescription defaultAlert = AlertDescription.InternalError)
		{
			// Ignore verify result if verification is not the issue.
			if (verifyResult == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS) {
				CheckAndThrow (errorState, context, defaultAlert);
				return;
			}

			AlertDescription alert = UnityTlsConversions.VerifyResultToAlertDescription (verifyResult, defaultAlert);
			string message = string.Format ("{0} - error code: {1}, verify result: {2}", context, errorState.code, verifyResult);
			throw new TlsException (alert, message);
		}
	}
}
#endif