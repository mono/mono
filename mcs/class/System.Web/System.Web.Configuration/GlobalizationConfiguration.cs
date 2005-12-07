// 
// System.Web.Configuration.GlobalizationConfiguration
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Text;

namespace System.Web.Configuration
{
	class GlobalizationConfiguration
	{
		internal Encoding RequestEncoding;
		internal Encoding ResponseEncoding;
		internal Encoding FileEncoding;
		internal CultureInfo Culture;
		internal CultureInfo UICulture;

		internal GlobalizationConfiguration (object p)
		{
			if (!(p is GlobalizationConfiguration))
				return;

			GlobalizationConfiguration parent = (GlobalizationConfiguration) p;
			RequestEncoding = parent.RequestEncoding;
			ResponseEncoding = parent.ResponseEncoding;
			FileEncoding = parent.FileEncoding;
			Culture = parent.Culture;
			UICulture = parent.UICulture;
		}

		static public GlobalizationConfiguration GetInstance (HttpContext context)
		{
			GlobalizationConfiguration config;
			try {
				if (context == null)
					config = HttpContext.GetAppConfig ("system.web/globalization")
						 as GlobalizationConfiguration;
				else
					config = context.GetConfig ("system.web/globalization")
						 as GlobalizationConfiguration;
			} catch {
				return null;
			}
			return config;
		}

	}
}

