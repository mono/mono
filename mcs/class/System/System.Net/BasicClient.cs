//
// System.Net.BasicClient
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Net
{
	class BasicClient : IAuthenticationModule
	{
		[MonoTODO]
		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials)
		{
			throw new NotImplementedException ();
		}

		public virtual string AuthenticationType {
			get { return "Basic"; }
		}

		public virtual bool CanPreAuthenticate {
			get { return true; }
		}

	}
}

