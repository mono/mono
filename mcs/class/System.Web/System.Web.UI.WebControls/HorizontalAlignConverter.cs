//
// System.Web.UI.WebControls.HorizontalAlignConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

namespace System.Web.UI.WebControls
{
	class HorizontalAlignConverter : EnumConverter
	{
		public HorizontalAlignConverter () : base (typeof(HorizontalAlign))
		{
		}

		// The base class is enough to handle everything.
		// The methods are here just to make the class status page happy.
		// Add some optimizations?

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			return base.ConvertFrom (context, culture, value);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

