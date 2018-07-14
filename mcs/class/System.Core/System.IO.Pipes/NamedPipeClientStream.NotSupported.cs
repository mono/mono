using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace System.IO.Pipes
{
    /// <summary>
    /// Named pipe client. Use this to open the client end of a named pipes created with 
    /// NamedPipeServerStream.
    /// </summary>
    public sealed partial class NamedPipeClientStream : PipeStream
    {
        private bool TryConnect(int timeout, CancellationToken cancellationToken)
        {
			throw new PlatformNotSupportedException();
        }

        public int NumberOfServerInstances
        {
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Security model of pipes: demand at creation but no subsequent demands")]
            get
            {
                throw new PlatformNotSupportedException();
            }
        }

        private void ValidateRemotePipeUser(SafePipeHandle handle)
        {
			throw new PlatformNotSupportedException();
        }
    }
}
