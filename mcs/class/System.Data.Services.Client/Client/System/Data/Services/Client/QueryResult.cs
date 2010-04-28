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


namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif

    internal class QueryResult : BaseAsyncResult
    {
        internal readonly DataServiceRequest ServiceRequest;

        internal readonly HttpWebRequest Request;

        private static byte[] reusableAsyncCopyBuffer;

        private MemoryStream requestStreamContent;

        private Stream requestStream;

        private HttpWebResponse httpWebResponse;

        private Stream asyncResponseStream;

        private byte[] asyncStreamCopyBuffer;

        private Stream responseStream;

        private string contentType;

        private long contentLength;

        private HttpStatusCode statusCode;

        private bool responseStreamOwner;

        private bool usingBuffer;

#if StreamContainsBuffer
        private bool responseStreamIsCopyBuffer;
#endif

        internal QueryResult(object source, string method, DataServiceRequest serviceRequest, HttpWebRequest request, AsyncCallback callback, object state)
            : base(source, method, callback, state)
        {
            Debug.Assert(null != request, "null request");
            this.ServiceRequest = serviceRequest;
            this.Request = request;
            this.Abortable = request;
        }

        #region HttpResponse wrapper - ContentLength, ContentType, StatusCode

        internal long ContentLength
        {
            get { return this.contentLength; }
        }

        internal string ContentType
        {
            get { return this.contentType; }
        }

        internal HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }

        #endregion

        internal static QueryResult EndExecute<TElement>(object source, IAsyncResult asyncResult)
        {
            QueryResult response = null;

            try
            {
                response = BaseAsyncResult.EndExecute<QueryResult>(source, "Execute", asyncResult);
            }
            catch (InvalidOperationException ex)
            {
                response = asyncResult as QueryResult;
                Debug.Assert(response != null, "response != null, BaseAsyncResult.EndExecute() would have thrown a different exception otherwise.");

                QueryOperationResponse operationResponse = response.GetResponse<TElement>(MaterializeAtom.EmptyResults);
                if (operationResponse != null)
                {
                    operationResponse.Error = ex;
                    throw new DataServiceQueryException(Strings.DataServiceException_GeneralError, ex, operationResponse);
                }

                throw;
            }

            return response;
        }

        internal Stream GetResponseStream()
        {
            return this.responseStream;
        }

        internal void BeginExecute()
        {
            try
            {
                IAsyncResult asyncResult;
#if false
                if ((null != requestContent) && (0 < requestContent.Length))
                {
                    requestContent.Position = 0;
                    this.requestStreamContent = requestContent;
                    this.Request.ContentLength = requestContent.Length;
                    asyncResult = this.Request.BeginGetRequestStream(QueryAsyncResult.AsyncEndGetRequestStream, this);
                }
                else
#endif
                {
                    asyncResult = BaseAsyncResult.InvokeAsync(this.Request.BeginGetResponse, QueryResult.AsyncEndGetResponse, this);
                }

                this.CompletedSynchronously &= asyncResult.CompletedSynchronously;
            }
            catch (Exception e)
            {
                this.HandleFailure(e);
                throw;
            }
            finally
            {
                this.HandleCompleted();
            }

            Debug.Assert(!this.CompletedSynchronously || this.IsCompleted, "if CompletedSynchronously then MUST IsCompleted");
        }

