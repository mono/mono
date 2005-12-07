using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Configuration;

namespace TestMonoWeb
{
	public class MyHost : MarshalByRefObject {
		public MyHost() {
		}
	}
	/// <summary>
	/// Summary description for Test1.
	/// </summary>
	public class Test1
	{
		static void Main(string[] args) {
			// Create the application host
			object host = ApplicationHost.CreateApplicationHost(typeof(MyHost), "/", "c:\\");
			
			int request_count = 10;
			SimpleWorkerRequest [] requests = new SimpleWorkerRequest[request_count];

			int pos;
			for (pos = 0; pos != request_count; pos++) {
				requests[pos] = new SimpleWorkerRequest("test.aspx", "", Console.Out);
			}

			ModulesConfiguration.Add("syncmodule", typeof(SyncModule).AssemblyQualifiedName);
			ModulesConfiguration.Add("asyncmodule", typeof(AsyncModule).AssemblyQualifiedName);
			
			HandlerFactoryConfiguration.Add("get", "/", typeof(AsyncHandler).AssemblyQualifiedName);
			//HandlerFactoryConfiguration.Add("get", "/", typeof(SyncHandler).AssemblyQualifiedName);

			for (pos = 0; pos != request_count; pos++) 
				HttpRuntime.ProcessRequest(requests[pos]);

			HttpRuntime.Close();
/*
			Console.Write("Press Enter to quit.");
			Console.WriteLine();
			Console.ReadLine();
*/
		}	
	}
}
