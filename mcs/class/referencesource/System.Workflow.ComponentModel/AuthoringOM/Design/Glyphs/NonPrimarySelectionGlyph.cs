namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs


    #region Class NonPrimarySelectionGlyph
    internal sealed class NonPrimarySelectionGlyph : SelectionGlyph
    {
        private static NonPrimarySelectionGlyph defaultNonPrimarySelectionGlyph = null;

        internal static NonPrimarySelectionGlyph Default
        {
            get
            {
                if (defaultNonPrimarySelectionGlyph == null)
                    defaultNonPrimarySelectionGlyph = new NonPrimarySelectionGlyph();
                return defaultNonPrimarySelectionGlyph;
            }
        }

        public override bool IsPrimarySelection
        {
            get
            {
                return false;
            }
        }
    }
    #endregion


    #endregion
}

