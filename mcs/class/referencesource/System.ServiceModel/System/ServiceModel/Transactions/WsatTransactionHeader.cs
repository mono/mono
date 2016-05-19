//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Transactions;
    using System.Xml;

    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System.Security.Permissions;

    class WsatTransactionHeader : MessageHeader
    {
        string wsatHeaderElement;
        string wsatNamespace;               
        CoordinationContext context;

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call into CoordinationStrings.Version(..), which is defined in a non-APTCA assembly and does an Environment.FailFast 
        // if the argument is invalid. It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public WsatTransactionHeader(CoordinationContext context, ProtocolVersion protocolVersion)
        {
            this.context = context;
            CoordinationStrings coordinationStrings = CoordinationStrings.Version(protocolVersion);                        
            this.wsatHeaderElement = coordinationStrings.CoordinationContext;
            this.wsatNamespace = coordinationStrings.Namespace;
        }

        public override bool MustUnderstand
        {
            get { return true; }                
        }
        
        public override string Name
        {
            get { return wsatHeaderElement; }
        }

        public override string Namespace
        {
            get { return wsatNamespace; }
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call into CoordinationContext and CoordinationStrings, which are defined in a non-APTCA assembly. Also, CoordinationStrings.Version(..) 
        // does an Environment.FailFast if the argument is invalid. It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public static CoordinationContext GetCoordinationContext(Message message, ProtocolVersion protocolVersion)
        {
            CoordinationStrings coordinationStrings = CoordinationStrings.Version(protocolVersion);
            string locWsatHeaderElement = coordinationStrings.CoordinationContext;
            string locWsatNamespace = coordinationStrings.Namespace;
            
            int index;
            try
            {
                index = message.Headers.FindHeader(locWsatHeaderElement, locWsatNamespace);
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                return null;
            }
            if (index < 0)
                return null;

            CoordinationContext context;
            XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index);
            using (reader)
            {
                context = GetCoordinationContext(reader, protocolVersion);
            }

            MessageHeaderInfo header = message.Headers[index];
            if (!message.Headers.UnderstoodHeaders.Contains(header))
            {
                message.Headers.UnderstoodHeaders.Add(header);
            }

            return context;
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we call into CoordinationContext, which is defined in a non-APTCA assembly; WSATs are not supported in partial trust, so customers should not be broken by this demand
        */
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.context.WriteContent(writer);
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call into CoordinationXmlDictionaryStrings.Version(..), which is defined in a non-APTCA assembly and does an Environment.FailFast 
        // if the argument is invalid. It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public static CoordinationContext GetCoordinationContext(XmlDictionaryReader reader, ProtocolVersion protocolVersion)
        {
            CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings = 
                                                CoordinationXmlDictionaryStrings.Version(protocolVersion); 
            try
            {
                return CoordinationContext.ReadFrom(reader,
                                                    coordinationXmlDictionaryStrings.CoordinationContext,
                                                    coordinationXmlDictionaryStrings.Namespace,
                                                    protocolVersion);
            }
            catch (InvalidCoordinationContextException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(SR.GetString(SR.WsatHeaderCorrupt), e));
            }
        }
    }
}
