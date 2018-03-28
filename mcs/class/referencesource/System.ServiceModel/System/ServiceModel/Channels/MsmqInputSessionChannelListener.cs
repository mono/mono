//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Transactions;

    sealed class MsmqInputSessionChannelListener
        : MsmqChannelListenerBase<IInputSessionChannel>
    {
        MsmqReceiveHelper receiver;
        MsmqReceiveContextLockManager receiveContextManager;

        internal MsmqInputSessionChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters)
            : base(bindingElement, context, receiveParameters, TransportDefaults.GetDefaultMessageEncoderFactory())
        {
            SetSecurityTokenAuthenticator(MsmqUri.NetMsmqAddressTranslator.Scheme, context);
            this.receiver = new MsmqReceiveHelper(
                this.ReceiveParameters,
                this.Uri,
                new MsmqInputMessagePool((this.ReceiveParameters as MsmqTransportReceiveParameters).MaxPoolSize),
                null,
                this
                );

            if (this.ReceiveParameters.ReceiveContextSettings.Enabled)
            {
                this.receiveContextManager = new MsmqReceiveContextLockManager(this.ReceiveParameters.ReceiveContextSettings, this.receiver.Queue);
            }
        }

        internal MsmqReceiveHelper MsmqReceiveHelper
        {
            get { return this.receiver; }
        }

        protected override void OnCloseCore(bool aborting)
        {
            if (this.receiver != null)
            {
                this.receiver.Close();
            }
            if (this.receiveContextManager != null)
            {
                this.receiveContextManager.Dispose();
            }
        }

        protected override void OnOpenCore(TimeSpan timeout)
        {
            base.OnOpenCore(timeout);
            try
            {
                this.receiver.Open();
            }
            catch (MsmqException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
        }

        // AcceptChannel
        public override IInputSessionChannel AcceptChannel()
        {
            return AcceptChannel(this.DefaultReceiveTimeout);
        }
        //
        public override IInputSessionChannel AcceptChannel(TimeSpan timeout)
        {
            if (DoneReceivingInCurrentState())
                return null;

            if (!this.ReceiveParameters.ReceiveContextSettings.Enabled && (Transaction.Current == null))
            {
                // In the absence of Receive context, Msmq Sessions can work only with the current transaction,
                this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTransactionRequired)));
            }

            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            try
            {
                MsmqMessageProperty property;
                bool retval = this.receiver.TryReceive(msmqMessage, timeout, MsmqTransactionMode.CurrentOrThrow, out property);
                if (retval)
                {
                    if (null != property)
                    {
                        return MsmqDecodeHelper.DecodeTransportSessiongram(this, msmqMessage, property, this.receiveContextManager);
                    }
                    else
                    {
                        if (CommunicationState.Opened == this.State)
                        {
                            this.Fault();
                        }
                        return null;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }
            catch (MsmqException ex)
            {
                if (ex.FaultReceiver)
                    this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
            finally
            {
                this.receiver.ReturnMessage(msmqMessage);
            }
        }
        //
        public override IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }
        //
        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
                return new DoneReceivingAsyncResult(callback, state);

            if (!this.ReceiveParameters.ReceiveContextSettings.Enabled && (Transaction.Current == null))
            {
                // In the absence of Receive context, Msmq Sessions can work only with the current transaction,
                this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTransactionRequired)));
            }

            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            return this.receiver.BeginTryReceive(
                msmqMessage,
                timeout,
                MsmqTransactionMode.CurrentOrThrow,
                callback,
                state);
        }
        //
        public override IInputSessionChannel EndAcceptChannel(IAsyncResult result)
        {
            if (null == result)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
            if (doneRecevingResult != null)
            {
                DoneReceivingAsyncResult.End(doneRecevingResult);
                return null;
            }

            MsmqInputMessage msmqMessage = null;
            MsmqMessageProperty property = null;
            try
            {
                bool retval = this.receiver.EndTryReceive(result, out msmqMessage, out property);
                if (retval)
                {
                    if (null != property)
                    {
                        return MsmqDecodeHelper.DecodeTransportSessiongram(this, msmqMessage, property, this.receiveContextManager);
                    }
                    else
                    {
                        if (CommunicationState.Opened == this.State)
                        {
                            this.Fault();
                        }
                        return null;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }
            catch (MsmqException ex)
            {
                if (ex.FaultReceiver)
                    this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
            finally
            {
                if (null != msmqMessage)
                    this.receiver.ReturnMessage(msmqMessage);
            }
        }

        // WaitForChannel
        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            if (DoneReceivingInCurrentState())
                return true;

            try
            {
                return this.receiver.WaitForMessage(timeout);
            }
            catch (MsmqException ex)
            {
                if (ex.FaultReceiver)
                    this.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
        }
        //
        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
                return new DoneAsyncResult(true, callback, state);

            return this.receiver.BeginWaitForMessage(timeout, callback, state);
        }
        //
        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            if (null == result)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            DoneAsyncResult doneAsyncResult = result as DoneAsyncResult;
            if (doneAsyncResult != null)
                return DoneAsyncResult.End(result);
            else
            {
                try
                {
                    return this.receiver.EndWaitForMessage(result);
                }
                catch (MsmqException ex)
                {
                    if (ex.FaultReceiver)
                        this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
                }
            }
        }

        protected override void OnFaulted()
        {
            OnCloseCore(true);
            base.OnFaulted();
        }

        class DoneAsyncResult : CompletedAsyncResult<bool>
        {
            internal DoneAsyncResult(bool data, AsyncCallback callback, object state)
                : base(data, callback, state)
            { }
        }
    }
}
