//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;

    abstract class MsmqInputChannelBase
        : ChannelBase, IInputChannel
    {
        EndpointAddress localAddress;
        MsmqReceiveHelper receiver;
        MsmqReceiveParameters receiveParameters;
        MsmqInputChannelListenerBase listener;
        MsmqReceiveContextLockManager receiveContextManager;

        public MsmqInputChannelBase(MsmqInputChannelListenerBase listener, IMsmqMessagePool messagePool)
            : base(listener)
        {
            this.receiveParameters = listener.ReceiveParameters;
            this.receiver = new MsmqReceiveHelper(listener.ReceiveParameters, listener.Uri, messagePool, this, listener);
            this.localAddress = new EndpointAddress(listener.Uri);
            this.listener = listener;
            if (this.receiveParameters.ReceiveContextSettings.Enabled)
            {
                this.receiveContextManager = new MsmqReceiveContextLockManager(this.receiveParameters.ReceiveContextSettings, this.receiver.Queue);
            }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        protected MsmqReceiveHelper MsmqReceiveHelper
        {
            get { return this.receiver; }
        }

        protected MsmqReceiveParameters ReceiveParameters
        {
            get { return this.receiveParameters; }
        }

        protected virtual void OnCloseCore(bool isAborting)
        {
            receiver.Close();
            if (receiveContextManager != null)
            {
                receiveContextManager.Dispose();
            }
        }

        protected virtual void OnOpenCore()
        {
            try
            {
                receiver.Open();
            }
            catch (MsmqException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnOpenCore();
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            OnOpenCore();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnAbort()
        {
            OnCloseCore(true);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseCore(false);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(false);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            OnCloseCore(true);
            base.OnFaulted();
        }

        abstract protected Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty property);

        internal void FaultChannel()
        {
            this.Fault();
        }

        // Receive
        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }
        //
        public Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceive(this, timeout);
        }
        //
        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (MsmqDiagnostics.BoundReceiveOperation(this.receiver))
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }
        }

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        // TryReceive
        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = null;
            if (DoneReceivingInCurrentState())
                return true;

            using (MsmqDiagnostics.BoundReceiveOperation(this.receiver))
            {
                MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
                try
                {
                    MsmqMessageProperty property;
                    bool retval = this.receiver.TryReceive(
                        msmqMessage,
                        timeout,
                        this.ReceiveParameters.ExactlyOnce ? MsmqTransactionMode.CurrentOrNone : MsmqTransactionMode.None,
                        out property);
                    if (retval)
                    {
                        if (null != property)
                        {
                            message = DecodeMsmqMessage(msmqMessage, property);
                            message.Properties[MsmqMessageProperty.Name] = property;

                            if (this.receiveParameters.ReceiveContextSettings.Enabled)
                            {
                                message.Properties[MsmqReceiveContext.Name] = this.receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                            }
                            MsmqDiagnostics.DatagramReceived(msmqMessage.MessageId, message);
                            this.listener.RaiseMessageReceived();
                        }
                        else if (CommunicationState.Opened == this.State)
                        {
                            this.listener.FaultListener();
                            this.Fault();
                        }
                    }
                    return retval;
                }
                catch (MsmqException ex)
                {
                    if (ex.FaultReceiver)
                    {
                        this.listener.FaultListener();
                        this.Fault();
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
                }
                finally
                {
                    this.receiver.ReturnMessage(msmqMessage);
                }
            }
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
                return new DoneReceivingAsyncResult(callback, state);

            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            return this.receiver.BeginTryReceive(
                msmqMessage,
                timeout,
                this.ReceiveParameters.ExactlyOnce ? MsmqTransactionMode.CurrentOrNone : MsmqTransactionMode.None,
                callback,
                state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            message = null;

            if (null == result)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
            if (doneRecevingResult != null)
                return DoneReceivingAsyncResult.End(doneRecevingResult);

            MsmqInputMessage msmqMessage = null;
            MsmqMessageProperty property = null;
            try
            {
                bool retval = this.receiver.EndTryReceive(result, out msmqMessage, out property);
                if (retval)
                {
                    if (null != property)
                    {
                        message = DecodeMsmqMessage(msmqMessage, property);
                        message.Properties[MsmqMessageProperty.Name] = property;

                        if (this.receiveParameters.ReceiveContextSettings.Enabled)
                        {
                            message.Properties[MsmqReceiveContext.Name] = this.receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                        }

                        MsmqDiagnostics.DatagramReceived(msmqMessage.MessageId, message);
                        this.listener.RaiseMessageReceived();
                    }
                    else if (CommunicationState.Opened == this.State)
                    {
                        this.listener.FaultListener();
                        this.Fault();
                    }
                }
                return retval;
            }
            catch (MsmqException ex)
            {
                if (ex.FaultReceiver)
                {
                    this.listener.FaultListener();
                    this.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
            finally
            {
                if (null != msmqMessage)
                    this.receiver.ReturnMessage(msmqMessage);
            }
        }

        // WaitForMessage
        public bool WaitForMessage(TimeSpan timeout)
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
                {
                    this.listener.FaultListener();
                    this.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DoneReceivingInCurrentState())
                return new DoneReceivingAsyncResult(callback, state);

            return this.receiver.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            if (null == result)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            DoneReceivingAsyncResult doneRecevingResult = result as DoneReceivingAsyncResult;
            if (doneRecevingResult != null)
                return DoneReceivingAsyncResult.End(doneRecevingResult);

            try
            {
                return this.receiver.EndWaitForMessage(result);
            }
            catch (MsmqException ex)
            {
                if (ex.FaultReceiver)
                {
                    this.listener.FaultListener();
                    this.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex.Normalized);
            }
        }
    }
}
