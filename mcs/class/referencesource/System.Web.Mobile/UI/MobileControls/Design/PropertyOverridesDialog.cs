//------------------------------------------------------------------------------
// <copyright file="PropertyOverridesDialog.cs" company="Microsoft">
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
    using System.Globalization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using Control = System.Web.UI.Control;

    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Converters;
    using System.Web.UI.Design.MobileControls.Util;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class PropertyOverridesDialog :
        DesignerForm, IRefreshableDeviceSpecificEditor, IDeviceSpecificDesigner
    {
        private bool _isDirty = true;
        private IDeviceSpecificDesigner _designer;
        private int _mergingContext;
        private System.Windows.Forms.Control _header;
        private String _currentDeviceSpecificID;
        private IDictionary _cachedDeviceSpecifics =
            new HybridDictionary(true /* make case-insensitive */);
        private bool _ignoreSelectionChanged = true;

        private System.Windows.Forms.Label _lblProperties;
        private System.Windows.Forms.PropertyGrid _pgProperties;
        private System.Windows.Forms.Button _btnEditFilters;
        private System.Windows.Forms.ComboBox _cbChoices;
        private System.Windows.Forms.Label _lblAppliedFilters;
        private System.Windows.Forms.Button _cmdOK;
        private System.Windows.Forms.Button _cmdCancel;
        private System.Windows.Forms.Panel _pnlMain;
        
        internal PropertyOverridesDialog(
            IDeviceSpecificDesigner designer,
            int mergingContext
        ) : base(designer.UnderlyingControl.Site) {
            _designer = designer;
            _mergingContext = mergingContext;
           
            // Required for Win Form Designer support
            InitializeComponent();

            this._lblAppliedFilters.Text =
                SR.GetString(SR.PropertyOverridesDialog_AppliedDeviceFilters);
            this._btnEditFilters.Text = SR.GetString(SR.GenericDialog_Edit);
            this._lblProperties.Text =
                SR.GetString(SR.PropertyOverridesDialog_DeviceSpecificProperties);
            this._cmdOK.Text = SR.GetString(SR.GenericDialog_OKBtnCaption);
            this._cmdCancel.Text = SR.GetString(SR.GenericDialog_CancelBtnCaption);

            int tabOffset = GenericUI.InitDialog(
                this,
                _designer,
                _mergingContext
            );

            this.Text = _designer.UnderlyingControl.ID
                + " - " + SR.GetString(SR.PropertyOverridesDialog_Title);
            SetTabIndexes(tabOffset);
            _designer.SetDeviceSpecificEditor(this);

            // Note that the following can cause an
            // IDeviceSpecificDesigner.Refresh() to occur as a side-effect.
            _designer.RefreshHeader(_mergingContext);
            _ignoreSelectionChanged = false;

            // NOTE: Calling CurrentDeviceSpecificID will cause a refresh to
            //       happen as a side effect.
            _currentDeviceSpecificID = _designer.CurrentDeviceSpecificID;
            if(_currentDeviceSpecificID != null)
            {
                _cbChoices.Items.Clear();
                LoadChoices(_currentDeviceSpecificID);
                if(!ValidateLoadedChoices())
                {
                    // Throw to prevent dialog from opening.  Caught and hidden
                    // by PropertyOverridesTypeEditor.cs
                    throw new InvalidChoiceException(
                        "Property overrides dialog can not open because there " +
                        "are invalid choices defined in the page."
                    );
                }
            }
            
            // Register Event Handlers
            _cbChoices.SelectedIndexChanged += new EventHandler(
                OnFilterSelected
            );
            _btnEditFilters.Click += new EventHandler(OnEditFilters);
            _cmdOK.Click += new EventHandler(OnOK);
            _cmdCancel.Click += new EventHandler(OnCancel);
            UpdateUI();
        }

        protected override string HelpTopic {
            get {
                return "net.Mobile.PropertyOverridesDialog";
            }
        }

        internal void SetTabIndexes(int tabOffset)
        {
            this._pnlMain.TabIndex = ++tabOffset;
            this._lblAppliedFilters.TabIndex = ++tabOffset;
            this._cbChoices.TabIndex = ++tabOffset;
            this._btnEditFilters.TabIndex = ++tabOffset;
            this._lblProperties.TabIndex = ++tabOffset;
            this._pgProperties.TabIndex = ++tabOffset;
            this._cmdOK.TabIndex = ++tabOffset;
            this._cmdCancel.TabIndex = ++tabOffset;
        }
        
        private void InitializeComponent()
        {
            this._cbChoices = new System.Windows.Forms.ComboBox();
            this._cmdOK = new System.Windows.Forms.Button();
            this._btnEditFilters = new System.Windows.Forms.Button();
            this._pnlMain = new System.Windows.Forms.Panel();
            this._pgProperties = new System.Windows.Forms.PropertyGrid();
            this._lblProperties = new System.Windows.Forms.Label();
            this._lblAppliedFilters = new System.Windows.Forms.Label();
            this._cmdCancel = new System.Windows.Forms.Button();
            this._cbChoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbChoices.DropDownWidth = 195;
            this._cbChoices.Location = new System.Drawing.Point(0, 16);
            this._cbChoices.Size = new System.Drawing.Size(195, 21);
            this._cmdOK.Location = new System.Drawing.Point(120, 290);
            this._cmdCancel.Location = new System.Drawing.Point(201, 290);
            this._btnEditFilters.Location = new System.Drawing.Point(201, 15);
            this._btnEditFilters.Size = new System.Drawing.Size(75, 23);
            this._pnlMain.Anchor = (System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left);
            this._pnlMain.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._cmdCancel,
                this._cmdOK,
                this._lblProperties,
                this._pgProperties,
                this._btnEditFilters,
                this._cbChoices,
                this._lblAppliedFilters});
            this._pnlMain.Location = new System.Drawing.Point(6, 5);
            this._pnlMain.Size = new System.Drawing.Size(276, 313);
            this._pgProperties.CommandsVisibleIfAvailable = false;
            this._pgProperties.HelpVisible = false;
            this._pgProperties.LargeButtons = false;
            this._pgProperties.LineColor = System.Drawing.SystemColors.ScrollBar;
            this._pgProperties.Location = new System.Drawing.Point(0, 64);
            this._pgProperties.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this._pgProperties.Size = new System.Drawing.Size(275, 220);
            this._pgProperties.Text = "PropertyGrid";
            this._pgProperties.ToolbarVisible = false;
            this._pgProperties.ViewBackColor = System.Drawing.SystemColors.Window;
            this._pgProperties.ViewForeColor = System.Drawing.SystemColors.WindowText;
            this._pgProperties.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnPropertyValueChanged);
            this._lblProperties.Location = new System.Drawing.Point(0, 48);
            this._lblProperties.Size = new System.Drawing.Size(275, 16);
            this._lblAppliedFilters.Size = new System.Drawing.Size(275, 16);
            this.AcceptButton = _cmdOK;
            this.CancelButton = _cmdCancel;
            this.ClientSize = new System.Drawing.Size(285, 325);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this._pnlMain});
        }
        
        private void CacheState(String deviceSpecificID)
        {
            _cachedDeviceSpecifics[deviceSpecificID] = 
                new PropertyOverridesCachedState(_cbChoices);
        }

        private void CacheCurrentState()
        {
            CacheState(_currentDeviceSpecificID);
        }

        private bool RestoreState(String deviceSpecificID)
        {
            if (null != deviceSpecificID)
            {
                _currentDeviceSpecificID = deviceSpecificID.ToLower(CultureInfo.InvariantCulture);
                PropertyOverridesCachedState state =
                    (PropertyOverridesCachedState) _cachedDeviceSpecifics[
                        _currentDeviceSpecificID
                    ];
                if(state != null)
                {
                    state.Restore(_cbChoices);
                    foreach(ChoiceTreeNode node in state.Choices)
                    {
                        node.Choice.Refresh();
                    }
                    return true;
                }
            }
            else
            {
                _currentDeviceSpecificID = null;
            }
            return false;
        }

        [Conditional("DEBUG")]
        private void debug_CheckChoicesForDuplicate(DeviceSpecificChoice runtimeChoice)
        {
            foreach(ChoiceTreeNode choiceNode in _cbChoices.Items)
            {
                if(choiceNode.Name == runtimeChoice.Filter
                    && choiceNode.RuntimeChoice.Argument == runtimeChoice.Argument)
                {
                    Debug.Fail("Loaded duplicate choice: " + 
                        DesignerUtility.ChoiceToUniqueIdentifier(runtimeChoice));
                }
            }
        }

        private void LoadChoices(String deviceSpecificID)
        {
            DeviceSpecific ds;
            _designer.GetDeviceSpecific(deviceSpecificID, out ds);
            LoadChoices(ds);
        }
        
        private void LoadChoices(DeviceSpecific deviceSpecific)
        {
            if(deviceSpecific != null)
            {
                foreach(DeviceSpecificChoice runtimeChoice in deviceSpecific.Choices)
                {
                    debug_CheckChoicesForDuplicate(runtimeChoice);
                    ChoiceTreeNode newChoiceNode = new ChoiceTreeNode(
                        null,
                        runtimeChoice,
                        _designer
                    );
                    newChoiceNode.IncludeArgument = true;
                    _cbChoices.Items.Add(newChoiceNode);
                }
            }
            UpdateUI();
        }

        private bool ValidateLoadedChoices()
        {
            StringCollection duplicateChoices =
                DesignerUtility.GetDuplicateChoiceTreeNodes(
                    _cbChoices.Items
                );

            if(duplicateChoices.Count > 0)
            {
                if (!_ignoreSelectionChanged)
                {
                    GenericUI.ShowWarningMessage(
                        SR.GetString(SR.PropertyOverridesDialog_Title),
                        SR.GetString(SR.PropertyOverridesDialog_DuplicateChoices,
                        GenericUI.BuildCommaDelimitedList(duplicateChoices))
                        );
                }
                return false;
            }
            return true;
        }
        
        private void SaveChoices()
        {
            if(_currentDeviceSpecificID != null)
            {
                CacheCurrentState();
            }
            foreach (DictionaryEntry entry in _cachedDeviceSpecifics)
            {
                PropertyOverridesCachedState state =
                    (PropertyOverridesCachedState) entry.Value;
                state.SaveChoicesFromComboBox(
                    _designer,
                    (String) entry.Key
                );
            }
        }
        
        private void UpdateUI()
        {
            if(_cbChoices.SelectedItem == null && _cbChoices.Items.Count > 0)
            {
                _cbChoices.SelectedItem = _cbChoices.Items[0];
            }
            
            ChoiceTreeNode choice = (ChoiceTreeNode) _cbChoices.SelectedItem;
            bool isChoiceSelected = (choice != null);
            if (isChoiceSelected)
            {
                _cbChoices.Text = choice.ToString();
                _pgProperties.SelectedObject =
                    choice.Choice;
            }
            else
            {
                _cbChoices.Text = String.Empty;
                _pgProperties.SelectedObject = null;
            }
            _cbChoices.Enabled = isChoiceSelected;
            _pgProperties.Enabled = isChoiceSelected;
            _btnEditFilters.Enabled = (_currentDeviceSpecificID != null);
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

        ////////////////////////////////////////////////////////////////////////
        //  Begin Event Handling
        ////////////////////////////////////////////////////////////////////////

        private void OnEditFilters(Object sender, EventArgs e)
        {
            ISite componentSite = ((IComponent)(_designer.UnderlyingControl)).Site;
            Debug.Assert(componentSite != null, "Expected the runtime control to be sited.");

            IComponentChangeService changeService =
                (IComponentChangeService)componentSite.GetService(typeof(IComponentChangeService));

            IMobileWebFormServices wfServices = 
                (IMobileWebFormServices)componentSite.GetService(typeof(IMobileWebFormServices));

            DialogResult result = DialogResult.Cancel;
            try
            {
                AppliedDeviceFiltersDialog dialog = new
                    AppliedDeviceFiltersDialog(
                    this,
                    _mergingContext
                    );

                result = dialog.ShowDialog();
            }
            finally
            {
                if (result != DialogResult.Cancel)
                {
                    SaveChoices();
                    SetDirty(false);

                    if (changeService != null)
                    {
                        changeService.OnComponentChanged(_designer.UnderlyingControl, null, null, null);
                    }
                }
            }
        }
        
        private void OnFilterSelected(Object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void OnOK(Object sender, EventArgs e)
        {
            SaveChoices();
            Close();
            DialogResult = DialogResult.OK;
        }
        
        private void OnCancel(Object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }
        
        private void OnPropertyValueChanged(Object sender, 
                                            PropertyValueChangedEventArgs e)
        {
            SetDirty(true);
        }

        ////////////////////////////////////////////////////////////////////////
        //  End Event Handling
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        //  Begin IRefreshableComponentEditor Implementation
        ////////////////////////////////////////////////////////////////////////

        bool IRefreshableDeviceSpecificEditor.RequestRefresh()
        {
            return true;
        }
        
        void IRefreshableDeviceSpecificEditor.Refresh(
            String deviceSpecificID,
            DeviceSpecific deviceSpecific
        ) {
            if (_currentDeviceSpecificID != null)
            {
                CacheCurrentState();
            }
            _cbChoices.Items.Clear();

            if (!RestoreState(deviceSpecificID))
            {
                LoadChoices(deviceSpecific);
                if(!ValidateLoadedChoices())
                {
                    _designer.RefreshHeader(
                        MobileControlDesigner.MergingContextProperties
                    );
                }
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
        //  End IRefreshableComponentEditor Implementation
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        void IDeviceSpecificDesigner.SetDeviceSpecificEditor
            (IRefreshableDeviceSpecificEditor editor)
        {
        }

        String IDeviceSpecificDesigner.CurrentDeviceSpecificID
        {
            get
            {
                return _currentDeviceSpecificID;
            }
        }

        System.Windows.Forms.Control IDeviceSpecificDesigner.Header
        {
            get
            {
                return _header;
            }
        }

        System.Web.UI.Control IDeviceSpecificDesigner.UnderlyingControl
        {
            get
            {
                return _designer.UnderlyingControl;
            }
        }

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                return _designer.UnderlyingObject;
            }
        }

        bool IDeviceSpecificDesigner.GetDeviceSpecific(String deviceSpecificParentID, out DeviceSpecific ds)
        {
            Debug.Assert(deviceSpecificParentID == _currentDeviceSpecificID);

            ds = null;
            if (_cbChoices.Items.Count > 0)
            {
                ds = new DeviceSpecific();
                foreach (ChoiceTreeNode choiceNode in _cbChoices.Items)
                {
                    DeviceSpecificChoice choice = choiceNode.Choice.RuntimeChoice;
                    ds.Choices.Add(choice);
                }
            }

            return true;
        }

        void IDeviceSpecificDesigner.SetDeviceSpecific(String deviceSpecificParentID, DeviceSpecific ds)
        {
            Debug.Assert(_currentDeviceSpecificID != null);
            _cbChoices.Items.Clear();
            LoadChoices(ds);
            UpdateUI();
        }

        void IDeviceSpecificDesigner.InitHeader(int mergingContext)
        {
            HeaderPanel panel = new HeaderPanel();
            HeaderLabel lblDescription = new HeaderLabel();

            lblDescription.TabIndex = 0;
            lblDescription.Text = SR.GetString(SR.MobileControl_SettingGenericChoiceDescription);
            
            panel.Height = lblDescription.Height;
            panel.Width = lblDescription.Width;
            panel.Controls.Add(lblDescription);
            _header = panel;
        }

        void IDeviceSpecificDesigner.RefreshHeader(int mergingContext)
        {
        }

        void IDeviceSpecificDesigner.UseCurrentDeviceSpecificID()
        {
        }

        /////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        /////////////////////////////////////////////////////////////////////////
    }
    
    ////////////////////////////////////////////////////////////////////////////
    //  Begin Internal Class
    ////////////////////////////////////////////////////////////////////////////

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ChoicePropertyFilter :
        ICustomTypeDescriptor, IDeviceSpecificChoiceDesigner, IComponent
    {
        private DeviceSpecificChoice _choice;
        private Object _copyOfOriginalObject;
        private Object _underlyingObject;
        private IDeviceSpecificDesigner _designer;
        private Hashtable _specialProp_buffer = new Hashtable();
        private EventHandler _specialProp_delegate = null;
        private ISite _site = null;
        private EventHandlerList _events;
        private static readonly Object _eventDisposed = new Object();
        private static readonly String _alternateUrl = "AlternateUrl";
        private static readonly String _navigateUrl = "NavigateUrl";

        internal ChoicePropertyFilter(
            DeviceSpecificChoice choice,
            IDeviceSpecificDesigner designer,
            ISite site
        ) {
            _events = new EventHandlerList();
            _choice = choice;
            _site = site;
            _designer = designer;

            CreateLocalCopiesOfObjects();
        }

        private void CreateLocalCopiesOfObjects()
        {
            // We make this copy of the original to remove the object from
            // the inheritance chain.
            _copyOfOriginalObject = CloneTarget(_designer.UnderlyingObject);
            _underlyingObject = CloneTarget(_designer.UnderlyingObject);
            
            // We need to pop up editors when certain property values change    
            RegisterForPropertyChangeEvents();
            
            // Copy properties set on DeviceSpecificChoice
            ApplyChoiceToRuntimeControl();
        }

        internal void Refresh()
        {
            ApplyChangesToRuntimeChoice();
            CreateLocalCopiesOfObjects();
        }

        private void RegisterForPropertyChangeEvents()
        {
            foreach(PropertyDescriptor property in TypeDescriptor.GetProperties(
                _underlyingObject.GetType()
            )) {
                if(property.Converter is NavigateUrlConverter &&
                   (property.Name == _navigateUrl ||
                    property.Name == _alternateUrl))
                {
                    // 

                    if (property.Name == _navigateUrl)
                    {
                        _specialProp_delegate = new EventHandler(OnNavigateUrlChanged);
                    }
                    else
                    {
                        _specialProp_delegate = new EventHandler(OnAlternateUrlChanged);
                    }
                    _specialProp_buffer[property.Name] = property.GetValue(_underlyingObject);
                    property.AddValueChanged(
                        _underlyingObject,
                        _specialProp_delegate
                    );
                }
            }
        }
        
        private Object CloneTarget(Object target)
        {
            Object clone = Activator.CreateInstance(
                target.GetType()
            );
            // We need to copy the Site over to the new object incase setting
            // properties has a side effect that requires the component model
            // to be intact.  (e.g., Launching UrlPicker for NavigateUrl).
            if(clone is IComponent)
            {
                ((IComponent)clone).Site = ((IComponent)target).Site;
            }
            // We also need to copy the Page over in case runtime properties
            // try to access the page.
            if(clone is System.Web.UI.Control)
            {
                ((Control)clone).Page = ((Control)target).Page;
            }
            CopyOverridableProperties(target, clone);
            return clone;
        }
        
        private void CopyStyleProperties(Style source, Style dest)
        {
            // We copy the StateBag to duplicate the style properties without
            // walking the inheritance.
            dest.State.Clear();
            foreach(String key in source.State.Keys)
            {
                dest.State[key] = source.State[key];
            }
        }

        private void CopyOverridableProperties(Object source, Object dest)
        {
            MobileControl destControl = null;
           
            // HACK: To avoid copying expandable property FontInfo.  We will
            //       need to required that expandable properties implement
            //       ICloneable for our designer extensibility story.
            if(source is Style)
            {
                CopyStyleProperties((Style)source, (Style)dest);
                return;
            }
            
            if(source is MobileControl)
            {
                // If the control is a MobileControl, we copy the style's
                // StateBag to get the non-inherited proprety values.
                destControl = (MobileControl) dest;
                MobileControl sourceControl = (MobileControl) source;
                CopyStyleProperties(sourceControl.Style, destControl.Style);
            }

            // Copy remaining properties not contained in the style (or
            // all properties if not a mobile control.)
            PropertyDescriptorCollection properties = 
                TypeDescriptor.GetProperties(dest.GetType());
            foreach(PropertyDescriptor property in properties)
            {
                if(IsDeviceOverridable(property)
                    && (destControl == null
                    || !PropertyExistsInStyle(property, destControl.Style)))
                {
                    CopyProperty(property, source, dest);
                }
            }
        }

        private void CopyProperty(PropertyDescriptor property,
            Object source,
            Object dest)
        {
            Object value = property.GetValue(source);

            if(property.Converter is ExpandableObjectConverter)
            {
                if(value is ICloneable)
                {
                    value = ((ICloneable)value).Clone();
                }
                else
                {
                    throw new Exception(
                        SR.GetString(
                            SR.PropertyOverridesDialog_NotICloneable,
                            property.Name,
                            property.PropertyType.FullName
                        )
                    );
                }
            }
            property.SetValue(dest, value);
        }
        
        private bool PropertyExistsInStyle(
            PropertyDescriptor property, Style style)
        {
            return style.GetType().GetProperty(property.Name) != null;
        }
        
        public event EventHandler Disposed 
        {
            add 
            {
                _events.AddHandler(_eventDisposed, value);
            }
            remove 
            {
                _events.RemoveHandler(_eventDisposed, value);
            }
        }

        public ISite Site
        {
            get
            {
                Debug.Assert(_site != null);
                return _site;
            }

            set
            {
                _site = value;
            }
        }

        public void Dispose()
        {
            if (_events != null) 
            {
                EventHandler handler = (EventHandler)_events[_eventDisposed];
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        private void OnNavigateUrlChanged(Object sender, EventArgs e)
        {
            OnSpecialPropertyChanged(sender, true);
        }

        private void OnAlternateUrlChanged(Object sender, EventArgs e)
        {
            OnSpecialPropertyChanged(sender, false);
        }

        // 


        private void OnSpecialPropertyChanged(Object sender, bool navigateUrl)
        {
            IComponent component = (IComponent) sender;
            PropertyDescriptor property
                = TypeDescriptor.GetProperties(component)[navigateUrl ? _navigateUrl : _alternateUrl];
            String newValue = (String) property.GetValue(component);
            String oldValue = (String) _specialProp_buffer[navigateUrl ? _navigateUrl : _alternateUrl];
            newValue = NavigateUrlConverter.GetUrl(
                component,
                newValue,
                oldValue
            );
            property.RemoveValueChanged(
                _underlyingObject,
                _specialProp_delegate
            );
            property.SetValue(component, newValue);
            property.AddValueChanged(
                _underlyingObject,
                _specialProp_delegate
            );
        }

        private static bool IsDeviceOverridable(PropertyDescriptor property)
        {
            // 
            return (
                property.IsBrowsable
                && ((!property.IsReadOnly)
                    || (property.Converter is ExpandableObjectConverter))
                && !property.SerializationVisibility.Equals(
                     DesignerSerializationVisibility.Hidden)
                && property.Name != "ID"
            );
        }

        private void ApplyChoiceToRuntimeControl()
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(
                    _underlyingObject.GetType());

            foreach(PropertyDescriptor property in properties)
            {
                if(IsDeviceOverridable(property))
                {
                    ApplyChoiceToRuntimeControl_helper(
                        property,
                        _underlyingObject,
                        ""
                    );
                }
            }
        }

        private void ApplyChoiceToRuntimeControl_helper(
            PropertyDescriptor property,
            Object target,
            String prefix
        ) {
            String propertyName = prefix + property.Name;
            String value = ((IAttributeAccessor)_choice).GetAttribute(propertyName) as String;

            if(property.Converter is ExpandableObjectConverter)
            {
                PropertyDescriptorCollection properties =
                    TypeDescriptor.GetProperties(
                        property.PropertyType
                    );
                foreach(PropertyDescriptor embeddedProperty in properties)
                {
                    if(IsDeviceOverridable(embeddedProperty))
                    {
                        ApplyChoiceToRuntimeControl_helper(
                            embeddedProperty,
                            property.GetValue(target),
                            propertyName + "-"
                        );
                    }
                }
                return;
            }
            
            if(value != null)
            {
                try
                {
                    property.SetValue(
                        target,
                        property.Converter.ConvertFromString(value)
                    );
                }
                catch
                {
                    GenericUI.ShowWarningMessage(
                        SR.GetString(SR.PropertyOverridesDialog_Title),
                        SR.GetString(
                            SR.PropertyOverridesDialog_InvalidPropertyValue,
                            value,
                            propertyName
                        )
                    );
                }
            }
        }

        private void ApplyChangesToRuntimeChoice()
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(
                    _underlyingObject.GetType()
                );

            foreach(PropertyDescriptor property in properties)
            {
                if (IsDeviceOverridable(property))
                {
                    ApplyChangesToRuntimeChoice_helper(
                        property,
                        _copyOfOriginalObject,
                        _underlyingObject,
                        "");
                }
            }
        }
        
        private void ApplyChangesToRuntimeChoice_helper(
            PropertyDescriptor property,
            Object sourceTarget,
            Object destTarget,
            String prefix
        ) {
            Object oldValue = property.GetValue(sourceTarget);
            Object newValue = property.GetValue(destTarget);

            String propertyName = prefix + property.Name;
         
            if(property.Converter is ExpandableObjectConverter)
            {
                PropertyDescriptorCollection properties =
                    TypeDescriptor.GetProperties(
                        newValue.GetType()
                    );
                foreach(PropertyDescriptor embeddedProperty in properties)
                {
                    if(IsDeviceOverridable(embeddedProperty))
                    {
                        ApplyChangesToRuntimeChoice_helper(
                            embeddedProperty,
                            oldValue,
                            newValue,
                            propertyName + "-"
                        );
                    }
                }
            }
            else if(IsDeviceOverridable(property))
            {
                IAttributeAccessor overrides = (IAttributeAccessor)_choice;
                String oldValueString =
                    property.Converter.ConvertToInvariantString(
                        oldValue
                    );
                String newValueString =
                    property.Converter.ConvertToInvariantString(
                        newValue
                    );                
                if(newValueString != oldValueString)
                {
                    overrides.SetAttribute(propertyName, newValueString);
                }
                else
                {
                    // Clear any previous values we might have loaded
                    overrides.SetAttribute(propertyName, null);
                }
            }
        }

        internal DeviceSpecificChoice RuntimeChoice
        {
            get
            {
                ApplyChangesToRuntimeChoice();
                return _choice;
            }
        }

        internal IDeviceSpecificDesigner Designer
        {
            get
            {
                return _designer;
            }
        }

        internal Object Owner
        {
            get
            {
                return _underlyingObject;
            }
        }

        private PropertyDescriptorCollection PreFilterProperties(
            PropertyDescriptorCollection originalProperties
        ) {
            PropertyDescriptorCollection newProperties =
                new PropertyDescriptorCollection(
                    new PropertyDescriptor[] {}
                );
            
            foreach(PropertyDescriptor property in originalProperties)
            {
                if (IsDeviceOverridable(property))
                {
                    newProperties.Add(property);
                }
            }

            PropertyDescriptor[] arpd = new PropertyDescriptor[newProperties.Count];
            for(int i = 0; i < newProperties.Count; i++)
            {
                arpd[i] = newProperties[i];
            }
            newProperties = new PropertyDescriptorCollection(arpd);

            return newProperties;
        }

        ////////////////////////////////////////////////////////////////////
        //  Begin ICustomTypeDescriptor Implementation
        ////////////////////////////////////////////////////////////////////

        System.ComponentModel.AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this.GetType());
        }

        String ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this.GetType());
        }

        String ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this.GetType());
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this.GetType());
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this.GetType());
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this.GetType());
        }

        Object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this.GetType(), editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this.GetType());
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this.GetType(), attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            PropertyDescriptorCollection collection =
                TypeDescriptor.GetProperties(
                    _underlyingObject.GetType()
                );
            collection = PreFilterProperties(collection);
            return collection;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection collection =
                TypeDescriptor.GetProperties(
                    _underlyingObject.GetType(),
                    attributes
                );
            collection = PreFilterProperties(collection);
            return collection;
        }

        Object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor property)
        {
            return _underlyingObject;
        }

        ////////////////////////////////////////////////////////////////////////
        //  End ICustomTypeDescriptor Implementation
        ////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificChoiceDesigner Implementation
        ///////////////////////////////////////////////////////////////////////

        Object IDeviceSpecificChoiceDesigner.UnderlyingObject
        {
            get
            {
                return _designer.UnderlyingObject;
            }
        }

        Control IDeviceSpecificChoiceDesigner.UnderlyingControl
        {
            get
            {
                return _designer.UnderlyingControl;
            }
        }
        
        ///////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificChoiceDesigner Implementation
        ///////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////
    //  End Internal Class
    ////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////
    //  Begin Internal Class
    /////////////////////////////////////////////////////////////////////////

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class PropertyOverridesCachedState : DeviceSpecificDialogCachedState
    {
        private ArrayList _cachedComboBox = null;
        
        internal PropertyOverridesCachedState(
            ComboBox comboBox
        ) {
            _cachedComboBox = new ArrayList();
            foreach(Object o in comboBox.Items)
            {
                _cachedComboBox.Add(o);
            }
        }
        
        internal void Restore(
            ComboBox comboBox
        ) {
            Object selectedItem = comboBox.SelectedItem;
            comboBox.Items.Clear();
            comboBox.Items.AddRange(_cachedComboBox.ToArray());
            if(selectedItem != null)
            {
                int index = comboBox.Items.IndexOf(selectedItem);
                if(index >= 0)
                {
                    comboBox.SelectedItem = comboBox.Items[index];
                }
            }
        }

        internal bool FilterExistsInComboBox(DeviceFilterNode filter)
        {
            foreach(DeviceFilterNode availableFilter in _cachedComboBox)
            {
                if(availableFilter.Name == filter.Name)
                {
                    return true;
                }
            }
            return false;
        }

        internal void SaveChoicesFromComboBox(
            IDeviceSpecificDesigner designer,
            String deviceSpecificID
        ) {
            SaveChoices(designer, deviceSpecificID, _cachedComboBox);
        }

        internal ArrayList Choices
        {
            get
            {
                return _cachedComboBox;
            }
        }
    }
    
    /////////////////////////////////////////////////////////////////////////
    //  End Internal Class
    /////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////
    //  Begin Internal Class
    /////////////////////////////////////////////////////////////////////////

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class InvalidChoiceException : ApplicationException
    {
        internal InvalidChoiceException(String message) : base(message)
        {
        }
    }
    
    /////////////////////////////////////////////////////////////////////////
    //  End Internal Class
    /////////////////////////////////////////////////////////////////////////
}
