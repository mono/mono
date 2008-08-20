using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace MonoTests.Features.Contracts
{
	// Define a service contract.
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IOperationContract
	{

		[OperationContract (Name = "RenamedMethod")]
		int OrigMethod ();

		[OperationContract (Name = "OrigMethod")]
		int RenamedMethod ();


		[OperationContract (IsOneWay = true)]
		void Sleep (int mili);
	}

	public class OperationContractServer : IOperationContract
	{
		public int OrigMethod () { return 1; }
		public int RenamedMethod () { return 2; }

		public void Sleep(int mili) 
		{
			 Thread.Sleep(mili); 
		}
	}
}
