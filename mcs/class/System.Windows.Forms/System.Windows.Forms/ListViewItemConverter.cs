//
// System.Windows.Forms.ListViewItemConverter.cs
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

		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			//FIXME:
			return base.CanConvertTo(context, type);
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object obj, Type type)
		{
			//FIXME:
			return base.ConvertTo(context, type);
		}
	 }
}
