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
    using System.Globalization;
    using System.IO;

    internal abstract class HttpWebResponse : System.Data.Services.Http.WebResponse, IDisposable
    {
        #region Properties.

        public abstract System.Data.Services.Http.WebHeaderCollection Headers
        {
            get;
        }

        public abstract System.Data.Services.Http.HttpWebRequest Request
        {
            get;
        }

        public abstract System.Data.Services.Http.HttpStatusCode StatusCode
        {
            get;
        }

        #endregion Properties.

        public abstract string GetResponseHeader(string headerName);
    }
}

