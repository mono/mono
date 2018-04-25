//------------------------------------------------------------------------------
// <copyright file="GlobalProxySelection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    [Obsolete("This class has been deprecated. Please use WebRequest.DefaultWebProxy instead to access and set the global default proxy. Use 'null' instead of GetEmptyWebProxy. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class GlobalProxySelection
    {
        // This just wraps WebRequest.DefaultWebProxy and modifies it to be compatible with Everett.
        // It needs to return a WebProxy whenever possible, and an EmptyWebProxy instead of null.
        public static IWebProxy Select
        {
            get
            {
                IWebProxy proxy = WebRequest.DefaultWebProxy;
                if (proxy == null)
                {
                    return GetEmptyWebProxy();
                }
                WebRequest.WebProxyWrapper wrap = proxy as WebRequest.WebProxyWrapper;
                if (wrap != null)
                {
                    return wrap.WebProxy;
                }
                return proxy;
            }

            set
            {
                WebRequest.DefaultWebProxy = value;
            }
        }

        public static IWebProxy GetEmptyWebProxy() {
            return new EmptyWebProxy();
        }
    }
}
