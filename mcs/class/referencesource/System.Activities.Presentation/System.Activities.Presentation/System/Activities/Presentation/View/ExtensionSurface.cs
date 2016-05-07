//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Globalization;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    //This class is responsible for providing functionality to display additional information in context of
    //the designer view in a popup-like manner. It is basically the canvas control, which is placed on top of 
    //the other visual elements. It provides functionality to add and remove extension windows, as well as manipulating
    //their position and size
    sealed class ExtensionSurface : Panel
    {

        public static readonly DependencyProperty DesignerProperty = DependencyProperty.Register(
            "Designer",
            typeof(DesignerView),
            typeof(ExtensionSurface),
            new PropertyMetadata(OnDesignerChanged));

        public static readonly DependencyProperty AutoExpandCanvasProperty = DependencyProperty.Register(
            "AutoExpandCanvas",
            typeof(bool),
            typeof(ExtensionSurface),
            new UIPropertyMetadata(false));

        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.RegisterAttached(
            "PlacementTarget",
            typeof(FrameworkElement),
            typeof(ExtensionSurface),
            new UIPropertyMetadata(null, OnPlacementTargetChanged));

        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.RegisterAttached(
            "Alignment",
            typeof(PositionAlignment),
            typeof(ExtensionSurface),
            new UIPropertyMetadata(PositionAlignment.LeftTop));

        public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached(
            "Mode",
            typeof(PlacementMode),
            typeof(ExtensionSurface),
            new UIPropertyMetadata(PlacementMode.Absolute, OnPlacementModeChanged));

        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached(
            "Position",
            typeof(Point),
            typeof(ExtensionSurface),
            new UIPropertyMetadata(new Point()));


        Func<double, double, double, bool> IsGreater;

        KeyValuePair<FrameworkElement, Point> selectedChild;
        Size rearangeStartSize = new Size();
        Rect actualPanelRect = new Rect(0, 0, 0, 0);
        Point canvasOffset = new Point();
        int currentZIndex = 1000;

        public ExtensionSurface()
        {
            //add global handled for ExtensionWindow's CloseEvent 
            this.AddHandler(ExtensionWindow.CloseEvent, new RoutedEventHandler(OnExtensionWindowClosed));
            this.ClipToBounds = true;
            this.IsGreater = (v1, v2, v3) => (v1 + v2 > v3);
        }

        [Fx.Tag.KnownXamlExternal]
        public DesignerView Designer
        {
            get { return (DesignerView)GetValue(DesignerProperty); }
            set { SetValue(DesignerProperty, value); }
        }

        public bool AutoExpandCanvas
        {
            get { return (bool)GetValue(AutoExpandCanvasProperty); }
            set { SetValue(AutoExpandCanvasProperty, value); }
        }

        static void OnPlacementModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ExtensionWindow window = sender as ExtensionWindow;
            if (null != window && null != window.Surface && window.Visibility == Visibility.Visible)
            {
                window.Surface.PlaceWindow(window);
            }
        }

        static void OnPlacementTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
        }


        //hook for designer mouse events - they are required to handle positioning and resizing
        static void OnDesignerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ExtensionSurface ctrl = (ExtensionSurface)sender;
            DesignerView designer;
            if (null != args.OldValue)
            {
                designer = (DesignerView)args.OldValue;
            }
            if (null != args.NewValue)
            {
                designer = (DesignerView)args.NewValue;
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            ExtensionWindow window = visualRemoved as ExtensionWindow;
            if (null != window)
            {
                window.VisibilityChanged -= OnWindowVisibilityChanged;
                // window.SizeChanged -= OnWindowSizeChanged;
                this.rearangeStartSize.Width = 0;
                this.rearangeStartSize.Height = 0;
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            window = visualAdded as ExtensionWindow;
            if (null != window)
            {
                window.VisibilityChanged += OnWindowVisibilityChanged;
                // window.SizeChanged += OnWindowSizeChanged;
                if (!window.IsLoaded)
                {
                    window.Loaded += OnChildWindowLoaded;
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            foreach (FrameworkElement child in this.Children)
            {
                ExtensionWindow window = child as ExtensionWindow;
                if (null != window)
                {
                    if (PlacementMode.Relative == GetMode(window) && null != GetPlacementTarget(window))
                    {
                        this.PlaceWindow(window);
                        continue;
                    }
                    if (!this.AutoExpandCanvas)
                    {
                        this.EnsureWindowIsVisible(window);
                    }
                }
            }
        }

        void OnChildWindowLoaded(object sender, EventArgs e)
        {
            ExtensionWindow window = (ExtensionWindow)sender;
            this.OnWindowVisibilityChanged(window, null);
            window.Loaded -= OnChildWindowLoaded;
        }

        //void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    ExtensionWindow window = (ExtensionWindow)sender;
        //   // EnsureWindowIsVisible(window);
        //}

        void OnWindowVisibilityChanged(object sender, RoutedEventArgs args)
        {
            ExtensionWindow window = (ExtensionWindow)sender;
            if (window.IsVisible)
            {
                Func<double, bool> IsInvalid = x => (double.IsInfinity(x) || double.IsNaN(x) || double.Epsilon > x);

                if (IsInvalid(window.ActualWidth) || IsInvalid(window.ActualWidth) || IsInvalid(window.DesiredSize.Width) || IsInvalid(window.DesiredSize.Height))
                {
                    window.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
                PlaceWindow(window);
            }
        }

        void PlaceWindow(ExtensionWindow window)
        {
            if (null != window)
            {
                FrameworkElement target = ExtensionSurface.GetPlacementTarget(window);
                PositionAlignment alignment = ExtensionSurface.GetAlignment(window);
                PlacementMode mode = ExtensionSurface.GetMode(window);
                Point position = ExtensionSurface.GetPosition(window);

                Point calculatedPosition = new Point();
                FrameworkElement commonRoot = null;
                MatrixTransform transform = null;

                switch (mode)
                {
                    case PlacementMode.Relative:
                        if (null != target)
                        {
                            commonRoot = target.FindCommonVisualAncestor(this) as FrameworkElement;
                            if (null == commonRoot)
                            {
                                return;
                            }
                            transform = (MatrixTransform)target.TransformToAncestor(commonRoot);
                        }
                        else
                        {
                            if (!DesignerProperties.GetIsInDesignMode(this))
                            {
                                Fx.Assert(string.Format(CultureInfo.InvariantCulture, "PlacementTarget must be set in RelativeMode on ExtensionSurface '{0}'", this.Name));
                            }
                        }
                        break;

                    case PlacementMode.Absolute:
                        calculatedPosition = position;
                        break;

                    default:
                        Fx.Assert(string.Format(CultureInfo.CurrentCulture, "ExtensionWindowPlacement.Mode {0} specified in ExtensionWindow '{1}' is not supported for ExtensionSurface", mode, window.Name));
                        return;
                }

                if (PlacementMode.Relative == mode)
                {
                    if (null != target)
                    {
                        double x;
                        double y;
                        switch (alignment)
                        {
                            case PositionAlignment.LeftTop:
                                calculatedPosition = transform.Transform(calculatedPosition);
                                break;

                            case PositionAlignment.LeftBottom:
                                calculatedPosition = transform.Transform(new Point(0.0, target.ActualHeight));
                                break;

                            case PositionAlignment.RightTop:
                                calculatedPosition = transform.Transform(new Point(target.ActualWidth, 0.0));
                                break;

                            case PositionAlignment.RightBottom:
                                calculatedPosition = transform.Transform(new Point(target.ActualWidth, target.ActualHeight));
                                break;

                            case PositionAlignment.Center:
                                calculatedPosition = transform.Transform(calculatedPosition);
                                x = ((target.ActualWidth * transform.Matrix.M11) - window.Width) / 2.0;
                                y = ((target.ActualHeight * transform.Matrix.M22) - window.Height) / 2.0;
                                calculatedPosition.Offset(x, y);
                                break;

                            case PositionAlignment.CenterHorizontal:
                                calculatedPosition = transform.Transform(calculatedPosition);
                                x = ((target.ActualWidth * transform.Matrix.M11) - window.Width) / 2.0;
                                calculatedPosition.Offset(x, 0.0);
                                break;

                            case PositionAlignment.CenterVertical:
                                calculatedPosition = transform.Transform(calculatedPosition);
                                y = ((target.ActualHeight * transform.Matrix.M22) - window.Height) / 2.0;
                                calculatedPosition.Offset(0.0, y);
                                break;

                            default:
                                Fx.Assert(string.Format(CultureInfo.CurrentCulture, "ExtensionWindowPlacement.Position = '{0}' is not supported", alignment));
                                return;
                        }
                    }
                }
                SetWindowPosition(window, calculatedPosition);
            }
        }

        internal void SetWindowPosition(ExtensionWindow window, Point position)
        {
            Func<double, double, double, double, double> CalculateInBoundsValue =
                (pos, size, limit, modifier) =>
                {
                    if (this.AutoExpandCanvas)
                    {
                        return pos - modifier;
                    }
                    else
                    {
                        pos = Math.Max(0.0, pos);
                        return pos + size > limit ? limit - size : pos;
                    }
                };

            //in case of AutoExpandCanvas == false:
            // - do not allow placing window outside surface bounds
            //in case of AutoExpandCanvas == true:
            // - include possible negative canvas offset
            position.X = CalculateInBoundsValue(position.X, window.DesiredSize.Width, this.ActualWidth, this.selectedChild.Value.X);
            position.Y = CalculateInBoundsValue(position.Y, window.DesiredSize.Height, this.ActualHeight, this.selectedChild.Value.Y);

            //update its position on canvas
            ExtensionSurface.SetPosition(window, position);

            bool requiresMeasure = false;
            if (this.AutoExpandCanvas)
            {
                requiresMeasure = true;
                this.canvasOffset.X = 0;
                this.canvasOffset.Y = 0;

                foreach (UIElement item in this.Children)
                {
                    FrameworkElement child = item as FrameworkElement;
                    if (null != child)
                    {
                        Point p = ExtensionSurface.GetPosition(child);
                        this.canvasOffset.X = Math.Min(this.canvasOffset.X, p.X);
                        this.canvasOffset.Y = Math.Min(this.canvasOffset.Y, p.Y);
                    }
                }
                this.canvasOffset.X = Math.Abs(this.canvasOffset.X);
                this.canvasOffset.Y = Math.Abs(this.canvasOffset.Y);
            }
            if (requiresMeasure)
            {
                this.InvalidateMeasure();
            }
            else
            {
                this.InvalidateArrange();
            }
        }

        void EnsureWindowIsVisible(ExtensionWindow window)
        {
            SetWindowPosition(window, ExtensionSurface.GetPosition(window));
        }

        internal void SetSize(ExtensionWindow window, Size size)
        {
            Point pos = ExtensionSurface.GetPosition(window);
            if (!this.AutoExpandCanvas)
            {
                if (IsGreater(pos.X, size.Width, this.ActualWidth))
                {
                    size.Width = this.ActualWidth - pos.X;
                }
                if (IsGreater(pos.Y, size.Height, this.ActualHeight))
                {
                    size.Height = this.ActualHeight - pos.Y;
                }
            }
            System.Diagnostics.Debug.WriteLine("SetSize oldSize (" + window.Width + "," + window.Height + ") newSize (" + size.Width + "," + size.Height + ")");
            window.Width = size.Width;
            window.Height = size.Height;
            if (this.AutoExpandCanvas)
            {
                // this.InvalidateMeasure();
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement child in this.Children)
            {
                //get (left, top) coorinates
                Point pos = ExtensionSurface.GetPosition(child);
                //include eventual negative offset (panel wouldn't display elements with negative coorinates by default)
                pos.Offset(this.canvasOffset.X, this.canvasOffset.Y);
                //request child to rearange itself in given rectangle
                child.Arrange(new Rect(pos, child.DesiredSize));
            }
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "ArrangeOverride Size({0},{1})", arrangeSize.Width, arrangeSize.Height));
            return arrangeSize;

        }


        protected override Size MeasureOverride(Size constraint)
        {
            Size result;

            if (this.AutoExpandCanvas)
            {
                double panelWidth = 0.0;
                double panelHeight = 0.0;

                //initially assume that whole content fits in rectangle with coordinates (0,0, ActualWidth, ActualHeight)
                double offsetMinusX = 0.0;
                double offsetMinusY = 0.0;
                double offsetPlusX = this.rearangeStartSize.Width;
                double offsetPlusY = this.rearangeStartSize.Height;

                foreach (UIElement item in this.Children)
                {
                    FrameworkElement child = item as FrameworkElement;
                    if (null != child)
                    {
                        child.Measure(constraint);

                        //get child's position
                        Point pos = ExtensionSurface.GetPosition(child);

                        //calculate the minimum value of panel's (left,top) corner
                        offsetMinusX = Math.Min(offsetMinusX, pos.X);
                        offsetMinusY = Math.Min(offsetMinusY, pos.Y);

                        //calculate the maximum value of panel's (right, bottom) corner
                        offsetPlusX = Math.Max(offsetPlusX, pos.X + child.DesiredSize.Width);
                        offsetPlusY = Math.Max(offsetPlusY, pos.Y + child.DesiredSize.Height);
                    }
                }

                //get required panel's width and height
                panelWidth = Math.Abs(offsetPlusX - offsetMinusX);
                panelHeight = Math.Abs(offsetPlusY - offsetMinusY);

                this.actualPanelRect.Location = new Point(offsetMinusX, offsetMinusY);
                this.actualPanelRect.Size = new Size(panelWidth, panelHeight);

                //return it as result
                result = new Size(panelWidth, panelHeight);
            }
            else
            {
                result = base.MeasureOverride(constraint);
            }
            System.Diagnostics.Debug.WriteLine("MO constraint:" + constraint.Width + "," + constraint.Height + " new: " + result.Width + "," + result.Height);
            return result;
        }

        public void SelectWindow(ExtensionWindow window)
        {
            if (null != window && this.Children.Contains(window))
            {
                this.selectedChild = new KeyValuePair<FrameworkElement, Point>(window, this.canvasOffset);
                this.rearangeStartSize.Width = this.ActualWidth;
                this.rearangeStartSize.Height = this.ActualHeight;
                Panel.SetZIndex(window, ++this.currentZIndex);
            }
        }

        void OnExtensionWindowClosed(object sender, RoutedEventArgs args)
        {
            ExtensionWindow window = args.Source as ExtensionWindow;

            if (null != window)
            {
                //remove window from children collection
                this.Children.Remove(window);
            }
        }

        public static void SetPlacementTarget(DependencyObject container, FrameworkElement value)
        {
            container.SetValue(PlacementTargetProperty, value);
        }

        public static FrameworkElement GetPlacementTarget(DependencyObject container)
        {
            return (FrameworkElement)container.GetValue(PlacementTargetProperty);
        }

        public static void SetAlignment(DependencyObject container, PositionAlignment value)
        {
            container.SetValue(AlignmentProperty, value);
        }

        public static PositionAlignment GetAlignment(DependencyObject container)
        {
            return (PositionAlignment)container.GetValue(AlignmentProperty);
        }

        public static void SetMode(DependencyObject container, PlacementMode value)
        {
            container.SetValue(ModeProperty, value);
        }

        public static PlacementMode GetMode(DependencyObject container)
        {
            return (PlacementMode)container.GetValue(ModeProperty);
        }

        public static void SetPosition(DependencyObject container, Point value)
        {
            container.SetValue(PositionProperty, value);
        }

        public static Point GetPosition(DependencyObject container)
        {
            return (Point)container.GetValue(PositionProperty);
        }
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Suppress to avoid unnecessary changes.")]
        public enum PlacementMode
        {
            Relative, Absolute
        }
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Suppress to avoid unnecessary changes.")]
        public enum PositionAlignment
        {
            LeftTop, LeftBottom, RightTop, RightBottom, Center, CenterHorizontal, CenterVertical
        };
    }
}
