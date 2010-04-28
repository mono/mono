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
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows.Browser;
    using System.Diagnostics;

    #endregion Namespaces.

    internal sealed class ClientHttpWebRequest : System.Data.Services.Http.HttpWebRequest
    {
        private readonly System.Net.HttpWebRequest innerRequest;

        private ClientWebHeaderCollection headerCollection;

        public ClientHttpWebRequest(Uri requestUri)
        {
            Debug.Assert(requestUri != null, "requestUri can't be null.");
            this.innerRequest = (System.Net.HttpWebRequest)System.Net.Browser.WebRequestCreator.ClientHttp.Create(requestUri);
            Debug.Assert(this.innerRequest != null, "ClientHttp.Create failed to create a new request without throwing exception.");
        }

        public override string Accept
        {
            get
            {
                return this.innerRequest.Accept;
            }

            set
            {
                this.innerRequest.Accept = value;
            }
        }

        public override long ContentLength
        {
            set
            {
                return;
            }
        }

        public override bool AllowReadStreamBuffering
        {
            get
            {
                return this.innerRequest.AllowReadStreamBuffering;
            }

            set
            {
                this.innerRequest.AllowReadStreamBuffering = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.innerRequest.ContentType;
            }

            set
            {
                this.innerRequest.ContentType = value;
            }
        }

        public override System.Data.Services.Http.WebHeaderCollection Headers
        {
            get
            {
                if (this.headerCollection == null)
                {
                    this.headerCollection = new ClientWebHeaderCollection(this.innerRequest.Headers, this.innerRequest);
                }

                return this.headerCollection;
            }
        }

        public override string Method
        {
            get
            {
                return this.innerRequest.Method;
            }

            set
            {
                this.innerRequest.Method = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.innerRequest.RequestUri;
            }
        }

        public override void Abort()
        {
            this.innerRequest.Abort();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return this.innerRequest.BeginGetRequestStream(callback, state);
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return this.innerRequest.BeginGetResponse(callback, state);
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return this.innerRequest.EndGetRequestStream(asyncResult);
        }

        public override System.Data.Services.Http.WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            try
            {
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)this.innerRequest.EndGetResponse(asyncResult);
                return new ClientHttpWebResponse(response, this);
            }
            catch (System.Net.WebException exception)
            {
                ClientHttpWebResponse response = new ClientHttpWebResponse((System.Net.HttpWebResponse)exception.Response, this);
                throw new System.Data.Services.Http.WebException(exception.Message, exception, response);
            }
        }

        public override System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection()
        {
            return new System.Net.WebHeaderCollection();
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
