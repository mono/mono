//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;

    public sealed class OperationContext : IExtensibleObject<OperationContext>
    {
        [ThreadStatic]
        static Holder currentContext;

        ServiceChannel channel;
        Message clientReply;
        bool closeClientReply;
        ExtensionCollection<OperationContext> extensions;
        ServiceHostBase host;
        RequestContext requestContext;
        Message request;
        InstanceContext instanceContext;
        bool isServiceReentrant = false;
        internal IPrincipal threadPrincipal;
        TransactionRpcFacet txFacet;
        MessageProperties outgoingMessageProperties;
        MessageHeaders outgoingMessageHeaders;
        MessageVersion outgoingMessageVersion;
        EndpointDispatcher endpointDispatcher;

        public event EventHandler OperationCompleted;

        public OperationContext(IContextChannel channel)
        {
            if (channel == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("channel"));

            ServiceChannel serviceChannel = channel as ServiceChannel;

            //Could be a TransparentProxy
            if (serviceChannel == null)
            {
                serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            }

            if (serviceChannel != null)
            {
                this.outgoingMessageVersion = serviceChannel.MessageVersion;
                this.channel = serviceChannel;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidChannelToOperationContext)));
            }
        }

        internal OperationContext(ServiceHostBase host)
            : this(host, MessageVersion.Soap12WSAddressing10)
        {
        }

        internal OperationContext(ServiceHostBase host, MessageVersion outgoingMessageVersion)
        {
            if (outgoingMessageVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("outgoingMessageVersion"));

            this.host = host;
            this.outgoingMessageVersion = outgoingMessageVersion;
        }

        internal OperationContext(RequestContext requestContext, Message request, ServiceChannel channel, ServiceHostBase host)
        {
            this.channel = channel;
            this.host = host;
            this.requestContext = requestContext;
            this.request = request;
            this.outgoingMessageVersion = channel.MessageVersion;
        }

        public IContextChannel Channel
        {
            get { return this.GetCallbackChannel<IContextChannel>(); }
        }

        public static OperationContext Current
        {
            get
            {
                return CurrentHolder.Context;
            }

            set
            {
                CurrentHolder.Context = value;
            }
        }

        internal static Holder CurrentHolder
        {
            get
            {
                Holder holder = OperationContext.currentContext;
                if (holder == null)
                {
                    holder = new Holder();
                    OperationContext.currentContext = holder;
                }
                return holder;
            }
        }

        public EndpointDispatcher EndpointDispatcher
        {
            get
            {
                return this.endpointDispatcher;
            }
            set
            {
                this.endpointDispatcher = value;
            }
        }

        public bool IsUserContext
        {
            get
            {
                return (this.request == null);
            }
        }

        public IExtensionCollection<OperationContext> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ExtensionCollection<OperationContext>(this);
                }
                return this.extensions;
            }
        }

        internal bool IsServiceReentrant
        {
            get { return this.isServiceReentrant; }
            set { this.isServiceReentrant = value; }
        }

        public bool HasSupportingTokens
        {
            get
            {
                MessageProperties properties = this.IncomingMessageProperties;
                return properties != null && properties.Security != null &&
                    properties.Security.HasIncomingSupportingTokens;
            }
        }

        public ServiceHostBase Host
        {
            get { return this.host; }
        }

        internal Message IncomingMessage
        {
            get { return this.clientReply ?? this.request; }
        }

        internal ServiceChannel InternalServiceChannel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        internal bool HasOutgoingMessageHeaders
        {
            get { return (this.outgoingMessageHeaders != null); }
        }

        public MessageHeaders OutgoingMessageHeaders
        {
            get
            {
                if (this.outgoingMessageHeaders == null)
                    this.outgoingMessageHeaders = new MessageHeaders(this.OutgoingMessageVersion);

                return this.outgoingMessageHeaders;
            }
        }

        internal bool HasOutgoingMessageProperties
        {
            get { return (this.outgoingMessageProperties != null); }
        }

        public MessageProperties OutgoingMessageProperties
        {
            get
            {
                if (this.outgoingMessageProperties == null)
                    this.outgoingMessageProperties = new MessageProperties();

                return this.outgoingMessageProperties;
            }
        }

        internal MessageVersion OutgoingMessageVersion
        {
            get { return this.outgoingMessageVersion; }
        }

        public MessageHeaders IncomingMessageHeaders
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                    return message.Headers;
                else
                    return null;
            }
        }

        public MessageProperties IncomingMessageProperties
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                    return message.Properties;
                else
                    return null;
            }
        }

        public MessageVersion IncomingMessageVersion
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                    return message.Version;
                else
                    return null;
            }
        }

        public InstanceContext InstanceContext
        {
            get { return this.instanceContext; }
        }

        public RequestContext RequestContext
        {
            get { return this.requestContext; }
            set { this.requestContext = value; }
        }

        public ServiceSecurityContext ServiceSecurityContext
        {
            get
            {
                MessageProperties properties = this.IncomingMessageProperties;
                if (properties != null && properties.Security != null)
                {
                    return properties.Security.ServiceSecurityContext;
                }
                return null;
            }
        }

        public string SessionId
        {
            get
            {
                if (this.channel != null)
                {
                    IChannel inner = this.channel.InnerChannel;
                    if (inner != null)
                    {
                        ISessionChannel<IDuplexSession> duplex = inner as ISessionChannel<IDuplexSession>;
                        if ((duplex != null) && (duplex.Session != null))
                            return duplex.Session.Id;

                        ISessionChannel<IInputSession> input = inner as ISessionChannel<IInputSession>;
                        if ((input != null) && (input.Session != null))
                            return input.Session.Id;

                        ISessionChannel<IOutputSession> output = inner as ISessionChannel<IOutputSession>;
                        if ((output != null) && (output.Session != null))
                            return output.Session.Id;
                    }
                }
                return null;
            }
        }

        public ICollection<SupportingTokenSpecification> SupportingTokens
        {
            get
            {
                MessageProperties properties = this.IncomingMessageProperties;
                if (properties != null && properties.Security != null)
                {
                    return new System.Collections.ObjectModel.ReadOnlyCollection<SupportingTokenSpecification>(
                        properties.Security.IncomingSupportingTokens);
                }
                return null;
            }
        }

        internal IPrincipal ThreadPrincipal
        {
            get { return this.threadPrincipal; }
            set { this.threadPrincipal = value; }
        }

        public ClaimsPrincipal ClaimsPrincipal
        {
            get;
            internal set;
        }

        internal TransactionRpcFacet TransactionFacet
        {
            get { return this.txFacet; }
            set { this.txFacet = value; }
        }

        internal void ClearClientReplyNoThrow()
        {
            this.clientReply = null;
        }

        internal void FireOperationCompleted()
        {
            try
            {
                EventHandler handler = this.OperationCompleted;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        public T GetCallbackChannel<T>()
        {
            if (this.channel == null || this.IsUserContext)
                return default(T);

            // yes, we might throw InvalidCastException here.  Is it really
            // better to check and throw something else instead?
            return (T)this.channel.Proxy;
        }

        internal void ReInit(RequestContext requestContext, Message request, ServiceChannel channel)
        {
            this.requestContext = requestContext;
            this.request = request;
            this.channel = channel;
        }

        internal void Recycle()
        {
            this.requestContext = null;
            this.request = null;
            this.extensions = null;
            this.instanceContext = null;
            this.threadPrincipal = null;
            this.txFacet = null;
            this.SetClientReply(null, false);
        }

        internal void SetClientReply(Message message, bool closeMessage)
        {
            Message oldClientReply = null;

            if (!object.Equals(message, this.clientReply))
            {
                if (this.closeClientReply && (this.clientReply != null))
                {
                    oldClientReply = this.clientReply;
                }

                this.clientReply = message;
            }

            this.closeClientReply = closeMessage;

            if (oldClientReply != null)
            {
                oldClientReply.Close();
            }
        }

        public void SetTransactionComplete()
        {
            if (this.txFacet == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoTransactionInContext)));
            }

            this.txFacet.Completed();
        }

        internal void SetInstanceContext(InstanceContext instanceContext)
        {
            this.instanceContext = instanceContext;
        }

        internal class Holder
        {
            OperationContext context;

            public OperationContext Context
            {
                get
                {
                    return this.context;
                }

                set
                {
                    this.context = value;
                }
            }
        }
    }
}

