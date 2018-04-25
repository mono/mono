//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Xml;
    using System.Collections.Generic;

    // NOTE: This is a dynamic dictionary of XmlDictionaryStrings for the Binary Encoder to dynamically encode should
    // the string not exist in the static cache.
    // When adding or removing memebers please keep the capacity of the XmlDictionary field current.
    static class DXD
    {
        static AtomicTransactionExternal11Dictionary atomicTransactionExternal11Dictionary;
        static CoordinationExternal11Dictionary coordinationExternal11Dictionary;
        static SecureConversationDec2005Dictionary secureConversationDec2005Dictionary;
        static SecurityAlgorithmDec2005Dictionary securityAlgorithmDec2005Dictionary;
        static TrustDec2005Dictionary trustDec2005Dictionary;
        static Wsrm11Dictionary wsrm11Dictionary;

        static DXD()
        {
            // Each string added to the XmlDictionary will keep a reference to the XmlDictionary so this class does
            // not need to keep a reference.
            XmlDictionary dictionary = new XmlDictionary(137);

            // Each dictionaries' constructor should add strings to the XmlDictionary.
            atomicTransactionExternal11Dictionary = new AtomicTransactionExternal11Dictionary(dictionary);
            coordinationExternal11Dictionary = new CoordinationExternal11Dictionary(dictionary);
            secureConversationDec2005Dictionary = new SecureConversationDec2005Dictionary(dictionary);
            secureConversationDec2005Dictionary.PopulateSecureConversationDec2005();
            securityAlgorithmDec2005Dictionary = new SecurityAlgorithmDec2005Dictionary(dictionary);
            securityAlgorithmDec2005Dictionary.PopulateSecurityAlgorithmDictionaryString();
            trustDec2005Dictionary = new TrustDec2005Dictionary(dictionary);
            trustDec2005Dictionary.PopulateDec2005DictionaryStrings();
            trustDec2005Dictionary.PopulateFeb2005DictionaryString();
            wsrm11Dictionary = new Wsrm11Dictionary(dictionary);
        }

        static public AtomicTransactionExternal11Dictionary AtomicTransactionExternal11Dictionary
        {
            get { return atomicTransactionExternal11Dictionary; }
        }

        static public CoordinationExternal11Dictionary CoordinationExternal11Dictionary
        {
            get { return coordinationExternal11Dictionary; }
        }

        static public SecureConversationDec2005Dictionary SecureConversationDec2005Dictionary
        {
            get { return secureConversationDec2005Dictionary; }
        }

        static public SecurityAlgorithmDec2005Dictionary SecurityAlgorithmDec2005Dictionary
        {
            get { return securityAlgorithmDec2005Dictionary; }
        }

        static public TrustDec2005Dictionary TrustDec2005Dictionary
        {
            get { return trustDec2005Dictionary; }
        }

        static public Wsrm11Dictionary Wsrm11Dictionary
        {
            get { return wsrm11Dictionary; }
        }
    }

    class AtomicTransactionExternal11Dictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString CompletionUri;
        public XmlDictionaryString Durable2PCUri;
        public XmlDictionaryString Volatile2PCUri;
        public XmlDictionaryString CommitAction;
        public XmlDictionaryString RollbackAction;
        public XmlDictionaryString CommittedAction;
        public XmlDictionaryString AbortedAction;
        public XmlDictionaryString PrepareAction;
        public XmlDictionaryString PreparedAction;
        public XmlDictionaryString ReadOnlyAction;
        public XmlDictionaryString ReplayAction;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString UnknownTransaction;

        public AtomicTransactionExternal11Dictionary(XmlDictionary dictionary)
        {
            this.Namespace = dictionary.Add(AtomicTransactionExternal11Strings.Namespace);
            this.CompletionUri = dictionary.Add(AtomicTransactionExternal11Strings.CompletionUri);
            this.Durable2PCUri = dictionary.Add(AtomicTransactionExternal11Strings.Durable2PCUri);
            this.Volatile2PCUri = dictionary.Add(AtomicTransactionExternal11Strings.Volatile2PCUri);
            this.CommitAction = dictionary.Add(AtomicTransactionExternal11Strings.CommitAction);
            this.RollbackAction = dictionary.Add(AtomicTransactionExternal11Strings.RollbackAction);
            this.CommittedAction = dictionary.Add(AtomicTransactionExternal11Strings.CommittedAction);
            this.AbortedAction = dictionary.Add(AtomicTransactionExternal11Strings.AbortedAction);
            this.PrepareAction = dictionary.Add(AtomicTransactionExternal11Strings.PrepareAction);
            this.PreparedAction = dictionary.Add(AtomicTransactionExternal11Strings.PreparedAction);
            this.ReadOnlyAction = dictionary.Add(AtomicTransactionExternal11Strings.ReadOnlyAction);
            this.ReplayAction = dictionary.Add(AtomicTransactionExternal11Strings.ReplayAction);
            this.FaultAction = dictionary.Add(AtomicTransactionExternal11Strings.FaultAction);
            this.UnknownTransaction = dictionary.Add(AtomicTransactionExternal11Strings.UnknownTransaction);
        }
    }

    class CoordinationExternal11Dictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString CreateCoordinationContextAction;
        public XmlDictionaryString CreateCoordinationContextResponseAction;
        public XmlDictionaryString RegisterAction;
        public XmlDictionaryString RegisterResponseAction;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString CannotCreateContext;
        public XmlDictionaryString CannotRegisterParticipant;

        public CoordinationExternal11Dictionary(XmlDictionary dictionary)
        {
            this.Namespace = dictionary.Add(CoordinationExternal11Strings.Namespace);
            this.CreateCoordinationContextAction = dictionary.Add(CoordinationExternal11Strings.CreateCoordinationContextAction);
            this.CreateCoordinationContextResponseAction = dictionary.Add(CoordinationExternal11Strings.CreateCoordinationContextResponseAction);
            this.RegisterAction = dictionary.Add(CoordinationExternal11Strings.RegisterAction);
            this.RegisterResponseAction = dictionary.Add(CoordinationExternal11Strings.RegisterResponseAction);
            this.FaultAction = dictionary.Add(CoordinationExternal11Strings.FaultAction);
            this.CannotCreateContext = dictionary.Add(CoordinationExternal11Strings.CannotCreateContext);
            this.CannotRegisterParticipant = dictionary.Add(CoordinationExternal11Strings.CannotRegisterParticipant);
        }
    }

    class SecureConversationDec2005Dictionary : SecureConversationDictionary
    {
        public XmlDictionaryString RequestSecurityContextRenew;
        public XmlDictionaryString RequestSecurityContextRenewResponse;
        public XmlDictionaryString RequestSecurityContextClose;
        public XmlDictionaryString RequestSecurityContextCloseResponse;
        public XmlDictionaryString Instance;

        public List<XmlDictionaryString> SecureConversationDictionaryStrings = new List<XmlDictionaryString>();  

        public SecureConversationDec2005Dictionary(XmlDictionary dictionary)
        {
            this.SecurityContextToken = dictionary.Add(SecureConversationDec2005Strings.SecurityContextToken);
            this.AlgorithmAttribute = dictionary.Add(SecureConversationDec2005Strings.AlgorithmAttribute);
            this.Generation = dictionary.Add(SecureConversationDec2005Strings.Generation);
            this.Label = dictionary.Add(SecureConversationDec2005Strings.Label);
            this.Offset = dictionary.Add(SecureConversationDec2005Strings.Offset);
            this.Properties = dictionary.Add(SecureConversationDec2005Strings.Properties);
            this.Identifier = dictionary.Add(SecureConversationDec2005Strings.Identifier);
            this.Cookie = dictionary.Add(SecureConversationDec2005Strings.Cookie);
            this.RenewNeededFaultCode = dictionary.Add(SecureConversationDec2005Strings.RenewNeededFaultCode);
            this.BadContextTokenFaultCode = dictionary.Add(SecureConversationDec2005Strings.BadContextTokenFaultCode);
            this.Prefix = dictionary.Add(SecureConversationDec2005Strings.Prefix);
            this.DerivedKeyTokenType = dictionary.Add(SecureConversationDec2005Strings.DerivedKeyTokenType);
            this.SecurityContextTokenType = dictionary.Add(SecureConversationDec2005Strings.SecurityContextTokenType);
            this.SecurityContextTokenReferenceValueType = dictionary.Add(SecureConversationDec2005Strings.SecurityContextTokenReferenceValueType);
            this.RequestSecurityContextIssuance = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextIssuance);
            this.RequestSecurityContextIssuanceResponse = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextIssuanceResponse);
            this.RequestSecurityContextRenew = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextRenew);
            this.RequestSecurityContextRenewResponse = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextRenewResponse);
            this.RequestSecurityContextClose = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextClose);
            this.RequestSecurityContextCloseResponse = dictionary.Add(SecureConversationDec2005Strings.RequestSecurityContextCloseResponse);
            this.Namespace = dictionary.Add(SecureConversationDec2005Strings.Namespace);
            this.DerivedKeyToken = dictionary.Add(SecureConversationDec2005Strings.DerivedKeyToken);
            this.Nonce = dictionary.Add(SecureConversationDec2005Strings.Nonce);
            this.Length = dictionary.Add(SecureConversationDec2005Strings.Length);
            this.Instance = dictionary.Add(SecureConversationDec2005Strings.Instance);
        }

        public void PopulateSecureConversationDec2005()
        {
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.SecurityContextToken);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.AlgorithmAttribute);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Generation);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Label);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Offset);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Properties);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Identifier);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Cookie);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RenewNeededFaultCode);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.BadContextTokenFaultCode);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Prefix);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.DerivedKeyTokenType);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.SecurityContextTokenType);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.SecurityContextTokenReferenceValueType);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextIssuance);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextIssuanceResponse);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextRenew);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextRenewResponse);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextClose);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.RequestSecurityContextCloseResponse);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Namespace);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.DerivedKeyToken);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Nonce);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Length);
            SecureConversationDictionaryStrings.Add(DXD.SecureConversationDec2005Dictionary.Instance);
        }

    }

    class SecurityAlgorithmDec2005Dictionary
    {
        public XmlDictionaryString Psha1KeyDerivationDec2005;

        public List<XmlDictionaryString> SecurityAlgorithmDictionaryStrings = new List<XmlDictionaryString>();

        public SecurityAlgorithmDec2005Dictionary(XmlDictionary dictionary)
        {
            this.Psha1KeyDerivationDec2005 = dictionary.Add(SecurityAlgorithmDec2005Strings.Psha1KeyDerivationDec2005);
        }

        public void PopulateSecurityAlgorithmDictionaryString()
        {
            SecurityAlgorithmDictionaryStrings.Add(DXD.SecurityAlgorithmDec2005Dictionary.Psha1KeyDerivationDec2005);
        }
    }

    class TrustDec2005Dictionary : TrustDictionary
    {
        public XmlDictionaryString AsymmetricKeyBinarySecret;
        public XmlDictionaryString RequestSecurityTokenCollectionIssuanceFinalResponse;
        public XmlDictionaryString RequestSecurityTokenRenewal;
        public XmlDictionaryString RequestSecurityTokenRenewalResponse;
        public XmlDictionaryString RequestSecurityTokenCollectionRenewalFinalResponse;
        public XmlDictionaryString RequestSecurityTokenCancellation;
        public XmlDictionaryString RequestSecurityTokenCancellationResponse;
        public XmlDictionaryString RequestSecurityTokenCollectionCancellationFinalResponse;
        public XmlDictionaryString KeyWrapAlgorithm;
        public XmlDictionaryString BearerKeyType;
        public XmlDictionaryString SecondaryParameters;
        public XmlDictionaryString Dialect;
        public XmlDictionaryString DialectType;

        public List<XmlDictionaryString> Feb2005DictionaryStrings = new List<XmlDictionaryString>();
        public List<XmlDictionaryString> Dec2005DictionaryString = new List<XmlDictionaryString>();

        public TrustDec2005Dictionary(XmlDictionary dictionary)
        {
            this.CombinedHashLabel = dictionary.Add(TrustDec2005Strings.CombinedHashLabel);
            this.RequestSecurityTokenResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenResponse);
            this.TokenType = dictionary.Add(TrustDec2005Strings.TokenType);
            this.KeySize = dictionary.Add(TrustDec2005Strings.KeySize);
            this.RequestedTokenReference = dictionary.Add(TrustDec2005Strings.RequestedTokenReference);
            this.AppliesTo = dictionary.Add(TrustDec2005Strings.AppliesTo);
            this.Authenticator = dictionary.Add(TrustDec2005Strings.Authenticator);
            this.CombinedHash = dictionary.Add(TrustDec2005Strings.CombinedHash);
            this.BinaryExchange = dictionary.Add(TrustDec2005Strings.BinaryExchange);
            this.Lifetime = dictionary.Add(TrustDec2005Strings.Lifetime);
            this.RequestedSecurityToken = dictionary.Add(TrustDec2005Strings.RequestedSecurityToken);
            this.Entropy = dictionary.Add(TrustDec2005Strings.Entropy);
            this.RequestedProofToken = dictionary.Add(TrustDec2005Strings.RequestedProofToken);
            this.ComputedKey = dictionary.Add(TrustDec2005Strings.ComputedKey);
            this.RequestSecurityToken = dictionary.Add(TrustDec2005Strings.RequestSecurityToken);
            this.RequestType = dictionary.Add(TrustDec2005Strings.RequestType);
            this.Context = dictionary.Add(TrustDec2005Strings.Context);
            this.BinarySecret = dictionary.Add(TrustDec2005Strings.BinarySecret);
            this.Type = dictionary.Add(TrustDec2005Strings.Type);
            this.SpnegoValueTypeUri = dictionary.Add(TrustDec2005Strings.SpnegoValueTypeUri);
            this.TlsnegoValueTypeUri = dictionary.Add(TrustDec2005Strings.TlsnegoValueTypeUri);
            this.Prefix = dictionary.Add(TrustDec2005Strings.Prefix);
            this.RequestSecurityTokenIssuance = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenIssuance);
            this.RequestSecurityTokenIssuanceResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenIssuanceResponse);
            this.RequestTypeIssue = dictionary.Add(TrustDec2005Strings.RequestTypeIssue);
            this.AsymmetricKeyBinarySecret = dictionary.Add(TrustDec2005Strings.AsymmetricKeyBinarySecret);
            this.SymmetricKeyBinarySecret = dictionary.Add(TrustDec2005Strings.SymmetricKeyBinarySecret);
            this.NonceBinarySecret = dictionary.Add(TrustDec2005Strings.NonceBinarySecret);
            this.Psha1ComputedKeyUri = dictionary.Add(TrustDec2005Strings.Psha1ComputedKeyUri);
            this.KeyType = dictionary.Add(TrustDec2005Strings.KeyType);
            this.SymmetricKeyType = dictionary.Add(TrustDec2005Strings.SymmetricKeyType);
            this.PublicKeyType = dictionary.Add(TrustDec2005Strings.PublicKeyType);
            this.Claims = dictionary.Add(TrustDec2005Strings.Claims);
            this.InvalidRequestFaultCode = dictionary.Add(TrustDec2005Strings.InvalidRequestFaultCode);
            this.FailedAuthenticationFaultCode = dictionary.Add(TrustDec2005Strings.FailedAuthenticationFaultCode);
            this.UseKey = dictionary.Add(TrustDec2005Strings.UseKey);
            this.SignWith = dictionary.Add(TrustDec2005Strings.SignWith);
            this.EncryptWith = dictionary.Add(TrustDec2005Strings.EncryptWith);
            this.EncryptionAlgorithm = dictionary.Add(TrustDec2005Strings.EncryptionAlgorithm);
            this.CanonicalizationAlgorithm = dictionary.Add(TrustDec2005Strings.CanonicalizationAlgorithm);
            this.ComputedKeyAlgorithm = dictionary.Add(TrustDec2005Strings.ComputedKeyAlgorithm);
            this.RequestSecurityTokenResponseCollection = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenResponseCollection);
            this.Namespace = dictionary.Add(TrustDec2005Strings.Namespace);
            this.BinarySecretClauseType = dictionary.Add(TrustDec2005Strings.BinarySecretClauseType);
            this.RequestSecurityTokenCollectionIssuanceFinalResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenCollectionIssuanceFinalResponse);
            this.RequestSecurityTokenRenewal = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenRenewal);
            this.RequestSecurityTokenRenewalResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenRenewalResponse);
            this.RequestSecurityTokenCollectionRenewalFinalResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenCollectionRenewalFinalResponse);
            this.RequestSecurityTokenCancellation = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenCancellation);
            this.RequestSecurityTokenCancellationResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenCancellationResponse);
            this.RequestSecurityTokenCollectionCancellationFinalResponse = dictionary.Add(TrustDec2005Strings.RequestSecurityTokenCollectionCancellationFinalResponse);
            this.RequestTypeRenew = dictionary.Add(TrustDec2005Strings.RequestTypeRenew);
            this.RequestTypeClose = dictionary.Add(TrustDec2005Strings.RequestTypeClose);
            this.RenewTarget = dictionary.Add(TrustDec2005Strings.RenewTarget);
            this.CloseTarget = dictionary.Add(TrustDec2005Strings.CloseTarget);
            this.RequestedTokenClosed = dictionary.Add(TrustDec2005Strings.RequestedTokenClosed);
            this.RequestedAttachedReference = dictionary.Add(TrustDec2005Strings.RequestedAttachedReference);
            this.RequestedUnattachedReference = dictionary.Add(TrustDec2005Strings.RequestedUnattachedReference);
            this.IssuedTokensHeader = dictionary.Add(TrustDec2005Strings.IssuedTokensHeader);
            this.KeyWrapAlgorithm = dictionary.Add(TrustDec2005Strings.KeyWrapAlgorithm);
            this.BearerKeyType = dictionary.Add(TrustDec2005Strings.BearerKeyType);
            this.SecondaryParameters = dictionary.Add(TrustDec2005Strings.SecondaryParameters);
            this.Dialect = dictionary.Add(TrustDec2005Strings.Dialect);
            this.DialectType = dictionary.Add(TrustDec2005Strings.DialectType);
        }

        public void PopulateFeb2005DictionaryString()
        {
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestSecurityTokenResponseCollection);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Namespace);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.BinarySecretClauseType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.CombinedHashLabel);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestSecurityTokenResponse);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.TokenType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.KeySize);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedTokenReference);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.AppliesTo);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Authenticator);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.CombinedHash);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.BinaryExchange);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Lifetime);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedSecurityToken);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Entropy);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedProofToken);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.ComputedKey);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestSecurityToken);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Context);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.BinarySecret);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Type);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.SpnegoValueTypeUri);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.TlsnegoValueTypeUri);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Prefix);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestSecurityTokenIssuance);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestSecurityTokenIssuanceResponse);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestTypeIssue);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.SymmetricKeyBinarySecret);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Psha1ComputedKeyUri);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.NonceBinarySecret);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RenewTarget);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.CloseTarget);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedTokenClosed);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedAttachedReference);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestedUnattachedReference);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.IssuedTokensHeader);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestTypeRenew);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.RequestTypeClose);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.KeyType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.SymmetricKeyType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.PublicKeyType);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.Claims);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.InvalidRequestFaultCode);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.FailedAuthenticationFaultCode);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.UseKey);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.SignWith);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.EncryptWith);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.EncryptionAlgorithm);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.CanonicalizationAlgorithm);
            Feb2005DictionaryStrings.Add(XD.TrustFeb2005Dictionary.ComputedKeyAlgorithm);
        }

        public void PopulateDec2005DictionaryStrings()
        {
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.CombinedHashLabel);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.TokenType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.KeySize);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedTokenReference);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.AppliesTo);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Authenticator);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.CombinedHash);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.BinaryExchange);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Lifetime);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedSecurityToken);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Entropy);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedProofToken);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.ComputedKey);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityToken);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Context);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.BinarySecret);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Type);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.SpnegoValueTypeUri);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.TlsnegoValueTypeUri);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Prefix);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenIssuance);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenIssuanceResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestTypeIssue);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.AsymmetricKeyBinarySecret);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.SymmetricKeyBinarySecret);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.NonceBinarySecret);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Psha1ComputedKeyUri);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.KeyType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.SymmetricKeyType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.PublicKeyType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Claims);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.InvalidRequestFaultCode);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.FailedAuthenticationFaultCode);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.UseKey);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.SignWith);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.EncryptWith);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.EncryptionAlgorithm);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.CanonicalizationAlgorithm);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.ComputedKeyAlgorithm);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenResponseCollection);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Namespace);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.BinarySecretClauseType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenCollectionIssuanceFinalResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenRenewal);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenRenewalResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenCollectionRenewalFinalResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenCancellation);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenCancellationResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestSecurityTokenCollectionCancellationFinalResponse);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestTypeRenew);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestTypeClose);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RenewTarget);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.CloseTarget);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedTokenClosed);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedAttachedReference);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.RequestedUnattachedReference);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.IssuedTokensHeader);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.KeyWrapAlgorithm);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.BearerKeyType);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.SecondaryParameters);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.Dialect);
            Dec2005DictionaryString.Add(DXD.TrustDec2005Dictionary.DialectType);
        }
    }

    class Wsrm11Dictionary
    {
        public XmlDictionaryString AckRequestedAction;
        public XmlDictionaryString CloseSequence;
        public XmlDictionaryString CloseSequenceAction;
        public XmlDictionaryString CloseSequenceResponse;
        public XmlDictionaryString CloseSequenceResponseAction;
        public XmlDictionaryString CreateSequenceAction;
        public XmlDictionaryString CreateSequenceResponseAction;
        public XmlDictionaryString DiscardFollowingFirstGap;
        public XmlDictionaryString Endpoint;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Final;
        public XmlDictionaryString IncompleteSequenceBehavior;
        public XmlDictionaryString LastMsgNumber;
        public XmlDictionaryString MaxMessageNumber;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NoDiscard;
        public XmlDictionaryString None;
        public XmlDictionaryString SequenceAcknowledgementAction;
        public XmlDictionaryString SequenceClosed;
        public XmlDictionaryString TerminateSequenceAction;
        public XmlDictionaryString TerminateSequenceResponse;
        public XmlDictionaryString TerminateSequenceResponseAction;
        public XmlDictionaryString UsesSequenceSSL;
        public XmlDictionaryString UsesSequenceSTR;
        public XmlDictionaryString WsrmRequired;

        public Wsrm11Dictionary(XmlDictionary dictionary)
        {
            this.AckRequestedAction = dictionary.Add(Wsrm11Strings.AckRequestedAction);
            this.CloseSequence = dictionary.Add(Wsrm11Strings.CloseSequence);
            this.CloseSequenceAction = dictionary.Add(Wsrm11Strings.CloseSequenceAction);
            this.CloseSequenceResponse = dictionary.Add(Wsrm11Strings.CloseSequenceResponse);
            this.CloseSequenceResponseAction = dictionary.Add(Wsrm11Strings.CloseSequenceResponseAction);
            this.CreateSequenceAction = dictionary.Add(Wsrm11Strings.CreateSequenceAction);
            this.CreateSequenceResponseAction = dictionary.Add(Wsrm11Strings.CreateSequenceResponseAction);
            this.DiscardFollowingFirstGap = dictionary.Add(Wsrm11Strings.DiscardFollowingFirstGap);
            this.Endpoint = dictionary.Add(Wsrm11Strings.Endpoint);
            this.FaultAction = dictionary.Add(Wsrm11Strings.FaultAction);
            this.Final = dictionary.Add(Wsrm11Strings.Final);
            this.IncompleteSequenceBehavior = dictionary.Add(Wsrm11Strings.IncompleteSequenceBehavior);
            this.LastMsgNumber = dictionary.Add(Wsrm11Strings.LastMsgNumber);
            this.MaxMessageNumber = dictionary.Add(Wsrm11Strings.MaxMessageNumber);
            this.Namespace = dictionary.Add(Wsrm11Strings.Namespace);
            this.NoDiscard = dictionary.Add(Wsrm11Strings.NoDiscard);
            this.None = dictionary.Add(Wsrm11Strings.None);
            this.SequenceAcknowledgementAction = dictionary.Add(Wsrm11Strings.SequenceAcknowledgementAction);
            this.SequenceClosed = dictionary.Add(Wsrm11Strings.SequenceClosed);
            this.TerminateSequenceAction = dictionary.Add(Wsrm11Strings.TerminateSequenceAction);
            this.TerminateSequenceResponse = dictionary.Add(Wsrm11Strings.TerminateSequenceResponse);
            this.TerminateSequenceResponseAction = dictionary.Add(Wsrm11Strings.TerminateSequenceResponseAction);
            this.UsesSequenceSSL = dictionary.Add(Wsrm11Strings.UsesSequenceSSL);
            this.UsesSequenceSTR = dictionary.Add(Wsrm11Strings.UsesSequenceSTR);
            this.WsrmRequired = dictionary.Add(Wsrm11Strings.WsrmRequired);
        }
    }

    static class AtomicTransactionExternal11Strings
    {
        // dictionary strings
        public const string Namespace = "http://docs.oasis-open.org/ws-tx/wsat/2006/06";
        public const string CompletionUri = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Completion";
        public const string Durable2PCUri = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Durable2PC";
        public const string Volatile2PCUri = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Volatile2PC";
        public const string CommitAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Commit";
        public const string RollbackAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Rollback";
        public const string CommittedAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Committed";
        public const string AbortedAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Aborted";
        public const string PrepareAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepare";
        public const string PreparedAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepared";
        public const string ReadOnlyAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/ReadOnly";
        public const string ReplayAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Replay";
        public const string FaultAction = "http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault";
        public const string UnknownTransaction = "UnknownTransaction";
    }

    static class CoordinationExternal11Strings
    {
        // dictionary strings
        public const string Namespace = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06";
        public const string CreateCoordinationContextAction = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContext";
        public const string CreateCoordinationContextResponseAction = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContextResponse";
        public const string RegisterAction = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/Register";
        public const string RegisterResponseAction = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/RegisterResponse";
        public const string FaultAction = "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault";
        public const string CannotCreateContext = "CannotCreateContext";
        public const string CannotRegisterParticipant = "CannotRegisterParticipant";
    }

    static class SecureConversationDec2005Strings
    {
        // dictionary strings
        public const string SecurityContextToken = "SecurityContextToken";
        public const string AlgorithmAttribute = "Algorithm";
        public const string Generation = "Generation";
        public const string Label = "Label";
        public const string Offset = "Offset";
        public const string Properties = "Properties";
        public const string Identifier = "Identifier";
        public const string Cookie = "Cookie";
        public const string RenewNeededFaultCode = "RenewNeeded";
        public const string BadContextTokenFaultCode = "BadContextToken";
        public const string Prefix = "sc";
        public const string DerivedKeyTokenType = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk";
        public const string SecurityContextTokenType = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct";
        public const string SecurityContextTokenReferenceValueType = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct";
        public const string RequestSecurityContextIssuance = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT";
        public const string RequestSecurityContextIssuanceResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT";
        public const string RequestSecurityContextRenew = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT/Renew";
        public const string RequestSecurityContextRenewResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT/Renew";
        public const string RequestSecurityContextClose = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/SCT/Cancel";
        public const string RequestSecurityContextCloseResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/SCT/Cancel";
        public const string Namespace = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512";
        public const string DerivedKeyToken = "DerivedKeyToken";
        public const string Nonce = "Nonce";
        public const string Length = "Length";
        public const string Instance = "Instance";
    }

    static class SecurityAlgorithmDec2005Strings
    {
        // dictionary strings
        public const string Psha1KeyDerivationDec2005 = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1";
    }

    static class TrustDec2005Strings
    {
        // dictionary strings
        public const string CombinedHashLabel = "AUTH-HASH";
        public const string RequestSecurityTokenResponse = "RequestSecurityTokenResponse";
        public const string TokenType = "TokenType";
        public const string KeySize = "KeySize";
        public const string RequestedTokenReference = "RequestedTokenReference";
        public const string AppliesTo = "AppliesTo";
        public const string Authenticator = "Authenticator";
        public const string CombinedHash = "CombinedHash";
        public const string BinaryExchange = "BinaryExchange";
        public const string Lifetime = "Lifetime";
        public const string RequestedSecurityToken = "RequestedSecurityToken";
        public const string Entropy = "Entropy";
        public const string RequestedProofToken = "RequestedProofToken";
        public const string ComputedKey = "ComputedKey";
        public const string RequestSecurityToken = "RequestSecurityToken";
        public const string RequestType = "RequestType";
        public const string Context = "Context";
        public const string BinarySecret = "BinarySecret";
        public const string Type = "Type";
        public const string SpnegoValueTypeUri = "http://schemas.xmlsoap.org/ws/2005/02/trust/spnego";
        public const string TlsnegoValueTypeUri = "http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego";
        public const string Prefix = "trust";
        public const string RequestSecurityTokenIssuance = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue";
        public const string RequestSecurityTokenIssuanceResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Issue";
        public const string RequestTypeIssue = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue";
        public const string AsymmetricKeyBinarySecret = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/AsymmetricKey";
        public const string SymmetricKeyBinarySecret = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey";
        public const string NonceBinarySecret = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Nonce";
        public const string Psha1ComputedKeyUri = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/CK/PSHA1";
        public const string KeyType = "KeyType";
        public const string SymmetricKeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey";
        public const string PublicKeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey";
        public const string Claims = "Claims";
        public const string InvalidRequestFaultCode = "InvalidRequest";
        public const string FailedAuthenticationFaultCode = "FailedAuthentication";
        public const string UseKey = "UseKey";
        public const string SignWith = "SignWith";
        public const string EncryptWith = "EncryptWith";
        public const string EncryptionAlgorithm = "EncryptionAlgorithm";
        public const string CanonicalizationAlgorithm = "CanonicalizationAlgorithm";
        public const string ComputedKeyAlgorithm = "ComputedKeyAlgorithm";
        public const string RequestSecurityTokenResponseCollection = "RequestSecurityTokenResponseCollection";
        public const string Namespace = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
        public const string BinarySecretClauseType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512#BinarySecret";
        public const string RequestSecurityTokenCollectionIssuanceFinalResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTRC/IssueFinal";
        public const string RequestSecurityTokenRenewal = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Renew";
        public const string RequestSecurityTokenRenewalResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Renew";
        public const string RequestSecurityTokenCollectionRenewalFinalResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/RenewFinal";
        public const string RequestSecurityTokenCancellation = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Cancel";
        public const string RequestSecurityTokenCancellationResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Cancel";
        public const string RequestSecurityTokenCollectionCancellationFinalResponse = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/CancelFinal";
        public const string RequestTypeRenew = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Renew";
        public const string RequestTypeClose = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Cancel";
        public const string RenewTarget = "RenewTarget";
        public const string CloseTarget = "CancelTarget";
        public const string RequestedTokenClosed = "RequestedTokenCancelled";
        public const string RequestedAttachedReference = "RequestedAttachedReference";
        public const string RequestedUnattachedReference = "RequestedUnattachedReference";
        public const string IssuedTokensHeader = "IssuedTokens";
        public const string KeyWrapAlgorithm = "KeyWrapAlgorithm";
        public const string BearerKeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer";
        public const string SecondaryParameters = "SecondaryParameters";
        public const string Dialect = "Dialect";
        public const string DialectType = "http://schemas.xmlsoap.org/ws/2005/05/identity";
    }

    static class Wsrm11Strings
    {
        // dictionary strings
        public const string AckRequestedAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/AckRequested";
        public const string CloseSequence = "CloseSequence";
        public const string CloseSequenceAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence";
        public const string CloseSequenceResponse = "CloseSequenceResponse";
        public const string CloseSequenceResponseAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse";
        public const string CreateSequenceAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequence";
        public const string CreateSequenceResponseAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequenceResponse";
        public const string DiscardFollowingFirstGap = "DiscardFollowingFirstGap";
        public const string Endpoint = "Endpoint";
        public const string FaultAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/fault";
        public const string Final = "Final";
        public const string IncompleteSequenceBehavior = "IncompleteSequenceBehavior";
        public const string LastMsgNumber = "LastMsgNumber";
        public const string MaxMessageNumber = "MaxMessageNumber";
        public const string Namespace = "http://docs.oasis-open.org/ws-rx/wsrm/200702";
        public const string NoDiscard = "NoDiscard";
        public const string None = "None";
        public const string SequenceAcknowledgementAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/SequenceAcknowledgement";
        public const string SequenceClosed = "SequenceClosed";
        public const string TerminateSequenceAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequence";
        public const string TerminateSequenceResponse = "TerminateSequenceResponse";
        public const string TerminateSequenceResponseAction = "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse";
        public const string UsesSequenceSSL = "UsesSequenceSSL";
        public const string UsesSequenceSTR = "UsesSequenceSTR";
        public const string WsrmRequired = "WsrmRequired";
        // string constants
        public const string DiscardEntireSequence = "DiscardEntireSequence";
    }
}
