//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;

    delegate void ExtensionWindowCloseEventHandler(object sender, RoutedEventArgs e);
    delegate void ExtensionWindowClosingEventHandler(object sender, ExtensionWindowClosingRoutedEventArgs e);

    //This class provides PopupWindow like expirience while editing data on designer surface. It 
    //behaves like ordinary popup, with additional functionality - allows resizing, moving, and 
    //easier styling. 
    [TemplatePart(Name = "PART_Content"), 
     TemplatePart(Name = "PART_ShapeBorder")]
    class ExtensionWindow : ContentControl
    {

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
                "Data",
                typeof(object),
                typeof(ExtensionWindow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnDataChanged)));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ExtensionWindow),
                new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
                "Icon",
                typeof(DrawingBrush),
                typeof(ExtensionWindow),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty ShowWindowHeaderProperty = DependencyProperty.Register(
                "ShowWindowHeader",
                typeof(bool),
                typeof(ExtensionWindow),
                new UIPropertyMetadata(true));

        public static readonly DependencyProperty ShowResizeGripProperty = DependencyProperty.Register(
                "ShowResizeGrip",
                typeof(bool),
                typeof(ExtensionWindow),
                new UIPropertyMetadata(true));

        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
                "MenuItems",
                typeof(ObservableCollection<MenuItem>),
                typeof(ExtensionWindow));

        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
                "IsResizable",
                typeof(bool),
                typeof(ExtensionWindow),
                new UIPropertyMetadata(false));


        public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent("Closing",
                RoutingStrategy.Bubble,
                typeof(ExtensionWindowClosingEventHandler),
                typeof(ExtensionWindow));

        public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent("Close",
                RoutingStrategy.Bubble,
                typeof(ExtensionWindowCloseEventHandler),
                typeof(ExtensionWindow));

        public static readonly RoutedEvent VisibilityChangedEvent = EventManager.RegisterRoutedEvent("VisibilityChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ExtensionWindow));

        static readonly double BorderOffset = 20.0;

        ContentPresenter presenter;
        ExtensionSurface surface;
        Border border;
        ResizeValues resizeOption = ResizeValues.NONE;
        Point bottomRight;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "Overriding metadata for dependency properties in static constructor is the way suggested by WPF")]
        static ExtensionWindow()
        {
            VisibilityProperty.AddOwner(typeof(ExtensionWindow), new PropertyMetadata(OnVisibilityChanged));

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExtensionWindow),
                new FrameworkPropertyMetadata(typeof(ExtensionWindow)));
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is internal code with no derived class")]
        public ExtensionWindow()
        {
            this.MenuItems = new ObservableCollection<MenuItem>();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.MenuItems.CollectionChanged += new NotifyCollectionChangedEventHandler(OnMenuItemsChanged);
            this.DataContext = this;

        }

        public event ExtensionWindowClosingEventHandler Closing
        {
            add
            {
                AddHandler(ClosingEvent, value);
            }
            remove
            {
                RemoveHandler(ClosingEvent, value);
            }
        }


        public event ExtensionWindowCloseEventHandler Close
        {
            add 
            { 
                AddHandler(CloseEvent, value); 
            }
            remove 
            { 
                RemoveHandler(CloseEvent, value); 
            }
        }

        public event RoutedEventHandler VisibilityChanged
        {
            add
            {
                AddHandler(VisibilityChangedEvent, value);
            }
            remove
            {
                RemoveHandler(VisibilityChangedEvent, value);
            }
        }


        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public bool ShowWindowHeader
        {
            get { return (bool)GetValue(ShowWindowHeaderProperty); }
            set { SetValue(ShowWindowHeaderProperty, value); }
        }

        public bool ShowResizeGrip
        {
            get { return (bool)GetValue(ShowResizeGripProperty); }
            set { SetValue(ShowResizeGripProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ObservableCollection<MenuItem> MenuItems
        {
            get { return (ObservableCollection<MenuItem>)GetValue(MenuItemsProperty); }
            private set { SetValue(MenuItemsProperty, value); }
        }

        public ExtensionSurface Surface
        {
            get { return this.surface; }
        }

        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        protected ContentPresenter ContentPresenter
        {
            get { return this.presenter; }
        }

        public Point GetPlacementTargetOffset()
        {           
            Point offset = new Point();
            FrameworkElement target = ExtensionSurface.GetPlacementTarget(this);
            if (null != target)
            {
                FrameworkElement commonRoot = target.FindCommonVisualAncestor(this) as FrameworkElement;
                MatrixTransform transform = (MatrixTransform)target.TransformToAncestor(commonRoot);
                Point targetPosition = transform.Transform(new Point());
                Point windowPosition = ExtensionSurface.GetPosition(this);
                offset.X = targetPosition.X - windowPosition.X;
                offset.Y = targetPosition.Y - windowPosition.Y;                
            }
            return offset;
        }


        static void OnDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ExtensionWindow)sender).OnDataChanged(args.OldValue, args.NewValue);
        }

        static void OnPositionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ExtensionWindow)sender).OnPositionChanged((Point)args.NewValue);
        }

        static void OnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ExtensionWindow)sender).OnVisibilityChanged((Visibility)e.OldValue, (Visibility)e.NewValue);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            ExtensionWindow dummy;
            if (!DesignerProperties.GetIsInDesignMode(this) && !TryGetParentExtensionWindow(this, out dummy, out this.surface))
            {
                Fx.Assert(string.Format(CultureInfo.InvariantCulture, "ExtensionWindow '{0}' cannot be used outside ExtensionSurface", this.Name));
            }
        }

        protected virtual void OnDataChanged(object oldData, object newData)
        {
        }

        protected virtual void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
            RaiseEvent(new RoutedEventArgs(VisibilityChangedEvent, this));
        }

        protected virtual void OnPositionChanged(Point position)
        {
            if (null != this.surface)
            {
                this.surface.SetWindowPosition(this, position);
            }
        }

        public bool TryFindElement(string elementName, out object element)
        {
            //helper method - it looks for named visual elements in the template provided by the user
            element = null;
            if (null != this.presenter)
            {
                element = this.presenter.ContentTemplate.FindName(elementName, this.presenter);
            }
            return (null != element);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //lookup for content presenter (should always be there)
            this.presenter = this.Template.FindName("PART_Content", this) as ContentPresenter;
            if (!DesignerProperties.GetIsInDesignMode(this) && null != this.presenter && null == this.presenter.Content)
            {
                //setup bindings - datacontext and content may possibly change, so i want them beeing set via databinding
                Binding binding = new Binding();
                binding.Source = this;
                this.presenter.SetBinding(ContentPresenter.ContentProperty, binding);
                binding = new Binding();
                binding.Source = this;
                this.presenter.SetBinding(ContentPresenter.DataContextProperty, binding);
                this.presenter.ApplyTemplate();
            }
            this.border = this.Template.FindName("PART_ShapeBorder", this) as Border;
            if (null != this.border)
            {
                this.border.MouseMove += OnBorderMouseMove;
                this.border.MouseDown += OnBorderMouseDown;
                this.border.MouseUp += OnBorderMouseUp;
                this.border.MouseLeave += OnBorderMouseLeave;
            }
        }

        static internal void RaiseWindowCloseEvent(ExtensionWindow sender)
        {
            ExtensionWindowClosingRoutedEventArgs args = new ExtensionWindowClosingRoutedEventArgs(ClosingEvent, sender);
            sender.RaiseEvent(args);
            if (!args.Cancel)
            {
                sender.RaiseEvent(new RoutedEventArgs(CloseEvent, sender));
            }
        }

        void OnBorderMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.border.IsMouseCaptured)
            {
                if (this.border.IsMouseDirectlyOver && this.IsResizable && ExtensionSurface.GetMode(this) == ExtensionSurface.PlacementMode.Absolute)
                {
                    Point position = e.GetPosition(this.border);

                    if (position.X <= BorderOffset && position.Y <= BorderOffset)
                    {
                        this.resizeOption = ResizeValues.TopLeft;
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    }
                    else if (position.X >= this.border.ActualWidth - BorderOffset && position.Y <= BorderOffset)
                    {
                        this.resizeOption = ResizeValues.TopRight;
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                    }
                    else if (position.X <= BorderOffset && position.Y >= this.border.ActualHeight - BorderOffset)
                    {
                        this.resizeOption = ResizeValues.BottomLeft;
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                    }
                    else if (position.X >= this.border.ActualWidth - BorderOffset && position.Y >= this.border.ActualHeight - BorderOffset)
                    {
                        this.resizeOption = ResizeValues.BottomRight;
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    }
                    else if (position.Y <= (BorderOffset / 2.0))
                    {
                        this.resizeOption = ResizeValues.Top;
                        Mouse.OverrideCursor = Cursors.SizeNS;
                    }
                    else if (position.Y >= this.border.ActualHeight - (BorderOffset / 2.0))
                    {
                        this.resizeOption = ResizeValues.Bottom;
                        Mouse.OverrideCursor = Cursors.SizeNS;
                    }
                    else if (position.X <= (BorderOffset / 2.0))
                    {
                        this.resizeOption = ResizeValues.Left;
                        Mouse.OverrideCursor = Cursors.SizeWE;
                    }
                    else if (position.X >= this.border.ActualWidth - (BorderOffset / 2.0))
                    {
                        this.resizeOption = ResizeValues.Right;
                        Mouse.OverrideCursor = Cursors.SizeWE;
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        this.resizeOption = ResizeValues.NONE;
                    }
                    Point topLeft = ExtensionSurface.GetPosition(this);
                    this.bottomRight = new Point(topLeft.X + Width, topLeft.Y + Height);
                }
                else if (Mouse.OverrideCursor != null)
                {
                    Mouse.OverrideCursor = null;
                    this.resizeOption = ResizeValues.NONE;
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.HandleWindowResize();
            }
        }

        void OnBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.resizeOption != ResizeValues.NONE && sender == this.border)
            {
                Mouse.Capture(this.border);
            }
        }

        void OnBorderMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.border.IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void OnBorderMouseLeave(object sender, MouseEventArgs e)
        {
            this.resizeOption = ResizeValues.NONE;
            Mouse.OverrideCursor = null;
        }

        void HandleWindowResize()
        {
            switch (this.resizeOption)
            {
                case ResizeValues.Top:
                    CalculateSize(false, true, false, true);
                    break;

                case ResizeValues.TopLeft:
                    CalculateSize(true, true, true, true);
                    break;

                case ResizeValues.Left:
                    CalculateSize(true, false, true, false);
                    break;

                case ResizeValues.BottomLeft:
                    CalculateSize(true, false, true, true);
                    break;

                case ResizeValues.TopRight:
                    CalculateSize(false, true, true, true);
                    break;

                case ResizeValues.Bottom:
                    CalculateSize(false, false, false, true);
                    break;

                case ResizeValues.Right:
                    CalculateSize(false, false, true, false);
                    break;

                case ResizeValues.BottomRight:
                    CalculateSize(false, false, true, true);
                    break;

                default:
                    Fx.Assert("not supported resize option " + this.resizeOption);
                    break;
            };
        }

        void CalculateSize(bool changeX, bool changeY, bool changeWidth, bool changeHeight)
        {
            Point current = Mouse.GetPosition(this);
            Point absolutePosition = Mouse.GetPosition(this.surface);

            Point topLeft = ExtensionSurface.GetPosition(this);

            double initialHeight = this.bottomRight.Y - topLeft.Y;
            double initialWidth = this.bottomRight.X - topLeft.X;

            Size size = new Size(initialWidth, initialHeight);

            if (changeX)
            {
                if (bottomRight.X > absolutePosition.X)
                {
                    if ((double.IsNaN(MinWidth) || double.IsInfinity(MinWidth) || bottomRight.X - absolutePosition.X >= MinWidth) &&
                        (double.IsNaN(MaxWidth) || double.IsInfinity(MaxWidth) || bottomRight.X - absolutePosition.X <= MaxWidth))
                    {
                        size.Width = this.bottomRight.X - absolutePosition.X;
                        topLeft.X = absolutePosition.X;
                    }
                }
            }
            else
            {
                if (changeWidth)
                {
                    size.Width = Math.Min(Math.Max(MinWidth, current.X), MaxWidth);
                }
            }
            if (changeY)
            {
                if (bottomRight.Y > absolutePosition.Y)
                {
                    if ((double.IsNaN(MinHeight) || double.IsInfinity(MinHeight) || bottomRight.Y - absolutePosition.Y >= MinHeight) &&
                        (double.IsNaN(MaxHeight) || double.IsInfinity(MaxHeight) || bottomRight.Y - absolutePosition.Y <= MaxHeight)) 
                    {
                        size.Height = this.bottomRight.Y - absolutePosition.Y;
                        topLeft.Y = absolutePosition.Y;
                    }
                }
            }
            else
            {
                if (changeHeight)
                {
                    size.Height = Math.Min(Math.Max(MinHeight, current.Y), MaxHeight);
                }
            }
            if (changeX || changeY)
            {
                this.surface.SetWindowPosition(this, topLeft);
            }
            this.surface.SetSize(this, size);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //if ESC - close me
            if (e.Key == Key.Escape)
            {
                RaiseWindowCloseEvent(this);
            }
            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.SelectWindow();
        }



        public void SelectWindow()
        {
            this.surface.SelectWindow(this);
        }

        void OnMenuItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (null != e.NewItems)
            {
                foreach (MenuItem item in e.NewItems)
                {
                    item.DataContext = this;
                }
            }
        }


        internal static bool TryGetParentExtensionWindow(FrameworkElement element, out ExtensionWindow window, out ExtensionSurface surface)
        {
            window = null;
            surface = null;
            if (null != element)
            {
                FrameworkElement current = element;
                window = element.TemplatedParent as ExtensionWindow;
                while (null == window && null != current)
                {
                    window = current as ExtensionWindow;
                    current = (FrameworkElement)current.Parent;
                }
                if (null != window)
                {
                    current = window;
                    surface = window.TemplatedParent as ExtensionSurface;
                    while (null == surface && null != current)
                    {
                        surface = current as ExtensionSurface;
                        current = (FrameworkElement)current.Parent;
                    }
                }
            }
            return (null != window && null != surface);
        }

        enum ResizeValues
        {
            NONE,
            TopLeft,
            Left,
            BottomLeft,
            Bottom,
            BottomRight,
            Right,
            TopRight,
            Top,
        };
    }
}
