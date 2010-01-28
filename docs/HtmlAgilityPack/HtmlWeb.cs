// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Microsoft.Win32;

namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to get HTML document from HTTP.
    /// </summary>
    public class HtmlWeb
    {
        #region Delegates

        /// <summary>
        /// Represents the method that will handle the PostResponse event.
        /// </summary>
        public delegate void PostResponseHandler(HttpWebRequest request, HttpWebResponse response);

        /// <summary>
        /// Represents the method that will handle the PreHandleDocument event.
        /// </summary>
        public delegate void PreHandleDocumentHandler(HtmlDocument document);

        /// <summary>
        /// Represents the method that will handle the PreRequest event.
        /// </summary>
        public delegate bool PreRequestHandler(HttpWebRequest request);

        #endregion

        #region Fields

        private bool _autoDetectEncoding = true;
        private bool _cacheOnly;

        private string _cachePath;
        private bool _fromCache;
        private int _requestDuration;
        private Uri _responseUri;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private int _streamBufferSize = 1024;
        private bool _useCookies;
        private bool _usingCache;

        /// <summary>
        /// Occurs after an HTTP request has been executed.
        /// </summary>
        public PostResponseHandler PostResponse;

        /// <summary>
        /// Occurs before an HTML document is handled.
        /// </summary>
        public PreHandleDocumentHandler PreHandleDocument;

        /// <summary>
        /// Occurs before an HTTP request is executed.
        /// </summary>
        public PreRequestHandler PreRequest;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets a value indicating if document encoding must be automatically detected.
        /// </summary>
        public bool AutoDetectEncoding
        {
            get { return _autoDetectEncoding; }
            set { _autoDetectEncoding = value; }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether to get document only from the cache.
        /// If this is set to true and document is not found in the cache, nothing will be loaded.
        /// </summary>
        public bool CacheOnly
        {
            get { return _cacheOnly; }
            set
            {
                if ((value) && !UsingCache)
                {
                    throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
                }
                _cacheOnly = value;
            }
        }

        /// <summary>
        /// Gets or Sets the cache path. If null, no caching mechanism will be used.
        /// </summary>
        public string CachePath
        {
            get { return _cachePath; }
            set { _cachePath = value; }
        }

        /// <summary>
        /// Gets a value indicating if the last document was retrieved from the cache.
        /// </summary>
        public bool FromCache
        {
            get { return _fromCache; }
        }

        /// <summary>
        /// Gets the last request duration in milliseconds.
        /// </summary>
        public int RequestDuration
        {
            get { return _requestDuration; }
        }

        /// <summary>
        /// Gets the URI of the Internet resource that actually responded to the request.
        /// </summary>
        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        /// <summary>
        /// Gets the last request status.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary>
        /// Gets or Sets the size of the buffer used for memory operations.
        /// </summary>
        public int StreamBufferSize
        {
            get { return _streamBufferSize; }
            set
            {
                if (_streamBufferSize <= 0)
                {
                    throw new ArgumentException("Size must be greater than zero.");
                }
                _streamBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating if cookies will be stored.
        /// </summary>
        public bool UseCookies
        {
            get { return _useCookies; }
            set { _useCookies = value; }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether the caching mechanisms should be used or not.
        /// </summary>
        public bool UsingCache
        {
            get
            {
                if (_cachePath == null)
                {
                    return false;
                }
                return _usingCache;
            }
            set
            {
                if ((value) && (_cachePath == null))
                {
                    throw new HtmlWebException("You need to define a CachePath first.");
                }
                _usingCache = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the MIME content type for a given path extension.
        /// </summary>
        /// <param name="extension">The input path extension.</param>
        /// <param name="def">The default content type to return if any error occurs.</param>
        /// <returns>The path extension's MIME content type.</returns>
        public static string GetContentTypeForExtension(string extension, string def)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return def;
            }
            string contentType = "";
            try
            {
                RegistryKey reg = Registry.ClassesRoot;
                reg = reg.OpenSubKey(extension, false);
                if (reg != null) contentType = (string)reg.GetValue("", def);
            }
            catch (Exception)
            {
                contentType = def;
            }
            return contentType;
        }

        /// <summary>
        /// Gets the path extension for a given MIME content type.
        /// </summary>
        /// <param name="contentType">The input MIME content type.</param>
        /// <param name="def">The default path extension to return if any error occurs.</param>
        /// <returns>The MIME content type's path extension.</returns>
        public static string GetExtensionForContentType(string contentType, string def)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return def;
            }
            string ext = "";
            try
            {
                RegistryKey reg = Registry.ClassesRoot;
                reg = reg.OpenSubKey(@"MIME\Database\Content Type\" + contentType, false);
                if (reg != null) ext = (string)reg.GetValue("Extension", def);
            }
            catch (Exception)
            {
                ext = def;
            }
            return ext;
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string url, Type type)
        {
            return CreateInstance(url, null, null, type);
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An <see cref="XsltArgumentList"/> containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type)
        {
            return CreateInstance(htmlUrl, xsltUrl, xsltArgs, type, null);
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An <see cref="XsltArgumentList"/> containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="type">The requested type.</param>
        /// <param name="xmlPath">A file path where the temporary XML before transformation will be saved. Mostly used for debugging purposes.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type,
                                     string xmlPath)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            if (xsltUrl == null)
            {
                LoadHtmlAsXml(htmlUrl, writer);
            }
            else
            {
                if (xmlPath == null)
                {
                    LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer);
                }
                else
                {
                    LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, xmlPath);
                }
            }
            writer.Flush();
            StringReader sr = new StringReader(sw.ToString());
            XmlTextReader reader = new XmlTextReader(sr);
            XmlSerializer serializer = new XmlSerializer(type);
            object o;
            try
            {
                o = serializer.Deserialize(reader);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex + ", --- xml:" + sw);
            }
            return o;
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        public void Get(string url, string path)
        {
            Get(url, path, "GET");
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="proxy"></param>
        /// <param name="credentials"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials)
        {
            Get(url, path, proxy, credentials, "GET");
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        public void Get(string url, string path, string method)
        {
            Uri uri = new Uri(url);
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                Get(uri, method, path, null, null, null);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="credentials"></param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials, string method)
        {
            Uri uri = new Uri(url);
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                Get(uri, method, path, null, proxy, credentials);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }

        /// <summary>
        /// Gets the cache file path for a specified url.
        /// </summary>
        /// <param name="uri">The url fo which to retrieve the cache path. May not be null.</param>
        /// <returns>The cache file path.</returns>
        public string GetCachePath(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!UsingCache)
            {
                throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
            }
            string cachePath;
            if (uri.AbsolutePath == "/")
            {
                cachePath = Path.Combine(_cachePath, ".htm");
            }
            else
            {
                cachePath = Path.Combine(_cachePath, (uri.Host + uri.AbsolutePath).Replace('/', '\\'));
            }
            return cachePath;
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url)
        {
            return Load(url, "GET");
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="proxyHost">Host to use for Proxy</param>
        /// <param name="proxyPort">Port the Proxy is on</param>
        /// <param name="userId">User Id for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string proxyHost, int proxyPort, string userId, string password)
        {
            //Create my proxy
            WebProxy myProxy = new WebProxy(proxyHost, proxyPort);
            myProxy.BypassProxyOnLocal = true;

            //Create my credentials
            NetworkCredential myCreds = null;
            if ((userId != null) && (password != null))
            {
                myCreds = new NetworkCredential(userId, password);
                CredentialCache credCache = new CredentialCache();
                //Add the creds
                credCache.Add(myProxy.Address, "Basic", myCreds);
                credCache.Add(myProxy.Address, "Digest", myCreds);
            }

            return Load(url, "GET", myProxy, myCreds);
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method)
        {
            Uri uri = new Uri(url);
            HtmlDocument doc;
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                doc = LoadUrl(uri, method, null, null);
            }
            else
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    doc.DetectEncodingAndLoad(url, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method, WebProxy proxy, NetworkCredential credentials)
        {
            Uri uri = new Uri(url);
            HtmlDocument doc;
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                doc = LoadUrl(uri, method, proxy, credentials);
            }
            else
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    doc.DetectEncodingAndLoad(url, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        public void LoadHtmlAsXml(string htmlUrl, XmlTextWriter writer)
        {
            HtmlDocument doc = Load(htmlUrl);
            doc.Save(writer);
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer)
        {
            LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, null);
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp". May not be null.</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        /// <param name="xmlPath">A file path where the temporary XML before transformation will be saved. Mostly used for debugging purposes.</param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer,
                                  string xmlPath)
        {
            if (htmlUrl == null)
            {
                throw new ArgumentNullException("htmlUrl");
            }

            HtmlDocument doc = Load(htmlUrl);

            if (xmlPath != null)
            {
                XmlTextWriter w = new XmlTextWriter(xmlPath, doc.Encoding);
                doc.Save(w);
                w.Close();
            }
            if (xsltArgs == null)
            {
                xsltArgs = new XsltArgumentList();
            }

            // add some useful variables to the xslt doc
            xsltArgs.AddParam("url", "", htmlUrl);
            xsltArgs.AddParam("requestDuration", "", RequestDuration);
            xsltArgs.AddParam("fromCache", "", FromCache);

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltUrl);
            xslt.Transform(doc, xsltArgs, writer);
        }

        #endregion

        #region Private Methods

        private static void FilePreparePath(string target)
        {
            if (File.Exists(target))
            {
                FileAttributes atts = File.GetAttributes(target);
                File.SetAttributes(target, atts & ~FileAttributes.ReadOnly);
            }
            else
            {
                string dir = Path.GetDirectoryName(target);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private static DateTime RemoveMilliseconds(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        // ReSharper disable UnusedMethodReturnValue.Local
        private static long SaveStream(Stream stream, string path, DateTime touchDate, int streamBufferSize)
        // ReSharper restore UnusedMethodReturnValue.Local
        {
            FilePreparePath(path);
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryReader br = null;
            BinaryWriter bw = null;
            long len = 0;
            try
            {
                br = new BinaryReader(stream);
                bw = new BinaryWriter(fs);

                byte[] buffer;
                do
                {
                    buffer = br.ReadBytes(streamBufferSize);
                    len += buffer.Length;
                    if (buffer.Length > 0)
                    {
                        bw.Write(buffer);
                    }
                } while (buffer.Length > 0);
            }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }
                if (bw != null)
                {
                    bw.Flush();
                    bw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            File.SetLastWriteTime(path, touchDate);
            return len;
        }

        private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
                                   ICredentials creds)
        {
            string cachePath = null;
            HttpWebRequest req;
            bool oldFile = false;

            req = WebRequest.Create(uri) as HttpWebRequest;
            req.Method = method;

            if (proxy != null)
            {
                if (creds != null)
                {
                    proxy.Credentials = creds;
                    req.Credentials = creds;
                }
                else
                {
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    req.Credentials = CredentialCache.DefaultCredentials;
                }
                req.Proxy = proxy;
            }

            _fromCache = false;
            _requestDuration = 0;
            int tc = Environment.TickCount;
            if (UsingCache)
            {
                cachePath = GetCachePath(req.RequestUri);
                if (File.Exists(cachePath))
                {
                    req.IfModifiedSince = File.GetLastAccessTime(cachePath);
                    oldFile = true;
                }
            }

            if (_cacheOnly)
            {
                if (!File.Exists(cachePath))
                {
                    throw new HtmlWebException("File was not found at cache path: '" + cachePath + "'");
                }

                if (path != null)
                {
                    IOLibrary.CopyAlways(cachePath, path);
                    // touch the file
                    File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                }
                _fromCache = true;
                return HttpStatusCode.NotModified;
            }

            if (_useCookies)
            {
                req.CookieContainer = new CookieContainer();
            }

            if (PreRequest != null)
            {
                // allow our user to change the request at will
                if (!PreRequest(req))
                {
                    return HttpStatusCode.ResetContent;
                }

                // dump cookie
                //				if (_useCookies)
                //				{
                //					foreach(Cookie cookie in req.CookieContainer.GetCookies(req.RequestUri))
                //					{
                //						HtmlLibrary.Trace("Cookie " + cookie.Name + "=" + cookie.Value + " path=" + cookie.Path + " domain=" + cookie.Domain);
                //					}
                //				}
            }

            HttpWebResponse resp;

            try
            {
                resp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                _requestDuration = Environment.TickCount - tc;
                resp = (HttpWebResponse)we.Response;
                if (resp == null)
                {
                    if (oldFile)
                    {
                        if (path != null)
                        {
                            IOLibrary.CopyAlways(cachePath, path);
                            // touch the file
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }
                        return HttpStatusCode.NotModified;
                    }
                    throw;
                }
            }
            catch (Exception)
            {
                _requestDuration = Environment.TickCount - tc;
                throw;
            }

            // allow our user to get some info from the response
            if (PostResponse != null)
            {
                PostResponse(req, resp);
            }

            _requestDuration = Environment.TickCount - tc;
            _responseUri = resp.ResponseUri;

            bool html = IsHtmlContent(resp.ContentType);
            Encoding respenc;

            if ((resp.ContentEncoding != null) && (resp.ContentEncoding.Length > 0))
            {
                respenc = Encoding.GetEncoding(resp.ContentEncoding);
            }
            else
            {
                respenc = null;
            }

            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                if (UsingCache)
                {
                    _fromCache = true;
                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        // touch the file
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                    return resp.StatusCode;
                }
                else
                {
                    // this should *never* happen...
                    throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
                }
            }
            Stream s = resp.GetResponseStream();
            if (s != null)
            {
                if (UsingCache)
                {
                    // NOTE: LastModified does not contain milliseconds, so we remove them to the file
                    SaveStream(s, cachePath, RemoveMilliseconds(resp.LastModified), _streamBufferSize);

                    // save headers
                    SaveCacheHeaders(req.RequestUri, resp);

                    if (path != null)
                    {
                        // copy and touch the file
                        IOLibrary.CopyAlways(cachePath, path);
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                }
                else
                {
                    // try to work in-memory
                    if ((doc != null) && (html))
                    {
                        if (respenc != null)
                        {
                            doc.Load(s, respenc);
                        }
                        else
                        {
                            doc.Load(s, true);
                        }
                    }
                }
                resp.Close();
            }
            return resp.StatusCode;
        }

        private string GetCacheHeader(Uri requestUri, string name, string def)
        {
            // note: some headers are collection (ex: www-authenticate)
            // we don't handle that here
            XmlDocument doc = new XmlDocument();
            doc.Load(GetCacheHeadersPath(requestUri));
            XmlNode node =
                doc.SelectSingleNode("//h[translate(@n, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='" +
                                     name.ToUpper() + "']");
            if (node == null)
            {
                return def;
            }
            // attribute should exist
            return node.Attributes[name].Value;
        }

        private string GetCacheHeadersPath(Uri uri)
        {
            //return Path.Combine(GetCachePath(uri), ".h.xml");
            return GetCachePath(uri) + ".h.xml";
        }

        private bool IsCacheHtmlContent(string path)
        {
            string ct = GetContentTypeForExtension(Path.GetExtension(path), null);
            return IsHtmlContent(ct);
        }

        private bool IsHtmlContent(string contentType)
        {
            return contentType.ToLower().StartsWith("text/html");
        }

        private HtmlDocument LoadUrl(Uri uri, string method, WebProxy proxy, NetworkCredential creds)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = false;
            doc.OptionFixNestedTags = true;
            _statusCode = Get(uri, method, null, doc, proxy, creds);
            if (_statusCode == HttpStatusCode.NotModified)
            {
                // read cached encoding
                doc.DetectEncodingAndLoad(GetCachePath(uri));
            }
            return doc;
        }

        private void SaveCacheHeaders(Uri requestUri, HttpWebResponse resp)
        {
            // we cache the original headers aside the cached document.
            string file = GetCacheHeadersPath(requestUri);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<c></c>");
            XmlNode cache = doc.FirstChild;
            foreach (string header in resp.Headers)
            {
                XmlNode entry = doc.CreateElement("h");
                XmlAttribute att = doc.CreateAttribute("n");
                att.Value = header;
                entry.Attributes.Append(att);

                att = doc.CreateAttribute("v");
                att.Value = resp.Headers[header];
                entry.Attributes.Append(att);

                cache.AppendChild(entry);
            }
            doc.Save(file);
        }

        #endregion
    }
}