namespace System.Web.Services.Description {
internal class ServiceDescriptionSerializationWriter : System.Xml.Serialization.XmlSerializationWriter {
        

        public void Write125_definitions(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"definitions", @"http://schemas.xmlsoap.org/wsdl/");
                return;
            }
            TopLevelElement();
            Write124_ServiceDescription(@"definitions", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.ServiceDescription)o), true, false);
        }

        void Write124_ServiceDescription(string n, string ns, global::System.Web.Services.Description.ServiceDescription o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.ServiceDescription)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"ServiceDescription", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"targetNamespace", @"", ((global::System.String)o.@TargetNamespace));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.ImportCollection a = (global::System.Web.Services.Description.ImportCollection)o.@Imports;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write4_Import(@"import", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Import)a[ia]), false, false);
                    }
                }
            }
            Write67_Types(@"types", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Types)o.@Types), false, false);
            {
                global::System.Web.Services.Description.MessageCollection a = (global::System.Web.Services.Description.MessageCollection)o.@Messages;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write69_Message(@"message", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Message)a[ia]), false, false);
                    }
                }
            }
            {
                global::System.Web.Services.Description.PortTypeCollection a = (global::System.Web.Services.Description.PortTypeCollection)o.@PortTypes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write75_PortType(@"portType", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.PortType)a[ia]), false, false);
                    }
                }
            }
            {
                global::System.Web.Services.Description.BindingCollection a = (global::System.Web.Services.Description.BindingCollection)o.@Bindings;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write117_Binding(@"binding", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Binding)a[ia]), false, false);
                    }
                }
            }
            {
                global::System.Web.Services.Description.ServiceCollection a = (global::System.Web.Services.Description.ServiceCollection)o.@Services;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write123_Service(@"service", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Service)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write123_Service(string n, string ns, global::System.Web.Services.Description.Service o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Service)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Service", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.PortCollection a = (global::System.Web.Services.Description.PortCollection)o.@Ports;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write122_Port(@"port", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Port)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write122_Port(string n, string ns, global::System.Web.Services.Description.Port o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Port)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Port", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"binding", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Binding)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12AddressBinding) {
                                Write121_Soap12AddressBinding(@"address", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12AddressBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.HttpAddressBinding) {
                                Write118_HttpAddressBinding(@"address", @"http://schemas.xmlsoap.org/wsdl/http/", ((global::System.Web.Services.Description.HttpAddressBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapAddressBinding) {
                                Write119_SoapAddressBinding(@"address", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapAddressBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write119_SoapAddressBinding(string n, string ns, global::System.Web.Services.Description.SoapAddressBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapAddressBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapAddressBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"location", @"", ((global::System.String)o.@Location));
            WriteEndElement(o);
        }

        void Write118_HttpAddressBinding(string n, string ns, global::System.Web.Services.Description.HttpAddressBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.HttpAddressBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"HttpAddressBinding", @"http://schemas.xmlsoap.org/wsdl/http/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"location", @"", ((global::System.String)o.@Location));
            WriteEndElement(o);
        }

        void Write121_Soap12AddressBinding(string n, string ns, global::System.Web.Services.Description.Soap12AddressBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12AddressBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12AddressBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"location", @"", ((global::System.String)o.@Location));
            WriteEndElement(o);
        }

        void Write117_Binding(string n, string ns, global::System.Web.Services.Description.Binding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Binding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Binding", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"type", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Type)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12Binding) {
                                Write84_Soap12Binding(@"binding", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12Binding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.HttpBinding) {
                                Write77_HttpBinding(@"binding", @"http://schemas.xmlsoap.org/wsdl/http/", ((global::System.Web.Services.Description.HttpBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapBinding) {
                                Write80_SoapBinding(@"binding", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.OperationBindingCollection a = (global::System.Web.Services.Description.OperationBindingCollection)o.@Operations;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write116_OperationBinding(@"operation", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.OperationBinding)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write116_OperationBinding(string n, string ns, global::System.Web.Services.Description.OperationBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.OperationBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"OperationBinding", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12OperationBinding) {
                                Write88_Soap12OperationBinding(@"operation", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12OperationBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.HttpOperationBinding) {
                                Write85_HttpOperationBinding(@"operation", @"http://schemas.xmlsoap.org/wsdl/http/", ((global::System.Web.Services.Description.HttpOperationBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapOperationBinding) {
                                Write86_SoapOperationBinding(@"operation", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapOperationBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write110_InputBinding(@"input", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.InputBinding)o.@Input), false, false);
            Write111_OutputBinding(@"output", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.OutputBinding)o.@Output), false, false);
            {
                global::System.Web.Services.Description.FaultBindingCollection a = (global::System.Web.Services.Description.FaultBindingCollection)o.@Faults;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write115_FaultBinding(@"fault", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.FaultBinding)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write115_FaultBinding(string n, string ns, global::System.Web.Services.Description.FaultBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.FaultBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"FaultBinding", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12FaultBinding) {
                                Write114_Soap12FaultBinding(@"fault", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12FaultBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapFaultBinding) {
                                Write112_SoapFaultBinding(@"fault", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapFaultBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write112_SoapFaultBinding(string n, string ns, global::System.Web.Services.Description.SoapFaultBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapFaultBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapFaultBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write98_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            WriteEndElement(o);
        }

        string Write98_SoapBindingUse(global::System.Web.Services.Description.SoapBindingUse v) {
            string s = null;
            switch (v) {
                case global::System.Web.Services.Description.SoapBindingUse.@Encoded: s = @"encoded"; break;
                case global::System.Web.Services.Description.SoapBindingUse.@Literal: s = @"literal"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Services.Description.SoapBindingUse");
            }
            return s;
        }

        void Write114_Soap12FaultBinding(string n, string ns, global::System.Web.Services.Description.Soap12FaultBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12FaultBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12FaultBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write100_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            WriteEndElement(o);
        }

        string Write100_SoapBindingUse(global::System.Web.Services.Description.SoapBindingUse v) {
            string s = null;
            switch (v) {
                case global::System.Web.Services.Description.SoapBindingUse.@Encoded: s = @"encoded"; break;
                case global::System.Web.Services.Description.SoapBindingUse.@Literal: s = @"literal"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Services.Description.SoapBindingUse");
            }
            return s;
        }

        void Write111_OutputBinding(string n, string ns, global::System.Web.Services.Description.OutputBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.OutputBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"OutputBinding", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12BodyBinding) {
                                Write102_Soap12BodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12BodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.Soap12HeaderBinding) {
                                Write109_Soap12HeaderBinding(@"header", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12HeaderBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapHeaderBinding) {
                                Write106_SoapHeaderBinding(@"header", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapHeaderBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapBodyBinding) {
                                Write99_SoapBodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapBodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeXmlBinding) {
                                Write94_MimeXmlBinding(@"mimeXml", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeXmlBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeContentBinding) {
                                Write93_MimeContentBinding(@"content", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeContentBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeTextBinding) {
                                Write97_MimeTextBinding(@"text", @"http://microsoft.com/wsdl/mime/textMatching/", ((global::System.Web.Services.Description.MimeTextBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeMultipartRelatedBinding) {
                                Write104_MimeMultipartRelatedBinding(@"multipartRelated", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeMultipartRelatedBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write104_MimeMultipartRelatedBinding(string n, string ns, global::System.Web.Services.Description.MimeMultipartRelatedBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimeMultipartRelatedBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimeMultipartRelatedBinding", @"http://schemas.xmlsoap.org/wsdl/mime/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            {
                global::System.Web.Services.Description.MimePartCollection a = (global::System.Web.Services.Description.MimePartCollection)o.@Parts;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write103_MimePart(@"part", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimePart)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write103_MimePart(string n, string ns, global::System.Web.Services.Description.MimePart o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimePart)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimePart", @"http://schemas.xmlsoap.org/wsdl/mime/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12BodyBinding) {
                                Write102_Soap12BodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12BodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapBodyBinding) {
                                Write99_SoapBodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapBodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeContentBinding) {
                                Write93_MimeContentBinding(@"content", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeContentBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeXmlBinding) {
                                Write94_MimeXmlBinding(@"mimeXml", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeXmlBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeTextBinding) {
                                Write97_MimeTextBinding(@"text", @"http://microsoft.com/wsdl/mime/textMatching/", ((global::System.Web.Services.Description.MimeTextBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write97_MimeTextBinding(string n, string ns, global::System.Web.Services.Description.MimeTextBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimeTextBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimeTextBinding", @"http://microsoft.com/wsdl/mime/textMatching/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            {
                global::System.Web.Services.Description.MimeTextMatchCollection a = (global::System.Web.Services.Description.MimeTextMatchCollection)o.@Matches;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write96_MimeTextMatch(@"match", @"http://microsoft.com/wsdl/mime/textMatching/", ((global::System.Web.Services.Description.MimeTextMatch)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write96_MimeTextMatch(string n, string ns, global::System.Web.Services.Description.MimeTextMatch o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimeTextMatch)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimeTextMatch", @"http://microsoft.com/wsdl/mime/textMatching/");
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"type", @"", ((global::System.String)o.@Type));
            if (((global::System.Int32)o.@Group) != 1) {
                WriteAttribute(@"group", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@Group)));
            }
            if (((global::System.Int32)o.@Capture) != 0) {
                WriteAttribute(@"capture", @"", System.Xml.XmlConvert.ToString((global::System.Int32)((global::System.Int32)o.@Capture)));
            }
            if (((global::System.String)o.@RepeatsString) != @"1") {
                WriteAttribute(@"repeats", @"", ((global::System.String)o.@RepeatsString));
            }
            WriteAttribute(@"pattern", @"", ((global::System.String)o.@Pattern));
            WriteAttribute(@"ignoreCase", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IgnoreCase)));
            {
                global::System.Web.Services.Description.MimeTextMatchCollection a = (global::System.Web.Services.Description.MimeTextMatchCollection)o.@Matches;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write96_MimeTextMatch(@"match", @"http://microsoft.com/wsdl/mime/textMatching/", ((global::System.Web.Services.Description.MimeTextMatch)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write94_MimeXmlBinding(string n, string ns, global::System.Web.Services.Description.MimeXmlBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimeXmlBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimeXmlBinding", @"http://schemas.xmlsoap.org/wsdl/mime/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            WriteEndElement(o);
        }

        void Write93_MimeContentBinding(string n, string ns, global::System.Web.Services.Description.MimeContentBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MimeContentBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"MimeContentBinding", @"http://schemas.xmlsoap.org/wsdl/mime/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            WriteAttribute(@"type", @"", ((global::System.String)o.@Type));
            WriteEndElement(o);
        }

        void Write99_SoapBodyBinding(string n, string ns, global::System.Web.Services.Description.SoapBodyBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapBodyBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapBodyBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write98_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            WriteAttribute(@"parts", @"", ((global::System.String)o.@PartsString));
            WriteEndElement(o);
        }

        void Write102_Soap12BodyBinding(string n, string ns, global::System.Web.Services.Description.Soap12BodyBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12BodyBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12BodyBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write100_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            WriteAttribute(@"parts", @"", ((global::System.String)o.@PartsString));
            WriteEndElement(o);
        }

        void Write106_SoapHeaderBinding(string n, string ns, global::System.Web.Services.Description.SoapHeaderBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapHeaderBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapHeaderBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write98_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            Write105_SoapHeaderFaultBinding(@"headerfault", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapHeaderFaultBinding)o.@Fault), false, false);
            WriteEndElement(o);
        }

        void Write105_SoapHeaderFaultBinding(string n, string ns, global::System.Web.Services.Description.SoapHeaderFaultBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapHeaderFaultBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapHeaderFaultBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write98_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            WriteEndElement(o);
        }

        void Write109_Soap12HeaderBinding(string n, string ns, global::System.Web.Services.Description.Soap12HeaderBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12HeaderBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12HeaderBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write100_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            Write107_SoapHeaderFaultBinding(@"headerfault", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.SoapHeaderFaultBinding)o.@Fault), false, false);
            WriteEndElement(o);
        }

        void Write107_SoapHeaderFaultBinding(string n, string ns, global::System.Web.Services.Description.SoapHeaderFaultBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapHeaderFaultBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapHeaderFaultBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            WriteAttribute(@"part", @"", ((global::System.String)o.@Part));
            if (((global::System.Web.Services.Description.SoapBindingUse)o.@Use) != global::System.Web.Services.Description.SoapBindingUse.@Default) {
                WriteAttribute(@"use", @"", Write100_SoapBindingUse(((global::System.Web.Services.Description.SoapBindingUse)o.@Use)));
            }
            if ((((global::System.String)o.@Encoding) != null) && (((global::System.String)o.@Encoding).Length != 0)) {
                WriteAttribute(@"encodingStyle", @"", ((global::System.String)o.@Encoding));
            }
            if ((((global::System.String)o.@Namespace) != null) && (((global::System.String)o.@Namespace).Length != 0)) {
                WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            }
            WriteEndElement(o);
        }

        void Write110_InputBinding(string n, string ns, global::System.Web.Services.Description.InputBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.InputBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"InputBinding", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Object ai = (global::System.Object)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.Soap12BodyBinding) {
                                Write102_Soap12BodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12BodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.Soap12HeaderBinding) {
                                Write109_Soap12HeaderBinding(@"header", @"http://schemas.xmlsoap.org/wsdl/soap12/", ((global::System.Web.Services.Description.Soap12HeaderBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapBodyBinding) {
                                Write99_SoapBodyBinding(@"body", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapBodyBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.SoapHeaderBinding) {
                                Write106_SoapHeaderBinding(@"header", @"http://schemas.xmlsoap.org/wsdl/soap/", ((global::System.Web.Services.Description.SoapHeaderBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeTextBinding) {
                                Write97_MimeTextBinding(@"text", @"http://microsoft.com/wsdl/mime/textMatching/", ((global::System.Web.Services.Description.MimeTextBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.HttpUrlReplacementBinding) {
                                Write91_HttpUrlReplacementBinding(@"urlReplacement", @"http://schemas.xmlsoap.org/wsdl/http/", ((global::System.Web.Services.Description.HttpUrlReplacementBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.HttpUrlEncodedBinding) {
                                Write90_HttpUrlEncodedBinding(@"urlEncoded", @"http://schemas.xmlsoap.org/wsdl/http/", ((global::System.Web.Services.Description.HttpUrlEncodedBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeContentBinding) {
                                Write93_MimeContentBinding(@"content", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeContentBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeMultipartRelatedBinding) {
                                Write104_MimeMultipartRelatedBinding(@"multipartRelated", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeMultipartRelatedBinding)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.MimeXmlBinding) {
                                Write94_MimeXmlBinding(@"mimeXml", @"http://schemas.xmlsoap.org/wsdl/mime/", ((global::System.Web.Services.Description.MimeXmlBinding)ai), false, false);
                            }
                            else if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write90_HttpUrlEncodedBinding(string n, string ns, global::System.Web.Services.Description.HttpUrlEncodedBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.HttpUrlEncodedBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"HttpUrlEncodedBinding", @"http://schemas.xmlsoap.org/wsdl/http/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteEndElement(o);
        }

        void Write91_HttpUrlReplacementBinding(string n, string ns, global::System.Web.Services.Description.HttpUrlReplacementBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.HttpUrlReplacementBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"HttpUrlReplacementBinding", @"http://schemas.xmlsoap.org/wsdl/http/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteEndElement(o);
        }

        void Write86_SoapOperationBinding(string n, string ns, global::System.Web.Services.Description.SoapOperationBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapOperationBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapOperationBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"soapAction", @"", ((global::System.String)o.@SoapAction));
            if (((global::System.Web.Services.Description.SoapBindingStyle)o.@Style) != global::System.Web.Services.Description.SoapBindingStyle.@Default) {
                WriteAttribute(@"style", @"", Write79_SoapBindingStyle(((global::System.Web.Services.Description.SoapBindingStyle)o.@Style)));
            }
            WriteEndElement(o);
        }

        string Write79_SoapBindingStyle(global::System.Web.Services.Description.SoapBindingStyle v) {
            string s = null;
            switch (v) {
                case global::System.Web.Services.Description.SoapBindingStyle.@Document: s = @"document"; break;
                case global::System.Web.Services.Description.SoapBindingStyle.@Rpc: s = @"rpc"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Services.Description.SoapBindingStyle");
            }
            return s;
        }

        void Write85_HttpOperationBinding(string n, string ns, global::System.Web.Services.Description.HttpOperationBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.HttpOperationBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"HttpOperationBinding", @"http://schemas.xmlsoap.org/wsdl/http/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"location", @"", ((global::System.String)o.@Location));
            WriteEndElement(o);
        }

        void Write88_Soap12OperationBinding(string n, string ns, global::System.Web.Services.Description.Soap12OperationBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12OperationBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12OperationBinding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"soapAction", @"", ((global::System.String)o.@SoapAction));
            if (((global::System.Web.Services.Description.SoapBindingStyle)o.@Style) != global::System.Web.Services.Description.SoapBindingStyle.@Default) {
                WriteAttribute(@"style", @"", Write82_SoapBindingStyle(((global::System.Web.Services.Description.SoapBindingStyle)o.@Style)));
            }
            if (((global::System.Boolean)o.@SoapActionRequired) != false) {
                WriteAttribute(@"soapActionRequired", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@SoapActionRequired)));
            }
            WriteEndElement(o);
        }

        string Write82_SoapBindingStyle(global::System.Web.Services.Description.SoapBindingStyle v) {
            string s = null;
            switch (v) {
                case global::System.Web.Services.Description.SoapBindingStyle.@Document: s = @"document"; break;
                case global::System.Web.Services.Description.SoapBindingStyle.@Rpc: s = @"rpc"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Services.Description.SoapBindingStyle");
            }
            return s;
        }

        void Write80_SoapBinding(string n, string ns, global::System.Web.Services.Description.SoapBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.SoapBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"SoapBinding", @"http://schemas.xmlsoap.org/wsdl/soap/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"transport", @"", ((global::System.String)o.@Transport));
            if (((global::System.Web.Services.Description.SoapBindingStyle)o.@Style) != global::System.Web.Services.Description.SoapBindingStyle.@Document) {
                WriteAttribute(@"style", @"", Write79_SoapBindingStyle(((global::System.Web.Services.Description.SoapBindingStyle)o.@Style)));
            }
            WriteEndElement(o);
        }

        void Write77_HttpBinding(string n, string ns, global::System.Web.Services.Description.HttpBinding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.HttpBinding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"HttpBinding", @"http://schemas.xmlsoap.org/wsdl/http/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"verb", @"", ((global::System.String)o.@Verb));
            WriteEndElement(o);
        }

        void Write84_Soap12Binding(string n, string ns, global::System.Web.Services.Description.Soap12Binding o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Soap12Binding)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Soap12Binding", @"http://schemas.xmlsoap.org/wsdl/soap12/");
            if (((global::System.Boolean)o.@Required) != false) {
                WriteAttribute(@"required", @"http://schemas.xmlsoap.org/wsdl/", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@Required)));
            }
            WriteAttribute(@"transport", @"", ((global::System.String)o.@Transport));
            if (((global::System.Web.Services.Description.SoapBindingStyle)o.@Style) != global::System.Web.Services.Description.SoapBindingStyle.@Document) {
                WriteAttribute(@"style", @"", Write82_SoapBindingStyle(((global::System.Web.Services.Description.SoapBindingStyle)o.@Style)));
            }
            WriteEndElement(o);
        }

        void Write75_PortType(string n, string ns, global::System.Web.Services.Description.PortType o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.PortType)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"PortType", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.OperationCollection a = (global::System.Web.Services.Description.OperationCollection)o.@Operations;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write74_Operation(@"operation", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.Operation)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write74_Operation(string n, string ns, global::System.Web.Services.Description.Operation o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Operation)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Operation", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((((global::System.String)o.@ParameterOrderString) != null) && (((global::System.String)o.@ParameterOrderString).Length != 0)) {
                WriteAttribute(@"parameterOrder", @"", ((global::System.String)o.@ParameterOrderString));
            }
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.OperationMessageCollection a = (global::System.Web.Services.Description.OperationMessageCollection)o.@Messages;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Web.Services.Description.OperationMessage ai = (global::System.Web.Services.Description.OperationMessage)a[ia];
                        {
                            if (ai is global::System.Web.Services.Description.OperationOutput) {
                                Write72_OperationOutput(@"output", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.OperationOutput)ai), false, false);
                            }
                            else if (ai is global::System.Web.Services.Description.OperationInput) {
                                Write71_OperationInput(@"input", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.OperationInput)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.OperationFaultCollection a = (global::System.Web.Services.Description.OperationFaultCollection)o.@Faults;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write73_OperationFault(@"fault", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.OperationFault)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write73_OperationFault(string n, string ns, global::System.Web.Services.Description.OperationFault o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.OperationFault)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"OperationFault", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write71_OperationInput(string n, string ns, global::System.Web.Services.Description.OperationInput o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.OperationInput)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"OperationInput", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write72_OperationOutput(string n, string ns, global::System.Web.Services.Description.OperationOutput o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.OperationOutput)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"OperationOutput", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"message", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Message)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write69_Message(string n, string ns, global::System.Web.Services.Description.Message o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Message)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Message", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Web.Services.Description.MessagePartCollection a = (global::System.Web.Services.Description.MessagePartCollection)o.@Parts;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write68_MessagePart(@"part", @"http://schemas.xmlsoap.org/wsdl/", ((global::System.Web.Services.Description.MessagePart)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write68_MessagePart(string n, string ns, global::System.Web.Services.Description.MessagePart o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.MessagePart)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"MessagePart", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"element", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Element)));
            WriteAttribute(@"type", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Type)));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write67_Types(string n, string ns, global::System.Web.Services.Description.Types o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Types)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Types", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            {
                global::System.Xml.Serialization.XmlSchemas a = (global::System.Xml.Serialization.XmlSchemas)o.@Schemas;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write66_XmlSchema(@"schema", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchema)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write66_XmlSchema(string n, string ns, global::System.Xml.Schema.XmlSchema o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchema)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchema", @"http://www.w3.org/2001/XMLSchema");
            if (((global::System.Xml.Schema.XmlSchemaForm)o.@AttributeFormDefault) != global::System.Xml.Schema.XmlSchemaForm.@None) {
                WriteAttribute(@"attributeFormDefault", @"", Write6_XmlSchemaForm(((global::System.Xml.Schema.XmlSchemaForm)o.@AttributeFormDefault)));
            }
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@BlockDefault) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"blockDefault", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@BlockDefault)));
            }
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@FinalDefault) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"finalDefault", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@FinalDefault)));
            }
            if (((global::System.Xml.Schema.XmlSchemaForm)o.@ElementFormDefault) != global::System.Xml.Schema.XmlSchemaForm.@None) {
                WriteAttribute(@"elementFormDefault", @"", Write6_XmlSchemaForm(((global::System.Xml.Schema.XmlSchemaForm)o.@ElementFormDefault)));
            }
            WriteAttribute(@"targetNamespace", @"", ((global::System.String)o.@TargetNamespace));
            WriteAttribute(@"version", @"", ((global::System.String)o.@Version));
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Includes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaRedefine) {
                                Write64_XmlSchemaRedefine(@"redefine", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaRedefine)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaImport) {
                                Write13_XmlSchemaImport(@"import", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaImport)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaInclude) {
                                Write12_XmlSchemaInclude(@"include", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaInclude)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaElement) {
                                Write52_XmlSchemaElement(@"element", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaElement)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaComplexType) {
                                Write62_XmlSchemaComplexType(@"complexType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexType)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaSimpleType) {
                                Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroup) {
                                Write40_XmlSchemaAttributeGroup(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroup)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaNotation) {
                                Write65_XmlSchemaNotation(@"notation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaNotation)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaGroup) {
                                Write63_XmlSchemaGroup(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroup)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAnnotation) {
                                Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write11_XmlSchemaAnnotation(string n, string ns, global::System.Xml.Schema.XmlSchemaAnnotation o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAnnotation)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAnnotation", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAppInfo) {
                                Write10_XmlSchemaAppInfo(@"appinfo", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAppInfo)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaDocumentation) {
                                Write9_XmlSchemaDocumentation(@"documentation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaDocumentation)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write9_XmlSchemaDocumentation(string n, string ns, global::System.Xml.Schema.XmlSchemaDocumentation o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaDocumentation)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaDocumentation", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"source", @"", ((global::System.String)o.@Source));
            WriteAttribute(@"lang", @"http://www.w3.org/XML/1998/namespace", ((global::System.String)o.@Language));
            {
                global::System.Xml.XmlNode[] a = (global::System.Xml.XmlNode[])o.@Markup;
                if (a != null) {
                    for (int ia = 0; ia < a.Length; ia++) {
                        global::System.Xml.XmlNode ai = (global::System.Xml.XmlNode)a[ia];
                        {
                            if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else if (ai is global::System.Xml.XmlNode) {
                                ((global::System.Xml.XmlNode)ai).WriteTo(Writer);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write10_XmlSchemaAppInfo(string n, string ns, global::System.Xml.Schema.XmlSchemaAppInfo o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAppInfo)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAppInfo", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"source", @"", ((global::System.String)o.@Source));
            {
                global::System.Xml.XmlNode[] a = (global::System.Xml.XmlNode[])o.@Markup;
                if (a != null) {
                    for (int ia = 0; ia < a.Length; ia++) {
                        global::System.Xml.XmlNode ai = (global::System.Xml.XmlNode)a[ia];
                        {
                            if (ai is System.Xml.XmlElement) {
                                System.Xml.XmlElement elem = (System.Xml.XmlElement)ai;
                                if ((elem) is System.Xml.XmlNode || elem == null) {
                                    WriteElementLiteral((System.Xml.XmlNode)elem, @"", null, false, true);
                                }
                                else {
                                    throw CreateInvalidAnyTypeException(elem);
                                }
                            }
                            else if (ai is global::System.Xml.XmlNode) {
                                ((global::System.Xml.XmlNode)ai).WriteTo(Writer);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write63_XmlSchemaGroup(string n, string ns, global::System.Xml.Schema.XmlSchemaGroup o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaGroup)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaGroup", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Particle is global::System.Xml.Schema.XmlSchemaAll) {
                    Write55_XmlSchemaAll(@"all", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAll)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaChoice) {
                    Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaSequence) {
                    Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)o.@Particle), false, false);
                }
                else {
                    if (o.@Particle != null) {
                        throw CreateUnknownTypeException(o.@Particle);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write53_XmlSchemaSequence(string n, string ns, global::System.Xml.Schema.XmlSchemaSequence o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSequence)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSequence", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaChoice) {
                                Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaSequence) {
                                Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaGroupRef) {
                                Write44_XmlSchemaGroupRef(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroupRef)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaElement) {
                                Write52_XmlSchemaElement(@"element", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaElement)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAny) {
                                Write46_XmlSchemaAny(@"any", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAny)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write46_XmlSchemaAny(string n, string ns, global::System.Xml.Schema.XmlSchemaAny o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAny)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAny", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            if (((global::System.Xml.Schema.XmlSchemaContentProcessing)o.@ProcessContents) != global::System.Xml.Schema.XmlSchemaContentProcessing.@None) {
                WriteAttribute(@"processContents", @"", Write38_XmlSchemaContentProcessing(((global::System.Xml.Schema.XmlSchemaContentProcessing)o.@ProcessContents)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        string Write38_XmlSchemaContentProcessing(global::System.Xml.Schema.XmlSchemaContentProcessing v) {
            string s = null;
            switch (v) {
                case global::System.Xml.Schema.XmlSchemaContentProcessing.@Skip: s = @"skip"; break;
                case global::System.Xml.Schema.XmlSchemaContentProcessing.@Lax: s = @"lax"; break;
                case global::System.Xml.Schema.XmlSchemaContentProcessing.@Strict: s = @"strict"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Xml.Schema.XmlSchemaContentProcessing");
            }
            return s;
        }

        void Write52_XmlSchemaElement(string n, string ns, global::System.Xml.Schema.XmlSchemaElement o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaElement)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaElement", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            if (((global::System.Boolean)o.@IsAbstract) != false) {
                WriteAttribute(@"abstract", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsAbstract)));
            }
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Block) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"block", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Block)));
            }
            WriteAttribute(@"default", @"", ((global::System.String)o.@DefaultValue));
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"final", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final)));
            }
            WriteAttribute(@"fixed", @"", ((global::System.String)o.@FixedValue));
            if (((global::System.Xml.Schema.XmlSchemaForm)o.@Form) != global::System.Xml.Schema.XmlSchemaForm.@None) {
                WriteAttribute(@"form", @"", Write6_XmlSchemaForm(((global::System.Xml.Schema.XmlSchemaForm)o.@Form)));
            }
            if ((((global::System.String)o.@Name) != null) && (((global::System.String)o.@Name).Length != 0)) {
                WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            }
            if (((global::System.Boolean)o.@IsNillable) != false) {
                WriteAttribute(@"nillable", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsNillable)));
            }
            WriteAttribute(@"ref", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@RefName)));
            WriteAttribute(@"substitutionGroup", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@SubstitutionGroup)));
            WriteAttribute(@"type", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@SchemaTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@SchemaType is global::System.Xml.Schema.XmlSchemaComplexType) {
                    Write62_XmlSchemaComplexType(@"complexType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexType)o.@SchemaType), false, false);
                }
                else if (o.@SchemaType is global::System.Xml.Schema.XmlSchemaSimpleType) {
                    Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)o.@SchemaType), false, false);
                }
                else {
                    if (o.@SchemaType != null) {
                        throw CreateUnknownTypeException(o.@SchemaType);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Constraints;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaKeyref) {
                                Write51_XmlSchemaKeyref(@"keyref", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaKeyref)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaUnique) {
                                Write50_XmlSchemaUnique(@"unique", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaUnique)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaKey) {
                                Write49_XmlSchemaKey(@"key", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaKey)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write49_XmlSchemaKey(string n, string ns, global::System.Xml.Schema.XmlSchemaKey o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaKey)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaKey", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write47_XmlSchemaXPath(@"selector", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)o.@Selector), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write47_XmlSchemaXPath(@"field", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write47_XmlSchemaXPath(string n, string ns, global::System.Xml.Schema.XmlSchemaXPath o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaXPath)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaXPath", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            if ((((global::System.String)o.@XPath) != null) && (((global::System.String)o.@XPath).Length != 0)) {
                WriteAttribute(@"xpath", @"", ((global::System.String)o.@XPath));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write50_XmlSchemaUnique(string n, string ns, global::System.Xml.Schema.XmlSchemaUnique o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaUnique)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaUnique", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write47_XmlSchemaXPath(@"selector", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)o.@Selector), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write47_XmlSchemaXPath(@"field", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write51_XmlSchemaKeyref(string n, string ns, global::System.Xml.Schema.XmlSchemaKeyref o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaKeyref)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaKeyref", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"refer", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@Refer)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write47_XmlSchemaXPath(@"selector", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)o.@Selector), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write47_XmlSchemaXPath(@"field", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaXPath)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write34_XmlSchemaSimpleType(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleType o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleType)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleType", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"final", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Content is global::System.Xml.Schema.XmlSchemaSimpleTypeUnion) {
                    Write33_XmlSchemaSimpleTypeUnion(@"union", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleTypeUnion)o.@Content), false, false);
                }
                else if (o.@Content is global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction) {
                    Write32_XmlSchemaSimpleTypeRestriction(@"restriction", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction)o.@Content), false, false);
                }
                else if (o.@Content is global::System.Xml.Schema.XmlSchemaSimpleTypeList) {
                    Write17_XmlSchemaSimpleTypeList(@"list", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleTypeList)o.@Content), false, false);
                }
                else {
                    if (o.@Content != null) {
                        throw CreateUnknownTypeException(o.@Content);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write17_XmlSchemaSimpleTypeList(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleTypeList o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleTypeList)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleTypeList", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"itemType", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@ItemTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)o.@ItemType), false, false);
            WriteEndElement(o);
        }

        void Write32_XmlSchemaSimpleTypeRestriction(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleTypeRestriction", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"base", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@BaseTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)o.@BaseType), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Facets;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaLengthFacet) {
                                Write23_XmlSchemaLengthFacet(@"length", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaTotalDigitsFacet) {
                                Write24_XmlSchemaTotalDigitsFacet(@"totalDigits", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaTotalDigitsFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxLengthFacet) {
                                Write22_XmlSchemaMaxLengthFacet(@"maxLength", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaFractionDigitsFacet) {
                                Write20_XmlSchemaFractionDigitsFacet(@"fractionDigits", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaFractionDigitsFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMinLengthFacet) {
                                Write31_XmlSchemaMinLengthFacet(@"minLength", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet) {
                                Write28_XmlSchemaMaxExclusiveFacet(@"maxExclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet) {
                                Write29_XmlSchemaWhiteSpaceFacet(@"whiteSpace", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMinExclusiveFacet) {
                                Write30_XmlSchemaMinExclusiveFacet(@"minExclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinExclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaPatternFacet) {
                                Write25_XmlSchemaPatternFacet(@"pattern", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaPatternFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMinInclusiveFacet) {
                                Write21_XmlSchemaMinInclusiveFacet(@"minInclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinInclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet) {
                                Write27_XmlSchemaMaxInclusiveFacet(@"maxInclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaEnumerationFacet) {
                                Write26_XmlSchemaEnumerationFacet(@"enumeration", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaEnumerationFacet)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write26_XmlSchemaEnumerationFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaEnumerationFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaEnumerationFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaEnumerationFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write27_XmlSchemaMaxInclusiveFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMaxInclusiveFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write21_XmlSchemaMinInclusiveFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMinInclusiveFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMinInclusiveFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMinInclusiveFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write25_XmlSchemaPatternFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaPatternFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaPatternFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaPatternFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write30_XmlSchemaMinExclusiveFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMinExclusiveFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMinExclusiveFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMinExclusiveFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write29_XmlSchemaWhiteSpaceFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaWhiteSpaceFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write28_XmlSchemaMaxExclusiveFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMaxExclusiveFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write31_XmlSchemaMinLengthFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMinLengthFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMinLengthFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMinLengthFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write20_XmlSchemaFractionDigitsFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaFractionDigitsFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaFractionDigitsFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaFractionDigitsFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write22_XmlSchemaMaxLengthFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaMaxLengthFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaMaxLengthFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaMaxLengthFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write24_XmlSchemaTotalDigitsFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaTotalDigitsFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaTotalDigitsFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaTotalDigitsFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write23_XmlSchemaLengthFacet(string n, string ns, global::System.Xml.Schema.XmlSchemaLengthFacet o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaLengthFacet)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaLengthFacet", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"value", @"", ((global::System.String)o.@Value));
            if (((global::System.Boolean)o.@IsFixed) != false) {
                WriteAttribute(@"fixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsFixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write33_XmlSchemaSimpleTypeUnion(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleTypeUnion o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleTypeUnion)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleTypeUnion", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            {
                global::System.Xml.XmlQualifiedName[] a = (global::System.Xml.XmlQualifiedName[])o.@MemberTypes;
                if (a != null) {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlQualifiedName ai = (global::System.Xml.XmlQualifiedName)a[i];
                        if (i != 0) sb.Append(" ");
                        sb.Append(FromXmlQualifiedName(ai));
                    }
                    if (sb.Length != 0) {
                        WriteAttribute(@"memberTypes", @"", sb.ToString());
                    }
                }
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@BaseTypes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        string Write7_XmlSchemaDerivationMethod(global::System.Xml.Schema.XmlSchemaDerivationMethod v) {
            string s = null;
            switch (v) {
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@Empty: s = @""; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@Substitution: s = @"substitution"; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@Extension: s = @"extension"; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@Restriction: s = @"restriction"; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@List: s = @"list"; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@Union: s = @"union"; break;
                case global::System.Xml.Schema.XmlSchemaDerivationMethod.@All: s = @"#all"; break;
                default: s = FromEnum(((System.Int64)v), new string[] { @"", 
                    @"substitution", 
                    @"extension", 
                    @"restriction", 
                    @"list", 
                    @"union", 
                    @"#all" }, new System.Int64[] { (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Empty, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Substitution, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Extension, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Restriction, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@List, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Union, 
                    (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@All }, @"System.Xml.Schema.XmlSchemaDerivationMethod"); break;
            }
            return s;
        }

        void Write62_XmlSchemaComplexType(string n, string ns, global::System.Xml.Schema.XmlSchemaComplexType o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaComplexType)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaComplexType", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"final", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Final)));
            }
            if (((global::System.Boolean)o.@IsAbstract) != false) {
                WriteAttribute(@"abstract", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsAbstract)));
            }
            if (((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Block) != (global::System.Xml.Schema.XmlSchemaDerivationMethod.@None)) {
                WriteAttribute(@"block", @"", Write7_XmlSchemaDerivationMethod(((global::System.Xml.Schema.XmlSchemaDerivationMethod)o.@Block)));
            }
            if (((global::System.Boolean)o.@IsMixed) != false) {
                WriteAttribute(@"mixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsMixed)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@ContentModel is global::System.Xml.Schema.XmlSchemaSimpleContent) {
                    Write61_XmlSchemaSimpleContent(@"simpleContent", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleContent)o.@ContentModel), false, false);
                }
                else if (o.@ContentModel is global::System.Xml.Schema.XmlSchemaComplexContent) {
                    Write58_XmlSchemaComplexContent(@"complexContent", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexContent)o.@ContentModel), false, false);
                }
                else {
                    if (o.@ContentModel != null) {
                        throw CreateUnknownTypeException(o.@ContentModel);
                    }
                }
            }
            {
                if (o.@Particle is global::System.Xml.Schema.XmlSchemaChoice) {
                    Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaAll) {
                    Write55_XmlSchemaAll(@"all", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAll)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaSequence) {
                    Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaGroupRef) {
                    Write44_XmlSchemaGroupRef(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroupRef)o.@Particle), false, false);
                }
                else {
                    if (o.@Particle != null) {
                        throw CreateUnknownTypeException(o.@Particle);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write39_XmlSchemaAnyAttribute(string n, string ns, global::System.Xml.Schema.XmlSchemaAnyAttribute o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAnyAttribute)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAnyAttribute", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            if (((global::System.Xml.Schema.XmlSchemaContentProcessing)o.@ProcessContents) != global::System.Xml.Schema.XmlSchemaContentProcessing.@None) {
                WriteAttribute(@"processContents", @"", Write38_XmlSchemaContentProcessing(((global::System.Xml.Schema.XmlSchemaContentProcessing)o.@ProcessContents)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write36_XmlSchemaAttribute(string n, string ns, global::System.Xml.Schema.XmlSchemaAttribute o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAttribute)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAttribute", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"default", @"", ((global::System.String)o.@DefaultValue));
            WriteAttribute(@"fixed", @"", ((global::System.String)o.@FixedValue));
            if (((global::System.Xml.Schema.XmlSchemaForm)o.@Form) != global::System.Xml.Schema.XmlSchemaForm.@None) {
                WriteAttribute(@"form", @"", Write6_XmlSchemaForm(((global::System.Xml.Schema.XmlSchemaForm)o.@Form)));
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"ref", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@RefName)));
            WriteAttribute(@"type", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@SchemaTypeName)));
            if (((global::System.Xml.Schema.XmlSchemaUse)o.@Use) != global::System.Xml.Schema.XmlSchemaUse.@None) {
                WriteAttribute(@"use", @"", Write35_XmlSchemaUse(((global::System.Xml.Schema.XmlSchemaUse)o.@Use)));
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)o.@SchemaType), false, false);
            WriteEndElement(o);
        }

        string Write35_XmlSchemaUse(global::System.Xml.Schema.XmlSchemaUse v) {
            string s = null;
            switch (v) {
                case global::System.Xml.Schema.XmlSchemaUse.@Optional: s = @"optional"; break;
                case global::System.Xml.Schema.XmlSchemaUse.@Prohibited: s = @"prohibited"; break;
                case global::System.Xml.Schema.XmlSchemaUse.@Required: s = @"required"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Xml.Schema.XmlSchemaUse");
            }
            return s;
        }

        string Write6_XmlSchemaForm(global::System.Xml.Schema.XmlSchemaForm v) {
            string s = null;
            switch (v) {
                case global::System.Xml.Schema.XmlSchemaForm.@Qualified: s = @"qualified"; break;
                case global::System.Xml.Schema.XmlSchemaForm.@Unqualified: s = @"unqualified"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Xml.Schema.XmlSchemaForm");
            }
            return s;
        }

        void Write37_XmlSchemaAttributeGroupRef(string n, string ns, global::System.Xml.Schema.XmlSchemaAttributeGroupRef o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAttributeGroupRef", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"ref", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@RefName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write44_XmlSchemaGroupRef(string n, string ns, global::System.Xml.Schema.XmlSchemaGroupRef o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaGroupRef)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaGroupRef", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            WriteAttribute(@"ref", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@RefName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write55_XmlSchemaAll(string n, string ns, global::System.Xml.Schema.XmlSchemaAll o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAll)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAll", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        Write52_XmlSchemaElement(@"element", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaElement)a[ia]), false, false);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write54_XmlSchemaChoice(string n, string ns, global::System.Xml.Schema.XmlSchemaChoice o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaChoice)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaChoice", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"minOccurs", @"", ((global::System.String)o.@MinOccursString));
            WriteAttribute(@"maxOccurs", @"", ((global::System.String)o.@MaxOccursString));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaSequence) {
                                Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaChoice) {
                                Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaGroupRef) {
                                Write44_XmlSchemaGroupRef(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroupRef)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaElement) {
                                Write52_XmlSchemaElement(@"element", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaElement)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAny) {
                                Write46_XmlSchemaAny(@"any", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAny)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write58_XmlSchemaComplexContent(string n, string ns, global::System.Xml.Schema.XmlSchemaComplexContent o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaComplexContent)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaComplexContent", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"mixed", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsMixed)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Content is global::System.Xml.Schema.XmlSchemaComplexContentRestriction) {
                    Write57_Item(@"restriction", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexContentRestriction)o.@Content), false, false);
                }
                else if (o.@Content is global::System.Xml.Schema.XmlSchemaComplexContentExtension) {
                    Write56_Item(@"extension", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexContentExtension)o.@Content), false, false);
                }
                else {
                    if (o.@Content != null) {
                        throw CreateUnknownTypeException(o.@Content);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write56_Item(string n, string ns, global::System.Xml.Schema.XmlSchemaComplexContentExtension o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaComplexContentExtension)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaComplexContentExtension", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"base", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@BaseTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Particle is global::System.Xml.Schema.XmlSchemaAll) {
                    Write55_XmlSchemaAll(@"all", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAll)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaSequence) {
                    Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaChoice) {
                    Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaGroupRef) {
                    Write44_XmlSchemaGroupRef(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroupRef)o.@Particle), false, false);
                }
                else {
                    if (o.@Particle != null) {
                        throw CreateUnknownTypeException(o.@Particle);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write57_Item(string n, string ns, global::System.Xml.Schema.XmlSchemaComplexContentRestriction o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaComplexContentRestriction)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaComplexContentRestriction", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"base", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@BaseTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Particle is global::System.Xml.Schema.XmlSchemaAll) {
                    Write55_XmlSchemaAll(@"all", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAll)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaSequence) {
                    Write53_XmlSchemaSequence(@"sequence", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSequence)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaChoice) {
                    Write54_XmlSchemaChoice(@"choice", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaChoice)o.@Particle), false, false);
                }
                else if (o.@Particle is global::System.Xml.Schema.XmlSchemaGroupRef) {
                    Write44_XmlSchemaGroupRef(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroupRef)o.@Particle), false, false);
                }
                else {
                    if (o.@Particle != null) {
                        throw CreateUnknownTypeException(o.@Particle);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write61_XmlSchemaSimpleContent(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleContent o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleContent)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleContent", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                if (o.@Content is global::System.Xml.Schema.XmlSchemaSimpleContentExtension) {
                    Write60_Item(@"extension", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleContentExtension)o.@Content), false, false);
                }
                else if (o.@Content is global::System.Xml.Schema.XmlSchemaSimpleContentRestriction) {
                    Write59_Item(@"restriction", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleContentRestriction)o.@Content), false, false);
                }
                else {
                    if (o.@Content != null) {
                        throw CreateUnknownTypeException(o.@Content);
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write59_Item(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleContentRestriction o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleContentRestriction)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleContentRestriction", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"base", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@BaseTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)o.@BaseType), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Facets;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaMinLengthFacet) {
                                Write31_XmlSchemaMinLengthFacet(@"minLength", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxLengthFacet) {
                                Write22_XmlSchemaMaxLengthFacet(@"maxLength", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaLengthFacet) {
                                Write23_XmlSchemaLengthFacet(@"length", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaLengthFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaFractionDigitsFacet) {
                                Write20_XmlSchemaFractionDigitsFacet(@"fractionDigits", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaFractionDigitsFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaTotalDigitsFacet) {
                                Write24_XmlSchemaTotalDigitsFacet(@"totalDigits", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaTotalDigitsFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMinExclusiveFacet) {
                                Write30_XmlSchemaMinExclusiveFacet(@"minExclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinExclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet) {
                                Write27_XmlSchemaMaxInclusiveFacet(@"maxInclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet) {
                                Write28_XmlSchemaMaxExclusiveFacet(@"maxExclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaMinInclusiveFacet) {
                                Write21_XmlSchemaMinInclusiveFacet(@"minInclusive", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaMinInclusiveFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet) {
                                Write29_XmlSchemaWhiteSpaceFacet(@"whiteSpace", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaEnumerationFacet) {
                                Write26_XmlSchemaEnumerationFacet(@"enumeration", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaEnumerationFacet)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaPatternFacet) {
                                Write25_XmlSchemaPatternFacet(@"pattern", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaPatternFacet)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write60_Item(string n, string ns, global::System.Xml.Schema.XmlSchemaSimpleContentExtension o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaSimpleContentExtension)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaSimpleContentExtension", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"base", @"", FromXmlQualifiedName(((global::System.Xml.XmlQualifiedName)o.@BaseTypeName)));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write65_XmlSchemaNotation(string n, string ns, global::System.Xml.Schema.XmlSchemaNotation o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaNotation)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaNotation", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"public", @"", ((global::System.String)o.@Public));
            WriteAttribute(@"system", @"", ((global::System.String)o.@System));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write40_XmlSchemaAttributeGroup(string n, string ns, global::System.Xml.Schema.XmlSchemaAttributeGroup o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaAttributeGroup)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaAttributeGroup", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"name", @"", ((global::System.String)o.@Name));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroupRef) {
                                Write37_XmlSchemaAttributeGroupRef(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroupRef)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttribute) {
                                Write36_XmlSchemaAttribute(@"attribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttribute)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            Write39_XmlSchemaAnyAttribute(@"anyAttribute", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnyAttribute)o.@AnyAttribute), false, false);
            WriteEndElement(o);
        }

        void Write12_XmlSchemaInclude(string n, string ns, global::System.Xml.Schema.XmlSchemaInclude o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaInclude)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaInclude", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"schemaLocation", @"", ((global::System.String)o.@SchemaLocation));
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write13_XmlSchemaImport(string n, string ns, global::System.Xml.Schema.XmlSchemaImport o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaImport)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaImport", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"schemaLocation", @"", ((global::System.String)o.@SchemaLocation));
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)o.@Annotation), false, false);
            WriteEndElement(o);
        }

        void Write64_XmlSchemaRedefine(string n, string ns, global::System.Xml.Schema.XmlSchemaRedefine o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Xml.Schema.XmlSchemaRedefine)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"XmlSchemaRedefine", @"http://www.w3.org/2001/XMLSchema");
            WriteAttribute(@"schemaLocation", @"", ((global::System.String)o.@SchemaLocation));
            WriteAttribute(@"id", @"", ((global::System.String)o.@Id));
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@UnhandledAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            {
                global::System.Xml.Schema.XmlSchemaObjectCollection a = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        global::System.Xml.Schema.XmlSchemaObject ai = (global::System.Xml.Schema.XmlSchemaObject)a[ia];
                        {
                            if (ai is global::System.Xml.Schema.XmlSchemaSimpleType) {
                                Write34_XmlSchemaSimpleType(@"simpleType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaSimpleType)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaComplexType) {
                                Write62_XmlSchemaComplexType(@"complexType", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaComplexType)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaGroup) {
                                Write63_XmlSchemaGroup(@"group", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaGroup)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAttributeGroup) {
                                Write40_XmlSchemaAttributeGroup(@"attributeGroup", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAttributeGroup)ai), false, false);
                            }
                            else if (ai is global::System.Xml.Schema.XmlSchemaAnnotation) {
                                Write11_XmlSchemaAnnotation(@"annotation", @"http://www.w3.org/2001/XMLSchema", ((global::System.Xml.Schema.XmlSchemaAnnotation)ai), false, false);
                            }
                            else {
                                if (ai != null) {
                                    throw CreateUnknownTypeException(ai);
                                }
                            }
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        void Write4_Import(string n, string ns, global::System.Web.Services.Description.Import o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(global::System.Web.Services.Description.Import)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, o.@Namespaces);
            if (needType) WriteXsiType(@"Import", @"http://schemas.xmlsoap.org/wsdl/");
            {
                global::System.Xml.XmlAttribute[] a = (global::System.Xml.XmlAttribute[])o.@ExtensibleAttributes;
                if (a != null) {
                    for (int i = 0; i < a.Length; i++) {
                        global::System.Xml.XmlAttribute ai = (global::System.Xml.XmlAttribute)a[i];
                        WriteXmlAttribute(ai, o);
                    }
                }
            }
            WriteAttribute(@"namespace", @"", ((global::System.String)o.@Namespace));
            WriteAttribute(@"location", @"", ((global::System.String)o.@Location));
            if ((o.@DocumentationElement) is System.Xml.XmlNode || o.@DocumentationElement == null) {
                WriteElementLiteral((System.Xml.XmlNode)o.@DocumentationElement, @"documentation", @"http://schemas.xmlsoap.org/wsdl/", false, true);
            }
            else {
                throw CreateInvalidAnyTypeException(o.@DocumentationElement);
            }
            {
                global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
                if (a != null) {
                    for (int ia = 0; ia < ((System.Collections.ICollection)a).Count; ia++) {
                        if ((a[ia]) is System.Xml.XmlNode || a[ia] == null) {
                            WriteElementLiteral((System.Xml.XmlNode)a[ia], @"", null, false, true);
                        }
                        else {
                            throw CreateInvalidAnyTypeException(a[ia]);
                        }
                    }
                }
            }
            WriteEndElement(o);
        }

        protected override void InitCallbacks() {
        }
    }
    internal class ServiceDescriptionSerializationReader : System.Xml.Serialization.XmlSerializationReader {
        

        public object Read125_definitions() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_definitions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o = Read124_ServiceDescription(true, true);
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null, @"http://schemas.xmlsoap.org/wsdl/:definitions");
            }
            return (object)o;
        }

        global::System.Web.Services.Description.ServiceDescription Read124_ServiceDescription(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_ServiceDescription && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.ServiceDescription o;
            o = new global::System.Web.Services.Description.ServiceDescription();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.ImportCollection a_5 = (global::System.Web.Services.Description.ImportCollection)o.@Imports;
            global::System.Web.Services.Description.MessageCollection a_7 = (global::System.Web.Services.Description.MessageCollection)o.@Messages;
            global::System.Web.Services.Description.PortTypeCollection a_8 = (global::System.Web.Services.Description.PortTypeCollection)o.@PortTypes;
            global::System.Web.Services.Description.BindingCollection a_9 = (global::System.Web.Services.Description.BindingCollection)o.@Bindings;
            global::System.Web.Services.Description.ServiceCollection a_10 = (global::System.Web.Services.Description.ServiceCollection)o.@Services;
            bool[] paramsRead = new bool[12];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[11] && ((object) Reader.LocalName == (object)id6_targetNamespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TargetNamespace = Reader.Value;
                    paramsRead[11] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id8_import && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read4_Import(false, true));
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id9_types && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@Types = Read67_Types(false, true);
                        paramsRead[6] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read69_Message(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id11_portType && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read75_PortType(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id12_binding && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_9) == null) Reader.Skip(); else a_9.Add(Read117_Binding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id13_service && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_10) == null) Reader.Skip(); else a_10.Add(Read123_Service(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:import, http://schemas.xmlsoap.org/wsdl/:types, http://schemas.xmlsoap.org/wsdl/:message, http://schemas.xmlsoap.org/wsdl/:portType, http://schemas.xmlsoap.org/wsdl/:binding, http://schemas.xmlsoap.org/wsdl/:service");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Service Read123_Service(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id14_Service && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Service o;
            o = new global::System.Web.Services.Description.Service();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.PortCollection a_5 = (global::System.Web.Services.Description.PortCollection)o.@Ports;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations1 = 0;
            int readerCount1 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id15_port && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read122_Port(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:port");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations1, ref readerCount1);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Port Read122_Port(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id16_Port && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Port o;
            o = new global::System.Web.Services.Description.Port();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id12_binding && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Binding = ToXmlQualifiedName(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations2 = 0;
            int readerCount2 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id17_address && (object) Reader.NamespaceURI == (object)id18_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read118_HttpAddressBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id17_address && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read119_SoapAddressBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id17_address && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read121_Soap12AddressBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:address, http://schemas.xmlsoap.org/wsdl/soap/:address, http://schemas.xmlsoap.org/wsdl/soap12/:address");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations2, ref readerCount2);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12AddressBinding Read121_Soap12AddressBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id21_Soap12AddressBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12AddressBinding o;
            o = new global::System.Web.Services.Description.Soap12AddressBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_location && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Location = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :location");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations3 = 0;
            int readerCount3 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations3, ref readerCount3);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapAddressBinding Read119_SoapAddressBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id24_SoapAddressBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapAddressBinding o;
            o = new global::System.Web.Services.Description.SoapAddressBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_location && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Location = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :location");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations4 = 0;
            int readerCount4 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations4, ref readerCount4);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.HttpAddressBinding Read118_HttpAddressBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id25_HttpAddressBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id18_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.HttpAddressBinding o;
            o = new global::System.Web.Services.Description.HttpAddressBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_location && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Location = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :location");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations5 = 0;
            int readerCount5 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations5, ref readerCount5);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Binding Read117_Binding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id26_Binding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Binding o;
            o = new global::System.Web.Services.Description.Binding();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.OperationBindingCollection a_5 = (global::System.Web.Services.Description.OperationBindingCollection)o.@Operations;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Type = ToXmlQualifiedName(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations6 = 0;
            int readerCount6 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id12_binding && (object) Reader.NamespaceURI == (object)id18_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read77_HttpBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id12_binding && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read80_SoapBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id12_binding && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read84_Soap12Binding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id28_operation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read116_OperationBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:binding, http://schemas.xmlsoap.org/wsdl/soap/:binding, http://schemas.xmlsoap.org/wsdl/soap12/:binding, http://schemas.xmlsoap.org/wsdl/:operation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations6, ref readerCount6);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.OperationBinding Read116_OperationBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id29_OperationBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.OperationBinding o;
            o = new global::System.Web.Services.Description.OperationBinding();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.FaultBindingCollection a_7 = (global::System.Web.Services.Description.FaultBindingCollection)o.@Faults;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations7 = 0;
            int readerCount7 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id28_operation && (object) Reader.NamespaceURI == (object)id18_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read85_HttpOperationBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id28_operation && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read86_SoapOperationBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id28_operation && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read88_Soap12OperationBinding(false, true));
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id30_input && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@Input = Read110_InputBinding(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id31_output && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@Output = Read111_OutputBinding(false, true);
                        paramsRead[6] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id32_fault && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read115_FaultBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:operation, http://schemas.xmlsoap.org/wsdl/soap/:operation, http://schemas.xmlsoap.org/wsdl/soap12/:operation, http://schemas.xmlsoap.org/wsdl/:input, http://schemas.xmlsoap.org/wsdl/:output, http://schemas.xmlsoap.org/wsdl/:fault");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations7, ref readerCount7);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.FaultBinding Read115_FaultBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id33_FaultBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.FaultBinding o;
            o = new global::System.Web.Services.Description.FaultBinding();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations8 = 0;
            int readerCount8 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id32_fault && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read112_SoapFaultBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id32_fault && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read114_Soap12FaultBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/soap/:fault, http://schemas.xmlsoap.org/wsdl/soap12/:fault");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations8, ref readerCount8);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12FaultBinding Read114_Soap12FaultBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id34_Soap12FaultBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12FaultBinding o;
            o = new global::System.Web.Services.Description.Soap12FaultBinding();
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read100_SoapBindingUse(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :use, :name, :namespace, :encodingStyle");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations9 = 0;
            int readerCount9 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations9, ref readerCount9);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBindingUse Read100_SoapBindingUse(string s) {
            switch (s) {
                case @"encoded": return global::System.Web.Services.Description.SoapBindingUse.@Encoded;
                case @"literal": return global::System.Web.Services.Description.SoapBindingUse.@Literal;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Services.Description.SoapBindingUse));
            }
        }

        global::System.Web.Services.Description.SoapFaultBinding Read112_SoapFaultBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id38_SoapFaultBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapFaultBinding o;
            o = new global::System.Web.Services.Description.SoapFaultBinding();
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read98_SoapBindingUse(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :use, :name, :namespace, :encodingStyle");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations10 = 0;
            int readerCount10 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations10, ref readerCount10);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBindingUse Read98_SoapBindingUse(string s) {
            switch (s) {
                case @"encoded": return global::System.Web.Services.Description.SoapBindingUse.@Encoded;
                case @"literal": return global::System.Web.Services.Description.SoapBindingUse.@Literal;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Services.Description.SoapBindingUse));
            }
        }

        global::System.Web.Services.Description.OutputBinding Read111_OutputBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id39_OutputBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.OutputBinding o;
            o = new global::System.Web.Services.Description.OutputBinding();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations11 = 0;
            int readerCount11 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id40_content && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read93_MimeContentBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id42_mimeXml && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read94_MimeXmlBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id43_multipartRelated && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read104_MimeMultipartRelatedBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id44_text && (object) Reader.NamespaceURI == (object)id45_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read97_MimeTextBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read99_SoapBodyBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id47_header && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read106_SoapHeaderBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read102_Soap12BodyBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id47_header && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read109_Soap12HeaderBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://schemas.xmlsoap.org/wsdl/mime/:multipartRelated, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap/:header, http://schemas.xmlsoap.org/wsdl/soap12/:body, http://schemas.xmlsoap.org/wsdl/soap12/:header");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations11, ref readerCount11);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12HeaderBinding Read109_Soap12HeaderBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id48_Soap12HeaderBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12HeaderBinding o;
            o = new global::System.Web.Services.Description.Soap12HeaderBinding();
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read100_SoapBindingUse(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations12 = 0;
            int readerCount12 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[6] && ((object) Reader.LocalName == (object)id50_headerfault && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        o.@Fault = Read107_SoapHeaderFaultBinding(false, true);
                        paramsRead[6] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/soap12/:headerfault");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/soap12/:headerfault");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations12, ref readerCount12);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapHeaderFaultBinding Read107_SoapHeaderFaultBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id51_SoapHeaderFaultBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapHeaderFaultBinding o;
            o = new global::System.Web.Services.Description.SoapHeaderFaultBinding();
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read100_SoapBindingUse(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations13 = 0;
            int readerCount13 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations13, ref readerCount13);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12BodyBinding Read102_Soap12BodyBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id52_Soap12BodyBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12BodyBinding o;
            o = new global::System.Web.Services.Description.Soap12BodyBinding();
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read100_SoapBindingUse(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id53_parts && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@PartsString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :use, :namespace, :encodingStyle, :parts");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations14 = 0;
            int readerCount14 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations14, ref readerCount14);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapHeaderBinding Read106_SoapHeaderBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id54_SoapHeaderBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapHeaderBinding o;
            o = new global::System.Web.Services.Description.SoapHeaderBinding();
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read98_SoapBindingUse(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations15 = 0;
            int readerCount15 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[6] && ((object) Reader.LocalName == (object)id50_headerfault && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        o.@Fault = Read105_SoapHeaderFaultBinding(false, true);
                        paramsRead[6] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/soap/:headerfault");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/soap/:headerfault");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations15, ref readerCount15);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapHeaderFaultBinding Read105_SoapHeaderFaultBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id51_SoapHeaderFaultBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapHeaderFaultBinding o;
            o = new global::System.Web.Services.Description.SoapHeaderFaultBinding();
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read98_SoapBindingUse(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations16 = 0;
            int readerCount16 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations16, ref readerCount16);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBodyBinding Read99_SoapBodyBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id55_SoapBodyBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapBodyBinding o;
            o = new global::System.Web.Services.Description.SoapBodyBinding();
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read98_SoapBindingUse(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id37_encodingStyle && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Encoding = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id53_parts && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@PartsString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :use, :namespace, :encodingStyle, :parts");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations17 = 0;
            int readerCount17 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations17, ref readerCount17);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimeTextBinding Read97_MimeTextBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id56_MimeTextBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id45_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimeTextBinding o;
            o = new global::System.Web.Services.Description.MimeTextBinding();
            global::System.Web.Services.Description.MimeTextMatchCollection a_1 = (global::System.Web.Services.Description.MimeTextMatchCollection)o.@Matches;
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations18 = 0;
            int readerCount18 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id57_match && (object) Reader.NamespaceURI == (object)id45_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read96_MimeTextMatch(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://microsoft.com/wsdl/mime/textMatching/:match");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://microsoft.com/wsdl/mime/textMatching/:match");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations18, ref readerCount18);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimeTextMatch Read96_MimeTextMatch(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id58_MimeTextMatch && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id45_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimeTextMatch o;
            o = new global::System.Web.Services.Description.MimeTextMatch();
            global::System.Web.Services.Description.MimeTextMatchCollection a_7 = (global::System.Web.Services.Description.MimeTextMatchCollection)o.@Matches;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Type = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Group = System.Xml.XmlConvert.ToInt32(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id60_capture && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Capture = System.Xml.XmlConvert.ToInt32(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id61_repeats && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@RepeatsString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id62_pattern && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Pattern = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id63_ignoreCase && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IgnoreCase = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @":name, :type, :group, :capture, :repeats, :pattern, :ignoreCase");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations19 = 0;
            int readerCount19 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id57_match && (object) Reader.NamespaceURI == (object)id45_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read96_MimeTextMatch(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://microsoft.com/wsdl/mime/textMatching/:match");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://microsoft.com/wsdl/mime/textMatching/:match");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations19, ref readerCount19);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimeMultipartRelatedBinding Read104_MimeMultipartRelatedBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id64_MimeMultipartRelatedBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id41_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimeMultipartRelatedBinding o;
            o = new global::System.Web.Services.Description.MimeMultipartRelatedBinding();
            global::System.Web.Services.Description.MimePartCollection a_1 = (global::System.Web.Services.Description.MimePartCollection)o.@Parts;
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations20 = 0;
            int readerCount20 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read103_MimePart(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/mime/:part");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/mime/:part");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations20, ref readerCount20);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimePart Read103_MimePart(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id65_MimePart && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id41_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimePart o;
            o = new global::System.Web.Services.Description.MimePart();
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_1 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations21 = 0;
            int readerCount21 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id40_content && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read93_MimeContentBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id42_mimeXml && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read94_MimeXmlBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id44_text && (object) Reader.NamespaceURI == (object)id45_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read97_MimeTextBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read99_SoapBodyBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_1) == null) Reader.Skip(); else a_1.Add(Read102_Soap12BodyBinding(false, true));
                    }
                    else {
                        a_1.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap12/:body");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations21, ref readerCount21);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimeXmlBinding Read94_MimeXmlBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id66_MimeXmlBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id41_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimeXmlBinding o;
            o = new global::System.Web.Services.Description.MimeXmlBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :part");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations22 = 0;
            int readerCount22 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations22, ref readerCount22);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MimeContentBinding Read93_MimeContentBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id67_MimeContentBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id41_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MimeContentBinding o;
            o = new global::System.Web.Services.Description.MimeContentBinding();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Part = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Type = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :part, :type");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations23 = 0;
            int readerCount23 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations23, ref readerCount23);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.InputBinding Read110_InputBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id68_InputBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.InputBinding o;
            o = new global::System.Web.Services.Description.InputBinding();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations24 = 0;
            int readerCount24 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id69_urlEncoded && (object) Reader.NamespaceURI == (object)id18_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read90_HttpUrlEncodedBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id70_urlReplacement && (object) Reader.NamespaceURI == (object)id18_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read91_HttpUrlReplacementBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id40_content && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read93_MimeContentBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id42_mimeXml && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read94_MimeXmlBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id43_multipartRelated && (object) Reader.NamespaceURI == (object)id41_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read104_MimeMultipartRelatedBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id44_text && (object) Reader.NamespaceURI == (object)id45_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read97_MimeTextBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read99_SoapBodyBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id47_header && (object) Reader.NamespaceURI == (object)id19_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read106_SoapHeaderBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id46_body && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read102_Soap12BodyBinding(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id47_header && (object) Reader.NamespaceURI == (object)id20_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read109_Soap12HeaderBinding(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:urlEncoded, http://schemas.xmlsoap.org/wsdl/http/:urlReplacement, http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://schemas.xmlsoap.org/wsdl/mime/:multipartRelated, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap/:header, http://schemas.xmlsoap.org/wsdl/soap12/:body, http://schemas.xmlsoap.org/wsdl/soap12/:header");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations24, ref readerCount24);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.HttpUrlReplacementBinding Read91_HttpUrlReplacementBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id71_HttpUrlReplacementBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id18_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.HttpUrlReplacementBinding o;
            o = new global::System.Web.Services.Description.HttpUrlReplacementBinding();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations25 = 0;
            int readerCount25 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations25, ref readerCount25);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.HttpUrlEncodedBinding Read90_HttpUrlEncodedBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id72_HttpUrlEncodedBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id18_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.HttpUrlEncodedBinding o;
            o = new global::System.Web.Services.Description.HttpUrlEncodedBinding();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations26 = 0;
            int readerCount26 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations26, ref readerCount26);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12OperationBinding Read88_Soap12OperationBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id73_Soap12OperationBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12OperationBinding o;
            o = new global::System.Web.Services.Description.Soap12OperationBinding();
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id74_soapAction && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SoapAction = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id75_style && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Style = Read82_SoapBindingStyle(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id76_soapActionRequired && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SoapActionRequired = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :soapAction, :style, :soapActionRequired");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations27 = 0;
            int readerCount27 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations27, ref readerCount27);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBindingStyle Read82_SoapBindingStyle(string s) {
            switch (s) {
                case @"document": return global::System.Web.Services.Description.SoapBindingStyle.@Document;
                case @"rpc": return global::System.Web.Services.Description.SoapBindingStyle.@Rpc;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Services.Description.SoapBindingStyle));
            }
        }

        global::System.Web.Services.Description.SoapOperationBinding Read86_SoapOperationBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id77_SoapOperationBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapOperationBinding o;
            o = new global::System.Web.Services.Description.SoapOperationBinding();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id74_soapAction && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SoapAction = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id75_style && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Style = Read79_SoapBindingStyle(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :soapAction, :style");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations28 = 0;
            int readerCount28 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations28, ref readerCount28);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBindingStyle Read79_SoapBindingStyle(string s) {
            switch (s) {
                case @"document": return global::System.Web.Services.Description.SoapBindingStyle.@Document;
                case @"rpc": return global::System.Web.Services.Description.SoapBindingStyle.@Rpc;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Web.Services.Description.SoapBindingStyle));
            }
        }

        global::System.Web.Services.Description.HttpOperationBinding Read85_HttpOperationBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id78_HttpOperationBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id18_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.HttpOperationBinding o;
            o = new global::System.Web.Services.Description.HttpOperationBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id23_location && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Location = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :location");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations29 = 0;
            int readerCount29 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations29, ref readerCount29);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Soap12Binding Read84_Soap12Binding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id79_Soap12Binding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id20_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Soap12Binding o;
            o = new global::System.Web.Services.Description.Soap12Binding();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id80_transport && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Transport = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id75_style && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Style = Read82_SoapBindingStyle(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :transport, :style");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations30 = 0;
            int readerCount30 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations30, ref readerCount30);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.SoapBinding Read80_SoapBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id81_SoapBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id19_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.SoapBinding o;
            o = new global::System.Web.Services.Description.SoapBinding();
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id80_transport && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Transport = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id75_style && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Style = Read79_SoapBindingStyle(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :transport, :style");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations31 = 0;
            int readerCount31 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations31, ref readerCount31);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.HttpBinding Read77_HttpBinding(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id82_HttpBinding && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id18_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.HttpBinding o;
            o = new global::System.Web.Services.Description.HttpBinding();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[0] && ((object) Reader.LocalName == (object)id22_required && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o.@Required = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[0] = true;
                }
                else if (!paramsRead[1] && ((object) Reader.LocalName == (object)id83_verb && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Verb = Reader.Value;
                    paramsRead[1] = true;
                }
                else if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:required, :verb");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations32 = 0;
            int readerCount32 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    UnknownNode((object)o, @"");
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations32, ref readerCount32);
            }
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.PortType Read75_PortType(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id84_PortType && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.PortType o;
            o = new global::System.Web.Services.Description.PortType();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.OperationCollection a_5 = (global::System.Web.Services.Description.OperationCollection)o.@Operations;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations33 = 0;
            int readerCount33 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id28_operation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read74_Operation(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:operation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations33, ref readerCount33);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Operation Read74_Operation(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id85_Operation && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Operation o;
            o = new global::System.Web.Services.Description.Operation();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.OperationMessageCollection a_6 = (global::System.Web.Services.Description.OperationMessageCollection)o.@Messages;
            global::System.Web.Services.Description.OperationFaultCollection a_7 = (global::System.Web.Services.Description.OperationFaultCollection)o.@Faults;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id86_parameterOrder && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ParameterOrderString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations34 = 0;
            int readerCount34 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id30_input && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read71_OperationInput(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id31_output && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read72_OperationOutput(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id32_fault && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read73_OperationFault(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:input, http://schemas.xmlsoap.org/wsdl/:output, http://schemas.xmlsoap.org/wsdl/:fault");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations34, ref readerCount34);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.OperationFault Read73_OperationFault(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id87_OperationFault && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.OperationFault o;
            o = new global::System.Web.Services.Description.OperationFault();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_5 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations35 = 0;
            int readerCount35 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else {
                        a_5.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations35, ref readerCount35);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.OperationOutput Read72_OperationOutput(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id88_OperationOutput && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.OperationOutput o;
            o = new global::System.Web.Services.Description.OperationOutput();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_5 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations36 = 0;
            int readerCount36 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else {
                        a_5.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations36, ref readerCount36);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.OperationInput Read71_OperationInput(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id89_OperationInput && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.OperationInput o;
            o = new global::System.Web.Services.Description.OperationInput();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_5 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id10_message && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Message = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations37 = 0;
            int readerCount37 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else {
                        a_5.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations37, ref readerCount37);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Message Read69_Message(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id90_Message && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Message o;
            o = new global::System.Web.Services.Description.Message();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Web.Services.Description.MessagePartCollection a_5 = (global::System.Web.Services.Description.MessagePartCollection)o.@Parts;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations38 = 0;
            int readerCount38 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id49_part && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read68_MessagePart(false, true));
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:part");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations38, ref readerCount38);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.MessagePart Read68_MessagePart(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id91_MessagePart && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.MessagePart o;
            o = new global::System.Web.Services.Description.MessagePart();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_4 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[3] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[3] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id92_element && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Element = ToXmlQualifiedName(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Type = ToXmlQualifiedName(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations39 = 0;
            int readerCount39 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else {
                        a_4.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations39, ref readerCount39);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Types Read67_Types(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id93_Types && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Types o;
            o = new global::System.Web.Services.Description.Types();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_3 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            global::System.Xml.Serialization.XmlSchemas a_4 = (global::System.Xml.Serialization.XmlSchemas)o.@Schemas;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations40 = 0;
            int readerCount40 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id94_schema && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read66_XmlSchema(false, true));
                    }
                    else {
                        a_3.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation, http://www.w3.org/2001/XMLSchema:schema");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations40, ref readerCount40);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchema Read66_XmlSchema(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id96_XmlSchema && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchema o;
            o = new global::System.Xml.Schema.XmlSchema();
            global::System.Xml.Schema.XmlSchemaObjectCollection a_7 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Includes;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_8 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            global::System.Xml.XmlAttribute[] a_10 = null;
            int ca_10 = 0;
            bool[] paramsRead = new bool[11];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id97_attributeFormDefault && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@AttributeFormDefault = Read6_XmlSchemaForm(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id98_blockDefault && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BlockDefault = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id99_finalDefault && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FinalDefault = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[3] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id100_elementFormDefault && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ElementFormDefault = Read6_XmlSchemaForm(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id6_targetNamespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@TargetNamespace = CollapseWhitespace(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id101_version && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Version = CollapseWhitespace(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[9] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_10 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_10, ca_10, typeof(global::System.Xml.XmlAttribute)); a_10[ca_10++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_10, ca_10, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_10, ca_10, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations41 = 0;
            int readerCount41 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id103_include && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read12_XmlSchemaInclude(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id8_import && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read13_XmlSchemaImport(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id104_redefine && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read64_XmlSchemaRedefine(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read34_XmlSchemaSimpleType(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id106_complexType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read62_XmlSchemaComplexType(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read11_XmlSchemaAnnotation(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id108_notation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read65_XmlSchemaNotation(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read63_XmlSchemaGroup(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id92_element && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read52_XmlSchemaElement(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_8) == null) Reader.Skip(); else a_8.Add(Read40_XmlSchemaAttributeGroup(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:include, http://www.w3.org/2001/XMLSchema:import, http://www.w3.org/2001/XMLSchema:redefine, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:notation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:include, http://www.w3.org/2001/XMLSchema:import, http://www.w3.org/2001/XMLSchema:redefine, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:notation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations41, ref readerCount41);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_10, ca_10, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAttributeGroup Read40_XmlSchemaAttributeGroup(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id111_XmlSchemaAttributeGroup && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAttributeGroup o;
            o = new global::System.Xml.Schema.XmlSchemaAttributeGroup();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_5 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations42 = 0;
            int readerCount42 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[6] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations42, ref readerCount42);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAnyAttribute Read39_XmlSchemaAnyAttribute(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id113_XmlSchemaAnyAttribute && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAnyAttribute o;
            o = new global::System.Xml.Schema.XmlSchemaAnyAttribute();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id114_processContents && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ProcessContents = Read38_XmlSchemaContentProcessing(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations43 = 0;
            int readerCount43 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations43, ref readerCount43);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAnnotation Read11_XmlSchemaAnnotation(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id115_XmlSchemaAnnotation && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAnnotation o;
            o = new global::System.Xml.Schema.XmlSchemaAnnotation();
            global::System.Xml.Schema.XmlSchemaObjectCollection a_2 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations44 = 0;
            int readerCount44 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_2) == null) Reader.Skip(); else a_2.Add(Read9_XmlSchemaDocumentation(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id116_appinfo && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_2) == null) Reader.Skip(); else a_2.Add(Read10_XmlSchemaAppInfo(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:documentation, http://www.w3.org/2001/XMLSchema:appinfo");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:documentation, http://www.w3.org/2001/XMLSchema:appinfo");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations44, ref readerCount44);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAppInfo Read10_XmlSchemaAppInfo(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id117_XmlSchemaAppInfo && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAppInfo o;
            o = new global::System.Xml.Schema.XmlSchemaAppInfo();
            global::System.Xml.XmlNode[] a_2 = null;
            int ca_2 = 0;
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id118_source && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Source = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    UnknownNode((object)o, @":source");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@Markup = (global::System.Xml.XmlNode[])ShrinkArray(a_2, ca_2, typeof(global::System.Xml.XmlNode), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations45 = 0;
            int readerCount45 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    a_2 = (global::System.Xml.XmlNode[])EnsureArrayIndex(a_2, ca_2, typeof(global::System.Xml.XmlNode)); a_2[ca_2++] = (global::System.Xml.XmlNode)ReadXmlNode(false);
                }
                else if (Reader.NodeType == System.Xml.XmlNodeType.Text || 
                Reader.NodeType == System.Xml.XmlNodeType.CDATA || 
                Reader.NodeType == System.Xml.XmlNodeType.Whitespace || 
                Reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace) {
                    a_2 = (global::System.Xml.XmlNode[])EnsureArrayIndex(a_2, ca_2, typeof(global::System.Xml.XmlNode)); a_2[ca_2++] = (global::System.Xml.XmlNode)Document.CreateTextNode(Reader.ReadString());
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations45, ref readerCount45);
            }
            o.@Markup = (global::System.Xml.XmlNode[])ShrinkArray(a_2, ca_2, typeof(global::System.Xml.XmlNode), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaDocumentation Read9_XmlSchemaDocumentation(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id119_XmlSchemaDocumentation && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaDocumentation o;
            o = new global::System.Xml.Schema.XmlSchemaDocumentation();
            global::System.Xml.XmlNode[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id118_source && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Source = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id120_lang && (object) Reader.NamespaceURI == (object)id121_Item)) {
                    o.@Language = Reader.Value;
                    paramsRead[2] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    UnknownNode((object)o, @":source, http://www.w3.org/XML/1998/namespace");
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@Markup = (global::System.Xml.XmlNode[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlNode), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations46 = 0;
            int readerCount46 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    a_3 = (global::System.Xml.XmlNode[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlNode)); a_3[ca_3++] = (global::System.Xml.XmlNode)ReadXmlNode(false);
                }
                else if (Reader.NodeType == System.Xml.XmlNodeType.Text || 
                Reader.NodeType == System.Xml.XmlNodeType.CDATA || 
                Reader.NodeType == System.Xml.XmlNodeType.Whitespace || 
                Reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace) {
                    a_3 = (global::System.Xml.XmlNode[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlNode)); a_3[ca_3++] = (global::System.Xml.XmlNode)Document.CreateTextNode(Reader.ReadString());
                }
                else {
                    UnknownNode((object)o, @"");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations46, ref readerCount46);
            }
            o.@Markup = (global::System.Xml.XmlNode[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlNode), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaContentProcessing Read38_XmlSchemaContentProcessing(string s) {
            switch (s) {
                case @"skip": return global::System.Xml.Schema.XmlSchemaContentProcessing.@Skip;
                case @"lax": return global::System.Xml.Schema.XmlSchemaContentProcessing.@Lax;
                case @"strict": return global::System.Xml.Schema.XmlSchemaContentProcessing.@Strict;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Xml.Schema.XmlSchemaContentProcessing));
            }
        }

        global::System.Xml.Schema.XmlSchemaAttributeGroupRef Read37_XmlSchemaAttributeGroupRef(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id122_XmlSchemaAttributeGroupRef && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAttributeGroupRef o;
            o = new global::System.Xml.Schema.XmlSchemaAttributeGroupRef();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id123_ref && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@RefName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations47 = 0;
            int readerCount47 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations47, ref readerCount47);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAttribute Read36_XmlSchemaAttribute(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id124_XmlSchemaAttribute && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAttribute o;
            o = new global::System.Xml.Schema.XmlSchemaAttribute();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[12];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id125_default && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@DefaultValue = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FixedValue = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id127_form && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Form = Read6_XmlSchemaForm(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[7] = true;
                }
                else if (!paramsRead[8] && ((object) Reader.LocalName == (object)id123_ref && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@RefName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[8] = true;
                }
                else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SchemaTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[9] = true;
                }
                else if (!paramsRead[11] && ((object) Reader.LocalName == (object)id35_use && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Use = Read35_XmlSchemaUse(Reader.Value);
                    paramsRead[11] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations48 = 0;
            int readerCount48 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@SchemaType = Read34_XmlSchemaSimpleType(false, true);
                        paramsRead[10] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations48, ref readerCount48);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleType Read34_XmlSchemaSimpleType(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id128_XmlSchemaSimpleType && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleType o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleType();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id129_final && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Final = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations49 = 0;
            int readerCount49 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id130_list && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read17_XmlSchemaSimpleTypeList(false, true);
                        paramsRead[6] = true;
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id131_restriction && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read32_XmlSchemaSimpleTypeRestriction(false, true);
                        paramsRead[6] = true;
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id132_union && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read33_XmlSchemaSimpleTypeUnion(false, true);
                        paramsRead[6] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:list, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:union");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:list, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:union");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations49, ref readerCount49);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleTypeUnion Read33_XmlSchemaSimpleTypeUnion(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id133_XmlSchemaSimpleTypeUnion && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleTypeUnion o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleTypeUnion();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_4 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@BaseTypes;
            global::System.Xml.XmlQualifiedName[] a_5 = null;
            int ca_5 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (((object) Reader.LocalName == (object)id134_memberTypes && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    string listValues = Reader.Value;
                    string[] vals = listValues.Split(null);
                    for (int i = 0; i < vals.Length; i++) {
                        a_5 = (global::System.Xml.XmlQualifiedName[])EnsureArrayIndex(a_5, ca_5, typeof(global::System.Xml.XmlQualifiedName)); a_5[ca_5++] = ToXmlQualifiedName(vals[i]);
                    }
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            o.@MemberTypes = (global::System.Xml.XmlQualifiedName[])ShrinkArray(a_5, ca_5, typeof(global::System.Xml.XmlQualifiedName), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                o.@MemberTypes = (global::System.Xml.XmlQualifiedName[])ShrinkArray(a_5, ca_5, typeof(global::System.Xml.XmlQualifiedName), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations50 = 0;
            int readerCount50 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read34_XmlSchemaSimpleType(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations50, ref readerCount50);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            o.@MemberTypes = (global::System.Xml.XmlQualifiedName[])ShrinkArray(a_5, ca_5, typeof(global::System.Xml.XmlQualifiedName), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction Read32_XmlSchemaSimpleTypeRestriction(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id135_XmlSchemaSimpleTypeRestriction && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleTypeRestriction();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Facets;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id136_base && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BaseTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations51 = 0;
            int readerCount51 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@BaseType = Read34_XmlSchemaSimpleType(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id137_fractionDigits && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read20_XmlSchemaFractionDigitsFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id138_minInclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read21_XmlSchemaMinInclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id139_maxLength && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read22_XmlSchemaMaxLengthFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id140_length && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read23_XmlSchemaLengthFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id141_totalDigits && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read24_XmlSchemaTotalDigitsFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id62_pattern && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read25_XmlSchemaPatternFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id142_enumeration && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read26_XmlSchemaEnumerationFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id143_maxInclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read27_XmlSchemaMaxInclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id144_maxExclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read28_XmlSchemaMaxExclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id145_whiteSpace && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read29_XmlSchemaWhiteSpaceFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id146_minExclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read30_XmlSchemaMinExclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id147_minLength && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read31_XmlSchemaMinLengthFacet(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:minLength");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:minLength");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations51, ref readerCount51);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMinLengthFacet Read31_XmlSchemaMinLengthFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id148_XmlSchemaMinLengthFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMinLengthFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMinLengthFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations52 = 0;
            int readerCount52 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations52, ref readerCount52);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMinExclusiveFacet Read30_XmlSchemaMinExclusiveFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id150_XmlSchemaMinExclusiveFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMinExclusiveFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMinExclusiveFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations53 = 0;
            int readerCount53 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations53, ref readerCount53);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet Read29_XmlSchemaWhiteSpaceFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id151_XmlSchemaWhiteSpaceFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet o;
            o = new global::System.Xml.Schema.XmlSchemaWhiteSpaceFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations54 = 0;
            int readerCount54 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations54, ref readerCount54);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet Read28_XmlSchemaMaxExclusiveFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id152_XmlSchemaMaxExclusiveFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMaxExclusiveFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations55 = 0;
            int readerCount55 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations55, ref readerCount55);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet Read27_XmlSchemaMaxInclusiveFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id153_XmlSchemaMaxInclusiveFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMaxInclusiveFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations56 = 0;
            int readerCount56 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations56, ref readerCount56);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaEnumerationFacet Read26_XmlSchemaEnumerationFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id154_XmlSchemaEnumerationFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaEnumerationFacet o;
            o = new global::System.Xml.Schema.XmlSchemaEnumerationFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations57 = 0;
            int readerCount57 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations57, ref readerCount57);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaPatternFacet Read25_XmlSchemaPatternFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id155_XmlSchemaPatternFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaPatternFacet o;
            o = new global::System.Xml.Schema.XmlSchemaPatternFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations58 = 0;
            int readerCount58 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations58, ref readerCount58);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaTotalDigitsFacet Read24_XmlSchemaTotalDigitsFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id156_XmlSchemaTotalDigitsFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaTotalDigitsFacet o;
            o = new global::System.Xml.Schema.XmlSchemaTotalDigitsFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations59 = 0;
            int readerCount59 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations59, ref readerCount59);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaLengthFacet Read23_XmlSchemaLengthFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id157_XmlSchemaLengthFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaLengthFacet o;
            o = new global::System.Xml.Schema.XmlSchemaLengthFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations60 = 0;
            int readerCount60 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations60, ref readerCount60);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMaxLengthFacet Read22_XmlSchemaMaxLengthFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id158_XmlSchemaMaxLengthFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMaxLengthFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMaxLengthFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations61 = 0;
            int readerCount61 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations61, ref readerCount61);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaMinInclusiveFacet Read21_XmlSchemaMinInclusiveFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id159_XmlSchemaMinInclusiveFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaMinInclusiveFacet o;
            o = new global::System.Xml.Schema.XmlSchemaMinInclusiveFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations62 = 0;
            int readerCount62 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations62, ref readerCount62);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaFractionDigitsFacet Read20_XmlSchemaFractionDigitsFacet(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id160_XmlSchemaFractionDigitsFacet && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaFractionDigitsFacet o;
            o = new global::System.Xml.Schema.XmlSchemaFractionDigitsFacet();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id149_value && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Value = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsFixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations63 = 0;
            int readerCount63 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations63, ref readerCount63);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleTypeList Read17_XmlSchemaSimpleTypeList(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id161_XmlSchemaSimpleTypeList && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleTypeList o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleTypeList();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id162_itemType && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ItemTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations64 = 0;
            int readerCount64 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@ItemType = Read34_XmlSchemaSimpleType(false, true);
                        paramsRead[5] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations64, ref readerCount64);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        System.Collections.Hashtable _XmlSchemaDerivationMethodValues;

        internal System.Collections.Hashtable XmlSchemaDerivationMethodValues {
            get {
                if ((object)_XmlSchemaDerivationMethodValues == null) {
                    System.Collections.Hashtable h = new System.Collections.Hashtable();
                    h.Add(@"", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Empty);
                    h.Add(@"substitution", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Substitution);
                    h.Add(@"extension", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Extension);
                    h.Add(@"restriction", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Restriction);
                    h.Add(@"list", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@List);
                    h.Add(@"union", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@Union);
                    h.Add(@"#all", (long)global::System.Xml.Schema.XmlSchemaDerivationMethod.@All);
                    _XmlSchemaDerivationMethodValues = h;
                }
                return _XmlSchemaDerivationMethodValues;
            }
        }

        global::System.Xml.Schema.XmlSchemaDerivationMethod Read7_XmlSchemaDerivationMethod(string s) {
            return (global::System.Xml.Schema.XmlSchemaDerivationMethod)ToEnum(s, XmlSchemaDerivationMethodValues, @"global::System.Xml.Schema.XmlSchemaDerivationMethod");
        }

        global::System.Xml.Schema.XmlSchemaUse Read35_XmlSchemaUse(string s) {
            switch (s) {
                case @"optional": return global::System.Xml.Schema.XmlSchemaUse.@Optional;
                case @"prohibited": return global::System.Xml.Schema.XmlSchemaUse.@Prohibited;
                case @"required": return global::System.Xml.Schema.XmlSchemaUse.@Required;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Xml.Schema.XmlSchemaUse));
            }
        }

        global::System.Xml.Schema.XmlSchemaForm Read6_XmlSchemaForm(string s) {
            switch (s) {
                case @"qualified": return global::System.Xml.Schema.XmlSchemaForm.@Qualified;
                case @"unqualified": return global::System.Xml.Schema.XmlSchemaForm.@Unqualified;
                default: throw CreateUnknownConstantException(s, typeof(global::System.Xml.Schema.XmlSchemaForm));
            }
        }

        global::System.Xml.Schema.XmlSchemaElement Read52_XmlSchemaElement(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id163_XmlSchemaElement && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaElement o;
            o = new global::System.Xml.Schema.XmlSchemaElement();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_18 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Constraints;
            bool[] paramsRead = new bool[19];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id166_abstract && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsAbstract = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id167_block && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Block = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[7] = true;
                }
                else if (!paramsRead[8] && ((object) Reader.LocalName == (object)id125_default && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@DefaultValue = Reader.Value;
                    paramsRead[8] = true;
                }
                else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id129_final && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Final = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[9] = true;
                }
                else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id126_fixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@FixedValue = Reader.Value;
                    paramsRead[10] = true;
                }
                else if (!paramsRead[11] && ((object) Reader.LocalName == (object)id127_form && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Form = Read6_XmlSchemaForm(Reader.Value);
                    paramsRead[11] = true;
                }
                else if (!paramsRead[12] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[12] = true;
                }
                else if (!paramsRead[13] && ((object) Reader.LocalName == (object)id168_nillable && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsNillable = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[13] = true;
                }
                else if (!paramsRead[14] && ((object) Reader.LocalName == (object)id123_ref && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@RefName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[14] = true;
                }
                else if (!paramsRead[15] && ((object) Reader.LocalName == (object)id169_substitutionGroup && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SubstitutionGroup = ToXmlQualifiedName(Reader.Value);
                    paramsRead[15] = true;
                }
                else if (!paramsRead[16] && ((object) Reader.LocalName == (object)id27_type && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SchemaTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[16] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations65 = 0;
            int readerCount65 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[17] && ((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@SchemaType = Read34_XmlSchemaSimpleType(false, true);
                        paramsRead[17] = true;
                    }
                    else if (!paramsRead[17] && ((object) Reader.LocalName == (object)id106_complexType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@SchemaType = Read62_XmlSchemaComplexType(false, true);
                        paramsRead[17] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id170_key && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_18) == null) Reader.Skip(); else a_18.Add(Read49_XmlSchemaKey(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id171_unique && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_18) == null) Reader.Skip(); else a_18.Add(Read50_XmlSchemaUnique(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id172_keyref && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_18) == null) Reader.Skip(); else a_18.Add(Read51_XmlSchemaKeyref(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:key, http://www.w3.org/2001/XMLSchema:unique, http://www.w3.org/2001/XMLSchema:keyref");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:key, http://www.w3.org/2001/XMLSchema:unique, http://www.w3.org/2001/XMLSchema:keyref");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations65, ref readerCount65);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaKeyref Read51_XmlSchemaKeyref(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id173_XmlSchemaKeyref && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaKeyref o;
            o = new global::System.Xml.Schema.XmlSchemaKeyref();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id174_refer && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Refer = ToXmlQualifiedName(Reader.Value);
                    paramsRead[7] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations66 = 0;
            int readerCount66 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id175_selector && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Selector = Read47_XmlSchemaXPath(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id176_field && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read47_XmlSchemaXPath(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations66, ref readerCount66);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaXPath Read47_XmlSchemaXPath(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id177_XmlSchemaXPath && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaXPath o;
            o = new global::System.Xml.Schema.XmlSchemaXPath();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id178_xpath && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@XPath = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations67 = 0;
            int readerCount67 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations67, ref readerCount67);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaUnique Read50_XmlSchemaUnique(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id179_XmlSchemaUnique && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaUnique o;
            o = new global::System.Xml.Schema.XmlSchemaUnique();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations68 = 0;
            int readerCount68 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id175_selector && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Selector = Read47_XmlSchemaXPath(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id176_field && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read47_XmlSchemaXPath(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations68, ref readerCount68);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaKey Read49_XmlSchemaKey(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id180_XmlSchemaKey && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaKey o;
            o = new global::System.Xml.Schema.XmlSchemaKey();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Fields;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations69 = 0;
            int readerCount69 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id175_selector && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Selector = Read47_XmlSchemaXPath(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id176_field && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read47_XmlSchemaXPath(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations69, ref readerCount69);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaComplexType Read62_XmlSchemaComplexType(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id181_XmlSchemaComplexType && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaComplexType o;
            o = new global::System.Xml.Schema.XmlSchemaComplexType();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_11 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[13];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id129_final && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Final = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id166_abstract && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsAbstract = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id167_block && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Block = Read7_XmlSchemaDerivationMethod(Reader.Value);
                    paramsRead[7] = true;
                }
                else if (!paramsRead[8] && ((object) Reader.LocalName == (object)id182_mixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsMixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[8] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations70 = 0;
            int readerCount70 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id183_complexContent && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@ContentModel = Read58_XmlSchemaComplexContent(false, true);
                        paramsRead[9] = true;
                    }
                    else if (!paramsRead[9] && ((object) Reader.LocalName == (object)id184_simpleContent && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@ContentModel = Read61_XmlSchemaSimpleContent(false, true);
                        paramsRead[9] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read44_XmlSchemaGroupRef(false, true);
                        paramsRead[10] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read53_XmlSchemaSequence(false, true);
                        paramsRead[10] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read54_XmlSchemaChoice(false, true);
                        paramsRead[10] = true;
                    }
                    else if (!paramsRead[10] && ((object) Reader.LocalName == (object)id187_all && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read55_XmlSchemaAll(false, true);
                        paramsRead[10] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_11) == null) Reader.Skip(); else a_11.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_11) == null) Reader.Skip(); else a_11.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (!paramsRead[12] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[12] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:complexContent, http://www.w3.org/2001/XMLSchema:simpleContent, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:complexContent, http://www.w3.org/2001/XMLSchema:simpleContent, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations70, ref readerCount70);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAll Read55_XmlSchemaAll(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id188_XmlSchemaAll && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAll o;
            o = new global::System.Xml.Schema.XmlSchemaAll();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations71 = 0;
            int readerCount71 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id92_element && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read52_XmlSchemaElement(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations71, ref readerCount71);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaChoice Read54_XmlSchemaChoice(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id189_XmlSchemaChoice && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaChoice o;
            o = new global::System.Xml.Schema.XmlSchemaChoice();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations72 = 0;
            int readerCount72 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id190_any && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read46_XmlSchemaAny(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read54_XmlSchemaChoice(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read53_XmlSchemaSequence(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id92_element && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read52_XmlSchemaElement(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read44_XmlSchemaGroupRef(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:group");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations72, ref readerCount72);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaGroupRef Read44_XmlSchemaGroupRef(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id191_XmlSchemaGroupRef && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaGroupRef o;
            o = new global::System.Xml.Schema.XmlSchemaGroupRef();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id123_ref && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@RefName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[6] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations73 = 0;
            int readerCount73 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations73, ref readerCount73);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSequence Read53_XmlSchemaSequence(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id192_XmlSchemaSequence && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSequence o;
            o = new global::System.Xml.Schema.XmlSchemaSequence();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations74 = 0;
            int readerCount74 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id92_element && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read52_XmlSchemaElement(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read53_XmlSchemaSequence(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id190_any && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read46_XmlSchemaAny(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read54_XmlSchemaChoice(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read44_XmlSchemaGroupRef(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations74, ref readerCount74);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaAny Read46_XmlSchemaAny(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id193_XmlSchemaAny && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaAny o;
            o = new global::System.Xml.Schema.XmlSchemaAny();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id164_minOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MinOccursString = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id165_maxOccurs && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@MaxOccursString = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[6] = true;
                }
                else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id114_processContents && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@ProcessContents = Read38_XmlSchemaContentProcessing(Reader.Value);
                    paramsRead[7] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations75 = 0;
            int readerCount75 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations75, ref readerCount75);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleContent Read61_XmlSchemaSimpleContent(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id194_XmlSchemaSimpleContent && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleContent o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleContent();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations76 = 0;
            int readerCount76 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id131_restriction && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read59_Item(false, true);
                        paramsRead[4] = true;
                    }
                    else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id195_extension && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read60_Item(false, true);
                        paramsRead[4] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:extension");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:extension");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations76, ref readerCount76);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleContentExtension Read60_Item(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id196_Item && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleContentExtension o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleContentExtension();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_5 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id136_base && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BaseTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations77 = 0;
            int readerCount77 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_5) == null) Reader.Skip(); else a_5.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[6] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations77, ref readerCount77);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaSimpleContentRestriction Read59_Item(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id197_Item && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaSimpleContentRestriction o;
            o = new global::System.Xml.Schema.XmlSchemaSimpleContentRestriction();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Facets;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_7 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[9];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id136_base && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BaseTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations78 = 0;
            int readerCount78 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@BaseType = Read34_XmlSchemaSimpleType(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id138_minInclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read21_XmlSchemaMinInclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id144_maxExclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read28_XmlSchemaMaxExclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id145_whiteSpace && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read29_XmlSchemaWhiteSpaceFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id147_minLength && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read31_XmlSchemaMinLengthFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id62_pattern && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read25_XmlSchemaPatternFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id142_enumeration && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read26_XmlSchemaEnumerationFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id143_maxInclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read27_XmlSchemaMaxInclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id140_length && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read23_XmlSchemaLengthFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id139_maxLength && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read22_XmlSchemaMaxLengthFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id146_minExclusive && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read30_XmlSchemaMinExclusiveFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id141_totalDigits && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read24_XmlSchemaTotalDigitsFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id137_fractionDigits && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read20_XmlSchemaFractionDigitsFacet(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_7) == null) Reader.Skip(); else a_7.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (!paramsRead[8] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[8] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minLength, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minLength, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations78, ref readerCount78);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaComplexContent Read58_XmlSchemaComplexContent(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id198_XmlSchemaComplexContent && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaComplexContent o;
            o = new global::System.Xml.Schema.XmlSchemaComplexContent();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id182_mixed && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@IsMixed = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations79 = 0;
            int readerCount79 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id195_extension && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read56_Item(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id131_restriction && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Content = Read57_Item(false, true);
                        paramsRead[5] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:extension, http://www.w3.org/2001/XMLSchema:restriction");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:extension, http://www.w3.org/2001/XMLSchema:restriction");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations79, ref readerCount79);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaComplexContentRestriction Read57_Item(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id199_Item && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaComplexContentRestriction o;
            o = new global::System.Xml.Schema.XmlSchemaComplexContentRestriction();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id136_base && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BaseTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations80 = 0;
            int readerCount80 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read54_XmlSchemaChoice(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read44_XmlSchemaGroupRef(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id187_all && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read55_XmlSchemaAll(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read53_XmlSchemaSequence(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[7] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations80, ref readerCount80);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaComplexContentExtension Read56_Item(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id200_Item && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaComplexContentExtension o;
            o = new global::System.Xml.Schema.XmlSchemaComplexContentExtension();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_6 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Attributes;
            bool[] paramsRead = new bool[8];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id136_base && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@BaseTypeName = ToXmlQualifiedName(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations81 = 0;
            int readerCount81 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read44_XmlSchemaGroupRef(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read54_XmlSchemaChoice(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id187_all && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read55_XmlSchemaAll(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read53_XmlSchemaSequence(false, true);
                        paramsRead[5] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read37_XmlSchemaAttributeGroupRef(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id109_attribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_6) == null) Reader.Skip(); else a_6.Add(Read36_XmlSchemaAttribute(false, true));
                    }
                    else if (!paramsRead[7] && ((object) Reader.LocalName == (object)id112_anyAttribute && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@AnyAttribute = Read39_XmlSchemaAnyAttribute(false, true);
                        paramsRead[7] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations81, ref readerCount81);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaGroup Read63_XmlSchemaGroup(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id201_XmlSchemaGroup && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaGroup o;
            o = new global::System.Xml.Schema.XmlSchemaGroup();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations82 = 0;
            int readerCount82 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id185_sequence && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read53_XmlSchemaSequence(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id186_choice && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read54_XmlSchemaChoice(false, true);
                        paramsRead[5] = true;
                    }
                    else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id187_all && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Particle = Read55_XmlSchemaAll(false, true);
                        paramsRead[5] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations82, ref readerCount82);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaNotation Read65_XmlSchemaNotation(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id202_XmlSchemaNotation && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaNotation o;
            o = new global::System.Xml.Schema.XmlSchemaNotation();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[7];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id4_name && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Name = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id203_public && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Public = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (!paramsRead[6] && ((object) Reader.LocalName == (object)id204_system && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@System = Reader.Value;
                    paramsRead[6] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations83 = 0;
            int readerCount83 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[2] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[2] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations83, ref readerCount83);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaRedefine Read64_XmlSchemaRedefine(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id205_XmlSchemaRedefine && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaRedefine o;
            o = new global::System.Xml.Schema.XmlSchemaRedefine();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            global::System.Xml.Schema.XmlSchemaObjectCollection a_4 = (global::System.Xml.Schema.XmlSchemaObjectCollection)o.@Items;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id206_schemaLocation && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SchemaLocation = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations84 = 0;
            int readerCount84 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (((object) Reader.LocalName == (object)id110_attributeGroup && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read40_XmlSchemaAttributeGroup(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id106_complexType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read62_XmlSchemaComplexType(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id105_simpleType && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read34_XmlSchemaSimpleType(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read11_XmlSchemaAnnotation(false, true));
                    }
                    else if (((object) Reader.LocalName == (object)id59_group && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        if ((object)(a_4) == null) Reader.Skip(); else a_4.Add(Read63_XmlSchemaGroup(false, true));
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations84, ref readerCount84);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaImport Read13_XmlSchemaImport(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id207_XmlSchemaImport && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaImport o;
            o = new global::System.Xml.Schema.XmlSchemaImport();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id206_schemaLocation && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SchemaLocation = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (!paramsRead[4] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = CollapseWhitespace(Reader.Value);
                    paramsRead[4] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations85 = 0;
            int readerCount85 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[5] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[5] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations85, ref readerCount85);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Xml.Schema.XmlSchemaInclude Read12_XmlSchemaInclude(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id208_XmlSchemaInclude && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id95_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            DecodeName = false;
            global::System.Xml.Schema.XmlSchemaInclude o;
            o = new global::System.Xml.Schema.XmlSchemaInclude();
            global::System.Xml.XmlAttribute[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[5];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[1] && ((object) Reader.LocalName == (object)id206_schemaLocation && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@SchemaLocation = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                }
                else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id102_id && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Id = CollapseWhitespace(Reader.Value);
                    paramsRead[2] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_3 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.Xml.XmlAttribute)); a_3[ca_3++] = attr;
                }
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations86 = 0;
            int readerCount86 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[4] && ((object) Reader.LocalName == (object)id107_annotation && (object) Reader.NamespaceURI == (object)id95_Item)) {
                        o.@Annotation = Read11_XmlSchemaAnnotation(false, true);
                        paramsRead[4] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://www.w3.org/2001/XMLSchema:annotation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations86, ref readerCount86);
            }
            o.@UnhandledAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_3, ca_3, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        global::System.Web.Services.Description.Import Read4_Import(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object) ((System.Xml.XmlQualifiedName)xsiType).Name == (object)id209_Import && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::System.Web.Services.Description.Import o;
            o = new global::System.Web.Services.Description.Import();
            global::System.Xml.XmlAttribute[] a_1 = null;
            int ca_1 = 0;
            global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection a_3 = (global::System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection)o.@Extensions;
            bool[] paramsRead = new bool[6];
            while (Reader.MoveToNextAttribute()) {
                if (!paramsRead[4] && ((object) Reader.LocalName == (object)id36_namespace && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Namespace = Reader.Value;
                    paramsRead[4] = true;
                }
                else if (!paramsRead[5] && ((object) Reader.LocalName == (object)id23_location && (object) Reader.NamespaceURI == (object)id5_Item)) {
                    o.@Location = Reader.Value;
                    paramsRead[5] = true;
                }
                else if (IsXmlnsAttribute(Reader.Name)) {
                    if (o.@Namespaces == null) o.@Namespaces = new global::System.Xml.Serialization.XmlSerializerNamespaces();
                    ((global::System.Xml.Serialization.XmlSerializerNamespaces)o.@Namespaces).Add(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                }
                else {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);
                    ParseWsdlArrayType(attr);
                    a_1 = (global::System.Xml.XmlAttribute[])EnsureArrayIndex(a_1, ca_1, typeof(global::System.Xml.XmlAttribute)); a_1[ca_1++] = attr;
                }
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations87 = 0;
            int readerCount87 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id7_documentation && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        o.@DocumentationElement = (global::System.Xml.XmlElement)ReadXmlNode(false);
                        paramsRead[0] = true;
                    }
                    else {
                        a_3.Add((global::System.Xml.XmlElement)ReadXmlNode(false));
                    }
                }
                else {
                    UnknownNode((object)o, @"http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations87, ref readerCount87);
            }
            o.@ExtensibleAttributes = (global::System.Xml.XmlAttribute[])ShrinkArray(a_1, ca_1, typeof(global::System.Xml.XmlAttribute), true);
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks() {
        }

        string id133_XmlSchemaSimpleTypeUnion;
        string id143_maxInclusive;
        string id46_body;
        string id190_any;
        string id88_OperationOutput;
        string id6_targetNamespace;
        string id158_XmlSchemaMaxLengthFacet;
        string id11_portType;
        string id182_mixed;
        string id172_keyref;
        string id187_all;
        string id162_itemType;
        string id68_InputBinding;
        string id25_HttpAddressBinding;
        string id82_HttpBinding;
        string id17_address;
        string id3_ServiceDescription;
        string id38_SoapFaultBinding;
        string id123_ref;
        string id198_XmlSchemaComplexContent;
        string id53_parts;
        string id35_use;
        string id157_XmlSchemaLengthFacet;
        string id207_XmlSchemaImport;
        string id44_text;
        string id117_XmlSchemaAppInfo;
        string id203_public;
        string id69_urlEncoded;
        string id7_documentation;
        string id19_Item;
        string id129_final;
        string id163_XmlSchemaElement;
        string id60_capture;
        string id37_encodingStyle;
        string id185_sequence;
        string id166_abstract;
        string id23_location;
        string id111_XmlSchemaAttributeGroup;
        string id192_XmlSchemaSequence;
        string id33_FaultBinding;
        string id153_XmlSchemaMaxInclusiveFacet;
        string id201_XmlSchemaGroup;
        string id43_multipartRelated;
        string id168_nillable;
        string id149_value;
        string id64_MimeMultipartRelatedBinding;
        string id193_XmlSchemaAny;
        string id191_XmlSchemaGroupRef;
        string id74_soapAction;
        string id63_ignoreCase;
        string id101_version;
        string id47_header;
        string id195_extension;
        string id48_Soap12HeaderBinding;
        string id134_memberTypes;
        string id121_Item;
        string id146_minExclusive;
        string id84_PortType;
        string id42_mimeXml;
        string id138_minInclusive;
        string id118_source;
        string id73_Soap12OperationBinding;
        string id131_restriction;
        string id152_XmlSchemaMaxExclusiveFacet;
        string id135_XmlSchemaSimpleTypeRestriction;
        string id188_XmlSchemaAll;
        string id116_appinfo;
        string id86_parameterOrder;
        string id147_minLength;
        string id78_HttpOperationBinding;
        string id161_XmlSchemaSimpleTypeList;
        string id205_XmlSchemaRedefine;
        string id194_XmlSchemaSimpleContent;
        string id91_MessagePart;
        string id92_element;
        string id114_processContents;
        string id18_Item;
        string id50_headerfault;
        string id154_XmlSchemaEnumerationFacet;
        string id96_XmlSchema;
        string id127_form;
        string id176_field;
        string id49_part;
        string id5_Item;
        string id57_match;
        string id52_Soap12BodyBinding;
        string id104_redefine;
        string id20_Item;
        string id21_Soap12AddressBinding;
        string id142_enumeration;
        string id24_SoapAddressBinding;
        string id103_include;
        string id139_maxLength;
        string id165_maxOccurs;
        string id65_MimePart;
        string id102_id;
        string id196_Item;
        string id140_length;
        string id27_type;
        string id106_complexType;
        string id31_output;
        string id1_definitions;
        string id4_name;
        string id132_union;
        string id29_OperationBinding;
        string id170_key;
        string id45_Item;
        string id95_Item;
        string id169_substitutionGroup;
        string id178_xpath;
        string id9_types;
        string id97_attributeFormDefault;
        string id62_pattern;
        string id58_MimeTextMatch;
        string id180_XmlSchemaKey;
        string id10_message;
        string id8_import;
        string id148_XmlSchemaMinLengthFacet;
        string id105_simpleType;
        string id181_XmlSchemaComplexType;
        string id164_minOccurs;
        string id144_maxExclusive;
        string id160_XmlSchemaFractionDigitsFacet;
        string id124_XmlSchemaAttribute;
        string id209_Import;
        string id206_schemaLocation;
        string id179_XmlSchemaUnique;
        string id75_style;
        string id119_XmlSchemaDocumentation;
        string id136_base;
        string id66_MimeXmlBinding;
        string id30_input;
        string id40_content;
        string id93_Types;
        string id94_schema;
        string id200_Item;
        string id67_MimeContentBinding;
        string id59_group;
        string id32_fault;
        string id80_transport;
        string id98_blockDefault;
        string id13_service;
        string id54_SoapHeaderBinding;
        string id204_system;
        string id16_Port;
        string id108_notation;
        string id186_choice;
        string id110_attributeGroup;
        string id79_Soap12Binding;
        string id77_SoapOperationBinding;
        string id115_XmlSchemaAnnotation;
        string id83_verb;
        string id72_HttpUrlEncodedBinding;
        string id39_OutputBinding;
        string id183_complexContent;
        string id202_XmlSchemaNotation;
        string id81_SoapBinding;
        string id199_Item;
        string id28_operation;
        string id122_XmlSchemaAttributeGroupRef;
        string id155_XmlSchemaPatternFacet;
        string id76_soapActionRequired;
        string id90_Message;
        string id159_XmlSchemaMinInclusiveFacet;
        string id208_XmlSchemaInclude;
        string id85_Operation;
        string id130_list;
        string id14_Service;
        string id22_required;
        string id174_refer;
        string id71_HttpUrlReplacementBinding;
        string id56_MimeTextBinding;
        string id87_OperationFault;
        string id125_default;
        string id15_port;
        string id51_SoapHeaderFaultBinding;
        string id128_XmlSchemaSimpleType;
        string id36_namespace;
        string id175_selector;
        string id150_XmlSchemaMinExclusiveFacet;
        string id100_elementFormDefault;
        string id26_Binding;
        string id197_Item;
        string id126_fixed;
        string id107_annotation;
        string id99_finalDefault;
        string id137_fractionDigits;
        string id70_urlReplacement;
        string id189_XmlSchemaChoice;
        string id2_Item;
        string id112_anyAttribute;
        string id89_OperationInput;
        string id141_totalDigits;
        string id61_repeats;
        string id184_simpleContent;
        string id55_SoapBodyBinding;
        string id145_whiteSpace;
        string id167_block;
        string id151_XmlSchemaWhiteSpaceFacet;
        string id12_binding;
        string id109_attribute;
        string id171_unique;
        string id120_lang;
        string id173_XmlSchemaKeyref;
        string id177_XmlSchemaXPath;
        string id34_Soap12FaultBinding;
        string id41_Item;
        string id156_XmlSchemaTotalDigitsFacet;
        string id113_XmlSchemaAnyAttribute;

        protected override void InitIDs() {
            id133_XmlSchemaSimpleTypeUnion = Reader.NameTable.Add(@"XmlSchemaSimpleTypeUnion");
            id143_maxInclusive = Reader.NameTable.Add(@"maxInclusive");
            id46_body = Reader.NameTable.Add(@"body");
            id190_any = Reader.NameTable.Add(@"any");
            id88_OperationOutput = Reader.NameTable.Add(@"OperationOutput");
            id6_targetNamespace = Reader.NameTable.Add(@"targetNamespace");
            id158_XmlSchemaMaxLengthFacet = Reader.NameTable.Add(@"XmlSchemaMaxLengthFacet");
            id11_portType = Reader.NameTable.Add(@"portType");
            id182_mixed = Reader.NameTable.Add(@"mixed");
            id172_keyref = Reader.NameTable.Add(@"keyref");
            id187_all = Reader.NameTable.Add(@"all");
            id162_itemType = Reader.NameTable.Add(@"itemType");
            id68_InputBinding = Reader.NameTable.Add(@"InputBinding");
            id25_HttpAddressBinding = Reader.NameTable.Add(@"HttpAddressBinding");
            id82_HttpBinding = Reader.NameTable.Add(@"HttpBinding");
            id17_address = Reader.NameTable.Add(@"address");
            id3_ServiceDescription = Reader.NameTable.Add(@"ServiceDescription");
            id38_SoapFaultBinding = Reader.NameTable.Add(@"SoapFaultBinding");
            id123_ref = Reader.NameTable.Add(@"ref");
            id198_XmlSchemaComplexContent = Reader.NameTable.Add(@"XmlSchemaComplexContent");
            id53_parts = Reader.NameTable.Add(@"parts");
            id35_use = Reader.NameTable.Add(@"use");
            id157_XmlSchemaLengthFacet = Reader.NameTable.Add(@"XmlSchemaLengthFacet");
            id207_XmlSchemaImport = Reader.NameTable.Add(@"XmlSchemaImport");
            id44_text = Reader.NameTable.Add(@"text");
            id117_XmlSchemaAppInfo = Reader.NameTable.Add(@"XmlSchemaAppInfo");
            id203_public = Reader.NameTable.Add(@"public");
            id69_urlEncoded = Reader.NameTable.Add(@"urlEncoded");
            id7_documentation = Reader.NameTable.Add(@"documentation");
            id19_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/soap/");
            id129_final = Reader.NameTable.Add(@"final");
            id163_XmlSchemaElement = Reader.NameTable.Add(@"XmlSchemaElement");
            id60_capture = Reader.NameTable.Add(@"capture");
            id37_encodingStyle = Reader.NameTable.Add(@"encodingStyle");
            id185_sequence = Reader.NameTable.Add(@"sequence");
            id166_abstract = Reader.NameTable.Add(@"abstract");
            id23_location = Reader.NameTable.Add(@"location");
            id111_XmlSchemaAttributeGroup = Reader.NameTable.Add(@"XmlSchemaAttributeGroup");
            id192_XmlSchemaSequence = Reader.NameTable.Add(@"XmlSchemaSequence");
            id33_FaultBinding = Reader.NameTable.Add(@"FaultBinding");
            id153_XmlSchemaMaxInclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMaxInclusiveFacet");
            id201_XmlSchemaGroup = Reader.NameTable.Add(@"XmlSchemaGroup");
            id43_multipartRelated = Reader.NameTable.Add(@"multipartRelated");
            id168_nillable = Reader.NameTable.Add(@"nillable");
            id149_value = Reader.NameTable.Add(@"value");
            id64_MimeMultipartRelatedBinding = Reader.NameTable.Add(@"MimeMultipartRelatedBinding");
            id193_XmlSchemaAny = Reader.NameTable.Add(@"XmlSchemaAny");
            id191_XmlSchemaGroupRef = Reader.NameTable.Add(@"XmlSchemaGroupRef");
            id74_soapAction = Reader.NameTable.Add(@"soapAction");
            id63_ignoreCase = Reader.NameTable.Add(@"ignoreCase");
            id101_version = Reader.NameTable.Add(@"version");
            id47_header = Reader.NameTable.Add(@"header");
            id195_extension = Reader.NameTable.Add(@"extension");
            id48_Soap12HeaderBinding = Reader.NameTable.Add(@"Soap12HeaderBinding");
            id134_memberTypes = Reader.NameTable.Add(@"memberTypes");
            id121_Item = Reader.NameTable.Add(@"http://www.w3.org/XML/1998/namespace");
            id146_minExclusive = Reader.NameTable.Add(@"minExclusive");
            id84_PortType = Reader.NameTable.Add(@"PortType");
            id42_mimeXml = Reader.NameTable.Add(@"mimeXml");
            id138_minInclusive = Reader.NameTable.Add(@"minInclusive");
            id118_source = Reader.NameTable.Add(@"source");
            id73_Soap12OperationBinding = Reader.NameTable.Add(@"Soap12OperationBinding");
            id131_restriction = Reader.NameTable.Add(@"restriction");
            id152_XmlSchemaMaxExclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMaxExclusiveFacet");
            id135_XmlSchemaSimpleTypeRestriction = Reader.NameTable.Add(@"XmlSchemaSimpleTypeRestriction");
            id188_XmlSchemaAll = Reader.NameTable.Add(@"XmlSchemaAll");
            id116_appinfo = Reader.NameTable.Add(@"appinfo");
            id86_parameterOrder = Reader.NameTable.Add(@"parameterOrder");
            id147_minLength = Reader.NameTable.Add(@"minLength");
            id78_HttpOperationBinding = Reader.NameTable.Add(@"HttpOperationBinding");
            id161_XmlSchemaSimpleTypeList = Reader.NameTable.Add(@"XmlSchemaSimpleTypeList");
            id205_XmlSchemaRedefine = Reader.NameTable.Add(@"XmlSchemaRedefine");
            id194_XmlSchemaSimpleContent = Reader.NameTable.Add(@"XmlSchemaSimpleContent");
            id91_MessagePart = Reader.NameTable.Add(@"MessagePart");
            id92_element = Reader.NameTable.Add(@"element");
            id114_processContents = Reader.NameTable.Add(@"processContents");
            id18_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/http/");
            id50_headerfault = Reader.NameTable.Add(@"headerfault");
            id154_XmlSchemaEnumerationFacet = Reader.NameTable.Add(@"XmlSchemaEnumerationFacet");
            id96_XmlSchema = Reader.NameTable.Add(@"XmlSchema");
            id127_form = Reader.NameTable.Add(@"form");
            id176_field = Reader.NameTable.Add(@"field");
            id49_part = Reader.NameTable.Add(@"part");
            id5_Item = Reader.NameTable.Add(@"");
            id57_match = Reader.NameTable.Add(@"match");
            id52_Soap12BodyBinding = Reader.NameTable.Add(@"Soap12BodyBinding");
            id104_redefine = Reader.NameTable.Add(@"redefine");
            id20_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/soap12/");
            id21_Soap12AddressBinding = Reader.NameTable.Add(@"Soap12AddressBinding");
            id142_enumeration = Reader.NameTable.Add(@"enumeration");
            id24_SoapAddressBinding = Reader.NameTable.Add(@"SoapAddressBinding");
            id103_include = Reader.NameTable.Add(@"include");
            id139_maxLength = Reader.NameTable.Add(@"maxLength");
            id165_maxOccurs = Reader.NameTable.Add(@"maxOccurs");
            id65_MimePart = Reader.NameTable.Add(@"MimePart");
            id102_id = Reader.NameTable.Add(@"id");
            id196_Item = Reader.NameTable.Add(@"XmlSchemaSimpleContentExtension");
            id140_length = Reader.NameTable.Add(@"length");
            id27_type = Reader.NameTable.Add(@"type");
            id106_complexType = Reader.NameTable.Add(@"complexType");
            id31_output = Reader.NameTable.Add(@"output");
            id1_definitions = Reader.NameTable.Add(@"definitions");
            id4_name = Reader.NameTable.Add(@"name");
            id132_union = Reader.NameTable.Add(@"union");
            id29_OperationBinding = Reader.NameTable.Add(@"OperationBinding");
            id170_key = Reader.NameTable.Add(@"key");
            id45_Item = Reader.NameTable.Add(@"http://microsoft.com/wsdl/mime/textMatching/");
            id95_Item = Reader.NameTable.Add(@"http://www.w3.org/2001/XMLSchema");
            id169_substitutionGroup = Reader.NameTable.Add(@"substitutionGroup");
            id178_xpath = Reader.NameTable.Add(@"xpath");
            id9_types = Reader.NameTable.Add(@"types");
            id97_attributeFormDefault = Reader.NameTable.Add(@"attributeFormDefault");
            id62_pattern = Reader.NameTable.Add(@"pattern");
            id58_MimeTextMatch = Reader.NameTable.Add(@"MimeTextMatch");
            id180_XmlSchemaKey = Reader.NameTable.Add(@"XmlSchemaKey");
            id10_message = Reader.NameTable.Add(@"message");
            id8_import = Reader.NameTable.Add(@"import");
            id148_XmlSchemaMinLengthFacet = Reader.NameTable.Add(@"XmlSchemaMinLengthFacet");
            id105_simpleType = Reader.NameTable.Add(@"simpleType");
            id181_XmlSchemaComplexType = Reader.NameTable.Add(@"XmlSchemaComplexType");
            id164_minOccurs = Reader.NameTable.Add(@"minOccurs");
            id144_maxExclusive = Reader.NameTable.Add(@"maxExclusive");
            id160_XmlSchemaFractionDigitsFacet = Reader.NameTable.Add(@"XmlSchemaFractionDigitsFacet");
            id124_XmlSchemaAttribute = Reader.NameTable.Add(@"XmlSchemaAttribute");
            id209_Import = Reader.NameTable.Add(@"Import");
            id206_schemaLocation = Reader.NameTable.Add(@"schemaLocation");
            id179_XmlSchemaUnique = Reader.NameTable.Add(@"XmlSchemaUnique");
            id75_style = Reader.NameTable.Add(@"style");
            id119_XmlSchemaDocumentation = Reader.NameTable.Add(@"XmlSchemaDocumentation");
            id136_base = Reader.NameTable.Add(@"base");
            id66_MimeXmlBinding = Reader.NameTable.Add(@"MimeXmlBinding");
            id30_input = Reader.NameTable.Add(@"input");
            id40_content = Reader.NameTable.Add(@"content");
            id93_Types = Reader.NameTable.Add(@"Types");
            id94_schema = Reader.NameTable.Add(@"schema");
            id200_Item = Reader.NameTable.Add(@"XmlSchemaComplexContentExtension");
            id67_MimeContentBinding = Reader.NameTable.Add(@"MimeContentBinding");
            id59_group = Reader.NameTable.Add(@"group");
            id32_fault = Reader.NameTable.Add(@"fault");
            id80_transport = Reader.NameTable.Add(@"transport");
            id98_blockDefault = Reader.NameTable.Add(@"blockDefault");
            id13_service = Reader.NameTable.Add(@"service");
            id54_SoapHeaderBinding = Reader.NameTable.Add(@"SoapHeaderBinding");
            id204_system = Reader.NameTable.Add(@"system");
            id16_Port = Reader.NameTable.Add(@"Port");
            id108_notation = Reader.NameTable.Add(@"notation");
            id186_choice = Reader.NameTable.Add(@"choice");
            id110_attributeGroup = Reader.NameTable.Add(@"attributeGroup");
            id79_Soap12Binding = Reader.NameTable.Add(@"Soap12Binding");
            id77_SoapOperationBinding = Reader.NameTable.Add(@"SoapOperationBinding");
            id115_XmlSchemaAnnotation = Reader.NameTable.Add(@"XmlSchemaAnnotation");
            id83_verb = Reader.NameTable.Add(@"verb");
            id72_HttpUrlEncodedBinding = Reader.NameTable.Add(@"HttpUrlEncodedBinding");
            id39_OutputBinding = Reader.NameTable.Add(@"OutputBinding");
            id183_complexContent = Reader.NameTable.Add(@"complexContent");
            id202_XmlSchemaNotation = Reader.NameTable.Add(@"XmlSchemaNotation");
            id81_SoapBinding = Reader.NameTable.Add(@"SoapBinding");
            id199_Item = Reader.NameTable.Add(@"XmlSchemaComplexContentRestriction");
            id28_operation = Reader.NameTable.Add(@"operation");
            id122_XmlSchemaAttributeGroupRef = Reader.NameTable.Add(@"XmlSchemaAttributeGroupRef");
            id155_XmlSchemaPatternFacet = Reader.NameTable.Add(@"XmlSchemaPatternFacet");
            id76_soapActionRequired = Reader.NameTable.Add(@"soapActionRequired");
            id90_Message = Reader.NameTable.Add(@"Message");
            id159_XmlSchemaMinInclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMinInclusiveFacet");
            id208_XmlSchemaInclude = Reader.NameTable.Add(@"XmlSchemaInclude");
            id85_Operation = Reader.NameTable.Add(@"Operation");
            id130_list = Reader.NameTable.Add(@"list");
            id14_Service = Reader.NameTable.Add(@"Service");
            id22_required = Reader.NameTable.Add(@"required");
            id174_refer = Reader.NameTable.Add(@"refer");
            id71_HttpUrlReplacementBinding = Reader.NameTable.Add(@"HttpUrlReplacementBinding");
            id56_MimeTextBinding = Reader.NameTable.Add(@"MimeTextBinding");
            id87_OperationFault = Reader.NameTable.Add(@"OperationFault");
            id125_default = Reader.NameTable.Add(@"default");
            id15_port = Reader.NameTable.Add(@"port");
            id51_SoapHeaderFaultBinding = Reader.NameTable.Add(@"SoapHeaderFaultBinding");
            id128_XmlSchemaSimpleType = Reader.NameTable.Add(@"XmlSchemaSimpleType");
            id36_namespace = Reader.NameTable.Add(@"namespace");
            id175_selector = Reader.NameTable.Add(@"selector");
            id150_XmlSchemaMinExclusiveFacet = Reader.NameTable.Add(@"XmlSchemaMinExclusiveFacet");
            id100_elementFormDefault = Reader.NameTable.Add(@"elementFormDefault");
            id26_Binding = Reader.NameTable.Add(@"Binding");
            id197_Item = Reader.NameTable.Add(@"XmlSchemaSimpleContentRestriction");
            id126_fixed = Reader.NameTable.Add(@"fixed");
            id107_annotation = Reader.NameTable.Add(@"annotation");
            id99_finalDefault = Reader.NameTable.Add(@"finalDefault");
            id137_fractionDigits = Reader.NameTable.Add(@"fractionDigits");
            id70_urlReplacement = Reader.NameTable.Add(@"urlReplacement");
            id189_XmlSchemaChoice = Reader.NameTable.Add(@"XmlSchemaChoice");
            id2_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/");
            id112_anyAttribute = Reader.NameTable.Add(@"anyAttribute");
            id89_OperationInput = Reader.NameTable.Add(@"OperationInput");
            id141_totalDigits = Reader.NameTable.Add(@"totalDigits");
            id61_repeats = Reader.NameTable.Add(@"repeats");
            id184_simpleContent = Reader.NameTable.Add(@"simpleContent");
            id55_SoapBodyBinding = Reader.NameTable.Add(@"SoapBodyBinding");
            id145_whiteSpace = Reader.NameTable.Add(@"whiteSpace");
            id167_block = Reader.NameTable.Add(@"block");
            id151_XmlSchemaWhiteSpaceFacet = Reader.NameTable.Add(@"XmlSchemaWhiteSpaceFacet");
            id12_binding = Reader.NameTable.Add(@"binding");
            id109_attribute = Reader.NameTable.Add(@"attribute");
            id171_unique = Reader.NameTable.Add(@"unique");
            id120_lang = Reader.NameTable.Add(@"lang");
            id173_XmlSchemaKeyref = Reader.NameTable.Add(@"XmlSchemaKeyref");
            id177_XmlSchemaXPath = Reader.NameTable.Add(@"XmlSchemaXPath");
            id34_Soap12FaultBinding = Reader.NameTable.Add(@"Soap12FaultBinding");
            id41_Item = Reader.NameTable.Add(@"http://schemas.xmlsoap.org/wsdl/mime/");
            id156_XmlSchemaTotalDigitsFacet = Reader.NameTable.Add(@"XmlSchemaTotalDigitsFacet");
            id113_XmlSchemaAnyAttribute = Reader.NameTable.Add(@"XmlSchemaAnyAttribute");
        }
    }
}
