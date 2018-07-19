//------------------------------------------------------------------------------
// <copyright file="MimeXmlReflector.cs" company="Microsoft">
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
    using System.Xml;

    internal class MimeXmlReflector : MimeReflector {
        internal override bool ReflectParameters() {
            return false;
        }

        internal override bool ReflectReturn() {
            MessagePart part = new MessagePart();
            part.Name = "Body";
            ReflectionContext.OutputMessage.Parts.Add(part);

            if (typeof(XmlNode).IsAssignableFrom(ReflectionContext.Method.ReturnType)) {
                MimeContentBinding mimeContentBinding = new MimeContentBinding();
                mimeContentBinding.Type = "text/xml";
                mimeContentBinding.Part = part.Name;
                ReflectionContext.OperationBinding.Output.Extensions.Add(mimeContentBinding);
            }
            else {
                MimeXmlBinding mimeXmlBinding = new MimeXmlBinding();
                mimeXmlBinding.Part = part.Name;

                LogicalMethodInfo methodInfo = ReflectionContext.Method;
                XmlAttributes a = new XmlAttributes(methodInfo.ReturnTypeCustomAttributeProvider);
                XmlTypeMapping xmlTypeMapping = ReflectionContext.ReflectionImporter.ImportTypeMapping(methodInfo.ReturnType, a.XmlRoot);
                xmlTypeMapping.SetKey(methodInfo.GetKey() + ":Return");
                ReflectionContext.SchemaExporter.ExportTypeMapping(xmlTypeMapping);
                part.Element = new XmlQualifiedName(xmlTypeMapping.XsdElementName, xmlTypeMapping.Namespace);
                ReflectionContext.OperationBinding.Output.Extensions.Add(mimeXmlBinding);
            }

            return true;
        }
    }
}
