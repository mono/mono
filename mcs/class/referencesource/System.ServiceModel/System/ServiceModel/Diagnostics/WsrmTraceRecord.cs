//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;
    
    class WsrmTraceRecord : TraceRecord
    {
        UniqueId id;

        internal WsrmTraceRecord(UniqueId id)            
        {
            this.id = id;
        }

        internal override string EventId { get { return BuildEventId("Sequence"); } }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Identifier");            
            writer.WriteString(id.ToString());
            writer.WriteEndElement();
        }
    }

    class ReliableChannelTraceRecord : ChannelTraceRecord
    {
        UniqueId id;

        internal ReliableChannelTraceRecord(IChannel channel, UniqueId id) : base(channel)
        {
            this.id = id;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Identifier");
            writer.WriteString(id.ToString());
            writer.WriteEndElement();
        }
    }

    class SequenceTraceRecord : WsrmTraceRecord
    {        
        Int64 sequenceNumber;
        bool isLast;

        internal SequenceTraceRecord(UniqueId id, Int64 sequenceNumber, bool isLast) : base(id)
        {            
            this.sequenceNumber = sequenceNumber;
            this.isLast = isLast;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("MessageNumber");
            writer.WriteString(this.sequenceNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteStartElement("LastMessage");
            writer.WriteString(this.isLast.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }

    class SequenceFaultedTraceRecord : WsrmTraceRecord
    {
        string reason;

        internal SequenceFaultedTraceRecord(UniqueId id, string reason) : base(id)
        {
            this.reason = reason;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Reason");
            writer.WriteString(reason);
            writer.WriteEndElement();
        }
    }

    class AcknowledgementTraceRecord : WsrmTraceRecord
    {
        int bufferRemaining;
        IList<SequenceRange> ranges;

        internal AcknowledgementTraceRecord(UniqueId id, IList<SequenceRange> ranges, int bufferRemaining)
            : base(id)
        {
            this.bufferRemaining = bufferRemaining;
            this.ranges = ranges;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Ranges");
            for (int i = 0; i < this.ranges.Count; i++)
            {
                writer.WriteStartElement("Range");
                writer.WriteAttributeString("Lower", this.ranges[i].Lower.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Upper", this.ranges[i].Upper.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (this.bufferRemaining != -1)
            {
                writer.WriteStartElement("BufferRemaining");
                writer.WriteString(bufferRemaining.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
        }
    }
}

