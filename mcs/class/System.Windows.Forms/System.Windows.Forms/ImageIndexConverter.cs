//
// System.Windows.Forms.ImageIndexConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Globalization;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class ImageIndexConverter : Int32Converter {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ImageIndexConverter()
		{
			
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context,
			CultureInfo culture, object value )
		{
			//FIXME:
			return base.ConvertFrom(context, culture, value);
		}


		[MonoTODO]
		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, 
			object value, Type destinationType)
		{
			//FIXME:
			return base.ConvertTo(context, culture, value, destinationType);
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValues(context);
		}

		[MonoTODO]
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValuesExclusive(context);
		}

		[MonoTODO]
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValuesSupported(context);
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		protected virtual bool IncludeNoneAsStandardValue {
			get {
				throw new NotImplementedException ();
			}
		}
	 }
}
