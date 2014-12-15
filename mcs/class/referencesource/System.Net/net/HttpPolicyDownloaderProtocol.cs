using System;
using System.ComponentModel;

namespace System.Net.Sockets
{
    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class HttpPolicyDownloaderProtocol
    {
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public HttpPolicyDownloaderProtocol(Uri appUri, IPAddress address) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Abort() { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginDownload(SecurityCriticalAction callback) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DownloadCallback(IAsyncResult ar) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ReadCallback(IAsyncResult ar) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterUnsafeWebRequestCreator(IUnsafeWebRequestCreate creator) { }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SocketPolicy Result { get { throw new PlatformNotSupportedException(); } }
    }

    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void SecurityCriticalAction();

    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SocketPolicy
    {
    }
}

 

 


