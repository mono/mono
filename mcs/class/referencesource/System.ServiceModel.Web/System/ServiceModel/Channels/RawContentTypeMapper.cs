//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    class RawContentTypeMapper : WebContentTypeMapper
    {
        static readonly RawContentTypeMapper instance = new RawContentTypeMapper();

        public static RawContentTypeMapper Instance
        {
            get
            {
                return instance;
            }
        }

        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Raw;
        }
    }
}
