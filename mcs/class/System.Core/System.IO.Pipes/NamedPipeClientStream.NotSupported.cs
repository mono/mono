using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Security.Principal;

namespace System.IO.Pipes
{
	/// <summary>
	/// Named pipe client. Use this to open the client end of a named pipes created with 
	/// NamedPipeServerStream.
	/// </summary>
	public sealed partial class NamedPipeClientStream : PipeStream
	{
		public NamedPipeClientStream (string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
			: base (PipeDirection.In, 0)
		{
			throw new PlatformNotSupportedException ();
		}

		private bool TryConnect (int timeout, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException ();
		}

		public int NumberOfServerInstances
		{
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		private void ValidateRemotePipeUser (SafePipeHandle handle)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
