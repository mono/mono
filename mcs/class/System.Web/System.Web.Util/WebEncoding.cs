//
// System.Web.Util.WebEncoding
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Text;

namespace System.Web.Util
{
	internal class WebEncoding
	{
		static Encoding encoding = new UTF8Encoding (false);

		static public Encoding Encoding
		{
			get { return encoding; }
		}
	}
}

