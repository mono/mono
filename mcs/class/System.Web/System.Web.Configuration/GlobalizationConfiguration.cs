// 
// System.Web.Configuration.GlobalizationConfiguration
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
	}
}

