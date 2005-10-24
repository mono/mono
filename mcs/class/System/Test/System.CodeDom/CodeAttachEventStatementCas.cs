//
// CodeAttachEventStatementCas.cs
//	- CAS unit tests for System.CodeDom.CodeAttachEventStatement
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom {

	[TestFixture]
	[Category ("CAS")]
	public class CodeAttachEventStatementCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CodeAttachEventStatement caes = new CodeAttachEventStatement ();
			Assert.AreEqual (String.Empty, caes.Event.EventName, "Event.EventName");
			Assert.IsNull (caes.Event.TargetObject, "Event.TargetObject");
			caes.Event = new CodeEventReferenceExpression ();
			Assert.IsNull (caes.Listener, "Listener");
			caes.Listener = new CodeExpression ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeEventReferenceExpression eventref = new CodeEventReferenceExpression ();
			CodeExpression listener = new CodeExpression ();
			CodeAttachEventStatement caes = new CodeAttachEventStatement (eventref, listener);
			Assert.AreSame (eventref, caes.Event, "Event");
			caes.Event = new CodeEventReferenceExpression ();
			Assert.AreSame (listener, caes.Listener, "Listener");
			caes.Listener = new CodeExpression ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeExpression target = new CodeExpression ();
			string eventName = "Mono";
			CodeExpression listener = new CodeExpression ();
			CodeAttachEventStatement caes = new CodeAttachEventStatement (target, eventName, listener);
			Assert.AreEqual (eventName, caes.Event.EventName, "Event.EventName");
			Assert.AreSame (target, caes.Event.TargetObject, "Event.TargetObject");
			caes.Event = new CodeEventReferenceExpression ();
			Assert.AreSame (listener, caes.Listener, "Listener");
			caes.Listener = new CodeExpression ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeAttachEventStatement).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
