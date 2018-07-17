using System.Security.Principal;

namespace System.IO.Pipes
{
	public sealed partial class NamedPipeClientStream
	{
		public NamedPipeClientStream(string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
			: this(serverName, pipeName, AccessRightsToDirection(desiredAccessRights), options, impersonationLevel, inheritability)
		{
            if ((desiredAccessRights & ~(PipeAccessRights.FullControl | PipeAccessRights.AccessSystemSecurity)) != 0) {
                throw new ArgumentOutOfRangeException(nameof(desiredAccessRights), SR.ArgumentOutOfRange_InvalidPipeAccessRights);
            }
			// TODO: desiredAccessRights are not implemented
		}

		private static PipeDirection AccessRightsToDirection(PipeAccessRights accessRights)
		{
			return
				((accessRights & PipeAccessRights.ReadData) != 0 ? PipeDirection.In : 0) |
				((accessRights & PipeAccessRights.WriteData) != 0 ? PipeDirection.Out : 0);
		}
	}
}
