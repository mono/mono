//
// System.Windows.Forms.ImageIndexConverter.cs
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
