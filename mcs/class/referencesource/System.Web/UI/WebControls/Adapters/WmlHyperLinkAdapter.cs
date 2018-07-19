//------------------------------------------------------------------------------
// <copyright file="WmlHyperLinkAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Web.Security;
    using System.Web.Util;

    public class WmlHyperLinkAdapter : HyperLinkAdapter {

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            String targetUrl = Control.NavigateUrl;

            String text = Control.Text;
            if (text.Length == 0) {
                // Whidbey 18195 UNDONE: This solution is somewhat ad hoc, awaiting spec resolution on IStaticTextControl
                // in M2.  For now, take text from first IStaticTextControl or DataboundLiteralControl.
                foreach(Control child in Control.Controls) {
                    if (child is IStaticTextControl) {
                        text = ((IStaticTextControl)child).Text;
                        break;
                    }
                    else if (child is DataBoundLiteralControl) {
                        text = ((DataBoundLiteralControl)child).Text;
                        break;
                    }
                }
            }

            String softkeyLabel = Control.SoftkeyLabel;
            if (softkeyLabel.Length == 0) {
                softkeyLabel = Control.Text;
            }
            writer.EnterStyle(Control.ControlStyle);
            // AUI 3652
            targetUrl = Control.ResolveClientUrl(targetUrl);

            targetUrl = Control.GetCountClickUrl(targetUrl);

            // If cookieless mode is on, we need to apply the app path modifier for if the request is authenticated
            HttpContext context = HttpContext.Current;
            Debug.Assert(context != null);
            bool cookieless = CookielessHelperClass.UseCookieless(context, false, FormsAuthentication.CookieMode);
            if (cookieless && context.Request != null && context.Request.IsAuthenticated && context.Response != null) {
                targetUrl = context.Response.ApplyAppPathModifier(targetUrl);
            }

            PageAdapter.RenderBeginHyperlink(writer, targetUrl, false /* encode, Whidbey 111129 */, softkeyLabel, Control.AccessKey);
            String source = Control.ImageUrl;
            if (Control.ImageUrl != null && Control.ImageUrl.Length > 0) {
                writer.RenderImage(source, null /* localsource */, text /* alternateText */);
            }
            else {
                writer.Write(text);
            }
            PageAdapter.RenderEndHyperlink(writer);
            writer.ExitStyle(Control.ControlStyle);
        }
    }
}

#endif
