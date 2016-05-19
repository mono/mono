//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    //This class is visual representation of WindowHeader like control, which is used with ExtenstionWindows to allow
    //moving and closing. Actual moving logic is handled by ExtensionSurface class
    [TemplatePart(Name = "PART_Header")]
    [TemplatePart(Name = "PART_CloseButton")]
    class ExtensionWindowHeader : Control
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ExtensionWindowHeader));

        public static readonly DependencyProperty ButtonCloseIconProperty =
            DependencyProperty.Register("ButtonCloseIcon", typeof(DrawingBrush), typeof(ExtensionWindowHeader));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(DrawingBrush), typeof(ExtensionWindowHeader));

        public static readonly DependencyProperty DropDownMenuIconProperty =
            DependencyProperty.Register("DropDownMenuIcon", typeof(DrawingBrush), typeof(ExtensionWindowHeader));



        Button closeButton;
        ExtensionWindow parent;
        ExtensionSurface surface;
        Point offset;


        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "Overriding metadata for dependency properties in static constructor is the way suggested by WPF")]
        static ExtensionWindowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExtensionWindowHeader),
                new FrameworkPropertyMetadata(typeof(ExtensionWindowHeader)));
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DrawingBrush ButtonCloseIcon
        {
            get { return (DrawingBrush)GetValue(ButtonCloseIconProperty); }
            set { SetValue(ButtonCloseIconProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DrawingBrush DropDownMenuIcon
        {
            get { return (DrawingBrush)GetValue(DropDownMenuIconProperty); }
            set { SetValue(DropDownMenuIconProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.closeButton = this.Template.FindName("PART_CloseButton", this) as Button;
            if (null != this.closeButton)
            {
                this.closeButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    ExtensionWindow.RaiseWindowCloseEvent(this.parent);
                }
                    );
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonDown(args);
            if (ExtensionSurface.PlacementMode.Absolute == ExtensionSurface.GetMode(this.parent))
            {
                this.offset = Mouse.GetPosition(this);
                Mouse.OverrideCursor = Cursors.Arrow;
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            if (args.LeftButton == MouseButtonState.Pressed && this.IsMouseCaptured)
            {
                Point moveTo = Mouse.GetPosition(this.surface);
                moveTo.Offset(-offset.X, -offset.Y);
                this.surface.SetWindowPosition(this.parent, moveTo);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonUp(args);
            if (this.IsMouseCaptured)
            {
                Mouse.OverrideCursor = null;
                Mouse.Capture(null);
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if (!DesignerProperties.GetIsInDesignMode(this) && 
                !ExtensionWindow.TryGetParentExtensionWindow(this, out this.parent, out this.surface))
            {
                Fx.Assert("ExtensionWindowHeader cannot be used outside ExtensionWindow");                
            }
        }

    }
}
