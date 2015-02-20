//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Media;

    using System.Runtime;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // This class contains the core logic for CategoryContainerAutomationPeer.  It's constructor is
    // private because it can be instantiated in two different ways, which are exposed through
    // static methods: CreateStandAloneAutomationPeer() and CreateItemAutomationPeer().
    //
    // CreateStandAloneAutomationPeer() returns an AutomationPeer that is agnostic of its parent
    // CreateItemAutomationPeer() returns an AutomationPeer where the parent is assumed to be an ItemsControl.
    //
    // Because in the CategoryList : ItemsControl class we manually override GetContainerForItemOverride()
    // as a way of reducing control count in PI, we need to provide both versions of the
    // CategoryContainerAutomationPeer.
    // </summary>
    internal class CategoryContainerAutomationPeer : IExpandCollapseProvider, IScrollItemProvider 
    {

        private CiderCategoryContainer _container;
        private AutomationPeer _itemAutomationPeer;

        // Private ctor called from two public, static accessors
        private CategoryContainerAutomationPeer(CiderCategoryContainer container, AutomationPeer itemPeer) 
        {
            Fx.Assert(container != null, "CategoryContainer not specified.");
            Fx.Assert(itemPeer != null, "CategoryContainerItemAutomationPeer not specified.");
            _container = container;
            _itemAutomationPeer = itemPeer;

        }

        // <summary>
        // Gets the expand / collapse state of the contained CategoryContainer
        // </summary>
        public ExpandCollapseState ExpandCollapseState 
        {
            get {
                if (_container == null)
                {
                    return ExpandCollapseState.Collapsed;
                }

                return _container.Expanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
            }
        }
        
        // IScrollItemProvider Members

        // <summary>
        // Creates a CategoryContainerAutomationPeer that can be returned directly from
        // the CiderCategoryContainer control itself.
        // </summary>
        // <param name="container">Container to automate</param>
        // <returns>Instance of CategoryContainerAutomationPeer that can be returned directly from
        // the CiderCategoryContainer control itself</returns>
        public static UIElementAutomationPeer CreateStandAloneAutomationPeer(CiderCategoryContainer container) 
        {
            return new CategoryContainerStandAloneAutomationPeer(container);
        }

        // <summary>
        // Creates a CategoryContainerAutomationPeer that can be returned for CategoryContainers
        // belonging to a CategoryList control
        // </summary>
        // <param name="category">CategoryEntry instance used by ItemsControl</param>
        // <param name="container">Container to automate</param>
        // <param name="parentAutomationPeer">Parent AutomationPeer</param>
        // <returns>Instance of CategoryContainerAutomationPeer that can be returned for CategoryContainers
        // belonging to a CategoryList control</returns>
        public static ItemAutomationPeer CreateItemAutomationPeer(ModelCategoryEntry category, CiderCategoryContainer container, CategoryListAutomationPeer parentAutomationPeer) 
        {
            return new CategoryContainerItemAutomationPeer(category, container, parentAutomationPeer);
        }

        // Gets the children of the contained CategoryContainer, returning AutomationPeers
        // for CategoryEditors followed by AutomationPeers for any left-over properties
        private List<AutomationPeer> GetChildrenCore() 
        {
            List<AutomationPeer> children = new List<AutomationPeer>();

            if (_container != null) 
            {
                AddCategoryEditors(children, VisualTreeUtils.GetNamedChild<ItemsControl>(_container, "PART_BasicCategoryEditors"), Resources.PropertyEditing_BasicCategoryEditors);
                AddCategoryProperties(children, VisualTreeUtils.GetNamedChild<ItemsControl>(_container, "PART_BasicPropertyList"));

                if (_container.Expanded) 
                {

                    Expander advancedExpander = VisualTreeUtils.GetNamedChild<Expander>(_container, "PART_AdvancedExpander");

                    if (advancedExpander != null &&
                        advancedExpander.Visibility == Visibility.Visible) 
                    {
                        children.Add(new AdvancedCategoryContainerAutomationPeer(_container, advancedExpander));
                    }
                }
            }

            return children;
        }

        // Adds AutomationPeers for all CategoryEditors displayed within the contained CategoryContainer
        private static void AddCategoryEditors(List<AutomationPeer> peers, ItemsControl editors, string containerDisplayName) 
        {
            if (editors == null || editors.Items.Count == 0)
            {
                return;
            }

            peers.Add(new CategoryEditorListAutomationPeer(containerDisplayName, editors));
        }

        // Adds AutomationPeers for all PropertyEntries not consumed by the CategoryEditors within this
        // CategoryContainer
        private static void AddCategoryProperties(List<AutomationPeer> peers, ItemsControl properties) 
        {
            if (properties == null)
            {
                return;
            }

            int childCount = properties.Items.Count;

            for (int i = 0; i < childCount; i++) 
            {
                PropertyContainer propertyContainer = properties.ItemContainerGenerator.ContainerFromIndex(i) as PropertyContainer;

                if (propertyContainer != null)
                {
                    peers.Add(new PropertyContainerAutomationPeer(propertyContainer));
                }
            }
        }

        // Gets the implementation of the specified pattern if one exists.
        // Currently supported patterns: ExpandCollapse
        private object GetPattern(PatternInterface patternInterface) 
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }
            else if (patternInterface == PatternInterface.ScrollItem)
            {
                return this;
            }

            return null;
        }

        // Returns the name of the represented category
        private string GetNameCore() 
        {
            if (_container != null)
            {
                return _container.Category.CategoryName;
            }

            return string.Empty;
        }

        // Return "CategoryContainer"
        private static string GetClassNameCore() 
        {
            return typeof(CategoryContainer).Name;
        }

        // Gets the help text to associated with the contained CategoryContainer
        private static string GetHelpTextCore() 
        {
            return Resources.PropertyEditing_CategoryContainerAutomationPeerHelp;
        }

        // IExpandCollapseProvider Members

        // <summary>
        // Collapses the contained CategoryContainer
        // </summary>
        public void Collapse() 
        {
            if (_container != null)
            {
                _container.Expanded = false;
            }
        }

        // <summary>
        // Expands the contained CategoryContainer
        // </summary>
        public void Expand() 
        {
            if (_container != null)
            {
                _container.Expanded = true;
            }
        }

        // <summary>
        // Scrolls the contained CategoryContainer into view
        // </summary>
        public void ScrollIntoView() 
        {
            if (_container != null)
            {
                _container.BringIntoView();
            }
        }


        // RaiseSelectionEventsForScreenReader

        // <summary>
        //  This public method is called when parent creates the CategoryContainerItemAutomationPeer
        //  in the GetChildrenCore, thus listening to the changes in the IsSelectedProperty.
        // </summary>
        public void AddFocusEvents() 
        {
            if (_container != null) 
            {
                HookUpFocusEvents(VisualTreeUtils.GetNamedChild<Expander>(_container, "PART_MainExpander"), OnIsSelectedValueChanged);
            }
        }

        // <summary>
        // Private helper function to listen to the changes in the IsSelectedProperty,
        // which then fires the OnValueChanged event.
        // </summary>
        // <param name="expander">Expander control</param>
        // <param name="valueChangedEvent">ValueChanged event</param>
        private static void HookUpFocusEvents(Expander expander, EventHandler valueChangedEvent) 
        {
            if (expander != null) 
            {
                FrameworkElement expanderGrid = VisualTreeUtils.GetNamedChild<FrameworkElement>(expander, "PART_BasicSection");
                if (expanderGrid != null) 
                {
                    DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(Grid));
                    if (dpd != null) 
                    {
                        dpd.AddValueChanged(expanderGrid, valueChangedEvent);
                    }
                }
            }
        }

        // <summary>
        // The actual event handler, that fires when the IsSelected DP changes.
        // Here we raise the AutomationFocus event.
        // </summary>
        // <param name="sender">Expander</param>
        // <param name="e">EventArgs</param>
        private void OnIsSelectedValueChanged(object sender, EventArgs e) 
        {
            // Add logic to respond to "Selection"
            bool curVal = PropertySelection.GetIsSelected(sender as DependencyObject);
            if (curVal) 
            {
                _itemAutomationPeer.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
            }
        }



        // <summary>
        //  This public method is called when parent creates the CategoryContainerItemAutomationPeer
        //  in the GetChildrenCore, thus clearing off all the event listeners before we add new ones
        // </summary>
        public void RemoveFocusEvents() 
        {
            if (_container != null) 
            {
                UnHookFocusEvents(VisualTreeUtils.GetNamedChild<Expander>(_container, "PART_MainExpander"), OnIsSelectedValueChanged);
            }
        }

        // <summary>
        // Private method to unhook the ValueChanged event.
        // </summary>
        // <param name="expander">Expander</param>
        // <param name="valueChangedEvent">ValueChanged event</param>
        private static void UnHookFocusEvents(Expander expander, EventHandler valueChangedEvent) 
        {
            if (expander != null) 
            {
                FrameworkElement expanderGrid = VisualTreeUtils.GetNamedChild<FrameworkElement>(expander, "PART_BasicSection");
                if (expanderGrid != null) 
                {
                    DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(Grid));
                    if (dpd != null) 
                    {
                        dpd.RemoveValueChanged(expanderGrid, valueChangedEvent);
                    }
                }
            }
        }

        // end RaiseSelectionEventsForScreenReader

        // <summary>
        // Helper AutomationPeer that represents CategoryContainerAutomationPeer when the CategoryContainer
        // is hosted in a CategoryList control.  All of its methods delegate to CategoryContainerAutomationPeer
        // </summary>
        internal class CategoryContainerItemAutomationPeer : ItemAutomationPeer, IAutomationFocusChangedEventSource 
        {

            private CategoryContainerAutomationPeer _coreLogic;
            private List<AutomationPeer> _children;

            public CategoryContainerItemAutomationPeer(
                ModelCategoryEntry item,
                CiderCategoryContainer container,
                CategoryListAutomationPeer parentAutomationPeer)
                : base(item, parentAutomationPeer) 
            {
                _coreLogic = new CategoryContainerAutomationPeer(container, this);
                _coreLogic.AddFocusEvents();
            }

            // Implementation of this method is specific to CategoryContainerItemAutomationPeer
            protected override AutomationControlType GetAutomationControlTypeCore() 
            {
                return AutomationControlType.List;
            }

            protected override List<AutomationPeer> GetChildrenCore() 
            {
                _children = _coreLogic.GetChildrenCore();
                return _children;
            }

            protected override string GetNameCore() 
            {
                return _coreLogic.GetNameCore();
            }

            protected override string GetClassNameCore() 
            {
                return CategoryContainerAutomationPeer.GetClassNameCore();
            }

            protected override string GetHelpTextCore() 
            {
                return CategoryContainerAutomationPeer.GetHelpTextCore();
            }

            public override object GetPattern(PatternInterface patternInterface) 
            {
                return _coreLogic.GetPattern(patternInterface);
            }

            // IAutomationFocusChangedEventSource Members
            public void UnloadEventHook() 
            {
                _coreLogic.RemoveFocusEvents();
                if (_children != null) 
                {
                    foreach (AutomationPeer peer in _children) 
                    {
                        IAutomationFocusChangedEventSource unhookEventPeer = peer as IAutomationFocusChangedEventSource;
                        if (unhookEventPeer != null) 
                        {
                            unhookEventPeer.UnloadEventHook();
                        }
                    }
                }
            }
        }

        // <summary>
        // Automation peer we use to display the list of CategoryEditors
        // </summary>
        private class CategoryEditorListAutomationPeer : ItemsControlAutomationPeer 
        {

            private string _displayName;

            // Note: the display name we use here is for completeness only.  This class is hidden
            // from screen readers.
            public CategoryEditorListAutomationPeer(string displayName, ItemsControl owner)
                : base(owner) 
            {
                _displayName = displayName ?? string.Empty;
            }

            protected override ItemAutomationPeer CreateItemAutomationPeer(object item) 
            {
                return new CategoryEditorAutomationPeer(item, this);
            }

            protected override string GetClassNameCore() 
            { 
                return typeof(ItemsControl).Name; 
            }
            protected override string GetNameCore() 
            { 
                return _displayName; 
            }
            protected override bool IsControlElementCore() 
            { 
                return false; 
            }
            protected override bool IsContentElementCore() 
            { 
                return false; 
            }

            private class CategoryEditorAutomationPeer : ItemAutomationPeer 
            {

                public CategoryEditorAutomationPeer(object item, CategoryEditorListAutomationPeer parent)
                    : base(item, parent) 
                {
                }

                public override object GetPattern(PatternInterface patternInterface) 
                { 
                    return null; 
                }
                protected override AutomationControlType GetAutomationControlTypeCore() 
                { 
                    return AutomationControlType.Custom; 
                }
                protected override string GetClassNameCore() 
                { 
                    return typeof(CategoryEditor).Name; 
                }
                protected override string GetNameCore() 
                { 
                    return Item == null ? string.Empty : Item.GetType().Name; 
                }
                protected override bool IsContentElementCore() 
                { 
                    return true; 
                }
                protected override bool IsControlElementCore() 
                { 
                    return true; 
                }
            }
        }

        // <summary>
        // Helper AutomationPeer for the advanced portion of the CategoryContainer
        // </summary>
        private class AdvancedCategoryContainerAutomationPeer : ExpanderAutomationPeer, IAutomationFocusChangedEventSource 
        {

            private CiderCategoryContainer _container;
            private Expander _expander;
            private List<AutomationPeer> _children;

            public AdvancedCategoryContainerAutomationPeer(CiderCategoryContainer container, Expander expander)
                : base(expander) 
            {
                Fx.Assert(container != null, "CategoryContainer not specified.");
                Fx.Assert(expander != null, "Expander not specified.");
                _expander = expander;
                _container = container;
                AddFocusEvents();
            }

            // <summary>
            //  This public method is called when parent creates the CategoryContainerItemAutomationPeer
            //  in the GetChildrenCore, thus listening to the changes in the IsSelectedProperty.
            // </summary>
            private void AddFocusEvents() 
            {
                if (_container != null) 
                {
                    HookUpFocusEvents(VisualTreeUtils.GetNamedChild<Expander>(_container, "PART_AdvancedExpander"), OnIsSelectedValueChanged);
                }
            }

            protected override List<AutomationPeer> GetChildrenCore() 
            {
                _children = new List<AutomationPeer>();
                if (_container != null) 
                {
                    CategoryContainerAutomationPeer.AddCategoryEditors(_children, VisualTreeUtils.GetNamedChild<ItemsControl>(_container, "PART_AdvancedCategoryEditors"), Resources.PropertyEditing_AdvancedCategoryEditors);
                    CategoryContainerAutomationPeer.AddCategoryProperties(_children, VisualTreeUtils.GetNamedChild<ItemsControl>(_container, "PART_AdvancedPropertyList"));
                }
                //Add focus events for Subproperty editor
                foreach (AutomationPeer peer in _children) 
                {
                    PropertyContainerAutomationPeer pcAutomationPeer = peer as PropertyContainerAutomationPeer;
                    if (pcAutomationPeer != null) 
                    {
                        pcAutomationPeer.AddFocusEvents();
                    }
                }
                return _children;
            }

            protected override string GetNameCore() 
            {
                return _expander.Header.ToString();
            }

            // <summary>
            // Private helper function to listen to the changes in the IsSelectedProperty,
            // which then fires the OnValueChanged event.
            // </summary>
            // <param name="expander">Expander control</param>
            // <param name="valueChangedEvent">ValueChanged event</param>
            private static void HookUpFocusEvents(Expander expander, EventHandler valueChangedEvent) 
            {
                if (expander != null) 
                {
                    FrameworkElement expanderGrid = VisualTreeUtils.GetNamedChild<FrameworkElement>(expander, "PART_AdvancedSection");
                    if (expanderGrid != null) 
                    {
                        DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(Grid));
                        if (dpd != null) 
                        {
                            dpd.AddValueChanged(expanderGrid, valueChangedEvent);
                        }
                    }
                }
            }

            // <summary>
            // The actual event handler, that fires when the IsSelected DP changes.
            // Here we raise the AutomationFocus event for the
            // Advanced (More Properties) properties expander.
            // </summary>
            // <param name="sender">Expander</param>
            // <param name="e">EventArgs</param>
            private void OnIsSelectedValueChanged(object sender, EventArgs e) 
            {
                // Add logic to respond to "Selection"
                bool curVal = PropertySelection.GetIsSelected(sender as DependencyObject);
                if (curVal) 
                {
                    this.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
                }
            }

            // <summary>
            //  This public method is called when parent creates the CategoryContainerItemAutomationPeer
            //  in the GetChildrenCore, thus clearing off all the event listeners before we add new ones
            // </summary>
            public void RemoveFocusEvents() 
            {
                if (_container != null) 
                {
                    UnHookFocusEvents(VisualTreeUtils.GetNamedChild<Expander>(_container, "PART_AdvancedExpander"), OnIsSelectedValueChanged);
                }
            }

            // <summary>
            // Private method to unhook the ValueChanged event.
            // </summary>
            // <param name="expander">Expander</param>
            // <param name="valueChangedEvent">ValueChanged event</param>
            private static void UnHookFocusEvents(Expander expander, EventHandler valueChangedEvent) 
            {
                if (expander != null) 
                {
                    FrameworkElement expanderGrid = VisualTreeUtils.GetNamedChild<FrameworkElement>(expander, "PART_AdvancedSection");
                    if (expanderGrid != null) 
                    {
                        DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(Grid));
                        if (dpd != null) 
                        {
                            dpd.RemoveValueChanged(expanderGrid, valueChangedEvent);
                        }
                    }
                }
            }

            // IAutomationFocusChangedEventSource Members
            public void UnloadEventHook() 
            {
                RemoveFocusEvents();
                if (_children != null) 
                {
                    foreach (AutomationPeer peer in _children) 
                    {
                        IAutomationFocusChangedEventSource unhookEventPeer = peer as IAutomationFocusChangedEventSource;
                        if (unhookEventPeer != null) 
                        {
                            unhookEventPeer.UnloadEventHook();
                        }
                    }
                }
            }
        }

        // <summary>
        // Helper AutomationPeer that represents CategoryContainerAutomationPeer when the CategoryContainer
        // sits on its own.  All of its methods delegate to CategoryContainerAutomationPeer
        // </summary>
        private class CategoryContainerStandAloneAutomationPeer : UIElementAutomationPeer, IAutomationFocusChangedEventSource
        {

            private CategoryContainerAutomationPeer _coreLogic;

            public CategoryContainerStandAloneAutomationPeer(CiderCategoryContainer container)
                : base(container) 
            {
                _coreLogic = new CategoryContainerAutomationPeer(container, this);
                _coreLogic.AddFocusEvents();
            }

            protected override List<AutomationPeer> GetChildrenCore() 
            {
                return _coreLogic.GetChildrenCore();
            }

            protected override string GetNameCore() 
            {
                return _coreLogic.GetNameCore();
            }

            protected override string GetClassNameCore() 
            {
                return CategoryContainerAutomationPeer.GetClassNameCore();
            }

            protected override string GetHelpTextCore() 
            {
                return CategoryContainerAutomationPeer.GetHelpTextCore();
            }

            public override object GetPattern(PatternInterface patternInterface) 
            {
                return _coreLogic.GetPattern(patternInterface);
            }

            // IAutomationFocusChangedEventSource Members

            public void UnloadEventHook() 
            {
                _coreLogic.RemoveFocusEvents();
            }
        }
    }
}
