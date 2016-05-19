//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Xml;
    
    public class SamlAttribute
    {
        string name;
        string nameSpace;
        readonly ImmutableCollection<string> attributeValues = new ImmutableCollection<string>();
        string originalIssuer;
        string attributeValueXsiType = System.Security.Claims.ClaimValueTypes.String;

        List<Claim> claims;
        string claimType;
        bool isReadOnly = false;

        public SamlAttribute()
        {
        }

        public SamlAttribute(string attributeNamespace, string attributeName, IEnumerable<string> attributeValues)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeNameAttributeRequired));

            if (string.IsNullOrEmpty(attributeNamespace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeNamespaceAttributeRequired));

            if (attributeValues == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeValues");

            this.name = StringUtil.OptimizeString(attributeName);
            this.nameSpace = StringUtil.OptimizeString(attributeNamespace);
            this.claimType = string.IsNullOrEmpty(this.nameSpace) ? this.name : this.nameSpace + "/" + this.name;

            foreach (string value in attributeValues)
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeValueCannotBeNull));

                this.attributeValues.Add(value);
            }

            if (this.attributeValues.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeShouldHaveOneValue));
        }

        public SamlAttribute(Claim claim)
        {
            if (claim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");

            if (!(claim.Resource is String))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SamlAttributeClaimResourceShouldBeAString));

            if (claim.Right != Rights.PossessProperty)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SamlAttributeClaimRightShouldBePossessProperty));

#pragma warning suppress 56506 // claim.CalimType can never be null.
            int lastSlashIndex = claim.ClaimType.LastIndexOf('/');
            if ((lastSlashIndex == -1) || (lastSlashIndex == 0) || (lastSlashIndex == claim.ClaimType.Length - 1))
            {
                this.nameSpace = String.Empty;
                this.name = claim.ClaimType;
            }
            else
            {
                this.nameSpace = StringUtil.OptimizeString(claim.ClaimType.Substring(0, lastSlashIndex));
                this.name = StringUtil.OptimizeString(claim.ClaimType.Substring(lastSlashIndex + 1, claim.ClaimType.Length - (lastSlashIndex + 1)));
            }
            this.claimType = claim.ClaimType;
            this.attributeValues.Add(claim.Resource as string);
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeNameAttributeRequired));

                this.name = StringUtil.OptimizeString(value);
            }
        }

        public string Namespace
        {
            get { return this.nameSpace; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeNamespaceAttributeRequired));

                this.nameSpace = StringUtil.OptimizeString(value);
            }
        }

        public IList<string> AttributeValues
        {
            get { return this.attributeValues; }
        }

        /// <summary>
        /// Gets or Sets the string that represents the OriginalIssuer of the SAML Attribute.
        /// </summary>
        public string OriginalIssuer
        {
            get { return this.originalIssuer; }
            set
            {
                if (value == String.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4251));
                }

                this.originalIssuer = StringUtil.OptimizeString(value);
            }
        }

        /// <summary>
        /// Gets or sets the xsi:type of the values contained in the SAML Attribute.
        /// </summary>
        public string AttributeValueXsiType
        {
            get { return attributeValueXsiType; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                int indexOfHash = value.IndexOf('#');
                if (indexOfHash == -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                string prefix = value.Substring(0, indexOfHash);
                if (prefix.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                string suffix = value.Substring(indexOfHash + 1);
                if (suffix.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                attributeValueXsiType = value;
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.attributeValues.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        public virtual ReadOnlyCollection<Claim> ExtractClaims()
        {
            if (this.claims == null)
            {
                List<Claim> tempClaims = new List<Claim>(this.attributeValues.Count);

                for (int i = 0; i < this.attributeValues.Count; i++)
                {
                    if (this.attributeValues[i] == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeValueCannotBeNull));

                    tempClaims.Add(new Claim(this.claimType, this.attributeValues[i], Rights.PossessProperty));
                }
                this.claims = tempClaims;
            }

            return this.claims.AsReadOnly();
        }

        void CheckObjectValidity()
        {
            if (string.IsNullOrEmpty(this.name))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeNameAttributeRequired)));

            if (string.IsNullOrEmpty(this.nameSpace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeNamespaceAttributeRequired)));

            if (this.attributeValues.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeShouldHaveOneValue)));
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            this.name = reader.GetAttribute(dictionary.AttributeName, null);
            if (string.IsNullOrEmpty(this.name))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeMissingNameAttributeOnRead)));

            this.nameSpace = reader.GetAttribute(dictionary.AttributeNamespace, null);
            if (string.IsNullOrEmpty(this.nameSpace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeMissingNamespaceAttributeOnRead)));

            this.claimType = string.IsNullOrEmpty(this.nameSpace) ? this.name : this.nameSpace + "/" + this.name;

            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement(dictionary.AttributeValue, dictionary.Namespace))
            {
                // We will load all Attributes as a string value by default.
                string attrValue = reader.ReadString();
                this.attributeValues.Add(attrValue);

                reader.MoveToContent();
                reader.ReadEndElement();
            }

            if (this.attributeValues.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAttributeShouldHaveOneValue)));

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Attribute, dictionary.Namespace);

            writer.WriteStartAttribute(dictionary.AttributeName, null);
            writer.WriteString(this.name);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(dictionary.AttributeNamespace, null);
            writer.WriteString(this.nameSpace);
            writer.WriteEndAttribute();

            for (int i = 0; i < this.attributeValues.Count; i++)
            {
                if (this.attributeValues[i] == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAttributeValueCannotBeNull));

                writer.WriteElementString(dictionary.PreferredPrefix.Value, dictionary.AttributeValue, dictionary.Namespace, this.attributeValues[i]);
            }

            writer.WriteEndElement();
        }

    }

}