#if !ASTORIA_LIGHT
        internal void Execute()
        {
            try
            {
#if false
                if ((null != requestContent) && (0 < requestContent.Length))
                {
                    using (System.IO.Stream stream = Util.NullCheck(this.Request.GetRequestStream(), InternalError.InvalidGetRequestStream))
                    {
                        byte[] buffer = requestContent.GetBuffer();
                        int bufferOffset = checked((int)requestContent.Position);
                        int bufferLength = checked((int)requestContent.Length) - bufferOffset;

                        stream.Write(buffer, bufferOffset, bufferLength);
                    }
                }
#endif

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)this.Request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                    if (null == response)
                    {
                        throw;
                    }
                }

                this.SetHttpWebResponse(Util.NullCheck(response, InternalError.InvalidGetResponse));

                if (HttpStatusCode.NoContent != this.StatusCode)
                {
                    using (Stream stream = this.httpWebResponse.GetResponseStream())
                    {
                        if (null != stream)
                        {
                            Stream copy = this.GetAsyncResponseStreamCopy();
                            this.responseStream = copy;

                            Byte[] buffer = this.GetAsyncResponseStreamCopyBuffer();

                            long copied = WebUtil.CopyStream(stream, copy, ref buffer);
                            if (this.responseStreamOwner)
                            {
                                if (0 == copied)
                                {
                                    this.responseStream = null;
                                }
                                else if (copy.Position < copy.Length)
                                {                                    ((MemoryStream)copy).SetLength(copy.Position);
                                }
                            }

                            this.PutAsyncResponseStreamCopyBuffer(buffer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.HandleFailure(e);
                throw;
            }
            finally
            {
                this.SetCompleted();
                this.CompletedRequest();
            }

            if (null != this.Failure)
            {
                throw this.Failure;
            }
        }
#endif

        internal QueryOperationResponse<TElement> GetResponse<TElement>(MaterializeAtom results)
        {
            if (this.httpWebResponse != null)
            {
                Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(this.httpWebResponse);
                QueryOperationResponse<TElement> response = new QueryOperationResponse<TElement>(headers, this.ServiceRequest, results);
                response.StatusCode = (int)this.httpWebResponse.StatusCode;
                return response;
            }

            return null;
        }

        internal QueryOperationResponse GetResponseWithType(MaterializeAtom results, Type elementType)
        {
            if (this.httpWebResponse != null)
            {
                Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(this.httpWebResponse);
                QueryOperationResponse response = QueryOperationResponse.GetInstance(elementType, headers, this.ServiceRequest, results);
                response.StatusCode = (int)this.httpWebResponse.StatusCode;
                return response;
            }

            return null;
        }

        internal MaterializeAtom GetMaterializer(DataServiceContext context, ProjectionPlan plan)
        {
            Debug.Assert(this.IsCompletedInternally, "request hasn't completed yet");

            MaterializeAtom materializer;
            if (HttpStatusCode.NoContent != this.StatusCode)
            {
                materializer = DataServiceRequest.Materialize(context, this.ServiceRequest.QueryComponents, plan, this.ContentType, this.GetResponseStream());
            }
            else
            {
                materializer = MaterializeAtom.EmptyResults;
            }

            return materializer;
        }
        
        internal QueryOperationResponse<TElement> ProcessResult<TElement>(DataServiceContext context, ProjectionPlan plan)
        {
            MaterializeAtom materializeAtom = DataServiceRequest.Materialize(context, this.ServiceRequest.QueryComponents, plan, this.ContentType, this.GetResponseStream());
            return this.GetResponse<TElement>(materializeAtom);
        }
        
        protected override void CompletedRequest()
        {
            Util.Dispose(ref this.asyncResponseStream);
            Util.Dispose(ref this.requestStream);
            Util.Dispose(ref this.requestStreamContent);

            byte[] buffer = this.asyncStreamCopyBuffer;
            this.asyncStreamCopyBuffer = null;
#if StreamContainsBuffer
            if (!this.responseStreamIsCopyBuffer)
#endif
            if ((null != buffer) && !this.usingBuffer)
            {
                this.PutAsyncResponseStreamCopyBuffer(buffer);
            }

            if (this.responseStreamOwner)
            {
                if (null != this.responseStream)
                {
                    this.responseStream.Position = 0;
                }
            }

            Debug.Assert(null != this.httpWebResponse || null != this.Failure, "should have response or exception");
            if (null != this.httpWebResponse)
            {
                this.httpWebResponse.Close();

                Exception ex = DataServiceContext.HandleResponse(this.StatusCode, this.httpWebResponse.Headers[XmlConstants.HttpDataServiceVersion], this.GetResponseStream, false);
                if (null != ex)
                {
                    this.HandleFailure(ex);
                }
            }
        }

        protected virtual Stream GetAsyncResponseStreamCopy()
        {
            this.responseStreamOwner = true;

            long length = this.contentLength;
            if ((0 < length) && (length <= Int32.MaxValue))
            {
                Debug.Assert(null == this.asyncStreamCopyBuffer, "not expecting buffer");

#if StreamContainsBuffer
                byte[] buffer = new byte[(int)length];
                if (length < UInt16.MaxValue)
                {                    responseStreamIsCopyBuffer = true;
                    this.asyncStreamCopyBuffer = buffer;
                }
                return new MemoryStream(buffer, 0, buffer.Length, true, true);
#else
                return new MemoryStream((int)length);
#endif
            }

            return new MemoryStream();
        }

        protected virtual byte[] GetAsyncResponseStreamCopyBuffer()
        {            Debug.Assert(null == this.asyncStreamCopyBuffer, "non-null this.asyncStreamCopyBuffer");
            return System.Threading.Interlocked.Exchange(ref reusableAsyncCopyBuffer, null) ?? new byte[8000];
        }

        protected virtual void PutAsyncResponseStreamCopyBuffer(byte[] buffer)
        {
            reusableAsyncCopyBuffer = buffer;
        }

        protected virtual void SetHttpWebResponse(HttpWebResponse response)
        {
            this.httpWebResponse = response;
            this.statusCode = response.StatusCode;
            this.contentLength = response.ContentLength;
            this.contentType = response.ContentType;
        }

#if false
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
        private static void AsyncEndGetRequestStream(IAsyncResult asyncResult)
        {
            QueryAsyncResult state = asyncResult.AsyncState as QueryAsyncResult;
            try
            {
                int step = CompleteCheck(state, InternalError.InvalidEndGetRequestCompleted);
                state.CompletedSynchronously &= asyncResult.CompletedSynchronously;
                HttpWebRequest httpWebRequest = Util.NullCheck(state.Request, InternalError.InvalidEndGetRequestStreamRequest);

                Stream stream = Util.NullCheck(httpWebRequest.EndGetRequestStream(asyncResult), InternalError.InvalidEndGetRequestStreamStream);
                state.requestStream = stream;

                MemoryStream memoryStream = Util.NullCheck(state.requestStreamContent, InternalError.InvalidEndGetRequestStreamContent);
                byte[] buffer = memoryStream.GetBuffer();
                int bufferOffset = checked((int)memoryStream.Position);
                int bufferLength = checked((int)memoryStream.Length) - bufferOffset;
                if ((null == buffer) || (0 == bufferLength))
                {
                    Error.ThrowInternalError(InternalError.InvalidEndGetRequestStreamContentLength);
                }

                asyncResult = stream.BeginWrite(buffer, bufferOffset, bufferLength, QueryAsyncResult.AsyncEndWrite, state);

                bool reallyCompletedSynchronously = asyncResult.CompletedSynchronously && (step < state.asyncCompleteStep);
                state.CompletedSynchronously &= reallyCompletedSynchronously;            }
            catch (Exception e)
            {
                if (state.HandleFailure(e))
                {
                    throw;
                }
            }
            finally
            {
                state.HandleCompleted();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
        private static void AsyncEndWrite(IAsyncResult asyncResult)
        {
            QueryAsyncResult state = asyncResult.AsyncState as QueryAsyncResult;
            try
            {
                int step = CompleteCheck(state, InternalError.InvalidEndWriteCompleted);
                state.CompletedSynchronously &= asyncResult.CompletedSynchronously;
                HttpWebRequest httpWebRequest = Util.NullCheck(state.Request, InternalError.InvalidEndWriteRequest);

                Stream stream = Util.NullCheck(state.requestStream, InternalError.InvalidEndWriteStream);
                stream.EndWrite(asyncResult);

                state.requestStream = null;
                stream.Dispose();

                stream = state.requestStreamContent;
                if (null != stream)
                {
                    state.requestStreamContent = null;
                    stream.Dispose();
                }

                asyncResult = httpWebRequest.BeginGetResponse(QueryAsyncResult.AsyncEndGetResponse, state);

                bool reallyCompletedSynchronously = asyncResult.CompletedSynchronously && (step < state.asyncCompleteStep);
                state.CompletedSynchronously &= reallyCompletedSynchronously;            }
            catch (Exception e)
            {
                if (state.HandleFailure(e))
                {
                    throw;
                }
            }
            finally
            {
                state.HandleCompleted();
            }
        }
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
        private static void AsyncEndGetResponse(IAsyncResult asyncResult)
        {
            Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
            QueryResult state = asyncResult.AsyncState as QueryResult;
            try
            {
                CompleteCheck(state, InternalError.InvalidEndGetResponseCompleted);
                state.CompletedSynchronously &= asyncResult.CompletedSynchronously;
                HttpWebRequest httpWebRequest = Util.NullCheck(state.Request, InternalError.InvalidEndGetResponseRequest);

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);
                }
                catch (WebException e)
                {
                    response = (HttpWebResponse)e.Response;
                    if (null == response)
                    {
                        throw;
                    }
                }

                state.SetHttpWebResponse(Util.NullCheck(response, InternalError.InvalidEndGetResponseResponse));
                Debug.Assert(null == state.asyncResponseStream, "non-null asyncResponseStream");

                Stream stream = null;
                if (HttpStatusCode.NoContent != response.StatusCode)
                {
                    stream = response.GetResponseStream();
                    state.asyncResponseStream = stream;
                }

                if ((null != stream) && stream.CanRead)
                {
                    if (null == state.responseStream)
                    {                        state.responseStream = Util.NullCheck(state.GetAsyncResponseStreamCopy(), InternalError.InvalidAsyncResponseStreamCopy);
                    }

                    if (null == state.asyncStreamCopyBuffer)
                    {                        state.asyncStreamCopyBuffer = Util.NullCheck(state.GetAsyncResponseStreamCopyBuffer(), InternalError.InvalidAsyncResponseStreamCopyBuffer);
                    }

                    QueryResult.ReadResponseStream(state);
                }
                else
                {
                    state.SetCompleted();
                }
            }
            catch (Exception e)
            {
                if (state.HandleFailure(e))
                {
                    throw;
                }
            }
            finally
            {
                state.HandleCompleted();
            }
        }

        private static void ReadResponseStream(QueryResult queryResult)
        {
            IAsyncResult asyncResult;

            byte[] buffer = queryResult.asyncStreamCopyBuffer;
            Stream stream = queryResult.asyncResponseStream;
            do
            {
                int bufferOffset, bufferLength;
#if StreamContainsBuffer
                if (state.responseStreamIsCopyBuffer)
                {                    bufferOffset = checked((int)state.responseStream.Position);
                    bufferLength = buffer.Length - bufferOffset;
                }
                else
#endif
                {
                    bufferOffset = 0;
                    bufferLength = buffer.Length;
                }

                queryResult.usingBuffer = true;
                asyncResult = BaseAsyncResult.InvokeAsync(stream.BeginRead, buffer, bufferOffset, bufferLength, QueryResult.AsyncEndRead, queryResult);
                queryResult.CompletedSynchronously &= asyncResult.CompletedSynchronously;            }
            while (asyncResult.CompletedSynchronously && !queryResult.IsCompletedInternally && stream.CanRead);

            Debug.Assert(!queryResult.CompletedSynchronously || queryResult.IsCompletedInternally, "AsyncEndGetResponse !IsCompleted");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
        private static void AsyncEndRead(IAsyncResult asyncResult)
        {
            Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
            QueryResult state = asyncResult.AsyncState as QueryResult;
            int count = 0;
            try
            {
                CompleteCheck(state, InternalError.InvalidEndReadCompleted);
                state.CompletedSynchronously &= asyncResult.CompletedSynchronously;
                Stream stream = Util.NullCheck(state.asyncResponseStream, InternalError.InvalidEndReadStream);
                Stream outputResponse = Util.NullCheck(state.responseStream, InternalError.InvalidEndReadCopy);
                byte[] buffer = Util.NullCheck(state.asyncStreamCopyBuffer, InternalError.InvalidEndReadBuffer);

                count = stream.EndRead(asyncResult);
                state.usingBuffer = false;
                if (0 < count)
                {
#if StreamContainsBuffer
                    if (state.responseStreamIsCopyBuffer)
                    {                        outputResponse.Position = outputResponse.Position + count;
                    }
                    else
#endif
                    {
                        outputResponse.Write(buffer, 0, count);
                    }
                }

                if (0 < count && 0 < buffer.Length && stream.CanRead)
                {
                    if (!asyncResult.CompletedSynchronously)
                    {
                        QueryResult.ReadResponseStream(state);
                    }
                }
                else
                {
#if StreamContainsBuffer
                    Debug.Assert(!state.responseStreamIsCopyBuffer || (outputResponse.Position == outputResponse.Length), "didn't read expected count");
#endif
                    if (outputResponse.Position < outputResponse.Length)
                    {
                        ((MemoryStream)outputResponse).SetLength(outputResponse.Position);
                    }

                    state.SetCompleted();
                }
            }
            catch (Exception e)
            {
                if (state.HandleFailure(e))
                {
                    throw;
                }
            }
            finally
            {
                state.HandleCompleted();
            }
        }

        private static void CompleteCheck(QueryResult pereq, InternalError errorcode)
        {
            if ((null == pereq) || (pereq.IsCompletedInternally && !pereq.IsAborted))
            {
                Error.ThrowInternalError(errorcode);
            }
        }
    }
}
