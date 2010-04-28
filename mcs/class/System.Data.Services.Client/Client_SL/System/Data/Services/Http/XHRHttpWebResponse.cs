//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Http
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    internal sealed class XHRHttpWebResponse : System.Data.Services.Http.HttpWebResponse
    {
        #region Fields.

        private System.Data.Services.Http.XHRWebHeaderCollection headers;
        
        private System.Data.Services.Http.XHRHttpWebRequest request;
        
        private int statusCode;

        #endregion Fields.

        internal XHRHttpWebResponse(System.Data.Services.Http.XHRHttpWebRequest request, int statusCode, string responseHeaders)
        {
            Debug.Assert(request != null, "request can't be null.");
            this.request = request;
            NormalizeResponseStatus(ref statusCode);
            this.statusCode = statusCode;
            this.headers = new System.Data.Services.Http.XHRWebHeaderCollection();
            
            this.ParseHeaders(responseHeaders);
        }

        #region Properties.

        public override long ContentLength
        {
            get
            {
                return Convert.ToInt64(this.Headers["Content-Length"], CultureInfo.InvariantCulture);
            }
        }

        public override string ContentType
        {
            get
            {
                return this.Headers["Content-Type"];
            }
        }

        public override System.Data.Services.Http.WebHeaderCollection Headers
        {
            get
            {
                return this.headers;
            }
        }

        public override System.Data.Services.Http.HttpWebRequest Request
        {
            get
            {
                return this.request;
            }
        }

        public override System.Data.Services.Http.HttpStatusCode StatusCode
        {
            get
            {
                return (System.Data.Services.Http.HttpStatusCode)this.statusCode;
            }
        }

        internal System.Data.Services.Http.XHRHttpWebRequest InternalRequest
        {
            set
            {
                this.request = value;
            }
        }

        #endregion Properties.

        public override void Close()
        {
            this.request.Close();
        }

        public override string GetResponseHeader(string headerName)
        {
            return this.headers[headerName];
        }

        public override Stream GetResponseStream()
        {
            return this.request.ReadResponse(this);
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
        }

        private static void NormalizeResponseStatus(ref int statusCodeParam)
        {

            string userAgent = System.Windows.Browser.HtmlPage.BrowserInformation.UserAgent;
            bool browserIsIE = userAgent != null && userAgent.ToUpper(CultureInfo.InvariantCulture).Contains("MSIE");
            if (browserIsIE)
            {
                if (statusCodeParam == 1223)
                {
                    statusCodeParam = 204;
                }
                else if (statusCodeParam == 12150)
                {
                    statusCodeParam = 399;
                }
            }

            if (statusCodeParam > (int)HttpStatusCodeRange.MaxValue || statusCodeParam < (int)HttpStatusCodeRange.MinValue)
            {
                throw WebException.CreateInternal("HttpWebResponse.NormalizeResponseStatus");
            }
        }

        private void ParseHeaders(string responseHeaders)
        {
            if (string.IsNullOrEmpty(responseHeaders))
            {
                return;
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHeaders);
            WebParseError error = new WebParseError();
            int totalResponseHeadersLength = 0;
            int offset = 0;
            int maxHeaderLength = 64000;
            try
            {
                DataParseStatus result = this.headers.ParseHeaders(buffer, buffer.Length, ref offset, ref totalResponseHeadersLength, maxHeaderLength, ref error);

                if (result != DataParseStatus.Done)
                {
                    throw WebException.CreateInternal("HttpWebResponse.ParseHeaders");
                }
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception e)
            {
                string message = System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebResponse.ParseHeaders.2");
                throw new WebException(message, e);
            }
        }
    }
}

