//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime;
    using System.Security.Authentication;
    using System.ServiceModel.Security;

    abstract class StreamSecurityUpgradeInitiatorAsyncResult : AsyncResult
    {
        Stream originalStream;
        SecurityMessageProperty remoteSecurity;
        Stream upgradedStream;

        static AsyncCallback onAuthenticateAsClient = Fx.ThunkCallback(new AsyncCallback(OnAuthenticateAsClient));

        public StreamSecurityUpgradeInitiatorAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            // empty
        }

        public void Begin(Stream stream)
        {
            this.originalStream = stream;
            IAsyncResult result;

            try
            {
                result = this.OnBeginAuthenticateAsClient(this.originalStream, onAuthenticateAsClient);
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

            CompleteAuthenticateAsClient(result);
            base.Complete(true);
        }

        void CompleteAuthenticateAsClient(IAsyncResult result)
        {
            try
            {
                this.upgradedStream = this.OnCompleteAuthenticateAsClient(result);
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
            StreamSecurityUpgradeInitiatorAsyncResult thisPtr = AsyncResult.End<StreamSecurityUpgradeInitiatorAsyncResult>(result);
            remoteSecurity = thisPtr.remoteSecurity;
            return thisPtr.upgradedStream;
        }

        static void OnAuthenticateAsClient(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            StreamSecurityUpgradeInitiatorAsyncResult thisPtr =
                (StreamSecurityUpgradeInitiatorAsyncResult)result.AsyncState;

            Exception completionException = null;
            try
            {
                thisPtr.CompleteAuthenticateAsClient(result);
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
            thisPtr.Complete(false, completionException);
        }

        protected abstract IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback);
        protected abstract Stream OnCompleteAuthenticateAsClient(IAsyncResult result);
        protected abstract SecurityMessageProperty ValidateCreateSecurity();
    }
}
