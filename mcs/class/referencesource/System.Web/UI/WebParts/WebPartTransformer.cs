//------------------------------------------------------------------------------
// <copyright file="WebPartTransformer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public abstract class WebPartTransformer {

        /// <devdoc>
        /// Overridden by derived classes.  Should return a Control that implements
        /// ITransformerConfigurationControl
        /// </devdoc>
        public virtual Control CreateConfigurationControl() {
            return null;
        }

        protected internal virtual void LoadConfigurationState(object savedState) {
        }

        protected internal virtual object SaveConfigurationState() {
            return null;
        }

        public abstract object Transform(object providerData);
    }
}
