//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading;

    public sealed class OperationContextScope : IDisposable
    {
        [ThreadStatic]
        static OperationContextScope legacyCurrentScope;

        static AsyncLocal<OperationContextScope> currentScope = new AsyncLocal<OperationContextScope>();

        OperationContext currentContext;
        bool disposed;
        readonly OperationContext originalContext = OperationContext.Current;
        readonly OperationContextScope originalScope = OperationContextScope.CurrentScope;
        readonly Thread thread = Thread.CurrentThread;

        public OperationContextScope(IContextChannel channel)
        {
            this.PushContext(new OperationContext(channel));
        }

        public OperationContextScope(OperationContext context)
        {
            this.PushContext(context);
        }

        private static OperationContextScope CurrentScope
        {
            get
            {
                return ServiceModelAppSettings.DisableOperationContextAsyncFlow ? legacyCurrentScope : currentScope.Value;
            }

            set
            {
                if (ServiceModelAppSettings.DisableOperationContextAsyncFlow)
                {
                    legacyCurrentScope = value;
                }
                else
                {
                    currentScope.Value = value;
                }
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.PopContext();
            }
        }

        void PushContext(OperationContext context)
        {
            bool isAsyncFlowEnabled = OperationContext.ShouldUseAsyncLocalContext;

            this.currentContext = context;

            if (isAsyncFlowEnabled)
            {
                OperationContext.EnableAsyncFlow(this.currentContext);
            }

            CurrentScope = this;
            OperationContext.Current = this.currentContext;
        }

        void PopContext()
        {
            if (ServiceModelAppSettings.DisableOperationContextAsyncFlow && this.thread != Thread.CurrentThread)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidContextScopeThread0)));

            if (CurrentScope != this)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInterleavedContextScopes0)));

            if (OperationContext.Current != this.currentContext)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxContextModifiedInsideScope0)));

            CurrentScope = this.originalScope;
            OperationContext.Current = this.originalContext;

            if (this.currentContext != null)
                this.currentContext.SetClientReply(null, false);
        }
    }
}

