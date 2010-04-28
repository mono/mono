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

    internal sealed class WebException : InvalidOperationException
    {
        private HttpWebResponse response;

        public WebException()
        {
        }

        public WebException(string message) : this(message, (Exception) null)
        {
        }

        public WebException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebException(string message, Exception innerException, HttpWebResponse response)
            : base(message, innerException)
        {
            this.response = response;
        }

        public System.Data.Services.Http.HttpWebResponse Response
        {
            get
            {
                return this.response;
            }
        }

        internal static WebException CreateInternal(string location)
        {
            return new WebException(System.Data.Services.Client.Strings.HttpWeb_Internal(location));
        }
    }
}
