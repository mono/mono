//
// System.Windows.Forms.DataGridPreferredColumnWidthTypeConverter
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

