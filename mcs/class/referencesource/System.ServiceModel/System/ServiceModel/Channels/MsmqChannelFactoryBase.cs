//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  
namespace System.ServiceModel.Channels
{
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    abstract class MsmqChannelFactoryBase<TChannel> : TransportChannelFactory<TChannel>
    {
        MsmqUri.IAddressTranslator addressTranslator;
        Uri customDeadLetterQueue;
        bool durable;
        DeadLetterQueue deadLetterQueue;
        string deadLetterQueuePathName;
        bool exactlyOnce = true;
        TimeSpan timeToLive;
        MsmqTransportSecurity msmqTransportSecurity;
        bool useMsmqTracing;
        bool useSourceJournal;
        SecurityTokenManager securityTokenManager;

        protected MsmqChannelFactoryBase(MsmqBindingElementBase bindingElement, BindingContext context) :
            this(bindingElement, context, TransportDefaults.GetDefaultMessageEncoderFactory())
        { }


        protected MsmqChannelFactoryBase(MsmqBindingElementBase bindingElement, BindingContext context, MessageEncoderFactory encoderFactory)
            : base(bindingElement, context)
        {
            this.addressTranslator = bindingElement.AddressTranslator;
            this.customDeadLetterQueue = bindingElement.CustomDeadLetterQueue;
            this.durable = bindingElement.Durable;
            this.deadLetterQueue = bindingElement.DeadLetterQueue;
            this.exactlyOnce = bindingElement.ExactlyOnce;
            this.msmqTransportSecurity = new MsmqTransportSecurity(bindingElement.MsmqTransportSecurity);
            this.timeToLive = bindingElement.TimeToLive;
            this.useMsmqTracing = bindingElement.UseMsmqTracing;
            this.useSourceJournal = bindingElement.UseSourceJournal;

            if (this.MsmqTransportSecurity.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate)
            {
                InitializeSecurityTokenManager(context);
            }

            if (null != this.customDeadLetterQueue)
                this.deadLetterQueuePathName = MsmqUri.DeadLetterQueueAddressTranslator.UriToFormatName(this.customDeadLetterQueue);
        }

        internal MsmqUri.IAddressTranslator AddressTranslator
        {
            get
            {
                return this.addressTranslator;
            }
        }

        public Uri CustomDeadLetterQueue
        {
            get
            {
                return this.customDeadLetterQueue;
            }
        }

        public DeadLetterQueue DeadLetterQueue
        {
            get
            {
                return this.deadLetterQueue;
            }
        }

        internal string DeadLetterQueuePathName
        {
            get
            {
                return this.deadLetterQueuePathName;
            }
        }

        public bool Durable
        {
            get
            {
                return this.durable;
            }
        }

        public bool ExactlyOnce
        {
            get
            {
                return this.exactlyOnce;
            }
        }


        public MsmqTransportSecurity MsmqTransportSecurity
        {
            get
            {
                return this.msmqTransportSecurity;
            }
        }

        public override string Scheme
        {
            get { return this.addressTranslator.Scheme; }
        }

        public TimeSpan TimeToLive
        {
            get
            {
                return this.timeToLive;
            }
        }

        public SecurityTokenManager SecurityTokenManager
        {
            get { return this.securityTokenManager; }
        }

        public bool UseSourceJournal
        {
            get
            {
                return this.useSourceJournal;
            }
        }

        public bool UseMsmqTracing
        {
            get
            {
                return this.useMsmqTracing;
            }
        }

        internal bool IsMsmqX509SecurityConfigured
        {
            get
            {
                return (MsmqAuthenticationMode.Certificate == this.MsmqTransportSecurity.MsmqAuthenticationMode);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InitializeSecurityTokenManager(BindingContext context)
        {
            SecurityCredentialsManager credentials = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentials != null)
                this.securityTokenManager = credentials.CreateSecurityTokenManager();
        }


        internal SecurityTokenProvider CreateTokenProvider(EndpointAddress to, Uri via)
        {
            InitiatorServiceModelSecurityTokenRequirement x509Requirement = new InitiatorServiceModelSecurityTokenRequirement();
            x509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
            x509Requirement.TargetAddress = to;
            x509Requirement.Via = via;
            x509Requirement.KeyUsage = SecurityKeyUsage.Signature;
            x509Requirement.TransportScheme = this.Scheme;
            return this.SecurityTokenManager.CreateSecurityTokenProvider(x509Requirement);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityTokenProviderContainer CreateX509TokenProvider(EndpointAddress to, Uri via)
        {
            if (MsmqAuthenticationMode.Certificate == this.MsmqTransportSecurity.MsmqAuthenticationMode && this.SecurityTokenManager != null)
            {
                return new SecurityTokenProviderContainer(CreateTokenProvider(to, via));
            }
            else
            {
                return null;
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}
