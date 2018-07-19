namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs


    #region Class ConnectorSelectionGlyph
    //

    internal abstract class ConnectorSelectionGlyph : SelectionGlyph
    {
        protected int connectorIndex = 0;
        protected bool isPrimarySelectionGlyph = true;

        public ConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph)
        {
            this.connectorIndex = connectorIndex;
            this.isPrimarySelectionGlyph = isPrimarySelectionGlyph;
        }
    }
    #endregion


    #endregion
}

