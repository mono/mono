namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs

    #region Class ConnectionPointGlyph
    internal sealed class ConnectionPointGlyph : DesignerGlyph
    {
        private ConnectionPoint connectionPoint; 

        internal ConnectionPointGlyph(ConnectionPoint connectionPoint)
        {
            this.connectionPoint = connectionPoint;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            if (designer.Activity != null && designer.Activity.Site != null && this.connectionPoint != null)
            {
                WorkflowView workflowView = designer.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                Rectangle viewPort = (workflowView != null) ? workflowView.ViewPortRectangle : Rectangle.Empty;
                Rectangle clipRectangle = (designer.ParentDesigner != null) ? designer.ParentDesigner.Bounds : designer.Bounds;
                ConnectionManager connectionManager = designer.Activity.Site.GetService(typeof(ConnectionManager)) as ConnectionManager;

                ActivityDesignerPaintEventArgs e = new ActivityDesignerPaintEventArgs(graphics, clipRectangle, viewPort, designer.DesignerTheme);
                bool drawHilited = (connectionManager != null && this.connectionPoint.Equals(connectionManager.SnappedConnectionPoint));
                this.connectionPoint.OnPaint(e, drawHilited);
            }
        }

        public override int Priority
        {
            get 
            {
                return DesignerGlyph.ConnectionPointPriority;
            }
        }
    }
    #endregion

    #endregion
}

