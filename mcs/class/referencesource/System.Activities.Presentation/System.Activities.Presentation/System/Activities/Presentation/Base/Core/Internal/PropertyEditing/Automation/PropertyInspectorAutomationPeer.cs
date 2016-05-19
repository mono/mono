//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Internal.Properties;
    using System.Windows.Input;

    // <summary>
    // AutomationPeer for PropertyInspector
    // </summary>
    internal class PropertyInspectorAutomationPeer : UIElementAutomationPeer 
    {

        private PropertyInspector _inspector;

        // Current list of children automation peers.
        private List<AutomationPeer> _children;

        public PropertyInspectorAutomationPeer(PropertyInspector owner)
            : base(owner) 
        {
            if (owner == null)
            {
                throw FxTrace.Exception.ArgumentNull("owner");
            }

            _inspector = owner;
        }

        // <summary>
        // Gets a list of AutomationPeers that contains the following:
        //     Type text box
        //     Name text box
        //     List of CategoryContainerAutomationPeers
        // </summary>
        // <returns></returns>
        protected override List<AutomationPeer> GetChildrenCore() 
        {
            // If children list is not null and contains AutomationPeer that implements IAutomationFocusChangedEventSource
            // Then, unhook the automation focus events before clearing the list 
            // Else, create a new one.
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
                _children.Clear();
            }
            else 
            {
                _children = new List<AutomationPeer>();
            }
            _children.Add(new TextBlockAutomationPeer(_inspector.SelectionTypeLabel));
            _children.Add(new UIElementAutomationPeer(_inspector.PropertyToolBar));
            _children.Add(new InfoTextBlockAutomationPeer(_inspector.UninitializedLabel));
            _children.Add(new InfoTextBlockAutomationPeer(_inspector.NoSearchResultsLabel));
            _children.Add(new CategoryListAutomationPeer(_inspector.CategoryList));

            return _children;
        }

        protected override Point GetClickablePointCore() 
        {
            // return a point that, when clicked, selects the grid without selecting
            // any of the rows
            return this.Owner.PointToScreen(new Point(10, 10));
        }

        protected override string GetHelpTextCore() 
        {
            return Resources.PropertyEditing_PropertyInspectorAutomationPeerHelp;
        }

        protected override string GetNameCore() 
        {
            return Resources.PropertyEditing_PropertyInspector;
        }

        protected override string GetClassNameCore() 
        {
            return typeof(PropertyInspector).Name;
        }

        // The following automation peers provide accessiblity support (Raise automation events on receiving keyboard focus)
        // This is necessary for ACC-TOOLS especially screen readers like JAWS. 
        // We cannot use the base AutomationPeers (like UIElementAutomationPeer) and *have* to derive from the respective types
        // since the actual implementation of the handler when the focus event differs from element to element.
        // So we cannot use single base class to achieve the desired goal. 
        // 


        private class InfoTextBlockAutomationPeer : TextBlockAutomationPeer, IAutomationFocusChangedEventSource 
        {

            private TextBlock _informationLabel;

            public InfoTextBlockAutomationPeer(TextBlock informationLabel)
                : base(informationLabel) 
            {
                if (informationLabel == null)
                {
                    throw FxTrace.Exception.ArgumentNull("informationLabel");
                }
                _informationLabel = informationLabel;
                _informationLabel.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus);
            }

            // <summary>
            // PreviewGotKeyboardFocus event to raise the "AutomationFocus" event.
            // </summary>
            // <param name="sender">TextBlock</param>
            // <param name="e">KeyboardFocusChangedEventArgs</param>
            void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) 
            {
                this.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
            }

            protected override AutomationPeer GetLabeledByCore() 
            {
                return new TextBlockAutomationPeer(_informationLabel);
            }

        // IAutomationFocusChangedEventSource Members

            public void UnloadEventHook() 
            {
                Owner.PreviewGotKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus);
            }

        }
    }
}
