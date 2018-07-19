//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;

    internal partial class GradientPanel : Panel
    {
        protected Rectangle frameRect;
        private Color baseColor;
        private Color borderColor;
        private bool dropShadow;
        int glossHeight;
        private bool glossy;
        private Color lightingColor;
        private int radius;

        public GradientPanel()
        {
            BaseColor = Color.FromArgb(255, 255, 255, 255);
            LightingColor = Color.FromArgb(255, 176, 186, 196);
            Radius = 7;
            this.DoubleBuffered = true;
        }

        public Color BaseColor
        {
            get { return baseColor; }
            set { baseColor = value; }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }

        public bool DropShadow
        {
            get { return dropShadow; }
            set { dropShadow = value; }
        }

        public bool Glossy
        {
            get { return glossy; }
            set { glossy = value; }
        }

        public Color LightingColor
        {
            get { return lightingColor; }
            set { lightingColor = value; }
        }

        public int Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.SuspendLayout();
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            if (DropShadow)
            {
                frameRect = new Rectangle(6, 0, this.Size.Width - 14, this.Size.Height - 8);
            }
            else
            {
                frameRect = new Rectangle(0, 0, this.Size.Width - 1, this.Size.Height - 1);
            }
            frameRect.X -= Margin.Left;
            frameRect.Y -= Margin.Top;
            frameRect.Width += Margin.Left + Margin.Right;
            frameRect.Height += Margin.Top + Margin.Bottom;
            Rectangle shadowRect = new Rectangle(frameRect.X, frameRect.Y + 6, frameRect.Width, frameRect.Height - 5);
            glossHeight = frameRect.Height / 3;
            Brush glossBrush = new LinearGradientBrush(new Point(frameRect.Left, frameRect.Top), new Point(frameRect.Left, frameRect.Top + glossHeight + 1), Color.FromArgb(120, 255, 255, 255), Color.FromArgb(60, 255, 255, 255)); // SolidBrush(Color.FromArgb(32, 255, 255, 255));
            Brush frameBrush = new LinearGradientBrush(new Point(frameRect.Left, frameRect.Top), new Point(frameRect.Left, frameRect.Bottom), BaseColor, LightingColor);
            Graphics outputGraphics = e.Graphics;
            if (DropShadow)
            {
                shadowRect = DropRoundedRectangleShadow(shadowRect, outputGraphics);
            }
            e.Graphics.FillPath(frameBrush, RoundedRect(frameRect));
            if (Glossy)
            {
                e.Graphics.FillPath(glossBrush, RoundedRectTopHalf(frameRect));
            }
            e.Graphics.DrawPath(new Pen(this.BorderColor), RoundedRect(frameRect));
            this.ResumeLayout();
        }

        private Rectangle DropRoundedRectangleShadow(Rectangle shadowRect, Graphics outputGraphics)
        {
            int shadowIntensity = 1;
            using (Pen shadowPen = new Pen(Color.FromArgb(shadowIntensity, 0, 0, 0)))
            {
                shadowPen.Width = 16;
                for (int i = 0; i < 8; i++)
                {
                    outputGraphics.DrawPath(shadowPen, RoundedRect(shadowRect));
                    shadowPen.Color = Color.FromArgb(shadowIntensity - 1, 0, 0, 0);
                    shadowIntensity += 8;
                    shadowPen.Width = shadowPen.Width - 2;;
                }
                return shadowRect;
            }
        }

        private GraphicsPath RoundedRect(Rectangle frame)
        {
            GraphicsPath path = new GraphicsPath();
            if (Radius < 1)
            {
                path.AddRectangle(frame);
                return path;
            }
            int diameter = Radius * 2;
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

        private GraphicsPath RoundedRectTopHalf(Rectangle frame)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Radius * 2;
            Rectangle arc = new Rectangle(frame.Left, frame.Top, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = frame.Right - diameter;
            path.AddArc(arc, 270, 90);
            path.AddLine(new Point(frame.Right, frame.Top + glossHeight), new Point(frame.Left, frame.Top + glossHeight));
            path.CloseFigure();
            return path;
        }

    }
}
