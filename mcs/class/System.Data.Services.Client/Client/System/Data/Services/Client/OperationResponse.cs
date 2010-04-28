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
    using System.Collections.Generic;
    using System.Diagnostics;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public abstract class OperationResponse
    {
        private Dictionary<string, string> headers;

        private int statusCode;

        private Exception innerException;

        internal OperationResponse(Dictionary<string, string> headers)
        {
            Debug.Assert(null != headers, "null headers");
            this.headers = headers;
        }

        public IDictionary<string, string> Headers
        {
            get { return this.headers; }
        }

        public int StatusCode
        {
            get { return this.statusCode; }
            internal set { this.statusCode = value; }
        }

        public Exception Error
        {
            get
            {
                return this.innerException;
            }

            set
            {
                Debug.Assert(null != value, "should not set null");
                this.innerException = value;
            }
        }
    }
}
