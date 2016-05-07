//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Xml;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using System.ComponentModel;

    public class MessageHeaderDescription : MessagePartDescription
    {
        bool mustUnderstand;
        bool relay;
        string actor;
        bool typedHeader;
        bool isUnknownHeader;

        public MessageHeaderDescription(string name, string ns)
            : base(name, ns)
        {

        }

        internal MessageHeaderDescription(MessageHeaderDescription other)
            : base(other)
        {
            this.MustUnderstand = other.MustUnderstand;
            this.Relay = other.Relay;
            this.Actor = other.Actor;
            this.TypedHeader = other.TypedHeader;
            this.IsUnknownHeaderCollection = other.IsUnknownHeaderCollection;
        }

        internal override MessagePartDescription Clone()
        {
            return new MessageHeaderDescription(this);
        }

        [DefaultValue(null)]
        public string Actor
        {
            get { return this.actor; }
            set { this.actor = value; }
        }

        [DefaultValue(false)]
        public bool MustUnderstand
        {
            get { return this.mustUnderstand; }
            set { this.mustUnderstand = value; }
        }

        [DefaultValue(false)]
        public bool Relay
        {
            get { return this.relay; }
            set { this.relay = value; }
        }

        [DefaultValue(false)]
        public bool TypedHeader
        {
            get { return this.typedHeader; }
            set { this.typedHeader = value; }
        }

        internal bool IsUnknownHeaderCollection
        {
            get
            {
                return isUnknownHeader || Multiple && (Type == typeof(XmlElement));
            }
            set
            {
                isUnknownHeader = value;
            }
        }
    }
}
