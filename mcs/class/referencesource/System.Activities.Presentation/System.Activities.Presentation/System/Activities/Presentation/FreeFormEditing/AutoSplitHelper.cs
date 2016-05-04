//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Collections.Generic;
    using System.Windows;

    internal static class AutoSplitHelper
    {
        private const double ShapeMargin = 40;

        public static void CalculateEntryExitEdges(Point mousePosition, Connector connector, out EdgeLocation entryEdge, out EdgeLocation exitEdge)
        {
            UIElement srcShape = FreeFormPanel.GetSourceConnectionPoint(connector).ParentDesigner;
            UIElement destShape = FreeFormPanel.GetDestinationConnectionPoint(connector).ParentDesigner;
            Point srcLocation = FreeFormPanel.GetLocation(srcShape);
            Point destLocation = FreeFormPanel.GetLocation(destShape);
            Size srcSize = FreeFormPanel.GetChildSize(srcShape);
            Size destSize = FreeFormPanel.GetChildSize(destShape);

            Point srcCenter = new Point(srcLocation.X + (srcSize.Width / 2), srcLocation.Y + (srcSize.Height / 2));
            Point destCenter = new Point(destLocation.X + (destSize.Width / 2), destLocation.Y + (destSize.Height / 2));

            entryEdge = CalculateEdgeLocation(mousePosition, srcCenter);
            exitEdge = CalculateEdgeLocation(mousePosition, destCenter);

            if (exitEdge == entryEdge)
            {
                switch (entryEdge)
                {
                    case EdgeLocation.Top:
                        exitEdge = EdgeLocation.Bottom;
                        break;

                    case EdgeLocation.Bottom:
                        exitEdge = EdgeLocation.Top;
                        break;

                    case EdgeLocation.Left:
                        exitEdge = EdgeLocation.Right;
                        break;

                    case EdgeLocation.Right:
                        exitEdge = EdgeLocation.Left;
                        break;
                }
            }
        }

        public static Point CalculateDropLocation(Point mousePosition, Point originalDropLocation, Connector connector, Size droppedSize, HashSet<Point> shapeLocations)
        {
            UIElement srcShape = FreeFormPanel.GetSourceConnectionPoint(connector).ParentDesigner;
            UIElement destShape = FreeFormPanel.GetDestinationConnectionPoint(connector).ParentDesigner;
            Point srcLocation = FreeFormPanel.GetLocation(srcShape);
            Point destLocation = FreeFormPanel.GetLocation(destShape);
            Size srcSize = FreeFormPanel.GetChildSize(srcShape);
            Size destSize = FreeFormPanel.GetChildSize(destShape);

            return CalculateDropLocation(mousePosition, originalDropLocation, droppedSize, srcLocation, destLocation, srcSize, destSize, shapeLocations);
        }

        internal static Point CalculateDropLocation(Point mousePosition, Point originalDropLocation, Size droppedSize, Point srcLocation, Point destLocation, Size srcSize, Size destSize, HashSet<Point> shapeLocations)
        {
            Point dropLocation = originalDropLocation;

            double distToSrc = DesignerGeometryHelper.ManhattanDistanceBetweenPoints(mousePosition, new Point(srcLocation.X + (srcSize.Width / 2), srcLocation.Y + (srcSize.Height / 2)));
            double distToDest = DesignerGeometryHelper.ManhattanDistanceBetweenPoints(mousePosition, new Point(destLocation.X + (destSize.Width / 2), destLocation.Y + (destSize.Height / 2)));

            AutoSplitAlignment srcAlignment = GetAlignment(mousePosition, srcLocation, srcSize);
            AutoSplitAlignment destAlignment = GetAlignment(mousePosition, destLocation, destSize);

            if ((distToSrc <= distToDest || destAlignment == AutoSplitAlignment.None) && srcAlignment == AutoSplitAlignment.Vertical)
            {
                dropLocation = CalculateDropLocationToAlignVertically(dropLocation, droppedSize, srcLocation, srcSize);
            }
            else if ((distToSrc <= distToDest || destAlignment == AutoSplitAlignment.None) && srcAlignment == AutoSplitAlignment.Horizontal)
            {
                dropLocation = CalculateDropLocationToAlignHorizontally(dropLocation, droppedSize, srcLocation, srcSize);
            }
            else if ((distToSrc >= distToDest || srcAlignment == AutoSplitAlignment.None) && destAlignment == AutoSplitAlignment.Vertical)
            {
                dropLocation = CalculateDropLocationToAlignVertically(dropLocation, droppedSize, destLocation, destSize);
            }
            else if ((distToSrc >= distToDest || srcAlignment == AutoSplitAlignment.None) && destAlignment == AutoSplitAlignment.Horizontal)
            {
                dropLocation = CalculateDropLocationToAlignHorizontally(dropLocation, droppedSize, destLocation, destSize);
            }

            dropLocation.X = dropLocation.X < 0 ? 0 : dropLocation.X;
            dropLocation.Y = dropLocation.Y < 0 ? 0 : dropLocation.Y;

            // To avoid overlaps with existing shapes
            if (shapeLocations != null)
            {
                while (shapeLocations.Contains(dropLocation))
                {
                    dropLocation.Offset(FreeFormPanel.GridSize, FreeFormPanel.GridSize);
                }
            }

            return dropLocation;
        }

        internal static AutoSplitAlignment GetAlignment(Point mousePosition, Point targetLocation, Size targetSize)
        {
            AutoSplitAlignment alignment = AutoSplitAlignment.None;
            if (mousePosition.X >= targetLocation.X && mousePosition.X <= targetLocation.X + targetSize.Width)
            {
                alignment = AutoSplitAlignment.Vertical;
            }
            else if (mousePosition.Y >= targetLocation.Y && mousePosition.Y <= targetLocation.Y + targetSize.Height)
            {
                alignment = AutoSplitAlignment.Horizontal;
            }

            return alignment;
        }

        internal static Point CalculateDropLocationToAlignVertically(Point originalDropLocation, Size droppedSize, Point targetLocation, Size targetSize)
        {
            Point dropLocation = originalDropLocation;
            dropLocation.X = targetLocation.X + ((targetSize.Width - droppedSize.Width) / 2);
            if (dropLocation.Y >= targetLocation.Y && dropLocation.Y <= targetLocation.Y + targetSize.Height + ShapeMargin)
            {
                dropLocation.Y = targetLocation.Y + targetSize.Height + ShapeMargin;
            }
            else if (dropLocation.Y + droppedSize.Height + ShapeMargin >= targetLocation.Y && dropLocation.Y <= targetLocation.Y)
            {
                dropLocation.Y = targetLocation.Y - droppedSize.Height - ShapeMargin;
            }

            return dropLocation;
        }

        internal static Point CalculateDropLocationToAlignHorizontally(Point originalDropLocation, Size droppedSize, Point targetLocation, Size targetSize)
        {
            Point dropLocation = originalDropLocation;
            dropLocation.Y = targetLocation.Y + ((targetSize.Height - droppedSize.Height) / 2);
            if (dropLocation.X >= targetLocation.X && dropLocation.X <= targetLocation.X + targetSize.Width + ShapeMargin)
            {
                dropLocation.X = targetLocation.X + targetSize.Width + ShapeMargin;
            }
            else if (dropLocation.X + droppedSize.Width + ShapeMargin >= targetLocation.X && dropLocation.X <= targetLocation.X)
            {
                dropLocation.X = targetLocation.X - droppedSize.Width - ShapeMargin;
            }

            return dropLocation;
        }

        internal static EdgeLocation CalculateEdgeLocation(Point mousePosition, Point shapeCenter)
        {
            double distX = Math.Abs(shapeCenter.X - mousePosition.X);
            double distY = Math.Abs(shapeCenter.Y - mousePosition.Y);
            if (distX > distY)
            {
                return shapeCenter.X < mousePosition.X ? EdgeLocation.Left : EdgeLocation.Right;
            }
            else
            {
                return shapeCenter.Y < mousePosition.Y ? EdgeLocation.Top : EdgeLocation.Bottom;
            }
        }
    }
}
