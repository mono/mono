//
// System.Runtime.Remoting.Channels.Http.HttpThread
//
// Authors:
//		Ahmad Tantawy (popsito82@hotmail.com)
//		Ahmad Kadry (kadrianoz@hotmail.com)
//		Hussein Mehanna (hussein_mehanna@hotmail.com)
//
// (C) 2003 Ximian, Inc.
//

using System;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Http
{

	internal class HttpThread
	{
		RequestArguments reqArg;
		public HttpThread(object Object)
		{
			if(Object as RequestArguments == null)
				return;

			reqArg = (RequestArguments)Object;

			Thread proc = new Thread(new ThreadStart(ProcessRequest));
			proc.IsBackground = true;
			proc.Start();

		}
		private void ProcessRequest()
		{
			HttpServer.ProcessRequest(reqArg);
		}
	}
}
