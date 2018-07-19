//------------------------------------------------------------------------------
// <copyright file="WebServiceErrorEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {

    public class WebServiceErrorEvent : WebRequestErrorEvent{
        private const int _webServiceErrorEventCode = 100001;
        public static int WebServiceErrorEventCode{
            get{
                return _webServiceErrorEventCode;
            }
        }
        internal protected WebServiceErrorEvent(string message, object eventSource, Exception exception)
            : base(message, eventSource, WebServiceErrorEventCode, exception)
        {
        }
        
    }
}
