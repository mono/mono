using System;
using System.ComponentModel;

namespace System.Net
{
    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface INetworkProgress
    {
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]

        event EventHandler<NetworkProgressChangedEventArgs> ProgressChanged;

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        event EventHandler<NetworkProgressChangedEventArgs> ProgressCompleted;

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        event EventHandler<NetworkProgressChangedEventArgs> ProgressFailed;
    }

    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkProgressChangedEventArgs : ProgressChangedEventArgs
    {
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NetworkProgressChangedEventArgs(int percentage, int processedBytes, int totalBytes, object userState) : base(default(int), null) { }

        // Properties
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ProcessedBytes { get { throw new PlatformNotSupportedException(); } }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TotalBytes { get { throw new PlatformNotSupportedException(); } }
    }
}
