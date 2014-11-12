
namespace System.Xml    {

    using System;
    using System.IO;
    using System.Security;
    using System.Collections;
    using System.Net;
    using System.Net.Cache;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;

    //
    // XmlDownloadManager
    //
    internal partial class XmlDownloadManager   {

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        internal Task<Stream> GetStreamAsync(Uri uri, ICredentials credentials, IWebProxy proxy,
            RequestCachePolicy cachePolicy) {
            if (uri.Scheme == "file")   {
                return Task.Run<Stream>(() => { return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1, true); });
            }
            else {
                return GetNonFileStreamAsync(uri, credentials, proxy, cachePolicy);
            }
        }

        private async Task<Stream> GetNonFileStreamAsync(Uri uri, ICredentials credentials, IWebProxy proxy,
            RequestCachePolicy cachePolicy) {
            WebRequest req = WebRequest.Create(uri);
            if (credentials != null)    {
                req.Credentials = credentials;
            }
            if (proxy != null)  {
                req.Proxy = proxy;
            }
            if (cachePolicy != null)    {
                req.CachePolicy = cachePolicy;
            }

            WebResponse resp = await Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null).ConfigureAwait(false);
            HttpWebRequest webReq = req as HttpWebRequest;
            if (webReq != null) {
                lock (this) {
                    if (connections == null)    {
                        connections = new Hashtable();
                    }
                    OpenedHost openedHost = (OpenedHost)connections[webReq.Address.Host];
                    if (openedHost == null) {
                        openedHost = new OpenedHost();
                    }

                    if (openedHost.nonCachedConnectionsCount < webReq.ServicePoint.ConnectionLimit - 1) {
                        // we are not close to connection limit -> don't cache the stream
                        if (openedHost.nonCachedConnectionsCount == 0)  {
                            connections.Add(webReq.Address.Host, openedHost);
                        }
                        openedHost.nonCachedConnectionsCount++;
                        return new XmlRegisteredNonCachedStream(resp.GetResponseStream(), this, webReq.Address.Host);
                    }
                    else {
                        // cache the stream and save the connection for the next request
                        return new XmlCachedStream(resp.ResponseUri, resp.GetResponseStream());
                    }
                }
            }
            else {
                return resp.GetResponseStream();
            }
        }
    }
}
