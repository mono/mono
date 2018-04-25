//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Net;

    public sealed class JavascriptCallbackResponseMessageProperty
    {
        static readonly string JavascriptCallbackResponseMessagePropertyName = "javascriptCallbackResponse";

        public JavascriptCallbackResponseMessageProperty()
        {
        }

        public static string Name 
        {
            get { return JavascriptCallbackResponseMessagePropertyName; }
        }

        public string CallbackFunctionName { get; set; }

        public HttpStatusCode? StatusCode { get; set; }
    }
}
