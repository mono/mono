//------------------------------------------------------------------------------
// <copyright file="WmlRadioButtonAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Web.Util;

    public class WmlRadioButtonAdapter : WmlCheckBoxAdapter, IPostBackDataHandler {


        private const String _groupPrefix = "__rb_";

        protected new RadioButton Control {
            get {
                return (RadioButton)base.Control;
            }
        }

        protected string GroupFormVariable  {
            get  {
                return _groupPrefix + Control.UniqueGroupName;
            }
        }

        private void DetermineGroup(RadioButton r) {
            if (RadioButtonGroups[r.UniqueGroupName] == null) {
                RadioButtonGroups[r.UniqueGroupName] = RenderAsGroup(r);
            }
        }

        private RadioButtonGroup GetGroupByName(string groupName) {
            Debug.Assert(RadioButtonGroups.Contains(groupName), "Attemping to get group name without procesing group");
            return RadioButtonGroups[groupName] as RadioButtonGroup;
        }

        // Returns a textual representation of a textbox.
        protected override string InputElementText {
            get {
                return Control.Checked ? RadioButtonAdapter.AltSelectedText : RadioButtonAdapter.AltUnselectedText;
            }
        }

        private bool IsWhiteSpace(string s) {
            for (int i = 0; i < s.Length; i++) {
                if (!Char.IsWhiteSpace(s, i)) return false;
            }
            return true;
        }

        private IDictionary RadioButtonGroups {
            get {
                if (Control.Page != null && Control.Page.Items[this.GetType()] == null) {
                    Control.Page.Items[this.GetType()] = new HybridDictionary();
                }
                return (HybridDictionary)Control.Page.Items[this.GetType()];
            }
        }

        // Renders the control.
        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            DetermineGroup(Control);
            RadioButtonGroup group = GetGroupByName(Control.UniqueGroupName);
            if (group != null) {
                if (!group.RegisteredGroup) {
                    // GroupFormVariable is passed as the name & value becuase it is both the key
                    // for the postback data, and the WML client side var used to
                    // select an item in the list.
                    ((WmlPageAdapter)PageAdapter).RegisterPostField(writer, GroupFormVariable, GroupFormVariable, true /*dynamic field*/, false /*random*/);
                    group.RegisteredGroup = true;
                }
                if (group.RenderAsGroup) {

                    // Render opening select if not already opened
                    if (!group.RenderedSelect) {
                        if (group.SelectedButton != null) {
                            ((WmlPageAdapter)PageAdapter).AddFormVariable(writer, GroupFormVariable, group.SelectedButton, false /*random*/);
                        }
                        writer.WriteBeginSelect(GroupFormVariable, group.SelectedButton, null /*iname */,
                                                null /*ivalue*/, Control.ToolTip, false /*multiple */);                      
                        if (!writer.AnalyzeMode) {
                            group.RenderedSelect = true; 
                        }
                    }

                    // Render option 
                    // We don't do autopostback if the radio button has been selected.
                    // This is to make it consistent that it only posts back if its
                    // state has been changed.  Also, it avoids the problem of missing
                    // validation since the data changed event would not be fired if the
                    // selected radio button was posting back.
                    if (Control.AutoPostBack && !Control.Checked) {
                        ((WmlPageAdapter)PageAdapter).RenderSelectOptionAsAutoPostBack(writer, Control.Text, Control.UniqueID);
                    }
                    else {
                        ((WmlPageAdapter)PageAdapter).RenderSelectOption(writer, Control.Text, Control.UniqueID);
                    }

                    // Close, if list is finished
                    if (!writer.AnalyzeMode && --group.ButtonsInGroup == 0) {
                        writer.WriteEndSelect();           
                    }

                }
                // must render as autopostback, radio buttons not in consecutive group.
                else {
                    if (!Control.Enabled) {
                        RenderDisabled(writer);
                        return;
                    }

                    string iname = Control.Checked ? Control.ClientID : null;
                    string ivalue = Control.Checked ? "1" : null;
                    if (ivalue != null) {
                        ((WmlPageAdapter)PageAdapter).AddFormVariable(writer, iname, ivalue, false /*random*/);
                    }

                    writer.WriteBeginSelect(null, null, iname, ivalue, Control.ToolTip, true /*multiple*/);
                    if (!Control.Checked) {
                        ((WmlPageAdapter)PageAdapter).RenderSelectOptionAsAutoPostBack(writer, Control.Text, GroupFormVariable, Control.UniqueID);
                    }
                    else {
                        ((WmlPageAdapter)PageAdapter).RenderSelectOption(writer, Control.Text, Control.UniqueID);
                    }
                    writer.WriteEndSelect();
                }
            }
        }

        // RenderAsGroup returns a RadioButtonGroup object if the group should be 
        // rendered in a single <select> statement, or null if autopostback should
        // be enabled.
        // 
        private RadioButtonGroup RenderAsGroup(RadioButton r) {
            bool startedSequence = false;
            bool finishedSequence = false;

            RadioButtonGroup group = new RadioButtonGroup();

            //
            foreach (Control c in r.Parent.Controls) {
                RadioButton radioSibling = c as RadioButton;
                LiteralControl literalSibling = c as LiteralControl;
                if (radioSibling != null && radioSibling.UniqueGroupName == r.UniqueGroupName) {
                    startedSequence = true;
                    group.ButtonsInGroup++;

                    if (radioSibling.Checked == true) {
                        group.SelectedButton = radioSibling.UniqueID;
                    }
                    if (finishedSequence || !radioSibling.Enabled) {
                        group.ButtonsInGroup = -1; // can't be rendered in a group
                        break;
                    }
                }
                else if (startedSequence && (literalSibling == null || !IsWhiteSpace(literalSibling.Text))) {
                    finishedSequence = true;
                }
            }
            return group;
        }

        /// <internalonly/>
        // Implements IPostBackDataHandler.LoadPostData.
        protected override bool LoadPostData(String key, NameValueCollection data) {
            bool dataChanged = false;

            string selButtonID = data[GroupFormVariable];
            if (!String.IsNullOrEmpty(selButtonID)) {

                // Check if this radio button is now checked
                if (StringUtil.EqualsIgnoreCase(selButtonID, Control.UniqueID)) {

                    if (Control.Checked == false) {
                        Control.Checked = true;
                        dataChanged = true;
                    }
                }
                else {
                    // Don't need to check if radiobutton was
                    // previously checked.  We only raise an event
                    // for the radiobutton that is selected.
                    // This is how a normal RadioButton behaves.
                    Control.Checked = false;
                }
            }
            return dataChanged;
        }

        /// <internalonly/>
        // Implements IPostBackDataHandler.RaisePostDataChangedEvent()
        protected override void RaisePostDataChangedEvent() {
            ((IPostBackDataHandler)Control).RaisePostDataChangedEvent();
        }
    }

    internal class RadioButtonGroup {
        public int      ButtonsInGroup;
        public bool     RenderedSelect;
        public bool     RegisteredGroup;
        public String   SelectedButton;

        public bool RenderAsGroup  {
            get  {
                if (ButtonsInGroup < 0) {
                    return false;
                }
                return true;
            }
        }
    }
}

#endif

