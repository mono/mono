//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.ServiceModel.Security;
    using System.Xml;

    public class ScopedMessagePartSpecification
    {
        MessagePartSpecification channelParts;
        Dictionary<string, MessagePartSpecification> actionParts;
        Dictionary<string, MessagePartSpecification> readOnlyNormalizedActionParts;
        bool isReadOnly;

        public ScopedMessagePartSpecification()
        {
            this.channelParts = new MessagePartSpecification();
            this.actionParts = new Dictionary<string, MessagePartSpecification>();
        }

        public ICollection<string> Actions
        {
            get
            {
                return this.actionParts.Keys;
            }
        }

        public MessagePartSpecification ChannelParts
        {
            get
            {
                return this.channelParts;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public ScopedMessagePartSpecification(ScopedMessagePartSpecification other)
            : this()
        {
            if (other == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("other"));

            this.channelParts.Union(other.channelParts);
            if (other.actionParts != null)
            {
                foreach (string action in other.actionParts.Keys)
                {
                    MessagePartSpecification p = new MessagePartSpecification();
                    p.Union(other.actionParts[action]);
                    this.actionParts[action] = p;
                }
            }
        }

        internal ScopedMessagePartSpecification(ScopedMessagePartSpecification other, bool newIncludeBody)
            : this(other)
        {
            this.channelParts.IsBodyIncluded = newIncludeBody;
            foreach (string action in this.actionParts.Keys)
                this.actionParts[action].IsBodyIncluded = newIncludeBody;
        }

        public void AddParts(MessagePartSpecification parts)
        {
            if (parts == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts"));

            ThrowIfReadOnly();

            this.channelParts.Union(parts);
        }

        public void AddParts(MessagePartSpecification parts, string action)
        {
            if (action == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            if (parts == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts"));

            ThrowIfReadOnly();

            if (!this.actionParts.ContainsKey(action))
                this.actionParts[action] = new MessagePartSpecification();
            this.actionParts[action].Union(parts);
        }

        internal void AddParts(MessagePartSpecification parts, XmlDictionaryString action)
        {
            if (action == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            AddParts(parts, action.Value);
        }

        internal bool IsEmpty()
        {
            bool result;
            if (!channelParts.IsEmpty())
            {
                result = false;
            }
            else
            {
                result = true;
                foreach (string action in this.Actions)
                {
                    MessagePartSpecification parts;
                    if (TryGetParts(action, true, out parts))
                    {
                        if (!parts.IsEmpty())
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;   
        }

        public bool TryGetParts(string action, bool excludeChannelScope, out MessagePartSpecification parts)
        {
            if (action == null)
                action = MessageHeaders.WildcardAction;
            parts = null;

            if (this.isReadOnly)
            {
                if (this.readOnlyNormalizedActionParts.ContainsKey(action))
                    if (excludeChannelScope)
                        parts = this.actionParts[action];
                    else
                        parts = this.readOnlyNormalizedActionParts[action];
            }
            else if (this.actionParts.ContainsKey(action))
            {
                MessagePartSpecification p = new MessagePartSpecification();
                p.Union(this.actionParts[action]);
                if (!excludeChannelScope)
                    p.Union(this.channelParts);
                parts = p;
            }

            return parts != null;
        }

        internal void CopyTo(ScopedMessagePartSpecification target)
        {
            if (target == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("target");
            }
            target.ChannelParts.IsBodyIncluded = this.ChannelParts.IsBodyIncluded;
            foreach (XmlQualifiedName headerType in ChannelParts.HeaderTypes)
            {
                if (!target.channelParts.IsHeaderIncluded(headerType.Name, headerType.Namespace))
                {
                    target.ChannelParts.HeaderTypes.Add(headerType);
                }
            }
            foreach (string action in this.actionParts.Keys)
            {
                target.AddParts(this.actionParts[action], action);
            }
        }

        public bool TryGetParts(string action, out MessagePartSpecification parts)
        {
            return this.TryGetParts(action, false, out parts);
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.readOnlyNormalizedActionParts = new Dictionary<string, MessagePartSpecification>();
                foreach (string action in this.actionParts.Keys)
                {
                    MessagePartSpecification p = new MessagePartSpecification();
                    p.Union(this.actionParts[action]);
                    p.Union(this.channelParts);
                    p.MakeReadOnly();
                    this.readOnlyNormalizedActionParts[action] = p;
                }
                this.isReadOnly = true;
            }
        }

        void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
    }
}
