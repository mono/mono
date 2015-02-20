namespace System.Workflow.ComponentModel
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Specialized;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    #region Class CompensateDesigner
    [ActivityDesignerTheme(typeof(CompensateDesignerTheme))]
    internal sealed class CompensateDesigner : ActivityDesigner
    {
        #region Properties and Methods
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            Activity parentActivity = parentActivityDesigner.Activity;
            while (parentActivity != null)
            {
                if (parentActivity is CancellationHandlerActivity || parentActivity is CompensationHandlerActivity || parentActivity is FaultHandlerActivity)
                    return true;

                parentActivity = parentActivity.Parent;
            }

            return false;
        }
        #endregion
    }
    #endregion

    #region CompensateDesignerTheme
    internal sealed class CompensateDesignerTheme : ActivityDesignerTheme
    {
        public CompensateDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x73, 0x51, 0x08);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xF7, 0xF7, 0x9C);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xDE, 0xAA, 0x00);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
    #endregion

}
