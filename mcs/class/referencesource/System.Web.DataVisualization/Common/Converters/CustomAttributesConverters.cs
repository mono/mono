//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		CustomattributesConverter.cs
//
//  Namespace:	DataVisualization.Charting.Design
//
//  Interfaces: IDataPointCustomPropertiesProvider
//
//	Classes:	CustomPropertiesTypeConverter, DynamicPropertyDescriptor
//
//  Purpose:	AxisName converter of the design-time CustomProperties
//				property object.
//
//	Reviewed:	
//
//===================================================================

#region Used Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
    using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
	/// <summary>
	/// Custom properties object type converter.
	/// </summary>
	internal class CustomPropertiesTypeConverter : TypeConverter
	{
		#region String to/from convertion methods

		/// <summary>
		/// Overrides the CanConvertFrom method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="sourceType">Convertion source type.</param>
		/// <returns>Indicates if convertion is possible.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Overrides the CanConvertTo method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Indicates if convertion is possible.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if(destinationType == typeof(CustomProperties))
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Overrides the ConvertTo method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="destinationType">Convertion destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) 
		{  
			if (destinationType == typeof(string)) 
			{
				return ((CustomProperties)value).DataPointCustomProperties.CustomProperties;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Overrides the ConvertFrom method of TypeConverter.
		/// Converts from string with comma separated values.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert from.</param>
		/// <returns>Indicates if convertion is possible.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "Too large of a code change to justify making this change")]
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) 
		{
            string stringValue = value as string;
			if(stringValue != null && context != null && context.Instance != null) 
			{
				// Create new custom attribute class with a reference to the DataPointCustomProperties
				if(context.Instance is DataPointCustomProperties)
				{
                    ((DataPointCustomProperties)context.Instance).CustomProperties = stringValue;
					CustomProperties newAttributes = new CustomProperties(((DataPointCustomProperties)context.Instance));
					return newAttributes;
				}

				else if (context.Instance is CustomProperties)
				{
                    CustomProperties newAttributes = new CustomProperties(((CustomProperties)context.Instance).DataPointCustomProperties);
					return newAttributes;
				}
                else if (context.Instance is IDataPointCustomPropertiesProvider)
                {
                    CustomProperties newAttributes = new CustomProperties(((IDataPointCustomPropertiesProvider)context.Instance).DataPointCustomProperties);
                    return newAttributes;
                }

				else if (context.Instance is Array)
				{
					DataPointCustomProperties attributes = null;
					foreach (object obj in ((Array)context.Instance))
					{
						if (obj is DataPointCustomProperties)
						{
							attributes = (DataPointCustomProperties)obj;
                            attributes.CustomProperties = stringValue;
						}
					}
					if (attributes != null)
					{
						CustomProperties newAttributes = new CustomProperties(attributes);
						return newAttributes;
					}
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
	
		#endregion // String to/from convertion methods

		#region Property Descriptor Collection methods

		/// <summary>
		/// Returns whether this object supports properties.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <returns>true if GetProperties should be called to find the properties of this object; otherwise, false.</returns>
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Returns a collection of properties for the type of array specified by the value parameter,
		/// using the specified context and properties.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="obj">An Object that specifies the type of array for which to get properties.</param>
        /// <param name="attributes">An array of type Attribute that is used as a filter.</param>
		/// <returns>A PropertyDescriptorCollection with the properties that are exposed for this data type, or a null reference (Nothing in Visual Basic) if there are no properties.</returns>
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object obj, Attribute[] attributes)
		{
			PropertyDescriptorCollection propCollection = new PropertyDescriptorCollection(null);
			CustomProperties attr = obj as CustomProperties;
			if(attr != null && context != null)
			{
				// Get series associated with custom attribute
				Series series = (attr.DataPointCustomProperties is Series) ? ( (Series) attr.DataPointCustomProperties) : attr.DataPointCustomProperties.series;
				if(series != null && 
					series.Common != null)
				{
					// Loop through all registered custom properties
					CustomPropertyRegistry registry = (CustomPropertyRegistry)series.Common.container.GetService(typeof(CustomPropertyRegistry));
					foreach(CustomPropertyInfo attrInfo in registry.registeredCustomProperties)
					{
						// Check if attribute description matches curent selection in property browser
						if(IsApplicableCustomProperty(attrInfo, context.Instance))
						{
							// Get array of property properties
							Attribute[] propAttributes = GetPropertyAttributes(attrInfo);

							// Create property descriptor
							CustomAttributesPropertyDescriptor propertyDescriptor = new CustomAttributesPropertyDescriptor(
								typeof(CustomProperties),
								attrInfo.Name, 
								attrInfo.ValueType,
								propAttributes,
								attrInfo);

							// Add descriptor into the collection
							propCollection.Add(propertyDescriptor);
						}
					}

					// Always add "UserDefined" property for all user defined custom properties
					Attribute[] propUserDefinedAttributes = new Attribute[] {
							new NotifyParentPropertyAttribute(true),
							new RefreshPropertiesAttribute(RefreshProperties.All),
							new DescriptionAttribute(SR.DescriptionAttributeUserDefined)
						};

					// Create property descriptor
					CustomAttributesPropertyDescriptor propertyUserDefinedDescriptor = new CustomAttributesPropertyDescriptor(
						typeof(CustomProperties),
						"UserDefined", 
						typeof(string),
						propUserDefinedAttributes,
						null);

					// Add descriptor into the collection
					propCollection.Add(propertyUserDefinedDescriptor);
				}
			}

			return propCollection;
		}

		/// <summary>
		/// Checks if provided custom attribute appies to the selected points or series.
		/// </summary>
		/// <param name="attrInfo">Custom attribute information.</param>
		/// <param name="obj">Selected series or points.</param>
		/// <returns>True if custom attribute applies.</returns>
		private bool IsApplicableCustomProperty(CustomPropertyInfo attrInfo, object obj)
		{

            CustomProperties customProperties = obj as CustomProperties;
			if (customProperties != null)
			{
                obj = customProperties.DataPointCustomProperties;
			}

			// Check if custom attribute applies to the series or points
			if( (IsDataPoint(obj) && attrInfo.AppliesToDataPoint) ||
				(!IsDataPoint(obj) && attrInfo.AppliesToSeries) )
			{
				// Check if attribute do not apply to 3D or 2D chart types
				if( (Is3DChartType(obj) && attrInfo.AppliesTo3D) ||
					(!Is3DChartType(obj) && attrInfo.AppliesTo2D) )
				{

					// Check if custom attribute applies to the chart types selected
					SeriesChartType[] chartTypes = GetSelectedChartTypes(obj);
					foreach(SeriesChartType chartType in chartTypes)
					{
						foreach(SeriesChartType attrChartType in attrInfo.AppliesToChartType)
						{
							if(attrChartType == chartType)
							{
								return true;
							}
						}
					}

				}
			}

			return false;
		}

		/// <summary>
		/// Checks if specified object represent a single or array of data points.
		/// </summary>
		/// <param name="obj">Object to test.</param>
		/// <returns>True if specified object contains one or more data points.</returns>
		private bool IsDataPoint(object obj)
		{
            Series series = obj as Series;
			if(series != null)
			{
				return false;
			}

            Array array = obj as Array;
			if(array != null && array.Length > 0)
			{
                if (array.GetValue(0) is Series)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if specified object represent a single or array of data points.
		/// </summary>
		/// <param name="obj">Object to test.</param>
		/// <returns>True if specified object contains one or more data points.</returns>
		private bool Is3DChartType(object obj)
		{
			// Get array of series
			Series[] seriesArray = GetSelectedSeries(obj);
			
			// Loop through all series and check if its plotted on 3D chart area
			foreach(Series series in seriesArray)
			{
				ChartArea chartArea = series.Chart.ChartAreas[series.ChartArea];
				if(chartArea.Area3DStyle.Enable3D)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get array of selected series.
		/// </summary>
		/// <param name="obj">Selected objects.</param>
		/// <returns>Selected series array.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "Too large of a code change to justify making this change")]
		private Series[] GetSelectedSeries(object obj)
		{
			// Get array of series
			Series[] seriesArray = new Series[0];
			if(obj is Array && ((Array)obj).Length > 0)
			{
				if(((Array)obj).GetValue(0) is Series)
				{
					seriesArray = new Series[((Array)obj).Length];
					((Array)obj).CopyTo(seriesArray, 0);
				}
				else if(((Array)obj).GetValue(0) is DataPointCustomProperties)
				{
					seriesArray = new Series[] { ((DataPointCustomProperties)((Array)obj).GetValue(0)).series };
				}
			}
			else if(obj is Series)
			{
				seriesArray = new Series[] { ((Series)obj) };
			}
			else if(obj is DataPointCustomProperties)
			{
				seriesArray = new Series[] { ((DataPointCustomProperties)obj).series };
			}

			return seriesArray;
		}

		/// <summary>
		/// Get array of chart types from the selected series.
		/// </summary>
		/// <param name="obj">Selected series or data points.</param>
		/// <returns>Array of selected chart types.</returns>
		private SeriesChartType[] GetSelectedChartTypes(object obj)
		{
			// Get array of series
			Series[] seriesArray = GetSelectedSeries(obj);

			// Create array of chart types
			int index = 0;
			SeriesChartType[] chartTypes = new SeriesChartType[seriesArray.Length];
			foreach(Series series in seriesArray)
			{
				chartTypes[index++] = series.ChartType;
			}

			return chartTypes;
		}

		/// <summary>
		/// Gets array of properties for the dynamic property.
		/// </summary>
		/// <param name="attrInfo">Custom attribute information.</param>
		/// <returns>Array of properties.</returns>
		private Attribute[] GetPropertyAttributes(CustomPropertyInfo attrInfo)
		{
			// Create default value attribute
			DefaultValueAttribute defaultValueAttribute = null;
            if (attrInfo.DefaultValue.GetType() == attrInfo.ValueType)
            {
                defaultValueAttribute = new DefaultValueAttribute(attrInfo.DefaultValue);
            }
            else if (attrInfo.DefaultValue is string)
            {
                defaultValueAttribute = new DefaultValueAttribute(attrInfo.ValueType, (string)attrInfo.DefaultValue);
            }
            else
            {
                throw (new InvalidOperationException(SR.ExceptionCustomAttributeDefaultValueTypeInvalid));
            }
			// Add all properties into the list
			ArrayList propList = new ArrayList();

			propList.Add(new NotifyParentPropertyAttribute(true));
			propList.Add(new RefreshPropertiesAttribute(RefreshProperties.All));
			propList.Add(new DescriptionAttribute(attrInfo.Description));
			propList.Add(defaultValueAttribute);

            if (attrInfo.Name.Equals(CustomPropertyName.ErrorBarType, StringComparison.Ordinal))
            {
                propList.Add(new TypeConverterAttribute(typeof(ErrorBarTypeConverter)));
            }

			// Convert list to array
			int index = 0;
			Attribute[] propAttributes = new Attribute[propList.Count];
			foreach(Attribute attr in propList)
			{
				propAttributes[index++] = attr;
			}
			return propAttributes;
		}

        /// <summary>
        /// Special convertor for ErrorBarType custom attribute
        /// </summary>
        internal class ErrorBarTypeConverter : StringConverter
        {
            /// <summary>
            /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <returns>
            /// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> should be called to find a common set of values the object supports; otherwise, false.
            /// </returns>
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            /// <summary>
            /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exclusive list of possible values, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <returns>
            /// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exhaustive list of possible values; false if other values are possible.
            /// </returns>
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }
            
            /// <summary>
            /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
            /// <returns>
            /// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
            /// </returns>
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                ArrayList result = new ArrayList();
                foreach (ChartTypes.ErrorBarType item in Enum.GetValues(typeof(ChartTypes.ErrorBarType)))
                {
                    string itemStr = String.Format(CultureInfo.InvariantCulture, "{0}({1:N0})", item, ChartTypes.ErrorBarChart.DefaultErrorBarTypeValue(item));
                    result.Add(itemStr);
                }
                return new StandardValuesCollection(result);
            }
        }

		#endregion // Property Descriptor Collection methods

		#region Custom Attributes Property Descriptor

		/// <summary>
		/// Custom properties inner property descriptor class.
		/// </summary>
		protected class CustomAttributesPropertyDescriptor : TypeConverter.SimplePropertyDescriptor 
		{
			#region Fields

			// Property name
			private	string					_name = string.Empty;

			// Custom attribute information
			private	CustomPropertyInfo		_customAttributeInfo = null;

			#endregion // Fields

			#region Constructor

			/// <summary>
			/// Property descriptor constructor.
			/// </summary>
			/// <param name="componentType">Component type.</param>
			/// <param name="name">Property name.</param>
			/// <param name="propertyType">Property type.</param>
            /// <param name="attributes">Property attributes.</param>
            /// <param name="customAttributeInfo">Custom attribute information.</param>
			internal CustomAttributesPropertyDescriptor(  
				Type componentType,
				string name,
				Type propertyType,
				Attribute[] attributes,
				CustomPropertyInfo customAttributeInfo) 
				: base(componentType, name, propertyType, attributes)
			{
				this._name = name;
				this._customAttributeInfo = customAttributeInfo;
			}

			#endregion // Constructor

            #region Methods

            /// <summary>
			/// Gets the current value of the property on a component.
			/// </summary>
			/// <param name="component">The component with the property for which to retrieve the value.</param>
			/// <returns>The value of a property for a given component.</returns>
			public override object GetValue(object component)
			{
				// "UserDefined" property expose comma separated user defined properties
				CustomProperties customAttr = component as CustomProperties;
				if(this._name == "UserDefined")
				{
					return customAttr.GetUserDefinedCustomProperties();
				}
				else
				{
					object val = null;

					// Check if custom attribute with this name is set
					string stringValue = customAttr.DataPointCustomProperties[this._name];
					if(this._customAttributeInfo != null)
					{
						if(stringValue == null || stringValue.Length == 0)
						{
							val = GetValueFromString(this._customAttributeInfo.DefaultValue);
						}
						else
						{
							val = GetValueFromString(stringValue);
						}
					}
					else
					{
						val = stringValue;
					}
					
					return val;
				}
			}

			/// <summary>
			/// Sets the value of the component to a different value.
			/// </summary>
			/// <param name="component">The component with the property value that is to be set.</param>
			/// <param name="value">The new value.</param>
			public override void SetValue(object component, object value)
			{
				// Validate new value
				ValidateValue(this._name, value);

				// Get new value as string
				string stringValue = GetStringFromValue(value);

				// "UserDefined" property expose comma separated user defined properties
				CustomProperties customAttr = component as CustomProperties;
				if( this._name == "UserDefined" )
				{
					customAttr.SetUserDefinedAttributes(stringValue);
				}
				else
				{
					// Check if the new value is the same as DefaultValue
					bool setAttributeValue = true;
					if( IsDefaultValue(stringValue) )
					{
						// Remove custom properties with default values from data point
						// only when series do not have this attribute set.
						if( !(customAttr.DataPointCustomProperties is DataPoint) ||
							!((DataPoint)customAttr.DataPointCustomProperties).series.IsCustomPropertySet(this._name) ) 
						{
							// Delete attribute
							if(customAttr.DataPointCustomProperties.IsCustomPropertySet(this._name))
							{
								customAttr.DataPointCustomProperties.DeleteCustomProperty(this._name);
								setAttributeValue = false;
							}
						}
					}

					// Set custom attribute value
					if( setAttributeValue )
					{
						customAttr.DataPointCustomProperties[this._name] = stringValue;
					}
				}
				customAttr.DataPointCustomProperties.CustomProperties = customAttr.DataPointCustomProperties.CustomProperties;

                IChangeTracking changeTracking = component as IChangeTracking;
                if (changeTracking != null)
                {
                    changeTracking.AcceptChanges();
                }

			}

			/// <summary>
			/// Checks if specified value is the default value of the attribute.
			/// </summary>
			/// <param name="val">Value to check.</param>
			/// <returns>True if specified value is the default attribute value.</returns>
			public bool IsDefaultValue(string val)
			{
				// Get default value string
				string defaultValue = GetStringFromValue(this._customAttributeInfo.DefaultValue);
				return (String.Compare(val, defaultValue, StringComparison.Ordinal) == 0);
			}

			/// <summary>
			/// Gets value from string a native type of attribute.
			/// </summary>
			/// <param name="obj">Object to convert to string.</param>
			/// <returns>String representation of the specified object.</returns>
			public virtual object GetValueFromString(object obj)
			{
				object result = null;
				if(obj != null)
				{
					if(this._customAttributeInfo.ValueType == obj.GetType() )
					{
						return obj;
					}

                    string stringValue = obj as string;
                    if (stringValue != null)
					{
						if(this._customAttributeInfo.ValueType == typeof(string) )
						{
							result = stringValue;
						}
						else if(this._customAttributeInfo.ValueType == typeof(float) )
						{
							result = float.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
						}
						else if(this._customAttributeInfo.ValueType == typeof(double) )
						{
							result = double.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
						}
						else if(this._customAttributeInfo.ValueType == typeof(int) )
						{
							result = int.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
						}
						else if(this._customAttributeInfo.ValueType == typeof(bool) )
						{
							result = bool.Parse(stringValue);
						}
						else if(this._customAttributeInfo.ValueType == typeof(Color) )
						{
							ColorConverter colorConverter = new ColorConverter();
							result = (Color)colorConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, stringValue);
						}
						else if(this._customAttributeInfo.ValueType.IsEnum)
						{
							result = Enum.Parse(this._customAttributeInfo.ValueType, stringValue, true);
						}
						else
						{
                            throw (new InvalidOperationException(SR.ExceptionCustomAttributeTypeUnsupported( this._customAttributeInfo.ValueType.ToString() )));
						}

					}
				}
				return result;
			}


			/// <summary>
			/// Converts attribute value to string.
			/// </summary>
			/// <param name="value">Attribute value to convert.</param>
			/// <returns>Return attribute value converted to string.</returns>
			public string GetStringFromValue(object value)
			{
				if(value is Color)
				{
					ColorConverter colorConverter = new ColorConverter();
					return colorConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, value);
				}
				else if(value is float)
				{
					return ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
				else if(value is double)
				{
					return ((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
				else if(value is int)
				{
					return ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
				else if(value is bool)
				{
					return ((bool)value).ToString();
				}

				return value.ToString();
			}

			/// <summary>
			/// Validates attribute value. Method throws exception in case of any issues.
			/// </summary>
			/// <param name="attrName">Attribute name.</param>
			/// <param name="value">Attribute value to validate.</param>
            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
                Justification = "Too large of a code change to justify making this change")]
			public virtual void ValidateValue(string attrName, object value)
			{
				// Check for validation rules
				if(this._customAttributeInfo == null)
				{
					return;
				}

				// Check if property Min/Max value is provided
				bool	outOfRange = false;
				if(this._customAttributeInfo.MaxValue != null)
				{
					if(value.GetType() != this._customAttributeInfo.MaxValue.GetType())
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeTypeOrMaximumPossibleValueInvalid( attrName ) ) );
					}

					if(value is float)
					{
						if((float)value > (float)this._customAttributeInfo.MaxValue)
						{
							outOfRange = true;
						}
					}
					else if(value is double)
					{
						if((double)value > (double)this._customAttributeInfo.MaxValue)
						{
							outOfRange = true;
						}
					}
					else if(value is int)
					{
						if((int)value > (int)this._customAttributeInfo.MaxValue)
						{
							outOfRange = true;
						}
					}
					else
					{
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeTypeOrMinimumPossibleValueUnsupported(attrName)));
					}

				}
			
				// Check if property Min value is provided
				if(this._customAttributeInfo.MinValue != null)
				{
					if(value.GetType() != this._customAttributeInfo.MinValue.GetType())
					{
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeTypeOrMinimumPossibleValueInvalid( attrName ) ) );
					}
					
					if(value is float)
					{
						if((float)value < (float)this._customAttributeInfo.MinValue)
						{
							outOfRange = true;
						}
					}
					else if(value is double)
					{
						if((double)value < (double)this._customAttributeInfo.MinValue)
						{
							outOfRange = true;
						}
					}
					else if(value is int)
					{
						if((int)value < (int)this._customAttributeInfo.MinValue)
						{
							outOfRange = true;
						}
					}
					else
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeTypeOrMinimumPossibleValueUnsupported(attrName)));
					}
				}

				// Value out of range exception
				if(outOfRange)
				{
					if(this._customAttributeInfo.MaxValue != null && this._customAttributeInfo.MinValue != null)
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeMustBeInRange(attrName, this._customAttributeInfo.MinValue.ToString(),this._customAttributeInfo.MaxValue.ToString() )));
					}
					else if(this._customAttributeInfo.MinValue != null)
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeMustBeBiggerThenValue(attrName, this._customAttributeInfo.MinValue.ToString())));
					}
					else if(this._customAttributeInfo.MaxValue != null)
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeMustBeMoreThenValue(attrName, this._customAttributeInfo.MaxValue.ToString())));
					}
				}
			}

			#endregion // Methods
		}

		#endregion // Custom Attributes Property Descriptor
	}


	/// <summary>
	/// Property descriptor with ability to dynamically change properties 
	/// of the base property descriptor object.
	/// </summary>
	internal class DynamicPropertyDescriptor : PropertyDescriptor
	{
		#region Fields

		// Reference to the base property descriptor
		private PropertyDescriptor	_basePropertyDescriptor = null; 

		// Dynamic display name of the property
		private string				_displayName = string.Empty;

		#endregion // Fields 

		#region Constructor

		/// <summary>
		/// Constructor of the dynamic property descriptor.
		/// </summary>
		/// <param name="basePropertyDescriptor">Base property descriptor.</param>
		/// <param name="displayName">New display name of the property.</param>
		public DynamicPropertyDescriptor(
			PropertyDescriptor basePropertyDescriptor, 
			string displayName)
			: base(basePropertyDescriptor)
		{
			this._displayName = displayName;
			this._basePropertyDescriptor = basePropertyDescriptor;
		}

		#endregion // Constructor

		#region Properties

		/// <summary>
		/// Gets the type of the component this property is bound to.
		/// </summary>
		public override Type ComponentType
		{
			get 
			{ 
				return _basePropertyDescriptor.ComponentType; 
			}
		}

		/// <summary>
		/// Gets the name that can be displayed in a window, such as a Properties window.
		/// </summary>
		public override string DisplayName
		{
			get
			{
				if(this._displayName.Length > 0)
				{
					return this._displayName;
				}
				return this._basePropertyDescriptor.DisplayName;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this property is browsable.
		/// </summary>
		public override bool IsBrowsable 
		{
			get
			{
				return this._basePropertyDescriptor.IsBrowsable;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this property is read-only.
		/// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return this._basePropertyDescriptor.IsReadOnly;
			}
		}

		/// <summary>
		/// Gets the type of the property.
		/// </summary>
		public override Type PropertyType
		{
			get 
			{ 
				return this._basePropertyDescriptor.PropertyType; 
			}
		}

		#endregion // Properties

		#region Methods

		/// <summary>
		/// Returns whether resetting an object changes its value.
		/// </summary>
		/// <param name="component">The component to test for reset capability.</param>
		/// <returns>true if resetting the component changes its value; otherwise, false.</returns>
		public override bool CanResetValue(object component)
		{
			return _basePropertyDescriptor.CanResetValue(component);
		}

		/// <summary>
		/// Gets the current value of the property on a component.
		/// </summary>
		/// <param name="component">The component with the property for which to retrieve the value.</param>
		/// <returns>The value of a property for a given component.</returns>
		public override object GetValue(object component)
		{
			return this._basePropertyDescriptor.GetValue(component);
		}

		/// <summary>
		/// Resets the value for this property of the component to the default value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be reset to the default value.</param>
		public override void ResetValue(object component)
		{
			this._basePropertyDescriptor.ResetValue(component);
		}

		/// <summary>
		/// Determines a value indicating whether the value of this property needs to be persisted.
		/// </summary>
		/// <param name="component">The component with the property to be examined for persistence.</param>
		/// <returns>True if the property should be persisted; otherwise, false.</returns>
		public override bool ShouldSerializeValue(object component)
		{
			return this._basePropertyDescriptor.ShouldSerializeValue(component);
		}

		/// <summary>
		/// Sets the value of the component to a different value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be set.</param>
		/// <param name="value">The new value.</param>
		public override void SetValue(object component, object value)
		{
			this._basePropertyDescriptor.SetValue(component, value);
		}

		#endregion // Methods
	}

    internal interface IDataPointCustomPropertiesProvider
    {
        DataPointCustomProperties DataPointCustomProperties { get; }
    }
}
