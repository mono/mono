//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    using System.Transactions;
    using System.ServiceModel.Transactions;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using SR = System.ServiceModel.SR;
    using System.ServiceModel.Diagnostics;

    internal interface ITransactionChannel
    {
        // These get run on forward-going messages only
        void WriteTransactionDataToMessage(Message message, MessageDirection direction);
        void ReadTransactionDataFromMessage(Message message, MessageDirection direction);
        // These get run in both directions (request and reply).  If other flowable-things are added 
        // that need to flow both ways, these methods should be renamed and generalized to do it
        void ReadIssuedTokens(Message message, MessageDirection direction);
        void WriteIssuedTokens(Message message, MessageDirection direction);
    }

    abstract class TransactionChannel<TChannel>
        : LayeredChannel<TChannel>, ITransactionChannel
        where TChannel : class, IChannel
    {
        ITransactionChannelManager factory;
        TransactionFormatter formatter;

        protected TransactionChannel(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {
            this.factory = (ITransactionChannelManager)channelManager;

            if (this.factory.TransactionProtocol == TransactionProtocol.OleTransactions)
            {
                this.formatter = TransactionFormatter.OleTxFormatter;
            }
            else if (this.factory.TransactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
            {
                this.formatter = TransactionFormatter.WsatFormatter10;
            }
            else if (this.factory.TransactionProtocol == TransactionProtocol.WSAtomicTransaction11)
            {
                this.formatter = TransactionFormatter.WsatFormatter11;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.SFxBadTransactionProtocols)));
            }
        }

        internal TransactionFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
        }

        internal TransactionProtocol Protocol
        {
            get
            {
                return this.factory.TransactionProtocol;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return (T)(object)new TransactionChannelFaultConverter<TChannel>(this);
            }

            return base.GetProperty<T>();
        }

        public T GetInnerProperty<T>() where T : class
        {
            return base.InnerChannel.GetProperty<T>();
        }

        static bool Found(int index)
        {
            return index != -1;
        }

        void FaultOnMessage(Message message, string reason, string codeString)
        {
            FaultCode code = FaultCode.CreateSenderFaultCode(codeString, FaultCodeConstants.Namespaces.Transactions);
            FaultException fault = new FaultException(reason, code, FaultCodeConstants.Actions.Transactions);
            throw TraceUtility.ThrowHelperError(fault, message);
        }

        ICollection<RequestSecurityTokenResponse> GetIssuedTokens(Message message)
        {
            return IssuedTokensHeader.ExtractIssuances(message, this.factory.StandardsManager, message.Version.Envelope.UltimateDestinationActorValues, null);
        }

        public void ReadIssuedTokens(Message message, MessageDirection direction)
        {
            TransactionFlowOption option = this.factory.FlowIssuedTokens;

            ICollection<RequestSecurityTokenResponse> issuances = this.GetIssuedTokens(message);

            if (issuances != null && issuances.Count != 0)
            {
                if (option == TransactionFlowOption.NotAllowed)
                {
                    FaultOnMessage(message, SR.GetString(SR.IssuedTokenFlowNotAllowed), FaultCodeConstants.Codes.IssuedTokenFlowNotAllowed);
                }

                foreach (RequestSecurityTokenResponse rstr in issuances)
                {
                    TransactionFlowProperty.Ensure(message).IssuedTokens.Add(rstr);
                }
            }
        }

        void ReadTransactionFromMessage(Message message, TransactionFlowOption txFlowOption)
        {
            TransactionInfo transactionInfo = null;
            try
            {
                transactionInfo = this.formatter.ReadTransaction(message);
            }
            catch (TransactionException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                FaultOnMessage(message, SR.GetString(SR.SFxTransactionDeserializationFailed, e.Message), FaultCodeConstants.Codes.TransactionHeaderMalformed);
            }

            if (transactionInfo != null)
            {
                TransactionMessageProperty.Set(transactionInfo, message);
            }
            else if (txFlowOption == TransactionFlowOption.Mandatory)
            {
                FaultOnMessage(message, SR.GetString(SR.SFxTransactionFlowRequired), FaultCodeConstants.Codes.TransactionHeaderMissing);
            }
        }

        public virtual void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            this.ReadIssuedTokens(message, direction);

            TransactionFlowOption txFlowOption = this.factory.GetTransaction(direction, message.Headers.Action);
            if (TransactionFlowOptionHelper.AllowedOrRequired(txFlowOption))
            {
                this.ReadTransactionFromMessage(message, txFlowOption);
            }
        }

        public void WriteTransactionDataToMessage(Message message, MessageDirection direction)
        {
            TransactionFlowOption txFlowOption = this.factory.GetTransaction(direction, message.Headers.Action);
            if (TransactionFlowOptionHelper.AllowedOrRequired(txFlowOption))
            {
                this.WriteTransactionToMessage(message, txFlowOption);
            }

            if (TransactionFlowOptionHelper.AllowedOrRequired(this.factory.FlowIssuedTokens))
            {
                this.WriteIssuedTokens(message, direction);
            }

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void WriteTransactionToMessage(Message message, TransactionFlowOption txFlowOption)
        {
            Transaction transaction = TransactionFlowProperty.TryGetTransaction(message);

            if (transaction != null)
            {
                try
                {
                    this.formatter.WriteTransaction(transaction, message);
                }
                catch (TransactionException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(e.Message, e));
                }
            }
            else if (txFlowOption == TransactionFlowOption.Mandatory)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.SFxTransactionFlowRequired)));
            }
        }

        public void WriteIssuedTokens(Message message, MessageDirection direction)
        {
            ICollection<RequestSecurityTokenResponse> issuances = TransactionFlowProperty.TryGetIssuedTokens(message);
            if (issuances != null)
            {
                IssuedTokensHeader header = new IssuedTokensHeader(issuances, this.factory.StandardsManager);
                message.Headers.Add(header);

            }
        }
    }
}
