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
    using System.Diagnostics;

    #endregion Namespaces.

    internal abstract class HttpWebRequest : System.Data.Services.Http.WebRequest, IDisposable
    {
        public abstract string Accept
        {
            get;
            set;
        }

        public abstract long ContentLength
        {
            set;
        }

        public abstract bool AllowReadStreamBuffering
        {
            get;
            set;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public abstract System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection();

        protected abstract void Dispose(bool disposing);
    }
}
