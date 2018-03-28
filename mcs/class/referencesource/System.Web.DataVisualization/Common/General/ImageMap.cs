//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ImageMap.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	MapArea, MapAreasCollection
//
//  Purpose:	Collection of MapArea classes is used to generate 
//              Chart image map, which provides functionality like
//              tooltip, drilldown and client-side scripting.
//
//	Reviewed:	AG - Jul 31, 2002
//              AG - Microsoft 14, 2007
//
//===================================================================


#region Used namespaces

using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Utilities;
using System.Text.RegularExpressions;
using System.IO;

#endif

#endregion

#if Microsoft_CONTROL

	namespace System.Windows.Forms.DataVisualization.Charting

#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{

#if ! Microsoft_CONTROL

	#region Map area shape enumeration

	/// <summary>
	/// An enumeration of map areas shapes.
	/// </summary>
	public enum MapAreaShape
	{
		/// <summary>
		/// The shape of the map area is rectangular.
		/// </summary>
		Rectangle,

		/// <summary>
        /// The shape of the map area is circular.
		/// </summary>
		Circle,

		/// <summary>
        /// The shape of the map area is polygonal.
		/// </summary>
		Polygon
	}


	#endregion

	#region IMapArea interface defenition

	/// <summary>
	/// Interface which defines common properties for the map area
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public interface IChartMapArea
	{
        /// <summary>
        /// Map area tooltip
        /// </summary>
        /// <value>The tooltip.</value>
		string ToolTip
		{
			set; get;
		}
        /// <summary>
        /// Map area Href
        /// </summary>
        /// <value>The map area Href.</value>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		string Url
		{
			set; get;
		}
        /// <summary>
        /// Map area other custom attributes
        /// </summary>
        /// <value>The map area attributes.</value>
		string MapAreaAttributes
		{
			set; get;
        }

        /// <summary>
        /// Map area custom data
        /// </summary>
        /// <value>The tag.</value>
        object Tag
        {
            set;
            get;
        }

        /// <summary>
        /// Map area post back value.
        /// </summary>
        /// <value>The post back value.</value>
        string PostBackValue { get; set; }
    }

	#endregion

	/// <summary>
    /// The MapArea class represents an area of the chart with end-user 
    /// interactivity like tooltip, HREF or custom attributes.
	/// </summary>
	[
	DefaultProperty("ToolTip"),
	SRDescription("DescriptionAttributeMapArea_MapArea")
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class MapArea : ChartNamedElement, IChartMapArea
    {

        #region Member variables

		private	string			_toolTip = String.Empty;
        private string          _url = String.Empty;
        private string          _attributes = String.Empty;
        private string          _postBackValue = String.Empty;
		private bool			_isCustom = true;
		private MapAreaShape	_shape = MapAreaShape.Rectangle;
        private float[]         _coordinates = new float[4];
        private static Regex    _mapAttributesRegex;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
		public MapArea() 
            : base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="url">The destination URL or anchor point of the map area.</param>
        /// <param name="path">A GraphicsPath object that defines the shape of the map area.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public MapArea(string url, GraphicsPath path)
            : this(String.Empty, url, String.Empty, String.Empty, path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="url">The destination URL or anchor point of the map area.</param>
        /// <param name="rect">A RectangleF structure that defines shape of the rectangular map area.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public MapArea(string url, RectangleF rect)
            : this(String.Empty, url, String.Empty, String.Empty, rect, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="shape">Area shape.</param>
        /// <param name="url">The destination URL or anchor point of the map area.</param>
        /// <param name="coordinates">Coordinates array that determines the location of the circle, rectangle or polygon.
        /// The type of shape that is being used determines the type of coordinates required.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public MapArea(MapAreaShape shape, string url, float[] coordinates)
            : this(shape, String.Empty, url, String.Empty, String.Empty, coordinates, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="toolTip">Tool tip.</param>
        /// <param name="url">Jump URL.</param>
        /// <param name="attributes">Other area attributes.</param>
        /// <param name="postBackValue">The postback value.</param>
        /// <param name="path">Area coordinates as graphic path</param>
        /// <param name="tag">The tag.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public MapArea(string toolTip, string url, string attributes, string postBackValue, GraphicsPath path, object tag) 
            : base()
		{
			if(path.PointCount == 0)
			{
                throw new ArgumentException(SR.ExceptionImageMapPolygonShapeInvalid);
            }

            // Flatten all curved lines
			path.Flatten();

			// Allocate array of floats
			PointF[] pathPoints = path.PathPoints;
			float[]	coord = new float[pathPoints.Length * 2];

			// Transfer path points
			int	index = 0;
			foreach(PointF point in pathPoints)
			{
				coord[index++] = point.X;
				coord[index++] = point.Y;
			}

            // Initiazize area
            Initialize(MapAreaShape.Polygon, toolTip, url, attributes, postBackValue, coord, tag);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="toolTip">Tool tip.</param>
        /// <param name="url">Jump URL.</param>
        /// <param name="attributes">Other area attributes.</param>
        /// <param name="postBackValue">The postback value.</param>
        /// <param name="rect">Rect coordinates</param>
        /// <param name="tag">The tag.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public MapArea(string toolTip, string url, string attributes, string postBackValue, RectangleF rect, object tag)
            : base()
        {
            float[] coord = new float[4];
            coord[0] = rect.X;
            coord[1] = rect.Y;
            coord[2] = rect.Right;
            coord[3] = rect.Bottom;

            Initialize(MapAreaShape.Rectangle, toolTip, url, attributes, postBackValue, coord, tag);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapArea"/> class.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="url">The URL.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="postBackValue">The postback value.</param>
        /// <param name="coordinates">The coordinates.</param>
        /// <param name="tag">The tag.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#")]
        public MapArea(MapAreaShape shape, string toolTip, string url, string attributes, string postBackValue, float[] coordinates, object tag) 
            : base()
        {
            Initialize(shape, toolTip, url, attributes, postBackValue, coordinates, tag);
        }


        private void Initialize(MapAreaShape shape, string toolTip, string url, string attributes, string postBackValue, float[] coordinates, object tag)
        {
            // Check number of coordinates depending on the area shape
            if (shape == MapAreaShape.Circle && coordinates.Length != 3)
            {
                throw (new InvalidOperationException(SR.ExceptionImageMapCircleShapeInvalid));
            }
            if (shape == MapAreaShape.Rectangle && coordinates.Length != 4)
            {
                throw (new InvalidOperationException(SR.ExceptionImageMapRectangleShapeInvalid));
            }
            if (shape == MapAreaShape.Polygon && (coordinates.Length % 2f) != 0f)
            {
                throw (new InvalidOperationException(SR.ExceptionImageMapPolygonShapeInvalid));
            }

            // Create new area object
            this._toolTip = toolTip;
            this._url = url;
            this._attributes = attributes;
            this._shape = shape;
            this._coordinates = new float[coordinates.Length];
            this._postBackValue = postBackValue;
            this.Tag = tag;
            coordinates.CopyTo(this._coordinates, 0);
        }
		#endregion

        #region Map area HTML tag generation methods

        /// <summary>
        /// Gets the name of the shape.
        /// </summary>
        /// <returns></returns>
        private string GetShapeName()
        {
            //*****************************************
            //** Set shape type
            //*****************************************
            if (_shape == MapAreaShape.Circle)
            {
                return "circle";
            }
            else if (_shape == MapAreaShape.Rectangle)
            {
                return "rect";
            }
            else if (_shape == MapAreaShape.Polygon)
            {
                return "poly";
            }
            return String.Empty;
        }


        /// <summary>
        /// Gets the coordinates.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <returns></returns>
        private string GetCoordinates(ChartGraphics graph)
        {
            // Transform coordinates from relative to pixels
            float[] transformedCoord = new float[this.Coordinates.Length];
            if (this.Shape == MapAreaShape.Circle)
            {
                PointF p = graph.GetAbsolutePoint(new PointF(this.Coordinates[0], this.Coordinates[1]));
                transformedCoord[0] = p.X;
                transformedCoord[1] = p.Y;
                p = graph.GetAbsolutePoint(new PointF(this.Coordinates[2], this.Coordinates[1]));
                transformedCoord[2] = p.X;
            }
            else if (this.Shape == MapAreaShape.Rectangle)
            {
                PointF p = graph.GetAbsolutePoint(new PointF(this.Coordinates[0], this.Coordinates[1]));
                transformedCoord[0] = p.X;
                transformedCoord[1] = p.Y;
                p = graph.GetAbsolutePoint(new PointF(this.Coordinates[2], this.Coordinates[3]));
                transformedCoord[2] = p.X;
                transformedCoord[3] = p.Y;

                // Check if rectangle has width and height
                if ((int)Math.Round(transformedCoord[0]) == (int)Math.Round(transformedCoord[2]))
                {
                    transformedCoord[2] = (float)Math.Round(transformedCoord[2]) + 1;
                }
                if ((int)Math.Round(transformedCoord[1]) == (int)Math.Round(transformedCoord[3]))
                {
                    transformedCoord[3] = (float)Math.Round(transformedCoord[3]) + 1;
                }
            }
            else
            {
                PointF pConverted = Point.Empty;
                PointF pOriginal = Point.Empty;
                for (int index = 0; index < this.Coordinates.Length - 1; index += 2)
                {
                    pOriginal.X = this.Coordinates[index];
                    pOriginal.Y = this.Coordinates[index + 1];
                    pConverted = graph.GetAbsolutePoint(pOriginal);
                    transformedCoord[index] = pConverted.X;
                    transformedCoord[index + 1] = pConverted.Y;
                }
            }
            
            StringBuilder tagStringBuilder = new StringBuilder();
            // Store transformed coordinates in the string
            bool firstElement = true;
            foreach (float f in transformedCoord)
            {
                if (!firstElement)
                {
                    tagStringBuilder.Append(",");
                }
                firstElement = false;
                tagStringBuilder.Append((int)Math.Round(f));
            }

            return tagStringBuilder.ToString();
        }

        private static bool IsJavaScript(string value)
        {
            string checkValue = value.Trim().Replace("\r", String.Empty).Replace("\n", String.Empty);
            if (checkValue.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Encodes the value.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string EncodeValue(Chart chart, string name, string value)
        {
            if (chart.IsMapAreaAttributesEncoded)
            {
                if (IsJavaScript(value) ||
                    name.Trim().StartsWith("on", StringComparison.OrdinalIgnoreCase))
                {
                    return HttpUtility.UrlEncode(value);
                }
            }
            return value;
        }

        /// <summary>
        /// Renders the tag.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="chart">The chart.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification="We use lower case to generate html attributes.")]
        internal void RenderTag(HtmlTextWriter writer, Chart chart)
        {
            StringBuilder excludedAttributes = new StringBuilder();

            writer.WriteLine();
            
            writer.AddAttribute(HtmlTextWriterAttribute.Shape, this.GetShapeName(), false);
            writer.AddAttribute(HtmlTextWriterAttribute.Coords, this.GetCoordinates(chart.chartPicture.ChartGraph));

            if (!String.IsNullOrEmpty(this.ToolTip))
            {
                excludedAttributes.Append("title,");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, EncodeValue(chart, "title", this.ToolTip));
            }
            
            bool postbackRendered = false;
            if (!String.IsNullOrEmpty(this.Url))
            {
                excludedAttributes.Append("href,");
                string resolvedUrl = chart.ResolveClientUrl(this.Url);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, EncodeValue(chart, "href", resolvedUrl));
            }
            else if (!String.IsNullOrEmpty(this.PostBackValue) && chart.Page != null)
            {
                postbackRendered = true;
                excludedAttributes.Append("href,");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, chart.Page.ClientScript.GetPostBackClientHyperlink(chart, this.PostBackValue));
            }
            
            if (!postbackRendered && !String.IsNullOrEmpty(this.PostBackValue) && chart.Page != null)
            {
                excludedAttributes.Append("onclick,");
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, chart.Page.ClientScript.GetPostBackEventReference(chart, this.PostBackValue));
            }
            
            if (!String.IsNullOrEmpty(this._attributes))
            {
                string excludedAttr = excludedAttributes.ToString();
                
                // matches name1="value1" name2="value2", don't match name1="val"ue1" or name1="value1" />
                if (_mapAttributesRegex == null)
                {
                    _mapAttributesRegex = new Regex(@"\s?(?<name>(\w+))\s?=\s?""(?<value>[^""]+)""\s?", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
                }
                
                foreach (Match match in _mapAttributesRegex.Matches(this._attributes))
                {
                    Group names = match.Groups["name"];
                    Group values = match.Groups["value"];
                    for (int i = 0; i < names.Captures.Count || i < values.Captures.Count; i++)
                    {
                        string name = names.Captures[i].Value.ToLowerInvariant();
                        string value = values.Captures[i].Value;
                        
                        // skip already rendered attributes
                        if (!excludedAttr.Contains(name + ","))  
                        {
                            // is it url?
                            if ("src,href,longdesc,background,".Contains(name + ",") && !IsJavaScript(value))
                            {
                                value = chart.ResolveClientUrl(value);
                            }
                            else
                            {
                                value = HttpUtility.HtmlAttributeEncode(value);
                            }
                            value = EncodeValue(chart, name, value);
                            writer.AddAttribute(name, value, false);
                        }
                    }
                }
            }

            if (this._attributes.IndexOf(" alt=", StringComparison.OrdinalIgnoreCase) == -1)
            {
                if (!String.IsNullOrEmpty(this.ToolTip))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, EncodeValue(chart, "title", this.ToolTip));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, "");
                }
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Area);
            writer.RenderEndTag();
        }

		#endregion

		#region	MapArea Properties

		/// <summary>
		/// Gets or sets a flag which indicates whether the map area is custom.
		/// </summary>
		[
		Browsable(false),
		SRDescription("DescriptionAttributeMapArea_Custom"),
		DefaultValue(""),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public bool IsCustom
		{
			get
			{
				return _isCustom;
			}
			internal set
			{
				_isCustom = value;
			}
		}

		/// <summary>
        /// Gets or sets the coordinates of of the map area.
		/// </summary>
		[
		SRCategory("CategoryAttributeShape"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapArea_Coordinates"),
		DefaultValue(""),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		TypeConverter(typeof(MapAreaCoordinatesConverter))
		]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public float[] Coordinates
		{
			get
			{
				return _coordinates;
			}
			set
			{
				_coordinates = value;
			}
		}

		/// <summary>
        /// Gets or sets the shape of the map area.
		/// </summary>
		[
		SRCategory("CategoryAttributeShape"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapArea_Shape"),
		DefaultValue(typeof(MapAreaShape), "Rectangle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public MapAreaShape Shape
		{
			get
			{
				return _shape;
			}
			set
			{
				_shape = value;
			}
		}
	
		/// <summary>
        /// Gets or sets the name of the map area.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		SRDescription("DescriptionAttributeMapArea_Name"),
		DefaultValue("Map Area"),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}
		#endregion

		#region	IMapAreaAttributesutes Properties implementation

		/// <summary>
        /// Gets or sets the tooltip of the map area.
		/// </summary>
		[

        SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeToolTip"),
		DefaultValue(""),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
		]
		public string ToolTip
		{
			set
			{
				_toolTip = value;
			}
			get
			{
				return _toolTip;
			}
		}

		/// <summary>
        /// Gets or sets the URL of the map area.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeUrl"),
		DefaultValue(""),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)
#endif
		]
		public string Url
		{
			set
			{
				_url = value;
			}
			get
			{
				return _url;
			}
		}

		/// <summary>
        /// Gets or sets the attributes of the map area.
		/// </summary>
		[
        SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapAreaAttributes"),
		DefaultValue(""),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
		]
		public string MapAreaAttributes
		{
			set
			{
				_attributes = value;
			}
			get
			{
				return _attributes;
			}
        }

        /// <summary>
        /// Gets or sets the postback value which can be processed on click event.
        /// </summary>
        /// <value>The value which is passed to click event as argument.</value>
        [DefaultValue("")]
        [SRCategory(SR.Keys.CategoryAttributeMapArea)]
        [SRDescription(SR.Keys.DescriptionAttributePostBackValue)]
        public string PostBackValue 
        {
            get
            {
                return this._postBackValue;
            }
            set
            {
                this._postBackValue = value;
            }
        }


        #endregion

    }


	/// <summary>
    /// The MapAreasCollection class is a strongly typed collection of MapAreas.
	/// </summary>
	[
	SRDescription("DescriptionAttributeMapAreasCollection_MapAreasCollection")
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class MapAreasCollection : ChartElementCollection<MapArea>
	{

        #region Constructors

		/// <summary>
		/// Public constructor.
		/// </summary>
		public MapAreasCollection()
            : base(null)
		{
		}

        #endregion 

        #region Methods

        /// <summary>
        /// Insert new map area items into the collection.
        /// </summary>
        /// <param name="index">Index to insert at.</param>
        /// <param name="toolTip">Tool tip.</param>
        /// <param name="url">Jump URL.</param>
        /// <param name="attributes">Other area attributes.</param>
        /// <param name="postBackValue">The post back value associated with this item.</param>
        /// <param name="path">Area coordinates as graphics path.</param>
        /// <param name="absCoordinates">Absolute coordinates in the graphics path.</param>
        /// <param name="graph">Chart graphics object.</param>
		internal void InsertPath(
			int index, 
			string toolTip, 
			string url,
            string attributes,
            string postBackValue, 
			GraphicsPath path, 
			bool absCoordinates,
			ChartGraphics graph) 
		{

			// If there is more then one graphical path split them and create 
			// image maps for every graphical path separately.
			GraphicsPathIterator iterator = new GraphicsPathIterator(path);

			// There is more then one path.
			if( iterator.SubpathCount > 1 )
			{
				GraphicsPath subPath = new GraphicsPath();
				while(iterator.NextMarker(subPath) > 0)
				{
                    InsertSubpath(index, toolTip, url, attributes, postBackValue, subPath, absCoordinates, graph);
					subPath.Reset();
				}
			}
			// There is only one path
			else
			{
                InsertSubpath(index, toolTip, url, attributes, postBackValue, path, absCoordinates, graph);
			}
		}

        /// <summary>
        /// Insert new map area item into the collection.
        /// </summary>
        /// <param name="index">Index to insert at.</param>
        /// <param name="toolTip">Tool tip.</param>
        /// <param name="url">Jump URL.</param>
        /// <param name="attributes">Other area attributes.</param>
        /// <param name="postBackValue">The post back value associated with this item.</param>
        /// <param name="path">Area coordinates as graphics path.</param>
        /// <param name="absCoordinates">Absolute coordinates in the graphics path.</param>
        /// <param name="graph">Chart graphics object.</param>
		private void InsertSubpath(
			int index, 
			string toolTip, 
			string url,
            string attributes,
            string postBackValue, 
			GraphicsPath path, 
			bool absCoordinates,
			ChartGraphics graph)
		{
			if(path.PointCount > 0)
			{
				// Flatten all curved lines
				path.Flatten();

				// Allocate array of floats
				PointF[] pathPoints = path.PathPoints;
				float[]	coord = new float[pathPoints.Length * 2];

				// Convert absolute coordinates to relative
				if(absCoordinates)
				{
					for(int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
					{
						pathPoints[pointIndex] = graph.GetRelativePoint( pathPoints[pointIndex] );
					}
				}

				// Transfer path points
				int	i = 0;
				foreach(PointF point in pathPoints)
				{
					coord[i++] = point.X;
					coord[i++] = point.Y;
				}

				// Add new area
                MapArea area = new MapArea(MapAreaShape.Polygon, toolTip, url, attributes, postBackValue, coord, null);
                area.IsCustom = false;
                this.Insert(index, area);
			}
		}

        /// <summary>
		/// Removes all non custom map areas items from the collection.
		/// </summary>
		internal void RemoveNonCustom()
		{
			for(int index = 0; index < this.Count; index++)
			{
				// Check the custom flag
				if(!this[index].IsCustom)
				{
					this.RemoveAt(index);
					--index;
				}
			}
		}

        #endregion
	}

#endif
}
