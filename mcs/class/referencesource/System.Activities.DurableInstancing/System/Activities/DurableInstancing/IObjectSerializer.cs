//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    interface IObjectSerializer
    {
        object DeserializeValue(byte[] bytes);
        Dictionary<XName, object> DeserializePropertyBag(byte[] bytes);
        ArraySegment<byte> SerializeValue(object value);
        ArraySegment<byte> SerializePropertyBag(Dictionary<XName, object> value);
    }
}
