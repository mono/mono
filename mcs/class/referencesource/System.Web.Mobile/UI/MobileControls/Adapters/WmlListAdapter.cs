//------------------------------------------------------------------------------
// <copyright file="WmlListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * WmlListAdapter provides the wml device functionality for List controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlListAdapter.uex' path='docs/doc[@for="WmlListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlListAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlListAdapter.uex' path='docs/doc[@for="WmlListAdapter.Control"]/*' />
        protected new List Control
        {
            get
            {
                return (List)base.Control;
            }
        }

        /// <include file='doc\WmlListAdapter.uex' path='docs/doc[@for="WmlListAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
        }

        /// <include file='doc\WmlListAdapter.uex' path='docs/doc[@for="WmlListAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            if(Control.HasControls())
            {
                writer.BeginCustomMarkup();
                RenderChildren(writer);
                writer.EndCustomMarkup();
                return;
            }

            int pageStart = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;
            if (pageSize == 0)
            {
                return;
            }

            MobileListItemCollection items = Control.Items;
            if (items.Count == 0)
            {
                return;
            }

            bool itemsAsLinks = Control.ItemsAsLinks;
            bool hasCmdHandler = Control.HasItemCommandHandler;

            writer.EnterStyle(Style);
            for (int i = 0; i < pageSize; i++)
            {                        
                MobileListItem item = items[pageStart + i];

                if (itemsAsLinks)
                {
                    RenderLink(writer, item.Value, null, false, false, item.Text, true);
                }
                else if (hasCmdHandler)
                {
                    RenderPostBackEvent(writer, item.Index.ToString(CultureInfo.InvariantCulture), null, true, item.Text, true); 
                }
                else
                {
                    writer.RenderText(item.Text, true);
                }
            }
            writer.ExitStyle(Style);
        }
    }
}










