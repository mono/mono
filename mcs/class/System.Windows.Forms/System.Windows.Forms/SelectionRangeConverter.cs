//
// System.Windows.Forms.SelectionRangeConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
using System.Collections;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>
using System.ComponentModel;
    public class SelectionRangeConverter : TypeConverter {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public SelectionRangeConverter()
		{
			//FIXME:
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type type)
		{
			//FIXME:
			return base.CanConvertFrom(context,type);
		}

		[MonoTODO]
		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			//FIXME:
			return base.CanConvertTo(context, type);
		}
		
		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			//FIXME:
			return base.ConvertFrom(context, culture, value);
		}

		[MonoTODO]
		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type type)
		{
			//FIXME:
			return base.ConvertTo(context, culture, value, type);
		}

		[MonoTODO]
		public override object CreateInstance(ITypeDescriptorContext context, IDictionary dict)
		{
			//FIXME:
			return base.CreateInstance(context, dict );
		}

		[MonoTODO]
		public bool CreateInstanceSupported()
		{
			//FIXME:
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetCreateInstanceSupported(context);
		}
		
		//Not part of Spec??
		//[MonoTODO]
		//public override PropertyDescriptorCollection GetProperties( ITypeDescriptorContext context, object obj, Attribute[] atts)
		//{
		//	throw new NotImplementedException ();
		//}
		//
		//[MonoTODO]
		//public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		//{
		//	throw new NotImplementedException ();
		//}

	 }
}
