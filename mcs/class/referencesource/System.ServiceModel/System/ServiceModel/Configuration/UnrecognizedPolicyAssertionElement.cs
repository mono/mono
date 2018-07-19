//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;

    sealed partial class UnrecognizedPolicyAssertionElement : BindingElementExtensionElement
    {
        XmlQualifiedName wsdlBinding;
        ICollection<XmlElement> bindingAsserions;
        IDictionary<OperationDescription, ICollection<XmlElement>> operationAssertions;
        IDictionary<MessageDescription, ICollection<XmlElement>> messageAssertions;


        public override Type BindingElementType
        {
            get { return typeof(UnrecognizedAssertionsBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            UnrecognizedPolicyAssertionElement source = (UnrecognizedPolicyAssertionElement)from;
#pragma warning suppress 56506 //markg; base.CopyFrom() checks for 'from' being null
            this.wsdlBinding = source.wsdlBinding;
            this.bindingAsserions = source.bindingAsserions;
            this.operationAssertions = source.operationAssertions;
            this.messageAssertions = source.messageAssertions;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return new UnrecognizedAssertionsBindingElement(XmlQualifiedName.Empty, null);
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            UnrecognizedAssertionsBindingElement binding = (UnrecognizedAssertionsBindingElement)bindingElement;

            this.wsdlBinding = binding.WsdlBinding;
            this.bindingAsserions = binding.BindingAsserions;
            this.operationAssertions = binding.OperationAssertions;
            this.messageAssertions = binding.MessageAssertions;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, String elementName)
        {
            XmlDocument document = new XmlDocument();
            if (writer != null && this.bindingAsserions != null && this.bindingAsserions.Count > 0)
            {
                int indent = 1;
                XmlWriterSettings settings = WriterSettings(writer);
                Fx.Assert(this.wsdlBinding != null, "");
                WriteComment(SR.GetString(SR.UnrecognizedBindingAssertions1, this.wsdlBinding.Namespace), indent, writer, settings);
                WriteComment(String.Format(CultureInfo.InvariantCulture, "<wsdl:binding name='{0}'>", this.wsdlBinding.Name), indent, writer, settings);
                indent++;
                foreach (XmlElement assertion in this.bindingAsserions)
                {
                    WriteComment(ToString(assertion, document), indent, writer, settings);
                }
                if (this.operationAssertions == null || this.operationAssertions.Count == 0)
                    return true;

                foreach (OperationDescription operation in this.operationAssertions.Keys)
                {
                    WriteComment(String.Format(CultureInfo.InvariantCulture, "<wsdl:operation name='{0}'>", operation.Name), indent, writer, settings);
                    indent++;
                    foreach (XmlElement assertion in this.operationAssertions[operation])
                    {
                        WriteComment(ToString(assertion, document), indent, writer, settings);
                    }

                    if (this.messageAssertions == null || this.messageAssertions.Count == 0)
                        return true;

                    foreach (MessageDescription message in operation.Messages)
                    {
                        ICollection<XmlElement> assertions;
                        if (this.messageAssertions.TryGetValue(message, out assertions))
                        {
                            if (message.Direction == MessageDirection.Input)
                                WriteComment("<wsdl:input>", indent, writer, settings);
                            else if (message.Direction == MessageDirection.Output)
                                WriteComment("<wsdl:output>", indent, writer, settings);
                            foreach (XmlElement assertion in assertions)
                            {
                                WriteComment(ToString(assertion, document), indent + 1, writer, settings);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement is UnrecognizedPolicyAssertionElement)
            {
                this.wsdlBinding = ((UnrecognizedPolicyAssertionElement)sourceElement).wsdlBinding;
                this.bindingAsserions = ((UnrecognizedPolicyAssertionElement)sourceElement).bindingAsserions;
                this.operationAssertions = ((UnrecognizedPolicyAssertionElement)sourceElement).operationAssertions;
                this.messageAssertions = ((UnrecognizedPolicyAssertionElement)sourceElement).messageAssertions;
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        string ToString(XmlElement e, XmlDocument document)
        {
            XmlElement top = document.CreateElement(e.Prefix, e.LocalName, e.NamespaceURI);
            top.InsertBefore(document.CreateTextNode(".."), null);
            return top.OuterXml;
        }

        void WriteComment(string text, int indent, XmlWriter writer, XmlWriterSettings settings)
        {
            if (settings.Indent)
            {
                // indent is always > 0
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++)
                {
                    sb.Append(settings.IndentChars);
                }
                sb.Append(text);
                sb.Append(settings.IndentChars);
                text = sb.ToString();
            }
            writer.WriteComment(text);
        }

        XmlWriterSettings WriterSettings(XmlWriter writer)
        {
            if (writer.Settings == null)
            {
                // V1 writers
                XmlWriterSettings settings = new XmlWriterSettings();
                XmlTextWriter xmlTextWriter = writer as XmlTextWriter;
                if (xmlTextWriter != null)
                {
                    settings.Indent = xmlTextWriter.Formatting == Formatting.Indented;
                    if (settings.Indent && xmlTextWriter.Indentation > 0)
                    {
                        StringBuilder sb = new StringBuilder(xmlTextWriter.Indentation);
                        for (int i = 0; i < xmlTextWriter.Indentation; i++)
                            sb.Append(xmlTextWriter.IndentChar);
                        settings.IndentChars = sb.ToString();
                    }
                }
                return settings;
            }
            return writer.Settings;
        }
    }
}



