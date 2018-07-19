//-----------------------------------------------------------------------
// <copyright file="WSTrustFeb2005Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Defines constants for WS-Trust version Feb,2005
    /// </summary>
    internal static class WSTrustFeb2005Constants
    {
#pragma warning disable 1591
        public const string NamespaceURI = "http://schemas.xmlsoap.org/ws/2005/02/trust";
        public const string Prefix = "t";
        public const string SchemaLocation = "http://schemas.xmlsoap.org/ws/2005/02/trust/ws-trust.xsd";

        public const string Schema = @"<?xml version='1.0' encoding='utf-8'?>
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:wst='http://schemas.xmlsoap.org/ws/2005/02/trust'
           targetNamespace='http://schemas.xmlsoap.org/ws/2005/02/trust'
           elementFormDefault='qualified' >

<xs:element name='RequestSecurityToken' type='wst:RequestSecurityTokenType' />
  <xs:complexType name='RequestSecurityTokenType' >
    <xs:choice minOccurs='0' maxOccurs='unbounded' >
        <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded' />
    </xs:choice>
    <xs:attribute name='Context' type='xs:anyURI' use='optional' />
    <xs:anyAttribute namespace='##other' processContents='lax' />
  </xs:complexType>

<xs:element name='RequestSecurityTokenResponse' type='wst:RequestSecurityTokenResponseType' />
  <xs:complexType name='RequestSecurityTokenResponseType' >
    <xs:choice minOccurs='0' maxOccurs='unbounded' >
        <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded' />
    </xs:choice>
    <xs:attribute name='Context' type='xs:anyURI' use='optional' />
    <xs:anyAttribute namespace='##other' processContents='lax' />
  </xs:complexType>

        </xs:schema>";

        public static class Actions
        {
            public const string Issue = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";
            public const string IssueResponse = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue";

            public const string Renew = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Renew";
            public const string RenewResponse = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Renew";

            public const string Validate = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate";
            public const string ValidateResponse = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate";

            public const string Cancel = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Cancel";
            public const string CancelResponse = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Cancel";

            public const string RequestSecurityContextToken = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT";
            public const string RequestSecurityContextTokenResponse = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT";

            public const string RequestSecurityContextTokenCancel = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT-Cancel";
            public const string RequestSecurityContextTokenResponseCancel = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT-Cancel";
        }

        public static class AttributeNames
        {
            public const string Allow = "Allow";
            public const string Context = "Context";
            public const string EncodingType = "EncodingType";
            public const string OK = "OK";
            public const string Type = "Type";
            public const string ValueType = "ValueType";
            public const string Dialect = "Dialect";
        }

        public static class ElementNames
        {
            public const string AllowPostdating = "AllowPostdating";
            public const string AuthenticationType = "AuthenticationType";
            public const string BinarySecret = "BinarySecret";
            public const string BinaryExchange = "BinaryExchange";
            public const string Code = "Code";
            public const string Delegatable = "Delegatable";
            public const string DelegateTo = "DelegateTo";
            public const string Encryption = "Encryption";
            public const string EncryptionAlgorithm = "EncryptionAlgorithm";
            public const string EncryptWith = "EncryptWith";
            public const string Entropy = "Entropy";
            public const string Forwardable = "Forwardable";
            public const string Lifetime = "Lifetime";
            public const string Claims = "Claims";
            public const string ComputedKey = "ComputedKey";
            public const string ComputedKeyAlgorithm = "ComputedKeyAlgorithm";
            public const string CanonicalizationAlgorithm = "CanonicalizationAlgorithm";
            public const string CancelTarget = "CancelTarget";
            public const string Issuer = "Issuer";
            public const string KeyType = "KeyType";
            public const string KeySize = "KeySize";
            public const string OnBehalfOf = "OnBehalfOf";
            public const string Participant = "Participant";
            public const string Participants = "Participants";
            public const string Primary = "Primary";
            public const string ProofEncryption = "ProofEncryption";
            public const string Reason = "Reason";
            public const string Renewing = "Renewing";
            public const string RenewTarget = "RenewTarget";
            public const string RequestType = "RequestType";
            public const string RequestSecurityTokenResponse = "RequestSecurityTokenResponse";
            public const string RequestSecurityToken = "RequestSecurityToken";
            public const string RequestedSecurityToken = "RequestedSecurityToken";
            public const string RequestedProofToken = "RequestedProofToken";
            public const string RequestKeySize = "RequestKeySize";
            public const string RequestedAttachedReference = "RequestedAttachedReference";
            public const string RequestedUnattachedReference = "RequestedUnattachedReference";
            public const string RequestedTokenCancelled = "RequestedTokenCancelled";
            public const string SecurityContextToken = "SecurityContextToken";
            public const string SignatureAlgorithm = "SignatureAlgorithm";
            public const string SignWith = "SignWith";
            public const string Status = "Status";
            public const string TokenType = "TokenType";
            public const string UseKey = "UseKey";                       
        }

        public static class FaultCodeValues
        {
            public const string AuthenticationBadElements = "AuthenticationBadElements";
            public const string BadRequest = "BadRequest";
            public const string ExpiredData = "ExpiredData";
            public const string FailedAuthentication = "FailedAuthentication";
            public const string InvalidRequest = "InvalidRequest";
            public const string InvalidScope = "InvalidScope";
            public const string InvalidSecurityToken = "InvalidSecurityToken";
            public const string InvalidTimeRange = "InvalidTimeRange";
            public const string RenewNeeded = "RenewNeeded";
            public const string RequestFailed = "RequestFailed";
            public const string UnableToRenew = "UnableToRenew";
        }

        public static class RequestTypes
        {
            public const string Issue = "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue";
            public const string Renew = "http://schemas.xmlsoap.org/ws/2005/02/trust/Renew";
            public const string Validate = "http://schemas.xmlsoap.org/ws/2005/02/trust/Validate";
            public const string Cancel = "http://schemas.xmlsoap.org/ws/2005/02/trust/Cancel";
        }

        public static class KeyTypes
        {
            public const string Asymmetric = "http://schemas.xmlsoap.org/ws/2005/02/trust/PublicKey";
            public const string Symmetric = "http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey";
            public const string Bearer = "http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey";
        }
        
        public static class ComputedKeyAlgorithms
        {
            public const string PSHA1 = "http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1";
        }

#pragma warning restore 1591
    }
}
