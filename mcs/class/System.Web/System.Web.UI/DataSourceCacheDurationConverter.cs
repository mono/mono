//
// System.Web.UI.DataSourceCacheDurationConverter.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
//
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


using System.Collections;
using System.ComponentModel;
using System.Globalization;

#if NET_2_0
namespace System.Web.UI
{
	public class DataSourceCacheDurationConverter : Int32Converter
	{
		public DataSourceCacheDurationConverter () {
			throw new NotImplementedException ();
		}
		public bool CanConvertFrom (Type sourceType) {
			throw new NotImplementedException ();
		}
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			throw new NotImplementedException ();
		}
		public bool CanConvertTo (Type destinationType) {
			throw new NotImplementedException ();
		}
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) {
			throw new NotImplementedException ();
		}
		public Object ConvertFrom (Object value) {
			throw new NotImplementedException ();
		}
		public override Object ConvertFrom (ITypeDescriptorContext context,
											CultureInfo culture,
											Object value) {
			throw new NotImplementedException ();
		}
		public Object ConvertTo (Object value, Type destinationType) {
			throw new NotImplementedException ();
		}
		public override Object ConvertTo (ITypeDescriptorContext context,
										CultureInfo culture,
										Object value,
										Type destinationType) {
			throw new NotImplementedException ();
		}
		public ICollection GetStandardValues () {
			throw new NotImplementedException ();
		}
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
		public bool GetStandardValuesExclusive () {
			throw new NotImplementedException ();
		}
		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
		public bool GetStandardValuesSupported () {
			throw new NotImplementedException ();
		}
		public override bool GetStandardValuesSupported (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
	}
}
#endif
