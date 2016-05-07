//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Globalization;
    using System.Xml;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;

    class WSAddressing10ProblemHeaderQNameFault : MessageFault
    {
        FaultCode code;
        FaultReason reason;
        string actor;
        string node;
        string invalidHeaderName;

        public WSAddressing10ProblemHeaderQNameFault(MessageHeaderException e)
        {
            this.invalidHeaderName = e.HeaderName;

            if (e.IsDuplicate)
            {
                this.code = FaultCode.CreateSenderFaultCode(
                    new FaultCode(Addressing10Strings.InvalidAddressingHeader,
                                  AddressingVersion.WSAddressing10.Namespace,
                                  new FaultCode(Addressing10Strings.InvalidCardinality,
                                                AddressingVersion.WSAddressing10.Namespace)));
            }
            else
            {
                this.code = FaultCode.CreateSenderFaultCode(
                    new FaultCode(Addressing10Strings.MessageAddressingHeaderRequired,
                                  AddressingVersion.WSAddressing10.Namespace));
            }

            this.reason = new FaultReason(e.Message, CultureInfo.CurrentCulture);
            this.actor = "";
            this.node = "";
        }

        public WSAddressing10ProblemHeaderQNameFault(ActionMismatchAddressingException e)
        {
            this.invalidHeaderName = AddressingStrings.Action;
            this.code = FaultCode.CreateSenderFaultCode(
                new FaultCode(Addressing10Strings.ActionMismatch, AddressingVersion.WSAddressing10.Namespace));
            this.reason = new FaultReason(e.Message, CultureInfo.CurrentCulture);
            this.actor = "";
            this.node = "";
        }

        public override string Actor
        {
            get
            {
                return actor;
            }
        }

        public override FaultCode Code
        {
            get
            {
                return code;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return true;
            }
        }

        public override string Node
        {
            get
            {
                return node;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return reason;
            }
        }

        protected override void OnWriteDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            if (version == EnvelopeVersion.Soap12)  // Soap11 wants the detail in the header
            {
                OnWriteStartDetail(writer, version);
                OnWriteDetailContents(writer);
                writer.WriteEndElement();
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(Addressing10Strings.ProblemHeaderQName, AddressingVersion.WSAddressing10.Namespace);
            writer.WriteQualifiedName(this.invalidHeaderName, AddressingVersion.WSAddressing10.Namespace);
            writer.WriteEndElement();
        }

        public void AddHeaders(MessageHeaders headers)
        {
            if (headers.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                headers.Add(new WSAddressing10ProblemHeaderQNameHeader(this.invalidHeaderName));
            }
        }

        class WSAddressing10ProblemHeaderQNameHeader : MessageHeader
        {
            string invalidHeaderName;

            public WSAddressing10ProblemHeaderQNameHeader(string invalidHeaderName)
            {
                this.invalidHeaderName = invalidHeaderName;
            }

            public override string Name
            {
                get { return Addressing10Strings.FaultDetail; }
            }

            public override string Namespace
            {
                get { return AddressingVersion.WSAddressing10.Namespace; }
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(this.Name, this.Namespace);
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(Addressing10Strings.ProblemHeaderQName, this.Namespace);
                writer.WriteQualifiedName(this.invalidHeaderName, this.Namespace);
                writer.WriteEndElement();
            }
        }
    }
}
