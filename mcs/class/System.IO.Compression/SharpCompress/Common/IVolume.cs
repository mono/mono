using System;
#if !PORTABLE && !NETFX_CORE
using System.IO;
#endif

namespace SharpCompress.Common
{
    internal interface IVolume : IDisposable
    {
    }
}