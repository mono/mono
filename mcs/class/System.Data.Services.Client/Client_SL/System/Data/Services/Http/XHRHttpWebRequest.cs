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

    internal sealed class XHRHttpWebRequest : System.Data.Services.Http.HttpWebRequest
    {
        #region Private fields.

        private bool aborted;

        private HttpWebRequestAsyncResult asyncRequestResult;

        private HttpWebRequestAsyncResult asyncResponseResult;

        private NonClosingMemoryStream contentStream;

        private System.Data.Services.Http.XHRWebHeaderCollection headers;

        private bool invoked;

        private string method;

        private System.Data.Services.Http.HttpWebResponse response;

        private ScriptXmlHttpRequest underlyingRequest;

        private Uri uri;

        #endregion Private fields.

        internal XHRHttpWebRequest(Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");
            this.uri = uri;
        }

        public override string Accept
        {
            get
            {
                return this.headers[System.Data.Services.Http.HttpRequestHeader.Accept];
            }

            set
            {
                this.headers.SetSpecialHeader("accept", value);
            }
        }

        public override long ContentLength
        {
            set
            {
                this.headers[System.Data.Services.Http.HttpRequestHeader.ContentLength] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override bool AllowReadStreamBuffering
        {
            get { return true; }
            set {  }
        }

        public override string ContentType
        {
            get
            {
                return this.headers[System.Data.Services.Http.HttpRequestHeader.ContentType];
            }

            set
            {
                this.headers.SetSpecialHeader("content-type", value);
            }
        }

        public override System.Data.Services.Http.WebHeaderCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new System.Data.Services.Http.XHRWebHeaderCollection(System.Data.Services.Http.WebHeaderCollectionType.HttpWebRequest);
                }

                return this.headers;
            }
        }

        public override string Method
        {
            get { return this.method; }
            set { this.method = value; }
        }

        public override Uri RequestUri
        {
            get { return this.uri; }
        }

        public static bool IsAvailable()
        {
            try
            {
                ScriptXmlHttpRequest request = new ScriptXmlHttpRequest();
                return (null != request);
            }
            catch (WebException)
            {
                return false;
            }
        }

       public override void Abort()
        {
            this.aborted = true;
            if (this.underlyingRequest != null)
            {
                this.underlyingRequest.Abort();
                this.underlyingRequest.Dispose();
                this.underlyingRequest = null;
            }

            if (this.response != null)
            {
                ((XHRHttpWebResponse)this.response).InternalRequest = null;
                this.response = null;
            }

            this.Close();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            if (this.aborted)
            {
                throw CreateAbortException();
            }

            if (this.contentStream == null)
            {
                this.contentStream = new NonClosingMemoryStream();
            }
            else
            {
                this.contentStream.Seek(0L, SeekOrigin.Begin);
            }
            
            HttpWebRequestAsyncResult asyncResult = new HttpWebRequestAsyncResult(callback, state);
            this.asyncRequestResult = asyncResult;
            this.asyncRequestResult.CompletedSynchronously = true;  
            if (asyncResult != null)
            {
                asyncResult.SetCompleted();
                asyncResult.Callback(asyncResult);
            }
            
            return asyncResult;
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            HttpWebRequestAsyncResult asyncResult = new HttpWebRequestAsyncResult(callback, state);
            try
            {
                asyncResult.InsideBegin = true;
                this.asyncResponseResult = asyncResult;
                this.InvokeRequest();
            }
            finally
            {
                asyncResult.InsideBegin = false;
            }

            return asyncResult;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
         
            if (this.asyncRequestResult != asyncResult)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebRequest.EndGetRequestStream"));
            }
            
            if (this.asyncRequestResult.EndCalled)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebRequest.EndGetRequestStream.2"));
            }
            
            if (this.aborted)
            {
                throw CreateAbortException();
            }

            this.asyncRequestResult.EndCalled = true;
            this.asyncRequestResult.Dispose();
            this.asyncRequestResult = null;
            return this.contentStream;
        }

         public override System.Data.Services.Http.WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            if (this.asyncResponseResult != asyncResult)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebRequest.EndGetResponse"));
            }
            
            if (this.asyncResponseResult.EndCalled)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebRequest.EndGetResponse.2"));
            }
            
            if (this.aborted)
            {
                throw CreateAbortException();
            }

            this.asyncResponseResult.EndCalled = true;
            this.CreateResponse();
            this.asyncResponseResult.Dispose();
            this.asyncResponseResult = null;
            return this.response;
        }

       public override System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection()
        {
            return System.Net.WebRequest.Create(this.RequestUri).Headers;
        }

        internal void Close()
        {
            this.Dispose(true);
        }

        internal Stream ReadResponse(IDisposable connection)
        {
            Debug.Assert(connection != null, "connection != null");

            if ((this.response.ContentType == null) ||
                this.response.ContentType.Contains("json") ||
                this.response.ContentType.Contains("xml") ||
                this.response.ContentType.Contains("text") ||
                this.response.ContentType.Contains("multipart"))
            {
                string buffer = this.underlyingRequest.ReadResponseAsString();

                return (!string.IsNullOrEmpty(buffer) ? new DisposingMemoryStream(connection, Encoding.UTF8.GetBytes(buffer)) : null);
            }

            throw WebException.CreateInternal("HttpWebRequest.ReadResponse");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.contentStream != null)
                {
                    this.contentStream.InternalDispose();
                    this.contentStream = null;
                }
            }
        }

        private static System.Data.Services.Http.WebException CreateAbortException()
        {
            return new System.Data.Services.Http.WebException(System.Data.Services.Client.Strings.HttpWebRequest_Aborted);
        }

        private void CreateResponse()
        {
            int statusCode;
            this.underlyingRequest.GetResponseStatus(out statusCode);
            if (statusCode != -1)
            {
                string responseHeaders = this.underlyingRequest.GetResponseHeaders();
                this.response = new System.Data.Services.Http.XHRHttpWebResponse(this, statusCode, responseHeaders);
            }
        }

        private void ReadyStateChanged()
        {
            if (this.underlyingRequest.IsCompleted && (this.asyncResponseResult != null))
            {
                try
                {
                    if (this.asyncResponseResult.InsideBegin)
                    {
                        this.asyncResponseResult.CompletedSynchronously = true;
                    }

                    this.asyncResponseResult.SetCompleted();
                    this.asyncResponseResult.Callback(this.asyncResponseResult);
                }
                finally
                {
                    this.underlyingRequest.Dispose();
                }
            }
        }

        private void InvokeRequest()
        {
            if (this.aborted)
            {
                throw CreateAbortException();
            }

            if (this.invoked)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("HttpWebRequest.InvokeRequest"));
            }

            this.invoked = true;
            this.underlyingRequest = new ScriptXmlHttpRequest();
            this.underlyingRequest.Open(this.uri.AbsoluteUri, this.Method, (Action)this.ReadyStateChanged);

            if ((this.headers != null) && (this.headers.Count != 0))
            {
                foreach (string header in this.headers.AllKeys)
                {
                    string value = this.headers[header];
                    this.underlyingRequest.SetRequestHeader(header, value);
                }
            }

            string content = null;
            if (this.contentStream != null)
            {
                byte[] buf = this.contentStream.GetBuffer();
                if (buf != null)
                {
                    int bufferSize = checked((int)this.contentStream.Position);
                    content = Encoding.UTF8.GetString(buf, 0, bufferSize);
                    this.underlyingRequest.SetRequestHeader("content-length", bufferSize.ToString(CultureInfo.InvariantCulture));
                }
            }

            this.underlyingRequest.Send(content);
        }

        private sealed class HttpWebRequestAsyncResult : IAsyncResult, IDisposable
        {
            private AsyncCallback callback;

            private bool completed;

            private bool completedSynchronously;

            private bool endCalled;

             private object state;

            private ManualResetEvent waitHandle;

            public HttpWebRequestAsyncResult(AsyncCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public object AsyncState
            {
                get { return this.state; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.waitHandle == null)
                    {
                        this.waitHandle = new ManualResetEvent(false);
                    }

                    return this.waitHandle;
                }
            }

            public AsyncCallback Callback
            {
                get { return this.callback; }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return this.completedSynchronously;
                }

                internal set
                {
                    this.completedSynchronously = value;
                }
            }

            public bool EndCalled
            {
                get { return this.endCalled; }
                set { this.endCalled = value; }
            }

            public bool IsCompleted
            {
                get { return this.completed; }
            }

            public bool InsideBegin
            { 
                get; 
                set;
            }

            public void Dispose()
            {
                if (this.waitHandle != null)
                {
                    ((IDisposable)this.waitHandle).Dispose();
                }
            }

            public void SetCompleted()
            {
                this.completed = true;
                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }
        }

        private sealed class DisposingMemoryStream : MemoryStream
        {
            private readonly IDisposable disposable;

            internal DisposingMemoryStream(IDisposable disposable, byte[] buffer) : base(buffer)
            {
                Debug.Assert(disposable != null, "disposable != null");
                this.disposable = disposable;
            }

            protected override void Dispose(bool disposing)
            {
                this.disposable.Dispose();
                base.Dispose(disposing);
            }
        }

        private sealed class NonClosingMemoryStream : MemoryStream
        {
            public override void Close()
            {
                
            }

            internal void InternalDispose()
            {
                base.Dispose();
            }

            protected override void Dispose(bool disposing)
            {
            }
        }
    }
}
