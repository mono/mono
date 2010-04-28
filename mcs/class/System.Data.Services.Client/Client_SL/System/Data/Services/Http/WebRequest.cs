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
    #region Namespaces.

    using System;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.IO;

    #endregion Namespaces.

    internal abstract class WebRequest
    {
        public abstract string ContentType 
        { 
            get; 
            set; 
        }

        public abstract System.Data.Services.Http.WebHeaderCollection Headers
        {
            get;
        }

        public abstract string Method 
        { 
            get; 
            set; 
        }

        public abstract Uri RequestUri 
        { 
            get; 
        }

        public static System.Data.Services.Http.WebRequest Create(Uri requestUri, HttpStack httpStack)
        {
            Debug.Assert(requestUri != null, "requestUri != null");
            if ((requestUri.Scheme != Uri.UriSchemeHttp) && (requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new NotSupportedException();
            }

            if (httpStack == HttpStack.Auto)
            {
                if (UriRequiresClientHttpWebRequest(requestUri))
                {
                    httpStack = HttpStack.ClientHttp;
                }
                else
                {
                    httpStack = HttpStack.XmlHttp;
                }
            }

            if (httpStack == HttpStack.ClientHttp)
            {
                return new ClientHttpWebRequest(requestUri);
            }
            else
            {
                Debug.Assert(httpStack == HttpStack.XmlHttp, "Only ClientHttp and XmlHttp are supported for now.");
                return new XHRHttpWebRequest(requestUri);
            }
        }

        public abstract void Abort();

        public abstract IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state);

        public abstract IAsyncResult BeginGetResponse(AsyncCallback callback, object state);

        public abstract Stream EndGetRequestStream(IAsyncResult asyncResult);

        public abstract System.Data.Services.Http.WebResponse EndGetResponse(IAsyncResult asyncResult);

        private static bool UriRequiresClientHttpWebRequest(Uri uri)
        {
            if (!XHRHttpWebRequest.IsAvailable())
            {
                return true;
            }

            Uri sameDomainUri = System.Windows.Browser.HtmlPage.Document.DocumentUri;

            if (sameDomainUri.Scheme != uri.Scheme || sameDomainUri.Port != uri.Port ||
                !string.Equals(sameDomainUri.DnsSafeHost, uri.DnsSafeHost, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
