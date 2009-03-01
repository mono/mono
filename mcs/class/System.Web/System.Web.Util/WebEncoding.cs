//
// System.Web.Util.WebEncoding
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Web.Configuration;

namespace System.Web.Util
{
	internal class WebEncoding
	{
#if NET_2_0
		static bool cached;
		static GlobalizationSection sect;
		static GlobalizationSection GlobalizationConfig {
			get {
				if (!cached) {
					try {
						sect = (GlobalizationSection) WebConfigurationManager.GetWebApplicationSection ("system.web/globalization");
					}
					catch { }
					cached = true;
				}
				return sect;
			}
		}
#else
		static GlobalizationConfiguration GlobalizationConfig {
			get {
				return GlobalizationConfiguration.GetInstance (null);
			}
		}
#endif

		static public Encoding FileEncoding {
			get {
				return GlobalizationConfig != null ? GlobalizationConfig.FileEncoding : Encoding.Default;
			}
		}

		static public Encoding ResponseEncoding {
			get {
				return GlobalizationConfig != null ? GlobalizationConfig.ResponseEncoding : Encoding.Default;
			}
		}

		static public Encoding RequestEncoding {
			get {
				return GlobalizationConfig != null ? GlobalizationConfig.RequestEncoding : Encoding.Default;
			}
		}
	}
}

