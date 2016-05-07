//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;

    // <summary>
    // Standard ComboBox.  However, it uses AutomatedListBoxItemAutomationPeer to represent
    // all items within it, which is our class and which allows us to return user-friendly
    // representation of all Cider structures exposed through automation.
    // </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class AutomatedComboBox : ComboBox 
    {

        protected override AutomationPeer OnCreateAutomationPeer() 
        {
            return new AutomatedComboBoxAutomationPeer(this);
        }

        private class AutomatedComboBoxAutomationPeer : ComboBoxAutomationPeer 
        {
            public AutomatedComboBoxAutomationPeer(AutomatedComboBox owner)
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
