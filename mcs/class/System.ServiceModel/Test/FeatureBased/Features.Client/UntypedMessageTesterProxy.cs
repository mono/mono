using System;
using System.Collections.Generic;
using System.Text;

namespace Proxy.MonoTests.Features.Client
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	[System.ServiceModel.ServiceContractAttribute (Namespace = "http://MonoTests.Features.Contracts", ConfigurationName = "IUntypedMessageTesterContract")]
	public interface IUntypedMessageTesterContract
	{
		[System.ServiceModel.OperationContractAttribute (Action = "http://localhost/UntypedMessageTester/Message_RequestAction", ReplyAction = "http://localhost/UntypedMessageTester/Message_ReplyAction")]
		System.ServiceModel.Channels.Message ConcatStrings (System.ServiceModel.Channels.Message request);
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public interface IUntypedMessageTesterContractChannel : IUntypedMessageTesterContract, System.ServiceModel.IClientChannel
	{
	}

	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public partial class UntypedMessageTesterContractClient : System.ServiceModel.ClientBase<IUntypedMessageTesterContract>, IUntypedMessageTesterContract
	{

		public UntypedMessageTesterContractClient ()
		{
		}

		public UntypedMessageTesterContractClient (string endpointConfigurationName)
			:
				base (endpointConfigurationName)
		{
		}

		public UntypedMessageTesterContractClient (string endpointConfigurationName, string remoteAddress)
			:
				base (endpointConfigurationName, remoteAddress)
		{
		}

		public UntypedMessageTesterContractClient (string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
			:
				base (endpointConfigurationName, remoteAddress)
		{
		}

		public UntypedMessageTesterContractClient (System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
			:
				base (binding, remoteAddress)
		{
		}

		public System.ServiceModel.Channels.Message ConcatStrings (System.ServiceModel.Channels.Message request)
		{
			return base.Channel.ConcatStrings (request);
		}
	}
}
