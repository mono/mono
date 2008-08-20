using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IFirstContract
	{
		[OperationContract]
		int FirstMethod ();
	}

	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface ISecondContract
	{
		[OperationContract]
		int SecondMethod ();
	}

	public class DualContractServer : IFirstContract, ISecondContract
	{
		public int FirstMethod () { return 1; }
		public int SecondMethod () { return 2; }
	}
}
