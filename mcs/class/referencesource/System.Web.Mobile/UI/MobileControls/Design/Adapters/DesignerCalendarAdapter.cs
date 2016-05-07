//------------------------------------------------------------------------------
// <copyright file="DesignerCalendarAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Drawing;
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
    internal class DesignerCalendarAdapter : HtmlCalendarAdapter
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
            writer.WriteBeginTag("div");
            String width = DesignerAdapterUtil.GetWidth(Control);

            writer.WriteAttribute("style", "cellpadding=2px;width:" + width);

            Alignment alignment = (Alignment)Style[Style.AlignmentKey, true];
            if (alignment != Alignment.NotSet)
            {
                writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
            }
            writer.Write("/>");

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();

            //Note: Although this is an internal method of runtime, but it is still
            //      pretty easy to achieve the same goal without using this method.
            Style.ApplyTo(Control.WebCalendar);
            base.Render(writer);

            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
            writer.WriteEndTag("div");
        }
    }
}
