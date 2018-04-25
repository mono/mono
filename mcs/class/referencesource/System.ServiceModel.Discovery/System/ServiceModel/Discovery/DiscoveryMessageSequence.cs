//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;    

    [Fx.Tag.XamlVisible(false)]
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class DiscoveryMessageSequence : 
        IComparable<DiscoveryMessageSequence>, 
        IEquatable<DiscoveryMessageSequence>
    {
        internal DiscoveryMessageSequence()
        {            
        }

        internal DiscoveryMessageSequence(long instanceId, Uri sequenceId, long messageNumber)
        {
            Fx.Assert((instanceId >= 0) && (instanceId <= UInt32.MaxValue), "The instanceId must be within UInt32 range");
            Fx.Assert((messageNumber >= 0) && (messageNumber <= UInt32.MaxValue), "The messageNumber must be within UInt32 range");

            this.InstanceId = instanceId;
            this.SequenceId = sequenceId;
            this.MessageNumber = messageNumber;
        }

        public long InstanceId
        {
            get;
            private set;
        }

        public Uri SequenceId
        {
            get;
            private set;
        }

        public long MessageNumber
        {
            get;
            private set;
        }

        public static bool operator ==(DiscoveryMessageSequence messageSequence1, DiscoveryMessageSequence messageSequence2)
        {
            if (object.ReferenceEquals(messageSequence1, null) && object.ReferenceEquals(messageSequence2, null))
            {
                return true;
            }
            if (object.ReferenceEquals(messageSequence1, null) || object.ReferenceEquals(messageSequence2, null))
            {
                return false;
            }
            return messageSequence1.Equals(messageSequence2);
        }

        public static bool operator !=(DiscoveryMessageSequence messageSequence1, DiscoveryMessageSequence messageSequence2)
        {
            return !(messageSequence1 == messageSequence2);
        }

        public override bool Equals(object obj)
        {
            DiscoveryMessageSequence other = obj as DiscoveryMessageSequence;
            return this.Equals(other);
        }

        public bool Equals(DiscoveryMessageSequence other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return (long.Equals(this.InstanceId, other.InstanceId) &&
                Uri.Equals(this.SequenceId, other.SequenceId) &&
                long.Equals(this.MessageNumber, other.MessageNumber));
        }

        public override string ToString()
        {
            return SR.DiscoveryMessageSequenceToString(this.InstanceId, this.SequenceId, this.MessageNumber);
        }

        public bool CanCompareTo(DiscoveryMessageSequence other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            else
            {
                return ((this.InstanceId != other.InstanceId) ||
                    (Uri.Equals(this.SequenceId, other.SequenceId)));
            }
        }

        public override int GetHashCode()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "{0}:{1}:{2}", 
                this.InstanceId, 
                this.SequenceId, 
                this.MessageNumber).GetHashCode();
        }

        public int CompareTo(DiscoveryMessageSequence other)
        {
            if (object.ReferenceEquals(other, null))
            {
                throw FxTrace.Exception.ArgumentNull("other");
            }

            int result = this.InstanceId.CompareTo(other.InstanceId);
            if (result == 0)
            {
                if (!Uri.Equals(this.SequenceId, other.SequenceId))
                {
                    throw FxTrace.Exception.Argument("other", SR2.DiscoveryIncompatibleMessageSequence);
                }

                result = this.MessageNumber.CompareTo(other.MessageNumber);
            }

            return result;
        }

        [Fx.Tag.Throws(typeof(XmlException), "throws on incorrect xml data")]
        internal void ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }

            string instanceIdString = reader.GetAttribute(ProtocolStrings.SchemaNames.AppSequenceInstanceId);

            this.InstanceId = SerializationUtility.ReadUInt(
                instanceIdString, 
                SR2.DiscoveryXmlMissingAppSequenceInstanceId, 
                SR2.DiscoveryXmlInvalidAppSequenceInstanceId);

            string sequenceIdString = reader.GetAttribute(ProtocolStrings.SchemaNames.AppSequenceSequenceId);

            if (sequenceIdString != null)
            {
                try
                {
                    this.SequenceId = new Uri(sequenceIdString, UriKind.RelativeOrAbsolute);
                }
                catch (FormatException fe)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlUriFormatError(sequenceIdString), fe));
                }
            }

            string messageNumberString = reader.GetAttribute(ProtocolStrings.SchemaNames.AppSequenceMessageNumber);

            this.MessageNumber = SerializationUtility.ReadUInt(
                messageNumberString, 
                SR2.DiscoveryXmlMissingAppSequenceMessageNumber, 
                SR2.DiscoveryXmlInvalidAppSequenceMessageNumber);           
        }

        internal void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw FxTrace.Exception.ArgumentNull("writer");
            }

            writer.WriteAttributeString(
                ProtocolStrings.SchemaNames.AppSequenceInstanceId, 
                this.InstanceId.ToString(CultureInfo.InvariantCulture));

            if (this.SequenceId != null)
            {
                writer.WriteAttributeString(
                    ProtocolStrings.SchemaNames.AppSequenceSequenceId, 
                    this.SequenceId.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            }

            writer.WriteAttributeString(
                ProtocolStrings.SchemaNames.AppSequenceMessageNumber, 
                this.MessageNumber.ToString(CultureInfo.InvariantCulture));
        }
    }
}
