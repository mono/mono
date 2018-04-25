namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs

    #region Class ShadowGlyph
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ShadowGlyph : DesignerGlyph
    {
        private static ShadowGlyph defaultShadowGlyph = null;

        internal static ShadowGlyph Default
        {
            get
            {
                if (defaultShadowGlyph == null)
                    defaultShadowGlyph = new ShadowGlyph();
                return defaultShadowGlyph;
            }
        }

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            Rectangle bounds = designer.Bounds;
            bounds.Inflate(AmbientTheme.DropShadowWidth + 1, AmbientTheme.DropShadowWidth + 1);
            return bounds;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            Rectangle bounds = GetBounds(designer, activated);
            if (!bounds.Size.IsEmpty)
            {
                bool drawRounded = (designer.DesignerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle && !designer.IsRootDesigner);
                ActivityDesignerPaint.DrawDropShadow(graphics, designer.Bounds, designer.DesignerTheme.BorderPen.Color, AmbientTheme.DropShadowWidth, LightSourcePosition.Left | LightSourcePosition.Top, 0.5f, drawRounded);
            }
        }

        public override int Priority
        {
            get
            {
                return DesignerGlyph.LowestPriority;
            }
        }
    }
    #endregion

    #endregion
}

