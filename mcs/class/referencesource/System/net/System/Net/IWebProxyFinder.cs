using System;
using System.Collections.Generic;

namespace System.Net
{
    // The purpose of all derived classes of WebProxyFinder is to determine the PAC file location,
    // download and compile the file and then execute it to retrieve the list of proxies for a certain
    // Uri.
    internal interface IWebProxyFinder : IDisposable
    {
        bool GetProxies(Uri destination, out IList<string> proxyList);

        void Abort();

        void Reset();

        bool IsValid { get; }
    }
}
