//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Web;

namespace System.Runtime.Remoting.Channels.Http 
{
	public class HttpRemotingHandlerFactory : IHttpHandlerFactory
	{
		[MonoTODO]
		public HttpRemotingHandlerFactory ()
		{
		}

		[MonoTODO]
		public IHttpHandler GetHandler (HttpContext context,
						string verb,
						string url,
						string filePath)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ReleaseHandler (IHttpHandler handler)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~HttpRemotingHandlerFactory()
		{
		}
	}
}
