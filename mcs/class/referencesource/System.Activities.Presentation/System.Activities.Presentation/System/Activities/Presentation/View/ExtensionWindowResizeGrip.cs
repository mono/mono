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

    //This class is visual representation of ResizeGrip like control, which is used with ExtenstionWindows to allow
    //resizing. Actual resize logic is handled by ExtensionSurface class
    [TemplatePart(Name = "PART_ResizeGrip")]
    class ExtensionWindowResizeGrip : Control
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(DrawingBrush), typeof(ExtensionWindowResizeGrip));

        ExtensionWindow parent;
        ExtensionSurface surface;
        Point offset;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "Overriding metadata for dependency properties in static constructor is the way suggested by WPF")]
        static ExtensionWindowResizeGrip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExtensionWindowResizeGrip),
                new FrameworkPropertyMetadata(typeof(ExtensionWindowResizeGrip)));            
        }

        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (this.parent.IsResizable)
            {
                this.Cursor = Cursors.SizeNWSE;
                this.offset = e.GetPosition(this);
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            if (args.LeftButton == MouseButtonState.Pressed && this.IsMouseCaptured)
            {
                Point currentPosition = Mouse.GetPosition(this.parent);
                currentPosition.Offset(this.offset.X, this.offset.Y);
                Size newSize = new Size();
                newSize.Width = Math.Min(Math.Max(this.parent.MinWidth, currentPosition.X), this.parent.MaxWidth);
                newSize.Height = Math.Min(Math.Max(this.parent.MinHeight, currentPosition.Y), this.parent.MaxHeight);
                System.Diagnostics.Debug.WriteLine("NewSize = (" + newSize.Width + "," + newSize.Height + ")");
                this.surface.SetSize(this.parent, newSize);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            Mouse.OverrideCursor = null;
            Mouse.Capture(null);
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
