//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Web;

namespace System.Runtime.Remoting.Channels.Http 
{
        public class HttpRemotingHandler : IHttpHandler 
	{
		[MonoTODO]
		public HttpRemotingHandler()
		{
		}

		public bool IsReusable {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public void ProcessRequest (HttpContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~HttpRemotingHandler()
		{
		}
	}	
}
