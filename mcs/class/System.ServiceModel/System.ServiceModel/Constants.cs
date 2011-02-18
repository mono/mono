//
// Constants.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Xml;

namespace System.ServiceModel
{
	internal class Constants
	{
		public const string Soap11 = "http://schemas.xmlsoap.org/soap/envelope/";
		public const string Soap12 = "http://www.w3.org/2003/05/soap-envelope";

		public const string WSBasicSecurityProfileCore1 = "http://ws-i.org/profiles/basic-security/core/1.0";

		public const string WsaAnonymousUri = "http://www.w3.org/2005/08/addressing/anonymous";
		public const string WsaIdentityUri = "http://schemas.xmlsoap.org/ws/2006/02/addressingidentity";

		public const string MSSerialization = "http://schemas.microsoft.com/2003/10/Serialization/";

		public const string WssKeyIdentifierX509Thumbptint = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1";

		public const string WssBase64BinaryEncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

		public const string WssKeyIdentifierEncryptedKey = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1";

		public const string XmlDsig = "http://www.w3.org/2000/09/xmldsig#";

		public const string WSSSamlToken = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";
		public const string WSSX509Token = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
		public const string WssKeyIdentifierSamlAssertion = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID";
		public const string WSSUserNameToken = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#UsernameToken";
		public const string WsscContextToken = "http://schemas.xmlsoap.org/ws/2005/02/sc/sct";
		public const string WSSKerberosToken = "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ";
		public const string WSSEncryptedKeyToken = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey";

		public const string WstNamespace = "http://schemas.xmlsoap.org/ws/2005/02/trust";
		public const string WssNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
		public const string Wss11Namespace = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";
		public const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";
		// FIXME: eliminate use of this incorrectly-fixing-addressing-version constants (remains few).
		public const string WsaNamespace = "http://www.w3.org/2005/08/addressing";
		public const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
		public const string WsscNamespace = "http://schemas.xmlsoap.org/ws/2005/02/sc";
		public const string WsidNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";

		public const string WstIssueAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";
		public const string WstRenewAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Renew";
		public const string WstCancelAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Cancel";
		public const string WstValidateAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate";
		public const string WstIssueReplyAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue";
		public const string WstRenewReplyAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Renew";
		public const string WstCancelReplyAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Cancel";
		public const string WstValidateReplyAction = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate";

		public const string WsscDefaultLabel = "WS-SecureConversationWS-SecureConversation";

		// .NET BUG: it requires extra white space !
		public const string WstBinaryExchangeValueTls = " http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego";
		public const string WstBinaryExchangeValueGss = "http://schemas.xmlsoap.org/ws/2005/02/trust/spnego";

		public const string MSTlsnegoTokenContent = "http://schemas.microsoft.com/ws/2006/05/security";

		public const string WstTlsnegoProofTokenType = "http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap";
		public const string WstSpnegoProofTokenType = "http://schemas.xmlsoap.org/2005/02/trust/spnego#TLS_Wrap";

		public const string WstIssueRequest = "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue";
		public const string WstRenewRequest = "http://schemas.xmlsoap.org/ws/2005/02/trust/Renew";
		public const string WstCancelRequest = "http://schemas.xmlsoap.org/ws/2005/02/trust/Cancel";
		public const string WstValidateRequest = "http://schemas.xmlsoap.org/ws/2005/02/trust/Validate";

		public const string WstSymmetricKeyTypeUri = "http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey";
		public const string WstAsymmetricKeyTypeUri = "http://schemas.xmlsoap.org/ws/2005/02/trust/AsymmetricKey";

		public const string LifetimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffZ";

		// Those OIDs except for Kerberos5 are described here:
		// http://www.alvestrand.no/objectid/
		// (searching web for those OIDs would give you pointers.)
		public const string OidSpnego = "1.3.6.1.5.5.2";
		public const string OidNtlmSsp = "1.3.6.1.4.1.311.2.2.10";
		public const string OidKerberos5 = "1.2.840.48018.1.2.2";
		public const string OidMIT = "1.2.840.113554.1.2.2";

