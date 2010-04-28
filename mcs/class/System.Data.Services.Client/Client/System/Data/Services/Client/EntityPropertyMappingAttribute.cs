//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Common
{
    using System;
    using System.Data.Services.Client;

    public enum SyndicationItemProperty
    {
        CustomProperty,

        AuthorEmail,

        AuthorName,
        AuthorUri,

        ContributorEmail,

        ContributorName,
        ContributorUri,

        Updated,


        Published,

        Rights,

        Summary,

        Title
    }

    public enum SyndicationTextContentKind
    {
        Plaintext,

        Html,

        Xhtml
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class EntityPropertyMappingAttribute : Attribute
    {
#region Private Members

        private const string AtomNamespacePrefix = "atom";

        private readonly String sourcePath;

        private readonly String targetPath;

        private readonly SyndicationItemProperty targetSyndicationItem;

        private readonly SyndicationTextContentKind targetTextContentKind;

        private readonly String targetNamespacePrefix;

        private readonly String targetNamespaceUri;

        private readonly bool keepInContent;
#endregion

#region Constructors

        public EntityPropertyMappingAttribute(String sourcePath, SyndicationItemProperty targetSyndicationItem, SyndicationTextContentKind targetTextContentKind, bool keepInContent)
        {
            if (String.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("sourcePath"));
            }

            this.sourcePath            = sourcePath;
            this.targetPath            = SyndicationItemPropertyToPath(targetSyndicationItem);
            this.targetSyndicationItem = targetSyndicationItem;
            this.targetTextContentKind = targetTextContentKind;
            this.targetNamespacePrefix = EntityPropertyMappingAttribute.AtomNamespacePrefix;
            this.targetNamespaceUri    = XmlConstants.AtomNamespace;
            this.keepInContent         = keepInContent;
        }

        public EntityPropertyMappingAttribute(String sourcePath, String targetPath, String targetNamespacePrefix, String targetNamespaceUri, bool keepInContent)
        {
            if (String.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("sourcePath"));
            }

            this.sourcePath = sourcePath;

            if (String.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("targetPath"));
            }

            if (targetPath[0] == '@')
            {
                throw new ArgumentException(Strings.EpmTargetTree_InvalidTargetPath(targetPath));
            }

            this.targetPath = targetPath;

            this.targetSyndicationItem = SyndicationItemProperty.CustomProperty;
            this.targetTextContentKind = SyndicationTextContentKind.Plaintext;
            this.targetNamespacePrefix = targetNamespacePrefix;

            if (String.IsNullOrEmpty(targetNamespaceUri))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("targetNamespaceUri"));
            }

            this.targetNamespaceUri = targetNamespaceUri;

            Uri uri;
            if (!Uri.TryCreate(targetNamespaceUri, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_TargetNamespaceUriNotValid(targetNamespaceUri));
            }

            this.keepInContent = keepInContent;
        }

        #region Properties

        public String SourcePath
        {
            get { return this.sourcePath; }
        }

        public String TargetPath
        {
            get { return this.targetPath; }
        }

        public SyndicationItemProperty TargetSyndicationItem
        {
            get { return this.targetSyndicationItem; }
        }

        public String TargetNamespacePrefix
        {
            get { return this.targetNamespacePrefix; }
        }

        public String TargetNamespaceUri
        {
            get { return this.targetNamespaceUri; }
        }

        public SyndicationTextContentKind TargetTextContentKind
        {
            get { return this.targetTextContentKind; }
        }

        public bool KeepInContent
        {
            get { return this.keepInContent; }
        }

        #endregion

        internal static String SyndicationItemPropertyToPath(SyndicationItemProperty targetSyndicationItem)
        {
            switch (targetSyndicationItem)
            {
                case SyndicationItemProperty.AuthorEmail:
                    return "author/email";
                case SyndicationItemProperty.AuthorName:
                    return "author/name";
                case SyndicationItemProperty.AuthorUri:
                    return "author/uri";
                case SyndicationItemProperty.ContributorEmail:
                    return "contributor/email";
                case SyndicationItemProperty.ContributorName:
                    return "contributor/name";
                case SyndicationItemProperty.ContributorUri:
                    return "contributor/uri";
                case SyndicationItemProperty.Updated:
                    return "updated";
                case SyndicationItemProperty.Published:
                    return "published";
                case SyndicationItemProperty.Rights:
                    return "rights";
                case SyndicationItemProperty.Summary:
                    return "summary";
                case SyndicationItemProperty.Title:
                    return "title";
                default:
                    throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("targetSyndicationItem"));
            }
        }

#endregion 
    }
}
