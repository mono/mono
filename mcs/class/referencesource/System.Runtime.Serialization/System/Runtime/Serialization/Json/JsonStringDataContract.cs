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

    class JsonStringDataContract : JsonDataContract
    {
        public JsonStringDataContract(StringDataContract traditionalStringDataContract)
            : base(traditionalStringDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context == null)
            {
                return TryReadNullAtTopLevel(jsonReader) ? null : jsonReader.ReadElementContentAsString();
            }
            else
            {
                return HandleReadValue(jsonReader.ReadElementContentAsString(), context);
            }
        }
    }
}
