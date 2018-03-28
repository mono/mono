namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs


    #region Class PrimarySelectionGlyph
    internal sealed class PrimarySelectionGlyph : SelectionGlyph
    {
        private static PrimarySelectionGlyph defaultPrimarySelectionGlyph = null;

        internal static PrimarySelectionGlyph Default
        {
            get
            {
                if (defaultPrimarySelectionGlyph == null)
                    defaultPrimarySelectionGlyph = new PrimarySelectionGlyph();
                return defaultPrimarySelectionGlyph;
            }
        }

        public override bool IsPrimarySelection
        {
            get
            {
                return true;
            }
        }
    }
    #endregion


    #endregion
}

