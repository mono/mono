//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Channels;

    abstract class DelegatingMessage : Message
    {
        Message innerMessage;

        protected DelegatingMessage(Message innerMessage)
        {
            if (innerMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerMessage");
            }
            this.innerMessage = innerMessage;
        }

        public override bool IsEmpty
        {
            get
            {
                return this.innerMessage.IsEmpty;
            }
        }

        public override bool IsFault
        {
            get { return this.innerMessage.IsFault; }
        }

        public override MessageHeaders Headers
        {
            get { return this.innerMessage.Headers; }
        }

        public override MessageProperties Properties
        {
            get { return this.innerMessage.Properties; }
        }

        public override MessageVersion Version
        {
            get { return this.innerMessage.Version; }
        }

        protected Message InnerMessage
        {
            get { return this.innerMessage; }
        }

        protected override void OnClose()
        {
            base.OnClose();
            this.innerMessage.Close();
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartEnvelope(writer);
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartHeaders(writer);
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartBody(writer);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteBodyContents(writer);
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            return this.innerMessage.GetBodyAttribute(localName, ns);
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            this.innerMessage.BodyToString(writer);
        }
    }
}
