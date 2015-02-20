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
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Diagnostics;

    #region Class ConditionalToolboxItem
    [Serializable]
    internal sealed class IfElseToolboxItem : ActivityToolboxItem
    {
        public IfElseToolboxItem(Type type)
            : base(type)
        {
        }
        private IfElseToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }
        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity conditionalActivity = new IfElseActivity();
            conditionalActivity.Activities.Add(new IfElseBranchActivity());
            conditionalActivity.Activities.Add(new IfElseBranchActivity());
            return (IComponent[])new IComponent[] { conditionalActivity };
        }
    }
    #endregion

    #region Class ConditionalDesigner
    [ActivityDesignerTheme(typeof(IfElseDesignerTheme))]
    internal sealed class IfElseDesigner : ParallelActivityDesigner
    {
        #region Properties and Methods
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is IfElseBranchActivity))
                    return false;
            }

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if ((ContainedDesigners.Count - activitiesToRemove.Count) < 1)
                return false;

            return true;
        }

        public override bool CanMoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if ((ContainedDesigners.Count - activitiesToMove.Count) < 1)
            {
                if (moveLocation != null && moveLocation.AssociatedDesigner != this)
                    return false;
            }

            return true;
        }

        protected override CompositeActivity OnCreateNewBranch()
        {
            return new IfElseBranchActivity();
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

            Rectangle diamondRectangle = Rectangle.Empty;
            diamondRectangle.Width = compositeDesignerTheme.ConnectorSize.Height - 2 * e.AmbientTheme.Margin.Height + 2;
            diamondRectangle.Height = diamondRectangle.Width;
            diamondRectangle.X = bounds.Left + bounds.Width / 2 - diamondRectangle.Width / 2;
            diamondRectangle.Y = bounds.Top + TitleHeight + (compositeDesignerTheme.ConnectorSize.Height * 3 / 2 - diamondRectangle.Height) / 2 + 1;

            using (GraphicsPath decisionDiamond = GetDiamondPath(diamondRectangle))
            {
                e.Graphics.FillPath(compositeDesignerTheme.ForegroundBrush, decisionDiamond);
                e.Graphics.DrawPath(compositeDesignerTheme.ForegroundPen, decisionDiamond);
            }

            diamondRectangle.Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 2 + (compositeDesignerTheme.ConnectorSize.Height * 3 / 2 - diamondRectangle.Height) / 2 + 1;
            using (GraphicsPath decisionDiamond = GetDiamondPath(diamondRectangle))
            {
                e.Graphics.FillPath(compositeDesignerTheme.ForegroundBrush, decisionDiamond);
                e.Graphics.DrawPath(compositeDesignerTheme.ForegroundPen, decisionDiamond);
            }
        }

        private GraphicsPath GetDiamondPath(Rectangle rectangle)
        {
            Point[] diamondPoints = 
            {
                new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top), 
                new Point(rectangle.Right - 1, rectangle.Top + rectangle.Height / 2),
                new Point(rectangle.Left + rectangle.Width / 2, rectangle.Bottom - 1),
                new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2),
                new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top)
            };

            GraphicsPath diamondPath = new GraphicsPath();
            diamondPath.AddLines(diamondPoints);
            diamondPath.CloseFigure();
            return diamondPath;
        }
        #endregion
    }
    #endregion

    #region IfElseDesignerTheme
    internal sealed class IfElseDesignerTheme : CompositeDesignerTheme
    {
        public IfElseDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x64, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
