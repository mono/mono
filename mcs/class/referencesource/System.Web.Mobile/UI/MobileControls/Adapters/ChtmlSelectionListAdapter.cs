//------------------------------------------------------------------------------
// <copyright file="ChtmlSelectionListAdapter.cs" company="Microsoft">
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
     * ChtmlSelectionListAdapter provides the chtml device functionality for SelectionList controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ChtmlSelectionListAdapter.uex' path='docs/doc[@for="ChtmlSelectionListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ChtmlSelectionListAdapter : HtmlSelectionListAdapter
    {
        /// <include file='doc\ChtmlSelectionListAdapter.uex' path='docs/doc[@for="ChtmlSelectionListAdapter.RequiresFormTag"]/*' />
        public override bool RequiresFormTag
        {
            get
            {
                // Some browsers require the form tag to display the selection
                // list properly
                return true;
            }
        }

        /// <include file='doc\ChtmlSelectionListAdapter.uex' path='docs/doc[@for="ChtmlSelectionListAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            ListSelectType selectType = Control.SelectType;
            if (selectType == ListSelectType.MultiSelectListBox && 
                Device.SupportsSelectMultiple == false)
            {
                // Render occurs after SaveViewState.  Here we make a temp
                // change which is not persisted to the view state.
                Control.SelectType = selectType = ListSelectType.CheckBox;
            }

            if (!Device.RequiresUniqueHtmlCheckboxNames ||
                selectType != ListSelectType.CheckBox)
            {
                base.Render(writer);
            }
            else
            { 
                MobileListItemCollection items = Control.Items;
                if (items.Count == 0)
                {
                    return;
                }
                writer.EnterStyle(Style);
                bool writeBreak = false;
                foreach (MobileListItem item in items)
                {
                    int index = items.IndexOf(item);
                    if(writeBreak)
                    {
                        writer.WriteBreak();
                    }

                    writer.Write("<input type=\"checkbox\" name=\"");
                    if(Device.RequiresAttributeColonSubstitution)
                    {
                        writer.Write(Control.UniqueID.Replace(':', ','));
                    }
                    else
                    {
                        writer.Write(Control.UniqueID);
                    }
                    writer.Write(Constants.SelectionListSpecialCharacter);
                    writer.Write(index);
                    writer.Write("\" value=\"");
                    if (!String.IsNullOrEmpty(Control.Form.Action))
                    {
                        writer.WriteEncodedText(item.Value);
                    }
                    else
                    {
                        writer.Write(item.Index.ToString(CultureInfo.InvariantCulture));
                    }
                    if (item.Selected &&
                        Device.SupportsUncheck)
                    {
                        writer.Write("\" checked>");
                    }
                    else
                    {
                        writer.Write("\">");
                    }

                    writer.WriteText(item.Text, true);
                    writeBreak = true;
                }
                writer.ExitStyle(Style, Control.BreakAfter);
            }
        }
    }
}
