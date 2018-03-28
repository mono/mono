//------------------------------------------------------------------------------
// <copyright file="ZoneLinkButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// </devdoc>
    [SupportsEventValidation]
    internal sealed class ZoneLinkButton : LinkButton {

        private WebZone _owner;
        private string _eventArgument;
        private string _imageUrl;

        public ZoneLinkButton(WebZone owner, string eventArgument) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }
            _owner = owner;
            _eventArgument = eventArgument;
        }

        public string ImageUrl {
            get {
                return (_imageUrl != null) ? _imageUrl : String.Empty;
            }
            set {
                _imageUrl = value;
            }
        }

        protected override PostBackOptions GetPostBackOptions() {
            // _owner.Page may be null in the designer
            if (!String.IsNullOrEmpty(_eventArgument) && _owner.Page != null) {
                PostBackOptions options = new PostBackOptions(_owner, _eventArgument);
                options.RequiresJavaScriptProtocol = true;

                return options;
            }

            return base.GetPostBackOptions();
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            // Copied from HyperLink.RenderContents() and modified slightly
            string imageUrl = ImageUrl;
            if (!String.IsNullOrEmpty(imageUrl)) {
                Image image = new Image();

                // NOTE: The Url resolution happens right here, because the image is not parented
                //       and will not be able to resolve when it tries to do so.
                image.ImageUrl = ResolveClientUrl(imageUrl);

                string toolTip = ToolTip;
                if (!String.IsNullOrEmpty(toolTip)) {
                    image.ToolTip = toolTip;
                }

                string text = Text;
                if (!String.IsNullOrEmpty(text)) {
                    image.AlternateText = text;
                }

                image.Page = Page;
                image.RenderControl(writer);
            }
            else {
                base.RenderContents(writer);
            }
        }

    }
}
