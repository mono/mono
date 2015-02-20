//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;

    using System.Runtime;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    using System.Activities.Presentation.Internal.PropertyEditing.Automation;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using ModelUtilities = System.Activities.Presentation.Internal.PropertyEditing.Model.ModelUtilities;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // We use the SubPropertyEditor to replace the entire property row
    // when the property exposes its subproperties.  This control is _not_
    // just used within the value-editing portion of a property row.
    // We cheat because we can.
    // </summary>
    internal class SubPropertyEditor : Control, INotifyPropertyChanged, ISelectionStop 
    {

        // <summary>
        // PropertyEntry is used to store the currently displayed PropertyEntry
        // </summary>
        public static readonly DependencyProperty PropertyEntryProperty = DependencyProperty.Register(
            "PropertyEntry",
            typeof(PropertyEntry),
            typeof(SubPropertyEditor),
            new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyEntryChanged)));

        // <summary>
        // Boolean used to indicate whether the sub-properties are being shown or not.  As an optimization,
        // we don't actually expose the PropertyValue's sub-properties through SelectiveSubProperties until
        // the sub-property expando-pane has been open at least once.
        // </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(SubPropertyEditor),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsExpandedChanged)));

        // <summary>
        // Exposes the currently selected QuickType in the QuickType drop-down.  Essentially,
        // the value of this DP is plumbed through to reflect the value of _quickTypeView.CurrentItem
        // </summary>
        public static readonly DependencyProperty CurrentQuickTypeProperty = DependencyProperty.Register(
            "CurrentQuickType",
            typeof(NewItemFactoryTypeModel),
            typeof(SubPropertyEditor),
            new PropertyMetadata(null, new PropertyChangedCallback(OnCurrentQuickTypeChanged)));

        private ICollectionView _quickTypeView;
        private ObservableCollection<NewItemFactoryTypeModel> _quickTypeCollection;
        private bool _ignoreInternalChanges;
        private bool _exposedSubProperties;

        private ItemsControl _subPropertyListControl;

        // <summary>
        // Basic ctor
        // </summary>
        public SubPropertyEditor() 
        {
            _quickTypeCollection = new ObservableCollection<NewItemFactoryTypeModel>();

            _quickTypeView = CollectionViewSource.GetDefaultView(_quickTypeCollection);
            _quickTypeView.CurrentChanged += new EventHandler(OnCurrentQuickTypeChanged);
        }

        // Automation

        public event PropertyChangedEventHandler PropertyChanged;

        // Internal event we fire for the sake of SubPropertyEditorAutomationPeer that
        // causes it to refresh its offered set of children
        internal event EventHandler VisualsChanged;

        public PropertyEntry PropertyEntry 
        {
            get { return (PropertyEntry)this.GetValue(PropertyEntryProperty); }
            set { this.SetValue(PropertyEntryProperty, value); }
        }

        public bool IsExpanded 
        {
            get { return (bool)this.GetValue(IsExpandedProperty); }
            set { this.SetValue(IsExpandedProperty, value); }
        }

        public NewItemFactoryTypeModel CurrentQuickType 
        {
            get { return (NewItemFactoryTypeModel)this.GetValue(CurrentQuickTypeProperty); }
            set { this.SetValue(CurrentQuickTypeProperty, value); }
        }

        // <summary>
        // Gets a flag indicating whether QuickTypes exist
        // </summary>
        public bool HasQuickTypes 
        {
            get {
                return _quickTypeCollection.Count > 0;
            }
        }

        // <summary>
        // Returns a list of available QuickTypes (collection of NewItemFactoryTypeModel instances)
        // </summary>
        public ICollectionView QuickTypes 
        {
            get {
                return _quickTypeView;
            }
        }

        // <summary>
        // Exposes PropertyValue.SubProperties when the IsExpanded flag first gets set to true
        // and forever thereafter (or at least until the current PropertyValue changes and we
        // collapse the sub-properties)
        // </summary>
        public IEnumerable<PropertyEntry> SelectiveSubProperties 
        {
            get {
                if (!_exposedSubProperties) 
                {
                    if (!this.IsExpanded)
                    {
                        yield break;
                    }

                    _exposedSubProperties = true;
                }

                PropertyEntry parent = this.PropertyEntry;
                if (parent == null)
                {
                    yield break;
                }

                foreach (ModelPropertyEntry subProperty in parent.PropertyValue.SubProperties)
                {
                    if (subProperty.IsBrowsable)
                    {
                        yield return subProperty;
                    }
                }
            }
        }

        // <summary>
        // Gets a flag indicating whether the sub-property editor can be expanded or not.
        // </summary>
        public bool IsExpandable 
        {
            get { return this.HasQuickTypes && this.CurrentQuickType != null; }
        }

        // <summary>
        // Gets a SelectionPath to itself.
        // </summary>
        public SelectionPath Path 
        {
            get { return PropertySelectionPathInterpreter.Instance.ConstructSelectionPath(this.PropertyEntry); }
        }

        // <summary>
        // Gets a description of the contained property
        // to expose through automation
        // </summary>
        public string Description 
        {
            get {
                PropertyEntry property = this.PropertyEntry;
                if (property != null) 
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        Properties.Resources.PropertyEditing_SelectionStatus_Property,
                        this.PropertyEntry.PropertyName);
                }

                return string.Empty;
            }
        }


        // <summary>
        // Exposes the ItemsControl used to display the list of sub-properties.  UI-specific
        // </summary>
        private ItemsControl SubPropertyListControl 
        {
            get {
                if (_subPropertyListControl == null) 
                {
                    _subPropertyListControl = VisualTreeUtils.GetNamedChild<ItemsControl>(this, "PART_SubPropertyList");
                    Fx.Assert(_subPropertyListControl != null, "UI for SubPropertyEditor changed.  Need to update SubPropertyEditor class logic.");
                }

                return _subPropertyListControl;
            }
        }


        // Keyboard Navigation

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new SubPropertyEditorAutomationPeer(this);
        }


        // Properties

        // PropertyEntry DP

        // When the displayed PropertyEntry changes, make sure we update the UI and hook into the
        // new PropertyEntry's notification mechanism
        private static void OnPropertyEntryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            SubPropertyEditor theThis = obj as SubPropertyEditor;
            if (theThis == null)
            {
                return;
            }

            PropertyEntry oldValue = e.OldValue as PropertyEntry;
            if (oldValue != null) 
            {
                oldValue.PropertyValue.RootValueChanged -= new EventHandler(theThis.OnPropertyValueRootValueChanged);
            }

            PropertyEntry newValue = e.NewValue as PropertyEntry;
            if (newValue != null) 
            {
                newValue.PropertyValue.RootValueChanged += new EventHandler(theThis.OnPropertyValueRootValueChanged);
            }

            theThis.RefreshVisuals();
        }

        private void OnPropertyValueRootValueChanged(object sender, EventArgs e) 
        {
            if (_ignoreInternalChanges)
            {
                return;
            }

            RefreshVisuals();
        }


        // IsExpanded DP

        private static void OnIsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            SubPropertyEditor theThis = obj as SubPropertyEditor;
            if (theThis == null)
            {
                return;
            }

            bool newIsExpanded = (bool)e.NewValue;
            PropertyEntry containedProperty = theThis.PropertyEntry;

            // Store the new expansion state
            if (containedProperty != null) 
            {
                PropertyState state = PropertyStateContainer.Instance.GetPropertyState(
                    ModelUtilities.GetCachedSubPropertyHierarchyPath(containedProperty));
                state.SubPropertiesExpanded = newIsExpanded;
            }

            // If we are expanded but we never exposed the sub-properties to anyone before,
            // fire a signal saying that a list of sub-properties may be now available, so that
            // UI DataBindings refresh themselves
            if (newIsExpanded == true &&
                theThis._exposedSubProperties == false) 
            {
                theThis.FireSubPropertiesListChangedEvents();
            }
        }

        // CurrentQuickType DP

        // This method gets called when the DP changes
        private static void OnCurrentQuickTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            SubPropertyEditor theThis = obj as SubPropertyEditor;
            if (theThis == null)
            {
                return;
            }

            if (theThis._ignoreInternalChanges)
            {
                return;
            }

            theThis._quickTypeView.MoveCurrentTo(e.NewValue);

            theThis.ExpandSubProperties();
            theThis.FireSubPropertiesListChangedEvents();
        }

        // This method gets called when the CurrentItem on _quickTypeView changes
        private void OnCurrentQuickTypeChanged(object sender, EventArgs e) 
        {
            if (_ignoreInternalChanges)
            {
                return;
            }

            NewItemFactoryTypeModel selectedTypeModel = _quickTypeView.CurrentItem as NewItemFactoryTypeModel;

            if (selectedTypeModel == null)
            {
                return;
            }

            Fx.Assert(this.PropertyEntry != null, "PropertyEntry should not be null");
            if (this.PropertyEntry == null)
            {
                return;
            }

            bool previousValue = IgnoreInternalChanges();
            try 
            {
                this.PropertyEntry.PropertyValue.Value = selectedTypeModel.CreateInstance();
            }
            finally 
            {
                NoticeInternalChanges(previousValue);
            }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) 
        {
            if (e.Property == PropertyContainer.OwningPropertyContainerProperty) 
            {

                // A quick and dirty way to register this instance as the implementation of 
                // ISelectionBranchPoint that controls the expansion / collapse of this control
                //
                OnOwningPropertyContainerChanged((PropertyContainer)e.OldValue, (PropertyContainer)e.NewValue);

            }

            base.OnPropertyChanged(e);
        }

        private void OnOwningPropertyContainerChanged(PropertyContainer oldValue, PropertyContainer newValue) 
        {
            if (oldValue != null) 
            {
                PropertySelection.ClearSelectionStop(oldValue);
                PropertySelection.ClearIsSelectionStopDoubleClickTarget(oldValue);
            }

            if (newValue != null) 
            {
                PropertySelection.SetSelectionStop(newValue, this);
                PropertySelection.SetIsSelectionStopDoubleClickTarget(newValue, true);
            }
        }


        // Visual Lookup Helpers

        // <summary>
        // Looks for and returns the specified sub-property
        // </summary>
        // <param name="propertyName">Sub-property to look up</param>
        // <returns>Corresponding PropertyEntry if found, null otherwise.</returns>
        internal PropertyEntry FindSubPropertyEntry(string propertyName) 
        {
            if (string.IsNullOrEmpty(propertyName)) 
            {
                return null;
            }

            foreach (PropertyEntry property in SelectiveSubProperties)
            {
                if (property.PropertyName.Equals(propertyName))
                {
                    return property;
                }
            }

            return null;
        }

        // <summary>
        // Looks for and returns the PropertyContainer used to display
        // the specified PropertyEntry
        // </summary>
        // <param name="property">Property to look for</param>
        // <returns>Corresponding PropertyContainer if found, null otherwise.</returns>
        internal PropertyContainer FindSubPropertyEntryVisual(PropertyEntry property) 
        {
            if (property == null) 
            {
                return null;
            }

            ItemsControl subPropertyListControl = this.SubPropertyListControl;
            if (subPropertyListControl == null)
            {
                return null;
            }

            return subPropertyListControl.ItemContainerGenerator.ContainerFromItem(property) as PropertyContainer;
        }


        // Helpers

        private void RefreshVisuals() 
        {
            RefreshQuickTypes();
            RestoreIsExpandedState();
            FireVisualsChangedEvents();
        }

        private void RefreshQuickTypes() 
        {
            bool previousValue = IgnoreInternalChanges();
            try 
            {
                _quickTypeCollection.Clear();

                PropertyEntry containedProperty = this.PropertyEntry;
                if (containedProperty == null)
                {
                    return;
                }

                ModelProperty property = ((ModelPropertyEntry)containedProperty).FirstModelProperty;
                Type containerValueType = ((ModelPropertyEntryBase)containedProperty).CommonValueType;
                NewItemFactoryTypeModel selectedFactoryModel = null;
                Type defaultItemType = GetDefaultItemType(property);

                // Find all elligible NewItemFactoryTypes declared through metadata
                IEnumerable<NewItemFactoryTypeModel> factoryModels =
                    ExtensibilityAccessor.GetNewItemFactoryTypeModels(
                    property,
                    ResourceUtilities.GetDesiredTypeIconSize(this));

                if (factoryModels != null) 
                {
                    foreach (NewItemFactoryTypeModel factoryModel in factoryModels) 
                    {
                        _quickTypeCollection.Add(factoryModel);

                        if (selectedFactoryModel == null) 
                        {
                            if (object.Equals(containerValueType, factoryModel.Type)) 
                            {
                                selectedFactoryModel = factoryModel;
                            }
                        }

                        if (defaultItemType != null &&
                            object.Equals(defaultItemType, factoryModel.Type)) 
                        {
                            defaultItemType = null;
                        }
                    }
                }

                //add a null value - user should always have an option to clear property value
                NewItemFactoryTypeModel nullTypeFactoryTypeModel =
                    new NewItemFactoryTypeModel(null, new NullItemFactory());

                // Add a default item type based on the property type (if it wasn't also added through
                // metadata)
                if (defaultItemType != null) 
                {
                    NewItemFactoryTypeModel defaultItemFactoryTypeModel = new NewItemFactoryTypeModel(defaultItemType, new NewItemFactory());
                    _quickTypeCollection.Add(defaultItemFactoryTypeModel);

                    if (selectedFactoryModel == null) 
                    {
                        if (object.Equals(containerValueType, defaultItemFactoryTypeModel.Type)) 
                        {
                            selectedFactoryModel = defaultItemFactoryTypeModel;
                        }
                        else if (containerValueType == null)
                        {
                            selectedFactoryModel = nullTypeFactoryTypeModel;
                        }
                    }
                }
                
                _quickTypeCollection.Add(nullTypeFactoryTypeModel);

                // Make sure the currently selected value on the CollectionView reflects the
                // actual value of the property
                _quickTypeView.MoveCurrentTo(selectedFactoryModel);
                this.CurrentQuickType = selectedFactoryModel;
            }
            finally 
            {
                NoticeInternalChanges(previousValue);
            }
        }

        private static Type GetDefaultItemType(ModelProperty property) 
        {
            if (property == null)
            {
                return null;
            }

            Type propertyType = property.PropertyType;
            if (EditorUtilities.IsConcreteWithDefaultCtor(propertyType))
            {
                return propertyType;
            }

            return null;
        }

        private void RestoreIsExpandedState() 
        {
            bool newIsExpanded = false;
            PropertyEntry property = this.PropertyEntry;

            if (property != null) 
            {
                PropertyState state = PropertyStateContainer.Instance.GetPropertyState(
                    ModelUtilities.GetCachedSubPropertyHierarchyPath(property));
                newIsExpanded = state.SubPropertiesExpanded;
            }

            this.IsExpanded = newIsExpanded;
            _exposedSubProperties = false;
        }

        private void ExpandSubProperties() 
        {
            this.IsExpanded = true;
        }


        // Change Notification Helpers

        private bool IgnoreInternalChanges() 
        {
            bool previousValue = _ignoreInternalChanges;
            _ignoreInternalChanges = true;
            return previousValue;
        }

        private void NoticeInternalChanges(bool previousValue) 
        {
            _ignoreInternalChanges = previousValue;
        }

        private void FireVisualsChangedEvents() 
        {
            // Fire updated events
            OnPropertyChanged("HasQuickTypes");
            OnPropertyChanged("QuickTypes");
            FireSubPropertiesListChangedEvents();
        }

        private void FireSubPropertiesListChangedEvents() 
        {
            OnPropertyChanged("IsExpandable");
            OnPropertyChanged("SelectiveSubProperties");

            if (VisualsChanged != null)
            {
                VisualsChanged(this, EventArgs.Empty);
            }
        }


        // INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName) 
        {
            Fx.Assert(!string.IsNullOrEmpty(propertyName), "Can't raise OnPropertyChanged event without a valid property name.");

            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // ISelectionStop Members

        //NullItemFactory - this class is used to provide a null entry in quick types list - it is required to allow user 
        //to clear value of an object.
        sealed class NullItemFactory : NewItemFactory
        {
            public override object CreateInstance(Type type)
            {
                //no input type is allowed - we never create instance of anything
                Fx.Assert(type == null, "NullItemFactory supports only null as type parameter");
                return null;
            }

            public override string GetDisplayName(Type type)
            {
                //no input type is allowed - we always return (null) string
                Fx.Assert(type == null, "NullItemFactory supports only null as type parameter");
                return "(null)";
            }
        }
    }
}
