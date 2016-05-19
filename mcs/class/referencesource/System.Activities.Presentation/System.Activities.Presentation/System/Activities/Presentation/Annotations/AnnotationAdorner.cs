//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Activities.Presentation.View;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    internal class AnnotationAdorner : Adorner
    {
        public static readonly DependencyProperty AnchorProperty = DependencyProperty.RegisterAttached("Anchor", typeof(AdornerLocation), typeof(AnnotationAdorner), new FrameworkPropertyMetadata(AdornerLocation.None));
        private UIElement content;

        public AnnotationAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        internal ScrollViewer ScrollViewer
        {
            get;
            set;
        }

        internal UIElement Content
        {
            get
            {
                return this.content;
            }

            set
            {
                if (this.content != value)
                {
                    if (this.content != null)
                    {
                        this.RemoveVisualChild(this.content);
                    }

                    this.content = value;
                    if (this.content != null)
                    {
                        this.AddVisualChild(this.content);
                    }
                }
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        public static AdornerLocation GetAnchor(DependencyObject obj)
        {
            return (AdornerLocation)obj.GetValue(AnnotationAdorner.AnchorProperty);
        }

        public static void SetAnchor(DependencyObject obj, AdornerLocation anchor)
        {
            obj.SetValue(AnnotationAdorner.AnchorProperty, anchor);
        }

        // (3)   |   (2)
        // ______|______
        // (4)   |   (1)
        //       |
        // canvas is divided into four areas by anchorPoint
        // try to figure out which area could fit desiredSize from area1 to area4, if there's no fit, default to area4.
        internal static AdornerLocation FindAnchor(Point anchorPoint, Size desiredSize, Rect canvas)
        {
            Fx.Assert(anchorPoint.X >= canvas.X && anchorPoint.X <= canvas.X + canvas.Width, "X axis of anchorPoint not within canvas");
            Fx.Assert(anchorPoint.Y >= canvas.Y && anchorPoint.Y <= canvas.Y + canvas.Height, "Y axis of anchorPoint not within canvas");

            AdornerLocation anchor = AdornerLocation.None;

            // try area 1 and 2
            if (anchorPoint.X + desiredSize.Width <= canvas.X + canvas.Width)
            {
                // area 1
                if (anchorPoint.Y + desiredSize.Height <= canvas.Y + canvas.Height)
                {
                    anchor = AdornerLocation.BottomRight;
                }

                // area 2
                if (anchor == AdornerLocation.None && anchorPoint.Y - desiredSize.Height >= canvas.Y)
                {
                    anchor = AdornerLocation.TopRight;
                }
            }

            // area 3
            if (anchor == AdornerLocation.None && anchorPoint.X - desiredSize.Width >= canvas.X && anchorPoint.Y - desiredSize.Height >= canvas.Y)
            {
                anchor = AdornerLocation.TopLeft;
            }

            // default to area 4
            if (anchor == AdornerLocation.None)
            {
                anchor = AdornerLocation.BottomLeft;
            }

            return anchor;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.Content;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.content.Measure(constraint);
            return this.content.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point anchorPoint = this.AdornedElement.TranslatePoint(new Point(this.AdornedElement.RenderSize.Width, 0), this.ScrollViewer);

            AdornerLocation anchor = AnnotationAdorner.GetAnchor(this);
            if (anchor == AdornerLocation.None)
            {
                // Calculate based on the real size of the adorner, depending on current zoom level
                DesignerView designerView = ((WorkflowViewElement)AdornedElement).Context.Services.GetService<DesignerView>();
                double zoomLevel = designerView.ZoomFactor;
                Size adornerSize = new Size(this.content.DesiredSize.Width * zoomLevel, this.content.DesiredSize.Height * zoomLevel);
                anchor = FindAnchor(anchorPoint, adornerSize, new Rect(0, 0, this.ScrollViewer.ViewportWidth, this.ScrollViewer.ViewportHeight));
                AnnotationAdorner.SetAnchor(this, anchor);
            }

            Point location = CalculateLocation(anchor, this.AdornedElement.RenderSize, this.content.DesiredSize);

            this.content.Arrange(new Rect(location, finalSize));

            return finalSize;
        }

        private static Point CalculateLocation(AdornerLocation anchor, Size adorneeSize, Size adornerSize)
        {
            Point location = new Point();

            switch (anchor)
            {
                case AdornerLocation.BottomRight:
                    location = new Point(adorneeSize.Width, 0);
                    break;
                case AdornerLocation.TopRight:
                    location = new Point(adorneeSize.Width, -adornerSize.Height);
                    break;
                case AdornerLocation.TopLeft:
                    location = new Point(adorneeSize.Width - adornerSize.Width, -adornerSize.Height);
                    break;
                case AdornerLocation.BottomLeft:
                    location = new Point(adorneeSize.Width - adornerSize.Width, 20);
                    break;
            }

            return location;
        }
    }
}
