//
// CodeAttachEventStatementTest.cs
//	- Unit tests for System.CodeDom.CodeAttachEventStatement 
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
//

using NUnit.Framework;

using System;
using System.CodeDom;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeAttachEventStatementTest
	{
		[Test]
		public void Constructor0 ()
		{
			CodeAttachEventStatement caes = new CodeAttachEventStatement ();
			Assert.IsNotNull (caes.Event, "#1");
			Assert.IsNull (caes.Listener, "#2");
			Assert.AreEqual (string.Empty, caes.Event.EventName, "#3");
			Assert.IsNull (caes.Event.TargetObject, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			CodeEventReferenceExpression eventref = new CodeEventReferenceExpression ();
			CodeExpression listener = new CodeExpression ();

			CodeAttachEventStatement caes = new CodeAttachEventStatement (eventref, listener);
			Assert.AreSame (eventref, caes.Event, "#1");
			Assert.AreEqual (string.Empty, caes.Event.EventName, "#2");
			Assert.IsNull (caes.Event.TargetObject, "#3");
			Assert.AreSame (listener, caes.Listener, "#4");

			caes.Event = null;
			Assert.IsNotNull (caes.Event, "#5");
			Assert.AreEqual (string.Empty, caes.Event.EventName, "#6");
			Assert.IsNull (caes.Event.TargetObject, "#7");
			Assert.AreSame (listener, caes.Listener, "#8");

			caes.Listener = null;
			Assert.IsNull (caes.Listener, "#9");

			caes.Event = eventref;
			Assert.AreSame (eventref, caes.Event, "#10");

			caes.Listener = listener;
			Assert.AreSame (listener, caes.Listener, "#11");

			caes = new CodeAttachEventStatement ((CodeEventReferenceExpression) null, (CodeExpression) null);
			Assert.IsNotNull (caes.Event, "#12");
			Assert.IsNull (caes.Listener, "#13");
			Assert.AreEqual (string.Empty, caes.Event.EventName, "#14");
			Assert.IsNull (caes.Event.TargetObject, "#15");
		}

		[Test]
		public void Constructor2 ()
		{
			CodeExpression targetObject = new CodeExpression ();
			CodeExpression listener = new CodeExpression ();

			CodeAttachEventStatement caes = new CodeAttachEventStatement (targetObject, "mono", listener);
			Assert.IsNotNull (caes.Event, "#1");
			Assert.AreEqual ("mono", caes.Event.EventName, "#2");
			Assert.AreSame (targetObject, caes.Event.TargetObject, "#3");
			Assert.AreSame (listener, caes.Listener, "#4");

		}
	}
}
