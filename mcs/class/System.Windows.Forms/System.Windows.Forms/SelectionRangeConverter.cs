//
// System.Windows.Forms.SelectionRangeConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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
