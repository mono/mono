//
// System.Web.UI.ObjectConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

namespace System.Web.UI
{
	[Obsolete ("Use the System.Convert class and String.Format instead", false)]
	public sealed class ObjectConverter
	{
		public static object ConvertValue (object value, Type toType, string formatString)
		{
			throw new NotImplementedException ("Not implemented and [Obsolete]");
		}
	}
}

