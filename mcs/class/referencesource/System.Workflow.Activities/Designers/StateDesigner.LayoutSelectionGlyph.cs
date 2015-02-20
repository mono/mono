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
        /// <summary>
        /// Draws the selection retangle around the selected designer
        /// </summary>
        private class LayoutSelectionGlyph : SelectionGlyph
        {
            private Layout _layout;

            public LayoutSelectionGlyph(Layout layout)
            {
                if (layout == null)
                    throw new ArgumentNullException("layout");

                _layout = layout;
            }

            public override int Priority
            {
                get
                {
                    return DesignerGlyph.HighestPriority;
                }
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return true;
                }
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                if (designer == null)
                    throw new ArgumentNullException("designer");
                return _layout.Bounds;
            }
        }
    }
}
