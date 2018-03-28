#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //


    #region SequentialActivityDesigner Class
    /// <summary>
    /// Base class used for all the designers which have sequential vertical layout.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequentialActivityDesigner : StructuredCompositeActivityDesigner
    {
        #region Constants
        private static readonly Size DefaultHelpTextSize = new Size(100, 85);
        #endregion

        #region Fields
        private SequenceDesignerAccessibleObject accessibilityObject;
        private string helpText = String.Empty;
        private Size helpTextSize = Size.Empty;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SequentialActivityDesigner()
        {
        }
        #endregion

        #region Properties

        #region Public Properties
        public override bool Expanded
        {
            get
            {
                if (ParentDesigner is ParallelActivityDesigner)
                    return true;
                else
                    return base.Expanded;
            }

            set
            {
                if (ParentDesigner is ParallelActivityDesigner)
                    value = true;
                base.Expanded = value;
            }
        }


        public override bool CanExpandCollapse
        {
            get
            {
                if (ParentDesigner is ParallelActivityDesigner)
                    return false;
                else
                    return base.CanExpandCollapse;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (ActiveDesigner != this)
                    return base.FirstSelectableObject;

                if (GetConnectors().Length == 0 || !Expanded || !IsVisible)
                    return null;

                // This assumes there is always atleast one connector in SequenceContainer
                return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (ActiveDesigner != this)
                    return base.LastSelectableObject;

                Rectangle[] connectors = GetConnectors();
                if (connectors.Length == 0 || !Expanded || !IsVisible)
                    return null;

                // This assumes there is always atleast one connector in SequenceContainer
                return new ConnectorHitTestInfo(this, HitTestLocations.Designer, connectors.Length - 1);
            }
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new SequenceDesignerAccessibleObject(this);
                return this.accessibilityObject;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the help text to be displayed when the are no contained designers to display
        /// </summary>
        protected virtual string HelpText
        {
            get
            {
                return this.helpText;
            }

            set
            {
                this.helpText = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets the Rectangle bounding the help text.
        /// </summary>
        protected virtual Rectangle HelpTextRectangle
        {
            get
            {
                Rectangle[] connectors = GetConnectors();
                if (this.HelpText.Length == 0 || ContainedDesigners.Count > 0 || !Expanded || connectors.Length == 0)
                    return Rectangle.Empty;

                //Get the first connector and align the help text right in the center
                Rectangle helpTextRectangle = Rectangle.Empty;
                helpTextRectangle.X = connectors[0].Left + connectors[0].Width / 2 - this.helpTextSize.Width / 2;
                helpTextRectangle.Y = connectors[0].Top + connectors[0].Height / 2 - this.helpTextSize.Height / 2;
                helpTextRectangle.Size = this.helpTextSize;
                return helpTextRectangle;
            }
        }

        /// <summary>
        /// Gets the array of rectangles bounding the connectors
        /// </summary>
        protected internal virtual Rectangle[] GetConnectors()
        {
            if (ActiveDesigner != this)
                return new Rectangle[] { };

            if (!Expanded)
                return new Rectangle[] { };

            CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
            if (designerTheme == null)
                return new Rectangle[] { };

            //Calculate no of connectors for the container; if there is no activityDesigner then we display single connector
            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
            int connectorCount = (containedDesigners.Count > 0) ? (2 + (containedDesigners.Count - 1)) : 1;
            Rectangle[] connectorBounds = new Rectangle[connectorCount];

            ReadOnlyCollection<Point> containerConnections = GetInnerConnections(DesignerEdges.Top | DesignerEdges.Bottom);
            Point containerStartConnection = new Point();
            Point containerEndConnection = new Point();
            if (containerConnections != null && containerConnections.Count > 0)
            {
                containerStartConnection = containerConnections[0];
                containerEndConnection = containerConnections[containerConnections.Count - 1];
            }

            if (containedDesigners.Count > 0)
            {
                //Add the first connector. This will be from bitmap bottom to activityDesigner start
                ActivityDesigner startDesigner = containedDesigners[0] as ActivityDesigner;
                ReadOnlyCollection<Point> startDesignerConnections = startDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                if (startDesignerConnections.Count == 0)
                {
                    Rectangle startDesignerBounds = startDesigner.Bounds;
                    List<Point> connections = new List<Point>();
                    connections.Add(new Point(startDesignerBounds.Left + startDesignerBounds.Width / 2, startDesignerBounds.Top));
                    connections.Add(new Point(startDesignerBounds.Left + startDesignerBounds.Width / 2, startDesignerBounds.Bottom));
                    startDesignerConnections = connections.AsReadOnly();
                }

                connectorBounds[0].Location = new Point(containerStartConnection.X - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2, containerStartConnection.Y);
                connectorBounds[0].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), startDesignerConnections[0].Y - containerStartConnection.Y);

                //Add rest of the points
                for (int i = 0; i < containedDesigners.Count - 1; i++)
                {
                    ActivityDesigner designerFrom = containedDesigners[i];
                    ActivityDesigner designerTo = containedDesigners[i + 1];
                    if (designerFrom != null && designerTo != null)
                    {
                        ReadOnlyCollection<Point> designerFromConnections = designerFrom.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                        int designerFromPointCount = designerFromConnections.Count;
                        ReadOnlyCollection<Point> designerToConnections = designerTo.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                        connectorBounds[i + 1].Location = new Point(designerFromConnections[designerFromPointCount - 1].X - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2, designerFromConnections[designerFromPointCount - 1].Y);
                        connectorBounds[i + 1].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), designerToConnections[0].Y - designerFromConnections[designerFromPointCount - 1].Y);
                    }
                }

                //Add the last connector
                ActivityDesigner endDesigner = containedDesigners[containedDesigners.Count - 1];
                ReadOnlyCollection<Point> endDesignerConnections = endDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                if (endDesignerConnections.Count == 0)
                {
                    Rectangle endDesignerBounds = endDesigner.Bounds;
                    List<Point> connections = new List<Point>();
                    connections.Add(new Point(endDesignerBounds.Left + endDesignerBounds.Width / 2, endDesignerBounds.Top));
                    connections.Add(new Point(endDesignerBounds.Left + endDesignerBounds.Width / 2, endDesignerBounds.Bottom));
                    endDesignerConnections = connections.AsReadOnly();
                }

                connectorBounds[connectorCount - 1].Location = new Point(endDesignerConnections[endDesignerConnections.Count - 1].X - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2, endDesignerConnections[endDesignerConnections.Count - 1].Y);
                connectorBounds[connectorCount - 1].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), containerEndConnection.Y - endDesignerConnections[endDesignerConnections.Count - 1].Y);
            }
            else
            {
                //If there are no activity designers in the container then we show only one connector
                connectorBounds[0].Location = new Point(containerStartConnection.X - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2, containerStartConnection.Y);
                connectorBounds[0].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), containerEndConnection.Y - containerStartConnection.Y);
            }

            //extend the targets a little bit
            for (int i = 0; i < connectorBounds.Length; i++)
                connectorBounds[i].Inflate(3 * connectorBounds[i].Width, 0);

            return connectorBounds;
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection designerGlyphs = new ActivityDesignerGlyphCollection();
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                {
                    ICollection selectedObjects = selectionService.GetSelectedComponents();
                    foreach (object obj in selectedObjects)
                    {
                        ConnectorHitTestInfo connectorHitTestInfo = obj as ConnectorHitTestInfo;
                        if (connectorHitTestInfo != null && connectorHitTestInfo.AssociatedDesigner == this)
                            designerGlyphs.Add(new SequentialConnectorSelectionGlyph(connectorHitTestInfo.MapToIndex(), (selectionService.PrimarySelection == obj)));
                    }
                }

                designerGlyphs.AddRange(CreateConnectorDragDropGlyphs());
                designerGlyphs.AddRange(base.Glyphs);
                return designerGlyphs;
            }
        }

        protected Size HelpTextSize
        {
            get
            {
                return this.helpTextSize;
            }
        }
        #endregion

        #region Private Properties
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public override HitTestInfo HitTest(Point point)
        {
            if (ActiveDesigner != this)
                return base.HitTest(point);

            HitTestInfo hitInfo = HitTestInfo.Nowhere;
            if (!Expanded)
            {
                hitInfo = base.HitTest(point);
            }
            else if (ContainedDesigners.Count == 0 && HelpTextRectangle.Contains(point))
            {
                hitInfo = new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
            }
            else
            {
                //Check if the hit is on any of the connectors
                Rectangle[] connectors = GetConnectors();
                for (int i = 0; i < connectors.Length; i++)
                {
                    if (connectors[i].Contains(point))
                    {
                        hitInfo = new ConnectorHitTestInfo(this, HitTestLocations.Designer, i);
                        break;
                    }
                }

                if (hitInfo.HitLocation == HitTestLocations.None)
                    hitInfo = base.HitTest(point);
            }

            return hitInfo;
        }

        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (ActiveDesigner != this)
                return base.GetNextSelectableObject(obj, direction);

            if (direction != DesignerNavigationDirection.Down && direction != DesignerNavigationDirection.Up)
                return null;

            object nextObject = null;
            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
            if (direction == DesignerNavigationDirection.Down)
            {
                if (obj is ConnectorHitTestInfo)
                {
                    int currentIndex = ((ConnectorHitTestInfo)obj).MapToIndex();
                    if (currentIndex >= 0 && currentIndex < containedDesigners.Count)
                        nextObject = ((ActivityDesigner)containedDesigners[currentIndex]).Activity;
                }
                else if (obj is Activity)
                {
                    ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(obj as Activity);
                    int currentIndex = (activityDesigner != null) ? containedDesigners.IndexOf(activityDesigner) : -1;
                    if (currentIndex >= 0 && (currentIndex + 1) < GetConnectors().Length)
                        nextObject = new ConnectorHitTestInfo(this, HitTestLocations.Designer, currentIndex + 1);
                }
            }
            else if (direction == DesignerNavigationDirection.Up)
            {
                if (obj is ConnectorHitTestInfo)
                {
                    int currentIndex = ((ConnectorHitTestInfo)obj).MapToIndex();
                    if (currentIndex > 0 && currentIndex < GetConnectors().Length)
                        nextObject = ((ActivityDesigner)containedDesigners[currentIndex - 1]).Activity;
                }
                else if (obj is Activity)
                {
                    ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(obj as Activity);
                    int currentIndex = (activityDesigner != null) ? containedDesigners.IndexOf(activityDesigner) : -1;
                    if (currentIndex >= 0 && currentIndex < GetConnectors().Length)
                        nextObject = new ConnectorHitTestInfo(this, HitTestLocations.Designer, currentIndex);
                }
            }

            return nextObject;
        }
        #endregion

        #region Protected Methods
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            HelpText = DR.GetString(DR.DropActivitiesHere);
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if (HelpTextRectangle.Contains(dropPoint))
                return new Rectangle[] { HelpTextRectangle };
            else
                return GetConnectors();
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (Expanded && compositeDesignerTheme != null)
            {
                //Draw the connectors
                Rectangle helpTextRectangle = HelpTextRectangle;
                if (CurrentDropTarget == -1 && !helpTextRectangle.Size.IsEmpty)
                {
                    Rectangle[] connectors = GetConnectors();
                    if (connectors.Length > 0)
                    {
                        DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, new Point[] { new Point(connectors[0].X + connectors[0].Width / 2, connectors[0].Y + 2), new Point(connectors[0].X + connectors[0].Width / 2, helpTextRectangle.Top - 2) }, compositeDesignerTheme.ConnectorStartCap, LineAnchor.None);
                        DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, new Point[] { new Point(connectors[0].X + connectors[0].Width / 2, helpTextRectangle.Bottom + 2), new Point(connectors[0].X + connectors[0].Width / 2, connectors[0].Bottom - 2) }, LineAnchor.None, compositeDesignerTheme.ConnectorEndCap);
                    }

                    ActivityDesignerPaint.DrawText(e.Graphics, compositeDesignerTheme.Font, this.HelpText, helpTextRectangle, StringAlignment.Center, e.AmbientTheme.TextQuality, compositeDesignerTheme.ForegroundBrush);
                }
                else
                {
                    Rectangle[] connectors = GetConnectors();
                    for (int i = 0; i < connectors.Length; i++)
                    {
                        Pen pen = (i == CurrentDropTarget) ? e.AmbientTheme.DropIndicatorPen : compositeDesignerTheme.ForegroundPen;
                        LineAnchor startCap = ((i == 0 && connectors.Length > 2) || i == connectors.Length - 1) ? LineAnchor.None : compositeDesignerTheme.ConnectorStartCap;
                        LineAnchor endCap = (i == 0 || (i == connectors.Length - 1 && connectors.Length > 2)) ? LineAnchor.None : compositeDesignerTheme.ConnectorEndCap;
                        DrawConnectors(e.Graphics, pen, new Point[] { new Point(connectors[i].Left + connectors[i].Width / 2, connectors[i].Top + 2), new Point(connectors[i].Left + connectors[i].Width / 2, connectors[i].Bottom - 2) }, startCap, endCap);
                    }
                }
            }
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            base.OnLayoutPosition(e);

            if (Expanded && ActiveDesigner == this)
            {
                CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;

                //This condition is to center align the children
                int height = 0;

                ReadOnlyCollection<Point> connectionPoints = GetInnerConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                Point location = (connectionPoints.Count > 0) ? connectionPoints[0] : Location;

                if (ContainedDesigners.Count == 1)
                {
                    int heightDelta = 0;
                    if (connectionPoints.Count > 0)
                        heightDelta = Size.Height - (connectionPoints[connectionPoints.Count - 1].Y - connectionPoints[0].Y);
                    height += (Size.Height - heightDelta) / 2 - ContainedDesigners[0].Size.Height / 2;
                }
                else
                {
                    height += ((compositeDesignerTheme != null) ? compositeDesignerTheme.ConnectorSize.Height : 0);
                }

                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                {
                    Size designerSize = activityDesigner.Size;
                    activityDesigner.Location = new Point(location.X - (designerSize.Width / 2), location.Y + height);
                    height += designerSize.Height + ((compositeDesignerTheme != null) ? compositeDesignerTheme.ConnectorSize.Height : 0);
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size containerSize = base.OnLayoutSize(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (Expanded && ActiveDesigner == this && compositeDesignerTheme != null)
            {
                if (this.HelpText.Length > 0)
                    this.helpTextSize = ActivityDesignerPaint.MeasureString(e.Graphics, compositeDesignerTheme.Font, this.HelpText, StringAlignment.Center, SequentialActivityDesigner.DefaultHelpTextSize);

                //Calculate the size based on child size
                containerSize.Height += compositeDesignerTheme.ConnectorSize.Height; //Add the height of first connector

                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                {
                    Size childSize = activityDesigner.Size;
                    containerSize.Width = Math.Max(containerSize.Width, childSize.Width);
                    containerSize.Height += childSize.Height;
                    containerSize.Height += compositeDesignerTheme.ConnectorSize.Height;
                }

                if (ContainedDesigners.Count == 0)
                {
                    Rectangle helpTextRectangle = HelpTextRectangle;
                    containerSize.Width = Math.Max(helpTextRectangle.Width, containerSize.Width);
                    containerSize.Height += helpTextRectangle.Height;
                    containerSize.Height += compositeDesignerTheme.ConnectorSize.Height; //Add the height of last connector
                }

                containerSize.Width = Math.Max(containerSize.Width, compositeDesignerTheme.Size.Width);
                containerSize.Width += 3 * e.AmbientTheme.Margin.Width;
                containerSize.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                containerSize.Height = Math.Max(containerSize.Height, compositeDesignerTheme.Size.Height);
            }

            return containerSize;
        }
        #endregion

        #region Private Methods
        private DesignerGlyph[] CreateConnectorDragDropGlyphs()
        {
            //see if there's a drag'n'drop operation going on
            WorkflowView parentView = ParentView;
            DragDropManager dragDropManager = GetService(typeof(DragDropManager)) as DragDropManager;
            if (parentView == null || dragDropManager == null || !parentView.DragDropInProgress || DrawingState != DrawingStates.Valid)
                return new DesignerGlyph[] { };

            List<DesignerGlyph> glyphs = new List<DesignerGlyph>();
            Rectangle logicalViewPort = parentView.ClientRectangleToLogical(new Rectangle(Point.Empty, parentView.ViewPortSize));
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Rectangle[] connectors = GetConnectors();
            Rectangle helpTextRectangle = HelpTextRectangle;
            for (int i = 0; i < connectors.Length; i++)
            {
                if (logicalViewPort.IntersectsWith(connectors[i]) && i != CurrentDropTarget && dragDropManager.IsValidDropContext(new ConnectorHitTestInfo(this, HitTestLocations.Designer, i)))
                {
                    Point glyphLocation = Point.Empty;
                    if (helpTextRectangle.IsEmpty)
                        glyphLocation = new Point(connectors[i].Location.X + connectors[i].Size.Width / 2 + 1, connectors[i].Location.Y + connectors[i].Size.Height / 2);
                    else
                        glyphLocation = new Point(helpTextRectangle.Left + helpTextRectangle.Width / 2 + 1, helpTextRectangle.Top - ambientTheme.DropIndicatorSize.Height / 2);

                    glyphs.Add(new ConnectorDragDropGlyph(i, glyphLocation));
                }
            }

            return glyphs.ToArray();
        }

        #endregion

        #endregion

        #region Class SequentialConnectorSelectionGlyph
        private sealed class SequentialConnectorSelectionGlyph : ConnectorSelectionGlyph
        {
            public SequentialConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph)
                : base(connectorIndex, isPrimarySelectionGlyph)
            {
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle bounds = Rectangle.Empty;
                if (designer is SequentialActivityDesigner)
                {
                    Rectangle[] connectors = ((SequentialActivityDesigner)designer).GetConnectors();
                    if (this.connectorIndex < connectors.Length)
                        bounds = connectors[this.connectorIndex];
                }
                return bounds;
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                Rectangle bounds = GetBounds(designer, activated);
                Rectangle[] grabHandles = new Rectangle[2];
                grabHandles[0] = new Rectangle(bounds.X + bounds.Width / 2 - ambientTheme.SelectionSize.Width / 2, bounds.Y, ambientTheme.SelectionSize.Width, ambientTheme.SelectionSize.Height);
                grabHandles[1] = new Rectangle(bounds.X + bounds.Width / 2 - ambientTheme.SelectionSize.Width / 2, bounds.Bottom - ambientTheme.SelectionSize.Height, ambientTheme.SelectionSize.Width, ambientTheme.SelectionSize.Height);
                ActivityDesignerPaint.DrawGrabHandles(graphics, grabHandles, this.isPrimarySelectionGlyph);
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return this.isPrimarySelectionGlyph;
                }
            }
        }
        #endregion
    }
    #endregion



}
