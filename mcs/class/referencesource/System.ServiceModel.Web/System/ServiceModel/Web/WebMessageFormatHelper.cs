//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    internal static class WebMessageFormatHelper
    {
        internal static bool IsDefined(WebMessageFormat format)
        {
            return (format == WebMessageFormat.Xml || format == WebMessageFormat.Json);
        }
    }
}
