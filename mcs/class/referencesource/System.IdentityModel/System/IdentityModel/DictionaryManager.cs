//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class DictionaryManager
    {
        SamlDictionary samlDictionary;
        XmlSignatureDictionary sigantureDictionary;
        UtilityDictionary utilityDictionary;
        ExclusiveC14NDictionary exclusiveC14NDictionary;
        SecurityAlgorithmDec2005Dictionary securityAlgorithmDec2005Dictionary;
        SecurityAlgorithmDictionary securityAlgorithmDictionary;
        SecurityJan2004Dictionary securityJan2004Dictionary;
        SecurityXXX2005Dictionary securityJanXXX2005Dictionary;
        SecureConversationDec2005Dictionary secureConversationDec2005Dictionary;
        SecureConversationFeb2005Dictionary secureConversationFeb2005Dictionary;
        TrustFeb2005Dictionary trustFeb2005Dictionary;
        TrustDec2005Dictionary trustDec2005Dictionary;
        XmlEncryptionDictionary xmlEncryptionDictionary;
        IXmlDictionary parentDictionary;

        public DictionaryManager()
        {
            this.samlDictionary = XD.SamlDictionary;
            this.sigantureDictionary = XD.XmlSignatureDictionary;
            this.utilityDictionary = XD.UtilityDictionary;
            this.exclusiveC14NDictionary = XD.ExclusiveC14NDictionary;
            this.securityAlgorithmDictionary = XD.SecurityAlgorithmDictionary;
            this.parentDictionary = XD.Dictionary;
            this.securityJan2004Dictionary = XD.SecurityJan2004Dictionary;
            this.securityJanXXX2005Dictionary = XD.SecurityXXX2005Dictionary;
            this.secureConversationFeb2005Dictionary = XD.SecureConversationFeb2005Dictionary;
            this.trustFeb2005Dictionary = XD.TrustFeb2005Dictionary;
            this.xmlEncryptionDictionary = XD.XmlEncryptionDictionary;

            // These 3 are factored into a seperate dictionary in ServiceModel under DXD. 
            this.secureConversationDec2005Dictionary = XD.SecureConversationDec2005Dictionary;
            this.securityAlgorithmDec2005Dictionary = XD.SecurityAlgorithmDec2005Dictionary;
            this.trustDec2005Dictionary = XD.TrustDec2005Dictionary;
        }

        public DictionaryManager(IXmlDictionary parentDictionary)
        {
            this.samlDictionary = new SamlDictionary(parentDictionary);
            this.sigantureDictionary = new XmlSignatureDictionary(parentDictionary);
            this.utilityDictionary = new UtilityDictionary(parentDictionary);
            this.exclusiveC14NDictionary = new ExclusiveC14NDictionary(parentDictionary);
            this.securityAlgorithmDictionary = new SecurityAlgorithmDictionary(parentDictionary);
            this.securityJan2004Dictionary = new SecurityJan2004Dictionary(parentDictionary);
            this.securityJanXXX2005Dictionary = new SecurityXXX2005Dictionary(parentDictionary);
            this.secureConversationFeb2005Dictionary = new SecureConversationFeb2005Dictionary(parentDictionary);
            this.trustFeb2005Dictionary = new TrustFeb2005Dictionary(parentDictionary);
            this.xmlEncryptionDictionary = new XmlEncryptionDictionary(parentDictionary);
            this.parentDictionary = parentDictionary;

            // These 3 are factored into a seperate dictionary in ServiceModel under DXD. 
            // ServiceModel should set these seperately using the property setters.
            this.secureConversationDec2005Dictionary = XD.SecureConversationDec2005Dictionary;
            this.securityAlgorithmDec2005Dictionary = XD.SecurityAlgorithmDec2005Dictionary;
            this.trustDec2005Dictionary = XD.TrustDec2005Dictionary;
        }

        public SamlDictionary SamlDictionary
        {
            get { return this.samlDictionary; }
            set { this.samlDictionary = value; }
        }

        public XmlSignatureDictionary XmlSignatureDictionary
        {
            get { return this.sigantureDictionary; }
            set { this.sigantureDictionary = value; }
        }

        public UtilityDictionary UtilityDictionary
        {
            get { return this.utilityDictionary; }
            set { this.utilityDictionary = value; }
        }

        public ExclusiveC14NDictionary ExclusiveC14NDictionary
        {
            get { return this.exclusiveC14NDictionary; }
            set { this.exclusiveC14NDictionary = value; }
        }

        public SecurityAlgorithmDec2005Dictionary SecurityAlgorithmDec2005Dictionary
        {
            get { return this.securityAlgorithmDec2005Dictionary; }
            set { this.securityAlgorithmDec2005Dictionary = value; }
        }

        public SecurityAlgorithmDictionary SecurityAlgorithmDictionary
        {
            get { return this.securityAlgorithmDictionary; }
            set { this.securityAlgorithmDictionary = value; }
        }
 
        public SecurityJan2004Dictionary SecurityJan2004Dictionary
        {
            get { return this.securityJan2004Dictionary; }
            set { this.securityJan2004Dictionary = value; }
        }

        public SecurityXXX2005Dictionary SecurityJanXXX2005Dictionary
        {
            get { return this.securityJanXXX2005Dictionary; }
            set { this.securityJanXXX2005Dictionary = value; }
        }

        public SecureConversationDec2005Dictionary SecureConversationDec2005Dictionary
        {
            get { return this.secureConversationDec2005Dictionary; }
            set { this.secureConversationDec2005Dictionary = value; }
        }

        public SecureConversationFeb2005Dictionary SecureConversationFeb2005Dictionary
        {
            get { return this.secureConversationFeb2005Dictionary; }
            set { this.secureConversationFeb2005Dictionary = value; }
        }

        public TrustDec2005Dictionary TrustDec2005Dictionary
        {
            get { return this.trustDec2005Dictionary; }
            set { this.trustDec2005Dictionary = value; }
        }

        public TrustFeb2005Dictionary TrustFeb2005Dictionary
        {
            get { return this.trustFeb2005Dictionary; }
            set { this.trustFeb2005Dictionary = value; }
        }

        public XmlEncryptionDictionary XmlEncryptionDictionary
        {
            get { return this.xmlEncryptionDictionary; }
            set { this.xmlEncryptionDictionary = value; }
        }

        public IXmlDictionary ParentDictionary
        {
            get { return this.parentDictionary; }
            set { this.parentDictionary = value; }
        }
    }
}
