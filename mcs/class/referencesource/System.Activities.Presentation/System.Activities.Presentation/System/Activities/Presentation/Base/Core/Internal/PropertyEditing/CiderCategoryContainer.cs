//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;

    using System.Activities.Presentation.PropertyEditing;
    using Blend = System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    using System.Activities.Presentation.Internal.PropertyEditing.Automation;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // HACK: wrapper around Sparkle's CategoryContainer that doesn't come with
    // any initial set of visuals, since we re-brand the control ourselves.
    // Sparkle doesn't plan to change this code before they ship, so we need
    // this work-around.
    // </summary>
    internal class CiderCategoryContainer : Blend.CategoryContainer 
    {

        private static readonly DependencyPropertyKey IsEmptyPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsEmpty",
            typeof(bool),
            typeof(CiderCategoryContainer),
            new PropertyMetadata(true, null, new CoerceValueCallback(CiderCategoryContainer.CoerceIsEmpty)));

        // <summary>
        // IsEmpty property indicates whether this CategoryContainer contains any editors or unconsumed properties.
        // If the value is true, UI for this container will hide itself
        // </summary>
        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        // <summary>
        // Property indicating whether the header for the category should be rendered or not
        // </summary>
        public static DependencyProperty ShowCategoryHeaderProperty = DependencyProperty.Register(
            "ShowCategoryHeader",
            typeof(bool),
            typeof(CiderCategoryContainer),
            new PropertyMetadata(true));

        // Exposed ItemsControls that know how to convert objects into their
        // visual counterparts
        private ItemsControl _basicCategoryEditorsContainer;
        private ItemsControl _advancedCategoryEditorsContainer;
        private ItemsControl _basicPropertyContainersContainer;
        private ItemsControl _advancedPropertyContainersContainer;
        private bool _UIHooksInitialized;

        // Keyboard navigation helpers
        private CategorySelectionStop _basicCategorySelectionStop;
        private CategorySelectionStop _advancedCategorySelectionStop;

        public CiderCategoryContainer()
            :
            base(false) 
        {

            // Note: this logic, along with the IsEmpty DP should be pushed into the base class
            this.BasicCategoryEditors.CollectionChanged += new NotifyCollectionChangedEventHandler(OnContentChanged);
            this.AdvancedCategoryEditors.CollectionChanged += new NotifyCollectionChangedEventHandler(OnContentChanged);
            this.UnconsumedBasicProperties.CollectionChanged += new NotifyCollectionChangedEventHandler(OnContentChanged);
            this.UnconsumedAdvancedProperties.CollectionChanged += new NotifyCollectionChangedEventHandler(OnContentChanged);
        }

        // IsEmpty ReadOnly DP

        // Events that would have ideally been baked in the base class
        public event EventHandler ExpandedChanged;
        public event EventHandler AdvancedSectionPinnedChanged;

        // <summary>
        // Gets the value for IsEmpty DP
        // </summary>
        public bool IsEmpty 
        {
            get { return (bool)this.GetValue(IsEmptyProperty); }
        }

        // <summary>
        // Gets or set the value of ShowCategoryHeaderProperty
        // </summary>
        public bool ShowCategoryHeader 
        {
            get { return (bool)this.GetValue(ShowCategoryHeaderProperty); }
            set { this.SetValue(ShowCategoryHeaderProperty, value); }
        }

        // <summary>
        // Wrapper around the ExpandedProperty that keyboard navigation understands.  Called from Xaml.
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ISelectionStop BasicCategorySelectionStop 
        {
            get {
                if (_basicCategorySelectionStop == null)
                {
                    _basicCategorySelectionStop = new CategorySelectionStop(this, false);
                }

                return _basicCategorySelectionStop;
            }
        }

        // <summary>
        // Wrapper around the AdvancedSectionPinnedProperty that keyboard navigation understands
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ISelectionStop AdvancedCategorySelectionStop 
        {
            get {
                if (_advancedCategorySelectionStop == null)
                {
                    _advancedCategorySelectionStop = new CategorySelectionStop(this, true);
                }

                return _advancedCategorySelectionStop;
            }
        }

        private ItemsControl BasicCategoryEditorsContainer 
        {
            get {
                //
                // If called before the UI itself is rendered, this property
                // will return null, which needs to be handled by the caller
                //
                EnsureUIHooksInitialized();
                return _basicCategoryEditorsContainer;
            }
        }

        private ItemsControl AdvancedCategoryEditorsContainer 
        {
            get {
                //
                // If called before the UI itself is rendered, this property
                // will return null, which needs to be handled by the caller
                //
                EnsureUIHooksInitialized();
                return _advancedCategoryEditorsContainer;
            }
        }

        private ItemsControl BasicPropertyContainersContainer 
        {
            get {
                //
                // If called before the UI itself is rendered, this property
                // will return null, which needs to be handled by the caller
                //
                EnsureUIHooksInitialized();
                return _basicPropertyContainersContainer;
            }
        }

        private ItemsControl AdvancedPropertyContainersContainer 
        {
            get {
                //
                // If called before the UI itself is rendered, this property
                // will return null, which needs to be handled by the caller
                //
                EnsureUIHooksInitialized();
                return _advancedPropertyContainersContainer;
            }
        }

        private static object CoerceIsEmpty(DependencyObject obj, object value) 
        {
            CiderCategoryContainer theThis = obj as CiderCategoryContainer;

            if (theThis == null)
            {
                return value;
            }

            return theThis.BasicCategoryEditors.Count == 0 &&
                theThis.UnconsumedBasicProperties.Count == 0 &&
                theThis.AdvancedCategoryEditors.Count == 0 &&
                theThis.UnconsumedAdvancedProperties.Count == 0;
        }

        private void OnContentChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            this.CoerceValue(IsEmptyProperty);
        }

        // ShowCategoryHeader DP

        // In Cider and unlike in Blend, we expose all properties (browsable and non-browsable) through the
        // property editing object model.  Hence, we need to make sure that the UI representing it (CategoryContainer)
        // does the filtering instead.  This is by design and, for consistency, it should be pushed into Blend as well
        protected override void AddProperty(PropertyEntry property, ObservableCollection<PropertyEntry> unconsumedProperties, ObservableCollection<PropertyEntry> referenceOrder, ObservableCollection<CategoryEditor> categoryEditors) 
        {

            // Is this property browsable?
            ModelPropertyEntry modelPropertyEntry = property as ModelPropertyEntry;
            if (modelPropertyEntry != null && !modelPropertyEntry.IsBrowsable)
            {
                return;
            }

            // Yes, so we can safely add it to the list
            base.AddProperty(property, unconsumedProperties, referenceOrder, categoryEditors);
        }

        // <summary>
        // Attempts to look up the corresponding PropertyContainer to the specified PropertyEntry
        // </summary>
        // <param name="property">Property to look up</param>
        // <param name="pendingGeneration">Set to true if the specified property may have a container
        // but the visual does not exist yet and should be requested later.</param>
        // <returns>Corresponding PropertyContainer, if found, null otherwise</returns>
        public PropertyContainer ContainerFromProperty(PropertyEntry property, out bool pendingGeneration) 
        {
            pendingGeneration = false;
            if (property == null)
            {
                return null;
            }

            if (this.BasicPropertyContainersContainer != null) 
            {
                PropertyContainer propertyContainer = this.BasicPropertyContainersContainer.ItemContainerGenerator.ContainerFromItem(property) as PropertyContainer;
                if (propertyContainer != null)
                {
                    return propertyContainer;
                }
            }

            if (this.AdvancedPropertyContainersContainer != null) 
            {
                PropertyContainer propertyContainer = this.AdvancedPropertyContainersContainer.ItemContainerGenerator.ContainerFromItem(property) as PropertyContainer;
                if (propertyContainer != null)
                {
                    return propertyContainer;
                }
            }

            if (!_UIHooksInitialized)
            {
                pendingGeneration = true;
            }

            return null;
        }

        // <summary>
        // Attempts to look up the corresponding UI representation of the specified CategoryEditor
        // </summary>
        // <param name="editor">Editor to look up</param>
        // <param name="pendingGeneration">Set to true if the specified editor may have a container
        // but the visual does not exist yet and should be requested later.</param>
        // <returns>UI representation of the specified CategoryEditor, if found, null otherwise.</returns>
        public UIElement ContainerFromEditor(CategoryEditor editor, out bool pendingGeneration) 
        {
            pendingGeneration = false;
            if (editor == null)
            {
                return null;
            }

            if (this.BasicCategoryEditorsContainer != null) 
            {
                UIElement categoryEditor = this.BasicCategoryEditorsContainer.ItemContainerGenerator.ContainerFromItem(editor) as UIElement;
                if (categoryEditor != null)
                {
                    return categoryEditor;
                }
            }

            if (this.AdvancedCategoryEditorsContainer != null) 
            {
                UIElement categoryEditor = this.AdvancedCategoryEditorsContainer.ItemContainerGenerator.ContainerFromItem(editor) as UIElement;
                if (categoryEditor != null)
                {
                    return categoryEditor;
                }
            }

            if (!_UIHooksInitialized)
            {
                pendingGeneration = true;
            }

            return null;
        }

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return CategoryContainerAutomationPeer.CreateStandAloneAutomationPeer(this);
        }

        // This method fires ExpandedChanged and AdvancedSectionPinnedChanged events.  Ideally we would include
        // these events in Blend.CategoryContainer, but the assembly containing that class has already been locked
        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e) 
        {
            base.OnPropertyChanged(e);

            if (e.Property == ExpandedProperty &&
                ExpandedChanged != null) 
            {
                ExpandedChanged(this, EventArgs.Empty);
            }

            if (e.Property == AdvancedSectionPinnedProperty &&
                AdvancedSectionPinnedChanged != null) 
            {
                AdvancedSectionPinnedChanged(this, EventArgs.Empty);
            }
        }

        // Returns true if the hooks should have already been generated
        // false otherwise.
        //
        private bool EnsureUIHooksInitialized() 
        {
            if (_UIHooksInitialized)
            {
                return true;
            }

            _basicCategoryEditorsContainer = VisualTreeUtils.GetNamedChild<ItemsControl>(this, "PART_BasicCategoryEditors");
            _advancedCategoryEditorsContainer = VisualTreeUtils.GetNamedChild<ItemsControl>(this, "PART_AdvancedCategoryEditors");
            _basicPropertyContainersContainer = VisualTreeUtils.GetNamedChild<ItemsControl>(this, "PART_BasicPropertyList");
            _advancedPropertyContainersContainer = VisualTreeUtils.GetNamedChild<ItemsControl>(this, "PART_AdvancedPropertyList");

            if (_basicCategoryEditorsContainer == null &&
                _advancedCategoryEditorsContainer == null &&
                _basicPropertyContainersContainer == null &&
                _advancedPropertyContainersContainer == null) 
            {

                return false;
            }

            if (_basicCategoryEditorsContainer == null ||
                _advancedCategoryEditorsContainer == null ||
                _basicPropertyContainersContainer == null ||
                _advancedPropertyContainersContainer == null) 
            {

                Debug.Fail("UI for CategoryContainer changed.  Need to update CiderCategoryContainer logic.");
            }

            _UIHooksInitialized = true;
            return true;
        }
    }
}
