namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    internal partial class StateDesigner : FreeformActivityDesigner
    {
        internal class DesignerLayoutConnectionPoint : ConnectionPoint
        {
            private CompositeActivity _eventHandler;
            private DesignerEdges _designerEdges;

            public DesignerLayoutConnectionPoint(ActivityDesigner associatedDesigner, int connectionIndex, CompositeActivity eventHandler, DesignerEdges designerEdges)
                : base(associatedDesigner, designerEdges, connectionIndex)
            {
                Debug.Assert(designerEdges == DesignerEdges.Left || designerEdges == DesignerEdges.Right);
                _eventHandler = eventHandler;
                _designerEdges = designerEdges;

            }

            public override Point Location
            {
                get
                {
                    Debug.Assert(this.DesignerEdges == DesignerEdges.Left || this.DesignerEdges == DesignerEdges.Right);
                    DesignerLayout designerLayout = this.DesignerLayout;
                    if (designerLayout == null)
                        return Point.Empty;
                    if (this.DesignerEdges == DesignerEdges.Left)
                        return designerLayout.LeftConnectionPoint;
                    else
                        return designerLayout.RightConnectionPoint;
                }
            }

            public DesignerLayout DesignerLayout
            {
                get
                {
                    DesignerLayout designerLayout;
                    ((StateDesigner)this.AssociatedDesigner).DesignerLayouts.TryGetValue(this._eventHandler, out designerLayout);
                    return designerLayout;
                }
            }

            public DesignerEdges DesignerEdges
            {
                get
                {
                    return _designerEdges;
                }
            }

            public CompositeActivity EventHandler
            {
                get
                {
                    return _eventHandler;
                }
            }
        }

        #region Layout class

        internal abstract class Layout
        {
            #region Fields
            private List<Layout> _layouts = new List<Layout>();
            private Point _location;
            private Size _size;
            private Size _minimumSize;
            private ActivityDesigner _activityDesigner;
            private Layout _mouseOverLayout;
            #endregion Fields

            #region Constructor
            public Layout(ActivityDesigner activityDesigner)
            {
                _activityDesigner = activityDesigner;
            }
            #endregion Constructor

            #region Events

            public event MouseEventHandler MouseMove;
            public event MouseEventHandler MouseDown;
            public event MouseEventHandler MouseUp;
            public event EventHandler MouseEnter;
            public event EventHandler MouseLeave;
            public event MouseEventHandler MouseDoubleClick;

            #endregion

            #region Properties
            public ActivityDesigner ActivityDesigner
            {
                get
                {
                    return _activityDesigner;
                }
            }

            public Rectangle Bounds
            {
                get
                {
                    return new Rectangle(_location, _size);
                }
            }

            public List<Layout> Layouts
            {
                get
                {
                    return _layouts;
                }
            }

            public Point Location
            {
                get
                {
                    return _location;
                }
                set
                {
                    _location = value;
                }
            }

            public Size Size
            {
                get
                {
                    return _size;
                }
                set
                {
                    _size = value;
                }
            }

            public Layout MouseOverLayout
            {
                get
                {
                    return _mouseOverLayout;
                }
                set
                {
                    if (value == _mouseOverLayout)
                        return;

                    if (_mouseOverLayout != null)
                        _mouseOverLayout.OnMouseLeave();

                    _mouseOverLayout = value;

                    if (value != null)
                        value.OnMouseEnter();
                }
            }

            public virtual Size MinimumSize
            {
                get
                {
                    return _minimumSize;
                }
                set
                {
                    _minimumSize = value;
                }
            }

            #endregion Properties

            #region Methods

            public Layout GetLayout(ActivityDesigner designer)
            {
                if (this.ActivityDesigner == designer)
                    return this;

                foreach (Layout layout in Layouts)
                {
                    Layout found = layout.GetLayout(designer);
                    if (found != null)
                        return found;
                }
                return null;
            }

            public virtual void ResizeLayout(Size newSize)
            {
                _size = newSize;
            }

            public virtual void MoveLayout(Point newLocation)
            {
                if (newLocation == _location)
                    return;

                Point offset = new Point(_location.X - newLocation.X, _location.Y - newLocation.Y);
                foreach (Layout layout in _layouts)
                {
                    Point currentLocation = layout.Location;
                    Point newChildDesignerLocation = new Point(currentLocation.X - offset.X, currentLocation.Y - offset.Y);
                    layout.MoveLayout(newChildDesignerLocation);
                }

                _location = newLocation;
            }

            public virtual void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
            }

            public virtual void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
            }

            public virtual void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                foreach (Layout layout in _layouts)
                {
                    layout.OnPaint(graphics, designerTheme, ambientTheme);
                }
            }

            public virtual void OnMouseMove(MouseEventArgs e)
            {
                Layout layout = GetLayoutAt(e.X, e.Y);
                if (layout != this.MouseOverLayout)
                {
                    this.MouseOverLayout = layout;
                }

                if (this.MouseOverLayout == this)
                {
                    if (this.MouseMove != null)
                        this.MouseMove(this, e);
                }
                else
                {
                    if (this.MouseOverLayout != null)
                    {
                        this.MouseOverLayout.OnMouseMove(e);
                    }
                }
            }

            public virtual void OnMouseDown(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseDown != null)
                        this.MouseDown(this, e);
                }
                else
                {
                    if (this.MouseOverLayout != null)
                    {
                        this.MouseOverLayout.OnMouseDown(e);
                    }
                }
            }

            public virtual void OnMouseUp(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseUp != null)
                        this.MouseUp(this, e);
                }
                else
                {
                    if (this.MouseOverLayout != null)
                    {
                        this.MouseOverLayout.OnMouseUp(e);
                    }
                }

                Layout layout = GetLayoutAt(e.X, e.Y);
                this.MouseOverLayout = layout;
            }

            public virtual void OnMouseEnter()
            {
                if (this.MouseEnter != null)
                    this.MouseEnter(this, EventArgs.Empty);
            }

            public virtual void OnMouseLeave()
            {
                if (this.MouseOverLayout != this)
                    this.MouseOverLayout = null;

                if (this.MouseLeave != null)
                    this.MouseLeave(this, EventArgs.Empty);
            }

            public virtual void OnMouseDoubleClick(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseDoubleClick != null)
                        this.MouseDoubleClick(this, e);
                }
                else
                {
                    if (this.MouseOverLayout != null)
                    {
                        this.MouseOverLayout.OnMouseDoubleClick(e);
                    }
                }

                Layout layout = GetLayoutAt(e.X, e.Y);
                this.MouseOverLayout = layout;
            }

            public virtual void Invalidate()
            {
                WorkflowView parentView = StateDesigner.GetService(_activityDesigner, typeof(WorkflowView)) as WorkflowView;
                if (parentView != null)
                    parentView.InvalidateLogicalRectangle(this.Bounds);
            }

            public virtual HitTestInfo HitTest(Point point)
            {
                HitTestInfo hitInfo = HitTestInfo.Nowhere;
                if (this.Bounds.Contains(point))
                {
                    Layout layout = GetLayoutAt(point.X, point.Y);
                    if (layout != this)
                        hitInfo = layout.HitTest(point);
                }

                return hitInfo;
            }

            private Layout GetLayoutAt(int x, int y)
            {
                foreach (Layout layout in _layouts)
                {
                    if (layout.Bounds.Contains(x, y))
                    {
                        return layout;
                    }
                }

                if (this.Bounds.Contains(x, y))
                    return this;

                return null;
            }

            #endregion Methods
        }

        #endregion Layout class

        #region DesignerLayoutBase class

        internal class DesignerLayoutBase : Layout
        {
            #region Fields
            public const int ImagePadding = 4;
            private Point _textLocation;
            private Point _imageLocation;
            private Size _textSize;
            private Size _imageSize;
            #endregion Fields

            #region Constructor

            public DesignerLayoutBase(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            #endregion Constructor

            #region Properties

            public Rectangle ImageRectangle
            {
                get
                {
                    Rectangle rectangle = new Rectangle(_imageLocation, _imageSize);
                    return StateMachineDesignerPaint.TrimRectangle(rectangle, Bounds);
                }
            }

            public Rectangle TextRectangle
            {
                get
                {
                    Rectangle rectangle = new Rectangle(_textLocation, _textSize);
                    return StateMachineDesignerPaint.TrimRectangle(rectangle, Bounds);
                }
            }

            public string Text
            {
                get
                {
                    return this.ActivityDesigner.Activity.Name;
                }
            }

            #endregion Properties

            #region Methods

            public override void MoveLayout(Point newLocation)
            {
                Point offset = new Point(this.Location.X - newLocation.X, this.Location.Y - newLocation.Y);
                _textLocation = new Point(_textLocation.X - offset.X, _textLocation.Y - offset.Y);
                _imageLocation = new Point(_imageLocation.X - offset.X, _imageLocation.Y - offset.Y);
                base.MoveLayout(newLocation);
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                _imageSize = designerTheme.ImageSize;
                string text = this.Text;
                Font font = designerTheme.Font;
                _textSize = StateMachineDesignerPaint.MeasureString(graphics, font, text, StringAlignment.Near, Size.Empty);
                int width = _imageSize.Width + ImagePadding + _textSize.Width;
                width += ambientTheme.Margin.Width * 2;
                int height = Math.Max(_imageSize.Height, _textSize.Height);
                height += ambientTheme.Margin.Height;
                Size size = new Size(width, height);
                this.MinimumSize = size;
                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                base.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                Point origin = this.Location;
                origin.X += ambientTheme.Margin.Width;
                origin.Y += ambientTheme.Margin.Height / 2;
                _imageLocation = origin;
                origin.X += _imageSize.Width + ImagePadding;
                _textLocation = origin;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                ActivityDesigner activityDesigner = this.ActivityDesigner;

                Font font = designerTheme.Font;

                Image image = StateDesigner.GetDesignerImage(activityDesigner);
                if (image != null)
                    ActivityDesignerPaint.DrawImage(graphics, image, this.ImageRectangle, DesignerContentAlignment.Fill);

                ActivityDesignerPaint.DrawText(graphics, font, this.Text, TextRectangle, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
            }

            #endregion Methods
        }

        #endregion DesignerLayoutBase class

        #region DesignerLayout class

        internal class DesignerLayout : DesignerLayoutBase
        {
            public DesignerLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            public override HitTestInfo HitTest(Point point)
            {
                HitTestInfo hitInfo = HitTestInfo.Nowhere;
                if (this.Bounds.Contains(point))
                {
                    hitInfo = new HitTestInfo(this.ActivityDesigner, HitTestLocations.Designer | HitTestLocations.ActionArea);
                }

                return hitInfo;
            }

            public virtual Point LeftConnectionPoint
            {
                get
                {
                    Rectangle layoutBounds = this.Bounds;
                    int midVert = layoutBounds.Y + (layoutBounds.Height / 2);
                    Point point = new Point(layoutBounds.Left - 4, midVert);
                    return point;
                }
            }

            public virtual Point RightConnectionPoint
            {
                get
                {
                    Rectangle layoutBounds = this.Bounds;
                    int midVert = layoutBounds.Y + (layoutBounds.Height / 2);
                    Point point = new Point(layoutBounds.Right + 4, midVert);
                    return point;
                }
            }
        }

        #endregion DesignerLayout class

        #region DesignerLinkLayout class

        private class DesignerLinkLayout : DesignerLayoutBase
        {
            #region Fields
            private Cursor _previousCursor;
            private bool _mouseOver;
            private StateDesigner _parentStateDesigner;
            #endregion Fields

            #region Constructor
            public DesignerLinkLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }
            #endregion Constructor

            #region Properties
            public bool MouseOver
            {
                get
                {
                    return _mouseOver;
                }
                set
                {
                    if (value == _mouseOver)
                        return;

                    _mouseOver = value;
                    Invalidate();
                }
            }

            public StateDesigner ParentStateDesigner
            {
                get
                {
                    return _parentStateDesigner;
                }
                set
                {
                    _parentStateDesigner = value;
                }
            }
            #endregion Properties

            #region Methods

            public override void OnMouseEnter()
            {
                base.OnMouseEnter();
                if (this.ParentStateDesigner != null)
                {
                    _previousCursor = this.ParentStateDesigner.Cursor;
                    this.ParentStateDesigner.Cursor = Cursors.Hand;
                }
                this.MouseOver = true;
            }

            public override void OnMouseLeave()
            {
                base.OnMouseLeave();
                if (this.ParentStateDesigner != null)
                {
                    this.ParentStateDesigner.Cursor = _previousCursor;
                }
                this.MouseOver = false;
                Invalidate();
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                ActivityDesigner activityDesigner = this.ActivityDesigner;

                if (this.MouseOver)
                {
                    using (Font font = new Font(designerTheme.Font, FontStyle.Underline | designerTheme.Font.Style))
                    {
                        Image image = StateDesigner.GetDesignerImage(activityDesigner);
                        if (image != null)
                            ActivityDesignerPaint.DrawImage(graphics, image, this.ImageRectangle, DesignerContentAlignment.Fill);

                        ActivityDesignerPaint.DrawText(graphics, font, this.Text, TextRectangle, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
                    }
                }
                else
                    base.OnPaint(graphics, designerTheme, ambientTheme);
            }
            #endregion Methods
        }
        #endregion DesignerLinkLayout class

        #region EventHandlersLayout class

        private class EventHandlersLayout : Layout
        {
            #region Fields
            internal const int EventDrivenPadding = 8;
            #endregion Fields

            #region Constructor

            public EventHandlersLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            #endregion Constructor

            #region Methods

            public override void ResizeLayout(Size newSize)
            {
                base.ResizeLayout(newSize);
                int maxEventDrivenWidth = newSize.Width - (EventDrivenPadding * 2);
                foreach (Layout layout in this.Layouts)
                {
                    Size size = layout.Size;
                    if (size.Width > maxEventDrivenWidth)
                    {
                        layout.ResizeLayout(new Size(maxEventDrivenWidth, size.Height));
                    }
                }
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                Size selectionSize = ambientTheme.SelectionSize;
                Size minimumSize = new Size();

                foreach (Layout layout in Layouts)
                {
                    layout.OnLayoutSize(graphics, designerTheme, ambientTheme, minimumSize);
                    minimumSize.Height += layout.Size.Height;
                    minimumSize.Height += selectionSize.Height;
                    int layoutWidth = layout.Size.Width + 2 * (selectionSize.Width + ambientTheme.Margin.Width);
                    minimumSize.Width = Math.Max(minimumSize.Width, layoutWidth);
                }

                if (this.Layouts.Count > 0)
                    minimumSize.Height += EventDrivenPadding;

                this.MinimumSize = minimumSize;

                Size size = new Size();
                size.Width = Math.Max(containerSize.Width, minimumSize.Height);
                size.Height = minimumSize.Height;
                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Size selectionSize = ambientTheme.SelectionSize;
                int x = Location.X + EventDrivenPadding;
                int y = Location.Y + EventDrivenPadding;
                foreach (Layout layout in this.Layouts)
                {
                    layout.Location = new Point(x, y);
                    DesignerLayoutBase designerLayout = layout as DesignerLayoutBase;
                    if (designerLayout != null)
                        designerLayout.ActivityDesigner.Location = layout.Location;
                    layout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    y += layout.Size.Height + selectionSize.Height;
                }
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                base.OnPaint(graphics, designerTheme, ambientTheme);

                StateDesigner stateDesigner = (StateDesigner)this.ActivityDesigner;
                ContainedDesignersParser parser = stateDesigner._designersParser;
                // we only draw the separation line
                // if we have at least one event driven and one state
                if ((parser.EventDrivenDesigners.Count > 0 || parser.StateInitializationDesigners.Count > 0 || parser.StateFinalizationDesigners.Count > 0) &&
                    (parser.StateDesigners.Count > 0 || parser.LeafStateDesigners.Count > 0))
                {
                    Rectangle bounds = this.Bounds;
                    graphics.DrawLine(designerTheme.BorderPen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                }
            }

            #endregion
        }

        #endregion

        #region BreadCrumbBarLayout class

        private class BreadCrumbBarLayout : Layout
        {
            #region Fields

            private const string BreadCrumbSeparator = " : ";
            private Size _breadCrumbSeparatorSize;

            #endregion Fields

            #region Constructor

            public BreadCrumbBarLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            #endregion Constructor

            #region Methods

            private void InitializeLayouts()
            {
                this.Layouts.Clear();
                StateDesigner rootStateDesigner = (StateDesigner)this.ActivityDesigner;
                StateDesigner currentDesigner = rootStateDesigner;
                while (currentDesigner != null)
                {
                    DesignerLinkLayout layout = currentDesigner.InlineLayout;
                    layout.ParentStateDesigner = rootStateDesigner;
                    this.Layouts.Add(currentDesigner.InlineLayout);
                    currentDesigner = currentDesigner.ActiveDesigner as StateDesigner;
                }
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                InitializeLayouts();

                CompositeDesignerTheme compositeDesignerTheme = designerTheme as CompositeDesignerTheme;
                if (compositeDesignerTheme == null)
                    return;

                Font font = designerTheme.BoldFont;
                _breadCrumbSeparatorSize = StateMachineDesignerPaint.MeasureString(graphics, font, BreadCrumbSeparator, StringAlignment.Near, Size.Empty);
                Size size = Size.Empty;

                foreach (Layout layout in Layouts)
                {
                    layout.OnLayoutSize(graphics, compositeDesignerTheme, ambientTheme, size);
                    size.Width += layout.Size.Width + _breadCrumbSeparatorSize.Width;
                    size.Height = Math.Max(size.Height, layout.Size.Height);
                }

                this.MinimumSize = size;
                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Point location = this.Location;
                foreach (Layout layout in this.Layouts)
                {
                    layout.Location = location;
                    layout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    location.X += layout.Size.Width + _breadCrumbSeparatorSize.Width;
                }
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                if (Layouts.Count == 0)
                    return;

                Font font = designerTheme.BoldFont;
                TextQuality textQuality = ambientTheme.TextQuality;
                Brush brush = designerTheme.ForegroundBrush;

                List<Layout> layouts = this.Layouts;
                Layout layout;
                for (int i = 0; i < layouts.Count - 1; i++)
                {
                    layout = layouts[i];
                    layout.OnPaint(graphics, designerTheme, ambientTheme);

                    Rectangle separatorRectangle = new Rectangle(
                        layout.Bounds.Right,
                        layout.Location.Y,
                        _breadCrumbSeparatorSize.Width,
                        _breadCrumbSeparatorSize.Height);

                    ActivityDesignerPaint.DrawText(graphics, font, BreadCrumbSeparator, separatorRectangle,
                        StringAlignment.Near, textQuality, brush);
                }

                // draw the last one
                layout = layouts[layouts.Count - 1];
                layout.OnPaint(graphics, designerTheme, ambientTheme);
            }

            #endregion Methods
        }

        #endregion

        #region TextLayout class

        private class TextLayout : Layout
        {
            #region Constructor

            public TextLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            #endregion Constructor

            #region Methods

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                CompositeDesignerTheme compositeDesignerTheme = designerTheme as CompositeDesignerTheme;
                string text = this.ActivityDesigner.Text;
                Size size = Size.Empty;
                if (compositeDesignerTheme != null && !String.IsNullOrEmpty(text))
                {
                    size = StateMachineDesignerPaint.MeasureString(graphics, compositeDesignerTheme.Font, text, StringAlignment.Center, Size.Empty);
                }
                this.MinimumSize = size;
                this.Size = size;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                string text = this.ActivityDesigner.Text;
                if (String.IsNullOrEmpty(text))
                    return;

                Font font = designerTheme.Font;
                ActivityDesignerPaint.DrawText(graphics, font, text, this.Bounds, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
            }

            #endregion Methods
        }

        #endregion

        #region ImageLayout class

        private class ImageLayout : Layout
        {
            #region Constructor

            public ImageLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            #endregion Constructor

            #region Methods

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                CompositeDesignerTheme compositeDesignerTheme = designerTheme as CompositeDesignerTheme;
                Size size = Size.Empty;
                if (this.ActivityDesigner.Image != null && compositeDesignerTheme != null)
                {
                    size = designerTheme.ImageSize;
                }
                this.MinimumSize = size;
                this.Size = size;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Image image = this.ActivityDesigner.Image;
                if (image != null)
                    ActivityDesignerPaint.DrawImage(graphics, image, this.Bounds, DesignerContentAlignment.Fill);
            }

            #endregion Methods
        }

        #endregion

        #region TitleBarLayout class

        private class TitleBarLayout : Layout
        {
            #region Fields

            private TextLayout _textLayout;
            private ImageLayout _imageLayout;
            private const int Padding = 4;

            #endregion Fields

            #region Constructor

            public TitleBarLayout(ActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
                _textLayout = new TextLayout(activityDesigner);
                this.Layouts.Add(_textLayout);
                _imageLayout = new ImageLayout(activityDesigner);
                this.Layouts.Add(_imageLayout);
            }

            #endregion Constructor

            #region Properties

            public TextLayout TextLayout
            {
                get
                {
                    return _textLayout;
                }
            }

            public ImageLayout ImageLayout
            {
                get
                {
                    return _imageLayout;
                }
            }

            #endregion Properties

            #region Methods

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                Size minimumSize = new Size();
                _textLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, Size.Empty);
                _imageLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, Size.Empty);

                minimumSize.Width = designerTheme.BorderWidth * 2 + 10 + _textLayout.Size.Width + _imageLayout.Size.Width;
                minimumSize.Height = Math.Max(_textLayout.Size.Height, _imageLayout.Size.Height);
                minimumSize.Height += designerTheme.BorderWidth * 2 + 4;

                this.MinimumSize = minimumSize;
                Size size = minimumSize;
                size.Width = Math.Max(minimumSize.Width, containerSize.Width);
                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Point origin = this.Location;
                origin.X += margin.Width;
                origin.Y += 2;

                _imageLayout.Location = origin;

                CalculateTextLayout();
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Rectangle rectangle = this.Bounds;

                Brush backgroundBrush = designerTheme.GetBackgroundBrush(this.Bounds);
                graphics.FillRectangle(backgroundBrush, rectangle);

                Color color1;
                Color color2;
                StateActivity state = (StateActivity)this.ActivityDesigner.Activity;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    color1 = Color.FromArgb(32, designerTheme.BorderColor);
                    color2 = Color.FromArgb(160, designerTheme.BorderColor);
                }
                else
                {
                    if (StateMachineHelpers.IsRootState(state))
                    {
                        color1 = Color.Empty;
                        color2 = Color.FromArgb(128, designerTheme.BorderColor);
                    }
                    else
                    {
                        color1 = Color.FromArgb(16, designerTheme.BorderColor);
                        color2 = Color.FromArgb(16, designerTheme.BorderColor);
                    }
                }

                if (rectangle.Width > 0 && rectangle.Height > 0)
                {
                    using (Brush linearGradientBrush = new LinearGradientBrush(rectangle, color1, color2, LinearGradientMode.Vertical))
                    {
                        graphics.FillRectangle(linearGradientBrush, rectangle);
                        graphics.DrawLine(designerTheme.BorderPen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
                    }
                }

                base.OnPaint(graphics, designerTheme, ambientTheme);
            }

            public override void ResizeLayout(Size newSize)
            {
                base.ResizeLayout(newSize);
                CalculateTextLayout();
            }

            private void CalculateTextLayout()
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                int minX = _imageLayout.Bounds.Right + Padding;

                int maxStringWidth = _textLayout.Size.Width;
                int xPos = (this.Location.X + this.Size.Width / 2 - maxStringWidth / 2);
                if (xPos < minX)
                    xPos = minX;

                if ((xPos + maxStringWidth) > (this.Bounds.Right - margin.Width))
                    maxStringWidth = (this.Bounds.Right - margin.Width) - xPos;

                _textLayout.Location = new Point(xPos, this.Location.Y + margin.Height);
                _textLayout.Size = new Size(maxStringWidth, _textLayout.Size.Height);
            }

            #endregion Methods
        }

        #endregion

        #region StatesLayout class

        private class StatesLayout : Layout
        {
            #region Fields
            private const int StatePadding = 16;
            private static readonly Size RealMinimumSize = new Size(160, 80);
            private TitleBarLayout _titleBarLayout;
            private EventHandlersLayout _eventHandlersLayout;
            #endregion Fields

            #region Constructor

            public StatesLayout(
                ActivityDesigner activityDesigner,
                TitleBarLayout titleBarLayout,
                EventHandlersLayout eventHandlersLayout)
                : base(activityDesigner)
            {
                _titleBarLayout = titleBarLayout;
                this.Layouts.Add(titleBarLayout);
                _eventHandlersLayout = eventHandlersLayout;
                this.Layouts.Add(eventHandlersLayout);
            }

            #endregion Constructor

            #region Properties

            private StateDesigner StateDesigner
            {
                get
                {
                    return (StateDesigner)this.ActivityDesigner;
                }
            }

            public override Size MinimumSize
            {
                get
                {
                    Size minimumSize = base.MinimumSize;
                    minimumSize.Width = Math.Max(minimumSize.Width, RealMinimumSize.Width);
                    minimumSize.Height = Math.Max(minimumSize.Height, RealMinimumSize.Height);
                    return minimumSize;
                }
            }

            public EventHandlersLayout EventHandlersLayout
            {
                get
                {
                    return _eventHandlersLayout;
                }
            }

            #endregion Properties

            #region Methods

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                CompositeDesignerTheme compositeDesignerTheme = (CompositeDesignerTheme)designerTheme;

                Size size = containerSize;

                _titleBarLayout.OnLayoutSize(graphics, compositeDesignerTheme, ambientTheme, size);
                _eventHandlersLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);

                int minWidth = Math.Max(_titleBarLayout.MinimumSize.Width, _eventHandlersLayout.MinimumSize.Width);
                int minHeight = _titleBarLayout.MinimumSize.Height + _eventHandlersLayout.MinimumSize.Height;
                this.MinimumSize = new Size(minWidth, minHeight);

                size.Width = Math.Max(minWidth, size.Width);
                size.Height = Math.Max(minHeight, size.Height);

                if (this.StateDesigner.NeedsAutoLayout)
                {
                    int maximumX = size.Width;
                    int maximumY = _titleBarLayout.Size.Height +
                        _eventHandlersLayout.Size.Height +
                        DefaultStateDesignerAutoLayoutDistance;

                    bool containsStates = false;

                    foreach (ActivityDesigner designer in this.StateDesigner.ContainedDesigners)
                    {
                        if (!this.StateDesigner.IsContainedDesignerVisible(designer))
                            continue;

                        StateDesigner stateDesigner = designer as StateDesigner;
                        if (stateDesigner == null)
                            continue;

                        containsStates = true;

                        maximumX = Math.Max(maximumX, designer.Size.Width);
                        maximumY += designer.Size.Height + DefaultStateDesignerAutoLayoutDistance;
                    }

                    if (containsStates)
                    {
                        // Add some extra padding to take into account for the AutoSize growth
                        maximumY += DefaultStateDesignerAutoLayoutDistance * 2;
                    }

                    size = new Size(maximumX, maximumY);
                }

                _titleBarLayout.ResizeLayout(new Size(size.Width, _titleBarLayout.Size.Height));

                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                _titleBarLayout.Location = this.Location;
                _titleBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);

                int x = this.Location.X;
                int y = _titleBarLayout.Bounds.Bottom + 1;

                _eventHandlersLayout.Location = new Point(x, y);
                _eventHandlersLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                GraphicsPath path = StateMachineDesignerPaint.GetDesignerPath(this.ActivityDesigner, this.Bounds, designerTheme);
                Brush backgroundBrush = designerTheme.GetBackgroundBrush(this.Bounds);
                graphics.FillPath(backgroundBrush, path);

                base.OnPaint(graphics, designerTheme, ambientTheme);

                if (ambientTheme.ShowDesignerBorder)
                    graphics.DrawPath(designerTheme.BorderPen, path);

                if (this.StateDesigner.ContainedDesigners.Count == 0 &&
                    !this.StateDesigner.IsStateCustomActivity)
                {
                    Point location = new Point(this.Location.X, _titleBarLayout.Bounds.Bottom);
                    Size size = new Size(this.Size.Width, this.Size.Height - _titleBarLayout.Bounds.Height);
                    Rectangle rectangle = new Rectangle(location, size);
                    rectangle.Inflate(-1, -1);

                    StateActivity state = (StateActivity)this.StateDesigner.Activity;
                    if (StateMachineHelpers.IsLeafState(state) &&
                        StateMachineHelpers.IsCompletedState(state))
                        return;

                    if (this.StateDesigner.DragDropActive)
                    {
                        Color color = Color.FromArgb(64, designerTheme.ForeColor);
                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            ActivityDesignerPaint.DrawText(
                                graphics,
                                designerTheme.Font,
                                this.StateDesigner.HelpText,
                                rectangle,
                                StringAlignment.Center,
                                ambientTheme.TextQuality,
                                brush);
                        }
                    }
                    else
                    {
                        ActivityDesignerPaint.DrawText(
                            graphics,
                            designerTheme.Font,
                            this.StateDesigner.HelpText,
                            rectangle,
                            StringAlignment.Center,
                            ambientTheme.TextQuality,
                            designerTheme.ForegroundBrush);
                    }
                }
            }

            public override void ResizeLayout(Size newSize)
            {
                _eventHandlersLayout.ResizeLayout(new Size(newSize.Width, _eventHandlersLayout.Size.Height));
                _titleBarLayout.ResizeLayout(new Size(newSize.Width, _titleBarLayout.Size.Height));
                base.ResizeLayout(newSize);
            }

            #endregion Methods
        }

        #endregion StatesLayout class

        #region EventDrivenLayout class

        private class EventDrivenLayout : Layout
        {
            #region Fields

            private BreadCrumbBarLayout _breadCrumbBarLayout;
            private TitleBarLayout _titleBarLayout;
            private DesignerLinkLayout _designerLayout;
            private const int ActiveDesignerPadding = 16;
            #endregion

            #region Constructor
            public EventDrivenLayout(ActivityDesigner activityDesigner, TitleBarLayout titleBarLayout)
                : base(activityDesigner)
            {
                _breadCrumbBarLayout = new BreadCrumbBarLayout(activityDesigner);
                _designerLayout = new DesignerLinkLayout(activityDesigner);
                StateDesigner stateDesigner = activityDesigner as StateDesigner;
                if (stateDesigner != null)
                {
                    _designerLayout.ParentStateDesigner = stateDesigner;
                    _designerLayout.MouseDown += new MouseEventHandler(stateDesigner.StateDesignerLinkMouseDown);
                }
                _titleBarLayout = titleBarLayout;
            }

            private void InitializeLayout()
            {
                this.Layouts.Clear();
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    // we only display the title bar and 
                    // the bread crumb bar at the top most level
                    this.Layouts.Add(_titleBarLayout);
                    this.Layouts.Add(_breadCrumbBarLayout);
                }
                else
                {
                    this.Layouts.Add(_designerLayout);
                }
            }

            #endregion

            #region Properties

            private StateDesigner StateDesigner
            {
                get
                {
                    return (StateDesigner)this.ActivityDesigner;
                }
            }

            #endregion Properties

            #region Methods
            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);

                InitializeLayout();

                Size size = containerSize;
                Size minSize = this.StateDesigner.MinimumSize;
                size.Width = Math.Max(size.Width, minSize.Width);
                size.Height = Math.Max(size.Height, minSize.Height);

                ActivityDesigner activeDesigner = this.StateDesigner.ActiveDesigner;
                Size activeDesignerSize = activeDesigner.Size;

                if (this.StateDesigner.IsRootStateDesigner)
                {
                    _titleBarLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                    _breadCrumbBarLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);

                    size.Width = Math.Max(size.Width, activeDesignerSize.Width + ActiveDesignerPadding * 2);
                    size.Width = Math.Max(size.Width, _titleBarLayout.Size.Width);
                    size.Width = Math.Max(size.Width, _breadCrumbBarLayout.Size.Width);

                    int minHeight =
                        activeDesignerSize.Height +
                        _titleBarLayout.Size.Height +
                        _breadCrumbBarLayout.Size.Height +
                        ActiveDesignerPadding * 3 +
                        ambientTheme.SelectionSize.Height * 2;
                    size.Height = Math.Max(size.Height, minHeight);
                    _titleBarLayout.ResizeLayout(new Size(size.Width, _titleBarLayout.Size.Height));
                }
                else
                {
                    _designerLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                    size.Width = Math.Max(size.Width, activeDesigner.Size.Width + ActiveDesignerPadding * 2);
                    size.Width = Math.Max(size.Width, _designerLayout.Size.Width);
                    size.Height = Math.Max(size.Height, activeDesigner.Size.Height + ActiveDesignerPadding * 2 + _designerLayout.Size.Height + ambientTheme.SelectionSize.Height * 2);
                }

                this.MinimumSize = size;
                this.Size = size;
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                CompositeDesignerTheme compositeDesignerTheme = designerTheme as CompositeDesignerTheme;
                if (compositeDesignerTheme == null)
                    return;

                Rectangle bounds = this.Bounds;
                Point origin = bounds.Location;
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    _titleBarLayout.Location = origin;
                    _titleBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    origin.X += ActiveDesignerPadding;
                    origin.Y += _titleBarLayout.Size.Height + ActiveDesignerPadding;
                    _breadCrumbBarLayout.Location = origin;
                    _breadCrumbBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    origin.Y += _breadCrumbBarLayout.Size.Height + ActiveDesignerPadding;
                }
                else
                {
                    Point designerLayoutLocation = new Point(
                        bounds.Left + (bounds.Width - _designerLayout.Size.Width) / 2,
                        bounds.Top + ambientTheme.SelectionSize.Height);
                    _designerLayout.Location = designerLayoutLocation;
                    _designerLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    origin.Y = _designerLayout.Bounds.Bottom + ambientTheme.SelectionSize.Height + ActiveDesignerPadding;
                }

                Size activeDesignerSize = this.StateDesigner.ActiveDesigner.Size;
                origin.X = bounds.Left + (bounds.Width - activeDesignerSize.Width) / 2;

                this.StateDesigner.ActiveDesigner.Location = origin;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                GraphicsPath path = StateMachineDesignerPaint.GetDesignerPath(this.ActivityDesigner, this.Bounds, designerTheme);
                Brush backgroundBrush = designerTheme.GetBackgroundBrush(this.Bounds);
                graphics.FillPath(backgroundBrush, path);

                base.OnPaint(graphics, designerTheme, ambientTheme);

                if (ambientTheme.ShowDesignerBorder)
                    graphics.DrawPath(designerTheme.BorderPen, path);
            }

            public override void ResizeLayout(Size newSize)
            {
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    _titleBarLayout.ResizeLayout(new Size(newSize.Width, _titleBarLayout.Size.Height));
                }
                base.ResizeLayout(newSize);
            }

            #endregion Methods
        }

        #endregion
    }
}
