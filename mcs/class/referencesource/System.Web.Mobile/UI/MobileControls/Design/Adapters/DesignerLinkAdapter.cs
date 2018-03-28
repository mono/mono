//------------------------------------------------------------------------------
// <copyright file="DesignerLinkAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.Web.Mobile;
using System.Web.UI.Design.MobileControls;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerLinkAdapter : HtmlLinkAdapter 
    {
        public override MobileCapabilities Device
        {
            get
            {
                return DesignerCapabilities.Instance;
            }
        }

        public override void Render(HtmlMobileTextWriter writer)
        {
            Alignment alignment = (Alignment)Style[Style.AlignmentKey, true];
            Wrapping wrapping = (Wrapping) Style[Style.WrappingKey, true];
            bool wrap = (wrapping == Wrapping.Wrap || wrapping == Wrapping.NotSet);

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
            writer.WriteBeginTag("div");
            String width = DesignerAdapterUtil.GetWidth(Control);

            if (!wrap)
            {
                byte templateStatus;
                int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);
                if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
                {
                    width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
                }
                writer.WriteAttribute("style", "overflow-x:hidden;width:" + width);
            }
            else
            {
                writer.WriteAttribute("style", "word-wrap:break-word;width:" + width);
            }

            if (alignment != Alignment.NotSet)
            {
                writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
            }
            writer.Write(">");

            writer.WriteBeginTag("a");
            writer.WriteAttribute("href", "NavigationUrl");
            writer.Write(">");
            ((DesignerTextWriter)writer).WriteCssStyleText(Style, null, Control.Text, true);
            writer.WriteEndTag("a");

            writer.WriteEndTag("div");
            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
        }
    }
}

