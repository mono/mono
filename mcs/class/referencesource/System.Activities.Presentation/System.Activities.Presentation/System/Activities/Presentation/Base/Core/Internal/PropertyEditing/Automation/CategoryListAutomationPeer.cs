//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Media;

    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation.Internal.Properties;

    // <summary>
    // AutomationPeer for CategoryList class
    // </summary>
    internal class CategoryListAutomationPeer : ItemsControlAutomationPeer, IAutomationFocusChangedEventSource 
    {

        private CategoryList _control;
        // Children list.
        private List<AutomationPeer> _children;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="owner">Contained CategoryList</param>
        public CategoryListAutomationPeer(CategoryList owner)
            : base(owner) 
        {
            if (owner == null)
            {
                throw FxTrace.Exception.ArgumentNull("owner");
            }

            _control = owner;
            _children = new List<AutomationPeer>();
        }

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item) 
        {

            ItemAutomationPeer peer = CategoryContainerAutomationPeer.CreateItemAutomationPeer(
                item as ModelCategoryEntry,
                _control.ItemContainerGenerator.ContainerFromItem(item) as CiderCategoryContainer,
                this);

            //Add each item to the children's list
            _children.Add(peer);

            return peer;
        }

        protected override string GetClassNameCore() 
        {
            return typeof(ItemsControl).Name;
        }

        protected override string GetNameCore() 
        {
            return Resources.PropertyEditing_CategoryList;
        }

        // We use the ItemStatus to convey read-only property selection status
        protected override string GetItemStatusCore() 
        {
            FrameworkElement selection = _control.Selection;
            string status;

            if (selection != null) 
            {
                ISelectionStop selectionStop = PropertySelection.GetSelectionStop(selection);
                status = selectionStop == null ? null : selectionStop.Description;

                if (status == null) 
                {
                    status = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.PropertyEditing_SelectionStatus_Unknown,
                        selection.GetType().Name);
                }
            }
            else 
            {
                status = Resources.PropertyEditing_SelectionStatus_Empty;
            }

            return status;
        }

        // IAutomationFocusChangedEventSource Members
        public void UnloadEventHook() 
        {
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
}
