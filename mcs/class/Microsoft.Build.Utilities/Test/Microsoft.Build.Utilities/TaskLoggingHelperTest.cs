//
// TaskLoggingHelperTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (ankit.jain@xamarin.com)
//
// (C) 2005 Marek Sieradzki
// (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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
using System.Reflection;
using System.Resources;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;
using MonoTests.Microsoft.Build.Tasks;

namespace MonoTests.Microsoft.Build.Utilities {
	
	class TestTask : Task {
		public static Action<TaskLoggingHelper> action = null;

		public TestTask ()
			: base (new ResourceManager("Strings", typeof(TestTask).GetTypeInfo().Assembly))
		{
		}

		public override bool Execute ()
		{
			action (Log);
			return true; 
		}
	}
	
	[TestFixture]
	public class TaskLoggingHelperTest {
	
		TaskLoggingHelper tlh;
		TestTask task;

		public TaskLoggingHelperTest ()
		{
			task = new TestTask ();
		}
	
		[Test]
		public void TestAssignment ()
		{
			tlh = new TaskLoggingHelper (task);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestExtractMessageCode1 ()
		{
			tlh = new TaskLoggingHelper (task);
			
			string message = "MYTASK1001: This is an error message.";
			string validCode = "MYTASK1001";
			string validMessageWithoutCodePrefix = "This is an error message.";
			string code, messageWithoutCodePrefix;
			
			code = tlh.ExtractMessageCode (message, out messageWithoutCodePrefix);
			
			Assert.AreEqual (validCode, code, "#1");
			Assert.AreEqual (validMessageWithoutCodePrefix, messageWithoutCodePrefix, "#2");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestExtractMessageCode2 ()
		{
			tlh = new TaskLoggingHelper (task);
			string output;
			tlh.ExtractMessageCode (null, out output);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogErrorFromResourcesNullMessage ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogErrorFromResources (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogErrorFromResourcesNullMessage2 ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogErrorFromResources (null, null, null, null, 0, 0, 0, 0, null);
		}

		[Test]
		public void TestLogErrorFromResources1 ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogErrorFromResources ("MessageResource1", "foo"),
					(l) => Assert.IsTrue (l.CheckFullLog ("Message from resources with arg 'foo'") == 0, "Message not found")
					);
		}

		[Test]
		public void TestLogErrorFromResourcesNonExistantResourceName ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogErrorFromResources ("NonExistantResourceName", "foo"),
					null,
					(p, l) => {
						Assert.IsFalse (p.Build (), "Build should have failed");
						Assert.IsTrue (l.CheckFullLog (
								"Error executing task TestTask: No resource string found for resource named NonExistantResourceName") == 0,
								"Error not found in the log");
					}
				);
		}


		[Test]
		public void TestLogErrorFromResourcesNullSubcategoryResourceName ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogErrorFromResources (null, null, null, null, 0, 0, 0, 0, "MessageResource1", "foo"),
					(l) => Assert.IsTrue (l.CheckFullLog ("Message from resources with arg 'foo'") == 0, "Message not found")
					);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogWarningFromResourcesNullMessage ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogWarningFromResources (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogWarningFromResourcesNullMessage2 ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogWarningFromResources (null, null, null, null, 0, 0, 0, 0, null);
		}

		[Test]
		public void TestLogWarningFromResourcesNullSubcategoryResourceName ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogWarningFromResources (null, null, null, null, 0, 0, 0, 0, "MessageResource1", "foo"),
					(l) => Assert.IsTrue (l.CheckFullLog ("Message from resources with arg 'foo'") == 0, "Message not found")
					);
		}

		[Test]
		public void TestLogWarningFromResources1 ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogWarningFromResources ("MessageResource1", "foo"),
					(l) => Assert.IsTrue (l.CheckFullLog ("Message from resources with arg 'foo'") == 0, "Message not found")
					);
		}

		[Test]
		public void TestLogWarningFromResourcesNonExistantResourceName ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogWarningFromResources ("NonExistantResourceName", "foo"),
					null,
					(p, l) => {
						if (p.Build ()) { l.DumpMessages (); Assert.Fail ("Build should have failed"); }
						Assert.IsTrue (l.CheckFullLog (
								"Error executing task TestTask: No resource string found for resource named NonExistantResourceName") == 0,
								"Error not found in the log");
					}
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogMessageFromResourcesNullMessage ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogMessageFromResources (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestLogMessageFromResourcesNullMessage2 ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.LogMessageFromResources (MessageImportance.Low, null);
		}

		[Test]
		public void TestLogMessageFromResources1 ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogMessageFromResources ("MessageResource1", "foo"),
					(l) => Assert.IsTrue (l.CheckFullLog ("Message from resources with arg 'foo'") == 0, "Message not found")
					);
		}

		[Test]
		public void TestLogMessageFromResourcesNonExistantResourceName ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.LogMessageFromResources ("NonExistantResourceName", "foo"),
					null,
					(p, l) => {
						if (p.Build ()) { l.DumpMessages (); Assert.Fail ("Build should have failed"); }
						l.DumpMessages ();
						Assert.IsTrue (l.CheckFullLog (
								"Error executing task TestTask: No resource string found for resource named NonExistantResourceName") == 0,
								"Error not found in the log");
					}
				);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestFormatResourceString1 ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.FormatResourceString (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestFormatResourceString2 ()
		{
			tlh = new TaskLoggingHelper (task);
			tlh.FormatResourceString ("MessageResource1");
		}

		[Test]
		public void TestFormatResourceString3 ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => tlh.FormatResourceString ("NonExistantResourceName"),
					null,
					(p, l) => {
						if (p.Build ()) { l.DumpMessages (); Assert.Fail ("Build should have failed"); }
						l.DumpMessages ();
						Assert.IsTrue (l.CheckFullLog (
								"Error executing task TestTask: No resource string found for resource named NonExistantResourceName") == 0,
								"Error not found in the log");
					}
				);
		}

		[Test]
		public void TestFormatResourceString4 ()
		{
			RunAndCheckTaskLoggingHelper (
					(tlh) => Assert.AreEqual (
						tlh.FormatResourceString ("MessageResource1", "foo"),
						"Message from resources with arg 'foo'"),
					null
				);
		}
		void RunAndCheckTaskLoggingHelper (Action<TaskLoggingHelper> taskAction, Action<TestMessageLogger> loggerAction, Action<Project, TestMessageLogger> projectBuildAction = null)
		{
			string asmLocation = typeof (TaskLoggingHelperTest).Assembly.Location;
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName='MonoTests.Microsoft.Build.Utilities.TestTask' AssemblyFile='" + asmLocation + @"' />
			<Target Name=""1"">
				<TestTask />
			</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			proj.LoadXml (project_xml);
			TestMessageLogger logger = new TestMessageLogger ();
			engine.RegisterLogger (logger);

			TestTask.action = taskAction;

			if (projectBuildAction == null) {
				if (!proj.Build ("1")) {
					logger.DumpMessages ();
					Assert.Fail ("Build failed");
				}
			} else
				projectBuildAction (proj, logger);

			if (loggerAction != null)
				loggerAction (logger);
		}
	}
}
