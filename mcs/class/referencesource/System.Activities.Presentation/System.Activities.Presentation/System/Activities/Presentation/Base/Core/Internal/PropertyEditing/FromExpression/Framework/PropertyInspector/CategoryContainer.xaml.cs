
// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    //Cider change [CLSCompliant(false)]
    internal partial class CategoryContainer : ContentControl
    {

        // This will be set by the property inspector if the category is hosted in a popup.
        public static readonly DependencyProperty PopupHostProperty = DependencyProperty.RegisterAttached(
            "PopupHost", typeof(Popup), typeof(CategoryContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnPopupHostChanged));

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category",
            typeof(CategoryBase),
            typeof(CategoryContainer),
            new PropertyMetadata(
            (CategoryEntry)null,
            new PropertyChangedCallback(OnCategoryPropertyChanged)));

        public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register("Expanded", typeof(bool), typeof(CategoryContainer), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnExpandedChanged)));

        public static readonly DependencyProperty AdvancedSectionPinnedProperty = DependencyProperty.Register("AdvancedSectionPinned", typeof(bool), typeof(CategoryContainer), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty BasicPropertyMatchesFilterProperty = DependencyProperty.Register("BasicPropertyMatchesFilter", typeof(bool), typeof(CategoryContainer), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty AdvancedPropertyMatchesFilterProperty = DependencyProperty.Register("AdvancedPropertyMatchesFilter", typeof(bool), typeof(CategoryContainer), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(CategoryContainer.OnAdvancedPropertyMatchesFilterChanged)));

        public static readonly DependencyProperty ShowAdvancedHeaderProperty = DependencyProperty.Register("ShowAdvancedHeader", typeof(bool), typeof(CategoryContainer), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, null, new CoerceValueCallback(CategoryContainer.CoerceShowAdvancedHeader)));

        public static readonly DependencyProperty OwningCategoryContainerProperty = DependencyProperty.RegisterAttached("OwningCategoryContainer", typeof(CategoryContainer), typeof(CategoryContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));



        // Data for managing expanded state based on a filter.
        private FilterState filterIsEmpty = FilterState.Unknown;

        // garylins 11/15/2006 - This variable has been added to fix 




        private bool haveCachedExpanded = false;
        private bool wasAdvancedPinnedBeforeFilter = false;
        private bool wasExpandedBeforeFilter = true;

        // used for managing category editors and overflow properties.
        private ObservableCollection<CategoryEditor> basicCategoryEditors = new ObservableCollection<CategoryEditor>();
        private ObservableCollection<CategoryEditor> advancedCategoryEditors = new ObservableCollection<CategoryEditor>();
        private ObservableCollection<PropertyEntry> unconsumedBasicProperties = new ObservableCollection<PropertyEntry>();
        private ObservableCollection<PropertyEntry> unconsumedAdvancedProperties = new ObservableCollection<PropertyEntry>();



        public CategoryContainer() : this(true)
        {
        }

        public CategoryContainer(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }

            SetOwningCategoryContainer(this, this);

            Binding basicMatchesFilterBinding = new Binding("Category.BasicPropertyMatchesFilter");
            basicMatchesFilterBinding.Source = this;
            basicMatchesFilterBinding.Mode = BindingMode.OneWay;
            this.SetBinding(CategoryContainer.BasicPropertyMatchesFilterProperty, basicMatchesFilterBinding);

            Binding advancedMatchesFilterBinding = new Binding("Category.AdvancedPropertyMatchesFilter");
            advancedMatchesFilterBinding.Source = this;
            advancedMatchesFilterBinding.Mode = BindingMode.OneWay;
            this.SetBinding(CategoryContainer.AdvancedPropertyMatchesFilterProperty, advancedMatchesFilterBinding);
        }

        public ObservableCollection<CategoryEditor> BasicCategoryEditors
        {
            get { return this.basicCategoryEditors; }
        }

        public ObservableCollection<CategoryEditor> AdvancedCategoryEditors
        {
            get { return this.advancedCategoryEditors; }
        }

        public ObservableCollection<PropertyEntry> UnconsumedBasicProperties
        {
            get { return this.unconsumedBasicProperties; }
        }

        public ObservableCollection<PropertyEntry> UnconsumedAdvancedProperties
        {
            get { return this.unconsumedAdvancedProperties; }
        }

        public CategoryBase Category
        {
            get { return (CategoryBase)this.GetValue(CategoryContainer.CategoryProperty); }
            set { this.SetValue(CategoryContainer.CategoryProperty, value); }
        }

        public bool Expanded
        {
            get { return (bool)this.GetValue(CategoryContainer.ExpandedProperty); }
            set { this.SetValue(CategoryContainer.ExpandedProperty, value); }
        }

        public bool AdvancedSectionPinned
        {
            get { return (bool)this.GetValue(CategoryContainer.AdvancedSectionPinnedProperty); }
            set { this.SetValue(CategoryContainer.AdvancedSectionPinnedProperty, value); }
        }

        public bool BasicPropertyMatchesFilter
        {
            get { return (bool)this.GetValue(CategoryContainer.BasicPropertyMatchesFilterProperty); }
            set { this.SetValue(CategoryContainer.BasicPropertyMatchesFilterProperty, value); }
        }

        public bool AdvancedPropertyMatchesFilter
        {
            get { return (bool)this.GetValue(CategoryContainer.AdvancedPropertyMatchesFilterProperty); }
            set { this.SetValue(CategoryContainer.AdvancedPropertyMatchesFilterProperty, value); }
        }

        public bool ShowAdvancedHeader
        {
            get { return (bool)this.GetValue(CategoryContainer.ShowAdvancedHeaderProperty); }
            set { this.SetValue(CategoryContainer.ShowAdvancedHeaderProperty, value); }
        }


        // <summary>
        // Writes the attached property OwningCategoryContainer to the given element.
        // </summary>
        // <param name="d">The element to which to write the attached property.</param>
        // <param name="value">The property value to set</param>
        public static void SetOwningCategoryContainer(DependencyObject dependencyObject, CategoryContainer value)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            dependencyObject.SetValue(CategoryContainer.OwningCategoryContainerProperty, value);
        }

        // <summary>
        // Reads the attached property OwningCategoryContainer from the given element.
        // </summary>
        // <param name="d">The element from which to read the attached property.</param>
        // <returns>The property's value.</returns>
        public static CategoryContainer GetOwningCategoryContainer(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("dependencyObject");
            }
            return (CategoryContainer)dependencyObject.GetValue(CategoryContainer.OwningCategoryContainerProperty);
        }

        public static Popup GetPopupHost(DependencyObject target)
        {
            return (Popup)target.GetValue(CategoryContainer.PopupHostProperty);
        }

        public static void SetPopupHost(DependencyObject target, Popup value)
        {
            target.SetValue(CategoryContainer.PopupHostProperty, value);
        }

        private static void OnPopupHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CategoryContainer categoryEditor = d as CategoryContainer;
            if (categoryEditor != null)
            {
                // If we are hosted in a popup, do not show the advanced category expander, and pin the advanced section.
                if (e.NewValue != null)
                {
                    categoryEditor.AdvancedSectionPinned = true;
                }
                else
                {
                    categoryEditor.AdvancedSectionPinned = false;
                }
            }
        }

        private static void OnCategoryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CategoryContainer theThis = (CategoryContainer)d;

            if (e.NewValue != null)
            {
                CategoryBase category = (CategoryBase)e.NewValue;
                theThis.SetValue(
                    System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Diagnostics.Automation.AutomationElement.IdProperty,
                    category.CategoryName + "Category");


                CategoryBase oldCategory = (CategoryBase)e.OldValue;
                if (oldCategory != null)
                {
                    oldCategory.FilterApplied -= new EventHandler<PropertyFilterAppliedEventArgs>(theThis.OnFilterApplied);

                    category.CategoryEditors.CollectionChanged -= theThis.CategoryEditors_CollectionChanged;
                    theThis.basicCategoryEditors.Clear();
                    theThis.advancedCategoryEditors.Clear();

                    category.BasicProperties.CollectionChanged -= theThis.BasicProperties_CollectionChanged;
                    category.AdvancedProperties.CollectionChanged -= theThis.AdvancedProperties_CollectionChanged;
                    theThis.unconsumedBasicProperties.Clear();
                    theThis.unconsumedAdvancedProperties.Clear();
                }
                if (category != null)
                {
                    category.FilterApplied += new EventHandler<PropertyFilterAppliedEventArgs>(theThis.OnFilterApplied);

                    theThis.AddCategoryEditors(category.CategoryEditors);
                    category.CategoryEditors.CollectionChanged += theThis.CategoryEditors_CollectionChanged;

                    foreach (PropertyEntry property in category.BasicProperties)
                    {
                        theThis.AddProperty(property, theThis.unconsumedBasicProperties, theThis.Category.BasicProperties, theThis.basicCategoryEditors);
                    }
                    foreach (PropertyEntry property in category.AdvancedProperties)
                    {
                        theThis.AddProperty(property, theThis.unconsumedAdvancedProperties, theThis.Category.AdvancedProperties, theThis.advancedCategoryEditors);
                    }
                    category.BasicProperties.CollectionChanged += theThis.BasicProperties_CollectionChanged;
                    category.AdvancedProperties.CollectionChanged += theThis.AdvancedProperties_CollectionChanged;
                }
                theThis.CoerceValue(CategoryContainer.ShowAdvancedHeaderProperty);
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################

        // This method used to be non-virtual, private
        protected virtual void AddProperty(PropertyEntry property, ObservableCollection<PropertyEntry> unconsumedProperties, ObservableCollection<PropertyEntry> referenceOrder, ObservableCollection<CategoryEditor> categoryEditors)
        {

            // ###################################################
            // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
            // ###################################################

            bool consumed = false;

            foreach (CategoryEditor categoryEditor in categoryEditors)
            {
                if (categoryEditor.ConsumesProperty(property))
                {
                    consumed = true;
                }
            }
            if (!consumed)
            {
                // We need to insert this property in the correct location.  Reference order is sorted and contains all properties in the unconsumed properties collection.
                Fx.Assert(referenceOrder.Contains(property), "Reference order should contain the property to be added.");
#if DEBUG
                foreach (PropertyEntry unconsumedProperty in unconsumedProperties)
                {
                    Fx.Assert(referenceOrder.Contains(unconsumedProperty), "Reference order should contain all unconsumed properties.");
                }
#endif

                // We'll walk both collections, and advance the insertion index whenever we see an unconsumed property come ahead of the target in the reference order.
                int referenceIndex = 0;
                int insertionIndex = 0;
                while (referenceOrder[referenceIndex] != property && insertionIndex < unconsumedProperties.Count)
                {
                    if (unconsumedProperties[insertionIndex] == referenceOrder[referenceIndex])
                    {
                        insertionIndex++;
                    }
                    referenceIndex++;
                }
                unconsumedProperties.Insert(insertionIndex, property);
            }
        }

        private void OnFilterApplied(object source, PropertyFilterAppliedEventArgs args)
        {
            // If the filter just switched between empty and non-empty
            if (args.Filter.IsEmpty && this.filterIsEmpty != FilterState.Empty || !args.Filter.IsEmpty && this.filterIsEmpty != FilterState.NotEmpty)
            {
                // If the filter is now empty
                if (args.Filter.IsEmpty)
                {
                    if (this.haveCachedExpanded)
                    {
                        // Set Pinned and Expanded to what they were before the filter
                        this.Expanded = this.wasExpandedBeforeFilter;
                        this.AdvancedSectionPinned = this.wasAdvancedPinnedBeforeFilter;
                    }
                }
                else
                {
                    // Cache the Pinned and Expanded state
                    this.haveCachedExpanded = true;
                    this.wasExpandedBeforeFilter = this.Expanded;
                    this.wasAdvancedPinnedBeforeFilter = this.AdvancedSectionPinned;
                }
            }

            if (!args.Filter.IsEmpty)
            {
                this.Expanded = this.BasicPropertyMatchesFilter || this.AdvancedPropertyMatchesFilter;
                this.AdvancedSectionPinned = this.AdvancedPropertyMatchesFilter;
            }

            this.filterIsEmpty = args.Filter.IsEmpty ? FilterState.Empty : FilterState.NotEmpty;
        }


        private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (CategoryContainerCommands.UpdateCategoryExpansionState.CanExecute(null, d as IInputElement))
            {
                CategoryContainerCommands.UpdateCategoryExpansionState.Execute(null, d as IInputElement);
            }
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            // dismiss the popup
            Popup popupHost = CategoryContainer.GetPopupHost(this);
            Fx.Assert(popupHost != null, "popupHost should not be null");
            if (popupHost != null)
            {
                popupHost.IsOpen = false;
            }
        }

        private static void OnAdvancedPropertyMatchesFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CategoryContainer editor = d as CategoryContainer;
            if (editor != null)
            {
                editor.CoerceValue(CategoryContainer.ShowAdvancedHeaderProperty);
            }
        }

        private static object CoerceShowAdvancedHeader(DependencyObject d, object value)
        {
            CategoryContainer editor = d as CategoryContainer;
            if (editor != null)
            {

                // ###################################################
                // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
                // ###################################################

                // Bugfix: this condition used to reference editor.Category.AdvancedProperties.Count instead of
                // editor.unconsumedAdvancedProperties, which is a 
                if ((editor.unconsumedAdvancedProperties.Count <= 0 && editor.advancedCategoryEditors.Count == 0) || !editor.AdvancedPropertyMatchesFilter)

                // ###################################################
                // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
                // ###################################################

                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += new RoutedEventHandler(CategoryContainer_Loaded);
        }

        private void CategoryContainer_Loaded(object sender, RoutedEventArgs e)
        {
            IPropertyInspector owningPI = PropertyInspectorHelper.GetOwningPropertyInspectorModel(this);
            if (owningPI != null)
            {
                if (CategoryContainer.GetPopupHost(this) == null)
                {
                    this.Expanded = owningPI.IsCategoryExpanded(this.Category.CategoryName);
                }
            }
        }


        // Category editor management
        private void AdvancedProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (PropertyEntry property in e.NewItems)
                    {
                        this.AddProperty(property, this.unconsumedAdvancedProperties, this.Category.AdvancedProperties, this.advancedCategoryEditors);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (PropertyEntry property in e.OldItems)
                    {
                        this.unconsumedAdvancedProperties.Remove(property);
                    }
                    break;

                default:
                    Debug.Fail("BasicProperties should not change in a way other than an add or a remove.");
                    break;
            }

            this.CoerceValue(CategoryContainer.ShowAdvancedHeaderProperty);
        }

        private void BasicProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (PropertyEntry property in e.NewItems)
                    {
                        this.AddProperty(property, this.unconsumedBasicProperties, this.Category.BasicProperties, this.basicCategoryEditors);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (PropertyEntry property in e.OldItems)
                    {
                        this.unconsumedBasicProperties.Remove(property);
                    }
                    break;

                default:
                    Debug.Fail("BasicProperties should not change in a way other than an add or a remove.");
                    break;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        private bool IsAdvanced(CategoryEditor editor)
        {
            AttributeCollection attributes = null;
            try
            {
                attributes = TypeDescriptor.GetAttributes(editor);
            }
            catch (Exception)
            {
            }
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    EditorBrowsableAttribute browsable = attribute as EditorBrowsableAttribute;
                    if (browsable != null)
                    {
                        return browsable.State == EditorBrowsableState.Advanced;
                    }
                }
            }
            return false;
        }

        private void AddCategoryEditors(IList editors)
        {
            foreach (CategoryEditor editor in editors)
            {
                if (this.IsAdvanced(editor))
                {
                    this.advancedCategoryEditors.Add(editor);
                    this.UpdateUnconsumedProperties(editor, this.unconsumedAdvancedProperties);
                }
                else
                {
                    this.basicCategoryEditors.Add(editor);
                    this.UpdateUnconsumedProperties(editor, this.unconsumedBasicProperties);
                }
            }
        }

        private void UpdateUnconsumedProperties(CategoryEditor newEditor, ObservableCollection<PropertyEntry> unconsumedProperties)
        {
            for (int i = unconsumedProperties.Count - 1; i >= 0; i--)
            {
                if (newEditor.ConsumesProperty(unconsumedProperties[i]))
                {
                    unconsumedProperties.RemoveAt(i);
                }
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################

        // This change is a result of 


        // Original code:

        //private void RemoveCategoryEditors(IList editors) {
        //    foreach (CategoryEditor editor in editors) {
        //        if (this.IsAdvanced(editor)) {
        //            this.advancedCategoryEditors.Remove(editor);
        //        }
        //        else {
        //            this.basicCategoryEditors.Remove(editor);
        //        }
        //    }
        //}

        // Updated code:

        private void RemoveCategoryEditors(IList editors)
        {
            bool refreshBasicProperties = false;
            bool refreshAdvancedProperties = false;

            foreach (CategoryEditor editor in editors)
            {
                if (this.IsAdvanced(editor))
                {
                    this.advancedCategoryEditors.Remove(editor);
                    refreshAdvancedProperties = true;
                }
                else
                {
                    this.basicCategoryEditors.Remove(editor);
                    refreshBasicProperties = true;
                }
            }

            if (this.Category != null)
            {
                if (refreshBasicProperties)
                {
                    RefreshConsumedProperties(this.unconsumedBasicProperties, this.Category.BasicProperties, this.basicCategoryEditors);
                }

                if (refreshAdvancedProperties)
                {
                    RefreshConsumedProperties(this.unconsumedAdvancedProperties, this.Category.AdvancedProperties, this.advancedCategoryEditors);
                }
            }
        }

        private void RefreshConsumedProperties(ObservableCollection<PropertyEntry> unconsumedProperties, ObservableCollection<PropertyEntry> allProperties, ObservableCollection<CategoryEditor> categoryEditors)
        {
            if (allProperties == null || unconsumedProperties == null || unconsumedProperties.Count == allProperties.Count)
            {
                return;
            }

            foreach (PropertyEntry property in allProperties)
            {
                if (!unconsumedProperties.Contains(property))
                {
                    // The following method will only add the specified property to the unconsumed
                    // list if it isn't already consumed by some existing category editor.
                    AddProperty(property, unconsumedProperties, allProperties, categoryEditors);
                }
            }
        }

        // #################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // #################################################

        private void CategoryEditors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // we need to add/remove category editors
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    this.basicCategoryEditors.Clear();
                    this.advancedCategoryEditors.Clear();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    this.AddCategoryEditors(e.NewItems);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    this.RemoveCategoryEditors(e.OldItems);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    this.RemoveCategoryEditors(e.OldItems);
                    this.AddCategoryEditors(e.NewItems);
                    break;
            }
        }

        // Constructors
        private enum FilterState
        {
            Unknown,
            Empty,
            NotEmpty
        }
        // Fields
    }
}

