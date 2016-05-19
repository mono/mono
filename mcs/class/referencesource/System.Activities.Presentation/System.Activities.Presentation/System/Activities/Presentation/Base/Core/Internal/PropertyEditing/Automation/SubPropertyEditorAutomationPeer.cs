//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Media;

    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using System.Activities.Presentation.Internal.Properties;

    // <summary>
    // AutomationPeer for the SubPropertyEditor
    // </summary>
    internal class SubPropertyEditorAutomationPeer : UIElementAutomationPeer, IExpandCollapseProvider 
    {

        private SubPropertyEditor _editor;
        List<AutomationPeer> _children;

        public SubPropertyEditorAutomationPeer(SubPropertyEditor editor)
            : base(editor) 
        {
            _editor = editor;

            // Hook into the VisualsChanged event so that this peer can invalidate
            // itself appropriately
            if (editor != null)
            {
                editor.VisualsChanged += new EventHandler(OnEditorVisualsChanged);
            }

            _children = new List<AutomationPeer>();

        }

        // <summary>
        // Gets the ExpandCollapse state of the sub-properties
        // </summary>
        public ExpandCollapseState ExpandCollapseState 
        {
            get {
                return _editor.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
            }
        }


        protected override string GetNameCore() 
        {
            return Resources.PropertyEditing_SubPropertyEditorAutomationName;
        }

        protected override string GetClassNameCore() 
        {
            return typeof(SubPropertyEditor).Name;
        }

        // Support the ExpandCollapse pattern
        public override object GetPattern(PatternInterface patternInterface) 
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override List<AutomationPeer> GetChildrenCore() 
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
                _children.Clear();
            }

            // See if we have access to the QuickTypes combo box (it may or may not be available)
            AutomatedChoiceEditor choiceEditor = VisualTreeUtils.GetNamedChild<AutomatedChoiceEditor>(_editor, "PART_ValueEditor");

            // If we do, present it as one of our children
            if (choiceEditor != null) 
            {
                _children.Add(new HiddenUIElementAutomationPeer(choiceEditor));
            }

            // Add any sub-properties
            ItemsControl properties = VisualTreeUtils.GetNamedChild<ItemsControl>(_editor, "PART_SubPropertyList");

            if (properties != null) 
            {
                int childCount = properties.Items.Count;

                for (int i = 0; i < childCount; i++) 
                {
                    PropertyContainer propertyContainer = properties.ItemContainerGenerator.ContainerFromIndex(i) as PropertyContainer;

                    if (propertyContainer != null) 
                    {
                        PropertyContainerAutomationPeer peer = new PropertyContainerAutomationPeer(propertyContainer);
                        _children.Add(peer);
                    }

                }
            }

            return _children;
        }

        private void OnEditorVisualsChanged(object sender, EventArgs e) 
        {
            this.InvalidatePeer();
        }

        // IExpandCollapseProvider Members

        // <summary>
        // Attempts to collapse the sub-properties
        // </summary>
        public void Collapse() 
        {
            if (_editor != null)
            {
                _editor.IsExpanded = false;
            }
        }

        // <summary>
        // Attempts to expand the sub-properties
        // </summary>
        public void Expand() 
        {
            if (_editor != null &&
                _editor.PropertyEntry != null &&
                _editor.PropertyEntry.PropertyValue.HasSubProperties == true)
            {
                _editor.IsExpanded = true;
            }
        }
    }
}
