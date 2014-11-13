//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    class PeerSecurityHelpers
    {
        public static byte[] ComputeHash(X509Certificate2 cert, string pwd)
        {
            RSACryptoServiceProvider keyProv = cert.PublicKey.Key as RSACryptoServiceProvider;
            Fx.Assert(keyProv != null, "Remote Peer's credentials are invalid!");
            byte[] key = keyProv.ExportCspBlob(false);
            return ComputeHash(key, pwd);
        }

        public static byte[] ComputeHash(Claim claim, string pwd)
        {
            RSACryptoServiceProvider provider = claim.Resource as RSACryptoServiceProvider;
            if (provider == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            using (provider)
            {
                byte[] keyBlob = provider.ExportCspBlob(false);
                if (keyBlob == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                return ComputeHash(keyBlob, pwd);
            }
        }

        public static byte[] ComputeHash(byte[] message, string pwd)
        {
            byte[] returnValue = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            byte[] pwdBytes = null;
            byte[] pwdHash = null;
            byte[] tempBuffer = null;
            try
            {
                pwdBytes = UnicodeEncoding.Unicode.GetBytes(pwd.Trim());
                using (HMACSHA256 algo = new HMACSHA256(pwdBytes))
                {
                    using (SHA256Managed sha = new SHA256Managed())
                    {
                        pwdHash = sha.ComputeHash(pwdBytes);
                        tempBuffer = DiagnosticUtility.Utility.AllocateByteArray(checked(message.Length + pwdHash.Length));
                        Array.Copy(pwdHash, tempBuffer, pwdHash.Length);
                        Array.Copy(message, 0, tempBuffer, pwdHash.Length, message.Length);

                        returnValue = algo.ComputeHash(tempBuffer);
                    }
                }
            }
            finally
            {
                ArrayClear(pwdBytes);
                ArrayClear(pwdHash);
                ArrayClear(tempBuffer);
            }
            return returnValue;
        }

        static void ArrayClear(byte[] buffer)
        {
            if (buffer != null)
                Array.Clear(buffer, 0, buffer.Length);
        }

        public static bool Authenticate(Claim claim, string password, byte[] authenticator)
        {
            bool returnValue = false;
            if (authenticator == null)
                return false;
            byte[] hash = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                hash = ComputeHash(claim, password);
                if (hash.Length == authenticator.Length)
                {
                    for (int i = 0; i < hash.Length; i++)
                    {
                        if (hash[i] != authenticator[i])
                        {
                            returnValue = false;
                            break;
                        }
                    }
                    returnValue = true;
                }
            }
            finally
            {
                ArrayClear(hash);
            }

            return returnValue;
        }

        public static bool AuthenticateRequest(Claim claim, string password, Message message)
        {
            PeerHashToken request = PeerRequestSecurityToken.CreateHashTokenFrom(message);
            return request.Validate(claim, password);
        }

        public static bool AuthenticateResponse(Claim claim, string password, Message message)
        {
            PeerHashToken request = PeerRequestSecurityTokenResponse.CreateHashTokenFrom(message);
            return request.Validate(claim, password);
        }

    }


    internal class PeerIdentityClaim
    {
        const string resourceValue = "peer";
        const string resourceRight = "peer";
        public const string PeerClaimType = PeerStrings.Namespace + "/peer";
        static internal Claim Claim()
        {
            return new Claim(PeerClaimType, resourceValue, resourceRight);
        }
        static internal bool IsMatch(EndpointIdentity identity)
        {
            return identity.IdentityClaim.ClaimType == PeerClaimType;
        }
    }

    class PeerDoNothingSecurityProtocol : SecurityProtocol
    {
        public PeerDoNothingSecurityProtocol(SecurityProtocolFactory factory) : base(factory, null, null) { }
        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
        }
        public override void VerifyIncomingMessage(ref Message request, TimeSpan timeout)
        {
            try
            {
                int i = request.Headers.FindHeader(SecurityJan2004Strings.Security, SecurityJan2004Strings.Namespace);
                if (i >= 0)
                {
                    request.Headers.AddUnderstood(i);
                }
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (XmlException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (SerializationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }

        }

        public override void OnAbort()
        {
        }

        public override void OnClose(TimeSpan timeout)
        {
        }

        public override void OnOpen(TimeSpan timeout)
        {
        }
    }

    class PeerDoNothingSecurityProtocolFactory : SecurityProtocolFactory
    {
        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new PeerDoNothingSecurityProtocol(this);
        }

        public override void OnAbort()
        {
        }


        public override void OnOpen(TimeSpan timeout)
        {
        }

        public override void OnClose(TimeSpan timeout)
        {
        }
    }

    class PeerIdentityVerifier : IdentityVerifier
    {
        public PeerIdentityVerifier() : base() { }
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            return true;
        }
        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            if (reference == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reference");

            identity = reference.Identity;
            if (identity == null)
            {
                identity = new PeerEndpointIdentity();
            }
            return true;
        }
    }

    class PeerEndpointIdentity : EndpointIdentity
    {
        public PeerEndpointIdentity()
            : base()
        {
            base.Initialize(PeerIdentityClaim.Claim());
        }
    }

    class PeerX509TokenProvider : X509SecurityTokenProvider
    {
        X509CertificateValidator validator;
        public PeerX509TokenProvider(X509CertificateValidator validator, X509Certificate2 credential)
            : base(credential)
        {
            this.validator = validator;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            X509SecurityToken token = (X509SecurityToken)base.GetTokenCore(timeout);
            if (validator != null)
            {
                validator.Validate(token.Certificate);
            }
            return token;
        }
    }

    class PeerCertificateClientCredentials : SecurityCredentialsManager
    {
        X509Certificate2 selfCertificate;
        X509CertificateValidator certificateValidator;

        public PeerCertificateClientCredentials(X509Certificate2 selfCertificate, X509CertificateValidator validator)
        {
            this.selfCertificate = selfCertificate;
            this.certificateValidator = validator;
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new PeerCertificateClientCredentialsSecurityTokenManager(this);
        }

        class PeerCertificateClientCredentialsSecurityTokenManager : SecurityTokenManager
        {
            PeerCertificateClientCredentials creds;

            public PeerCertificateClientCredentialsSecurityTokenManager(PeerCertificateClientCredentials creds)
            {
                this.creds = creds;
            }

            public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            {
                MessageSecurityTokenVersion messageVersion = (MessageSecurityTokenVersion)version;
                return new WSSecurityTokenSerializer(messageVersion.SecurityVersion, messageVersion.TrustVersion, messageVersion.SecureConversationVersion, messageVersion.EmitBspRequiredAttributes, null, null, null);
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
            {
                if (requirement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requirement");
                }
                if (requirement.TokenType == SecurityTokenTypes.X509Certificate && requirement.KeyUsage == SecurityKeyUsage.Signature)
                {
                    return new PeerX509TokenProvider(this.creds.certificateValidator, this.creds.selfCertificate);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }
        }
    }

    internal class PeerHashToken : SecurityToken
    {
        string id = SecurityUniqueId.Create().Value;
        Uri status;
        bool isValid;
        ReadOnlyCollection<SecurityKey> keys;
        internal const string TokenTypeString = PeerStrings.Namespace + "/peerhashtoken";
        internal const string RequestTypeString = "http://schemas.xmlsoap.org/ws/2005/02/trust/Validate";
        internal const string Action = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate";
        public const string PeerNamespace = PeerStrings.Namespace;
        public const string PeerTokenElementName = "PeerHashToken";
        public const string PeerAuthenticatorElementName = "Authenticator";
        public const string PeerPrefix = "peer";
        static PeerHashToken invalid = new PeerHashToken();

        byte[] authenticator;
        DateTime effectiveTime = DateTime.UtcNow;
        DateTime expirationTime = DateTime.UtcNow.AddHours(10);

        PeerHashToken()
        {
            CheckValidity();
        }

        public PeerHashToken(byte[] authenticator)
        {
            this.authenticator = authenticator;
            CheckValidity();
        }

        public PeerHashToken(X509Certificate2 certificate, string password)
        {
            this.authenticator = PeerSecurityHelpers.ComputeHash(certificate, password);
            CheckValidity();
        }

        public PeerHashToken(Claim claim, string password)
        {
            this.authenticator = PeerSecurityHelpers.ComputeHash(claim, password);
            CheckValidity();
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.expirationTime; }
        }

        public static PeerHashToken Invalid
        {
            get
            {
                return invalid;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (null == this.keys)
                {
                    this.keys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey>());
                }
                return this.keys;
            }
        }

        public Uri Status
        {
            get
            {
                return this.status;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public bool Validate(Claim claim, string password)
        {
            if (!(this.authenticator != null))
            {
                throw Fx.AssertAndThrow("Incorrect initialization");
            }
            bool result = PeerSecurityHelpers.Authenticate(claim, password, this.authenticator);
            return result;
        }

        void CheckValidity()
        {
            isValid = this.authenticator != null;
            status = new Uri(isValid ? PeerRequestSecurityTokenResponse.ValidString : PeerRequestSecurityTokenResponse.InvalidString);
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PeerPrefix, PeerTokenElementName, PeerNamespace);
            writer.WriteStartElement(PeerPrefix, PeerAuthenticatorElementName, PeerNamespace);
            writer.WriteString(Convert.ToBase64String(this.authenticator));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        internal static PeerHashToken CreateFrom(XmlElement child)
        {
            byte[] auth = null;
            foreach (XmlNode node in child.ChildNodes)
            {
                XmlElement element = (XmlElement)node;

                if (element == null || !PeerRequestSecurityToken.CompareWithNS(element.LocalName, element.NamespaceURI, PeerTokenElementName, PeerNamespace))
                    continue;
                if (element.ChildNodes.Count != 1)
                    break;
                XmlElement authElement = element.ChildNodes[0] as XmlElement;
                if (authElement == null || !PeerRequestSecurityToken.CompareWithNS(authElement.LocalName, authElement.NamespaceURI, PeerAuthenticatorElementName, PeerNamespace))
                    break;
                try
                {
                    auth = Convert.FromBase64String(XmlHelper.ReadTextElementAsTrimmedString(authElement));
                    break;
                }
                catch (ArgumentNullException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (FormatException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
            return new PeerHashToken(auth);
        }

        public override bool Equals(object token)
        {
            PeerHashToken that = token as PeerHashToken;
            if (that == null)
                return false;
            if (Object.ReferenceEquals(that, this))
                return true;
            if (this.authenticator != null && that.authenticator != null && this.authenticator.Length == that.authenticator.Length)
            {
                for (int i = 0; i < this.authenticator.Length; i++)
                {
                    if (this.authenticator[i] != that.authenticator[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return isValid ? this.authenticator.GetHashCode() : 0;
        }
    }

    class PeerSecurityTokenSerializer : WSSecurityTokenSerializer
    {
        public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            return null;
        }
    }

    internal class PeerRequestSecurityToken : RequestSecurityToken
    {
        PeerHashToken token;
        public const string TrustNamespace = TrustFeb2005Strings.Namespace;
        public const string PeerNamespace = PeerStrings.Namespace;
        public const string RequestElementName = "RequestSecurityToken";
        public const string RequestedSecurityTokenElementName = "RequestedSecurityToken";
        public const string PeerHashTokenElementName = "PeerHashToken";

        public PeerRequestSecurityToken(PeerHashToken token)
            : base()
        {
            this.token = token;
            this.TokenType = PeerHashToken.TokenTypeString;
            this.RequestType = PeerHashToken.RequestTypeString;
        }

        public PeerHashToken Token
        {
            get
            {
                return this.token;
            }
        }

        public static PeerHashToken CreateHashTokenFrom(Message message)
        {
            PeerHashToken token = PeerHashToken.Invalid;
            XmlReader reader = message.GetReaderAtBodyContents();
            RequestSecurityToken rst = RequestSecurityToken.CreateFrom(reader);
            XmlElement rstXml = rst.RequestSecurityTokenXml;
            if (rstXml != null)
            {

                //find the wrapper element
                foreach (XmlNode node in rst.RequestSecurityTokenXml.ChildNodes)
                {
                    XmlElement element = (XmlElement)node;
                    if (element == null || !PeerRequestSecurityToken.CompareWithNS(element.LocalName, element.NamespaceURI, PeerRequestSecurityToken.RequestedSecurityTokenElementName, TrustFeb2005Strings.Namespace))
                        continue;
                    token = PeerHashToken.CreateFrom(element);
                }
            }
            return token;
        }

        public PeerRequestSecurityToken CreateFrom(X509Certificate2 credential, string password)
        {
            PeerHashToken token = new PeerHashToken(credential, password);
            return new PeerRequestSecurityToken(token);
        }


        internal protected override void OnWriteCustomElements(XmlWriter writer)
        {
            if (!(token != null && token.IsValid))
            {
                throw Fx.AssertAndThrow("Could not construct a valid RST without token!");
            }
            string wstprefix = writer.LookupPrefix(TrustNamespace);

            writer.WriteStartElement(wstprefix, TrustFeb2005Strings.RequestedSecurityToken, TrustFeb2005Strings.Namespace);
            token.Write(writer);
            writer.WriteEndElement();
        }

        internal protected override void OnMakeReadOnly() { }
        internal static bool CompareWithNS(string first, string firstNS, string second, string secondNS)
        {
            return ((String.Compare(first, second, StringComparison.Ordinal) == 0)
                && (String.Compare(firstNS, secondNS, StringComparison.OrdinalIgnoreCase) == 0));
        }
    }

    class PeerRequestSecurityTokenResponse : RequestSecurityTokenResponse
    {
        public const string Action = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate";
        public const string ValidString = "http://schemas.xmlsoap.org/ws/2005/02/trust/status/valid";
        public const string InvalidString = "http://schemas.xmlsoap.org/ws/2005/02/trust/status/invalid";
        public const string StatusString = "Status";
        public const string CodeString = "Code";

        PeerHashToken token;
        bool isValid = false;

        public PeerRequestSecurityTokenResponse()
            : this(null)
        {
        }

        public PeerRequestSecurityTokenResponse(PeerHashToken token)
        {
            this.token = token;
            this.isValid = (token != null && token.IsValid);
        }

        public PeerHashToken Token
        {
            get
            {
                if (!(this.isValid))
                {
                    throw Fx.AssertAndThrow("should not be called when the token is invalid!");
                }
                return this.token;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public static PeerHashToken CreateHashTokenFrom(Message message)
        {
            PeerHashToken token = PeerHashToken.Invalid;
            RequestSecurityTokenResponse response = RequestSecurityTokenResponse.CreateFrom(message.GetReaderAtBodyContents(), MessageSecurityVersion.Default, new PeerSecurityTokenSerializer());
            if (String.Compare(response.TokenType, PeerHashToken.TokenTypeString, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return token;
            }
            XmlElement responseXml = response.RequestSecurityTokenResponseXml;
            if (responseXml != null)
            {
                foreach (XmlElement child in responseXml.ChildNodes)
                {
                    if (PeerRequestSecurityToken.CompareWithNS(child.LocalName, child.NamespaceURI, StatusString, TrustFeb2005Strings.Namespace))
                    {
                        if (child.ChildNodes.Count == 1)
                        {
                            XmlElement desc = (child.ChildNodes[0] as XmlElement);
                            if (PeerRequestSecurityToken.CompareWithNS(desc.LocalName, desc.NamespaceURI, CodeString, TrustFeb2005Strings.Namespace))
                            {
                                string code = XmlHelper.ReadTextElementAsTrimmedString(desc);
                                if (String.Compare(code, ValidString, StringComparison.OrdinalIgnoreCase) != 0)
                                    break;
                            }
                        }
                    }
                    else if (PeerRequestSecurityToken.CompareWithNS(child.LocalName, child.NamespaceURI, TrustFeb2005Strings.RequestedSecurityToken, TrustFeb2005Strings.Namespace))
                    {
                        token = PeerHashToken.CreateFrom(child);
                        break;
                    }
                }
            }
            return token;
        }

        public static RequestSecurityTokenResponse CreateFrom(X509Certificate2 credential, string password)
        {
            PeerHashToken token = new PeerHashToken(credential, password);
            return new PeerRequestSecurityTokenResponse(token);
        }

        internal protected override void OnWriteCustomElements(XmlWriter writer)
        {
            string wstprefix = writer.LookupPrefix(TrustFeb2005Strings.Namespace);

            writer.WriteStartElement(wstprefix, TrustFeb2005Strings.TokenType, TrustFeb2005Strings.Namespace);
            writer.WriteString(PeerHashToken.TokenTypeString);
            writer.WriteEndElement();

            writer.WriteStartElement(wstprefix, StatusString, TrustFeb2005Strings.Namespace);
            writer.WriteStartElement(wstprefix, CodeString, TrustFeb2005Strings.Namespace);
            if (!this.IsValid)
                writer.WriteString(InvalidString);
            else
                writer.WriteString(ValidString);
            writer.WriteEndElement();
            writer.WriteEndElement();
            if (this.IsValid)
            {
                writer.WriteStartElement(wstprefix, PeerRequestSecurityToken.RequestedSecurityTokenElementName, TrustFeb2005Strings.Namespace);
                this.token.Write(writer);
                writer.WriteEndElement();
            }
        }
    }

    class PeerChannelAuthenticatorExtension : IExtension<IPeerNeighbor>
    {
        IPeerNeighbor host;
        PeerSecurityManager securityManager;
        PeerAuthState state;
        EventArgs originalArgs;
        EventHandler onSucceeded;
        IOThreadTimer timer = null;
        object thisLock = new object();
        static TimeSpan Timeout = new TimeSpan(0, 2, 0);
        string meshId;

        enum PeerAuthState
        {
            Created,
            Authenticated,
            Failed
        }

        public PeerChannelAuthenticatorExtension(PeerSecurityManager securityManager, EventHandler onSucceeded, EventArgs args, string meshId)
        {
            this.securityManager = securityManager;
            this.state = PeerAuthState.Created;
            this.originalArgs = args;
            this.onSucceeded = onSucceeded;
            this.meshId = meshId;
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public void Attach(IPeerNeighbor host)
        {
            Fx.AssertAndThrow(this.securityManager.AuthenticationMode == PeerAuthenticationMode.Password, "Invalid AuthenticationMode!");
            Fx.AssertAndThrow(host != null, "unrecognized host!");
            this.host = host;
            this.timer = new IOThreadTimer(new Action<object>(OnTimeout), null, true);
            this.timer.Set(Timeout);
        }

        static public void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            Fx.Assert(neighbor != null, "Neighbor must have a value");
            PeerChannelAuthenticatorExtension ext = neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>();
            if (ext != null) neighbor.Extensions.Remove(ext);
        }

        public void Detach(IPeerNeighbor host)
        {

            Fx.Assert(host != null, "unrecognized host!");
            if (host.State < PeerNeighborState.Authenticated)
            {
                OnFailed(host);
            }
            this.host = null;
            this.timer.Cancel();
        }

        void OnTimeout(object state)
        {
            IPeerNeighbor neighbor = host;
            if (neighbor == null)
                return;
            if (neighbor.State < PeerNeighborState.Authenticated)
            {
                OnFailed(neighbor);
            }
        }

        public void InitiateHandShake()
        {
            IPeerNeighbor neighbor = host;
            Message reply = null;

            Fx.Assert(host != null, "Cannot initiate security handshake without a host!");

            //send the RST message.
            using (OperationContextScope scope = new OperationContextScope(new OperationContext((ServiceHostBase)null)))
            {
                PeerHashToken token = this.securityManager.GetSelfToken();
                Message request = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, TrustFeb2005Strings.RequestSecurityToken, new PeerRequestSecurityToken(token));
                bool fatal = false;
                try
                {
                    reply = neighbor.RequestSecurityToken(request);

                    if (!(reply != null))
                    {
                        throw Fx.AssertAndThrow("SecurityHandshake return empty message!");
                    }
                    ProcessRstr(neighbor, reply, PeerSecurityManager.FindClaim(ServiceSecurityContext.Current));
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        fatal = true;
                        throw;
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    this.state = PeerAuthState.Failed;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        ServiceSecurityContext context = ServiceSecurityContext.Current;
                        ClaimSet claimSet = null;
                        if (context != null && context.AuthorizationContext != null && context.AuthorizationContext.ClaimSets != null && context.AuthorizationContext.ClaimSets.Count > 0)
                            claimSet = context.AuthorizationContext.ClaimSets[0];
                        PeerAuthenticationFailureTraceRecord record = new PeerAuthenticationFailureTraceRecord(
                                                                    meshId,
                                                                    neighbor.ListenAddress.EndpointAddress.ToString(),
                                                                    claimSet,
                                                                    e);
                        TraceUtility.TraceEvent(TraceEventType.Error,
                            TraceCode.PeerNodeAuthenticationFailure, SR.GetString(SR.TraceCodePeerNodeAuthenticationFailure),
                            record, this, null);
                    }
                    neighbor.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
                }
                finally
                {
                    if (!fatal)
                        request.Close();
                }
            }
        }

        public Message ProcessRst(Message message, Claim claim)
        {
            IPeerNeighbor neighbor = host;
            PeerRequestSecurityTokenResponse response = null;
            Message reply = null;

            lock (ThisLock)
            {
                if (this.state != PeerAuthState.Created || neighbor == null || neighbor.IsInitiator || neighbor.State != PeerNeighborState.Opened)
                {
                    OnFailed(neighbor);
                    return null;
                }
            }

            try
            {
                PeerHashToken receivedToken = PeerRequestSecurityToken.CreateHashTokenFrom(message);
                PeerHashToken expectedToken = securityManager.GetExpectedTokenForClaim(claim);

                if (!expectedToken.Equals(receivedToken))
                {
                    OnFailed(neighbor);
                }
                else
                {
                    this.state = PeerAuthState.Authenticated;
                    PeerHashToken selfToken = securityManager.GetSelfToken();
                    response = new PeerRequestSecurityTokenResponse(selfToken);
                    reply = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, TrustFeb2005Strings.RequestSecurityTokenResponse, response);
                    OnAuthenticated();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                OnFailed(neighbor);
            }
            return reply;
        }

        public void ProcessRstr(IPeerNeighbor neighbor, Message message, Claim claim)
        {
            PeerHashToken receivedToken = PeerRequestSecurityTokenResponse.CreateHashTokenFrom(message);

            if (!receivedToken.IsValid)
            {
                OnFailed(neighbor);
            }
            else
            {
                PeerHashToken expectedToken = securityManager.GetExpectedTokenForClaim(claim);
                if (!expectedToken.Equals(receivedToken))
                    OnFailed(neighbor);
                else
                    OnAuthenticated();
            }
        }

        public void OnAuthenticated()
        {
            IPeerNeighbor neighbor = null;
            lock (ThisLock)
            {
                this.timer.Cancel();
                neighbor = this.host;
                this.state = PeerAuthState.Authenticated;
            }
            if (neighbor == null)
                return;
            neighbor.TrySetState(PeerNeighborState.Authenticated);
            onSucceeded(neighbor, originalArgs);
        }

        void OnFailed(IPeerNeighbor neighbor)
        {
            lock (ThisLock)
            {
                this.state = PeerAuthState.Failed;
                this.timer.Cancel();
                this.host = null;
            }
            if (DiagnosticUtility.ShouldTraceError)
            {
                PeerAuthenticationFailureTraceRecord record = null;
                String remoteUri = "";
                PeerNodeAddress remoteAddress = neighbor.ListenAddress;
                if (remoteAddress != null)
                {
                    remoteUri = remoteAddress.EndpointAddress.ToString();
                }
                OperationContext opContext = OperationContext.Current;
                if (opContext != null)
                {
                    remoteUri = opContext.IncomingMessageProperties.Via.ToString();
                    ServiceSecurityContext secContext = opContext.ServiceSecurityContext;
                    if (secContext != null)
                    {
                        record = new PeerAuthenticationFailureTraceRecord(
                            meshId,
                            remoteUri,
                            secContext.AuthorizationContext.ClaimSets[0], null);

                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            TraceUtility.TraceEvent(
                                TraceEventType.Error,
                                TraceCode.PeerNodeAuthenticationFailure,
                                SR.GetString(SR.TraceCodePeerNodeAuthenticationFailure),
                                record,
                                this,
                                null);
                        }
                    }
                }
                else
                {
                    record = new PeerAuthenticationFailureTraceRecord(meshId, remoteUri);
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error,
                                                TraceCode.PeerNodeAuthenticationTimeout,
                                                SR.GetString(SR.TraceCodePeerNodeAuthenticationTimeout),
                                                record,
                                                this,
                                                null);
                    }
                }
            }
            neighbor.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
        }
    }
}

namespace System.ServiceModel.Channels
{
    internal enum PeerAuthenticationMode
    {
        None = 0,
        Password = 1,
        MutualCertificate = 2
    }
}




