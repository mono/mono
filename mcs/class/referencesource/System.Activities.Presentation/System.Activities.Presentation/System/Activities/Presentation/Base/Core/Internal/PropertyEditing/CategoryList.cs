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
    using System.Globalization;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    using System.Activities.Presentation;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Automation;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // Wrapper around ItemsControl that knows how to contain and display CategoryEntries,
    // deals with AutomationPeers, and persists the open and closed state of individual
    // CategoryContainers.
    //
    // This class should ideally be internal, but Avalon can't handle attached properties
    // (which this class defines) on internal classes.
    // </summary>
    internal class CategoryList : ItemsControl, IEnumerable<CategoryBase>, IStateContainer, INotifyPropertyChanged 
    {

        // This guy is static so that its values persist across designers and CategoryList instances
        private static CategoryStateContainer _categoryStates = new CategoryStateContainer();

        // This guy is not because it caches FilterString, which is specific to each CategoryList instance
        private IStateContainer _stateContainer;

        // Used for property selection
        private FrameworkElement _selection;
        private PropertySelectionMode _selectionMode;

        // Used for property filtering
        private string _filterString;
        private PropertyFilter _currentFilter;
        private bool _hasAnyFilterMatches = true;
        private ICommand _clearFilterCommand;

        // Miscelaneous
        private SharedPropertyValueColumnWidthContainer _sharedWidthContainer;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CategoryList() 
        {
            // Make CategoryList non-focusable by default
            UIElement.FocusableProperty.OverrideMetadata(typeof(CategoryList), new FrameworkPropertyMetadata(false));

            // Mark the uber-CategoryList as the scope for property selection
            PropertySelection.IsSelectionScopeProperty.OverrideMetadata(typeof(CategoryList), new PropertyMetadata(true));
        }

        // <summary>
        // Basic ctor
        // </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CategoryList() 
        {

            _stateContainer = new AggregateStateContainer(
                new CategoryListStateContainer(this),
                _categoryStates);

            // Setup the shared width container
            _sharedWidthContainer = new SharedPropertyValueColumnWidthContainer();
            SharedPropertyValueColumnWidthContainer.SetOwningSharedPropertyValueColumnWidthContainer(this, _sharedWidthContainer);

            // When someone new gets focus, we may need to mess around with selected property, so listen to the event
            this.AddHandler(FocusManager.GotFocusEvent, new RoutedEventHandler(OnSomeoneGotFocus));

            // When editing is done in the value editor, shift focus back to the selected property
            this.CommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.FinishEditing, OnFinishEditing));

            // Need to call this method from a UI thread because some of Sparkle's value editors rely on it
            UIThreadDispatcher.InitializeInstance();
        }

        // <summary>
        // Event fired whenever a new CategoryContainer instance is generated
        // </summary>
        public event ContainerGeneratedHandler ContainerGenerated;

        // AutomationPeer

        public event PropertyChangedEventHandler PropertyChanged;

        public int Count 
        {
            get {
                return this.Items.Count;
            }
        }

        // <summary>
        // Gets or sets the filter string to apply to the shown properties.
        // Setting this value will automatically apply the filter to the shown properties.
        // </summary>
        public string FilterString 
        {
            get {
                return _filterString;
            }
            set {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }

                if (_filterString != value) 
                {
                    _filterString = value;
                    _currentFilter = new PropertyFilter(_filterString);
                    RefreshFilter();
                    this.OnPropertyChanged("FilterString");
                }
            }
        }

        // <summary>
        // Command-wrapper for the ClearFilter method
        // </summary>
        public ICommand ClearFilterCommand 
        {
            get {
                if (_clearFilterCommand == null) 
                {
                    _clearFilterCommand = new DelegateCommand(this.ClearFilter);
                }
                return _clearFilterCommand;
            }
        }

        // <summary>
        // Gets a value indicating whether there are any categories or properties that
        // match the current filter string
        // </summary>
        public bool HasAnyFilterMatches 
        {
            get {
                return _hasAnyFilterMatches;
            }
            private set {
                if (_hasAnyFilterMatches != value) 
                {
                    _hasAnyFilterMatches = value;
                    this.OnPropertyChanged("HasAnyFilterMatches");
                }
            }
        }

        // <summary>
        // Gets the currently selected visual.
        // </summary>
        public FrameworkElement Selection 
        {
            get {
                return _selection;
            }
        }

        // <summary>
        // Gets or sets the path to the currently selected item in the CategoryList.
        // Only "sticky" selections will return a valid SelectionPath - otherwise
        // null is returned.  Setting this property changes the selection if the specified path
        // can be resolved to a FrameworkElement to select and sets the SelectionMode to
        // Sticky.
        // </summary>
        public SelectionPath SelectionPath 
        {
            get {
                if (_selection == null) 
                {
                    return null;
                }
                if (this.SelectionMode != PropertySelectionMode.Sticky) 
                {
                    return null;
                }

                ISelectionStop selectionStop = PropertySelection.GetSelectionStop(_selection);
                if (selectionStop == null) 
                {
                    return null;
                }

                return selectionStop.Path;
            }
            set {
                SetSelectionPath(value);
            }
        }

        // <summary>
        // Gets or sets the current SelectionMode
        // </summary>
        private PropertySelectionMode SelectionMode 
        {
            get { return _selectionMode; }
            set { _selectionMode = value; }
        }

        public CategoryEntry this[int index] {
            get {
                return (CategoryEntry)this.Items[index];
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new CategoryListAutomationPeer(this);
        }


        // Convenience Accessors

        public void Insert(int index, CategoryEntry category) 
        {
            if (category == null)
            {
                throw FxTrace.Exception.ArgumentNull("category");
            }

            this.Items.Insert(index, category);
        }

        public void InsertAlphabetically(CategoryEntry category) 
        {

            // POSSIBLE OPTIMIZATION: optimize using the fact that the list of categories in this
            // collection is ordered.
            int index = 0;
            for (; index < this.Count; index++) 
            {
                if (string.Compare(category.CategoryName, this[index].CategoryName, StringComparison.CurrentCulture) < 0)
                {
                    break;
                }
            }

            this.Insert(index, category);
        }

        public void RemoveAt(int index) 
        {
            this.Items.RemoveAt(index);
        }


        // Command Handlers

        private void OnFinishEditing(object sender, ExecutedRoutedEventArgs e) 
        {
            // Re-focus the selected selection stop
            this.SynchronizeSelectionFocus(StealFocusMode.Always);
        }


        // IStateContainer

        public void RestoreState(object state) 
        {
            _stateContainer.RestoreState(state);
        }

        public object RetrieveState() 
        {
            return _stateContainer.RetrieveState();
        }

        // This override both gets rid of the inbetween ContentPresenter and
        // it makes the CategoryContainer available as an instance in PrepareContainerForItemOverride()
        protected override DependencyObject GetContainerForItemOverride() 
        {
            return new CiderCategoryContainer();
        }

        // Set the expansion state on the CategoryContainer based on an existing container
        // or a cached value for the contained category
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) 
        {
            CiderCategoryContainer container = element as CiderCategoryContainer;
            CategoryEntry category = item as CategoryEntry;

            if (container != null && category != null) 
            {

                // Ideally, we would want to initalize the expansion state here.  However,
                // in collection editor, Blend messes around with the expansion state _after_
                // the call to this method, which breaks our ability to remember which categories
                // were open and which were closed.  So, we set the state here (because Blend's
                // search logic relies on it to be set by this point) and _reset_ it in the Loaded
                // event handler.

                // Look into stored state
                CategoryState state = _categoryStates.GetCategoryState(category.CategoryName);
                container.Expanded = state.CategoryExpanded;
                container.AdvancedSectionPinned = state.AdvancedSectionExpanded;

                // Hook into other event handlers that we care about (including the Loaded event)
                container.Loaded += new RoutedEventHandler(OnContainerLoaded);
                container.Unloaded += new RoutedEventHandler(OnContainerUnloaded);

                if (ContainerGenerated != null)
                {
                    ContainerGenerated(this, new ContainerGeneratedEventArgs(container));
                }
            }
            else 
            {
                Debug.Fail("CategoryList should only be populated with CategoryEntries.");
            }

            base.PrepareContainerForItemOverride(element, item);
        }

        // Re-initialize the expansion state here and hook into the ExpandedChanged and
        // AdvancedSectionPinnedChanged events, because by now Blend may have ----ed those
        // two values up (see comment in PrepareContainerForItemOverride() )
        private void OnContainerLoaded(object sender, RoutedEventArgs e) 
        {
            CiderCategoryContainer container = sender as CiderCategoryContainer;

            if (container != null) 
            {

                CategoryEntry category = container.Category;
                if (category != null) 
                {
                    // Look into stored state
                    CategoryState state = _categoryStates.GetCategoryState(category.CategoryName);
                    container.Expanded = state.CategoryExpanded;
                    container.AdvancedSectionPinned = state.AdvancedSectionExpanded;

                    // Hook into these events here, because Blend won't mess the state up at this point
                    container.ExpandedChanged += new EventHandler(OnContainerExpandedChanged);
                    container.AdvancedSectionPinnedChanged += new EventHandler(OnAdvancedSectionPinnedChanged);
                }
            }
            else 
            {
                Debug.Fail("CategoryList expects the individual items to be CiderCategoryContainers");
            }
        }

        private void OnContainerUnloaded(object sender, RoutedEventArgs e) 
        {
            CiderCategoryContainer container = sender as CiderCategoryContainer;
            if (container != null) 
            {
                // Unhook from any events that we used to care about
                container.ExpandedChanged -= new EventHandler(OnContainerExpandedChanged);
                container.AdvancedSectionPinnedChanged -= new EventHandler(OnAdvancedSectionPinnedChanged);
                container.Loaded -= new RoutedEventHandler(OnContainerLoaded);
                container.Unloaded -= new RoutedEventHandler(OnContainerUnloaded);
            }
            else 
            {
                Debug.Fail("Couldn't clean up event binding and store container state.");
            }
        }

        private void OnContainerExpandedChanged(object sender, EventArgs e) 
        {
            // If we are in "Filter-applied" mode, don't store the expansion state, since applying
            // the filter automatically expands everything that matches that filter
            if (_currentFilter != null && !_currentFilter.IsEmpty)
            {
                return;
            }

            CiderCategoryContainer container = sender as CiderCategoryContainer;
            if (container != null) 
            {
                CategoryEntry category = container.Category;
                if (category != null) 
                {
                    CategoryState state = _categoryStates.GetCategoryState(container.Category.CategoryName);
                    state.CategoryExpanded = container.Expanded;
                }
            }
        }

        private void OnAdvancedSectionPinnedChanged(object sender, EventArgs e) 
        {
            // If we are in "Filter-applied" mode, don't store the expansion state, since applying
            // the filter automatically expands everything that matches that filter
            if (_currentFilter != null && !_currentFilter.IsEmpty)
            {
                return;
            }

            CiderCategoryContainer container = sender as CiderCategoryContainer;
            if (container != null) 
            {
                CategoryEntry category = container.Category;
                if (category != null) 
                {
                    CategoryState state = _categoryStates.GetCategoryState(container.Category.CategoryName);
                    state.AdvancedSectionExpanded = container.AdvancedSectionPinned;
                }
            }
        }

        // Searching

        // <summary>
        // Applies the current filter to the existing list of categories and properties
        // and updates the value of HasAnyFilterMatches.  This class does not update itself
        // automatically when new CategoryEntries are added or removed, so call this method
        // explicitly when things change.
        // </summary>
        public void RefreshFilter() 
        {
            bool? matchesFilter = null;

            foreach (CategoryBase category in this.Items) 
            {
                matchesFilter = matchesFilter == null ? false : matchesFilter;
                matchesFilter |= ApplyFilter(_currentFilter, category);
            }

            this.HasAnyFilterMatches = matchesFilter == null ? true : (bool)matchesFilter;
        }

        // <summary>
        // Clears the current property filter, if any
        // </summary>
        public void ClearFilter() 
        {
            this.FilterString = null;
        }

        // Applies the specified filter to the specified category, returning a boolean indicating
        // whether anything in that category matched the filter or not
        private static bool ApplyFilter(PropertyFilter filter, CategoryBase category) 
        {
            category.ApplyFilter(filter);
            return category.MatchesFilter || category.BasicPropertyMatchesFilter || category.AdvancedPropertyMatchesFilter;
        }


        // Property Selection

        // <summary>
        // Sets the SelectionMode back to default.  This is a common enough
        // operation that it makes sense to abstract it to its own method.
        // </summary>
        private void ResetSelectionMode() 
        {
            SelectionMode = PropertySelectionMode.Default;
        }

        // <summary>
        // Updates property selection to the specified SelectionPath (if any)
        // or the specified default property.  Returns true if some property was selected,
        // false otherwise (such as in the case when no properties are showing).
        // </summary>
        // <param name="stickyPath">SelectionPath to select.  Takes precedence over default property.</param>
        // <param name="defaultPropertyName">Property to select when no SelectionPath is specified or
        // if the path cannot be resolved.</param>
        // <param name="fallbackPath">SelectionPath to use when all else fails.  May be null.</param>
        // <returns>True if some property was selected, false otherwise (such as in the case
        // when no properties are showing).</returns>
        public bool UpdateSelectedProperty(
            SelectionPath stickyPath,
            string defaultPropertyName,
            SelectionPath fallbackPath) 
        {

            // First, try selecting the given stickyPath, if any
            //
            if (stickyPath == null || !SetSelectionPath(stickyPath))
            {
                ResetSelectionMode();
            }

            bool propertySelected;

            if (SelectionMode == PropertySelectionMode.Default) 
            {

                // Then, try finding and selecting the default property
                //
                propertySelected = defaultPropertyName == null ? false : SelectDefaultProperty(defaultPropertyName);
                if (!propertySelected && fallbackPath != null) 
                {

                    // And if that fails, go to the specified fallback SelectionPath,
                    // if any
                    //
                    propertySelected = SetSelectionPath(fallbackPath);
                }

                // Make sure that we are still in Default selection mode
                // at this point
                //
                ResetSelectionMode();
            }
            else 
            {
                propertySelected = true;
            }

            return propertySelected;
        }

        private bool SetSelectionPath(SelectionPath path) 
        {
            // Dummy, this variable is only to satisfy SetSelectionPath(SelectionPath path, out bool pendingGeneration)
            bool isPendingGenerationDummy;
            return SetSelectionPath(path, out isPendingGenerationDummy);
        }

        // Attempts to resolve the specified path and set it as the current selection, returning
        // true or false based on success.  If the path is found, selection is set to Sticky.
        // If the UI is not ready, return false and pendingGeneration is true.
        internal bool SetSelectionPath(SelectionPath path, out bool pendingGeneration)
        {
            DependencyObject newSelection = SelectionPathResolver.ResolveSelectionPath(this, path, out pendingGeneration);
            if (newSelection != null)
            {
                SelectAndFocus(newSelection, StealFocusMode.OnlyIfCategoryListHasFocusWithin);
                this.SelectionMode = PropertySelectionMode.Sticky;
                return true;
            }

            return false;
        }

        // When the user clicks somewhere, we try to find the closest parent with IsSelectionStop DP set to true and
        // select it.
        //
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) 
        {
            Select(e.OriginalSource as DependencyObject);
            base.OnPreviewMouseDown(e);
        }

        // If we set Focus in OnMouseDown, it would be overwritten by the time the mouse went up.  So, we set the focus
        // in OnMouseUp() to make sure it sticks
        //
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e) 
        {
            base.OnPreviewMouseUp(e);
            SynchronizeSelectionFocus(StealFocusMode.OnlyIfCurrentSelectionDoesNotHaveFocusWithin);
        }

        // When a UIElement gets focus, we try to find the parent PropertyContainer and make sure
        // it's selected
        //
        private void OnSomeoneGotFocus(object source, RoutedEventArgs e) 
        {
            Select(e.OriginalSource as DependencyObject);
        }

        // We only synchronize the IsSelected object with keyboard focus when the CategoryList itself
        // has focus.  If it doesn't we don't want to accidentally steal focus from somewhere else
        // (say the design surface when the user is just tabbing through objects on it).  However, once
        // the CategoryList itself gains focus, we do want to synchronize the IsSelected object with 
        // keyboard focus so that Tabs and keyboard navigation works correctly.
        //
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e) 
        {

            bool hasKeyboardFocus = (bool)e.NewValue;

            if (hasKeyboardFocus) 
            {
                // We just gained keyboard focus.  Make sure we [....] up the current property selection
                // with the keyboard focus, so that navigation works.
                SynchronizeSelectionFocus(StealFocusMode.OnlyIfCurrentSelectionDoesNotHaveFocusWithin);
            }

            base.OnIsKeyboardFocusWithinChanged(e);
        }

        // Helper method that ensures that the element marked as IsSelected also has focus.
        // If it cannot receive focus, we look for its closest visual child that can
        //
        private void SynchronizeSelectionFocus(StealFocusMode focusMode) 
        {
            // Is there something to select?
            if (_selection == null)
            {
                return;
            }

            // Are we even allowed to mess around with focus or does someone else on the
            // design surface have focus right now and we should just let them have it?
            if (focusMode == StealFocusMode.OnlyIfCategoryListHasFocusWithin && !this.IsKeyboardFocusWithin)
            {
                return;
            }

            if (focusMode == StealFocusMode.OnlyIfCurrentSelectionDoesNotHaveFocusWithin && _selection.IsKeyboardFocusWithin)
            {
                return;
            }

            FrameworkElement focusableElement = VisualTreeUtils.FindFocusableElement<FrameworkElement>(_selection);

            if (focusableElement != null) 
            {
                focusableElement.Focus();
            }
        }

        // Attempt to select the right thing for the specified default property name:
        //
        //  * If there is a common DefaultProperty among selected objects AND there is a CategoryEditor consuming it,
        //    select the CategoryEditor
        //
        //  * If there is a common DefaultProperty among selected objects AND there is NO CategoryEditor consuming it,
        //    select the property itself
        //
        //  * If there is no common DefaultProperty, select the first common category
        //
        //  * Otherwise fail by returning false
        //
        private bool SelectDefaultProperty(string defaultPropertyName) 
        {
            return SelectDefaultPropertyHelper(defaultPropertyName, true);
        }

        private bool SelectDefaultPropertyHelper(string defaultPropertyName, bool firstTime) 
        {
            if (string.IsNullOrEmpty(defaultPropertyName))
            {
                return false;
            }

            ModelCategoryEntry defaultPropertyCategory;
            PropertyEntry defaultProperty = FindPropertyEntry(defaultPropertyName, out defaultPropertyCategory);
            CategoryEditor defaultCategoryEditor = FindCategoryEditor(defaultProperty, defaultPropertyCategory);
            UIElement element = null;
            bool elementPendingGeneration = false;

            // Try to look up the correct UIElement to select
            //
            if (defaultCategoryEditor != null) 
            {
                element = FindCategoryEditorVisual(defaultCategoryEditor, defaultPropertyCategory, out elementPendingGeneration);
            }
            else if (defaultProperty != null) 
            {
                element = FindPropertyEntryVisual(defaultProperty, defaultPropertyCategory, out elementPendingGeneration);
            }
            else if (this.Count > 0) 
            {
                // Nothing found, so select the first selectable thing in the list
                element = PropertySelection.FindNeighborSelectionStop<UIElement>(this, SearchDirection.Next);
                elementPendingGeneration = false;
            }

            // If the UIElement was found, select it.  Otherwise, if it should exist but it wasn't generated yet,
            // wait until it is and try again.
            //
            if (element != null) 
            {

                SelectAndFocus(element, StealFocusMode.OnlyIfCategoryListHasFocusWithin);

                // Ensure that we are in Default SelectionMode because calling SelectAndFocus automatically switches us
                // to Sticky mode
                //
                ResetSelectionMode();

            }
            else if (elementPendingGeneration && firstTime) 
            {

                // Set the firstTime flag to false, to prevent any infinite loops should things go wrong
                this.Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new SelectDefaultPropertyHelperDelegate(SelectDefaultPropertyHelper),
                    defaultPropertyName,
                    false);

            }
            else if (elementPendingGeneration && !firstTime) 
            {
                elementPendingGeneration = false;
            }

            return element != null || elementPendingGeneration;
        }

        // Find the closest IsSelectable parent, select it and set focus on it.
        //
        private void SelectAndFocus(DependencyObject element, StealFocusMode focusMode) 
        {
            Select(element);
            SynchronizeSelectionFocus(focusMode);
        }

        // Find the closest IsSelectable parent and select it.  Don't mess with focus.
        //
        private void Select(DependencyObject visualSource) 
        {
            if (visualSource != null) 
            {
                FrameworkElement selection = PropertySelection.FindParentSelectionStop<FrameworkElement>(visualSource);

                if (selection != _selection) 
                {

                    // Unselect anything that was selected previously
                    if (_selection != null) 
                    {
                        PropertySelection.SetIsSelected(_selection, false);
                    }

                    _selection = selection;

                    // Select whatever we need to select now
                    if (_selection != null) 
                    {
                        PropertySelection.SetIsSelected(_selection, true);

                        // Bring the full PropertyContainer into view, if one exists
                        FrameworkElement focusableElement = VisualTreeUtils.FindFocusableElement<FrameworkElement>(_selection) ?? _selection;
                        FrameworkElement parentPropertyContainer = VisualTreeUtils.FindVisualAncestor<PropertyContainer>(focusableElement);
                        FrameworkElement bringIntoViewElement = parentPropertyContainer ?? focusableElement;

                        bringIntoViewElement.BringIntoView();

                        // As soon as the user manually selects a property, automatically switch to Sticky mode
                        _selectionMode = PropertySelectionMode.Sticky;
                    }
                }
            }
        }


        // Keyboard Navigation

        protected override void OnKeyDown(KeyEventArgs e) 
        {

            // Intercept Up, Down, Left, Right key strokes and use them to navigate around the
            // the control
            //
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down) 
            {
                if (_selection != null && !e.Handled) 
                {
                    if (object.Equals(
                        Keyboard.FocusedElement,
                        VisualTreeUtils.FindFocusableElement<FrameworkElement>(_selection))) 
                    {

                        ISelectionStop selectionStop = PropertySelection.GetSelectionStop(_selection);
                        if (selectionStop != null) 
                        {

                            if (selectionStop.IsExpandable) 
                            {
                                if ((e.Key == Key.Right && !selectionStop.IsExpanded) ||
                                    (e.Key == Key.Left && selectionStop.IsExpanded)) 
                                {

                                    selectionStop.IsExpanded = !selectionStop.IsExpanded;
                                    e.Handled = true;
                                }
                            }

                        }

                        if (!e.Handled) 
                        {

                            SearchDirection direction = e.Key == Key.Up || e.Key == Key.Left ? SearchDirection.Previous : SearchDirection.Next;
                            FrameworkElement nextStop = PropertySelection.FindNeighborSelectionStop<FrameworkElement>(_selection, direction);

                            // If need to select something, select it
                            if (nextStop != null) 
                            {
                                SelectAndFocus(nextStop, StealFocusMode.Always);
                            }

                            e.Handled = true;
                        }
                    }
                }
            }

            base.OnKeyDown(e);
        }

        // SharedPropertyValueColumnWidth State Logic

        protected override Size ArrangeOverride(Size arrangeBounds) 
        {

            // Set the content width, rather than the entire container width
            object gridLength = this.FindResource("OpenCloseColumnGridLength");
            if (gridLength != null && gridLength is GridLength)
            {
                _sharedWidthContainer.ContainerWidth = arrangeBounds.Width - ((GridLength)gridLength).Value;
            }
            else
            {
                _sharedWidthContainer.ContainerWidth = arrangeBounds.Width;
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        // Visual Lookup Helpers

        // <summary>
        // Looks for and returns the PropertyContainer used to represent the specified PropertyEntry.  Returns
        // null if not found.
        // </summary>
        // <param name="property">PropertyEntry to look up</param>
        // <param name="parentCategory">Category to examine</param>
        // <param name="pendingGeneration">Set to true if the specified property exists in a collapsed container
        // (CategoryContainer or an advanced section).  If so, the section is expanded, but the visual does not
        // exist yet and should be requested later.
        // </param>
        // <returns>PropertyContainer for the specified PropertyEntry if found, null otherwise</returns>
        internal PropertyContainer FindPropertyEntryVisual(PropertyEntry property, CategoryEntry parentCategory, out bool pendingGeneration) 
        {
            pendingGeneration = false;

            if (property == null || parentCategory == null)
            {
                return null;
            }

            if (property.MatchesFilter == false)
            {
                return null;
            }

            CiderCategoryContainer categoryContainer = this.ItemContainerGenerator.ContainerFromItem(parentCategory) as CiderCategoryContainer;
            if (categoryContainer == null)
            {
                return null;
            }

            // Expand the parent category, if it isn't already
            if (!categoryContainer.Expanded) 
            {
                categoryContainer.Expanded = true;
                pendingGeneration = true;
            }

            // Expand the parent advanced section, if any and if it isn't already
            if (property.IsAdvanced && !categoryContainer.AdvancedSectionPinned) 
            {
                categoryContainer.AdvancedSectionPinned = true;
                pendingGeneration = true;
            }

            bool pendingGenerationTemp;
            PropertyContainer propertyContainer = categoryContainer.ContainerFromProperty(property, out pendingGenerationTemp);
            pendingGeneration |= pendingGenerationTemp;

            if (propertyContainer != null)
            {
                pendingGeneration = false;
            }

            return propertyContainer;
        }

        // <summary>
        // Looks for and returns CategoryContainer for the specified CategoryEntry.  Returns null if not
        // found.
        // </summary>
        // <param name="category">CategoryEntry to look for.</param>
        // <returns>Corresponding CategoryContainer if found, null otherwise.</returns>
        internal CategoryContainer FindCategoryEntryVisual(CategoryEntry category) 
        {
            if (category == null)
            {
                return null;
            }

            CiderCategoryContainer categoryContainer = this.ItemContainerGenerator.ContainerFromItem(category) as CiderCategoryContainer;
            return categoryContainer;
        }

        // <summary>
        // Looks for and returns the UIElement used to represent the specified CategoryEditor. Returns
        // null if not found.
        // </summary>
        // <param name="editor">CategoryEditor to look for.</param>
        // <param name="category">Category to look in.</param>
        // <param name="pendingGeneration">Set to true if the specified editor exists in a collapsed container
        // (CategoryContainer or an advanced section).  If so, the section is expanded, but the visual does not
        // exist yet and should be requested later.</param>
        // <returns>UIElement for the specified CategoryEditor if found, null otherwise</returns>
        internal UIElement FindCategoryEditorVisual(CategoryEditor editor, ModelCategoryEntry category, out bool pendingGeneration) 
        {
            pendingGeneration = false;

            if (editor == null || category == null)
            {
                return null;
            }

            UIElement editorVisual = null;

            CiderCategoryContainer categoryContainer = this.ItemContainerGenerator.ContainerFromItem(category) as CiderCategoryContainer;
            if (categoryContainer == null)
            {
                return null;
            }

            // Expand the parent category, if it isn't already
            if (!categoryContainer.Expanded) 
            {
                categoryContainer.Expanded = true;
                pendingGeneration = true;
            }

            // Expand the parent advanced section, if any and if it isn't already
            if (!categoryContainer.AdvancedSectionPinned && ExtensibilityAccessor.GetIsAdvanced(editor)) 
            {
                categoryContainer.AdvancedSectionPinned = true;
                pendingGeneration = true;
            }

            bool pendingGenerationTemp;
            editorVisual = categoryContainer.ContainerFromEditor(editor, out pendingGenerationTemp);
            pendingGeneration |= pendingGenerationTemp;

            if (editorVisual != null)
            {
                pendingGeneration = false;
            }

            return editorVisual;
        }

        // Logical Lookup Helpers

        // <summary>
        // Looks for a CategoryEntry contained in this list with the given name
        // </summary>
        // <param name="name">Name to look for</param>
        // <returns>CategoryEntry with the specified name if found, null otherwise</returns>
        internal CategoryEntry FindCategory(string name) 
        {
            if (name == null)
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }

            foreach (CategoryEntry category in this.Items) 
            {
                if (category.CategoryName.Equals(name))
                {
                    return category;
                }
            }

            return null;
        }

        // <summary>
        // Looks for the PropertyEntry and its parent CategoryEntry for the specified property
        // </summary>
        // <param name="propertyName">Property to look for</param>
        // <param name="parentCategory">Parent CategoryEntry of the given property</param>
        // <returns>Corresponding PropertyEntry if found, null otherwise.</returns>
        internal PropertyEntry FindPropertyEntry(string propertyName, out ModelCategoryEntry parentCategory) 
        {
            parentCategory = null;
            if (propertyName == null)
            {
                return null;
            }

            foreach (ModelCategoryEntry category in this.Items) 
            {
                PropertyEntry property = category[propertyName];
                if (property != null) 
                {
                    parentCategory = category;
                    return property;
                }
            }

            return null;
        }

        // <summary>
        // Looks for the CategoryEditor of the specified type and returns it if found.
        // </summary>
        // <param name="editorTypeName">Type name of the editor to look for.</param>
        // <param name="category">CategoryEntry that the editor belongs to, if found.</param>
        // <returns>CategoryEditor instance of the given type name if found, null otherwise.</returns>
        internal CategoryEditor FindCategoryEditor(string editorTypeName, out ModelCategoryEntry category) 
        {
            category = null;
            if (string.IsNullOrEmpty(editorTypeName) || this.Items == null)
            {
                return null;
            }

            foreach (ModelCategoryEntry currentCategory in this.Items) 
            {
                foreach (CategoryEditor editor in currentCategory.CategoryEditors) 
                {
                    if (string.Equals(editorTypeName, editor.GetType().Name)) 
                    {
                        category = currentCategory;
                        return editor;
                    }
                }
            }

            return null;
        }

        // Find the first CategoryEditor that consumes the specified property in the specified category.
        //
        private static CategoryEditor FindCategoryEditor(PropertyEntry property, ModelCategoryEntry parentCategory) 
        {
            if (property == null || parentCategory == null)
            {
                return null;
            }

            foreach (CategoryEditor editor in parentCategory.CategoryEditors) 
            {
                if (editor.ConsumesProperty(property))
                {
                    return editor;
                }
            }

            return null;
        }


        // IEnumerable<CategoryBase> Members

        public IEnumerator<CategoryBase> GetEnumerator() 
        {
            foreach (CategoryBase categoryBase in this.Items)
            {
                yield return categoryBase;
            }
        }

        // IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
        {
            return this.GetEnumerator();
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

        // Delegate for SelectDefaultPropertyHelper method so that we can invoke it on Render priority if the
        // appropriate UIElements have not been generated yet
        //
        private delegate bool SelectDefaultPropertyHelperDelegate(string defaultPropertyName, bool firstTime);

        // Enum that defines how and when we steal keyboard focus from the rest of the application
        // when the property selection changes
        //
        private enum StealFocusMode 
        {
            Always,
            OnlyIfCategoryListHasFocusWithin,
            OnlyIfCurrentSelectionDoesNotHaveFocusWithin
        }

        // Manages state specific to this CategoryList instance
        private class CategoryListStateContainer : IStateContainer 
        {

            private CategoryList _parent;
            private IStateContainer _containers;

            public CategoryListStateContainer(CategoryList parent) 
            {
                if (parent == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("parent");
                }
                _parent = parent;
            }

            private IStateContainer Containers 
            {
                get {
                    if (_containers == null)
                    {
                        _containers = new AggregateStateContainer(
                            new FilterStringStateContainer(_parent),
                            new PropertyValueWidthStateContainer(_parent));
                    }

                    return _containers;
                }
            }

            public object RetrieveState() 
            {
                return Containers.RetrieveState();
            }

            public void RestoreState(object state) 
            {
                Containers.RestoreState(state);
            }

            // FilterStringStateContainer

            // StateContainer responsible for the FilterString
            //
            private class FilterStringStateContainer : IStateContainer 
            {
                private CategoryList _parent;

                public FilterStringStateContainer(CategoryList parent) 
                {
                    if (parent == null) 
                    {
                        throw FxTrace.Exception.ArgumentNull("parent");
                    }
                    _parent = parent;
                }

                public object RetrieveState() 
                {
                    return _parent.FilterString;
                }

                public void RestoreState(object state) 
                {
                    string filterString = state as string;
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        _parent.FilterString = filterString;
                    }
                }
            }

            // PropertyValueWidthStateContainer

            // StateContainer responsible for the width of the property value column
            //
            private class PropertyValueWidthStateContainer : IStateContainer 
            {
                private CategoryList _parent;

                public PropertyValueWidthStateContainer(CategoryList parent) 
                {
                    if (parent == null) 
                    {
                        throw FxTrace.Exception.ArgumentNull("parent");
                    }
                    _parent = parent;
                }

                public object RetrieveState() 
                {
                    return _parent._sharedWidthContainer.ValueColumnPercentage.ToString(CultureInfo.InvariantCulture);
                }

                public void RestoreState(object state) 
                {
                    string stateString = state as string;
                    if (stateString == null)
                    {
                        return;
                    }

                    double percentage;
                    if (!double.TryParse(stateString, NumberStyles.Float, CultureInfo.InvariantCulture, out percentage)) 
                    {
                        Debug.Fail("Invalid PI state: " + stateString);
                        return;
                    }

                    if (percentage >= 0 && percentage <= 1) 
                    {
                        _parent._sharedWidthContainer.ValueColumnPercentage = percentage;
                    }
                    else 
                    {
                        Debug.Fail("Invalid percentage width stored in PI state: " + percentage.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

       }

    }
}
