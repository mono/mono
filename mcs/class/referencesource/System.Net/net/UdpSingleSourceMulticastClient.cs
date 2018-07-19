using System;
using System.ComponentModel;
namespace System.Net.Sockets
{
    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UdpSingleSourceMulticastClient : IDisposable
    {
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]

        public UdpSingleSourceMulticastClient(IPAddress sourceAddress, IPAddress groupAddress, int localPort) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IAsyncResult BeginJoinGroup(AsyncCallback callback, object state) { throw new SocketException((int)(SocketError.AccessDenied)); }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IAsyncResult BeginReceiveFromSource(byte[] buffer, int offset, int count, AsyncCallback callback, object state) { throw new SocketException((int)(SocketError.AccessDenied)); }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IAsyncResult BeginSendToSource(byte[] buffer, int offset, int count, int remotePort, AsyncCallback callback, object state) { throw new SocketException((int)(SocketError.AccessDenied)); }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Dispose() { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndJoinGroup(IAsyncResult result) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int EndReceiveFromSource(IAsyncResult result, out int sourcePort) { throw new SocketException((int)(SocketError.AccessDenied)); }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndSendToSource(IAsyncResult result) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ReceiveBufferSize { get; set; }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int SendBufferSize { get; set; }
    }
}
