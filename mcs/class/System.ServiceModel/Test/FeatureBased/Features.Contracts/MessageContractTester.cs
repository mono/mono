using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IMessageContractTesterContract
	{
		[OperationContract (Action = "http://test/TestMessage_action", ReplyAction = "http://test/TestMessage_action")]
		TestMessage FormatDate (TestMessage testMessage);
	}

	[MessageContract]
	public class TestMessage
	{
		private string formatString;
		private DateTime date;
		private string formattedDate;

		public TestMessage () 
		{
		}

		public TestMessage (DateTime date, string formatString, string formattedDate)
		{
			this.date = date;
			this.formatString = formatString;
			this.formattedDate = formattedDate;
		}

		public TestMessage (TestMessage message)
		{
			this.date = message.date;
			this.formatString = message.formatString;
			this.formattedDate = message.formattedDate;
		}

		[MessageHeader]
		public string FormatString
		{
			get { return formatString; }
			set { formatString = value; }
		}

		[MessageBodyMember]
		public DateTime Date
		{
			get { return date; }
			set { date = value; }
		}

		[MessageBodyMember]
		public string FormattedDate
		{
			get { return formattedDate; }
			set { formattedDate = value; }
		}
	}

	public class MessageContractTester : IMessageContractTesterContract
	{
		public TestMessage FormatDate (TestMessage testMessage)
		{
			TestMessage r = new TestMessage (testMessage);
			r.FormattedDate = r.Date.ToString (r.FormatString);
			return r;
		}
	}
}
