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
    using System.Linq;
    using System.Text;
#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif
    using System.Xml;

    public abstract class DataServiceRequest
    {
        internal DataServiceRequest()
        {
        }

        public abstract Type ElementType
        {
            get;
        }

        public abstract Uri RequestUri
        {
            get;
        }

        internal abstract ProjectionPlan Plan
        {
            get;
        }

        internal abstract QueryComponents QueryComponents
        {
            get;
        }

        public override string ToString()
        {
            return this.QueryComponents.Uri.ToString();
        }   

        internal static MaterializeAtom Materialize(DataServiceContext context, QueryComponents queryComponents, ProjectionPlan plan, string contentType, Stream response)
        {
            Debug.Assert(null != queryComponents, "querycomponents");

            string mime = null;
            Encoding encoding = null;
            if (!String.IsNullOrEmpty(contentType))
            {
                HttpProcessUtility.ReadContentType(contentType, out mime, out encoding);
            }

            if (String.Equals(mime, XmlConstants.MimeApplicationAtom, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(mime, XmlConstants.MimeApplicationXml, StringComparison.OrdinalIgnoreCase))
            {
                if (null != response)
                {
                    XmlReader reader = XmlUtil.CreateXmlReader(response, encoding);
                    return new MaterializeAtom(context, reader, queryComponents, plan, context.MergeOption);
                }
            }

            return MaterializeAtom.EmptyResults;
        }

        internal static DataServiceRequest GetInstance(Type elementType, Uri requestUri)
        {
            Type genericType = typeof(DataServiceRequest<>).MakeGenericType(elementType);
            return (DataServiceRequest)Activator.CreateInstance(genericType, new object[] { requestUri });
        }

        internal static IEnumerable<TElement> EndExecute<TElement>(object source, DataServiceContext context, IAsyncResult asyncResult)
        {
            QueryResult result = null;
            try
            {
                result = QueryResult.EndExecute<TElement>(source, asyncResult);
                return result.ProcessResult<TElement>(context, result.ServiceRequest.Plan);
            }
            catch (DataServiceQueryException ex)
            {
                Exception inEx = ex;
                while (inEx.InnerException != null)
                {
                    inEx = inEx.InnerException;
                }

                DataServiceClientException serviceEx = inEx as DataServiceClientException;
                if (context.IgnoreResourceNotFoundException && serviceEx != null && serviceEx.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    QueryOperationResponse qor = new QueryOperationResponse<TElement>(new Dictionary<string, string>(ex.Response.Headers), ex.Response.Query, MaterializeAtom.EmptyResults);
                    qor.StatusCode = (int)HttpStatusCode.NotFound;
                    return (IEnumerable<TElement>)qor;
                }

                throw;
            }
        }

#if !ASTORIA_LIGHT        
        internal QueryOperationResponse<TElement> Execute<TElement>(DataServiceContext context, QueryComponents queryComponents)
        {
            QueryResult result = null;
            try
            {
                DataServiceRequest<TElement> serviceRequest = new DataServiceRequest<TElement>(queryComponents, this.Plan);
                result = serviceRequest.CreateResult(this, context, null, null);
                result.Execute();
                return result.ProcessResult<TElement>(context, this.Plan);
            }
            catch (InvalidOperationException ex)
            {
                QueryOperationResponse operationResponse = result.GetResponse<TElement>(MaterializeAtom.EmptyResults);

                if (null != operationResponse)
                {
                    if (context.IgnoreResourceNotFoundException)
                    {
                        DataServiceClientException cex = ex as DataServiceClientException;
                        if (cex != null && cex.StatusCode == (int)HttpStatusCode.NotFound)
                        {
                            return (QueryOperationResponse<TElement>)operationResponse;
                        }
                    }

                    operationResponse.Error = ex;
                    throw new DataServiceQueryException(Strings.DataServiceException_GeneralError, ex, operationResponse);
                }

                throw;
            }
        }

        internal long GetQuerySetCount(DataServiceContext context)
        {
            Debug.Assert(null != context, "context is null");
            this.QueryComponents.Version = Util.DataServiceVersion2;

            QueryResult response = null;
            DataServiceRequest<long> serviceRequest = new DataServiceRequest<long>(this.QueryComponents, null);
            HttpWebRequest request = context.CreateRequest(this.QueryComponents.Uri, XmlConstants.HttpMethodGet, false, null, this.QueryComponents.Version, false);
            
            request.Accept = "text/plain";
            response = new QueryResult(this, "Execute", serviceRequest, request, null, null);

            try
            {
                response.Execute();

                if (HttpStatusCode.NoContent != response.StatusCode)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    long r = -1;
                    try
                    {
                        r = XmlConvert.ToInt64(sr.ReadToEnd());
                    }
                    finally
                    {
                        sr.Close();
                    }

                    return r;
                }
                else
                {
                    throw new DataServiceQueryException(Strings.DataServiceRequest_FailGetCount, response.Failure);
                }
            }
            catch (InvalidOperationException ex)
            {
                QueryOperationResponse operationResponse = null;
                operationResponse = response.GetResponse<long>(MaterializeAtom.EmptyResults);
                if (null != operationResponse)
                {
                    operationResponse.Error = ex;
                    throw new DataServiceQueryException(Strings.DataServiceException_GeneralError, ex, operationResponse);
                }

                throw;
            }
        }
#endif

        internal IAsyncResult BeginExecute(object source, DataServiceContext context, AsyncCallback callback, object state)
        {
            QueryResult result = this.CreateResult(source, context, callback, state);
            result.BeginExecute();
            return result;
        }

        private QueryResult CreateResult(object source, DataServiceContext context, AsyncCallback callback, object state)
        {
            Debug.Assert(null != context, "context is null");
            HttpWebRequest request = context.CreateRequest(this.QueryComponents.Uri, XmlConstants.HttpMethodGet, false, null, this.QueryComponents.Version, false);
            return new QueryResult(source, "Execute", this, request, callback, state);
        }
    }
}
