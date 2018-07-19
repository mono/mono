//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;

    /* public */
    class BinaryVersion
    {
        static public readonly BinaryVersion Version1 = new BinaryVersion(FramingEncodingString.Binary, FramingEncodingString.BinarySession, ServiceModelDictionary.Version1);
        static public readonly BinaryVersion GZipVersion1 = new BinaryVersion(FramingEncodingString.ExtendedBinaryGZip, FramingEncodingString.ExtendedBinarySessionGZip, ServiceModelDictionary.Version1);
        static public readonly BinaryVersion DeflateVersion1 = new BinaryVersion(FramingEncodingString.ExtendedBinaryDeflate, FramingEncodingString.ExtendedBinarySessionDeflate, ServiceModelDictionary.Version1);

        string contentType;
        string sessionContentType;
        IXmlDictionary dictionary;

        BinaryVersion(string contentType, string sessionContentType, IXmlDictionary dictionary)
        {
            this.contentType = contentType;
            this.sessionContentType = sessionContentType;
            this.dictionary = dictionary;
        }

        static public BinaryVersion CurrentVersion { get { return Version1; } }
        public string ContentType { get { return contentType; } }
        public string SessionContentType { get { return sessionContentType; } }
        public IXmlDictionary Dictionary { get { return dictionary; } }
    }
}
