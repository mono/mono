//------------------------------------------------------------------------------
// <copyright file="HttpMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;

    /// <include file='doc\HttpMethodAttribute.uex' path='docs/doc[@for="HttpMethodAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HttpMethodAttribute : System.Attribute {
        Type returnFormatter;
        Type parameterFormatter;
        
        /// <include file='doc\HttpMethodAttribute.uex' path='docs/doc[@for="HttpMethodAttribute.HttpMethodAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public HttpMethodAttribute() {
            returnFormatter = null;
            parameterFormatter = null;
        }

        /// <include file='doc\HttpMethodAttribute.uex' path='docs/doc[@for="HttpMethodAttribute.HttpMethodAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public HttpMethodAttribute(Type returnFormatter, Type parameterFormatter) {
            this.returnFormatter = returnFormatter;
            this.parameterFormatter = parameterFormatter;
        }
        
        /// <include file='doc\HttpMethodAttribute.uex' path='docs/doc[@for="HttpMethodAttribute.ReturnFormatter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type ReturnFormatter {
            get { return returnFormatter; }
            set { returnFormatter = value; }
        }

        /// <include file='doc\HttpMethodAttribute.uex' path='docs/doc[@for="HttpMethodAttribute.ParameterFormatter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type ParameterFormatter {
            get { return parameterFormatter; }
            set { parameterFormatter = value; }
        }
    }
}
