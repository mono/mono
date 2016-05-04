//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
#if DEBUG
//#define MINIMAP_DEBUG
#endif

namespace System.Activities.Presentation
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Diagnostics;
    using System.Windows.Threading;
    using System.Globalization;

    // This class is a control displaying minimap of the attached scrollableview control
    // this class's functionality is limited to delegating events to minimap view controller

    partial class MiniMapControl : UserControl
    {
        public static readonly DependencyProperty MapSourceProperty =
                DependencyProperty.Register("MapSource",
                typeof(ScrollViewer),
                typeof(MiniMapControl),
                new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnMapSourceChanged)));

        MiniMapViewController lookupWindowManager;
        bool isMouseDown = false;

        public MiniMapControl()
        {
            InitializeComponent();
            this.lookupWindowManager = new MiniMapViewController(this.lookupCanvas, this.lookupWindow, this.contentGrid);
        }

        public ScrollViewer MapSource
        {
            get { return GetValue(MapSourceProperty) as ScrollViewer; }
            set { SetValue(MapSourceProperty, value); }
        }

        static void OnMapSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            MiniMapControl mapControl = (MiniMapControl)sender;
            mapControl.lookupWindowManager.MapSource = mapControl.MapSource;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (this.lookupWindowManager.StartMapLookupDrag(e))
            {
                this.CaptureMouse();
                this.isMouseDown = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.isMouseDown)
            {
                this.lookupWindowManager.DoMapLookupDrag(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (this.isMouseDown)
            {
                Mouse.Capture(null);
                this.isMouseDown = false;
                this.lookupWindowManager.StopMapLookupDrag();
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            this.lookupWindowManager.CenterView(e);
            e.Handled = true;
            base.OnMouseDoubleClick(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.lookupWindowManager.MapViewSizeChanged(sizeInfo);
        }

        // This class wraps positioning and calculating logic of the map view lookup window
        // It is also responsible for handling mouse movements

        internal class LookupWindow
        {
            Point mousePosition;
            Rectangle lookupWindowRectangle;
            MiniMapViewController parent;


            public LookupWindow(MiniMapViewController parent, Rectangle lookupWindowRectangle)
            {
                this.mousePosition = new Point();
                this.parent = parent;
                this.lookupWindowRectangle = lookupWindowRectangle;
            }

            public double Left
            {
                get { return Canvas.GetLeft(this.lookupWindowRectangle); }
                set
                {
                    //check if left corner is within minimap's range - clip if necessary
                    double left = Math.Max(value - this.mousePosition.X, 0.0);
                    //check if right corner is within minimap's range - clip if necessary
                    left = (left + Width > this.parent.MapWidth ? this.parent.MapWidth - Width : left);
                    //update canvas
                    Canvas.SetLeft(this.lookupWindowRectangle, left);
                }
            }

            public double Top
            {
                get { return Canvas.GetTop(this.lookupWindowRectangle); }
                set
                {
                    //check if top corner is within minimap's range - clip if necessary
                    double top = Math.Max(value - this.mousePosition.Y, 0.0);
                    //check if bottom corner is within minimap's range - clip if necessary
                    top = (top + Height > this.parent.MapHeight ? this.parent.MapHeight - Height : top);
                    //update canvas
                    Canvas.SetTop(this.lookupWindowRectangle, top);
                }
            }

            public double Width
            {
                get { return this.lookupWindowRectangle.Width; }
                set { this.lookupWindowRectangle.Width = value; }
            }

            public double Height
            {
                get { return this.lookupWindowRectangle.Height; }
                set { this.lookupWindowRectangle.Height = value; }
            }

            public double MapCenterXPoint
            {
                get { return this.Left + (this.Width / 2.0); }
            }

            public double MapCenterYPoint
            {
                get { return this.Top + (this.Height / 2.0); }
            }

            public double MousePositionX
            {
                get { return this.mousePosition.X; }
            }

            public double MousePositionY
            {
                get { return this.mousePosition.Y; }
            }

            public bool IsSelected
            {
                get;
                private set;
            }

            public void SetPosition(double left, double top)
            {
                Left = left;
                Top = top;
            }

            public void SetSize(double width, double height)
            {
                Width = width;
                Height = height;
            }

            //whenever user clicks on the minimap, i check if clicked object is 
            //a lookup window - if yes - i store mouse offset within the window
            //and mark it as selected
            public bool Select(object clickedItem, Point clickedPosition)
            {
                if (clickedItem == this.lookupWindowRectangle)
                {
                    this.mousePosition = clickedPosition;
                    this.IsSelected = true;
                }
                else
                {
                    Unselect();
                }
                return this.IsSelected;
            }

            public void Unselect()
            {
                this.mousePosition.X = 0;
                this.mousePosition.Y = 0;
                this.IsSelected = false;
            }

            public void Center(double x, double y)
            {
                Left = x - (Width / 2.0);
                Top = y - (Height / 2.0);
            }

            public void Refresh(bool unselect)
            {
                if (unselect)
                {
                    Unselect();
                }
                SetPosition(Left, Top);
            }
        }

        // This class is responsible for calculating size of the minimap's view area, as well as
        // maintaining the bi directional link between minimap and control beeing visualized.
        // Whenever minimap's view window position is updated, the control's content is scrolled 
        // to calculated position
        // Whenever control's content is resized or scrolled, minimap reflects that change in 
        // recalculating view's window size and/or position

        internal class MiniMapViewController
        {
            Canvas lookupCanvas;
            Grid contentGrid;
            ScrollViewer mapSource;
            LookupWindow lookupWindow;

            public MiniMapViewController(Canvas lookupCanvas, Rectangle lookupWindowRectangle, Grid contentGrid)
            {
                this.lookupWindow = new LookupWindow(this, lookupWindowRectangle);
                this.lookupCanvas = lookupCanvas;
                this.contentGrid = contentGrid;
            }

            public ScrollViewer MapSource
            {
                get { return this.mapSource; }
                set
                {
                    this.mapSource = value;
                    //calculate view's size and set initial position
                    this.lookupWindow.Unselect();
                    this.CalculateLookupWindowSize();
                    this.lookupWindow.SetPosition(0.0, 0.0);
                    CalculateMapPosition(this.lookupWindow.Left, this.lookupWindow.Top);
                    this.UpdateContentGrid();

                    if (null != this.mapSource && null != this.mapSource.Content && this.mapSource.Content is FrameworkElement)
                    {
                        FrameworkElement content = (FrameworkElement)this.mapSource.Content;
                        //hook up for all content size changes - handle them in OnContentSizeChanged method
                        content.SizeChanged += (s, e) =>
                            {
                                this.contentGrid.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                    new Action(() => { OnContentSizeChanged(s, e); }));
                            };

                        //in case of scroll viewer - there are two different events to handle in one notification:
                        this.mapSource.ScrollChanged += (s, e) =>
                        {
                            this.contentGrid.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                new Action(() =>
                                    {
                                        //when user changes scroll position - delegate it to OnMapSourceScrollChange
                                        if (0.0 != e.HorizontalChange || 0.0 != e.VerticalChange)
                                        {
                                            OnMapSourceScrollChanged(s, e);
                                        }
                                        //when size of the scroll changes delegate it to OnContentSizeChanged
                                        if (0.0 != e.ViewportWidthChange || 0.0 != e.ViewportHeightChange)
                                        {
                                            OnContentSizeChanged(s, e);
                                        }
                                    }));
                        };
                        this.OnMapSourceScrollChanged(this, null);
                        this.OnContentSizeChanged(this, null);
                    }
                }
            }

            //bunch of helper getters - used to increase algorithm readability and provide default
            //values, always valid values, so no additional divide-by-zero checks are neccessary

            public double MapWidth
            {
                get { return this.contentGrid.ActualWidth - 2 * (this.contentGrid.ColumnDefinitions[0].MinWidth); }
            }

            public double MapHeight
            {
                get { return this.contentGrid.ActualHeight - 2 * (this.contentGrid.RowDefinitions[0].MinHeight); }
            }

            internal LookupWindow LookupWindow
            {
                get { return this.lookupWindow; }
            }

            double VisibleSourceWidth
            {
                get { return (null == MapSource || 0.0 == MapSource.ViewportWidth ? 1.0 : MapSource.ViewportWidth); }
            }

            double VisibleSourceHeight
            {
                get { return (null == MapSource || 0.0 == MapSource.ViewportHeight ? 1.0 : MapSource.ViewportHeight); }
            }


            public void CenterView(MouseEventArgs args)
            {
                Point pt = args.GetPosition(this.lookupCanvas);
                this.lookupWindow.Unselect();
                this.lookupWindow.Center(pt.X, pt.Y);
                CalculateMapPosition(this.lookupWindow.Left, this.lookupWindow.Top);
            }

            public void MapViewSizeChanged(SizeChangedInfo sizeInfo)
            {
                this.OnContentSizeChanged(this, EventArgs.Empty);
                this.lookupWindow.Unselect();
                this.CalculateLookupWindowSize();
                if (sizeInfo.WidthChanged && 0.0 != sizeInfo.PreviousSize.Width)
                {
                    this.lookupWindow.Left =
                        this.lookupWindow.Left * (sizeInfo.NewSize.Width / sizeInfo.PreviousSize.Width);
                }
                if (sizeInfo.HeightChanged && 0.0 != sizeInfo.PreviousSize.Height)
                {
                    this.lookupWindow.Top =
                        this.lookupWindow.Top * (sizeInfo.NewSize.Height / sizeInfo.PreviousSize.Height);
                }
            }

            public bool StartMapLookupDrag(MouseEventArgs args)
            {
                bool result = false;
                HitTestResult hitTest =
                    VisualTreeHelper.HitTest(this.lookupCanvas, args.GetPosition(this.lookupCanvas));

                if (null != hitTest && null != hitTest.VisualHit)
                {
                    Point clickedPosition = args.GetPosition(hitTest.VisualHit as IInputElement);
                    result = this.lookupWindow.Select(hitTest.VisualHit, clickedPosition);
                }
                return result;
            }

            public void StopMapLookupDrag()
            {
                this.lookupWindow.Unselect();
            }

            public void DoMapLookupDrag(MouseEventArgs args)
            {
                if (args.LeftButton == MouseButtonState.Released && this.lookupWindow.IsSelected)
                {
                    this.lookupWindow.Unselect();
                }
                if (this.lookupWindow.IsSelected)
                {
                    Point to = args.GetPosition(this.lookupCanvas);
                    this.lookupWindow.SetPosition(to.X, to.Y);
                    CalculateMapPosition(
                        to.X - this.lookupWindow.MousePositionX,
                        to.Y - this.lookupWindow.MousePositionY);
                }
            }

            void CalculateMapPosition(double left, double top)
            {
                if (null != MapSource && 0 != this.lookupWindow.Width && 0 != this.lookupWindow.Height)
                {
                    MapSource.ScrollToHorizontalOffset((left / this.lookupWindow.Width) * VisibleSourceWidth);
                    MapSource.ScrollToVerticalOffset((top / this.lookupWindow.Height) * VisibleSourceHeight);
                }
            }

            //this method calculates position of the lookup window on the minimap - it should be triggered when:
            // - user modifies a scroll position by draggin a scroll bar
            // - scroll sizes are updated by change of the srcollviewer size
            // - user drags minimap view - however, in this case no lookup update takes place
            void OnMapSourceScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                if (!this.lookupWindow.IsSelected && null != MapSource)
                {
                    this.lookupWindow.Unselect();
                    this.lookupWindow.Left =
                        this.lookupWindow.Width * (MapSource.HorizontalOffset / VisibleSourceWidth);

                    this.lookupWindow.Top =
                        this.lookupWindow.Height * (MapSource.VerticalOffset / VisibleSourceHeight);
                }
                DumpData("OnMapSourceScrollChange");
            }

            //this method calculates size and position of the minimap view - it should be triggered when:
            // - zoom changes
            // - visible size of the scrollviewer (which is map source) changes
            // - visible size of the minimap control changes 
            void OnContentSizeChanged(object sender, EventArgs e)
            {
                //get old center point coordinates
                double centerX = this.lookupWindow.MapCenterXPoint;
                double centeryY = this.lookupWindow.MapCenterYPoint;
                //update the minimap itself
                this.UpdateContentGrid();
                //calculate new size
                this.CalculateLookupWindowSize();
                //try to center around old center points (window may be moved if doesn't fit)
                this.lookupWindow.Center(centerX, centeryY);
                DumpData("OnContentSizeChanged");
            }

            //this method calculates size of the lookup rectangle, based on the visible size of the object, 
            //including current map width
            void CalculateLookupWindowSize()
            {
                double width = this.MapWidth;
                double height = this.MapHeight;

                if (this.MapSource.ScrollableWidth != 0 && this.MapSource.ExtentWidth != 0)
                {
                    width = (this.MapSource.ViewportWidth / this.MapSource.ExtentWidth) * this.MapWidth;
                }
                else
                {
                    //width = 
                }
                if (this.MapSource.ScrollableHeight != 0 && this.MapSource.ExtentHeight != 0)
                {
                    height = (this.MapSource.ViewportHeight / this.MapSource.ExtentHeight) * this.MapHeight;
                }
                this.lookupWindow.SetSize(width, height);
            }

            //this method updates content grid of the minimap - most likely, minimap view will be scaled to fit
            //the window - so there will be some extra space visible on the left and right sides or above and below actual
            //mini map view - we don't want lookup rectangle to navigate within that area, since it is not representing
            //actual view - we increase margins of the minimap to disallow this
            void UpdateContentGrid()
            {
                bool resetToDefault = true;
                if (this.MapSource.ExtentWidth != 0 && this.MapSource.ExtentHeight != 0)
                {
                    //get width to height ratio from map source - we want to display our minimap in the same ratio
                    double widthToHeightRatio = this.MapSource.ExtentWidth / this.MapSource.ExtentHeight;

                    //calculate current width to height ratio on the minimap
                    double height = this.contentGrid.ActualHeight;
                    double width = this.contentGrid.ActualWidth;
                    //ideally - it should be 1 - whole view perfectly fits minimap 
                    double minimapWidthToHeightRatio = (height * widthToHeightRatio) / (width > 1.0 ? width : 1.0);

                    //if value is greater than one - we have to reduce height
                    if (minimapWidthToHeightRatio > 1.0)
                    {
                        double margin = (height - (height / minimapWidthToHeightRatio)) / 2.0;

                        this.contentGrid.ColumnDefinitions[0].MinWidth = 0.0;
                        this.contentGrid.ColumnDefinitions[2].MinWidth = 0.0;
                        this.contentGrid.RowDefinitions[0].MinHeight = margin;
                        this.contentGrid.RowDefinitions[2].MinHeight = margin;
                        resetToDefault = false;
                    }
                    //if value is less than one - we have to reduce width
                    else if (minimapWidthToHeightRatio < 1.0)
                    {
                        double margin = (width - (width * minimapWidthToHeightRatio)) / 2.0;
                        this.contentGrid.ColumnDefinitions[0].MinWidth = margin;
                        this.contentGrid.ColumnDefinitions[2].MinWidth = margin;
                        this.contentGrid.RowDefinitions[0].MinHeight = 0.0;
                        this.contentGrid.RowDefinitions[2].MinHeight = 0.0;
                        resetToDefault = false;
                    }
                }
                //perfect match or nothing to display - no need to setup margins
                if (resetToDefault)
                {
                    this.contentGrid.ColumnDefinitions[0].MinWidth = 0.0;
                    this.contentGrid.ColumnDefinitions[2].MinWidth = 0.0;
                    this.contentGrid.RowDefinitions[0].MinHeight = 0.0;
                    this.contentGrid.RowDefinitions[2].MinHeight = 0.0;
                }
            }

            [Conditional("MINIMAP_DEBUG")]
            void DumpData(string prefix)
            {
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} ScrollViewer: EWidth {1}, EHeight {2}, AWidth {3}, AHeight {4}, ViewPortW {5} ViewPortH {6}", prefix, mapSource.ExtentWidth, mapSource.ExtentHeight, mapSource.ActualWidth, mapSource.ActualHeight, mapSource.ViewportWidth, mapSource.ViewportHeight));
            }

        }
    }
}
