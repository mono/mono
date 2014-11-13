//------------------------------------------------------------------------------
// <copyright file="CustomErrorsSectionWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Diagnostics;
    using System.Web.Configuration;

    internal sealed class CustomErrorsSectionWrapper : ICustomErrorsSection {
        private readonly CustomErrorsSection _customErrorsSection;

        public CustomErrorsSectionWrapper(CustomErrorsSection customErrorsSection) {
            Debug.Assert(customErrorsSection != null);
            _customErrorsSection = customErrorsSection;
        }

        #region ICustomErrorsSection Members
        string ICustomErrorsSection.DefaultRedirect {
            get {
                return _customErrorsSection.DefaultRedirect;
            }
        }

        CustomErrorCollection ICustomErrorsSection.Errors {
            get {
                return _customErrorsSection.Errors;
            }
        }
        #endregion
    }
}
