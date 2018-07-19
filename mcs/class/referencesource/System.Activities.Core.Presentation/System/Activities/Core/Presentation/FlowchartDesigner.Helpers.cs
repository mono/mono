    //------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Activities.Statements;
    using System.Activities.Presentation.Model;
    using System.Linq;
    using System.Runtime;
    using System.Globalization;
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;

    partial class FlowchartDesigner
    {
        //Returns true if visual is on the visual tree for point p relative to the panel.
        bool IsVisualHit(UIElement visual, UIElement reference, Point point)
        {
            bool visualIsHit = false;
            HitTestResult result = VisualTreeHelper.HitTest(reference, point);
            if (result != null)
            {
                DependencyObject obj = result.VisualHit;
                while (obj != null)
                {
                    if (visual.Equals(obj))
                    {
                        visualIsHit = true;
                        break;
                    }
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }
            return visualIsHit;
        }

        /// <summary>
        /// Priority order of finding the connection point:
        /// 1. Unoccupied (free) connection point closest to the object
        /// 2. Existing incoming/outgoing connection point, depending on the 
        /// Fallback: Connection point closest to the object)
        /// </summary>
        /// <param name="srcConnPoints"></param>
        /// <param name="destConnPoints"></param>
        /// <param name="srcConnPoint"></param>
        /// <param name="destConnPoint"></param>
        internal void FindBestMatchConnectionPointPair(
            List<ConnectionPoint> srcConnPoints, 
            List<ConnectionPoint> destConnPoints, 
            out ConnectionPoint srcConnPoint, 
            out ConnectionPoint destConnPoint)
        {
            double minDist = double.PositiveInfinity;
            double dist;
            ConnectionPoint tempConnPoint;
            srcConnPoint = null;
            destConnPoint = null;

            List<ConnectionPoint> candidateSrcConnPoints = FindCandidatePointsForLink(srcConnPoints, ConnectionPointKind.Incoming);
            List<ConnectionPoint> candidateDestConnPoints = FindCandidatePointsForLink(destConnPoints, ConnectionPointKind.Outgoing);

            foreach (ConnectionPoint connPoint in candidateSrcConnPoints)
            {
                tempConnPoint = FindClosestConnectionPoint(connPoint, candidateDestConnPoints, out dist);
                if (dist < minDist)
                {
                    minDist = dist;
                    srcConnPoint = connPoint;
                    destConnPoint = tempConnPoint;
                }
            }

            Fx.Assert(srcConnPoint != null, "No ConnectionPoint found");
            Fx.Assert(destConnPoint != null, "No ConnectionPoint found");
        }

        private static List<ConnectionPoint> FindCandidatePointsForLink(List<ConnectionPoint> destConnPoints, ConnectionPointKind excludePointType)
        {
            List<ConnectionPoint> candidateDestConnPoints;
            IEnumerable<ConnectionPoint> freeDestConnPoints = destConnPoints.Where(p =>
                    p.PointType != excludePointType &&
                    !p.AttachedConnectors.Any());

            if (freeDestConnPoints.Any())
            {
                candidateDestConnPoints = freeDestConnPoints.ToList();
            }
            else
            {
                IEnumerable<ConnectionPoint> availablePoints =
                    destConnPoints.Where(
                        p => p.PointType != excludePointType &&
                        p.AttachedConnectors.Any(connector => FreeFormPanel.GetDestinationConnectionPoint(connector).Equals(p)));

                candidateDestConnPoints = availablePoints.Any() ? availablePoints.ToList() : destConnPoints;
            }

            return candidateDestConnPoints;
        }

        /// <summary>
        /// for connection:
        /// 1. return all free connection points are available on the object
        /// 2. return any existing points that are already connected on the object, excluding the unmatched type.
        ///    Fallback: return all connection points of the given object
        /// </summary>
        /// <param name="sourceConnectionPoint"></param>
        /// <param name="dest"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        ConnectionPoint FindBestMatchDestConnectionPoint(ConnectionPoint sourceConnectionPoint, UIElement dest, out string errorMessage)
        {
            List<ConnectionPoint> destConnPoints = FlowchartDesigner.GetConnectionPoints(dest);
            Fx.Assert(null != destConnPoints && destConnPoints.Any(), "A flownode designer object should have one connection point.");

            errorMessage = string.Empty;

            if (sourceConnectionPoint.PointType == ConnectionPointKind.Incoming)
            {
                errorMessage = SR.FCInvalidLink;
                return null;
            }
            
            ConnectionPoint destConnectionPoint;
            double minDist;
            List<ConnectionPoint> candidateDestConnPoints = FindCandidatePointsForLink(destConnPoints, ConnectionPointKind.Outgoing);
            destConnectionPoint = FindClosestConnectionPoint(sourceConnectionPoint, candidateDestConnPoints, out minDist);

            return destConnectionPoint;
        }

        internal ConnectionPoint FindClosestConnectionPoint(ConnectionPoint srcConnPoint, List<ConnectionPoint> destConnPoints, out double minDist)
        {
            return FindClosestConnectionPoint(srcConnPoint.Location, destConnPoints, out minDist);
        }

        internal ConnectionPoint FindClosestConnectionPoint(Point srcConnPointLocation, List<ConnectionPoint> destConnPoints, out double minDist)
        {
            return ConnectionPoint.GetClosestConnectionPoint(destConnPoints, srcConnPointLocation, out minDist);
        }

        ConnectionPoint FindClosestConnectionPointNotOfType(ConnectionPoint srcConnectionPoint, List<ConnectionPoint> targetConnectionPoints, ConnectionPointKind illegalConnectionPointKind)
        {
            double minDist;
            List<ConnectionPoint> filteredConnectionPoints = new List<ConnectionPoint>();
            foreach (ConnectionPoint connPoint in targetConnectionPoints)
            {
                if (connPoint != null && connPoint.PointType != illegalConnectionPointKind && !connPoint.Equals(srcConnectionPoint))
                {
                    filteredConnectionPoints.Add(connPoint);
                }
            }
            return FindClosestConnectionPoint(srcConnectionPoint, filteredConnectionPoints, out minDist);
        }

        void RemoveAdorner(UIElement adornedElement, Type adornerType)
        {
            Fx.Assert(adornedElement != null, "Invalid argument");
            Fx.Assert(typeof(Adorner).IsAssignableFrom(adornerType), "Invalid argument");
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (adornerLayer != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(adornedElement);
                if (adorners != null)
                {
                    foreach (Adorner adorner in adorners)
                    {
                        if (adornerType.IsAssignableFrom(adorner.GetType()))
                        {
                            adornerLayer.Remove(adorner);
                        }
                    }
                }
            }
        }

        //Returns true if child is a member of the tree rooted at the parent;
        bool IsParentOf(ModelItem parent, ModelItem child)
        {
            Fx.Assert(parent != null, "Invalid argument");
            bool isParentOf = false;
            while (child != null)
            {
                if (parent.Equals(child))
                {
                    isParentOf = true;
                    break;
                }
                child = child.Parent;
            }
            return isParentOf;
        }

        ConnectionPoint ConnectionPointHitTest(UIElement element, Point hitPoint)
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            List<ConnectionPoint> defaultConnectionPoints = FlowchartDesigner.GetConnectionPoints(element);
            connectionPoints.InsertRange(0, defaultConnectionPoints);
            connectionPoints.Add(FlowchartDesigner.GetTrueConnectionPoint(element));
            connectionPoints.Add(FlowchartDesigner.GetFalseConnectionPoint(element));
            return FreeFormPanel.ConnectionPointHitTest(hitPoint, connectionPoints, this.panel);
        }

        internal int NumberOfIncomingLinks(UIElement designer)
        {
            return GetInComingConnectors(designer).Count;
        }

        List<Connector> GetAttachedConnectors(UIElement shape)
        {
            HashSet<Connector> attachedConnectors = new HashSet<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetAllConnectionPoints(shape);
            foreach (ConnectionPoint connPoint in allConnectionPoints)
            {
                if (connPoint != null)
                {
                    foreach (Connector connector in connPoint.AttachedConnectors)
                    {
                        attachedConnectors.Add(connector);
                    }
                }
            }
            return attachedConnectors.ToList<Connector>();
        }

        List<Connector> GetOutGoingConnectors(UIElement shape)
        {
            List<Connector> outGoingConnectors = new List<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetAllConnectionPoints(shape);
            foreach (ConnectionPoint connPoint in allConnectionPoints)
            {
                if (connPoint != null)
                {
                    outGoingConnectors.AddRange(connPoint.AttachedConnectors.Where(p => FreeFormPanel.GetSourceConnectionPoint(p).Equals(connPoint)));
                }
            }
            return outGoingConnectors;
        }

        List<Connector> GetInComingConnectors(UIElement shape)
        {
            List<Connector> inComingConnectors = new List<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetAllConnectionPoints(shape);
            foreach (ConnectionPoint connPoint in allConnectionPoints)
            {
                if (connPoint != null)
                {
                    inComingConnectors.AddRange(connPoint.AttachedConnectors.Where(p => FreeFormPanel.GetDestinationConnectionPoint(p).Equals(connPoint)));
                }
            }
            return inComingConnectors;
        }

        static List<ConnectionPoint> GetAllConnectionPoints(UIElement shape)
        {
            List<ConnectionPoint> allConnectionPoints = new List<ConnectionPoint>(6);
            allConnectionPoints.AddRange(FlowchartDesigner.GetConnectionPoints(shape));
            allConnectionPoints.Add(FlowchartDesigner.GetTrueConnectionPoint(shape));
            allConnectionPoints.Add(FlowchartDesigner.GetFalseConnectionPoint(shape));
            return allConnectionPoints;
        }

        Point SnapPointToGrid(Point pt)
        {
            pt.X -= pt.X % FreeFormPanel.GridSize;
            pt.Y -= pt.Y % FreeFormPanel.GridSize;
            pt.X = pt.X < 0 ? 0 : pt.X;
            pt.Y = pt.Y < 0 ? 0 : pt.Y;
            return pt;
        }

        //This snaps the center of the element to grid.
        //This is called only when dropping an item 
        //Whereever, shapeAnchorPoint is valid, it is made co-incident with the drop location.
        Point SnapVisualToGrid(UIElement element, Point location, Point shapeAnchorPoint, bool isAnchorPointValid)
        {
            Fx.Assert(element != null, "Input UIElement is null");
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Point oldCenter = location;
            if (!isAnchorPointValid)
            {
                //shapeAnchorPoint is set to (-1, -1) in case where it does not make sense (Eg. toolbox drop).
                //In that scenario align the center of the shape to the drop point.
                location.X -= element.DesiredSize.Width / 2;
                location.Y -= element.DesiredSize.Height / 2;
            }
            else
            {
                //The else part also takes care of the ActivityDesigner case, 
                //where the drag handle is outside the shape.
                location.X -= shapeAnchorPoint.X;
                location.Y -= shapeAnchorPoint.Y;
                oldCenter = new Point(location.X + element.DesiredSize.Width / 2, location.Y + element.DesiredSize.Height / 2);
            }

            Point newCenter = SnapPointToGrid(oldCenter);

            location.Offset(newCenter.X - oldCenter.X, newCenter.Y - oldCenter.Y);

            if (location.X < 0)
            {
                double correction = FreeFormPanel.GridSize - ((location.X * (-1)) % FreeFormPanel.GridSize);
                location.X = (correction == FreeFormPanel.GridSize) ? 0 : correction;
            }
            if (location.Y < 0)
            {
                double correction = FreeFormPanel.GridSize - ((location.Y * (-1)) % FreeFormPanel.GridSize);
                location.Y = (correction == FreeFormPanel.GridSize) ? 0 : correction;
            }
            return location;
        }


        // This creates a link from modelItems[i] to modelItems[i+1] - foreach i between 0 and modelItems.Count-2;
        void CreateLinks(List<ModelItem> modelItems)
        {
            Fx.Assert(modelItems.Count > 1, "Link creation requires more than one ModelItem");
            modelItems.ForEach(p => { Fx.Assert(this.modelElement.ContainsKey(p), "View should be in the flowchart"); });
            ModelItem[] modelItemsArray = modelItems.ToArray();
            string errorMessage = string.Empty;
            for (int i = 0; i < modelItemsArray.Length - 1; i++)
            {
                string error = string.Empty;
                CreateLinkGesture(this.modelElement[modelItemsArray[i]], this.modelElement[modelItemsArray[i + 1]], out error, null);
                if (!string.Empty.Equals(error))
                {
                    errorMessage += string.Format(CultureInfo.CurrentUICulture, "Link{0}:{1}\n", i + 1, error);
                }
            }
            if (!string.Empty.Equals(errorMessage))
            {
                ErrorReporting.ShowErrorMessage(errorMessage);
            }
        }

        // This is a utility function to pack all the elements in an array that match a particular predicate
        // to the end of the array, while maintaining the rest of the system unchanged.
        public static bool Pack<T>(T[] toPack, Func<T, bool> isPacked) where T : class
        {
            if (toPack == null)
            {
                throw FxTrace.Exception.ArgumentNull("toPack");
            }
            if (isPacked == null)
            {
                throw FxTrace.Exception.ArgumentNull("isPacked");
            }
            int count = toPack.Length;
            bool needRearrange = false;
            bool found = false;
            T[] arranged = new T[count];
            for (int i = 0; i < count; i++)
            {
                if (isPacked(toPack[i]))
                {
                    arranged[i] = toPack[i];
                    toPack[i] = null;
                    found = true;
                }
                else
                {
                    if (found)
                    {
                        needRearrange = true;
                    }
                    arranged[i] = null;
                }
            }
            if (needRearrange)
            {
                int j = 0;
                for (int i = 0; i < count; i++)
                {
                    if (toPack[i] != null)
                    {
                        toPack[j++] = toPack[i];
                    }
                }
                j = count;
                for (int i = 0; i < count; i++)
                {
                    int k = count - i - 1;
                    if (arranged[k] != null)
                    {
                        toPack[--j] = arranged[k];
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (arranged[i] != null)
                    {
                        toPack[i] = arranged[i];
                    }
                }
            }
            return needRearrange;
        }

    }
}
