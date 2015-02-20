//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation;

    class ExpressionTextBoxAutomationPeer : UIElementAutomationPeer
    {
        public ExpressionTextBoxAutomationPeer(ExpressionTextBox owner)
            : base(owner)
        { 
        }

        protected override string GetItemStatusCore()
        {
            ExpressionTextBox expressionTextBox = this.Owner as ExpressionTextBox;
            if ((expressionTextBox != null) && (expressionTextBox.Editor != null))
            {
                return expressionTextBox.Editor.ItemStatus;
            }
            else
            {
                return base.GetItemStatusCore();
            }
        }
    }
}
