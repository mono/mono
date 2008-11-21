using Microsoft.Build.Framework;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MonoTests.Microsoft.Build.Tasks
{
	internal class TestMessageLogger : ILogger
	{
		List<BuildMessageEventArgs> messages;

		public TestMessageLogger ()
		{
			messages = new List<BuildMessageEventArgs> ();
		}

		public int Count
		{
			get { return messages.Count; }
		}

		public LoggerVerbosity Verbosity { get { return LoggerVerbosity.Normal; } set { } }

		public string Parameters { get { return null; } set { } }

		public void Initialize (IEventSource eventSource)
		{
			eventSource.MessageRaised += new BuildMessageEventHandler (MessageHandler);
		}

		public void Shutdown ()
		{
		}

		private void MessageHandler (object sender, BuildMessageEventArgs args)
		{
			if (args.Message.StartsWith ("Using") == false)
				messages.Add (args);
		}

		public int CheckHead (string text, MessageImportance importance)
		{
			string actual_msg;
			return CheckHead (text, importance, out actual_msg);
		}

		public int CheckHead (string text, MessageImportance importance, out string actual_msg)
		{
			BuildMessageEventArgs actual;
			actual_msg = String.Empty;

			if (messages.Count > 0) {
				actual = messages [0];
				messages.RemoveAt (0);
			} else
				return 1;

			actual_msg = actual.Message;
			if (text == actual.Message && importance == actual.Importance)
				return 0;
			else
				return 2;
		}

		//return: 0 - found
		//		1 - msg not found,
		public int CheckAny (string text, MessageImportance importance)
		{
			if (messages.Count <= 0)
				return 1;

			int foundAt = -1;
			for (int i = 0; i < messages.Count; i ++) {
				BuildMessageEventArgs arg = messages [i];
				if (text == arg.Message && importance == arg.Importance) {
					foundAt = i;
					break;
				}
			}

			if (foundAt < 0)
				return 1;

			//found
			messages.RemoveAt (foundAt);
			return 0;
		}

		public void DumpMessages ()
		{
			foreach (BuildMessageEventArgs arg in messages)
				Console.WriteLine ("Msg: {0}", arg.Message);
		}
	}

}
