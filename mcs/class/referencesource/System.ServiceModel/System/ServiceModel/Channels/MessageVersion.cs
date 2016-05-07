//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Configuration;

    [TypeConverter(typeof(MessageVersionConverter))]
    public sealed class MessageVersion
    {
        EnvelopeVersion envelope;
        AddressingVersion addressing;
        static MessageVersion none;
        static MessageVersion soap11;
        static MessageVersion soap12;
        static MessageVersion soap11Addressing10;
        static MessageVersion soap12Addressing10;
        static MessageVersion soap11Addressing200408;
        static MessageVersion soap12Addressing200408;

        static MessageVersion()
        {
            none = new MessageVersion(EnvelopeVersion.None, AddressingVersion.None);
            soap11 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
            soap12 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.None);
            soap11Addressing10 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10);
            soap12Addressing10 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);
            soap11Addressing200408 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressingAugust2004);
            soap12Addressing200408 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressingAugust2004);
        }

        MessageVersion(EnvelopeVersion envelopeVersion, AddressingVersion addressingVersion)
        {
            this.envelope = envelopeVersion;
            this.addressing = addressingVersion;
        }

        public static MessageVersion CreateVersion(EnvelopeVersion envelopeVersion)
        {
            return CreateVersion(envelopeVersion, AddressingVersion.WSAddressing10);
        }

        public static MessageVersion CreateVersion(EnvelopeVersion envelopeVersion, AddressingVersion addressingVersion)
        {
            if (envelopeVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("envelopeVersion");
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }

            if (envelopeVersion == EnvelopeVersion.Soap12)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return soap12Addressing10;
                }
                else if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    return soap12Addressing200408;
                }
                else if (addressingVersion == AddressingVersion.None)
                {
                    return soap12;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion",
                        SR.GetString(SR.AddressingVersionNotSupported, addressingVersion));
                }
            }
            else if (envelopeVersion == EnvelopeVersion.Soap11)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return soap11Addressing10;
                }
                else if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    return soap11Addressing200408;
                }
                else if (addressingVersion == AddressingVersion.None)
                {
                    return soap11;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion",
                        SR.GetString(SR.AddressingVersionNotSupported, addressingVersion));
                }
            }
            else if (envelopeVersion == EnvelopeVersion.None)
            {
                if (addressingVersion == AddressingVersion.None)
                {
                    return none;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion",
                        SR.GetString(SR.AddressingVersionNotSupported, addressingVersion));
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("envelopeVersion",
                    SR.GetString(SR.EnvelopeVersionNotSupported, envelopeVersion));
            }
        }

        public AddressingVersion Addressing
        {
            get { return addressing; }
        }

        public static MessageVersion Default
        {
            get { return soap12Addressing10; }
        }

        public EnvelopeVersion Envelope
        {
            get { return envelope; }
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            int code = 0;
            if (this.Envelope == EnvelopeVersion.Soap11)
                code += 1;
            if (this.Addressing == AddressingVersion.WSAddressingAugust2004)
                code += 2;
            return code;
        }

        public static MessageVersion None
        {
            get { return none; }
        }

        public static MessageVersion Soap12WSAddressing10
        {
            get { return soap12Addressing10; }
        }

        public static MessageVersion Soap11WSAddressing10
        {
            get { return soap11Addressing10; }
        }

        public static MessageVersion Soap12WSAddressingAugust2004
        {
            get { return soap12Addressing200408; }
        }

        public static MessageVersion Soap11WSAddressingAugust2004
        {
            get { return soap11Addressing200408; }
        }

        public static MessageVersion Soap11
        {
            get { return soap11; }
        }

        public static MessageVersion Soap12
        {
            get { return soap12; }
        }

        public override string ToString()
        {
            return SR.GetString(SR.MessageVersionToStringFormat, envelope.ToString(), addressing.ToString());
        }

        internal bool IsMatch(MessageVersion messageVersion)
        {
            if (messageVersion == null)
            {
                Fx.Assert("Invalid (null) messageVersion value");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (addressing == null)
            {
                Fx.Assert("Invalid (null) addressing value");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "MessageVersion.Addressing cannot be null")));
            }

            if (envelope != messageVersion.Envelope)
                return false;
            if (addressing.Namespace != messageVersion.Addressing.Namespace)
                return false;
            return true;
        }
    }
}
