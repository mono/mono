//------------------------------------------------------------------------------
// <copyright file="DeviceFilterEditorDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Web.UI;
//    using System.Web.UI.Design.Util;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;
    using System.Web.UI.Design.MobileControls.Util;

    using Control = System.Windows.Forms.Control;
    using Label = System.Windows.Forms.Label;
    using TextBox = System.Windows.Forms.TextBox;
    using DeviceFilterMode = Util.DeviceFilterNode.DeviceFilterMode;

    /// <summary>
    ///   The General page for the TextView control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class DeviceFilterEditorDialog : DesignerForm
    {
        /// <summary>
        ///   Initializes the UI of the form.
        /// </summary>
        
        private EditableTreeList        _filterList = null;
        private DefaultDialogButtons    _dialogButtons = null;
        private WebConfigManager        _webConfig = null;
        private ISite                   _site = null;

        private static readonly String _nameOfDefaultFilter =
            SR.GetString(SR.DeviceFilter_DefaultChoice);

        private System.Windows.Forms.Label _lblCompare;
        private System.Windows.Forms.ComboBox _cbCompare;
        private System.Windows.Forms.Label _lblArgument;
        private System.Windows.Forms.TextBox _txtArgument;
        private System.Windows.Forms.Panel _pnlCompare;
        private System.Windows.Forms.Label _lblType;
        private System.Windows.Forms.Label _lblMethod;
        private System.Windows.Forms.TextBox _txtMethod;
        private System.Windows.Forms.TextBox _txtType;
        private System.Windows.Forms.Panel _pnlDelegate;
        private System.Windows.Forms.RadioButton _rbDelegate;
        private System.Windows.Forms.RadioButton _rbCompare;
        private System.Windows.Forms.Panel _pnlRight;
        private System.Windows.Forms.Panel _pnlMain;
        private GroupLabel _glAttributes;
        private GroupLabel _glType;
        private HeaderPanel _pnlHeader;
        private HeaderLabel _lblHeader;

        internal DeviceFilterEditorDialog(ISite site)
            : this(site, new WebConfigManager(site))
        {
        }
        
        // NOTE: A FileLoadException is thrown if an error occurs while reading
        //       web.config.
        internal DeviceFilterEditorDialog(ISite site, WebConfigManager webConfig) : base(site)
        {
            InitializeComponent();

            _lblArgument.Text = SR.GetString(SR.DeviceFilterEditorDialog_Argument);
            _glAttributes.Text = SR.GetString(SR.DeviceFilterEditorDialog_Attributes);
            _lblMethod.Text = SR.GetString(SR.DeviceFilterEditorDialog_Method);
            _glType.Text = SR.GetString(SR.DeviceFilterEditorDialog_TypeGl);
            _rbCompare.Text = SR.GetString(SR.DeviceFilterEditorDialog_Equality);
            _lblCompare.Text = SR.GetString(SR.DeviceFilterEditorDialog_Compare);
            _rbDelegate.Text = SR.GetString(SR.DeviceFilterEditorDialog_Evaluator);
            _lblType.Text = SR.GetString(SR.DeviceFilterEditorDialog_TypeTxt);
            _lblHeader.Text = SR.GetString(SR.DeviceFilterEditorDialog_Header);
            this.Text = SR.GetString(SR.DeviceFilterEditorDialog_Title);

            int tabOffset = 0;
            this._pnlMain.TabIndex = tabOffset++;
            this._filterList.TabIndex = tabOffset++;
            this._pnlRight.TabIndex = tabOffset++;
            this._glType.TabIndex = tabOffset++;
            this._rbCompare.TabIndex = tabOffset++;
            this._rbDelegate.TabIndex = tabOffset++;
            this._glAttributes.TabIndex = tabOffset++;
            this._pnlCompare.TabIndex = tabOffset++;
            this._pnlDelegate.TabIndex = tabOffset++;
            this._lblCompare.TabIndex = tabOffset++;
            this._cbCompare.TabIndex = tabOffset++;
            this._lblType.TabIndex = tabOffset++;
            this._txtType.TabIndex = tabOffset++;
            this._lblArgument.TabIndex = tabOffset++;
            this._txtArgument.TabIndex = tabOffset++;
            this._lblMethod.TabIndex = tabOffset++;
            this._txtMethod.TabIndex = tabOffset++;
            this._dialogButtons.TabIndex = tabOffset++;
            
            _webConfig = webConfig;
            this._site = site;
            GenericUI.InitDialog(this, site);

            _filterList.LblTitle.Text = SR.GetString(SR.DeviceFilterEditorDialog_DeviceFilters);
            _filterList.BtnAdd.Text = SR.GetString(SR.DeviceFilterEditorDialog_NewDeviceFilter);

            // Attempt to load Device Filters
            ArrayList filters = null;
            try 
            {
                filters = _webConfig.ReadDeviceFilters();
            }
            catch (FileNotFoundException e)
            {
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_WebConfigMissingOnOpen)
                );
                throw new FileLoadException(
                    SR.GetString(SR.WebConfig_FileLoadException, e)
                );
            }
            catch (Exception e)
            {
                if (e.Message.Equals(SR.GetString(SR.DeviceFilterEditorDialog_DuplicateNames)))
                {
                    GenericUI.ShowWarningMessage(
                        SR.GetString(SR.DeviceFilterEditorDialog_Title),
                        SR.GetString(SR.DeviceFilterEditorDialog_DuplicateNames)
                    );
                }
                else
                {
                    GenericUI.ShowWarningMessage(
                        SR.GetString(SR.DeviceFilterEditorDialog_Title),
                        SR.GetString(
                            SR.DeviceFilterEditorDialog_WebConfigParsingError,
                            e.Message
                        )
                    );
                }
                throw new FileLoadException(
                    SR.GetString(SR.WebConfig_FileLoadException, e)
                );
            }

            // Make sure web.config is checked out before we make changes.
            _webConfig.EnsureWebConfigCheckedOut();

            // Insert the Device Filters into the List UI
            foreach(DeviceFilterNode filter in filters)
            {
                DeviceFilterTreeNode node = new DeviceFilterTreeNode(filter);
                _filterList.TvList.Nodes.Add(node);
            }

            // Make sure all filters have a name...
            // NOTE: Do not combine with the above loop or GetUniqueLabel()
            //       will not necessarily be unique.  It could be done if
            //       we wrote another implementation of GetUniqueLabel()
            //       that compared against filters [ArrayList returned
            //       from ReadDeviceFilters()].
            foreach(DeviceFilterTreeNode node in _filterList.TvList.Nodes)
            {
                if(String.IsNullOrEmpty(node.Text))
                {
                    node.Text = _filterList.GetUniqueLabel(
                        SR.GetString(SR.DeviceFilterNode_DefaultFilterName)
                    );
                }
            }
                
            // Initialize the UI
            _rbCompare.Click += new EventHandler(OnClickCompareRadioButton);
            _rbDelegate.Click += new EventHandler(OnClickDelegateRadioButton);
            _cbCompare.TextChanged += new EventHandler(OnTextChanged);
            _cbCompare.SelectedIndexChanged += new EventHandler(OnTextChanged);
            _txtArgument.TextChanged += new EventHandler(OnTextChanged);
            _txtType.TextChanged += new EventHandler(OnTextChanged);
            _txtMethod.TextChanged += new EventHandler(OnTextChanged);
            _filterList.TvList.AfterLabelEdit += new NodeLabelEditEventHandler(OnAfterLabelEdit);
            _filterList.TvList.AfterSelect += new TreeViewEventHandler(OnFilterSelected);
            _filterList.BtnAdd.Click += new EventHandler(OnClickAddButton);
            _filterList.BtnRemove.Click += new EventHandler(OnClickRemoveButton);
            _filterList.TvList.SelectedNode = null;

            LoadAvailableCapabilities();
            UpdateButtonsEnabling();

            _dialogButtons.CmdOK.Click += new EventHandler(OnClickOK);
            _dialogButtons.CmdCancel.Click += new EventHandler(OnClickCancel);
        }

        protected override string HelpTopic {
            get {
                return "net.Mobile.DeviceFilterEditorDialog"; 
            }
        }

        private void InitializeComponent()
        {
            this._pnlHeader = new HeaderPanel();
            this._lblHeader = new HeaderLabel();
            this._glAttributes = new GroupLabel();
            this._glType = new GroupLabel();
            this._txtType = new System.Windows.Forms.TextBox();
            this._pnlMain = new System.Windows.Forms.Panel();
            this._filterList = new EditableTreeList();
            this._rbCompare = new System.Windows.Forms.RadioButton();
            this._lblCompare = new System.Windows.Forms.Label();
            this._dialogButtons = new DefaultDialogButtons();
            this._lblType = new System.Windows.Forms.Label();
            this._txtMethod = new System.Windows.Forms.TextBox();
            this._txtArgument = new System.Windows.Forms.TextBox();
            this._pnlRight = new System.Windows.Forms.Panel();
            this._lblMethod = new System.Windows.Forms.Label();
            this._rbDelegate = new System.Windows.Forms.RadioButton();
            this._pnlCompare = new System.Windows.Forms.Panel();
            this._cbCompare = new System.Windows.Forms.ComboBox();
            this._lblArgument = new System.Windows.Forms.Label();
            this._pnlDelegate = new System.Windows.Forms.Panel();
            this._txtType.Location = new System.Drawing.Point(0, 20);
            this._txtType.Size = new System.Drawing.Size(211, 20);

            this._lblHeader.Location = new System.Drawing.Point(0, 0);
            this._lblHeader.Size = new System.Drawing.Size(434, 16);
            this._lblHeader.Anchor = (System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left);
            this._pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._lblHeader
            });
            this._pnlHeader.Location = new System.Drawing.Point(6, 5);
            this._pnlHeader.Size = new System.Drawing.Size(434, 16);
            this._pnlHeader.Anchor = (System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left);
            this._pnlMain.Controls.AddRange(new System.Windows.Forms.Control[] {this._dialogButtons,
                                                                                  this._pnlRight,
                                                                                  this._filterList});
            this._pnlMain.Location = new System.Drawing.Point(6, 27);
            this._pnlMain.Size = new System.Drawing.Size(434, 253);
            this._pnlMain.Anchor = (System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left);
            this._filterList.Size = new System.Drawing.Size(198, 224);
            this._filterList.Location = new System.Drawing.Point(0, 0);
            this._rbCompare.Location = new System.Drawing.Point(8, 21);
            this._rbCompare.Size = new System.Drawing.Size(211, 17);
            this._lblCompare.Location = new System.Drawing.Point(0, 4);
            this._lblCompare.Size = new System.Drawing.Size(211, 16);
            this._dialogButtons.Location = new System.Drawing.Point(197, 230);
            this._dialogButtons.Size = new System.Drawing.Size(237, 23);
            this._lblType.Location = new System.Drawing.Point(0, 4);
            this._lblType.Size = new System.Drawing.Size(211, 16);
            this._txtMethod.Location = new System.Drawing.Point(0, 64);
            this._txtMethod.Size = new System.Drawing.Size(211, 20);
            this._txtArgument.Location = new System.Drawing.Point(0, 64);
            this._txtArgument.Size = new System.Drawing.Size(211, 20);
            this._pnlRight.Controls.AddRange(new System.Windows.Forms.Control[] {this._pnlCompare,
                                                                                 this._pnlDelegate,
                                                                                 this._glAttributes,
                                                                                 this._glType,
                                                                                 this._rbDelegate,
                                                                                 this._rbCompare});
            this._pnlRight.Location = new System.Drawing.Point(215, 0);
            this._pnlRight.Size = new System.Drawing.Size(219, 226);
            this._lblMethod.Location = new System.Drawing.Point(0, 48);
            this._lblMethod.Size = new System.Drawing.Size(211, 16);
            this._glAttributes.Location = new System.Drawing.Point(0, 73);
            this._glAttributes.Size = new System.Drawing.Size(216, 16);
            this._rbDelegate.Location = new System.Drawing.Point(8, 46);
            this._rbDelegate.Size = new System.Drawing.Size(211, 17);
            this._glType.Size = new System.Drawing.Size(216, 16);
            this._pnlCompare.Controls.AddRange(new System.Windows.Forms.Control[] {this._txtArgument,
                                                                                     this._lblArgument,
                                                                                     this._cbCompare,
                                                                                     this._lblCompare});
            this._pnlCompare.Location = new System.Drawing.Point(8, 90);
            this._pnlCompare.Size = new System.Drawing.Size(211, 136);
            this._cbCompare.DropDownWidth = 211;
            this._cbCompare.Location = new System.Drawing.Point(0, 20);
            this._cbCompare.Size = new System.Drawing.Size(211, 21);
            this._cbCompare.Sorted = true;
            this._lblArgument.Location = new System.Drawing.Point(0, 48);
            this._lblArgument.Size = new System.Drawing.Size(211, 16);
            this._pnlDelegate.Controls.AddRange(new System.Windows.Forms.Control[] {this._txtType,
                                                                                      this._txtMethod,
                                                                                      this._lblMethod,
                                                                                      this._lblType});
            this._pnlDelegate.Location = new System.Drawing.Point(8, 90);
            this._pnlDelegate.Size = new System.Drawing.Size(211, 136);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.ClientSize = new System.Drawing.Size(448, 289);
            this.AcceptButton = _dialogButtons.CmdOK;
            this.CancelButton = _dialogButtons.CmdCancel;
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this._pnlHeader, this._pnlMain});
        }
        
        private void LoadAvailableCapabilities()
        {
            Type type = typeof(System.Web.Mobile.MobileCapabilities);
            PropertyInfo[] properties = type.GetProperties();
            foreach(PropertyInfo property in properties)
            {
                _cbCompare.Items.Add(property.Name);
            }
        }

        private void UpdateButtonsEnabling()
        {
            _filterList.UpdateButtonsEnabling();
            bool filterIsSelected = (_filterList.SelectedNode != null);
            _rbCompare.Enabled = filterIsSelected;
            _rbDelegate.Enabled = filterIsSelected;
            _cbCompare.Enabled = filterIsSelected;
            _txtArgument.Enabled = filterIsSelected;
            _txtMethod.Enabled = filterIsSelected;
            _txtType.Enabled = filterIsSelected;
        }

        internal void SelectFilterByName(String name)
        {
            foreach(DeviceFilterTreeNode filter in _filterList.TvList.Nodes)
            {
                if(filter.DeviceFilter.Name == name)
                {
                    _filterList.TvList.SelectedNode = filter;
                    break;
                }
            }
        }

        private enum RequirementFlag
        {
            NotAllowed,
            Required,
            Optional
        };

        private bool FilterIsLegal_CheckRow(RequirementFlag[] row1, bool[] row2)
        {
            Debug.Assert(row1.Length == row2.Length);
            for(int i = 0; i < row1.Length; i++)
            {
                if(row1[i] == RequirementFlag.NotAllowed && row2[i] == true)
                {
                    return false;
                }
                else if(row1[i] == RequirementFlag.Required && row2[i] == false)
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool FilterIsLegal(DeviceFilterNode filter)
        {
            Object[] legalCombinations = {
                new RequirementFlag[] {
                    RequirementFlag.Required,         // compare mode
                    RequirementFlag.Required,         // compare
                    RequirementFlag.Optional,         // argument
                    RequirementFlag.Optional,         // method
                    RequirementFlag.Optional          // type
                },
                new RequirementFlag[] {
                    RequirementFlag.NotAllowed,       // compare mode
                    RequirementFlag.Optional,         // compare
                    RequirementFlag.Optional,         // argument
                    RequirementFlag.Required,         // method
                    RequirementFlag.Required          // type
                }
            };
            
            bool[] filterCombination = {
                (filter.Mode == DeviceFilterMode.Compare),
                ((filter.Compare != null) && (filter.Compare.Length > 0)),
                ((filter.Argument != null) && (filter.Argument.Length > 0)),
                ((filter.Type != null) && (filter.Type.Length > 0)),
                ((filter.Method != null) && (filter.Method.Length > 0)),
            };
            
            foreach(RequirementFlag[] legalCombination in legalCombinations)
            {
                if(FilterIsLegal_CheckRow(legalCombination, filterCombination))
                {
                    return true;
                }
            }
            return false;
        }

        #if DEBUG
        private bool Debug_DuplicateFiltersExist(ICollection filters)
        {
            // Filter names are case-sensitive.
            IDictionary namesEncountered = new Hashtable();            
            foreach(DeviceFilterTreeNode node in filters)
            {
                DeviceFilterNode filter = node.DeviceFilter;
                if(namesEncountered[filter.Name] != null)
                {
                    return true;
                }
                namesEncountered[filter.Name] = true;
            }
            return false;
        }
        #endif

        private bool FiltersAreValid()
        {
            #if DEBUG
            Debug.Assert(
                !Debug_DuplicateFiltersExist(_filterList.TvList.Nodes),
                "UI failed to prevent duplicate filters from being created."
            );
            #endif

            ArrayList filtersInErrorList = new ArrayList();
            foreach(DeviceFilterTreeNode filterNode in _filterList.TvList.Nodes)
            {
                DeviceFilterNode filter = filterNode.DeviceFilter;
                if(!FilterIsLegal(filter))
                {
                    filtersInErrorList.Add(filter.Name);
                }
            }
            if(filtersInErrorList.Count != 0)
            {
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(
                        SR.DeviceFilterEditorDialog_InvalidFilter,
                        GenericUI.BuildCommaDelimitedList(
                            filtersInErrorList
                        )
                    )
                );
                return false;
            }
            return true;
        }
        
        private bool SaveFilters()
        {
            Cursor oldCursor = null;
            try
            {
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                ArrayList oldFilters = _webConfig.ReadDeviceFilters();
                foreach(DeviceFilterNode filter in oldFilters)
                {
                    filter.Delete();
                }

                _webConfig.EnsureSystemWebSectionIsPresent();
                foreach(DeviceFilterTreeNode filter in _filterList.TvList.Nodes)
                {
                    filter.DeviceFilter.Save();
                }
                _webConfig.Save();
            }
            catch (FileNotFoundException)
            {
                this.Cursor = oldCursor;
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_WebConfigMissing)
                );
                return false;
            }
            catch (Exception e)
            {
                this.Cursor = oldCursor;
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_WebConfigParsingError, e.Message)
                );
                Debug.Fail(e.ToString());
                return false;
            }
            this.Cursor = oldCursor;
            return true;
        }

        // NOTE: AppliedDeviceFiltersDialog also uses this test...
        internal static bool NewLabelIsLegal(
            ISite site,
            EditableTreeList filterList,
            String oldLabel,
            String newLabel,
            String errorDialogTitle
        ) {
            Debug.Assert(site != null);

            if(newLabel.Length == 0)
            {
                GenericUI.ShowWarningMessage(
                    errorDialogTitle,
                    SR.GetString(SR.DeviceFilterEditorDialog_UnnamedFilter)
                );
                return false;
            }

            /* Removed for DCR 4240
            if (!DesignerUtility.IsValidName(newLabel))
            {
                GenericUI.ShowWarningMessage(
                    errorDialogTitle,
                    SR.GetString(SR.DeviceFilterEditorDialog_IllegalName, newLabel)
                );
                return false;
            }
            */

            return true;
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin Event Handling
        ////////////////////////////////////////////////////////////////////////

        private void OnClickOK(Object sender, EventArgs e)
        {
            if(!FiltersAreValid())
            {
                return;
            }
            if(!SaveFilters())
            {
                return;
            }
            Close();
            DialogResult = DialogResult.OK;
        }
        
        private void OnClickCancel(Object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }
        
        private void CompareMode()
        {
            DeviceFilterTreeNode node = (DeviceFilterTreeNode)_filterList.SelectedNode;
            node.DeviceFilter.Mode = DeviceFilterMode.Compare;
            _pnlCompare.Visible = true;
            _pnlDelegate.Visible = false;
            _rbCompare.Checked = true;
            // We set Text proprety twice to preserve casing.  (See AUI 3964 /
            // URT 99595)
            String compare = node.DeviceFilter.Compare;
            _cbCompare.Text = compare;
            _cbCompare.Text = compare;
            _txtArgument.Text = node.DeviceFilter.Argument;
        }

        private void DelegateMode()
        {
            DeviceFilterTreeNode node = (DeviceFilterTreeNode)_filterList.SelectedNode;
            node.DeviceFilter.Mode = DeviceFilterMode.Delegate;
            _pnlCompare.Visible = false;
            _pnlDelegate.Visible = true;
            _rbDelegate.Checked = true;
            _txtType.Text = node.DeviceFilter.Type;
            _txtMethod.Text = node.DeviceFilter.Method;
        }

        private void OnClickRemoveButton(Object sender, EventArgs e)
        {
            if (_filterList.SelectedNode == null)
            {
                _pnlCompare.Visible = true;
                _pnlDelegate.Visible = false;
                _rbCompare.Checked = false;
                _rbDelegate.Checked = false;
                _cbCompare.Text = String.Empty;
                _txtArgument.Text = String.Empty;
                UpdateButtonsEnabling();
            }
        }

        private void OnClickAddButton(Object sender, EventArgs e)
        {
            DeviceFilterTreeNode node = new DeviceFilterTreeNode(_webConfig);
            node.Text = _filterList.GetUniqueLabel(node.Text);
            _filterList.TvList.Nodes.Add(node);
            _filterList.TvList.SelectedNode = node;
            node.EnsureVisible();
            UpdateButtonsEnabling();
            node.BeginEdit();
        }
        
        private void OnClickCompareRadioButton(Object Sender, EventArgs e)
        {
            CompareMode();
        }
        
        private void OnClickDelegateRadioButton(Object Sender, EventArgs e)
        {
            DelegateMode(); 
        }

        private void OnFilterSelected(Object sender, TreeViewEventArgs e)
        {
            DeviceFilterTreeNode node = (DeviceFilterTreeNode) e.Node;
            UpdateButtonsEnabling();
            if(node.DeviceFilter.Mode == DeviceFilterMode.Compare)
            {
                CompareMode();
            }
            else
            {
                DelegateMode();
            }
        }

        private void OnAfterLabelEdit(Object sender, NodeLabelEditEventArgs e)
        {
            // null still returned if label unmodified (verified 2310)
            if(e.Label == null)
            {
                return;
            }

            String oldLabel = e.Node.Text;
            String newLabel = e.Label;
            
            if(String.Compare(oldLabel, newLabel, StringComparison.OrdinalIgnoreCase) != 0
                && _filterList.LabelExists(newLabel))
            {
                // if the filter is duplicate
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_DuplicateName, newLabel)
                );
            }
            else if(String.Compare(newLabel, _nameOfDefaultFilter, StringComparison.OrdinalIgnoreCase) == 0)
            {
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.DeviceFilterEditorDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_IllegalDefaultName, newLabel)
                );
            }
            else if(NewLabelIsLegal(_site, _filterList, oldLabel, newLabel,
                SR.GetString(SR.DeviceFilterEditorDialog_Title)
            )) {
                // if the filter name is legal
                ((DeviceFilterTreeNode)e.Node).DeviceFilter.Name = e.Label;
                return;
            }
            // if the filter name was duplicate or not legal
            e.CancelEdit = true;
        }

        private void OnTextChanged(Object sender, EventArgs e)
        {
            if (null != _filterList.SelectedNode)
            {
                DeviceFilterNode node = ((DeviceFilterTreeNode)_filterList.SelectedNode).DeviceFilter;
                if(sender == _cbCompare)
                {
                    node.Compare = _cbCompare.Text;
                }
                else if(sender == _txtArgument)
                {
                    node.Argument = _txtArgument.Text;
                }
                else if(sender == _txtType)
                {
                    node.Type = _txtType.Text;
                }
                else if(sender == _txtMethod)
                {
                    node.Method = _txtMethod.Text;
                }
                else
                {
                    Debug.Fail("Unknown sender.");
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //  End Event Handling
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        //  Begin Private Class
        ////////////////////////////////////////////////////////////////////////

        // AppliedDeviceFiltersDialog also needs access to this class.
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class DeviceFilterTreeNode : TreeNode, ICloneable
        {
            internal readonly DeviceFilterNode DeviceFilter;
        
            internal DeviceFilterTreeNode(WebConfigManager webConfig) : base()
            {
                DeviceFilter = new DeviceFilterNode(webConfig);
                base.Text = DeviceFilter.Name;
            }

            internal DeviceFilterTreeNode(DeviceFilterNode node)
            {
                DeviceFilter = node;
                base.Text = node.Name;
            }

            internal new String Text
            {
                get
                {
                    Debug.Assert(DeviceFilter.Name == base.Text);
                    return DeviceFilter.Name;
                }

                set
                {
                    base.Text = value;
                    DeviceFilter.Name = value;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //  End Private Class
        ////////////////////////////////////////////////////////////////////////
    }
}
