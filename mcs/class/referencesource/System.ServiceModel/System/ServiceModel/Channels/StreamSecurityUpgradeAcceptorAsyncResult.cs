//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime;
    using System.Security.Authentication;
    using System.ServiceModel.Security;
    using System.ServiceModel.Diagnostics;

    abstract class StreamSecurityUpgradeAcceptorAsyncResult : TraceAsyncResult
    {
        SecurityMessageProperty remoteSecurity;
        Stream upgradedStream;

        static AsyncCallback onAuthenticateAsServer = Fx.ThunkCallback(new AsyncCallback(OnAuthenticateAsServer));

        protected StreamSecurityUpgradeAcceptorAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public void Begin(Stream stream)
        {
            IAsyncResult result;
            try
            {
                result = this.OnBegin(stream, onAuthenticateAsServer);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                    exception));
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                    SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
            }

            if (!result.CompletedSynchronously)
            {
                return;
            }

            CompleteAuthenticateAsServer(result);
            base.Complete(true);
        }

        void CompleteAuthenticateAsServer(IAsyncResult result)
        {
            try
            {
                this.upgradedStream = this.OnCompleteAuthenticateAsServer(result);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                    exception));
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                    SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
            }

            this.remoteSecurity = this.ValidateCreateSecurity();
        }

        public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            StreamSecurityUpgradeAcceptorAsyncResult thisPtr = AsyncResult.End<StreamSecurityUpgradeAcceptorAsyncResult>(result);
            remoteSecurity = thisPtr.remoteSecurity;
            return thisPtr.upgradedStream;
        }

        static void OnAuthenticateAsServer(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            StreamSecurityUpgradeAcceptorAsyncResult acceptUpgradeAsyncResult =
                (StreamSecurityUpgradeAcceptorAsyncResult)result.AsyncState;

            Exception completionException = null;
            try
            {
                acceptUpgradeAsyncResult.CompleteAuthenticateAsServer(result);
            }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                completionException = e;
            }
            acceptUpgradeAsyncResult.Complete(false, completionException);
        }

        protected abstract IAsyncResult OnBegin(Stream stream, AsyncCallback callback);
        protected abstract Stream OnCompleteAuthenticateAsServer(IAsyncResult result);
        protected abstract SecurityMessageProperty ValidateCreateSecurity();
    }

}
