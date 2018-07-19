//------------------------------------------------------------------------------
// <copyright file="WebPartCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    /// <devdoc>
    /// Read-only collection of WebParts.  Collection cannot be modified after contstruction.
    /// </devdoc>
    public sealed class WebPartCollection : ReadOnlyCollectionBase {

        public WebPartCollection() {
        }

        public WebPartCollection(ICollection webParts) {
            if (webParts == null) {
                throw new ArgumentNullException("webParts");
            }

            foreach (object obj in webParts) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "webParts");
                }
                if (!(obj is WebPart)) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPart"), "webParts");
                }
                InnerList.Add(obj);
            }
        }

        internal int Add(WebPart value) {
            Debug.Assert(value != null);
            return InnerList.Add(value);
        }

        public bool Contains(WebPart value) {
            return InnerList.Contains(value);
        }

        public int IndexOf(WebPart value) {
            return InnerList.IndexOf(value);
        }

        public WebPart this[int index] {
            get {
                return (WebPart) InnerList[index];
            }
        }

        /// <devdoc>
        /// Returns the WebPart with the specified id, or the GenericWebPart containing a control with
        /// the specified id, or the ProxyWebPart with OriginalID or GenericWebPartID equal to the
        /// specified id, performing a case-insensitive comparison.  Returns null if there are no matches.
        /// </devdoc>
        public WebPart this[string id] {
            // PERF: Use a hashtable for lookup, instead of a linear search
            get {
                foreach (WebPart webPart in InnerList) {
                    if (String.Equals(webPart.ID, id, StringComparison.OrdinalIgnoreCase)) {
                        return webPart;
                    }

                    GenericWebPart genericWebPart = webPart as GenericWebPart;
                    if (genericWebPart != null) {
                        Control control = genericWebPart.ChildControl;
                        if (control != null) {
                            if (String.Equals(control.ID, id, StringComparison.OrdinalIgnoreCase)) {
                                return genericWebPart;
                            }
                        }
                    }

                    ProxyWebPart proxyWebPart = webPart as ProxyWebPart;
                    if (proxyWebPart != null) {
                        if ((String.Equals(proxyWebPart.OriginalID, id, StringComparison.OrdinalIgnoreCase)) ||
                            (String.Equals(proxyWebPart.GenericWebPartID, id, StringComparison.OrdinalIgnoreCase))) {
                            return proxyWebPart;
                        }
                    }
                }

                return null;
            }
        }

        /// <devdoc>
        /// <para>Copies contents from the collection to a specified array with a
        /// specified starting index.</para>
        /// </devdoc>
        public void CopyTo(WebPart[] array, int index) {
            InnerList.CopyTo(array, index);
        }

    }
}
