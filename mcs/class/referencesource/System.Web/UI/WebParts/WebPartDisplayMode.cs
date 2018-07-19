//------------------------------------------------------------------------------
// <copyright file="WebPartDisplayMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public abstract class WebPartDisplayMode {

        private string _name;

        protected WebPartDisplayMode(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }
            _name = name;
        }

        public virtual bool AllowPageDesign {
            get {
                return false;
            }
        }

        public virtual bool AssociatedWithToolZone {
            get {
                return false;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        public virtual bool RequiresPersonalization {
            get {
                return false;
            }
        }

        public virtual bool ShowHiddenWebParts {
            get {
                return false;
            }
        }

        public virtual bool IsEnabled(WebPartManager webPartManager) {
            return (!RequiresPersonalization || webPartManager.Personalization.IsModifiable);
        }
    }
}