		// Peer resolvers
		public const string NetPeer = "http://schemas.microsoft.com/net/2006/05/peer";

		// See [MC-NBFS] in Microsoft OSP. The strings are copied from the PDF, so the actual values might be wrong.
		static readonly string [] dict_strings = {
			"mustUnderstand", "Envelope",
			"http://www.w3.org/2003/05/soap-envelope",
			"http://www.w3.org/2005/08/addressing", "Header", "Action", "To", "Body", "Algorithm", "RelatesTo",
			"http://www.w3.org/2005/08/addressing/anonymous", "URI", "Reference", "MessageID", "Id", "Identifier",
			"http://schemas.xmlsoap.org/ws/2005/02/rm", "Transforms", "Transform", "DigestMethod", "DigestValue", "Address", "ReplyTo", "SequenceAcknowledgement", "AcknowledgementRange", "Upper", "Lower", "BufferRemaining",
			"http://schemas.microsoft.com/ws/2006/05/rm",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement", "SecurityTokenReference", "Sequence", "MessageNumber",
			"http://www.w3.org/2000/09/xmldsig#",
			"http://www.w3.org/2000/09/xmldsig#enveloped-signature", "KeyInfo",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
			"http://www.w3.org/2001/04/xmlenc#",
			"http://schemas.xmlsoap.org/ws/2005/02/sc", "DerivedKeyToken", "Nonce", "Signature", "SignedInfo", "CanonicalizationMethod", "SignatureMethod", "SignatureValue", "DataReference", "EncryptedData", "EncryptionMethod", "CipherData", "CipherValue",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", "Security", "Timestamp", "Created", "Expires", "Length", "ReferenceList", "ValueType", "Type", "EncryptedHeader",
			"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd", "RequestSecurityTokenResponseCollection",
			"http://schemas.xmlsoap.org/ws/2005/02/trust",
			"http://schemas.xmlsoap.org/ws/2005/02/trust#BinarySecret",
			"http://schemas.microsoft.com/ws/2006/02/transactions", "s", "Fault", "MustUnderstand", "role", "relay", "Code", "Reason", "Text", "Node", "Role", "Detail", "Value", "Subcode", "NotUnderstood", "qname", "", "From", "FaultTo", "EndpointReference", "PortType", "ServiceName", "PortName", "ReferenceProperties", "RelationshipType", "Reply", "a",
			"http://schemas.xmlsoap.org/ws/2006/02/addressingidentity", "Identity", "Spn", "Upn", "Rsa", "Dns", "X509v3Certificate",
			"http://www.w3.org/2005/08/addressing/fault", "ReferenceParameters", "IsReferenceParameter",
			"http://www.w3.org/2005/08/addressing/reply",
			"http://www.w3.org/2005/08/addressing/none", "Metadata",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing/fault",
			"http://schemas.xmlsoap.org/ws/2004/06/addressingex", "RedirectTo", "Via",
			"http://www.w3.org/2001/10/xml-exc-c14n#", "PrefixList", "InclusiveNamespaces", "ec", "SecurityContextToken", "Generation", "Label", "Offset", "Properties", "Cookie", "wsc",
			"http://schemas.xmlsoap.org/ws/2004/04/sc",
			"http://schemas.xmlsoap.org/ws/2004/04/security/sc/dk",
			"http://schemas.xmlsoap.org/ws/2004/04/security/sc/sct",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT", "RenewNeeded", "BadContextToken", "c",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/dk",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/sct",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Cancel",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Cancel",
			"http://www.w3.org/2001/04/xmlenc#aes128-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes128",
			"http://www.w3.org/2001/04/xmlenc#aes192-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes192",
			"http://www.w3.org/2001/04/xmlenc#aes256-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes256",
			"http://www.w3.org/2001/04/xmlenc#des-cbc",
			"http://www.w3.org/2000/09/xmldsig#dsa-sha1",
			"http://www.w3.org/2001/10/xml-exc-c14n#WithComments",
			"http://www.w3.org/2000/09/xmldsig#hmac-sha1",
			"http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1",
			"http://www.w3.org/2001/04/xmlenc#ripemd160",
			"http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p",
			"http://www.w3.org/2000/09/xmldsig#rsa-sha1",
			"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
			"http://www.w3.org/2001/04/xmlenc#rsa-1_5",
			"http://www.w3.org/2000/09/xmldsig#sha1",
			"http://www.w3.org/2001/04/xmlenc#sha256",
			"http://www.w3.org/2001/04/xmlenc#sha512",
			"http://www.w3.org/2001/04/xmlenc#tripledes-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-tripledes",
			"http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap",
			"http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap",
			"http://schemas.microsoft.com/ws/2006/05/security", "dnse", "o", "Password", "PasswordText", "Username", "UsernameToken", "BinarySecurityToken", "EncodingType", "KeyIdentifier",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ1510",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID", "Assertion", "urn:oasis:names:tc:SAML:1.0:assertion",
			"http://docs.oasis-open.org/wss/oasis-wss-rel-token-profile-1.0.pdf#license", "FailedAuthentication", "InvalidSecurityToken", "InvalidSecurity", "k", "SignatureConfirmation", "TokenType",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID", "AUTH-HASH", "RequestSecurityTokenResponse", "KeySize", "RequestedTokenReference", "AppliesTo", "Authenticator", "CombinedHash", "BinaryExchange", "Lifetime", "RequestedSecurityToken", "Entropy", "RequestedProofToken", "ComputedKey", "RequestSecurityToken", "RequestType", "Context", "BinarySecret",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/spnego",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego", "wst",
			"http://schemas.xmlsoap.org/ws/2004/04/trust",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/CK/PSHA1",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/Nonce", "KeyType",
			"http://schemas.xmlsoap.org/ws/2004/04/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2004/04/trust/PublicKey", "Claims", "InvalidRequest", "RequestFailed", "SignWith", "EncryptWith", "EncryptionAlgorithm", "CanonicalizationAlgorithm", "ComputedKeyAlgorithm", "UseKey",
			"http://schemas.microsoft.com/net/2004/07/secext/WS-SPNego",
			"http://schemas.microsoft.com/net/2004/07/secext/TLSNego", "t",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Nonce", "RenewTarget", "CancelTarget", "RequestedTokenCancelled", "RequestedAttachedReference", "RequestedUnattachedReference", "IssuedTokens",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Cancel",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/PublicKey", "Access", "AccessDecision", "Advice", "AssertionID", "AssertionIDReference", "Attribute", "AttributeName", "AttributeNamespace", "AttributeStatement", "AttributeValue", "Audience", "AudienceRestrictionCondition", "AuthenticationInstant", "AuthenticationMethod", "AuthenticationStatement", "AuthorityBinding", "AuthorityKind", "AuthorizationDecisionStatement", "Binding", "Condition", "Conditions", "Decision", "DoNotCacheCondition", "Evidence", "IssueInstant", "Issuer", "Location", "MajorVersion", "MinorVersion", "NameIdentifier", "Format", "NameQualifier", "Namespace", "NotBefore", "NotOnOrAfter", "saml", "Statement", "Subject", "SubjectConfirmation", "SubjectConfirmationData", "ConfirmationMethod", "urn:oasis:names:tc:SAML:1.0:cm:holder-of-key", "urn:oasis:names:tc:SAML:1.0:cm:sender-vouches", "SubjectLocality", "DNSAddress", "IPAddress", "SubjectStatement", "urn:oasis:names:tc:SAML:1.0:am:unspecified", "xmlns", "Resource", "UserName", "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName", "EmailName", "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress", "u", "ChannelInstance",
			"http://schemas.microsoft.com/ws/2005/02/duplex", "Encoding", "MimeType", "CarriedKeyName", "Recipient", "EncryptedKey", "KeyReference", "e",
			"http://www.w3.org/2001/04/xmlenc#Element",
			"http://www.w3.org/2001/04/xmlenc#Content", "KeyName", "MgmtData", "KeyValue", "RSAKeyValue", "Modulus", "Exponent", "X509Data", "X509IssuerSerial", "X509IssuerName", "X509SerialNumber", "X509Certificate", "AckRequested",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/AckRequested", "AcksTo", "Accept", "CreateSequence",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequence", "CreateSequenceRefused", "CreateSequenceResponse",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequenceResponse", "FaultCode", "InvalidAcknowledgement", "LastMessage",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage", "LastMessageNumberExceeded", "MessageNumberRollover", "Nack", "netrm", "Offer", "r", "SequenceFault", "SequenceTerminated", "TerminateSequence",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence", "UnknownSequence",
			"http://schemas.microsoft.com/ws/2006/02/tx/oletx", "oletx", "OleTxTransaction", "PropagationToken",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor", "wscoor", "CreateCoordinationContext", "CreateCoordinationContextResponse", "CoordinationContext", "CurrentContext", "CoordinationType", "RegistrationService", "Register", "RegisterResponse", "ProtocolIdentifier", "CoordinatorProtocolService", "ParticipantProtocolService",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", "ActivationCoordinatorPortType", "RegistrationCoordinatorPortType", "InvalidState", "InvalidProtocol", "InvalidParameters", "NoActivity", "ContextRefused", "AlreadyRegistered",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat", "wsat",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Completion",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Durable2PC",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Volatile2PC", "Prepare", "Prepared", "ReadOnly", "Commit", "Rollback", "Committed", "Aborted", "Replay",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Commit",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Rollback",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepare",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepared",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/ReadOnly",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Replay",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", "CompletionCoordinatorPortType", "CompletionParticipantPortType", "CoordinatorPortType", "ParticipantPortType", "InconsistentInternalState", "mstx", "Enlistment", "protocol", "LocalTransactionId", "IsolationLevel", "IsolationFlags", "Description", "Loopback", "RegisterInfo", "ContextId", "TokenId", "AccessDenied", "InvalidPolicy", "CoordinatorRegistrationFailed", "TooManyEnlistments", "Disabled", "ActivityId",
			"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#Kerberosv5APREQSHA1",
			"http://schemas.xmlsoap.org/ws/2002/12/policy", "FloodMessage", "LinkUtility", "Hops",
			"http://schemas.microsoft.com/net/2006/05/peer/HopCount", "PeerVia",
			"http://schemas.microsoft.com/net/2006/05/peer", "PeerFlooder", "PeerTo",
			"http://schemas.microsoft.com/ws/2005/05/routing", "PacketRoutable",
			"http://schemas.microsoft.com/ws/2005/05/addressing/none",
			"http://schemas.microsoft.com/ws/2005/05/envelope/none",
			"http://www.w3.org/2001/XMLSchema-instance",
			"http://www.w3.org/2001/XMLSchema", "nil", "type", "char", "boolean", "byte", "unsignedByte", "short", "unsignedShort", "int", "unsignedInt", "long", "unsignedLong", "float", "double", "decimal", "dateTime", "string", "base64Binary", "anyType", "duration", "guid", "anyURI", "QName", "time", "date", "hexBinary", "gYearMonth", "gYear", "gMonthDay", "gDay", "gMonth", "integer", "positiveInteger", "negativeInteger", "nonPositiveInteger", "nonNegativeInteger", "normalizedString", "ConnectionLimitReached",
			"http://schemas.xmlsoap.org/soap/envelope/", "Actor", "Faultcode", "Faultstring", "Faultactor", "Detail"
		};

		static Constants ()
		{
			var d = new XmlDictionary ();
			SoapDictionary = d;
			foreach (var s in dict_strings)
				d.Add (s);
		}

		public static XmlDictionary SoapDictionary { get; private set; }
	}
}
