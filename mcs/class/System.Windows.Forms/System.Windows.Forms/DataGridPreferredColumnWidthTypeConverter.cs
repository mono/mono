//
// System.Windows.Forms.DataGridPreferredColumnWidthTypeConverter
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms {

	/// <summary>
	/// Converts the value of an object to a different data type.
	/// </summary>
	
	[MonoTODO]
	public class DataGridPreferredColumnWidthTypeConverter : TypeConverter {

		#region Constructors
		[MonoTODO]
		public DataGridPreferredColumnWidthTypeConverter() 
		{
			
		}
		#endregion

		#region Methods
		[MonoTODO]
		public override bool CanConvertFrom(ITypeDescriptorContext context,Type sourceType) 
		{
			//FIXME:
			return base.CanConvertFrom(context, sourceType);
		}
		
		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context,CultureInfo culture,object value) 
		{
			//FIXME:
			return base.ConvertFrom(context, culture, value);
		}
		
		[MonoTODO]
		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType) {
			//FIXME:
			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion
		
	}
}

