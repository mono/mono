//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    internal static class WebMessageBodyStyleHelper
    {
        internal static bool IsDefined(WebMessageBodyStyle style)
        {
            return (style == WebMessageBodyStyle.Bare
                || style == WebMessageBodyStyle.Wrapped
                || style == WebMessageBodyStyle.WrappedRequest
                || style == WebMessageBodyStyle.WrappedResponse);
        }
    }
}
