//------------------------------------------------------------------------------
// <copyright file="XmlReturnWriter.cs" company="Microsoft">
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
    using System.Text;
    using System.Web.Services.Diagnostics;

    internal class XmlReturnWriter : MimeReturnWriter {
        XmlSerializer xmlSerializer;

        public override void Initialize(object o) {
            xmlSerializer = (XmlSerializer)o;
        }

        public override object[] GetInitializers(LogicalMethodInfo[] methodInfos) {
            return XmlReturn.GetInitializers(methodInfos);
        }

        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            return XmlReturn.GetInitializer(methodInfo);
        }

        internal override void Write(HttpResponse response, Stream outputStream, object returnValue) {
            Encoding encoding = new UTF8Encoding(false);
            response.ContentType = ContentType.Compose("text/xml", encoding);
            StreamWriter writer = new StreamWriter(outputStream, encoding);

            TraceMethod caller = Tracing.On ? new TraceMethod(this, "Write") : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceWriteResponse), caller, new TraceMethod(xmlSerializer, "Serialize", writer, returnValue));
            xmlSerializer.Serialize(writer, returnValue);
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceWriteResponse), caller);
        }
    }

}
