//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.IO;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Base class for non-SOAP messages
    /// </summary>
    abstract class ContentOnlyMessage : Message
    {
        MessageHeaders headers;
        MessageProperties properties;

        protected ContentOnlyMessage()
        {
            this.headers = new MessageHeaders(MessageVersion.None);
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
                {
#pragma warning suppress 56503 // [....], required by base class contract
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                }

                return this.headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (IsDisposed)
                {
#pragma warning suppress 56503 // [....], required by base class contract
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                }

                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }

                return this.properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                return headers.MessageVersion;
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            OnWriteBodyContents(writer);
        }
    }

    class StringMessage : ContentOnlyMessage
    {
        string data;

        public StringMessage(string data)
            : base()
        {
            this.data = data;
        }

        public override bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(this.data);
            }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (data != null && data.Length > 0)
            {
                writer.WriteElementString("BODY", data);
            }
        }
    }

    class NullMessage : StringMessage
    {
        public NullMessage()
            : base(string.Empty)
        {
        }
    }
}
