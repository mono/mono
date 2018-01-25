#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Description;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IFaultsTesterContract
	{
		[OperationContract]
		void FaultMethod (string faultText);
	}

	public class FaultsTester : IFaultsTesterContract
	{
		public void FaultMethod (string faultText)
		{
			throw new Exception (faultText);
		}
	}

	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IFaultsTesterContractIncludeDetails
	{
		[OperationContract]
		void FaultMethod (string faultText);
	}

	[ServiceBehavior (IncludeExceptionDetailInFaults = true)]
	public class FaultsTesterIncludeDetails : IFaultsTesterContractIncludeDetails
	{
		public void FaultMethod (string faultText)
		{
			throw new Exception (faultText);
		}
	}
}
#endif