//
// System.Web.UI.Design.UrlBuilder
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel;

namespace System.Web.UI.Design
{
	public sealed class UrlBuilder
	{
		private UrlBuilder ()
		{
		}

		[MonoTODO]
		public static string BuildUrl (IComponent component, System.Windows.Forms.Control owner, string initialUrl, string caption, string filter)
		{
			return UrlBuilder.BuildUrl (component, owner, initialUrl, 
				caption, filter, UrlBuilderOptions.None);
		}

		[MonoTODO]
		public static string BuildUrl (IComponent component, System.Windows.Forms.Control owner, string initialUrl, string caption, string filter, UrlBuilderOptions options)
		{
			throw new NotImplementedException ();
		}
	}
}
