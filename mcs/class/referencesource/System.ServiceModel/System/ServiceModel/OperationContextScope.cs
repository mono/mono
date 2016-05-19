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
        static OperationContextScope currentScope;

        OperationContext currentContext;
        bool disposed;
        readonly OperationContext originalContext = OperationContext.Current;
        readonly OperationContextScope originalScope = OperationContextScope.currentScope;
        readonly Thread thread = Thread.CurrentThread;

        public OperationContextScope(IContextChannel channel)
        {
            this.PushContext(new OperationContext(channel));
        }

        public OperationContextScope(OperationContext context)
        {
            this.PushContext(context);
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
            this.currentContext = context;
            OperationContextScope.currentScope = this;
            OperationContext.Current = this.currentContext;
        }

        void PopContext()
        {
            if (this.thread != Thread.CurrentThread)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidContextScopeThread0)));

            if (OperationContextScope.currentScope != this)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInterleavedContextScopes0)));

            if (OperationContext.Current != this.currentContext)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxContextModifiedInsideScope0)));

            OperationContextScope.currentScope = this.originalScope;
            OperationContext.Current = this.originalContext;

            if (this.currentContext != null)
                this.currentContext.SetClientReply(null, false);
        }
    }
}

