//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using View = System.Activities.Presentation.View;
    using System.Activities.Presentation.PropertyEditing;
    using System.Runtime;

    using System.Activities.Presentation.Internal.PropertyEditing.Automation;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using ModelUtilities = System.Activities.Presentation.Internal.PropertyEditing.Model.ModelUtilities;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation.Internal.PropertyEditing.State;
    using System.Text;
    using Microsoft.Activities.Presentation;

    // <summary>
    // The main control that acts as the PropertyInspector
    // </summary>
    [SuppressMessage(FxCop.Category.Naming, "CA1724:TypeNamesShouldNotMatchNamespaces",
        Justification = "Code imported from Cider; keeping changes to a minimum as it impacts xaml files as well")]
    partial class PropertyInspector :
        INotifyPropertyChanged
    {

        private static readonly Size DesiredIconSize = new Size(40, 40);

        private View.Selection _displayedSelection;
        private View.Selection _lastNotifiedSelection;
        private ModelItem _lastParent;

        private bool _ignoreSelectionNameChanges;

        private List<ModelEditingScope> _pendingTransactions = new List<ModelEditingScope>();
        private PropertyValueEditorCommandHandler _defaultCommandHandler;
        private IStateContainer _sessionStateContainer;

        private SelectionPath _lastSelectionPath;
        private bool _objectSelectionInitialized;

        private bool _disposed;
        private bool _isReadOnly;

        private string propertyPathToSelect;

        private ContextItemManager designerContextItemManager;
        private DesignerPerfEventProvider designerPerfEventProvider;

        // Map between currently displayed category editors and the names of the categories they belong to
        private Dictionary<Type, string> _activeCategoryEditors = new Dictionary<Type, string>();

        // <summary>
        // Basic ctor
        // </summary>
        // FxCop complains this.DataContext, which is somewhat bogus
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PropertyInspector()
        {
            this.DataContext = this;

            HookIntoCommands();

            this.InitializeComponent();

            //Handle the commit and cancel keys within the property inspector
            ValueEditorUtils.SetHandlesCommitKeys(this, true);

            _propertyToolBar.CurrentViewManagerChanged += new EventHandler(OnCurrentViewManagerChanged);
        }

        // <summary>
        // Event fired when the IsInAlphaView changes as a result of some
        // user or internal interaction.  When IsInAlphaView is set by the
        // external host, this event will not and should not be fired.
        // </summary>
        public event EventHandler RootViewModified;

        public event PropertyChangedEventHandler PropertyChanged;

        [SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "No need for a Setter")]
        public ContextItemManager DesignerContextItemManager
        {
            set
            {
                this.designerContextItemManager = value;
                this.designerContextItemManager.Subscribe<View.Selection>(this.OnSelectionChanged);
            }
        }

        // <summary>
        // Gets a value indicating whether the selected object Name should be read-only
        // </summary>
        public bool IsInfoBarNameReadOnly
        {
            get
            {
                return _displayedSelection == null || _displayedSelection.SelectionCount != 1;
            }
        }

        // <summary>
        // Gets the selection name to display
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        public string SelectionName
        {
            get
            {
                if (_displayedSelection == null || _displayedSelection.SelectionCount == 0)
                {
                    return null;
                }

                if (_displayedSelection.SelectionCount == 1)
                {
                    return _displayedSelection.PrimarySelection.Name;
                }

                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_MultipleObjectsSelected;
            }
            set
            {
                if (_disposed)
                {
                    return;
                }

                if (CanSetSelectionName(_displayedSelection))
                {
                    ModelItem selection = _displayedSelection.PrimarySelection;
                    Fx.Assert(selection != null, "PrimarySelection should not be null");

                    try
                    {
                        _ignoreSelectionNameChanges = true;

                        using (ModelEditingScope change = selection.BeginEdit(System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_NameChangeUndoText))
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                // Null with cause ClearValue to be called in the base implementation on the NameProperty
                                selection.Name = null;
                            }
                            else
                            {
                                selection.Name = value;
                            }

                            if (change != null)
                                change.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());

                        ErrorReporting.ShowErrorMessage(e.Message);
                    }
                    finally
                    {
                        _ignoreSelectionNameChanges = false;
                    }

                    OnPropertyChanged("SelectionName");
                }
                else
                {
                    Debug.Fail("Shouldn't be able to set a selection name if no or more than one object is selected.");
                }
            }
        }

        // <summary>
        // Gets the icon for the selection
        // </summary>
        public object SelectionIcon
        {
            get
            {
                if (_displayedSelection == null || _displayedSelection.SelectionCount == 0)
                {
                    return null;
                }

                if (_displayedSelection.SelectionCount == 1 || AreHomogenous(_displayedSelection.SelectedObjects))
                {

                    if (_displayedSelection.SelectionCount == 1)
                    {

                        Visual selectedVisual = _displayedSelection.PrimarySelection.View as Visual;
                        // We dont want to show tooltips for elements that derive from "Window" class.  
                        // But we do want to show it for DesignTimeWindow, hence we check the View, so that modelItem returns the correct value 
                        // for designtimewindow.
                        if (selectedVisual != null && !typeof(Window).IsAssignableFrom(_displayedSelection.PrimarySelection.View.GetType()))
                        {
                            // Show a small preview of the selected single object
                            VisualBrush controlBrush = new VisualBrush(selectedVisual);
                            controlBrush.Stretch = Stretch.Uniform;
                            Rectangle rect = new Rectangle();
                            rect.Width = DesiredIconSize.Width;
                            rect.Height = DesiredIconSize.Height;
                            rect.DataContext = string.Empty;

                            // If the control's parent is RTLed, then the VisualBrush "mirrors" the text.
                            // so apply "mirror" transform to "negate" the mirroring.
                            FrameworkElement curElement = selectedVisual as FrameworkElement;
                            FrameworkElement parentElement = curElement.Parent as FrameworkElement;
                            if (parentElement != null && parentElement.FlowDirection == FlowDirection.RightToLeft)
                            {
                                ScaleTransform mirrorTransform = new ScaleTransform(-1, 1);
                                mirrorTransform.CenterX = rect.Width / 2;
                                mirrorTransform.CenterY = rect.Height / 2;
                                controlBrush.Transform = mirrorTransform;
                            }
                            rect.Fill = controlBrush;
                            return rect;
                        }
                        else
                        {
                            // The selected object is not a visual, so show a non-designable object icon
                            return GetEmbeddedImage("NonDesignableSelection.png");
                        }
                    }

                    // Show mutliple-selection of the same type icon
                    return GetEmbeddedImage("MultiSelectionSameType.png");
                }

                // Show multiple-selection of different types icon
                return GetEmbeddedImage("MultiSelectionDifferentType.png");
            }
        }

        // <summary>
        // Gets the Type name for the current selection
        // </summary>
        public string SelectionTypeName
        {
            get
            {
                if (_displayedSelection == null || _displayedSelection.SelectionCount == 0)
                {
                    return null;
                }

                if (_displayedSelection.SelectionCount == 1 || AreHomogenous(_displayedSelection.SelectedObjects))
                {
                    return GetStringRepresentation(_displayedSelection.PrimarySelection.ItemType);
                }

                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_MultipleTypesSelected;
            }
        }

        static string GetStringRepresentation(Type type)
        {
            return TypeNameHelper.GetDisplayName(type, true);
        }

        // Property View

        // <summary>
        // Gets the state that should be persisted while the host is
        // running, but discarded when the host shuts down.
        // </summary>
        public object SessionState
        {
            get
            {
                // Don't instantiate the SessionStateContainer until
                // CategoryList has been instantiated.  Otherwise, we would
                // get an invalid container.
                if (_categoryList == null)
                {
                    return null;
                }

                return SessionStateContainer.RetrieveState();
            }
            set
            {
                // Don't instantiate the SessionStateContainer until
                // CategoryList has been instantiated.  Otherwise, we would
                // get an invalid container.
                if (_categoryList == null || value == null)
                {
                    return;
                }

                SessionStateContainer.RestoreState(value);

                _objectSelectionInitialized = false;
            }
        }

        public bool IsReadOnly
        {
            get { return this._isReadOnly; }
            internal set
            {
                this._isReadOnly = value;
                this._categoryList.Opacity = this._isReadOnly ? 0.8 : 1.0;
                this._categoryList.ToolTip = this._isReadOnly ? this.FindResource("editingDisabledHint") : null;
                this.OnPropertyChanged("IsReadOnly");
            }
        }

        // <summary>
        // Gets or sets a flag indicating whether the root PropertyInspector
        // control is in alpha-view.  We isolate this state from any other
        // to make VS integration easier.
        // </summary>
        public bool IsInAlphaView
        {
            get { return _propertyToolBar.IsAlphaViewSelected; }
            set { _propertyToolBar.IsAlphaViewSelected = value; }
        }

        private void SelectPropertyByPathOnIdle()
        {
            SelectionPath selectionPath =
                new SelectionPath(PropertySelectionPathInterpreter.PropertyPathTypeId, propertyPathToSelect);
            bool pendingGeneration;
            bool result = this._categoryList.SetSelectionPath(selectionPath, out pendingGeneration);
            if (!result && pendingGeneration)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new MethodInvoker(SelectPropertyByPathOnIdle));
            }
        }

        internal void SelectPropertyByPath(string path)
        {
            this.propertyPathToSelect = path;
            // must do it in application idle time, otherwise the propertygrid is not popugrated yet.
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new MethodInvoker(SelectPropertyByPathOnIdle));
        }

        internal TextBlock SelectionTypeLabel
        { get { return _typeLabel; } }
        //internal TextBlock SelectionNameLabel 
        //{ get { return _nameLabel; } }
        //internal StringEditor SelectionNameEditor 
        //{ get { return _nameEditor; } }
        internal PropertyToolBar PropertyToolBar
        { get { return _propertyToolBar; } }
        internal TextBlock NoSearchResultsLabel
        { get { return _noSearchResultsLabel; } }
        internal TextBlock UninitializedLabel
        { get { return _uninitializedLabel; } }
        internal CategoryList CategoryList
        { get { return _categoryList; } }

        internal EditingContext EditingContext { get; set; }

        private DesignerPerfEventProvider DesignerPerfEventProvider
        {
            get
            {
                if (this.designerPerfEventProvider == null && this.EditingContext != null)
                {
                    this.designerPerfEventProvider = this.EditingContext.Services.GetService<DesignerPerfEventProvider>();
                }
                return this.designerPerfEventProvider;
            }
        }

        private SelectionPath LastSelectionPath
        {
            get { return _lastSelectionPath; }
            set { _lastSelectionPath = value; }
        }

        private IStateContainer SessionStateContainer
        {
            get
            {
                if (_categoryList == null)
                {
                    return null;
                }

                if (_sessionStateContainer == null)
                {
                    _sessionStateContainer = new AggregateStateContainer(
                        PropertyStateContainer.Instance,
                        _categoryList,
                        new SelectionPathStateContainer(this),
                        PropertyActiveEditModeStateContainer.Instance,
                        PropertyViewManagerStateContainer.Instance);
                }

                return _sessionStateContainer;
            }
        }

        // IPropertyInspectorState

        internal void Dispose()
        {
            _disposed = true;
            DisassociateAllProperties();
            UpdateSelectionPropertyChangedEventHooks(_displayedSelection, null);
            _displayedSelection = null;
            _defaultCommandHandler.Dispose();
            _defaultCommandHandler = null;
        }

        private void HookIntoCommands()
        {
            // Use a helper classes to handle all the standard PI commands
            _defaultCommandHandler = new PropertyValueEditorCommandHandler(this);
        }

        // <summary>
        // Marks all shown properties as disassociated which disables all modifications
        // done to them through the PI model objects.
        // </summary>
        private void DisassociateAllProperties()
        {
            if (_categoryList != null && _categoryList.IsLoaded)
            {
                foreach (ModelCategoryEntry category in _categoryList)
                {
                    category.MarkAllPropertiesDisassociated();
                }
            }
        }

        // Properties

        private void OnCurrentViewManagerChanged(object sender, EventArgs e)
        {
            this.RefreshPropertyList(false);

            // Isolate the current view of the root PropertyInspector into
            // its own separate flag and event to appease the VS ----s
            //
            if (this.RootViewModified != null)
            {
                RootViewModified(null, EventArgs.Empty);
            }
        }

        private void RefreshPropertyList(bool attachedOnly)
        {
            UpdateCategories(_lastNotifiedSelection, attachedOnly);
            UpdateCategoryEditors(_lastNotifiedSelection);

            //
            // The first time SelectionChanges, there is nothing selected, so don't store the
            // current property selected.  It would just overwrite the selection path that we
            // received from SelectionPathStateContainer, which is not what we want.
            //
            if (_objectSelectionInitialized)
            {
                LastSelectionPath = _categoryList.SelectionPath;
            }

            _objectSelectionInitialized = true;

            //
            // Call UpdateSelectedProperty() _after_ the UI renders.  We need to set PropertySelection.IsSelected
            // property on a templated visual objects (CategoryContainer, PropertyContainer) and those may not exist yet. 
            //
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new UpdateSelectedPropertyInvoker(UpdateSelectedProperty), _lastNotifiedSelection);
        }


        // Selection Logic

        // SelectionPathStateContainer

        // <summary>
        // Called externally whenever selection changes
        // </summary>
        // <param name="selection">New selection</param>
        public void OnSelectionChanged(View.Selection selection)
        {
            _lastNotifiedSelection = selection;
            RefreshSelection();
        }

        // <summary>
        // Called when visibility of the PropertyBrowserPane changes and the
        // PropertyInspector may be showing a stale selection.  This method is identical
        // to OnSelectionChanged() but with no new selection instance introduced.
        // </summary>
        public void RefreshSelection()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new MethodInvoker(OnSelectionChangedIdle));
        }

        // Updates PI when the application becomes Idle (perf optimization)
        private void OnSelectionChangedIdle()
        {
            if (DesignerPerfEventProvider != null)
            {
                DesignerPerfEventProvider.PropertyInspectorUpdatePropertyListStart();
            }

            if (AreSelectionsEquivalent(_lastNotifiedSelection, _displayedSelection))
            {
                return;
            }

            if (!VisualTreeUtils.IsVisible(this))
            {
                return;
            }

            // Change the SelectedControlFlowDirectionRTL resource property
            // This will allow the 3rd party editors to look at this property
            // and change to RTL for controls that support RTL. 
            // We set the resource to the primary selections RTL property. 
            FlowDirection commmonFD = this.FlowDirection;
            if (_lastNotifiedSelection != null && _lastNotifiedSelection.PrimarySelection != null)
            {

                FrameworkElement selectedElement = _lastNotifiedSelection.PrimarySelection.View as FrameworkElement;
                if (selectedElement != null)
                {
                    commmonFD = selectedElement.FlowDirection;
                }

                // In case of mulitislection, 
                // if the FlowDirection is different then always set it to LTR.
                // else set it to common FD.
                if (_lastNotifiedSelection.SelectionCount > 1)
                {
                    foreach (ModelItem item in _lastNotifiedSelection.SelectedObjects)
                    {
                        FrameworkElement curElm = item.View as FrameworkElement;
                        if (curElm != null && curElm.FlowDirection != commmonFD)
                        {
                            //reset to LTR (since the FD's are different within multiselect)
                            commmonFD = FlowDirection.LeftToRight;
                            break;
                        }
                    }
                }
            }

            PropertyInspectorResources.GetResources()["SelectedControlFlowDirectionRTL"] = commmonFD;

            RefreshPropertyList(false);

            UpdateSelectionPropertyChangedEventHooks(_displayedSelection, _lastNotifiedSelection);
            _displayedSelection = _lastNotifiedSelection;
            _lastParent = GetCommonParent(_lastNotifiedSelection);

            // Handle dangling transactions
            _defaultCommandHandler.CommitOpenTransactions();

            OnPropertyChanged("IsInfoBarNameReadOnly");
            OnPropertyChanged("SelectionName");
            OnPropertyChanged("SelectionIcon");
            OnPropertyChanged("SelectionTypeName");
        }

        // Removes / adds a PropertyChanged listener from / to the previous / current selection
        private void UpdateSelectionPropertyChangedEventHooks(View.Selection previousSelection, View.Selection currentSelection)
        {
            if (previousSelection != null && previousSelection.PrimarySelection != null)
            {
                previousSelection.PrimarySelection.PropertyChanged -= OnSelectedItemPropertyChanged;
            }

            if (currentSelection != null && currentSelection.PrimarySelection != null)
            {
                currentSelection.PrimarySelection.PropertyChanged += OnSelectedItemPropertyChanged;
            }
        }

        private void OnSelectedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_ignoreSelectionNameChanges)
            {
                return;
            }

            // PS 40699 - Name is not a special property for WF
            //if ("Name".Equals(e.PropertyName))
            //{
            //  OnSelectedItemNameChanged();
            //}

            if ("Parent".Equals(e.PropertyName))
            {
                OnParentChanged();
            }
        }

        // Called when the name changes
        private void OnSelectedItemNameChanged()
        {
            OnPropertyChanged("SelectionName");
        }

        // Called when the parent of the current selection changes
        private void OnParentChanged()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new MethodInvoker(OnParentChangedIdle));
        }

        private void OnParentChangedIdle()
        {
            if (_displayedSelection == null || _displayedSelection.SelectionCount < 1)
            {
                return;
            }

            ModelItem newParent = GetCommonParent(_displayedSelection);

            if (_lastParent != newParent)
            {
                RefreshPropertyList(true);
                _lastParent = newParent;
            }
        }

        // Looks for common parent ModelItem among all the items in the selection
        private static ModelItem GetCommonParent(View.Selection selection)
        {
            if (selection == null || selection.SelectionCount < 1)
            {
                return null;
            }

            ModelItem parent = null;
            foreach (ModelItem item in selection.SelectedObjects)
            {
                if (parent == null)
                {
                    parent = item.Parent;
                }
                else if (parent != item.Parent)
                {
                    return null;
                }
            }

            return parent;
        }

        // The user can only specify the name for the selected objects iff exactly one
        // object is selected.
        private static bool CanSetSelectionName(View.Selection selection)
        {
            return selection != null && selection.SelectionCount == 1;
        }

        private static bool AreSelectionsEquivalent(View.Selection a, View.Selection b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.SelectionCount != b.SelectionCount)
            {
                return false;
            }

            // POSSIBLE OPTIMIZATION: be smarter about same selection in a different order
            IEnumerator<ModelItem> ea = a.SelectedObjects.GetEnumerator();
            IEnumerator<ModelItem> eb = b.SelectedObjects.GetEnumerator();

            while (ea.MoveNext() && eb.MoveNext())
            {
                if (!object.Equals(ea.Current, eb.Current))
                {
                    return false;
                }
            }

            return true;
        }

        // This is the work-horse that refreshes the list of properties and categories within a PropertyInspector
        // window, including refreshing of CategoryEditors, based on the specified selection
        private void UpdateCategories(View.Selection selection, bool attachedOnly)
        {

            // Optimization stolen from Sparkle:
            // re-rendering the categories is the number one perf issue. Clearing
            // the databound collection results in massive Avalon code execution, and
            // then re-adding everything causes another huge shuffle. What is more, 
            // even when changing the selection between different objects, most properties
            // do not change. Therefore we are going to take the new list of properties 
            // and we are going to merge them into the existing stuff, using an 
            // approach I call Mark, Match, and Cull.
            //
            // First we mark all the properties in the current collection. Those which 
            // are still marked at the end will be culled out
            foreach (ModelCategoryEntry category in _categoryList)
            {
                if (attachedOnly)
                {
                    category.MarkAttachedPropertiesDisassociated();
                }
                else
                {
                    category.MarkAllPropertiesDisassociated();
                }
            }

            // Second we try to match each property in the list of properties for the newly selected objects 
            // against something that we already have. If we have a match, then we reset the existing
            // ModelPropertyEntry and clear the mark
            //
            foreach (IEnumerable<ModelProperty> propertySet in
                ModelPropertyMerger.GetMergedProperties(
                selection == null ? null : selection.SelectedObjects,
                selection == null ? 0 : selection.SelectionCount))
            {

                string propertyName = GetPropertyName(propertySet);

                // Specifically filter out the Name property
                // PS 40699 - Name is not a special property for WF
                //if ("Name".Equals(propertyName))
                //{
                //    continue;
                //}

                if (attachedOnly && propertyName.IndexOf('.') < 0)
                {
                    continue;
                }

                ModelPropertyEntry wrappedProperty = _propertyToolBar.CurrentViewManager.AddProperty(propertySet, propertyName, _categoryList);

                // Make sure no valid properties get culled out
                wrappedProperty.Disassociated = false;
            }

            // Third, we walk the properties and categories, and we cull out all of the
            // marked properties. Empty categories are removed.
            //
            for (int i = _categoryList.Count - 1; i >= 0; i--)
            {
                ModelCategoryEntry category = (ModelCategoryEntry)_categoryList[i];
                category.CullDisassociatedProperties();
                if (category.IsEmpty)
                {
                    _categoryList.RemoveAt(i);
                }
            }

            _categoryList.RefreshFilter();
        }

        // Helper method that adjusts the visible set of CategoryEditors based on the specified selection
        private void UpdateCategoryEditors(View.Selection selection)
        {

            // Figure out which category editors to show
            Dictionary<Type, object> newCategoryEditorTypes = _propertyToolBar.CurrentViewManager.GetCategoryEditors(
                FindCommonType(selection == null ? null : selection.SelectedObjects),
                _categoryList);

            // Figure out which CategoryEditors are no longer needed and remove them
            List<Type> editorTypesToRemove = null;
            foreach (KeyValuePair<Type, string> item in _activeCategoryEditors)
            {
                if (!newCategoryEditorTypes.ContainsKey(item.Key) || !IsCategoryShown(item.Key))
                {

                    // New selection does not include this existing category editor 
                    // or the category that contains this editor
                    // so remove the editor.
                    if (editorTypesToRemove == null)
                    {
                        editorTypesToRemove = new List<Type>();
                    }

                    editorTypesToRemove.Add(item.Key);
                }
                else
                {
                    // This category editor already exists, so don't re-add it
                    newCategoryEditorTypes.Remove(item.Key);
                }
            }

            if (editorTypesToRemove != null)
            {
                foreach (Type editorTypeToRemove in editorTypesToRemove)
                {
                    ModelCategoryEntry affectedCategory = _categoryList.FindCategory(_activeCategoryEditors[editorTypeToRemove]) as ModelCategoryEntry;
                    if (affectedCategory != null)
                    {
                        affectedCategory.RemoveCategoryEditor(editorTypeToRemove);
                    }

                    _activeCategoryEditors.Remove(editorTypeToRemove);
                }
            }

            // Figure out which CategoryEditors are now required and add them
            foreach (Type editorTypeToAdd in newCategoryEditorTypes.Keys)
            {
                CategoryEditor editor = (CategoryEditor)ExtensibilityAccessor.SafeCreateInstance(editorTypeToAdd);
                if (editor == null)
                {
                    continue;
                }

                ModelCategoryEntry affectedCategory = _categoryList.FindCategory(editor.TargetCategory) as ModelCategoryEntry;
                if (affectedCategory == null)
                {
                    continue;
                }

                affectedCategory.AddCategoryEditor(editor);
                _activeCategoryEditors[editorTypeToAdd] = editor.TargetCategory;
            }
        }

        // Check if the category is shown for the current category editor type
        private bool IsCategoryShown(Type categoryEditorType)
        {
            bool ret = true;
            CategoryEditor editorToRemove = (CategoryEditor)ExtensibilityAccessor.SafeCreateInstance(categoryEditorType);
            if (editorToRemove != null)
            {
                ModelCategoryEntry affectedCategory = _categoryList.FindCategory(editorToRemove.TargetCategory) as ModelCategoryEntry;
                if (affectedCategory == null)
                {
                    ret = false;
                }
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        // Tries to figure out what property to select and selects is
        private void UpdateSelectedProperty(View.Selection selection)
        {

            // If we are not loaded, skip any and all selection magic
            if (!this.IsLoaded)
            {
                return;
            }

            if (selection != null)
            {

                // See what the view would like us to select if we run out of things
                // we can think of selecting
                //
                SelectionPath fallbackSelection = null;
                if (_propertyToolBar.CurrentViewManager != null)
                {
                    fallbackSelection = _propertyToolBar.CurrentViewManager.GetDefaultSelectionPath(_categoryList);
                }

                // Select the first thing we request that exists, using the following
                // precedence order:
                //
                //  * LastSelectionPath
                //  * DefaultProperty
                //  * Whatever the view wants to show (first category, first property, ...)
                //
                _categoryList.UpdateSelectedProperty(
                    this.LastSelectionPath,
                    ModelPropertyMerger.GetMergedDefaultProperty(selection.SelectedObjects),
                    fallbackSelection);
            }

            if (DesignerPerfEventProvider != null)
            {
                DesignerPerfEventProvider.PropertyInspectorUpdatePropertyListEnd();
            }
        }

        private static Type FindCommonType(IEnumerable<ModelItem> modelItems)
        {
            Type commonType = null;

            if (modelItems != null)
            {
                foreach (ModelItem selectedItem in modelItems)
                {
                    if (commonType == null)
                    {
                        commonType = selectedItem.ItemType;
                    }
                    else
                    {
                        commonType = ModelUtilities.GetCommonAncestor(commonType, selectedItem.ItemType);
                    }
                }
            }

            return commonType;
        }

        private static bool AreHomogenous(IEnumerable<ModelItem> items)
        {
            Fx.Assert(items != null, "items parameter is null");

            Type type = null;
            foreach (ModelItem item in items)
            {
                if (type == null)
                {
                    type = item.ItemType;
                }
                else if (type != item.ItemType)
                {
                    return false;
                }
            }

            return true;
        }

        // Static Helpers

        private static string GetPropertyName(IEnumerable<ModelProperty> propertySet)
        {
            if (propertySet == null)
            {
                return null;
            }
            foreach (ModelProperty property in propertySet)
            {
                return property.Name;
            }
            return null;
        }

        private static Image GetEmbeddedImage(string imageName)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(
                string.Concat(
                "/System.Activities.Presentation;component/System/Activities/Presentation/Base/Core/Internal/PropertyEditing/Resources/",
                imageName),
                UriKind.RelativeOrAbsolute));
            return image;
        }


        // AutomationPeer Stuff

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PropertyInspectorAutomationPeer(this);
        }


        // Cross-domain State Storage

        // <summary>
        // Clears the FilterString
        // </summary>
        public void ClearFilterString()
        {
            _categoryList.FilterString = null;
        }

        // INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private delegate void MethodInvoker();
        private delegate void UpdateSelectedPropertyInvoker(View.Selection selection);

        // Container for property-selection state represented by SelectionPath.
        // Since we receive a stored SelectionPath on the reload of this control,
        // at which point the visuals themselves have not been rendered yet, we
        // store the supplied SelectionPath instance and use it to select the 
        // correct property only after the UI has been rendered.
        //
        private class SelectionPathStateContainer : IStateContainer
        {
            private PropertyInspector _parent;

            public SelectionPathStateContainer(PropertyInspector parent)
            {
                if (parent == null)
                {
                    throw FxTrace.Exception.ArgumentNull("parent");
                }
                _parent = parent;
            }

            //
            // Pulls the SelectionPath from the CategoryList, but only if it was Sticky,
            // meaning we should preserve it
            //
            public object RetrieveState()
            {
                if (_parent.CategoryList != null)
                {
                    SelectionPath path = _parent._objectSelectionInitialized ? _parent.CategoryList.SelectionPath : _parent.LastSelectionPath;
                    return path == null ? null : path.State;
                }

                return null;
            }

            //
            // Pulls the SelectionPath from the CategoryList, but only if it was Sticky,
            // meaning we should preserve it
            //
            public void RestoreState(object state)
            {
                if (state != null)
                {
                    SelectionPath restoredPath = SelectionPath.FromState(state);
                    _parent.LastSelectionPath = restoredPath;
                }
            }
        }
    }
}
