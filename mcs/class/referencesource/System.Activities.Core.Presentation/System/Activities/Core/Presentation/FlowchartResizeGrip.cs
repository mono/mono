//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation;
    using System.ComponentModel;
    using System.Activities.Presentation.FreeFormEditing;

    //This class is visual representation of ResizeGrip like control, which is used in a Grid to allow resizing.
    class FlowchartResizeGrip : Control
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(DrawingBrush), typeof(FlowchartResizeGrip));

        public static readonly DependencyProperty ParentFlowchartDesignerProperty =
            DependencyProperty.Register("ParentFlowchartDesigner", typeof(FlowchartDesigner), typeof(FlowchartResizeGrip));

        public static readonly DependencyProperty ParentGridProperty =
            DependencyProperty.Register("ParentGrid", typeof(Grid), typeof(FlowchartResizeGrip));

        public static readonly DependencyProperty DisabledProperty =
            DependencyProperty.Register("Disabled", typeof(bool), typeof(FlowchartResizeGrip), new UIPropertyMetadata(false));

        Point offset;

        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public FlowchartDesigner ParentFlowchartDesigner
        {
            get { return (FlowchartDesigner)GetValue(ParentFlowchartDesignerProperty); }
            set { SetValue(ParentFlowchartDesignerProperty, value); }
        }

        public Grid ParentGrid
        {
            get { return (Grid)GetValue(ParentGridProperty); }
            set { SetValue(ParentGridProperty, value); }
        }

        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Cursor = Cursors.SizeNWSE;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!this.Disabled)
            {
                this.offset = e.GetPosition(this);
                this.CaptureMouse();
                ParentFlowchartDesigner.IsResizing = true;
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            if (!this.Disabled)
            {
                if (args.LeftButton == MouseButtonState.Pressed && this.IsMouseCaptured)
                {
                    FlowchartDesigner flowchartDesigner = this.ParentFlowchartDesigner;
                    FreeFormPanel panel = flowchartDesigner.panel;
                    Grid flowchartGrid = this.ParentGrid;
                    Point currentPosition = Mouse.GetPosition(flowchartGrid);

                    currentPosition.Offset(this.offset.X, this.offset.Y);

                    flowchartDesigner.FlowchartWidth = Math.Min(Math.Max(panel.RequiredWidth, currentPosition.X), flowchartGrid.MaxWidth);
                    flowchartDesigner.FlowchartHeight = Math.Min(Math.Max(panel.RequiredHeight, currentPosition.Y), flowchartGrid.MaxHeight);
                    args.Handled = true;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!this.Disabled)
            {
                FlowchartDesigner flowchartDesigner = this.ParentFlowchartDesigner;
                ModelItem flowchartModelItem = this.ParentFlowchartDesigner.ModelItem;
                using (ModelEditingScope scope = flowchartModelItem.BeginEdit(SR.FCResizeUndoUnitName))
                {
                    TypeDescriptor.GetProperties(flowchartModelItem)[FlowchartSizeFeature.WidthPropertyName].SetValue(flowchartModelItem, flowchartDesigner.FlowchartWidth);
                    TypeDescriptor.GetProperties(flowchartModelItem)[FlowchartSizeFeature.HeightPropertyName].SetValue(flowchartModelItem, flowchartDesigner.FlowchartHeight);
                    scope.Complete();
                }
                Mouse.OverrideCursor = null;
                Mouse.Capture(null);
                ParentFlowchartDesigner.IsResizing = false;
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }
    }
}
