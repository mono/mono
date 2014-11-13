//------------------------------------------------------------------------------
// <copyright file="SoapMessageStage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;

    /// <include file='doc\SoapMessageStage.uex' path='docs/doc[@for="SoapMessageStage"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    //[Flags]
    public enum SoapMessageStage {
        /// <include file='doc\SoapMessageStage.uex' path='docs/doc[@for="SoapMessageStage.BeforeSerialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        BeforeSerialize = 1,
        /// <include file='doc\SoapMessageStage.uex' path='docs/doc[@for="SoapMessageStage.AfterSerialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        AfterSerialize = 2,
        /// <include file='doc\SoapMessageStage.uex' path='docs/doc[@for="SoapMessageStage.BeforeDeserialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        BeforeDeserialize = 4,
        /// <include file='doc\SoapMessageStage.uex' path='docs/doc[@for="SoapMessageStage.AfterDeserialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        AfterDeserialize = 8
    }
}
