//------------------------------------------------------------------------------
// <copyright file="WmlListControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    
    public class WmlListControlAdapter : ListControlAdapter, IPostBackDataHandler {

        private const String ClientPrefix = "__slst_";        
        private string _ivalue = null;

        // Called during the PreRender page lifecycle phase.
        protected internal override void OnPreRender(EventArgs e) {
            int realCounter;
            int firstSelectedIndex;
            ListItemCollection items = Control.Items;
            int count = items.Count;

            for (firstSelectedIndex = realCounter = 0; realCounter < count; realCounter++) {

                if (items[firstSelectedIndex].Selected) {
                    break;
                }

                if (items[realCounter].Enabled) {
                    firstSelectedIndex++;
                }
            }

            if (firstSelectedIndex < count) {
                StringBuilder ivalue= new StringBuilder();
                ivalue.Append((firstSelectedIndex + 1).ToString(CultureInfo.InvariantCulture));
                if (IsMultiSelect) {
                    int i = 0;
                    for (i = realCounter = firstSelectedIndex + 1; realCounter < count; realCounter++) {
                        if (items[i].Selected) {
                            ivalue.Append(";");
                            ivalue.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                        }
                        if (items[realCounter].Enabled) {
                            i++;
                        }
                    }
                }

                _ivalue = ivalue.ToString();
            }
            else {
                String defaultValue = null;

                // For single select list, 1st element is initially selected
                // if no other selection.  1 is the first index
                if (!IsMultiSelect) {
                    defaultValue = "1";
                }

                if (defaultValue != null) {
                    _ivalue = defaultValue;
                }
            }

            base.OnPreRender(e);
        }

        protected virtual string GetInputElementText(ListItem item) {
            return item.Selected ? CheckBoxAdapter.AltSelectedText : CheckBoxAdapter.AltUnselectedText;
        }

        protected virtual void RenderDisabledItem(HtmlTextWriter writer, ListItem item) {
            string selectionText = GetInputElementText(item);
            string text = item.Text;
            bool renderSpace = text != null && text.Length > 0;
            bool leftTextAlign = (Control is CheckBoxList && ((CheckBoxList)Control).TextAlign == TextAlign.Left);

            if (leftTextAlign) {
                writer.WriteEncodedText(item.Text);
                if (renderSpace) {writer.Write(" ");}
                writer.WriteEncodedText(selectionText);
            }
            else {
                writer.WriteEncodedText(selectionText);
                if (renderSpace) {writer.Write(" ");}
                writer.WriteEncodedText(item.Text);
            }

            writer.WriteBreak();
        }

        protected internal override void Render(HtmlTextWriter markupWriter) {

            WmlTextWriter writer = (WmlTextWriter) markupWriter;

            ListItemCollection items = Control.Items;
            int count = items.Count;

            if (count == 0) {
                return;
            }

            writer.EnterStyle(Control.ControlStyle);
            bool selected = false;
            if (!Control.Enabled) {
                foreach (ListItem item in items) {
                    // VSWhidbey 115824
                    if (item.Selected) {
                        if (selected) {
                            Control.VerifyMultiSelect();
                        }
                        selected = true;
                    }
                    RenderDisabledItem(writer, item);
                }
            }
            else {

                // Only register post fields if the control is enabled.
                ((WmlPageAdapter)PageAdapter).RegisterPostField(writer, Control);

                if (_ivalue != null) {
                    ((WmlPageAdapter)PageAdapter).AddFormVariable(writer, Control.ClientID, _ivalue, false);
                }
                // does not render _ivalue if null or form variables written.
                writer.WriteBeginSelect(null /*name*/, 
                                        null /*value*/, 
                                        Control.ClientID /*iname*/, 
                                        _ivalue /*ivalue*/, 
                                        Control.ToolTip /*title*/, 
                                        IsMultiSelect);            

                foreach (ListItem item in items) {
                    // If the item is disabled, don't render it.
                    // WML only allows selectable <options> within <select> elements.
                    if (!item.Enabled) {
                        continue;
                    }

                    // VSWhidbey 115824
                    if (item.Selected) {
                        if (selected && !IsMultiSelect) {
                            throw new HttpException(SR.GetString(SR.Cant_Multiselect_In_Single_Mode));
                        }
                        selected = true;
                    }
                    RenderSelectOption(writer, item);
                }
                writer.WriteEndSelect();
            }

            writer.ExitStyle(Control.ControlStyle);
        }

        internal virtual void RenderSelectOption(WmlTextWriter writer, ListItem item) {
            if (Control.AutoPostBack) {
                ((WmlPageAdapter)PageAdapter).RenderSelectOptionAsAutoPostBack(writer, item.Text, null);
            }
            else {
                ((WmlPageAdapter)PageAdapter).RenderSelectOption(writer, item.Text);
            }
        }

        /// <internalonly/>
        // Implements IPostBackDataHandler.LoadPostData.
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        /// <internalonly/>
        // Implements IPostBackDataHandler.LoadPostData.
        protected virtual bool LoadPostData(String key, NameValueCollection data) {
            int[] selectedItemIndices;
            bool dataChanged = false;
            String[] selectedItems = data.GetValues(key);            

            if (selectedItems == null || Control.Items.Count == 0) {
                return false;
            }

            ArrayList originalSelection = Control.SelectedIndicesInternal;

            if (originalSelection == null) {
                originalSelection = new ArrayList();
            }

            // If singleselect && nothing was selected, select
            // first element.
            if (!IsMultiSelect &&
                originalSelection.Count == 0 &&
                Control.Items.Count > 0) {

                Control.Items[0].Selected = true;                
            }


            // Case where nothing is selected.
            if (selectedItems == null ||
                (selectedItems.Length == 1 && 
                 selectedItems[0] != null &&
                 ((String)selectedItems[0]).Length == 0) ||
                (selectedItems.Length == 1 && 
                 selectedItems[0] == "0")) {

                // non-selected MultiSelect case
                selectedItems = new String[]{};
            }

            // WML multiselect case with more than one selection.
            if (selectedItems.Length == 1 && selectedItems[0].IndexOf(';') > -1) {
                String selected = selectedItems[0];
                // Eliminate trailing semicolon, if there is one.
                selected = Regex.Replace(selected, ";$", String.Empty);
                selectedItems = Regex.Split(selected, ";");
            }

            selectedItemIndices = new int[selectedItems.Length];
            for (int i = 0; i < selectedItems.Length; i++) {
                // WML iname gives index + 1, so subtract one back out. 
                string selItem = selectedItems[i];
                selectedItemIndices[i] = Int32.Parse(selItem, CultureInfo.InvariantCulture) - 1;
            }

            // Do not assume posted selected indices are ascending.  
            // We do know originalSelectedIndices are ascending.
            Array.Sort(selectedItemIndices);

            // Check whether selections have changed.
            if (selectedItemIndices.Length != originalSelection.Count) {
                dataChanged = true;
            }
            else {
                for (int i = 0; i < selectedItemIndices.Length; i++) {
                    if (selectedItemIndices[i] != (int)originalSelection[i]) {
                        dataChanged = true;
                    }
                }
            }

            // Update selections
            Control.ClearSelection();
            for (int i = 0; i < selectedItemIndices.Length; i++) {
                Control.Items[selectedItemIndices[i]].Selected = true;
            }

            return dataChanged;
        }

        /// <internalonly/>
        // Implements IPostBackDataHandler.RaisePostDataChangedEvent.
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }        

        /// <internalonly/>
        // Implements IPostBackDataHandler.RaisePostDataChangedEvent.
        protected virtual void RaisePostDataChangedEvent() {
            ((IPostBackDataHandler)Control).RaisePostDataChangedEvent();
        }
    }
}

#endif

