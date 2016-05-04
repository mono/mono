//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    internal partial class ConnectorWithStartDot : Connector
    {
        public ConnectorWithStartDot()
        {
            this.InitializeComponent();
        }

        public override FrameworkElement StartDot
        {
            get
            {
                return this.startDotGrid;
            }
        }

        public override void SetLabelToolTip(object toolTip)
        {
            this.labelTextBlock.ToolTip = toolTip;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (!this.IsMouseOnStartDot(e))
            {
                base.OnDragEnter(e);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            System.Windows.Controls.Panel.SetZIndex(this, 999);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            System.Windows.Controls.Panel.SetZIndex(this, 1);
            base.OnMouseLeave(e);
        }

        private bool IsMouseOnStartDot(DragEventArgs e)
        {
            HitTestResult result = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (result != null && this.startDotGrid.IsAncestorOf(result.VisualHit))
            {
                return true;
            }

            return false;
        }
    }
}
