//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Xml;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;

    public class FaultCode
    {
        FaultCode subCode;
        string name;
        string ns;
        EnvelopeVersion version;

        public FaultCode(string name)
            : this(name, "", null)
        {
        }

        public FaultCode(string name, FaultCode subCode)
            : this(name, "", subCode)
        {
        }

        public FaultCode(string name, string ns)
            : this(name, ns, null)
        {
        }

        public FaultCode(string name, string ns, FaultCode subCode)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            if (name.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("name"));

            if (!string.IsNullOrEmpty(ns))
                NamingHelper.CheckUriParameter(ns, "ns");

            this.name = name;
            this.ns = ns;
            this.subCode = subCode;

            if (ns == Message12Strings.Namespace)
                this.version = EnvelopeVersion.Soap12;
            else if (ns == Message11Strings.Namespace)
                this.version = EnvelopeVersion.Soap11;
            else if (ns == MessageStrings.Namespace)
                this.version = EnvelopeVersion.None;
            else
                this.version = null;
        }

        public bool IsPredefinedFault
        {
            get
            {
                return ns.Length == 0 || version != null;
            }
        }

        public bool IsSenderFault
        {
            get
            {
                if (IsPredefinedFault)
                    return name == (this.version ?? EnvelopeVersion.Soap12).SenderFaultName;

                return false;
            }
        }

        public bool IsReceiverFault
        {
            get
            {
                if (IsPredefinedFault)
                    return name == (this.version ?? EnvelopeVersion.Soap12).ReceiverFaultName;

                return false;
            }
        }

        public string Namespace
        {
            get
            {
                return ns;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public FaultCode SubCode
        {
            get
            {
                return subCode;
            }
        }

        public static FaultCode CreateSenderFaultCode(FaultCode subCode)
        {
            return new FaultCode("Sender", subCode);
        }

        public static FaultCode CreateSenderFaultCode(string name, string ns)
        {
            return CreateSenderFaultCode(new FaultCode(name, ns));
        }

        public static FaultCode CreateReceiverFaultCode(FaultCode subCode)
        {
            if (subCode == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("subCode"));
            return new FaultCode("Receiver", subCode);
        }

        public static FaultCode CreateReceiverFaultCode(string name, string ns)
        {
            return CreateReceiverFaultCode(new FaultCode(name, ns));
        }
    }
}
