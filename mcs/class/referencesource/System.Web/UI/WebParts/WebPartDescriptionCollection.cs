//------------------------------------------------------------------------------
// <copyright file="WebPartDescriptionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class WebPartDescriptionCollection : ReadOnlyCollectionBase {

        private HybridDictionary _ids;

        public WebPartDescriptionCollection() {
        }

        public WebPartDescriptionCollection(ICollection webPartDescriptions) {
            if (webPartDescriptions == null) {
                throw new ArgumentNullException("webPartDescriptions");
            }

            _ids = new HybridDictionary(webPartDescriptions.Count, true /* caseInsensitive */);
            foreach (object obj in webPartDescriptions) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "webPartDescriptions");
                }
                WebPartDescription description = obj as WebPartDescription;
                if (description == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartDescription"),
                                                "webPartDescriptions");
                }
                string id = description.ID;
                if (!_ids.Contains(id)) {
                    InnerList.Add(description);
                    _ids.Add(id, description);
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "WebPartDescription", id), "webPartDescriptions");
                }
            }
        }

        public bool Contains(WebPartDescription value) {
            return InnerList.Contains(value);
        }

        public int IndexOf(WebPartDescription value) {
            return InnerList.IndexOf(value);
        }

        public WebPartDescription this[int index] {
            get {
                return (WebPartDescription) InnerList[index];
            }
        }

        public WebPartDescription this[string id] {
            get {
                return ((_ids != null) ? (WebPartDescription)_ids[id] : null);
            }
        }

        public void CopyTo(WebPartDescription[] array, int index) {
            InnerList.CopyTo(array, index);
        }
    }
}
