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
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Windows.Browser;

    #endregion Namespaces.

    internal sealed class ScriptXmlHttpRequest
    {
        #region Private fields.

        private ScriptObject request;

        #endregion Private fields.

        #region Constructors.

        public ScriptXmlHttpRequest()
        {
            this.request = CreateNativeRequest();
            Debug.Assert(this.request != null, "this.request != null");
        }

        #endregion Constructors.

        #region Properties.

        internal bool IsCompleted
        {
            get
            {
                return
                    this.request == null ||
                    (Convert.ToInt32((double)this.request.GetProperty("readyState")) == 4);
            }
        }

        #endregion Properties.

        #region Methods.

        public void Dispose()
        {
            var currentRequest = this.request;
            if (currentRequest != null)
            {
                try
                {
                    ScriptObjectUtility.SetReadyStateChange(currentRequest, null);
                }
                finally
                {
                    this.request = null;
                }

            }
        }

        public string GetResponseHeaders()
        {
            string responseHeaders = (string)this.request.Invoke("getAllResponseHeaders", new object[0]);
            if (string.IsNullOrEmpty(responseHeaders))
            {
                return string.Empty;
            }

            int indexOfColon = responseHeaders.IndexOf(':');
            int indexOfSeparator = responseHeaders.IndexOf('\n');
            if (indexOfColon > indexOfSeparator)
            {
                responseHeaders = responseHeaders.Substring(indexOfSeparator + 1);
            }

            if (responseHeaders.IndexOf("\r\n", StringComparison.Ordinal) == -1)
            {
                responseHeaders = responseHeaders.Replace("\n", "\r\n");
            }

            if (responseHeaders.EndsWith("\r\n\r\n", StringComparison.Ordinal))
            {
                return responseHeaders;
            }

            if (!responseHeaders.EndsWith("\r\n", StringComparison.Ordinal))
            {
                return (responseHeaders + "\r\n\r\n");
            }

            return (responseHeaders + "\r\n");
        }

        public void GetResponseStatus(out int statusCode)
        {
            try
            {
                statusCode = Convert.ToInt32((double)this.request.GetProperty("status"));
            }
            catch (Exception e)
            {
                string message = System.Data.Services.Client.Strings.HttpWeb_Internal("ScriptXmlHttpRequest.HttpWebRequest");
                throw new WebException(message, e);
            }
        }

        public void Open(string uri, string method, Action readyStateChangeCallback)
        {
            Util.CheckArgumentNull(uri, "uri");
            Util.CheckArgumentNull(method, "method");
            Util.CheckArgumentNull(readyStateChangeCallback, "readyStateChangeCallback");
            
            ScriptObject callback = ScriptObjectUtility.ToScriptFunction(readyStateChangeCallback);
            ScriptObjectUtility.CallOpen(this.request, method, uri);
            ScriptObjectUtility.SetReadyStateChange(this.request, callback);
        }

        public string ReadResponseAsString()
        {
            Debug.Assert(this.request != null, "this.request != null");
            return (string)this.request.GetProperty("responseText");
        }

        public void Send(string content)
        {
            Debug.Assert(this.request != null, "this.request != null");
            this.request.Invoke("send", content ?? string.Empty);
        }

        public void SetRequestHeader(string header, string value)
        {
            Debug.Assert(this.request != null, "this.request != null");
            this.request.Invoke("setRequestHeader", header, value);
        }

        internal void Abort()
        {
            var requestValue = this.request;
            if (requestValue != null)
            {
                requestValue.Invoke("abort", new object[0]);
            }
        }

        private static bool CreateInstance(string typeName, object arg, out ScriptObject request)
        {
            request = null;
            try
            {
                object[] args = (arg == null) ? null : new object[] { arg };
                request = HtmlPage.Window.CreateInstance(typeName, args);
            }
            catch (Exception exception)
            {
                if (Util.DoNotHandleException(exception))
                {
                    throw;
                }

            }

            return (null != request);
        }

        private static ScriptObject CreateNativeRequest()
        {
            ScriptObject result;
            if (!CreateInstance("XMLHttpRequest", null, out result) &&
                !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.6.0", out result) &&
                !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.3.0", out result) &&
                !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.2.0", out result) &&
                !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP", out result) &&
                !CreateInstance("ActiveXObject", "XMLHttpRequest", out result) &&
                !CreateInstance("ActiveXObject", "Microsoft.XMLHTTP", out result))
            {
                throw WebException.CreateInternal("ScriptXmlHttpRequest.CreateNativeRequest");
            }

            Debug.Assert(result != null, "result != null -- otherwise CreateInstance should not have returned true");
            return result;
        }

        #endregion Methods.
    }
}
