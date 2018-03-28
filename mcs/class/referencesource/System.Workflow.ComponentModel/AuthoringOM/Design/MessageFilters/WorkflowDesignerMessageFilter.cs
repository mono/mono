namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.ComponentModel.Design;

    #region Class WorkflowDesignerMessageFilter
    //All Coordinates passed in physical coordinate system
    //Some of the functions will have coordinates in screen coordinates ie ShowContextMenu
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowDesignerMessageFilter : IDisposable, IWorkflowDesignerMessageSink
    {
        #region Members and Contructor/Destruction
        private WorkflowView parentView;

        protected WorkflowDesignerMessageFilter()
        {
        }

        ~WorkflowDesignerMessageFilter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Properties and Methods
        protected virtual void Initialize(WorkflowView parentView)
        {
            this.parentView = parentView;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected virtual bool OnMouseDown(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseMove(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseUp(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseHover(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseLeave()
        {
            return false;
        }

        protected virtual bool OnMouseWheel(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseCaptureChanged()
        {
            return false;
        }

        protected virtual bool OnDragEnter(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnDragOver(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnDragLeave()
        {
            return false;
        }

        protected virtual bool OnDragDrop(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnGiveFeedback(GiveFeedbackEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnQueryContinueDrag(QueryContinueDragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnKeyDown(KeyEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnKeyUp(KeyEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnScroll(ScrollBar sender, int value)
        {
            return false;
        }

        protected virtual bool OnShowContextMenu(Point screenMenuPoint)
        {
            return false;
        }

        protected virtual bool OnPaint(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            return false;
        }

        protected virtual bool OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            return false;
        }

        protected virtual bool ProcessMessage(Message message)
        {
            return false;
        }

        protected virtual void OnLayout(LayoutEventArgs eventArgs)
        {
        }

        protected virtual void OnThemeChange()
        {
        }

        protected WorkflowView ParentView
        {
            get
            {
                return this.parentView;
            }
        }

        protected HitTestInfo MessageHitTestContext
        {
            get
            {
                HitTestInfo hitInfo = ParentView.MessageHitTestContext;
                if (hitInfo == null)
                    hitInfo = HitTestInfo.Nowhere;

                return hitInfo;
            }
        }
        #endregion

        #region Private Methods
        internal object GetService(Type serviceType)
        {
            object service = null;
            if (this.parentView != null)
                service = ((IServiceProvider)this.parentView).GetService(serviceType);
            return service;
        }

        internal void SetParentView(WorkflowView parentView)
        {
            Initialize(parentView);
        }
        #endregion

        #region IWorkflowDesignerMessageSink Members
        bool IWorkflowDesignerMessageSink.OnMouseDown(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseDown(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseMove(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseMove(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseUp(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseUp(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseDoubleClick(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseEnter(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseEnter(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseHover(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseHover(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseLeave()
        {
            bool handled = false;
            try
            {
                handled = OnMouseLeave();
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseWheel(MouseEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnMouseWheel(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseCaptureChanged()
        {
            bool handled = false;
            try
            {
                handled = OnMouseCaptureChanged();
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragBegin(Point initialPoint, MouseEventArgs eventArgs)
        {
            //This message is not used in MessageFilters
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragMove(MouseEventArgs eventArgs)
        {
            //This message is not used in MessageFilters
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragEnd()
        {
            //This message is not used in MessageFilters
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnDragEnter(DragEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnDragEnter(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnDragOver(DragEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnDragOver(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnDragLeave()
        {
            bool handled = false;
            try
            {
                handled = OnDragLeave();
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnDragDrop(DragEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnDragDrop(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnGiveFeedback(GiveFeedbackEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnGiveFeedback(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnQueryContinueDrag(QueryContinueDragEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnQueryContinueDrag(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnKeyDown(KeyEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnKeyDown(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnKeyUp(KeyEventArgs eventArgs)
        {
            bool handled = false;
            try
            {
                handled = OnKeyUp(eventArgs);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnScroll(ScrollBar sender, int value)
        {
            bool handled = false;
            try
            {
                handled = OnScroll(sender, value);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnShowContextMenu(Point screenMenuPoint)
        {
            bool handled = false;
            try
            {
                handled = OnShowContextMenu(screenMenuPoint);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnPaint(PaintEventArgs eventArgs, Rectangle viewPort)
        {
            bool handled = false;
            try
            {
                handled = OnPaint(eventArgs, viewPort, WorkflowTheme.CurrentTheme.AmbientTheme);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort)
        {
            bool handled = false;
            try
            {
                handled = OnPaintWorkflowAdornments(eventArgs, viewPort, WorkflowTheme.CurrentTheme.AmbientTheme);
            }
            catch
            {
            }

            return handled;
        }

        bool IWorkflowDesignerMessageSink.ProcessMessage(Message message)
        {
            bool handled = false;
            try
            {
                handled = ProcessMessage(message);
            }
            catch
            {
            }

            return handled;
        }

        void IWorkflowDesignerMessageSink.OnLayout(LayoutEventArgs layoutEventArgs)
        {
            try
            {
                OnLayout(layoutEventArgs);
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnLayoutPosition(Graphics graphics)
        {
            //This message is not used in MessageFilters
        }

        void IWorkflowDesignerMessageSink.OnLayoutSize(Graphics graphics)
        {
            //This message is not used in MessageFilters
        }

        void IWorkflowDesignerMessageSink.OnThemeChange()
        {
            try
            {
                OnThemeChange();
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnBeginResizing(DesignerEdges sizingEdge)
        {
        }

        void IWorkflowDesignerMessageSink.OnResizing(DesignerEdges sizingEdge, Rectangle bounds)
        {
        }

        void IWorkflowDesignerMessageSink.OnEndResizing()
        {
        }
        #endregion
    }
    #endregion
}
