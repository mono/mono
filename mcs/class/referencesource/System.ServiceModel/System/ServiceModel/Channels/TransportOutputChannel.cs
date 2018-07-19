//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.Xml;
    using System.Runtime.Diagnostics;
    using System.Runtime;

    abstract class TransportOutputChannel : OutputChannel
    {
        bool anyHeadersToAdd;
        bool manualAddressing;
        MessageVersion messageVersion;
        EndpointAddress to;
        Uri via;
        ToHeader toHeader;
        EventTraceActivity channelEventTraceActivity;

        protected TransportOutputChannel(ChannelManagerBase channelManager, EndpointAddress to, Uri via, bool manualAddressing, MessageVersion messageVersion)
            : base(channelManager)
        {
            this.manualAddressing = manualAddressing;
            this.messageVersion = messageVersion;
            this.to = to;
            this.via = via;

            if (!manualAddressing && to != null)
            {
                Uri toUri;
                if (to.IsAnonymous)
                {
                    toUri = this.messageVersion.Addressing.AnonymousUri;
                }
                else if (to.IsNone)
                {
                    toUri = this.messageVersion.Addressing.NoneUri;
                }
                else
                {
                    toUri = to.Uri;
                }

                if (toUri != null)
                {
                    XmlDictionaryString dictionaryTo = new ToDictionary(toUri.AbsoluteUri).To;
                    this.toHeader = ToHeader.Create(toUri, dictionaryTo, messageVersion.Addressing);
                }

                this.anyHeadersToAdd = to.Headers.Count > 0;
            }

            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                this.channelEventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
            }
        }

        protected bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.to;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.via;
            }
        }

        public EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.channelEventTraceActivity;
            }
        }

        protected override void AddHeadersTo(Message message)
        {
            base.AddHeadersTo(message);

            if (toHeader != null)
            {
                // we don't use to.ApplyTo(message) since it's faster to cache and
                // use the actual <To> header then to call message.Headers.To = Uri...
                message.Headers.SetToHeader(toHeader);
                if (anyHeadersToAdd)
                {
                    to.Headers.AddHeadersTo(message);
                }
            }
        }

        class ToDictionary : IXmlDictionary
        {
            XmlDictionaryString to;

            public ToDictionary(string to)
            {
                this.to = new XmlDictionaryString(this, to, 0);
            }

            public XmlDictionaryString To
            {
                get
                {
                    return to;
                }
            }

            public bool TryLookup(string value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                if (value == to.Value)
                {
                    result = to;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                if (key == 0)
                {
                    result = to;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                if (value == to)
                {
                    result = to;
                    return true;
                }
                result = null;
                return false;
            }
        }
    }
}
