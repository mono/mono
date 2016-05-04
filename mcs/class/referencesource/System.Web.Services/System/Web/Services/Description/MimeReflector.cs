//------------------------------------------------------------------------------
// <copyright file="MimeReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;

    internal abstract class MimeReflector {
        HttpProtocolReflector protocol;

        internal abstract bool ReflectParameters();
        internal abstract bool ReflectReturn();

        internal HttpProtocolReflector ReflectionContext {
            get { return protocol; }
            set { protocol = value; }
        }
    }
}
