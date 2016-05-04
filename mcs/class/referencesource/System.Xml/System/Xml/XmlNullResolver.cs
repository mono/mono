//------------------------------------------------------------------------------
// <copyright file="XmlNullResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

#if !SILVERLIGHT
using System.Net;
#endif

namespace System.Xml {
    internal class XmlNullResolver : XmlResolver {
        public static readonly XmlNullResolver Singleton = new XmlNullResolver();

        // Private constructor ensures existing only one instance of XmlNullResolver
        private XmlNullResolver() { }

        public override Object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            throw new XmlException(Res.Xml_NullResolver, string.Empty);
        }

#if !SILVERLIGHT
        public override ICredentials Credentials {
            set { /* Do nothing */ }
        }
#endif
    }
}
