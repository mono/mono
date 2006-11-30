//
// MessageTest.cs: Test cases for Message
//
// Authors:
//   Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	public class MessageTest
	{
		[Test]
		public void ToStringTest ()
		{
			Message msg = Message.Create (new IntPtr (123), 2, new IntPtr (234), new IntPtr (345));
			Assert.AreEqual ("msg=0x2 (WM_DESTROY) hwnd=0x7b wparam=0xea lparam=0x159 result=0x0", msg.ToString ());
			msg.Result = new IntPtr (2);
			Assert.AreEqual ("msg=0x2 (WM_DESTROY) hwnd=0x7b wparam=0xea lparam=0x159 result=0x2", msg.ToString ());
		}
	}
}
