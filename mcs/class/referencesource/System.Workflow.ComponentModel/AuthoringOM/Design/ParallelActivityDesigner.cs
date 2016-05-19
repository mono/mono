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

    #region ParallelActivityDesigner Class
    /// <summary>
    /// Base class used for all the designers which have parallel vertical layout.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ParallelActivityDesigner : StructuredCompositeActivityDesigner
    {
        #region Fields
        private ActivityDesignerVerbCollection designerVerbs;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the ParallelActivityDesigner
        /// </summary>
        public ParallelActivityDesigner()
        {
        }
        #endregion

        #region Properties

        #region Public Properties
        public override object FirstSelectableObject
        {
            get
            {
                if (ActiveDesigner != this)
                    return base.FirstSelectableObject;

                if (!Expanded || !IsVisible)
                    return null;

                object firstComponent = null;
                if (ContainedDesigners.Count > 0)
                    firstComponent = ContainedDesigners[0].Activity;

                return firstComponent;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (ActiveDesigner != this)
                    return base.FirstSelectableObject;

                if (!Expanded || !IsVisible)
                    return null;

                object firstComponent = (ContainedDesigners.Count > 0) ? ContainedDesigners[0].Activity : null;
                CompositeActivityDesigner firstDesigner = (firstComponent is Activity) ? ActivityDesigner.GetDesigner(firstComponent as Activity) as CompositeActivityDesigner : null;

                object lastObject = null;
                if (firstDesigner != null)
                    lastObject = firstDesigner.LastSelectableObject;

                // this might have happen when the first activityDesigner is collapsed
                if (lastObject == null)
                    lastObject = firstComponent;

                return lastObject;
            }
        }
        #endregion

        #region Protected Properties
        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(base.Verbs);

                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();
                    this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.AddBranch), new EventHandler(OnAddBranch), new EventHandler(OnStatusAddBranch)));
                }

                verbs.AddRange(this.designerVerbs);
                return verbs;
            }
        }

        #endregion

        #region Private Properties
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (ActiveDesigner != this)
                return base.GetNextSelectableObject(obj, direction);

            if (direction != DesignerNavigationDirection.Left && direction != DesignerNavigationDirection.Right)
                return null;

            object nextObject = null;

            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
            ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(obj as Activity);
            int index = (activityDesigner != null) ? containedDesigners.IndexOf(activityDesigner) : -1;
            if (direction == DesignerNavigationDirection.Left && index >= 0 && index < containedDesigners.Count)
                nextObject = ((ActivityDesigner)containedDesigners[(index > 0) ? index - 1 : containedDesigners.Count - 1]).Activity;
            else if (direction == DesignerNavigationDirection.Right && index <= containedDesigners.Count - 1)
                nextObject = ((ActivityDesigner)containedDesigners[(index < containedDesigners.Count - 1) ? index + 1 : 0]).Activity;

            return nextObject;
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
                throw new ArgumentNullException("activitiesToRemove");

            if (ActiveDesigner != this)
                return base.CanRemoveActivities(activitiesToRemove);

            if ((ContainedDesigners.Count - activitiesToRemove.Count) < 2)
                return false;
            else
                return base.CanRemoveActivities(activitiesToRemove);
        }

        public override bool CanMoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
                throw new ArgumentNullException("moveLocation");

            if (activitiesToMove == null)
                throw new ArgumentNullException("activitiesToMove");

            if (ActiveDesigner != this)
                return base.CanMoveActivities(moveLocation, activitiesToMove);

            if ((ContainedDesigners.Count - activitiesToMove.Count) < 2)
            {
                if (moveLocation != null && moveLocation.AssociatedDesigner != this)
                    return false;
            }

            return base.CanMoveActivities(moveLocation, activitiesToMove);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Create new activity to be added as branch within the parallel designer
        /// </summary>
        /// <returns>CompositeActivity to be added as branch</returns>
        protected virtual CompositeActivity OnCreateNewBranch()
        {
            return null;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            if (Expanded && ActiveDesigner == this)
            {
                //Draw the connectors
                if (ContainedDesigners.Count > 0)
                    DrawParallelConnectors(e);

                //Drop targets to add the branches
                if (CurrentDropTarget >= 0)
                    DrawParallelDropTargets(e, CurrentDropTarget);
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
                if (compositeDesignerTheme != null)
                {
                    ReadOnlyCollection<Point> connectionPoints = GetInnerConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                    Point location = (connectionPoints.Count > 0) ? new Point(Location.X, connectionPoints[0].Y) : Location;

                    int width = 0;
                    int height = compositeDesignerTheme.ConnectorSize.Height * 3 / 2;
                    foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                    {
                        width += e.AmbientTheme.SelectionSize.Width;
                        width += compositeDesignerTheme.ConnectorSize.Width;
                        Size designerSize = activityDesigner.Size;
                        activityDesigner.Location = new Point(location.X + width, location.Y + height);
                        width += designerSize.Width;
                        width += e.AmbientTheme.SelectionSize.Width;
                    }
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (Expanded && ActiveDesigner == this && compositeDesignerTheme != null)
            {
                //Calculate the container size
                Size containerSize = Size.Empty;

                //Calculate the size based on child size
                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                {
                    Size childSize = activityDesigner.Size;
                    containerSize.Width += e.AmbientTheme.SelectionSize.Width;
                    containerSize.Width += compositeDesignerTheme.ConnectorSize.Width;
                    containerSize.Width += childSize.Width;
                    containerSize.Width += e.AmbientTheme.SelectionSize.Width;
                    containerSize.Height = Math.Max(containerSize.Height, childSize.Height);
                }
                containerSize.Width += (ContainedDesigners.Count > 0) ? compositeDesignerTheme.ConnectorSize.Width : 0;

                //Once this happens then make sure that all the branches are of same size
                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                    activityDesigner.Size = new Size(activityDesigner.Size.Width, containerSize.Height);

                //Add the margin for drawing the parallel connectors
                containerSize.Height += 3 * compositeDesignerTheme.ConnectorSize.Height;

                //Now consider the base classe's size
                size.Width = Math.Max(size.Width, containerSize.Width);
                size.Height += containerSize.Height;
            }

            return size;
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if (!Expanded || ActiveDesigner != this)
                return new Rectangle[] { };

            CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;

            Rectangle bounds = Bounds;
            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
            Rectangle[] dropTargets = new Rectangle[containedDesigners.Count + 1];
            if (containedDesigners.Count > 0)
            {
                //Add the first connector
                ActivityDesigner firstDesigner = containedDesigners[0];
                dropTargets[0].Location = new Point(bounds.X, firstDesigner.Location.Y);
                dropTargets[0].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), firstDesigner.Size.Height);

                for (int i = 0; i < containedDesigners.Count - 1; i++)
                {
                    ActivityDesigner designer1 = containedDesigners[i];
                    Rectangle designerBounds1 = designer1.Bounds;

                    ActivityDesigner designer2 = containedDesigners[i + 1];
                    Rectangle designerBounds2 = designer2.Bounds;

                    dropTargets[i + 1].Location = new Point(designerBounds1.Right + (designerBounds2.Left - designerBounds1.Right) / 2 - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2, designerBounds1.Top);
                    dropTargets[i + 1].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), designerBounds1.Height);
                }

                //Add last connector
                ActivityDesigner lastDesigner = containedDesigners[containedDesigners.Count - 1];
                dropTargets[containedDesigners.Count].Location = new Point(bounds.Right - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), lastDesigner.Location.Y);
                dropTargets[containedDesigners.Count].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), lastDesigner.Size.Height);
            }
            else
            {
                dropTargets[0].Location = new Point(Location.X + (Size.Width - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0)) / 2, TextRectangle.Bottom);
                dropTargets[0].Size = new Size(((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), Location.Y + Size.Height - dropTargets[0].Location.Y);
            }

            return dropTargets;
        }
        #endregion

        #region Private Methods
        private void OnStatusAddBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                verb.Enabled = IsEditable;
            }
        }

        private void DrawParallelConnectors(ActivityDesignerPaintEventArgs e)
        {
            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme == null)
                return;

            //Get all the information required to calculate the connectors
            Rectangle bounds = Bounds;
            int parallelConnectorTop = bounds.Top;
            parallelConnectorTop += TitleHeight;

            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;

            ActivityDesigner firstDesigner = containedDesigners[0];
            ReadOnlyCollection<Point> firstDesignerConnections = firstDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);

            ActivityDesigner lastDesigner = containedDesigners[containedDesigners.Count - 1];
            ReadOnlyCollection<Point> lastDesignerConnections = lastDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);

            Point[] parallelLinks = new Point[2];

            //Draw small vertical line at the top
            parallelLinks[0].X = bounds.Left + bounds.Width / 2;
            parallelLinks[0].Y = parallelConnectorTop;
            parallelLinks[1].X = bounds.Left + bounds.Width / 2;
            parallelLinks[1].Y = parallelConnectorTop + compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, parallelLinks, LineAnchor.None, LineAnchor.None);

            //Draw Horizontal line at the top
            parallelLinks[0].X = firstDesignerConnections[0].X;
            parallelLinks[0].Y = parallelConnectorTop + compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            parallelLinks[1].X = lastDesignerConnections[0].X;
            parallelLinks[1].Y = parallelConnectorTop + compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, parallelLinks, LineAnchor.None, LineAnchor.None);

            //Draw Horizontal line at the bottom
            parallelLinks[0].X = firstDesignerConnections[firstDesignerConnections.Count - 1].X;
            parallelLinks[0].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            parallelLinks[1].X = lastDesignerConnections[lastDesignerConnections.Count - 1].X;
            parallelLinks[1].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, parallelLinks, LineAnchor.None, LineAnchor.None);

            //Draw small vertical line at the bottom
            parallelLinks[0].X = bounds.Left + bounds.Width / 2;
            parallelLinks[0].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
            parallelLinks[1].X = bounds.Left + bounds.Width / 2;
            parallelLinks[1].Y = bounds.Bottom;
            DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, parallelLinks, LineAnchor.None, LineAnchor.None);

            foreach (ActivityDesigner activityDesigner in containedDesigners)
            {
                ReadOnlyCollection<Point> designerConnections = activityDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);
                int connectionCount = designerConnections.Count;
                Point[] connectingLine = new Point[2];

                //Draw connectors for every activityDesigner
                connectingLine[0].X = designerConnections[0].X;
                connectingLine[0].Y = parallelConnectorTop + compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
                connectingLine[1].X = designerConnections[0].X;
                connectingLine[1].Y = designerConnections[0].Y;
                DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, connectingLine, compositeDesignerTheme.ConnectorStartCap, compositeDesignerTheme.ConnectorEndCap);

                //Draw connectors for every activityDesigner
                connectingLine[0].X = designerConnections[connectionCount - 1].X;
                connectingLine[0].Y = designerConnections[connectionCount - 1].Y;
                connectingLine[1].X = designerConnections[connectionCount - 1].X;
                connectingLine[1].Y = bounds.Bottom - compositeDesignerTheme.ConnectorSize.Height * 3 / 4;
                DrawConnectors(e.Graphics, compositeDesignerTheme.ForegroundPen, connectingLine, compositeDesignerTheme.ConnectorStartCap, compositeDesignerTheme.ConnectorEndCap);
            }
        }

        private void DrawParallelDropTargets(ActivityDesignerPaintEventArgs e, int index)
        {
            Rectangle[] dropTargets = GetDropTargets(Point.Empty);
            if (index < 0 || index >= dropTargets.Length)
                return;

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme == null)
                return;

            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
            Rectangle dropTargetRectangle = dropTargets[index];

            Rectangle bounds = Bounds;
            int parallelConnectorTop = bounds.Top;
            parallelConnectorTop += TitleHeight;
            parallelConnectorTop += (containedDesigners.Count > 0) ? compositeDesignerTheme.ConnectorSize.Height * 3 / 4 : 0;

            int heightDelta = dropTargetRectangle.Y - parallelConnectorTop;
            heightDelta += bounds.Bottom - dropTargetRectangle.Bottom;
            heightDelta -= (containedDesigners.Count > 0) ? compositeDesignerTheme.ConnectorSize.Height * 3 / 4 : 0;

            dropTargetRectangle.Y = parallelConnectorTop;
            dropTargetRectangle.Height += heightDelta;
            DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, new Point[] { new Point(dropTargetRectangle.Left + dropTargetRectangle.Width / 2, dropTargetRectangle.Top + 2), new Point(dropTargetRectangle.Left + dropTargetRectangle.Width / 2, dropTargetRectangle.Bottom - 2) }, compositeDesignerTheme.ConnectorStartCap, compositeDesignerTheme.ConnectorEndCap);

            if (containedDesigners.Count > 0)
            {
                if (index == 0)
                {
                    ActivityDesigner firstDesigner = containedDesigners[0];
                    ReadOnlyCollection<Point> firstDesignerConnections = firstDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);

                    Point[] connectorPoints = new Point[2];
                    connectorPoints[0].X = dropTargetRectangle.X + dropTargetRectangle.Width / 2;
                    connectorPoints[0].Y = dropTargetRectangle.Y;
                    connectorPoints[1].X = firstDesignerConnections[0].X;
                    connectorPoints[1].Y = dropTargetRectangle.Y;
                    DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, connectorPoints, LineAnchor.None, LineAnchor.None);

                    connectorPoints[0].Y = dropTargetRectangle.Bottom;
                    connectorPoints[1].Y = dropTargetRectangle.Bottom;
                    DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, connectorPoints, LineAnchor.None, LineAnchor.None);
                }
                else if (index == containedDesigners.Count)
                {
                    ActivityDesigner lastDesigner = containedDesigners[containedDesigners.Count - 1];
                    ReadOnlyCollection<Point> lastDesignerConnections = lastDesigner.GetConnections(DesignerEdges.Top | DesignerEdges.Bottom);

                    Point[] connectorPoints = new Point[2];
                    connectorPoints[0].X = lastDesignerConnections[0].X;
                    connectorPoints[0].Y = dropTargetRectangle.Y;
                    connectorPoints[1].X = dropTargetRectangle.X + dropTargetRectangle.Width / 2;
                    connectorPoints[1].Y = dropTargetRectangle.Y;
                    DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, connectorPoints, LineAnchor.None, LineAnchor.None);

                    connectorPoints[0].Y = dropTargetRectangle.Bottom;
                    connectorPoints[1].Y = dropTargetRectangle.Bottom;
                    DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, connectorPoints, LineAnchor.None, LineAnchor.None);
                }
            }
        }

        private void OnAddBranch(object sender, EventArgs e)
        {
            CompositeActivity branchActivity = OnCreateNewBranch();
            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity != null && branchActivity != null)
            {
                // Record the current number of child activities
                int designerCount = ContainedDesigners.Count;

                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, compositeActivity.Activities.Count), new List<Activity>(new Activity[] { branchActivity }).AsReadOnly(), DR.GetString(DR.AddingBranch, branchActivity.GetType().Name));

                // If the number of child activities has increased, the branch add was successful, so
                // make sure the highest indexed branch is visible
                if (ContainedDesigners.Count > designerCount && ContainedDesigners.Count > 0)
                    ContainedDesigners[ContainedDesigners.Count - 1].EnsureVisible();
            }
        }
        #endregion

        #endregion
    }
    #endregion

}
