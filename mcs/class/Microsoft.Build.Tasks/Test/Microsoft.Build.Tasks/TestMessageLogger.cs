using Microsoft.Build.Framework;
using System.Collections;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks
{
	internal class TestMessageLogger : ILogger
	{
		List<BuildMessageEventArgs> messages;
		List<BuildEventArgs> all_messages;
		int target_started, target_finished;
		int task_started, task_finished;

		public TestMessageLogger ()
		{
			messages = new List<BuildMessageEventArgs> ();
			all_messages = new List<BuildEventArgs> ();
		}

		public int TargetFinished
		{
			get { return target_finished; }
			set { target_finished = value; }
		}

		public int TargetStarted
		{
			get { return target_started; }
			set { target_started = value; }
		}

		public int TaskStarted
		{
			get { return task_started; }
			set { task_started = value; }
		}

		public int TaskFinished
		{
			get { return task_finished; }
			set { task_finished = value; }
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
			eventSource.ErrorRaised += new BuildErrorEventHandler (AllMessagesHandler);
			eventSource.WarningRaised += new BuildWarningEventHandler(AllMessagesHandler);
			eventSource.TargetStarted += delegate { target_started++; };
			eventSource.TargetFinished += delegate { target_finished++; };
			eventSource.TaskStarted += delegate { task_started++; };
			eventSource.TaskFinished += delegate { task_finished++; };
		}

		public void Shutdown ()
		{
		}

		private void MessageHandler (object sender, BuildMessageEventArgs args)
		{
			if (args.Message.StartsWith ("Using") == false)
				messages.Add (args);
			all_messages.Add (args);
		}

		private void AllMessagesHandler (object sender, BuildEventArgs args)
		{
			all_messages.Add (args);
		}

		public int NormalMessageCount {
			get
			{
				int count = 0, i = 0;
				while (i ++ < messages.Count)
					if (messages [i - 1].Importance == MessageImportance.Normal)
						count++;
				return count;
			}
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
				//find first @importance level message
				int i = 0;
				while (messages [i].Importance != importance && i < messages.Count)
					i++;

				actual = messages [i];
				messages.RemoveAt (i);
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
			foreach (BuildEventArgs arg in all_messages)
				Console.WriteLine ("Msg: {0}", arg.Message);
		}

		public void CheckLoggedMessageHead (string expected, string id)
		{
			string actual;
			int result = CheckHead (expected, MessageImportance.Normal, out actual);
			if (result == 1)
				Assert.Fail ("{0}: Expected message '{1}' was not emitted.", id, expected);
			if (result == 2)
				Assert.AreEqual (expected, actual, id);
		}


	}

}
