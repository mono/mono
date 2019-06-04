using System.Security.Principal;

namespace System.IO.Pipes
{
	public sealed partial class NamedPipeClientStream
	{
		public NamedPipeClientStream (string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
			: this (serverName, pipeName, (PipeDirection)(desiredAccessRights & (PipeAccessRights.ReadData | PipeAccessRights.WriteData)), options, impersonationLevel, inheritability)
		{
			if ((desiredAccessRights & ~(PipeAccessRights.FullControl | PipeAccessRights.AccessSystemSecurity)) != 0) {
				throw new ArgumentOutOfRangeException(nameof(desiredAccessRights), SR.ArgumentOutOfRange_InvalidPipeAccessRights);
			}
			if ((desiredAccessRights & ~(PipeAccessRights.ReadData | PipeAccessRights.WriteData)) != 0) {
				throw new PlatformNotSupportedException();
			}
		}
	}
}
