namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs

    #region Class ConnectorDragDropGlyph
    internal sealed class ConnectorDragDropGlyph : DesignerGlyph
    {
        private int connectorIndex = 0;
        private Point glyphPoint = Point.Empty;

        public ConnectorDragDropGlyph(int connectorIndex, Point connectorCenter)
        {
            this.connectorIndex = connectorIndex;

            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            this.glyphPoint = new Point(connectorCenter.X - ambientTheme.DropIndicatorSize.Width / 2, connectorCenter.Y - ambientTheme.DropIndicatorSize.Height / 2);
        }

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            return new Rectangle(this.glyphPoint, WorkflowTheme.CurrentTheme.AmbientTheme.DropIndicatorSize);
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.DropIndicatorImage, GetBounds(designer, activated), DesignerContentAlignment.Fill);
        }

        public override int Priority
        {
            get
            {
                return DesignerGlyph.ConnectorDragDropPriority;
            }
        }
    }
    #endregion

    #endregion
}

