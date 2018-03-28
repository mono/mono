//------------------------------------------------------------------------------
// <copyright file="BuilderPropertyEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    /// <devdoc>
    /// Abstract base class for all property entries that require a ControlBuilder
    /// </devdoc>
    public abstract class BuilderPropertyEntry : PropertyEntry {
        private ControlBuilder _builder;

        internal BuilderPropertyEntry() {
        }


        /// <devdoc>
        /// </devdoc>
        public ControlBuilder Builder {
            get {
                return _builder;
            }
            set {
                _builder = value;
            }
        }
    }
}


