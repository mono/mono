//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;

    class HttpsHostedTransportConfiguration : HttpHostedTransportConfiguration
    {
        internal HttpsHostedTransportConfiguration()
            : base(Uri.UriSchemeHttps)
        { }
    }
}
