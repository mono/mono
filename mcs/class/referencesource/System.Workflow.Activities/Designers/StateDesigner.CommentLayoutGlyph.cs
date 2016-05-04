namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    internal partial class StateDesigner : FreeformActivityDesigner
    {
        private class CommentLayoutGlyph : DesignerGlyph
        {
            private Layout _layout;

            public CommentLayoutGlyph(Layout layout)
            {
                if (layout == null)
                    throw new ArgumentNullException("layout");

                _layout = layout;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                if (designer == null)
                    throw new ArgumentNullException("designer");

                Rectangle bounds = _layout.Bounds;
                return bounds;
            }

            public override int Priority
            {
                get
                {
                    return DesignerGlyph.NormalPriority;
                }
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                if (designer == null)
                    throw new ArgumentNullException("designer");
                if (graphics == null)
                    throw new ArgumentNullException("graphics");
                Rectangle bounds = GetBounds(designer, false);
                graphics.FillRectangle(StateMachineDesignerPaint.FadeBrush, bounds);
                graphics.FillRectangle(ambientTheme.CommentIndicatorBrush, bounds);
                graphics.DrawRectangle(ambientTheme.CommentIndicatorPen, bounds);
            }
        }
    }
}
