//------------------------------------------------------------------------------
// <copyright file="DesignerValidationSummaryAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.Web.Mobile;
using System.Web.UI.Design.MobileControls;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Diagnostics;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerValidationSummaryAdapter : HtmlValidationSummaryAdapter
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
            String additionalStyle;

            Alignment alignment = (Alignment) Style[Style.AlignmentKey, true];
            Wrapping wrapping = (Wrapping) Style[Style.WrappingKey, true];
            bool wrap = (wrapping == Wrapping.Wrap || wrapping == Wrapping.NotSet);
            String width = DesignerAdapterUtil.GetWidth(Control);

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
            writer.EnterLayout(Style);
            writer.WriteBeginTag("div");
            if (!wrap)
            {
                byte templateStatus;
                int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);
                if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
                {
                    width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
                }
                additionalStyle = "overflow-x:hidden;width:" + width + ";";
            }
            else
            {
                additionalStyle = "word-wrap:break-word;width:" + width + ";";
            }

            ((DesignerTextWriter)writer).WriteStyleAttribute(Style, additionalStyle);
            if (alignment != Alignment.NotSet)
            {
                writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
            }
            writer.Write(">");

            writer.WriteText(Control.HeaderText, true);
            
            writer.WriteFullBeginTag("ul");
            for (int i = 1; i <= 2; i++)
            {
                writer.WriteFullBeginTag("li");
                writer.Write(SR.GetString(SR.ValidationSummary_ErrorMessage, i.ToString(CultureInfo.InvariantCulture)));
                writer.WriteEndTag("li");
            }
            writer.WriteEndTag("ul");

            writer.WriteBeginTag("a");
            writer.WriteAttribute("href", "NavigationUrl");
            writer.Write(">");
            writer.WriteText(String.IsNullOrEmpty(Control.BackLabel) ? GetDefaultLabel(BackLabel) : Control.BackLabel, true);
            writer.WriteEndTag("a");

            writer.WriteEndTag("div");
            writer.ExitLayout(Style);
            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
        }
    }
}
