//
// System.Windows.Forms.KeysConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.Globalization;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>
    public class KeysConverter : TypeConverter, IComparer {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public KeysConverter()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Methods
		//
		//public bool CanConvertFrom(Type type)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool CanCompareTo(Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool CanCompareTo(ITypeDescriptorContext context, Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Compare(object a, object b)
		{
			throw new NotImplementedException ();
		}

		//public object ConvertFrom(object o)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object o)
		{
			throw new NotImplementedException ();
		}
		//public object ConvertFromInvariantString(string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object ConvertFromInvariantString( ITypeDescriptorContext context, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object ConvertFromString(string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object ConvertFromString( ITypeDescriptorContext context, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object ConvertTo( object o, Type t)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object o, Type t)
		{
			throw new NotImplementedException ();
		}
		//public string ConvertToInvariantString( object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public string ConvertToInvariantString( ITypeDescriptorContext context, object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public string ConvertToString( object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public string ConvertToString( ITypeDescriptorContext context , object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object CreateInstance(IDictionary dict)
		//{
		//	throw new NotImplementedException ();
		//}
		//public override object CreateInstance( ITypeDescriptorContext context, IDictionary dict)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		//public bool GetCreateInstanceSupported()
		//{
		//	throw new NotImplementedException ();
		//}
		//public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		//{
		//	throw new NotImplementedException ();
		//}
		//public PropertyDescriptorCollection GetProperties(object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public PropertyDescriptorCollection GetProperties( ITypeDescriptorContext context, object o)
		//{
		//	throw new NotImplementedException ();
		//}
		//public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context,  object o, Attribute[] attributes)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public bool GetProertiesSupported()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool GetProertiesSupported(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}
		//public ICollection GetStandardValues()
		//{
		//	throw new NotImplementedException ();
		//}
		//public bool GetStandardValuesExclusive()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		//public bool GetStandardValuesSupported()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		//public bool IsValid(object o)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool IsValid(ITypeDescriptorContext context, object o)
		{
			throw new NotImplementedException ();
		}
	 }
}
