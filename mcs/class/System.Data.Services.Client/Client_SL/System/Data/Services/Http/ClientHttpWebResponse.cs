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

    internal sealed class ClientHttpWebResponse : System.Data.Services.Http.HttpWebResponse
    {
        private System.Net.HttpWebResponse innerResponse;

        private ClientWebHeaderCollection headerCollection;

        private ClientHttpWebRequest request;

        internal ClientHttpWebResponse(System.Net.HttpWebResponse innerResponse, ClientHttpWebRequest request)
        {
            Debug.Assert(innerResponse != null, "innerResponse can't be null.");
            this.innerResponse = innerResponse;
            this.request = request;
            int statusCode = (int)this.innerResponse.StatusCode;
            if (statusCode > (int)HttpStatusCodeRange.MaxValue || statusCode < (int)HttpStatusCodeRange.MinValue)
            {
                throw WebException.CreateInternal("HttpWebResponse.NormalizeResponseStatus");
            }
        }

        #region Properties.

        public override long ContentLength
        {
            get
            {
                return this.innerResponse.ContentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.innerResponse.ContentType;
            }
        }

        public override System.Data.Services.Http.WebHeaderCollection Headers
        {
            get
            {
                if (this.headerCollection == null)
                {
                    this.headerCollection = new ClientWebHeaderCollection(this.innerResponse.Headers);
                }

                return this.headerCollection;
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
                return (System.Data.Services.Http.HttpStatusCode)(int)this.innerResponse.StatusCode;
            }
        }

        #endregion Properties.

        public override void Close()
        {
            this.innerResponse.Close();
        }

        public override string GetResponseHeader(string headerName)
        {
            return this.innerResponse.Headers[headerName];
        }

        public override Stream GetResponseStream()
        {
            return this.innerResponse.GetResponseStream();
        }

        protected override void Dispose(bool disposing)
        {
            ((IDisposable)this.innerResponse).Dispose();
        }
    }
}

