#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using System.Security.Authentication;
using System.Net.Security;

namespace Mono.Unity
{
	internal static class UnityTlsConversions
	{
		public static UnityTls.unitytls_protocol GetMinProtocol (SslProtocols protocols)
		{
			if (protocols.HasFlag (SslProtocols.Tls))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;
			if (protocols.HasFlag (SslProtocols.Tls11))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_1;
			if (protocols.HasFlag (SslProtocols.Tls12))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;
			return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;	// Behavior as in AppleTlsContext
		}

		public static UnityTls.unitytls_protocol GetMaxProtocol (SslProtocols protocols)
		{
			if (protocols.HasFlag (SslProtocols.Tls12))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;
			if (protocols.HasFlag (SslProtocols.Tls11))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_1;
			if (protocols.HasFlag (SslProtocols.Tls))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;
			return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;	// Behavior as in AppleTlsContext
		}

		public static TlsProtocols ConvertProtocolVersion(UnityTls.unitytls_protocol protocol)
		{
			switch (protocol)
			{
			case UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0:
				return TlsProtocols.Tls10;
			case UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_1:
				return TlsProtocols.Tls11;
			case UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2:
				return TlsProtocols.Tls12;
			case UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_INVALID:
				return TlsProtocols.Zero;
			}
			return TlsProtocols.Zero;
		}

		public static AlertDescription VerifyResultToAlertDescription (UnityTls.unitytls_x509verify_result verifyResult, AlertDescription defaultAlert = AlertDescription.InternalError)
		{
			if (verifyResult == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FATAL_ERROR)
				return AlertDescription.CertificateUnknown;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_EXPIRED))
				return AlertDescription.CertificateExpired;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_REVOKED))
				return AlertDescription.CertificateRevoked;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_CN_MISMATCH))
				return AlertDescription.UnknownCA;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_NOT_TRUSTED))
				return AlertDescription.CertificateUnknown;

			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR1))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR2))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR2))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR3))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR4))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR5))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR6))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR7))
				return AlertDescription.UserCancelled;
			else if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_USER_ERROR8))
				return AlertDescription.UserCancelled;

			return defaultAlert;
		}

		public static SslPolicyErrors VerifyResultToPolicyErrror (UnityTls.unitytls_x509verify_result verifyResult)
		{
			// First, check "non-flags"
			if (verifyResult == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS)
				return SslPolicyErrors.None;
			else if (verifyResult == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FATAL_ERROR)
				return SslPolicyErrors.RemoteCertificateChainErrors;

			SslPolicyErrors error = SslPolicyErrors.None;
			if (verifyResult.HasFlag (UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_CN_MISMATCH))
				error |= SslPolicyErrors.RemoteCertificateNameMismatch;
			// Anything else translates to SslPolicyErrors.RemoteCertificateChainErrors. So if it is not the only flag, add it.
			if (verifyResult != UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_CN_MISMATCH)
				error |= SslPolicyErrors.RemoteCertificateChainErrors;
			return error;
		}
	}
}
#endif