//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;

    static class RedirectionConstants
    {
        public const string AddressElementName = "Address";
        public const string LocationElementName = "Location";
        public const string Namespace = "http://schemas.microsoft.com/ws/2008/06/redirect";
        public const string Prefix = "r";
        public const string RedirectionElementName = "Redirection";

        internal static class Duration
        {
            public const string Permanent = "Permanent";
            public const string Temporary = "Temporary";
            public const string XmlName = "duration";
        }

        internal static class Scope
        {
            public const string Endpoint = "Endpoint";
            public const string Message = "Message";
            public const string Session = "Session";
            public const string XmlName = "scope";
        }

        internal static class Type
        {
            public const string Cache = "Cache";
            public const string Resource = "Resource";
            public const string UseIntermediary = "UseIntermediary";
            public const string XmlName = "type";

        }
    }
}
