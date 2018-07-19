//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Xml;

    class JsonUriDataContract : JsonDataContract
    {
        public JsonUriDataContract(UriDataContract traditionalUriDataContract)
            : base(traditionalUriDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context == null)
            {
                return TryReadNullAtTopLevel(jsonReader) ? null : jsonReader.ReadElementContentAsUri();
            }
            else
            {
                return HandleReadValue(jsonReader.ReadElementContentAsUri(), context);
            }
        }
    }
}
