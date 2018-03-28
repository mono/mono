//------------------------------------------------------------------------------
// <copyright file="HtmlSelectionListAdapter.cs" company="Microsoft">
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{
    /*
     * HtmlSelectionListAdapter provides the html device functionality for SelectionList controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlSelectionListAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter.Control"]/*' />
        protected new SelectionList Control
        {
            get
            {
                return (SelectionList)base.Control;
            }
        }

        /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
        }

        /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            MobileListItemCollection items = Control.Items;
            ListSelectType selectType = Control.SelectType;

            if (items.Count == 0 && 
                selectType != ListSelectType.ListBox && 
                selectType != ListSelectType.MultiSelectListBox)
            {
                return;
            }
            
            int selectedIndex = Control.SelectedIndex;
            String renderName;
            if(Device.RequiresAttributeColonSubstitution)
            {
                renderName = Control.UniqueID.Replace(':', ',');
            }
            else
            {
                renderName = Control.UniqueID;
            }

            switch(selectType)
            {
                case ListSelectType.DropDown:
                case ListSelectType.ListBox:
                case ListSelectType.MultiSelectListBox:

                    if (items.Count == 0 && !Device.CanRenderEmptySelects)
                    {
                        break;
                    }

                    writer.EnterLayout(Style);
                    writer.WriteBeginTag("select");

                    if (selectType == ListSelectType.MultiSelectListBox)
                    {
                        writer.Write(" multiple");
                    }

                    if (selectType == ListSelectType.ListBox || selectType == ListSelectType.MultiSelectListBox)
                    {
                        writer.WriteAttribute("size", Control.Rows.ToString(CultureInfo.InvariantCulture));
                    }

                    writer.WriteAttribute("name", renderName);
                    writer.Write(">");

                    for(int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                    {
                        MobileListItem item = items[itemIndex];
                        writer.WriteBeginTag("option");
                        WriteItemValueAttribute(writer, itemIndex, item.Value);
                        if (item.Selected && (Control.IsMultiSelect || itemIndex == selectedIndex))
                        {
                            writer.Write(" selected>");
                        }
                        else
                        {
                            writer.Write(">");
                        }
                        writer.WriteEncodedText(item.Text);
                        writer.WriteLine("");
                    }
                    writer.Write("</select>");
            
                    if(Device.HidesRightAlignedMultiselectScrollbars &&
                        selectType == ListSelectType.MultiSelectListBox)
                    {
                        // nested if for perf
                        if((Alignment)Style[Style.AlignmentKey, true] == Alignment.Right)
                        {                                                
                            writer.Write("&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                    }
                    writer.WriteLine("");
                    
                    if (!Page.DesignMode)
                    {
                        writer.ExitLayout(Style, Control.BreakAfter);
                    }
                    else
                    {
                        writer.ExitLayout(Style, false);
                    }
                    break;

                case ListSelectType.Radio:
                case ListSelectType.CheckBox:

                    String selectTypeString =
                        (selectType == ListSelectType.Radio) ?
                        "radio" :
                        "checkbox";
                    Alignment alignment = (Alignment)Style[Style.AlignmentKey, true];
                    if(!Device.Tables || alignment == Alignment.Left || alignment == Alignment.NotSet)
                    {
                        writer.EnterStyle(Style);
                        bool breakAfter = false;
                        for(int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                        {
                            if(breakAfter)
                            {
                                writer.WriteBreak();
                            }
                            MobileListItem item = items[itemIndex];

                            writer.WriteBeginTag("input");
                            writer.WriteAttribute("type", selectTypeString);
                            writer.WriteAttribute("name", renderName);
                            WriteItemValueAttribute(writer, itemIndex, item.Value);
                            if (item.Selected && 
                                (Control.IsMultiSelect || itemIndex == selectedIndex) &&
                                Device.SupportsUncheck)
                            {
                                writer.Write(" checked>");
                            }
                            else
                            {
                                writer.Write(">");
                            }
                            writer.WriteEncodedText(item.Text);
                            breakAfter = true;
                        }
                        writer.ExitStyle(Style, Control.BreakAfter);
                    }
                    else // Device supports tables and alignment is non default.
                    {
                        Wrapping  wrapping  = (Wrapping) Style[Style.WrappingKey , true];
                        bool nowrap = (wrapping == Wrapping.NoWrap);

                        writer.EnterLayout(Style);
                        writer.WriteFullBeginTag("table");
                        writer.BeginStyleContext();
                        for(int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                        {
                            MobileListItem item = items[itemIndex];
                            writer.WriteFullBeginTag("tr");
                            writer.WriteBeginTag("td");
                            if(nowrap)
                            {
                                writer.WriteAttribute("nowrap","true");
                            }
                            writer.Write(">");

                            writer.WriteBeginTag("input");
                            writer.WriteAttribute("type", selectTypeString);
                            writer.WriteAttribute("name", renderName);
                            WriteItemValueAttribute(writer, itemIndex, item.Value);
                            if (item.Selected && 
                                (Control.IsMultiSelect || itemIndex == selectedIndex) &&
                                Device.SupportsUncheck)
                            {
                                writer.Write(" checked>");
                            }
                            else
                            {
                                writer.Write(">");
                            }

                            writer.MarkStyleContext();
                            writer.EnterFormat(Style);
                            writer.WriteEncodedText(item.Text);
                            writer.ExitFormat(Style);
                            writer.UnMarkStyleContext();
                            writer.WriteEndTag("td");
                            writer.WriteEndTag("tr");
                        }
                        writer.WriteEndTag("table");
                        writer.EndStyleContext();
                        writer.ExitFormat(Style, Control.BreakAfter);
                    }
                break;
            }
        }

        // Parse the HTML posted data appropriately.
        /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter.LoadPostData"]/*' />
        public override bool LoadPostData(String key,
                                          NameValueCollection data,
                                          Object controlPrivateData,
                                          out bool dataChanged)
        {
            String[] selectedItems = data.GetValues(key);
            Debug.Assert (String.IsNullOrEmpty(Control.Form.Action), 
                "Cross page post (!IsPostBack) assumed when Form.Action nonempty." +
                "LoadPostData should not have been be called.");

            // If no post data is included, and the control is either not visible, or
            // not on active form, this call should be ignored (the lack of post data 
            // is not due to there being no selection, but due to there being no 
            // markup rendered that could generate the post data).

            if (selectedItems == null && 
                    (!Control.Visible || Control.Form != Control.MobilePage.ActiveForm))
            {
                dataChanged = false;
                return true;
            }

            // Case where nothing is selected.
            if(selectedItems == null ||
               (selectedItems.Length == 1 && (selectedItems[0] != null && selectedItems[0].Length == 0)))
            {
                selectedItems = new String[]{};
            }

            int[] selectedItemIndices = new int[selectedItems.Length];

            // Note: controlPrivateData is selected indices from the viewstate.
            int[] originalSelectedIndices = (int[])controlPrivateData;
            dataChanged = false;

            // If SelectType is DropDown && nothing was selected, select
            // first elt.  (Non-mobile DropDown does same by getting SelectedIndex).
            if(Control.SelectType == ListSelectType.DropDown &&
               originalSelectedIndices.Length == 0 &&
               Control.Items.Count > 0)
            {
                Control.Items[0].Selected = true;
                originalSelectedIndices = new int[]{0};
            }

            for(int i = 0; i < selectedItems.Length; i++)
            {
                selectedItemIndices[i] = Int32.Parse(selectedItems[i], CultureInfo.InvariantCulture);
            }

            // Do not assume posted selected indices are ascending.
            // We do know originalSelectedIndices are ascending.
            Array.Sort(selectedItemIndices);

            // Check whether selections have changed.
            if(selectedItemIndices.Length != originalSelectedIndices.Length)
            {
                dataChanged = true;
            }
            else
            {
                for(int i = 0; i < selectedItems.Length; i++)
                {
                    if(selectedItemIndices[i] != originalSelectedIndices[i])
                    {
                        dataChanged = true;
                    }
                }
            }

            for (int i = 0; i < Control.Items.Count; i++)
            {
                Control.Items[i].Selected = false;
            }

            for(int i = 0; i < selectedItemIndices.Length; i++)
            {
                Control.Items[selectedItemIndices[i]].Selected = true;
            }
            return true;
        }

        private void WriteItemValueAttribute(HtmlTextWriter writer, int index, String value)
        {
            if (Page.DesignMode || String.IsNullOrEmpty(Control.Form.Action))
            {
                writer.WriteAttribute("value", index.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("value", value, true /*encode*/);
            }
        }

        /// <include file='doc\HtmlSelectionListAdapter.uex' path='docs/doc[@for="HtmlSelectionListAdapter.RenderAsHiddenInputField"]/*' />
        protected override void RenderAsHiddenInputField(HtmlMobileTextWriter writer)
        {
            // Optimization - if viewstate is enabled for this control, and the
            // postback returns to this page, we just let it do the trick.
            // One catch though - if the control is multiselect, it always 
            // interprets return values, so we do need to write out.

            if (Control.IsMultiSelect || Control.Form.Action.Length > 0 || !IsViewStateEnabled())
            {
                String uniqueID = Control.UniqueID;
                MobileListItemCollection items = Control.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Selected)
                    {
                        writer.WriteHiddenField(uniqueID, i.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private bool IsViewStateEnabled()
        {
            Control ctl = Control;
            while (ctl != null)
            {
                if (!ctl.EnableViewState)
                {
                    return false;
                }
                ctl = ctl.Parent;
            }
            return true;
        }
   }
}
