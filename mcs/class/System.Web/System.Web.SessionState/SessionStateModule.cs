//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System.Web;

namespace System.Web.SessionState
{
	public sealed class SessionStateModule : IHttpModule
	{
		public SessionStateModule ()
		{
		}

		[MonoTODO()]
		public void Dispose ()
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void Init (HttpApplication app)
		{
			//throw new NotImplementedException ();
		}

		public event EventHandler Start;
		public event EventHandler End;
	}
}

