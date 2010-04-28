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


namespace System.Data.Services.Client
{
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Xml;

    internal sealed class EpmSyndicationContentSerializer : EpmContentSerializerBase, IDisposable
    {
        private bool authorInfoPresent;

        private bool updatedPresent;

        private bool authorNamePresent;

        internal EpmSyndicationContentSerializer(EpmTargetTree tree, object element, XmlWriter target)
            : base(tree, true, element, target)
        {
        }

        public void Dispose()
        {
            this.CreateAuthor(true);
            this.CreateUpdated();
        }

        protected override void Serialize(EpmTargetPathSegment targetSegment, EpmSerializationKind kind)
        {
            if (targetSegment.HasContent)
            {
                EntityPropertyMappingInfo epmInfo = targetSegment.EpmInfo;
                Object propertyValue;

                try
                {
                    propertyValue = epmInfo.PropValReader.DynamicInvoke(this.Element);
                }
                catch (System.Reflection.TargetInvocationException)
                {
                    throw;
                }

                String contentType;
                Action<String> contentWriter;

                switch (epmInfo.Attribute.TargetTextContentKind)
                {
                    case SyndicationTextContentKind.Html:
                        contentType = "html";
                        contentWriter = this.Target.WriteString;
                        break;
                    case SyndicationTextContentKind.Xhtml:
                        contentType = "xhtml";
                        contentWriter = this.Target.WriteRaw;
                        break;
                    default:
                        contentType = "text";
                        contentWriter = this.Target.WriteString;
                        break;
                }

                Action<String, bool, bool> textSyndicationWriter = (c, nonTextPossible, atomDateConstruct) =>
                {
                    this.Target.WriteStartElement(c, XmlConstants.AtomNamespace);
                    if (nonTextPossible)
                    {
                        this.Target.WriteAttributeString(XmlConstants.AtomTypeAttributeName, String.Empty, contentType);
                    }

                    String textPropertyValue = 
                        propertyValue != null   ? ClientConvert.ToString(propertyValue, atomDateConstruct) :
                        atomDateConstruct       ? ClientConvert.ToString(DateTime.MinValue, atomDateConstruct) : 
                        String.Empty;

                    contentWriter(textPropertyValue);
                    this.Target.WriteEndElement();
                };

                switch (epmInfo.Attribute.TargetSyndicationItem)
                {
                    case SyndicationItemProperty.AuthorEmail:
                    case SyndicationItemProperty.ContributorEmail:
                        textSyndicationWriter(XmlConstants.AtomEmailElementName, false, false);
                        break;
                    case SyndicationItemProperty.AuthorName:
                    case SyndicationItemProperty.ContributorName:
                        textSyndicationWriter(XmlConstants.AtomNameElementName, false, false);
                        this.authorNamePresent = true;
                        break;
                    case SyndicationItemProperty.AuthorUri:
                    case SyndicationItemProperty.ContributorUri:
                        textSyndicationWriter(XmlConstants.AtomUriElementName, false, false);
                        break;
                    case SyndicationItemProperty.Updated:
                        textSyndicationWriter(XmlConstants.AtomUpdatedElementName, false, true);
                        this.updatedPresent = true;
                        break;
                    case SyndicationItemProperty.Published:
                        textSyndicationWriter(XmlConstants.AtomPublishedElementName, false, true);
                        break;
                    case SyndicationItemProperty.Rights:
                        textSyndicationWriter(XmlConstants.AtomRightsElementName, true, false);
                        break;
                    case SyndicationItemProperty.Summary:
                        textSyndicationWriter(XmlConstants.AtomSummaryElementName, true, false);
                        break;
                    case SyndicationItemProperty.Title:
                        textSyndicationWriter(XmlConstants.AtomTitleElementName, true, false);
                        break;
                    default:
                        Debug.Assert(false, "Unhandled SyndicationItemProperty enum value - should never get here.");
                        break;
                }
            }
            else
            {
                if (targetSegment.SegmentName == XmlConstants.AtomAuthorElementName)
                {
                    this.CreateAuthor(false);
                    base.Serialize(targetSegment, kind);
                    this.FinishAuthor();
                }
                else if (targetSegment.SegmentName == XmlConstants.AtomContributorElementName)
                {
                    this.Target.WriteStartElement(XmlConstants.AtomContributorElementName, XmlConstants.AtomNamespace);
                    base.Serialize(targetSegment, kind);
                    this.Target.WriteEndElement();
                }
                else
                {
                    Debug.Assert(false, "Only authors and contributors have nested elements");
                }
            }
        }

        private void CreateAuthor(bool createNull)
        {
            if (!this.authorInfoPresent)
            {
                if (createNull)
                {
                    this.Target.WriteStartElement(XmlConstants.AtomAuthorElementName, XmlConstants.AtomNamespace);
                    this.Target.WriteElementString(XmlConstants.AtomNameElementName, XmlConstants.AtomNamespace, String.Empty);
                    this.Target.WriteEndElement();
                }
                else
                {
                    this.Target.WriteStartElement(XmlConstants.AtomAuthorElementName, XmlConstants.AtomNamespace);
                }

                this.authorInfoPresent = true;
            }
        }

        private void FinishAuthor()
        {
            Debug.Assert(this.authorInfoPresent == true, "Must have already written the start element for author");
            if (this.authorNamePresent == false)
            {
                this.Target.WriteElementString(XmlConstants.AtomNameElementName, XmlConstants.AtomNamespace, String.Empty);
                this.authorNamePresent = true;
            }
 
            this.Target.WriteEndElement();
        }

        private void CreateUpdated()
        {
            if (!this.updatedPresent)
            {
                this.Target.WriteElementString(XmlConstants.AtomUpdatedElementName, XmlConstants.AtomNamespace, XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.RoundtripKind));
            }
        }
    }
}