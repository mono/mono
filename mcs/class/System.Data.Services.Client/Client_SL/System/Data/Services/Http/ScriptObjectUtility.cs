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
    using System.Diagnostics;
    using System.Windows.Browser;

    #endregion Namespaces.

    internal static class ScriptObjectUtility
    {
        private const string HelperScript =
@"({
    cd: function(f) { return function() { f(); }; },
    callOpen: function(requestObj, method, uri) {
        requestObj.open(method,uri,true);
    },
    setReadyStateChange: function(requestObj, o1) {
        requestObj.onreadystatechange = o1;
    },
    setReadyStateChangeToNull: function(requestObj) {
        try { requestObj.onreadystatechange = null; }
        catch (e) { requestObj.onreadystatechange = new Function(); }
    }
})";

        private static readonly ScriptObject HelperScriptObject = (ScriptObject)HtmlPage.Window.Eval(HelperScript);

        internal static ScriptObject ToScriptFunction(Delegate d)
        {
            Debug.Assert(d != null, "d != null");
            return (ScriptObject)HelperScriptObject.Invoke("cd", d);
        }

        internal static void CallOpen(ScriptObject request, string method, string uri)
        {
            Debug.Assert(request != null, "request != null");
            HelperScriptObject.Invoke("callOpen", request, method, uri);
        }

        internal static void SetReadyStateChange(ScriptObject request, ScriptObject callback)
        {
            Debug.Assert(request != null, "request != null");
            if (callback == null)
            {
                HelperScriptObject.Invoke("setReadyStateChangeToNull", request);
            }
            else
            {
                HelperScriptObject.Invoke("setReadyStateChange", request, callback);
            }
        }
    }
}
