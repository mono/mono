//
// System.Web.Security.FileAuthorizationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;

namespace System.Web.Security
{
	public sealed class FileAuthorizationModule : IHttpModule
	{
		public void Dispose ()
		{
		}

		[MonoTODO]
		public void Init (HttpApplication app)
		{
			throw new NotImplementedException ();
		}
	}
}

