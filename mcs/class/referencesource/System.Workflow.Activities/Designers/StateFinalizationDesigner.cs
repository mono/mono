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
    using System.Collections.ObjectModel;

    #region StateFinalizationDesigner
    [ActivityDesignerTheme(typeof(StateFinalizationDesignerTheme))]
    internal sealed class StateFinalizationDesigner : System.Workflow.Activities.SequenceDesigner
    {
        #region Properties and Methods
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivityDesigner");

            if (!(parentActivityDesigner.Activity is StateActivity))
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
                return false;
            }
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity is IEventActivity)
                    return false;
            }

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        #endregion
    }
    #endregion

    #region StateFinalizationDesignerTheme
    internal sealed class StateFinalizationDesignerTheme : CompositeDesignerTheme
    {
        public StateFinalizationDesignerTheme(WorkflowTheme theme)
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
