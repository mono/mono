//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		XmlSerializer.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Utilities
//
//	Classes:	XmlFormatSerializer, BinaryFormatSerializer
//				SerializerBase, SerializationVisibilityAttribute
//
//  Purpose:	
//  
//  Chart serializer allows persisting of all chart data and 
//  settings into the stream or file using XML or binary format. 
//  This data can be later loaded back into the chart completely 
//  restoring its state. Serialize can also be used to reset chart 
//  control state to its default values. 
//  
//  Both XML and Binary serialization methods use reflection to 
//  discover class properties which need to be serialized. Only 
//  properties with non-default values are persisted. Full Trust 
//  is required to use chartserialization.
//  
//  SerializeBase class implements all the chart serializer 
//  properties and methods to reset chart content. XmlFormatSerializer 
//  and BinaryFormatSerializer classes derive from the SerializeBase 
//  class and provide saving and loading functionality for XML and 
//  binary format.
//  
//  By default, all chart content is Saved, Loaded or Reset, but 
//  this can be changed using serializer Content, SerializableContent 
//  and NonSerializableContent properties. Content property allows a 
//  simple way to serialize everything, appearance or just chart data. 
//  
//  SerializableContent and NonSerializableContent properties provide 
//  more control over what is beign persisted and they override the 
//  Content property settings. Each of the properties is a string 
//  which is a comma-separated listing of all chart properties to be 
//  serialized. The syntax of this property is "Class.Property[,Class.Property]", 
//  and wildcards may be used (represented by an asterisk). For example, 
//  to serialize all chart BackColor properties set this property to 
//  "*.BackColor".
//  
//	Reviewed:	AG - August 7, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================


#region Used Namespaces

using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Specialized;
using System.Security;

#if Microsoft_CONTROL
    using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
#else
    using System.Web.UI.WebControls;
    using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Utilities
#else
	namespace System.Web.UI.DataVisualization.Charting.Utilities
