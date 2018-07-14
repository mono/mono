using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Pipes
{
    public sealed partial class NamedPipeServerStream : PipeStream
    {
        private void Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
                HandleInheritability inheritability)
        {
			throw new PlatformNotSupportedException();
        }

        public void WaitForConnection()
        {
			throw new PlatformNotSupportedException();
        }

        public Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
			throw new PlatformNotSupportedException();
        }

        private void HandleAcceptedSocket(Socket acceptedSocket)
        {
			throw new PlatformNotSupportedException();
        }

        public void Disconnect()
        {
			throw new PlatformNotSupportedException();
        }

        // Gets the username of the connected client.  Not that we will not have access to the client's 
        // username until it has written at least once to the pipe (and has set its impersonationLevel 
        // argument appropriately). 
        public string GetImpersonationUserName()
        {
			throw new PlatformNotSupportedException();
        }
    }
}
