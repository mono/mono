//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    internal sealed class AutoConnectHelper
    {
        private const double HitRegionOffset = 30;
        private const double DropTargetWidth = 20;
        private const double DropPointOffset = 50;

        private FreeFormPanel panel = null;
        private UIElement currentTarget = null;

        public AutoConnectHelper(FreeFormPanel panel)
        {
            this.panel = panel;
        }

        internal UIElement CurrentTarget
        {
            get
            {
                UIElement target = this.currentTarget;
                
                // It is possible that currentTarget has been removed from the FreeFormPanel
                if (target != null && VisualTreeHelper.GetParent(target) == null)
                {
                    this.currentTarget = null;
                    return null;
                }

                return target;
            }

            set
            {
                this.currentTarget = value;
            }
        }

        internal static Rect GetAutoConnectHitRect(DependencyObject target)
        {
            Size size = FreeFormPanel.GetChildSize(target);
            Point location = FreeFormPanel.GetLocation(target);
            Rect rect = new Rect(new Point(location.X - HitRegionOffset, location.Y - HitRegionOffset), new Size(size.Width + (HitRegionOffset * 2), size.Height + (HitRegionOffset * 2)));
            return rect;
        }

        // *                                                                   
        // * Diagram: Hit test rect for auto connect (best view with Courier New)                           
        // *                                                                   
        // *                        ├         W         ┤                      
        // *                                 ┤a├                               
        // *               ├   O    ┤                   ├    O   ┤             
        // *                                 ┌─┐                    ┬          
        // *                                 │ │                               
        // *                                 │ │                    O          
        // *                                 │ │                               
        // *                        ┌────────┴─┴────────┐           ┴       ┬  
        // *                        │                   │                      
        // *               ┌────────┤                   ├────────┐     ┴ a  H  
        // *               └────────┤                   ├────────┘     ┬       
        // *                        │                   │                      
        // *                        └────────┬─┬────────┘           ┬       ┴  
        // *                                 │ │                               
        // *                                 │ │                    O          
        // *                                 │ │                               
        // *                                 └─┘                    ┴          
        // *                                                                   
        // * W: Width                                                          
        // * H: Height                                                         
        // * O: HitRegionOffset                                                
        // * a: DropTargetWidth                                                
        // *                                                                   
        internal static List<Rect> CreateHitTestRects(Point targetLocation, Size targetSize)
        {
            List<Rect> rects = new List<Rect>();

            // See the diagram above for these rects
            rects.Add(new Rect(new Point(targetLocation.X - HitRegionOffset, targetLocation.Y + ((targetSize.Height - DropTargetWidth) / 2)), new Size(HitRegionOffset, DropTargetWidth)));
            rects.Add(new Rect(new Point(targetLocation.X + targetSize.Width, targetLocation.Y + ((targetSize.Height - DropTargetWidth) / 2)), new Size(HitRegionOffset, DropTargetWidth)));
            rects.Add(new Rect(new Point(targetLocation.X + ((targetSize.Width - DropTargetWidth) / 2), targetLocation.Y - HitRegionOffset), new Size(DropTargetWidth, HitRegionOffset)));
            rects.Add(new Rect(new Point(targetLocation.X + ((targetSize.Width - DropTargetWidth) / 2), targetLocation.Y + targetSize.Height), new Size(DropTargetWidth, HitRegionOffset)));
            return rects;
        }

        internal static AutoConnectDirections GetAutoConnectDirection(int index)
        {
            switch (index)
            {
                case 0:
                    return AutoConnectDirections.Left;
                case 1:
                    return AutoConnectDirections.Right;
                case 2:
                    return AutoConnectDirections.Top;
                case 3:
                    return AutoConnectDirections.Bottom;
                default:
                    return AutoConnectDirections.None;
            }
        }

        internal static Point CalculateDropLocation(Size droppedSize, DependencyObject autoConnectTarget, AutoConnectDirections direction, HashSet<Point> shapeLocations)
        {
            Point dropPoint = new Point(-1, -1);
            if (autoConnectTarget != null)
            {
                Point location = FreeFormPanel.GetLocation(autoConnectTarget);
                Size size = FreeFormPanel.GetChildSize(autoConnectTarget);
                switch (direction)
                {
                    case AutoConnectDirections.Left:
                        dropPoint = new Point(location.X - DropPointOffset - droppedSize.Width, location.Y + ((size.Height - droppedSize.Height) / 2));
                        break;
                    case AutoConnectDirections.Right:
                        dropPoint = new Point(location.X + size.Width + DropPointOffset, location.Y + ((size.Height - droppedSize.Height) / 2));
                        break;
                    case AutoConnectDirections.Top:
                        dropPoint = new Point(location.X + ((size.Width - droppedSize.Width) / 2), location.Y - DropPointOffset - droppedSize.Height);
                        break;
                    case AutoConnectDirections.Bottom:
                        dropPoint = new Point(location.X + ((size.Width - droppedSize.Width) / 2), location.Y + DropPointOffset + size.Height);
                        break;
                    default:
                        Fx.Assert(false, "Should not be here");
                        break;
                }

                dropPoint = new Point(dropPoint.X < 0 ? 0 : dropPoint.X, dropPoint.Y < 0 ? 0 : dropPoint.Y);
                if (shapeLocations != null)
                {
                    while (shapeLocations.Contains(dropPoint))
                    {
                        dropPoint.Offset(FreeFormPanel.GridSize, FreeFormPanel.GridSize);
                    }
                }
            }

            return dropPoint;
        }

        internal static EdgeLocation AutoConnectDirection2EdgeLocation(AutoConnectDirections direction)
        {
            EdgeLocation edgeLocation = EdgeLocation.Right;
            switch (direction)
            {
                case AutoConnectDirections.Left:
                    edgeLocation = EdgeLocation.Left;
                    break;
                case AutoConnectDirections.Right:
                    edgeLocation = EdgeLocation.Right;
                    break;
                case AutoConnectDirections.Top:
                    edgeLocation = EdgeLocation.Top;
                    break;
                case AutoConnectDirections.Bottom:
                    edgeLocation = EdgeLocation.Bottom;
                    break;
            }

            return edgeLocation;
        }
        
        internal static DependencyObject GetShapeContainingPoint(Point point, List<DependencyObject> shapes)
        {
            DependencyObject result = null;

            foreach (DependencyObject shape in shapes)
            {
                Rect rect = GetAutoConnectHitRect(shape);
                if (rect.Contains(point))
                {
                    // The design is that if the point is inside of multiple hit test regions, we do not 
                    // show any drop targets to avoid confusion.
                    if (result != null)
                    {
                        return null;
                    }

                    result = shape;
                }
            }

            return result;
        }

        internal DependencyObject FindTarget(Point point, DependencyObject dragged, out AutoConnectDirections directions)
        {
            directions = AutoConnectDirections.None;
            List<DependencyObject> childShapes = this.panel.GetChildShapes(dragged);
            DependencyObject target = GetShapeContainingPoint(point, childShapes);

            if (target != null)
            {
                directions = this.GetAutoConnectDirections(directions, childShapes, target);
            }

            return target;
        }

        internal void OnPreviewDragOverPanel(DragEventArgs e)
        {
            // Do not do auto-connect if we are currently auto-splitting.
            if (this.panel.CurrentAutoSplitTarget != null)
            {
                return;
            }

            DependencyObject currentTarget = this.CurrentTarget;
            if (currentTarget != null)
            {
                Rect rect = GetAutoConnectHitRect(currentTarget);
                if (rect.Contains(e.GetPosition(this.panel)))
                {
                    // Do not update the adorner if the cursor is still in the hit region of the current target
                    return;
                }
            }

            ModelItem draggedModelItem = DragDropHelper.GetDraggedModelItemInternal(e);
            DependencyObject draggedView = draggedModelItem != null ? draggedModelItem.View : null;
            AutoConnectDirections direction;
            UIElement target = this.FindTarget(e.GetPosition(this.panel), draggedView, out direction) as UIElement;
            this.RemoveDropTargets();
            if (target != null && (direction & this.panel.AutoConnectContainer.GetDirectionsAllowed(e, target)) != AutoConnectDirections.None)
            {
                this.AddDropTargets(e, target, direction);
            }  
        }

        internal void RemoveDropTargets()
        {
            UIElement adornedElement = this.CurrentTarget;
            if (adornedElement != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
                Fx.Assert(adornerLayer != null, "AdornerLayer should not be null.");
                Adorner[] adorners = adornerLayer.GetAdorners(adornedElement);
                foreach (Adorner adorner in adorners)
                {
                    if (adorner is AutoConnectAdorner)
                    {
                        adornerLayer.Remove(adorner);
                        this.CurrentTarget = null;
                        return;
                    }
                }
            }
        }

        // Check if hit test rects collide with any children of the FreeFormPanel, and remove that direction in case a collision is found.
        private static void RemoveDirectionsInCollision(List<DependencyObject> childShapes, DependencyObject target, List<Rect> hitTestRects, ref AutoConnectDirections directions)
        {
            foreach (DependencyObject shape in childShapes)
            {
                if (directions == AutoConnectDirections.None)
                {
                    break;
                }

                if (object.Equals(shape, target))
                {
                    continue;
                }

                Point shapeLocation = FreeFormPanel.GetLocation(shape);
                Size shapeSize = FreeFormPanel.GetChildSize(shape);
                Rect shapeRect = new Rect(shapeLocation, shapeSize);
                for (int i = 0; i < hitTestRects.Count; i++)
                {
                    if (hitTestRects[i].IntersectsWith(shapeRect))
                    {
                        directions &= ~AutoConnectHelper.GetAutoConnectDirection(i);
                    }
                }
            }
        }

        // Check if hit test rects are completely within the FreeFormPanel rect, and remove that direction in case it's not.
        private void RemoveDirectionsOutsideOfPanel(List<Rect> hitTestRects, ref AutoConnectDirections directions)
        {
            Rect panelRect = new Rect(0, 0, this.panel.Width, this.panel.Height);
            for (int i = 0; i < hitTestRects.Count; i++)
            {
                if (!panelRect.Contains(hitTestRects[i]))
                {
                    directions &= ~AutoConnectHelper.GetAutoConnectDirection(i);
                }
            }
        }

        private AutoConnectDirections GetAutoConnectDirections(AutoConnectDirections directions, List<DependencyObject> childShapes, DependencyObject target)
        {
            directions = AutoConnectDirections.Top | AutoConnectDirections.Bottom | AutoConnectDirections.Left | AutoConnectDirections.Right;
            List<Rect> hitTestRects = CreateHitTestRects(FreeFormPanel.GetLocation(target), FreeFormPanel.GetChildSize(target));
            this.RemoveDirectionsOutsideOfPanel(hitTestRects, ref directions);
            RemoveDirectionsInCollision(childShapes, target, hitTestRects, ref directions);
            return directions;
        }

        private void AddDropTargets(DragEventArgs e, UIElement adornedElement, AutoConnectDirections directions)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            Fx.Assert(adornerLayer != null, "AdornerLayer should not be null.");
            adornerLayer.Add(new AutoConnectAdorner(adornedElement, this.panel, (directions & this.panel.AutoConnectContainer.GetDirectionsAllowed(e, adornedElement))));
            this.CurrentTarget = adornedElement;
        }
    }
}
