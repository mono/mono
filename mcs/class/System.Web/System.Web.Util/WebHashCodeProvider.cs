//
// System.Web.Util.WebHashCodeProvider.cs
//
// Authors:
//   Gaurav Vaish (my_scripts2001@yahoo.com, gvaish@iitk.ac.in)
//
// (c) Gaurav Vaish 2001
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
using System.Globalization;

namespace System.Web.Util
{
	internal class WebHashCodeProvider : IHashCodeProvider
	{
		private static IHashCodeProvider defHcp;

		public WebHashCodeProvider()
		{
		}
		
		int IHashCodeProvider.GetHashCode(object key)
		{
			return Default.GetHashCode(key);
		}

		public static IHashCodeProvider Default
		{
			get
			{
				if(defHcp==null)
				{
					 defHcp = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
				}
				return defHcp;
			}
		}
	}
}
