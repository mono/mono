//------------------------------------------------------------------------------
// <copyright file="XhtmlCssHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI.MobileControls.Adapters;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    /// <include file='doc\XhtmlCssHandler.uex' path='docs/doc[@for="XhtmlCssHandler"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlCssHandler : IHttpHandler, IRequiresSessionState {
        
        /// <include file='doc\XhtmlCssHandler.uex' path='docs/doc[@for="XhtmlCssHandler.ProcessRequest"]/*' />
        public void ProcessRequest (HttpContext context) {
            String cssQueryStringValue = (String) context.Request.QueryString[XhtmlConstants.CssQueryStringName];
            String response;
            if (cssQueryStringValue != null) {
                // Recall that Page.Cache is application level
                if (cssQueryStringValue.StartsWith(XhtmlConstants.SessionKeyPrefix, StringComparison.Ordinal)) {
                    response = (String) context.Session[cssQueryStringValue];
                }
                else {
                    response = (String) context.Cache[cssQueryStringValue];
                }
                context.Response.ContentType="text/css";                
            }
            else {
                throw new HttpException (404, SR.GetString(
                    SR.XhtmlCssHandler_IdNotPresent));
            }
            if (response == null) {
                throw new HttpException (404,  SR.GetString(
                    SR.XhtmlCssHandler_StylesheetNotFound));            
            }
            context.Response.Write (response);
        }

        /// <include file='doc\XhtmlCssHandler.uex' path='docs/doc[@for="XhtmlCssHandler.IsReusable"]/*' />
        public bool IsReusable {
            get {
                return true;
            }
        }        
    }
}
