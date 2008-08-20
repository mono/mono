using System;
using System.Collections.Generic;
using System.Text;

namespace Proxy.MonoTests.Features.Client
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	[System.ServiceModel.ServiceContractAttribute (Namespace = "http://MonoTests.Features.Contracts", ConfigurationName = "IMessageContractTesterContract")]
	public interface IMessageContractTesterContract
	{
		[System.ServiceModel.OperationContractAttribute (Action = "http://test/TestMessage_action", ReplyAction = "http://test/TestMessage_action")]
		TestMessage FormatDate (TestMessage testMessage);
	}

	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	[System.ServiceModel.MessageContractAttribute (WrapperName = "TestMessage", WrapperNamespace = "http://MonoTests.Features.Contracts", IsWrapped = true)]
	public partial class TestMessage
	{

		[System.ServiceModel.MessageHeaderAttribute (Namespace = "http://MonoTests.Features.Contracts")]
		public string FormatString;

		[System.ServiceModel.MessageBodyMemberAttribute (Namespace = "http://MonoTests.Features.Contracts", Order = 0)]
		public DateTime Date;

		[System.ServiceModel.MessageBodyMemberAttribute (Namespace = "http://MonoTests.Features.Contracts", Order = 1)]
		public string FormattedDate;

		public TestMessage ()
		{
		}

		public TestMessage (DateTime Date, string FormatString, string FormattedDate)
		{
			this.Date = Date;
			this.FormatString = FormatString;
			this.FormattedDate = FormattedDate;
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public interface IMessageContractTesterContractChannel : IMessageContractTesterContract, System.ServiceModel.IClientChannel
	{
	}

	[System.Diagnostics.DebuggerStepThroughAttribute ()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "3.0.0.0")]
	public partial class MessageContractTesterContractClient : System.ServiceModel.ClientBase<IMessageContractTesterContract>, IMessageContractTesterContract
	{

		public MessageContractTesterContractClient ()
		{
		}

		public MessageContractTesterContractClient (string endpointConfigurationName) :
			base (endpointConfigurationName)
		{
		}

		public MessageContractTesterContractClient (string endpointConfigurationName, string remoteAddress) :
			base (endpointConfigurationName, remoteAddress)
		{
		}

		public MessageContractTesterContractClient (string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
			base (endpointConfigurationName, remoteAddress)
		{
		}

		public MessageContractTesterContractClient (System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
			base (binding, remoteAddress)
		{
		}

		TestMessage IMessageContractTesterContract.FormatDate (TestMessage testMessage)
		{
			return base.Channel.FormatDate (testMessage);
		}

		public void Calculate (ref DateTime Date, ref string FormatString, ref string FormattedDate)
		{
			TestMessage inValue = new TestMessage ();
			inValue.Date = Date;
			inValue.FormatString = FormatString;
			inValue.FormattedDate = FormattedDate;
			TestMessage retVal = ((IMessageContractTesterContract) (this)).FormatDate (inValue);
			Date = retVal.Date;
			FormatString = retVal.FormatString;
			FormattedDate = retVal.FormattedDate;
		}
	}
}
