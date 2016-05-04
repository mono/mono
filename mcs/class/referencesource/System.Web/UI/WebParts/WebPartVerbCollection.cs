//------------------------------------------------------------------------------
// <copyright file="WebPartVerbCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;

    public sealed class WebPartVerbCollection : ReadOnlyCollectionBase {

        private HybridDictionary _ids;

        public static readonly WebPartVerbCollection Empty = new WebPartVerbCollection();

        public WebPartVerbCollection() {
            Initialize(null, null);
        }

        public WebPartVerbCollection(ICollection verbs) {
            Initialize(null, verbs);
        }

        public WebPartVerbCollection(WebPartVerbCollection existingVerbs, ICollection verbs) {
            Initialize(existingVerbs, verbs);
        }

        public WebPartVerb this[int index] {
            get {
                return (WebPartVerb) InnerList[index];
            }
        }

        // Currently, the WebPartVerbCollection class allows verbs to be added with duplicate IDs.
        // Ideally, we would not allow this.  However, since the WebPartChrome.GetWebParts() method
        // returns both the Part and Zone verbs, there is the potential to have multiple verbs
        // with the same ID.  So this property must be internal, since we are guaranteed to never
        // call it on a collection with duplicate IDs.
        internal WebPartVerb this[string id] {
            get {
                return (WebPartVerb)_ids[id];
            }
        }

        // Do not throw for duplicate IDs, since we call this method to add items
        // to the collection internally, and we need to allow duplicate IDs.
        internal int Add(WebPartVerb value) {
            return InnerList.Add(value);
        }

        public bool Contains(WebPartVerb value) {
            return InnerList.Contains(value);
        }

        public void CopyTo(WebPartVerb[] array, int index) {
            InnerList.CopyTo(array, index);
        }

        public int IndexOf(WebPartVerb value) {
            return InnerList.IndexOf(value);
        }

        private void Initialize(WebPartVerbCollection existingVerbs, ICollection verbs) {
            int count = ((existingVerbs != null) ? existingVerbs.Count : 0) + ((verbs != null) ? verbs.Count : 0);
            _ids = new HybridDictionary(count, /* caseInsensitive */ true);

            if (existingVerbs != null) {
                foreach (WebPartVerb existingVerb in existingVerbs) {
                    // Don't need to check arg, since we know it is valid since it came
                    // from a CatalogPartCollection.
                    if (_ids.Contains(existingVerb.ID)) {
                        throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "WebPartVerb", existingVerb.ID), "existingVerbs");
                    }
                    _ids.Add(existingVerb.ID, existingVerb);

                    InnerList.Add(existingVerb);
                }
            }

            if (verbs != null) {
                foreach (object obj in verbs) {
                    if (obj == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "verbs");
                    }
                    WebPartVerb webPartVerb = obj as WebPartVerb;
                    if (webPartVerb == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartVerb"), "verbs");
                    }

                    if (_ids.Contains(webPartVerb.ID)) {
                        throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "WebPartVerb", webPartVerb.ID), "verbs");
                    }
                    _ids.Add(webPartVerb.ID, webPartVerb);

                    InnerList.Add(webPartVerb);
                }
            }
        }
    }
}
