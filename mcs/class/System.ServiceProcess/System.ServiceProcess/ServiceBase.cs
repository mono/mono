using System;

namespace System.ServiceProcess
{
	public class ServiceBase
	{
		public ServiceBase() { }

		protected virtual void Dispose( bool disposing ) { }

		protected virtual void OnStart(string[] args) { }

		protected virtual void OnStop() { }

		protected string ServiceName;

		public static void Run(ServiceBase[] ServicesToRun) { }

	}
}