//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public class ResolveCriteria
    {
        static TimeSpan defaultDuration = TimeSpan.FromSeconds(20);

        EndpointAddress endpointAddress;
        TimeSpan duration;
        NonNullItemCollection<XElement> extensions;

        public ResolveCriteria()
            : this(new EndpointAddress(EndpointAddress.AnonymousUri))
        {
        }

        public ResolveCriteria(EndpointAddress address)
        {
            if (address == null)
            {
                throw FxTrace.Exception.ArgumentNull("address");
            }

            this.endpointAddress = address;
            this.duration = ResolveCriteria.defaultDuration;
        }

        public EndpointAddress Address
        {
            get
            {
                return this.endpointAddress;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.endpointAddress = value;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                if (value.CompareTo(TimeSpan.Zero) <= 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR2.DiscoveryResolveDurationLessThanZero);
                }
                this.duration = value;
            }
        }

        public Collection<XElement> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new NonNullItemCollection<XElement>();
                }

                return this.extensions;
            }
        }

        [Fx.Tag.Throws(typeof(XmlException), "throws on incorrect xml data")]
        internal void ReadFrom(DiscoveryVersion discoveryVersion, XmlReader reader)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            int startDepth = reader.Depth;
            reader.ReadStartElement();

            this.endpointAddress = SerializationUtility.ReadEndpointAddress(discoveryVersion, reader);

            this.extensions = null;
            this.duration = TimeSpan.MaxValue;
            while (true)
            {
                reader.MoveToContent();

                if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Depth == startDepth))
                {
                    break;
                }
                else if (reader.IsStartElement(ProtocolStrings.SchemaNames.DurationElement, ProtocolStrings.VersionInternal.Namespace))
                {
                    this.duration = SerializationUtility.ReadDuration(reader);
                }
                else if (reader.IsStartElement())
                {
                    XElement xElement = XElement.ReadFrom(reader) as XElement;
                    Extensions.Add(xElement);
                }
                else
                {
                    reader.Read();
                }
            }
        }

        internal void WriteTo(DiscoveryVersion discoveryVersion, XmlWriter writer)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (writer == null)
            {
                throw FxTrace.Exception.ArgumentNull("writer");
            }

            SerializationUtility.WriteEndPointAddress(discoveryVersion, this.endpointAddress, writer);

            if (this.duration != TimeSpan.MaxValue)
            {
                writer.WriteElementString(
                    ProtocolStrings.SchemaNames.DurationElement, 
                    ProtocolStrings.VersionInternal.Namespace, 
                    XmlConvert.ToString(this.duration));
            }

            if (this.extensions != null)
            {
                foreach (XElement xElement in Extensions)
                {
                    xElement.WriteTo(writer);
                }
            }
        }
    }
}
