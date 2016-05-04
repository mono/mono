//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    
    internal sealed class AutoConnectAdorner : Adorner
    {
        private const double TriangleBaseLength = 15;
        private const double TriangleHeight = 10;
        private const double HitTestWidth = 20;
        private const double HitTestHeight = 30;
        private const double DropTargetOffset = 15;
        private AutoConnectDirections highlightedDirection = AutoConnectDirections.None;
        private AutoConnectDirections directions;
        private Rect[] hitTestRects;
        private PathGeometry[] renderGeometries;
        private Rect adornedElementRect;
        private SolidColorBrush hitTestBrush = Brushes.Transparent;
        private Pen hitTestPen = new Pen(Brushes.Transparent, 0);
        private SolidColorBrush renderBrush = Brushes.Azure;
        private Pen renderPen = new Pen(Brushes.LightBlue, 1.0);
        private SolidColorBrush highlightBrush = Brushes.LightBlue;
        private Pen highlightPen = new Pen(Brushes.SteelBlue, 1);
        private FreeFormPanel panel;
        
        public AutoConnectAdorner(UIElement adornedElement, FreeFormPanel panel, AutoConnectDirections directions) 
            : base(adornedElement)
        {
            this.panel = panel;
            this.directions = directions;

            Size size = FreeFormPanel.GetChildSize(this.AdornedElement);
            this.adornedElementRect = new Rect(new Point(0, 0), size);
            this.hitTestRects = new Rect[] 
            { 
                new Rect(-HitTestHeight, (size.Height / 2) - (HitTestWidth / 2), HitTestHeight, HitTestWidth),
                new Rect(size.Width, (size.Height / 2) - (HitTestWidth / 2), HitTestHeight, HitTestWidth),
                new Rect((size.Width / 2) - (HitTestWidth / 2), -HitTestHeight, HitTestWidth, HitTestHeight),
                new Rect((size.Width / 2) - (HitTestWidth / 2), size.Height, HitTestWidth, HitTestHeight)
            };

            this.renderGeometries = new PathGeometry[]
            {
                new PathGeometry() 
                { 
                    Figures =
                    {
                        new PathFigure()
                        {
                            StartPoint = new Point(-DropTargetOffset - TriangleHeight, size.Height / 2), Segments = 
                            { 
                                new LineSegment() { Point = new Point(-DropTargetOffset, (size.Height / 2) - (TriangleBaseLength / 2)) },
                                new LineSegment() { Point = new Point(-DropTargetOffset, (size.Height / 2) + (TriangleBaseLength / 2)) },
                                new LineSegment() { Point = new Point(-DropTargetOffset - TriangleHeight, size.Height / 2) }
                            }
                        }
                    } 
                },
                new PathGeometry() 
                { 
                    Figures =
                    {
                        new PathFigure()
                        {
                            StartPoint = new Point(size.Width + DropTargetOffset, (size.Height / 2) - (TriangleBaseLength / 2)), Segments = 
                            { 
                                new LineSegment() { Point = new Point(size.Width + DropTargetOffset + TriangleHeight, size.Height / 2) },
                                new LineSegment() { Point = new Point(size.Width + DropTargetOffset, (size.Height / 2) + (TriangleBaseLength / 2)) },
                                new LineSegment() { Point = new Point(size.Width + DropTargetOffset, (size.Height / 2) - (TriangleBaseLength / 2)) }
                            }
                        }
                    }
                },
                new PathGeometry() 
                { 
                    Figures =
                    {
                        new PathFigure()
                        {
                            StartPoint = new Point((size.Width / 2) - (TriangleBaseLength / 2), -DropTargetOffset), Segments = 
                            { 
                                new LineSegment() { Point = new Point((size.Width / 2), -DropTargetOffset - TriangleHeight) },
                                new LineSegment() { Point = new Point((size.Width / 2) + (TriangleBaseLength / 2), -DropTargetOffset) },
                                new LineSegment() { Point = new Point((size.Width / 2) - (TriangleBaseLength / 2), -DropTargetOffset) }
                            }
                        }
                    }
                },
                new PathGeometry() 
                { 
                    Figures = 
                    {
                        new PathFigure()
                        {
                            StartPoint = new Point((size.Width / 2) - (TriangleBaseLength / 2), size.Height + DropTargetOffset), Segments = 
                            { 
                                new LineSegment() { Point = new Point((size.Width / 2) + (TriangleBaseLength / 2), size.Height + DropTargetOffset) },
                                new LineSegment() { Point = new Point((size.Width / 2), size.Height + TriangleHeight + DropTargetOffset) },
                                new LineSegment() { Point = new Point((size.Width / 2) - (TriangleBaseLength / 2), size.Height + DropTargetOffset) }
                            }
                        }
                    }
                }
            };
        }

        internal AutoConnectDirections AutoConnectDirection
        {
            get
            {
                return this.highlightedDirection;
            }
        }

        internal void OnDrag(DragEventArgs e)
        {
            Point position = e.GetPosition(this.AdornedElement);
            this.UpdateHighlightedDirection(position);
            e.Effects |= DragDropEffects.Move;
            e.Handled = true;
        }

        internal void UpdateHighlightedDirection(Point position)
        {
            Size size = FreeFormPanel.GetChildSize(this.AdornedElement);
            if (position.X < 0 && this.highlightedDirection != AutoConnectDirections.Left)
            {
                this.highlightedDirection = AutoConnectDirections.Left;
                this.InvalidateVisual();
            }
            else if (position.X > size.Width && this.highlightedDirection != AutoConnectDirections.Right)
            {
                this.highlightedDirection = AutoConnectDirections.Right;
                this.InvalidateVisual();
            }
            else if (position.Y < 0 && this.highlightedDirection != AutoConnectDirections.Top)
            {
                this.highlightedDirection = AutoConnectDirections.Top;
                this.InvalidateVisual();
            }
            else if (position.Y > size.Height && this.highlightedDirection != AutoConnectDirections.Bottom)
            {
                this.highlightedDirection = AutoConnectDirections.Bottom;
                this.InvalidateVisual();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.AllowDrop = true;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            this.OnDrag(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            this.OnDrag(e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            this.highlightedDirection = AutoConnectDirections.None;
            this.InvalidateVisual();
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (this.panel.AutoConnectContainer != null)
            {
                try
                {
                    this.panel.AutoConnectContainer.DoAutoConnect(e, this.AdornedElement, this.AutoConnectDirection);
                }
                finally
                {
                    e.Handled = true;
                    this.panel.RemoveAutoConnectAdorner();
                }
            }

            base.OnDrop(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            for (int i = 0; i < 4; i++)
            {
                if ((AutoConnectHelper.GetAutoConnectDirection(i) & this.directions) != 0)
                {
                    drawingContext.DrawRectangle(this.hitTestBrush, this.hitTestPen, this.hitTestRects[i]);
                    drawingContext.DrawGeometry(
                        (AutoConnectHelper.GetAutoConnectDirection(i) == this.highlightedDirection) ? this.highlightBrush : this.renderBrush,
                        (AutoConnectHelper.GetAutoConnectDirection(i) == this.highlightedDirection) ? this.highlightPen : this.renderPen,
                        this.renderGeometries[i]);
                }
            }

            if (this.AutoConnectDirection != AutoConnectDirections.None)
            {
                drawingContext.DrawRectangle(null, this.highlightPen, this.adornedElementRect);
            }
        }
    }
}
