//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartSerializer.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartSerializer
//
//  Purpose:	Serialization saves the state of the chart and also 
//              provides the ability to load the serialized data back
//              into the chart. All chart properties can be persisted, 
//              including the chart's data.
//
//              ChartSerializer class only provides serialization API
//              for the user and actual serialization is performed by
//              XmlFormatSerializer or BinaryFormatserializer classes
//              depending on the Format property.
//
//	Reviewed:	AG - Jul 31, 2002
//              GS - Aug 7, 2002
//              AG - Microsoft 15, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region Serialization enumeration

	/// <summary>
	/// An enumeration of the formats of the chart serializer.
	/// </summary>
	public enum SerializationFormat
	{
		/// <summary>
		/// XML serializer format.
		/// </summary>
		Xml,

		/// <summary>
		/// Binary serializer format.
		/// </summary>
		Binary
	}


	/// <summary>
	/// An enumeration of chart serializable content definition flags
	/// </summary>
	[Flags]
	public enum SerializationContents 
	{
		/// <summary>
		/// Default content.
		/// </summary>
		Default = 1,

		/// <summary>
		/// Serialize only series data.
		/// </summary>
		Data = 2,

		/// <summary>
		/// Serialize chart visual appearance (e.g. Colors, Line Styles).
		/// </summary>
		Appearance = 4,

		/// <summary>
		/// All content is serialized. 
		/// </summary>
		All = Default | Data | Appearance

	}

	#endregion

	/// <summary>
    /// ChartSerializer class provides chart serialization.
	/// </summary>
	[
		SRDescription("DescriptionAttributeChartSerializer_ChartSerializer"),
		DefaultProperty("Format"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartSerializer 
	{
		#region Private fields

		// Reference to the service container
		private IServiceContainer		_serviceContainer = null;

		// Reference to the chart object
		private Chart					_chart = null;

		// Reference to the serializer object

		private SerializerBase			_serializer = new XmlFormatSerializer();

		// Format of the serializer in use
		private SerializationFormat		_format = SerializationFormat.Xml;

		// Serialization content 
		private SerializationContents 	_content  = SerializationContents .Default;

		#endregion

		#region Constructors and Service Provider methods

		/// <summary>
		/// Default constructor is unavailable
		/// </summary>
		private ChartSerializer()
		{
		}

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <param name="container">Service container reference.</param>
        internal ChartSerializer(IServiceContainer container)
		{
			if(container == null)
			{
				throw(new ArgumentNullException(SR.ExceptionInvalidServiceContainer));
			}
			_serviceContainer = container;
		}

		/// <summary>
		/// Returns ChartSerializer service object
		/// </summary>
		/// <param name="serviceType">Requested service type.</param>
		/// <returns>ChartSerializer service object.</returns>
		internal object GetService(Type serviceType)
		{
			if(serviceType == typeof(ChartSerializer))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionChartSerializerUnsupportedType( serviceType.ToString())));
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the serializable content.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(typeof(SerializationContents ), "Default"),
		SRDescription("DescriptionAttributeChartSerializer_Content")
		]
		public SerializationContents  Content
		{
			get
			{
				return _content;
			}
			set
			{
				// Set content value
				_content = value;

				// Automatically set SerializableContent and NonSerializableContent properties
				SetSerializableContent();
			}
		}

		/// <summary>
		/// Gets or sets the format used to serialize the chart data.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(typeof(SerializationFormat), "Xml"),
		SRDescription("DescriptionAttributeChartSerializer_Format")
		]
		public SerializationFormat Format
		{
			get
			{
				return _format;
			}
			set
			{
				if(_format != value)
				{
					_format = value;

					// Create new serializer object
					SerializerBase newSerializer = null;

					if(_format == SerializationFormat.Binary)
					{
						newSerializer = new BinaryFormatSerializer();
					}
					else
					{
						newSerializer = new XmlFormatSerializer();
					}
					// Copy serializer settings
					newSerializer.IsUnknownAttributeIgnored = _serializer.IsUnknownAttributeIgnored;
					newSerializer.NonSerializableContent = _serializer.NonSerializableContent;
					newSerializer.IsResetWhenLoading = _serializer.IsResetWhenLoading;
					newSerializer.SerializableContent = _serializer.SerializableContent;
					_serializer = newSerializer;
				}
			}
		}

		/// <summary>
		/// Gets or sets a flag which indicates whether object properties are reset to default
		/// values before loading.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(true),
		SRDescription("DescriptionAttributeChartSerializer_ResetWhenLoading")
		]
		public bool IsResetWhenLoading
		{
			get
			{
				return _serializer.IsResetWhenLoading;
			}
			set
			{
				_serializer.IsResetWhenLoading = value;
			}
		}



		/// <summary>
		/// Gets or sets a flag which indicates whether unknown XML properties and elements will be 
		/// ignored without throwing an exception.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeChartSerializer_IgnoreUnknownXmlAttributes")
		]
		public bool IsUnknownAttributeIgnored
		{
			get
			{
				return _serializer.IsUnknownAttributeIgnored;
			}
			set
			{
				_serializer.IsUnknownAttributeIgnored = value;
			}
		}

		/// <summary>
		/// Gets or sets a flag which indicates whether chart 
        /// serializer is working in template creation mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeChartSerializer_TemplateMode")
		]
		public bool IsTemplateMode
		{
			get
			{
				return _serializer.IsTemplateMode;
			}
			set
			{
				_serializer.IsTemplateMode = value;
			}
		}



		/// <summary>
        /// Gets or sets the chart properties that can be serialized.
        /// Comma separated list of serializable (Save/Load/Reset) properties. 
		/// "ClassName.PropertyName,[ClassName.PropertyName]".
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(""),
		SRDescription("DescriptionAttributeChartSerializer_SerializableContent")
		]
		public string SerializableContent
		{
			get
			{
				return _serializer.SerializableContent;
			}
			set
			{
				_serializer.SerializableContent = value;
			}
		}

		/// <summary>
        /// Gets or sets the chart properties that will not be serialized.
		/// Comma separated list of non-serializable (Save/Load/Reset) properties. 
		/// "ClassName.PropertyName,[ClassName.PropertyName]".
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(""),
		SRDescription("DescriptionAttributeChartSerializer_NonSerializableContent")
		]
		public string NonSerializableContent
		{
			get
			{
				return _serializer.NonSerializableContent;
			}
			set
			{
				_serializer.NonSerializableContent = value;
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// This method resets all properties of the chart to default values. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be reset.
		/// </summary>
		public void Reset()
		{
			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Resetting;
			}

			// Reset properties
			_serializer.ResetObjectProperties(GetChartObject());

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
			}
		}

		/// <summary>
		/// This method saves all properties of the chart into a file. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be saved.
		/// </summary>
		/// <param name="fileName">The file name used to write the data.</param>
		public void Save(string fileName)
		{
            //Check arguments
            if (fileName == null)
                throw new ArgumentNullException("fileName");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Saving;
				//GetChartObject().BeginInit();
			}

			// Reset all auto-detected properties values
			GetChartObject().ResetAutoValues();

			// Serialize chart data
			_serializer.Serialize(GetChartObject(), fileName);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
				//GetChartObject().EndInit();
			}
		}

		/// <summary>
		/// This method saves all properties of the chart into a stream.  By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be saved.
		/// </summary>
		/// <param name="stream">The stream where to save the data.</param>
		public void Save(Stream stream)
		{
            //Check arguments
            if (stream == null)
                throw new ArgumentNullException("stream");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Saving;
				//GetChartObject().BeginInit();
			}

			// Reset all auto-detected properties values
			GetChartObject().ResetAutoValues();

			// Serialize chart data
			_serializer.Serialize(GetChartObject(), stream);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
				//GetChartObject().EndInit();
			}
		}

		/// <summary>
		/// This method saves all properties of the chart into an XML writer. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be saved.
		/// </summary>
		/// <param name="writer">XML writer to save the data.</param>
		public void Save(XmlWriter writer)
		{
            //Check arguments
            if (writer == null)
                throw new ArgumentNullException("writer");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Saving;
				//GetChartObject().BeginInit();
			}

			// Reset all auto-detected properties values
			GetChartObject().ResetAutoValues();

			// Serialize chart data
			_serializer.Serialize(GetChartObject(), writer);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
				//GetChartObject().EndInit();
			}
		}

		/// <summary>
		/// This method saves all properties of the chart into a text writer.  By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be saved.
		/// </summary>
		/// <param name="writer">Text writer to save the data.</param>
		public void Save(TextWriter writer)
		{
            //Check arguments
            if (writer == null)
                throw new ArgumentNullException("writer");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Saving;
				//GetChartObject().BeginInit();
			}

			// Reset all auto-detected properties values
			GetChartObject().ResetAutoValues();

			// Serialize chart data
			_serializer.Serialize(GetChartObject(), writer);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
				//GetChartObject().EndInit();
			}
		}

		/// <summary>
		/// This method loads all properties of the chart from a file. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be loaded.
		/// </summary>
		/// <param name="fileName">The file to load the data from.</param>
		public void Load(string fileName)
		{
            //Check arguments
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            
            // Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Loading;
			}

			_serializer.Deserialize(GetChartObject(), fileName);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
			}
		}


		/// <summary>
		/// This method loads all properties of the chart from a stream. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be loaded.
		/// </summary>
		/// <param name="stream">The stream to load the data from.</param>
		public void Load(Stream stream)
		{
            //Check arguments
            if (stream == null)
                throw new ArgumentNullException("stream");
            
            // Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Loading;
			}

			_serializer.Deserialize(GetChartObject(), stream);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
			}
		}

		/// <summary>
		/// This method loads all properties of the chart from an XML reader. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be loaded.
		/// </summary>
		/// <param name="reader">The XML reader to load the data from.</param>
		public void Load(XmlReader reader)
		{
            //Check arguments
            if (reader == null)
                throw new ArgumentNullException("reader");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Loading;
			}

			_serializer.Deserialize(GetChartObject(), reader);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
			}
		}

		/// <summary>
		/// This method loads all properties of the chart from the text reader. By setting Content or 
		/// SerializableContent/NonSerializableContent properties, specific set of 
		/// properties can be loaded.
		/// </summary>
		/// <param name="reader">The text reader to load the data from.</param>
		public void Load(TextReader reader)
		{
            //Check arguments
            if (reader == null)
                throw new ArgumentNullException("reader");

			// Set serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = true;
                GetChartObject().serializationStatus = SerializationStatus.Loading;
			}

			_serializer.Deserialize(GetChartObject(), reader);

			// Clear serializing flag
			if(GetChartObject() != null)
			{
				GetChartObject().serializing = false;
                GetChartObject().serializationStatus = SerializationStatus.None;
			}
		}

		#endregion

		#region Protected helper methods

		/// <summary>
		/// Sets SerializableContent and NonSerializableContent properties
		/// depending on the flags in the Content property.
		/// </summary>
		internal void SetSerializableContent()
		{
			// Reset content definition strings
			this.SerializableContent = "";
			this.NonSerializableContent = "";

			// Loop through all enumeration flags
			Array	enumValues = Enum.GetValues(typeof(SerializationContents ));
			foreach(object flagObject in enumValues)
			{
				if(flagObject is SerializationContents )
				{
					// Check if flag currently set
					SerializationContents 	flag = (SerializationContents )flagObject;
					if((this.Content & flag) == flag && 
						flag != SerializationContents .All &&
						this.Content != SerializationContents .All)
					{
						// Add comma at the end of existing string
						if(this.NonSerializableContent.Length != 0)
						{
							this.NonSerializableContent += ", ";
						}

						// Add serializable class/properties names
						this.NonSerializableContent += GetContentString(flag, false);
						this.NonSerializableContent = this.NonSerializableContent.TrimStart(',');

						// Add comma at the end of existing string
						if(this.SerializableContent.Length != 0)
						{
							this.SerializableContent += ", ";
						}

						// Add serializable class/properties names
						this.SerializableContent += GetContentString(flag, true);
						this.SerializableContent = this.SerializableContent.TrimStart(',');
					}
				}
			}
		}


		/// <summary>
		/// Return a serializable or non serializable class/properties names
		/// for the specific flag.
		/// </summary>
        /// <param name="content">Serializable content</param>
		/// <param name="serializable">True - get serializable string, False - non serializable.</param>
		/// <returns>Serializable or non serializable string with class/properties names.</returns>
		protected string GetContentString(SerializationContents  content, bool serializable)
		{
			switch(content)
			{
				case(SerializationContents .All):
					return "";
				case(SerializationContents .Default):
					return "";
				case(SerializationContents .Data):
					if(serializable)
					{
						return	
							"Chart.BuildNumber, " +
							"Chart.Series, " +
							"Series.Points, " +
							"Series.Name, " +
							"DataPoint.XValue, " +
							"DataPoint.YValues," +
							"DataPoint.LabelStyle," +
							"DataPoint.AxisLabel," +
							"DataPoint.LabelFormat," +
							"DataPoint.IsEmpty, " +
							"Series.YValuesPerPoint, " +
							"Series.IsXValueIndexed, " + 
							"Series.XValueType, " +
							"Series.YValueType";
					}
					return "";
				case(SerializationContents .Appearance):
					if(serializable)
					{
						return 
							"Chart.BuildNumber, " +
                            "*.Name*, " +
                            "*.Fore*, " +
							"*.Back*, " +
							"*.Border*, " +
							"*.Line*, " +
							"*.Frame*, " +
							"*.PageColor*, " +
							"*.SkinStyle*, " +
							"*.Palette, " +
							"*.PaletteCustomColors, " +
							"*.Font*, " +
							"*.*Font, " +
							"*.Color, " +
							"*.Shadow*, " +
							"*.MarkerColor, " +
							"*.MarkerStyle, " +
							"*.MarkerSize, " +
							"*.MarkerBorderColor, " +
							"*.MarkerImage, " +
							"*.MarkerImageTransparentColor, " +
							"*.LabelBackColor, " +
							"*.LabelBorder*, " +
							"*.Enable3D, " +
							"*.IsRightAngleAxes, " +
							"*.IsClustered, " +
							"*.LightStyle, " +
							"*.Perspective, " +
							"*.Inclination, " +
							"*.Rotation, " +
							"*.PointDepth, " +
							"*.PointGapDepth, " +
							"*.WallWidth";
					}
					return ""; 

				default:
                    throw (new InvalidOperationException(SR.ExceptionChartSerializerContentFlagUnsupported));
			}
		}

		/// <summary>
		/// Returns chart object for serialization.
		/// </summary>
		/// <returns>Chart object.</returns>
        internal Chart GetChartObject()
		{
			if(_chart == null)
			{
				_chart = (Chart)_serviceContainer.GetService(typeof(Chart));
			}
			return _chart;
		}

		#endregion
	}
}
