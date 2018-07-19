//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public abstract class TypedMessageConverter
    {
        public static TypedMessageConverter Create(Type messageContract, string action)
        {
            return Create(messageContract, action, null, TypeLoader.DefaultDataContractFormatAttribute);
        }

        public static TypedMessageConverter Create(Type messageContract, string action, string defaultNamespace)
        {
            return Create(messageContract, action, defaultNamespace, TypeLoader.DefaultDataContractFormatAttribute);
        }

        public static TypedMessageConverter Create(Type messageContract, string action, XmlSerializerFormatAttribute formatterAttribute)
        {
            return Create(messageContract, action, null, formatterAttribute);
        }

        public static TypedMessageConverter Create(Type messageContract, string action, DataContractFormatAttribute formatterAttribute)
        {
            return Create(messageContract, action, null, formatterAttribute);
        }

        public static TypedMessageConverter Create(Type messageContract, String action, String defaultNamespace, XmlSerializerFormatAttribute formatterAttribute)
        {
            if (messageContract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageContract"));

            if (defaultNamespace == null)
                defaultNamespace = NamingHelper.DefaultNamespace;

            return new XmlMessageConverter(GetOperationFormatter(messageContract, formatterAttribute, defaultNamespace, action));
        }

        public static TypedMessageConverter Create(Type messageContract, String action, String defaultNamespace, DataContractFormatAttribute formatterAttribute)
        {
            if (messageContract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageContract"));

            if (!messageContract.IsDefined(typeof(MessageContractAttribute), false))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxMessageContractAttributeRequired, messageContract), "messageContract"));

            if (defaultNamespace == null)
                defaultNamespace = NamingHelper.DefaultNamespace;

            return new XmlMessageConverter(GetOperationFormatter(messageContract, formatterAttribute, defaultNamespace, action));
        }

        public abstract Message ToMessage(Object typedMessage);
        public abstract Message ToMessage(Object typedMessage, MessageVersion version);
        public abstract Object FromMessage(Message message);

        static OperationFormatter GetOperationFormatter(Type t, Attribute formatAttribute, string defaultNS, string action)
        {
            bool isXmlSerializer = (formatAttribute is XmlSerializerFormatAttribute);
            TypeLoader typeLoader = new TypeLoader();
            MessageDescription message = typeLoader.CreateTypedMessageDescription(t, null, null, defaultNS, action, MessageDirection.Output);
            ContractDescription contract = new ContractDescription("dummy_contract", defaultNS);
            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(t.Name), contract, false);
            operation.Messages.Add(message);

            if (isXmlSerializer)
                return XmlSerializerOperationBehavior.CreateOperationFormatter(operation, (XmlSerializerFormatAttribute)formatAttribute);
            else
                return new DataContractSerializerOperationFormatter(operation, (DataContractFormatAttribute)formatAttribute, null);
        }
    }

    internal class XmlMessageConverter : TypedMessageConverter 
    {
        OperationFormatter formatter;

        internal XmlMessageConverter(OperationFormatter formatter)
        {
            this.formatter = formatter;
        }

        internal string Action { get { return formatter.RequestAction; } }

        public override Message ToMessage(Object typedMessage)
        {
            return ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
        }

        public override Message ToMessage(Object typedMessage, MessageVersion version)
        {
            if (typedMessage == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typedMessage"));

            return formatter.SerializeRequest(version, new object[] { typedMessage });
        }

        public override Object FromMessage(Message message)
        {
            Fx.Assert(message.Headers != null, "");
            if (this.Action != null && message.Headers.Action != null && message.Headers.Action != this.Action)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxActionMismatch, this.Action, message.Headers.Action)));

            object[] result = new object[1];
            formatter.DeserializeRequest(message, result);

            return result[0];
        }
    }
}
