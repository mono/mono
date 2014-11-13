
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    internal static class WebContentFormatHelper
    {
        internal static bool IsDefined(WebContentFormat format)
        {
            return (format == WebContentFormat.Default
                || format == WebContentFormat.Xml
                || format == WebContentFormat.Json
                || format == WebContentFormat.Raw);
        }
    }

}
