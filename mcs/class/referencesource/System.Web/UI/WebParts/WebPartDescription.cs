//------------------------------------------------------------------------------
// <copyright file="WebPartDescription.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public class WebPartDescription {

        private string _id;
        private string _title;
        private string _description;
        private string _imageUrl;
        private WebPart _part;

        private WebPartDescription() {
        }

        public WebPartDescription(string id, string title, string description, string imageUrl) {
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id");
            }
            if (String.IsNullOrEmpty(title)) {
                throw new ArgumentNullException("title");
            }
            _id = id;
            _title = title;
            _description = (description != null) ? description : String.Empty;
            _imageUrl = (imageUrl != null) ? imageUrl : String.Empty;
        }

        public WebPartDescription(WebPart part) {
            string id = part.ID;
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_NoWebPartID), "part");
            }

            _id = id;

            string displayTitle = part.DisplayTitle;
            _title = (displayTitle != null) ? displayTitle : String.Empty;

            string description = part.Description;
            _description = (description != null) ? description : String.Empty;

            string imageUrl = part.CatalogIconImageUrl;
            _imageUrl = (imageUrl != null) ? imageUrl : String.Empty;

            _part = part;
        }

        public string CatalogIconImageUrl {
            get {
                return _imageUrl;
            }
        }

        public string Description {
            get {
                return _description;
            }
        }

        public string ID {
            get {
                return _id;
            }
        }

        public string Title {
            get {
                return _title;
            }
        }

        internal WebPart WebPart {
            get {
                return _part;
            }
        }
    }
}
