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

    class JsonQNameDataContract : JsonDataContract
    {
        public JsonQNameDataContract(QNameDataContract traditionalQNameDataContract)
            : base(traditionalQNameDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context == null)
            {
                return TryReadNullAtTopLevel(jsonReader) ? null : jsonReader.ReadElementContentAsQName();
            }
            else
            {
                return HandleReadValue(jsonReader.ReadElementContentAsQName(), context);
            }
        }
    }
}
