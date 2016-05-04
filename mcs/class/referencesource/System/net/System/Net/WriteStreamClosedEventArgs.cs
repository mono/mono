using System;
using System.ComponentModel;

namespace System.Net
{
    //cannot mark obsolete, because we are type-forwarding this from System.Net
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WriteStreamClosedEventArgs : EventArgs
    {
        //introducing a default constructor that's obsolete, to avoid the 
        //issue above, caused by type forwarding
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public WriteStreamClosedEventArgs() { }
        // Properties
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Exception Error { get { return null; } }
    }

    //cannot mark obsolete, because we are type-forwarding this from System.Net
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void WriteStreamClosedEventHandler(object sender, WriteStreamClosedEventArgs e);
}
