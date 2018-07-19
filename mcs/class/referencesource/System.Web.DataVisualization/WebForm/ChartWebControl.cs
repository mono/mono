//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		WebCustomControl1.cs
//
//  Namespace:	System.Web.UI.DataVisualization.Charting
//
//	Classes:	Chart, TraceManager 
//				CustomizeMapAreasEventArgs
//
//  Purpose:	Chart web control main class.
//
//	Reviewed:	
//
//===================================================================

#region Used namespaces 

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using System.Collections;
using System.Diagnostics;
using System.Xml;
using System.Web.UI.DataVisualization.Charting;
using System.Globalization;

using System.Web.UI.DataVisualization.Charting.Data;
using System.Web.UI.DataVisualization.Charting.Utilities;
using System.Web.UI.DataVisualization.Charting.ChartTypes;
using System.Web.UI.DataVisualization.Charting.Borders3D;

using System.Web.UI.DataVisualization.Charting.Formulas;
using System.Security;
using System.Security.Permissions;



#endregion

namespace System.Web.UI.DataVisualization.Charting
{

	#region Chart enumerations

    /// <summary>
    /// Chart image storage mode.
    /// </summary>
    public enum ImageStorageMode
    {

        /// <summary>
        /// Images are stored using HTTP Handler.
        /// </summary>
        UseHttpHandler,

        /// <summary>
        /// Images is saved in temp. file using ImageLocation specified.
        /// </summary>
        UseImageLocation
    }

	/// <summary>
	/// Specifies the format of the image
	/// </summary>
	public enum ChartImageFormat
	{
		/// <summary>
		/// Gets the Joint Photographic Experts Group (JPEG) image format.
		/// </summary>
		Jpeg,

		/// <summary>
		/// Gets the W3C Portable Network Graphics (PNG) image format.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Png")]
        Png,

		/// <summary>
		/// Gets the bitmap image format (BMP).
		/// </summary>
		Bmp,

		/// <summary>
		/// Gets the Tag Image File Format (TIFF) image format.
		/// </summary>
		Tiff,

		/// <summary>
		/// Gets the Graphics Interchange Format (GIF) image format.
		/// </summary>
		Gif,

        /// <summary>
		/// Gets the Enhanced Meta File (Emf) image format.
		/// </summary>
		Emf,

		/// <summary>
		/// Gets the Enhanced Meta File (Emf+) image format.
		/// </summary>
		EmfPlus,

		/// <summary>
		/// Gets the Enhanced Meta File (EmfDual) image format.
		/// </summary>
		EmfDual,
	}

	/// <summary>
	/// Chart image rendering type
	/// </summary>
 	public enum RenderType
	{
        /// <summary>
		/// Chart image is rendered as image tag.
		/// </summary>
		ImageTag, 

		/// <summary>
		/// Chart image is streamed back directly.
		/// </summary>
		BinaryStreaming,

		/// <summary>
		/// Chart image map is rendered.
		/// </summary>
		ImageMap
	}

	#endregion

