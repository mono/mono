//
// System.Windows.Forms.CursorConverter.cs
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
	/// Provides a type converter to convert Cursor objects to and from various other representations.
	///
	/// </summary>

	[MonoTODO]
	public class CursorConverter : TypeConverter {
		
		#region Constructors
		[MonoTODO]
		public CursorConverter() 
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
		public override bool CanConvertTo(ITypeDescriptorContext context,Type destinationType) 
		{
			//FIXME:
			return base.CanConvertTo(context, destinationType);
		}
		
		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context,CultureInfo culture,object value) 
		{
			//FIXME:
			return base.ConvertFrom(context, culture, value);
		}
		
		[MonoTODO]
		public override object ConvertTo(ITypeDescriptorContext context,CultureInfo culture,object value,Type destinationType) 
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
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) 
		{
			//FIXME:
			return base.GetStandardValuesSupported(context);
		}
		#endregion
	}
}
