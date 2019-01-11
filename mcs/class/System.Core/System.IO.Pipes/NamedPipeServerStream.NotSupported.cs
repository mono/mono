using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Permissions;

namespace System.IO.Pipes
{
	public sealed partial class NamedPipeServerStream : PipeStream
	{
		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity)
			: base (PipeDirection.In, 0)
		{
			throw new PlatformNotSupportedException ();
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability)
			: base (PipeDirection.In, 0)
		{
			throw new PlatformNotSupportedException ();
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
			: base (PipeDirection.In, 0)
		{
			throw new PlatformNotSupportedException ();
		}

		[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void RunAsClient (PipeStreamImpersonationWorker impersonationWorker)
		{
			throw new PlatformNotSupportedException ();
		}

		private void Create (string pipeName, PipeDirection direction, int maxNumberOfServerInstances,
			PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
			HandleInheritability inheritability)
		{
			throw new PlatformNotSupportedException ();
		}

		public void WaitForConnection ()
		{
			throw new PlatformNotSupportedException ();
		}

		public Task WaitForConnectionAsync (CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException ();
		}

		private void HandleAcceptedSocket (Socket acceptedSocket)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Disconnect ()
		{
			throw new PlatformNotSupportedException ();
		}

		public string GetImpersonationUserName ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
