//
// System.Web.Configuration.HttpConfigurationContext
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.Configuration
{
	public class HttpConfigurationContext
	{
		private string virtualPath;

		internal HttpConfigurationContext (string virtualPath)
		{
			this.virtualPath = virtualPath;
		}

		public string VirtualPath
		{
			get {
				return virtualPath;
			}
		}
	}
}

