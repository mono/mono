//
// System.ComponentModel.TypeConverter.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public class TypeConverter
	{
		public bool CanConvertFrom (Type sourceType)
		{
			return CanConvertFrom (null, sourceType);
		}

		public virtual bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return false;
		}

		public bool CanConvertTo (Type destinationType)
		{
			return CanConvertTo (null, destinationType);
		}

		public virtual bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return (destinationType == typeof (string));
		}

		public object ConvertFrom (object o)
		{
			return ConvertFrom (null, CultureInfo.CurrentCulture, o);
		}

		public virtual object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			throw new NotSupportedException (this.ToString() + " cannot be created from '" +
						         value.GetType().ToString() + "'");
		}

		public object ConvertFromInvariantString (string text)
		{
			return ConvertFromInvariantString (null, text); 
		}

		public object ConvertFromInvariantString (ITypeDescriptorContext context, string text)
		{
			return ConvertFromString (context, CultureInfo.InvariantCulture, text);
		}

		public object ConvertFromString (string text)
		{
			return ConvertFrom (text);
		}

		public object ConvertFromString (ITypeDescriptorContext context, string text)
		{
			return ConvertFromString (context, CultureInfo.CurrentCulture, text);
		}

		public object ConvertFromString (ITypeDescriptorContext context, CultureInfo culture, string text)
		{
			return ConvertFrom (context, culture, text);
		}

		public object ConvertTo (object value, Type destinationType)
		{
			return ConvertTo (null, null, value, destinationType);
		}

		public virtual object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						 Type destinationType)
		{
			// context? culture?
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (destinationType == typeof (string)) {
				if (value != null)
					return value.ToString();
				return String.Empty;
			}

			throw new NotSupportedException ("Conversion not supported");
		}

		public string ConvertToInvariantString (object value)
		{
			return ConvertToInvariantString (null, value);
		}

		public string ConvertToInvariantString (ITypeDescriptorContext context, object value)
		{
			return (string) ConvertTo (context, CultureInfo.InvariantCulture, value, typeof (string));
		}

		public string ConvertToString (object value)
		{
			return (string) ConvertTo (null, CultureInfo.CurrentCulture, value, typeof (string));
		}

		public string ConvertToString (ITypeDescriptorContext context, object value)
		{
			return (string) ConvertTo (context, CultureInfo.CurrentCulture, value, typeof (string));
		}

		public string ConvertToString (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return (string) ConvertTo (context, culture, value, typeof (string));
		}

		protected Exception GetConvertFromException (object value)
		{
			throw new NotSupportedException (this.ToString() + " cannot convert from '" + 
							value.GetType().ToString() + "'");
		}

		protected Exception GetConvertToException (object value, Type destinationType)
		{
			throw new NotSupportedException (this.ToString() + " cannot convert from '" + 
							value.GetType().ToString() + "' to '" + destinationType.ToString() + "'");
		}

		public object CreateInstance (IDictionary propertyValues)
		{
			return CreateInstance (null, propertyValues);
		}

		public virtual object CreateInstance (ITypeDescriptorContext context, IDictionary propertyValues)
		{
			return null;
		}

		public bool GetCreateInstanceSupported ()
		{
			return GetCreateInstanceSupported (null);
		}

		public virtual bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return false;
		}

		public PropertyDescriptorCollection GetProperties (object value)
		{
			return GetProperties (null, value);
		}

		public PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value)
		{
			return GetProperties (context, value, null);
		}

		public virtual PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									   object value, Attribute[] attributes)
		{
			return null;
		}

		public bool GetPropertiesSupported ()
		{
			return GetPropertiesSupported (null);
		}

		public virtual bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return false;
		}

		public ICollection GetStandardValues ()
		{
			return GetStandardValues (null);
		}

		public virtual StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return null;
		}

		public bool GetStandardValuesExclusive ()
		{
			return GetStandardValuesExclusive (null);
		}

		public virtual bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public bool GetStandardValuesSupported ()
		{
			return GetStandardValuesSupported (null);
		}

		public virtual bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return false;
		}

		public bool IsValid (object value)
		{
			return IsValid (null, value);
		}

		public virtual bool IsValid (ITypeDescriptorContext context, object value)
		{
			return true;
		}

		protected PropertyDescriptorCollection SortProperties (PropertyDescriptorCollection props, string[] names)
		{
			props.Sort (names);
			return props; 
		}

		public class StandardValuesCollection : ICollection, IEnumerable
		{
			private ICollection values;

			public StandardValuesCollection (ICollection values)
			{
				this.values = values;
			}

			public void CopyTo (Array array, int index)
			{
				values.CopyTo (array, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return values.GetEnumerator ();
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return null; }
			}

			int ICollection.Count {
				get { return this.Count; }
			}

			public int Count {
				get { return values.Count; }
			}

			public object this [int index] {
				get { return ((IList) values) [index]; }
			}
		}

		protected abstract class SimplePropertyDescriptor : PropertyDescriptor
		{
			private Type componentType;
			private Type propertyType;

			public SimplePropertyDescriptor (Type componentType,
							 string name,
							 Type propertyType) :
				this (componentType, name, propertyType, new Attribute [0])
			{
			}

			public SimplePropertyDescriptor (Type componentType,
							 string name,
							 Type propertyType,
							 Attribute [] attributes) : base (name, attributes)
			{
				this.componentType = componentType;
				this.propertyType = propertyType;
			}

			public override Type ComponentType {
				get { return componentType; }
			}

			public override Type PropertyType {
				get { return propertyType; }
			}

			public override bool IsReadOnly {
				get { return Attributes.Contains (ReadOnlyAttribute.Yes); }
			}

			public override bool ShouldSerializeValue (object component)
			{
					return false; 
			}

			public override bool CanResetValue (object component)
			{
				DefaultValueAttribute Attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
				if (Attrib == null) {
					return false; 
				}
				return (Attrib.Value == GetValue (component)); 
			}

			public override void ResetValue (object component)
			{
				DefaultValueAttribute Attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
				if (Attrib != null) {
					SetValue (component, Attrib.Value); 
				}
 
			} 
		}
	}
}

