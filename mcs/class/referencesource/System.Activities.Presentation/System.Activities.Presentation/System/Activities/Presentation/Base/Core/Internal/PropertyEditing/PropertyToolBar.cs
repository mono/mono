//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Runtime;

    using System.Activities.Presentation.Internal.PropertyEditing.Automation;
    using System.Activities.Presentation.Internal.PropertyEditing.State;
    using System.Activities.Presentation.Internal.PropertyEditing.Views;

    // <summary>
    // Container control that hosts both the property-search UI as well as the view-switching UI
    // </summary>
    internal sealed class PropertyToolBar : Control, INotifyPropertyChanged 
    {

        // <summary>
        // Property representing the currently-selected IPropertyViewManager
        // </summary>
        public static DependencyProperty CurrentViewManagerProperty = DependencyProperty.Register(
            "CurrentViewManager",
            typeof(IPropertyViewManager),
            typeof(PropertyToolBar),
            new PropertyMetadata(null, OnCurrentViewManagerChanged));

        // <summary>
        // Property containing a link to the CategoryList control instance that this
        // PropertyToolBar controls.  We need this link to be able to change the appearance
        // of each new generated CategoryContainer (whether it should show a header or not).
        // That way CategoryList doesn't need to know anything about this class.
        // </summary>
        public static DependencyProperty CategoryListProperty = DependencyProperty.Register(
            "CategoryList",
            typeof(CategoryList),
            typeof(PropertyToolBar),
            new PropertyMetadata(null, OnCategoryListChanged));

        private string _persistenceId;
        private bool _persistViewManagerChanges = true;

        public PropertyToolBar() 
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
        }

        // <summary>
        // Event we fire when the CurrentViewManager changes
        // </summary>
        public event EventHandler CurrentViewManagerChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        // <summary>
        // Gets or set the currently selected IPropertyViewManager
        // </summary>
        public IPropertyViewManager CurrentViewManager 
        {
            get { return (IPropertyViewManager)this.GetValue(CurrentViewManagerProperty); }
            set { this.SetValue(CurrentViewManagerProperty, value); }
        }

        // <summary>
        // Gets or set the value of the CategoryListProperty
        // </summary>
        public CategoryList CategoryList 
        {
            get { return (CategoryList)this.GetValue(CategoryListProperty); }
            set { this.SetValue(CategoryListProperty, value); }
        }

        // <summary>
        // Gets or sets a string ID that we use to differentiate between the states of different
        // PropertyToolBar instances.  Currently, we only care about two different PropertyToolBar
        // buckets - (1) the PTBs we use in the main PI and (2) the PTBs we use in collection editors.
        // </summary>
        public string PersistenceId 
        {
            get {
                return _persistenceId;
            }
            set {
                if (_persistenceId != value) 
                {
                    _persistenceId = value;

                    if (this.CurrentViewManager == null) 
                    {
                        bool oldPersistViewManagerChanges = _persistViewManagerChanges;
                        try 
                        {
                            _persistViewManagerChanges = false;
                            this.CurrentViewManager = PropertyViewManagerStateContainer.Instance.GetPropertyViewManager(_persistenceId);
                        }
                        finally 
                        {
                            _persistViewManagerChanges = oldPersistViewManagerChanges;
                        }
                    }

                    OnPropertyChanged("PersistenceId");
                }
            }
        }

        // <summary>
        // Convenience accessor for the UI data binding
        // </summary>
        public bool IsCategoryViewSelected 
        {
            get {
                if (this.CurrentViewManager != null)
                {
                    return this.CurrentViewManager == ByCategoryViewManager.Instance;
                }

                return false;
            }
            set {
                // No need to fire PropertyChanged events here - changing CurrentViewManager
                // will fire those events as a side-effect
                //
                if (this.CurrentViewManager == ByCategoryViewManager.Instance ^ value) 
                {
                    if (value)
                    {
                        this.CurrentViewManager = ByCategoryViewManager.Instance;
                    }
                    else
                    {
                        this.CurrentViewManager = AlphabeticalViewManager.Instance;
                    }
                }
            }
        }

        // <summary>
        // Convenience accessor for the UI data binding
        // </summary>
        public bool IsAlphaViewSelected 
        {
            get {
                if (this.CurrentViewManager != null)
                {
                    return this.CurrentViewManager == AlphabeticalViewManager.Instance;
                }

                return false;
            }
            set {
                // No need to fire PropertyChanged events here - changing CurrentViewManager
                // will fire those events as a side-effect
                //
                if (this.CurrentViewManager == AlphabeticalViewManager.Instance ^ value) 
                {
                    if (value)
                    {
                        this.CurrentViewManager = AlphabeticalViewManager.Instance;
                    }
                    else
                    {
                        this.CurrentViewManager = ByCategoryViewManager.Instance;
                    }
                }
            }
        }

        // AutomationPeer Stuff

        internal RadioButton ByCategoryViewButton 
        { get { return VisualTreeUtils.GetNamedChild<RadioButton>(this, "PART_ByCategoryViewButton"); } }
        internal RadioButton AlphaViewButton 
        { get { return VisualTreeUtils.GetNamedChild<RadioButton>(this, "PART_AlphaViewButton"); } }
        internal TextBlock SearchLabel 
        { get { return VisualTreeUtils.GetNamedChild<TextBlock>(this, "PART_SearchLabel"); } }
        internal TextBox SearchTextBox 
        { get { return VisualTreeUtils.GetNamedChild<TextBox>(this, "PART_SearchTextBox"); } }
        internal Button SearchClearButton 
        { get { return VisualTreeUtils.GetNamedChild<Button>(this, "PART_SearchClearButton"); } }

        private void OnLoaded(object sender, RoutedEventArgs e) 
        {
            PropertyViewManagerStateContainer.Instance.ContentRestored += new EventHandler(OnGlobalPropertyViewManagerRestored);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) 
        {
            PropertyViewManagerStateContainer.Instance.ContentRestored -= new EventHandler(OnGlobalPropertyViewManagerRestored);
        }

        private void OnGlobalPropertyViewManagerRestored(object sender, EventArgs e) 
        {

            // If the state of PropertyViewManagerStateContainer has been restored
            // update the current view of the PropertyToolBar based on the restored state

            if (this.PersistenceId != null) 
            {
                bool oldPersistViewManagerChanges = _persistViewManagerChanges;
                try 
                {
                    _persistViewManagerChanges = false;
                    this.CurrentViewManager = PropertyViewManagerStateContainer.Instance.GetPropertyViewManager(this.PersistenceId);
                }
                finally 
                {
                    _persistViewManagerChanges = oldPersistViewManagerChanges;
                }
            }
        }

        // CurrentViewManager DP

        private static void OnCurrentViewManagerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            PropertyToolBar theThis = obj as PropertyToolBar;
            if (theThis == null) 
            {
                return;
            }

            theThis.OnPropertyChanged("IsCategoryViewSelected");
            theThis.OnPropertyChanged("IsAlphaViewSelected");

            // Store and persist the CurrentViewManager value only if:
            // 
            // * this change did not come from PropertyViewManagerStateContainer itself
            // * we have a persistence ID to differentiate this control instance by
            // * this is not the first time the value was set - in other words, only
            //   store values that were triggered by the user making a conscious change
            //
            if (theThis.PersistenceId != null && theThis._persistViewManagerChanges && e.OldValue != null)
            {
                PropertyViewManagerStateContainer.Instance.StorePropertyViewManager(theThis.PersistenceId, e.NewValue as IPropertyViewManager);
            }

            // fire this event after we have stored the propertyviewmanager, so that the StateChanged event will get
            // the updated view-manager
            if (theThis.CurrentViewManagerChanged != null)
            {
                theThis.CurrentViewManagerChanged(theThis, EventArgs.Empty);
            }

            //refresh filter 
            if (null != theThis.CategoryList)
            {
                theThis.CategoryList.RefreshFilter();
            }

        }



        // CategoryList DP

        private static void OnCategoryListChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            PropertyToolBar theThis = obj as PropertyToolBar;
            if (theThis == null) 
            {
                return;
            }

            if (e.OldValue != null)
            {
                theThis.UnhookEvents(e.OldValue as CategoryList);
            }

            if (e.NewValue != null)
            {
                theThis.HookEvents(e.NewValue as CategoryList);
            }
        }

        private void HookEvents(CategoryList categoryList) 
        {
            if (categoryList == null) 
            {
                Debug.Fail("This method shouldn't be called when there is no CategoryList instance to process.");
                return;
            }

            categoryList.ContainerGenerated += new ContainerGeneratedHandler(OnCategoryContainerGenerated);
        }

        private void UnhookEvents(CategoryList categoryList) 
        {
            if (categoryList == null) 
            {
                Debug.Fail("This method shouldn't be called when there is no CategoryList instance to process.");
                return;
            }

            categoryList.ContainerGenerated -= new ContainerGeneratedHandler(OnCategoryContainerGenerated);
        }

        private void OnCategoryContainerGenerated(object sender, ContainerGeneratedEventArgs e) 
        {
            if (e.Container == null) 
            {
                return;
            }
            e.Container.ShowCategoryHeader = this.CurrentViewManager == null ? true : this.CurrentViewManager.ShowCategoryHeaders;
        }



        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new UIElementAutomationPeer(this);
        }



        // INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName) 
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
