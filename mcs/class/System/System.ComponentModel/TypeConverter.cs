//
// System.ComponentModel.TypeConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Globalization;

namespace System.ComponentModel {

[MonoTODO("Only has the minimal implementation needed to use ColorConverter")]
public class TypeConverter
{
	public TypeConverter ()
	{
	}

	public bool CanConvertFrom (Type sourceType)
	{
		return CanConvertFrom (null, sourceType);
	}
	
	[MonoTODO]
	public virtual bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
	{
		throw new NotImplementedException ();
	}

	public bool CanConvertTo (Type destinationType)
	{
		return CanConvertTo (null, destinationType);
	}
	
	[MonoTODO]
	public virtual bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
	{
		throw new NotImplementedException ();
	}

	public object ConvertFrom (object o)
	{
		return ConvertFrom (null, CultureInfo.CurrentCulture, o);
	}

	[MonoTODO]
	public virtual object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		throw new NotImplementedException ();
	}
	
	public virtual object ConvertFromInvariantString (string text)
	{
		return ConvertFromInvariantString (null, text); 
	}

	[MonoTODO]
	public virtual object ConvertFromInvariantString (ITypeDescriptorContext context, string text)
	{
		throw new NotImplementedException ();
	}

	public object ConvertFromString (string s)
	{
		return ConvertFrom (s);
	}

	public object ConvertFromString (ITypeDescriptorContext context, string text)
	{
		return ConvertFromString (context, CultureInfo.CurrentCulture, text);
	}

	[MonoTODO]
	public object ConvertFromString (ITypeDescriptorContext context, CultureInfo culture, string text)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public object ConvertTo (object value, Type destinationType)
	{
		throw new NotImplementedException ();
	}
	[MonoTODO]
	public virtual object ConvertTo (ITypeDescriptorContext context,
					 CultureInfo culture,
					 object value,
					 Type destinationType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string ConvertToInvariantString (object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string ConvertToInvariantString (ITypeDescriptorContext context, object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string ConvertToString (object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string ConvertToString (ITypeDescriptorContext context, object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string ConvertToString (ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public object CreateInstance (IDictionary propertyValues)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual object CreateInstance (ITypeDescriptorContext context, IDictionary propertyValues)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public bool GetCreateInstanceSupported ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual bool GetCreateInstanceSupported (ITypeDescriptorContext context)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public PropertyDescriptorCollection GetProperties (object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
								   object value,
								   Attribute[] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public bool GetPropertiesSupported ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual bool GetPropertiesSupported (ITypeDescriptorContext context)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public ICollection GetStandardValues ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public bool GetStandardValuesExclusive ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual bool GetStandardValuesExclusive (ITypeDescriptorContext context)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public bool GetStandardValuesSupported ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual bool GetStandardValuesSupported (ITypeDescriptorContext context)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public bool IsValid (object value)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual bool IsValid (ITypeDescriptorContext context, object value)
	{
		throw new NotImplementedException ();
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

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return null; }
		}

		int ICollection.Count
		{
			get { return this.Count; }
		}

		public int Count
		{
			get { return values.Count; }
		}

		public object this [int index]
		{
			get { return ((IList) values) [index]; }
		}
	}
}
}

