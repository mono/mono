//------------------------------------------------------------------------------
// <copyright file="DesignerAdRotatorAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
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
    internal class DesignerAdRotatorAdapter : System.Web.UI.MobileControls.Adapters.HtmlControlAdapter
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
            String width = DesignerAdapterUtil.GetWidth(Control);

            byte templateStatus;
            int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);

            if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
            {
                width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
            }

            writer.WriteBeginTag("div");
            if (alignment == Alignment.Center)
            {
                writer.WriteAttribute("align", "center");
            }
            writer.WriteAttribute("style", "padding=2px;overflow-x:hidden;width:" + width);
            writer.Write(">");

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();

            writer.WriteBeginTag("img");
            writer.WriteAttribute("alt", Control.ID);
            ((DesignerTextWriter)writer).WriteStyleAttribute(Style);

            // center alignment not part of HTML for images.
            if (alignment == Alignment.Right ||
                alignment == Alignment.Left)
            {
                writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
            }

            writer.WriteAttribute("height", "40");
            writer.WriteAttribute("width", "250");
            writer.WriteAttribute("border", "0");
            writer.Write(">");

            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
            writer.WriteEndTag("div");
        }
    }
}
