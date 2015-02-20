//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Xml.Linq;

    sealed class GZipObjectSerializer : DefaultObjectSerializer
    {
        protected override Dictionary<XName, object> DeserializePropertyBag(Stream stream)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                return base.DeserializePropertyBag(gzip);
            }
        }
        protected override object DeserializeValue(Stream stream)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                return base.DeserializeValue(gzip);
            }
        }

        protected override void SerializePropertyBag(Stream stream, Dictionary<XName, object> propertyBag)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionLevel.Fastest, true))
            {
                base.SerializePropertyBag(gzip, propertyBag);
            }
        }

        protected override void SerializeValue(Stream stream, object value)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionLevel.Fastest, true))
            {
                base.SerializeValue(gzip, value);
            }
        }
    }
}
