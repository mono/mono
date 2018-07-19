//-----------------------------------------------------------------------
// <copyright file="WSTrustConstantsAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    internal abstract class WSTrustConstantsAdapter
    {
        private static WSTrustAttributeNames attributeNames;
        private static WSTrustElementNames elementNames;
        private static FaultCodeValues faultCodes;

        private string namespaceURI;
        private string prefix;

        internal static WSTrustFeb2005ConstantsAdapter TrustFeb2005
        {
            get
            {
                return WSTrustFeb2005ConstantsAdapter.Instance;
            }
        }

        internal static WSTrust13ConstantsAdapter Trust13
        {
            get
            {
                return WSTrust13ConstantsAdapter.Instance;
            }
        }

        internal string NamespaceURI
        {
            get { return this.namespaceURI; }
            set { this.namespaceURI = value; }
        }

        internal string Prefix
        {
            get { return this.prefix; }
            set { this.prefix = value; }
        }

        internal abstract WSTrustActions Actions
        {
            get;
        }

        internal virtual WSTrustAttributeNames Attributes
        {
            get
            {
                if (attributeNames == null)
                {
                    attributeNames = new WSTrustAttributeNames();
                }

                return attributeNames;
            }
        }

        internal abstract WSTrustComputedKeyAlgorithm ComputedKeyAlgorithm
        {
            get;
        }

        internal virtual WSTrustElementNames Elements
        {
            get
            {
                if (elementNames == null)
                {
                    elementNames = new WSTrustElementNames();
                }

                return elementNames;
            }
        }

        internal virtual FaultCodeValues FaultCodes
        {
            get
            {
                if (faultCodes == null)
                {
                    faultCodes = new FaultCodeValues();
                }

                return faultCodes;
            }
        }

        internal abstract WSTrustRequestTypes RequestTypes
        {
            get;
        }

        internal abstract WSTrustKeyTypes KeyTypes
        {
            get;
        }

        internal static WSTrustConstantsAdapter GetConstantsAdapter(string ns)
        {
            if (StringComparer.Ordinal.Equals(ns, WSTrustFeb2005Constants.NamespaceURI))
            {
                return WSTrustConstantsAdapter.TrustFeb2005;
            }
            else if (StringComparer.Ordinal.Equals(ns, WSTrust13Constants.NamespaceURI))
            {
                return WSTrustConstantsAdapter.Trust13;
            }

            return null;
        }

        internal abstract class WSTrustActions
        {
            internal string Cancel
            {
                get;

                set;
            }

            internal string CancelResponse
            {
                get;

                set;
            }

            internal string Issue
            {
                get;

                set;
            }

            internal string IssueResponse
            {
                get;

                set;
            }

            internal string Renew
            {
                get;

                set;
            }

            internal string RenewResponse
            {
                get;

                set;
            }

            internal string RequestSecurityContextToken
            {
                get;

                set;
            }

            internal string RequestSecurityContextTokenCancel
            {
                get;

                set;
            }

            internal string RequestSecurityContextTokenResponse
            {
                get;

                set;
            }

            internal string RequestSecurityContextTokenResponseCancel
            {
                get;

                set;
            }

            internal string Validate
            {
                get;

                set;
            }

            internal string ValidateResponse
            {
                get;

                set;
            }
        }

        internal class WSTrustAttributeNames
        {
            private string allow = WSTrustFeb2005Constants.AttributeNames.Allow;
            private string context = WSTrustFeb2005Constants.AttributeNames.Context;
            private string dialect = WSTrustFeb2005Constants.AttributeNames.Dialect;
            private string encodingType = WSTrustFeb2005Constants.AttributeNames.EncodingType;
            private string oK = WSTrustFeb2005Constants.AttributeNames.OK;
            private string type = WSTrustFeb2005Constants.AttributeNames.Type;
            private string valueType = WSTrustFeb2005Constants.AttributeNames.ValueType;

            internal string Allow
            {
                get { return this.allow; }
            }

            internal string Context
            {
                get { return this.context; }
            }

            internal string Dialect
            {
                get { return this.dialect; }
            }

            internal string EncodingType
            {
                get { return this.encodingType; }
            }

            internal string OK
            {
                get { return this.oK; }
            }

            internal string Type
            {
                get { return this.type; }
            }

            internal string ValueType
            {
                get { return this.valueType; }
            }
        }

        internal abstract class WSTrustComputedKeyAlgorithm
        {
            internal string Psha1
            {
                get;

                set;
            }
        }

        internal class WSTrustElementNames
        {
            private string allowPostdating = WSTrustFeb2005Constants.ElementNames.AllowPostdating;
            private string authenticationType = WSTrustFeb2005Constants.ElementNames.AuthenticationType;
            private string binarySecret = WSTrustFeb2005Constants.ElementNames.BinarySecret;
            private string binaryExchange = WSTrustFeb2005Constants.ElementNames.BinaryExchange;
            private string cancelTarget = WSTrustFeb2005Constants.ElementNames.CancelTarget;
            private string claims = WSTrustFeb2005Constants.ElementNames.Claims;
            private string computedKey = WSTrustFeb2005Constants.ElementNames.ComputedKey;
            private string computedKeyAlgorithm = WSTrustFeb2005Constants.ElementNames.ComputedKeyAlgorithm;
            private string canonicalizationAlgorithm = WSTrustFeb2005Constants.ElementNames.CanonicalizationAlgorithm;
            private string code = WSTrustFeb2005Constants.ElementNames.Code;
            private string delegatable = WSTrustFeb2005Constants.ElementNames.Delegatable;
            private string delegateTo = WSTrustFeb2005Constants.ElementNames.DelegateTo;
            private string encryption = WSTrustFeb2005Constants.ElementNames.Encryption;
            private string encryptionAlgorithm = WSTrustFeb2005Constants.ElementNames.EncryptionAlgorithm;
            private string encryptWith = WSTrustFeb2005Constants.ElementNames.EncryptWith;
            private string entropy = WSTrustFeb2005Constants.ElementNames.Entropy;
            private string forwardable = WSTrustFeb2005Constants.ElementNames.Forwardable;
            private string issuer = WSTrustFeb2005Constants.ElementNames.Issuer;
            private string keySize = WSTrustFeb2005Constants.ElementNames.KeySize;
            private string keyType = WSTrustFeb2005Constants.ElementNames.KeyType;
            private string lifetime = WSTrustFeb2005Constants.ElementNames.Lifetime;
            private string onBehalfOf = WSTrustFeb2005Constants.ElementNames.OnBehalfOf;
            private string participant = WSTrustFeb2005Constants.ElementNames.Participant;
            private string participants = WSTrustFeb2005Constants.ElementNames.Participants;
            private string primary = WSTrustFeb2005Constants.ElementNames.Primary;
            private string proofEncryption = WSTrustFeb2005Constants.ElementNames.ProofEncryption;
            private string reason = WSTrustFeb2005Constants.ElementNames.Reason;
            private string renewing = WSTrustFeb2005Constants.ElementNames.Renewing;
            private string renewTarget = WSTrustFeb2005Constants.ElementNames.RenewTarget;
            private string requestedAttachedReference = WSTrustFeb2005Constants.ElementNames.RequestedAttachedReference;
            private string requestedProofToken = WSTrustFeb2005Constants.ElementNames.RequestedProofToken;
            private string requestedSecurityToken = WSTrustFeb2005Constants.ElementNames.RequestedSecurityToken;
            private string requestedTokenCancelled = WSTrustFeb2005Constants.ElementNames.RequestedTokenCancelled;
            private string requestedUnattachedReference = WSTrustFeb2005Constants.ElementNames.RequestedUnattachedReference;
            private string requestKeySize = WSTrustFeb2005Constants.ElementNames.RequestKeySize;
            private string requestSecurityToken = WSTrustFeb2005Constants.ElementNames.RequestSecurityToken;
            private string requestSecurityTokenResponse = WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse;
            private string requestType = WSTrustFeb2005Constants.ElementNames.RequestType;
            private string securityContextToken = WSTrustFeb2005Constants.ElementNames.SecurityContextToken;
            private string signWith = WSTrustFeb2005Constants.ElementNames.SignWith;
            private string signatureAlgorithm = WSTrustFeb2005Constants.ElementNames.SignatureAlgorithm;
            private string status = WSTrustFeb2005Constants.ElementNames.Status;
            private string tokenType = WSTrustFeb2005Constants.ElementNames.TokenType;
            private string useKey = WSTrustFeb2005Constants.ElementNames.UseKey;

            internal string AllowPostdating
            {
                get { return this.allowPostdating; }
            }

            internal string AuthenticationType
            {
                get { return this.authenticationType; }
            }

            internal string BinarySecret
            {
                get { return this.binarySecret; }
            }

            internal string BinaryExchange
            {
                get { return this.binaryExchange; }
            }

            internal string CancelTarget
            {
                get { return this.cancelTarget; }
            }

            internal string Claims
            {
                get { return this.claims; }
            }

            internal string ComputedKey
            {
                get { return this.computedKey; }
            }

            internal string ComputedKeyAlgorithm
            {
                get { return this.computedKeyAlgorithm; }
            }

            internal string CanonicalizationAlgorithm
            {
                get { return this.canonicalizationAlgorithm; }
            }

            internal string Code
            {
                get { return this.code; }
            }

            internal string Delegatable
            {
                get { return this.delegatable; }
            }

            internal string DelegateTo
            {
                get { return this.delegateTo; }
            }

            internal string Encryption
            {
                get { return this.encryption; }
            }

            internal string EncryptionAlgorithm
            {
                get { return this.encryptionAlgorithm; }
            }

            internal string EncryptWith
            {
                get { return this.encryptWith; }
            }

            internal string Entropy
            {
                get { return this.entropy; }
            }

            internal string Forwardable
            {
                get { return this.forwardable; }
            }

            internal string Issuer
            {
                get { return this.issuer; }
            }

            internal string KeySize
            {
                get { return this.keySize; }
            }

            internal string KeyType
            {
                get { return this.keyType; }
            }

            internal string Lifetime
            {
                get { return this.lifetime; }
            }

            internal string OnBehalfOf
            {
                get { return this.onBehalfOf; }
            }

            internal string Participant
            {
                get { return this.participant; }
            }

            internal string Participants
            {
                get { return this.participants; }
            }

            internal string Primary
            {
                get { return this.primary; }
            }

            internal string ProofEncryption
            {
                get { return this.proofEncryption; }
            }

            internal string Reason
            {
                get { return this.reason; }
            }

            internal string Renewing
            {
                get { return this.renewing; }
            }

            internal string RenewTarget
            {
                get { return this.renewTarget; }
            }

            internal string RequestedAttachedReference
            {
                get { return this.requestedAttachedReference; }
            }

            internal string RequestedProofToken
            {
                get { return this.requestedProofToken; }
            }

            internal string RequestedSecurityToken
            {
                get { return this.requestedSecurityToken; }
            }

            internal string RequestedTokenCancelled
            {
                get { return this.requestedTokenCancelled; }
            }

            internal string RequestedUnattachedReference
            {
                get { return this.requestedUnattachedReference; }
            }

            internal string RequestKeySize
            {
                get { return this.requestKeySize; }
            }

            internal string RequestSecurityToken
            {
                get { return this.requestSecurityToken; }
            }

            internal string RequestSecurityTokenResponse
            {
                get { return this.requestSecurityTokenResponse; }
            }

            internal string RequestType
            {
                get { return this.requestType; }
            }

            internal string SecurityContextToken
            {
                get { return this.securityContextToken; }
            }

            internal string SignWith
            {
                get { return this.signWith; }
            }

            internal string SignatureAlgorithm
            {
                get { return this.signatureAlgorithm; }
            }

            internal string Status
            {
                get { return this.status; }
            }

            internal string TokenType
            {
                get { return this.tokenType; }
            }

            internal string UseKey
            {
                get { return this.useKey; }
            }
        }

        internal abstract class WSTrustRequestTypes
        {
            internal string Cancel
            {
                get;
                
                set;
            }

            internal string Issue
            {
                get;

                set;
            }

            internal string Renew
            {
                get;

                set;
            }

            internal string Validate
            {
                get;

                set;
            }
        }

        internal abstract class WSTrustKeyTypes
        {
            internal string Asymmetric
            {
                get;

                set;
            }

            internal string Bearer
            {
                get;

                set;
            }

            internal string Symmetric
            {
                get;

                set;
            }
        }

        internal class FaultCodeValues
        {
            internal string AuthenticationBadElements
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.AuthenticationBadElements; }
            }

            internal string BadRequest
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.BadRequest; }
            }

            internal string ExpiredData
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.ExpiredData; }
            }

            internal string FailedAuthentication
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.FailedAuthentication; }
            }

            internal string InvalidRequest
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.InvalidRequest; }
            }

            internal string InvalidScope
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.InvalidScope; }
            }

            internal string InvalidSecurityToken
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.InvalidSecurityToken; }
            }

            internal string InvalidTimeRange
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.InvalidTimeRange; }
            }

            internal string RenewNeeded
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.RenewNeeded; }
            }

            internal string RequestFailed
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.RequestFailed; }
            }

            internal string UnableToRenew
            {
                get { return WSTrustFeb2005Constants.FaultCodeValues.UnableToRenew; }
            }
        }
    }
}
