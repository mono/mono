//-----------------------------------------------------------------------
// <copyright file="XmlEncryptionConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Constants for XML Encryption.
    /// Definitions for namespace, attributes and elements as defined in http://www.w3.org/TR/2002/REC-xmlenc-core-2002120
    /// Only constants that are absent in S.IM
    /// </summary>
    internal static class XmlEncryptionConstants
    {
#pragma warning disable 1591
        public const string Namespace = "http://www.w3.org/2001/04/xmlenc#";
        public const string Prefix    = "xenc";

        public static class Attributes
        {
            public const string Algorithm = "Algorithm";
            public const string Encoding  = "Encoding";
            public const string Id        = "Id";
            public const string MimeType  = "MimeType";
            public const string Recipient = "Recipient";
            public const string Type      = "Type";
            public const string Uri       = "URI";
        }

        public static class Elements
        {
            public const string CarriedKeyName       = "CarriedKeyName";
            public const string CipherData           = "CipherData";
            public const string CipherReference      = "CiperReference";
            public const string CipherValue          = "CipherValue";
            public const string DataReference        = "DataReference";
            public const string EncryptedData        = "EncryptedData";
            public const string EncryptedKey         = "EncryptedKey";
            public const string EncryptionMethod     = "EncryptionMethod";
            public const string EncryptionProperties = "EncryptionProperties";
            public const string KeyReference         = "KeyReference";
            public const string KeySize              = "KeySize";
            public const string OaepParams           = "OAEPparams";
            public const string Recipient            = "Recipient";
            public const string ReferenceList        = "ReferenceList";
        }

        public static class EncryptedDataTypes
        {
            public const string Element         = Namespace + "Element";
            public const string Content         = Namespace + "Content";
#pragma warning restore 1591
        }
    }
}
