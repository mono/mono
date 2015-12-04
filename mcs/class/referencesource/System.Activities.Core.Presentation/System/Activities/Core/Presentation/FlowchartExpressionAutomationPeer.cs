//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;

    [Fx.Tag.XamlVisible(false)]
    class FlowchartExpressionAutomationPeer : UIElementAutomationPeer
    {
        const string ExpressionNotShown = "(null)";
        AutomationPeer wrappedAutomationPeer;

        public FlowchartExpressionAutomationPeer(FrameworkElement owner, AutomationPeer wrappedAutomationPeer)
            : base(owner)
        {
            this.wrappedAutomationPeer = wrappedAutomationPeer;
        }

        protected override string GetItemStatusCore()
        {
            Fx.Assert(this.Owner != null, "FlowchartExpressionAutomationPeer should have this.Owner != null.");
            bool expressionShown = false;
            if (this.Owner is FlowDecisionDesigner)
            {
                expressionShown = ((FlowDecisionDesigner)this.Owner).ExpressionShown;
            }
            else
            {
                Fx.Assert(this.Owner is FlowSwitchDesigner, "this.Owner should either be FlowDecisionDesigner or FlowSwitchDesigner.");
                expressionShown = ((FlowSwitchDesigner)this.Owner).ExpressionShown;
            }
            return expressionShown ? FlowchartExpressionAdorner.GetExpressionString(this.Owner) : ExpressionNotShown;
        }

        protected override string GetClassNameCore()
        {
            return this.wrappedAutomationPeer.GetClassName();
        }

        protected override string GetNameCore()
        {
            return this.wrappedAutomationPeer.GetName();
        }

        protected override string GetAutomationIdCore()
        {
            return this.wrappedAutomationPeer.GetAutomationId();
        }
    }
}
