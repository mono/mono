//
// System.Windows.Forms.TreeNodeConverter
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
namespace System.Windows.Forms {

	// <summary>
	// </summary>
using System.ComponentModel;
    public class TreeNodeConverter : TypeConverter {

		
		//  --- Public Constructors
		
		[MonoTODO]
		public TreeNodeConverter()
		{
			//
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			//FIXME: return types that we can convert to
			return false;
		}
		[MonoTODO]
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			throw new NotImplementedException ();
			//FIXME: Implment our own conversion
			//return base.CanConvertTo(context, culture, value, destinationType);
		}
	}
}
