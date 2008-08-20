using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IExitProcessHelper
	{
		[OperationContract(IsOneWay=true)]
		void ExitProcess (int code);
	}

	public class ExitProcessHelperServer : IExitProcessHelper
	{
		public void ExitProcess (int code) {
			Environment.Exit (code);
		}
	}

}