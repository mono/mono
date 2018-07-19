namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs

    #region Class ConfigErrorGlyph
    //Class is internal but not sealed as we dont expect the ActivityDesigner writers to supply their own
    //Glyph instead based on designer actions the smart tag will be shown
    //Exception: StripItemConfigErrorGlyph
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConfigErrorGlyph : DesignerGlyph
    {
        private static ConfigErrorGlyph defaultConfigErrorGlyph = null;

        internal static ConfigErrorGlyph Default
        {
            get
            {
                if (defaultConfigErrorGlyph == null)
                    defaultConfigErrorGlyph = new ConfigErrorGlyph();
                return defaultConfigErrorGlyph;
            }
        }

        public override bool CanBeActivated
        {
            get
            {
                return true;
            }
        }

        public override int Priority
        {
            get
            {
                return DesignerGlyph.ConfigErrorPriority;
            }
        }

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            Size configErrorSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
            Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
            Point configErrorLocation = new Point(designer.Bounds.Right - configErrorSize.Width - margin.Width / 2, designer.Bounds.Top - configErrorSize.Height + margin.Height);
            Rectangle bounds = new Rectangle(configErrorLocation, configErrorSize);

            if (activated)
            {
                bounds.Width *= 2;
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                bounds.Inflate(ambientTheme.Margin.Width / 2, ambientTheme.Margin.Height / 2);
            }
            
            return bounds;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            Rectangle bounds = GetBounds(designer, false);
            Rectangle activatedBounds = GetBounds(designer, activated);
            Region clipRegion = null;
            Region oldClipRegion = graphics.Clip;
            try
            {
                if (oldClipRegion != null)
                {
                    clipRegion = oldClipRegion.Clone();
                    if (activated)
                        clipRegion.Union(activatedBounds);
                    graphics.Clip = clipRegion;
                }

                if (activated)
                {
                    graphics.FillRectangle(SystemBrushes.ButtonFace, activatedBounds);
                    graphics.DrawRectangle(SystemPens.ControlDarkDark, activatedBounds.Left, activatedBounds.Top, activatedBounds.Width - 1, activatedBounds.Height - 1);

                    activatedBounds.X += bounds.Width + ambientTheme.Margin.Width;
                    activatedBounds.Width -= (bounds.Width + 2 * ambientTheme.Margin.Width);

                    using (GraphicsPath dropDownIndicator = ActivityDesignerPaint.GetScrollIndicatorPath(activatedBounds, ScrollButton.Down))
                    {
                        graphics.FillPath(SystemBrushes.ControlText, dropDownIndicator);
                        graphics.DrawPath(SystemPens.ControlText, dropDownIndicator);
                    }
                }

                ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.ConfigErrorImage, bounds, DesignerContentAlignment.Fill);
            }
            finally
            {
                if (clipRegion != null)
                {
                    graphics.Clip = oldClipRegion;
                    clipRegion.Dispose();
                }
            }
        }

        protected override void OnActivate(ActivityDesigner designer)
        {
            if (designer != null)
            {
                if (designer.DesignerActions.Count > 0)
                {
                    Rectangle bounds = GetBounds(designer, false);
                    Point location = designer.ParentView.LogicalPointToScreen(new Point(bounds.Left, bounds.Bottom));
                    DesignerHelpers.ShowDesignerVerbs(designer, location, DesignerHelpers.GetDesignerActionVerbs(designer, designer.DesignerActions));
                }
            }
        }
    }
    #endregion

    #endregion
}

