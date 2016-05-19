namespace System.Workflow.Activities
{
    using System;
    using System.Diagnostics;
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
    using System.Runtime.Serialization;

    #region Class WhileDesigner
    [ActivityDesignerTheme(typeof(WhileDesignerTheme))]
    internal sealed class WhileDesigner : SequentialActivityDesigner
    {
        #region Properties and Methods
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            HelpText = DR.GetString(DR.DropActivityHere);
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            if (Expanded)
            {
                CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
                Debug.Assert(compositeDesignerTheme != null);
                if (compositeDesignerTheme == null)
                    return;

                Rectangle bounds = Bounds;
                Rectangle textRectangle = TextRectangle;
                Rectangle imageRectangle = ImageRectangle;

                Point connectionPoint = Point.Empty;
                if (!imageRectangle.IsEmpty)
                    connectionPoint = new Point(imageRectangle.Right + e.AmbientTheme.Margin.Width / 2, imageRectangle.Top + imageRectangle.Height / 2);
                else if (!textRectangle.IsEmpty)
                    connectionPoint = new Point(textRectangle.Right + e.AmbientTheme.Margin.Width / 2, textRectangle.Top + textRectangle.Height / 2);
                else
                    connectionPoint = new Point(bounds.Left + bounds.Width / 2 + e.AmbientTheme.Margin.Width / 2, bounds.Top + e.AmbientTheme.Margin.Height / 2);

                Point[] points = new Point[4];
                points[0].X = bounds.Left + bounds.Width / 2;
                points[0].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height / 3;
                points[1].X = bounds.Right - compositeDesignerTheme.ConnectorSize.Width / 3;
                points[1].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height / 3;
                points[2].X = bounds.Right - compositeDesignerTheme.ConnectorSize.Width / 3;
                points[2].Y = connectionPoint.Y;
                points[3].X = connectionPoint.X;
                points[3].Y = connectionPoint.Y;

                DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.ArrowAnchor);
                DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, new Point[] { points[0], new Point(bounds.Left + bounds.Width / 2, bounds.Bottom) }, LineAnchor.None, LineAnchor.None);
            }
        }

        protected override Rectangle[] GetConnectors()
        {
            Rectangle[] connectors = base.GetConnectors();

            CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
            Debug.Assert(designerTheme != null);
            if (Expanded && connectors.GetLength(0) > 0)
                connectors[connectors.GetLength(0) - 1].Height = connectors[connectors.GetLength(0) - 1].Height - (((designerTheme != null) ? designerTheme.ConnectorSize.Height : 0) / 3);

            return connectors;
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size containerSize = base.OnLayoutSize(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme != null && Expanded)
            {
                containerSize.Width += 2 * compositeDesignerTheme.ConnectorSize.Width;
                containerSize.Height += compositeDesignerTheme.ConnectorSize.Height;
            }

            return containerSize;
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            //we only allow one activity to be inserted
            if (this == ActiveView.AssociatedDesigner && ContainedDesigners.Count > 0)
                return false;

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }
        #endregion
    }
    #endregion

    #region WhileDesignerTheme
    internal sealed class WhileDesignerTheme : CompositeDesignerTheme
    {
        public WhileDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x52, 0x8A, 0xF7);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
