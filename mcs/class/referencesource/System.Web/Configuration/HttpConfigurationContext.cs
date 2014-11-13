//------------------------------------------------------------------------------
// <copyright file="HttpConfigurationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Property dictionary available to section handlers in 
    ///       web applications.
    ///    </para>
    /// </devdoc>
    public class HttpConfigurationContext {

        private string vpath;
        

        /// <devdoc>
        ///     <para>
        ///         Virtual path to the virtual directory containing web.config.
        ///         This could be the virtual path to a file in the case of a 
        ///         section in &lt;location path='file.aspx'&gt;.
        ///     </para>
        /// </devdoc>
        public string VirtualPath {
            get {
                return vpath;
            }
        }

        /// <devdoc>
        ///     <para>Can only be created by ASP.NET Configuration System.</para>
        /// </devdoc>
        internal HttpConfigurationContext(string vpath) {
            this.vpath = vpath;
        }

    }
}
