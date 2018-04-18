//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		RectangleAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	PolylineAnnotation, PolygonAnnotation
//
//  Purpose:	Polyline and polygon annotation classes.
//
//	Reviewed:	
//
//===================================================================

#region Used namespace
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
#if Microsoft_CONTROL
using System.Windows.Forms;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Collections.ObjectModel;

#else
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization.Charting.Data;
using System.Web.UI.DataVisualization.Charting.Utilities;
using System.Web.UI.DataVisualization.Charting.Borders3D;
using System.Collections.ObjectModel;
#endif

#endregion

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting

#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
	/// <b>PolylineAnnotation</b> is a class that represents a polyline annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributePolylineAnnotation_PolylineAnnotation"),
	]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class PolylineAnnotation : Annotation
	{
		#region Fields

		// Path with polygon points.
        private    GraphicsPath    _defaultGraphicsPath = new GraphicsPath();
        private    GraphicsPath    _graphicsPath;

		// Indicates that path was changed
		internal	bool			pathChanged = false;

		// Collection of path points exposed at design-time
		private AnnotationPathPointCollection _pathPoints;

		// Indicate that filled polygon must be drawn
		internal bool				isPolygon = false;

		// Indicates that annotation will be placed using free-draw style
		internal bool				isFreeDrawPlacement = false;

		// Line start/end caps
		private		LineAnchorCapStyle		_startCap = LineAnchorCapStyle.None;
		private		LineAnchorCapStyle		_endCap = LineAnchorCapStyle.None;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public PolylineAnnotation() 
            : base()
		{
            _pathPoints = new AnnotationPathPointCollection(this);
             
            _graphicsPath = _defaultGraphicsPath;
		}

		#endregion

		#region Properties

		#region Polyline Visual Attributes

		/// <summary>
		/// Gets or sets a cap style used at the start of an annotation line.
		/// <seealso cref="EndCap"/>
		/// </summary>
		/// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used for a cap style used at the start of an annotation line.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(LineAnchorCapStyle.None),
		SRDescription("DescriptionAttributeStartCap3"),
		]
		virtual public LineAnchorCapStyle StartCap
		{
			get
			{
				return _startCap;
			}
			set
			{
				_startCap = value;
				Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets a cap style used at the end of an annotation line.
		/// <seealso cref="StartCap"/>
		/// </summary>
		/// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used for a cap style used at the end of an annotation line.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(LineAnchorCapStyle.None),
		SRDescription("DescriptionAttributeStartCap3"),
		]
		virtual public LineAnchorCapStyle EndCap
		{
			get
			{
				return _endCap;
			}
			set
			{
				_endCap = value;
				Invalidate();
			}
		}

		#endregion

		#region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		/// <value>
		/// A <see cref="ContentAlignment"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(ContentAlignment), "MiddleCenter"),
		]
		override public ContentAlignment Alignment
		{
			get
			{
				return base.Alignment;
			}
			set
			{
				base.Alignment = value;
			}
		}

        /// <summary>
        /// Gets or sets an annotation's text style.
        /// <seealso cref="Font"/>
        /// 	<seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="TextStyle"/> value used to draw an annotation's text.
        /// </value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override TextStyle TextStyle
        {
            get
            {
                return base.TextStyle;
            }
            set
            {
                base.TextStyle = value;
            }
        }

		/// <summary>
		/// Not applicable to this annotation type.
		/// <seealso cref="Font"/>
		/// </summary>
		/// <value>
		/// A <see cref="Color"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeForeColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// <seealso cref="ForeColor"/>
		/// </summary>
		/// <value>
		/// A <see cref="Font"/> object.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
		]
		override public Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		override public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return base.BackHatchStyle;
			}
			set
			{
				base.BackHatchStyle = value;
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]		
		override public GradientStyle BackGradientStyle
		{
			get
			{
				return base.BackGradientStyle;
			}
			set
			{
				base.BackGradientStyle = value;
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		] 
		override public Color BackSecondaryColor
		{
			get
			{
				return base.BackSecondaryColor;
			}
			set
			{
				base.BackSecondaryColor = value;
			}
		}

		#endregion

		#region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
        /// (e.g. Line, Rectangle, Ellipse). 
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAnnotationType"),
		]
		public override string AnnotationType
		{
			get
			{
				return "Polyline";
			}
		}

		/// <summary>
		/// Gets or sets an annotation selection points style.
		/// </summary>
		/// <value>
		/// A <see cref="SelectionPointsStyle"/> value that represents the annotation 
		/// selection style.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(SelectionPointsStyle.Rectangle),
		ParenthesizePropertyNameAttribute(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeSelectionPointsStyle"),
		]
		override internal SelectionPointsStyle SelectionPointsStyle
		{
			get
			{
				return SelectionPointsStyle.Rectangle;
			}
		}

		/// <summary>
		/// Gets or sets a flag that determines whether an annotation should be placed using the free-draw mode.
		/// </summary>
		/// <value>
		/// <b>True</b> if an annotation should be placed using free-draw mode, 
		/// <b>false</b> otherwise.  Defaults to <b>false</b>.
		/// </value>
		/// <remarks>
		/// Two different placement modes are supported when the Annotation.BeginPlacement 
		/// method is called. Set this property to <b>true</b> to switch from the default 
		/// mode to free-draw mode, which allows the caller to free-draw while moving the mouse cursor.
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeFreeDrawPlacement"),
#if !Microsoft_CONTROL
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
#endif // !Microsoft_CONTROL
		]
		virtual public bool IsFreeDrawPlacement
		{
			get
			{
				return isFreeDrawPlacement;
			}
			set
			{
				isFreeDrawPlacement = value;
			}
		}

		/// <summary>
		/// Gets or sets the path points of a polyline at run-time.
		/// </summary>
		/// <value>
		/// A <see cref="GraphicsPath"/> object with the polyline shape.
		/// </value>
		/// <remarks>
		/// A polyline must use coordinates relative to an annotation object, where (0,0) is 
		/// the top-left coordinates and (100,100) is the bottom-right coordinates of the annotation.  
		/// <para>
		/// This property is not accessible at design time (at design-time, use the 
		/// <see cref="GraphicsPathPoints"/> property instead).
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(null),
		SRDescription("DescriptionAttributePath"),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		]
		virtual public GraphicsPath GraphicsPath
		{
			get
			{
				return _graphicsPath;
			}
			set
			{
				_graphicsPath = value;
				this.pathChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the path points of the polyline at design-time.
		/// </summary>
		/// <value>
		/// An <see cref="AnnotationPathPointCollection"/> object with the polyline shape.
		/// </value>
		/// <remarks>
		/// A polyline must use coordinates relative to an annotation object, where (0,0) is 
		/// the top-left coordinates and (100,100) is the bottom-right coordinates of the annotation.
		/// <para>
        /// This property is not accessible at runtime (at runtime, use the <see cref="GraphicsPath"/> 
		/// property instead).
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		SRDescription("DescriptionAttributePathPoints"),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty)
#endif
		]
		public AnnotationPathPointCollection GraphicsPathPoints
		{
			get
			{
				if(this.pathChanged ||
					_graphicsPath.PointCount != _pathPoints.Count)
				{
					// Recreate collection from graphics path
					_pathPoints.annotation = null;
					_pathPoints.Clear();
                    if (_graphicsPath.PointCount > 0 )
                    {
                        PointF[] points = _graphicsPath.PathPoints;
                        byte[] types = _graphicsPath.PathTypes;
                        for (int index = 0; index < points.Length; index++)
                        {
                            _pathPoints.Add(new AnnotationPathPoint(points[index].X, points[index].Y, types[index]));
                        }
                    }
					_pathPoints.annotation = this;
				}
				return _pathPoints;
			}
		}

		#endregion

		#endregion

		#region Methods

		#region Painting

		/// <summary>
		/// Paints an annotation object on the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the <see cref="Chart"/> owner control.
		/// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
		{
			// Check for empty path
			if(_graphicsPath.PointCount == 0)
			{
				return;
			}

			// Get annotation position in relative coordinates
			PointF firstPoint = PointF.Empty;
			PointF anchorPoint = PointF.Empty;
			SizeF size = SizeF.Empty;
			GetRelativePosition(out firstPoint, out size, out anchorPoint);
			PointF	secondPoint = new PointF(firstPoint.X + size.Width, firstPoint.Y + size.Height);

			// Create selection rectangle
			RectangleF selectionRect = new RectangleF(firstPoint, new SizeF(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y));

			// Get position
			RectangleF	rectanglePosition = new RectangleF(selectionRect.Location, selectionRect.Size);
			if(rectanglePosition.Width < 0)
			{
				rectanglePosition.X = rectanglePosition.Right;
				rectanglePosition.Width = -rectanglePosition.Width;
			}
			if(rectanglePosition.Height < 0)
			{
				rectanglePosition.Y = rectanglePosition.Bottom;
				rectanglePosition.Height = -rectanglePosition.Height;
			}

			// Check if position is valid
			if( float.IsNaN(rectanglePosition.X) || 
				float.IsNaN(rectanglePosition.Y) || 
				float.IsNaN(rectanglePosition.Right) || 
				float.IsNaN(rectanglePosition.Bottom) )
			{
				return;
			}

			// Get annotation absolute position
			RectangleF rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

			// Calculate scaling
			float groupScaleX = rectanglePositionAbs.Width / 100.0f;
			float groupScaleY = rectanglePositionAbs.Height / 100.0f;
			
			// Convert path to pixel coordinates
			PointF[] pathPoints = _graphicsPath.PathPoints;
			byte[] pathTypes = _graphicsPath.PathTypes;
			for(int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
			{
				pathPoints[pointIndex].X = rectanglePositionAbs.X + pathPoints[pointIndex].X * groupScaleX;
				pathPoints[pointIndex].Y = rectanglePositionAbs.Y + pathPoints[pointIndex].Y * groupScaleY;
			}

            using (GraphicsPath pathAbs = new GraphicsPath(pathPoints, pathTypes))
            {

                // Set line caps
                bool capChanged = false;
                LineCap oldStartCap = LineCap.Flat;
                LineCap oldEndCap = LineCap.Flat;
                if (!this.isPolygon)
                {
                    if (this._startCap != LineAnchorCapStyle.None ||
                        this._endCap != LineAnchorCapStyle.None)
                    {
                        capChanged = true;
                        oldStartCap = graphics.Pen.StartCap;
                        oldEndCap = graphics.Pen.EndCap;

                        // Apply anchor cap settings
                        if (this._startCap == LineAnchorCapStyle.Arrow)
                        {
                            // Adjust arrow size for small line width
                            if (this.LineWidth < 4)
                            {
                                int adjustment = 3 - this.LineWidth;
                                graphics.Pen.StartCap = LineCap.Custom;
                                graphics.Pen.CustomStartCap = new AdjustableArrowCap(
                                    this.LineWidth + adjustment,
                                    this.LineWidth + adjustment,
                                    true);
                            }
                            else
                            {
                                graphics.Pen.StartCap = LineCap.ArrowAnchor;
                            }
                        }
                        else if (this._startCap == LineAnchorCapStyle.Diamond)
                        {
                            graphics.Pen.StartCap = LineCap.DiamondAnchor;
                        }
                        else if (this._startCap == LineAnchorCapStyle.Round)
                        {
                            graphics.Pen.StartCap = LineCap.RoundAnchor;
                        }
                        else if (this._startCap == LineAnchorCapStyle.Square)
                        {
                            graphics.Pen.StartCap = LineCap.SquareAnchor;
                        }
                        if (this._endCap == LineAnchorCapStyle.Arrow)
                        {
                            // Adjust arrow size for small line width
                            if (this.LineWidth < 4)
                            {
                                int adjustment = 3 - this.LineWidth;
                                graphics.Pen.EndCap = LineCap.Custom;
                                graphics.Pen.CustomEndCap = new AdjustableArrowCap(
                                    this.LineWidth + adjustment,
                                    this.LineWidth + adjustment,
                                    true);
                            }
                            else
                            {
                                graphics.Pen.EndCap = LineCap.ArrowAnchor;
                            }
                        }
                        else if (this._endCap == LineAnchorCapStyle.Diamond)
                        {
                            graphics.Pen.EndCap = LineCap.DiamondAnchor;
                        }
                        else if (this._endCap == LineAnchorCapStyle.Round)
                        {
                            graphics.Pen.EndCap = LineCap.RoundAnchor;
                        }
                        else if (this._endCap == LineAnchorCapStyle.Square)
                        {
                            graphics.Pen.EndCap = LineCap.SquareAnchor;
                        }
                    }
                }

                // Painting mode
                if (this.Common.ProcessModePaint)
                {
                    if (this.isPolygon)
                    {
                        // Draw polygon
                        pathAbs.CloseAllFigures();
                        graphics.DrawPathAbs(
                            pathAbs,
                            this.BackColor,
                            this.BackHatchStyle,
                            String.Empty,
                            ChartImageWrapMode.Scaled,
                            Color.Empty,
                            ChartImageAlignmentStyle.Center,
                            this.BackGradientStyle,
                            this.BackSecondaryColor,
                            this.LineColor,
                            this.LineWidth,
                            this.LineDashStyle,
                            PenAlignment.Center,
                            this.ShadowOffset,
                            this.ShadowColor);
                    }
                    else
                    {
                        // Draw polyline
                        graphics.DrawPathAbs(
                            pathAbs,
                            Color.Transparent,
                            ChartHatchStyle.None,
                            String.Empty,
                            ChartImageWrapMode.Scaled,
                            Color.Empty,
                            ChartImageAlignmentStyle.Center,
                            GradientStyle.None,
                            Color.Empty,
                            this.LineColor,
                            this.LineWidth,
                            this.LineDashStyle,
                            PenAlignment.Center,
                            this.ShadowOffset,
                            this.ShadowColor);
                    }
                }

                if (this.Common.ProcessModeRegions)
                {
                    // Create line graphics path
                    GraphicsPath selectionPath = null;
                    GraphicsPath newPath = null;

                    if (this.isPolygon)
                    {
                        selectionPath = pathAbs;
                    }
                    else
                    {
                        newPath = new GraphicsPath();
                        selectionPath = newPath;
                        selectionPath.AddPath(pathAbs, false);
                        using (Pen pen = (Pen)graphics.Pen.Clone())
                        {
                            // Increase pen size by 2 pixels
                            pen.DashStyle = DashStyle.Solid;
                            pen.Width += 2;
                            try
                            {
                                selectionPath.Widen(pen);
                            }
                            catch (OutOfMemoryException)
                            {
                                // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                                // catching here and reacting by not widening
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }

                    // Add hot region
                    this.Common.HotRegionsList.AddHotRegion(
                        graphics,
                        selectionPath,
                        false,
                        ReplaceKeywords(this.ToolTip),
#if Microsoft_CONTROL
					String.Empty,
					String.Empty,
					String.Empty,
#else // Microsoft_CONTROL
 ReplaceKeywords(this.Url),
						ReplaceKeywords(this.MapAreaAttributes),
                        ReplaceKeywords(this.PostBackValue),
#endif // Microsoft_CONTROL
 this,
                        ChartElementType.Annotation);

                    //Clean up
                    if (newPath != null)
                        newPath.Dispose();
                }

                // Restore line caps
                if (capChanged)
                {
                    graphics.Pen.StartCap = oldStartCap;
                    graphics.Pen.EndCap = oldEndCap;
                }

                // Paint selection handles
                PaintSelectionHandles(graphics, rectanglePosition, pathAbs);
            }
		}

		#endregion // Painting

		#region Position Changing
#if Microsoft_CONTROL
		/// <summary>
		/// Changes annotation position, so it exactly matches the bounary of the 
		/// polyline path.
		/// </summary>
		private void ResizeToPathBoundary()
		{
			if(_graphicsPath.PointCount > 0)
			{
				// Get current annotation position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				GetRelativePosition(out firstPoint, out size, out anchorPoint);

				// Get path boundary and convert it to relative coordinates
				RectangleF pathBoundary = _graphicsPath.GetBounds();
				pathBoundary.X *= size.Width / 100f;
				pathBoundary.Y *= size.Height / 100f;
				pathBoundary.X += firstPoint.X;
				pathBoundary.Y += firstPoint.Y;
				pathBoundary.Width *= size.Width / 100f;
				pathBoundary.Height *= size.Height / 100f;

				// Scale all current points
				using( Matrix matrix = new Matrix() )
				{
					matrix.Scale(size.Width/pathBoundary.Width, size.Height/pathBoundary.Height);
					matrix.Translate(-pathBoundary.X, -pathBoundary.Y);
					_graphicsPath.Transform(matrix);
				}

				// Set new position for annotation
				this.SetPositionRelative(pathBoundary, anchorPoint);
			}
		}
#endif //Microsoft_CONTROL
        /// <summary>
        /// Adjust annotation location and\or size as a result of user action.
        /// </summary>
        /// <param name="movingDistance">Distance to resize/move the annotation.</param>
        /// <param name="resizeMode">Resizing mode.</param>
        /// <param name="pixelCoord">Distance is in pixels, otherwise relative.</param>
        /// <param name="userInput">Indicates if position changing was a result of the user input.</param>
		override internal void AdjustLocationSize(SizeF movingDistance, ResizingMode resizeMode, bool pixelCoord, bool userInput)
		{
			// Call base class when not resizing the path points
			if(resizeMode != ResizingMode.MovingPathPoints)
			{
				base.AdjustLocationSize(movingDistance, resizeMode, pixelCoord, userInput);
				return;
			}

			// Get annotation position in relative coordinates
			PointF firstPoint = PointF.Empty;
			PointF anchorPoint = PointF.Empty;
			SizeF size = SizeF.Empty;
			GetRelativePosition(out firstPoint, out size, out anchorPoint);

			// Remember path before moving operation
			if(userInput == true && startMovePathRel == null)
			{
#if Microsoft_CONTROL
				this.startMovePathRel = (GraphicsPath)_graphicsPath.Clone();
				this.startMovePositionRel = new RectangleF(firstPoint, size);
				this.startMoveAnchorLocationRel = new PointF(anchorPoint.X, anchorPoint.Y);

#endif // Microsoft_CONTROL
			}

			// Convert moving distance to coordinates relative to the anotation
			if(pixelCoord)
			{
				movingDistance = this.GetGraphics().GetRelativeSize(movingDistance);
			}
			movingDistance.Width /= startMovePositionRel.Width / 100.0f;
			movingDistance.Height /= startMovePositionRel.Height / 100.0f;

			// Get path points and adjust position of one of them
			if(_graphicsPath.PointCount > 0)
			{
				GraphicsPath pathToMove = (userInput) ? startMovePathRel : _graphicsPath;
				PointF[] pathPoints = pathToMove.PathPoints;
				byte[] pathTypes = pathToMove.PathTypes;

				for(int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
				{
					// Adjust position
					if( currentPathPointIndex == pointIndex ||
						currentPathPointIndex < 0 ||
						currentPathPointIndex >= pathPoints.Length )
					{
						pathPoints[pointIndex].X -= movingDistance.Width;
						pathPoints[pointIndex].Y -= movingDistance.Height;
					}
				}

#if Microsoft_CONTROL

				// Adjust annotation position to the boundary of the path
				if(userInput && this.AllowResizing)
				{
					// Get path bounds in relative coordinates
                    _defaultGraphicsPath.Dispose();
                    _defaultGraphicsPath = new GraphicsPath(pathPoints, pathTypes);
					_graphicsPath = _defaultGraphicsPath;

                    RectangleF pathBounds = _graphicsPath.GetBounds();
					pathBounds.X *= startMovePositionRel.Width / 100f;
					pathBounds.Y *= startMovePositionRel.Height / 100f;
					pathBounds.X += startMovePositionRel.X;
					pathBounds.Y += startMovePositionRel.Y;
					pathBounds.Width *= startMovePositionRel.Width / 100f;
					pathBounds.Height *= startMovePositionRel.Height / 100f;

					// Set new annotation position
					this.SetPositionRelative(pathBounds, anchorPoint);

					// Adjust path point position
					for(int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
					{
	
						pathPoints[pointIndex].X = startMovePositionRel.X + pathPoints[pointIndex].X * (startMovePositionRel.Width / 100f);
						pathPoints[pointIndex].Y = startMovePositionRel.Y + pathPoints[pointIndex].Y * (startMovePositionRel.Height / 100f);

						pathPoints[pointIndex].X = (pathPoints[pointIndex].X - pathBounds.X) / (pathBounds.Width / 100f);
						pathPoints[pointIndex].Y = (pathPoints[pointIndex].Y - pathBounds.Y) / (pathBounds.Height / 100f);
					}
				}

#endif // Microsoft_CONTROL

#if Microsoft_CONTROL
				// Position changed
				this.positionChanged = true;
#endif // Microsoft_CONTROL
			
				// Recreate path with new points
                _defaultGraphicsPath.Dispose();
                _defaultGraphicsPath = new GraphicsPath(pathPoints, pathTypes);
                _graphicsPath = _defaultGraphicsPath;
				this.pathChanged = true;

				// Invalidate annotation
				this.Invalidate();
			}
		}

		#endregion // Position Changing

		#region Placement Methods

#if Microsoft_CONTROL

		/// <summary>
		/// Ends user placement of an annotation.
		/// </summary>
		/// <remarks>
		/// Ends an annotation placement operation previously started by a 
		/// <see cref="Annotation.BeginPlacement"/> method call.
		/// <para>
		/// Calling this method is not required, since placement will automatically
		/// end when an end user enters all required points. However, it is useful when an annotation 
		/// placement operation needs to be aborted for some reason.
		/// </para>
		/// </remarks>
		override public void EndPlacement()
		{
			// Call base method
			base.EndPlacement();

			// Position was changed
			if(this.Chart != null)
			{
				this.Chart.OnAnnotationPositionChanged(this);
			}

			// Reset last placement position
			this.lastPlacementPosition = PointF.Empty;

			// Resize annotation to the boundary of the polygon
			ResizeToPathBoundary();

			// Position changed
			this.positionChanged = true;
		}

		/// <summary>
		/// Handles mouse down event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		/// <param name="buttons">Mouse button down.</param>
		internal override void PlacementMouseDown(PointF point, MouseButtons buttons)
		{
			// Call base class method if path editing is not allowed 
			if(!this.AllowPathEditing)
			{
				base.PlacementMouseDown(point, buttons);
				return;
			}

			if(buttons == MouseButtons.Right)
			{
				// Stop pacement
				this.EndPlacement();
			}
			if(buttons == MouseButtons.Left &&
				IsValidPlacementPosition(point.X, point.Y))
			{
				// Convert coordinate to relative
				PointF newPoint = this.GetGraphics().GetRelativePoint(point);

				if(this.lastPlacementPosition.IsEmpty)
				{
					// Set annotation coordinates to full chart
					this.X = 0f;
					this.Y = 0f;
					this.Width = 100f;
					this.Height = 100f;

					// Remeber position where mouse was clicked
					this.lastPlacementPosition = newPoint;
				}
				else
				{
					if(this.lastPlacementPosition.X == newPoint.X && 
						this.lastPlacementPosition.Y == newPoint.Y)
					{
						// Stop pacement
						this.EndPlacement();
					}
				}

				// Add a line from prev. position to current into the path
				using( GraphicsPath tmpPath = new GraphicsPath() )
				{
					PointF firstPoint = this.lastPlacementPosition;
					if(_graphicsPath.PointCount > 1)
					{
						firstPoint = _graphicsPath.GetLastPoint();
					}
					tmpPath.AddLine(firstPoint, newPoint);
					_graphicsPath.AddPath(tmpPath, true);
				}

				// Remember last position
				this.lastPlacementPosition = newPoint;

				// Invalidate and update the chart
				if(Chart != null)
				{
					Invalidate();
					Chart.UpdateAnnotations();
				}
			}
		}

		/// <summary>
		/// Handles mouse up event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		/// <param name="buttons">Mouse button Up.</param>
		/// <returns>Return true when placing finished.</returns>
		internal override bool PlacementMouseUp(PointF point, MouseButtons buttons)
		{
			// Call base class method if path editing is not allowed 
			if(!this.AllowPathEditing)
			{
				return base.PlacementMouseUp(point, buttons);
			}

			if(buttons == MouseButtons.Left &&
				isFreeDrawPlacement)
			{
				// Stop pacement
				this.EndPlacement();

			}

			return false;
		}

		/// <summary>
		/// Handles mouse move event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		internal override void PlacementMouseMove(PointF point)
		{
			// Call base class method if path editing is not allowed 
			if(!this.AllowPathEditing)
			{
				base.PlacementMouseMove(point);
				return;
			}

			// Check if annotation was moved
			if( this.GetGraphics() != null &&
				_graphicsPath.PointCount > 0 &&
				!this.lastPlacementPosition.IsEmpty)
			{
				// Convert coordinate to relative
				PointF newPoint = this.GetGraphics().GetRelativePoint(point);
				if(this.isFreeDrawPlacement)
				{
					// Add new point
					using( GraphicsPath tmpPath = new GraphicsPath() )
					{
						PointF firstPoint = this.lastPlacementPosition;
						if(_graphicsPath.PointCount > 1)
						{
							firstPoint = _graphicsPath.GetLastPoint();
						}
						tmpPath.AddLine(firstPoint, newPoint);
						_graphicsPath.AddPath(tmpPath, true);
					}
				}
				else
				{
					// Adjust last point position
					PointF[] pathPoints = _graphicsPath.PathPoints;
					byte[] pathTypes = _graphicsPath.PathTypes;
					pathPoints[pathPoints.Length - 1] = newPoint;

                    _defaultGraphicsPath.Dispose();
                    _defaultGraphicsPath = new GraphicsPath(pathPoints, pathTypes);
                    _graphicsPath = _defaultGraphicsPath;

				}

				// Position changed
				this.positionChanged = true;

				// Invalidate and update the chart
				if(this.Chart != null)
				{
					Invalidate();
					this.Chart.UpdateAnnotations();
				}
			}
		}

#endif // Microsoft_CONTROL

        #endregion // Placement Methods

        #endregion

        #region IDisposable override 
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_defaultGraphicsPath != null)
                {
                    _defaultGraphicsPath.Dispose();
                    _defaultGraphicsPath = null;
                }
                if (_pathPoints != null)
                {
                    _pathPoints.Dispose();
                    _pathPoints = null;
                }

            }
            base.Dispose(disposing);
        }
        #endregion

    }

	/// <summary>
	/// <b>PolygonAnnotation</b> is a class that represents a polygon annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributePolygonAnnotation_PolygonAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class PolygonAnnotation : PolylineAnnotation
	{
		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public PolygonAnnotation() 
            : base()
		{
			this.isPolygon = true;
		}

		#endregion

		#region Properties

		#region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

		/// <summary>
		/// Not applicable to this annotation type.
		/// <seealso cref="EndCap"/>
		/// </summary>
		/// <value>
		/// A <see cref="LineAnchorCapStyle"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(LineAnchorCapStyle.None),
		]
		override public LineAnchorCapStyle StartCap
		{
			get
			{
				return base.StartCap;
			}
			set
			{
				base.StartCap = value;
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// <seealso cref="StartCap"/>
		/// </summary>
		/// <value>
		/// A <see cref="LineAnchorCapStyle"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(LineAnchorCapStyle.None),
		]
		override public LineAnchorCapStyle EndCap
		{
			get
			{
				return base.EndCap;
			}
			set
			{
				base.EndCap = value;
			}
		}

		#endregion

		#region Applicable Annotation Appearance Attributes (set as Browsable)

        /// <summary>
        /// Gets or sets the background color of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the background of an annotation.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackColor"),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		override public Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

        /// <summary>
        /// Gets or sets the background hatch style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(true),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeBackHatchStyle"),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		override public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return base.BackHatchStyle;
			}
			set
			{
				base.BackHatchStyle = value;
			}
		}

        /// <summary>
        /// Gets or sets the background gradient style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(true),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
                SRDescription("DescriptionAttributeBackGradientStyle"),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]		
		override public GradientStyle BackGradientStyle
		{
			get
			{
				return base.BackGradientStyle;
			}
			set
			{
				base.BackGradientStyle = value;
			}
		}

        /// <summary>
        /// Gets or sets the secondary background color of an annotation.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of an annotation background with 
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(true),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		] 
		override public Color BackSecondaryColor
		{
			get
			{
				return base.BackSecondaryColor;
			}
			set
			{
				base.BackSecondaryColor = value;
			}
		}

		#endregion

		#region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
        /// (e.g. Line, Rectangle, Ellipse). 
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAnnotationType"),
		]
		public override string AnnotationType
		{
			get
			{
				return "Polygon";
			}
		}

		/// <summary>
		/// Gets or sets an annotation's selection points style.
		/// </summary>
		/// <value>
		/// A <see cref="SelectionPointsStyle"/> value that represents an annotation's 
		/// selection style.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(SelectionPointsStyle.Rectangle),
		ParenthesizePropertyNameAttribute(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeSelectionPointsStyle"),
		]
		override internal SelectionPointsStyle SelectionPointsStyle
		{
			get
			{
				return SelectionPointsStyle.Rectangle;
			}
		}

		#endregion

		#endregion
	}

    /// <summary><b>AnnotationPathPointCollection</b> is a collection of polyline 
    /// annotation path points, and is only available via the <b>GraphicsPathPoints</b> 
    /// property at design-time.
    /// <seealso cref="PolylineAnnotation.GraphicsPathPoints"/></summary>
    /// <remarks>
    /// This collection is used at design-time only, and uses serialization to expose the 
    /// shape of the polyline and polygon via their GraphicsPathPoints collection property.
    /// At run-time, use Path property to set the path of a polyline or polygon
    /// </remarks>
	[
		SRDescription("DescriptionAttributeAnnotationPathPointCollection_AnnotationPathPointCollection"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AnnotationPathPointCollection : ChartElementCollection<AnnotationPathPoint>
	{
		#region Fields

		internal		PolylineAnnotation	annotation = null;
        private         GraphicsPath        _graphicsPath = null;

		#endregion // Fields

		#region Constructors

		/// <summary>
		/// Default public constructor.
		/// </summary>
        public AnnotationPathPointCollection(PolylineAnnotation annotation)
            : base(annotation)
		{
            this.annotation = annotation;
		}

		#endregion // Constructors

		#region Methods

        /// <summary>
        /// Forces the invalidation of the chart element
        /// </summary>
        public override void Invalidate()
        {
            if (this.annotation != null)
            {
                //Dispose previously instantiated graphics path
                if (this._graphicsPath != null)
                {
                    this._graphicsPath.Dispose();
                    this._graphicsPath = null;
                }

                // Recreate polyline annotation path
                if (this.Count > 0)
                {
                    PointF[] points = new PointF[this.Count];
                    byte[] types = new byte[this.Count];
                    for (int index = 0; index < this.Count; index++)
                    {
                        points[index] = new PointF(this[index].X, this[index].Y);
                        types[index] = this[index].PointType;
                    }
                    this._graphicsPath = new GraphicsPath(points, types);
                }
                else
                {
                    this._graphicsPath = new GraphicsPath();
                }

                // Invalidate annotation
                this.annotation.GraphicsPath = this._graphicsPath;
                this.annotation.Invalidate();
            }
            base.Invalidate();
        }

		#endregion // Methods

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {   
                // Free up managed resources
                if (this._graphicsPath != null)
                {
                    this._graphicsPath.Dispose();
                    this._graphicsPath = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

	/// <summary>
	/// The <b>AnnotationPathPoint</b> class represents a path point of a polyline or polygon, 
	/// and is stored in their <b>GraphicsPathPoints</b> property, which is only available at design-time.
	/// </summary>
	/// <remarks>
	/// At run-time, use <b>Path</b> property to set the path of a polyline or polygon.
	/// </remarks>
	[
		SRDescription("DescriptionAttributeAnnotationPathPoint_AnnotationPathPoint"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AnnotationPathPoint: ChartElement
	{
		#region Fields

		// Point X value
		private float		_x = 0f;

		// Point Y value
		private float		_y = 0f;

		// Point type
		private byte		_pointType = 1;

		#endregion // Fields

		#region Constructors

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public AnnotationPathPoint()
		{
		}

		/// <summary>
		/// Constructor that takes X and Y parameters.
		/// </summary>
		/// <param name="x">Point's X value.</param>
		/// <param name="y">Point's Y value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification="X and Y are cartesian coordinates and well understood")]
        public AnnotationPathPoint(float x, float y)
		{
			this._x = x;
			this._y = y;
		}

		/// <summary>
		/// Constructor that takes X, Y and point type parameters.
		/// </summary>
		/// <param name="x">Point's X value.</param>
		/// <param name="y">Point's Y value.</param>
		/// <param name="type">Point type.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public AnnotationPathPoint(float x, float y, byte type)
		{
			this._x = x;
			this._y = y;
			this._pointType = type;
		}

		#endregion // Constructors

		#region Properties

		/// <summary>
		/// Gets or sets an annotation path point's X coordinate.
		/// </summary>
		/// <value>
		/// A float value for the point's X coordinate.
		/// </value>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(0f),
		Browsable(true),
		SRDescription("DescriptionAttributeAnnotationPathPoint_X"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        public float X
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
			}
		}

		/// <summary>
		/// Gets or sets an annotation path point's Y coordinate.
		/// </summary>
		/// <value>
		/// A float value for the point's Y coordinate.
		/// </value>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(0f),
		Browsable(true),
		SRDescription("DescriptionAttributeAnnotationPathPoint_Y"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public float Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
			}
		}

		/// <summary>
		/// Gets or sets an annotation path point's type.
		/// </summary>
		/// <value>
		/// A byte value.
		/// </value>
		/// <remarks>
		/// See the <see cref="PathPointType"/> enumeration for more details.
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(typeof(byte), "1"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		SRDescription("DescriptionAttributeAnnotationPathPoint_Name"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		]
		public byte PointType
		{
			get
			{
				return _pointType;
			}
			set
			{
				_pointType = value;
			}
		}

		/// <summary>
		/// Gets or sets an annotation path point's name.
		/// </summary>
		/// <para>
        /// This property is for internal use and is hidden at design and run time.
		/// </para>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue("PathPoint"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		SRDescription("DescriptionAttributeAnnotationPathPoint_Name"),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		]
		public string Name
		{
			get
			{
				return "PathPoint";
			}
		}

		#endregion // Properties

    }
}
