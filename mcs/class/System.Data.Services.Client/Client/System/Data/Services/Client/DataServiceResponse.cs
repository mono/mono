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
    using System.Collections.Generic;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public sealed class DataServiceResponse : IEnumerable<OperationResponse>
    {
        private Dictionary<string, string> headers;

        private int statusCode;

        private IEnumerable<OperationResponse> response;

        private bool batchResponse;

        internal DataServiceResponse(Dictionary<string, string> headers, int statusCode, IEnumerable<OperationResponse> response, bool batchResponse)
        {
            this.headers = headers ?? new Dictionary<string, string>(EqualityComparer<string>.Default);
            this.statusCode = statusCode;
            this.batchResponse = batchResponse;
            this.response = response;
        }

        public IDictionary<string, string> BatchHeaders
        {
            get { return this.headers; }
        }

        public int BatchStatusCode
        {
            get { return this.statusCode; }
        }

        public bool IsBatchResponse
        {
            get { return this.batchResponse; }
        }

        public IEnumerator<OperationResponse> GetEnumerator()
        {
            return this.response.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
