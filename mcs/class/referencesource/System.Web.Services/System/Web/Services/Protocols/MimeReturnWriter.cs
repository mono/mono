//------------------------------------------------------------------------------
// <copyright file="MimeReturnWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;

    internal abstract class MimeReturnWriter : MimeFormatter {
        internal abstract void Write(HttpResponse response, Stream outputStream, object returnValue);
    }

}
