//------------------------------------------------------------------------------
// <copyright file="PrecompHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


// precompile.axd is cut per DevDiv 33186
#if PRECOMPILE_AXD_SUPPORT

/*
 * PrecompHandler: precompiles the app
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */
namespace System.Web.Handlers {
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.Compilation;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal class PrecompHandler : IHttpHandler {

        internal PrecompHandler() {
        }

        void IHttpHandler.ProcessRequest(HttpContext context) {

            context.Server.ScriptTimeout = 3600;

            // Precompile the app starting at the current request's directory
            BuildManager.TheBuildManager.PrecompileApp(context.Request.FilePathObject, true);

            context.Response.Write("<html><body><h2>");
            context.Response.Write(SR.GetString(SR.Success_precompile));
            context.Response.Write("</h2></body></html>");
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }
    }
}

#endif
