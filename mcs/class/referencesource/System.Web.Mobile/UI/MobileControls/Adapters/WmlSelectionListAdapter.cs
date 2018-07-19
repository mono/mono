//------------------------------------------------------------------------------
// <copyright file="WmlSelectionListAdapter.cs" company="Microsoft">
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
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlSelectionListAdapter provides the wml device functionality for SelectionList controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlSelectionListAdapter : WmlControlAdapter
    {
        private String _ivalue = null;
        private const String ClientPrefix = "__slst_";

        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.Control"]/*' />
        protected new SelectionList Control
        {
            get
            {
                return (SelectionList)base.Control;
            }
        }

        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
        }

        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.OnPreRender"]/*' />
        public override void OnPreRender(EventArgs e)
        {
            int firstSelectedIndex;
            MobileListItemCollection items = Control.Items;
            int count = items.Count;
            for(firstSelectedIndex = 0; firstSelectedIndex < count; firstSelectedIndex++)
            {
                if(items[firstSelectedIndex].Selected)
                {
                    break;
                }
            }
            if(firstSelectedIndex < count)
            {
                StringBuilder ivalue=new StringBuilder();
                ivalue.Append((firstSelectedIndex + 1).ToString(CultureInfo.InvariantCulture));
                if(Control.IsMultiSelect) 
                {
                    for(int i = firstSelectedIndex + 1; i < count; i++)
                    {
                        if(items[i].Selected)
                        {
                            ivalue.Append(";");
                            ivalue.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }

                _ivalue = ivalue.ToString();
            }
            else
            {
                String defaultValue = null;

                if (!Control.IsMultiSelect)
                {
                    // 1 is the first index of a single selection list
                    defaultValue = "1";
                }
                else if (Device.CanRenderSetvarZeroWithMultiSelectionList)
                {
                    // 0 means no items have been selected, for MultiSelect case
                    defaultValue = "0";
                }

                if (defaultValue != null)
                {
                    _ivalue = defaultValue;
                }
            }            
        }

        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            MobileListItemCollection items = Control.Items;
            int count = items.Count;
            if (count == 0)
            {
                return;
            }           

            writer.EnterLayout(Style);
            bool crossPagePost = !String.IsNullOrEmpty(Control.Form.Action);

            if (crossPagePost)
            {
                if (_ivalue != null)
                {
                    String formVariable = ClientPrefix + Control.ClientID;
                    writer.AddFormVariable (formVariable, _ivalue, false);
                    // does not render _ivalue if null or form variables written.
                    writer.RenderBeginSelect(Control.ClientID, formVariable, _ivalue, Control.Title, Control.IsMultiSelect);
                }
                else // _ivalue == null
                {
                    writer.RenderBeginSelect(Control.ClientID, null, null, Control.Title, Control.IsMultiSelect);
                }
            }
            else // !crossPagePost
            {
                if (_ivalue != null)
                {
                    writer.AddFormVariable (Control.ClientID, _ivalue, false);
                }
                // does not render _ivalue if null or form variables written.
                writer.RenderBeginSelect(null, Control.ClientID, _ivalue, Control.Title, Control.IsMultiSelect);            
            }

            foreach (MobileListItem item in items)
            {
                if (crossPagePost)
                {
                    writer.RenderSelectOption(item.Text, item.Value);
                }
                else
                {
                    writer.RenderSelectOption(item.Text);
                }
            }
            writer.RenderEndSelect(Control.BreakAfter);
            writer.ExitLayout(Style);
        }

        // Parse the WML posted data appropriately.
        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.LoadPostData"]/*' />
        public override bool LoadPostData(String key,
                                          NameValueCollection data,
                                          Object controlPrivateData,
                                          out bool dataChanged)
        {
            int[] selectedItemIndices;
            String[] selectedItems = data.GetValues(key);
            Debug.Assert (String.IsNullOrEmpty(Control.Form.Action), 
                "Cross page post (!IsPostBack) assumed when Form.Action nonempty." +
                "LoadPostData should not have been be called.");
            // Note: controlPrivateData is selectedIndices from viewstate.
            int[] originalSelectedIndices = (int[])controlPrivateData;

            dataChanged = false;

            if (selectedItems == null)
            {
                // This shouldn't happen if we're posting back from the form that
                // contains the selection list. It could happen when being called 
                // as the result of a postback from another form on the page,
                // so we just return quietly.

                return true;
            }

            if (Control.Items.Count == 0)
            {
                return true;
            }

            // If singleselect && nothing was selected, select
            // first elt.  (Non-mobile DropDown does same by getting SelectedIndex).
            if(!Control.IsMultiSelect &&
               originalSelectedIndices.Length == 0 && 
               Control.Items.Count > 0)
            {
                Control.Items[0].Selected = true;
                originalSelectedIndices = new int[]{0};
            }

            
            // Case where nothing is selected.
            if(selectedItems == null ||
                (selectedItems.Length == 1 && 
                    (selectedItems[0] != null && selectedItems[0].Length == 0)) ||
                (selectedItems.Length == 1 && 
                    selectedItems[0] == "0"))  // non-selected MultiSelect case
            {
                selectedItems = new String[]{};
            }

            // WML multiselect case with more than one selection.
            if(selectedItems.Length == 1 && selectedItems[0].IndexOf(';') > -1)
            {
                String selected = selectedItems[0];
                // Eliminate trailing semicolon, if there is one.
                selected = Regex.Replace(selected, ";$", "");
                selectedItems = Regex.Split(selected, ";");
            }

            selectedItemIndices = new int[selectedItems.Length];
            for(int i = 0; i < selectedItems.Length; i++)
            {
                // WML iname gives index + 1, so subtract one back out. 
                selectedItemIndices[i] = Int32.Parse(selectedItems[i], CultureInfo.InvariantCulture) - 1;
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
                for(int i = 0; i < selectedItemIndices.Length; i++)
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

        /// <include file='doc\WmlSelectionListAdapter.uex' path='docs/doc[@for="WmlSelectionListAdapter.GetPostBackValue"]/*' />
        protected override String GetPostBackValue()
        {
            // Optimization - if viewstate is enabled for this control, and the
            // postback returns to this page, we just let it do the trick.

            if (Control.Form.Action.Length > 0 || !IsViewStateEnabled())
            {
                return _ivalue != null ? _ivalue : String.Empty;
            }
            else
            {
                return null;
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
