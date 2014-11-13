//------------------------------------------------------------------------------
// <copyright file="DesignerSelectionListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Drawing;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerSelectionListAdapter : HtmlSelectionListAdapter
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
            ((DesignerTextWriter)writer).WriteDesignerStyleAttributes(Control, Style);
            writer.Write("\">");

            base.Render(writer);

            writer.WriteEndTag("div");
        }
    }
}
