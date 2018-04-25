namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;

    #region Class ReadonlyMessageFilter
    //All Coordinates passed in physical coordinate system
    //Some of the functions will have coordinates in screen coordinates ie ShowContextMenu
    internal sealed class ReadonlyMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Contructor/Destruction
        public ReadonlyMessageFilter()
        {
        }
        #endregion

        #region MessageFilter Overridables
        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseLeave()
        {
            return true;
        }

        protected override bool OnMouseWheel(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseCaptureChanged()
        {
            return true;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnDragLeave()
        {
            return true;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            return true;
        }

        protected override bool OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            return true;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnShowContextMenu(Point menuPoint)
        {
            return true;
        }
        #endregion
    }
    #endregion
}
