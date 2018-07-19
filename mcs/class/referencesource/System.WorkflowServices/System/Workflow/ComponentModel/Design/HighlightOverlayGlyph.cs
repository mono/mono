//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;

    // <summary>
    // This class provides the visualisation of the backdrop + hightlighted designer when
    // in highlight view.
    // </summary>
    [ComVisible(false)]
    internal class HighlightOverlayGlyph : DesignerGlyph
    {
        private Rectangle bounds;
        private List<ActivityDesigner> highlightedDesigners;
        public HighlightOverlayGlyph(Rectangle bounds, List<ActivityDesigner> highlightedDesigners)
        {
            this.HighlightedDesigners = highlightedDesigners;
            this.Bounds = bounds;
        }

        public Rectangle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public List<ActivityDesigner> HighlightedDesigners
        {
            get { return highlightedDesigners; }
            set { highlightedDesigners = value; }
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            Rectangle frameRect = Bounds;
            Rectangle shadowRect = frameRect;

            Color BaseColor = Color.FromArgb(150, 0, 0, 0); // dark semitransparent backdrop 
            Color LightingColor = Color.FromArgb(150, 0, 0, 0);

            Brush frameBrush = new LinearGradientBrush(new Point(frameRect.Left, frameRect.Top), new Point(frameRect.Left, frameRect.Bottom), BaseColor, LightingColor);

            shadowRect = DropRoundedRectangleShadow(shadowRect, graphics);
            graphics.FillPath(frameBrush, RoundedRect(frameRect));
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            foreach (ActivityDesigner highlightedDesigner in HighlightedDesigners)
            {
                DesignerPainter.PaintDesigner(highlightedDesigner, new ActivityDesignerPaintEventArgs(graphics, designer.Bounds, designer.Bounds, null));
            }
        }

        private Rectangle DropRoundedRectangleShadow(Rectangle shadowRect, Graphics outputGraphics)
        {

            int shadowIntensity = 1;
            using (Pen shadowPen = new Pen(Color.FromArgb(shadowIntensity, 0, 0, 0)))
            {
                shadowPen.Width = 24;
                for (int i = 0; i < 12; i++)
                {
                    outputGraphics.DrawPath(shadowPen, RoundedRect(shadowRect));
                    shadowPen.Color = Color.FromArgb(shadowIntensity - 1, 0, 0, 0);
                    shadowIntensity += 2;
                    shadowPen.Width = shadowPen.Width - 2;;
                }

                return shadowRect;
            }
        }


        private GraphicsPath RoundedRect(Rectangle frame)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 1;
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(frame.Left, frame.Top, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = frame.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = frame.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = frame.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

}
