//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Media;

    using System.Activities.Presentation;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // AutomationPeer for PropertyContainer
    // </summary>
    internal class PropertyContainerAutomationPeer : UIElementAutomationPeer, IValueProvider, IScrollItemProvider, IAutomationFocusChangedEventSource
    {

        private PropertyContainer _container;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="container"></param>
        public PropertyContainerAutomationPeer(PropertyContainer container)
            : base(container) 
        {
            if (container == null)
            {
                throw FxTrace.Exception.ArgumentNull("container");
            }

            _container = container;
        }

        // <summary>
        // Gets a value indicating whether the contained PropertyContainer is read-only
        // </summary>
        public bool IsReadOnly 
        {
            get { return _container.PropertyEntry.IsReadOnly || !_container.PropertyEntry.PropertyValue.CanConvertFromString; }
        }

        // <summary>
        // Gets the value of the property within the PropertyContainer
        // </summary>
        public string Value 
        {
            get { return _container.PropertyEntry.PropertyValue.StringValue; }
        }


        // IScrollItemProvider Members

        // <summary>
        //  This public method is called when parent creates the CategoryContainerItemAutomationPeer
        //  in the GetChildrenCore, thus listening to the changes in the IsSelectedProperty.
        // </summary>
        internal void AddFocusEvents() 
        {
            if (_container != null) 
            {
                HookUpFocusEvents(_container, OnIsSelectedValueChanged);
            }
        }

        // <summary>
        // "PropertyContainer"
        // </summary>
        // <returns>"PropertyContainer"</returns>
        protected override string GetClassNameCore() 
        {
            return typeof(PropertyContainer).Name;
        }

        // <summary>
        // Returns the name of the contained property
        // </summary>
        // <returns></returns>
        protected override string GetNameCore() 
        {
            return _container.PropertyEntry == null ? string.Empty : _container.PropertyEntry.DisplayName;
        }

        // <summary>
        // Currently supported patterns: Value
        // </summary>
        // <param name="patternInterface"></param>
        // <returns></returns>
        public override object GetPattern(PatternInterface patternInterface) 
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            else if (patternInterface == PatternInterface.ScrollItem)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        // <summary>
        // Private helper function to listen to the changes in the IsSelectedProperty,
        // which then fires the OnValueChanged event.
        // </summary>
        // <param name="expander">Expander control</param>
        // <param name="valueChangedEvent">ValueChanged event</param>
        private static void HookUpFocusEvents(PropertyContainer container, EventHandler valueChangedEvent) 
        {
            if (container != null) 
            {
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(PropertyContainer));
                if (dpd != null) 
                {
                    dpd.AddValueChanged(container, valueChangedEvent);
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
                UnHookFocusEvents(_container, OnIsSelectedValueChanged);
            }
        }

        // <summary>
        // Private method to unhook the ValueChanged event.
        // </summary>
        // <param name="expander">Expander</param>
        // <param name="valueChangedEvent">ValueChanged event</param>
        private static void UnHookFocusEvents(PropertyContainer container, EventHandler valueChangedEvent) 
        {
            if (container != null) 
            {
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PropertySelection.IsSelectedProperty, typeof(PropertyContainer));
                if (dpd != null) 
                {
                    dpd.RemoveValueChanged(container, valueChangedEvent);
                }
            }
        }

        // IValueProvider Members

        // <summary>
        // Sets the value of the property within the PropertyContainer
        // </summary>
        // <param name="value">Value to set</param>
        public void SetValue(string value) 
        {
            _container.PropertyEntry.PropertyValue.StringValue = value;
        }

        // <summary>
        // Scrolls the contained PropertyContainer into view, if it's within a scrolling control
        // </summary>
        public void ScrollIntoView() 
        {
            _container.BringIntoView();
        }


        // IAutomationFocusChangedEventSource Members
        public void UnloadEventHook() 
        {
            RemoveFocusEvents();
        }

    }
}
