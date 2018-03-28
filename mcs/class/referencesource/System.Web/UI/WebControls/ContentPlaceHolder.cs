//------------------------------------------------------------------------------
// <copyright file="ContentPlaceHolder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.Util;

    internal class ContentPlaceHolderBuilder : ControlBuilder {

        private string _contentPlaceHolderID;
        private string _templateName;

        internal string Name { get { return _templateName; } }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder,
                                  Type type, string tagName, string ID, IDictionary attribs) {

            // Copy the ID so that it will be available when BuildObject is called
            _contentPlaceHolderID = ID;

            if (parser.FInDesigner) {
                // shortcut for designer
                base.Init(parser, parentBuilder, type, tagName, ID, attribs);
                return;
            }

            if (String.IsNullOrEmpty(ID)) {
                throw new HttpException(SR.GetString(SR.Control_Missing_Attribute, "ID", type.Name));
            }

            _templateName = ID;

            MasterPageParser masterPageParser = parser as MasterPageParser;
            if (masterPageParser == null) {
                throw new HttpException(SR.GetString(SR.ContentPlaceHolder_only_in_master));
            }

			base.Init(parser, parentBuilder, type, tagName, ID, attribs);

            if (masterPageParser.PlaceHolderList.Contains(Name))
                throw new HttpException(SR.GetString(SR.ContentPlaceHolder_duplicate_contentPlaceHolderID, Name));

            masterPageParser.PlaceHolderList.Add(Name);
        }

        public override object BuildObject() {
            MasterPage masterPage = TemplateControl as MasterPage;

            Debug.Assert(masterPage != null || InDesigner);

            // Instantiate the ContentPlaceHolder
            ContentPlaceHolder cph = (ContentPlaceHolder) base.BuildObject();

            // If the page is providing content, instantiate it in the holder
            if (PageProvidesMatchingContent(masterPage)) {
                ITemplate tpl = ((System.Web.UI.ITemplate)(masterPage.ContentTemplates[_contentPlaceHolderID]));
                masterPage.InstantiateInContentPlaceHolder(cph, tpl);
            }

            return cph;
        }

        internal override void BuildChildren(object parentObj) {

            MasterPage masterPage = TemplateControl as MasterPage;

            // If the page is providing content, don't call the base, which would
            // instantiate the default content (which we don't want)
            if (PageProvidesMatchingContent(masterPage))
                return;

            base.BuildChildren(parentObj);
        }

        private bool PageProvidesMatchingContent(MasterPage masterPage) {

            if (masterPage != null && masterPage.ContentTemplates != null
                        && masterPage.ContentTemplates.Contains(_contentPlaceHolderID)) {
                return true;
            }

            return false;
        }
    }

    // Factory used to efficiently create builder instances
    internal class ContentPlaceHolderBuilderFactory: IWebObjectFactory {
        object IWebObjectFactory.CreateInstance() {
            return new ContentPlaceHolderBuilder();
        }
    }

    [ControlBuilderAttribute(typeof(ContentPlaceHolderBuilder))]
    [Designer("System.Web.UI.Design.WebControls.ContentPlaceHolderDesigner, " + AssemblyRef.SystemDesign)]
    [ToolboxItemFilter("System.Web.UI")]
    [ToolboxItemFilter("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner", ToolboxItemFilterType.Require)]
    [ToolboxData("<{0}:ContentPlaceHolder runat=\"server\"></{0}:ContentPlaceHolder>")]

    public class ContentPlaceHolder : Control, INonBindingContainer {
    }
}
