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
    using System.Workflow.Activities.Common;

    #region EventDrivenDesigner
    [ActivityDesignerTheme(typeof(EventDrivenDesignerTheme))]
    internal sealed class EventDrivenDesigner : SequenceDesigner
    {
        #region Properties and Methods
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivity");

            if (!(Type.GetType("System.Workflow.Activities.ListenActivity," + AssemblyRef.ActivitiesAssemblyRef).IsAssignableFrom(parentActivityDesigner.Activity.GetType())) &&
                !(parentActivityDesigner.Activity is EventHandlersActivity) &&
                !(Type.GetType("System.Workflow.Activities.StateActivity," + AssemblyRef.ActivitiesAssemblyRef).IsAssignableFrom(parentActivityDesigner.Activity.GetType())))
                return false;

            return base.CanBeParentedTo(parentActivityDesigner);
        }

        protected override void DoDefaultAction()
        {
            base.DoDefaultAction();
            EnsureVisible();
        }

        public override bool CanExpandCollapse
        {
            get
            {
                if (ParentDesigner is System.Workflow.Activities.StateDesigner)
                    return false;
                return base.CanExpandCollapse;
            }
        }

        #endregion
    }
    #endregion

    #region EventDrivenDesignerTheme
    internal sealed class EventDrivenDesignerTheme : CompositeDesignerTheme
    {
        public EventDrivenDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
