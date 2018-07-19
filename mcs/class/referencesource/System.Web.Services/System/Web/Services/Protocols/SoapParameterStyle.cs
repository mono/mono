//------------------------------------------------------------------------------
// <copyright file="SoapParameterStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;

    /// <include file='doc\SoapParameterStyle.uex' path='docs/doc[@for="SoapParameterStyle"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum SoapParameterStyle {
        /// <include file='doc\SoapParameterStyle.uex' path='docs/doc[@for="SoapParameterStyle.Default"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Default,
        /// <include file='doc\SoapParameterStyle.uex' path='docs/doc[@for="SoapParameterStyle.Bare"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Bare,                    // parameters appear directly
        /// <include file='doc\SoapParameterStyle.uex' path='docs/doc[@for="SoapParameterStyle.Wrapped"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Wrapped,  // parameters are modeled as a struct
    }

}
