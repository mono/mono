//
// System.Windows.Forms.ListBindingConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>
    public class ListBindingConverter : TypeConverter {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListBindingConverter()
		{
			
		}

		//
		//  --- Public Methods
		//
		//
		
		[MonoTODO]
		public override bool CanConvertTo(ITypeDescriptorContext context, Type t) 
		{
			//FIXME:
			return base.CanConvertTo(context, t);
		}

		[MonoTODO]
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object o, Type t) 
		{
			//FIXME:
			return base.ConvertTo(context, culture,o, t);
		}
		[MonoTODO]
		public override object CreateInstance(ITypeDescriptorContext context, IDictionary dict) 
		{
			//FIXME:
			return base.CreateInstance(context, dict);;
		}

		[MonoTODO]
		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) 
		{
			//FIXME:
			return base.GetCreateInstanceSupported(context);
		}
	 }
}
