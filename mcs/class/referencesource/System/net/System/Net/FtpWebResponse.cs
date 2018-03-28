// ------------------------------------------------------------------------------
// <copyright file="FtpWebResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
//

namespace System.Net {

    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Security.Cryptography.X509Certificates ;
    using System.Security.Permissions;

    /// <summary>
    /// <para>The FtpWebResponse class contains the result of the FTP request
    /// interface.</para>
    /// </summary>
    public class FtpWebResponse : WebResponse, IDisposable {

        internal Stream m_ResponseStream;
        private long m_ContentLength;
        private Uri m_ResponseUri;
        private FtpStatusCode m_StatusCode;
        private string m_StatusLine;
        private WebHeaderCollection m_FtpRequestHeaders;
        private HttpWebResponse m_HttpWebResponse;
        private DateTime m_LastModified;
        private string   m_BannerMessage;
        private string   m_WelcomeMessage;
        private string   m_ExitMessage;


        internal FtpWebResponse(Stream responseStream, long contentLength, Uri responseUri, FtpStatusCode statusCode, string statusLine, DateTime lastModified, string bannerMessage, string welcomeMessage, string exitMessage) {
            GlobalLog.Print("FtpWebResponse#" + ValidationHelper.HashString(this) + "::.ctor(" + contentLength.ToString() + ","+ statusLine+ ")");
            m_ResponseStream = responseStream;
            if (responseStream == null && contentLength < 0) {
                contentLength = 0;
            }
            m_ContentLength = contentLength;
            m_ResponseUri = responseUri;
            m_StatusCode = statusCode;
            m_StatusLine = statusLine;
            m_LastModified = lastModified;
            m_BannerMessage = bannerMessage;
            m_WelcomeMessage = welcomeMessage;
            m_ExitMessage = exitMessage;
        }

        internal FtpWebResponse(HttpWebResponse httpWebResponse) {
            m_HttpWebResponse = httpWebResponse;
            InternalSetFromCache = m_HttpWebResponse.IsFromCache;
            InternalSetIsCacheFresh = m_HttpWebResponse.IsCacheFresh;
        }

        internal void UpdateStatus(FtpStatusCode statusCode, string statusLine, string exitMessage) {
            m_StatusCode = statusCode;
            m_StatusLine = statusLine;
            m_ExitMessage = exitMessage;
        }


        /// <summary>
        /// <para>Returns a data stream for FTP</para>
        /// </summary>
        public override Stream GetResponseStream()
        {
            Stream responseStream = null;
            if (HttpProxyMode) {
                responseStream = m_HttpWebResponse.GetResponseStream();
            }
            else if (m_ResponseStream != null) {
                responseStream = m_ResponseStream;
            }
            else {
                responseStream = m_ResponseStream = new EmptyStream();
            }
            return responseStream;
        }
        //
        internal class EmptyStream: MemoryStream
        {
            internal EmptyStream():base(new byte[0], false)
            {
            }
        }
        //
        // Only used when combining cached and live responses
        //
        internal void SetResponseStream(Stream stream)
        {
            if (stream == null || stream == Stream.Null || stream is EmptyStream)
                return;
            m_ResponseStream = stream;
        }


        /// <summary>
        /// <para>Closes the underlying FTP response stream, but does not close control connection</para>
        /// </summary>
        public override void Close() {
            if(Logging.On)Logging.Enter(Logging.Web, this, "Close", "");
            if (HttpProxyMode) {
                m_HttpWebResponse.Close();
            } else {
                Stream stream = m_ResponseStream;
                if (stream != null) {
                    stream.Close();
                }
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "Close", "");
        }

        /// <summary>
        /// <para>Queries the length of the response</para>
        /// </summary>
        public override long ContentLength {
            get {
                if (HttpProxyMode) {
                    return m_HttpWebResponse.ContentLength;
                }
                return m_ContentLength;
            }
        }
        internal void SetContentLength(long value)
        {
            if (HttpProxyMode)
                return; //m_HttpWebResponse.ContentLength = value;
            m_ContentLength = value;
        }


        /// <devdoc>
        /// <para>
        /// A collection of headers, currently nothing is return except an empty collection
        /// </para>
        /// </devdoc>
        public override WebHeaderCollection Headers {
            get {
                if (HttpProxyMode) {
                    return m_HttpWebResponse.Headers;
                }

                if (m_FtpRequestHeaders == null) {
                    lock(this) {
                        if (m_FtpRequestHeaders == null) {
                            m_FtpRequestHeaders         = new WebHeaderCollection(WebHeaderCollectionType.FtpWebResponse);
                        }
                    }
                }
                return m_FtpRequestHeaders;
            }
        }
        
        // For portability only
        public override bool SupportsHeaders {
            get {
                return true;
            }
        }

        /// <summary>
        /// <para>Shows the final Uri that the FTP request ended up on</para>
        /// </summary>
        public override Uri ResponseUri {
            get {
                if (HttpProxyMode) {
                    return m_HttpWebResponse.ResponseUri;
                }
                return m_ResponseUri;
            }
        }


        /// <summary>
        /// <para>Last status code retrived</para>
        /// </summary>
        public FtpStatusCode StatusCode {
            get {
                if (HttpProxyMode) {
                    return ((FtpStatusCode) ((int) m_HttpWebResponse.StatusCode));
                }
                return m_StatusCode;
            }
        }

        /// <summary>
        /// <para>Last status line retrived</para>
        /// </summary>
        public string StatusDescription {
            get {
                if (HttpProxyMode) {
                    return m_HttpWebResponse.StatusDescription;
                }
                return m_StatusLine;
            }
        }

        /// <summary>
        /// <para>Returns last modified date time for given file (null if not relavant/avail)</para>
        /// </summary>
        public  DateTime LastModified {
            get {
                if (HttpProxyMode) {
                    return m_HttpWebResponse.LastModified;
                }
                return m_LastModified;
            }
        }

        /// <summary>
        ///    <para>Returns the server message sent before user credentials are sent</para>
        /// </summary>
        public string BannerMessage {
            get {
                return m_BannerMessage;
            }
        }

        /// <summary>
        ///    <para>Returns the server message sent after user credentials are sent</para>
        /// </summary>
        public string WelcomeMessage {
            get {
                return m_WelcomeMessage;
            }
        }

        /// <summary>
        ///    <para>Returns the exit sent message on shutdown</para>
        /// </summary>
        public string ExitMessage {
            get {
                return m_ExitMessage;
            }
        }


        /// <summary>
        ///    <para>True if request is just wrapping HttpWebRequest</para>
        /// </summary>
        private bool HttpProxyMode {
            get {
                return (m_HttpWebResponse != null);
            }
        }
    }
}

