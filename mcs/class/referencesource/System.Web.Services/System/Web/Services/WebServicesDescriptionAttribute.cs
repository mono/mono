//------------------------------------------------------------------------------
// <copyright file="WebServicesDescriptionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {


    using System;
    using System.ComponentModel;   

    /// <include file='doc\WebServicesDescriptionAttribute.uex' path='docs/doc[@for="WebServicesDescriptionAttribute"]/*' />
    /// <devdoc>
    ///     DescriptionAttribute marks a property, event, or extender with a
    ///     description. Visual designers can display this description when referencing
    ///     the member.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    internal class WebServicesDescriptionAttribute : DescriptionAttribute {

        private bool replaced = false;

        /// <include file='doc\WebServicesDescriptionAttribute.uex' path='docs/doc[@for="WebServicesDescriptionAttribute.WebServicesDescriptionAttribute"]/*' />
        /// <devdoc>
        ///     Constructs a new sys description.
        /// </devdoc>
        internal WebServicesDescriptionAttribute(string description) : base(description) {
        }

        /// <include file='doc\WebServicesDescriptionAttribute.uex' path='docs/doc[@for="WebServicesDescriptionAttribute.Description"]/*' />
        /// <devdoc>
        ///     Retrieves the description text.
        /// </devdoc>
        public override string Description {
            get {
                if (!replaced) {
                    replaced = true;
                    DescriptionValue = Res.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}
