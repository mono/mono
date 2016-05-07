using System;
using System.ComponentModel;

namespace System.Net
{
    //introduced for supporting design-time loading of System.Windows.dll
    [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IUnsafeWebRequestCreate
    {
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        WebRequest Create(Uri uri);
    }
}
