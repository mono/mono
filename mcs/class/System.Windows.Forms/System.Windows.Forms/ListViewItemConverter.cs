//
// System.Windows.Forms.ListViewItemConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Globalization;
using System.ComponentModel;
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class ListViewItemConverter : ExpandableObjectConverter {
		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListViewItemConverter()
		{
		}

		//
		//  --- Public Methods
		//

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			//FIXME:
			return base.CanConvertTo(context, destinationType);
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			//FIXME:
			return base.ConvertTo(context, destinationType);
		}
	 }
}
