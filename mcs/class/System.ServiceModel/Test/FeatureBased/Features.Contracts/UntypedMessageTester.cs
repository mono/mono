using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IUntypedMessageTesterContract
	{
		[OperationContract (Action = UntypedMessageTester.RequestAction, ReplyAction = UntypedMessageTester.ReplyAction)]
		Message ConcatStrings (Message request);
	}

	public class UntypedMessageTester : IUntypedMessageTesterContract
	{
		public const String ReplyAction = "http://localhost/UntypedMessageTester/Message_ReplyAction";
		public const String RequestAction = "http://localhost/UntypedMessageTester/Message_RequestAction";

		public Message ConcatStrings (Message request)
		{
			string str = string.Empty;

			string [] inputStrings = request.GetBody<string []> ();
			for (int i = 0; i < inputStrings.Length; i++)
				str += inputStrings [i];

			Message response = Message.CreateMessage (request.Version, ReplyAction, str);
			return response;
		}
	}
}
