//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.Runtime.Diagnostics;

    class WrapperSecurityCommunicationObject : CommunicationObject
    {
        ISecurityCommunicationObject innerCommunicationObject;

        public WrapperSecurityCommunicationObject(ISecurityCommunicationObject innerCommunicationObject)
            : base()
        {
            if (innerCommunicationObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerCommunicationObject");
            }
            this.innerCommunicationObject = innerCommunicationObject;
        }

        protected override Type GetCommunicationObjectType()
        {
            return this.innerCommunicationObject.GetType();
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.innerCommunicationObject.DefaultCloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.innerCommunicationObject.DefaultOpenTimeout; }
        }

        protected override void OnAbort()
        {
            this.innerCommunicationObject.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerCommunicationObject.OnBeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerCommunicationObject.OnBeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerCommunicationObject.OnClose(timeout);
        }

        protected override void OnClosed()
        {
            this.innerCommunicationObject.OnClosed();
            base.OnClosed();
        }

        protected override void OnClosing()
        {
            this.innerCommunicationObject.OnClosing();
            base.OnClosing();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerCommunicationObject.OnEndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerCommunicationObject.OnEndOpen(result);
        }

        protected override void OnFaulted()
        {
            this.innerCommunicationObject.OnFaulted();
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerCommunicationObject.OnOpen(timeout);
        }

        protected override void OnOpened()
        {
            this.innerCommunicationObject.OnOpened();
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            this.innerCommunicationObject.OnOpening();
            base.OnOpening();
        }

        new internal void ThrowIfDisposedOrImmutable()
        {
            base.ThrowIfDisposedOrImmutable();
        }
    }

    abstract class CommunicationObjectSecurityTokenProvider : SecurityTokenProvider, ICommunicationObject, ISecurityCommunicationObject
    {
        EventTraceActivity eventTraceActivity;
        WrapperSecurityCommunicationObject communicationObject;

        protected CommunicationObjectSecurityTokenProvider()
        {
            communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                if (eventTraceActivity == null)
                { 
                    eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate(); 
                }
                return this.eventTraceActivity;
            }
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get { return this.communicationObject; }
        }

        public event EventHandler Closed
        {
            add { this.communicationObject.Closed += value; }
            remove { this.communicationObject.Closed -= value; }
        }

        public event EventHandler Closing
        {
            add { this.communicationObject.Closing += value; }
            remove { this.communicationObject.Closing -= value; }
        }

        public event EventHandler Faulted
        {
            add { this.communicationObject.Faulted += value; }
            remove { this.communicationObject.Faulted -= value; }
        }

        public event EventHandler Opened
        {
            add { this.communicationObject.Opened += value; }
            remove { this.communicationObject.Opened -= value; }
        }

        public event EventHandler Opening
        {
            add { this.communicationObject.Opening += value; }
            remove { this.communicationObject.Opening -= value; }
        }

        public CommunicationState State
        {
            get { return this.communicationObject.State; }
        }

        public virtual TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public virtual TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        // communication object
        public void Abort()
        {
            this.communicationObject.Abort();
        }

        public void Close()
        {
            this.communicationObject.Close();
        }

        public void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        public void Open()
        {
            this.communicationObject.Open();
        }

        public void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        public void Dispose()
        {
            this.Close();
        }

        // ISecurityCommunicationObject methods
        public virtual void OnAbort()
        {
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public virtual void OnClose(TimeSpan timeout)
        {
        }

        public virtual void OnClosed()
        {
            SecurityTraceRecordHelper.TraceTokenProviderClosed(this);
        }

        public virtual void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public virtual void OnFaulted()
        {
            this.OnAbort();
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
        }

        public virtual void OnOpened()
        {
            SecurityTraceRecordHelper.TraceTokenProviderOpened(this.EventTraceActivity, this);
        }

        public virtual void OnOpening()
        {
        }
    }

    abstract class CommunicationObjectSecurityTokenAuthenticator : SecurityTokenAuthenticator, ICommunicationObject, ISecurityCommunicationObject
    {
        WrapperSecurityCommunicationObject communicationObject;

        protected CommunicationObjectSecurityTokenAuthenticator()
        {
            communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get { return this.communicationObject; }
        }

        public event EventHandler Closed
        {
            add { this.communicationObject.Closed += value; }
            remove { this.communicationObject.Closed -= value; }
        }

        public event EventHandler Closing
        {
            add { this.communicationObject.Closing += value; }
            remove { this.communicationObject.Closing -= value; }
        }

        public event EventHandler Faulted
        {
            add { this.communicationObject.Faulted += value; }
            remove { this.communicationObject.Faulted -= value; }
        }

        public event EventHandler Opened
        {
            add { this.communicationObject.Opened += value; }
            remove { this.communicationObject.Opened -= value; }
        }

        public event EventHandler Opening
        {
            add { this.communicationObject.Opening += value; }
            remove { this.communicationObject.Opening -= value; }
        }

        public CommunicationState State
        {
            get { return this.communicationObject.State; }
        }

        public virtual TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public virtual TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        // communication object
        public void Abort()
        {
            this.communicationObject.Abort();
        }

        public void Close()
        {
            this.communicationObject.Close();
        }

        public void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        public void Open()
        {
            this.communicationObject.Open();
        }

        public void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        public void Dispose()
        {
            this.Close();
        }

        // ISecurityCommunicationObject methods
        public virtual void OnAbort()
        {
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public virtual void OnClose(TimeSpan timeout)
        {
        }

        public virtual void OnClosed()
        {
            SecurityTraceRecordHelper.TraceTokenAuthenticatorClosed(this);
        }

        public virtual void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public virtual void OnFaulted()
        {
            this.OnAbort();
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
        }

        public virtual void OnOpened()
        {
            SecurityTraceRecordHelper.TraceTokenAuthenticatorOpened(this);
        }

        public virtual void OnOpening()
        {
        }
    }
}
