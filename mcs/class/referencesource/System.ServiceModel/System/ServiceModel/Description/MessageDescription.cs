//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.Net.Security;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    [DebuggerDisplay("Action={action}, Direction={direction}, MessageType={messageType}")]
    public class MessageDescription
    {
        static Type typeOfUntypedMessage;
        string action;
        MessageDirection direction;
        MessageDescriptionItems items;
        XmlName messageName;
        Type messageType;
        XmlQualifiedName xsdType;
        ProtectionLevel protectionLevel;
        bool hasProtectionLevel;

        public MessageDescription(string action, MessageDirection direction) : this(action, direction, null) { }
        internal MessageDescription(string action, MessageDirection direction, MessageDescriptionItems items)
        {
            if (!MessageDirectionHelper.IsDefined(direction))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("direction"));

            this.action = action;
            this.direction = direction;
            this.items = items;
        }

        internal MessageDescription(MessageDescription other)
        {
            this.action = other.action;
            this.direction = other.direction;
            this.Items.Body = other.Items.Body.Clone();
            foreach (MessageHeaderDescription mhd in other.Items.Headers)
            {
                this.Items.Headers.Add(mhd.Clone() as MessageHeaderDescription);
            }
            foreach (MessagePropertyDescription mpd in other.Items.Properties)
            {
                this.Items.Properties.Add(mpd.Clone() as MessagePropertyDescription);
            }
            this.MessageName = other.MessageName;
            this.MessageType = other.MessageType;
            this.XsdTypeName = other.XsdTypeName;
            this.hasProtectionLevel = other.hasProtectionLevel;
            this.ProtectionLevel = other.ProtectionLevel;
        }

        internal MessageDescription Clone()
        {
            return new MessageDescription(this);
        }

        public string Action
        {
            get { return action; }
            internal set { action = value; }
        }
        
        public MessageBodyDescription Body
        {
            get { return Items.Body; }
        }

        public MessageDirection Direction
        {
            get { return direction; }
        }

        public MessageHeaderDescriptionCollection Headers
        {
            get { return Items.Headers; }
        }

        public MessagePropertyDescriptionCollection Properties
        {
            get { return Items.Properties; }
        }

        internal MessageDescriptionItems Items
        {
            get
            {
                if (items == null)
                    items = new MessageDescriptionItems();
                return items;
            }
        }

        public ProtectionLevel ProtectionLevel
        {
            get { return this.protectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool ShouldSerializeProtectionLevel()
        {
            return this.HasProtectionLevel;
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        internal static Type TypeOfUntypedMessage
        {
            get
            {
                if (typeOfUntypedMessage == null)
                {
                    typeOfUntypedMessage = typeof(Message);
                }
                return typeOfUntypedMessage;
            }
        }
        
        internal XmlName MessageName
        {
            get { return messageName; }
            set { messageName = value; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        [DefaultValue(null)]
        public Type MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        internal bool IsTypedMessage
        {
            get
            {
                return messageType != null;
            }
        }

        internal bool IsUntypedMessage
        {
            get
            {
                return (Body.ReturnValue != null && Body.Parts.Count == 0 && Body.ReturnValue.Type == TypeOfUntypedMessage) ||
                     (Body.ReturnValue == null && Body.Parts.Count == 1 && Body.Parts[0].Type == TypeOfUntypedMessage);
            }
        }

        internal bool IsVoid
        {
            get
            {
                return !IsTypedMessage && Body.Parts.Count == 0 && (Body.ReturnValue == null || Body.ReturnValue.Type == typeof(void));
            }
        }

        internal XmlQualifiedName XsdTypeName
        {
            get { return xsdType; }
            set { xsdType = value; }
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }
    }

    internal class MessageDescriptionItems
    {
        MessageHeaderDescriptionCollection headers;
        MessageBodyDescription body;
        MessagePropertyDescriptionCollection properties;

        internal MessageBodyDescription Body
        {
            get
            {
                if (body == null)
                    body = new MessageBodyDescription();
                return body;
            }
            set
            {
                this.body = value;
            }
        }

        internal MessageHeaderDescriptionCollection Headers
        {
            get
            {
                if (headers == null)
                    headers = new MessageHeaderDescriptionCollection();
                return headers;
            }
        }

        internal MessagePropertyDescriptionCollection Properties
        {
            get
            {
                if (properties == null)
                    properties = new MessagePropertyDescriptionCollection();
                return properties;
            }
        }
    }
}
