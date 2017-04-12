//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Security.Claims;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using DiagnosticUtility = System.IdentityModel.DiagnosticUtility;
    using Message = System.ServiceModel.Channels.Message;
    using RequestContext = System.ServiceModel.Channels.RequestContext;
    using RST = System.IdentityModel.Protocols.WSTrust.RequestSecurityToken;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;
    using SR = System.ServiceModel.SR;
    using STS = System.IdentityModel.SecurityTokenService;
    using Fx = System.Runtime.Fx;

    /// <summary>
    /// Definition of Trust Contract Implementation. Implements the following ServiceContract interfaces,
    /// 1. IWSTrustFeb2005SyncContract
    /// 2. IWSTrust13SyncContract
    /// 3. IWSTrustFeb2005AsyncContract
    /// 4. IWSTrust13AsyncContract
    /// </summary>
    [ServiceBehavior(Name = WSTrustServiceContractConstants.ServiceBehaviorName, Namespace = WSTrustServiceContractConstants.Namespace, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WSTrustServiceContract : IWSTrustFeb2005SyncContract, IWSTrust13SyncContract, IWSTrustFeb2005AsyncContract, IWSTrust13AsyncContract, IWsdlExportExtension, IContractBehavior
    {
        const string soap11Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        const string soap12Namespace = "http://www.w3.org/2003/05/soap-envelope";

        SecurityTokenServiceConfiguration _securityTokenServiceConfiguration;

        event EventHandler<WSTrustRequestProcessingErrorEventArgs> _requestFailed;

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustServiceContract"/>
        /// </summary>
        /// <param name="securityTokenServiceConfiguration">Configuration object that initializes this instance.</param>
        public WSTrustServiceContract(SecurityTokenServiceConfiguration securityTokenServiceConfiguration)
        {
            if (securityTokenServiceConfiguration == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenServiceConfiguration");
            }

            _securityTokenServiceConfiguration = securityTokenServiceConfiguration;
        }

        /// <summary>
        /// Occurs when a Failure happens processing a Trust request from the 
        /// client.
        /// </summary>
        public event EventHandler<WSTrustRequestProcessingErrorEventArgs> RequestFailed
        {
            add { _requestFailed += value; }
            remove { _requestFailed -= value; }
        }

        /// <summary>
        /// Returns the <see cref="SecurityTokenResolver" /> that resolves the following security tokens contained
        /// in the current WCF message request's security header: protection token, endorsing, or signed endorsing
        /// supporting tokens.
        /// </summary>
        /// <remarks>
        /// This <see cref="SecurityTokenResolver" /> is used to resolve any SecurityTokenIdentifiers
        /// when deserializing RST UseKey elements or RST RenewTarget elements.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><param name="requestContext"/> is null.</exception>
        protected virtual SecurityTokenResolver GetSecurityHeaderTokenResolver(RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestContext");
            }

            List<SecurityToken> tokenList = new List<SecurityToken>();
            if (requestContext.RequestMessage != null
                && requestContext.RequestMessage.Properties != null
                && requestContext.RequestMessage.Properties.Security != null)
            {
                // Add tokens in message
                SecurityMessageProperty msgProperty = requestContext.RequestMessage.Properties.Security;
                if (msgProperty.ProtectionToken != null)
                {
                    tokenList.Add(msgProperty.ProtectionToken.SecurityToken);
                }
                if (msgProperty.HasIncomingSupportingTokens)
                {
                    foreach (SupportingTokenSpecification tokenSpec in msgProperty.IncomingSupportingTokens)
                    {
                        if (tokenSpec != null &&
                             (tokenSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing ||
                               tokenSpec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                        {
                            tokenList.Add(tokenSpec.SecurityToken);
                        }
                    }
                }

                if (msgProperty.InitiatorToken != null)
                {
                    tokenList.Add(msgProperty.InitiatorToken.SecurityToken);
                }
            }

            if (tokenList.Count > 0)
            {
                return SecurityTokenResolver.CreateDefaultSecurityTokenResolver(tokenList.AsReadOnly(), true);
            }
            else
            {
                return EmptySecurityTokenResolver.Instance;
            }
        }

        /// <summary>
        /// Returns the <see cref="SecurityTokenResolver" /> that will be used when resolving tokens and keys in the
        /// Trust message body.
        /// </summary>
        /// <returns>A <see cref="SecurityTokenResolver" /> instance.</returns>
        /// <seealso cref="GetSecurityHeaderTokenResolver"/>
        protected virtual SecurityTokenResolver GetRstSecurityTokenResolver()
        {
            if (_securityTokenServiceConfiguration != null)
            {
                SecurityTokenResolver tokenResolver = _securityTokenServiceConfiguration.SecurityTokenHandlers.Configuration.ServiceTokenResolver;

                if (tokenResolver != null && (!Object.ReferenceEquals(tokenResolver, EmptySecurityTokenResolver.Instance)))
                {
                    return tokenResolver;
                }
            }

            if (OperationContext.Current != null && OperationContext.Current.Host != null &&
                OperationContext.Current.Host.Description != null)
            {
                ServiceCredentials serviceCreds = OperationContext.Current.Host.Description.Behaviors.Find<ServiceCredentials>();
                if (serviceCreds != null && serviceCreds.ServiceCertificate != null && serviceCreds.ServiceCertificate.Certificate != null)
                {
                    List<SecurityToken> serviceTokens = new List<SecurityToken>(1);
                    serviceTokens.Add(new X509SecurityToken(serviceCreds.ServiceCertificate.Certificate));
                    return SecurityTokenResolver.CreateDefaultSecurityTokenResolver(serviceTokens.AsReadOnly(), false);
                }
            }

            return EmptySecurityTokenResolver.Instance;
        }

        /// <summary>
        /// Creates a WSTrustSerializationContext using the local resolver information 
        /// of the WSTrustServiceClient.
        /// </summary>
        /// <returns>A WSTrustSerializationContext initialized with the current resolver information.</returns>
        protected virtual WSTrustSerializationContext CreateSerializationContext()
        {
            return new WSTrustSerializationContext(_securityTokenServiceConfiguration.SecurityTokenHandlerCollectionManager,
                                                   this.GetRstSecurityTokenResolver(),
                                                   this.GetSecurityHeaderTokenResolver(OperationContext.Current.RequestContext)
                                                   );

        }

        /// <summary>
        /// Begins an asynchronous call to <see cref="DispatchRequest"/>.
        /// </summary>
        /// <param name="dispatchContext">Defines the request parameters to process and exposes properties
        /// that determine the response message and action.</param>
        /// <param name="asyncCallback">An optional asynchronous callback, to be called when the 
        /// dispatch is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous 
        /// dispatch request from other requests.</param>
        /// <returns><see cref="IAsyncResult"/> that represents the asynchronous operation. Used as the input
        /// to <see cref="EndDispatchRequest"/>.</returns>
        protected virtual IAsyncResult BeginDispatchRequest(DispatchContext dispatchContext, AsyncCallback asyncCallback, object asyncState)
        {
            return new DispatchRequestAsyncResult(dispatchContext, asyncCallback, asyncState);
        }

        /// <summary>
        /// Completes an asynchronous call to <see cref="DispatchRequest"/>.
        /// </summary>
        /// <param name="ar"><see cref="IAsyncResult"/> that was returned by the 
        /// call to <see cref="BeginDispatchRequest"/>.</param>
        /// <returns>The <see cref="DispatchContext"/> that exposes properties which determine the response
        /// message and action.</returns>
        protected virtual DispatchContext EndDispatchRequest(IAsyncResult ar)
        {
            return DispatchRequestAsyncResult.End(ar);
        }

        /// <summary>
        /// Processes a WS-Trust request message, and optionally determines the appropriate
        /// response message and the WS-Addressing action for the response message.
        /// </summary>
        /// <param name="dispatchContext">Defines the request parameters to process and exposes properties
        /// that determine the response message and action.</param>
        protected virtual void DispatchRequest(DispatchContext dispatchContext)
        {
            RST rst = dispatchContext.RequestMessage as RST;
            STS sts = dispatchContext.SecurityTokenService;
            ClaimsPrincipal icp = dispatchContext.Principal;

            if (rst != null)
            {
                switch (rst.RequestType)
                {
                    case RequestTypes.Cancel:
                        dispatchContext.ResponseMessage = sts.Cancel(icp, rst);
                        break;
                    case RequestTypes.Issue:
                        dispatchContext.ResponseMessage = sts.Issue(icp, rst);
                        break;
                    case RequestTypes.Renew:
                        dispatchContext.ResponseMessage = sts.Renew(icp, rst);
                        break;
                    case RequestTypes.Validate:
                        dispatchContext.ResponseMessage = sts.Validate(icp, rst);
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3112, rst.RequestType)));
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3022)));
            }
        }

        /// <summary>
        /// Handles Synchronous calls to the STS.
        /// </summary>
        /// <param name="requestMessage">Incoming Request message.</param>
        /// <param name="requestSerializer">Trust Request Serializer.</param>
        /// <param name="responseSerializer">Trust Response Serializer.</param>
        /// <param name="requestAction">Request SOAP action.</param>
        /// <param name="responseAction">Response SOAP action.</param>
        /// <param name="trustNamespace">Namespace URI of the trust version of the incoming request.</param>
        /// <returns>Response message that contains the serialized RSTR.</returns>
        /// <exception cref="ArgumentNullException">One of the argument is null.</exception>
        protected virtual Message ProcessCore(Message requestMessage, WSTrustRequestSerializer requestSerializer, WSTrustResponseSerializer responseSerializer, string requestAction, string responseAction, string trustNamespace)
        {
            if (requestMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestMessage");
            }

            if (requestSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (responseSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (String.IsNullOrEmpty(requestAction))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestAction");
            }

            if (String.IsNullOrEmpty(responseAction))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseAction");
            }

            if (String.IsNullOrEmpty(trustNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustNamespace");
            }

            Message response = null;
            try
            {
                Fx.Assert(OperationContext.Current != null, "");
                Fx.Assert(OperationContext.Current.RequestContext != null, "");

                //
                // Create the Serialization and Dispatch context objects.
                //
                WSTrustSerializationContext serializationContext = CreateSerializationContext();

                DispatchContext dispatchContext = CreateDispatchContext(requestMessage,
                                                                         requestAction,
                                                                         responseAction,
                                                                         trustNamespace,
                                                                         requestSerializer,
                                                                         responseSerializer,
                                                                         serializationContext);

                //
                // Validate the dispatch context.
                //
                ValidateDispatchContext(dispatchContext);

                //
                // Dispatch the STS message.
                //
                DispatchRequest(dispatchContext);

                //
                // Create the response Message object with the appropriate action.
                //
                response = Message.CreateMessage(OperationContext.Current.RequestContext.RequestMessage.Version,
                                                  dispatchContext.ResponseAction,
                                                  new WSTrustResponseBodyWriter(dispatchContext.ResponseMessage, responseSerializer, serializationContext));
            }
            catch (Exception ex)
            {
                if (!HandleException(ex, trustNamespace, requestAction, requestMessage.Version.Envelope))
                {
                    throw;
                }
            }

            return response;
        }

        /// <summary>
        /// Creates a <see cref="DispatchContext"/> object for use by the <see cref="DispatchRequest"/> method.
        /// </summary>
        /// <param name="requestMessage">The incoming request message.</param>
        /// <param name="requestAction">The SOAP action of the request.</param>
        /// <param name="responseAction">The default SOAP action of the response.</param>
        /// <param name="trustNamespace">Namespace URI of the trust version of the incoming request.</param>
        /// <param name="requestSerializer">The <see cref="WSTrustRequestSerializer"/> used to deserialize 
        /// incoming RST messages.</param>
        /// <param name="responseSerializer">The <see cref="WSTrustResponseSerializer"/> used to deserialize 
        /// incoming RSTR messages.</param>
        /// <param name="serializationContext">The <see cref="WSTrustSerializationContext"/> to use 
        /// when deserializing incoming messages.</param>
        /// <returns>A <see cref="DispatchContext"/> object.</returns>
        protected virtual DispatchContext CreateDispatchContext(Message requestMessage,
                                                                 string requestAction,
                                                                 string responseAction,
                                                                 string trustNamespace,
                                                                 WSTrustRequestSerializer requestSerializer,
                                                                 WSTrustResponseSerializer responseSerializer,
                                                                 WSTrustSerializationContext serializationContext)
        {
            DispatchContext dispatchContext = new DispatchContext()
            {
                Principal = OperationContext.Current.ClaimsPrincipal as ClaimsPrincipal,
                RequestAction = requestAction,
                ResponseAction = responseAction,
                TrustNamespace = trustNamespace
            };

            XmlReader requestBodyReader = requestMessage.GetReaderAtBodyContents();
            //
            // Take a peek at the request with the serializers to figure out if this is a standard incoming
            // RST or if this is an instance of a challenge-response style message pattern where an RSTR comes in.
            //
            if (requestSerializer.CanRead(requestBodyReader))
            {
                dispatchContext.RequestMessage = requestSerializer.ReadXml(requestBodyReader, serializationContext);
            }
            else if (responseSerializer.CanRead(requestBodyReader))
            {
                dispatchContext.RequestMessage = responseSerializer.ReadXml(requestBodyReader, serializationContext);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidRequestException(SR.GetString(SR.ID3114)));
            }

            //
            // CAUTION: Don't create the STS until after the RST or RSTR is deserialized or the test team
            //          has major infrastructure problems.
            //          
            dispatchContext.SecurityTokenService = CreateSTS();
            return dispatchContext;
        }

        /// <summary>
        /// Validates the DispatchContext.
        /// </summary>
        /// <param name="dispatchContext">The <see cref="DispatchContext"/> to validate.</param>
        /// <remarks>
        /// This routine ensures that the <see cref="DispatchContext"/> represents a legal request
        /// prior to being passed into <see cref="DispatchRequest"/>. This routine's default implementation
        /// is to reject incoming RST messages with RSTR actions and vice versa.
        /// </remarks>
        protected virtual void ValidateDispatchContext(DispatchContext dispatchContext)
        {
            if (dispatchContext.RequestMessage is RST
                 && !IsValidRSTAction(dispatchContext))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidRequestException(
                        SR.GetString(SR.ID3113, "RequestSecurityToken", dispatchContext.RequestAction)));
            }

            if (dispatchContext.RequestMessage is RSTR
                 && !IsValidRSTRAction(dispatchContext))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidRequestException(
                        SR.GetString(SR.ID3113, "RequestSecurityTokenResponse", dispatchContext.RequestAction)));
            }
        }

        /// <summary>
        /// Determines if the DispatchContext contains a valid request action for incoming RST messages.
        /// </summary>
        private static bool IsValidRSTAction(DispatchContext dispatchContext)
        {
            bool valid = false;
            string action = dispatchContext.RequestAction;

            if (dispatchContext.TrustNamespace == WSTrust13Constants.NamespaceURI)
            {
                switch (action)
                {
                    case WSTrust13Constants.Actions.Cancel:
                    case WSTrust13Constants.Actions.Issue:
                    case WSTrust13Constants.Actions.Renew:
                    case WSTrust13Constants.Actions.Validate:
                        valid = true;
                        break;
                }
            }
            if (dispatchContext.TrustNamespace == WSTrustFeb2005Constants.NamespaceURI)
            {
                switch (action)
                {
                    case WSTrustFeb2005Constants.Actions.Cancel:
                    case WSTrustFeb2005Constants.Actions.Issue:
                    case WSTrustFeb2005Constants.Actions.Renew:
                    case WSTrustFeb2005Constants.Actions.Validate:
                        valid = true;
                        break;
                }
            }

            return valid;
        }

        /// <summary>
        /// Determines if the DispatchContext contains a valid request action for incoming RSTR messages.
        /// </summary>
        private static bool IsValidRSTRAction(DispatchContext dispatchContext)
        {
            bool valid = false;
            string action = dispatchContext.RequestAction;

            if (dispatchContext.TrustNamespace == WSTrust13Constants.NamespaceURI)
            {
                switch (action)
                {
                    case WSTrust13Constants.Actions.CancelFinalResponse:
                    case WSTrust13Constants.Actions.CancelResponse:
                    case WSTrust13Constants.Actions.IssueFinalResponse:
                    case WSTrust13Constants.Actions.IssueResponse:
                    case WSTrust13Constants.Actions.RenewFinalResponse:
                    case WSTrust13Constants.Actions.RenewResponse:
                    case WSTrust13Constants.Actions.ValidateFinalResponse:
                    case WSTrust13Constants.Actions.ValidateResponse:
                        valid = true;
                        break;
                }
            }
            if (dispatchContext.TrustNamespace == WSTrustFeb2005Constants.NamespaceURI)
            {
                switch (action)
                {
                    case WSTrustFeb2005Constants.Actions.CancelResponse:
                    case WSTrustFeb2005Constants.Actions.IssueResponse:
                    case WSTrustFeb2005Constants.Actions.RenewResponse:
                    case WSTrustFeb2005Constants.Actions.ValidateResponse:
                        valid = true;
                        break;
                }
            }

            return valid;
        }

        private STS CreateSTS()
        {
            STS sts = _securityTokenServiceConfiguration.CreateSecurityTokenService();

            if (sts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3002)));
            }

            return sts;
        }

        /// <summary>
        /// Handles Asynchronous call to the STS.
        /// </summary>
        /// <param name="requestMessage">Incoming Request message.</param>
        /// <param name="requestSerializer">Trust Request Serializer.</param>
        /// <param name="responseSerializer">Trust Response Serializer.</param>
        /// <param name="requestAction">Request SOAP action.</param>
        /// <param name="responseAction">Response SOAP action.</param>
        /// <param name="trustNamespace">Namespace URI of the trust version of the incoming request.</param>
        /// <param name="callback">Callback that gets invoked when the Asynchronous call ends.</param>
        /// <param name="state">state information of the Asynchronous call.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        /// <exception cref="ArgumentNullException">One of the argument is null.</exception>
        protected virtual IAsyncResult BeginProcessCore(Message requestMessage, WSTrustRequestSerializer requestSerializer, WSTrustResponseSerializer responseSerializer, string requestAction, string responseAction, string trustNamespace, AsyncCallback callback, object state)
        {
            if (requestMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (requestSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (responseSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (String.IsNullOrEmpty(requestAction))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestAction");
            }

            if (String.IsNullOrEmpty(responseAction))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseAction");
            }

            if (String.IsNullOrEmpty(trustNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustNamespace");
            }

            IAsyncResult result = null;
            try
            {
                Fx.Assert(OperationContext.Current != null, "");
                Fx.Assert(OperationContext.Current.RequestContext != null, "");

                //
                // Create the Serialization and Dispatch context objects.
                //
                WSTrustSerializationContext serializationContext = CreateSerializationContext();

                DispatchContext dispatchContext = CreateDispatchContext(requestMessage,
                                                                         requestAction,
                                                                         responseAction,
                                                                         trustNamespace,
                                                                         requestSerializer,
                                                                         responseSerializer,
                                                                         serializationContext);

                //
                // Validate the dispatch context.
                //
                ValidateDispatchContext(dispatchContext);

                //
                // Dispatch the message asynchronously.
                //
                result = new ProcessCoreAsyncResult(this,
                                                     dispatchContext,
                                                     OperationContext.Current.RequestContext.RequestMessage.Version,
                                                     responseSerializer,
                                                     serializationContext,
                                                     callback,
                                                     state);
            }
            catch (Exception ex)
            {
                if (!HandleException(ex, trustNamespace, requestAction, requestMessage.Version.Envelope))
                {
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Completes an Asynchronous call to the STS.
        /// </summary>
        /// <param name="ar">IAsyncResult that was returned by the call to the Asynchronous Begin method.</param>
        /// <param name="requestAction">Request SOAP Action.</param>
        /// <param name="responseAction">Response SOAP Action.</param>
        /// <param name="trustNamespace">Namespace URI of the current trust version.</param>
        /// <returns>Message that contains the serialized RST message.</returns>
        /// <exception cref="ArgumentNullException">One of the argument is null.</exception>
        protected virtual Message EndProcessCore(IAsyncResult ar, string requestAction, string responseAction, string trustNamespace)
        {
            if (ar == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ar");
            }

            ProcessCoreAsyncResult asyncResult = ar as ProcessCoreAsyncResult;
            if (asyncResult == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID2004, typeof(ProcessCoreAsyncResult), ar.GetType()), "ar"));
            }

            Message message = null;
            try
            {
                message = ProcessCoreAsyncResult.End(ar);
            }
            catch (Exception ex)
            {
                if (!HandleException(ex, trustNamespace, requestAction, asyncResult.MessageVersion.Envelope))
                {
                    throw;
                }
            }

            return message;
        }

        /// <summary>
        /// Raises the Error Event and converts the given exception to a FaultException if required. If the original
        /// exception was a FaultException or PreserveOriginalException flag is set to true then the conversion to 
        /// FaultException is not done.
        /// </summary>
        /// <param name="ex">The original exception.</param>
        /// <param name="trustNamespace">Trust Namespace of the current trust version.</param>
        /// <param name="action">The Trust action that caused the exception.</param>
        /// <param name="requestEnvelopeVersion">Version of the request envolope.</param>
        protected virtual bool HandleException(Exception ex, string trustNamespace, string action, EnvelopeVersion requestEnvelopeVersion)
        {
            if (System.Runtime.Fx.IsFatal(ex))
            {
                return false;
            }

            if (DiagnosticUtility.ShouldTrace(TraceEventType.Warning))
            {
                TraceUtility.TraceString(
                    TraceEventType.Warning,
                    "RequestFailed: TrustNamespace={0}, Action={1}, Exception={2}",
                    trustNamespace,
                    action,
                    ex);
            }

            // raise the exception events.
            if (_requestFailed != null)
            {
                _requestFailed(this, new WSTrustRequestProcessingErrorEventArgs(action, ex));
            }

            bool preserveOriginalException = false;
            ServiceDebugBehavior debugBehavior = OperationContext.Current.Host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior != null)
            {
                preserveOriginalException = debugBehavior.IncludeExceptionDetailInFaults;
            }

            if (String.IsNullOrEmpty(trustNamespace) || String.IsNullOrEmpty(action) || preserveOriginalException || ex is FaultException)
            {
                // Just throw the original exception.
                return false;
            }
            else
            {
                FaultException faultException = OperationContext.Current.Host.Credentials.ExceptionMapper.FromException(ex, (requestEnvelopeVersion == EnvelopeVersion.Soap11) ? soap11Namespace : soap12Namespace, trustNamespace);
                if (faultException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(faultException);
                }

                // The exception is not one of the recognized exceptions. Just throw the original exception.
                return false;
            }
        }

        #region IWSTrustFeb2005SyncContract and IWSTrust13SyncContract Methods

        /// <summary>
        /// Processes a Trust 1.3 Cancel message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13Cancel(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Cancel, WSTrust13Constants.Actions.CancelFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 Issue message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13Issue(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Issue, WSTrust13Constants.Actions.IssueFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 Renew message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13Renew(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Renew, WSTrust13Constants.Actions.RenewFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 Validate message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13Validate(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Validate, WSTrust13Constants.Actions.ValidateFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 RSTR/Cancel message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13CancelResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                WSTrust13Constants.Actions.CancelResponse,
                                WSTrust13Constants.Actions.CancelFinalResponse,
                                WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 RSTR/Issue message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13IssueResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                WSTrust13Constants.Actions.IssueResponse,
                                WSTrust13Constants.Actions.IssueFinalResponse,
                                WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 RSTR/Renew message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13RenewResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                WSTrust13Constants.Actions.RenewResponse,
                                WSTrust13Constants.Actions.RenewFinalResponse,
                                WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust 1.3 RSTR/Validate message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrust13ValidateResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                WSTrust13Constants.Actions.ValidateResponse,
                                WSTrust13Constants.Actions.ValidateFinalResponse,
                                WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 Cancel message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005Cancel(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Cancel, WSTrustFeb2005Constants.Actions.CancelResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 Issue message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005Issue(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Issue, WSTrustFeb2005Constants.Actions.IssueResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 Renew message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005Renew(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Renew, WSTrustFeb2005Constants.Actions.RenewResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 Validate message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005Validate(Message message)
        {
            return ProcessCore(message, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Validate, WSTrustFeb2005Constants.Actions.ValidateResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 RSTR/Cancel message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005CancelResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                WSTrustFeb2005Constants.Actions.CancelResponse,
                                WSTrustFeb2005Constants.Actions.CancelResponse,
                                WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 RSTR/Issue message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005IssueResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                WSTrustFeb2005Constants.Actions.IssueResponse,
                                WSTrustFeb2005Constants.Actions.IssueResponse,
                                WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 RSTR/Renew message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005RenewResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                WSTrustFeb2005Constants.Actions.RenewResponse,
                                WSTrustFeb2005Constants.Actions.RenewResponse,
                                WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes a Trust Feb 2005 RSTR/Validate message synchronously.
        /// </summary>
        /// <param name="message">Incoming Request message.</param>
        /// <returns>Message with the serialized response.</returns>
        public Message ProcessTrustFeb2005ValidateResponse(Message message)
        {
            return ProcessCore(message,
                                _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                WSTrustFeb2005Constants.Actions.ValidateResponse,
                                WSTrustFeb2005Constants.Actions.ValidateResponse,
                                WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Gets the SecurityTokenServiceConfiguration
        /// </summary>
        public SecurityTokenServiceConfiguration SecurityTokenServiceConfiguration
        {
            get
            {
                return _securityTokenServiceConfiguration;
            }
        }

        #endregion

        #region IWSTrustFeb2005AsyncContract and IWSTrust13AsyncContract  Methods

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 1.3 Cancel message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13Cancel(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Cancel, WSTrust13Constants.Actions.CancelFinalResponse, WSTrust13Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 Cancel message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13Cancel(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrust13Constants.Actions.Cancel, WSTrust13Constants.Actions.CancelFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 Issue message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13Issue(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Issue, WSTrust13Constants.Actions.IssueFinalResponse, WSTrust13Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 Issue message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13Issue(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrust13Constants.Actions.Issue, WSTrust13Constants.Actions.IssueFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 Renew message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13Renew(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Renew, WSTrust13Constants.Actions.RenewFinalResponse, WSTrust13Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 Renew message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13Renew(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrust13Constants.Actions.Renew, WSTrust13Constants.Actions.RenewFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 Validate message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13Validate(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrust13RequestSerializer, _securityTokenServiceConfiguration.WSTrust13ResponseSerializer, WSTrust13Constants.Actions.Validate, WSTrust13Constants.Actions.ValidateFinalResponse, WSTrust13Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 Validate message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13Validate(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrust13Constants.Actions.Validate, WSTrust13Constants.Actions.ValidateFinalResponse, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 RSTR/Cancel message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13CancelResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                     WSTrust13Constants.Actions.CancelResponse,
                                     WSTrust13Constants.Actions.CancelFinalResponse,
                                     WSTrust13Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 RSTR/Cancel message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13CancelResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrust13Constants.Actions.CancelResponse,
                                   WSTrust13Constants.Actions.CancelFinalResponse,
                                   WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 RSTR/Issue message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13IssueResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                     WSTrust13Constants.Actions.IssueResponse,
                                     WSTrust13Constants.Actions.IssueFinalResponse,
                                     WSTrust13Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 RSTR/Issue message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13IssueResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrust13Constants.Actions.IssueResponse,
                                   WSTrust13Constants.Actions.IssueFinalResponse,
                                   WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 RSTR/Renew message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13RenewResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                     WSTrust13Constants.Actions.RenewResponse,
                                     WSTrust13Constants.Actions.RenewFinalResponse,
                                     WSTrust13Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 RSTR/Renew message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13RenewResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrust13Constants.Actions.RenewResponse,
                                   WSTrust13Constants.Actions.RenewFinalResponse,
                                   WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 1.3 RSTR/Validate message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrust13ValidateResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrust13RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrust13ResponseSerializer,
                                     WSTrust13Constants.Actions.ValidateResponse,
                                     WSTrust13Constants.Actions.ValidateFinalResponse,
                                     WSTrust13Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust 1.3 RSTR/Validate message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrust13ValidateResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrust13Constants.Actions.ValidateResponse,
                                   WSTrust13Constants.Actions.ValidateFinalResponse,
                                   WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust 2005 Cancel message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005Cancel(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Cancel, WSTrustFeb2005Constants.Actions.CancelResponse, WSTrustFeb2005Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 Cancel message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005Cancel(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrustFeb2005Constants.Actions.Cancel, WSTrustFeb2005Constants.Actions.CancelResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 Issue message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005Issue(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Issue, WSTrustFeb2005Constants.Actions.IssueResponse, WSTrustFeb2005Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 Issue message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005Issue(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrustFeb2005Constants.Actions.Issue, WSTrustFeb2005Constants.Actions.IssueResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 Renew message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005Renew(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Renew, WSTrustFeb2005Constants.Actions.RenewResponse, WSTrustFeb2005Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 Renew message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005Renew(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrustFeb2005Constants.Actions.Renew, WSTrustFeb2005Constants.Actions.RenewResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 Validate message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005Validate(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request, _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer, _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer, WSTrustFeb2005Constants.Actions.Validate, WSTrustFeb2005Constants.Actions.ValidateResponse, WSTrustFeb2005Constants.NamespaceURI, callback, state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 Validate message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005Validate(IAsyncResult ar)
        {
            return EndProcessCore(ar, WSTrustFeb2005Constants.Actions.Validate, WSTrustFeb2005Constants.Actions.ValidateResponse, WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 RSTR/Cancel message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005CancelResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                     WSTrustFeb2005Constants.Actions.CancelResponse,
                                     WSTrustFeb2005Constants.Actions.CancelResponse,
                                     WSTrustFeb2005Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 RSTR/Cancel message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005CancelResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrustFeb2005Constants.Actions.CancelResponse,
                                   WSTrustFeb2005Constants.Actions.CancelResponse,
                                   WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 RSTR/Issue message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005IssueResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                     WSTrustFeb2005Constants.Actions.IssueResponse,
                                     WSTrustFeb2005Constants.Actions.IssueResponse,
                                     WSTrustFeb2005Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 RSTR/Issue message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005IssueResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrustFeb2005Constants.Actions.IssueResponse,
                                   WSTrustFeb2005Constants.Actions.IssueResponse,
                                   WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 RSTR/Renew message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005RenewResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                     WSTrustFeb2005Constants.Actions.RenewResponse,
                                     WSTrustFeb2005Constants.Actions.RenewResponse,
                                     WSTrustFeb2005Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 RSTR/Renew message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005RenewResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrustFeb2005Constants.Actions.RenewResponse,
                                   WSTrustFeb2005Constants.Actions.RenewResponse,
                                   WSTrustFeb2005Constants.NamespaceURI);
        }

        /// <summary>
        /// Processes an Asynchronous call to Trust Feb 2005 RSTR/Validate message.
        /// </summary>
        /// <param name="request">Incoming Request message.</param>
        /// <param name="callback">Callback to be invoked when the Asynchronous operation ends.</param>
        /// <param name="state">Asynchronous state.</param>
        /// <returns>IAsyncResult that should be passed back to the End method to complete the Asynchronous call.</returns>
        public IAsyncResult BeginTrustFeb2005ValidateResponse(Message request, AsyncCallback callback, object state)
        {
            return BeginProcessCore(request,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005RequestSerializer,
                                     _securityTokenServiceConfiguration.WSTrustFeb2005ResponseSerializer,
                                     WSTrustFeb2005Constants.Actions.ValidateResponse,
                                     WSTrustFeb2005Constants.Actions.ValidateResponse,
                                     WSTrustFeb2005Constants.NamespaceURI,
                                     callback,
                                     state);
        }

        /// <summary>
        /// Completes an Asynchronous call to Trust Feb 2005 RSTR/Validate message.
        /// </summary>
        /// <param name="ar">IAsyncResult object returned by the Begin method that started the Asynchronous call.</param>
        /// <returns>Message containing the Serialized RSTR.</returns>
        public Message EndTrustFeb2005ValidateResponse(IAsyncResult ar)
        {
            return EndProcessCore(ar,
                                   WSTrustFeb2005Constants.Actions.ValidateResponse,
                                   WSTrustFeb2005Constants.Actions.ValidateResponse,
                                   WSTrustFeb2005Constants.NamespaceURI);
        }

        #endregion

        //
        // An async result class that represents the async version of the ProcessCore method.
        //
        internal class ProcessCoreAsyncResult : AsyncResult
        {
            //
            // Encapsulate the local variables in the sync version of ProcessCore as fields.
            //
            WSTrustServiceContract _trustServiceContract;
            DispatchContext _dispatchContext;
            MessageVersion _messageVersion;
            WSTrustResponseSerializer _responseSerializer;
            WSTrustSerializationContext _serializationContext;

            public ProcessCoreAsyncResult(WSTrustServiceContract contract,
                                           DispatchContext dispatchContext,
                                           MessageVersion messageVersion,
                                           WSTrustResponseSerializer responseSerializer,
                                           WSTrustSerializationContext serializationContext,
                                           AsyncCallback asyncCallback,
                                           object asyncState)
                : base(asyncCallback, asyncState)
            {
                if (contract == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
                }

                if (dispatchContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchContext");
                }

                if (responseSerializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
                }

                if (serializationContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializationContext");
                }

                _trustServiceContract = contract;
                _dispatchContext = dispatchContext;
                _messageVersion = messageVersion;
                _responseSerializer = responseSerializer;
                _serializationContext = serializationContext;

                contract.BeginDispatchRequest(dispatchContext, OnDispatchRequestCompleted, null);
            }

            public WSTrustServiceContract TrustServiceContract
            {
                get { return _trustServiceContract; }
            }

            public DispatchContext DispatchContext
            {
                get { return _dispatchContext; }
            }

            public MessageVersion MessageVersion
            {
                get { return _messageVersion; }
            }

            public WSTrustResponseSerializer ResponseSerializer
            {
                get { return _responseSerializer; }
            }

            public WSTrustSerializationContext SerializationContext
            {
                get { return _serializationContext; }
            }

            public new static Message End(IAsyncResult ar)
            {
                AsyncResult.End(ar);

                ProcessCoreAsyncResult pcar = ar as ProcessCoreAsyncResult;
                if (pcar == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2004, typeof(ProcessCoreAsyncResult), ar.GetType()));
                }

                //
                // Create the response Message object with the appropriate action.
                //
                return Message.CreateMessage(OperationContext.Current.RequestContext.RequestMessage.Version,
                                              pcar.DispatchContext.ResponseAction,
                                              new WSTrustResponseBodyWriter(pcar.DispatchContext.ResponseMessage,
                                                                             pcar.ResponseSerializer,
                                                                             pcar.SerializationContext));
            }

            //
            // Asynchronously invoked when WSTrustServiceContract.BeginDispatchRequest completes.
            //
            private void OnDispatchRequestCompleted(IAsyncResult ar)
            {
                try
                {
                    _dispatchContext = _trustServiceContract.EndDispatchRequest(ar);
                    this.Complete(false);
                }
                catch (Exception ex)
                {
                    if (System.Runtime.Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    this.Complete(false, ex);
                }
            }
        }

        //
        // AsyncResult to encapsulate the default async implementation of DispatchRequest
        //
        internal class DispatchRequestAsyncResult : AsyncResult
        {
            DispatchContext _dispatchContext;

            public DispatchContext DispatchContext
            {
                get { return _dispatchContext; }
            }

            public DispatchRequestAsyncResult(DispatchContext dispatchContext, AsyncCallback asyncCallback, object asyncState)
                : base(asyncCallback, asyncState)
            {
                _dispatchContext = dispatchContext;

                ClaimsPrincipal icp = dispatchContext.Principal;
                RST rst = dispatchContext.RequestMessage as RST;
                STS sts = dispatchContext.SecurityTokenService;

                if (rst == null)
                {
                    this.Complete(true, DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3023))));
                    return;
                }

                switch (rst.RequestType)
                {
                    case RequestTypes.Cancel:
                        sts.BeginCancel(icp, rst, OnCancelComplete, null);
                        break;
                    case RequestTypes.Issue:
                        sts.BeginIssue(icp, rst, OnIssueComplete, null);
                        break;
                    case RequestTypes.Renew:
                        sts.BeginRenew(icp, rst, OnRenewComplete, null);
                        break;
                    case RequestTypes.Validate:
                        sts.BeginValidate(icp, rst, OnValidateComplete, null);
                        break;
                    default:
                        this.Complete(true, DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3112, rst.RequestType))));
                        break;
                }
            }

            public new static DispatchContext End(IAsyncResult ar)
            {
                AsyncResult.End(ar);

                DispatchRequestAsyncResult dcar = ar as DispatchRequestAsyncResult;
                if (dcar == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2004, typeof(DispatchRequestAsyncResult), ar.GetType()));
                }
                return dcar.DispatchContext;
            }

            void OnCancelComplete(IAsyncResult ar)
            {
                try
                {
                    _dispatchContext.ResponseMessage = _dispatchContext.SecurityTokenService.EndCancel(ar);
                    Complete(false);
                }
                catch (Exception e)
                {
                    System.ServiceModel.DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                    if (Fx.IsFatal(e)) throw;
                    Complete(false, e);
                }
            }

            void OnIssueComplete(IAsyncResult ar)
            {
                try
                {
                    _dispatchContext.ResponseMessage = _dispatchContext.SecurityTokenService.EndIssue(ar);
                    Complete(false);
                }
                catch (Exception e)
                {
                    System.ServiceModel.DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                    if (Fx.IsFatal(e)) throw;
                    Complete(false, e);
                }
            }

            void OnRenewComplete(IAsyncResult ar)
            {
                try
                {
                    _dispatchContext.ResponseMessage = _dispatchContext.SecurityTokenService.EndRenew(ar);
                    Complete(false);
                }
                catch (Exception e)
                {
                    System.ServiceModel.DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                    if (Fx.IsFatal(e)) throw;
                    Complete(false, e);
                }
            }

            void OnValidateComplete(IAsyncResult ar)
            {
                try
                {
                    _dispatchContext.ResponseMessage = _dispatchContext.SecurityTokenService.EndValidate(ar);
                    Complete(false);
                }
                catch (Exception e)
                {
                    System.ServiceModel.DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                    if (Fx.IsFatal(e)) throw;
                    Complete(false, e);
                }
            }

        }

        #region IContractBehavior Members

        /// <summary>
        /// Configures any binding elements to support the contract behavior.
        /// </summary>
        /// <remarks>
        /// Inherited from IContractBehavior
        /// </remarks>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            return;
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <remarks>
        /// Inherited from IContractBehavior
        /// </remarks>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            return;
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <remarks>
        /// Inherited from IContractBehavior
        /// </remarks>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            return;
        }

        /// <summary>
        /// Implement to confirm that the contract and endpoint can support the contract
        /// behavior.
        /// </summary>
        /// <remarks>
        /// Inherited from IContractBehavior
        /// </remarks>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            return;
        }

        #endregion

        #region IWsdlExportExtension Members

        /// <summary>
        /// Implementation for IWsdlExportExtension.ExportContract. The default implementation 
        /// does nothing. Can be overriden in the derived class for specific behavior.
        /// </summary>
        /// <param name="exporter">The WsdlExporter that exports the contract information.</param>
        /// <param name="context">Provides mappings from exported WSDL elements to the contract description.</param>
        public virtual void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            return;
        }

        /// <summary>
        /// Implements IWsdlExportExtensions.ExportEndpoint. The default implementation does the following,
        /// For every Trust contract found,
        /// 1. It includes the appropriate trust namespace in the WSDL.
        /// 2. Imports the appropriate Trust schema and all dependent schemas.
        /// 3. Fixes the Messages of each operation to it appropriate WS-Trust equivalent.
        ///     Trust Contract exposed by the Framework takes a System.ServiceModel.Channels.Message in and 
        ///     returns a System.ServiceModel.Channels.Message out. But Trust messages expects and RST and 
        ///     returns an RSTR/RSTRC. This method fixes the message names with the appropriate WS-Trust
        ///     messages.
        /// </summary>
        /// <param name="exporter">The WsdlExporter that exports the contract information.</param>
        /// <param name="context">Provides mappings from exported WSDL elements to the endpoint description.</param>
        /// <exception cref="ArgumentNullException">The input argument 'exporter' or 'context' is null.</exception>
        public virtual void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (context.WsdlPort == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3146));
            }

            if (context.WsdlPort.Service == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3147));
            }

            if (context.WsdlPort.Service.ServiceDescription == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3148));
            }

            System.Web.Services.Description.ServiceDescription serviceDescription = context.WsdlPort.Service.ServiceDescription;

            // Iterate throught the Ports and for each of our contracts fix the input and output messages 
            // of the contract and import the required schemas.
            foreach (PortType portType in serviceDescription.PortTypes)
            {
                if (StringComparer.Ordinal.Equals(portType.Name, WSTrustServiceContractConstants.Contracts.IWSTrustFeb2005Sync))
                {
                    IncludeNamespace(context, WSTrustFeb2005Constants.Prefix, WSTrustFeb2005Constants.NamespaceURI);
                    ImportSchema(exporter, context, WSTrustFeb2005Constants.NamespaceURI);

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005Cancel,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005Issue,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005Renew,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005Validate,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(portType.Name, WSTrustServiceContractConstants.Contracts.IWSTrust13Sync))
                {
                    IncludeNamespace(context, WSTrust13Constants.Prefix, WSTrust13Constants.NamespaceURI);
                    ImportSchema(exporter, context, WSTrust13Constants.NamespaceURI);

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13Cancel,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13Issue,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13Renew,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13Validate,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(portType.Name, WSTrustServiceContractConstants.Contracts.IWSTrustFeb2005Async))
                {
                    IncludeNamespace(context, WSTrustFeb2005Constants.Prefix, WSTrustFeb2005Constants.NamespaceURI);
                    ImportSchema(exporter, context, WSTrustFeb2005Constants.NamespaceURI);

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005CancelAsync,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005IssueAsync,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005RenewAsync,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.TrustFeb2005ValidateAsync,
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityToken,
                            WSTrustFeb2005Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse,
                            WSTrustFeb2005Constants.NamespaceURI));
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(portType.Name, WSTrustServiceContractConstants.Contracts.IWSTrust13Async))
                {
                    IncludeNamespace(context, WSTrust13Constants.Prefix, WSTrust13Constants.NamespaceURI);
                    ImportSchema(exporter, context, WSTrust13Constants.NamespaceURI);

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13CancelAsync,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13IssueAsync,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13RenewAsync,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));

                    FixMessageElement(
                        serviceDescription,
                        portType,
                        context,
                        WSTrustServiceContractConstants.Operations.Trust13ValidateAsync,
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityToken,
                            WSTrust13Constants.NamespaceURI),
                        new XmlQualifiedName(
                            WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection,
                            WSTrust13Constants.NamespaceURI));
                }
            }
        }

        #endregion

        /// <summary>
        /// Adds the required WS-Trust namespaces to the WSDL if not already present.
        /// </summary>
        /// <param name="context">Provides mappings from exported WSDL elements to the endpoint description.</param>
        /// <param name="prefix">The prefix of the namespace to be included.</param>
        /// <param name="ns">Namespace to be included.</param>
        /// <exception cref="ArgumentException">Either 'prefix' or 'ns' is null or empty string.</exception>
        /// <exception cref="ArgumentNullException">The 'context' parameter is null.</exception>
        protected virtual void IncludeNamespace(WsdlEndpointConversionContext context, string prefix, string ns)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (String.IsNullOrEmpty(prefix))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("prefix");
            }

            if (String.IsNullOrEmpty(ns))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("ns");
            }

            bool alreadyPresent = false;
            XmlQualifiedName[] namespaces = context.WsdlBinding.ServiceDescription.Namespaces.ToArray();
            for (int i = 0; i < namespaces.Length; ++i)
            {
                if (StringComparer.Ordinal.Equals(namespaces[i].Namespace, ns))
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent)
            {
                context.WsdlBinding.ServiceDescription.Namespaces.Add(prefix, ns);
            }
        }

        /// <summary>
        /// Imports all the required schema if not already present in the WSDL.
        /// The default implementation will import the following schemas,
        ///     (a) WS-Trust Feb 2005.
        ///     (b) WS-Trust 1.3
        /// Derived classes can override this method to import other schemas.
        /// </summary>
        /// <param name="exporter">The WsdlExporter that exports the contract information.</param>
        /// <param name="context">Provides mappings from exported WSDL elements to the endpoint description.</param>
        /// <param name="ns">The current WS-Trust namespace for which the schemas are imported.</param>
        /// <exception cref="ArgumentNullException">The parameter 'exporter' or 'context' is null.</exception>
        /// <exception cref="ArgumentException">The parameter 'ns' is either null or String.Empty.</exception>
        /// <exception cref="InvalidOperationException">The namespace 'ns' is not a recognized WS-Trust namespace.</exception>
        protected virtual void ImportSchema(WsdlExporter exporter, WsdlEndpointConversionContext context, string ns)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (String.IsNullOrEmpty(ns))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("ns");
            }

            foreach (XmlSchema xmlSchema in context.WsdlPort.Service.ServiceDescription.Types.Schemas)
            {
                foreach (XmlSchemaObject include in xmlSchema.Includes)
                {
                    XmlSchemaImport schemaImport = include as XmlSchemaImport;
                    if ((schemaImport != null) && StringComparer.Ordinal.Equals(schemaImport.Namespace, ns))
                    {
                        // The schema is already imported. Just return.
                        return;
                    }
                }
            }

            XmlSchema schema = GetXmlSchema(exporter, ns);
            if (schema == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3004, ns));
            }
            XmlSchema importedSchema = null;
            if (context.WsdlPort.Service.ServiceDescription.Types.Schemas.Count == 0)
            {
                importedSchema = new XmlSchema();
                context.WsdlPort.Service.ServiceDescription.Types.Schemas.Add(importedSchema);
            }
            else
            {
                importedSchema = context.WsdlPort.Service.ServiceDescription.Types.Schemas[0];
            }

            XmlSchemaImport import = new XmlSchemaImport();
            import.Namespace = ns;
            exporter.GeneratedXmlSchemas.Add(schema);
            importedSchema.Includes.Add(import);
        }


        /// <summary>
        /// For a given namespace this method looks up the WsdlExporter to see if an XmlSchema has been cached and returns that. 
        /// Else it loads the schema for that given namespace and returns the loaded XmlSchema.
        /// </summary>
        /// <param name="exporter">The WsdlExporter that exports the contract information.</param>
        /// <param name="ns">The namespace for which the schema is to be obtained.</param>
        /// <exception cref="ArgumentNullException">The parameter 'exporter' is null.</exception>
        /// <exception cref="ArgumentException">The parameter 'ns' is either null or String.Empty.</exception>
        /// <exception cref="InvalidOperationException">The namespace 'ns' is not a recognized WS-Trust namespace.</exception>
        static XmlSchema GetXmlSchema(WsdlExporter exporter, string ns)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (String.IsNullOrEmpty(ns))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("ns");
            }

            ICollection schemas = exporter.GeneratedXmlSchemas.Schemas(ns);
            if ((schemas != null) && (schemas.Count > 0))
            {
                foreach (XmlSchema s in schemas)
                {
                    return s;
                }
            }

            string xmlSchema = null;
            switch (ns)
            {
                case WSTrustFeb2005Constants.NamespaceURI:
                    xmlSchema = WSTrustFeb2005Constants.Schema;
                    break;
                case WSTrust13Constants.NamespaceURI:
                    xmlSchema = WSTrust13Constants.Schema;
                    break;
                default:
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID5004, ns));
            }

            StringReader reader = new StringReader(xmlSchema);
            return XmlSchema.Read(new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit }, null);
        }

        /// <summary>
        /// During WSDL generation, the method fixes a given operation message element to refer to the 
        /// RST and RSTR elements of the appropriate WS-Trust version. 
        /// </summary>
        /// <param name="serviceDescription">The ServiceDescription that has the current state of the exported 
        /// WSDL.</param>
        /// <param name="portType">The WSDL PortType whose messages are to be fixed.</param>
        /// <param name="context">Provides mappings from exported WSDL elements to the endpoint description.</param>
        /// <param name="operationName">The operation name inside the PortType.</param>
        /// <param name="inputMessageElement">The XmlQualifiedName of the input message element.</param>
        /// <param name="outputMessageElement">The XmlQualifiedName of the output message element.</param>
        /// <exception cref="ArgumentNullException">The parameter 'serviceDescription', 'portType', 'inputMessageType'
        /// or 'outputMessageType' is null.</exception>
        /// <exception cref="ArgumentException">The parameter 'operationName' is null or Empty.</exception>
        /// <remarks>
        /// Trust Contract exposed by the Framework takes a System.ServiceModel.Channels.Message in and 
        /// returns a System.ServiceModel.Channels.Message out. But Trust messages expects and RST and 
        /// returns an RSTR/RSTRC. This method fixes the message elements with the appropriate WS-Trust
        /// messages specified by the XmlQualified names 'inputMessageElement' and 'outputMessageElement'.
        /// </remarks>
        protected virtual void FixMessageElement(
                                System.Web.Services.Description.ServiceDescription serviceDescription,
                                PortType portType,
                                WsdlEndpointConversionContext context,
                                string operationName,
                                XmlQualifiedName inputMessageElement,
                                XmlQualifiedName outputMessageElement)
        {
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }

            if (portType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("portType");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (String.IsNullOrEmpty(operationName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("operationName");
            }

            if (inputMessageElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputMessageElement");
            }

            if (outputMessageElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputMessageElement");
            }

            Operation operation = null;
            System.Web.Services.Description.Message inputMessage = null;
            System.Web.Services.Description.Message outputMessage = null;

            foreach (Operation op in portType.Operations)
            {
                if (StringComparer.Ordinal.Equals(op.Name, operationName))
                {
                    operation = op;

                    // Find the correspinding message in the messages collection.
                    foreach (System.Web.Services.Description.Message message in serviceDescription.Messages)
                    {
                        if (StringComparer.Ordinal.Equals(message.Name, op.Messages.Input.Message.Name))
                        {
                            if (message.Parts.Count != 1)
                            {
                                throw DiagnosticUtility.ThrowHelperInvalidOperation(
                                    SR.GetString(SR.ID3144, portType.Name, op.Name, message.Name, message.Parts.Count));
                            }
                            inputMessage = message;
                        }
                        else if (StringComparer.Ordinal.Equals(message.Name, op.Messages.Output.Message.Name))
                        {
                            if (message.Parts.Count != 1)
                            {
                                throw DiagnosticUtility.ThrowHelperInvalidOperation(
                                    SR.GetString(SR.ID3144, portType.Name, op.Name, message.Name, message.Parts.Count));
                            }
                            outputMessage = message;
                        }

                        if ((inputMessage != null) && (outputMessage != null))
                        {
                            break;
                        }
                    }
                }

                if (operation != null)
                {
                    break;
                }
            }

            if (operation == null)
            {
                // This operation is missing. This might be due to another Behavior that has modified the WSDL as
                // well. Ignore this and return.
                return;
            }

            if (inputMessage == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(
                    SR.GetString(SR.ID3149, portType.Name, portType.Namespaces, operationName));
            }
            if (outputMessage == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(
                    SR.GetString(SR.ID3150, portType.Name, portType.Namespaces, operationName));
            }

            inputMessage.Parts[0].Element = inputMessageElement;
            outputMessage.Parts[0].Element = outputMessageElement;
            inputMessage.Parts[0].Type = null;
            outputMessage.Parts[0].Type = null;
        }
    }
}
