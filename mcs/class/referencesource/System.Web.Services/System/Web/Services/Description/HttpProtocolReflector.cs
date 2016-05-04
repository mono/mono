//------------------------------------------------------------------------------
// <copyright file="HttpProtocolReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.Web.Services.Configuration;

    internal abstract class HttpProtocolReflector : ProtocolReflector {
        MimeReflector[] reflectors;

        protected HttpProtocolReflector() {
            Type[] reflectorTypes = WebServicesSection.Current.MimeReflectorTypes;
            reflectors = new MimeReflector[reflectorTypes.Length];
            for (int i = 0; i < reflectors.Length; i++) {
                MimeReflector reflector = (MimeReflector)Activator.CreateInstance(reflectorTypes[i]);
                reflector.ReflectionContext = this;
                reflectors[i] = reflector;
            }
        }

        protected bool ReflectMimeParameters() {
            bool handled = false;
            for (int i = 0; i < reflectors.Length; i++) {
                if (reflectors[i].ReflectParameters())
                    handled = true;
            }
            return handled;
        }

        protected bool ReflectMimeReturn() {
            if (Method.ReturnType == typeof(void)) {
                Message outputMessage = OutputMessage;
                return true;
            }
            bool handled = false;
            for (int i = 0; i < reflectors.Length; i++) {
                if (reflectors[i].ReflectReturn()) {
                    handled = true;
                    break;
                }
            }
            return handled;
        }

        protected bool ReflectUrlParameters() {
            if (!HttpServerProtocol.AreUrlParametersSupported(Method))
                return false;
            ReflectStringParametersMessage();
            OperationBinding.Input.Extensions.Add(new HttpUrlEncodedBinding());
            return true;
        }

        internal void ReflectStringParametersMessage() {
            Message inputMessage = InputMessage;
            foreach (ParameterInfo parameterInfo in Method.InParameters) {
                MessagePart part = new MessagePart();
                part.Name = XmlConvert.EncodeLocalName(parameterInfo.Name);
                if (parameterInfo.ParameterType.IsArray) {
                    string typeNs = DefaultNamespace;
                    if (typeNs.EndsWith("/", StringComparison.Ordinal))
                        typeNs += "AbstractTypes";
                    else
                        typeNs += "/AbstractTypes";
                    string typeName = "StringArray";
                    if (!ServiceDescription.Types.Schemas.Contains(typeNs)) {
                        XmlSchema schema = new XmlSchema();
                        schema.TargetNamespace = typeNs;
                        ServiceDescription.Types.Schemas.Add(schema);
                       
                        XmlSchemaElement element = new XmlSchemaElement();
                        element.Name = "String";
                        element.SchemaTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
                        element.MinOccurs = decimal.Zero;
                        element.MaxOccurs = decimal.MaxValue;
                        XmlSchemaSequence all = new XmlSchemaSequence();
                        all.Items.Add(element);

                        XmlSchemaComplexContentRestriction restriction = new XmlSchemaComplexContentRestriction();
                        restriction.BaseTypeName = new XmlQualifiedName(Soap.ArrayType, Soap.Encoding);
                        restriction.Particle = all;

                        XmlSchemaImport import = new XmlSchemaImport();
                        import.Namespace = restriction.BaseTypeName.Namespace;
                        
                        XmlSchemaComplexContent model = new XmlSchemaComplexContent();
                        model.Content = restriction;

                        XmlSchemaComplexType type = new XmlSchemaComplexType();
                        type.Name = typeName;
                        type.ContentModel = model;

                        schema.Items.Add(type);
                        schema.Includes.Add(import);
                    }
                    part.Type = new XmlQualifiedName(typeName, typeNs);
                }
                else {
                    part.Type = new XmlQualifiedName("string", XmlSchema.Namespace);
                }
                inputMessage.Parts.Add(part);
            }
        }

        internal string MethodUrl {
            get {
                WebMethodAttribute methodAttribute = Method.MethodAttribute;
                string name = methodAttribute.MessageName;
                if (name.Length == 0) name = Method.Name;
                return "/" + name;
            }
        }
    }
}
