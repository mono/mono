namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Design;

    #region Class ActivityDesignerGlyphCollection
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityDesignerGlyphCollection : List<DesignerGlyph>
    {
        public ActivityDesignerGlyphCollection()
        {
        }

        public ActivityDesignerGlyphCollection(IEnumerable<DesignerGlyph> glyphs)
            : base(glyphs)
        {
        }

        public ActivityDesignerGlyphCollection(ActivityDesignerGlyphCollection glyphs)
            : base(glyphs)
        {
        }

        internal DesignerGlyph this[Type type]
        {
            get
            {
                if (type == null)
                    throw new ArgumentNullException();

                DesignerGlyph glyph = null;
                foreach (DesignerGlyph designerGlyph in this)
                {
                    if (designerGlyph.GetType() == type)
                    {
                        glyph = designerGlyph;
                        break;
                    }
                    else if (type.IsAssignableFrom(designerGlyph.GetType()) && glyph == null)
                    {
                        glyph = designerGlyph;
                    }
                }

                return glyph;
            }
        }
    }
    #endregion

    #region Class DesignerGlyph
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class DesignerGlyph
    {
        public const int HighestPriority = 0;
        public const int NormalPriority = 10000;
        public const int LowestPriority = 1000000;

        internal const int ConnectionPointPriority = 1;
        internal const int MoveAnchorPriority = 1;
        internal const int ConfigErrorPriority = 2;
        internal const int ConnectorDragDropPriority = 2;
        internal const int FadeGlyphPriority = 3;
        internal const int LockedGlyphPriority = 3;
        internal const int ReadOnlyGlyphPriority = 3;
        internal const int CommentPriority = 3;
        internal const int SelectionPriority = 4;
        internal const int NonExecutionStatePriority = 5;

        public virtual bool CanBeActivated
        {
            get
            {
                return false;
            }
        }

        public virtual int Priority
        {
            get
            {
                return DesignerGlyph.NormalPriority;
            }
        }

        public virtual Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            return designer.Bounds;
        }

        protected abstract void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer);

        protected virtual void OnActivate(ActivityDesigner designer)
        {
        }

        internal void DrawActivated(Graphics graphics, ActivityDesigner designer)
        {
            OnPaint(graphics, true, WorkflowTheme.CurrentTheme.AmbientTheme, designer);
        }

        internal void Draw(Graphics graphics, ActivityDesigner designer)
        {
            OnPaint(graphics, false, WorkflowTheme.CurrentTheme.AmbientTheme, designer);
        }

        internal void Activate(ActivityDesigner designer)
        {
            OnActivate(designer);
        }

        internal static int OnComparePriority(DesignerGlyph x, DesignerGlyph y)
        {
            return (y.Priority - x.Priority);
        }
    }
    #endregion

    #region Class GlyphManager
    internal class GlyphManager : WorkflowDesignerMessageFilter, IDesignerGlyphProviderService
    {
        #region Members and Constructor
        // cache all the services so that in the dispose we properly clean up ourselves
        private List<IDesignerGlyphProvider> designerGlyphProviders = new List<IDesignerGlyphProvider>();

        // these two variables are only valid during MouseEnter and MouseLeave of a glyph
        private DesignerGlyph activeGlyph = null;
        private ActivityDesigner activeDesigner = null;

        internal GlyphManager()
        {
        }

        protected override void Dispose(bool disposing)
        {
            this.designerGlyphProviders.Clear();
            this.activeGlyph = null;
            this.activeDesigner = null;

            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                if (GetService(typeof(IDesignerGlyphProviderService)) != null)
                    serviceContainer.RemoveService(typeof(IDesignerGlyphProviderService));
            }

            base.Dispose(disposing);
        }
        #endregion

        #region WorkflowDesignerMessageFilter Methods
        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);

            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                if (GetService(typeof(IDesignerGlyphProviderService)) != null)
                    serviceContainer.RemoveService(typeof(IDesignerGlyphProviderService));
                serviceContainer.AddService(typeof(IDesignerGlyphProviderService), this);
            }
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (this.activeGlyph != null)
            {
                this.activeGlyph.Activate(this.activeDesigner);
                return true;
            }
            else
            {
                return false;
            }
        }

        //if there is an active glyph, handle the double click event as the single click event
        //to make sure we dont execute the default action in that case
        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            if (this.activeGlyph != null)
            {
                this.activeGlyph.Activate(this.activeDesigner);
                return true;
            }
            return false;
        }


        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            RefreshActiveGlyph(ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            RefreshActiveGlyph(ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            RefreshActiveGlyph(ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }
        #endregion

        #region IDesignerGlyphProviderService Implementation
        void IDesignerGlyphProviderService.AddGlyphProvider(IDesignerGlyphProvider glyphProvider)
        {
            if (!this.designerGlyphProviders.Contains(glyphProvider))
            {
                this.designerGlyphProviders.Add(glyphProvider);
                ParentView.InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        void IDesignerGlyphProviderService.RemoveGlyphProvider(IDesignerGlyphProvider glyphProvider)
        {
            this.designerGlyphProviders.Remove(glyphProvider);
            ParentView.InvalidateClientRectangle(Rectangle.Empty);
        }

        ReadOnlyCollection<IDesignerGlyphProvider> IDesignerGlyphProviderService.GlyphProviders
        {
            get
            {
                return this.designerGlyphProviders.AsReadOnly();
            }
        }
        #endregion

        #region Internal methods
        internal void DrawDesignerGlyphs(ActivityDesignerPaintEventArgs e, ActivityDesigner designer)
        {
            foreach (DesignerGlyph glyph in GetDesignerGlyphs(designer))
                glyph.Draw(e.Graphics, designer);

            if (this.activeGlyph != null && designer == this.activeDesigner)
                this.activeGlyph.DrawActivated(e.Graphics, this.activeDesigner);
        }

        internal ActivityDesignerGlyphCollection GetDesignerGlyphs(ActivityDesigner designer)
        {
            ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
            if (designer.Glyphs != null)
                glyphs.AddRange(designer.Glyphs);

            foreach (IDesignerGlyphProvider glyphProvider in this.designerGlyphProviders)
            {
                ActivityDesignerGlyphCollection extendedGlyphs = glyphProvider.GetGlyphs(designer);
                if (extendedGlyphs != null)
                    glyphs.AddRange(extendedGlyphs);
            }

            glyphs.Sort(new Comparison<DesignerGlyph>(DesignerGlyph.OnComparePriority));
            return glyphs;
        }
        #endregion

        #region Helper Methods
        private void RefreshActiveGlyph(Point point)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                DesignerGlyph previousActiveGlyph = this.activeGlyph;

                if (this.activeGlyph == null || !this.activeGlyph.GetBounds(this.activeDesigner, true).Contains(point))
                {
                    ActivityDesigner newActiveDesigner = null;
                    DesignerGlyph newActiveGlyph = GlyphFromPoint(point, out newActiveDesigner);

                    if (this.activeGlyph != null)
                        parentView.InvalidateLogicalRectangle(this.activeGlyph.GetBounds(this.activeDesigner, true));

                    this.activeGlyph = newActiveGlyph;
                    this.activeDesigner = newActiveDesigner;

                    if (this.activeGlyph != null)
                        parentView.InvalidateLogicalRectangle(this.activeGlyph.GetBounds(this.activeDesigner, true));
                }

                if (previousActiveGlyph != this.activeGlyph)
                {
                    if (this.activeGlyph != null && this.activeGlyph.CanBeActivated)
                        parentView.Cursor = Cursors.Hand;
                    else if (parentView.Cursor == Cursors.Hand)
                        parentView.Cursor = Cursors.Default;
                }
            }
        }

        private class RectangleCollection
        {
            private List<Rectangle> rectangles = new List<Rectangle>();

            internal void AddRectangle(Rectangle rectangle)
            {
                this.rectangles.Add(rectangle);
            }

            internal bool IsPointInsideAnyRectangle(Point p)
            {
                for (int i = 0; i < this.rectangles.Count; i++)
                {
                    if (this.rectangles[i].Contains(p))
                        return true;
                }
                return false;
            }
        }

        private DesignerGlyph GlyphFromPoint(Point point, out ActivityDesigner activityDesigner)
        {
            activityDesigner = null;

            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                RectangleCollection collection = new RectangleCollection();
                {
                    ActivityDesigner[] containedDesigners = GetActivityDesigners(parentView.ClientRectangleToLogical(new Rectangle(Point.Empty, parentView.ViewPortSize)));
                    foreach (ActivityDesigner designer in containedDesigners)
                    {
                        if (!collection.IsPointInsideAnyRectangle(point))
                        {
                            foreach (DesignerGlyph glyph in GetDesignerGlyphs(designer))
                            {
                                if (glyph.GetBounds(designer, false).Contains(point))
                                {
                                    if (glyph.CanBeActivated)
                                    {
                                        activityDesigner = designer;
                                        return glyph;
                                    }
                                }
                            }
                        }
                        collection.AddRectangle(designer.Bounds);
                    }
                }
            }

            return null;
        }

        //Please note that before changing this algorithm, you need to know that changing this algorithm
        //will affect the z order of the designers and will affect the way glyphs are drawn. 
        //Here what we are using depth first search algorithm to maintain the Z order.
        //Please note that even though one might think the algo might cause some inefficiency, the algo
        //has been timed for huge workflow and typically takes < 20ms to execute
        private ActivityDesigner[] GetActivityDesigners(Rectangle logicalViewPort)
        {
            //We need to go to the deepest point and then start drawing outwards
            List<ActivityDesigner> designerList = new List<ActivityDesigner>();
            bool viewPortEmpty = logicalViewPort.IsEmpty;

            ActivityDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(ParentView);
            if (rootDesigner != null)
            {
                Stack<object> designerStack = new Stack<object>();

                designerStack.Push(rootDesigner);
                CompositeActivityDesigner compositeDesigner = rootDesigner as CompositeActivityDesigner;
                if (compositeDesigner != null && compositeDesigner.ContainedDesigners.Count > 0)
                    designerStack.Push(compositeDesigner.ContainedDesigners);

                while (designerStack.Count > 0)
                {
                    object topOfStack = designerStack.Pop();
                    ICollection designers = topOfStack as ICollection;
                    if (designers != null)
                    {
                        foreach (ActivityDesigner activityDesigner in designers)
                        {
                            if ((viewPortEmpty || logicalViewPort.IntersectsWith(activityDesigner.Bounds)) && activityDesigner.IsVisible)
                            {
                                designerStack.Push(activityDesigner);
                                compositeDesigner = activityDesigner as CompositeActivityDesigner;
                                if (compositeDesigner != null && compositeDesigner.ContainedDesigners.Count > 0)
                                    designerStack.Push(compositeDesigner.ContainedDesigners);
                            }
                        }
                    }
                    else
                    {
                        //Draw glyphs for composite designers
                        designerList.Add((ActivityDesigner)topOfStack);
                    }
                }
            }

            return designerList.ToArray();
        }
        #endregion
    }
    #endregion
}
