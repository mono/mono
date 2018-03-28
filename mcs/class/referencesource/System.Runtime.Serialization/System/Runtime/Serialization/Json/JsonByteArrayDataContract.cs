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

    class JsonByteArrayDataContract : JsonDataContract
    {
        public JsonByteArrayDataContract(ByteArrayDataContract traditionalByteArrayDataContract)
            : base(traditionalByteArrayDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context == null)
            {
                return TryReadNullAtTopLevel(jsonReader) ? null : jsonReader.ReadElementContentAsBase64();
            }
            else
            {
                return HandleReadValue(jsonReader.ReadElementContentAsBase64(), context);
            }
        }

    }
}
