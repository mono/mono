//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class WebContentTypeMapper
    {
        public abstract WebContentFormat GetMessageFormatForContentType(string contentType);
    }
}
