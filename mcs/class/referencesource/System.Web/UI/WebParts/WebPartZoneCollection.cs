//------------------------------------------------------------------------------
// <copyright file="WebPartZoneCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Globalization;

    /// <devdoc>
    /// Read-only collection of WebPartZones.  Collection cannot be modified after contstruction.
    /// </devdoc>
    public sealed class WebPartZoneCollection : ReadOnlyCollectionBase {

        public WebPartZoneCollection() {
        }

        public WebPartZoneCollection(ICollection webPartZones) {
            if (webPartZones == null) {
                throw new ArgumentNullException("webPartZones");
            }

            foreach (object obj in webPartZones) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), "webPartZones");
                }
                if (!(obj is WebPartZone)) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartZone"), "webPartZones");
                }
                InnerList.Add(obj);
            }
        }

        internal int Add(WebPartZoneBase value) {
            return InnerList.Add(value);
        }

        public bool Contains(WebPartZoneBase value) {
            return InnerList.Contains(value);
        }

        public int IndexOf(WebPartZoneBase value) {
            return InnerList.IndexOf(value);
        }

        public WebPartZoneBase this[int index] {
            get {
                return (WebPartZoneBase) InnerList[index];
            }
        }

        public WebPartZoneBase this[string id] {
            get {
                WebPartZoneBase selectedZone = null;

                foreach (WebPartZoneBase zone in InnerList) {
                    if (String.Equals(zone.ID, id, StringComparison.OrdinalIgnoreCase)) {
                        selectedZone = zone;
                        break;
                    }
                }

                return selectedZone;
            }
        }

        /// <devdoc>
        /// <para>Copies contents from the collection to a specified array with a
        /// specified starting index.</para>
        /// </devdoc>
        public void CopyTo(WebPartZoneBase[] array, int index) {
            InnerList.CopyTo(array, index);
        }

    }
}
