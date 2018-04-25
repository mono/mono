//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime.Serialization;
    using System.Xml;
    using System.ServiceModel.Channels;

    public sealed class EnvelopeVersion
    {
        string ultimateDestinationActor;
        string[] ultimateDestinationActorValues;
        string nextDestinationActorValue;
        string ns;
        XmlDictionaryString dictionaryNs;
        string actor;
        XmlDictionaryString dictionaryActor;
        string toStringFormat;
        string[] mustUnderstandActorValues;
        string senderFaultName;
        string receiverFaultName;
        static EnvelopeVersion soap11 =
            new EnvelopeVersion(
                "",
                "http://schemas.xmlsoap.org/soap/actor/next",
                Message11Strings.Namespace,
                XD.Message11Dictionary.Namespace,
                Message11Strings.Actor,
                XD.Message11Dictionary.Actor,
                SR.Soap11ToStringFormat,
                "Client",
                "Server");

        static EnvelopeVersion soap12 =
            new EnvelopeVersion(
                "http://www.w3.org/2003/05/soap-envelope/role/ultimateReceiver",
                "http://www.w3.org/2003/05/soap-envelope/role/next",
                Message12Strings.Namespace,
                XD.Message12Dictionary.Namespace,
                Message12Strings.Role,
                XD.Message12Dictionary.Role,
                SR.Soap12ToStringFormat,
                "Sender",
                "Receiver");

        static EnvelopeVersion none = new EnvelopeVersion(
                null,
                null,
                MessageStrings.Namespace,
                XD.MessageDictionary.Namespace,
                null,
                null,
                SR.EnvelopeNoneToStringFormat,
                "Sender",
                "Receiver");

        EnvelopeVersion(string ultimateReceiverActor, string nextDestinationActorValue,
            string ns, XmlDictionaryString dictionaryNs, string actor, XmlDictionaryString dictionaryActor,
            string toStringFormat, string senderFaultName, string receiverFaultName)
        {
            this.toStringFormat = toStringFormat;
            this.ultimateDestinationActor = ultimateReceiverActor;
            this.nextDestinationActorValue = nextDestinationActorValue;
            this.ns = ns;
            this.dictionaryNs = dictionaryNs;
            this.actor = actor;
            this.dictionaryActor = dictionaryActor;
            this.senderFaultName = senderFaultName;
            this.receiverFaultName = receiverFaultName;

            if (ultimateReceiverActor != null)
            {
                if (ultimateReceiverActor.Length == 0)
                {
                    mustUnderstandActorValues = new string[] { "", nextDestinationActorValue };
                    ultimateDestinationActorValues = new string[] { "", nextDestinationActorValue };
                }
                else
                {
                    mustUnderstandActorValues = new string[] { "", ultimateReceiverActor, nextDestinationActorValue };
                    ultimateDestinationActorValues = new string[] { "", ultimateReceiverActor, nextDestinationActorValue };
                }
            }
        }

        internal string Actor
        {
            get { return actor; }
        }

        internal XmlDictionaryString DictionaryActor
        {
            get { return dictionaryActor; }
        }

        internal string Namespace
        {
            get { return ns; }
        }

        internal XmlDictionaryString DictionaryNamespace
        {
            get { return dictionaryNs; }
        }

        public string NextDestinationActorValue
        {
            get { return nextDestinationActorValue; }
        }

        public static EnvelopeVersion None
        {
            get { return none; }
        }

        public static EnvelopeVersion Soap11
        {
            get { return soap11; }
        }

        public static EnvelopeVersion Soap12
        {
            get { return soap12; }
        }

        internal string ReceiverFaultName
        {
            get { return receiverFaultName; }
        }

        internal string SenderFaultName
        {
            get { return senderFaultName; }
        }

        internal string[] MustUnderstandActorValues
        {
            get { return this.mustUnderstandActorValues; }
        }

        internal string UltimateDestinationActor
        {
            get { return ultimateDestinationActor; }
        }

        public string[] GetUltimateDestinationActorValues()
        {
            return (string[])this.ultimateDestinationActorValues.Clone();
        }

        internal string[] UltimateDestinationActorValues
        {
            get { return ultimateDestinationActorValues; }
        }

        internal bool IsUltimateDestinationActor(string actor)
        {
            return actor.Length == 0 || actor == this.ultimateDestinationActor || actor == this.nextDestinationActorValue;
        }

        public override string ToString()
        {
            return SR.GetString(toStringFormat, Namespace);
        }
    }
}