	/// <summary>
	/// Summary description for enterprize chart control.
	/// </summary>
	[
	ToolboxData("<{0}:Chart runat=server>" +
        "<Series><{0}:Series Name=\"Series1\"></{0}:Series></Series>" +
        "<ChartAreas><{0}:ChartArea Name=\"ChartArea1\"></{0}:ChartArea></ChartAreas>" +
		"</{0}:Chart>"),
	ToolboxBitmap(typeof(Chart), "ChartControl.ico"),
    Designer(Editors.ChartWebDesigner)
	]
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	[DisplayNameAttribute("Chart")]
    [SupportsEventValidation]
    [DefaultEvent("Load")] 
#if ASPPERM_35
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class Chart : System.Web.UI.WebControls.DataBoundControl, IPostBackEventHandler
	{

        #region Control fields

        /// <summary>
        /// True if smart labels debug markings should be drawn.
        /// This field is for SmartLabels related issues debugging only.
        /// </summary>
        internal bool ShowDebugMarkings = false;

        // Chart services components
		private ChartTypeRegistry				_chartTypeRegistry = null;
		private BorderTypeRegistry				_borderTypeRegistry = null;
        private CustomPropertyRegistry          _customPropertyRegistry = null;
		private DataManager						_dataManager = null;
		internal ChartImage						chartPicture = null;
		private ImageLoader						_imageLoader = null;
		internal static ITypeDescriptorContext	controlCurrentContext = null;
		internal string							webFormDocumentURL = "";
		internal ServiceContainer				serviceContainer = null;

		// Named images collection
		private NamedImagesCollection			_namedImages = null;


		private FormulaRegistry					_formulaRegistry = null;


		// Product ID

		internal static string					productID = "MSC-WCE-10";

		// Control license
		private	License							_license = null;

		// Private data members, which store properties values
		private	RenderType						_renderType = RenderType.ImageTag;
		private string							_chartImageLocation = "ChartPic_#SEQ(300,3)";
        
		// Indicates that chart is serializing the data
		internal bool							serializing = false;

        // Detailed serialization status which allows not only to determine if serialization
        // is curently in process but also check if we are saving, loading or resetting the chart.
        internal SerializationStatus            serializationStatus = SerializationStatus.None;

		// Chart serializer
		private ChartSerializer					_chartSerializer = null;

		// Chart content saved in the view state
		private	SerializationContents 			_viewStateContent = SerializationContents .Default;

		// Image URL the chart will be renderd to
		private string							_currentChartImageLocation = String.Empty;

        // Image Handler URL the chart will be renderd to
        private string                          _currentChartHandlerImageLocation = String.Empty;

		// Indicates if unique GUID should be added to image file name to solve cashing issues
		private bool							_addGuidParam = true;

		private KeywordsRegistry				_keywordsRegistry = null;

    	// Indicates image storage mode.
    	private ImageStorageMode _imageStorageMode = ImageStorageMode.UseHttpHandler;


        // Selection class
        internal Selection                      selection = null;

		#endregion

		#region Constructors and initialization

		/// <summary>
		/// Chart control constructor.
		/// </summary>
		public Chart() : base()
		{
			base.EnableViewState = false;

			//*********************************************************
			//** Create services
			//*********************************************************
			serviceContainer = new ServiceContainer();
			_chartTypeRegistry = new ChartTypeRegistry();
			_borderTypeRegistry = new BorderTypeRegistry();
			_customPropertyRegistry = new CustomPropertyRegistry();

			_keywordsRegistry = new KeywordsRegistry();

			_dataManager = new DataManager(serviceContainer);
			_imageLoader = new ImageLoader(serviceContainer);
			chartPicture = new ChartImage(serviceContainer);
			_chartSerializer = new ChartSerializer(serviceContainer);


			_formulaRegistry = new FormulaRegistry();

			// Add services to the service container
			serviceContainer.AddService(typeof(Chart), this);							// Chart Control
			serviceContainer.AddService(_chartTypeRegistry.GetType(), _chartTypeRegistry);// Chart types registry
			serviceContainer.AddService(_borderTypeRegistry.GetType(), _borderTypeRegistry);// Border types registry
			serviceContainer.AddService(_customPropertyRegistry.GetType(), _customPropertyRegistry);// Custom attribute registry
			serviceContainer.AddService(_dataManager.GetType(), _dataManager);			// Data Manager service
			serviceContainer.AddService(_imageLoader.GetType(), _imageLoader);			// Image Loader service
			serviceContainer.AddService(chartPicture.GetType(), chartPicture);			// Chart image service
			serviceContainer.AddService(_chartSerializer.GetType(), _chartSerializer);	// Chart serializer service


			serviceContainer.AddService(_formulaRegistry.GetType(), _formulaRegistry);			// Formula modules service



			serviceContainer.AddService(_keywordsRegistry.GetType(), _keywordsRegistry);	// Keywords registry



			// Initialize objects
			_dataManager.Initialize();

			// Register known chart types
			_chartTypeRegistry.Register(ChartTypeNames.Bar, typeof(BarChart));
			_chartTypeRegistry.Register(ChartTypeNames.Column, typeof(ColumnChart));
			_chartTypeRegistry.Register(ChartTypeNames.Point, typeof(PointChart));
			_chartTypeRegistry.Register(ChartTypeNames.Bubble, typeof(BubbleChart));
			_chartTypeRegistry.Register(ChartTypeNames.Line, typeof(LineChart));
			_chartTypeRegistry.Register(ChartTypeNames.Spline, typeof(SplineChart));
			_chartTypeRegistry.Register(ChartTypeNames.StepLine, typeof(StepLineChart));
			_chartTypeRegistry.Register(ChartTypeNames.Area, typeof(AreaChart));
			_chartTypeRegistry.Register(ChartTypeNames.SplineArea, typeof(SplineAreaChart));
			_chartTypeRegistry.Register(ChartTypeNames.StackedArea, typeof(StackedAreaChart));
			_chartTypeRegistry.Register(ChartTypeNames.Pie, typeof(PieChart));
			_chartTypeRegistry.Register(ChartTypeNames.Stock, typeof(StockChart));
			_chartTypeRegistry.Register(ChartTypeNames.Candlestick, typeof(CandleStickChart));
			_chartTypeRegistry.Register(ChartTypeNames.Doughnut, typeof(DoughnutChart));
			_chartTypeRegistry.Register(ChartTypeNames.StackedBar, typeof(StackedBarChart));
			_chartTypeRegistry.Register(ChartTypeNames.StackedColumn, typeof(StackedColumnChart));
			_chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedColumn, typeof(HundredPercentStackedColumnChart));
			_chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedBar, typeof(HundredPercentStackedBarChart));
			_chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedArea, typeof(HundredPercentStackedAreaChart));



			_chartTypeRegistry.Register(ChartTypeNames.Range, typeof(RangeChart));
			_chartTypeRegistry.Register(ChartTypeNames.SplineRange, typeof(SplineRangeChart));
			_chartTypeRegistry.Register(ChartTypeNames.RangeBar, typeof(RangeBarChart));
			_chartTypeRegistry.Register(ChartTypeNames.RangeColumn, typeof(RangeColumnChart));
			_chartTypeRegistry.Register(ChartTypeNames.ErrorBar, typeof(ErrorBarChart));
			_chartTypeRegistry.Register(ChartTypeNames.BoxPlot, typeof(BoxPlotChart));
			_chartTypeRegistry.Register(ChartTypeNames.Radar, typeof(RadarChart));



			_chartTypeRegistry.Register(ChartTypeNames.Renko, typeof(RenkoChart));
			_chartTypeRegistry.Register(ChartTypeNames.ThreeLineBreak, typeof(ThreeLineBreakChart));
			_chartTypeRegistry.Register(ChartTypeNames.Kagi, typeof(KagiChart));
			_chartTypeRegistry.Register(ChartTypeNames.PointAndFigure, typeof(PointAndFigureChart));





			_chartTypeRegistry.Register(ChartTypeNames.Polar, typeof(PolarChart));
			_chartTypeRegistry.Register(ChartTypeNames.FastLine, typeof(FastLineChart));
			_chartTypeRegistry.Register(ChartTypeNames.Funnel, typeof(FunnelChart));
			_chartTypeRegistry.Register(ChartTypeNames.Pyramid, typeof(PyramidChart));





			_chartTypeRegistry.Register(ChartTypeNames.FastPoint, typeof(FastPointChart));
            



			// Register known formula modules
            _formulaRegistry.Register(SR.FormulaNamePriceIndicators, typeof(PriceIndicators));
            _formulaRegistry.Register(SR.FormulaNameGeneralTechnicalIndicators, typeof(GeneralTechnicalIndicators));
            _formulaRegistry.Register(SR.FormulaNameTechnicalVolumeIndicators, typeof(VolumeIndicators));
            _formulaRegistry.Register(SR.FormulaNameOscillator, typeof(Oscillators));
            _formulaRegistry.Register(SR.FormulaNameGeneralFormulas, typeof(GeneralFormulas));
            _formulaRegistry.Register(SR.FormulaNameTimeSeriesAndForecasting, typeof(TimeSeriesAndForecasting));
            _formulaRegistry.Register(SR.FormulaNameStatisticalAnalysis, typeof(StatisticalAnalysis));

			// Register known 3D border types
			_borderTypeRegistry.Register("Emboss", typeof(EmbossBorder));
			_borderTypeRegistry.Register("Raised", typeof(RaisedBorder));
			_borderTypeRegistry.Register("Sunken", typeof(SunkenBorder));
			_borderTypeRegistry.Register("FrameThin1", typeof(FrameThin1Border));
			_borderTypeRegistry.Register("FrameThin2", typeof(FrameThin2Border));
			_borderTypeRegistry.Register("FrameThin3", typeof(FrameThin3Border));
			_borderTypeRegistry.Register("FrameThin4", typeof(FrameThin4Border));
			_borderTypeRegistry.Register("FrameThin5", typeof(FrameThin5Border));
			_borderTypeRegistry.Register("FrameThin6", typeof(FrameThin6Border));
			_borderTypeRegistry.Register("FrameTitle1", typeof(FrameTitle1Border));
			_borderTypeRegistry.Register("FrameTitle2", typeof(FrameTitle2Border));
			_borderTypeRegistry.Register("FrameTitle3", typeof(FrameTitle3Border));
			_borderTypeRegistry.Register("FrameTitle4", typeof(FrameTitle4Border));
			_borderTypeRegistry.Register("FrameTitle5", typeof(FrameTitle5Border));
			_borderTypeRegistry.Register("FrameTitle6", typeof(FrameTitle6Border));
			_borderTypeRegistry.Register("FrameTitle7", typeof(FrameTitle7Border));
			_borderTypeRegistry.Register("FrameTitle8", typeof(FrameTitle8Border));

            // Create selection object
            this.selection = new Selection(serviceContainer);

			// Create named images collection
            _namedImages = new NamedImagesCollection();

            // Hook up event handlers
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Legends.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Titles.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Annotations.ChartAreaNameReferenceChanged);
            Legends.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.LegendNameReferenceChanged);

            this.AlternateText = String.Empty;
            this.DescriptionUrl = String.Empty;
        }

		#endregion

		#region Chart rendering methods

        /// <summary>
		/// Gets current image URL the chart control will be rendered into.
		/// </summary>
		/// <returns>Current chart image URL.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")] 
        [
		    Bindable(false),
		    Browsable(false),
		    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		    SerializationVisibility(SerializationVisibility.Hidden),
        ]
		public string CurrentImageLocation
		{
            get
            {

                if (this.RenderType == RenderType.ImageTag && this.GetImageStorageMode() == ImageStorageMode.UseHttpHandler)
                {
                    return _currentChartHandlerImageLocation;
                }

                // Image name is already created
                if (this._currentChartImageLocation.Length > 0)
                {
                    return this._currentChartImageLocation;
                }

                // Get picture name
                this._currentChartImageLocation = this.ImageLocation;
                int indexUID = -1;
                if (this.RenderType == RenderType.ImageTag)
                {
                    // Make sure image URL is not empty
                    if (this.ImageLocation.Length == 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionImageUrlIsEmpty));
                    }
                    // Add file extension if there is no one
                    char[] slashesArray = { '\\', '/' };
                    int pointIndex = _currentChartImageLocation.LastIndexOf('.');
                    int slashIndex = _currentChartImageLocation.LastIndexOfAny(slashesArray);
                    if (pointIndex < 0 || pointIndex < slashIndex)
                    {
                            switch (chartPicture.ImageType)
                            {
                                case (ChartImageType.Bmp):
                                    _currentChartImageLocation += ".bmp";
                                    break;
                                case (ChartImageType.Jpeg):
                                    _currentChartImageLocation += ".jpeg";
                                    break;
                                case (ChartImageType.Png):
                                    _currentChartImageLocation += ".png";
                                    break;
                                case (ChartImageType.Emf):
                                    _currentChartImageLocation += ".emf";
                                    break;
                            }
                    }

                    // Double chech that #UID is not used with #SEQ
                    // Add GUID to the filename 
                    indexUID = _currentChartImageLocation.IndexOf("#UID", StringComparison.Ordinal);
                    int indexSEQ = _currentChartImageLocation.IndexOf("#SEQ", StringComparison.Ordinal);
                    if (indexUID >= 0 && indexSEQ >= 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionImageUrlInvalidFormatters));
                    }

                    // Add GUID to the filename 
                    if (indexUID >= 0)
                    {
                        // Replace "#UID" with GUID string
                        _currentChartImageLocation = _currentChartImageLocation.Replace("#UID", Guid.NewGuid().ToString());
                    }

                        // Add GUID to the filename 
                    else if (indexSEQ >= 0)
                    {
                        // Replace "#SEQ(XXX,XXX)" with the sequence string number
                        _currentChartImageLocation = GetNewSeqImageUrl(_currentChartImageLocation);
                    }

                }

                // Check if GUID parameter should be added to the SRC tag
                // Solves issue with image caching in IE
                int indexNoGuidParam = _currentChartImageLocation.IndexOf("#NOGUIDPARAM", StringComparison.Ordinal);
                if (indexNoGuidParam > 0)
                {
                    _currentChartImageLocation = _currentChartImageLocation.Replace("#NOGUIDPARAM", "");
                }

                // Check for virtual root character
                if (_currentChartImageLocation.StartsWith("~", StringComparison.Ordinal) && HttpContext.Current != null && this.Page.Request != null)
                {
                    // NOTE: Solves issue #4771
                    _currentChartImageLocation = this.Page.ResolveUrl(_currentChartImageLocation);
                }

                return _currentChartImageLocation;
            }
        }



        /// <summary>
        /// Determines if chart should render image maps
        /// </summary>
        /// <returns>True if should render image maps</returns>
        private bool HasImageMaps()
        {
            // Render chart image map
            if (this.RenderType != RenderType.BinaryStreaming && this.IsMapEnabled)
            {
                if (this.MapAreas.Count > 0 || this.RenderType == RenderType.ImageMap)
                {
                    // Render image map 
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Caches the IsImageRendersBorder result.
        /// </summary>
        private static int _isImageRendersBorder;
        /// <summary>
        /// Checks and returns true if the image renders border. Before Fx 4.0 image control renders border if is not declared. 
        /// After Fx 4.0 this is not by default.  
        /// </summary>
        /// <returns>True if image control renders border style</returns>
        private static bool IsImageRendersBorder
        {
            get
            {
                if (_isImageRendersBorder == 0)
                {
                    using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        using (HtmlTextWriter w = new HtmlTextWriter(sw))
                        {
                            System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image();
                            img.RenderControl(w);
                        }
                        _isImageRendersBorder = sw.ToString().IndexOf("border", 0, StringComparison.OrdinalIgnoreCase) != -1 ? 1 : -1;
                    }
                }
                return _isImageRendersBorder == 1;
            }
        }

        /// <summary>
        /// Custom image control for supporting miage maps.
        /// </summary>
        private class CustomImageControl : System.Web.UI.WebControls.Image
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomImageControl"/> class.
            /// </summary>
            internal CustomImageControl() : base()
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether this instance has image map.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance has image map; otherwise, <c>false</c>.
            /// </value>
            internal bool HasImageMap { get; set; }

            /// <summary>
            /// Adds the attributes of an <see cref="T:System.Web.UI.WebControls.Image"/> to the output stream for rendering on the client.
            /// </summary>
            /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that contains the output stream to render on the client browser.</param>
            protected override void AddAttributesToRender(HtmlTextWriter writer)
            {
                base.AddAttributesToRender(writer);
                if (this.HasImageMap)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Usemap, "#"+this.ClientID+"ImageMap", false);
                }
                if (!this.Enabled)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                }
            }
        }

        /// <summary>
        /// Builds the image control.
        /// </summary>
        /// <param name="chartImageSrc">The chart image SRC.</param>
        /// <param name="addGuidParameter">if set to <c>true</c> to add GUID parameter.</param>
        /// <returns>A custom image control with image maps attribute</returns>
        private CustomImageControl BuildImageControl(string chartImageSrc, bool addGuidParameter)
        {
            CustomImageControl htmlImage = new CustomImageControl();
            htmlImage.ImageUrl = chartImageSrc + (addGuidParameter ? "?" + Guid.NewGuid().ToString() : "");
            htmlImage.ToolTip = this.ToolTip;
            htmlImage.CssClass = this.CssClass;
            htmlImage.AlternateText = this.AlternateText;
            htmlImage.DescriptionUrl = this.DescriptionUrl;
            htmlImage.AccessKey = this.AccessKey;
            htmlImage.TabIndex = this.TabIndex;
            htmlImage.Enabled = this.IsEnabled;
            htmlImage.CopyBaseAttributes(this);
            if (!IsImageRendersBorder)
            {
                // set border 0px only if is not declared yet.
                if ( String.IsNullOrEmpty(htmlImage.Style[HtmlTextWriterStyle.BorderWidth]) &&
                     String.IsNullOrEmpty(htmlImage.Style["border"]) &&
                     String.IsNullOrEmpty(htmlImage.Style["border-width"]))
                {
                    htmlImage.Style.Value = "border-width:0px;" + htmlImage.Style.Value;
                }
            }

            htmlImage.ID = this.ClientID;

            htmlImage.GenerateEmptyAlternateText = true;

            htmlImage.Width = this.Width;
            htmlImage.Height = this.Height;
            htmlImage.HasImageMap = this.HasImageMaps();

            return htmlImage;
        }

        private string _designTimeChart;
		/// <summary>
		/// Render this control to the output parameter specified.
		/// </summary>
        /// <param name="writer">HTML writer.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // If by any reason (rudimentary designer host, no designer, embeded in user control, etc) 
            // Render() is called in design mode ( should be handled by control designed )
            // we render the chart in temp file. 
            if (this.DesignMode)
            {
                if (String.IsNullOrEmpty(_designTimeChart))
                {
                    _designTimeChart = Path.GetTempFileName() + ".bmp";
                }
                SaveImage(_designTimeChart, ChartImageFormat.Bmp);
                using (CustomImageControl imageControl = this.BuildImageControl("file://" + _designTimeChart, false))
                {
                    imageControl.RenderControl(writer);
                }
                return;
            }

            // Check if GUID parameter should be added to the SRC tag
			// Solves issue with image caching in IE
			_addGuidParam = true;
			int		indexNoGuidParam = this.ImageLocation.IndexOf("#NOGUIDPARAM", StringComparison.Ordinal);
			if(indexNoGuidParam > 0)
			{
				_addGuidParam = false;
			}

			// Get picture name
			string	chartImage = this.CurrentImageLocation;


            if (this.RenderType == RenderType.ImageTag)
            {
                if (this.GetImageStorageMode() == ImageStorageMode.UseHttpHandler)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        this.SaveImage(stream);
                        chartImage = ChartHttpHandler.GetChartImageUrl(stream, this.ImageType.ToString());
                        _currentChartHandlerImageLocation = chartImage;
                    }
                    _addGuidParam = false;
                }
                else
                {
                    // Save chart into specified image URL
                    SaveImage(this.Page.MapPath(chartImage));
                }

                using (CustomImageControl imageControl = this.BuildImageControl(chartImage, _addGuidParam))
                {
                    imageControl.RenderControl(writer);
                }
                
			}

			// Render chart image as image tag + image map
			else if(this.RenderType == RenderType.ImageMap)
			{

				// Get chart image (do not save it)
                chartPicture.PaintOffScreen();

                using (CustomImageControl imageControl = this.BuildImageControl(chartImage, _addGuidParam))
                {
                    imageControl.RenderControl(writer);
                }

	        }
            // Render chart using binary data streaming
            else
            {

                // Set response content type
                switch (chartPicture.ImageType)
                {
                    case (ChartImageType.Bmp):
                        this.Page.Response.ContentType = "image/bmp";
                        break;
                    case (ChartImageType.Jpeg):
                        this.Page.Response.ContentType = "image/jpeg";
                        break;
                    case (ChartImageType.Png):
                        this.Page.Response.ContentType = "image/png";
                        break;
                }

                this.Page.Response.Charset = "";

                // Save image into the memory stream
                MemoryStream stream = new MemoryStream();
                SaveImage(stream);
                this.Page.Response.BinaryWrite(stream.GetBuffer());
            }


			// Render chart image map
            if (this.HasImageMaps())
			{
					// Render image map 
					chartPicture.WriteChartMapTag(writer, this.ClientID + "ImageMap");
			}

            // Reset image Url field
            this._currentChartImageLocation = String.Empty;

		}


        /// <summary>
		/// Checks image URL sequence format.
		/// </summary>
		/// <param name="imageURL">Image URL to test.</param>
		void CheckImageURLSeqFormat(string imageURL)
		{
			// Find the begginning of the "#SEQ" formatting string
			int indexSEQ = imageURL.IndexOf("#SEQ", StringComparison.Ordinal);
			indexSEQ += 4;

			// The "#SEQ" formatter must be followed by (MMM,TTT), where MMM - max sequence number and TTT - time to live
			if(imageURL[indexSEQ] != '(')
			{
				throw( new ArgumentException(SR.ExceptionImageUrlInvalidFormat, "imageURL"));
			}
			// Find closing bracket
			int indexClosing = imageURL.IndexOf(')', 1);
			if(indexClosing < 0)
			{
                throw (new ArgumentException(SR.ExceptionImageUrlInvalidFormat, "imageURL"));
			}

			// Get max sequence number and time to live
			string[] values = imageURL.Substring(indexSEQ + 1, indexClosing - indexSEQ - 1).Split(',');
			if(values == null || values.Length != 2)
			{
                throw (new ArgumentException(SR.ExceptionImageUrlInvalidFormat, "imageURL"));
			}

			// Make sure all characters are digits
			foreach(String str in values)
			{
                if (String.IsNullOrEmpty(str) || str.Length > 7)
                {
                    throw (new ArgumentException(SR.ExceptionImageUrlInvalidFormat, "imageURL"));
                }
                foreach (Char c in str)
				{
					if(!Char.IsDigit(c))
					{
						throw( new ArgumentException( SR.ExceptionImageUrlInvalidFormat, "imageURL"));
					}
				}
			}
		}

		/// <summary>
		/// Helper function, which returns a new image URL 
		/// using the sequence numbers
		/// </summary>
		/// <param name="imageUrl">Image URL format.</param>
		/// <returns>New image URL.</returns>
		private string GetNewSeqImageUrl(string imageUrl)
		{
			// Initialize image URL max sequence number and image time to live values
			int		maxSeqNumber = 0;
			int		imageTimeToLive = 0;
			string	result = "";

			//*********************************************************
			//** Check image URL format
			//*********************************************************

			// Find the begginning of the "#SEQ" formatting string
			int indexSEQ = imageUrl.IndexOf("#SEQ", StringComparison.Ordinal);
			if(indexSEQ < 0)
			{
				throw( new ArgumentException( SR.ExceptionImageUrlMissedFormatter, "imageUrl"));
			}

			// Check format
			CheckImageURLSeqFormat(imageUrl);

			// Copy everything till the beginning of the format in the result string
			result = imageUrl.Substring(0, indexSEQ);
			indexSEQ += 4;

			// Find closing bracket
			int indexClosing = imageUrl.IndexOf(')', 1);

			// Add sequence position and everything after closing bracket into the result string
			result += "{0:D6}";
			result += imageUrl.Substring(indexClosing + 1);

			// Get max sequence number and time to live
			string[] values = imageUrl.Substring(indexSEQ + 1, indexClosing - indexSEQ - 1).Split(',');
			maxSeqNumber = Int32.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
			imageTimeToLive = Int32.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);

			//*********************************************************
			//** Generate new sequence number
			//*********************************************************
			int		imageSeqNumber = 1;

			// Make sure application scope variable "ImageSeqNumber" exist
			this.Page.Application.Lock();
			if(this.Page.Application[Chart.productID+"_ImageSeqNumber"] != null)
			{
				imageSeqNumber = (int)this.Page.Application[Chart.productID+"_ImageSeqNumber"] + 1;
				if(imageSeqNumber > maxSeqNumber)
				{
					imageSeqNumber = 1;
				}
			}
			// Save sequence number
			this.Page.Application[Chart.productID+"_ImageSeqNumber"] = imageSeqNumber;
			this.Page.Application.UnLock();

			//*********************************************************
			//** Prepare result string
			//*********************************************************

			result = String.Format(CultureInfo.InvariantCulture, result, imageSeqNumber);

			//*********************************************************
			//** Check if the image with this name exsists and it's
			//** live time is smaller than image time-to-live specified.
			//** In this case put a warning into the even log.
			//*********************************************************
			if(imageTimeToLive > 0)
			{
				CheckChartFileTime(result, imageTimeToLive);
			}

			return result;
		}

		/// <summary>
		/// Check if the image with this name exsists and it's
		/// live time is smaller than image time-to-live specified.
		/// In this case put a warning into the even log.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="imageTimeToLive">Time to live.</param>
		private void CheckChartFileTime(string fileName, int imageTimeToLive)
		{
			//*********************************************************
			//** Check if the image with this name exsists and it's
			//** live time is smaller than image time-to-live specified.
			//** In this case put a warning into the even log.
			//*********************************************************
            try
            {
                if (imageTimeToLive > 0)
                {
                    fileName = this.Page.MapPath(fileName);
                    if (File.Exists(fileName))
                    {
                        DateTime fileTime = File.GetLastWriteTime(fileName);
                        if (fileTime.AddMinutes(imageTimeToLive) > DateTime.Now)
                        {
                            const string eventSource = "ChartComponent";

                            // Create the source, if it does not already exist.
                            if (!EventLog.SourceExists(eventSource))
                            {
                                EventLog.CreateEventSource(eventSource, "Application");
                            }

                            // Create an EventLog instance and assign its source.
                            EventLog eventLog = new EventLog();
                            eventLog.Source = eventSource;

                            // Write an informational entry to the event log.    
                            TimeSpan timeSpan = DateTime.Now - fileTime;
                            eventLog.WriteEntry(SR.EvenLogMessageChartImageFileTimeToLive(timeSpan.Minutes.ToString(CultureInfo.InvariantCulture)), EventLogEntryType.Warning);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (Win32Exception)
            {
            }
		}

		#endregion

		#region Chart selection methods

        /// <summary>
        /// This method performs the hit test and returns a HitTestResult objects.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Hit test result object</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public HitTestResult HitTest(int x, int y)
        {
            return selection.HitTest(x, y);
        }

        /// <summary>
        /// This method performs the hit test and returns a HitTestResult object.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="ignoreTransparent">Indicates that transparent elements should be ignored.</param>
        /// <returns>Hit test result object</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public HitTestResult HitTest(int x, int y, bool ignoreTransparent)
        {
            return selection.HitTest(x, y, ignoreTransparent);
        }

        /// <summary>
        /// This method performs the hit test and returns a HitTestResult object.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="requestedElement">Only this chart element will be hit tested.</param>
        /// <returns>Hit test result object</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public HitTestResult HitTest(int x, int y, ChartElementType requestedElement)
        {
            return selection.HitTest(x, y, requestedElement);
        }

        /// <summary>
        /// Call this method to determine the  chart element,
        /// if any, that is located at a point defined by the given X and Y 
        /// coordinates.
        /// <seealso cref="HitTestResult"/></summary>
        /// <param name="x">The X coordinate for the point in question.
        /// Often obtained from a parameter in an event
        /// (e.g. the X parameter value in the MouseDown event).</param>
        /// <param name="y">The Y coordinate for the point in question.
        /// Often obtained from a parameter in an event
        /// (e.g. the Y parameter value in the MouseDown event).</param>
        /// <param name="ignoreTransparent">Indicates that transparent 
        /// elements should be ignored.</param>
        /// <param name="requestedElement">
        /// An array of type which specify the types                  
        /// to test for, on order to filter the result. If omitted checking for                 
        /// elementTypes will be ignored and all kind of elementTypes will be 
        /// valid.
        ///  </param>
        /// <returns>
        /// A array of <see cref="HitTestResult"/> objects,
        /// which provides information concerning the  chart element
        /// (if any) that is at the specified location. Result contains at least
        /// one element, which could be ChartElementType.Nothing. 
        /// The objects in the result are sorted in from top to bottom of 
        /// different layers of control. </returns>
        /// <remarks>Call this method to determine the  gauge element
        /// (if any) that is located at a specified point. Often this method is used in
        /// some mouse-related event (e.g. MouseDown)
        /// to determine what  gauge element the end-user clicked on.
        /// The X and Y mouse coordinates obtained from the
        /// event parameters are then used for the X and Y parameter              
        /// values of this method call.   The returned 
        /// <see cref="HitTestResult"/> object's properties
        /// can then be used to determine what  chart element was clicked on,
        /// and also provides a reference to the actual object selected (if 
        /// any).</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public HitTestResult[] HitTest(int x, int y, bool ignoreTransparent, params ChartElementType[] requestedElement)
        {
            return this.selection.HitTest(x, y, ignoreTransparent, requestedElement);
        }

        /// <summary>
        /// Gets the chart element outline.
        /// </summary>
        /// <param name="chartElement">The chart element.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns> A <see cref="ChartElementOutline"/> object which contains
        /// 1) An array of points in absolute coordinates which can be used as outline markers arround this chart element.
        /// 2) A GraphicsPath for drawing aouline around this chart emenent.
        /// </returns>
        /// <remarks>
        /// If the <paramref name="chartElement"/> is not part of the chart or <paramref name="elementType"/> cannot be combined 
        /// with <paramref name="chartElement"/> then the result will contain empty array of marker points. 
        /// The marker points are sorted clockwise.
        /// </remarks>
        public ChartElementOutline GetChartElementOutline(object chartElement, ChartElementType elementType)
        {
            return this.selection.GetChartElementOutline(chartElement, elementType);
        }

		#endregion

		#region Chart image saving methods

        /// <summary>
        /// Draws chart on the graphics.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        /// <param name="position">Position to draw in the graphics.</param>
        public void Paint(Graphics graphics, Rectangle position)
        {
                // Change chart size to fit the new position
                int oldWidth = this.chartPicture.Width;
                int oldHeight = this.chartPicture.Height;
                // Save graphics state.
                GraphicsState transState = graphics.Save();
                try
                {
                    this.chartPicture.Width = position.Width;
                    this.chartPicture.Height = position.Height;
                    // Set required transformation
                    graphics.TranslateTransform(position.X, position.Y);
                    // Set printing indicator
                    this.chartPicture.isPrinting = true;
                    // Draw chart
                    this.chartPicture.Paint(graphics, false);
                    // Clear printing indicator
                    this.chartPicture.isPrinting = false;

                }
                finally
                {
                    // Restore graphics state.
                    graphics.Restore(transState);
                    // Restore old chart position
                    this.chartPicture.Width = oldWidth;
                    this.chartPicture.Height = oldHeight;

                }
        }
		
        /// <summary>
		/// Saves chart image into the file.
		/// </summary>
		/// <param name="imageFileName">Image file name</param>
		/// <param name="format">Image format.</param>
		public void SaveImage(string imageFileName, ChartImageFormat format)
        {
            // Check arguments
            if (imageFileName == null)
                throw new ArgumentNullException("imageFileName");

			// Create file stream for the specified file name
			FileStream	fileStream = new FileStream(imageFileName, FileMode.Create);

			// Save into stream
			try 
			{
				SaveImage(fileStream, format);
			}
			finally
			{
				// Close file stream
				fileStream.Close();
			}
		}


		/// <summary>
		/// Saves chart image into the stream.
		/// </summary>
		/// <param name="imageStream">Image stream.</param>
		/// <param name="format">Image format.</param>
		public void SaveImage( Stream imageStream, ChartImageFormat format)
		{
            // Check arguments
            if (imageStream == null)
                throw new ArgumentNullException("imageStream");

            this.chartPicture.isPrinting = true;
            try
            {
                if (format == ChartImageFormat.Emf ||
                    format == ChartImageFormat.EmfDual ||
                    format == ChartImageFormat.EmfPlus)
                {
                    EmfType emfType = EmfType.EmfOnly;
                    if (format == ChartImageFormat.EmfDual)
                    {
                        emfType = EmfType.EmfPlusDual;
                    }
                    else if (format == ChartImageFormat.EmfPlus)
                    {
                        emfType = EmfType.EmfPlusOnly;
                    }

                    // Save into the metafile
                    this.chartPicture.SaveIntoMetafile(imageStream, emfType);
                }
                else
                {
                    // Get chart image
                    System.Drawing.Image chartImage = this.chartPicture.GetImage();
                    
                    ImageFormat standardImageFormat = ImageFormat.Png;

                    switch (format)
                    {
                        case ChartImageFormat.Bmp:
                            standardImageFormat = ImageFormat.Bmp;
                            break;

                        case ChartImageFormat.Gif:
                            standardImageFormat = ImageFormat.Gif;
                            break;
                        
                       case ChartImageFormat.Tiff:
                            standardImageFormat = ImageFormat.Tiff;
                            break;


                        case ChartImageFormat.Jpeg:
                            standardImageFormat = ImageFormat.Jpeg;
                            break;
                        case ChartImageFormat.Png:
                            standardImageFormat = ImageFormat.Png;
                            break;

                        
                        case ChartImageFormat.Emf:
                            standardImageFormat = ImageFormat.Emf;
                            break;
                    }

                    // Save image into the file
                    chartImage.Save(imageStream, standardImageFormat);

                    // Dispose image
                    chartImage.Dispose();
                }
            }
            finally
            {
                this.chartPicture.isPrinting = false;
            }
		}


		/// <summary>
		/// Saves image into the stream. ImageType, Compression and other control properties are used.
		/// </summary>
		/// <param name="imageStream">Image stream.</param>
        public void SaveImage(Stream imageStream)
		{
            // Check arguments
            if (imageStream == null)
                throw new ArgumentNullException("imageStream");

			//*****************************************************
			//** Disable validating the license for now....
			//*****************************************************
			// ValidateLicense();

            this.chartPicture.isPrinting = true;
            try
            {

			// Save into the metafile
			if( ImageType == ChartImageType.Emf)
			{
				this.chartPicture.SaveIntoMetafile(imageStream, EmfType.EmfOnly);
				return;
			}

            System.Drawing.Image image = chartPicture.GetImage();
            // Set image settings
			ImageCodecInfo imageCodecInfo = null;
			EncoderParameter encoderParameter = null;
			EncoderParameters encoderParameters = new EncoderParameters(1);

			// Get image codec information
			if(ImageType == ChartImageType.Bmp)
			{
				imageCodecInfo = GetEncoderInfo("image/bmp");
			}
			else if(ImageType == ChartImageType.Jpeg)
			{
				imageCodecInfo = GetEncoderInfo("image/jpeg");
			}
			else if(ImageType == ChartImageType.Png)
			{
				imageCodecInfo = GetEncoderInfo("image/png");
			}

			// Set image quality
			encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100-Compression);
			encoderParameters.Param[0] = encoderParameter;

			// Save image into the file
			if(imageCodecInfo == null)
			{
				ImageFormat	format = (ImageFormat)new ImageFormatConverter().ConvertFromString(ImageType.ToString());
				image.Save(imageStream, format);
			}
			else
			{
				image.Save(imageStream, imageCodecInfo, encoderParameters);
			}

			image.Dispose();
        }
        finally
        {
            this.chartPicture.isPrinting = false;
        }
    }

			
		/// <summary>
		/// Saves image into the file. ImageType, Compression and other control properties are used.
		/// </summary>
		/// <param name="imageFileName">Image file name</param>
		public void SaveImage(string imageFileName)
		{
            // Check arguments
            if (imageFileName == null)
                throw new ArgumentNullException("imageFileName");

			// Create file stream for the specified file name
			FileStream	fileStream = new FileStream(imageFileName, FileMode.Create);

			// Save into stream
			try 
			{
				SaveImage(fileStream);
			}
			finally
			{
				// Close file stream
				fileStream.Close();
			}
		}





		/// <summary>
		/// Helper function. Returns image encoder using Mime image type
		/// </summary>
		/// <param name="mimeType">Mime image type</param>
		/// <returns>Image codec</returns>
		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for(j = 0; j < encoders.Length; ++j)
			{
				if(encoders[j].MimeType == mimeType)
				{
					return encoders[j];
				}
			}
			return null;
		}

#endregion

		#region Control events


        // Defines a key for storing the delegate for the PrePaint event
        // in the Events list.
        private static readonly object _prePaintEvent = new object();

		/// <summary>
		/// Fires after the chart element backround was drawn. 
		/// This event is fired for elements like: ChartPicture, ChartArea and Legend
		/// </summary>
		[
		SRDescription("DescriptionAttributeChartEvent_PrePaint")
		]
        public event EventHandler<ChartPaintEventArgs> PrePaint
        {
            add { Events.AddHandler(_prePaintEvent, value); }
            remove { Events.RemoveHandler(_prePaintEvent, value); }
        }

        // Defines a key for storing the delegate for the PrePaint event
        // in the Events list.
        private static readonly object _postPaintEvent = new object();

		/// <summary>
		/// Fires after chart element was drawn. 
		/// This event is fired for elements like: ChartPicture, ChartArea and Legend
		/// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_PostPaint")
        ]
        public event EventHandler<ChartPaintEventArgs> PostPaint
        {
            add { Events.AddHandler(_postPaintEvent, value); }
            remove { Events.RemoveHandler(_postPaintEvent, value); }
        }

        // Defines a key for storing the delegate for the CustomizeMapAreas event
        // in the Events list.
        private static readonly object _customizeMapAreasEvent = new object();

		/// <summary>
		/// Fires just before the chart image map is rendered. Use this event to customize the map areas items.
		/// </summary>
		[
		SRDescription("DescriptionAttributeChartEvent_CustomizeMapAreas")
		]
        public event EventHandler<CustomizeMapAreasEventArgs> CustomizeMapAreas
        {
            add { Events.AddHandler(_customizeMapAreasEvent, value); }
            remove { Events.RemoveHandler(_customizeMapAreasEvent, value); }
        }



        // Defines a key for storing the delegate for the CustomizeMapAreas event
        // in the Events list.
        private static readonly object _customizeEvent = new object();
        /// <summary>
		/// Fires just before the chart image is drawn. Use this event to customize the chart picture.
		/// </summary>
		[
		SRDescription("DescriptionAttributeChartEvent_Customize")
		]
		public event EventHandler Customize
        {
            add { Events.AddHandler(_customizeEvent, value); }
            remove { Events.RemoveHandler(_customizeEvent, value); }
        }

        // Defines a key for storing the delegate for the CustomizeMapAreas event
        // in the Events list.
        private static readonly object _customizeLegendEvent = new object();
        /// <summary>
		/// Fires just before the chart legend is drawn. Use this event to customize the chart legend items.
		/// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_CustomizeLegend")
        ]
        public event EventHandler<CustomizeLegendEventArgs> CustomizeLegend
        {
            add { Events.AddHandler(_customizeLegendEvent, value); }
            remove { Events.RemoveHandler(_customizeLegendEvent, value); }
        }

        // Defines a key for storing the delegate for the Click event
        // in the Events list.
        private static readonly object _clickEvent = new object();
        /// <summary>
        /// Occurs when active image map area defined by PostBackValue on Chart control is clicked.
        /// </summary>
        [
        SRCategory("CategoryAttributeAction"),
        SRDescription(SR.Keys.DescriptionAttributeChartEvent_Click)
        ]
        public event ImageMapEventHandler Click
        {
            add { Events.AddHandler(_clickEvent, value); }
            remove { Events.RemoveHandler(_clickEvent, value); }
        }


		#endregion

		#region Event Handling

      
		/// <summary>
		/// Invokes delegates registered with the Click event.
		/// </summary>
		/// <param name="e"></param>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnClick(ImageMapEventArgs e) 
		{
            ImageMapEventHandler clickEventDelegate = (ImageMapEventHandler)Events[_clickEvent];
			if (clickEventDelegate != null) 
			{
				clickEventDelegate(this, e);
			}  
		}
      
        
		/// <summary>
        /// Raises events for the Chart control when a form is posted back to the server.
		/// </summary>
		/// <param name="eventArgument">Event argument.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected virtual void RaisePostBackEvent(string eventArgument)
		{
            if (!String.IsNullOrEmpty(eventArgument))
            {
                this.OnClick(new ImageMapEventArgs(eventArgument));
            }
		}

		/// <summary>
		/// Fires when chart element backround must be drawn. 
		/// This event is fired for elements like: ChatPicture, ChartArea and Legend
		/// </summary>
		/// <param name="e">Event arguments.</param>
		[
		SRDescription("DescriptionAttributeChart_OnBackPaint")
		]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnPrePaint(ChartPaintEventArgs e) 
		{
            EventHandler<ChartPaintEventArgs> prePaintEventDelegate = (EventHandler<ChartPaintEventArgs>)Events[_prePaintEvent];
            if (prePaintEventDelegate != null)
            {
                prePaintEventDelegate(this, e);
            }  
		}

        /// <summary>
        /// Fires when chart element backround must be drawn. 
        /// This event is fired for elements like: ChatPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal void CallOnPrePaint(ChartPaintEventArgs e)
        {
            this.OnPrePaint(e);
        }

		/// <summary>
		/// Fires when chart element must be drawn. 
		/// This event is fired for elements like: ChatPicture, ChartArea and Legend
		/// </summary>
		/// <param name="e">Event arguments.</param>
		[
		SRDescription("DescriptionAttributeChart_OnPaint")
		]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnPostPaint(ChartPaintEventArgs e) 
		{
            EventHandler<ChartPaintEventArgs> postPaintEventDelegate = (EventHandler<ChartPaintEventArgs>)Events[_postPaintEvent];
            if (postPaintEventDelegate != null)
            {
                postPaintEventDelegate(this, e);
            }  
		}

        /// <summary>
        /// Fires when chart element must be drawn. 
        /// This event is fired for elements like: ChatPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal void CallOnPostPaint(ChartPaintEventArgs e)
        {
            this.OnPostPaint(e);
        }

        /// <summary>
        /// Fires when chart image map data is prepared to be rendered.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.DataVisualization.Charting.CustomizeMapAreasEventArgs"/> instance containing the event data.</param>
		[
		SRDescription("DescriptionAttributeChart_OnCustomizeMapAreas")
		]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnCustomizeMapAreas(CustomizeMapAreasEventArgs e) 
		{
            EventHandler<CustomizeMapAreasEventArgs> customizeMapAreasEventDelegate = (EventHandler<CustomizeMapAreasEventArgs>)Events[_customizeMapAreasEvent];
            if (customizeMapAreasEventDelegate != null)
            {
                customizeMapAreasEventDelegate(this, e);
            }  
		}

        /// <summary>
        /// Fires when chart image map data is prepared to be rendered.
        /// </summary>
        internal void CallOnCustomizeMapAreas(MapAreasCollection areaItems)
        {
            this.OnCustomizeMapAreas(new CustomizeMapAreasEventArgs(areaItems));
        }

        /// <summary>
        /// Fires when all chart data is prepared to be customized before drawing.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		[
		SRDescription("DescriptionAttributeChart_OnCustomize")
		]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnCustomize(EventArgs e) 
		{
            EventHandler customizeEventDelegate = (EventHandler)Events[_customizeEvent];
            if (customizeEventDelegate != null)
            {
                customizeEventDelegate(this, e);
            }  
		}

		/// <summary>
		/// Fires when all chart legend data is prepared to be customized before drawing.
		/// </summary>
		[
		SRDescription("DescriptionAttributeChart_OnCustomizeLegend")
		]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnCustomizeLegend(CustomizeLegendEventArgs e) 
		{
            EventHandler<CustomizeLegendEventArgs> customizeLegendEventDelegate = (EventHandler<CustomizeLegendEventArgs>)Events[_customizeLegendEvent];
            if (customizeLegendEventDelegate != null)
            {
                customizeLegendEventDelegate(this, e);
            }  
		}

		/// <summary>
		/// Event firing helper function.
		/// </summary>
		internal void CallOnCustomize() 
		{
            OnCustomize(EventArgs.Empty);
		}

		/// <summary>
		/// Event firing helper function.
		/// </summary>
		internal void CallOnCustomizeLegend(LegendItemsCollection legendItems, string legendName) 
		{
            OnCustomizeLegend(new CustomizeLegendEventArgs(legendItems, legendName));
		}

		#endregion

		#region View state properties and methods


		/// <summary>
		/// Restores view-state information from a previous page request that was saved by the SaveViewState method.
		/// </summary>
		/// <param name="savedState">An Object that represents the control state to be restored.</param>
		protected override void LoadViewState(object savedState)
		{
            // Call the base class
            base.LoadViewState(savedState);

			// Check if view state is enabled
			if(this.EnableViewState)
			{

                // Load chart's data if custom user state data was not set
				if(this.ViewState["ViewStateData"] != null &&
					(this.ViewState["CustomUserViewStateData"] == null ||
					((string)this.ViewState["CustomUserViewStateData"]) == "false"))
				{
					// Set serializable content
					SerializationContents  oldContent = this.Serializer.Content;
					string oldSerializable = this.Serializer.SerializableContent;
					string oldNonSerializable = this.Serializer.NonSerializableContent;
					SerializationFormat oldFormat = this.Serializer.Format;
					this.Serializer.Content = this.ViewStateContent;
					this.Serializer.Format = SerializationFormat.Xml;

					// Load data in the chart from the view state
					StringReader stringReader = new StringReader((string)this.ViewState["ViewStateData"]);

					this.Serializer.Load(stringReader);

					// Remove chart data from view state
					this.ViewState.Remove("ViewStateData");

					// Restore serializable content
					this.Serializer.Format = oldFormat;
					this.Serializer.Content = oldContent;
					this.Serializer.SerializableContent = oldSerializable;
					this.Serializer.NonSerializableContent = oldNonSerializable;
				}
			}
		}

		/// <summary>
		/// Saves any server control view-state changes that have occurred since the time the page was posted back to the server.
		/// </summary>
		/// <returns>Returns the server control's current view state. </returns>
		protected override object SaveViewState()
		{
            // Check if view state is enabled
			if(base.EnableViewState)
			{
				// Save chart's data if custom user state data was not set
				if(this.ViewState["ViewStateData"] == null)
				{
					// Set serializable content
					SerializationContents  oldContent = this.Serializer.Content;
					string oldSerializable = this.Serializer.SerializableContent;
					string oldNonSerializable = this.Serializer.NonSerializableContent;
					this.Serializer.Content = this.ViewStateContent;

					// Save data from the chart into the view state
					StringBuilder stringBuilder = new StringBuilder();
					StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
					this.Serializer.Save(stringWriter);

					// Put data in view state
					this.ViewState["ViewStateData"] = (string)stringBuilder.ToString();

					// Remove chart user custom view state flag
					this.ViewState.Remove("CustomUserViewStateData");

					// Restore serializable content
					this.Serializer.Content = oldContent;
					this.Serializer.SerializableContent = oldSerializable;
					this.Serializer.NonSerializableContent = oldNonSerializable;
				}
                // Call base class
            }
            return base.SaveViewState();
		}



		/// <summary>
		/// Gets or sets a value indicating whether the control persists its view state.
		/// </summary>
		[
		SRCategory("CategoryAttributeViewState"),
		Bindable(true),
		SRDescription("DescriptionAttributeChart_EnableViewState"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
		DefaultValue(false)
		]
		public override bool EnableViewState
		{
			get
			{
                return base.EnableViewState;
			}
			set
			{
				base.EnableViewState = value;
			}
		}



		/// <summary>
		/// Chart content saved in the view state.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(typeof(SerializationContents ), "Default"),
		SRDescription("DescriptionAttributeChart_ViewStateContent"),
        Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base)
		]
		public SerializationContents  ViewStateContent
		{
			get
			{
				return _viewStateContent;
			}
			set
			{
                int result = 0;
                if (Int32.TryParse(value.ToString(), out result))
                {
                    throw new ArgumentException(SR.ExceptionEnumInvalid(value.ToString()));
                }
                _viewStateContent = value;
			}
		}

		/// <summary>
		/// User defined control state data in XML format.
		/// </summary>
		[
		SRCategory("CategoryAttributeViewState"),
        Browsable(false),
        Obsolete("ViewStateData has been deprecated. Please investigate Control.ViewState instead."),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeChart_ViewStateData"),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never) 
		]
		public string ViewStateData
		{
			get
			{
				return (string)this.ViewState["ViewStateData"];
			}
			set
			{
				// Set state data
				this.ViewState["ViewStateData"] = value;

				// Set custom user state data indicator
				this.ViewState["CustomUserViewStateData"] = "true";
			}
		}


		#endregion

		#region Control properties



		/// <summary>
		/// Indicates that non-critical chart exceptions will be suppressed.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeSuppressExceptions"),
		]
		public bool SuppressExceptions
		{
			set
			{
				this.chartPicture.SuppressExceptions = value;
			}
			get
			{
				return this.chartPicture.SuppressExceptions;
			}
		}



		/// <summary>
		/// Chart named images collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		Bindable(false),
		SRDescription("DescriptionAttributeChart_Images"),
		Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        , EditorBrowsable(EditorBrowsableState.Never)
		]
		public NamedImagesCollection Images
		{
			get
			{
				return _namedImages;
			}
		}

        /// <summary>
		/// Font property is not used.
		/// </summary>
		[
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public override FontInfo Font
		{
			get
			{
				return base.Font;
			}
		}

        /// <summary>
		/// Chart rendering type. Image tag, input tag, binary data streaming and image map are the options.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		SRDescription("DescriptionAttributeChart_RenderType"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
		DefaultValue(RenderType.ImageTag)
		]
		public RenderType RenderType
		{
			get
			{
				return _renderType;
			}
			set
			{
				_renderType = value;

				if(_renderType == RenderType.ImageMap && this.IsMapEnabled == false)
				{
					this.IsMapEnabled = true;
				}
			}
		}



        /// <summary>
		/// Location where chart image is saved, when image tag is used for rendering.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		SRDescription("DescriptionAttributeChart_ImageUrl"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
		DefaultValue("ChartPic_#SEQ(300,3)"),
        Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base)
		]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string ImageLocation
		{
			get
			{
				return _chartImageLocation;
			}
			set
			{
				// Find the begginning of the "#SEQ" formatting string
				int indexSEQ = value.IndexOf("#SEQ", StringComparison.Ordinal);
				if(indexSEQ > 0)
				{
					// Check format
					CheckImageURLSeqFormat(value);
				}
				_chartImageLocation = value;
			}
		}

        // VSTS 96787-Text Direction (RTL/LTR)

        /// <summary>
        /// Indicates whether the control should draw right-to-left for RTL languages.
        /// <seealso cref="AntiAliasing"/>
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Forms.RightToLeft"/> values. The default is
        /// <b>RightToLeft.No</b>.
        /// </value>
        /// <remarks>This property affects the direction of legend color keys.</remarks>
        [
        Category("Appearance"),
        SRDescription("DescriptionAttributeRightToLeft"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(RightToLeft.No)
        ]
        public RightToLeft RightToLeft
        {
            get
            {
                return this.chartPicture.RightToLeft;
            }
            set
            {
                this.chartPicture.RightToLeft = value;
            }
        }

		#endregion

		#region Data Manager Properties

		/// <summary>
		/// Chart series collection.
		/// </summary>
		[
        SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeChart_Series"),
		PersistenceModeAttribute(PersistenceMode.InnerProperty),
        Editor(Editors.SeriesCollectionEditor.Editor, Editors.SeriesCollectionEditor.Base),
#if !Microsoft_CONTROL
        Themeable(false)
#endif 
		]
		public SeriesCollection Series
		{
			get
			{
				return _dataManager.Series;
			}
		}

		/// <summary>
		/// Color palette to use
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		SRDescription("DescriptionAttributePalette"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
        DefaultValue(ChartColorPalette.BrightPastel),
        Editor(Editors.ColorPaletteEditor.Editor, Editors.ColorPaletteEditor.Base)
		]
		public ChartColorPalette Palette
		{
			get
			{
				return _dataManager.Palette;
			}
			set
			{
				_dataManager.Palette = value;
			}
		}



		/// <summary>
		/// Array of custom palette colors.
		/// </summary>
		/// <remarks>
		/// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		SerializationVisibilityAttribute(SerializationVisibility.Attribute),
		SRDescription("DescriptionAttributeChart_PaletteCustomColors"),
		TypeConverter(typeof(ColorArrayConverter))
		]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Color[] PaletteCustomColors
		{
			set
			{
				this._dataManager.PaletteCustomColors = value;
			}
			get
			{
				return this._dataManager.PaletteCustomColors;
			}
		}

		/// <summary>
		/// Method resets custom colors array. Internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal void ResetPaletteCustomColors()
		{
			this.PaletteCustomColors = new Color[0];
		}

		/// <summary>
		/// Method resets custom colors array. Internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldSerializePaletteCustomColors()
		{
			if(this.PaletteCustomColors == null ||
				this.PaletteCustomColors.Length == 0)
			{
				return false;
			}
			return true;
		}



		#endregion

		#region Chart Properties


		/// <summary>
		/// "The data source used to populate series data. Series ValueMember properties must be also set."
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		SRDescription("DescriptionAttributeDataSource"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
		DefaultValue(null),
		TypeConverter(typeof(ChartDataSourceConverter)),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public override object DataSource
		{
			get
			{
				return base.DataSource;
			}
			set
			{
                base.DataSource = value;
				chartPicture.DataSource = value;
			}
		}

		/// <summary>
		/// Build number of the control
		/// </summary>
		[
		SRDescription("DescriptionAttributeChart_BuildNumber"),
		Browsable(false),
		EditorBrowsable(EditorBrowsableState.Never),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), 
		DefaultValue("")
		]
		public string BuildNumber
		{
			get
			{
				// Get build number from the assembly
				string	buildNumber = String.Empty;
				Assembly assembly = Assembly.GetExecutingAssembly();
				if(assembly != null)
				{
					buildNumber = assembly.FullName.ToUpper(CultureInfo.InvariantCulture);
					int	versionIndex = buildNumber.IndexOf("VERSION=", StringComparison.Ordinal);
					if(versionIndex >= 0)
					{
						buildNumber = buildNumber.Substring(versionIndex + 8);
					}
                    versionIndex = buildNumber.IndexOf(",", StringComparison.Ordinal);
					if(versionIndex >= 0)
					{
						buildNumber = buildNumber.Substring(0, versionIndex);
					}
				}
				return buildNumber;
			}
		}

		/// <summary>
		/// Chart serializer object.
		/// </summary>
		[
		SRCategory("CategoryAttributeSerializer"),
		SRDescription("DescriptionAttributeChart_Serializer"),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public ChartSerializer Serializer
		{
			get
			{
				return _chartSerializer;
			}
		}



		/// <summary>
		/// Image type (Jpeg, BMP, Png, Svg, Flash)
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(ChartImageType.Png),
		SRDescription("DescriptionAttributeChartImageType"),
        PersistenceMode(PersistenceMode.Attribute),
		RefreshProperties(RefreshProperties.All)
		]
		public ChartImageType ImageType
		{
			get
			{
				return chartPicture.ImageType;
			}
			set
			{
				chartPicture.ImageType = value;
			}
		}

		/// <summary>
		/// Image compression value
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeChart_Compression"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int Compression
		{
			get
			{
				return chartPicture.Compression;
			}
			set
			{
				chartPicture.Compression = value;
			}
		}

        /*
         * Disabled until we get responce from Microsoft
         *  --- Alex
         * 
                /// <summary>
                /// Gif image transparent color
                /// </summary>
                [
                SRCategory("CategoryAttributeImage"),
                Bindable(true),
                DefaultValue(typeof(Color), ""),
                Description("Gif image transparent color."),
                PersistenceMode(PersistenceMode.Attribute),
                TypeConverter(typeof(ColorConverter)),
                Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
                ]
                public Color TransparentColor
                {
                    get
                    {
                        return chartPicture.TransparentColor;
                    }
                    set
                    {
                        chartPicture.TransparentColor = value;
                    }
                }
        */
        #endregion

        #region Chart Image Properties

        /// <summary>
        /// Indicates that chart image map is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeMap"),
        Bindable(true),
        SRDescription(SR.Keys.DescriptionAttributeIsMapAreaAttributesEncoded),
        PersistenceModeAttribute(PersistenceMode.Attribute),
        DefaultValue(false)
        ]
        public bool IsMapAreaAttributesEncoded { get; set; }


        /// <summary>
		/// Indicates that chart image map is enabled.
		/// </summary>
		[
		SRCategory("CategoryAttributeMap"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapEnabled"),
		PersistenceModeAttribute(PersistenceMode.Attribute),
		DefaultValue(true)
		]
		public bool IsMapEnabled
		{
			get
			{
				return chartPicture.IsMapEnabled;
			}
			set
			{
				chartPicture.IsMapEnabled = value;
			}
		}

		/// <summary>
		/// Chart map areas collection.
		/// </summary>
		[
        SRCategory("CategoryAttributeMap"),
		SRDescription("DescriptionAttributeMapAreas"),
		PersistenceModeAttribute(PersistenceMode.InnerProperty),
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		public MapAreasCollection MapAreas
		{
			get
			{
				return chartPicture.MapAreas;
			}
		}

		/// <summary>
		/// Specifies whether smoothing (antialiasing) is applied while drawing chart.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
        DefaultValue(typeof(AntiAliasingStyles), "All"),
		SRDescription("DescriptionAttributeAntiAlias"),
		PersistenceMode(PersistenceMode.Attribute),
		Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base)
		]
        public AntiAliasingStyles AntiAliasing
		{
			get
			{
				return chartPicture.AntiAliasing;
			}
			set
			{
				chartPicture.AntiAliasing = value;
			}
		}

		/// <summary>
		/// Specifies the quality of text antialiasing.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(TextAntiAliasingQuality), "High"),
		SRDescription("DescriptionAttributeTextAntiAliasingQuality"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
		]
		public TextAntiAliasingQuality TextAntiAliasingQuality
		{
			get
			{
				return chartPicture.TextAntiAliasingQuality;
			}
			set
			{
				chartPicture.TextAntiAliasingQuality = value;
			}
		}

		/// <summary>
		/// Specifies whether smoothing is applied while drawing shadows.
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeChart_SoftShadows"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool IsSoftShadows
		{
			get
			{
				return chartPicture.IsSoftShadows;
			}
			set
			{
				chartPicture.IsSoftShadows = value;
			}
		}

		/// <summary>
		/// Reference to chart area collection
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		Bindable(true),
		SRDescription("DescriptionAttributeChartAreas"),
		PersistenceMode(PersistenceMode.InnerProperty),
		Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		public ChartAreaCollection ChartAreas
		{
			get
			{
				return chartPicture.ChartAreas;
			}
		}

		/// <summary>
		/// Back ground color for the Chart
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "White"),
        SRDescription("DescriptionAttributeBackColor"),
		PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public override Color BackColor
		{
			get
			{
				return chartPicture.BackColor;
			}
			set
			{
				chartPicture.BackColor = value;
			}
		}

		/// <summary>
		/// Fore color propery (not used)
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(false),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		SRDescription("DescriptionAttributeChart_ForeColor"),
		PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public override Color ForeColor
		{
			get
			{
				return Color.Empty;
			}
			set
			{
			}
		}

		/// <summary>
		/// Chart width
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(Unit), "300"),
		SRDescription("DescriptionAttributeWidth"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public override Unit Width
		{
			get
			{
				return new Unit(chartPicture.Width);
			}
			set
			{
				if(value.Type != UnitType.Pixel)
				{
                    throw (new ArgumentException(SR.ExceptionChartWidthIsNotInPixels));
				}
				chartPicture.Width = (int)value.Value;
			}
		}

		/// <summary>
		/// Chart legend collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeLegends"),
		PersistenceMode(PersistenceMode.InnerProperty),
		Editor(Editors.LegendCollectionEditor.Editor, Editors.LegendCollectionEditor.Base),
		]
		public LegendCollection Legends
		{
			get
			{
				return chartPicture.Legends;
			}
		}

		/// <summary>
		/// Chart title collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeTitles"),
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base),
		PersistenceMode(PersistenceMode.InnerProperty),
		]
		public TitleCollection Titles
		{
			get
			{
				return chartPicture.Titles;
			}
		}


		/// <summary>
		/// Chart annotation collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeChart"),
		SRDescription("DescriptionAttributeAnnotations3"),
		Editor(Editors.AnnotationCollectionEditor.Editor, Editors.AnnotationCollectionEditor.Base),
		PersistenceMode(PersistenceMode.InnerProperty),
		]
		public AnnotationCollection Annotations
		{
			get
			{
				return chartPicture.Annotations;
			}
		}


		/// <summary>
		/// Series data manipulator
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		SRDescription("DescriptionAttributeDataManipulator"),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public DataManipulator DataManipulator
		{
			get
			{
				return chartPicture.DataManipulator;
			}
		}


		/// <summary>
		/// Chart height
		/// </summary>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(Unit), "300"),
		SRDescription("DescriptionAttributeHeight3"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public override Unit Height
		{
			get
			{
				return new Unit(chartPicture.Height);
			}
			set
			{
				if(value.Type != UnitType.Pixel)
				{
                    throw (new ArgumentException(SR.ExceptionChartHeightIsNotInPixels));
				}
				chartPicture.Height = (int)value.Value;
			}
		}


		/// <summary>
		/// Back Hatch style
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartHatchStyle.None),
        SRDescription("DescriptionAttributeBackHatchStyle"),
		PersistenceMode(PersistenceMode.Attribute),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return chartPicture.BackHatchStyle;
			}
			set
			{
				chartPicture.BackHatchStyle = value;
			}
		}

		/// <summary>
		/// Chart area background image
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
        SRDescription("DescriptionAttributeBackImage"),
		PersistenceMode(PersistenceMode.Attribute),
		NotifyParentPropertyAttribute(true),
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)
		]
		public string BackImage
		{
			get
			{
				return chartPicture.BackImage;
			}
			set
			{
				chartPicture.BackImage = value;
			}
		}

		/// <summary>
		/// Chart area background image drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageWrapMode.Tile),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageWrapMode"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public ChartImageWrapMode BackImageWrapMode
		{
			get
			{
				return chartPicture.BackImageWrapMode;
			}
			set
			{
				chartPicture.BackImageWrapMode = value;
			}
		}

		/// <summary>
		/// Background image transparent color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageTransparentColor"),
		PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public Color BackImageTransparentColor
		{
			get
			{
				return chartPicture.BackImageTransparentColor;
			}
			set
			{
				chartPicture.BackImageTransparentColor = value;
			}
		}

		/// <summary>
		/// Background image alignment used by ClampUnscale drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageAlignmentStyle.TopLeft),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackImageAlign"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public ChartImageAlignmentStyle BackImageAlignment
		{
			get
			{
				return chartPicture.BackImageAlignment;
			}
			set
			{
				chartPicture.BackImageAlignment = value;
			}
		}

		/// <summary>
		/// A type for the background gradient
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(GradientStyle.None),
        SRDescription("DescriptionAttributeBackGradientStyle"),
		PersistenceMode(PersistenceMode.Attribute),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]
		public GradientStyle BackGradientStyle
		{
			get
			{
				return chartPicture.BackGradientStyle;
			}
			set
			{
				chartPicture.BackGradientStyle = value;
			}
		}

		/// <summary>
		/// The second color which is used for a gradient
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
		PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public Color BackSecondaryColor
		{
			get
			{
				return chartPicture.BackSecondaryColor;
			}
			set
			{
				chartPicture.BackSecondaryColor = value;
			}
		}

        
        /// <summary>
        /// Gets or sets the border color of the Chart.
        /// </summary>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Color BorderColor
        {
            get { return base.BorderColor; }
            set { base.BorderColor = value; }
        }
        
        /// <summary>
        /// Gets or sets the border width of the Chart.
        /// </summary>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Unit  BorderWidth
        {
          get { return base.BorderWidth;}
          set { base.BorderWidth = value;}
        }
        
        /// <summary>
        /// Gets or sets the border style of the Chart.
        /// </summary>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }


		/// <summary>
		/// Border line color for the Chart
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "White"),
		SRDescription("DescriptionAttributeBorderColor"),
		PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public Color BorderlineColor
		{
			get
			{
				return chartPicture.BorderColor;
			}
			set
			{
				chartPicture.BorderColor = value;
			}
		}

		/// <summary>
		/// The width of the border line
		/// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1),
		SRDescription("DescriptionAttributeChart_BorderlineWidth"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int BorderlineWidth
		{
			get
			{
				return chartPicture.BorderWidth;
			}
			set
			{
				chartPicture.BorderWidth = value;
			}
		}

		/// <summary>
		/// The style of the border line
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.NotSet),
		SRDescription("DescriptionAttributeBorderDashStyle"),
		PersistenceMode(PersistenceMode.Attribute) 
		]
		public ChartDashStyle BorderlineDashStyle
		{
			get
			{
				return chartPicture.BorderDashStyle;
			}
			set
			{
				chartPicture.BorderDashStyle = value;
			}
		}

        /// <summary>
		/// Chart border skin style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(BorderSkinStyle.None),
		SRDescription("DescriptionAttributeBorderSkin"),
		PersistenceMode(PersistenceMode.InnerProperty),
		NotifyParentProperty(true),
		TypeConverterAttribute(typeof(LegendConverter)),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content) 
		]
		public BorderSkin BorderSkin
		{
			get
			{
				return chartPicture.BorderSkin;
			}
			set
			{
				chartPicture.BorderSkin = value;
			}
		}

        /// <summary>
        /// When overridden in a derived class, gets or sets the alternate text displayed in the Chart control when the chart image is unavailable.
        /// </summary>
        [
        Bindable(true),
        SRDescription(SR.Keys.DescriptionAttributeChartImageAlternateText), 
        Localizable(true), 
        SRCategory(SR.Keys.CategoryAttributeAppearance), 
        DefaultValue("")
        ]
        public string AlternateText { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets or sets the location to a detailed description for the chart.
        /// </summary>
        [
        Bindable(true),
        SRDescription(SR.Keys.DescriptionAttributeChartImageDescriptionUrl), 
        Localizable(true), 
        SRCategory(SR.Keys.CategoryAttributeAccessibility), 
        DefaultValue(""), 
        UrlProperty,
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base),
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")
        ]
        public string DescriptionUrl { get; set; }

		#endregion

		#region Control public methods

        /// <summary>
        /// Creates the HTML text writer.
        /// </summary>
        /// <param name="tw">The inner text writer.</param>
        /// <returns></returns>
        private HtmlTextWriter CreateHtmlTextWriter(TextWriter tw)
        {
            if (((this.Context != null) && (this.Context.Request != null)) && (this.Context.Request.Browser != null))
            {
                return this.Context.Request.Browser.CreateHtmlTextWriter(tw);
            }
            return new Html32TextWriter(tw);
        }

		/// <summary>
		/// Gets HTML image map of the currently rendered chart.
		/// Save(...) method MUST be called before calling this method!
		/// </summary>
		/// <param name="name">Name of the image map tag.</param>
		/// <returns>HTML image map.</returns>
		public string GetHtmlImageMap(string name)
		{
            // Check arguments
            if (name == null)
                throw new ArgumentNullException("name");

            using (StringWriter swriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (HtmlTextWriter writer = CreateHtmlTextWriter(swriter))
                {
                    this.chartPicture.WriteChartMapTag(writer, name);
                    return swriter.GetStringBuilder().ToString();
                }
            }
		}


		/// <summary>
		/// Saves the current state of the chart to an XML file.  This is
		/// mainly used for support purposes.  The executing thread must have
		/// file write permission
		/// </summary>
		/// <param name="name">File path and name to save.</param>
		public void SaveXml(string name)
		{
			try
			{
				this.Serializer.Save(name);
			}
			catch(XmlException)
			{ }
		}


		/// <summary>
		/// Loads chart appearance template from file.
		/// </summary>
		/// <param name="name">Template file name to load from.</param>
		public void LoadTemplate(string name)
		{
			chartPicture.LoadTemplate(name);
		}

		/// <summary>
		/// Loads chart appearance template from stream.
		/// </summary>
		/// <param name="stream">Template stream to load from.</param>
		public void LoadTemplate(Stream stream)
		{
			chartPicture.LoadTemplate(stream);
		}


		/// <summary>
		/// Applies palette colors to series or data points.
		/// </summary>
		public void ApplyPaletteColors()
		{
			// Apply palette colors to series
			this._dataManager.ApplyPaletteColors();

			// Apply palette colors to data Points in series
			foreach(Series series in this.Series)
			{
				// Check if palette colors should be aplied to the points
				bool	applyToPoints = false;
				if(series.Palette != ChartColorPalette.None)
				{
					applyToPoints = true;
				}
				else
				{
					IChartType chartType = this._chartTypeRegistry.GetChartType(series.ChartTypeName);
					applyToPoints = chartType.ApplyPaletteColorsToPoints;
				}

				// Apply palette colors to the points
				if(applyToPoints)
				{
					series.ApplyPaletteColors();
				}
			}
		}

		/// <summary>
		/// Checks if control is in design mode.
		/// </summary>
		/// <returns>True if control is in design mode.</returns>
		internal bool IsDesignMode()
		{
            return this.DesignMode;
        }

		/// <summary>
		/// Reset auto calculated chart properties values to "Auto".
		/// </summary>
		public void ResetAutoValues()
		{
			// Reset auto calculated series properties values 
			foreach(Series series in this.Series)
			{
				series.ResetAutoValues();
			}

			// Reset auto calculated axis properties values 
			foreach(ChartArea chartArea in this.ChartAreas)
			{
				chartArea.ResetAutoValues();
			}

		}

		#endregion

		#region Control DataBind method

        /// <summary>
        /// Verifies that the object a data-bound control binds to is one it can work with.
        /// </summary>
        /// <param name="dataSource">The object to verify</param>
        protected override void ValidateDataSource(object dataSource)
        {
            if (!ChartImage.IsValidDataSource(dataSource))
            {
                base.ValidateDataSource(dataSource);
            }
        } 

        /// <summary>
        /// Binds the specified data source to the Chart control.
        /// </summary>
        /// <param name="data">An <see cref="IEnumerable"/> that represents the data source.</param>
        protected override void PerformDataBinding(IEnumerable data)
        {
            this.chartPicture.DataBind(data, null);
            this.chartPicture.boundToDataSource = true;
        }

		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		public void AlignDataPointsByAxisLabel()
		{
			this.chartPicture.AlignDataPointsByAxisLabel(false, PointSortOrder.Ascending);
		}

		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		/// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
		public void AlignDataPointsByAxisLabel(string series)
		{
			// Create list of series
			ArrayList seriesList = new ArrayList();
			string[] seriesNames = series.Split(',');
			foreach(string name in seriesNames)
			{
				seriesList.Add(this.Series[name.Trim()]);
			}

			// Align series
			this.chartPicture.AlignDataPointsByAxisLabel(seriesList, false, PointSortOrder.Ascending);
		}

		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		/// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
		/// <param name="sortingOrder">Points sorting order by axis labels.</param>
		public void AlignDataPointsByAxisLabel(string series, PointSortOrder sortingOrder)
		{
			// Create list of series
			ArrayList seriesList = new ArrayList();
			string[] seriesNames = series.Split(',');
			foreach(string name in seriesNames)
			{
				seriesList.Add(this.Series[name.Trim()]);
			}

			// Align series
			this.chartPicture.AlignDataPointsByAxisLabel(seriesList, true, sortingOrder);
		}

		/// <summary>
		/// Aligns data points using their axis labels.
		/// </summary>
		/// <param name="sortingOrder">Points sorting order by axis labels.</param>
		public void AlignDataPointsByAxisLabel(PointSortOrder sortingOrder)
		{
			this.chartPicture.AlignDataPointsByAxisLabel(true, sortingOrder);
		}



		/// <summary>
		/// Automatically creates and binds series to specified data table. 
		/// Each column of the table becomes a Y value in a separate series.
		/// Series X value field may also be provided. 
		/// </summary>
		/// <param name="dataSource">Data source.</param>
		/// <param name="xField">Name of the field for series X values.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X is a cartesian coordinate and well understood")]
        public void DataBindTable(
			IEnumerable dataSource, 
			string xField)
		{
			this.chartPicture.DataBindTable(
				dataSource, 
				xField);
		}

		/// <summary>
		/// Automatically creates and binds series to specified data table. 
		/// Each column of the table becomes a Y value in a separate series.
		/// </summary>
		/// <param name="dataSource">Data source.</param>
		public void DataBindTable(IEnumerable dataSource)
		{
			this.chartPicture.DataBindTable(
				dataSource, 
				String.Empty);
		}

		/// <summary>
		/// Data bind chart to the table. Series will be automatically added to the chart depending on 
		/// the number of unique values in the seriesGroupByField column of the data source.
		/// Data source can be the Ole(SQL)DataReader, DataView, DataSet, DataTable or DataRow.
		/// </summary>
		/// <param name="dataSource">Data source.</param>
		/// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
		/// <param name="xField">Name of the field for X values.</param>
		/// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
		/// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public void DataBindCrossTable(
			IEnumerable dataSource, 
			string seriesGroupByField, 
			string xField, 
			string yFields, 
			string otherFields)
		{
			this.chartPicture.DataBindCrossTab(
				dataSource, 
				seriesGroupByField, 
				xField, 
				yFields, 
				otherFields,
				false,
				PointSortOrder.Ascending);
		}

		/// <summary>
		/// Data bind chart to the table. Series will be automatically added to the chart depending on
		/// the number of unique values in the seriesGroupByField column of the data source.
		/// Data source can be the Ole(SQL)DataReader, DataView, DataSet, DataTable or DataRow.
		/// </summary>
		/// <param name="dataSource">Data source.</param>
		/// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
		/// <param name="xField">Name of the field for X values.</param>
		/// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
		/// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
		/// <param name="sortingOrder">Series will be sorted by group field values in specified order.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public void DataBindCrossTable(
			IEnumerable dataSource, 
			string seriesGroupByField, 
			string xField, 
			string yFields, 
			string otherFields,
			PointSortOrder sortingOrder)
		{
			this.chartPicture.DataBindCrossTab(
				dataSource, 
				seriesGroupByField, 
				xField, 
				yFields, 
				otherFields,
				true,
				sortingOrder);
		}


		#endregion

		#region Special Extension Methods and Properties

		/// <summary>
		/// Gets the requested chart service.
		/// </summary>
		/// <param name="serviceType">Type of requested chart service.</param>
		/// <returns>Instance of the service or null if it can't be found.</returns>
		public object GetService(Type serviceType)
		{
            // Check arguments
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

			object service = null;
			if(serviceContainer != null)
			{
				service = serviceContainer.GetService(serviceType);
			}

			return service;
		}

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_PrePaint")]
        public event EventHandler<FormatNumberEventArgs> FormatNumber;

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        /// <param name="caller">Event caller. Can be ChartPicture, ChartArea or Legend objects.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected virtual void OnFormatNumber(object caller, FormatNumberEventArgs e)
        {
            if (FormatNumber != null)
            {
                FormatNumber(caller, e);
            }
        }

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        /// <param name="caller">Event caller. Can be ChartPicture, ChartArea or Legend objects.</param>
        /// <param name="e">Event arguments.</param>
        internal void CallOnFormatNumber(object caller, FormatNumberEventArgs e)
        {
            this.OnFormatNumber(caller, e);
        }

		#endregion

        #region HttpHandler Support

        /// <summary>
        /// Chart rendering type. Image tag, input tag, binary data streaming and image map are the options.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        Bindable(true),
        SRDescription("DescriptionAttributeChart_ImageStorageMode"),
        PersistenceModeAttribute(PersistenceMode.Attribute),
        DefaultValue(ImageStorageMode.UseHttpHandler)
        ]
        public ImageStorageMode ImageStorageMode
        {
            get
            {
                return this._imageStorageMode;
            }
            set
            {
                this._imageStorageMode = value;
            }
        }
        
        /// <summary>
        /// Gets the image storage mode.
        /// </summary>
        /// <returns></returns>
        internal ImageStorageMode GetImageStorageMode()
        {
            if (this.ImageStorageMode == ImageStorageMode.UseHttpHandler)
            {
                ChartHttpHandler.EnsureInstalled();
            }
            return this.ImageStorageMode;
        }

        #endregion //HttpHandler Support

        #region IPostBackEventHandler Members

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        #endregion

        #region IDisposable overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!String.IsNullOrEmpty(_designTimeChart))
                {
                    try
                    {
                        File.Delete(_designTimeChart);
                    }
                    catch (ArgumentException) { }
                    catch (DirectoryNotFoundException) { }
                    catch (IOException) { }
                    catch (NotSupportedException) { }
                    catch (UnauthorizedAccessException) { }
                }

                // Dispose managed objects here
                if (_imageLoader != null)
                {
                    _imageLoader.Dispose();
                    _imageLoader = null;
                }
                if (_namedImages != null)
                {
                    _namedImages.Dispose();
                    _namedImages = null;
                }
                if (_chartTypeRegistry != null)
                {
                    _chartTypeRegistry.Dispose();
                    _chartTypeRegistry = null;
                }
                if (serviceContainer != null)
                {
                    serviceContainer.Dispose();
                    serviceContainer = null;
                }
                if (_license != null)
                {
                    _license.Dispose();
                    _license = null;
                }
            }
            //Base dispose
            base.Dispose();
            if (disposing)
            {
                if (_dataManager != null)
                {
                    _dataManager.Dispose();
                    _dataManager = null;
                }
                if (chartPicture != null)
                {
                    chartPicture.Dispose();
                    chartPicture = null;
                }
            }
        }

        /// <summary>
        /// Disposing control resoursec
        /// </summary>
        public override sealed void Dispose()
        {
            Dispose(true);                        
            GC.SuppressFinalize(this);
        }

        #endregion

    }

	/// <summary>
	/// Chart map areas customize events arguments
	/// </summary>
