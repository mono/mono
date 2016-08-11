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
        static AsyncLocal<OperationContextScope> currentScope = new AsyncLocal<OperationContextScope>();

        OperationContext currentContext;
        bool disposed;
        readonly OperationContext originalContext = OperationContext.Current;
        readonly OperationContextScope originalScope = OperationContextScope.currentScope.Value;

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
            OperationContextScope.currentScope.Value = this;
            OperationContext.Current = this.currentContext;
        }

        void PopContext()
        {
            if (OperationContextScope.currentScope.Value != this)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInterleavedContextScopes0)));

            if (OperationContext.Current != this.currentContext)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxContextModifiedInsideScope0)));

            OperationContextScope.currentScope.Value = this.originalScope;
            OperationContext.Current = this.originalContext;

            if (this.currentContext != null)
                this.currentContext.SetClientReply(null, false);
        }
    }
}

