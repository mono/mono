//-------------------------------------------------------------
// <copyright company=�Microsoft Corporation�>
//   Copyright � Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartArea3D.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartArea3DStyle, ChartArea3D
//
//  Purpose:	ChartArea3D class represents 3D chart area. It contains
//              methods for coordinates transformation, drawing the 3D
//              scene and many 3D related helper methods.
//
//	Reviewed:	AG - Microsoft 16, 2007
//
//===================================================================

#region Used namespaces
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

#if WINFORMS_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
    using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI;
#endif


#endregion

#if WINFORMS_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region 3D lightStyle style enumerations

		/// <summary>
		/// A lighting style for a 3D chart area.
		/// </summary>
		public enum LightStyle
		{
			/// <summary>
			/// No lighting.
			/// </summary>
			None,
            /// <summary>
            /// Simplistic lighting.
            /// </summary>
			Simplistic,
            /// <summary>
            /// Realistic lighting.
            /// </summary>
			Realistic
		}

	#endregion

	#region 3D Center of Projetion coordinates enumeration

		/// <summary>
		/// Coordinates of the Center Of Projection
		/// </summary>
		[Flags]
		internal enum COPCoordinates
		{
			/// <summary>
			/// Check X coordinate.
			/// </summary>
			X = 1,
			/// <summary>
			/// Check Y coordinate.
			/// </summary>
			Y = 2,
			/// <summary>
			/// Check Z coordinate.
			/// </summary>
			Z = 4
		}

	#endregion

        /// <summary>
        /// The ChartArea3DStyleClass class provides the functionality for 3D attributes of chart areas,
        /// such as rotation angles and perspective.
        /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartArea3DStyle
	{
		#region Constructor and Initialization

		/// <summary>
        /// ChartArea3DStyle constructor.
		/// </summary>
		public ChartArea3DStyle()
		{
		}

		/// <summary>
        /// ChartArea3DStyle constructor.
		/// </summary>
		public ChartArea3DStyle(ChartArea chartArea)
		{
            this._chartArea = chartArea;
		}

        /// <summary>
        /// Initialize Chart area and axes
        /// </summary>
        /// <param name="chartArea">Chart area object.</param>
        internal void Initialize(ChartArea chartArea)
        {
            this._chartArea = chartArea;
        }

        #endregion
		
		#region Fields

		// Reference to the chart area object
		private	ChartArea	_chartArea = null;

		// Enables/disables 3D chart types in the area.
		private	bool		_enable3D	= false;

		// Indicates that axes are set at the right angle independent of the rotation.
		private	bool		_isRightAngleAxes	= true;

		// Indicates that series should be drawn as isClustered.
		private	bool		_isClustered	= false;

		// 3D area lightStyle style.
		private LightStyle	_lightStyle = LightStyle.Simplistic;

		// 3D area perspective which controls the scaleView of the chart depth.
		private int			_perspective = 0;

		// Chart area rotation angle around the X axis.
		private int			_inclination = 30;

		// Chart area rotation angle around the Y axis.
		private int			_rotation = 30;

		// Chart area walls width.
		private int			_wallWidth = 7;

		// Series points depth in percentages
		private int			_pointDepth = 100;

		// Series points gap depth in percentages
		private int			_pointGapDepth = 100;

		#endregion

		#region Properties

        /// <summary>
        /// Gets or sets a Boolean value that toggles 3D for a chart area on and off.
        /// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(false),
		SRDescription("DescriptionAttributeChartArea3DStyle_Enable3D"),
		ParenthesizePropertyNameAttribute(true)
		]
		public bool Enable3D
		{
			get
			{
                return this._enable3D;
			}
			set
			{
                if (this._enable3D != value)
				{
                    this._enable3D = value;

                    if (this._chartArea != null)
					{
#if SUBAXES
						// If one of the axes has sub axis the scales has to be recalculated
						foreach(Axis axis in this._chartArea.Axes)
						{
							if(axis.SubAxes.Count > 0)
							{
								this._chartArea.ResetAutoValues();
								break;
							}
						}
#endif // SUBAXES

                        this._chartArea.Invalidate();
					}
				}
			}
		}


        /// <summary>
        /// Gets or sets a Boolean that determines if a chart area  is displayed using an isometric projection.  
        /// </summary>
		[
        SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeChartArea3DStyle_RightAngleAxes"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public bool IsRightAngleAxes
		{
			get
			{
                return _isRightAngleAxes;
			}
			set
			{
                _isRightAngleAxes = value;

				// Adjust 3D properties values
                if (_isRightAngleAxes)
				{
					// Disable perspective if right angle axis are used
                    this._perspective = 0;
				}

                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}


        /// <summary>
        /// Gets or sets a Boolean value that determines if bar chart or column 
        /// chart data series are clustered (displayed along distinct rows).  
        /// </summary>
		[
        SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(false),
		SRDescription("DescriptionAttributeChartArea3DStyle_Clustered"),
		]
		public bool IsClustered
		{
			get
			{
                return _isClustered;
			}
			set
			{
                _isClustered = value;
                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the style of lighting for a 3D chart area.  
		/// </summary>
		[
        SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(typeof(LightStyle), "Simplistic"),
		SRDescription("DescriptionAttributeChartArea3DStyle_Light"),
		]
		public LightStyle LightStyle
		{
			get
			{
                return _lightStyle;
			}
			set
			{
                _lightStyle = value;
                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the percent of perspective for a 3D chart area.
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeChartArea3DStyle_Perspective"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int Perspective
		{
			get
			{
                return _perspective;
			}
			set
			{
				if(value < 0 || value > 100)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DPerspectiveInvalid));
				}

                _perspective = value;

				// Adjust 3D properties values
                if (_perspective != 0)
				{
					// Disable right angle axes
                    this._isRightAngleAxes = false;
				}

                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the inclination for a 3D chart area.
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(30),
		SRDescription("DescriptionAttributeChartArea3DStyle_Inclination"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int Inclination
		{
			get
			{
                return _inclination;
			}
			set
			{
				if(value < -90 || value > 90)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DInclinationInvalid));
				}
                _inclination = value;

                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the rotation angle for a 3D chart area.
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(30),
		SRDescription("DescriptionAttributeChartArea3DStyle_Rotation"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int Rotation
		{
			get
			{
                return _rotation;
			}
			set
			{
				if(value < -180 || value > 180)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DRotationInvalid));
				}
                _rotation = value;

                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the width of the walls displayed in 3D chart areas.  
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(7),
		SRDescription("DescriptionAttributeChartArea3DStyle_WallWidth"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int WallWidth
		{
			get
			{
                return _wallWidth;
			}
			set
			{
				if(value < 0 || value > 30)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DWallWidthInvalid));
				}

                _wallWidth = value;
                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the depth of data points displayed in 3D chart areas (0-1000%).  
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(100),
		SRDescription("DescriptionAttributeChartArea3DStyle_PointDepth"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int PointDepth
		{
			get
			{
                return _pointDepth;
			}
			set
			{
				if(value < 0 || value > 1000)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DPointsDepthInvalid));
				}

                _pointDepth = value;
                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		/// <summary>
        /// Gets or sets the distance between series rows in 3D chart areas (0-1000%).
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(100),
		SRDescription("DescriptionAttributeChartArea3DStyle_PointGapDepth"),
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int PointGapDepth
		{
			get
			{
                return _pointGapDepth;
			}
			set
			{
				if(value < 0 || value > 1000)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionChartArea3DPointsGapInvalid));
				}

                _pointGapDepth = value;
                if (this._chartArea != null)
				{
                    this._chartArea.Invalidate();
				}
			}
		}

		#endregion
	}

	/// <summary>
    /// ChartArea3D class represents 3D chart area. It contains all the 3D 
    /// scene settings and methods for drawing the 3D plotting area, and calculating 
    /// the depth of chart elements.
	/// </summary>
	public partial class ChartArea
	{
		#region Fields

		// Chart area 3D style attribuytes
		private		ChartArea3DStyle	_area3DStyle = new ChartArea3DStyle();

		// Coordinate convertion matrix
		internal	Matrix3D			matrix3D = new Matrix3D();

		// Chart area scene wall width in relative coordinates
		internal	SizeF				areaSceneWallWidth = SizeF.Empty;

		// Chart area scene depth
		internal	float				areaSceneDepth = 0;

		// Visible surfaces in plotting area
		private		SurfaceNames			_visibleSurfaces;

		// Z axis depth of series points
		private		double				_pointsDepth = 0;

		// Z axis depth of the gap between isClustered series
		private		double				_pointsGapDepth = 0;

		/// <summary>
		/// Indicates that series order should be reversed to simulate Y axis rotation.
		/// </summary>
		private	bool				_reverseSeriesOrder = false;

		/// <summary>
		/// Old X axis reversed flag
		/// </summary>
		internal	bool				oldReverseX = false;

		/// <summary>
		/// Old Y axis reversed flag
		/// </summary>
		internal	bool				oldReverseY = false;

		/// <summary>
		/// Old Y axis rotation angle
		/// </summary>
		internal	int					oldYAngle = 30;

		/// <summary>
		/// List of all stack group names
		/// </summary>
		private	ArrayList			_stackGroupNames = null;

        /// <summary>
        /// This list contains an array of series names for each 3D cluster
        /// </summary>
		internal	List<List<string>>	seriesClusters = null;

        #endregion

		#region 3D Style properties

		/// <summary>
        /// Gets or sets a ChartArea3DStyle object, used to draw all series in a chart area in 3D.
		/// </summary>
		[
		SRCategory("CategoryAttribute3D"),
		Bindable(true),
		DefaultValue(null),
		SRDescription("DescriptionAttributeArea3DStyle"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		TypeConverter(typeof(NoNameExpandableObjectConverter)),
#if !WINFORMS_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public ChartArea3DStyle Area3DStyle
		{
			get
			{
                return _area3DStyle;
			}
			set
			{
                _area3DStyle = value;

				// Initialize style object
                _area3DStyle.Initialize((ChartArea)this);
			}
		}

        /// <summary>
        /// Indicates that series order should be reversed to simulate Y axis rotation.
        /// </summary>
        internal bool ReverseSeriesOrder
        {
            get { return _reverseSeriesOrder; }
        }

        /// <summary>
        /// Gets the list of all stack group names
        /// </summary>
        internal ArrayList StackGroupNames
        {
            get { return _stackGroupNames; }
        }

		#endregion

		#region 3D Coordinates transfotmation methods

		/// <summary>
        /// Call this method to apply 3D transformations on an array of 3D points (must be done before calling GDI+ drawing methods).
		/// </summary>
		/// <param name="points">3D Points array.</param>
		public void TransformPoints( Point3D[] points )
		{
			// Convert Z coordinates from 0-100% to axis values
			foreach(Point3D pt in points)
			{
				pt.Z = (pt.Z / 100f) * this.areaSceneDepth;
			}

			// Transform points
			this.matrix3D.TransformPoints( points );
		}

		#endregion

		#region 3D Scene drawing methods
		
		/// <summary>
		/// Draws chart area 3D scene, which consists of 3 or 2 walls.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="position">Chart area 2D position.</param>
		internal void DrawArea3DScene(ChartGraphics graph, RectangleF position)
		{
			// Reference to the chart area class
			ChartArea chartArea = (ChartArea)this;

			// Calculate relative size of the wall
			areaSceneWallWidth = graph.GetRelativeSize( new SizeF(this.Area3DStyle.WallWidth, this.Area3DStyle.WallWidth));

			//***********************************************************
			//** Calculate the depth of the chart area scene
			//***********************************************************
			areaSceneDepth = GetArea3DSceneDepth();

			//***********************************************************
			//** Initialize coordinate transformation matrix
			//***********************************************************
			this.matrix3D.Initialize(
				position, 
				areaSceneDepth, 
				this.Area3DStyle.Inclination,
				this.Area3DStyle.Rotation,
				this.Area3DStyle.Perspective,
				this.Area3DStyle.IsRightAngleAxes);

			//***********************************************************
			//** Initialize Lighting
			//***********************************************************
			this.matrix3D.InitLight( 
				this.Area3DStyle.LightStyle
			);

			//***********************************************************
			//** Find chart area visible surfaces
			//***********************************************************
            _visibleSurfaces = graph.GetVisibleSurfaces(
				position, 
				0, 
				areaSceneDepth, 
				this.matrix3D);

			//***********************************************************
			//** Chech if area scene should be drawn
			//***********************************************************
			Color	sceneBackColor = chartArea.BackColor;

			// Do not draw the transparent walls
			if(sceneBackColor == Color.Transparent)
			{
				// Area wall is not visible
				areaSceneWallWidth = SizeF.Empty;
				return;
			}

			// If color is not set (default) - use LightGray
			if(sceneBackColor == Color.Empty)
			{
				sceneBackColor = Color.LightGray;
			}

			//***********************************************************
			//** Adjust scene 2D rectangle so that wall are drawn
			//** outside plotting area.
			//***********************************************************
			// If bottom wall is visible
			if(IsBottomSceneWallVisible())
			{
				position.Height += areaSceneWallWidth.Height;
			}

			// Adjust for the left/right wall
			position.Width += areaSceneWallWidth.Width;
			if(this.Area3DStyle.Rotation > 0)
			{
				position.X -= areaSceneWallWidth.Width;
			}

			//***********************************************************
			//** Draw scene walls
			//***********************************************************

			// Draw back wall
			RectangleF	wallRect2D = new RectangleF(position.Location, position.Size);
			float		wallDepth = areaSceneWallWidth.Width;
			float		wallZPosition = -wallDepth;

			// For isometric projection Front wall should be visible sometimes
			if( IsMainSceneWallOnFront())
			{
				wallZPosition = areaSceneDepth;
			}

			graph.Fill3DRectangle( 
				wallRect2D,
				wallZPosition,
				wallDepth,
				this.matrix3D,
				chartArea.Area3DStyle.LightStyle,
				sceneBackColor, 
				chartArea.BorderColor, 
				chartArea.BorderWidth, 
				chartArea.BorderDashStyle, 
				DrawingOperationTypes.DrawElement );

			// Draw side wall on the left or right side
			wallRect2D = new RectangleF(position.Location, position.Size);
			wallRect2D.Width = areaSceneWallWidth.Width;
			if(!IsSideSceneWallOnLeft())
			{
				// Wall is on the right side
				wallRect2D.X = position.Right - areaSceneWallWidth.Width;
			}
			graph.Fill3DRectangle( 
				wallRect2D,
				0f,
				areaSceneDepth,
				this.matrix3D,
				chartArea.Area3DStyle.LightStyle,
				sceneBackColor, 
				chartArea.BorderColor, 
				chartArea.BorderWidth, 
				chartArea.BorderDashStyle, 
				DrawingOperationTypes.DrawElement);

			// Draw bottom wall
			if(IsBottomSceneWallVisible())
			{
				wallRect2D = new RectangleF(position.Location, position.Size);
				wallRect2D.Height = areaSceneWallWidth.Height;
				wallRect2D.Y = position.Bottom - areaSceneWallWidth.Height;
				wallRect2D.Width -= areaSceneWallWidth.Width;
				if(IsSideSceneWallOnLeft())
				{
					wallRect2D.X += areaSceneWallWidth.Width;
				}

				wallZPosition = 0;
				graph.Fill3DRectangle( 
					wallRect2D,
					0f,
					areaSceneDepth,
					this.matrix3D,
					chartArea.Area3DStyle.LightStyle,
					sceneBackColor, 
					chartArea.BorderColor, 
					chartArea.BorderWidth, 
					chartArea.BorderDashStyle, 
					DrawingOperationTypes.DrawElement );
			}

		}

		/// <summary>
		/// Helper method which return True if bottom wall of the 
		/// chart area scene is visible.
		/// </summary>
		/// <returns>True if bottom wall is visible.</returns>
		internal bool IsBottomSceneWallVisible()
		{
			return (this.Area3DStyle.Inclination >= 0);
		}

        /// <summary>
        /// Helper method which return True if main wall of the 
        /// chart area scene is displayed on the front side.
        /// </summary>
        /// <returns>True if front wall is visible.</returns>
		internal bool IsMainSceneWallOnFront()
		{
			// Note: Not used in this version!
			return false;
		}

        /// <summary>
        /// Helper method which return True if side wall of the 
        /// chart area scene is displayed on the left side.
        /// </summary>
        /// <returns>True if bottom wall is visible.</returns>
		internal bool IsSideSceneWallOnLeft()
		{
			return (this.Area3DStyle.Rotation > 0);
		}

		#endregion

		#region 3D Scene depth claculation methods

		/// <summary>
        /// Call this method to get the Z position of a series (useful for custom drawing).
		/// </summary>
        /// <param name="series">The series to retrieve the Z position for.</param>
		/// <returns>The Z position of the specified series. Measured as a percentage of the chart area's depth.</returns>
		public float GetSeriesZPosition(Series series)
		{
			float	positionZ, depth;
			GetSeriesZPositionAndDepth(series, out depth, out positionZ);
			return ((positionZ + depth/2f) / this.areaSceneDepth) * 100f;
		}

		/// <summary>
		/// Call this method to get the depth of a series in a chart area.
		/// </summary>
        /// <param name="series">The series to retrieve the depth for.</param>
		/// <returns>The depth of the specified series. Measured as a percentage of the chart area's depth.</returns>
		public float GetSeriesDepth(Series series)
		{
			float	positionZ, depth;
			GetSeriesZPositionAndDepth(series, out depth, out positionZ);
			return (depth / this.areaSceneDepth) * 100f;
		}

		/// <summary>
		/// Calculates area 3D scene depth depending on the number of isClustered 
		/// series and interval between points.
		/// </summary>
		/// <returns>Returns the depth of the chart area scene.</returns>
		private float GetArea3DSceneDepth()
		{
			//***********************************************************
			//** Calculate the smallest interval between points
			//***********************************************************

			// Check if any series attached to the area is indexed
            bool indexedSeries = ChartHelper.IndexedSeries(this.Common, this._series.ToArray());

			// Smallest interval series
			Series	smallestIntervalSeries = null;
			if(this._series.Count > 0)
			{
				smallestIntervalSeries = this.Common.DataManager.Series[(string)this._series[0]];
			}

			// Get X axis
			Axis	xAxis = ((ChartArea)this).AxisX;
			if(this._series.Count > 0)
			{
				Series	firstSeries = this.Common.DataManager.Series[this._series[0]];
				if(firstSeries != null && firstSeries.XAxisType == AxisType.Secondary)
				{
					xAxis = ((ChartArea)this).AxisX2;
				}
			}

			// Get smallest interval between points (use interval 1 for indexed series)
			double clusteredInterval = 1;
			if(!indexedSeries)
			{
				bool sameInterval;
				clusteredInterval = this.GetPointsInterval(this._series, xAxis.IsLogarithmic, xAxis.logarithmBase, false, out sameInterval, out smallestIntervalSeries);
			}

			//***********************************************************
			//** Check if "DrawSideBySide" attribute is set.
			//***********************************************************
			bool	drawSideBySide = false;
			if(smallestIntervalSeries != null)
			{
				drawSideBySide = Common.ChartTypeRegistry.GetChartType(smallestIntervalSeries.ChartTypeName).SideBySideSeries;
				foreach(string seriesName in this._series)
				{
					if(this.Common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
					{
						string attribValue = this.Common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
						if(String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase) == 0)
						{
							drawSideBySide = false;
						}
						else if(String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
						{
							drawSideBySide = true;
						}
                        else if (String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
						{
							// Do nothing
						}
						else
						{
                            throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
						}
					}
				}
			}

			// Get smallest interval cate----cal axis
			Axis	categoricalAxis = ((ChartArea)this).AxisX;
			if(smallestIntervalSeries != null && smallestIntervalSeries.XAxisType == AxisType.Secondary)
			{
				categoricalAxis = ((ChartArea)this).AxisX2;
			}

			//***********************************************************
			//** If series with the smallest interval is displayed
			//** side-by-side - devide the interval by number of series
			//** of the same chart type.
			//***********************************************************
			double pointWidthSize = 0.8;
			int	seriesNumber = 1;
			if(smallestIntervalSeries != null)
			{
				// Check if series is side-by-side
				if(this.Area3DStyle.IsClustered && drawSideBySide)
				{
					// Count number of side-by-side series
					seriesNumber = 0;
					foreach(string seriesName in this._series)
					{
						// Get series object from name
						Series	curSeries = this.Common.DataManager.Series[seriesName];
						if(String.Compare(curSeries.ChartTypeName, smallestIntervalSeries.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0 )
						{
							++seriesNumber;
						}
					}
				}
			}



			//***********************************************************
			//** Stacked column and bar charts can be drawn side-by-side
			//** using the StackGroupName custom properties. The code 
			//** checks if multiple groups are used how many of these
			//** groups exsist.
			//**
			//** If isClustered mode enabled each stack group is drawn 
			//** using it's own cluster.
			//***********************************************************
			if(smallestIntervalSeries != null && this.Area3DStyle.IsClustered)
			{
				// Check series support stack groups
				if(Common.ChartTypeRegistry.GetChartType(smallestIntervalSeries.ChartTypeName).SupportStackedGroups)
				{
					// Calculate how many stack groups exsist
					seriesNumber = 0;
					ArrayList stackGroupNames = new ArrayList();
					foreach(string seriesName in this._series)
					{
						// Get series object from name
						Series	curSeries = this.Common.DataManager.Series[seriesName];
						if(String.Compare(curSeries.ChartTypeName, smallestIntervalSeries.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0 )
						{
							string seriesStackGroupName = string.Empty;
							if(curSeries.IsCustomPropertySet(CustomPropertyName.StackedGroupName))
							{
								seriesStackGroupName = curSeries[CustomPropertyName.StackedGroupName];
							}

							// Add group name if it do not already exsist
							if(!stackGroupNames.Contains(seriesStackGroupName))
							{
								stackGroupNames.Add(seriesStackGroupName);
							}
						}
					}
					seriesNumber = stackGroupNames.Count;
				}
			}



			//***********************************************************
			//** Check if series provide custom value for point\gap depth
			//***********************************************************
            _pointsDepth = clusteredInterval * pointWidthSize * this.Area3DStyle.PointDepth / 100.0;
            _pointsDepth = categoricalAxis.GetPixelInterval(_pointsDepth);
			if(smallestIntervalSeries != null)
			{
                _pointsDepth = smallestIntervalSeries.GetPointWidth(
					this.Common.graph, 
					categoricalAxis, 
					clusteredInterval, 
					0.8) / seriesNumber;
                _pointsDepth *= this.Area3DStyle.PointDepth / 100.0;
			}
            _pointsGapDepth = (_pointsDepth * 0.8) * this.Area3DStyle.PointGapDepth / 100.0;

			// Get point depth and gap from series
			if(smallestIntervalSeries != null)
			{
                smallestIntervalSeries.GetPointDepthAndGap(
					this.Common.graph, 
					categoricalAxis,
                    ref _pointsDepth,
                    ref _pointsGapDepth);
			}


			//***********************************************************
			//** Calculate scene depth
			//***********************************************************
            return (float)((_pointsGapDepth + _pointsDepth) * GetNumberOfClusters());
		}

		/// <summary>
		/// Calculates the depth and Z position for specified series.
		/// </summary>
		/// <param name="series">Series object.</param>
		/// <param name="depth">Returns series depth.</param>
		/// <param name="positionZ">Returns series Z position.</param>
		internal void GetSeriesZPositionAndDepth(Series series, out float depth, out float positionZ)
		{
            // Check arguments
            if (series == null)
                throw new ArgumentNullException("series");

			// Get series cluster index
			int seriesIndex = GetSeriesClusterIndex(series);

			// Initialize the output parameters
            depth = (float)_pointsDepth;
            positionZ = (float)(_pointsGapDepth / 2.0 + (_pointsDepth + _pointsGapDepth) * seriesIndex);
		}



		/// <summary>
		/// Returns number of clusters on the Z axis.
		/// </summary>
		/// <returns>Number of clusters on the Z axis.</returns>
		internal int GetNumberOfClusters()
		{
			if(this.seriesClusters == null)
			{
				// Lists that hold processed chart types and stacked groups
				ArrayList	processedChartTypes = new ArrayList();
				ArrayList	processedStackedGroups = new ArrayList();

				// Reset series cluster list
				this.seriesClusters = new List<List<string>>();
			
				// Iterate through all series that belong to this chart area
				int clusterIndex = -1;
				foreach(string seriesName in this._series)
				{
					// Get series object by name
					Series	curSeries = this.Common.DataManager.Series[seriesName];

					// Check if stacked chart type is using multiple groups that 
					// can be displayed in individual clusters
					if(!this.Area3DStyle.IsClustered &&
						Common.ChartTypeRegistry.GetChartType(curSeries.ChartTypeName).SupportStackedGroups)
					{
						// Get group name
						string stackGroupName = StackedColumnChart.GetSeriesStackGroupName(curSeries);

						// Check if group was already counted
						if(processedStackedGroups.Contains(stackGroupName))
						{
							// Find in which cluster this stacked group is located
							bool found = false;
							for(int index = 0; !found && index < this.seriesClusters.Count; index++)
							{
								foreach(string name in this.seriesClusters[index])
								{
									// Get series object by name
									Series	ser = this.Common.DataManager.Series[name];
									if(stackGroupName == StackedColumnChart.GetSeriesStackGroupName(ser))
									{
										clusterIndex = index;
										found = true;
									}
								}
							}
						}
						else
						{
							// Increase cluster index
							clusterIndex = this.seriesClusters.Count;

							// Add processed group name
							processedStackedGroups.Add(stackGroupName);
						}
					}


						// Chech if series is displayed in the same cluster than other series
					else if( Common.ChartTypeRegistry.GetChartType(curSeries.ChartTypeName).Stacked ||
						(this.Area3DStyle.IsClustered && Common.ChartTypeRegistry.GetChartType(curSeries.ChartTypeName).SideBySideSeries) )
					{
						// Check if this chart type is already in the list
						if(processedChartTypes.Contains(curSeries.ChartTypeName.ToUpper(System.Globalization.CultureInfo.InvariantCulture)))
						{
							// Find in which cluster this chart type is located
							bool found = false;
							for(int index = 0; !found && index < this.seriesClusters.Count; index++)
							{
								foreach(string name in this.seriesClusters[index])
								{
									// Get series object by name
									Series	ser = this.Common.DataManager.Series[name];
									if(ser.ChartTypeName.ToUpper(System.Globalization.CultureInfo.InvariantCulture) == 
										curSeries.ChartTypeName.ToUpper(System.Globalization.CultureInfo.InvariantCulture))
									{
										clusterIndex = index;
										found = true;
									}
								}
							}
						}
						else
						{
							// Increase cluster index
							clusterIndex = this.seriesClusters.Count;

							// Add new chart type into the collection
							processedChartTypes.Add(curSeries.ChartTypeName.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
						}
					}
					else
					{
						// Create New cluster
						clusterIndex = this.seriesClusters.Count;
					}

					// Create an item in the cluster list that will hold all series names
					if(this.seriesClusters.Count <= clusterIndex)
					{
						this.seriesClusters.Add(new List<string>());
					}

					// Add series name into the current cluster
					this.seriesClusters[clusterIndex].Add(seriesName);
				}
			}
				
			return this.seriesClusters.Count;
		}

		/// <summary>
		/// Get series cluster index.
		/// </summary>
		/// <param name="series">Series object.</param>
		/// <returns>Series cluster index.</returns>
		internal int GetSeriesClusterIndex(Series series)
		{
			// Fill list of clusters
			if(this.seriesClusters == null)
			{
				this.GetNumberOfClusters();
			}

			// Iterate through all clusters
			for(int clusterIndex = 0; clusterIndex < this.seriesClusters.Count; clusterIndex++)
			{
				List<string> seriesNames = this.seriesClusters[clusterIndex];

				// Iterate through all series names
				foreach(string seriesName in seriesNames)
				{
					if(seriesName == series.Name)
					{
						// Check if series are drawn in reversed order
						if(this._reverseSeriesOrder)
						{
							clusterIndex = (this.seriesClusters.Count - 1) - clusterIndex;
						}
						return clusterIndex;
					}
				}
			}
			return 0;
		}



		#endregion

		#region 3D Scene helper methods

		/// <summary>
		/// This method is used to calculate estimated scene 
		/// depth. Regular scene depth method can not be used 
		/// because Plot area position is zero. Instead, Chart 
		/// area position is used to find depth of the scene. 
		/// Algorithm which draws axis labels will decide what 
		/// should be size and position of plotting area.
		/// </summary>
		/// <returns>Returns estimated scene depth</returns>
		private float GetEstimatedSceneDepth()
		{
			float sceneDepth;

			ChartArea area = (ChartArea) this;


			// Reset current list of clusters
			this.seriesClusters = null;


			ElementPosition plottingAreaRect = area.InnerPlotPosition;

			area.AxisX.PlotAreaPosition = area.Position;
			area.AxisY.PlotAreaPosition = area.Position;
			area.AxisX2.PlotAreaPosition = area.Position;
			area.AxisY2.PlotAreaPosition = area.Position;

			sceneDepth = GetArea3DSceneDepth();

			area.AxisX.PlotAreaPosition = plottingAreaRect;
			area.AxisY.PlotAreaPosition = plottingAreaRect;
			area.AxisX2.PlotAreaPosition = plottingAreaRect;
			area.AxisY2.PlotAreaPosition = plottingAreaRect;

			return sceneDepth;
		}

		/// <summary>
		/// Estimate Interval for 3D Charts. When scene is rotated the 
		/// number of labels should be changed.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		internal void Estimate3DInterval(ChartGraphics graph )
		{
			// Reference to the chart area class
            ChartArea area = (ChartArea)this;

			// Calculate relative size of the wall
			areaSceneWallWidth = graph.GetRelativeSize( new SizeF(this.Area3DStyle.WallWidth, this.Area3DStyle.WallWidth));
			
			//***********************************************************
			//** Calculate the depth of the chart area scene
			//***********************************************************
			areaSceneDepth = GetEstimatedSceneDepth();

			RectangleF plottingRect = area.Position.ToRectangleF();

			// Check if plot area position was recalculated.
			// If not and non-auto InnerPlotPosition & Position were
			// specified - do all needed calculations
			if(PlotAreaPosition.Width == 0 && 
				PlotAreaPosition.Height == 0 &&
				!area.InnerPlotPosition.Auto 
				&& !area.Position.Auto)
			{
				// Initialize plotting area position
				
				if(!area.InnerPlotPosition.Auto)
				{
					plottingRect.X += (area.Position.Width / 100F) * area.InnerPlotPosition.X;
					plottingRect.Y += (area.Position.Height / 100F) * area.InnerPlotPosition.Y;
					plottingRect.Width = (area.Position.Width / 100F) * area.InnerPlotPosition.Width;
					plottingRect.Height = (area.Position.Height / 100F) * area.InnerPlotPosition.Height;
				}

			}

			int yAngle = GetRealYAngle( );
			
			//***********************************************************
			//** Initialize coordinate transformation matrix
			//***********************************************************
			Matrix3D intervalMatrix3D = new Matrix3D();
			intervalMatrix3D.Initialize(
				plottingRect, 
				areaSceneDepth, 
				this.Area3DStyle.Inclination,
				yAngle,
				this.Area3DStyle.Perspective,
				this.Area3DStyle.IsRightAngleAxes);
			bool notUsed;
			float zPosition;
			double size;
			
			Point3D [] points = new Point3D[8];
			
			if( area.switchValueAxes )
			{
				
				// X Axis
				zPosition = axisX.GetMarksZPosition( out notUsed );
			
				points[0] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[1] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );

				// Y Axis
				zPosition = axisY.GetMarksZPosition( out notUsed );

				points[2] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );
				points[3] = new Point3D( plottingRect.Right, plottingRect.Bottom, zPosition );

				// X2 Axis
				zPosition = axisX2.GetMarksZPosition( out notUsed );
			
				points[4] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[5] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );

				// Y2 Axis
				zPosition = axisY2.GetMarksZPosition( out notUsed );

				points[6] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[7] = new Point3D( plottingRect.Right, plottingRect.Y, zPosition );
			}
			else
			{
				// X Axis
				zPosition = axisX.GetMarksZPosition( out notUsed );
			
				points[0] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );
				points[1] = new Point3D( plottingRect.Right, plottingRect.Bottom, zPosition );

				// Y Axis
				zPosition = axisY.GetMarksZPosition( out notUsed );

				points[2] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[3] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );

				// X2 Axis
				zPosition = axisX2.GetMarksZPosition( out notUsed );
			
				points[4] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[5] = new Point3D( plottingRect.Right, plottingRect.Y, zPosition );

				// Y2 Axis
				zPosition = axisY2.GetMarksZPosition( out notUsed );

				points[6] = new Point3D( plottingRect.X, plottingRect.Y, zPosition );
				points[7] = new Point3D( plottingRect.X, plottingRect.Bottom, zPosition );
			}

			// Crossing has to be reset because interval and 
			// sometimes minimum and maximum are changed.
			foreach( Axis axis in area.Axes )
			{
				axis.crossing = axis.tempCrossing;
			}

			// Transform all points
			intervalMatrix3D.TransformPoints( points );

			int axisIndx = 0;
			foreach( Axis axis in area.Axes )
			{
				// Find size of projected axis
				size = Math.Sqrt( 
					( points[axisIndx].X - points[axisIndx+1].X ) * ( points[axisIndx].X - points[axisIndx+1].X ) + 
					( points[axisIndx].Y - points[axisIndx+1].Y ) * ( points[axisIndx].Y - points[axisIndx+1].Y ) );

				// At the beginning plotting area chart is not calculated because 
				// algorithm for labels calculates plotting area position. To 
				// calculate labels position we need interval and interval 
				// need this correction. Because of that Chart area is used 
				// instead of plotting area position. If secondary label is 
				// enabled error for using chart area position instead of 
				// plotting area position is much bigger. This value 
				// corrects this error.
				float plottingChartAreaCorrection = 1;
				if( !area.switchValueAxes )
				{
					plottingChartAreaCorrection = 0.5F;
				}

				// Set correction for axis size
				if( axis.AxisName == AxisName.X || axis.AxisName == AxisName.X2 )
				{
					if( area.switchValueAxes )
						axis.interval3DCorrection =  size / plottingRect.Height;
					else
						axis.interval3DCorrection =  size / plottingRect.Width;
				}
				else
				{
					if( area.switchValueAxes )
						axis.interval3DCorrection =  size / plottingRect.Width;
					else
						axis.interval3DCorrection =  size / plottingRect.Height * plottingChartAreaCorrection;
				}

				// There is a limit for correction
				if( axis.interval3DCorrection < 0.15 )
					axis.interval3DCorrection = 0.15;

				// There is a limit for correction
				if( axis.interval3DCorrection > 0.8 )
					axis.interval3DCorrection = 1.0;

				axisIndx += 2;

			}
		}


		/// <summary>
		/// Calculates real Y angle from Y angle and reverseSeriesOrder field
		/// </summary>
		/// <returns>Real Y angle</returns>
		internal int GetRealYAngle( )
		{
			int yAngle;

			// Case from -90 to 90
			yAngle = this.Area3DStyle.Rotation;

			// Case from 90 to 180
			if( this._reverseSeriesOrder && this.Area3DStyle.Rotation >= 0 )
				yAngle = this.Area3DStyle.Rotation - 180;

			// Case from -90 to -180
			if( this._reverseSeriesOrder && this.Area3DStyle.Rotation <= 0 )
				yAngle = this.Area3DStyle.Rotation + 180;

			return yAngle;
		}

        /// <summary>
        /// Check if surface element should be drawn on the Back or Front layer.
        /// </summary>
        /// <param name="surfaceName">Surface name.</param>
        /// <param name="backLayer">Back/front layer.</param>
        /// <param name="onEdge">Indicates that element is on the edge (drawn on the back layer).</param>
        /// <returns>True if element should be drawn.</returns>
		internal bool ShouldDrawOnSurface(SurfaceNames surfaceName, bool backLayer, bool onEdge)
		{
			// Check if surface element should be drawn on the Back or Front layer.
            bool isVisible = ((this._visibleSurfaces & surfaceName) == surfaceName);

			// Elements on the edge are drawn on the back layer
			if(onEdge)
			{
				return backLayer;
			}

			return (backLayer == (!isVisible) );
		}

		/// <summary>
		/// Indicates that data points in all series of this 
		/// chart area should be drawn in reversed order.
		/// </summary>
		/// <returns>True if series points should be drawn in reversed order.</returns>
		internal bool DrawPointsInReverseOrder()
		{
			return (this.Area3DStyle.Rotation <= 0);
		}

        /// <summary>
        /// Checks if points should be drawn from sides to center.
        /// </summary>
        /// <param name="coord">Which coordinate of COP (X, Y or Z) to test for surface overlapping</param>
        /// <returns>True if points should be drawn from sides to center.</returns>
		internal bool DrawPointsToCenter(ref COPCoordinates coord)
		{
			bool			result = false;
			COPCoordinates	resultCoordinates = 0;

			// Check only if perspective is set
			if(this.Area3DStyle.Perspective != 0)
			{
				if( (coord & COPCoordinates.X) == COPCoordinates.X )
				{
					// Only when Left & Right sides of plotting area are invisible
                    if ((this._visibleSurfaces & SurfaceNames.Left) == 0 &&
                        (this._visibleSurfaces & SurfaceNames.Right) == 0)
					{
						result = true;
					}
					resultCoordinates = resultCoordinates | COPCoordinates.X;
				}
				if( (coord & COPCoordinates.Y) == COPCoordinates.Y )
				{
					// Only when Top & Bottom sides of plotting area are invisible
                    if ((this._visibleSurfaces & SurfaceNames.Top) == 0 &&
                        (this._visibleSurfaces & SurfaceNames.Bottom) == 0)
					{
						result = true;
					}
					resultCoordinates = resultCoordinates | COPCoordinates.Y;
				}
				if( (coord & COPCoordinates.Z) == COPCoordinates.Z )
				{
					// Only when Front & Back sides of plotting area are invisible
                    if ((this._visibleSurfaces & SurfaceNames.Front) == 0 &&
                        (this._visibleSurfaces & SurfaceNames.Back) == 0)
					{
						result = true;
					}
					resultCoordinates = resultCoordinates | COPCoordinates.Z;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks if series should be drawn from sides to center.
		/// </summary>
		/// <returns>True if series should be drawn from sides to center.</returns>
		internal bool DrawSeriesToCenter()
		{
			// Check only if perspective is set
			if(this.Area3DStyle.Perspective != 0)
			{
				// Only when Left & Right sides of plotting area are invisible
                if ((this._visibleSurfaces & SurfaceNames.Front) == 0 &&
                    (this._visibleSurfaces & SurfaceNames.Back) == 0)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region 3D Series drawing and selection methods

		/// <summary>
		/// Draws 3D series in the chart area.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		internal void PaintChartSeries3D( ChartGraphics graph )
		{
			// Reference to the chart area object
			ChartArea	area = (ChartArea)this;

			// Get order of series drawing
			List<Series>	seriesDrawingOrder = GetSeriesDrawingOrder(_reverseSeriesOrder);

			// Loop through all series in the order of drawing
			IChartType type;
			foreach( object seriesObj in seriesDrawingOrder)
			{
				Series series = (Series)seriesObj;
				type = Common.ChartTypeRegistry.GetChartType(series.ChartTypeName);
				type.Paint( graph, Common, area, series );
			}
		}

		#endregion

		#region 3D Series & Points drawing order methods
                
		/// <summary>
		/// Gets a list of series names that belong to the same 3D cluster.
		/// </summary>
		/// <param name="seriesName">One of the series names that belongs to the cluster.</param>
		/// <returns>List of all series names that belong to the same cluster as specified series.</returns>
		internal List<string> GetClusterSeriesNames(string seriesName)
		{
			// Iterate through all clusters
			foreach(List<string> seriesNames in this.seriesClusters)
			{
				if(seriesNames.Contains(seriesName))
				{
					return seriesNames;
				}
			}
			return new List<string>();
		}

        /// <summary>
        /// Gets the series list in drawing order.
        /// </summary>
        /// <param name="reverseSeriesOrder">Series order should be reversed because of the Y axis angle.</param>
        /// <returns>Gets the series list in drawing order.</returns>
		private List<Series> GetSeriesDrawingOrder(bool reverseSeriesOrder)
		{
			// Create list of series
            List<Series> seriesList = new List<Series>();

			// Iterate through all clusters
			foreach(List<string> seriesNames in this.seriesClusters)
			{
				// Make sure there is at least one series
				if(seriesNames.Count > 0)
				{
					// Get first series object in the current cluster
					Series series = Common.DataManager.Series[seriesNames[0]];

					// Add series into the drawing list
					seriesList.Add(series);
				}
			}

			// Reversed series list
			if(reverseSeriesOrder)
			{
				seriesList.Reverse();
			}

			// Check if series should be drawn from sides into the center
			if(DrawSeriesToCenter() &&
				this.matrix3D.IsInitialized())
			{
				// Get Z coordinate of Center Of Projection
				Point3D		areaProjectionCenter = new Point3D(float.NaN, float.NaN, float.NaN);
				areaProjectionCenter = this.GetCenterOfProjection(COPCoordinates.Z);
				if(!float.IsNaN(areaProjectionCenter.Z))
				{
					// Loop through all series
					for(int seriesIndex = 0; seriesIndex < seriesList.Count; seriesIndex++)
					{
						// Check if series is not empty
						if(((Series)seriesList[seriesIndex]).Points.Count == 0)
						{
							continue;
						}

						// Get series Z position
						float seriesDepth, seriesZPosition;
						this.GetSeriesZPositionAndDepth((Series)seriesList[seriesIndex], out seriesDepth, out seriesZPosition);

						// Check if series passes the Z coordinate of Center of Projection
						if(seriesZPosition >= areaProjectionCenter.Z)
						{
							// Reversed all series order staring from previous series
							--seriesIndex;
							if(seriesIndex < 0)
								seriesIndex = 0;
							seriesList.Reverse(seriesIndex, seriesList.Count - seriesIndex);
							break;
						}
					}
				}
			}

			return seriesList;
		}


		/// <summary>
		/// Gets number of stack groups in specified array of series names.
		/// </summary>
		/// <param name="seriesNamesList">Array of series names.</param>
		/// <returns>Number of stack groups. One by default.</returns>
		private int GetNumberOfStackGroups(IList<string> seriesNamesList)
		{
			this._stackGroupNames = new ArrayList();
			foreach( object seriesName in seriesNamesList )
			{
				// Get series object
				Series	ser = this.Common.DataManager.Series[(string)seriesName];

				// Get stack group name from the series
				string stackGroupName = string.Empty;
				if(ser.IsCustomPropertySet(CustomPropertyName.StackedGroupName))
				{
					stackGroupName = ser[CustomPropertyName.StackedGroupName];
				}

				// Add group name if it do not already exsist
				if(!this._stackGroupNames.Contains(stackGroupName))
				{
					this._stackGroupNames.Add(stackGroupName);
				}
			}

			return this._stackGroupNames.Count;
		}

		/// <summary>
		/// Gets index of the series stack group.
		/// </summary>
		/// <param name="series">Series to get the index for.</param>
		/// <param name="stackGroupName">Group name this series belongs to.</param>
		/// <returns>Index of series stack group.</returns>
		internal int GetSeriesStackGroupIndex(Series series, ref string stackGroupName)
		{
			stackGroupName = string.Empty;
			if(this._stackGroupNames != null)
			{
				// Get stack group name from the series
				if(series.IsCustomPropertySet(CustomPropertyName.StackedGroupName))
				{
					stackGroupName = series[CustomPropertyName.StackedGroupName];
				}
				return this._stackGroupNames.IndexOf(stackGroupName);
			}
			return 0;
		}



        /// <summary>
        /// Determine the order of points drawing from one or several series of the same type.
        /// </summary>
        /// <param name="seriesNamesList">List of series names.</param>
        /// <param name="chartType">Chart type.</param>
        /// <param name="selection">If True selection mode is active (points order should be reversed).</param>
        /// <param name="coord">Which coordinate of COP (X, Y or Z) to test for surface overlapping</param>
        /// <param name="comparer">Points comparer class. Can be Null.</param>
        /// <param name="mainYValueIndex">Index of the main Y value.</param>
        /// <param name="sideBySide">Series should be drawn side by side.</param>
        /// <returns>Array list of points in drawing order.</returns>
		internal ArrayList GetDataPointDrawingOrder(
			List<string> seriesNamesList, 
			IChartType chartType, 
			bool selection, 
			COPCoordinates coord, 
			IComparer comparer,
			int mainYValueIndex,
			bool sideBySide)
		{
			ChartArea area = (ChartArea)this;

			// Array of points in all series
			ArrayList pointsList = new ArrayList();

			//************************************************************
			//** Analyse input series
			//************************************************************

			// Find the number of data series for side-by-side drawing
			double	numOfSeries = 1;
			if(area.Area3DStyle.IsClustered && !chartType.Stacked && sideBySide)
			{
				numOfSeries = seriesNamesList.Count;
			}


			// Check stacked series group names
			if(chartType.SupportStackedGroups)
			{
				// Fill the list of group names and get the number of unique groups
				int numberOfGroups = this.GetNumberOfStackGroups(seriesNamesList);

				// If series are not isClustered set series number to the stacked group number
				if(this.Area3DStyle.IsClustered &&
					seriesNamesList.Count > 0)
				{
					numOfSeries = numberOfGroups;
				}
			}


			// Check if chart series are indexed
            bool indexedSeries = ChartHelper.IndexedSeries(this.Common, seriesNamesList.ToArray());

			//************************************************************
			//** Loop through all series and fill array of points
			//************************************************************
			int	seriesIndx = 0;
			foreach( object seriesName in seriesNamesList )
			{
				// Get series object
				Series	ser = this.Common.DataManager.Series[(string)seriesName];


				// Check if stacked groups present
				if(chartType.SupportStackedGroups && 
					this._stackGroupNames != null)
				{
					// Get index of the series using stacked group
					string groupName = string.Empty;
					seriesIndx = this.GetSeriesStackGroupIndex(ser, ref groupName);

					// Set current group name
                    StackedColumnChart stackedColumnChart = chartType as StackedColumnChart;
                    if (stackedColumnChart != null)
					{
                        stackedColumnChart.currentStackGroup = groupName;
					}
					else
					{
                        StackedBarChart stackedBarChart = chartType as StackedBarChart;
                        if (stackedBarChart != null)
                        {
                            stackedBarChart.currentStackGroup = groupName;
                        }
					}
				}


				// Set active vertical/horizontal axis and their max/min values
				Axis	vAxis = (ser.YAxisType == AxisType.Primary) ? area.AxisY : area.AxisY2;
				Axis	hAxis = (ser.XAxisType == AxisType.Primary) ? area.AxisX : area.AxisX2;

				// Get points interval:
				//  - set interval to 1 for indexed series
				//  - if points are not equaly spaced, the minimum interval between points is selected.
				//  - if points have same interval bars do not overlap each other.
				bool	sameInterval = true;
				double	interval = 1;
				if(!indexedSeries)
				{
					interval = area.GetPointsInterval( seriesNamesList, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out sameInterval );
				}

				// Get column width
				double	width = ser.GetPointWidth(area.Common.graph, hAxis, interval, 0.8) / numOfSeries;
					
				// Get series depth and Z position
				float seriesDepth, seriesZPosition;
				this.GetSeriesZPositionAndDepth(ser, out seriesDepth, out seriesZPosition);

				//************************************************************
				//** Loop through all points in series
				//************************************************************
				int	index = 0;
				foreach( DataPoint point in ser.Points )
				{
					// Increase point index
					index++;
						
					// Set x position
					double	xCenterVal;
					double	xPosition;
					if( indexedSeries )
					{
						// The formula for position is based on a distance 
						//from the grid line or nPoints position.
						xPosition = hAxis.GetPosition( (double)index ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						xCenterVal = hAxis.GetPosition( (double)index );

					}
					else if( sameInterval )
					{
						xPosition = hAxis.GetPosition( point.XValue ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						xCenterVal = hAxis.GetPosition( point.XValue );
					}
					else
					{
						xPosition = hAxis.GetPosition( point.XValue );
						xCenterVal = hAxis.GetPosition( point.XValue );
					}

	
					//************************************************************
					//** Create and add new DataPoint3D object
					//************************************************************
					DataPoint3D pointEx = new DataPoint3D();
					pointEx.indexedSeries = indexedSeries;
					pointEx.dataPoint = point;
					pointEx.index = index;
					pointEx.xPosition = xPosition;
					pointEx.xCenterVal = xCenterVal;
					pointEx.width = ser.GetPointWidth(area.Common.graph, hAxis, interval, 0.8) / numOfSeries;
					pointEx.depth = seriesDepth;
					pointEx.zPosition = seriesZPosition;

					// Set Y value and height
                    double yValue = chartType.GetYValue(Common, area, ser, point, index - 1, mainYValueIndex);
                    if (point.IsEmpty && Double.IsNaN(yValue))
                    {
                        yValue = 0.0;
                    }
					pointEx.yPosition = vAxis.GetPosition(yValue);
					pointEx.height = vAxis.GetPosition(yValue - chartType.GetYValue(Common, area, ser, point, index - 1, -1));


					pointsList.Add(pointEx);
				}

				// Data series index
				if(numOfSeries > 1 && sideBySide)
				{
					seriesIndx++;
				}
			}
		
			//************************************************************
			//** Sort points in drawing order
			//************************************************************
			if(comparer == null)
			{
				comparer = new PointsDrawingOrderComparer((ChartArea)this, selection, coord);
			}
			pointsList.Sort(comparer);

			return pointsList;
		}
		
		#endregion	

		#region Points drawing order comparer class

		/// <summary>
		/// Used to compare points in array and sort them by drawing order.
		/// </summary>
		internal class PointsDrawingOrderComparer : IComparer
		{
			/// <summary>
			/// Chart area object reference.
			/// </summary>
			private	ChartArea	_area = null;

			/// <summary>
			/// Area X position where visible sides are switched.
			/// </summary>
			private	Point3D		_areaProjectionCenter = new Point3D(float.NaN, float.NaN, float.NaN);

			/// <summary>
			/// Selection mode. Points order should be reversed.
			/// </summary>
			private bool		_selection = false;

            /// <summary>
            /// Public constructor.
            /// </summary>
            /// <param name="area">Chart area.</param>
            /// <param name="selection">Selection indicator.</param>
            /// <param name="coord">Which coordinate of COP (X, Y or Z) to test for surface overlapping</param>
			public PointsDrawingOrderComparer(ChartArea	area, bool selection, COPCoordinates coord)
			{
				this._area = area;
				this._selection = selection;

				// Get center of projection
				if(area.DrawPointsToCenter(ref coord))
				{
					_areaProjectionCenter = area.GetCenterOfProjection(coord);
				}
			}

            /// <summary>
            /// Comparer method.
            /// </summary>
            /// <param name="o1">First object.</param>
            /// <param name="o2">Second object.</param>
            /// <returns>Comparison result.</returns>
			public int Compare(object o1, object o2)
			{
				DataPoint3D point1 = (DataPoint3D) o1;
				DataPoint3D point2 = (DataPoint3D) o2;

				int	result = 0;
				if(point1.xPosition < point2.xPosition)
				{
					result = -1;
				}
				else if(point1.xPosition > point2.xPosition)
				{
					result = 1;
				}
				else
				{
					
					// If X coordinate is the same - filter by Y coordinate
					if(point1.yPosition < point2.yPosition)
					{
						result = 1;
					}
					else if(point1.yPosition > point2.yPosition)
					{
						result = -1;
					}

					// Order points from sides to center
					if(!float.IsNaN(_areaProjectionCenter.Y))
					{
						double yMin1 = Math.Min(point1.yPosition, point1.height);
						double yMax1 = Math.Max(point1.yPosition, point1.height);
						double yMin2 = Math.Min(point2.yPosition, point2.height);
						double yMax2 = Math.Max(point2.yPosition, point2.height);

						if(_area.IsBottomSceneWallVisible())
						{
							if( yMin1 <= _areaProjectionCenter.Y && yMin2 <= _areaProjectionCenter.Y )
							{
								result *= -1;
							}
							else if( yMin1 <= _areaProjectionCenter.Y)
							{
								result = 1;
							}

						}
						else
						{
							
							if( yMax1 >= _areaProjectionCenter.Y && yMax2 >= _areaProjectionCenter.Y )
							{
								result *= 1;
							}
							else if( yMax1 >= _areaProjectionCenter.Y)
							{
								result = 1;
							}
							else
							{
								result *= -1;
							}
						}
					}

						// Reversed order if looking from the bottom
					else if(!_area.IsBottomSceneWallVisible())
					{
						result *= -1;
					}
			
				}

				if(point1.xPosition != point2.xPosition)
				{
					// Order points from sides to center
					if (!float.IsNaN(_areaProjectionCenter.X))
					{
                        if ((point1.xPosition + point1.width / 2f) >= _areaProjectionCenter.X &&
                            (point2.xPosition + point2.width / 2f) >= _areaProjectionCenter.X)
						{
							result *= -1;
						}
					}

					// Reversed order of points by X value
                    else if (_area.DrawPointsInReverseOrder())
					{
						result *= -1;
					}
				}

                return (_selection) ? -result : result;
			}
		}

#endregion

		#region Center of Projection calculation methods

		/// <summary>
		/// Returns one or many (X, Y, Z) coordinates of the center of projection.
		/// </summary>
		/// <param name="coord">Defines coordinates to return.</param>
		/// <returns>Center of projection. Value can be set to float.NaN if not defined or outside plotting area.</returns>
		internal Point3D GetCenterOfProjection(COPCoordinates coord)
		{
			// Define 2 points in the opposite corners of the plotting area
			Point3D[]	points = new Point3D[2];
			points[0] = new Point3D(this.PlotAreaPosition.X, this.PlotAreaPosition.Bottom, 0f);
			points[1] = new Point3D(this.PlotAreaPosition.Right, this.PlotAreaPosition.Y, this.areaSceneDepth);

			// Check if surfaces (points 1 & 2) has same orientation
			bool	xSameOrientation, ySameOrientation, zSameOrientation;
			CheckSurfaceOrientation(
				coord,
				points[0],
				points[1],
				out xSameOrientation, 
				out ySameOrientation, 
				out zSameOrientation);

			// If orientation of all surfaces is the same - no futher processing is required (COP is outside of plotting area)
			Point3D		resultPoint = new Point3D(
				(xSameOrientation) ? float.NaN : 0f,
				(ySameOrientation) ? float.NaN : 0f,
				(zSameOrientation) ? float.NaN : 0f);
			if( ( ((coord & COPCoordinates.X) != COPCoordinates.X) || xSameOrientation ) &&
				( ((coord & COPCoordinates.Y) != COPCoordinates.Y) || ySameOrientation ) &&
				( ((coord & COPCoordinates.Z) != COPCoordinates.Z) || zSameOrientation ) )
			{
				return resultPoint;
			}

			// Calculate the smallest interval (0.5 pixels) in relative coordinates
			SizeF	interval = new SizeF(0.5f, 0.5f);
#if WINFORMS_CONTROL
			interval.Width = interval.Width * 100F / ((float)(this.Common.Chart.Width - 1)); 
			interval.Height = interval.Height * 100F / ((float)(this.Common.Chart.Height - 1)); 
#else
			interval.Width = interval.Width * 100F / ((float)(this.Common.Chart.Width.Value - 1)); 
			interval.Height = interval.Height * 100F / ((float)(this.Common.Chart.Height.Value - 1)); 
#endif	//#if WINFORMS_CONTROL

			// Find middle point and check it's surface orientation
			bool	doneFlag = false;
			while(!doneFlag)
			{
				// Find middle point
				Point3D	middlePoint = new Point3D( 
					(points[0].X + points[1].X) / 2f,
					(points[0].Y + points[1].Y) / 2f,
					(points[0].Z + points[1].Z) / 2f);

				// Check if surfaces (points 1 & middle) has same orientation
				CheckSurfaceOrientation(
					coord,
					points[0],
					middlePoint,
					out xSameOrientation, 
					out ySameOrientation, 
					out zSameOrientation);

				// Calculate points 1 & 2 depending on surface orientation of the middle point
				points[(xSameOrientation) ? 0 : 1].X = middlePoint.X;
				points[(ySameOrientation) ? 0 : 1].Y = middlePoint.Y;
				points[(zSameOrientation) ? 0 : 1].Z = middlePoint.Z;

				// Check if no more calculations required
				doneFlag = true;
				if( (coord & COPCoordinates.X) == COPCoordinates.X && 
					Math.Abs(points[1].X - points[0].X) >= interval.Width)
				{
					doneFlag = false;
				}
				if( (coord & COPCoordinates.Y) == COPCoordinates.Y && 
					Math.Abs(points[1].Y - points[0].Y) >= interval.Height)
				{
					doneFlag = false;
				}
				if( (coord & COPCoordinates.Z) == COPCoordinates.Z && 
					Math.Abs(points[1].Z - points[0].Z) >= interval.Width)
				{
					doneFlag = false;
				}
			}

			// Calculate result point
			if(!float.IsNaN(resultPoint.X))
				resultPoint.X = (points[0].X + points[1].X) / 2f;
			if(!float.IsNaN(resultPoint.Y))
				resultPoint.Y = (points[0].Y + points[1].Y) / 2f;
			if(!float.IsNaN(resultPoint.Z))
				resultPoint.Z = (points[0].Z + points[1].Z) / 2f;
			return resultPoint;
		}

		/// <summary>
		/// Checks orientations of two normal surfaces for each coordinate X, Y and Z.
		/// </summary>
		/// <param name="coord">Defines coordinates to return.</param>
		/// <param name="point1">First point.</param>
		/// <param name="point2">Second point.</param>
		/// <param name="xSameOrientation">X surfaces orientation is the same.</param>
		/// <param name="ySameOrientation">Y surfaces orientation is the same.</param>
		/// <param name="zSameOrientation">Z surfaces orientation is the same.</param>
		private void CheckSurfaceOrientation(
			COPCoordinates coord,
			Point3D point1, 
			Point3D point2, 
			out bool xSameOrientation, 
			out bool ySameOrientation, 
			out bool zSameOrientation)
		{
			Point3D[]	pointsSurface = new Point3D[3];
			bool		surf1, surf2;

			// Initialize returned values
			xSameOrientation = true;
			ySameOrientation = true;
			zSameOrientation = true;

			// Check X axis coordinates (ledt & right surfaces)
			if( (coord & COPCoordinates.X) == COPCoordinates.X )
			{
				// Define Left surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(point1.X, this.PlotAreaPosition.Y, 0f);
				pointsSurface[1] = new Point3D(point1.X, this.PlotAreaPosition.Bottom, 0f);
				pointsSurface[2] = new Point3D(point1.X, this.PlotAreaPosition.Bottom, this.areaSceneDepth);
				this.matrix3D.TransformPoints( pointsSurface );
				surf1 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Define Right surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(point2.X, this.PlotAreaPosition.Y, 0f);
				pointsSurface[1] = new Point3D(point2.X, this.PlotAreaPosition.Bottom, 0f);
				pointsSurface[2] = new Point3D(point2.X, this.PlotAreaPosition.Bottom, this.areaSceneDepth);
				this.matrix3D.TransformPoints( pointsSurface );
				surf2 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Check if surfaces have same visibility
				xSameOrientation = (surf1 == surf2);
			}

			// Check Y axis coordinates (top & bottom surfaces)
			if( (coord & COPCoordinates.Y) == COPCoordinates.Y )
			{
				// Define Bottom surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(this.PlotAreaPosition.X, point1.Y, this.areaSceneDepth);
				pointsSurface[1] = new Point3D(this.PlotAreaPosition.X, point1.Y, 0f);
				pointsSurface[2] = new Point3D(this.PlotAreaPosition.Right, point1.Y, 0f);
				this.matrix3D.TransformPoints( pointsSurface );
				surf1 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Define Top surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(this.PlotAreaPosition.X, point2.Y, this.areaSceneDepth);
				pointsSurface[1] = new Point3D(this.PlotAreaPosition.X, point2.Y, 0f);
				pointsSurface[2] = new Point3D(this.PlotAreaPosition.Right, point2.Y, 0f);
				this.matrix3D.TransformPoints( pointsSurface );
				surf2 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Check if surfaces have same visibility
				ySameOrientation = (surf1 == surf2);
			}

			// Check Y axis coordinates (front & back surfaces)
			if( (coord & COPCoordinates.Z) == COPCoordinates.Z )
			{
				// Define Front surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(this.PlotAreaPosition.X, this.PlotAreaPosition.Y, point1.Z);
				pointsSurface[1] = new Point3D(this.PlotAreaPosition.X, this.PlotAreaPosition.Bottom, point1.Z);
				pointsSurface[2] = new Point3D(this.PlotAreaPosition.Right, this.PlotAreaPosition.Bottom, point1.Z);
				this.matrix3D.TransformPoints( pointsSurface );
				surf1 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Define Back surface coordinates, transform them and check visibility
				pointsSurface[0] = new Point3D(this.PlotAreaPosition.X, this.PlotAreaPosition.Y, point2.Z);
				pointsSurface[1] = new Point3D(this.PlotAreaPosition.X, this.PlotAreaPosition.Bottom, point2.Z);
				pointsSurface[2] = new Point3D(this.PlotAreaPosition.Right, this.PlotAreaPosition.Bottom, point2.Z);
				this.matrix3D.TransformPoints( pointsSurface );
				surf2 = ChartGraphics.IsSurfaceVisible(pointsSurface[0], pointsSurface[1], pointsSurface[2]);

				// Check if surfaces have same visibility
				zSameOrientation = (surf1 == surf2);
			}
		}
#endregion
	}
}
