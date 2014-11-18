//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

// NOTE: this file was generated from 'xd.xml'

namespace System.IdentityModel
{
    using System.Xml;
    using System.Runtime.CompilerServices;

    // Static Xml Dictionary
    static class XD 
    {
        static public IdentityModelDictionary Dictionary { get { return IdentityModelDictionary.CurrentVersion; } }

        static ExclusiveC14NDictionary exclusiveC14NDictionary;
        static SamlDictionary samlDictionary;
        static SecureConversationDec2005Dictionary secureConversationDec2005Dictionary;
        static SecureConversationFeb2005Dictionary secureConversationFeb2005Dictionary;
        static SecurityAlgorithmDictionary securityAlgorithmDictionary;
        static SecurityAlgorithmDec2005Dictionary securityAlgorithmDec2005Dictionary;
        static SecurityJan2004Dictionary securityJan2004Dictionary;
        static SecurityXXX2005Dictionary securityXXX2005Dictionary;
        static TrustDec2005Dictionary trustDec2005Dictionary;
        static TrustFeb2005Dictionary trustFeb2005Dictionary;
        static UtilityDictionary utilityDictionary;
        static XmlEncryptionDictionary xmlEncryptionDictionary;
        static XmlSignatureDictionary xmlSignatureDictionary;

        static public ExclusiveC14NDictionary ExclusiveC14NDictionary
        {
            get
            {
                if (exclusiveC14NDictionary == null)
                    exclusiveC14NDictionary = new ExclusiveC14NDictionary(Dictionary);
                return exclusiveC14NDictionary;
            }
         }

        static public SamlDictionary SamlDictionary
        {
            get
            {
                if (samlDictionary == null)
                    samlDictionary = new SamlDictionary(Dictionary);
                return samlDictionary;
            }
         }

        static public SecureConversationDec2005Dictionary SecureConversationDec2005Dictionary
        {
            get
            {
                if (secureConversationDec2005Dictionary == null)
                    secureConversationDec2005Dictionary = new SecureConversationDec2005Dictionary(Dictionary);
                return secureConversationDec2005Dictionary;
            }
         }

        static public SecureConversationFeb2005Dictionary SecureConversationFeb2005Dictionary
        {
            get
            {
                if (secureConversationFeb2005Dictionary == null)
                    secureConversationFeb2005Dictionary = new SecureConversationFeb2005Dictionary(Dictionary);
                return secureConversationFeb2005Dictionary;
            }
         }

        static public SecurityAlgorithmDictionary SecurityAlgorithmDictionary
        {
            get
            {
                if (securityAlgorithmDictionary == null)
                    securityAlgorithmDictionary = new SecurityAlgorithmDictionary(Dictionary);
                return securityAlgorithmDictionary;
            }
         }

        static public SecurityAlgorithmDec2005Dictionary SecurityAlgorithmDec2005Dictionary
        {
            get
            {
                if (securityAlgorithmDec2005Dictionary == null)
                    securityAlgorithmDec2005Dictionary = new SecurityAlgorithmDec2005Dictionary(Dictionary);
                return securityAlgorithmDec2005Dictionary;
            }
         }

        static public SecurityJan2004Dictionary SecurityJan2004Dictionary
        {
            get
            {
                if (securityJan2004Dictionary == null)
                    securityJan2004Dictionary = new SecurityJan2004Dictionary(Dictionary);
                return securityJan2004Dictionary;
            }
         }

        static public SecurityXXX2005Dictionary SecurityXXX2005Dictionary
        {
            get
            {
                if (securityXXX2005Dictionary == null)
                    securityXXX2005Dictionary = new SecurityXXX2005Dictionary(Dictionary);
                return securityXXX2005Dictionary;
            }
         }

        static public TrustDec2005Dictionary TrustDec2005Dictionary
        {
            get
            {
                if (trustDec2005Dictionary == null)
                    trustDec2005Dictionary = new TrustDec2005Dictionary(Dictionary);
                return trustDec2005Dictionary;
            }
         }

        static public TrustFeb2005Dictionary TrustFeb2005Dictionary
        {
            get
            {
                if (trustFeb2005Dictionary == null)
                    trustFeb2005Dictionary = new TrustFeb2005Dictionary(Dictionary);
                return trustFeb2005Dictionary;
            }
         }

        static public UtilityDictionary UtilityDictionary
        {
            get
            {
                if (utilityDictionary == null)
                    utilityDictionary = new UtilityDictionary(Dictionary);
                return utilityDictionary;
            }
         }

        static public XmlEncryptionDictionary XmlEncryptionDictionary
        {
            get
            {
                if (xmlEncryptionDictionary == null)
                    xmlEncryptionDictionary = new XmlEncryptionDictionary(Dictionary);
                return xmlEncryptionDictionary;
            }
         }

        static public XmlSignatureDictionary XmlSignatureDictionary
        {
            get
            {
                if (xmlSignatureDictionary == null)
                    xmlSignatureDictionary = new XmlSignatureDictionary(Dictionary);
                return xmlSignatureDictionary;
            }
         }

    }

    class ExclusiveC14NDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString PrefixList;
        public XmlDictionaryString InclusiveNamespaces;
        public XmlDictionaryString Prefix;

