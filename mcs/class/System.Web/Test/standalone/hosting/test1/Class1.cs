using System;
using System.Web;
using System.Web.Hosting;

namespace ClassLibrary1
{
	public class Class1:MarshalByRefObject
	{
		public void Run ()
		{
			HttpRuntime.ProcessRequest (new SimpleWorkerRequest ("PageWithMaster.aspx", "", Console.Out));
		}
	}
}

