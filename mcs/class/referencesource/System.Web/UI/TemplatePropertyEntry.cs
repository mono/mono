//------------------------------------------------------------------------------
// <copyright file="TemplatePropertyEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    /// <devdoc>
    /// PropertyEntry for ITemplate properties
    /// </devdoc>
    public class TemplatePropertyEntry : BuilderPropertyEntry {
        private bool _bindableTemplate;

        internal TemplatePropertyEntry() {
        }
        
        internal TemplatePropertyEntry(bool bindableTemplate) {
            _bindableTemplate = bindableTemplate;
        }

        internal bool IsMultiple {
            get {
                return Util.IsMultiInstanceTemplateProperty(PropertyInfo);
            }
        }

        public bool BindableTemplate {
            get {
                return _bindableTemplate;
            }
        }
    }


}