        public ExclusiveC14NDictionary(IdentityModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String20, 20);
            this.PrefixList = dictionary.CreateString(IdentityModelStringsVersion1.String21, 21);
            this.InclusiveNamespaces = dictionary.CreateString(IdentityModelStringsVersion1.String22, 22);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String23, 23);
        }

        public ExclusiveC14NDictionary(IXmlDictionary dictionary)
        {
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String20);
            this.PrefixList = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String21);
            this.InclusiveNamespaces = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String22);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String23);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SamlDictionary
    {
        public XmlDictionaryString Access;
        public XmlDictionaryString AccessDecision;
        public XmlDictionaryString Action;
        public XmlDictionaryString Advice;
        public XmlDictionaryString Assertion;
        public XmlDictionaryString AssertionId;
        public XmlDictionaryString AssertionIdReference;
        public XmlDictionaryString Attribute;
        public XmlDictionaryString AttributeName;
        public XmlDictionaryString AttributeNamespace;
        public XmlDictionaryString AttributeStatement;
        public XmlDictionaryString AttributeValue;
        public XmlDictionaryString Audience;
        public XmlDictionaryString AudienceRestrictionCondition;
        public XmlDictionaryString AuthenticationInstant;
        public XmlDictionaryString AuthenticationMethod;
        public XmlDictionaryString AuthenticationStatement;
        public XmlDictionaryString AuthorityBinding;
        public XmlDictionaryString AuthorityKind;
        public XmlDictionaryString AuthorizationDecisionStatement;
        public XmlDictionaryString Binding;
        public XmlDictionaryString Condition;
        public XmlDictionaryString Conditions;
        public XmlDictionaryString Decision;
        public XmlDictionaryString DoNotCacheCondition;
        public XmlDictionaryString Evidence;
        public XmlDictionaryString IssueInstant;
        public XmlDictionaryString Issuer;
        public XmlDictionaryString Location;
        public XmlDictionaryString MajorVersion;
        public XmlDictionaryString MinorVersion;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NameIdentifier;
        public XmlDictionaryString NameIdentifierFormat;
        public XmlDictionaryString NameIdentifierNameQualifier;
        public XmlDictionaryString ActionNamespaceAttribute;
        public XmlDictionaryString NotBefore;
        public XmlDictionaryString NotOnOrAfter;
        public XmlDictionaryString PreferredPrefix;
        public XmlDictionaryString Statement;
        public XmlDictionaryString Subject;
        public XmlDictionaryString SubjectConfirmation;
        public XmlDictionaryString SubjectConfirmationData;
        public XmlDictionaryString SubjectConfirmationMethod;
        public XmlDictionaryString HolderOfKey;
        public XmlDictionaryString SenderVouches;
        public XmlDictionaryString SubjectLocality;
        public XmlDictionaryString SubjectLocalityDNSAddress;
        public XmlDictionaryString SubjectLocalityIPAddress;
        public XmlDictionaryString SubjectStatement;
        public XmlDictionaryString UnspecifiedAuthenticationMethod;
        public XmlDictionaryString NamespaceAttributePrefix;
        public XmlDictionaryString Resource;
        public XmlDictionaryString UserName;
        public XmlDictionaryString UserNameNamespace;
        public XmlDictionaryString EmailName;
        public XmlDictionaryString EmailNamespace;

        public SamlDictionary(IdentityModelDictionary dictionary)
        {
            this.Access = dictionary.CreateString(IdentityModelStringsVersion1.String24, 24);
            this.AccessDecision = dictionary.CreateString(IdentityModelStringsVersion1.String25, 25);
            this.Action = dictionary.CreateString(IdentityModelStringsVersion1.String26, 26);
            this.Advice = dictionary.CreateString(IdentityModelStringsVersion1.String27, 27);
            this.Assertion = dictionary.CreateString(IdentityModelStringsVersion1.String28, 28);
            this.AssertionId = dictionary.CreateString(IdentityModelStringsVersion1.String29, 29);
            this.AssertionIdReference = dictionary.CreateString(IdentityModelStringsVersion1.String30, 30);
            this.Attribute = dictionary.CreateString(IdentityModelStringsVersion1.String31, 31);
            this.AttributeName = dictionary.CreateString(IdentityModelStringsVersion1.String32, 32);
            this.AttributeNamespace = dictionary.CreateString(IdentityModelStringsVersion1.String33, 33);
            this.AttributeStatement = dictionary.CreateString(IdentityModelStringsVersion1.String34, 34);
            this.AttributeValue = dictionary.CreateString(IdentityModelStringsVersion1.String35, 35);
            this.Audience = dictionary.CreateString(IdentityModelStringsVersion1.String36, 36);
            this.AudienceRestrictionCondition = dictionary.CreateString(IdentityModelStringsVersion1.String37, 37);
            this.AuthenticationInstant = dictionary.CreateString(IdentityModelStringsVersion1.String38, 38);
            this.AuthenticationMethod = dictionary.CreateString(IdentityModelStringsVersion1.String39, 39);
            this.AuthenticationStatement = dictionary.CreateString(IdentityModelStringsVersion1.String40, 40);
            this.AuthorityBinding = dictionary.CreateString(IdentityModelStringsVersion1.String41, 41);
            this.AuthorityKind = dictionary.CreateString(IdentityModelStringsVersion1.String42, 42);
            this.AuthorizationDecisionStatement = dictionary.CreateString(IdentityModelStringsVersion1.String43, 43);
            this.Binding = dictionary.CreateString(IdentityModelStringsVersion1.String44, 44);
            this.Condition = dictionary.CreateString(IdentityModelStringsVersion1.String45, 45);
            this.Conditions = dictionary.CreateString(IdentityModelStringsVersion1.String46, 46);
            this.Decision = dictionary.CreateString(IdentityModelStringsVersion1.String47, 47);
            this.DoNotCacheCondition = dictionary.CreateString(IdentityModelStringsVersion1.String48, 48);
            this.Evidence = dictionary.CreateString(IdentityModelStringsVersion1.String49, 49);
            this.IssueInstant = dictionary.CreateString(IdentityModelStringsVersion1.String50, 50);
            this.Issuer = dictionary.CreateString(IdentityModelStringsVersion1.String51, 51);
            this.Location = dictionary.CreateString(IdentityModelStringsVersion1.String52, 52);
            this.MajorVersion = dictionary.CreateString(IdentityModelStringsVersion1.String53, 53);
            this.MinorVersion = dictionary.CreateString(IdentityModelStringsVersion1.String54, 54);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String55, 55);
            this.NameIdentifier = dictionary.CreateString(IdentityModelStringsVersion1.String56, 56);
            this.NameIdentifierFormat = dictionary.CreateString(IdentityModelStringsVersion1.String57, 57);
            this.NameIdentifierNameQualifier = dictionary.CreateString(IdentityModelStringsVersion1.String58, 58);
            this.ActionNamespaceAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String59, 59);
            this.NotBefore = dictionary.CreateString(IdentityModelStringsVersion1.String60, 60);
            this.NotOnOrAfter = dictionary.CreateString(IdentityModelStringsVersion1.String61, 61);
            this.PreferredPrefix = dictionary.CreateString(IdentityModelStringsVersion1.String62, 62);
            this.Statement = dictionary.CreateString(IdentityModelStringsVersion1.String63, 63);
            this.Subject = dictionary.CreateString(IdentityModelStringsVersion1.String64, 64);
            this.SubjectConfirmation = dictionary.CreateString(IdentityModelStringsVersion1.String65, 65);
            this.SubjectConfirmationData = dictionary.CreateString(IdentityModelStringsVersion1.String66, 66);
            this.SubjectConfirmationMethod = dictionary.CreateString(IdentityModelStringsVersion1.String67, 67);
            this.HolderOfKey = dictionary.CreateString(IdentityModelStringsVersion1.String68, 68);
            this.SenderVouches = dictionary.CreateString(IdentityModelStringsVersion1.String69, 69);
            this.SubjectLocality = dictionary.CreateString(IdentityModelStringsVersion1.String70, 70);
            this.SubjectLocalityDNSAddress = dictionary.CreateString(IdentityModelStringsVersion1.String71, 71);
            this.SubjectLocalityIPAddress = dictionary.CreateString(IdentityModelStringsVersion1.String72, 72);
            this.SubjectStatement = dictionary.CreateString(IdentityModelStringsVersion1.String73, 73);
            this.UnspecifiedAuthenticationMethod = dictionary.CreateString(IdentityModelStringsVersion1.String74, 74);
            this.NamespaceAttributePrefix = dictionary.CreateString(IdentityModelStringsVersion1.String75, 75);
            this.Resource = dictionary.CreateString(IdentityModelStringsVersion1.String76, 76);
            this.UserName = dictionary.CreateString(IdentityModelStringsVersion1.String77, 77);
            this.UserNameNamespace = dictionary.CreateString(IdentityModelStringsVersion1.String78, 78);
            this.EmailName = dictionary.CreateString(IdentityModelStringsVersion1.String79, 79);
            this.EmailNamespace = dictionary.CreateString(IdentityModelStringsVersion1.String80, 80);
        }

        public SamlDictionary(IXmlDictionary dictionary)
        {
            this.Access = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String24);
            this.AccessDecision = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String25);
            this.Action = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String26);
            this.Advice = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String27);
            this.Assertion = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String28);
            this.AssertionId = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String29);
            this.AssertionIdReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String30);
            this.Attribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String31);
            this.AttributeName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String32);
            this.AttributeNamespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String33);
            this.AttributeStatement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String34);
            this.AttributeValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String35);
            this.Audience = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String36);
            this.AudienceRestrictionCondition = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String37);
            this.AuthenticationInstant = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String38);
            this.AuthenticationMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String39);
            this.AuthenticationStatement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String40);
            this.AuthorityBinding = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String41);
            this.AuthorityKind = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String42);
            this.AuthorizationDecisionStatement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String43);
            this.Binding = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String44);
            this.Condition = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String45);
            this.Conditions = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String46);
            this.Decision = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String47);
            this.DoNotCacheCondition = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String48);
            this.Evidence = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String49);
            this.IssueInstant = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String50);
            this.Issuer = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String51);
            this.Location = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String52);
            this.MajorVersion = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String53);
            this.MinorVersion = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String54);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String55);
            this.NameIdentifier = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String56);
            this.NameIdentifierFormat = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String57);
            this.NameIdentifierNameQualifier = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String58);
            this.ActionNamespaceAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String59);
            this.NotBefore = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String60);
            this.NotOnOrAfter = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String61);
            this.PreferredPrefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String62);
            this.Statement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String63);
            this.Subject = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String64);
            this.SubjectConfirmation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String65);
            this.SubjectConfirmationData = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String66);
            this.SubjectConfirmationMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String67);
            this.HolderOfKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String68);
            this.SenderVouches = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String69);
            this.SubjectLocality = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String70);
            this.SubjectLocalityDNSAddress = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String71);
            this.SubjectLocalityIPAddress = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String72);
            this.SubjectStatement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String73);
            this.UnspecifiedAuthenticationMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String74);
            this.NamespaceAttributePrefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String75);
            this.Resource = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String76);
            this.UserName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String77);
            this.UserNameNamespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String78);
            this.EmailName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String79);
            this.EmailNamespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String80);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecureConversationDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString DerivedKeyToken;
        public XmlDictionaryString Nonce;
        public XmlDictionaryString Length;
        public XmlDictionaryString SecurityContextToken;
        public XmlDictionaryString AlgorithmAttribute;
        public XmlDictionaryString Generation;
        public XmlDictionaryString Label;
        public XmlDictionaryString Offset;
        public XmlDictionaryString Properties;
        public XmlDictionaryString Identifier;
        public XmlDictionaryString Cookie;
        public XmlDictionaryString RenewNeededFaultCode;
        public XmlDictionaryString BadContextTokenFaultCode;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString DerivedKeyTokenType;
        public XmlDictionaryString SecurityContextTokenType;
        public XmlDictionaryString SecurityContextTokenReferenceValueType;
        public XmlDictionaryString RequestSecurityContextIssuance;
        public XmlDictionaryString RequestSecurityContextIssuanceResponse;
        public XmlDictionaryString RequestSecurityContextRenew;
        public XmlDictionaryString RequestSecurityContextRenewResponse;
        public XmlDictionaryString RequestSecurityContextClose;
        public XmlDictionaryString RequestSecurityContextCloseResponse;
        public XmlDictionaryString Instance;

        public SecureConversationDictionary()
        {
        }

        public SecureConversationDictionary(IdentityModelDictionary dictionary)
        {
        }

        public SecureConversationDictionary(IXmlDictionary dictionary)
        {
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecureConversationDec2005Dictionary : SecureConversationDictionary
    {

        public SecureConversationDec2005Dictionary(IdentityModelDictionary dictionary) : base(dictionary)
        {
            this.SecurityContextToken = dictionary.CreateString(IdentityModelStringsVersion1.String175, 175);
            this.AlgorithmAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String0, 0);
            this.Generation = dictionary.CreateString(IdentityModelStringsVersion1.String176, 176);
            this.Label = dictionary.CreateString(IdentityModelStringsVersion1.String177, 177);
            this.Offset = dictionary.CreateString(IdentityModelStringsVersion1.String178, 178);
            this.Properties = dictionary.CreateString(IdentityModelStringsVersion1.String179, 179);
            this.Identifier = dictionary.CreateString(IdentityModelStringsVersion1.String180, 180);
            this.Cookie = dictionary.CreateString(IdentityModelStringsVersion1.String181, 181);
            this.RenewNeededFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String182, 182);
            this.BadContextTokenFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String183, 183);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String268, 268);
            this.DerivedKeyTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String269, 269);
            this.SecurityContextTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String270, 270);
            this.SecurityContextTokenReferenceValueType = dictionary.CreateString(IdentityModelStringsVersion1.String270, 270);
            this.RequestSecurityContextIssuance = dictionary.CreateString(IdentityModelStringsVersion1.String271, 271);
            this.RequestSecurityContextIssuanceResponse = dictionary.CreateString(IdentityModelStringsVersion1.String272, 272);
            this.RequestSecurityContextRenew = dictionary.CreateString(IdentityModelStringsVersion1.String273, 273);
            this.RequestSecurityContextRenewResponse = dictionary.CreateString(IdentityModelStringsVersion1.String274, 274);
            this.RequestSecurityContextClose = dictionary.CreateString(IdentityModelStringsVersion1.String275, 275);
            this.RequestSecurityContextCloseResponse = dictionary.CreateString(IdentityModelStringsVersion1.String276, 276);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String277, 277);
            this.DerivedKeyToken = dictionary.CreateString(IdentityModelStringsVersion1.String173, 173);
            this.Nonce = dictionary.CreateString(IdentityModelStringsVersion1.String120, 120);
            this.Length = dictionary.CreateString(IdentityModelStringsVersion1.String174, 174);
            this.Instance = dictionary.CreateString(IdentityModelStringsVersion1.String278, 278);
        }

        public SecureConversationDec2005Dictionary(IXmlDictionary dictionary) : base(dictionary)
        {
            this.SecurityContextToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String175);
            this.AlgorithmAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String0);
            this.Generation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String176);
            this.Label = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String177);
            this.Offset = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String178);
            this.Properties = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String179);
            this.Identifier = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String180);
            this.Cookie = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String181);
            this.RenewNeededFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String182);
            this.BadContextTokenFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String183);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String268);
            this.DerivedKeyTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String269);
            this.SecurityContextTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String270);
            this.SecurityContextTokenReferenceValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String270);
            this.RequestSecurityContextIssuance = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String271);
            this.RequestSecurityContextIssuanceResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String272);
            this.RequestSecurityContextRenew = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String273);
            this.RequestSecurityContextRenewResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String274);
            this.RequestSecurityContextClose = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String275);
            this.RequestSecurityContextCloseResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String276);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String277);
            this.DerivedKeyToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String173);
            this.Nonce = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String120);
            this.Length = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String174);
            this.Instance = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String278);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecureConversationFeb2005Dictionary : SecureConversationDictionary
    {

        public SecureConversationFeb2005Dictionary(IdentityModelDictionary dictionary) : base(dictionary)
        {
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String172, 172);
            this.DerivedKeyToken = dictionary.CreateString(IdentityModelStringsVersion1.String173, 173);
            this.Nonce = dictionary.CreateString(IdentityModelStringsVersion1.String120, 120);
            this.Length = dictionary.CreateString(IdentityModelStringsVersion1.String174, 174);
            this.SecurityContextToken = dictionary.CreateString(IdentityModelStringsVersion1.String175, 175);
            this.AlgorithmAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String0, 0);
            this.Generation = dictionary.CreateString(IdentityModelStringsVersion1.String176, 176);
            this.Label = dictionary.CreateString(IdentityModelStringsVersion1.String177, 177);
            this.Offset = dictionary.CreateString(IdentityModelStringsVersion1.String178, 178);
            this.Properties = dictionary.CreateString(IdentityModelStringsVersion1.String179, 179);
            this.Identifier = dictionary.CreateString(IdentityModelStringsVersion1.String180, 180);
            this.Cookie = dictionary.CreateString(IdentityModelStringsVersion1.String181, 181);
            this.RenewNeededFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String182, 182);
            this.BadContextTokenFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String183, 183);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String184, 184);
            this.DerivedKeyTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String185, 185);
            this.SecurityContextTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String186, 186);
            this.SecurityContextTokenReferenceValueType = dictionary.CreateString(IdentityModelStringsVersion1.String186, 186);
            this.RequestSecurityContextIssuance = dictionary.CreateString(IdentityModelStringsVersion1.String187, 187);
            this.RequestSecurityContextIssuanceResponse = dictionary.CreateString(IdentityModelStringsVersion1.String188, 188);
            this.RequestSecurityContextRenew = dictionary.CreateString(IdentityModelStringsVersion1.String189, 189);
            this.RequestSecurityContextRenewResponse = dictionary.CreateString(IdentityModelStringsVersion1.String190, 190);
            this.RequestSecurityContextClose = dictionary.CreateString(IdentityModelStringsVersion1.String191, 191);
            this.RequestSecurityContextCloseResponse = dictionary.CreateString(IdentityModelStringsVersion1.String192, 192);
        }

        public SecureConversationFeb2005Dictionary(IXmlDictionary dictionary) : base(dictionary)
        {
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String172);
            this.DerivedKeyToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String173);
            this.Nonce = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String120);
            this.Length = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String174);
            this.SecurityContextToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String175);
            this.AlgorithmAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String0);
            this.Generation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String176);
            this.Label = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String177);
            this.Offset = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String178);
            this.Properties = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String179);
            this.Identifier = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String180);
            this.Cookie = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String181);
            this.RenewNeededFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String182);
            this.BadContextTokenFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String183);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String184);
            this.DerivedKeyTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String185);
            this.SecurityContextTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String186);
            this.SecurityContextTokenReferenceValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String186);
            this.RequestSecurityContextIssuance = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String187);
            this.RequestSecurityContextIssuanceResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String188);
            this.RequestSecurityContextRenew = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String189);
            this.RequestSecurityContextRenewResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String190);
            this.RequestSecurityContextClose = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String191);
            this.RequestSecurityContextCloseResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String192);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecurityAlgorithmDictionary
    {
        public XmlDictionaryString Aes128Encryption;
        public XmlDictionaryString Aes128KeyWrap;
        public XmlDictionaryString Aes192Encryption;
        public XmlDictionaryString Aes192KeyWrap;
        public XmlDictionaryString Aes256Encryption;
        public XmlDictionaryString Aes256KeyWrap;
        public XmlDictionaryString DesEncryption;
        public XmlDictionaryString DsaSha1Signature;
        public XmlDictionaryString ExclusiveC14n;
        public XmlDictionaryString ExclusiveC14nWithComments;
        public XmlDictionaryString HmacSha1Signature;
        public XmlDictionaryString HmacSha256Signature;
        public XmlDictionaryString Psha1KeyDerivation;
        public XmlDictionaryString Ripemd160Digest;
        public XmlDictionaryString RsaOaepKeyWrap;
        public XmlDictionaryString RsaSha1Signature;
        public XmlDictionaryString RsaSha256Signature;
        public XmlDictionaryString RsaV15KeyWrap;
        public XmlDictionaryString Sha1Digest;
        public XmlDictionaryString Sha256Digest;
        public XmlDictionaryString Sha512Digest;
        public XmlDictionaryString TripleDesEncryption;
        public XmlDictionaryString TripleDesKeyWrap;
        public XmlDictionaryString TlsSspiKeyWrap;
        public XmlDictionaryString WindowsSspiKeyWrap;

        public SecurityAlgorithmDictionary(IdentityModelDictionary dictionary)
        {
            this.Aes128Encryption = dictionary.CreateString(IdentityModelStringsVersion1.String95, 95);
            this.Aes128KeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String96, 96);
            this.Aes192Encryption = dictionary.CreateString(IdentityModelStringsVersion1.String97, 97);
            this.Aes192KeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String98, 98);
            this.Aes256Encryption = dictionary.CreateString(IdentityModelStringsVersion1.String99, 99);
            this.Aes256KeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String100, 100);
            this.DesEncryption = dictionary.CreateString(IdentityModelStringsVersion1.String101, 101);
            this.DsaSha1Signature = dictionary.CreateString(IdentityModelStringsVersion1.String102, 102);
            this.ExclusiveC14n = dictionary.CreateString(IdentityModelStringsVersion1.String20, 20);
            this.ExclusiveC14nWithComments = dictionary.CreateString(IdentityModelStringsVersion1.String103, 103);
            this.HmacSha1Signature = dictionary.CreateString(IdentityModelStringsVersion1.String104, 104);
            this.HmacSha256Signature = dictionary.CreateString(IdentityModelStringsVersion1.String105, 105);
            this.Psha1KeyDerivation = dictionary.CreateString(IdentityModelStringsVersion1.String106, 106);
            this.Ripemd160Digest = dictionary.CreateString(IdentityModelStringsVersion1.String107, 107);
            this.RsaOaepKeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String108, 108);
            this.RsaSha1Signature = dictionary.CreateString(IdentityModelStringsVersion1.String109, 109);
            this.RsaSha256Signature = dictionary.CreateString(IdentityModelStringsVersion1.String110, 110);
            this.RsaV15KeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String111, 111);
            this.Sha1Digest = dictionary.CreateString(IdentityModelStringsVersion1.String112, 112);
            this.Sha256Digest = dictionary.CreateString(IdentityModelStringsVersion1.String113, 113);
            this.Sha512Digest = dictionary.CreateString(IdentityModelStringsVersion1.String114, 114);
            this.TripleDesEncryption = dictionary.CreateString(IdentityModelStringsVersion1.String115, 115);
            this.TripleDesKeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String116, 116);
            this.TlsSspiKeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String117, 117);
            this.WindowsSspiKeyWrap = dictionary.CreateString(IdentityModelStringsVersion1.String118, 118);
        }

        public SecurityAlgorithmDictionary(IXmlDictionary dictionary)
        {
            this.Aes128Encryption = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String95);
            this.Aes128KeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String96);
            this.Aes192Encryption = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String97);
            this.Aes192KeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String98);
            this.Aes256Encryption = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String99);
            this.Aes256KeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String100);
            this.DesEncryption = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String101);
            this.DsaSha1Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String102);
            this.ExclusiveC14n = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String20);
            this.ExclusiveC14nWithComments = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String103);
            this.HmacSha1Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String104);
            this.HmacSha256Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String105);
            this.Psha1KeyDerivation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String106);
            this.Ripemd160Digest = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String107);
            this.RsaOaepKeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String108);
            this.RsaSha1Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String109);
            this.RsaSha256Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String110);
            this.RsaV15KeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String111);
            this.Sha1Digest = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String112);
            this.Sha256Digest = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String113);
            this.Sha512Digest = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String114);
            this.TripleDesEncryption = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String115);
            this.TripleDesKeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String116);
            this.TlsSspiKeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String117);
            this.WindowsSspiKeyWrap = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String118);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecurityAlgorithmDec2005Dictionary
    {
        public XmlDictionaryString Psha1KeyDerivationDec2005;

        public SecurityAlgorithmDec2005Dictionary(IdentityModelDictionary dictionary)
        {
            this.Psha1KeyDerivationDec2005 = dictionary.CreateString(IdentityModelStringsVersion1.String267, 267);
        }

        public SecurityAlgorithmDec2005Dictionary(IXmlDictionary dictionary)
        {
            this.Psha1KeyDerivationDec2005 = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String267);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecurityJan2004Dictionary
    {
        public XmlDictionaryString Prefix;
        public XmlDictionaryString NonceElement;
        public XmlDictionaryString PasswordElement;
        public XmlDictionaryString PasswordTextName;
        public XmlDictionaryString UserNameElement;
        public XmlDictionaryString UserNameTokenElement;
        public XmlDictionaryString BinarySecurityToken;
        public XmlDictionaryString EncodingType;
        public XmlDictionaryString Reference;
        public XmlDictionaryString URI;
        public XmlDictionaryString KeyIdentifier;
        public XmlDictionaryString EncodingTypeValueBase64Binary;
        public XmlDictionaryString EncodingTypeValueHexBinary;
        public XmlDictionaryString EncodingTypeValueText;
        public XmlDictionaryString X509SKIValueType;
        public XmlDictionaryString KerberosTokenTypeGSS;
        public XmlDictionaryString KerberosTokenType1510;
        public XmlDictionaryString SamlAssertionIdValueType;
        public XmlDictionaryString SamlAssertion;
        public XmlDictionaryString SamlUri;
        public XmlDictionaryString RelAssertionValueType;
        public XmlDictionaryString FailedAuthenticationFaultCode;
        public XmlDictionaryString InvalidSecurityTokenFaultCode;
        public XmlDictionaryString InvalidSecurityFaultCode;
        public XmlDictionaryString SecurityTokenReference;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Security;
        public XmlDictionaryString ValueType;
        public XmlDictionaryString TypeAttribute;
        public XmlDictionaryString KerberosHashValueType;

        public SecurityJan2004Dictionary(IdentityModelDictionary dictionary)
        {
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String119, 119);
            this.NonceElement = dictionary.CreateString(IdentityModelStringsVersion1.String120, 120);
            this.PasswordElement = dictionary.CreateString(IdentityModelStringsVersion1.String121, 121);
            this.PasswordTextName = dictionary.CreateString(IdentityModelStringsVersion1.String122, 122);
            this.UserNameElement = dictionary.CreateString(IdentityModelStringsVersion1.String123, 123);
            this.UserNameTokenElement = dictionary.CreateString(IdentityModelStringsVersion1.String124, 124);
            this.BinarySecurityToken = dictionary.CreateString(IdentityModelStringsVersion1.String125, 125);
            this.EncodingType = dictionary.CreateString(IdentityModelStringsVersion1.String126, 126);
            this.Reference = dictionary.CreateString(IdentityModelStringsVersion1.String2, 2);
            this.URI = dictionary.CreateString(IdentityModelStringsVersion1.String1, 1);
            this.KeyIdentifier = dictionary.CreateString(IdentityModelStringsVersion1.String127, 127);
            this.EncodingTypeValueBase64Binary = dictionary.CreateString(IdentityModelStringsVersion1.String128, 128);
            this.EncodingTypeValueHexBinary = dictionary.CreateString(IdentityModelStringsVersion1.String129, 129);
            this.EncodingTypeValueText = dictionary.CreateString(IdentityModelStringsVersion1.String130, 130);
            this.X509SKIValueType = dictionary.CreateString(IdentityModelStringsVersion1.String131, 131);
            this.KerberosTokenTypeGSS = dictionary.CreateString(IdentityModelStringsVersion1.String132, 132);
            this.KerberosTokenType1510 = dictionary.CreateString(IdentityModelStringsVersion1.String133, 133);
            this.SamlAssertionIdValueType = dictionary.CreateString(IdentityModelStringsVersion1.String134, 134);
            this.SamlAssertion = dictionary.CreateString(IdentityModelStringsVersion1.String28, 28);
            this.SamlUri = dictionary.CreateString(IdentityModelStringsVersion1.String55, 55);
            this.RelAssertionValueType = dictionary.CreateString(IdentityModelStringsVersion1.String135, 135);
            this.FailedAuthenticationFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String136, 136);
            this.InvalidSecurityTokenFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String137, 137);
            this.InvalidSecurityFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String138, 138);
            this.SecurityTokenReference = dictionary.CreateString(IdentityModelStringsVersion1.String139, 139);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String140, 140);
            this.Security = dictionary.CreateString(IdentityModelStringsVersion1.String141, 141);
            this.ValueType = dictionary.CreateString(IdentityModelStringsVersion1.String142, 142);
            this.TypeAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String83, 83);
            this.KerberosHashValueType = dictionary.CreateString(IdentityModelStringsVersion1.String143, 143);
        }

        public SecurityJan2004Dictionary(IXmlDictionary dictionary)
        {
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String119);
            this.NonceElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String120);
            this.PasswordElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String121);
            this.PasswordTextName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String122);
            this.UserNameElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String123);
            this.UserNameTokenElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String124);
            this.BinarySecurityToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String125);
            this.EncodingType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String126);
            this.Reference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String2);
            this.URI = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String1);
            this.KeyIdentifier = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String127);
            this.EncodingTypeValueBase64Binary = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String128);
            this.EncodingTypeValueHexBinary = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String129);
            this.EncodingTypeValueText = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String130);
            this.X509SKIValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String131);
            this.KerberosTokenTypeGSS = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String132);
            this.KerberosTokenType1510 = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String133);
            this.SamlAssertionIdValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String134);
            this.SamlAssertion = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String28);
            this.SamlUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String55);
            this.RelAssertionValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String135);
            this.FailedAuthenticationFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String136);
            this.InvalidSecurityTokenFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String137);
            this.InvalidSecurityFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String138);
            this.SecurityTokenReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String139);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String140);
            this.Security = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String141);
            this.ValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String142);
            this.TypeAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String83);
            this.KerberosHashValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String143);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class SecurityXXX2005Dictionary
    {
        public XmlDictionaryString Prefix;
        public XmlDictionaryString SignatureConfirmation;
        public XmlDictionaryString ValueAttribute;
        public XmlDictionaryString TokenTypeAttribute;
        public XmlDictionaryString ThumbprintSha1ValueType;
        public XmlDictionaryString EncryptedKeyTokenType;
        public XmlDictionaryString EncryptedKeyHashValueType;
        public XmlDictionaryString SamlTokenType;
        public XmlDictionaryString Saml20TokenType;
        public XmlDictionaryString Saml11AssertionValueType;
        public XmlDictionaryString EncryptedHeader;
        public XmlDictionaryString Namespace;

        public SecurityXXX2005Dictionary(IdentityModelDictionary dictionary)
        {
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String144, 144);
            this.SignatureConfirmation = dictionary.CreateString(IdentityModelStringsVersion1.String145, 145);
            this.ValueAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String146, 146);
            this.TokenTypeAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String147, 147);
            this.ThumbprintSha1ValueType = dictionary.CreateString(IdentityModelStringsVersion1.String148, 148);
            this.EncryptedKeyTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String149, 149);
            this.EncryptedKeyHashValueType = dictionary.CreateString(IdentityModelStringsVersion1.String150, 150);
            this.SamlTokenType = dictionary.CreateString(IdentityModelStringsVersion1.String151, 151);
            this.Saml20TokenType = dictionary.CreateString(IdentityModelStringsVersion1.String152, 152);
            this.Saml11AssertionValueType = dictionary.CreateString(IdentityModelStringsVersion1.String153, 153);
            this.EncryptedHeader = dictionary.CreateString(IdentityModelStringsVersion1.String154, 154);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String155, 155);
        }

        public SecurityXXX2005Dictionary(IXmlDictionary dictionary)
        {
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String144);
            this.SignatureConfirmation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String145);
            this.ValueAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String146);
            this.TokenTypeAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String147);
            this.ThumbprintSha1ValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String148);
            this.EncryptedKeyTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String149);
            this.EncryptedKeyHashValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String150);
            this.SamlTokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String151);
            this.Saml20TokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String152);
            this.Saml11AssertionValueType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String153);
            this.EncryptedHeader = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String154);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String155);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class TrustDictionary
    {
        public XmlDictionaryString RequestSecurityTokenResponseCollection;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString BinarySecretClauseType;
        public XmlDictionaryString CombinedHashLabel;
        public XmlDictionaryString RequestSecurityTokenResponse;
        public XmlDictionaryString TokenType;
        public XmlDictionaryString KeySize;
        public XmlDictionaryString RequestedTokenReference;
        public XmlDictionaryString AppliesTo;
        public XmlDictionaryString Authenticator;
        public XmlDictionaryString CombinedHash;
        public XmlDictionaryString BinaryExchange;
        public XmlDictionaryString Lifetime;
        public XmlDictionaryString RequestedSecurityToken;
        public XmlDictionaryString Entropy;
        public XmlDictionaryString RequestedProofToken;
        public XmlDictionaryString ComputedKey;
        public XmlDictionaryString RequestSecurityToken;
        public XmlDictionaryString RequestType;
        public XmlDictionaryString Context;
        public XmlDictionaryString BinarySecret;
        public XmlDictionaryString Type;
        public XmlDictionaryString SpnegoValueTypeUri;
        public XmlDictionaryString TlsnegoValueTypeUri;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString RequestSecurityTokenIssuance;
        public XmlDictionaryString RequestSecurityTokenIssuanceResponse;
        public XmlDictionaryString RequestTypeIssue;
        public XmlDictionaryString SymmetricKeyBinarySecret;
        public XmlDictionaryString Psha1ComputedKeyUri;
        public XmlDictionaryString NonceBinarySecret;
        public XmlDictionaryString RenewTarget;
        public XmlDictionaryString CloseTarget;
        public XmlDictionaryString RequestedTokenClosed;
        public XmlDictionaryString RequestedAttachedReference;
        public XmlDictionaryString RequestedUnattachedReference;
        public XmlDictionaryString IssuedTokensHeader;
        public XmlDictionaryString RequestTypeRenew;
        public XmlDictionaryString RequestTypeClose;
        public XmlDictionaryString KeyType;
        public XmlDictionaryString SymmetricKeyType;
        public XmlDictionaryString PublicKeyType;
        public XmlDictionaryString Claims;
        public XmlDictionaryString InvalidRequestFaultCode;
        public XmlDictionaryString FailedAuthenticationFaultCode;
        public XmlDictionaryString UseKey;
        public XmlDictionaryString SignWith;
        public XmlDictionaryString EncryptWith;
        public XmlDictionaryString EncryptionAlgorithm;
        public XmlDictionaryString CanonicalizationAlgorithm;
        public XmlDictionaryString ComputedKeyAlgorithm;
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

        public TrustDictionary()
        {
        }

        public TrustDictionary(IdentityModelDictionary dictionary)
        {
        }

        public TrustDictionary(IXmlDictionary dictionary)
        {
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class TrustDec2005Dictionary : TrustDictionary
    {

        public TrustDec2005Dictionary(IdentityModelDictionary dictionary) : base(dictionary)
        {
            this.CombinedHashLabel = dictionary.CreateString(IdentityModelStringsVersion1.String196, 196);
            this.RequestSecurityTokenResponse = dictionary.CreateString(IdentityModelStringsVersion1.String197, 197);
            this.TokenType = dictionary.CreateString(IdentityModelStringsVersion1.String147, 147);
            this.KeySize = dictionary.CreateString(IdentityModelStringsVersion1.String198, 198);
            this.RequestedTokenReference = dictionary.CreateString(IdentityModelStringsVersion1.String199, 199);
            this.AppliesTo = dictionary.CreateString(IdentityModelStringsVersion1.String200, 200);
            this.Authenticator = dictionary.CreateString(IdentityModelStringsVersion1.String201, 201);
            this.CombinedHash = dictionary.CreateString(IdentityModelStringsVersion1.String202, 202);
            this.BinaryExchange = dictionary.CreateString(IdentityModelStringsVersion1.String203, 203);
            this.Lifetime = dictionary.CreateString(IdentityModelStringsVersion1.String204, 204);
            this.RequestedSecurityToken = dictionary.CreateString(IdentityModelStringsVersion1.String205, 205);
            this.Entropy = dictionary.CreateString(IdentityModelStringsVersion1.String206, 206);
            this.RequestedProofToken = dictionary.CreateString(IdentityModelStringsVersion1.String207, 207);
            this.ComputedKey = dictionary.CreateString(IdentityModelStringsVersion1.String208, 208);
            this.RequestSecurityToken = dictionary.CreateString(IdentityModelStringsVersion1.String209, 209);
            this.RequestType = dictionary.CreateString(IdentityModelStringsVersion1.String210, 210);
            this.Context = dictionary.CreateString(IdentityModelStringsVersion1.String211, 211);
            this.BinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String212, 212);
            this.Type = dictionary.CreateString(IdentityModelStringsVersion1.String83, 83);
            this.SpnegoValueTypeUri = dictionary.CreateString(IdentityModelStringsVersion1.String240, 240);
            this.TlsnegoValueTypeUri = dictionary.CreateString(IdentityModelStringsVersion1.String241, 241);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String242, 242);
            this.RequestSecurityTokenIssuance = dictionary.CreateString(IdentityModelStringsVersion1.String243, 243);
            this.RequestSecurityTokenIssuanceResponse = dictionary.CreateString(IdentityModelStringsVersion1.String244, 244);
            this.RequestTypeIssue = dictionary.CreateString(IdentityModelStringsVersion1.String245, 245);
            this.AsymmetricKeyBinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String246, 246);
            this.SymmetricKeyBinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String247, 247);
            this.NonceBinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String248, 248);
            this.Psha1ComputedKeyUri = dictionary.CreateString(IdentityModelStringsVersion1.String249, 249);
            this.KeyType = dictionary.CreateString(IdentityModelStringsVersion1.String230, 230);
            this.SymmetricKeyType = dictionary.CreateString(IdentityModelStringsVersion1.String247, 247);
            this.PublicKeyType = dictionary.CreateString(IdentityModelStringsVersion1.String250, 250);
            this.Claims = dictionary.CreateString(IdentityModelStringsVersion1.String232, 232);
            this.InvalidRequestFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String233, 233);
            this.FailedAuthenticationFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String136, 136);
            this.UseKey = dictionary.CreateString(IdentityModelStringsVersion1.String234, 234);
            this.SignWith = dictionary.CreateString(IdentityModelStringsVersion1.String235, 235);
            this.EncryptWith = dictionary.CreateString(IdentityModelStringsVersion1.String236, 236);
            this.EncryptionAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String237, 237);
            this.CanonicalizationAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String238, 238);
            this.ComputedKeyAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String239, 239);
            this.RequestSecurityTokenResponseCollection = dictionary.CreateString(IdentityModelStringsVersion1.String193, 193);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String251, 251);
            this.BinarySecretClauseType = dictionary.CreateString(IdentityModelStringsVersion1.String252, 252);
            this.RequestSecurityTokenCollectionIssuanceFinalResponse = dictionary.CreateString(IdentityModelStringsVersion1.String253, 253);
            this.RequestSecurityTokenRenewal = dictionary.CreateString(IdentityModelStringsVersion1.String254, 254);
            this.RequestSecurityTokenRenewalResponse = dictionary.CreateString(IdentityModelStringsVersion1.String255, 255);
            this.RequestSecurityTokenCollectionRenewalFinalResponse = dictionary.CreateString(IdentityModelStringsVersion1.String256, 256);
            this.RequestSecurityTokenCancellation = dictionary.CreateString(IdentityModelStringsVersion1.String257, 257);
            this.RequestSecurityTokenCancellationResponse = dictionary.CreateString(IdentityModelStringsVersion1.String258, 258);
            this.RequestSecurityTokenCollectionCancellationFinalResponse = dictionary.CreateString(IdentityModelStringsVersion1.String259, 259);
            this.RequestTypeRenew = dictionary.CreateString(IdentityModelStringsVersion1.String260, 260);
            this.RequestTypeClose = dictionary.CreateString(IdentityModelStringsVersion1.String261, 261);
            this.RenewTarget = dictionary.CreateString(IdentityModelStringsVersion1.String222, 222);
            this.CloseTarget = dictionary.CreateString(IdentityModelStringsVersion1.String223, 223);
            this.RequestedTokenClosed = dictionary.CreateString(IdentityModelStringsVersion1.String224, 224);
            this.RequestedAttachedReference = dictionary.CreateString(IdentityModelStringsVersion1.String225, 225);
            this.RequestedUnattachedReference = dictionary.CreateString(IdentityModelStringsVersion1.String226, 226);
            this.IssuedTokensHeader = dictionary.CreateString(IdentityModelStringsVersion1.String227, 227);
            this.KeyWrapAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String262, 262);
            this.BearerKeyType = dictionary.CreateString(IdentityModelStringsVersion1.String263, 263);
            this.SecondaryParameters = dictionary.CreateString(IdentityModelStringsVersion1.String264, 264);
            this.Dialect = dictionary.CreateString(IdentityModelStringsVersion1.String265, 265);
            this.DialectType = dictionary.CreateString(IdentityModelStringsVersion1.String266, 266);
        }

        public TrustDec2005Dictionary(IXmlDictionary dictionary) : base(dictionary)
        {
            this.CombinedHashLabel = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String196);
            this.RequestSecurityTokenResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String197);
            this.TokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String147);
            this.KeySize = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String198);
            this.RequestedTokenReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String199);
            this.AppliesTo = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String200);
            this.Authenticator = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String201);
            this.CombinedHash = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String202);
            this.BinaryExchange = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String203);
            this.Lifetime = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String204);
            this.RequestedSecurityToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String205);
            this.Entropy = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String206);
            this.RequestedProofToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String207);
            this.ComputedKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String208);
            this.RequestSecurityToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String209);
            this.RequestType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String210);
            this.Context = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String211);
            this.BinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String212);
            this.Type = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String83);
            this.SpnegoValueTypeUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String240);
            this.TlsnegoValueTypeUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String241);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String242);
            this.RequestSecurityTokenIssuance = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String243);
            this.RequestSecurityTokenIssuanceResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String244);
            this.RequestTypeIssue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String245);
            this.AsymmetricKeyBinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String246);
            this.SymmetricKeyBinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String247);
            this.NonceBinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String248);
            this.Psha1ComputedKeyUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String249);
            this.KeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String230);
            this.SymmetricKeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String247);
            this.PublicKeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String250);
            this.Claims = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String232);
            this.InvalidRequestFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String233);
            this.FailedAuthenticationFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String136);
            this.UseKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String234);
            this.SignWith = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String235);
            this.EncryptWith = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String236);
            this.EncryptionAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String237);
            this.CanonicalizationAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String238);
            this.ComputedKeyAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String239);
            this.RequestSecurityTokenResponseCollection = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String193);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String251);
            this.BinarySecretClauseType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String252);
            this.RequestSecurityTokenCollectionIssuanceFinalResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String253);
            this.RequestSecurityTokenRenewal = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String254);
            this.RequestSecurityTokenRenewalResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String255);
            this.RequestSecurityTokenCollectionRenewalFinalResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String256);
            this.RequestSecurityTokenCancellation = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String257);
            this.RequestSecurityTokenCancellationResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String258);
            this.RequestSecurityTokenCollectionCancellationFinalResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String259);
            this.RequestTypeRenew = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String260);
            this.RequestTypeClose = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String261);
            this.RenewTarget = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String222);
            this.CloseTarget = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String223);
            this.RequestedTokenClosed = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String224);
            this.RequestedAttachedReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String225);
            this.RequestedUnattachedReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String226);
            this.IssuedTokensHeader = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String227);
            this.KeyWrapAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String262);
            this.BearerKeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String263);
            this.SecondaryParameters = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String264);
            this.Dialect = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String265);
            this.DialectType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String266);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class TrustFeb2005Dictionary : TrustDictionary
    {

        public TrustFeb2005Dictionary(IdentityModelDictionary dictionary) : base(dictionary)
        {
            this.RequestSecurityTokenResponseCollection = dictionary.CreateString(IdentityModelStringsVersion1.String193, 193);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String194, 194);
            this.BinarySecretClauseType = dictionary.CreateString(IdentityModelStringsVersion1.String195, 195);
            this.CombinedHashLabel = dictionary.CreateString(IdentityModelStringsVersion1.String196, 196);
            this.RequestSecurityTokenResponse = dictionary.CreateString(IdentityModelStringsVersion1.String197, 197);
            this.TokenType = dictionary.CreateString(IdentityModelStringsVersion1.String147, 147);
            this.KeySize = dictionary.CreateString(IdentityModelStringsVersion1.String198, 198);
            this.RequestedTokenReference = dictionary.CreateString(IdentityModelStringsVersion1.String199, 199);
            this.AppliesTo = dictionary.CreateString(IdentityModelStringsVersion1.String200, 200);
            this.Authenticator = dictionary.CreateString(IdentityModelStringsVersion1.String201, 201);
            this.CombinedHash = dictionary.CreateString(IdentityModelStringsVersion1.String202, 202);
            this.BinaryExchange = dictionary.CreateString(IdentityModelStringsVersion1.String203, 203);
            this.Lifetime = dictionary.CreateString(IdentityModelStringsVersion1.String204, 204);
            this.RequestedSecurityToken = dictionary.CreateString(IdentityModelStringsVersion1.String205, 205);
            this.Entropy = dictionary.CreateString(IdentityModelStringsVersion1.String206, 206);
            this.RequestedProofToken = dictionary.CreateString(IdentityModelStringsVersion1.String207, 207);
            this.ComputedKey = dictionary.CreateString(IdentityModelStringsVersion1.String208, 208);
            this.RequestSecurityToken = dictionary.CreateString(IdentityModelStringsVersion1.String209, 209);
            this.RequestType = dictionary.CreateString(IdentityModelStringsVersion1.String210, 210);
            this.Context = dictionary.CreateString(IdentityModelStringsVersion1.String211, 211);
            this.BinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String212, 212);
            this.Type = dictionary.CreateString(IdentityModelStringsVersion1.String83, 83);
            this.SpnegoValueTypeUri = dictionary.CreateString(IdentityModelStringsVersion1.String213, 213);
            this.TlsnegoValueTypeUri = dictionary.CreateString(IdentityModelStringsVersion1.String214, 214);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String215, 215);
            this.RequestSecurityTokenIssuance = dictionary.CreateString(IdentityModelStringsVersion1.String216, 216);
            this.RequestSecurityTokenIssuanceResponse = dictionary.CreateString(IdentityModelStringsVersion1.String217, 217);
            this.RequestTypeIssue = dictionary.CreateString(IdentityModelStringsVersion1.String218, 218);
            this.SymmetricKeyBinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String219, 219);
            this.Psha1ComputedKeyUri = dictionary.CreateString(IdentityModelStringsVersion1.String220, 220);
            this.NonceBinarySecret = dictionary.CreateString(IdentityModelStringsVersion1.String221, 221);
            this.RenewTarget = dictionary.CreateString(IdentityModelStringsVersion1.String222, 222);
            this.CloseTarget = dictionary.CreateString(IdentityModelStringsVersion1.String223, 223);
            this.RequestedTokenClosed = dictionary.CreateString(IdentityModelStringsVersion1.String224, 224);
            this.RequestedAttachedReference = dictionary.CreateString(IdentityModelStringsVersion1.String225, 225);
            this.RequestedUnattachedReference = dictionary.CreateString(IdentityModelStringsVersion1.String226, 226);
            this.IssuedTokensHeader = dictionary.CreateString(IdentityModelStringsVersion1.String227, 227);
            this.RequestTypeRenew = dictionary.CreateString(IdentityModelStringsVersion1.String228, 228);
            this.RequestTypeClose = dictionary.CreateString(IdentityModelStringsVersion1.String229, 229);
            this.KeyType = dictionary.CreateString(IdentityModelStringsVersion1.String230, 230);
            this.SymmetricKeyType = dictionary.CreateString(IdentityModelStringsVersion1.String219, 219);
            this.PublicKeyType = dictionary.CreateString(IdentityModelStringsVersion1.String231, 231);
            this.Claims = dictionary.CreateString(IdentityModelStringsVersion1.String232, 232);
            this.InvalidRequestFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String233, 233);
            this.FailedAuthenticationFaultCode = dictionary.CreateString(IdentityModelStringsVersion1.String136, 136);
            this.UseKey = dictionary.CreateString(IdentityModelStringsVersion1.String234, 234);
            this.SignWith = dictionary.CreateString(IdentityModelStringsVersion1.String235, 235);
            this.EncryptWith = dictionary.CreateString(IdentityModelStringsVersion1.String236, 236);
            this.EncryptionAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String237, 237);
            this.CanonicalizationAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String238, 238);
            this.ComputedKeyAlgorithm = dictionary.CreateString(IdentityModelStringsVersion1.String239, 239);
        }

        public TrustFeb2005Dictionary(IXmlDictionary dictionary) : base(dictionary)
        {
            this.RequestSecurityTokenResponseCollection = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String193);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String194);
            this.BinarySecretClauseType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String195);
            this.CombinedHashLabel = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String196);
            this.RequestSecurityTokenResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String197);
            this.TokenType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String147);
            this.KeySize = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String198);
            this.RequestedTokenReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String199);
            this.AppliesTo = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String200);
            this.Authenticator = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String201);
            this.CombinedHash = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String202);
            this.BinaryExchange = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String203);
            this.Lifetime = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String204);
            this.RequestedSecurityToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String205);
            this.Entropy = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String206);
            this.RequestedProofToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String207);
            this.ComputedKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String208);
            this.RequestSecurityToken = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String209);
            this.RequestType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String210);
            this.Context = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String211);
            this.BinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String212);
            this.Type = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String83);
            this.SpnegoValueTypeUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String213);
            this.TlsnegoValueTypeUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String214);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String215);
            this.RequestSecurityTokenIssuance = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String216);
            this.RequestSecurityTokenIssuanceResponse = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String217);
            this.RequestTypeIssue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String218);
            this.SymmetricKeyBinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String219);
            this.Psha1ComputedKeyUri = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String220);
            this.NonceBinarySecret = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String221);
            this.RenewTarget = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String222);
            this.CloseTarget = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String223);
            this.RequestedTokenClosed = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String224);
            this.RequestedAttachedReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String225);
            this.RequestedUnattachedReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String226);
            this.IssuedTokensHeader = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String227);
            this.RequestTypeRenew = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String228);
            this.RequestTypeClose = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String229);
            this.KeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String230);
            this.SymmetricKeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String219);
            this.PublicKeyType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String231);
            this.Claims = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String232);
            this.InvalidRequestFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String233);
            this.FailedAuthenticationFaultCode = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String136);
            this.UseKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String234);
            this.SignWith = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String235);
            this.EncryptWith = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String236);
            this.EncryptionAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String237);
            this.CanonicalizationAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String238);
            this.ComputedKeyAlgorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String239);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class UtilityDictionary
    {
        public XmlDictionaryString IdAttribute;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Timestamp;
        public XmlDictionaryString CreatedElement;
        public XmlDictionaryString ExpiresElement;
        public XmlDictionaryString Prefix;

        public UtilityDictionary(IdentityModelDictionary dictionary)
        {
            this.IdAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String3, 3);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String16, 16);
            this.Timestamp = dictionary.CreateString(IdentityModelStringsVersion1.String17, 17);
            this.CreatedElement = dictionary.CreateString(IdentityModelStringsVersion1.String18, 18);
            this.ExpiresElement = dictionary.CreateString(IdentityModelStringsVersion1.String19, 19);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String81, 81);
        }

        public UtilityDictionary(IXmlDictionary dictionary)
        {
            this.IdAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String3);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String16);
            this.Timestamp = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String17);
            this.CreatedElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String18);
            this.ExpiresElement = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String19);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String81);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class XmlEncryptionDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString DataReference;
        public XmlDictionaryString EncryptedData;
        public XmlDictionaryString EncryptionMethod;
        public XmlDictionaryString CipherData;
        public XmlDictionaryString CipherValue;
        public XmlDictionaryString ReferenceList;
        public XmlDictionaryString Encoding;
        public XmlDictionaryString MimeType;
        public XmlDictionaryString Type;
        public XmlDictionaryString Id;
        public XmlDictionaryString CarriedKeyName;
        public XmlDictionaryString Recipient;
        public XmlDictionaryString EncryptedKey;
        public XmlDictionaryString URI;
        public XmlDictionaryString KeyReference;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString ElementType;
        public XmlDictionaryString ContentType;
        public XmlDictionaryString AlgorithmAttribute;

        public XmlEncryptionDictionary(IdentityModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String156, 156);
            this.DataReference = dictionary.CreateString(IdentityModelStringsVersion1.String157, 157);
            this.EncryptedData = dictionary.CreateString(IdentityModelStringsVersion1.String158, 158);
            this.EncryptionMethod = dictionary.CreateString(IdentityModelStringsVersion1.String159, 159);
            this.CipherData = dictionary.CreateString(IdentityModelStringsVersion1.String160, 160);
            this.CipherValue = dictionary.CreateString(IdentityModelStringsVersion1.String161, 161);
            this.ReferenceList = dictionary.CreateString(IdentityModelStringsVersion1.String162, 162);
            this.Encoding = dictionary.CreateString(IdentityModelStringsVersion1.String163, 163);
            this.MimeType = dictionary.CreateString(IdentityModelStringsVersion1.String164, 164);
            this.Type = dictionary.CreateString(IdentityModelStringsVersion1.String83, 83);
            this.Id = dictionary.CreateString(IdentityModelStringsVersion1.String3, 3);
            this.CarriedKeyName = dictionary.CreateString(IdentityModelStringsVersion1.String165, 165);
            this.Recipient = dictionary.CreateString(IdentityModelStringsVersion1.String166, 166);
            this.EncryptedKey = dictionary.CreateString(IdentityModelStringsVersion1.String167, 167);
            this.URI = dictionary.CreateString(IdentityModelStringsVersion1.String1, 1);
            this.KeyReference = dictionary.CreateString(IdentityModelStringsVersion1.String168, 168);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String169, 169);
            this.ElementType = dictionary.CreateString(IdentityModelStringsVersion1.String170, 170);
            this.ContentType = dictionary.CreateString(IdentityModelStringsVersion1.String171, 171);
            this.AlgorithmAttribute = dictionary.CreateString(IdentityModelStringsVersion1.String0, 0);
        }

        public XmlEncryptionDictionary(IXmlDictionary dictionary)
        {
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String156);
            this.DataReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String157);
            this.EncryptedData = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String158);
            this.EncryptionMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String159);
            this.CipherData = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String160);
            this.CipherValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String161);
            this.ReferenceList = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String162);
            this.Encoding = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String163);
            this.MimeType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String164);
            this.Type = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String83);
            this.Id = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String3);
            this.CarriedKeyName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String165);
            this.Recipient = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String166);
            this.EncryptedKey = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String167);
            this.URI = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String1);
            this.KeyReference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String168);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String169);
            this.ElementType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String170);
            this.ContentType = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String171);
            this.AlgorithmAttribute = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String0);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }

    class XmlSignatureDictionary
    {
        public XmlDictionaryString Algorithm;
        public XmlDictionaryString URI;
        public XmlDictionaryString Reference;
        public XmlDictionaryString Transforms;
        public XmlDictionaryString Transform;
        public XmlDictionaryString DigestMethod;
        public XmlDictionaryString DigestValue;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString EnvelopedSignature;
        public XmlDictionaryString KeyInfo;
        public XmlDictionaryString Signature;
        public XmlDictionaryString SignedInfo;
        public XmlDictionaryString CanonicalizationMethod;
        public XmlDictionaryString SignatureMethod;
        public XmlDictionaryString SignatureValue;
        public XmlDictionaryString KeyName;
        public XmlDictionaryString Type;
        public XmlDictionaryString MgmtData;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString KeyValue;
        public XmlDictionaryString RsaKeyValue;
        public XmlDictionaryString Modulus;
        public XmlDictionaryString Exponent;
        public XmlDictionaryString X509Data;
        public XmlDictionaryString X509IssuerSerial;
        public XmlDictionaryString X509IssuerName;
        public XmlDictionaryString X509SerialNumber;
        public XmlDictionaryString X509Certificate;

        public XmlSignatureDictionary(IdentityModelDictionary dictionary)
        {
            this.Algorithm = dictionary.CreateString(IdentityModelStringsVersion1.String0, 0);
            this.URI = dictionary.CreateString(IdentityModelStringsVersion1.String1, 1);
            this.Reference = dictionary.CreateString(IdentityModelStringsVersion1.String2, 2);
            this.Transforms = dictionary.CreateString(IdentityModelStringsVersion1.String4, 4);
            this.Transform = dictionary.CreateString(IdentityModelStringsVersion1.String5, 5);
            this.DigestMethod = dictionary.CreateString(IdentityModelStringsVersion1.String6, 6);
            this.DigestValue = dictionary.CreateString(IdentityModelStringsVersion1.String7, 7);
            this.Namespace = dictionary.CreateString(IdentityModelStringsVersion1.String8, 8);
            this.EnvelopedSignature = dictionary.CreateString(IdentityModelStringsVersion1.String9, 9);
            this.KeyInfo = dictionary.CreateString(IdentityModelStringsVersion1.String10, 10);
            this.Signature = dictionary.CreateString(IdentityModelStringsVersion1.String11, 11);
            this.SignedInfo = dictionary.CreateString(IdentityModelStringsVersion1.String12, 12);
            this.CanonicalizationMethod = dictionary.CreateString(IdentityModelStringsVersion1.String13, 13);
            this.SignatureMethod = dictionary.CreateString(IdentityModelStringsVersion1.String14, 14);
            this.SignatureValue = dictionary.CreateString(IdentityModelStringsVersion1.String15, 15);
            this.KeyName = dictionary.CreateString(IdentityModelStringsVersion1.String82, 82);
            this.Type = dictionary.CreateString(IdentityModelStringsVersion1.String83, 83);
            this.MgmtData = dictionary.CreateString(IdentityModelStringsVersion1.String84, 84);
            this.Prefix = dictionary.CreateString(IdentityModelStringsVersion1.String85, 85);
            this.KeyValue = dictionary.CreateString(IdentityModelStringsVersion1.String86, 86);
            this.RsaKeyValue = dictionary.CreateString(IdentityModelStringsVersion1.String87, 87);
            this.Modulus = dictionary.CreateString(IdentityModelStringsVersion1.String88, 88);
            this.Exponent = dictionary.CreateString(IdentityModelStringsVersion1.String89, 89);
            this.X509Data = dictionary.CreateString(IdentityModelStringsVersion1.String90, 90);
            this.X509IssuerSerial = dictionary.CreateString(IdentityModelStringsVersion1.String91, 91);
            this.X509IssuerName = dictionary.CreateString(IdentityModelStringsVersion1.String92, 92);
            this.X509SerialNumber = dictionary.CreateString(IdentityModelStringsVersion1.String93, 93);
            this.X509Certificate = dictionary.CreateString(IdentityModelStringsVersion1.String94, 94);
        }

        public XmlSignatureDictionary(IXmlDictionary dictionary)
        {
            this.Algorithm = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String0);
            this.URI = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String1);
            this.Reference = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String2);
            this.Transforms = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String4);
            this.Transform = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String5);
            this.DigestMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String6);
            this.DigestValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String7);
            this.Namespace = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String8);
            this.EnvelopedSignature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String9);
            this.KeyInfo = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String10);
            this.Signature = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String11);
            this.SignedInfo = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String12);
            this.CanonicalizationMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String13);
            this.SignatureMethod = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String14);
            this.SignatureValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String15);
            this.KeyName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String82);
            this.Type = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String83);
            this.MgmtData = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String84);
            this.Prefix = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String85);
            this.KeyValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String86);
            this.RsaKeyValue = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String87);
            this.Modulus = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String88);
            this.Exponent = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String89);
            this.X509Data = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String90);
            this.X509IssuerSerial = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String91);
            this.X509IssuerName = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String92);
            this.X509SerialNumber = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String93);
            this.X509Certificate = LookupDictionaryString(dictionary, IdentityModelStringsVersion1.String94);
        }

        XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString expectedValue;
            if (!dictionary.TryLookup(value, out expectedValue))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.XDCannotFindValueInDictionaryString, value));

            return expectedValue;
        }
    }


    static class ExclusiveC14NStrings
    {
        // Main dictionary strings
        public const string Namespace = IdentityModelStringsVersion1.String20;
        public const string PrefixList = IdentityModelStringsVersion1.String21;
        public const string InclusiveNamespaces = IdentityModelStringsVersion1.String22;
        public const string Prefix = IdentityModelStringsVersion1.String23;
    }

    static class SamlStrings
    {
        // Main dictionary strings
        public const string Access = IdentityModelStringsVersion1.String24;
        public const string AccessDecision = IdentityModelStringsVersion1.String25;
        public const string Action = IdentityModelStringsVersion1.String26;
        public const string Advice = IdentityModelStringsVersion1.String27;
        public const string Assertion = IdentityModelStringsVersion1.String28;
        public const string AssertionId = IdentityModelStringsVersion1.String29;
        public const string AssertionIdReference = IdentityModelStringsVersion1.String30;
        public const string Attribute = IdentityModelStringsVersion1.String31;
        public const string AttributeName = IdentityModelStringsVersion1.String32;
        public const string AttributeNamespace = IdentityModelStringsVersion1.String33;
        public const string AttributeStatement = IdentityModelStringsVersion1.String34;
        public const string AttributeValue = IdentityModelStringsVersion1.String35;
        public const string Audience = IdentityModelStringsVersion1.String36;
        public const string AudienceRestrictionCondition = IdentityModelStringsVersion1.String37;
        public const string AuthenticationInstant = IdentityModelStringsVersion1.String38;
        public const string AuthenticationMethod = IdentityModelStringsVersion1.String39;
        public const string AuthenticationStatement = IdentityModelStringsVersion1.String40;
        public const string AuthorityBinding = IdentityModelStringsVersion1.String41;
        public const string AuthorityKind = IdentityModelStringsVersion1.String42;
        public const string AuthorizationDecisionStatement = IdentityModelStringsVersion1.String43;
        public const string Binding = IdentityModelStringsVersion1.String44;
        public const string Condition = IdentityModelStringsVersion1.String45;
        public const string Conditions = IdentityModelStringsVersion1.String46;
        public const string Decision = IdentityModelStringsVersion1.String47;
        public const string DoNotCacheCondition = IdentityModelStringsVersion1.String48;
        public const string Evidence = IdentityModelStringsVersion1.String49;
        public const string IssueInstant = IdentityModelStringsVersion1.String50;
        public const string Issuer = IdentityModelStringsVersion1.String51;
        public const string Location = IdentityModelStringsVersion1.String52;
        public const string MajorVersion = IdentityModelStringsVersion1.String53;
        public const string MinorVersion = IdentityModelStringsVersion1.String54;
        public const string Namespace = IdentityModelStringsVersion1.String55;
        public const string NameIdentifier = IdentityModelStringsVersion1.String56;
        public const string NameIdentifierFormat = IdentityModelStringsVersion1.String57;
        public const string NameIdentifierNameQualifier = IdentityModelStringsVersion1.String58;
        public const string ActionNamespaceAttribute = IdentityModelStringsVersion1.String59;
        public const string NotBefore = IdentityModelStringsVersion1.String60;
        public const string NotOnOrAfter = IdentityModelStringsVersion1.String61;
        public const string PreferredPrefix = IdentityModelStringsVersion1.String62;
        public const string Statement = IdentityModelStringsVersion1.String63;
        public const string Subject = IdentityModelStringsVersion1.String64;
        public const string SubjectConfirmation = IdentityModelStringsVersion1.String65;
        public const string SubjectConfirmationData = IdentityModelStringsVersion1.String66;
        public const string SubjectConfirmationMethod = IdentityModelStringsVersion1.String67;
        public const string HolderOfKey = IdentityModelStringsVersion1.String68;
        public const string SenderVouches = IdentityModelStringsVersion1.String69;
        public const string SubjectLocality = IdentityModelStringsVersion1.String70;
        public const string SubjectLocalityDNSAddress = IdentityModelStringsVersion1.String71;
        public const string SubjectLocalityIPAddress = IdentityModelStringsVersion1.String72;
        public const string SubjectStatement = IdentityModelStringsVersion1.String73;
        public const string UnspecifiedAuthenticationMethod = IdentityModelStringsVersion1.String74;
        public const string NamespaceAttributePrefix = IdentityModelStringsVersion1.String75;
        public const string Resource = IdentityModelStringsVersion1.String76;
        public const string UserName = IdentityModelStringsVersion1.String77;
        public const string UserNameNamespace = IdentityModelStringsVersion1.String78;
        public const string EmailName = IdentityModelStringsVersion1.String79;
        public const string EmailNamespace = IdentityModelStringsVersion1.String80;
    }

    static class SecureConversationStrings
    {
    }

    static class SecureConversationDec2005Strings
    {
        // Main dictionary strings
        public const string SecurityContextToken = IdentityModelStringsVersion1.String175;
        public const string AlgorithmAttribute = IdentityModelStringsVersion1.String0;
        public const string Generation = IdentityModelStringsVersion1.String176;
        public const string Label = IdentityModelStringsVersion1.String177;
        public const string Offset = IdentityModelStringsVersion1.String178;
        public const string Properties = IdentityModelStringsVersion1.String179;
        public const string Identifier = IdentityModelStringsVersion1.String180;
        public const string Cookie = IdentityModelStringsVersion1.String181;
        public const string RenewNeededFaultCode = IdentityModelStringsVersion1.String182;
        public const string BadContextTokenFaultCode = IdentityModelStringsVersion1.String183;
        public const string Prefix = IdentityModelStringsVersion1.String268;
        public const string DerivedKeyTokenType = IdentityModelStringsVersion1.String269;
        public const string SecurityContextTokenType = IdentityModelStringsVersion1.String270;
        public const string SecurityContextTokenReferenceValueType = IdentityModelStringsVersion1.String270;
        public const string RequestSecurityContextIssuance = IdentityModelStringsVersion1.String271;
        public const string RequestSecurityContextIssuanceResponse = IdentityModelStringsVersion1.String272;
        public const string RequestSecurityContextRenew = IdentityModelStringsVersion1.String273;
        public const string RequestSecurityContextRenewResponse = IdentityModelStringsVersion1.String274;
        public const string RequestSecurityContextClose = IdentityModelStringsVersion1.String275;
        public const string RequestSecurityContextCloseResponse = IdentityModelStringsVersion1.String276;
        public const string Namespace = IdentityModelStringsVersion1.String277;
        public const string DerivedKeyToken = IdentityModelStringsVersion1.String173;
        public const string Nonce = IdentityModelStringsVersion1.String120;
        public const string Length = IdentityModelStringsVersion1.String174;
        public const string Instance = IdentityModelStringsVersion1.String278;
    }

    static class SecureConversationFeb2005Strings
    {
        // Main dictionary strings
        public const string Namespace = IdentityModelStringsVersion1.String172;
        public const string DerivedKeyToken = IdentityModelStringsVersion1.String173;
        public const string Nonce = IdentityModelStringsVersion1.String120;
        public const string Length = IdentityModelStringsVersion1.String174;
        public const string SecurityContextToken = IdentityModelStringsVersion1.String175;
        public const string AlgorithmAttribute = IdentityModelStringsVersion1.String0;
        public const string Generation = IdentityModelStringsVersion1.String176;
        public const string Label = IdentityModelStringsVersion1.String177;
        public const string Offset = IdentityModelStringsVersion1.String178;
        public const string Properties = IdentityModelStringsVersion1.String179;
        public const string Identifier = IdentityModelStringsVersion1.String180;
        public const string Cookie = IdentityModelStringsVersion1.String181;
        public const string RenewNeededFaultCode = IdentityModelStringsVersion1.String182;
        public const string BadContextTokenFaultCode = IdentityModelStringsVersion1.String183;
        public const string Prefix = IdentityModelStringsVersion1.String184;
        public const string DerivedKeyTokenType = IdentityModelStringsVersion1.String185;
        public const string SecurityContextTokenType = IdentityModelStringsVersion1.String186;
        public const string SecurityContextTokenReferenceValueType = IdentityModelStringsVersion1.String186;
        public const string RequestSecurityContextIssuance = IdentityModelStringsVersion1.String187;
        public const string RequestSecurityContextIssuanceResponse = IdentityModelStringsVersion1.String188;
        public const string RequestSecurityContextRenew = IdentityModelStringsVersion1.String189;
        public const string RequestSecurityContextRenewResponse = IdentityModelStringsVersion1.String190;
        public const string RequestSecurityContextClose = IdentityModelStringsVersion1.String191;
        public const string RequestSecurityContextCloseResponse = IdentityModelStringsVersion1.String192;
    }

    static class SecurityAlgorithmStrings
    {
        // Main dictionary strings
        public const string Aes128Encryption = IdentityModelStringsVersion1.String95;
        public const string Aes128KeyWrap = IdentityModelStringsVersion1.String96;
        public const string Aes192Encryption = IdentityModelStringsVersion1.String97;
        public const string Aes192KeyWrap = IdentityModelStringsVersion1.String98;
        public const string Aes256Encryption = IdentityModelStringsVersion1.String99;
        public const string Aes256KeyWrap = IdentityModelStringsVersion1.String100;
        public const string DesEncryption = IdentityModelStringsVersion1.String101;
        public const string DsaSha1Signature = IdentityModelStringsVersion1.String102;
        public const string ExclusiveC14n = IdentityModelStringsVersion1.String20;
        public const string ExclusiveC14nWithComments = IdentityModelStringsVersion1.String103;
        public const string HmacSha1Signature = IdentityModelStringsVersion1.String104;
        public const string HmacSha256Signature = IdentityModelStringsVersion1.String105;
        public const string Psha1KeyDerivation = IdentityModelStringsVersion1.String106;
        public const string Ripemd160Digest = IdentityModelStringsVersion1.String107;
        public const string RsaOaepKeyWrap = IdentityModelStringsVersion1.String108;
        public const string RsaSha1Signature = IdentityModelStringsVersion1.String109;
        public const string RsaSha256Signature = IdentityModelStringsVersion1.String110;
        public const string RsaV15KeyWrap = IdentityModelStringsVersion1.String111;
        public const string Sha1Digest = IdentityModelStringsVersion1.String112;
        public const string Sha256Digest = IdentityModelStringsVersion1.String113;
        public const string Sha512Digest = IdentityModelStringsVersion1.String114;
        public const string TripleDesEncryption = IdentityModelStringsVersion1.String115;
        public const string TripleDesKeyWrap = IdentityModelStringsVersion1.String116;
        public const string TlsSspiKeyWrap = IdentityModelStringsVersion1.String117;
        public const string WindowsSspiKeyWrap = IdentityModelStringsVersion1.String118;
        // String constants
        public const string StrTransform = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#STR-Transform";
    }

    static class SecurityAlgorithmDec2005Strings
    {
        // Main dictionary strings
        public const string Psha1KeyDerivationDec2005 = IdentityModelStringsVersion1.String267;
    }

    static class SecurityJan2004Strings
    {
        // Main dictionary strings
        public const string Prefix = IdentityModelStringsVersion1.String119;
        public const string NonceElement = IdentityModelStringsVersion1.String120;
        public const string PasswordElement = IdentityModelStringsVersion1.String121;
        public const string PasswordTextName = IdentityModelStringsVersion1.String122;
        public const string UserNameElement = IdentityModelStringsVersion1.String123;
        public const string UserNameTokenElement = IdentityModelStringsVersion1.String124;
        public const string BinarySecurityToken = IdentityModelStringsVersion1.String125;
        public const string EncodingType = IdentityModelStringsVersion1.String126;
        public const string Reference = IdentityModelStringsVersion1.String2;
        public const string URI = IdentityModelStringsVersion1.String1;
        public const string KeyIdentifier = IdentityModelStringsVersion1.String127;
        public const string EncodingTypeValueBase64Binary = IdentityModelStringsVersion1.String128;
        public const string EncodingTypeValueHexBinary = IdentityModelStringsVersion1.String129;
        public const string EncodingTypeValueText = IdentityModelStringsVersion1.String130;
        public const string X509SKIValueType = IdentityModelStringsVersion1.String131;
        public const string KerberosTokenTypeGSS = IdentityModelStringsVersion1.String132;
        public const string KerberosTokenType1510 = IdentityModelStringsVersion1.String133;
        public const string SamlAssertionIdValueType = IdentityModelStringsVersion1.String134;
        public const string SamlAssertion = IdentityModelStringsVersion1.String28;
        public const string SamlUri = IdentityModelStringsVersion1.String55;
        public const string RelAssertionValueType = IdentityModelStringsVersion1.String135;
        public const string FailedAuthenticationFaultCode = IdentityModelStringsVersion1.String136;
        public const string InvalidSecurityTokenFaultCode = IdentityModelStringsVersion1.String137;
        public const string InvalidSecurityFaultCode = IdentityModelStringsVersion1.String138;
        public const string SecurityTokenReference = IdentityModelStringsVersion1.String139;
        public const string Namespace = IdentityModelStringsVersion1.String140;
        public const string Security = IdentityModelStringsVersion1.String141;
        public const string ValueType = IdentityModelStringsVersion1.String142;
        public const string TypeAttribute = IdentityModelStringsVersion1.String83;
        public const string KerberosHashValueType = IdentityModelStringsVersion1.String143;
        // String constants
        public const string SecurityProfileNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0";
        public const string X509TokenProfileNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0";
        public const string UPTokenProfileNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0";
        public const string SamlTokenProfileNamespace = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0";
        public const string KerberosTokenProfileNamespace = "http://www.docs.oasis-open.org/wss/2004/07/oasis-000000-wss-kerberos-token-profile-1.0";
        public const string UPTokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#UsernameToken";
        public const string X509TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
        public const string UPTokenPasswordTextValue = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
    }

    static class SecurityXXX2005Strings
    {
        // Main dictionary strings
        public const string Prefix = IdentityModelStringsVersion1.String144;
        public const string SignatureConfirmation = IdentityModelStringsVersion1.String145;
        public const string ValueAttribute = IdentityModelStringsVersion1.String146;
        public const string TokenTypeAttribute = IdentityModelStringsVersion1.String147;
        public const string ThumbprintSha1ValueType = IdentityModelStringsVersion1.String148;
        public const string EncryptedKeyTokenType = IdentityModelStringsVersion1.String149;
        public const string EncryptedKeyHashValueType = IdentityModelStringsVersion1.String150;
        public const string SamlTokenType = IdentityModelStringsVersion1.String151;
        public const string Saml20TokenType = IdentityModelStringsVersion1.String152;
        public const string Saml11AssertionValueType = IdentityModelStringsVersion1.String153;
        public const string EncryptedHeader = IdentityModelStringsVersion1.String154;
        public const string Namespace = IdentityModelStringsVersion1.String155;
        // String constants
        public const string SecurityProfileNamespace = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1";
        public const string SamlTokenProfileNamespace = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1";
        public const string X509TokenProfileNamespace = "http://docs.oasis-open.org/wss/2004/xx/oasis-2004xx-wss-x509-token-profile-1.1";
    }

    static class TrustStrings
    {
    }

    static class TrustDec2005Strings
    {
        // Main dictionary strings
        public const string CombinedHashLabel = IdentityModelStringsVersion1.String196;
        public const string RequestSecurityTokenResponse = IdentityModelStringsVersion1.String197;
        public const string TokenType = IdentityModelStringsVersion1.String147;
        public const string KeySize = IdentityModelStringsVersion1.String198;
        public const string RequestedTokenReference = IdentityModelStringsVersion1.String199;
        public const string AppliesTo = IdentityModelStringsVersion1.String200;
        public const string Authenticator = IdentityModelStringsVersion1.String201;
        public const string CombinedHash = IdentityModelStringsVersion1.String202;
        public const string BinaryExchange = IdentityModelStringsVersion1.String203;
        public const string Lifetime = IdentityModelStringsVersion1.String204;
        public const string RequestedSecurityToken = IdentityModelStringsVersion1.String205;
        public const string Entropy = IdentityModelStringsVersion1.String206;
        public const string RequestedProofToken = IdentityModelStringsVersion1.String207;
        public const string ComputedKey = IdentityModelStringsVersion1.String208;
        public const string RequestSecurityToken = IdentityModelStringsVersion1.String209;
        public const string RequestType = IdentityModelStringsVersion1.String210;
        public const string Context = IdentityModelStringsVersion1.String211;
        public const string BinarySecret = IdentityModelStringsVersion1.String212;
        public const string Type = IdentityModelStringsVersion1.String83;
        public const string SpnegoValueTypeUri = IdentityModelStringsVersion1.String240;
        public const string TlsnegoValueTypeUri = IdentityModelStringsVersion1.String241;
        public const string Prefix = IdentityModelStringsVersion1.String242;
        public const string RequestSecurityTokenIssuance = IdentityModelStringsVersion1.String243;
        public const string RequestSecurityTokenIssuanceResponse = IdentityModelStringsVersion1.String244;
        public const string RequestTypeIssue = IdentityModelStringsVersion1.String245;
        public const string AsymmetricKeyBinarySecret = IdentityModelStringsVersion1.String246;
        public const string SymmetricKeyBinarySecret = IdentityModelStringsVersion1.String247;
        public const string NonceBinarySecret = IdentityModelStringsVersion1.String248;
        public const string Psha1ComputedKeyUri = IdentityModelStringsVersion1.String249;
        public const string KeyType = IdentityModelStringsVersion1.String230;
        public const string SymmetricKeyType = IdentityModelStringsVersion1.String247;
        public const string PublicKeyType = IdentityModelStringsVersion1.String250;
        public const string Claims = IdentityModelStringsVersion1.String232;
        public const string InvalidRequestFaultCode = IdentityModelStringsVersion1.String233;
        public const string FailedAuthenticationFaultCode = IdentityModelStringsVersion1.String136;
        public const string UseKey = IdentityModelStringsVersion1.String234;
        public const string SignWith = IdentityModelStringsVersion1.String235;
        public const string EncryptWith = IdentityModelStringsVersion1.String236;
        public const string EncryptionAlgorithm = IdentityModelStringsVersion1.String237;
        public const string CanonicalizationAlgorithm = IdentityModelStringsVersion1.String238;
        public const string ComputedKeyAlgorithm = IdentityModelStringsVersion1.String239;
        public const string RequestSecurityTokenResponseCollection = IdentityModelStringsVersion1.String193;
        public const string Namespace = IdentityModelStringsVersion1.String251;
        public const string BinarySecretClauseType = IdentityModelStringsVersion1.String252;
        public const string RequestSecurityTokenCollectionIssuanceFinalResponse = IdentityModelStringsVersion1.String253;
        public const string RequestSecurityTokenRenewal = IdentityModelStringsVersion1.String254;
        public const string RequestSecurityTokenRenewalResponse = IdentityModelStringsVersion1.String255;
        public const string RequestSecurityTokenCollectionRenewalFinalResponse = IdentityModelStringsVersion1.String256;
        public const string RequestSecurityTokenCancellation = IdentityModelStringsVersion1.String257;
        public const string RequestSecurityTokenCancellationResponse = IdentityModelStringsVersion1.String258;
        public const string RequestSecurityTokenCollectionCancellationFinalResponse = IdentityModelStringsVersion1.String259;
        public const string RequestTypeRenew = IdentityModelStringsVersion1.String260;
        public const string RequestTypeClose = IdentityModelStringsVersion1.String261;
        public const string RenewTarget = IdentityModelStringsVersion1.String222;
        public const string CloseTarget = IdentityModelStringsVersion1.String223;
        public const string RequestedTokenClosed = IdentityModelStringsVersion1.String224;
        public const string RequestedAttachedReference = IdentityModelStringsVersion1.String225;
        public const string RequestedUnattachedReference = IdentityModelStringsVersion1.String226;
        public const string IssuedTokensHeader = IdentityModelStringsVersion1.String227;
        public const string KeyWrapAlgorithm = IdentityModelStringsVersion1.String262;
        public const string BearerKeyType = IdentityModelStringsVersion1.String263;
        public const string SecondaryParameters = IdentityModelStringsVersion1.String264;
        public const string Dialect = IdentityModelStringsVersion1.String265;
        public const string DialectType = IdentityModelStringsVersion1.String266;
    }

    static class TrustFeb2005Strings
    {
        // Main dictionary strings
        public const string RequestSecurityTokenResponseCollection = IdentityModelStringsVersion1.String193;
        public const string Namespace = IdentityModelStringsVersion1.String194;
        public const string BinarySecretClauseType = IdentityModelStringsVersion1.String195;
        public const string CombinedHashLabel = IdentityModelStringsVersion1.String196;
        public const string RequestSecurityTokenResponse = IdentityModelStringsVersion1.String197;
        public const string TokenType = IdentityModelStringsVersion1.String147;
        public const string KeySize = IdentityModelStringsVersion1.String198;
        public const string RequestedTokenReference = IdentityModelStringsVersion1.String199;
        public const string AppliesTo = IdentityModelStringsVersion1.String200;
        public const string Authenticator = IdentityModelStringsVersion1.String201;
        public const string CombinedHash = IdentityModelStringsVersion1.String202;
        public const string BinaryExchange = IdentityModelStringsVersion1.String203;
        public const string Lifetime = IdentityModelStringsVersion1.String204;
        public const string RequestedSecurityToken = IdentityModelStringsVersion1.String205;
        public const string Entropy = IdentityModelStringsVersion1.String206;
        public const string RequestedProofToken = IdentityModelStringsVersion1.String207;
        public const string ComputedKey = IdentityModelStringsVersion1.String208;
        public const string RequestSecurityToken = IdentityModelStringsVersion1.String209;
        public const string RequestType = IdentityModelStringsVersion1.String210;
        public const string Context = IdentityModelStringsVersion1.String211;
        public const string BinarySecret = IdentityModelStringsVersion1.String212;
        public const string Type = IdentityModelStringsVersion1.String83;
        public const string SpnegoValueTypeUri = IdentityModelStringsVersion1.String213;
        public const string TlsnegoValueTypeUri = IdentityModelStringsVersion1.String214;
        public const string Prefix = IdentityModelStringsVersion1.String215;
        public const string RequestSecurityTokenIssuance = IdentityModelStringsVersion1.String216;
        public const string RequestSecurityTokenIssuanceResponse = IdentityModelStringsVersion1.String217;
        public const string RequestTypeIssue = IdentityModelStringsVersion1.String218;
        public const string SymmetricKeyBinarySecret = IdentityModelStringsVersion1.String219;
        public const string Psha1ComputedKeyUri = IdentityModelStringsVersion1.String220;
        public const string NonceBinarySecret = IdentityModelStringsVersion1.String221;
        public const string RenewTarget = IdentityModelStringsVersion1.String222;
        public const string CloseTarget = IdentityModelStringsVersion1.String223;
        public const string RequestedTokenClosed = IdentityModelStringsVersion1.String224;
        public const string RequestedAttachedReference = IdentityModelStringsVersion1.String225;
        public const string RequestedUnattachedReference = IdentityModelStringsVersion1.String226;
        public const string IssuedTokensHeader = IdentityModelStringsVersion1.String227;
        public const string RequestTypeRenew = IdentityModelStringsVersion1.String228;
        public const string RequestTypeClose = IdentityModelStringsVersion1.String229;
        public const string KeyType = IdentityModelStringsVersion1.String230;
        public const string SymmetricKeyType = IdentityModelStringsVersion1.String219;
        public const string PublicKeyType = IdentityModelStringsVersion1.String231;
        public const string Claims = IdentityModelStringsVersion1.String232;
        public const string InvalidRequestFaultCode = IdentityModelStringsVersion1.String233;
        public const string FailedAuthenticationFaultCode = IdentityModelStringsVersion1.String136;
        public const string UseKey = IdentityModelStringsVersion1.String234;
        public const string SignWith = IdentityModelStringsVersion1.String235;
        public const string EncryptWith = IdentityModelStringsVersion1.String236;
        public const string EncryptionAlgorithm = IdentityModelStringsVersion1.String237;
        public const string CanonicalizationAlgorithm = IdentityModelStringsVersion1.String238;
        public const string ComputedKeyAlgorithm = IdentityModelStringsVersion1.String239;
    }

    static class UtilityStrings
    {
        // Main dictionary strings
        public const string IdAttribute = IdentityModelStringsVersion1.String3;
        public const string Namespace = IdentityModelStringsVersion1.String16;
        public const string Timestamp = IdentityModelStringsVersion1.String17;
        public const string CreatedElement = IdentityModelStringsVersion1.String18;
        public const string ExpiresElement = IdentityModelStringsVersion1.String19;
        public const string Prefix = IdentityModelStringsVersion1.String81;
    }

    static class XmlEncryptionStrings
    {
        // Main dictionary strings
        public const string Namespace = IdentityModelStringsVersion1.String156;
        public const string DataReference = IdentityModelStringsVersion1.String157;
        public const string EncryptedData = IdentityModelStringsVersion1.String158;
        public const string EncryptionMethod = IdentityModelStringsVersion1.String159;
        public const string CipherData = IdentityModelStringsVersion1.String160;
        public const string CipherValue = IdentityModelStringsVersion1.String161;
        public const string ReferenceList = IdentityModelStringsVersion1.String162;
        public const string Encoding = IdentityModelStringsVersion1.String163;
        public const string MimeType = IdentityModelStringsVersion1.String164;
        public const string Type = IdentityModelStringsVersion1.String83;
        public const string Id = IdentityModelStringsVersion1.String3;
        public const string CarriedKeyName = IdentityModelStringsVersion1.String165;
        public const string Recipient = IdentityModelStringsVersion1.String166;
        public const string EncryptedKey = IdentityModelStringsVersion1.String167;
        public const string URI = IdentityModelStringsVersion1.String1;
        public const string KeyReference = IdentityModelStringsVersion1.String168;
        public const string Prefix = IdentityModelStringsVersion1.String169;
        public const string ElementType = IdentityModelStringsVersion1.String170;
        public const string ContentType = IdentityModelStringsVersion1.String171;
        public const string AlgorithmAttribute = IdentityModelStringsVersion1.String0;
    }

    static class XmlSignatureStrings
    {
        // Main dictionary strings
        public const string Algorithm = IdentityModelStringsVersion1.String0;
        public const string URI = IdentityModelStringsVersion1.String1;
        public const string Reference = IdentityModelStringsVersion1.String2;
        public const string Transforms = IdentityModelStringsVersion1.String4;
        public const string Transform = IdentityModelStringsVersion1.String5;
        public const string DigestMethod = IdentityModelStringsVersion1.String6;
        public const string DigestValue = IdentityModelStringsVersion1.String7;
        public const string Namespace = IdentityModelStringsVersion1.String8;
        public const string EnvelopedSignature = IdentityModelStringsVersion1.String9;
        public const string KeyInfo = IdentityModelStringsVersion1.String10;
        public const string Signature = IdentityModelStringsVersion1.String11;
        public const string SignedInfo = IdentityModelStringsVersion1.String12;
        public const string CanonicalizationMethod = IdentityModelStringsVersion1.String13;
        public const string SignatureMethod = IdentityModelStringsVersion1.String14;
        public const string SignatureValue = IdentityModelStringsVersion1.String15;
        public const string KeyName = IdentityModelStringsVersion1.String82;
        public const string Type = IdentityModelStringsVersion1.String83;
        public const string MgmtData = IdentityModelStringsVersion1.String84;
        public const string Prefix = IdentityModelStringsVersion1.String85;
        public const string KeyValue = IdentityModelStringsVersion1.String86;
        public const string RsaKeyValue = IdentityModelStringsVersion1.String87;
        public const string Modulus = IdentityModelStringsVersion1.String88;
        public const string Exponent = IdentityModelStringsVersion1.String89;
        public const string X509Data = IdentityModelStringsVersion1.String90;
        public const string X509IssuerSerial = IdentityModelStringsVersion1.String91;
        public const string X509IssuerName = IdentityModelStringsVersion1.String92;
        public const string X509SerialNumber = IdentityModelStringsVersion1.String93;
        public const string X509Certificate = IdentityModelStringsVersion1.String94;
        // String constants
        public const string SecurityJan2004Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string SecurityJan2004Prefix = "o";
        public const string X509Ski = "X509SKI";
        public const string TransformationParameters = "TransformationParameters";
    }
}
