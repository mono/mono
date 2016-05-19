//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Automation 
{
    using System;
    using System.Globalization;
    using System.Windows.Automation.Peers;

    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    // <summary>
    // Cider-specific wrapper for ListBoxItemAutomationPeer that overrides GetNameCore()
    // and returns a user-friendly name for all Cider structures exposed through automation.
    // </summary>
    internal class AutomatedListBoxItemAutomationPeer : ListBoxItemAutomationPeer 
    {

        public AutomatedListBoxItemAutomationPeer(object item, SelectorAutomationPeer owner)
            : base(item, owner) 
        {
        }

        protected override string GetNameCore() 
        {
            return EditorUtilities.GetDisplayName(this.Item);
        }
    }
}