#if ASPPERM_35
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class CustomizeMapAreasEventArgs : EventArgs
	{
		private	MapAreasCollection	_areaItems = null;

		/// <summary>
		/// Default construvtor is not accessible
		/// </summary>
		private CustomizeMapAreasEventArgs()
		{
		}

		/// <summary>
		/// Customize map area event arguments constructor
		/// </summary>
		/// <param name="areaItems">Legend items collection.</param>
		public CustomizeMapAreasEventArgs(MapAreasCollection areaItems)
		{
			this._areaItems = areaItems;
		}

		/// <summary>
		/// Legend items collection.
		/// </summary>
		public MapAreasCollection MapAreaItems
		{
			get
			{
				return _areaItems;
			}
		} 

	}



	/// <summary>
	/// Chart legend customize events arguments
	/// </summary>
#if ASPPERM_35
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif 
    public class CustomizeLegendEventArgs : EventArgs
	{
		private	LegendItemsCollection	_legendItems = null;
		private	string					_legendName = "";

		/// <summary>
		/// Default construvtor is not accessible
		/// </summary>
		private CustomizeLegendEventArgs()
		{
		}

		/// <summary>
		/// Customize legend event arguments constructor
		/// </summary>
		/// <param name="legendItems">Legend items collection.</param>
		public CustomizeLegendEventArgs(LegendItemsCollection legendItems)
		{
			this._legendItems = legendItems;
		}

		/// <summary>
		/// Customize legend event arguments constructor
		/// </summary>
		/// <param name="legendItems">Legend items collection.</param>
		/// <param name="legendName">Legend name.</param>
		public CustomizeLegendEventArgs(LegendItemsCollection legendItems, string legendName)
		{
			this._legendItems = legendItems;
			this._legendName = legendName;
		}

		/// <summary>
		/// Legend name.
		/// </summary>
		public string LegendName
		{
			get
			{
				return _legendName;
			}
		} 

		/// <summary>
		/// Legend items collection.
		/// </summary>
		public LegendItemsCollection LegendItems
		{
			get
			{
				return _legendItems;
			}
		} 

	}

    /// <summary>
    /// Specifies a value indicating whether the text appears from right to left, such as when using Hebrew or Arabic fonts
    /// </summary>
    public enum RightToLeft
    {
        /// <summary>
        /// The text reads from left to right. This is the default.
        /// </summary>
        No,
        /// <summary>
        /// The text reads from right to left.
        /// </summary>
        Yes,
        /// <summary>
        /// Not used
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        Inherit = No
    }
}
