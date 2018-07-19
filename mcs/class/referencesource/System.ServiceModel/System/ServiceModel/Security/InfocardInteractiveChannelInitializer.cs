//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class InfocardInteractiveChannelInitializer : IInteractiveChannelInitializer
    {
        ClientCredentials credentials;
        Binding binding;

        public InfocardInteractiveChannelInitializer(ClientCredentials credentials, Binding binding)
        {
            this.credentials = credentials;
            this.binding = binding;
        }

        public Binding Binding
        {
            get
            {
                return binding;
            }
        }

        public virtual IAsyncResult BeginDisplayInitializationUI(IClientChannel channel, AsyncCallback callback, object state)
        {
            return new GetTokenUIAsyncResult(binding, channel, this.credentials, callback, state);
        }

        public virtual void EndDisplayInitializationUI(IAsyncResult result)
        {
            GetTokenUIAsyncResult.End(result);
        }

    }

    internal class GetTokenUIAsyncResult : AsyncResult
    {

        IClientChannel proxy;
        ClientCredentials credentials;
        Uri relyingPartyIssuer;
        bool requiresInfoCard;
        Binding binding;


        static AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(GetTokenUIAsyncResult.Callback));

        internal GetTokenUIAsyncResult(Binding binding,
                                        IClientChannel channel,
                                        ClientCredentials credentials,
                                        AsyncCallback callback,
                                        object state)
            : base(callback, state)
        {
            this.credentials = credentials;
            this.proxy = channel;
            this.binding = binding;
            this.CallBegin(true);

        }

        void CallBegin(bool completedSynchronously)
        {

            IAsyncResult result = null;
            Exception exception = null;

            try
            {
                CardSpacePolicyElement[] chain;
                SecurityTokenManager tokenManager = credentials.CreateSecurityTokenManager();
                requiresInfoCard = InfoCardHelper.IsInfocardRequired(binding, credentials, tokenManager, proxy.RemoteAddress, out chain, out relyingPartyIssuer);
                MessageSecurityVersion bindingSecurityVersion = InfoCardHelper.GetBindingSecurityVersionOrDefault(binding);
                WSSecurityTokenSerializer tokenSerializer = WSSecurityTokenSerializer.DefaultInstance;
                result = credentials.GetInfoCardTokenCallback.BeginInvoke(requiresInfoCard, chain, tokenManager.CreateSecurityTokenSerializer(bindingSecurityVersion.SecurityTokenVersion), callback, this);

            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                exception = e;
            }
            if (exception == null)
            {
                if (!result.CompletedSynchronously)
                {
                    return;
                }

                this.CallEnd(result, out exception);
            }
            if (exception != null)
            {
                return;
            }


            this.CallComplete(completedSynchronously, null);
        }

        static void Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            GetTokenUIAsyncResult outer = (GetTokenUIAsyncResult)result.AsyncState;
            Exception exception = null;

            outer.CallEnd(result, out exception);
            outer.CallComplete(false, exception);

        }

        void CallEnd(IAsyncResult result, out Exception exception)
        {
            try
            {
                SecurityToken token = credentials.GetInfoCardTokenCallback.EndInvoke(result);

                ChannelParameterCollection channelParameters =
                           proxy.GetProperty<ChannelParameterCollection>();

                if (null != channelParameters)
                {
                    channelParameters.Add(new InfoCardChannelParameter(token, relyingPartyIssuer, requiresInfoCard));
                }
                exception = null;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                exception = e;
            }
        }

        void CallComplete(bool completedSynchronously, Exception exception)
        {
            this.Complete(completedSynchronously, exception);
        }

        internal static void End(IAsyncResult result)
        {
            AsyncResult.End<GetTokenUIAsyncResult>(result);
        }
    }
}


