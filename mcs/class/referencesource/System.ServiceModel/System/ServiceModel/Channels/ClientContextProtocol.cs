//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    class ClientContextProtocol : ContextProtocol, IContextManager
    {
        ContextMessageProperty context;
        bool contextInitialized;
        bool contextManagementEnabled;
        CookieContainer cookieContainer;
        IChannel owner;
        object thisLock;
        Uri uri;
        Uri callbackAddress;

        public ClientContextProtocol(ContextExchangeMechanism contextExchangeMechanism, Uri uri, IChannel owner, Uri callbackAddress, bool contextManagementEnabled)
            : base(contextExchangeMechanism)
        {
            if (contextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                this.cookieContainer = new CookieContainer();
            }
            this.context = ContextMessageProperty.Empty;
            this.contextManagementEnabled = contextManagementEnabled;
            this.owner = owner;
            this.thisLock = new object();
            this.uri = uri;
            this.callbackAddress = callbackAddress;
        }

        protected Uri Uri
        {
            get
            {
                return this.uri;
            }
        }

        bool IContextManager.Enabled
        {
            get
            {
                return this.contextManagementEnabled;
            }
            set
            {
                if (this.owner.State != CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR.GetString(SR.ChannelIsOpen)
                        ));
                }
                this.contextManagementEnabled = value;
            }
        }

        public IDictionary<string, string> GetContext()
        {
            if (!this.contextManagementEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR.GetString(SR.ContextManagementNotEnabled)
                    ));
            }
            return new Dictionary<string, string>(this.GetCurrentContext().Context);
        }

        public override void OnIncomingMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            ContextMessageProperty incomingContext = null;
            if (this.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                incomingContext = this.OnReceiveHttpCookies(message);
            }
            else
            {
                incomingContext = this.OnReceiveSoapContextHeader(message);
            }

            if (incomingContext != null)
            {
                if (this.contextManagementEnabled)
                {
                    EnsureInvariants(true, incomingContext);
                }
                else
                {
                    incomingContext.AddOrReplaceInMessage(message);
                }
            }

            // verify that the callback context was not sent on an incoming message
            if (message.Headers.FindHeader(CallbackContextMessageHeader.CallbackContextHeaderName, CallbackContextMessageHeader.CallbackContextHeaderNamespace) != -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(SR.GetString(SR.CallbackContextNotExpectedOnIncomingMessageAtClient, message.Headers.Action, CallbackContextMessageHeader.CallbackContextHeaderName, CallbackContextMessageHeader.CallbackContextHeaderNamespace)));
            }
        }

        public override void OnOutgoingMessage(Message message, RequestContext requestContext)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            ContextMessageProperty outgoingContext = null;

            if (ContextMessageProperty.TryGet(message, out outgoingContext))
            {
                if (this.contextManagementEnabled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR.GetString(SR.InvalidMessageContext)));
                }
            }

            if (this.ContextExchangeMechanism == ContextExchangeMechanism.ContextSoapHeader)
            {
                if (this.contextManagementEnabled)
                {
                    outgoingContext = GetCurrentContext();
                }

                if (outgoingContext != null)
                {
                    this.OnSendSoapContextHeader(message, outgoingContext);
                }
            }
            else if (this.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                if (this.contextManagementEnabled)
                {
                    this.OnSendHttpCookies(message, null);
                }
                else
                {
                    this.OnSendHttpCookies(message, outgoingContext);
                }
            }

            // serialize the callback context if the property was supplied
            CallbackContextMessageProperty callbackContext;
            if (CallbackContextMessageProperty.TryGet(message, out callbackContext))
            {
                // see if the callbackaddress is already set on the CCMP, if it is set use that
                // else if callbackaddress is set on the binding, use that
                EndpointAddress callbackAddress = callbackContext.CallbackAddress;
                if (callbackAddress == null && this.callbackAddress != null)
                {
                    callbackAddress = callbackContext.CreateCallbackAddress(this.callbackAddress);
                }
                // add the CallbackContextMessageHeader only if we have a valid CallbackAddress 
                if (callbackAddress != null)
                {
                    if (this.ContextExchangeMechanism != ContextExchangeMechanism.ContextSoapHeader)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CallbackContextOnlySupportedInSoap)));
                    }
                    message.Headers.Add(new CallbackContextMessageHeader(callbackAddress, message.Version.Addressing));
                }
            }
        }

        public void SetContext(IDictionary<string, string> context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            ContextMessageProperty newContext = new ContextMessageProperty(context);

            EnsureInvariants(false, newContext);

            if (this.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                lock (this.cookieContainer)
                {
                    this.cookieContainer.SetCookies(this.Uri, GetCookieHeaderFromContext(newContext));
                }
            }
        }

        //Called to update local context
        //1) From SetContext(to update client provided context)
        //2) From OnReceive*(to update server issued)
        void EnsureInvariants(bool isServerIssued, ContextMessageProperty newContext)
        {
            //Cannot SetContext when ContextManagement not enabled.
            if (!this.contextManagementEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR.GetString(SR.ContextManagementNotEnabled)
                    ));
            }

            //Cannot reset context after initialized in server case.
            if ((isServerIssued && !this.contextInitialized) ||
                (this.owner.State == CommunicationState.Created))
            {
                lock (this.thisLock)
                {
                    if ((isServerIssued && !this.contextInitialized) ||
                        (this.owner.State == CommunicationState.Created))
                    {
                        this.context = newContext;
                        this.contextInitialized = true;
                        return;
                    }
                }
            }

            if (isServerIssued)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(
                    SR.GetString(SR.InvalidContextReceived)
                    ));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR.GetString(SR.CachedContextIsImmutable)
                    ));
            }

        }


        string GetCookieHeaderFromContext(ContextMessageProperty contextMessageProperty)
        {
            if (contextMessageProperty.Context.Count == 0)
            {
                return HttpCookieToolbox.RemoveContextHttpCookieHeader;
            }
            else
            {
                return HttpCookieToolbox.EncodeContextAsHttpSetCookieHeader(contextMessageProperty, this.Uri);
            }
        }

        ContextMessageProperty GetCurrentContext()
        {
            ContextMessageProperty result;

            if (this.cookieContainer != null)
            {
                lock (this.cookieContainer)
                {
                    // This is to allow for the possibility of that the cookie has expired
                    if (this.cookieContainer.GetCookies(this.Uri)[HttpCookieToolbox.ContextHttpCookieName] == null)
                    {
                        result = ContextMessageProperty.Empty;
                    }
                    else
                    {
                        result = this.context;
                    }
                }
            }
            else
            {
                result = this.context;
            }

            return result;
        }

        ContextMessageProperty OnReceiveHttpCookies(Message message)
        {
            ContextMessageProperty newContext = null;
            object property;
            if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out property))
            {
                HttpResponseMessageProperty httpResponse = property as HttpResponseMessageProperty;
                if (httpResponse != null)
                {
                    string setCookieHeader = httpResponse.Headers[HttpResponseHeader.SetCookie];

                    if (!string.IsNullOrEmpty(setCookieHeader))
                    {
                        lock (this.cookieContainer)
                        {
                            if (!string.IsNullOrEmpty(setCookieHeader))
                            {
                                this.cookieContainer.SetCookies(this.Uri, setCookieHeader);
                                HttpCookieToolbox.TryCreateFromHttpCookieHeader(setCookieHeader, out newContext);
                            }

                            if (!this.contextManagementEnabled)
                            {
                                this.cookieContainer.SetCookies(this.Uri, HttpCookieToolbox.RemoveContextHttpCookieHeader);
                            }
                        }
                    }
                }
            }
            return newContext;
        }

        ContextMessageProperty OnReceiveSoapContextHeader(Message message)
        {
            ContextMessageProperty messageProperty = ContextMessageHeader.GetContextFromHeaderIfExists(message);
            if (messageProperty != null)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        TraceCode.ContextProtocolContextRetrievedFromMessage, SR.GetString(SR.TraceCodeContextProtocolContextRetrievedFromMessage),
                        this);
                }
            }
            return messageProperty;
        }


        void OnSendHttpCookies(Message message, ContextMessageProperty context)
        {
            string cookieHeader = null;

            if (this.contextManagementEnabled || context == null)
            {
                Fx.Assert(context == null, "Context should be null");

                lock (this.cookieContainer)
                {
                    cookieHeader = this.cookieContainer.GetCookieHeader(this.Uri);
                }
            }
            else
            {
                if (context != null) //User provided context is not null.
                {
                    string contextCookieHeader = this.GetCookieHeaderFromContext(context);

                    lock (this.cookieContainer)
                    {
                        this.cookieContainer.SetCookies(this.Uri, contextCookieHeader);
                        cookieHeader = this.cookieContainer.GetCookieHeader(this.Uri);
                        this.cookieContainer.SetCookies(this.Uri, HttpCookieToolbox.RemoveContextHttpCookieHeader);
                    }
                }
            }

            if (!string.IsNullOrEmpty(cookieHeader))
            {
                object tmpProperty;
                HttpRequestMessageProperty property = null;
                if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out tmpProperty))
                {
                    property = tmpProperty as HttpRequestMessageProperty;
                }
                if (property == null)
                {
                    property = new HttpRequestMessageProperty();
                    message.Properties.Add(HttpRequestMessageProperty.Name, property);
                }
                property.Headers.Add(HttpRequestHeader.Cookie, cookieHeader);
            }
        }
    }
}
