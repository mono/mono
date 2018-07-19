//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;
    using System.Xml;

    using Microsoft.Transactions.Wsat.Messaging;

    class OleTxTransactionHeader : MessageHeader
    {
        const string OleTxHeaderElement = OleTxTransactionExternalStrings.OleTxTransaction;
        const string OleTxNamespace = OleTxTransactionExternalStrings.Namespace;
        static readonly XmlDictionaryString CoordinationNamespace = XD.CoordinationExternal10Dictionary.Namespace; // we keep using wscoor10 namespace for compatibility
        
        byte[] propagationToken;
        WsatExtendedInformation wsatInfo;

        public OleTxTransactionHeader(byte[] propagationToken, WsatExtendedInformation wsatInfo)
        {
            this.propagationToken = propagationToken;
            this.wsatInfo = wsatInfo;
        }

        public override bool MustUnderstand
        {
            get { return true; }
        }
        
        public override string Name
        {
            get { return OleTxHeaderElement; }
        }

        public override string Namespace
        {
            get { return OleTxNamespace; }
        }

        public byte[] PropagationToken
        {
            get { return this.propagationToken; }
        }

        public WsatExtendedInformation WsatExtendedInformation
        {
            get { return this.wsatInfo; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (this.wsatInfo != null)
            {
                if (this.wsatInfo.Timeout != 0)
                {
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Expires,
                                                CoordinationNamespace,
                                                XmlConvert.ToString(this.wsatInfo.Timeout));
                }

                if (!string.IsNullOrEmpty(this.wsatInfo.Identifier))
                {
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Identifier,
                                                CoordinationNamespace,
                                                this.wsatInfo.Identifier);
                }
            }

            WritePropagationTokenElement(writer, this.propagationToken);
        }

        public static OleTxTransactionHeader ReadFrom(Message message)
        {
            int index;
            try
            {
                index = message.Headers.FindHeader(OleTxHeaderElement, OleTxNamespace);
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(SR.GetString(SR.OleTxHeaderCorrupt), e));
            }

            if (index < 0)
                return null;

            OleTxTransactionHeader oleTxHeader;
            XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index);
            using (reader)
            {
                try
                {
                    oleTxHeader = ReadFrom(reader);
                }
                catch (XmlException xe)
                {
                    DiagnosticUtility.TraceHandledException(xe, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(SR.GetString(SR.OleTxHeaderCorrupt), xe));
                }
            }

            MessageHeaderInfo header = message.Headers[index];
            if (!message.Headers.UnderstoodHeaders.Contains(header))
            {
                message.Headers.UnderstoodHeaders.Add(header);
            }

            return oleTxHeader;
        }

        static OleTxTransactionHeader ReadFrom(XmlDictionaryReader reader)
        {
            WsatExtendedInformation info = null;

            if (reader.IsStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction,
                                      XD.OleTxTransactionExternalDictionary.Namespace))
            {
                string identifier = reader.GetAttribute(XD.CoordinationExternalDictionary.Identifier, CoordinationNamespace);

                if (!string.IsNullOrEmpty(identifier))
                {
                    // Verify identifier is really a URI
                    Uri uri;
                    if (!Uri.TryCreate(identifier, UriKind.Absolute, out uri))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidWsatExtendedInfo)));
                    }
                }

                string attr = reader.GetAttribute(XD.CoordinationExternalDictionary.Expires, CoordinationNamespace);

                uint timeout = 0;
                if (!string.IsNullOrEmpty(attr))
                {
                    try
                    {
                        timeout = XmlConvert.ToUInt32(attr);
                    }
                    catch (FormatException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidWsatExtendedInfo), e));
                    }
                    catch (OverflowException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidWsatExtendedInfo), e));
                    }
                }

                if (!string.IsNullOrEmpty(identifier) || timeout != 0)
                {
                    info = new WsatExtendedInformation(identifier, timeout);
                }
            }

            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction,
                                        XD.OleTxTransactionExternalDictionary.Namespace);

            byte[] propagationToken = ReadPropagationTokenElement(reader);

            // Skip extensibility elements...
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();

            return new OleTxTransactionHeader(propagationToken, info);
        }

        public static void WritePropagationTokenElement(XmlDictionaryWriter writer, byte[] propagationToken)
        {
            writer.WriteStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken,
                                     XD.OleTxTransactionExternalDictionary.Namespace);
            writer.WriteBase64(propagationToken, 0, propagationToken.Length);
            writer.WriteEndElement();
        }

        public static bool IsStartPropagationTokenElement(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken,
                                         XD.OleTxTransactionExternalDictionary.Namespace);
        }

        public static byte[] ReadPropagationTokenElement(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken,
                                        XD.OleTxTransactionExternalDictionary.Namespace);

            byte[] propagationToken = reader.ReadContentAsBase64();
            if (propagationToken.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidPropagationToken)));
            }

            reader.ReadEndElement();

            return propagationToken;
        }
    }
}
