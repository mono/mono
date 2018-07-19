namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    #region Class ConditionedDesigner
    [ActivityDesignerTheme(typeof(ConditionedDesignerTheme))]
    internal sealed class IfElseBranchDesigner : SequentialActivityDesigner
    {
        #region Properties and Methods
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivity");

            if (!(parentActivityDesigner.Activity is IfElseActivity))
                return false;

            return base.CanBeParentedTo(parentActivityDesigner);
        }
        #endregion
    }
    #endregion

    #region ConditionedDesignerTheme
    internal sealed class ConditionedDesignerTheme : CompositeDesignerTheme
    {
        public ConditionedDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x64, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
