//
// MessageTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	internal class TestLogger : ILogger {
		IList messages;
		
		public TestLogger ()
		{
			messages = new ArrayList ();
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
		
		public bool CheckHead (string text, MessageImportance importance)
		{
			BuildMessageEventArgs actual;
		
			if (messages.Count > 0) {
				actual = (BuildMessageEventArgs) messages [0];
				messages.RemoveAt (0);
			} else
				return false;
			
			if (text == actual.Message && importance == actual.Importance)
				return true;
			else
				return false;
		}
	}

	[TestFixture]
	public class MessageTest {
	
		Engine engine;
		Project project;
		TestLogger testLogger;
		
		[Test]
		public void TestAssignment ()
		{
			string importance = "importance";
			string text = "text";
			
			Message message = new Message ();
			
			message.Importance = importance;
			message.Text = text;
			
			Assert.AreEqual (importance, message.Importance, "A1");
			Assert.AreEqual (text, message.Text, "A2");
		}
		
		[Test]
		public void TestExecution ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Message Text='Text' Importance='Low'/>
						<Message Text='Text' Importance='Normal'/>
						<Message Text='Text' Importance='High'/>
						<Message Text='Text' Importance='low'/>
						<Message Text='Text' Importance='normal'/>
						<Message Text='Text' Importance='high'/>
						<Message Text='Text' Importance='weird_importance'/>
					</Target>
				</Project>
			";
			
			
			engine = new Engine (Consts.BinPath);
			testLogger = new TestLogger ();
			engine.RegisterLogger (testLogger);
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.Build ("1");
			
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.Low), "A1");
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.Normal), "A2");
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.High), "A3");
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.Low), "A4");
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.Normal), "A5");
			Assert.AreEqual (true, testLogger.CheckHead ("Text", MessageImportance.High), "A6");
			Assert.AreEqual (false, testLogger.CheckHead ("Text", MessageImportance.Normal), "A7");
			
		}
	}
}	

