//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using IM = System.IdentityModel;

    /// <summary>
    /// A <see cref="ChannelFactory" /> that produces <see cref="WSTrustChannel" /> objects used to 
    /// communicate to a WS-Trust endpoint.
    /// </summary>
    [ComVisible(false)]
    public class WSTrustChannelFactory : ChannelFactory<IWSTrustChannelContract>
    {
        //
        // NOTE: The properties on this class are designed to facilitate ease of use of the component and
        //       to reduce the complexity of the constructors. The base class already gifts us with 8 constructor
        //       overloads.
        //
        //       Therefore, it is advisable that the fields *not* be used unless absolutely required.
        //

        /// <summary>
        /// These fields represent the property values that are "locked down" once the first channel is created.
        /// </summary>
        class WSTrustChannelLockedProperties
        {
            public TrustVersion TrustVersion;
            public WSTrustSerializationContext Context;
            public WSTrustRequestSerializer RequestSerializer;
            public WSTrustResponseSerializer ResponseSerializer;
        }

        //
        // Once we create a channel, our properties can be locked down.
        //
        object _factoryLock = new object();
        bool _locked = false;
        WSTrustChannelLockedProperties _lockedProperties;

        //
        // The TrustVersion property can be set to an instance of TrustVersion.WSTrust13 or TrustVersion.WSTrustFeb2005
        // to generate the built-in serializers for these trust namespaces.
        //
        TrustVersion _trustVersion;

        //
        // These fields contain the values used to construct the WSTrustSerializationContext used by the channels
        // we generate.
        //
        // _securityTokenResolver and _useKeyTokenResolver imply special behavior if they are null; however,
        // _securityTokenHandlerCollectionManager is not permitted to be null.
        //
        SecurityTokenResolver _securityTokenResolver;
        SecurityTokenResolver _useKeyTokenResolver;
        SecurityTokenHandlerCollectionManager _securityTokenHandlerCollectionManager
            = SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager();

        //
        // These serializers determine how the channels serialize RST and RSTR messages.
        //
        WSTrustRequestSerializer _wsTrustRequestSerializer;
        WSTrustResponseSerializer _wsTrustResponseSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class.
        /// </summary>
        public WSTrustChannelFactory()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class with a specified endpoint 
        /// configuration name.
        /// </summary>
        /// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
        public WSTrustChannelFactory(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class.
        /// </summary>
        /// <param name="binding">The <see cref="Binding" /> specified for the channels produced by the factory</param>
        public WSTrustChannelFactory(Binding binding)
            : base(binding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class with a specified endpoint.
        /// </summary>
        /// <param name="endpoint">The <see cref="ServiceEndpoint" />for the channels produced by the factory.</param>
        public WSTrustChannelFactory(ServiceEndpoint endpoint)
            : base(endpoint)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class associated with a specified 
        /// name for the endpoint configuration and remote address.
        /// </summary>
        /// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
        /// <param name="remoteAddress">The <see cref="EndpointAddress" /> that provides the location of the service.</param>
        public WSTrustChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class with a specified binding 
        /// and endpoint address.
        /// </summary>
        /// <param name="binding">The <see cref="Binding" /> specified for the channels produced by the factory</param>
        /// <param name="remoteAddress">The <see cref="EndpointAddress" /> that provides the location of the service.</param>        
        public WSTrustChannelFactory(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WSTrustChannelFactory" /> class with a specified binding
        /// and remote address.
        /// </summary>
        /// <param name="binding">The <see cref="Binding" /> specified for the channels produced by the factory</param>
        /// <param name="remoteAddress">The <see cref="EndpointAddress" /> that provides the location of the service.</param>
        public WSTrustChannelFactory(Binding binding, string remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Gets or sets the version of WS-Trust the created channels will use for serializing messages.
        /// </summary>
        /// <remarks>
        /// <para>If this property is not set, created channels will use the <see cref="TrustVersion" /> set on any
        /// <see cref="SecurityBindingElement" /> found on the channel factory's Endpoint object if one exists.
        /// </para>
        /// <para>This class will not support changing the value of this property after a channel is created.</para>
        /// </remarks>        
        public TrustVersion TrustVersion
        {
            get
            {
                return _trustVersion;
            }
            set
            {
                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _trustVersion = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenHandlerCollectionManager" /> containing the set of
        /// <see cref="SecurityTokenHandler" /> objects used by created channels for serializing and validating 
        /// tokens found in  WS-Trust messages.
        /// </summary>
        /// <remarks>
        /// This class will not support changing the value of this property after a channel is created.        
        /// </remarks> 
        public SecurityTokenHandlerCollectionManager SecurityTokenHandlerCollectionManager
        {
            get
            {
                return _securityTokenHandlerCollectionManager;
            }
            set
            {
                if (value == null)
                {
                    throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _securityTokenHandlerCollectionManager = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenResolver"/> used to resolve security token references found in most
        /// elements of WS-Trust messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is not set created channels will use the ClientCertificate set on the factory's 
        /// Endpoint's ClientCredentials behavior to create a resolver. If no such certificate is found, an empty
        /// resolver is used.
        /// </para>
        /// <para>
        /// This class will not support changing the value of this property after a channel is created.
        /// </para>
        /// </remarks>
        public SecurityTokenResolver SecurityTokenResolver
        {
            get
            {
                return _securityTokenResolver;
            }
            set
            {
                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _securityTokenResolver = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="SecurityTokenResolver"/> used to resolve security token references found in the
        /// UseKey element of RST messages as well as the RenewTarget element found in RST messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is not set an empty resolver is used.
        /// </para>
        /// <para>
        /// This class will not support changing the value of this property after a channel is created.
        /// </para>
        /// </remarks>
        public SecurityTokenResolver UseKeyTokenResolver
        {
            get
            {
                return _useKeyTokenResolver;
            }
            set
            {
                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _useKeyTokenResolver = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the WSTrustRequestSerializer to use for serializing RequestSecurityTokens messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is not set, either <see cref="WSTrust13RequestSerializer" /> or 
        /// <see cref="WSTrustFeb2005RequestSerializer" /> will be used. The serializer will correspond to the 
        /// version of WS-Trust indicated by the <see cref="TrustVersion" /> property.
        /// </para>
        /// <para>
        /// This class will not support changing the value of this property after a channel is created.
        /// </para>
        /// </remarks>
        public WSTrustRequestSerializer WSTrustRequestSerializer
        {
            get
            {
                return _wsTrustRequestSerializer;
            }
            set
            {
                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _wsTrustRequestSerializer = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the WSTrustResponseSerializer to use for serializing RequestSecurityTokensResponse messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is not set, either <see cref="WSTrust13ResponseSerializer" /> or 
        /// <see cref="WSTrustFeb2005ResponseSerializer" /> will be used. The serializer will correspond to the 
        /// version of WS-Trust indicated by the <see cref="TrustVersion" /> property.
        /// </para>
        /// <para>
        /// This class will not support changing the value of this property after a channel is created.
        /// </para>
        /// </remarks>
        public WSTrustResponseSerializer WSTrustResponseSerializer
        {
            get
            {
                return _wsTrustResponseSerializer;
            }
            set
            {
                lock (_factoryLock)
                {
                    if (_locked)
                    {
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3287));
                    }
                    _wsTrustResponseSerializer = value;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="WSTrustChannel" /> that is used to send messages to a service at a specific 
        /// endpoint address through a specified transport address.
        /// </summary>
        /// <param name="address">The <see cref="EndpointAddress" /> that provides the location of the service.</param>
        /// <param name="via">The <see cref="Uri" /> that contains the transport address to which the channel sends messages.</param>
        /// <returns></returns>
        public override IWSTrustChannelContract CreateChannel(EndpointAddress address, Uri via)
        {
            IWSTrustChannelContract innerChannel = base.CreateChannel(address, via);

            WSTrustChannelLockedProperties lockedProperties = GetLockedProperties();
            return CreateTrustChannel(innerChannel,
                                       lockedProperties.TrustVersion,
                                       lockedProperties.Context,
                                       lockedProperties.RequestSerializer,
                                       lockedProperties.ResponseSerializer);
        }

        /// <summary>
        /// Creates a <see cref="WSTrustChannel" /> using parameters that reflect the configuration of
        /// this factory.
        /// </summary>
        /// <param name="innerChannel">The channel created by the base class capable of sending and 
        /// receiving messages.</param>
        /// <param name="trustVersion">The version of WS-Trust that should be used.</param>
        /// <param name="context">
        /// The <see cref="WSTrustSerializationContext" /> that should be used to serialize WS-Trust messages.
        /// </param>
        /// <param name="requestSerializer">
        /// The <see cref="WSTrustRequestSerializer" /> that should be used to serialize WS-Trust request messages.
        /// </param>
        /// <param name="responseSerializer">
        /// The <see cref="WSTrustResponseSerializer" /> that should be used to serialize WS-Trust response messages.
        /// </param>
        /// <returns></returns>
        protected virtual WSTrustChannel CreateTrustChannel(IWSTrustChannelContract innerChannel,
                                                             TrustVersion trustVersion,
                                                             WSTrustSerializationContext context,
                                                             WSTrustRequestSerializer requestSerializer,
                                                             WSTrustResponseSerializer responseSerializer)
        {
            return new WSTrustChannel(this, innerChannel, trustVersion, context, requestSerializer, responseSerializer);
        }

        private WSTrustChannelLockedProperties GetLockedProperties()
        {
            lock (_factoryLock)
            {
                if (_lockedProperties == null)
                {
                    WSTrustChannelLockedProperties tmpLockedProperties = new WSTrustChannelLockedProperties();
                    tmpLockedProperties.TrustVersion = GetTrustVersion();
                    tmpLockedProperties.Context = CreateSerializationContext();
                    tmpLockedProperties.RequestSerializer = GetRequestSerializer(tmpLockedProperties.TrustVersion);
                    tmpLockedProperties.ResponseSerializer = GetResponseSerializer(tmpLockedProperties.TrustVersion);

                    _lockedProperties = tmpLockedProperties;
                    _locked = true;
                }
                return _lockedProperties;
            }
        }

        private WSTrustRequestSerializer GetRequestSerializer(TrustVersion trustVersion)
        {
            Fx.Assert(trustVersion != null, "trustVersion != null");

            if (_wsTrustRequestSerializer != null)
            {
                return _wsTrustRequestSerializer;
            }

            if (trustVersion == TrustVersion.WSTrust13)
            {
                return new WSTrust13RequestSerializer();
            }
            else if (trustVersion == TrustVersion.WSTrustFeb2005)
            {
                return new WSTrustFeb2005RequestSerializer();
            }
            else
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError( new NotSupportedException(SR.GetString(SR.ID3137, trustVersion.ToString())));
            }
        }

        private WSTrustResponseSerializer GetResponseSerializer(TrustVersion trustVersion)
        {
            Fx.Assert(trustVersion != null, "trustVersion != null");

            if (_wsTrustResponseSerializer != null)
            {
                return _wsTrustResponseSerializer;
            }

            if (trustVersion == TrustVersion.WSTrust13)
            {
                return new WSTrust13ResponseSerializer();
            }
            else if (trustVersion == TrustVersion.WSTrustFeb2005)
            {
                return new WSTrustFeb2005ResponseSerializer();
            }
            else
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID3137, trustVersion.ToString())));
            }
        }

        private TrustVersion GetTrustVersion()
        {
            TrustVersion trustVersion = _trustVersion;

            if (trustVersion == null)
            {
                BindingElementCollection elements = Endpoint.Binding.CreateBindingElements();
                SecurityBindingElement sbe = elements.Find<SecurityBindingElement>();
                if (null == sbe)
                {
                    throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString(SR.ID3269)));
                }
                trustVersion = sbe.MessageSecurityVersion.TrustVersion;
            }

            return trustVersion;
        }

        /// <summary>
        /// Creates a <see cref="WSTrustSerializationContext" /> used by <see cref="WSTrustChannel" /> objects created
        /// by this factory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="WSTrustChannelFactory.SecurityTokenResolver" /> is set to null, the 
        /// ClientCertificate set on the factory's  Endpoint's ClientCredentials behavior will be used to 
        /// create a resolver. If no such certificate is found, an empty resolver is used.
        /// </para>
        /// <para>
        /// If <see cref="WSTrustChannelFactory.UseKeyTokenResolver" /> is set to null, an empty resolver
        /// will be used.
        /// </para>
        /// </remarks>
        /// <returns>A WSTrustSerializationContext initialized with the trust client's properties.</returns>
        protected virtual WSTrustSerializationContext CreateSerializationContext()
        {
            //
            // Create a resolver with the ClientCredential's ClientCertificate if a resolver is not set.
            //
            SecurityTokenResolver resolver = _securityTokenResolver;
            if (resolver == null)
            {
                ClientCredentials factoryCredentials = Credentials;
                if (null != factoryCredentials.ClientCertificate && null != factoryCredentials.ClientCertificate.Certificate)
                {
                    List<SecurityToken> clientCredentialTokens = new List<SecurityToken>();
                    clientCredentialTokens.Add(new X509SecurityToken(factoryCredentials.ClientCertificate.Certificate));
                    resolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(clientCredentialTokens.AsReadOnly(), false);
                }
            }

            //
            // If it is _still_ null, then make it empty.
            //
            if (resolver == null)
            {
                resolver = EmptySecurityTokenResolver.Instance;
            }

            //
            // UseKeyTokenResolver is empty if null.
            //
            SecurityTokenResolver useKeyResolver = _useKeyTokenResolver ?? EmptySecurityTokenResolver.Instance;

            return new WSTrustSerializationContext(_securityTokenHandlerCollectionManager, resolver, useKeyResolver);
        }
    }
}
