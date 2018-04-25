#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region SequentialWorkflowRootDesigner Class
    /// <summary>
    /// Base class for root designer for workflow. It provides a consistent look and feel for all the roots.
    /// The root designers associated with root activities have to be derived from SequentialWorkflowRootDesigner.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequentialWorkflowRootDesigner : SequentialActivityDesigner
    {
        #region Statics
        private const int HeaderFooterSizeIncr = 8;
        private static readonly Image HeaderImage = DR.GetImage(DR.StartWorkflow);
        private static readonly Image FooterImage = DR.GetImage(DR.EndWorkflow);
        private static readonly Size PageStripItemSize = new Size(24, 20);
        private static readonly Size MinSize = new Size(240, 240);
        #endregion

        #region Fields
        //NOTE: The callingDesigner member is only set when the workflow is called from another
        //workflow, this is used to hookup the parent chain.
        private WorkflowHeader header;
        private WorkflowFooter footer;
        #endregion

        #region Properties

        #region Public Properties
        public override string Text
        {
            get
            {
                return String.Empty;
            }
        }

        public override Image Image
        {
            get
            {
                return Header.Image; //smart tag glyph needs an image
            }
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                return Rectangle.Empty; //we are using image rect from the header for that...
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;

                minimumSize.Width = Math.Max(minimumSize.Width, SequentialWorkflowRootDesigner.MinSize.Width);
                minimumSize.Height = Math.Max(minimumSize.Width, SequentialWorkflowRootDesigner.MinSize.Height);
                if (IsRootDesigner && InvokingDesigner == null)
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, ParentView.ViewPortSize.Width - 2 * WorkflowRootLayout.Separator.Width);
                    minimumSize.Height = Math.Max(minimumSize.Height, ParentView.ViewPortSize.Height - 2 * WorkflowRootLayout.Separator.Height);
                }

                return minimumSize;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the header associated with SequentialWorkflowRootDesigner
        /// </summary>
        protected virtual SequentialWorkflowHeaderFooter Header
        {
            get
            {
                if (this.header == null)
                    this.header = new WorkflowHeader(this);
                return this.header;
            }
        }

        /// <summary>
        /// Gets the footer associated with SequentialWorkflowRootDesigner
        /// </summary>
        protected virtual SequentialWorkflowHeaderFooter Footer
        {
            get
            {
                if (this.footer == null)
                    this.footer = new WorkflowFooter(this);
                return this.footer;
            }
        }

        protected override int TitleHeight
        {
            get
            {
                int titleHeight = base.TitleHeight;
                if (Header != null)
                    titleHeight += Header.Bounds.Height;
                return titleHeight;
            }
        }

        protected override bool ShowSmartTag
        {
            get
            {
                if (Header != null && !String.IsNullOrEmpty(Header.Text) && this.Views.Count > 1)
                    return true;
                else
                    return base.ShowSmartTag;
            }
        }

        protected override Rectangle SmartTagRectangle
        {
            get
            {
                Rectangle smartTagRectangle = Rectangle.Empty;
                if (Header != null)
                {
                    smartTagRectangle = Header.ImageRectangle;
                }

                return smartTagRectangle;
            }
        }

        protected override CompositeActivityDesigner InvokingDesigner
        {
            get
            {
                return base.InvokingDesigner;
            }

            set
            {
                base.InvokingDesigner = value;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection(base.Glyphs);
                if (InvokingDesigner != null)
                    glyphs.Add(LockedActivityGlyph.Default);
                return glyphs;
            }
        }
        #endregion

        #region Private Properties
        internal override WorkflowLayout SupportedLayout
        {
            get
            {
                return new WorkflowRootLayout(Activity.Site);
            }
        }

        private int OptimalHeight
        {
            get
            {
                CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                    return 0;

                //Calculate the size based on child size
                int optimalHeight = 0;

                if (ContainedDesigners.Count == 0)
                {
                    optimalHeight += designerTheme.ConnectorSize.Height; //Add the height of first connector
                    optimalHeight += HelpTextSize.Height;
                    optimalHeight += designerTheme.ConnectorSize.Height; //Add the height of last connector
                }
                else
                {
                    ActivityDesigner activeDesigner = ActiveDesigner;
                    if (activeDesigner == this)
                        optimalHeight += designerTheme.ConnectorSize.Height;

                    AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                    foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                    {
                        Size childSize = activityDesigner.Size;
                        optimalHeight += childSize.Height;

                        if (activeDesigner == this)
                            optimalHeight += designerTheme.ConnectorSize.Height;
                        else
                            optimalHeight += 2 * ambientTheme.SelectionSize.Height;
                    }
                }

                return optimalHeight;
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            return false;
        }
        #endregion

        #region Protected Methods
        protected override ReadOnlyCollection<Point> GetInnerConnections(DesignerEdges edges)
        {
            List<Point> connectionPoints = new List<Point>(base.GetInnerConnections(edges));
            if (connectionPoints.Count > 0 && Footer != null && (edges & DesignerEdges.Bottom) > 0)
                connectionPoints[connectionPoints.Count - 1] = new Point(connectionPoints[connectionPoints.Count - 1].X, connectionPoints[connectionPoints.Count - 1].Y - Footer.Bounds.Height);
            return connectionPoints.AsReadOnly();
        }

        protected override void OnSmartTagVisibilityChanged(bool visible)
        {
            base.OnSmartTagVisibilityChanged(visible);

            if (Header != null && !Header.TextRectangle.IsEmpty)
                Invalidate(Header.TextRectangle);
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);

            //Make sure that we set the minimum size
            WorkflowFooter footer = Footer as WorkflowFooter;
            if (footer != null)
                size.Height += footer.ImageRectangle.Height + 2 * e.AmbientTheme.Margin.Height + footer.FooterBarRectangle.Size.Height;

            if (Header != null)
                Header.OnLayout(e);

            if (Footer != null)
                Footer.OnLayout(e);

            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme == null)
                return;

            //Draw the watermark at right bottom
            Rectangle watermarkRectangle = Rectangle.Empty;
            if (compositeDesignerTheme.WatermarkImage != null)
            {
                Rectangle bounds = Bounds;
                bounds.Inflate(-e.AmbientTheme.Margin.Width, -e.AmbientTheme.Margin.Height);
                watermarkRectangle = ActivityDesignerPaint.GetRectangleFromAlignment(compositeDesignerTheme.WatermarkAlignment, bounds, compositeDesignerTheme.WatermarkImage.Size);
            }

            //Here we go, draw header and footer rectangles
            if (Header != null)
                Header.OnPaint(e);

            if (Footer != null)
                Footer.OnPaint(e);
        }
        #endregion

        #region Internal Methods
        //this is for the workflow header/footer class
        internal void InternalPerformLayout()
        {
            PerformLayout();
        }
        #endregion

        #endregion

        #region Nested Classes

        #region Class WorkflowHeader
        private sealed class WorkflowHeader : SequentialWorkflowHeaderFooter
        {
            public WorkflowHeader(SequentialWorkflowRootDesigner parent)
                : base(parent, true)
            {
                Image = SequentialWorkflowRootDesigner.HeaderImage;
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = base.Bounds;
                    Rectangle textRectangle = base.TextRectangle;
                    if (MinHeaderBarHeight > textRectangle.Height)
                        bounds.Height += (MinHeaderBarHeight - textRectangle.Height);
                    return bounds;
                }
            }

            public override Rectangle TextRectangle
            {
                get
                {
                    Rectangle textRectangle = base.TextRectangle;
                    if (MinHeaderBarHeight > textRectangle.Height)
                        textRectangle.Y += (MinHeaderBarHeight - textRectangle.Height) / 2;
                    return textRectangle;
                }
            }

            public override Rectangle ImageRectangle
            {
                get
                {
                    Rectangle imageRectangle = base.ImageRectangle;
                    if (Image != null)
                    {
                        ActivityDesignerTheme designerTheme = AssociatedDesigner.DesignerTheme;
                        imageRectangle.X -= SequentialWorkflowRootDesigner.HeaderFooterSizeIncr / 2;
                        imageRectangle.Y = HeaderBarRectangle.Bottom + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                        imageRectangle.Width += SequentialWorkflowRootDesigner.HeaderFooterSizeIncr;
                        imageRectangle.Height += SequentialWorkflowRootDesigner.HeaderFooterSizeIncr;
                    }
                    return imageRectangle;
                }
            }

            public override void OnPaint(ActivityDesignerPaintEventArgs e)
            {
                if (e == null)
                    throw new ArgumentNullException("e");

                Rectangle rectangle = HeaderBarRectangle;
                Color color1 = Color.Empty;
                Color color2 = Color.FromArgb(50, e.DesignerTheme.BorderColor);
                using (Brush linearGradientBrush = new LinearGradientBrush(rectangle, color1, color2, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(linearGradientBrush, rectangle);
                    e.Graphics.DrawLine(e.DesignerTheme.BorderPen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
                }

                base.OnPaint(e);
            }

            private Rectangle HeaderBarRectangle
            {
                get
                {
                    Rectangle headerBarRectangle = new Rectangle();
                    headerBarRectangle.Location = AssociatedDesigner.Location;
                    headerBarRectangle.Width = AssociatedDesigner.Size.Width;
                    headerBarRectangle.Height = Math.Max(2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height + base.textSize.Height, MinHeaderBarHeight);
                    return headerBarRectangle;
                }
            }

            private int MinHeaderBarHeight
            {
                get
                {
                    return 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                }
            }
        }
        #endregion

        #region Class WorkflowFooter
        private sealed class WorkflowFooter : SequentialWorkflowHeaderFooter
        {
            public WorkflowFooter(SequentialWorkflowRootDesigner parent)
                : base(parent, false)
            {
                Image = SequentialWorkflowRootDesigner.FooterImage;
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = base.Bounds;

                    SequentialWorkflowRootDesigner rootDesigner = AssociatedDesigner as SequentialWorkflowRootDesigner;
                    bounds.Height = Math.Max(bounds.Height, rootDesigner.Size.Height - rootDesigner.TitleHeight - rootDesigner.OptimalHeight);
                    bounds.Y = rootDesigner.Location.Y + rootDesigner.TitleHeight + rootDesigner.OptimalHeight;

                    int minHeight = ImageRectangle.Height;
                    minHeight += (minHeight > 0) ? 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height : 0;
                    minHeight += MinFooterBarHeight;

                    bounds.Height = Math.Max(minHeight, bounds.Height);
                    return bounds;
                }
            }

            public override Rectangle ImageRectangle
            {
                get
                {
                    Rectangle imageRectangle = base.ImageRectangle;
                    if (Image != null)
                    {
                        SequentialWorkflowRootDesigner rootDesigner = AssociatedDesigner as SequentialWorkflowRootDesigner;
                        imageRectangle.X -= SequentialWorkflowRootDesigner.HeaderFooterSizeIncr / 2;
                        imageRectangle.Width += SequentialWorkflowRootDesigner.HeaderFooterSizeIncr;
                        imageRectangle.Height += SequentialWorkflowRootDesigner.HeaderFooterSizeIncr;
                        imageRectangle.Y = rootDesigner.Location.Y + rootDesigner.TitleHeight + rootDesigner.OptimalHeight;
                        imageRectangle.Y += WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                    }
                    return imageRectangle;
                }
            }

            public override void OnPaint(ActivityDesignerPaintEventArgs e)
            {
                if (e == null)
                    throw new ArgumentNullException("e");

                Rectangle rectangle = FooterBarRectangle;
                if (!FooterBarRectangle.IsEmpty)
                {
                    Color color1 = Color.Empty;
                    Color color2 = Color.FromArgb(50, e.DesignerTheme.BorderColor);
                    using (Brush linearGradientBrush = new LinearGradientBrush(rectangle, color2, color1, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(linearGradientBrush, rectangle);
                        e.Graphics.DrawLine(e.DesignerTheme.BorderPen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
                    }
                }

                base.OnPaint(e);
            }

            internal Rectangle FooterBarRectangle
            {
                get
                {
                    return Rectangle.Empty;
                }
            }

            private int MinFooterBarHeight
            {
                get
                {
                    return 0;
                }
            }
        }
        #endregion

        #endregion
    }
    #endregion

}
