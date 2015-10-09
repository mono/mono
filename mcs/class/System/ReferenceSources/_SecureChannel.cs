//
// Mono-specific additions to Microsoft's _SecureChannel.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
namespace System.Net.Security
{
	partial class SecureChannel
	{
		internal MonoTlsConnectionInfo GetMonoConnectionInfo ()
		{
			return SSPIWrapper.GetMonoConnectionInfo (m_SecModule, m_SecurityContext);
		}

		internal ProtocolToken CreateShutdownMessage ()
		{
			var buffer = SSPIWrapper.CreateShutdownMessage (m_SecModule, m_SecurityContext);
			return new ProtocolToken (buffer, SecurityStatus.ContinueNeeded);
		}

		internal ProtocolToken CreateHelloRequestMessage ()
		{
			var buffer = SSPIWrapper.CreateHelloRequestMessage (m_SecModule, m_SecurityContext);
			return new ProtocolToken (buffer, SecurityStatus.ContinueNeeded);
		}

		internal bool IsClosed {
			get {
				if (m_SecModule == null || m_SecurityContext == null || m_SecurityContext.IsClosed)
					return true;
				return SSPIWrapper.IsClosed (m_SecModule, m_SecurityContext);
			}
		}
	}
}
#endif
