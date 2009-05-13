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


namespace Mono.ServiceModel.IdentitySelectors
{
	internal class Constants
	{
		public const string WSA1 = "http://www.w3.org/2005/08/addressing";

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
	}
}
