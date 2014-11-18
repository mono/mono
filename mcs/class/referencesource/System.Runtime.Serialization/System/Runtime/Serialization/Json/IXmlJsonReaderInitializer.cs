//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IXmlJsonReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas,
            OnXmlDictionaryReaderClose onClose);

        void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas,
            OnXmlDictionaryReaderClose onClose);
    }
}
