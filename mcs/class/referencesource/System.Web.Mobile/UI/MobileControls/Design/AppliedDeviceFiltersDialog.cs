//------------------------------------------------------------------------------
// <copyright file="AppliedDeviceFilterDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Util;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class AppliedDeviceFiltersDialog :
        DesignerForm, IRefreshableDeviceSpecificEditor
    {
        private bool _isDirty = true;
        private IDeviceSpecificDesigner _designer = null;
        private WebConfigManager _webConfig = null;
        private IDictionary _cachedDeviceSpecifics =
            new HybridDictionary(true /* make case-insensitive */ );
        private String _currentDeviceSpecificID = null;
        
        private System.Windows.Forms.Label _lblAvailableFilters;
        private System.Windows.Forms.Button _btnEditFilters;
        private System.Windows.Forms.ComboBox _cbAvailableFilters;
        private System.Windows.Forms.Button _btnApplyFilter;
        private System.Windows.Forms.Button _cmdCancel;
        private System.Windows.Forms.Button _cmdOK;
        private System.Windows.Forms.Panel _pnlMain;
        private System.Windows.Forms.Label _lblArgument;
        private System.Windows.Forms.TextBox _tbArgument;
        private EditableTreeList _appliedFiltersList;
        
        internal AppliedDeviceFiltersDialog(
            IDeviceSpecificDesigner designer, 
            int mergingContext) : base (designer.UnderlyingControl.Site)
        {
            _designer = designer;
            _designer.SetDeviceSpecificEditor(this);

            // Required for Win Form Designer support
            InitializeComponent();

            _lblAvailableFilters.Text =
                SR.GetString(SR.AppliedDeviceFiltersDialog_AvailableDeviceFilters);
            _appliedFiltersList.LblTitle.Text =
                SR.GetString(SR.AppliedDeviceFiltersDialog_AppliedDeviceFilters);
            _btnEditFilters.Text = SR.GetString(SR.GenericDialog_Edit);
            _btnApplyFilter.Text =
                SR.GetString(SR.AppliedDeviceFiltersDialog_ApplyDeviceFilter);
            _lblArgument.Text =
                SR.GetString(SR.AppliedDeviceFiltersDialog_Argument);
            _cmdOK.Text = SR.GetString(SR.GenericDialog_OKBtnCaption);
            _cmdCancel.Text = SR.GetString(SR.GenericDialog_CancelBtnCaption);

            int tabOffset = GenericUI.InitDialog(
                this,
                _designer,
                mergingContext
            );
            
            this.Text = _designer.UnderlyingControl.ID
                + " - " + SR.GetString(SR.AppliedDeviceFiltersDialog_Title);
            SetTabIndexes(tabOffset);
            _webConfig = new WebConfigManager(_designer.UnderlyingControl.Site);
            LoadAvailableFilters();

            // Note that the following can cause an
            // IDeviceSpecificDesigner.Refresh() to occur as a side-effect.
            _designer.RefreshHeader(mergingContext);

            _currentDeviceSpecificID = _designer.CurrentDeviceSpecificID;
            if(_currentDeviceSpecificID != null)
            {
                DeviceSpecific ds;
                _designer.GetDeviceSpecific(_currentDeviceSpecificID, out ds);
                LoadChoices(ds);
            }

            // Register Event Handlers
            _cbAvailableFilters.SelectedIndexChanged += new EventHandler(
                OnAvailableFilterSelected
            );
            _cbAvailableFilters.TextChanged += new EventHandler(
                OnAvailableFilterSelected
            );
            _btnApplyFilter.Click += new EventHandler(OnApplyFilter);
            _btnEditFilters.Click += new EventHandler(OnEditFilters);
            _appliedFiltersList.TvList.AfterSelect += new TreeViewEventHandler(OnAppliedFilterSelected);
            _appliedFiltersList.TvList.AfterLabelEdit += new NodeLabelEditEventHandler(OnAfterLabelEdit);
            _appliedFiltersList.BtnUp.Click += new EventHandler(OnAppliedFiltersReordered);
            _appliedFiltersList.BtnDown.Click += new EventHandler(OnAppliedFiltersReordered);
            _appliedFiltersList.BtnRemove.Click -= _appliedFiltersList.RemoveHandler;
            _appliedFiltersList.BtnRemove.Click += new EventHandler(OnRemove);
            _tbArgument.TextChanged += new EventHandler(OnArgumentChanged);
            _cmdOK.Click += new EventHandler(OnOK);
            _cmdCancel.Click += new EventHandler(OnCancel);

            UpdateUI();
        }
        
        protected override string HelpTopic {
            get { return "net.Mobile.AppliedDeviceFiltersDialog"; }
        }

        internal void SetTabIndexes(int tabOffset)
        {
            _pnlMain.TabIndex = ++tabOffset;
            _lblAvailableFilters.TabIndex = ++tabOffset;
            _cbAvailableFilters.TabIndex = ++tabOffset;
            _btnEditFilters.TabIndex = ++tabOffset;
            _btnApplyFilter.TabIndex = ++tabOffset;
            _appliedFiltersList.TabIndex = ++tabOffset;
            _lblArgument.TabIndex = ++tabOffset;
            _tbArgument.TabIndex = ++tabOffset;
            _cmdOK.TabIndex = ++tabOffset;
            _cmdCancel.TabIndex = ++tabOffset;
        }

        private void InitializeComponent()
        {
            this._appliedFiltersList =
                new System.Web.UI.Design.MobileControls.Util.EditableTreeList(
                false, true, 16
                );
            this._btnEditFilters = new System.Windows.Forms.Button();
            this._cmdOK = new System.Windows.Forms.Button();
            this._pnlMain = new System.Windows.Forms.Panel();
            this._tbArgument = new System.Windows.Forms.TextBox();
            this._lblArgument = new System.Windows.Forms.Label();
            this._cmdCancel = new System.Windows.Forms.Button();
            this._lblAvailableFilters = new System.Windows.Forms.Label();
            this._cbAvailableFilters = new System.Windows.Forms.ComboBox();
            this._btnApplyFilter = new System.Windows.Forms.Button();
            this._pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // _appliedFiltersList
            // 
            this._appliedFiltersList.Location = new System.Drawing.Point(0, 74);
            this._appliedFiltersList.Name = "_appliedFiltersList";
            this._appliedFiltersList.Size = new System.Drawing.Size(275, 208);
            // 
            // _btnEditFilters
            // 
            this._btnEditFilters.Location = new System.Drawing.Point(201, 15);
            this._btnEditFilters.Size = new System.Drawing.Size(75, 23);
            this._btnEditFilters.Name = "_btnEditFilters";
            // 
            // _cmdOK
            // 
            this._cmdOK.Location = new System.Drawing.Point(120, 334);
            this._cmdOK.Name = "_cmdOK";
            // 
            // _pnlMain
            // 
            this._pnlMain.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this._pnlMain.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                   this._tbArgument,
                                                                                   this._lblArgument,
                                                                                   this._appliedFiltersList,
                                                                                   this._cmdCancel,
                                                                                   this._cmdOK,
                                                                                   this._lblAvailableFilters,
                                                                                   this._btnEditFilters,
                                                                                   this._cbAvailableFilters,
                                                                                   this._btnApplyFilter});
            this._pnlMain.Location = new System.Drawing.Point(6, 8);
            this._pnlMain.Name = "_pnlMain";
            this._pnlMain.Size = new System.Drawing.Size(276, 357);
            // 
            // _tbArgument
            // 
            this._tbArgument.Location = new System.Drawing.Point(0, 306);
            this._tbArgument.Name = "_tbArgument";
            this._tbArgument.Size = new System.Drawing.Size(275, 20);
            this._tbArgument.Text = String.Empty;
            // 
            // _lblArgument
            // 
            this._lblArgument.Location = new System.Drawing.Point(0, 290);
            this._lblArgument.Name = "_lblArgument";
            this._lblArgument.Size = new System.Drawing.Size(275, 16);
            // 
            // _cmdCancel
            // 
            this._cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cmdCancel.Location = new System.Drawing.Point(201, 334);
            this._cmdCancel.Name = "_cmdCancel";
            // 
            // _lblAvailableFilters
            // 
            this._lblAvailableFilters.Name = "_lblAvailableFilters";
            this._lblAvailableFilters.Size = new System.Drawing.Size(198, 16);
            // 
            // _cbAvailableFilters
            // 
            this._cbAvailableFilters.DropDownWidth = 195;
            this._cbAvailableFilters.Location = new System.Drawing.Point(0, 16);
            this._cbAvailableFilters.Name = "_cbAvailableFilters";
            this._cbAvailableFilters.Size = new System.Drawing.Size(195, 21);
            this._cbAvailableFilters.Sorted = true;
            // 
            // _btnApplyFilter
            // 
            this._btnApplyFilter.Location = new System.Drawing.Point(0, 44);
            this._btnApplyFilter.Name = "_btnApplyFilter";
            this._btnApplyFilter.Size = new System.Drawing.Size(195, 23);
            // 
            // AppliedDeviceFiltersDialog
            // 
            this.AcceptButton = this._cmdOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this._cmdCancel;
            this.ClientSize = new System.Drawing.Size(285, 373);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this._pnlMain});
            this.Name = "AppliedDeviceFiltersDialog";
            this._pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void LoadAvailableFilters()
        {
            _cbAvailableFilters.Items.Clear();
            try
            {
                ArrayList filterList = _webConfig.ReadDeviceFilters();
                _cbAvailableFilters.Sorted = false;
                foreach(DeviceFilterNode node in filterList)
                {
                    _cbAvailableFilters.Items.Add(node);
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
                // This is okay.  They may still apply a default or external
                // device filter.
            }
            _cbAvailableFilters.Sorted = true;
            EnsureDefaultFilterAvailableXorApplied();

            // If there is no filter selected, or there was a filter selected
            // but that filter no longer exists
            if( _cbAvailableFilters.Text.Length == 0
                || FindAvailableFilter(_cbAvailableFilters.Text) == null)
            {
                SelectFirstAvailableFilter();
            }
        }

        private void CacheState(String deviceSpecificID)
        {
            _cachedDeviceSpecifics[deviceSpecificID] = new AppliedFiltersCachedState(
                _appliedFiltersList.TvList
            );
        }

        private void CacheCurrentState()
        {
            CacheState(_currentDeviceSpecificID);
        }

        private bool RestoreState(String deviceSpecificID)
        {
            if (null != deviceSpecificID)
            {
                _currentDeviceSpecificID = deviceSpecificID;
                AppliedFiltersCachedState state =
                    (AppliedFiltersCachedState) _cachedDeviceSpecifics[
                        _currentDeviceSpecificID
                    ];
                if(state != null)
                {
                    state.Restore(_appliedFiltersList.TvList);
                    EnsureDefaultFilterAvailableXorApplied();
                    return true;
                }
            }
            else
            {
                _currentDeviceSpecificID = null;
            }
            return false;
        }

        private DeviceFilterNode CreateExternalFilter(String name)
        {
            DeviceFilterNode externalFilter;
            externalFilter = new DeviceFilterNode(_webConfig);
            externalFilter.Name = name;
            return externalFilter;
        }

        private void LoadChoice(DeviceSpecificChoice runtimeChoice)
        {
            DeviceFilterNode filterUsed =
                FindAvailableFilter(runtimeChoice.Filter);
            
            ChoiceTreeNode choice = new ChoiceTreeNode(
                filterUsed,
                runtimeChoice,
                _designer
            );
            _appliedFiltersList.TvList.Nodes.Add(choice);
        }

        private void LoadChoices(DeviceSpecific deviceSpecific)
        {
            _appliedFiltersList.TvList.Nodes.Clear();

            if(deviceSpecific != null)
            {
                foreach(DeviceSpecificChoice runtimeChoice in deviceSpecific.Choices)
                {
                    LoadChoice(runtimeChoice);
                }
            }
            EnsureDefaultFilterAvailableXorApplied();
        }
        
        private void SaveChoices()
        {
            if(_currentDeviceSpecificID != null)
            {
                CacheCurrentState();
            }
            foreach (DictionaryEntry entry in _cachedDeviceSpecifics)
            {
                AppliedFiltersCachedState state =
                    (AppliedFiltersCachedState) entry.Value;
                state.SaveChoicesFromTreeView(
                    _designer,
                    (String) entry.Key
                );
            }
        }

        private void SelectFirstAvailableFilter()
        {
            if (_cbAvailableFilters.Items.Count > 0)
            {
                DeviceFilterNode filter =
                    (DeviceFilterNode) _cbAvailableFilters.Items[0];
                _cbAvailableFilters.SelectedItem = filter;
            }
            else
            {
                _cbAvailableFilters.Text = String.Empty;
            }
        }

        private void SelectNextAvailableFilter(DeviceFilterNode currentFilter)
        {
            // if an externally defined filter is selected, let
            // SelectFirstAvailableFitler handle this.
            if(currentFilter == null)
            {
                SelectFirstAvailableFilter();
                return;
            }
            
            int index = _cbAvailableFilters.Items.IndexOf(currentFilter);
            if((index + 1) < _cbAvailableFilters.Items.Count)
            {
                _cbAvailableFilters.SelectedItem =
                    _cbAvailableFilters.Items[index + 1];
            }
            else if(index > 0)
            {
                _cbAvailableFilters.SelectedItem =
                    _cbAvailableFilters.Items[index - 1];
            }
            else
            {
                _cbAvailableFilters.SelectedItem = null;
                _cbAvailableFilters.Text = String.Empty;
            }
        }

        private DeviceFilterNode FindAvailableFilter(String name)
        {
            if(IsDefaultFilter(name))
            {
                name = "";
            }
            foreach(DeviceFilterNode filter in _cbAvailableFilters.Items)
            {
                if(filter.Name == name)
                {
                    return filter;
                }
            }
            return null;
        }

        private bool DefaultFilterIsApplied
        {
            get
            {
                return FindAppliedFilter("") != null;
            }
        }

        private bool DefaultFilterIsAvailable
        {
            get
            {
                return FindAvailableFilter("") != null;
            }
        }

        private ChoiceTreeNode FindAppliedFilter(String name)
        {
            if(IsDefaultFilter(name))
            {
                name = "";
            }
            foreach(ChoiceTreeNode choice in _appliedFiltersList.TvList.Nodes)
            {
                if(choice.Name == name)
                {
                    return choice;
                }
            }
            return null;            
        }

        private void EnsureDefaultFilterAvailableXorApplied()
        {
            if(DefaultFilterIsApplied)
            {
                DeviceFilterNode filter = FindAvailableFilter("");
                if(filter != null)
                {
                    RemoveAvailableFilter(filter);
                }
            }
            else if(!DefaultFilterIsAvailable)
            {
                _cbAvailableFilters.Items.Add(CreateExternalFilter(""));
            }
        }

        private bool IsDefaultFilter(String name)
        {
            return(
                name == null
                || name.Length == 0
                || name == SR.GetString(SR.DeviceFilter_DefaultChoice)
            );
        }
        
        private bool AvailableFilterIsSelected
        {
            get
            {
                return _cbAvailableFilters.Text != null && 
                    _cbAvailableFilters.Text.Length > 0;
            }
        }
        
        private bool AppliedFilterIsSelected
        {
            get
            {
                return SelectedAppliedFilter != null;
            }
        }

        private ChoiceTreeNode SelectedAppliedFilter
        {
            get
            {
                return (ChoiceTreeNode) _appliedFiltersList.TvList.SelectedNode;
            }
        }

        private void UpdateUI()
        {
            if(AppliedFilterIsSelected
                && !IsDefaultFilter(SelectedAppliedFilter.Name))
            {
                _tbArgument.Enabled = true;
                _tbArgument.Text = SelectedAppliedFilter.Argument;
            }
            else
            {
                _tbArgument.Enabled = false;
                _tbArgument.Text = String.Empty;
            }
            _btnApplyFilter.Enabled =
                AvailableFilterIsSelected && (_designer.UnderlyingObject != null);
            _cbAvailableFilters.Enabled = (_designer.UnderlyingObject != null);
            _appliedFiltersList.UpdateButtonsEnabling();
        }

        private bool ChoiceHasContent(DeviceSpecificChoice runtimeChoice)
        {
            return (runtimeChoice.Contents.Count > 0) || runtimeChoice.HasTemplates;
        }

        private void RemoveAvailableFilter(DeviceFilterNode filter)
        {
            SelectNextAvailableFilter(filter);
            _cbAvailableFilters.Items.Remove(filter);
            UpdateUI();
        }
 
        private void SetDirty(bool dirty)
        {
            if (dirty)
            {
                if (false == _isDirty)
                {
                    _isDirty = true;
                    _cmdCancel.Text = SR.GetString(SR.GenericDialog_CancelBtnCaption);
                }
            }
            else
            {
                if (true == _isDirty)
                {
                    _isDirty = false;
                    _cmdCancel.Text = SR.GetString(SR.GenericDialog_CloseBtnCaption);
                }
            }
        }

        private bool ValidateAppliedFilters()
        {
            StringCollection duplicateChoices =
                DesignerUtility.GetDuplicateChoiceTreeNodes(
                    _appliedFiltersList.TvList.Nodes
                );
            if(duplicateChoices.Count > 0)
            {
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.AppliedDeviceFiltersDialog_Title),
                    SR.GetString(SR.AppliedDeviceFiltersDialog_DuplicateChoices,
                    GenericUI.BuildCommaDelimitedList(duplicateChoices))
                );
                return false;
            }
            return true;
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin Event Handling
        ////////////////////////////////////////////////////////////////////////

        private void OnArgumentChanged(Object sender, EventArgs e)
        {
            if(!AppliedFilterIsSelected)
            {
                // UpdateUI sets to empty string when no filter selected.
                Debug.Assert(_tbArgument.Text.Length == 0,
                    "Not a side effect of clearing the argument. "
                    + "Arugment changed with no applied filter selected. "
                    + "Missing a call to UpdateUI()?");
                return;
            }
            SelectedAppliedFilter.Argument = _tbArgument.Text;
            SetDirty(true);
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

            bool labelIsLegal = true;

            if(!DeviceFilterEditorDialog.NewLabelIsLegal(
                _designer.UnderlyingControl.Site,
                _appliedFiltersList,
                oldLabel,
                newLabel,
                SR.GetString(SR.AppliedDeviceFiltersDialog_Title)
            )) {
                labelIsLegal = false;
            }
            else if(IsDefaultFilter(newLabel))
            {
                GenericUI.ShowWarningMessage(
                    SR.GetString(SR.AppliedDeviceFiltersDialog_Title),
                    SR.GetString(SR.DeviceFilterEditorDialog_IllegalDefaultName)
                );
                labelIsLegal = false;
            }
            
            if(!labelIsLegal)
            {
                e.CancelEdit = true;
                return;
            }

            ((ChoiceTreeNode)e.Node).Name = newLabel;
            EnsureDefaultFilterAvailableXorApplied();
            SetDirty(true);
            UpdateUI();
        }

        private void OnAvailableFilterSelected(Object sender, EventArgs e)
        {
            // NOTE: This event handler is registed for both ItemSelected and
            //       TextChanged events of _cbAvailableFilters.
            UpdateUI();
        }
        
        private void OnAppliedFilterSelected(Object sender, TreeViewEventArgs e)
        {
            UpdateUI();
        }
        
        private void OnEditFilters(Object sender, EventArgs e)
        {
            if(!ValidateAppliedFilters())
            {
                return;
            }
            DeviceFilterEditorDialog dialog = null;

            try
            {
                try
                {
                    dialog = new DeviceFilterEditorDialog(
                        _designer.UnderlyingControl.Site,
                        _webConfig
                        );
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            catch (CheckoutException ex)
            {
                if (ex == CheckoutException.Canceled)
                {
                    return;
                }
                throw;
            }
            catch (FileLoadException)
            {
                // This is how the constructor tells us it failed to read
                // web.config.
                return;
            }
            
            if(AvailableFilterIsSelected)
            {
                dialog.SelectFilterByName(_cbAvailableFilters.Text);
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Cursor oldCursor = null;
                try
                {
                    oldCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;

                    // Fix for # 4667
                    // if (_currentDeviceSpecificID != null)
                    //{
                    LoadAvailableFilters();
                    //}
                    SaveChoices();
                    SetDirty(false);
                }
                finally
                {
                    this.Cursor = oldCursor;
                }
            }
        }

        private void OnApplyFilter(Object sender, EventArgs e)
        {
            DeviceFilterNode filter = (DeviceFilterNode) _cbAvailableFilters.SelectedItem;
            if(filter == null)
            {
                String name = _cbAvailableFilters.Text;
                Debug.Assert(
                    ((name != null) && (name.Length > 0)),
                    "Should not be trying to apply a filter with none selected. "
                    + "Missed a call to UpdateUI()?"
                );

                // If the user typed the name of a filter which exists in their
                // web.config, we need to find the original rather than create
                // a new external filter.
                filter = FindAvailableFilter(name);
                if(filter == null)
                {
                    /* Removed for DCR 4240
                    if (!DesignerUtility.IsValidName(name))
                    {
                        GenericUI.ShowWarningMessage(
                            SR.GetString(SR.AppliedDeviceFiltersDialog_Title),
                            SR.GetString(
                                SR.AppliedDeviceFiltersDialog_InvalidFilterName,
                                _cbAvailableFilters.Text
                            )
                        );
                        return;
                    }
                    */

                    filter = CreateExternalFilter(_cbAvailableFilters.Text);
                }
            }
            ChoiceTreeNode choice = new ChoiceTreeNode(
                filter,
                _designer
            );
            if(IsDefaultFilter(filter.Name))
            {
                if(DefaultFilterIsApplied)
                {
                    // Do not allow user to apply default filter if already
                    // been applied.
                    GenericUI.ShowWarningMessage(
                        SR.GetString(SR.AppliedDeviceFiltersDialog_Title),
                        SR.GetString(SR.AppliedDeviceFiltersDialog_DefaultFilterAlreadyApplied)
                    );
                }
                else
                {
                    // Add the default filter to the end of the list and
                    // remove it from list of available filters.
                    _appliedFiltersList.TvList.Nodes.Add(choice);
                    RemoveAvailableFilter(filter);
                }
            }
            else
            {
                // All other filters are added to the beginning
                _appliedFiltersList.TvList.Nodes.Insert(0, choice);
            }
            SetDirty(true);
            UpdateUI();
        }

        private void OnAppliedFiltersReordered(Object sender, EventArgs e)
        {
            SetDirty(true);
        }

        private void OnRemove(Object sender, EventArgs e)
        {
            ChoiceTreeNode choice = (ChoiceTreeNode) _appliedFiltersList.TvList.SelectedNode;
            if(ChoiceHasContent(choice.RuntimeChoice))
            {
                if(!GenericUI.ConfirmYesNo(
                    SR.GetString(SR.AppliedDeviceFiltersDialog_Title),
                    SR.GetString(
                        SR.AppliedDeviceFiltersDialog_AssociatedItemsWillBeLost
                    )
                )) {
                    return;
                }
            }
            _appliedFiltersList.TvList.Nodes.Remove(_appliedFiltersList.TvList.SelectedNode);

            // If it was the default filter, and it a duplicate is not still
            // still applied (error in HTML view), return it to the list of
            // available filters.
            if(IsDefaultFilter(choice.Name))
            {
                EnsureDefaultFilterAvailableXorApplied();
            }
            _appliedFiltersList.UpdateButtonsEnabling();
            SetDirty(true);
            UpdateUI();
        }
                
        private void OnOK(Object sender, EventArgs e)
        {
            if(!ValidateAppliedFilters())
            {
                return;
            }
            SaveChoices();
            Close();
            DialogResult = DialogResult.OK;
        }
        
        private void OnCancel(Object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }
        
        ////////////////////////////////////////////////////////////////////////
        //  End Event Handling
        ////////////////////////////////////////////////////////////////////////
        
        ////////////////////////////////////////////////////////////////////////
        //  Begin IRefreshableDeviceSpecificEditor Implementation
        ////////////////////////////////////////////////////////////////////////
        
        bool IRefreshableDeviceSpecificEditor.RequestRefresh()
        {
            return ValidateAppliedFilters();
        }
        
        void IRefreshableDeviceSpecificEditor.Refresh(
            String deviceSpecificID,
            DeviceSpecific deviceSpecific
        ) {
            if (_currentDeviceSpecificID != null)
            {
                CacheCurrentState();
            }
            if(!RestoreState(deviceSpecificID))
            {
                // If we could not restore the state, we have not edited
                // this DeviceSpecific yet and need to load choices.
                LoadChoices(deviceSpecific);
            }
            UpdateUI();
        }

        void IRefreshableDeviceSpecificEditor.UnderlyingObjectsChanged()
        {
            SaveChoices();
            SetDirty(false);
        }

        private bool InExternalCacheEditMode
        {
            get
            {
                return _cacheBuffer != null;
            }
        }
        
        private IDictionary _cacheBuffer = null;
        
        void IRefreshableDeviceSpecificEditor.BeginExternalDeviceSpecificEdit()
        {
            Debug.Assert(!InExternalCacheEditMode,
                "Call to BeginExternalDeviceSpecificEdit() while already in external "
                + "cache edit mode.");
            if(_currentDeviceSpecificID != null)
            {
                CacheCurrentState();
                _currentDeviceSpecificID = null;
            }
            _cacheBuffer = new HybridDictionary(
                true /* make case-insensitive*/ );
            foreach(DictionaryEntry entry in _cachedDeviceSpecifics)
            {
                _cacheBuffer.Add(entry.Key, entry.Value);
            }
        }
        
        void IRefreshableDeviceSpecificEditor.EndExternalDeviceSpecificEdit(
            bool commitChanges)
        {
            Debug.Assert(InExternalCacheEditMode,
                "Call to EndExternalDeviceSpecificEdit() while not in external "
                + "cache edit mode.");
            if(commitChanges)
            {
                _cachedDeviceSpecifics = _cacheBuffer;
            }
            _cacheBuffer = null;
        }
        
        void IRefreshableDeviceSpecificEditor.DeviceSpecificRenamed(
            String oldDeviceSpecificID, String newDeviceSpecificID)
        {
            Debug.Assert(InExternalCacheEditMode,
                "Call to DeviceSpecificRenamed() while not in external "
                + "cache edit mode.");
            Object value = _cacheBuffer[oldDeviceSpecificID];
            if(value != null)
            {
                _cacheBuffer.Remove(oldDeviceSpecificID);
                _cacheBuffer.Add(newDeviceSpecificID, value);
            }
        }
        
        void IRefreshableDeviceSpecificEditor.DeviceSpecificDeleted(
            String deviceSpecificID)
        {
            Debug.Assert(InExternalCacheEditMode,
                "Call to DeviceSpecificDeleted() while not in external "
                + "cache edit mode.");
            _cacheBuffer.Remove(deviceSpecificID);
        }

        ////////////////////////////////////////////////////////////////////////
        //  End IRefeshableComponentEditorPage Implementation
        ////////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////
    //  Begin Internal Class
    ////////////////////////////////////////////////////////////////////////////

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class AppliedFiltersCachedState : DeviceSpecificDialogCachedState
    {
        private TreeNode[] _cachedTreeView = null;
        
        internal AppliedFiltersCachedState(
            TreeView treeView
        ) {
            _cachedTreeView = new TreeNode[treeView.Nodes.Count];
            treeView.Nodes.CopyTo(_cachedTreeView, 0);
        }

        internal TreeNode[] AppliedFilters
        {
            get
            {
                return _cachedTreeView;
            }
        }
        
        internal void Restore(
            TreeView treeView
        ) {
            TreeNode selectedNode = treeView.SelectedNode;
            treeView.Nodes.Clear();
            treeView.Nodes.AddRange(_cachedTreeView);
            if(selectedNode != null)
            {
                int index = treeView.Nodes.IndexOf(selectedNode);
                if(index >= 0)
                {
                    treeView.SelectedNode = treeView.Nodes[index];
                }
            }
        }

        internal bool ChoiceExistsInTreeView(DeviceFilterNode filter)
        {
            foreach(ChoiceTreeNode appliedFilter in _cachedTreeView)
            {
                if(appliedFilter.Name == filter.Name)
                {
                    return true;
                }
            }
            return false;
        }
        
        internal void SaveChoicesFromTreeView(
            IDeviceSpecificDesigner designer,
            String deviceSpecificID
        ) {
            SaveChoices(designer, deviceSpecificID, _cachedTreeView);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////
    //  End Internal Class
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    //  Begin Internal Class
    ////////////////////////////////////////////////////////////////////////////

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ChoiceTreeNode : TreeNode
    {
        private IDeviceSpecificDesigner _designer;
        private ChoicePropertyFilter _choicePropertyFilter = null;
        private DeviceSpecificChoice _runtimeChoice = null;
        private String _filterName;
        private String _argument;
        private bool _includeArgument = false;

        internal ChoiceTreeNode(
            DeviceFilterNode filter,
            IDeviceSpecificDesigner designer
        ) : base() 
        {
            Name = filter.Name;
            _designer = designer;
            _runtimeChoice = new DeviceSpecificChoice();
            _runtimeChoice.Filter = filter.Name;

            if(
                // This looks like circular reasoning, but the designer is a
                // IDeviceSpecificDesigner and we are interested in the
                // type of the designer's parent control.
                Adapters.DesignerAdapterUtil.ControlDesigner(designer.UnderlyingControl)
                is MobileTemplatedControlDesigner
            ) 
            {
                _runtimeChoice.Xmlns = SR.GetString(SR.MarkupSchema_HTML32);
            }
        }

        internal ChoiceTreeNode(
            DeviceFilterNode filter,
            DeviceSpecificChoice runtimeChoice,
            IDeviceSpecificDesigner designer
        ) : base()
        {
            _designer = designer;
            _runtimeChoice = runtimeChoice;
            Name = _runtimeChoice.Filter;
            Argument = _runtimeChoice.Argument;
        }
        
        internal bool IncludeArgument
        {
            get
            {
                return _includeArgument;
            }

            set
            {
                _includeArgument = value;
            }
        }
                
        internal new String Name
        {
            get
            {
                return _filterName;
            }

            set
            {
                Debug.Assert(value != null);
                _filterName = value;
                base.Text = ToString();
            }
        }

        internal String Argument
        {
            get
            {
                return _argument;
            }

            set
            {
                _argument = value;
            }
        }

        internal ChoicePropertyFilter Choice
        {
            get
            {
                if(_choicePropertyFilter == null)
                {
                    _choicePropertyFilter = new ChoicePropertyFilter(
                        _runtimeChoice,
                        _designer,
                        _designer.UnderlyingControl.Site                        
                    );
                }
                return _choicePropertyFilter;
            }
        }

        internal void CommitChanges()
        {
            RuntimeChoice.Filter = _filterName;
            RuntimeChoice.Argument = _argument;
        }
        
        internal DeviceSpecificChoice RuntimeChoice
        {
            get
            {
                Debug.Assert(
                    (_choicePropertyFilter == null)
                    || (_runtimeChoice == _choicePropertyFilter.RuntimeChoice),
                    "Local runtime choice object out of [....]."
                );
                return _runtimeChoice;
            }
        }

        // This done so that these TreeNodes will display correctly when
        // inserted in a ComboBox.
        public override String ToString()
        {
            StringBuilder name = new StringBuilder(_filterName);
            
            if(name == null || name.Length == 0)
            {
                name = new StringBuilder(
                    SR.GetString(SR.DeviceFilter_DefaultChoice)
                );
            }
            else if(_includeArgument)
            {
                name.Append( " (\"" + _runtimeChoice.Argument + "\")" );
            }
            return name.ToString();
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////
    //  End Internal Class
    ////////////////////////////////////////////////////////////////////////////
}
