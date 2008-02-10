//
// System.Web.UI.Design.DataSourceConverter
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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

namespace System.Web.UI.Design
{
	public class DataSourceConverter : TypeConverter
	{
		public DataSourceConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string));
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				return string.Empty;

			if (value.GetType () == typeof(string))
				return ((string) value);

			throw base.GetConvertFromException (value);
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return (context.Instance is IComponent);
		}

#if NET_2_0
		[MonoTODO]
		protected virtual bool IsValidDataSource (IComponent component)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