#endif
{
	#region Serialization enumerations

	/// <summary>
	/// Enumeration which describes how to persist property during the serialization
	/// </summary>
	internal enum SerializationVisibility
	{
		/// <summary>
		/// Do not serialize
		/// </summary>
		Hidden,

		/// <summary>
		/// Serialize as XML attribute
		/// </summary>
		Attribute,

		/// <summary>
		/// Serialize as XML element
		/// </summary>
		Element
	}

    /// <summary>
    /// Determines chart current serialization status.
    /// </summary>
    internal enum SerializationStatus
    {
        /// <summary>
        /// Chart is not serializing
        /// </summary>
        None,

        /// <summary>
        /// Chart is loading
        /// </summary>
        Loading,

        /// <summary>
        /// Chart is saving
        /// </summary>
        Saving,

        /// <summary>
        /// Chart is resetting
        /// </summary>
        Resetting
    }

	#endregion

	/// <summary>
	/// Attribute which describes how to persist property during the serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class SerializationVisibilityAttribute : System.Attribute 
	{
		#region Fields

		// Visibility style
		private SerializationVisibility _visibility = SerializationVisibility.Attribute;

		#endregion

		#region Constructor

		/// <summary>
		/// Public constructor
		/// </summary>
		/// <param name="visibility">Serialization visibility.</param>
		internal SerializationVisibilityAttribute(SerializationVisibility visibility)
		{
            this._visibility = visibility;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Serialization visibility property
		/// </summary>
		public SerializationVisibility Visibility
		{
			get 
			{
                return _visibility;
			}
            //set
            //{
            //    _visibility = value;
            //}
		}

		#endregion
	}
	
	/// <summary>
	/// Base class of the serializers. Common properties and methods for all serializers.
	/// </summary>
	internal abstract class SerializerBase
	{
		#region Fields

		/// <summary>
		/// Indicates that unknown properties and elements are ignored
		/// </summary>
		private	bool					_isUnknownAttributeIgnored = false;

		/// <summary>
		/// Indicates that serializer works in template creation mode
		/// </summary>
		private	bool					_isTemplateMode = false;

		/// <summary>
		/// Indicates that object properties are reset before loading
		/// </summary>
		private	bool					_isResetWhenLoading = true;

		/// <summary>
		/// Comma separated list of serializable (Save/Load/Reset) properties. "ClassName.PropertyName"
		/// </summary>
		private	string					_serializableContent = "";

		/// <summary>
		/// Comma separated list of NON serializable (Save/Load/Reset) properties. "ClassName.PropertyName"
		/// </summary>
		private	string					_nonSerializableContent = "";
			
		/// <summary>
		/// Font converters used while serializing/deserializing 
		/// </summary>
		internal static	FontConverter		fontConverter = new FontConverter();

		/// <summary>
		/// Color converters used while serializing/deserializing 
		/// </summary>
		internal static	ColorConverter		colorConverter = new ColorConverter();

		/// <summary>
		/// Hash code provider.
		/// </summary>
        protected static StringComparer hashCodeProvider = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Contains chart specific converters
        /// </summary>
        HybridDictionary _converterDict = new HybridDictionary();


		#endregion

		#region Public properties

		/// <summary>
		/// Indicates that unknown properties and elements will be 
		/// ignored without throwing an exception.
		/// </summary>
		internal bool IsUnknownAttributeIgnored
		{
			get
			{
                return _isUnknownAttributeIgnored;
			}
			set
			{
                _isUnknownAttributeIgnored = value;
			}
		}

		/// <summary>
		/// Indicates that serializer works in template creation mode
		/// </summary>
        internal bool IsTemplateMode
		{
			get
			{
                return _isTemplateMode;
			}
			set
			{
                _isTemplateMode = value;
			}
		}

		/// <summary>
		/// Indicates that object properties are reset to default
		/// values before loading.
		/// </summary>
        internal bool IsResetWhenLoading
		{
			get
			{
				return _isResetWhenLoading;
			}
			set
			{
				_isResetWhenLoading = value;
			}
		}

		/// <summary>
		/// Comma separated list of serializable (Save/Load/Reset) properties. 
		/// "ClassName.PropertyName,[ClassName.PropertyName]".
		/// </summary>
        internal string SerializableContent
		{
			get
			{
				return _serializableContent;
			}
			set
			{
				_serializableContent = value;

				// Reset list
				serializableContentList = null;
			}
		}

		/// <summary>
		/// Comma separated list of serializable (Save/Load/Reset) properties. 
		/// "ClassName.PropertyName,[ClassName.PropertyName]".
		/// </summary>
        internal string NonSerializableContent
		{
			get
			{
				return _nonSerializableContent;
			}
			set
			{
				_nonSerializableContent = value;

				// Reset list
				nonSerializableContentList = null;
			}
		}

		#endregion

		#region Resetting methods
        
		/// <summary>
		/// Reset properties of the object to default values.
		/// </summary>
		/// <param name="objectToReset">Object to be reset.</param>
		virtual internal void ResetObjectProperties(object objectToReset)
		{
			// Reset object properties
			ResetObjectProperties(objectToReset, null, GetObjectName(objectToReset));
		}

		/// <summary>
		/// Reset properties of the object to default values.
		/// Method is called recursively to reset child objects properties.
		/// </summary>
		/// <param name="objectToReset">Object to be reset.</param>
		/// <param name="parent">Parent of the reset object.</param>
		/// <param name="elementName">Object element name.</param>
        virtual internal void ResetObjectProperties(object objectToReset, object parent, string elementName)
		{
			// Check input parameters
			if(objectToReset == null)
			{
				return;
			}

            IList list = objectToReset as IList;

			// Check if object is a list
			if(list != null && IsSerializableContent(elementName, parent))
			{
				// Reset list by clearing all the items
				list.Clear();
				return;
			}

			// Retrive properties list of the object
			PropertyInfo[] properties = objectToReset.GetType().GetProperties();
			if(properties != null)
			{
				// Loop through all properties and reset public properties
				foreach(PropertyInfo pi in properties)
				{
					// Get property descriptor
					PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToReset)[pi.Name];

					// Check XmlFormatSerializerStyle attribute
					if(pd != null)
					{
						SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
						if(styleAttribute != null)
						{
							// Hidden property
							if(styleAttribute.Visibility == SerializationVisibility.Hidden)
							{
								continue;
							}
						}
					}

					// Check if this property should be reset
					bool resetProperty = IsSerializableContent(pi.Name, objectToReset);

					// Skip inherited properties from the root object
					if(IsChartBaseProperty(objectToReset, parent, pi))
					{
						continue;
					}

					// Reset list
					if(pi.CanRead && pi.PropertyType.GetInterface("IList", true) != null)
					{
						if(resetProperty)
						{
							// Check if collection has "Reset" method
							bool resetComplete = false;
							MethodInfo mi = objectToReset.GetType().GetMethod("Reset" + pi.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
							if(mi != null)
							{
								mi.Invoke(objectToReset, null);
								resetComplete = true;
							}

							// Reset list by clearing all the items
							if(!resetComplete)
							{
								((IList)pi.GetValue(objectToReset, null)).Clear();
							}
						}
						else
						{
							// Reset objects of the list
							foreach(object listObject in ((IList)pi.GetValue(objectToReset, null)))
							{
								ResetObjectProperties(listObject, objectToReset, this.GetObjectName(listObject));
							}
						}
					}

						// Reset public properties with Get and Set methods
					else if(pi.CanRead && pi.CanWrite)
					{
						// Skip indexes
						if(pi.Name == "Item")
						{
							continue;
						}

                        // Skip Names
                        if (pi.Name == "Name")
                        {
                            continue;
                        }

						// Reset inner properies
						if(ShouldSerializeAsAttribute(pi, objectToReset))
						{
							if(resetProperty)
							{
								// Reset the property using property descriptor
								
								if(pd != null)
								{
									// Get property object
									object objectProperty = pi.GetValue(objectToReset, null);

									// Get default value of the property
									DefaultValueAttribute defValueAttribute = (DefaultValueAttribute)pd.Attributes[typeof(DefaultValueAttribute)];
									if(defValueAttribute != null)
									{
										if(objectProperty == null)
										{
											if(defValueAttribute.Value != null)
											{
												pd.SetValue(objectToReset, defValueAttribute.Value);
											}
										}
										else if(! objectProperty.Equals(defValueAttribute.Value))
										{
											pd.SetValue(objectToReset, defValueAttribute.Value);
										}
									}
									else
									{
										// Check if property has "Reset" method
                                        MethodInfo mi = objectToReset.GetType().GetMethod("Reset" + pi.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
										if(mi != null)
										{
											mi.Invoke(objectToReset, null);
										}
									}
								}
							}
						}
						else
						{
							// Reset inner object
							ResetObjectProperties(pi.GetValue(objectToReset, null), objectToReset, pi.Name);
						}
					}
				}
			}
			return;
		}


		#endregion

		#region Abstract Serialization/Deserialization methods

		/// <summary>
		/// Serialize specified object into the destination object.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="destination">Destination of the serialization.</param>
        internal abstract void Serialize(object objectToSerialize, object destination);
		
		/// <summary>
		/// Deserialize specified object from the source object.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="source">Source of the deserialization.</param>
        internal abstract void Deserialize(object objectToDeserialize, object source);

		#endregion

		#region Protected helper methods

		/// <summary>
		/// Converts specified font object into a string.
		/// </summary>
		/// <param name="font">Font object to convert.</param>
		/// <returns>String that contains font data.</returns>
		internal static string FontToString(Font font)
		{
			// Save basic properties persisted by font converter
			string fontData = (string)SerializerBase.fontConverter.ConvertToInvariantString(font);

			// Persist properties not serialiazed by the converter
			if(font.GdiCharSet != 1)
			{
				fontData += ", GdiCharSet=" + font.GdiCharSet.ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
			if(font.GdiVerticalFont)
			{
				fontData += ", GdiVerticalFont";
			}

			return fontData;
		}

		/// <summary>
		/// Converts string data into a font object.
		/// </summary>
		/// <param name="fontString">String with font data.</param>
		/// <returns>Newly created font object.</returns>
		internal static Font FontFromString(string fontString)
		{
			// Check if string contains non-standard values "GdiCharSet" or "GdiVerticalFont"
			string standardData = fontString;
			byte gdiCharSet = 1;
			bool gdiVerticalFont = false;
			int charIndex = fontString.IndexOf(", GdiCharSet=", StringComparison.Ordinal);
			if(charIndex >= 0)
			{
				// Read value
				string val = fontString.Substring(charIndex + 13);
                int commaIndex = val.IndexOf(",", StringComparison.Ordinal);
				if(commaIndex >= 0)
				{
					val = val.Substring(0, commaIndex);
				}

				gdiCharSet = (byte)Int32.Parse(val, System.Globalization.CultureInfo.InvariantCulture);

				// Truncate standard data string
				if(standardData.Length > charIndex)
				{
					standardData = standardData.Substring(0, charIndex);
				}
			}
            charIndex = fontString.IndexOf(", GdiVerticalFont", StringComparison.Ordinal);
			if(charIndex >= 0)
			{
				gdiVerticalFont = true;

				// Truncate standard data string
				if(standardData.Length > charIndex)
				{
					standardData = standardData.Substring(0, charIndex);
				}
			}

			// Create Font object from standard parameters
			Font font = (Font)SerializerBase.fontConverter.ConvertFromInvariantString(standardData);

			// check if non-standard parameters provided
			if(gdiVerticalFont || gdiCharSet != 1)
			{
				Font newFont = new Font(
					font.Name,
					font.SizeInPoints,
					font.Style,
					GraphicsUnit.Point,
					gdiCharSet,
					gdiVerticalFont);

				font.Dispose();

				return newFont;
			}

			return font;
		}

		/// <summary>
		/// Returns a hash code of a specified string.
		/// </summary>
		/// <param name="str">String to get the hash code for.</param>
		/// <returns>String hash code.</returns>
		internal static short GetStringHashCode(string str)
		{
			return (short)(hashCodeProvider.GetHashCode(str) + str.Length * 2);
		}

		/// <summary>
		/// Reads hash ID from the specified binary reader.
		/// </summary>
		/// <param name="reader">Binary reader to get the data from.</param>
		/// <returns>Property name or collection member type ID.</returns>
        internal Int16 ReadHashID(BinaryReader reader)
		{
			// For later versions return ID without transformations
			return reader.ReadInt16();
		}

		/// <summary>
		/// Checks if property belongs to the base class of the chart "Control".
		/// </summary>
		/// <param name="objectToSerialize">Serializable object.</param>
		/// <param name="parent">Object parent.</param>
		/// <param name="pi">Serializable property information.</param>
		/// <returns>True if property belongs to the base class.</returns>
        internal bool IsChartBaseProperty(object objectToSerialize, object parent, PropertyInfo pi)
		{
			bool	result = false;

			// Check only for the root object
			if(parent == null)
			{
				Type	currentType = objectToSerialize.GetType();
				while(currentType != null)
				{
					if(pi.DeclaringType == currentType)
					{
						result = false;
						break;
					}

					// Check if it's a chart class
					if( currentType == typeof(Chart))
					{
						result = true;
						break;
					}
				
					// Get base class type
					currentType = currentType.BaseType;
				}
			}

			return result;
		}


		/// <summary>
		/// Converts Image object into the BASE64 encoded string
		/// </summary>
		/// <param name="image">Image to convert.</param>
		/// <returns>BASE64 encoded image data.</returns>
		internal static string ImageToString(System.Drawing.Image image)
		{
			// Save image into the stream using BASE64 encoding
			MemoryStream imageStream = new MemoryStream();
			image.Save(imageStream, ImageFormat.Png);
			imageStream.Seek(0, SeekOrigin.Begin);

			// Create XmlTextWriter and save image in BASE64
			StringBuilder stringBuilder = new StringBuilder();
			XmlTextWriter textWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
			byte[] imageByteData = imageStream.ToArray();
			textWriter.WriteBase64(imageByteData, 0, imageByteData.Length);

			// Close image stream
			textWriter.Close();
			imageStream.Close();

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Converts BASE64 encoded string to image.
		/// </summary>
		/// <param name="data">BASE64 encoded data.</param>
		/// <returns>Image.</returns>
        internal static System.Drawing.Image ImageFromString(string data)
		{
			// Create XML text reader
			byte[]	buffer = new byte[1000];
			MemoryStream imageStream = new MemoryStream();
			XmlTextReader textReader = new XmlTextReader(new StringReader("<base64>" + data + "</base64>"));

			// Read tags and BASE64 encoded data
			textReader.Read();
			int bytesRead = 0;
			while((bytesRead = textReader.ReadBase64(buffer, 0, 1000)) > 0)
			{
				imageStream.Write(buffer, 0, bytesRead);
			}
			textReader.Read();

			// Create image from stream
			imageStream.Seek(0, SeekOrigin.Begin);
            System.Drawing.Image tempImage = System.Drawing.Image.FromStream(imageStream);
			System.Drawing.Bitmap image = new Bitmap(tempImage);	// !!! .Net bug when image source stream is closed - can create brush using the image
            image.SetResolution(tempImage.HorizontalResolution, tempImage.VerticalResolution); //The bitmap created using the constructor does not copy the resolution of the image

			// Close image stream
			textReader.Close();
			imageStream.Close();

			return image;
		}

		/// <summary>
		/// Get the name of the object class
		/// </summary>
		/// <param name="obj">Object to get the name of.</param>
		/// <returns>Name of the object class (without namespace).</returns>
		internal string GetObjectName(object obj)
		{
            string name = obj.GetType().ToString();
			return name.Substring(name.LastIndexOf('.') + 1);
		}

        /// <summary>
        /// Create new empty item for the list.
        /// AxisName of the objects is determined by the return type of the indexer.
        /// </summary>
        /// <param name="list">List used to detect type of the item objects.</param>
        /// <param name="itemTypeName">Name of collection type.</param>
        /// <param name="itemName">Optional item name to return.</param>
        /// <param name="reusedObject">Indicates that object with specified name was already in the collection and it being reused.</param>
        /// <returns>New list item object.</returns>
        internal object GetListNewItem(IList list, string itemTypeName, ref string itemName, ref bool reusedObject)
		{
			// Get type of item in collection
			Type itemType = null;
			if(itemTypeName.Length > 0)
			{
                itemType = Type.GetType(typeof(Chart).Namespace + "." + itemTypeName, false, true);
			}

			reusedObject = false;
			PropertyInfo pi = list.GetType().GetProperty("Item", itemType, new Type[] {typeof(string)} );
            MethodInfo mi = list.GetType().GetMethod("IndexOf", new Type[] { typeof(String) });
			ConstructorInfo ci = null;
			if(pi != null)
			{
				// Try to get object by name using the indexer
				if(itemName != null && itemName.Length > 0)
				{
                    bool itemChecked = false;
                    if (mi != null)
                    {
                        try
                        {
                            int index = -1;
                            object oindex = mi.Invoke(list, new object[] { itemName });
                            if (oindex is int)
                            {
                                index = (int)oindex;
                                itemChecked = true;
                            }
                            if (index != -1)
                            {
                                object objByName = list[index];
                                if (objByName != null)
                                {
                                    // Remove found object from the list
                                    list.Remove(objByName);

                                    // Return found object
                                    reusedObject = true;
                                    return objByName;
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                        }
                        catch (TargetException)
                        {
                        }
                        catch (TargetInvocationException)
                        {
                        }
                    }
                    if (!itemChecked)
                    {
                        object objByName = null;
                        try
                        {
                            objByName = pi.GetValue(list, new object[] { itemName });
                        }
                        catch (ArgumentException)
                        {
                            objByName = null;
                        }
                        catch (TargetException)
                        {
                            objByName = null;
                        }
                        catch (TargetInvocationException)
                        {
                            objByName = null;
                        }

                        if (objByName != null)
                        {
                            try
                            {
                                // Remove found object from the list
                                list.Remove(objByName);
                            }
                            catch (NotSupportedException)
                            {
                            }

                            // Return found object
                            reusedObject = true;
                            return objByName;
                        }
                    }
					itemName = null;
				}

			}
            // Get the constructor of the type returned by indexer
            if (itemType != null)
            {
                ci = itemType.GetConstructor(Type.EmptyTypes);
            }
            else
            {
                ci = pi.PropertyType.GetConstructor(Type.EmptyTypes);
            }
            if (ci == null)
            {
                throw (new InvalidOperationException(SR.ExceptionChartSerializerDefaultConstructorUndefined(pi.PropertyType.ToString())));
            }
            return ci.Invoke(null);
		}

        /// <summary>
        /// Returns true if the object property should be serialized as 
        /// parent element attribute. Otherwise as a child element.
        /// </summary>
        /// <param name="pi">Property information.</param>
        /// <param name="parent">Object that the property belongs to.</param>
        /// <returns>True if property should be serialized as attribute.</returns>
        internal bool ShouldSerializeAsAttribute(PropertyInfo pi, object parent)
		{
			// Check if SerializationVisibilityAttribute is set
			if(parent != null)
			{
				PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[pi.Name];
				if(pd != null)
				{
					SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
					if(styleAttribute != null)
					{
						if(styleAttribute.Visibility == SerializationVisibility.Attribute)
						{
							return true;
						}
						else if(styleAttribute.Visibility == SerializationVisibility.Element)
						{
							return false;
						}
					}
				}
			}

			// If a simple type - serialize as property
			if(!pi.PropertyType.IsClass)
			{
				return true;
			}

			// Some classes are serialized as properties
			if(pi.PropertyType == typeof(string) ||
				pi.PropertyType == typeof(Font) ||
				pi.PropertyType == typeof(Color) ||
				pi.PropertyType == typeof(System.Drawing.Image))
			{
				return true;
			}

			return false;
        }


        /// <summary>
        /// Determines if this property should be serialized as attribute
        /// </summary>
        /// <param name="pi">Property information.</param>
        /// <param name="objectToSerialize">Object that the property belongs to.</param>
        /// <returns>True if should be serialized as attribute</returns>
        internal bool SerializeICollAsAtribute(PropertyInfo pi, object objectToSerialize)
        {
            if (objectToSerialize != null)
            {
                PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToSerialize)[pi.Name];
                if (pd != null)
                {
                    SerializationVisibilityAttribute styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
                    if (styleAttribute != null)
                    {
                        if (styleAttribute.Visibility == SerializationVisibility.Attribute)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
		/// Returns true if the object property is serializable.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		/// <param name="parent">Object that the property belongs to.</param>
		/// <returns>True if property is serializable.</returns>
        internal bool IsSerializableContent(string propertyName, object parent)
        {
			bool	serializable = true;
			if(_serializableContent.Length > 0 || _nonSerializableContent.Length > 0)
			{
				int		serialzableClassFitType = 0;	// 0 - undefined; 1 - '*'; 2 - 'Back*'; 3 - Exact
				int		serialzablePropertyFitType = 0;	// 0 - undefined; 1 - '*'; 2 - 'Back*'; 3 - Exact
                string ownerClassName = GetObjectName(parent);

				// Check if property in this class is part of the serializable content
				serializable = IsPropertyInList(GetSerializableContentList(), ownerClassName, propertyName, out serialzableClassFitType, out serialzablePropertyFitType);

				// Check if property in this class is part of the NON serializable content
				if(serializable)
				{
					int		nonSerialzableClassFitType = 0;	// 0 - undefined; 1 - '*'; 2 - 'Back*'; 3 - Exact
					int		nonSerialzablePropertyFitType = 0;	// 0 - undefined; 1 - '*'; 2 - 'Back*'; 3 - Exact
					bool	nonSerializable = IsPropertyInList(GetNonSerializableContentList(), ownerClassName, propertyName, out nonSerialzableClassFitType, out nonSerialzablePropertyFitType);

					// If property was found in non serializable content list - check the type priority
					// Priority order: Exact match, 'Back*' mask match, '*' all mask match
					if(nonSerializable)
					{
						// Check priority
						if((nonSerialzableClassFitType + nonSerialzablePropertyFitType) > 
							(serialzableClassFitType + serialzablePropertyFitType))
						{
							serializable = false;
						}
					}
				}
			}

			return serializable;
		}

        /// <summary>
        /// Checks if property belongs is defined in the mask list.
        /// </summary>
        /// <param name="contentList">Array list of class/property items.</param>
        /// <param name="className">Class name.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="classFitType">Return class mask fit type.</param>
        /// <param name="propertyFitType">Return property mask fit type.</param>
        /// <returns>True if property was found in the list.</returns>
		private bool IsPropertyInList(ArrayList contentList, string className, string propertyName, out int classFitType, out int propertyFitType)
		{
			// Initialize result values
			classFitType = 0;
			propertyFitType = 0;

			if(contentList != null)
			{
				// Loop through all items in the list using step 2
				for(int itemIndex = 0; itemIndex < contentList.Count; itemIndex += 2)
				{
					// Initialize result values
					classFitType = 0;
					propertyFitType = 0;

					// Check if object class and property name match the mask
					if(NameMatchMask((ItemInfo)contentList[itemIndex], className, out classFitType))
					{
						if(NameMatchMask((ItemInfo)contentList[itemIndex + 1], propertyName, out propertyFitType))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

        /// <summary>
        /// Compares class/property name with the specified mask
        /// </summary>
        /// <param name="itemInfo">Class/Property item information.</param>
        /// <param name="objectName">Class/Property name.</param>
        /// <param name="type">AxisName of matching. 0-No Match; 1-'*' any wild card; 2-'Back*' contain wild card; 3-exact match</param>
        /// <returns>True if name match the mask.</returns>
		private bool NameMatchMask(ItemInfo itemInfo, string objectName, out int type)
		{
			// Initialize type
			type = 0;

			// Any class mask
			if(itemInfo.any)
			{
				type = 1;
				return true;
			}

			// Ends with class mask
			if(itemInfo.endsWith)
			{
				if(itemInfo.name.Length <= objectName.Length)
				{
					if(objectName.Substring(0, itemInfo.name.Length) == itemInfo.name)
					{
						type = 2;
						return true;
					}
				}
			}

			// Starts with class mask
			if(itemInfo.startsWith)
			{
				if(itemInfo.name.Length <= objectName.Length)
				{
					if(objectName.Substring(objectName.Length - itemInfo.name.Length, itemInfo.name.Length) == itemInfo.name)
					{
						type = 2;
						return true;
					}
				}
			}

			// Exact name is specified
			if(itemInfo.name == objectName)
			{
				type = 3;
				return true;
			}

			return false;
		}


        /// <summary>
        /// Finds a converter by property descriptor.
        /// </summary>
        /// <param name="pd">Property descriptor.</param>
        /// <returns>A converter registered in TypeConverterAttribute or by property type</returns>
        internal TypeConverter FindConverter(PropertyDescriptor pd)
        {
            TypeConverter result;
            TypeConverterAttribute typeConverterAttrib = (TypeConverterAttribute)pd.Attributes[typeof(TypeConverterAttribute)];
            if (typeConverterAttrib != null && typeConverterAttrib.ConverterTypeName.Length > 0)
            {
                result = this.FindConverterByType(typeConverterAttrib);
                if (result != null)
                {
                    return result;
                }
                try
                {
                    return pd.Converter;
                }
                catch (SecurityException)
                {
                }
                catch (MethodAccessException)
                {
                }
            }
            return TypeDescriptor.GetConverter(pd.PropertyType);
        }

        /// <summary>
        /// Finds a converter by TypeConverterAttribute.
        /// </summary>
        /// <param name="attr">TypeConverterAttribute.</param>
        /// <returns>TypeConvetrer or null</returns>
        internal TypeConverter FindConverterByType( TypeConverterAttribute attr)
        {
            // In default Inranet zone (partial trust) ConsrtuctorInfo.Invoke (PropertyDescriptor.Converter) 
            // throws SecurityException or MethodAccessException when the converter class is internal.
            // Thats why we have this giant if - elseif here - to create type converters whitout reflection.
            if (_converterDict.Contains(attr.ConverterTypeName))
            {
                return (TypeConverter)_converterDict[attr.ConverterTypeName];
            }
            String typeStr = attr.ConverterTypeName;
            
            if (attr.ConverterTypeName.Contains(",") )
            {
                typeStr = attr.ConverterTypeName.Split(',')[0];
            }

            TypeConverter result = null;

            if (typeStr.EndsWith(".CustomPropertiesTypeConverter", StringComparison.OrdinalIgnoreCase)) { result = new CustomPropertiesTypeConverter(); }
            else if (typeStr.EndsWith(".DoubleNanValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new DoubleNanValueConverter(); }
            else if (typeStr.EndsWith(".DoubleDateNanValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new DoubleDateNanValueConverter(); }
#if !Microsoft_CONTROL
            else if (typeStr.EndsWith(".MapAreaCoordinatesConverter", StringComparison.OrdinalIgnoreCase)) { result = new MapAreaCoordinatesConverter(); }
#endif //Microsoft_CONTROL
            else if (typeStr.EndsWith(".ElementPositionConverter", StringComparison.OrdinalIgnoreCase)) { result = new ElementPositionConverter(); }
            else if (typeStr.EndsWith(".SeriesAreaNameConverter", StringComparison.OrdinalIgnoreCase)) { result = new SeriesAreaNameConverter(); }
            else if (typeStr.EndsWith(".ChartDataSourceConverter", StringComparison.OrdinalIgnoreCase)) { result = new ChartDataSourceConverter(); }
            else if (typeStr.EndsWith(".SeriesDataSourceMemberConverter", StringComparison.OrdinalIgnoreCase)) { result = new SeriesDataSourceMemberConverter(); }
            else if (typeStr.EndsWith(".SeriesLegendNameConverter", StringComparison.OrdinalIgnoreCase)) { result = new SeriesLegendNameConverter(); }
            else if (typeStr.EndsWith(".ChartTypeConverter", StringComparison.OrdinalIgnoreCase)) { result = new ChartTypeConverter(); }
            else if (typeStr.EndsWith(".SeriesNameConverter", StringComparison.OrdinalIgnoreCase)) { result = new SeriesNameConverter(); }
            else if (typeStr.EndsWith(".NoNameExpandableObjectConverter", StringComparison.OrdinalIgnoreCase)) { result = new NoNameExpandableObjectConverter(); }
            else if (typeStr.EndsWith(".DoubleArrayConverter", StringComparison.OrdinalIgnoreCase)) { result = new DoubleArrayConverter(); }
            else if (typeStr.EndsWith(".DataPointValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new DataPointValueConverter(); }
            else if (typeStr.EndsWith(".SeriesYValueTypeConverter", StringComparison.OrdinalIgnoreCase)) { result = new SeriesYValueTypeConverter(typeof(ChartValueType)); }
            else if (typeStr.EndsWith(".ColorArrayConverter", StringComparison.OrdinalIgnoreCase)) { result = new ColorArrayConverter(); }
            else if (typeStr.EndsWith(".LegendAreaNameConverter", StringComparison.OrdinalIgnoreCase)) { result = new LegendAreaNameConverter(); }
            else if (typeStr.EndsWith(".LegendConverter", StringComparison.OrdinalIgnoreCase)) { result = new LegendConverter(); }
            else if (typeStr.EndsWith(".SizeEmptyValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new SizeEmptyValueConverter(); }
            else if (typeStr.EndsWith(".MarginExpandableObjectConverter", StringComparison.OrdinalIgnoreCase)) { result = new MarginExpandableObjectConverter(); }
            else if (typeStr.EndsWith(".IntNanValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new IntNanValueConverter(); }
            else if (typeStr.EndsWith(".AxesArrayConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxesArrayConverter(); }
            else if (typeStr.EndsWith(".AxisLabelDateValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisLabelDateValueConverter(); }
            else if (typeStr.EndsWith(".AxisMinMaxValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisMinMaxValueConverter(); }
            else if (typeStr.EndsWith(".AxisCrossingValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisCrossingValueConverter(); }
            else if (typeStr.EndsWith(".AxisMinMaxAutoValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisMinMaxAutoValueConverter(); }
            else if (typeStr.EndsWith(".StripLineTitleAngleConverter", StringComparison.OrdinalIgnoreCase)) { result = new StripLineTitleAngleConverter(); }
            else if (typeStr.EndsWith(".AxisIntervalValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisIntervalValueConverter(); }
            else if (typeStr.EndsWith(".AxisElementIntervalValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AxisElementIntervalValueConverter(); }
            else if (typeStr.EndsWith(".AnchorPointValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AnchorPointValueConverter(); }
            else if (typeStr.EndsWith(".AnnotationAxisValueConverter", StringComparison.OrdinalIgnoreCase)) { result = new AnnotationAxisValueConverter(); }

            if (result != null) _converterDict[attr.ConverterTypeName] = result;
            
            return result;
        }

		#endregion

		#region Serializable content list managment fields, methods and classes

		/// <summary>
		/// Stores information about content item (class or property)
		/// </summary>
		private class ItemInfo
		{
			public	string		name = "";
			public	bool		any = false;
			public	bool		startsWith = false;
			public	bool		endsWith = false;
		}

		// Storage for serializable content items
		private	ArrayList		serializableContentList = null;

		// Storage for non serializable content items
		private	ArrayList		nonSerializableContentList = null;

		/// <summary>
		/// Return serializable content list.
		/// </summary>
		/// <returns>Serializable content list.</returns>
		private ArrayList GetSerializableContentList()
		{
			if(serializableContentList == null)
			{
				serializableContentList = new ArrayList();
				FillContentList(
					serializableContentList, 
					(this.SerializableContent.Length > 0 ) ? this.SerializableContent : "*.*");
			}

			return serializableContentList;
		}

		/// <summary>
		/// Return non serializable content list.
		/// </summary>
		/// <returns>Non serializable content list.</returns>
		private ArrayList GetNonSerializableContentList()
		{
			if(nonSerializableContentList == null)
			{
				nonSerializableContentList = new ArrayList();
				FillContentList(nonSerializableContentList, this.NonSerializableContent);
			}

			return nonSerializableContentList;
		}

		/// <summary>
		/// Fill content list from the string.
		/// </summary>
		/// <param name="list">Array list class.</param>
		/// <param name="content">Content string.</param>
		private void FillContentList(ArrayList list, string content)
		{
			if(content.Length > 0)
			{
				string[]	classPropertyPairs = content.Split(',');
				foreach(string item in classPropertyPairs)
				{
					// Create two content items: one for the class and one for the property
					ItemInfo	classInfo = new ItemInfo();
					ItemInfo	propertyInfo = new ItemInfo();

					// Find class and property name
					int pointIndex = item.IndexOf('.');
					if(pointIndex == -1)
					{
                        throw (new ArgumentException(SR.ExceptionChartSerializerContentStringFormatInvalid));
					}
					classInfo.name = item.Substring(0, pointIndex).Trim();
					propertyInfo.name = item.Substring(pointIndex + 1).Trim();
					if(classInfo.name.Length == 0)
					{
                        throw (new ArgumentException(SR.ExceptionChartSerializerClassNameUndefined));
					}
					if(propertyInfo.name.Length == 0)
					{
                        throw (new ArgumentException(SR.ExceptionChartSerializerPropertyNameUndefined));
					}

					// Make sure property name do not have point character
					if(propertyInfo.name.IndexOf('.') != -1)
					{
                        throw (new ArgumentException(SR.ExceptionChartSerializerContentStringFormatInvalid));
					}

					// Check for wildcards in names
					CheckWildCars(classInfo);
					CheckWildCars(propertyInfo);

					// Add class & property items into the array
					list.Add(classInfo);
					list.Add(propertyInfo);
				}
			}
		}

		/// <summary>
		/// Checks wildcards in the name of the item.
		/// Possible values:
		///		"*"
		///		"*Name"
		///		"Name*"
		/// </summary>
		/// <param name="info">Item information class.</param>
		private void CheckWildCars(ItemInfo info)
		{
			// Any class mask
			if(info.name == "*")
			{
				info.any = true;
			}

			// Ends with class mask
			else if(info.name[info.name.Length - 1] == '*')
			{
				info.endsWith = true;
				info.name = info.name.TrimEnd('*');
			}

			// Starts with class mask
			else if(info.name[0] == '*')
			{
				info.startsWith = true;
				info.name = info.name.TrimStart('*');
			}
		}

		#endregion
    }

	/// <summary>
	/// Utility class which serialize object using XML format
	/// </summary>
	internal class XmlFormatSerializer : SerializerBase
	{
		#region Serialization public methods

		/// <summary>
		/// Serialize specified object into the stream.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="stream">The stream used to write the XML document.</param>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Serialize(object objectToSerialize, Stream stream)
		{
			Serialize(objectToSerialize, (object)stream);
		}

		/// <summary>
		/// Serialize specified object into the XML writer.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="xmlWriter">The XmlWriter used to write the XML document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Serialize(object objectToSerialize, XmlWriter xmlWriter)
		{
			Serialize(objectToSerialize, (object)xmlWriter);
		}

		/// <summary>
		/// Serialize specified object into the text writer.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="textWriter">The TextWriter used to write the XML document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Serialize(object objectToSerialize, TextWriter textWriter)
		{
			Serialize(objectToSerialize, (object)textWriter);
		}

		/// <summary>
		/// Serialize specified object into the file.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="fileName">The file name used to write the XML document.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Serialize(object objectToSerialize, string fileName)
		{
			Serialize(objectToSerialize, (object)fileName);
		}

		#endregion
        
		#region Serialization private methods

		/// <summary>
		/// Serialize specified object into different types of writers using XML format.
		/// Here is what is serialized in the object:
		///	 - all public properties with Set and Get methods
		///  - all public properties with Get method which derived from ICollection
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="writer">Defines the serialization destination. Can be Stream, TextWriter, XmlWriter or String (file name).</param>
		
        internal override void Serialize(object objectToSerialize, object writer)
		{
            // the possible writer types
            Stream stream = writer as Stream;
            TextWriter textWriter = writer as TextWriter;
            XmlWriter xmlWriter = writer as XmlWriter;
            string writerStr = writer as string;

			// Check input parameters
			if(objectToSerialize == null)
			{
				throw(new ArgumentNullException("objectToSerialize"));
			}
			if(writer == null)
			{
				throw(new ArgumentNullException("writer"));
			}
			if(stream == null && textWriter == null && xmlWriter == null && writerStr == null)
			{
                throw (new ArgumentException(SR.ExceptionChartSerializerWriterObjectInvalid, "writer"));
			}

			// Create XML document
			XmlDocument xmlDocument = new XmlDocument();
		
			// Create document fragment
			XmlDocumentFragment docFragment = xmlDocument.CreateDocumentFragment();



			// Serialize object
			SerializeObject(objectToSerialize, null, GetObjectName(objectToSerialize), docFragment, xmlDocument);


			// Append document fragment
			xmlDocument.AppendChild(docFragment);

			// Remove empty child nodes
			RemoveEmptyChildNodes(xmlDocument);

			// Save XML document into the writer
			if(stream != null)
			{
				xmlDocument.Save(stream);

				// Flush stream and seek to the beginning
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
			}

			if(writerStr != null)
			{
				xmlDocument.Save(writerStr);
			}

			if(xmlWriter != null)
			{
                xmlDocument.Save(xmlWriter);
			}

			if(textWriter != null)
			{
                xmlDocument.Save(textWriter);
			}
		}
        /// <summary>
		/// Serialize specified object into the XML format.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="parent">Parent of the serialized object.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="xmlParentNode">The XmlNode of the parent object to serialize the data in.</param>
		/// <param name="xmlDocument">The XmlDocument the parent node belongs to.</param>
        virtual protected void SerializeObject(object objectToSerialize, object parent, string elementName, XmlNode xmlParentNode, XmlDocument xmlDocument)
        {
			// Check input parameters
			if(objectToSerialize == null)
			{
				return;
			}

			// Check if object should be serialized
			if(parent != null)
			{
				PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[elementName];
				if(pd != null)
				{
					SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
					if(styleAttribute != null)
					{
						// Hidden property
						if(styleAttribute.Visibility == SerializationVisibility.Hidden)
						{
							return;
						}
					}
				}
			}

			// Check if object is a collection
			if(objectToSerialize is ICollection)
			{
				// Serialize collection
				SerializeCollection(objectToSerialize, elementName, xmlParentNode, xmlDocument);
				return;
			}

			// Create object element inside the parents node
			XmlNode xmlNode = xmlDocument.CreateElement(elementName);
			xmlParentNode.AppendChild(xmlNode);

			// Write template data into collection items
			bool templateListItem = false;
            IList parentList = parent as IList;
			if(this.IsTemplateMode && parentList != null)
			{
				// Create "_Template_" attribute
				XmlAttribute attrib = xmlDocument.CreateAttribute("_Template_");

				// Check number of items in collection
                if (parentList.Count == 1)
				{
					// If only one iten in collection, set "All" value.
					// This means that style of this object should be applied to all
					// existing items of the collection.
					attrib.Value = "All";
				}
				else
				{
					// If there is more than one item, use it's index.
					// When loading, style of these items will be applied to existing 
					// items in collection in the loop.
                    int itemIndex = parentList.IndexOf(objectToSerialize);
                    attrib.Value = itemIndex.ToString(CultureInfo.InvariantCulture);
				}

				// Add "_Template_" attribute into the XML node
				xmlNode.Attributes.Append(attrib);
				templateListItem = true;
			}

            // Retrive properties list of the object
            PropertyInfo[] properties = objectToSerialize.GetType().GetProperties();
            if (properties != null)
            {
				// Loop through all properties and serialize public properties
				foreach(PropertyInfo pi in properties)
				{

					// Skip "Name" property from collection items in template mode
					if(templateListItem && pi.Name == "Name")
					{
						continue;
					}

					// Skip inherited properties from the root object
					if(IsChartBaseProperty(objectToSerialize, parent, pi))
					{
						continue;
					}

                    // Check if this property is serializable content
                    if (!IsSerializableContent(pi.Name, objectToSerialize))
                    {
                        continue;
                    }
 
                    // Serialize collection

                    if (pi.CanRead && pi.PropertyType.GetInterface("ICollection", true) != null && !this.SerializeICollAsAtribute(pi, objectToSerialize))
                    {
                        // Check if SerializationVisibilityAttribute is set
						bool	serialize = true;
						if(objectToSerialize != null)
						{
							PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToSerialize)[pi.Name];
							if(pd != null)
							{
								SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
								if(styleAttribute != null)
								{
									if(styleAttribute.Visibility == SerializationVisibility.Hidden)
									{
										serialize = false;
									}
								}
							}
						}
						// Check if collection has "ShouldSerialize" method
						MethodInfo mi = objectToSerialize.GetType().GetMethod("ShouldSerialize" + pi.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
						if(mi != null)
						{
							object result = mi.Invoke(objectToSerialize, null);
							if(result is bool && ((bool)result) == false)
							{
								// Do not serialize collection
								serialize = false;
							}
						}

						// Serialize collection
						if(serialize)
						{
							SerializeCollection(pi.GetValue(objectToSerialize, null), pi.Name, xmlNode, xmlDocument);
						}
					}

					// Serialize public properties with Get and Set methods
					else if(pi.CanRead && pi.CanWrite)
					{
						// Skip indexes
						if(pi.Name == "Item")
						{
							continue;
						}

						// Check if an object should be serialized as a property or as a class
						if(ShouldSerializeAsAttribute(pi, objectToSerialize))
						{
							// Serialize property
							SerializeProperty(pi.GetValue(objectToSerialize, null), objectToSerialize, pi.Name, xmlNode, xmlDocument);
						}
						else
						{
							// Serialize inner object
							SerializeObject(pi.GetValue(objectToSerialize, null), objectToSerialize, pi.Name, xmlNode, xmlDocument);
						}
					}
				}
			}
			return;
		}


        /// <summary>
        /// Serializes the data point.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="xmlParentNode">The XML parent node.</param>
        /// <param name="xmlDocument">The XML document.</param>
        internal void SerializeDataPoint(object objectToSerialize, XmlNode xmlParentNode, XmlDocument xmlDocument)
        {
            // Create object element inside the parents node
            XmlNode xmlNode = xmlDocument.CreateElement(GetObjectName(objectToSerialize));
            xmlParentNode.AppendChild(xmlNode);
            
            DataPoint dataPoint = objectToSerialize as DataPoint;
            if (dataPoint.XValue != 0d && IsSerializableContent("XValue", objectToSerialize))
            {
                XmlAttribute attrib = xmlDocument.CreateAttribute("XValue");
                attrib.Value = GetXmlValue(dataPoint.XValue, dataPoint, "XValue");
                xmlNode.Attributes.Append(attrib);
            }
            if (dataPoint.YValues.Length > 0 && IsSerializableContent("YValues", objectToSerialize))
            {
                XmlAttribute attrib = xmlDocument.CreateAttribute("YValues");
                attrib.Value = GetXmlValue(dataPoint.YValues, dataPoint, "YValues");
                xmlNode.Attributes.Append(attrib);
            }
            if (dataPoint.IsEmpty && IsSerializableContent("IsEmpty", objectToSerialize))
            {
                XmlAttribute attrib = xmlDocument.CreateAttribute("IsEmpty");
                attrib.Value = GetXmlValue(dataPoint.isEmptyPoint, dataPoint, "IsEmpty");
                xmlNode.Attributes.Append(attrib);
            }
            bool hasCustomProperties = false;
            foreach (DictionaryEntry entry in dataPoint.properties)
            {
                if (entry.Key is int)
                {
                    CommonCustomProperties propertyType = (CommonCustomProperties)((int)entry.Key);
                    String properyName = propertyType.ToString();
                    if (IsSerializableContent(properyName, objectToSerialize))
                    {
                        XmlAttribute attrib = xmlDocument.CreateAttribute(properyName);
                        attrib.Value = GetXmlValue(entry.Value, dataPoint, properyName);
                        xmlNode.Attributes.Append(attrib);
                    }
                }
                else
                {
                    hasCustomProperties = true;
                }
            }

            if (hasCustomProperties && !String.IsNullOrEmpty(dataPoint.CustomProperties) && IsSerializableContent("CustomProperties", objectToSerialize))
            {
                XmlAttribute attrib = xmlDocument.CreateAttribute("CustomProperties");
                attrib.Value = GetXmlValue(dataPoint.CustomProperties, dataPoint, "CustomProperties");
                xmlNode.Attributes.Append(attrib);
            }            

        }

		/// <summary>
		/// Serialize specified object into the XML text writer.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="xmlParentNode">The XmlNode of the parent object to serialize the data in.</param>
		/// <param name="xmlDocument">The XmlDocument the parent node belongs to.</param>
		virtual protected void SerializeCollection(object objectToSerialize, string elementName, XmlNode xmlParentNode, XmlDocument xmlDocument)
		{
            ICollection collection = objectToSerialize as ICollection;
			if(collection != null)
			{
				// Create object element inside the parents node
				XmlNode xmlNode = xmlDocument.CreateElement(elementName);
				xmlParentNode.AppendChild(xmlNode);
                // Enumerate through all objects in collection and serialize them
				foreach(object obj in collection)
				{

                    if (obj is DataPoint)
                    {
                        SerializeDataPoint(obj, xmlNode, xmlDocument);
                        continue;
                    }

                    SerializeObject(obj, objectToSerialize, GetObjectName(obj), xmlNode, xmlDocument);
				}
			}
		}

		/// <summary>
		/// Serialize specified object into the XML text writer.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="parent">Parent of the serialized object.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="xmlParentNode">The XmlNode of the parent object to serialize the data in.</param>
		/// <param name="xmlDocument">The XmlDocument the parent node belongs to.</param>
		virtual protected void SerializeProperty(object objectToSerialize, object parent, string elementName, XmlNode xmlParentNode, XmlDocument xmlDocument)
		{
			// Check input parameters
			if(objectToSerialize == null || parent == null)
			{
				return;
			}

			// Check if property has non-default value
			PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[elementName];
			if(pd != null)
			{
				DefaultValueAttribute defValueAttribute = (DefaultValueAttribute)pd.Attributes[typeof(DefaultValueAttribute)];
				if(defValueAttribute != null)
				{
					if(objectToSerialize.Equals(defValueAttribute.Value))
					{
						// Do not serialize properties with default values
						return;
					}
				}
				else
				{
					// Check if property has "ShouldSerialize" method
                    MethodInfo mi = parent.GetType().GetMethod("ShouldSerialize" + elementName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					if(mi != null)
					{
						object result = mi.Invoke(parent, null);
						if(result is bool && ((bool)result) == false)
						{
							// Do not serialize properties with default values
							return;
						}
					}
				}
			
				// Check XmlFormatSerializerStyle attribute
				SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
				if(styleAttribute != null)
				{
					// Hidden property
					if(styleAttribute.Visibility == SerializationVisibility.Hidden)
					{
						return;
					}
				}
			}

			// Serialize property as a parents node attribute
			XmlAttribute attrib = xmlDocument.CreateAttribute(elementName);
			attrib.Value = GetXmlValue(objectToSerialize, parent, elementName);
			xmlParentNode.Attributes.Append(attrib);
		}

		/// <summary>
		/// Converts object value into the string.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="parent">Object parent.</param>
		/// <param name="elementName">Object name.</param>
		/// <returns>Object value as strig.</returns>
		protected string GetXmlValue(object obj, object parent, string elementName)
		{
            string objStr = obj as string;
			if(objStr != null)
			{
                return objStr;
			}

            Font font = obj as Font;
            if(font != null)
			{
				return SerializerBase.FontToString(font);
			}

			if(obj is Color)
			{
				return colorConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, obj);
			}

            Color[] colors = obj as Color[];
			if(colors != null)
			{
				return ColorArrayConverter.ColorArrayToString(colors);
			}

#if !Microsoft_CONTROL
            if(obj is Unit)
			{
                Unit unit = (Unit)obj;
				return unit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
#endif

            System.Drawing.Image image = obj as System.Drawing.Image;
			if(image != null)
			{
				return ImageToString(image);
			}

			// Look for the converter set with the attibute
			PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[elementName];
			if(pd != null)
			{
                TypeConverter converter = this.FindConverter(pd);
                if (converter != null && converter.CanConvertTo(typeof(string)))
				{
                    return converter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, obj);
				}
			}
			
			// Try using default string convertion
			return obj.ToString();
		}

		/// <summary>
		/// Removes all empty nodes from the XML document.
		/// Method is called recursively to remove empty child nodes first.
		/// </summary>
		/// <param name="xmlNode">The node where to start the removing.</param>
		private void RemoveEmptyChildNodes(XmlNode xmlNode)
		{
			// Loop through all child nodes
			for(int nodeIndex = 0; nodeIndex < xmlNode.ChildNodes.Count; nodeIndex++)
			{
				// Remove empty child nodes of the child
				RemoveEmptyChildNodes(xmlNode.ChildNodes[nodeIndex]);

				// Check if there are any non-empty nodes left
				XmlNode currentNode = xmlNode.ChildNodes[nodeIndex];
				if( currentNode.ParentNode != null &&
					!(currentNode.ParentNode is XmlDocument) )
				{
					if(!currentNode.HasChildNodes && 
						(currentNode.Attributes == null ||
						currentNode.Attributes.Count == 0))
					{
						// Remove node
						xmlNode.RemoveChild(xmlNode.ChildNodes[nodeIndex]);
						--nodeIndex;
					}
				}



				// Remove node with one "_Template_" attribute
				if(!currentNode.HasChildNodes && 
					currentNode.Attributes.Count == 1 &&
					currentNode.Attributes["_Template_"] != null)
				{
					// Remove node
					xmlNode.RemoveChild(xmlNode.ChildNodes[nodeIndex]);
					--nodeIndex;
				}



			}
		}

		#endregion

		#region Deserialization public methods

		/// <summary>
		/// Deserialize specified object from the stream.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="stream">The stream used to read the XML document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal void Deserialize(object objectToDeserialize, Stream stream)
		{
			Deserialize(objectToDeserialize, (object)stream);
		}

		/// <summary>
		/// Deserialize specified object from the XML reader.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="xmlReader">The XmlReader used to read the XML document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Deserialize(object objectToDeserialize, XmlReader xmlReader)
		{
			Deserialize(objectToDeserialize, (object)xmlReader);
		}

		/// <summary>
		/// Deserialize specified object from the text reader.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="textReader">The TextReader used to write the XML document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Deserialize(object objectToDeserialize, TextReader textReader)
		{
			Deserialize(objectToDeserialize, (object)textReader);
		}

		/// <summary>
		/// Deserialize specified object from the file.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="fileName">The file name used to read the XML document from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Deserialize(object objectToDeserialize, string fileName)
		{
			Deserialize(objectToDeserialize, (object)fileName);
		}

		#endregion

		#region Deserialization private methods

		/// <summary>
		/// Deserialize object from different types of readers using XML format.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="reader">Defines the deserialization data source. Can be Stream, TextReader, XmlReader or String (file name).</param>
        internal override void Deserialize(object objectToDeserialize, object reader)
		{
            // the four possible types of readers
            Stream stream = reader as Stream;
            TextReader textReader = reader as TextReader;
            XmlReader xmlReader = reader as XmlReader;
            string readerStr = reader as string;

			// Check input parameters
			if(objectToDeserialize == null)
			{
				throw(new ArgumentNullException("objectToDeserialize"));
			}
			if(reader == null)
			{
				throw(new ArgumentNullException("reader"));
			}
			if(stream == null && textReader == null && xmlReader == null && readerStr == null)
			{
                throw (new ArgumentException(SR.ExceptionChartSerializerReaderObjectInvalid, "reader"));
			}

			// Create XML document
			XmlDocument xmlDocument = new XmlDocument();
            XmlReader xmlBaseReader = null;
            try
            {
                // process files without DTD
                XmlReaderSettings settings = new XmlReaderSettings();
                // settings.ProhibitDtd is obsolete inn NetFx 4.0, the #ifdef stays for compilation under NetFx 3.5.
#if OLD_DTD
                settings.ProhibitDtd = true;
#else
                settings.DtdProcessing = DtdProcessing.Prohibit; //don't allow DTD
#endif
                // Load XML document from the reader
                if (stream != null)
                {
                    xmlBaseReader = XmlReader.Create(stream, settings);
                }
                if (readerStr != null)
                {
                    xmlBaseReader = XmlReader.Create(readerStr, settings);
                }
                if (xmlReader != null)
                {
                    xmlBaseReader = XmlReader.Create(xmlReader, settings);
                }
                if (textReader != null)
                {
                    xmlBaseReader = XmlReader.Create(textReader, settings);
                }

                xmlDocument.Load(xmlBaseReader);

                // Reset properties of the root object
                if (IsResetWhenLoading)
                {
                    ResetObjectProperties(objectToDeserialize);
                }

                // Deserialize object
                DeserializeObject(objectToDeserialize, null, GetObjectName(objectToDeserialize), xmlDocument.DocumentElement, xmlDocument);
            }
            finally
            {
                if (xmlBaseReader != null)
                {
                    ((IDisposable)xmlBaseReader).Dispose();
                }
            }
		}

		/// <summary>
		/// Deserialize object from the XML format.
		/// Method is called recursively to deserialize child objects.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="parent">Parent of the deserialized object.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="xmlParentNode">The XmlNode of the parent object to deserialize the data from.</param>
		/// <param name="xmlDocument">The XmlDocument the parent node belongs to.</param>
		/// <returns>Number of properties set.</returns>
		virtual internal int DeserializeObject(object objectToDeserialize, object parent, string elementName, XmlNode xmlParentNode, XmlDocument xmlDocument)
		{
			int	setPropertiesNumber = 0;

			// Check input parameters
			if(objectToDeserialize == null)
			{
				return setPropertiesNumber;
			}

			// Loop through all node properties
			foreach(XmlAttribute attr in xmlParentNode.Attributes)
			{
				// Skip template collection item attribute
				if(attr.Name == "_Template_")
				{
					continue;
				}

				// Check if this property is serializable content
				if(IsSerializableContent(attr.Name, objectToDeserialize))
				{
					SetXmlValue(objectToDeserialize, attr.Name, attr.Value);
					++setPropertiesNumber;
				}
			}



			// Read template data into the collection 
            IList list = objectToDeserialize as IList;

			if(this.IsTemplateMode && 
				list != null && 
				xmlParentNode.FirstChild.Attributes["_Template_"] != null)
			{
				// Loop through all items in collection
				int	itemIndex = 0;
				foreach(object listItem in list)
				{
					// Find XML node appropriate for the item from the collection
					XmlNode	listItemNode = null;

					// Loop through all child nodes
					foreach(XmlNode childNode in xmlParentNode.ChildNodes)
					{
						string templateString = childNode.Attributes["_Template_"].Value;
						if(templateString != null && templateString.Length > 0)
						{
							if(templateString == "All")
							{
								listItemNode = childNode;
								break;
							}
							else
							{
								// If there is more items in collection than XML node in template
								// apply items in a loop
								int loopItemIndex = itemIndex;
								while(loopItemIndex > xmlParentNode.ChildNodes.Count - 1)
								{
									loopItemIndex -= xmlParentNode.ChildNodes.Count;
								}

								// Convert attribute value to index
                                int nodeIndex = int.Parse(templateString, CultureInfo.InvariantCulture);
								if(nodeIndex == loopItemIndex)
								{
									listItemNode = childNode;
									break;
								}
							}
						}
					}

					// Load data from the node
					if(listItemNode != null)
					{
						// Load object data
						DeserializeObject(listItem, objectToDeserialize, "", listItemNode, xmlDocument);
					}

					// Increase item index
					++itemIndex;
				}

				// No futher loading required
				return 0;
			}



			// Loop through all child elements
			int	listItemIndex = 0;
			foreach(XmlNode childNode in xmlParentNode.ChildNodes)
			{
                // Special handling for the collections
                // Bug VSTS #235707 - The collections IsSerializableContent are already checked as a property in the else statement.
                if (list != null)
                {
                    // Create new item object
                    string itemName = null;
                    if (childNode.Attributes["Name"] != null)
                    {
                        itemName = childNode.Attributes["Name"].Value;
                    }

                    bool reusedObject = false;
                    object listItem = GetListNewItem(list, childNode.Name, ref itemName, ref reusedObject);

                    // Deserialize list item object
                    int itemSetProperties = DeserializeObject(listItem, objectToDeserialize, "", childNode, xmlDocument);
                    setPropertiesNumber += itemSetProperties;

                    // Add item object into the list
                    if (itemSetProperties > 0 || reusedObject)
                    {
                        list.Insert(listItemIndex++, listItem);
                    }
                }

                else
                {
                    // Check if this property is serializable content
                    if (IsSerializableContent(childNode.Name, objectToDeserialize))
                    {
                        // Deserialize the property using property descriptor
                        PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToDeserialize)[childNode.Name];
                        if (pd != null)
                        {
                            object innerObject = pd.GetValue(objectToDeserialize);

                            // Deserialize list item object
                            setPropertiesNumber += DeserializeObject(innerObject, objectToDeserialize, childNode.Name, childNode, xmlDocument);
                        }
                        else if (!IsUnknownAttributeIgnored)
                        {
                            throw (new InvalidOperationException(SR.ExceptionChartSerializerPropertyNameUnknown(childNode.Name, objectToDeserialize.GetType().ToString())));
                        }
                    }
                }
			}

			return setPropertiesNumber;
		}

        /// <summary>
        /// Sets a property of an object using name and value as string.
        /// </summary>
        /// <param name="obj">Object to set.</param>
        /// <param name="attrName">Attribute (property) name.</param>
        /// <param name="attrValue">Object value..</param>
        /// <returns>Object value as strig.</returns>
		private void SetXmlValue(object obj, string attrName, string attrValue)
		{
			PropertyInfo pi = obj.GetType().GetProperty(attrName);
			if(pi != null)
			{
				// Convert string to object value
				object objValue = attrValue;

				if(pi.PropertyType == typeof(string))
				{
					objValue = attrValue;
				}

				else if(pi.PropertyType == typeof(Font))
				{
					objValue = SerializerBase.FontFromString(attrValue);
				}

				else if(pi.PropertyType == typeof(Color))
				{
					objValue = (Color)colorConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, attrValue);
				}

#if !Microsoft_CONTROL
				else if(pi.PropertyType == typeof(Unit))
				{
					objValue = new Unit(Int32.Parse(attrValue, System.Globalization.CultureInfo.InvariantCulture));
				}
#endif

				else if(pi.PropertyType == typeof(System.Drawing.Image))
				{
					objValue = ImageFromString(attrValue);
				}

				else
				{
					// Look for the converter set with the attibute
					PropertyDescriptor pd = TypeDescriptor.GetProperties(obj)[attrName];
					if(pd != null)
					{
                        TypeConverter converter = this.FindConverter(pd);
                        if (converter != null && converter.CanConvertFrom(typeof(string)))
						{
                            objValue = converter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, attrValue);
						}
					}
				}

				// Set object value
				pi.SetValue(obj, objValue, null);
			}
			else if(!IsUnknownAttributeIgnored)
			{
				throw(new InvalidOperationException(SR.ExceptionChartSerializerPropertyNameUnknown( attrName,obj.GetType().ToString())));
			}
		}

		#endregion
	}

	/// <summary>
	/// Utility class which serialize object using binary format
	/// </summary>
	internal class BinaryFormatSerializer : SerializerBase
	{
		#region Serialization methods

		/// <summary>
		/// Serialize specified object into the destination using binary format.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="destination">Serialization destination.</param>
        internal override void Serialize(object objectToSerialize, object destination)
        {
            // Check input parameters
            if (objectToSerialize == null)
            {
                throw (new ArgumentNullException("objectToSerialize"));
            }
            if (destination == null)
            {
                throw (new ArgumentNullException("destination"));
            }

            string destinationStr = destination as string;
            if (destinationStr != null)
            {
                Serialize(objectToSerialize, destinationStr);
                return;
            }

            Stream stream = destination as Stream;
            if (stream != null)
            {
                Serialize(objectToSerialize, stream);
                return;
            }

            BinaryWriter binaryWriter = destination as BinaryWriter;
            if (binaryWriter != null)
            {
                Serialize(objectToSerialize, binaryWriter);
                return;
            }

            throw (new ArgumentException(SR.ExceptionChartSerializerDestinationObjectInvalid, "destination"));
        }

		/// <summary>
		/// Serialize specified object into the file using binary format.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="fileName">File name to serialize the data in.</param>
        internal void Serialize(object objectToSerialize, string fileName)
		{
			FileStream stream = new FileStream(fileName, FileMode.Create);
			Serialize(objectToSerialize, new BinaryWriter(stream));
			stream.Close();
		}


		/// <summary>
		/// Serialize specified object into the stream using binary format.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="stream">Defines the serialization destination.</param>
		internal void Serialize(object objectToSerialize, Stream stream)
		{
			Serialize(objectToSerialize, new BinaryWriter(stream));
		}

		/// <summary>
		/// Serialize specified object into different types of writers using binary format.
		/// Here is what is serialized in the object:
		///	 - all public properties with Set and Get methods
		///  - all public properties with Get method which derived from ICollection
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="writer">Defines the serialization destination.</param>
		internal void Serialize(object objectToSerialize, BinaryWriter writer)
		{
			// Check input parameters
			if(objectToSerialize == null)
			{
				throw(new ArgumentNullException("objectToSerialize"));
			}
			if(writer == null)
			{
				throw(new ArgumentNullException("writer"));
			}

			// Write bnary format header into the stream, which consist of 15 characters
			char[]	header = new char[15] {'D', 'C', 'B', 'F', '4', '0', '0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'};
			writer.Write(header);
			

			// Serialize object
			SerializeObject(objectToSerialize, null, GetObjectName(objectToSerialize), writer);


			// Flush the writer stream
			writer.Flush();

			// Reset stream position
			writer.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Serialize specified object into the binary format.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="parent">Parent of the serialized object.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="writer">Binary writer object.</param>
        virtual internal void SerializeObject(object objectToSerialize, object parent, string elementName, BinaryWriter writer)
		{
			// Check input parameters
			if(objectToSerialize == null)
			{
				return;
			}

			// Check if object should be serialized
			if(parent != null)
			{
				PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[elementName];
				if(pd != null)
				{
					SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
					if(styleAttribute != null)
					{
						// Hidden property
						if(styleAttribute.Visibility == SerializationVisibility.Hidden)
						{
							return;
						}
					}
				}
			}

			// Check if object is a collection
			if(objectToSerialize is ICollection)
			{
				// Serialize collection
				SerializeCollection(objectToSerialize, elementName, writer);
				return;
			}

			// Write object ID (hash of the name) into the writer
			writer.Write(SerializerBase.GetStringHashCode(elementName));

			// Remember position where object data is started
			long elementStartPosition = writer.Seek(0, SeekOrigin.Current);

			// Retrive properties list of the object
			ArrayList	propNamesList = new ArrayList();
			PropertyInfo[] properties = objectToSerialize.GetType().GetProperties();
			if(properties != null)
			{
				// Loop through all properties and serialize public properties
				foreach(PropertyInfo pi in properties)
				{
					// Skip inherited properties from the root object
					if(IsChartBaseProperty(objectToSerialize, parent, pi))
					{
						continue;
					}
					// Serialize collection
                    if (pi.CanRead && pi.PropertyType.GetInterface("ICollection", true) != null && !this.SerializeICollAsAtribute(pi, objectToSerialize))
                    {
                        bool serialize = IsSerializableContent(pi.Name, objectToSerialize);

                        // fixing Axes Array Framework 2.0 side effect
                        // fixed by:DT
                        if (serialize && objectToSerialize != null)
                        {
                            PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToSerialize)[pi.Name];
                            if (pd != null)
                            {
                                SerializationVisibilityAttribute styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
                                if (styleAttribute != null)
                                {
                                    if (styleAttribute.Visibility == SerializationVisibility.Hidden)
                                    {
                                        serialize = false;
                                    }
                                }
                            }
                        }

                        MethodInfo mi = objectToSerialize.GetType().GetMethod("ShouldSerialize" + pi.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
						if( serialize && mi != null)
						{
							object result = mi.Invoke(objectToSerialize, null);
							if(result is bool && ((bool)result) == false)
							{
								// Do not serialize collection
								serialize = false;
							}
						}

						// Serialize collection
						if(serialize)
						{
							propNamesList.Add(pi.Name);
							SerializeCollection(pi.GetValue(objectToSerialize, null), pi.Name, writer);
						}
					}

					// Serialize public properties with Get and Set methods
					else if(pi.CanRead && pi.CanWrite)
					{
						// Skip indexes
						if(pi.Name == "Item")
						{
							continue;
						}
                        // Check if this property is serializable content
                        if (IsSerializableContent(pi.Name, objectToSerialize))
                        {
                            // Check if an object should be serialized as a property or as a class
                            if (ShouldSerializeAsAttribute(pi, objectToSerialize))
                            {
                                // Serialize property
                                SerializeProperty(pi.GetValue(objectToSerialize, null), objectToSerialize, pi.Name, writer);
                            }
                            else
                            {
                                // Serialize inner object
                                SerializeObject(pi.GetValue(objectToSerialize, null), objectToSerialize, pi.Name, writer);
                            }
                        }
						propNamesList.Add(pi.Name);
					}
				}
			
				// Check that all properties have unique IDs
				CheckPropertiesID(propNamesList);
			}


			// If position of the writer did not change - nothing was written
			if(writer.Seek(0, SeekOrigin.Current) == elementStartPosition)
			{
				// Remove object ID from the stream
				writer.Seek(-2, SeekOrigin.Current);
				writer.Write((short)0);
				writer.Seek(-2, SeekOrigin.Current);
			}
			else
			{
				// Write the end objectTag 
				writer.Write((short)0);
			}

			return;
		}


        /// <summary>
        /// Serializes the data point.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="writer">The writer.</param>
        private void SerializeDataPoint(object objectToSerialize, string elementName, BinaryWriter writer)
        {

            // Write object ID (hash of the name) into the writer
            writer.Write(SerializerBase.GetStringHashCode(elementName));
            // Remember position where object data is started
            long elementStartPosition = writer.Seek(0, SeekOrigin.Current);    

            DataPoint dataPoint = objectToSerialize as DataPoint;
            if (dataPoint.XValue != 0d && IsSerializableContent("XValue", objectToSerialize))
            {
                SerializeProperty(dataPoint.XValue, dataPoint, "XValue", writer);
            }
            if (dataPoint.YValues.Length > 0 && IsSerializableContent("YValues", objectToSerialize))
            {
                SerializeProperty(dataPoint.YValues, dataPoint, "YValues", writer);
            }
            if (dataPoint.IsEmpty && IsSerializableContent("IsEmpty", objectToSerialize))
            {
                SerializeProperty(dataPoint.IsEmpty, dataPoint, "IsEmpty", writer);
            }
            bool hasCustomProperties = false;
            foreach (DictionaryEntry entry in dataPoint.properties)
            {
                if (entry.Key is int)
                {
                    CommonCustomProperties propertyType = (CommonCustomProperties)((int)entry.Key);
                    String properyName = propertyType.ToString();
                    if (IsSerializableContent(properyName, objectToSerialize))
                    {
                        SerializeProperty(entry.Value, dataPoint, properyName, writer);
                    }
                }
                else
                {
                    hasCustomProperties = true;
                }
            }

            if (hasCustomProperties && !String.IsNullOrEmpty(dataPoint.CustomProperties) && IsSerializableContent("CustomProperties", objectToSerialize))
            {
                SerializeProperty(dataPoint.CustomProperties, dataPoint, "CustomProperties", writer);
            }  

            // If position of the writer did not change - nothing was written
            if (writer.Seek(0, SeekOrigin.Current) == elementStartPosition)
            {
                // Remove object ID from the stream
                writer.Seek(-2, SeekOrigin.Current);
                writer.Write((short)0);
                writer.Seek(-2, SeekOrigin.Current);
            }
            else
            {
                // Write the end objectTag 
                writer.Write((short)0);
            }
        }


        /// <summary>
		/// Serialize specified object into the binary writer.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="writer">Binary writer.</param>
        virtual internal void SerializeCollection(object objectToSerialize, string elementName, BinaryWriter writer)
		{
            ICollection collection = objectToSerialize as ICollection;
			if(collection != null)
			{
				// Write object ID (hash of the name) into the writer
				writer.Write(SerializerBase.GetStringHashCode(elementName));

				// Remember position where object data is started
				long elementStartPosition = writer.Seek(0, SeekOrigin.Current);

				// Enumerate through all objects in collection and serialize them
                foreach (object obj in collection)
				{

                    if (obj is DataPoint)
                    {
                        SerializeDataPoint(obj, GetObjectName(obj), writer);
                        continue;
                    }

                    SerializeObject(obj, objectToSerialize, GetObjectName(obj), writer);
				}

				// If position of the writer did not change - nothing was written
				if(writer.Seek(0, SeekOrigin.Current) == elementStartPosition)
				{
					// Remove object ID from the stream
					writer.Seek(-2, SeekOrigin.Current);
					writer.Write((short)0);
					writer.Seek(-2, SeekOrigin.Current);
				}
				else
				{
					// Write the end objectTag 
					writer.Write((short)0);
				}

			}
		}

		/// <summary>
		/// Serialize specified object into the binary writer.
		/// Method is called recursively to serialize child objects.
		/// </summary>
		/// <param name="objectToSerialize">Object to be serialized.</param>
		/// <param name="parent">Parent of the serialized object.</param>
		/// <param name="elementName">Object element name.</param>
		/// <param name="writer">Binary writer.</param>
        virtual internal void SerializeProperty(object objectToSerialize, object parent, string elementName, BinaryWriter writer)
		{
			// Check input parameters
			if(objectToSerialize == null || parent == null)
			{
				return;
			}

			// Check if property has non-default value
			PropertyDescriptor pd = TypeDescriptor.GetProperties(parent)[elementName];
			if(pd != null)
			{
				DefaultValueAttribute defValueAttribute = (DefaultValueAttribute)pd.Attributes[typeof(DefaultValueAttribute)];
				if(defValueAttribute != null)
				{
					if(objectToSerialize.Equals(defValueAttribute.Value))
					{
						// Do not serialize properties with default values
						return;
					}
				}
				else
				{
					// Check if property has "ShouldSerialize" method
					MethodInfo mi = parent.GetType().GetMethod("ShouldSerialize" + elementName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
					if(mi != null)
					{
						object result = mi.Invoke(parent, null);
						if(result is bool && ((bool)result) == false)
						{
							// Do not serialize properties with default values
							return;
						}
					}
                }
			
				// Check XmlFormatSerializerStyle attribute
				SerializationVisibilityAttribute	styleAttribute = (SerializationVisibilityAttribute)pd.Attributes[typeof(SerializationVisibilityAttribute)];
				if(styleAttribute != null)
				{
					// Hidden property
					if(styleAttribute.Visibility == SerializationVisibility.Hidden)
					{
						return;
					}
				}
			}

			// Write property 
			WritePropertyValue(objectToSerialize, elementName, writer);
		}

		/// <summary>
		/// Converts object value into the string.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="elementName">Object name.</param>
		/// <param name="writer">Binary writer.</param>
		/// <returns>Object value as strig.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "Too large of a code change to justify making this change")]
        internal void WritePropertyValue(object obj, string elementName, BinaryWriter writer)
		{
			// Write property ID (hash of the name) into the writer
			writer.Write(SerializerBase.GetStringHashCode(elementName));
			
			if(obj is bool)
			{
				writer.Write(((bool)obj));
			}
			else if(obj is double)
			{
				writer.Write(((double)obj));
			}
			else if(obj is string)
			{
				writer.Write(((string)obj));
			}
			else if(obj is int)
			{
				writer.Write(((int)obj));
			}
			else if(obj is long)
			{
				writer.Write(((long)obj));
			}
			else if(obj is float)
			{
				writer.Write(((float)obj));
			}
			else if(obj.GetType().IsEnum)
			{
				// NOTE: Using 'ToString' method instead of the 'Enum.GetName' fixes
				// an issue (#4314 & #4424) with flagged enumerations when there are
				// more then 1 values set.
				string enumValue = obj.ToString();
				writer.Write(enumValue);
			}

			else if(obj is byte)
			{
				// Write as long
				writer.Write((byte)obj);
			}

#if !Microsoft_CONTROL
			else if(obj is Unit)
			{
				writer.Write(((Unit)obj).Value);
			}
#endif

			else if(obj is Font)
			{
				// Write as string
				writer.Write(SerializerBase.FontToString((Font)obj));
			}

			else if(obj is Color)
			{
				// Write as int
				writer.Write(((Color)obj).ToArgb());
			}

			else if(obj is DateTime)
			{
				// Write as long
				writer.Write(((DateTime)obj).Ticks);
			}

			else if(obj is Size)
			{
				// Write as two integers
				writer.Write(((Size)obj).Width);
				writer.Write(((Size)obj).Height);
			}

			else if(obj is double[])
			{
				double[] arr = (double[])obj;

				// Write the size of the array (int)
				writer.Write(arr.Length);

				// Write each element of the array
				foreach(double d in arr)
				{
					writer.Write(d);
				}
			}
			
			else if(obj is Color[])
			{
				Color[] arr = (Color[])obj;

				// Write the size of the array (int)
				writer.Write(arr.Length);

				// Write each element of the array
				foreach(Color color in arr)
				{
					writer.Write(color.ToArgb());
				}
			}

			else if(obj is System.Drawing.Image)
			{
				// Save image into the memory stream
				MemoryStream imageStream = new MemoryStream();
				((System.Drawing.Image)obj).Save(imageStream, ((System.Drawing.Image)obj).RawFormat);
				
				// Write the size of the data
				int imageSize = (int)imageStream.Seek(0, SeekOrigin.End);
				imageStream.Seek(0, SeekOrigin.Begin);
				writer.Write(imageSize);

				// Write the data
				writer.Write(imageStream.ToArray());

				imageStream.Close();
			}



			else if(obj is Margins)
			{
				// Write as 4 integers
				writer.Write(((Margins)obj).Top);
				writer.Write(((Margins)obj).Bottom);
				writer.Write(((Margins)obj).Left);
				writer.Write(((Margins)obj).Right);
			}



			else
			{
                throw (new InvalidOperationException(SR.ExceptionChartSerializerBinaryTypeUnsupported(obj.GetType().ToString())));
			}
		}

		/// <summary>
		/// Checks if all properties will have a unique ID.
		/// Property ID is a hash of it's name.
		/// !!!USED IN DEBUG BUILD ONLY!!!
		/// </summary>
		/// <param name="propNames">Array of properties names.</param>
        internal void CheckPropertiesID(ArrayList propNames)
		{
#if DEBUG
			if(propNames != null)
			{
				// Loop through all properties and check the hash values
				foreach(string name1 in propNames)
				{
					foreach(string name2 in propNames)
					{
						if(name1 != name2)
						{
							if( SerializerBase.GetStringHashCode(name1) == SerializerBase.GetStringHashCode(name2) )
							{
                                throw (new InvalidOperationException(SR.ExceptionChartSerializerBinaryHashCodeDuplicate(name1,name2)));
							}
						}
					}
				}
			}
#endif
		}
		#endregion

		#region Deserialization methods

		/// <summary>
		/// Deserialize specified object from the source using binary format.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="source">Deserialization source.</param>
        internal override void Deserialize(object objectToDeserialize, object source)
        {
            // Check input parameters
            if (objectToDeserialize == null)
            {
                throw (new ArgumentNullException("objectToDeserialize"));
            }
            if (source == null)
            {
                throw (new ArgumentNullException("source"));
            }

            string sourceStr = source as string;
            if (sourceStr != null)
            {
                Deserialize(objectToDeserialize, sourceStr);
                return;
            }

            Stream stream = source as Stream;
            if (stream != null)
            {
                Deserialize(objectToDeserialize, stream);
                return;
            }

            BinaryWriter binaryWriter = source as BinaryWriter;
            if (binaryWriter != null)
            {
                Deserialize(objectToDeserialize, binaryWriter);
                return;
            }

            throw (new ArgumentException(SR.ExceptionChartSerializerSourceObjectInvalid, "source"));
        }

		/// <summary>
		/// Deserialize object from the file using binary format.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="fileName">File name to read the data from.</param>
		public void Deserialize(object objectToDeserialize, string fileName)
		{
			FileStream stream = new FileStream(fileName, FileMode.Open);
			Deserialize(objectToDeserialize, new BinaryReader(stream));
			stream.Close();
		}

		/// <summary>
		/// Deserialize object from the stream using binary format.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="stream">Stream to read the data from.</param>
		public void Deserialize(object objectToDeserialize, Stream stream)
		{
			Deserialize(objectToDeserialize, new BinaryReader(stream));
		}

		/// <summary>
		/// Deserialize object from different types of readers using binary format.
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="reader">Binary reader.</param>
		public void Deserialize(object objectToDeserialize, BinaryReader reader)
		{
			// Check input parameters
			if(objectToDeserialize == null)
			{
				throw(new ArgumentNullException("objectToDeserialize"));
			}
			if(reader == null)
			{
				throw(new ArgumentNullException("reader"));
			}

			// Binary deserializer do not support IsUnknownAttributeIgnored property
			if(base.IsUnknownAttributeIgnored)
			{
                throw (new InvalidOperationException(SR.ExceptionChartSerializerBinaryIgnoreUnknownAttributesUnsupported));
			}

			// Read 15 characters of file header
			char[]	header = reader.ReadChars(15);
			if(header[0] != 'D' || header[1] != 'C' || header[2] != 'B' || header[3] != 'F')
			{
                throw (new InvalidOperationException(SR.ExceptionChartSerializerBinaryFromatInvalid));
			}

			// Get ID of the root object
			this.ReadHashID(reader);
 
			// Reset properties of the root object
			if(IsResetWhenLoading)
			{
				ResetObjectProperties(objectToDeserialize);
			}

			// Deserialize object
			DeserializeObject(objectToDeserialize, null, GetObjectName(objectToDeserialize), reader, false);
		}

        /// <summary>
        /// Deserialize object from the binary format.
        /// Method is called recursively to deserialize child objects.
        /// </summary>
        /// <param name="objectToDeserialize">Object to be deserialized.</param>
        /// <param name="parent">Parent of the deserialized object.</param>
        /// <param name="elementName">Object element name.</param>
        /// <param name="reader">Binary reader object.</param>
        /// <param name="skipElement">if set to <c>true</c> the element will not be set.</param>
        /// <returns>Number of properties set.</returns>
		virtual protected int DeserializeObject(object objectToDeserialize, object parent, string elementName, BinaryReader reader, bool skipElement)
		{
			int	setPropertiesNumber = 0;

			// Check input parameters
			if(objectToDeserialize == null)
			{
				return setPropertiesNumber;
			}

			// Special handling for the collections
			Type[] assemblyTypes = null;
			int	listItemIndex = 0;

            IList list = objectToDeserialize as IList;

			if(list != null)
			{
				// Loop through all list items
				Int16 typeHash = 0;
                PropertyInfo listItemPI = objectToDeserialize.GetType().GetProperty("Item", new Type[] { typeof(int) });
                while ((typeHash = this.ReadHashID(reader)) != 0)
				{
					// Get collection item type from hashed type name
					string	typeName = String.Empty;
					if(listItemPI != null)
					{
                        if ((SerializerBase.GetStringHashCode(listItemPI.PropertyType.Name)) == typeHash)
                        {
                            typeName = listItemPI.PropertyType.Name;
                        }
                        else
                        {
                            Assembly assembly = listItemPI.PropertyType.Assembly;
                            if (assembly != null)
                            {
                                // Find all classes derived from this type
                                if (assemblyTypes == null)
                                {
                                    assemblyTypes = assembly.GetExportedTypes();
                                }
                                foreach (Type type in assemblyTypes)
                                {
                                    if (type.IsSubclassOf(listItemPI.PropertyType))
                                    {
                                        if ((SerializerBase.GetStringHashCode(type.Name)) == typeHash)
                                        {
                                            typeName = type.Name;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
					}
		
					// Create new item object
					string itemName = null;
					bool	reusedObject = false;
					object listItem = GetListNewItem(list, typeName, ref itemName, ref reusedObject);


					// Deserialize list item object
					int itemSetProperties = DeserializeObject(listItem, objectToDeserialize, "", reader, skipElement);

                    // Add item object into the list
                    if (!skipElement && (itemSetProperties > 0 || reusedObject))
                    {
                        list.Insert(listItemIndex++, listItem);
                    }
                    // TD: here was removed a code which doesn't work but cause heavy workload: GetListNewItem removes the reusedObject from the list.
                    // Add properties set for collection item
					setPropertiesNumber += itemSetProperties;
				}

				return setPropertiesNumber;
			}	

			// Get list of object's properties
			PropertyInfo[] properties = objectToDeserialize.GetType().GetProperties();
			if(properties == null)
			{
				return setPropertiesNumber;
			}
            
			// Get property information by reading the ID
			PropertyInfo pi = null;
            while ( (pi = ReadPropertyInfo(objectToDeserialize, parent, properties, reader)) != null)
			{
				// Read simple properties
				if(ShouldSerializeAsAttribute(pi, objectToDeserialize))
				{
					if(SetPropertyValue(objectToDeserialize, pi, reader, skipElement))
					{
						++setPropertiesNumber;
					}
				}

				else
				{
					// Get property descriptor
					PropertyDescriptor pd = TypeDescriptor.GetProperties(objectToDeserialize)[pi.Name];
					if(pd != null)
					{
						object innerObject = pd.GetValue(objectToDeserialize);
						
						// Deserialize inner item object
                        setPropertiesNumber += DeserializeObject(innerObject, objectToDeserialize, pi.Name, reader, !IsSerializableContent(pi.Name, objectToDeserialize));
					}
					else if(!IsUnknownAttributeIgnored)
					{
						throw(new InvalidOperationException(SR.ExceptionChartSerializerPropertyNameUnknown( pi.Name,objectToDeserialize.GetType().ToString())));
					}
				}
			}

			return setPropertiesNumber;
		}

        /// <summary>
        /// Reads and sets a property of an object.
        /// </summary>
        /// <param name="obj">Object to set.</param>
        /// <param name="pi">Property information.</param>
        /// <param name="reader">Binary reader.</param>
        /// <param name="skipElement">if set to <c>true</c> the property will not be set.</param>
        /// <returns>True if property was set.</returns>
		private bool SetPropertyValue(object obj, PropertyInfo pi, BinaryReader reader, bool skipElement)
		{
			if(pi != null)
			{
				object objValue = null;


				if(pi.PropertyType == typeof(bool))
				{
					objValue = reader.ReadBoolean();
				}
				else if(pi.PropertyType == typeof(double))
				{
					objValue = reader.ReadDouble();
				}
				else if(pi.PropertyType == typeof(string))
				{
					objValue = reader.ReadString();
				}
				else if(pi.PropertyType == typeof(int))
				{
					objValue = reader.ReadInt32();
				}
				else if(pi.PropertyType == typeof(long))
				{
					objValue = reader.ReadInt64();
				}
				else if(pi.PropertyType == typeof(float))
				{
					objValue = reader.ReadSingle();
				}
				else if(pi.PropertyType.IsEnum)
				{
					// Read as string
					objValue = Enum.Parse(pi.PropertyType, reader.ReadString());
				}
				else if(pi.PropertyType == typeof(byte))
				{
					objValue = reader.ReadByte();
				}

#if !Microsoft_CONTROL
				else if(pi.PropertyType == typeof(Unit))
				{
					objValue = new Unit((double)reader.ReadDouble());
				}
#endif

				else if(pi.PropertyType == typeof(Font))
				{
					// Read as string
					objValue = SerializerBase.FontFromString(reader.ReadString());
				}

				else if(pi.PropertyType == typeof(Color))
				{
					// Read as int
					objValue = Color.FromArgb(reader.ReadInt32());
				}

				else if(pi.PropertyType == typeof(DateTime))
				{
					// Read as long
					objValue = new DateTime(reader.ReadInt64());
				}

				else if(pi.PropertyType == typeof(Size))
				{
					// Read as two integers
					objValue = new Size(reader.ReadInt32(), reader.ReadInt32());
				}



				else if(pi.PropertyType == typeof(Margins) )
				{
					// Read as 4 integers
					objValue = new Margins(
						reader.ReadInt32(), 
						reader.ReadInt32(), 
						reader.ReadInt32(), 
						reader.ReadInt32());
				}



				else if(pi.PropertyType == typeof(double[]))
				{
					// Allocate array
					double[] array = new double[reader.ReadInt32()];

					// Read each element of the array
					for(int arrayIndex = 0; arrayIndex < array.Length; arrayIndex++)
					{
						array[arrayIndex] = reader.ReadDouble();
					}

					objValue = array;
				}

				else if(pi.PropertyType == typeof(Color[]))
				{
					// Allocate array
					Color[] array = new Color[reader.ReadInt32()];

					// Read each element of the array
					for(int arrayIndex = 0; arrayIndex < array.Length; arrayIndex++)
					{
						array[arrayIndex] = Color.FromArgb(reader.ReadInt32());
					}

					objValue = array;
				}

				else if(pi.PropertyType == typeof(System.Drawing.Image))
				{	
					// Get image data size
					int imageSize = reader.ReadInt32();

					// Create image stream
					MemoryStream imageStream = new MemoryStream(imageSize + 10);

					// Copy image data into separate stream
					imageStream.Write(reader.ReadBytes(imageSize), 0, imageSize);

					// Create image object
					objValue = new Bitmap(System.Drawing.Image.FromStream(imageStream));	// !!! .Net bug when image source stream is closed - can create brush using the image

					// Close image stream
					imageStream.Close();
				}			

				else
				{
					throw(new InvalidOperationException(SR.ExceptionChartSerializerBinaryTypeUnsupported( obj.GetType().ToString() )));
				}


				// Check if this property is serializable content
                if (!skipElement && IsSerializableContent(pi.Name, obj))
				{
					// Set object value
					pi.SetValue(obj, objValue, null);

					return true;
				}
			}
		
			return false;
		}

		/// <summary>
		/// Reads property ID and return property information
		/// </summary>
		/// <param name="objectToDeserialize">Object to be deserialized.</param>
		/// <param name="parent">Parent of the deserialized object.</param>
		/// <param name="properties">List of properties information.</param>
		/// <param name="reader">Binary reader.</param>
		/// <returns>Property information.</returns>
		private PropertyInfo ReadPropertyInfo(object objectToDeserialize, object parent, PropertyInfo[] properties, BinaryReader reader)
		{
			// Read property ID
			short	propertyID = this.ReadHashID(reader);

			// End objectTag reached
			if(propertyID == 0)
			{
				return null;
			}

			// Loop through all properties and check properties IDs (hash code of name)
			foreach(PropertyInfo pi in properties)
			{
				// Skip inherited properties from the root object
				if(IsChartBaseProperty(objectToDeserialize, parent, pi))
				{
					continue;
				}

				// Check collection
                if (pi.CanRead && pi.PropertyType.GetInterface("ICollection", true) != null)
				{
					if((SerializerBase.GetStringHashCode(pi.Name)) == propertyID)
					{
						return pi;
					}
				}

				// Check public properties with Get and Set methods
				else if(pi.CanRead && pi.CanWrite)
				{
					// Skip indexes
					if(pi.Name == "Item")
					{
						continue;
					}

					if((SerializerBase.GetStringHashCode(pi.Name)) == propertyID)
					{
						return pi;
					}
				}
			}

			// Property was not found
            throw (new InvalidOperationException(SR.ExceptionChartSerializerPropertyNotFound));
		}

		#endregion
	}
}
