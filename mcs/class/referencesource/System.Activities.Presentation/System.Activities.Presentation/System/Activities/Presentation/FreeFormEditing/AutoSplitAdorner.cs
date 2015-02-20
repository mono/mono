//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    internal sealed class AutoSplitAdorner : Adorner
    {
        private Rect adornedElementRect;
        private Pen pen = new Pen(Brushes.SteelBlue, 1);

        public AutoSplitAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            Size size = FreeFormPanel.GetChildSize(this.AdornedElement);
            this.adornedElementRect = new Rect(new Point(0, 0), size);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(null, this.pen, this.adornedElementRect);
        }
    }
}
