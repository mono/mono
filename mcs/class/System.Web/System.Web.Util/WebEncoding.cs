//
// System.Web.Util.WebEncoding
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;
using System.Web.Configuration;

namespace System.Web.Util
{
	internal class WebEncoding
	{
		static public Encoding FileEncoding {
			get {
				GlobalizationConfiguration gc = GlobalizationConfiguration.GetInstance (null);
				if (gc == null)
					return Encoding.Default;

				return gc.FileEncoding;
			}
		}

		static public Encoding ResponseEncoding {
			get {
				GlobalizationConfiguration gc = GlobalizationConfiguration.GetInstance (null);
				if (gc == null)
					return Encoding.Default;

				return gc.ResponseEncoding;
			}
		}

		static public Encoding RequestEncoding {
			get {
				GlobalizationConfiguration gc = GlobalizationConfiguration.GetInstance (null);
				if (gc == null)
					return Encoding.Default;

				return gc.RequestEncoding;
			}
		}
	}
}

