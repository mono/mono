//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Xml;
    
    using Microsoft.Transactions.Wsat.Messaging;
    using XD = System.ServiceModel.XD;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    class WsatRegistrationHeader : AddressHeader
    {
        const string HeaderName = DotNetAtomicTransactionExternalStrings.RegisterInfo;
        const string HeaderNamespace = DotNetAtomicTransactionExternalStrings.Namespace;

        Guid transactionId;
        string contextId;
        string tokenId;

        public WsatRegistrationHeader(Guid transactionId, string contextId, string tokenId)
        {
            this.transactionId = transactionId;
            this.contextId = contextId;
            this.tokenId = tokenId;
        }

        public override string Name
        {
            get { return HeaderName; }
        }

        public override string Namespace
        {
            get { return HeaderNamespace; }
        }

        public Guid TransactionId
        {
            get { return this.transactionId; }
        }

        public string ContextId
        {
            get { return this.contextId; }
        }

        public string TokenId
        {
            get { return this.tokenId; }
        }

        protected override void OnWriteStartAddressHeader (XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(DotNetAtomicTransactionExternalStrings.Prefix,
                                     XD.DotNetAtomicTransactionExternalDictionary.RegisterInfo,
                                     XD.DotNetAtomicTransactionExternalDictionary.Namespace);
        }

        protected override void OnWriteAddressHeaderContents (XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId,
                                     XD.DotNetAtomicTransactionExternalDictionary.Namespace);

            writer.WriteValue(this.transactionId);
            writer.WriteEndElement();

            if (this.contextId != null)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.ContextId,
                                         XD.DotNetAtomicTransactionExternalDictionary.Namespace);

                writer.WriteValue(this.contextId);
                writer.WriteEndElement();
            }

            if (this.tokenId != null)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.TokenId,
                                         XD.DotNetAtomicTransactionExternalDictionary.Namespace);

                writer.WriteValue(this.tokenId);
                writer.WriteEndElement();
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The call to InvalidEnlistmentHeaderException is safe.")]
        public static WsatRegistrationHeader ReadFrom(Message message)
        {
            int index;
            try
            {
                index = message.Headers.FindHeader(HeaderName, HeaderNamespace);
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                return null;
            }
            if (index < 0)
            {
                return null;
            }

            WsatRegistrationHeader header;

            XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index);
            using (reader)
            {
                try
                {
                    header = ReadFrom(reader);
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(e.Message, e));
                }
            }

            MessageHeaderInfo headerInfo = message.Headers[index];
            if (!message.Headers.UnderstoodHeaders.Contains(headerInfo))
            {
                message.Headers.UnderstoodHeaders.Add(headerInfo);
            }

            return header;
        }

        static WsatRegistrationHeader ReadFrom(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement(XD.DotNetAtomicTransactionExternalDictionary.RegisterInfo,
                                        XD.DotNetAtomicTransactionExternalDictionary.Namespace);

            reader.MoveToStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId,
                                      XD.DotNetAtomicTransactionExternalDictionary.Namespace);

            // TransactionId
            Guid transactionId = reader.ReadElementContentAsGuid();
            if (transactionId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.InvalidRegistrationHeaderTransactionId)));
            }

            // ContextId
            string contextId;
            if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.ContextId,
                                      XD.DotNetAtomicTransactionExternalDictionary.Namespace))
            {
                Uri uri;
                contextId = reader.ReadElementContentAsString().Trim();
                if (contextId.Length == 0 ||
                    contextId.Length > CoordinationContext.MaxIdentifierLength ||
                    !Uri.TryCreate(contextId, UriKind.Absolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.InvalidRegistrationHeaderIdentifier)));
                }
            }
            else
            {
                contextId = null;
            }

            // TokenId
            string tokenId;
            if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.TokenId,
                                      XD.DotNetAtomicTransactionExternalDictionary.Namespace))
            {
                tokenId = reader.ReadElementContentAsString().Trim();
                if (tokenId.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.InvalidRegistrationHeaderTokenId)));
                }
            }
            else
            {
                tokenId = null;
            }

            // Skip unknown elements
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();

            return new WsatRegistrationHeader(transactionId, contextId, tokenId);
        }
    }
}
