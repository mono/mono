//
// System.Web.Util.TimeUtil
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc, (http://www.novell.com)
//

using System;

namespace System.Web.Util {

	internal sealed class TimeUtil {
		
		private TimeUtil () { }
		
		internal static string ToUtcTimeString (DateTime dt)
		{
			return dt.ToUniversalTime ().ToString ("ddd, d MMM yyyy HH:mm:ss ") + "GMT";
		}
	}
}

