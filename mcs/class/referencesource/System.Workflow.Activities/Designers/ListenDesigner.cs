namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    #region Class ListenToolboxItem
    [Serializable]
    internal sealed class ListenToolboxItem : ActivityToolboxItem
    {
        public ListenToolboxItem(Type type)
            : base(type)
        {
        }
        private ListenToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }
        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity listenActivity = new ListenActivity();
            listenActivity.Activities.Add(new EventDrivenActivity());
            listenActivity.Activities.Add(new EventDrivenActivity());
            return (IComponent[])new IComponent[] { listenActivity };
        }
    }
    #endregion

    #region Class ListenDesigner
    [ActivityDesignerTheme(typeof(ListenDesignerTheme))]
    internal sealed class ListenDesigner : ParallelActivityDesigner
    {
        #region Properties and Methods
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is EventDrivenActivity))
                    return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override CompositeActivity OnCreateNewBranch()
        {
            return new EventDrivenActivity();
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            if (!Expanded || ContainedDesigners.Count == 0 || this != ActiveView.AssociatedDesigner)
                return;

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            Debug.Assert(compositeDesignerTheme != null);
            if (compositeDesignerTheme == null)
                return;

            //Draw the Icon and Text
            Rectangle bounds = Bounds;
            Rectangle imageRectangle = ImageRectangle;

            Rectangle roundRectangle = Rectangle.Empty;
            roundRectangle.Width = compositeDesignerTheme.ConnectorSize.Height - 2 * e.AmbientTheme.Margin.Height - 1;
            roundRectangle.Height = roundRectangle.Width;
            roundRectangle.X = bounds.Left + bounds.Width / 2 - roundRectangle.Width / 2;
            roundRectangle.Y = bounds.Top + TitleHeight + (compositeDesignerTheme.ConnectorSize.Height * 3 / 2 - roundRectangle.Height) / 2;

            e.Graphics.FillEllipse(compositeDesignerTheme.ForegroundBrush, roundRectangle);
            e.Graphics.DrawEllipse(compositeDesignerTheme.ForegroundPen, roundRectangle);

            roundRectangle.Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 2 + (compositeDesignerTheme.ConnectorSize.Height * 3 / 2 - roundRectangle.Height) / 2;
            e.Graphics.FillEllipse(compositeDesignerTheme.ForegroundBrush, roundRectangle);
            e.Graphics.DrawEllipse(compositeDesignerTheme.ForegroundPen, roundRectangle);
        }
        #endregion
    }
    #endregion

    #region ListenDesignerTheme
    internal sealed class ListenDesignerTheme : CompositeDesignerTheme
    {
        public ListenDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
