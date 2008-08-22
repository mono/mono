//
// Unit tests for System.ObsoleteAttribute
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System;

using NUnit.Framework;

namespace MonoTests.System {

	[TestFixture]
	public class ObsoleteAttributeTest {

		[Test]
		public void Type ()
		{
			object[] attrs = typeof (ObsoleteAttribute).GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			AttributeUsageAttribute usage = (AttributeUsageAttribute) attrs [0];

			Assert.IsFalse (usage.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (usage.Inherited, "Inherited");
			Assert.AreEqual (usage.ValidOn, AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
				AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | 
				AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | 
				AttributeTargets.Delegate, "ValidOn");
		}

		private void Check (ObsoleteAttribute attr, string message, bool error)
		{
			Assert.AreEqual (message, attr.Message, "Message");
			Assert.AreEqual (error, attr.IsError, "IsError");
			Assert.AreEqual (typeof (ObsoleteAttribute), attr.TypeId, "TypeId");
		}

		[Test]
		public void Constructor ()
		{
			Check (new ObsoleteAttribute (), null, false);
		}

		[Test]
		public void ConstructorMessage_Null ()
		{
			Check (new ObsoleteAttribute (null), null, false);
		}

		[Test]
		public void ConstructorMessage_Empty ()
		{
			Check (new ObsoleteAttribute (String.Empty), String.Empty, false);
		}

		[Test]
		public void ConstructorMessage ()
		{
			string message = "too late, stuff is long gone";
			Check (new ObsoleteAttribute (message), message, false);
		}

		[Test]
		public void ConstructorMessageBoolTrue ()
		{
			Check (new ObsoleteAttribute (null, true), null, true);
		}

		[Test]
		public void ConstructorMessageBoolFalse ()
		{
			string message = "too late, stuff is long gone";
			Check (new ObsoleteAttribute (message, false), message, false);
		}
	}
}
