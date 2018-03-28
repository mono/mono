namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    #region Glyphs


    #region Class SelectionGlyph
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class SelectionGlyph : DesignerGlyph
    {
        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            Rectangle rectangle = designer.Bounds;
            rectangle.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize.Width / 2, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize.Height / 2);
            return rectangle;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            ActivityDesignerPaint.DrawSelection(graphics, GetBounds(designer, activated), IsPrimarySelection, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize, GetGrabHandles(designer));
        }

        public override int Priority
        {
            get
            {
                return DesignerGlyph.SelectionPriority;
            }
        }

        public abstract bool IsPrimarySelection { get; }

        public virtual Rectangle[] GetGrabHandles(ActivityDesigner designer) 
        {
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            Size grabHandleSize = new Size(selectionSize.Width, selectionSize.Height);
            Rectangle selectionRect = GetBounds(designer, false);
            selectionRect.Inflate(selectionSize.Width, selectionSize.Height);

            //we need grab handles only in case this activity is an immediate child of a free-form activity
            //otherwise, no grab handles

            ActivityDesigner parentDesigner = designer.ParentDesigner;
            Rectangle[] grabHandles = null;
            if (parentDesigner != null && parentDesigner is FreeformActivityDesigner)
            {
                grabHandles = new Rectangle[8];
                grabHandles[0] = new Rectangle(selectionRect.Location, grabHandleSize);
                grabHandles[1] = new Rectangle(new Point(selectionRect.Left + (selectionRect.Width - grabHandleSize.Width) / 2, selectionRect.Top), grabHandleSize);
                grabHandles[2] = new Rectangle(selectionRect.Right - grabHandleSize.Width, selectionRect.Top, grabHandleSize.Width, grabHandleSize.Height);
                grabHandles[3] = new Rectangle(new Point(selectionRect.Right - grabHandleSize.Width, selectionRect.Top + (selectionRect.Height - grabHandleSize.Height) / 2), grabHandleSize);
                grabHandles[4] = new Rectangle(selectionRect.Right - grabHandleSize.Width, selectionRect.Bottom - grabHandleSize.Height, grabHandleSize.Width, grabHandleSize.Height);
                grabHandles[5] = new Rectangle(new Point(selectionRect.Left + (selectionRect.Width - grabHandleSize.Width) / 2, selectionRect.Bottom - grabHandleSize.Height), grabHandleSize);
                grabHandles[6] = new Rectangle(selectionRect.Left, selectionRect.Bottom - grabHandleSize.Height, grabHandleSize.Width, grabHandleSize.Height);
                grabHandles[7] = new Rectangle(new Point(selectionRect.Left, selectionRect.Top + (selectionRect.Height - grabHandleSize.Height) / 2), grabHandleSize);
                return grabHandles;
            }
            else
            {
                grabHandles = new Rectangle[1];
                grabHandles[0] = new Rectangle(selectionRect.Location, grabHandleSize);
            }

            return grabHandles;            
        }
    }
    #endregion


    #endregion
}

