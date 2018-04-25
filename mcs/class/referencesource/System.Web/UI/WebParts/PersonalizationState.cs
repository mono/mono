//------------------------------------------------------------------------------
// <copyright file="PersonalizationState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Web;
    using System.Web.Util;

    /// <devdoc>
    /// </devdoc>
    public abstract class PersonalizationState {

        private WebPartManager _webPartManager;
        private bool _dirty;

        /// <devdoc>
        /// </devdoc>
        protected PersonalizationState(WebPartManager webPartManager) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            _webPartManager = webPartManager;
        }

        /// <devdoc>
        /// </devdoc>
        public bool IsDirty {
            get {
                return _dirty;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public abstract bool IsEmpty {
            get;
        }

        /// <devdoc>
        /// </devdoc>
        public WebPartManager WebPartManager {
            get {
                return _webPartManager;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public abstract void ApplyWebPartPersonalization(WebPart webPart);

        /// <devdoc>
        /// </devdoc>
        public abstract void ApplyWebPartManagerPersonalization();

        /// <devdoc>
        /// </devdoc>
        public abstract void ExtractWebPartPersonalization(WebPart webPart);

        /// <devdoc>
        /// </devdoc>
        public abstract void ExtractWebPartManagerPersonalization();

        // Returns the AuthorizationFilter string for a WebPart before it is instantiated
        // Returns null if there is no personalized value for AuthorizationFilter
        public abstract string GetAuthorizationFilter(string webPartID);

        /// <devdoc>
        /// </devdoc>
        protected void SetDirty() {
            _dirty = true;
        }

        /// <devdoc>
        /// </devdoc>
        public abstract void SetWebPartDirty(WebPart webPart);

        /// <devdoc>
        /// </devdoc>
        public abstract void SetWebPartManagerDirty();

        /// <devdoc>
        /// </devdoc>
        protected void ValidateWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            if (_webPartManager.WebParts.Contains(webPart) == false) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }
        }
    }
}
