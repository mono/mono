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
    using System.Diagnostics;

    public class SendingRequestEventArgs : EventArgs
    {
#if ASTORIA_LIGHT
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not used in Silverlight")]
#endif
        private System.Net.WebRequest request;

        private System.Net.WebHeaderCollection requestHeaders;

        internal SendingRequestEventArgs(System.Net.WebRequest request, System.Net.WebHeaderCollection requestHeaders)
        {
#if ASTORIA_LIGHT
            Debug.Assert(null == request, "non-null request in SL.");
#else
            Debug.Assert(null != request, "null request");
#endif
            Debug.Assert(null != requestHeaders, "null requestHeaders");
            this.request = request;
            this.requestHeaders = requestHeaders;
        }

#if !ASTORIA_LIGHT        
        public System.Net.WebRequest Request
        {
            get
            {
                return this.request;
            }

            set
            {
                Util.CheckArgumentNull(value, "value");
                if (!(value is System.Net.HttpWebRequest))
                {
                    throw Error.Argument(Strings.Context_SendingRequestEventArgsNotHttp, "value");
                }

                this.request = value;
                this.requestHeaders = value.Headers;
            }
        }
#endif

        public System.Net.WebHeaderCollection RequestHeaders
        {
            get { return this.requestHeaders; }
        }
    }
}
