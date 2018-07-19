//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Threading;

    public abstract class SecurityTokenProvider
    {
        protected SecurityTokenProvider() { }

        public virtual bool SupportsTokenRenewal
        {
            get { return false; }
        }

        public virtual bool SupportsTokenCancellation
        {
            get { return false; }
        }

        public SecurityToken GetToken(TimeSpan timeout)
        {
            SecurityToken token = this.GetTokenCore(timeout);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.TokenProviderUnableToGetToken, this)));
            }
            return token;
        }

        public IAsyncResult BeginGetToken(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginGetTokenCore(timeout, callback, state);
        }

        public SecurityToken EndGetToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SecurityToken token = this.EndGetTokenCore(result);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.TokenProviderUnableToGetToken, this)));
            }
            return token;
        }

        public SecurityToken RenewToken(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            if (tokenToBeRenewed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenToBeRenewed");
            }
            SecurityToken token = this.RenewTokenCore(timeout, tokenToBeRenewed);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.TokenProviderUnableToRenewToken, this)));
            }
            return token;
        }

        public IAsyncResult BeginRenewToken(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            if (tokenToBeRenewed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenToBeRenewed");
            }
            return this.BeginRenewTokenCore(timeout, tokenToBeRenewed, callback, state);
        }

        public SecurityToken EndRenewToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SecurityToken token = this.EndRenewTokenCore(result);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.TokenProviderUnableToRenewToken, this)));
            }
            return token;
        }

        public void CancelToken(TimeSpan timeout, SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.CancelTokenCore(timeout, token);
        }

        public IAsyncResult BeginCancelToken(TimeSpan timeout, SecurityToken token, AsyncCallback callback, object state)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return this.BeginCancelTokenCore(timeout, token, callback, state);
        }

        public void EndCancelToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            this.EndCancelTokenCore(result);
        }

        // protected methods
        protected abstract SecurityToken GetTokenCore(TimeSpan timeout);

        protected virtual SecurityToken RenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.TokenRenewalNotSupported, this)));
        }

        protected virtual void CancelTokenCore(TimeSpan timeout, SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.TokenCancellationNotSupported, this)));
        }

        protected virtual IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            SecurityToken token = this.GetToken(timeout);
            return new SecurityTokenAsyncResult(token, callback, state);
        }

        protected virtual SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return SecurityTokenAsyncResult.End(result);
        }

        protected virtual IAsyncResult BeginRenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            SecurityToken token = this.RenewTokenCore(timeout, tokenToBeRenewed);
            return new SecurityTokenAsyncResult(token, callback, state);
        }

        protected virtual SecurityToken EndRenewTokenCore(IAsyncResult result)
        {
            return SecurityTokenAsyncResult.End(result);
        }

        protected virtual IAsyncResult BeginCancelTokenCore(TimeSpan timeout, SecurityToken token, AsyncCallback callback, object state)
        {
            this.CancelToken(timeout, token);
            return new SecurityTokenAsyncResult(null, callback, state);
        }

        protected virtual void EndCancelTokenCore(IAsyncResult result)
        {
            SecurityTokenAsyncResult.End(result);
        }

        internal protected class SecurityTokenAsyncResult : IAsyncResult
        {
            SecurityToken token;
            object state;
            ManualResetEvent manualResetEvent;
            object thisLock = new object();

            public SecurityTokenAsyncResult(SecurityToken token, AsyncCallback callback, object state)
            {
                this.token = token;
                this.state = state;

                if (callback != null)
                {
                    try
                    {
                        callback(this);
                    }
#pragma warning suppress 56500
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.AsyncCallbackException), e);
                    }
                }
            }

            public object AsyncState
            {
                get { return this.state; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get 
                {
                    if (this.manualResetEvent != null)
                    {
                        return this.manualResetEvent;
                    }

                    lock (thisLock)
                    {
                        if (this.manualResetEvent == null)
                        {
                            this.manualResetEvent = new ManualResetEvent(true);
                        }
                    }
                    return this.manualResetEvent; 
                }
            }

            public bool CompletedSynchronously
            {
                get { return true; }
            }

            public bool IsCompleted
            {
                get { return true; }
            }

            public static SecurityToken End(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }

                SecurityTokenAsyncResult completedResult = result as SecurityTokenAsyncResult;
                if (completedResult == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidAsyncResult), "result"));
                }

                return completedResult.token;
            }
        }
    }
}
