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
//
// Copyright (c) 2019 AxxonSoft.
//
// Authors:
//	Nikita Voronchev <nikita.voronchev@ru.axxonsoft.com>
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NUnit.Framework;

using Mono.PortabilityUtils;

namespace MonoTests.Mono.PortabilityUtils
{
	using Helpers = WinFormsTestHelpers;

	[TestFixtureAttribute]
	public class WinFormsTest
	{
		[Test]
		public void TestRegisterWindowMessage ()
		{
			var pInfoA = Helpers.GetProcessStartInfo ("RegisterWindowMessage test-msg-name-A");
			var pInfoB = Helpers.GetProcessStartInfo ("RegisterWindowMessage test-msg-name-B");
			
			uint msgIdA1 = Helpers.RunProcessAndGetMsgId (pInfoA);
			uint msgIdB = Helpers.RunProcessAndGetMsgId (pInfoB);
			uint msgIdA2 = Helpers.RunProcessAndGetMsgId (pInfoA);

			Assert.IsTrue (msgIdA1 >= 0xC000, "#1.1: msgIdA1={0}", msgIdA1);
			Assert.IsTrue (msgIdA1 <= 0xFFFF, "#1.2: msgIdA1={0}", msgIdA1);

			Assert.IsTrue (msgIdB >= 0xC000, "#2.1: msgIdA2={0}", msgIdA2);
			Assert.IsTrue (msgIdB <= 0xFFFF, "#2.2: msgIdA2={0}", msgIdA2);

			Assert.AreEqual (msgIdA1, msgIdA2, "#3.1");
			Assert.AreNotEqual (msgIdA1, msgIdB, "#3.2");
		}

		[Test]
		public void TestEnumTopLevelWindows ()
		{
			const int LPARAM_CONST = 123;

			var intialWindows = Helpers.RunProcessAndGetWindows (0);

			Helpers.OpenedWindow[] windowsWithTestFrom;
			IntPtr formHwnd;
			using (var f = new Form ()) {
				f.Text = "TestEnumTopLevelWindows_Form";
				f.Visible = true;
				formHwnd = f.Handle;
				windowsWithTestFrom = Helpers.RunProcessAndGetWindows (LPARAM_CONST);
			}

			var formWindow = windowsWithTestFrom.FirstOrDefault (w => w.Hwnd == formHwnd);
			Assert.AreNotEqual (formWindow, null, "#3.1: Test form not found: formHwnd={0:X} windowsWithTestFrom={1}",
				formHwnd, string.Join<Helpers.OpenedWindow>(",", windowsWithTestFrom));

			Assert.AreEqual (formWindow.LParam.ToInt32 (), LPARAM_CONST, "#3.2: lParam passing fail");
		}

		[Test]
		public void TestPostMessage ()
		{
			int MESSAGE_ID_PROCESS = WinForms.RegisterWindowMessage ("test-msg-name-A");
			int MESSAGE_ID_THREAD = WinForms.RegisterWindowMessage ("test-msg-name-B");
			var MESSAGE_LPARAM = new IntPtr (123);
			var MESSAGE_WPARAM = new IntPtr (456);

			using (var f = new TestEnumTopLevelWindows_Form (MESSAGE_ID_PROCESS, MESSAGE_ID_THREAD)) {

				bool postProcessReturn = false, postThreadReturn = false;
				f.Shown += (object sender, EventArgs e) => {
   					postProcessReturn = Helpers.RunProcessAndPostMessage (f.Handle, MESSAGE_ID_PROCESS, MESSAGE_WPARAM, MESSAGE_LPARAM);
					postThreadReturn = WinForms.PostMessage (f.Handle, MESSAGE_ID_THREAD, MESSAGE_WPARAM, MESSAGE_LPARAM);
				};

				var appTask = Task.Run (() => {
					Application.Run(f);
				});

				var received = appTask.Wait (TimeSpan.FromSeconds (10));
				
				Assert.IsTrue (postProcessReturn, "#1.1: Fail message 0x{0:X} send by external process", MESSAGE_ID_PROCESS);
				Assert.IsTrue (postThreadReturn, "#1.2: Fail message 0x{0:X} send by thread", MESSAGE_ID_THREAD);
			
				Assert.IsTrue (f.IsAllReceivedMessageHasDifferentId (), "#2.1: Two or more received messages have the same id");
				Assert.IsTrue (f.IsMessageIdReceived (MESSAGE_ID_PROCESS),
					"#2.2: Test form didn't receive messages 0x{0:X} from external process", MESSAGE_ID_PROCESS);
				Assert.IsTrue (f.IsMessageIdReceived (MESSAGE_ID_THREAD),
					"#2.3: Test form didn't receive messages 0x{0:X} from another thread", MESSAGE_ID_THREAD);
			}
		}

		class TestEnumTopLevelWindows_Form : Form
		{
			private List<Message> ReceivedMessage = new List<Message> ();

			private int[] messageToWait;

			public TestEnumTopLevelWindows_Form (params int[] messageToWait)
			{
				this.Text = "TestEnumTopLevelWindows_Form";
				this.messageToWait = messageToWait;
			}

			protected override void WndProc (ref Message message)
			{
				base.WndProc(ref message);
				if (messageToWait.Contains (message.Msg)) {
					ReceivedMessage.Add (message);
					if (ReceivedMessage.Count == messageToWait.Length)
						this.Close ();
				}
			}

			public bool IsMessageIdReceived (int msgId)
			{
				return ReceivedMessage.Select (m => m.Msg).Contains (msgId);
			}

			public bool IsAllReceivedMessageHasDifferentId ()
			{
				return ReceivedMessage.GroupBy (m => m.Msg).Count () == ReceivedMessage.Count;
			}
		}
	}
}