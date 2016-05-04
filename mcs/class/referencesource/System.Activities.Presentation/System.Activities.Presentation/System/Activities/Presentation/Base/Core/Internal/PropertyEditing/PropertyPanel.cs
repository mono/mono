//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    // <summary>
    // PropertyPanel is a simplified version of a horizontal StackPanel that we use for PropertyContainer
    // visuals in lieu of Grid, which was too heavy-weight and bogging down perf.  It exposes a property,
    // LastChildWidth, that specifies the forced width of the last child in the panel.  All other
    // children are stacked on the left and eat up the remainder of the space left on the panel.
    //
    // The panel also deals with drawing compartments for itself and the last child and it deals with
    // visually nesting sub-properties based on their depth (Level).
    // </summary>
    internal class PropertyPanel : Panel 
    {

        // LastChildWidth DP

        public static readonly DependencyProperty OutlineBrushProperty =
            DependencyProperty.Register("OutlineBrush",
            typeof(Brush),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata((Brush)null,
            FrameworkPropertyMetadataOptions.AffectsRender |
            FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush",
            typeof(Brush),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata((Brush)null,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        public static readonly DependencyProperty OutlineThicknessProperty =
            DependencyProperty.Register("OutlineThickness",
            typeof(double),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata((double)1,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IgnoreFirstChildBackgroundProperty =
            DependencyProperty.Register("IgnoreFirstChildBackground",
            typeof(bool),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty LastChildWidthProperty = DependencyProperty.Register(
            "LastChildWidth",
            typeof(double),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata(
            (double)0,
            FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty LevelProperty = DependencyProperty.Register(
            "Level",
            typeof(int),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata(
            (int)0,
            FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static DependencyProperty LevelIndentProperty = DependencyProperty.Register(
            "LevelIndent",
            typeof(double),
            typeof(PropertyPanel),
            new FrameworkPropertyMetadata(
            (double)13,
            FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        // <summary>
        // Gets or sets the pixel width of the last child added into this panel.
        // </summary>
        public double LastChildWidth 
        {
            get { return (double)this.GetValue(LastChildWidthProperty); }
            set { this.SetValue(LastChildWidthProperty, value); }
        }



        // Level DP

        // <summary>
        // Gets or sets the indentation level for the first child in this panel.  Levels are
        // measured in ints, with 0 = no indentation, 1 = 1st sub-property, ...
        // The actual amount of space taken up by each level is controled by LevelIndent property
        // </summary>
        public int Level 
        {
            get { return (int)this.GetValue(LevelProperty); }
            set { this.SetValue(LevelProperty, value); }
        }



        // LevelIndent DP

        // <summary>
        // Gets or sets the pixel width that the first child is indented for each level that
        // it belongs to
        // </summary>
        public double LevelIndent 
        {
            get { return (double)this.GetValue(LevelIndentProperty); }
            set { this.SetValue(LevelIndentProperty, value); }
        }



        // OutlineBrush DP

        // <summary>
        // Gets or sets the line brush to use for the panel compartments
        // </summary>
        public Brush OutlineBrush 
        {
            get { return (Brush)GetValue(OutlineBrushProperty); }
            set { SetValue(OutlineBrushProperty, value); }
        }



        // SelectionBrush DP

        // <summary>
        // Gets or sets the brush to be used as the background for everything but the last
        // element in the panel
        // </summary>
        public Brush SelectionBrush 
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }



        // OutlineThickness DP

        // <summary>
        // Gets or sets the line thickness for the panel compartments (not as Thickness, but
        // instead as a simple double)
        // </summary>
        public double OutlineThickness 
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }



        // IgnoreFirstChildBackground DP

        // <summary>
        // Gets or sets a flag indicating whether the SelectionBrush background should
        // or should not be applied to the first child of the panel
        // </summary>
        public bool IgnoreFirstChildBackground 
        {
            get { return (bool)GetValue(IgnoreFirstChildBackgroundProperty); }
            set { SetValue(IgnoreFirstChildBackgroundProperty, value); }
        }



        // Stacks the children to the left, leaving LastChildWidth amount of space for the last child
        protected override Size MeasureOverride(Size availableSize) 
        {

            double lastChildWidth = Math.Max(0, this.LastChildWidth);
            double indent = this.LevelIndent * this.Level;
            double availableWidth = Math.Max(0, availableSize.Width - lastChildWidth - indent);
            int childrenCount = InternalChildren.Count;
            int lastIndex = childrenCount - 1;
            Size actualSize = new Size();

            for (int i = 0; i < childrenCount; i++) 
            {
                UIElement child = InternalChildren[i];

                if (i == lastIndex) 
                {
                    InternalChildren[i].Measure(new Size(lastChildWidth, availableSize.Height));
                }
                else 
                {
                    InternalChildren[i].Measure(new Size(availableWidth, availableSize.Height));
                }

                availableWidth -= child.DesiredSize.Width;
                //Compute the actual size for the propertypanel
                actualSize.Height = Math.Max(actualSize.Height, child.DesiredSize.Height);
                actualSize.Width += child.DesiredSize.Width;
            }

            return actualSize;
        }

        // Stacks the children to the left, leaving LastChildWidth amount of space for the last child
        protected override Size ArrangeOverride(Size finalSize) 
        {

            double lastChildWidth = Math.Max(0, this.LastChildWidth);
            double indent = this.LevelIndent * this.Level;
            double availableWidth = Math.Max(0, finalSize.Width - lastChildWidth - indent);
            double left = indent;
            int childrenCount = InternalChildren.Count;
            int lastIndex = childrenCount - 1;

            for (int i = 0; i < childrenCount; i++) 
            {

                UIElement child = InternalChildren[i];
                double desiredWidth = child.DesiredSize.Width;
                if (i == lastIndex) 
                {
                    child.Arrange(new Rect(Math.Max(0, finalSize.Width - lastChildWidth), 0, lastChildWidth, finalSize.Height));
                }
                else 
                {
                    child.Arrange(new Rect(left, 0, Math.Min(desiredWidth, availableWidth), finalSize.Height));
                }

                left += desiredWidth;
                availableWidth -= desiredWidth;
                availableWidth = Math.Max(0, availableWidth);
            }

            return finalSize;
        }

        // Custom renders compartments and dividers
        protected override void OnRender(DrawingContext dc) 
        {

            Size renderSize = this.RenderSize;
            Brush outlineBrush = this.OutlineBrush;
            double outlineThickness = this.OutlineThickness;
            double halfThickness = outlineThickness / 2.0;
            double dividerRight = Math.Max(0, this.LastChildWidth);
            double dividerLeft = renderSize.Width - dividerRight - outlineThickness;

            Brush selectionBrush = this.SelectionBrush;

            if (selectionBrush != null) 
            {
                bool ignoreFirstChildBackground = this.IgnoreFirstChildBackground;
                double firstChildWidth = 0;

                if (ignoreFirstChildBackground && this.Children.Count > 0) 
                {
                    firstChildWidth = this.Children[0].RenderSize.Width;
                }

                dc.DrawRectangle(selectionBrush, null, new Rect(
                    firstChildWidth,
                    0,
                    Math.Max(dividerLeft - firstChildWidth, 0),
                    renderSize.Height));
            }

            base.OnRender(dc);

            // Use Guidelines to avoid anti-aliasing (fuzzy border lines)
            dc.PushGuidelineSet(new GuidelineSet(
                // X coordinates for guidelines (vertical lines)
                new double[] { 0, dividerLeft, dividerLeft + outlineThickness, renderSize.Width - outlineThickness, renderSize.Width },
                // Y coordinates for guidelines (horizontal lines)
                new double[] { 0, renderSize.Height - outlineThickness, renderSize.Height }));

            Pen outlinePen = new Pen(outlineBrush, outlineThickness);

            // Bottom edge
            dc.DrawLine(
                outlinePen,
                new Point(0, renderSize.Height - halfThickness),
                new Point(renderSize.Width, renderSize.Height - halfThickness));

            // Top edge
            dc.DrawLine(
                outlinePen,
                new Point(0, 0 - halfThickness),
                new Point(renderSize.Width, 0 - halfThickness));

            // Right edge
            dc.DrawLine(
                outlinePen,
                new Point(renderSize.Width - halfThickness, 0),
                new Point(renderSize.Width - halfThickness, renderSize.Height));

            // Divider
            dc.DrawLine(
                outlinePen,
                new Point(dividerLeft + halfThickness, 0),
                new Point(dividerLeft + halfThickness, renderSize.Height));

            dc.Pop();
        }
    }
}
