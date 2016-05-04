//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Controls;
    using System.Windows.Automation.Peers;

    // <summary>
    // Standard ListBox.  However, it uses AutomatedListBoxItemAutomationPeer to represent
    // all items within it, which is our class and which allows us to return user-friendly
    // representation of all Cider structures exposed through automation.
    // </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class AutomatedListBox : ListBox 
    {

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new AutomatedListBoxAutomationPeer(this);
        }

        private class AutomatedListBoxAutomationPeer : ListBoxAutomationPeer 
        {
            public AutomatedListBoxAutomationPeer(ListBox owner)
                : base(owner) 
            {
            }

            protected override ItemAutomationPeer CreateItemAutomationPeer(object item) 
            {
                return new AutomatedListBoxItemAutomationPeer(item, this);
            }
        }
    }
}
