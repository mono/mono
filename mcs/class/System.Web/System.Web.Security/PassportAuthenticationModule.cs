//
// System.Web.Security.PassportAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;

namespace System.Web.Security
{
	public sealed class PassportAuthenticationModule : IHttpModule
	{
		public event PassportAuthenticationEventHandler Authenticate;

		public void Dispose ()
		{
		}

		[MonoTODO("Will we ever implement this? :-)")]
		public void Init (HttpApplication app)
		{
			throw new NotImplementedException ();
		}
	}
}

