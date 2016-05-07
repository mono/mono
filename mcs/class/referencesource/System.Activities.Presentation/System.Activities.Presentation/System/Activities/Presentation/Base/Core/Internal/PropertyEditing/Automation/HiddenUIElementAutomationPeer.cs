//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation
{
    using System.Windows.Automation.Peers;
    using System.Windows;
    // Implementation of UIElementAutomationPeer that in and of itself does not show up on
    // a screen reader's radar, but that does expose any of its children that should
    class HiddenUIElementAutomationPeer : UIElementAutomationPeer
    {
        public HiddenUIElementAutomationPeer(UIElement owner)
            : base(owner)
        {
        }

        protected override bool IsControlElementCore()
        {
            return false;
        }
    }
}
