//------------------------------------------------------------------------------
// <copyright file="HtmlListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.MobileControls;
using System.Security.Permissions;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * HtmlListAdapter provides the html device functionality for List controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlListAdapter.uex' path='docs/doc[@for="HtmlListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlListAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlListAdapter.uex' path='docs/doc[@for="HtmlListAdapter.Control"]/*' />
        protected new List Control
        {
            get
            {
                return (List)base.Control;
            }
        }

        /// <include file='doc\HtmlListAdapter.uex' path='docs/doc[@for="HtmlListAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            if(Control.HasControls())
            {
                RenderChildren(writer);
                return;
            }
            RenderList(writer);
        }

        /// <include file='doc\HtmlListAdapter.uex' path='docs/doc[@for="HtmlListAdapter.RenderList"]/*' />
        protected virtual void RenderList(HtmlMobileTextWriter writer)
        {
            MobileListItemCollection items = Control.Items;
            Wrapping wrap = Style.Wrapping; // used for tables, no decoration case.
            if (items.Count == 0)
            {
                return;
            }

            int pageStart = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;

            // Determine what markup to use.

            String listSuffix, itemPrefix, itemSuffix;
            ListDecoration decoration = Control.Decoration;
            bool insideStyle = true;

            // We know that for table tag, outer style doesn't carry over
            // into inside tags so style state needs to be reset.
            // For other cases, we enter the style here once and exit at the
            // end once.
            if (decoration != ListDecoration.None || !Device.Tables)
            {
                writer.EnterStyle(Style);
                insideStyle = false;
            }

            switch (decoration)
            {
                case ListDecoration.Bulleted:
                    writer.WriteLine("<ul>");
                    listSuffix = "</ul>";
                    itemPrefix = "<li>";
                    itemSuffix = "</li>";

                    if (!Device.RendersBreaksAfterHtmlLists)
                    {
                        listSuffix += "<br>";
                    }
                    break;
                case ListDecoration.Numbered:
                    if (pageStart == 0)
                    {
                        writer.WriteLine("<ol>");
                    }
                    else
                    {
                        writer.Write("<ol start=\"");
                        writer.Write(pageStart + 1);
                        writer.WriteLine("\">");
                    }
                    listSuffix = "</ol>";
                    itemPrefix = "<li>";
                    itemSuffix = "</li>";

                    if (!Device.RendersBreaksAfterHtmlLists)
                    {
                        listSuffix += "<br>";
                    }
                    break;
                default:
                    if (Device.Tables)
                    {
                        listSuffix = "</table>";
                        Style.Wrapping = Wrapping.NotSet;
                        writer.EnterLayout(Style);
                        writer.WriteLine("<table>");
                        if(wrap == Wrapping.NoWrap)
                        {
                            itemPrefix = "<tr nowrap><td>";
                        }
                        else
                        {
                            itemPrefix = "<tr><td>";
                        }
                        itemSuffix = "</td></tr>";
                    }
                    else
                    {
                        listSuffix = String.Empty;
                        itemPrefix = String.Empty;
                        itemSuffix = "<br>";
                    }
                    break;
            }

            bool hasCmdHandler = Control.HasItemCommandHandler;

            for (int i = 0; i < pageSize; i++)
            {
                MobileListItem item = items[pageStart + i];
                writer.Write(itemPrefix);

                if(insideStyle)
                {
                    writer.BeginStyleContext();
                    writer.EnterFormat(Style);
                }

                if(Control.ItemsAsLinks)
                {
                    RenderBeginLink(writer, item.Value);
                }
                else if(hasCmdHandler) 
                {
                    writer.WriteBeginTag("a");
                    RenderPostBackEventAsAttribute(writer, "href", item.Index.ToString(CultureInfo.InvariantCulture));
                    writer.Write(">");
                } 
                writer.WriteEncodedText(item.Text);
                
                if (hasCmdHandler || Control.ItemsAsLinks)
                {
                    RenderEndLink(writer);
                }

                if(insideStyle)
                {
                    writer.ExitFormat(Style);
                    writer.EndStyleContext();
                }
                writer.WriteLine(itemSuffix);
            }

            if (listSuffix == null || listSuffix.Length > 0)
            {
                writer.WriteLine(listSuffix);
            }

            if (decoration != ListDecoration.None || !Device.Tables)
            {
                writer.ExitStyle(Style);
            }
            else
            {
                writer.ExitLayout(Style);
                Style.Wrapping = wrap;
            }
        }
    }
}
