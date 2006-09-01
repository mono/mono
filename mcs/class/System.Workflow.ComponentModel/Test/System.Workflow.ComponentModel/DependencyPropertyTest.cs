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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Workflow.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace MonoTests.System.Workflow.ComponentModel
{

	[TestFixture]
	public class DependencyPropertyTest
	{
		public class ClassProp
		{
			public ClassProp () {}

			public static DependencyProperty FromProperty = DependencyProperty.Register
				("From", typeof(string), typeof(ClassProp), new PropertyMetadata("someone@example.com"));
		}

		public class ClassProp2
		{
			public ClassProp2 () {}

			public static DependencyProperty FromProperty = DependencyProperty.Register
				("From", typeof(string), typeof(ClassProp2), new PropertyMetadata("someone@example.com"));
		}

		public class ClassProp3
		{
			public ClassProp3 () {}

			public static DependencyProperty FromProperty = DependencyProperty.Register
				("From", typeof(string), typeof(ClassProp2), new PropertyMetadata("someone@example.com"));
		}

		public class ClassPropEv
		{
			public static readonly DependencyProperty ExecuteCodeEvent;

			public ClassPropEv ()
			{

			}
		}

		[Test]
		public void RegisterEvent ()
		{
			DependencyProperty dp = DependencyProperty.Register ("ExecuteCode", typeof (EventHandler),
				typeof (ClassPropEv));

			Assert.AreEqual (false, dp.IsAttached, "C2#1");
			Assert.AreEqual (true, dp.IsEvent, "C2#2");
			Assert.AreEqual ("ExecuteCode", dp.Name, "C2#3");
			Assert.AreEqual (typeof (ClassPropEv), dp.OwnerType, "C2#4");
			Assert.AreEqual (typeof (EventHandler), dp.PropertyType, "C2#5");

		}

		[Test]
		public void Test1 ()
		{
			DependencyProperty dp = DependencyProperty.Register ("From", typeof(string),
				typeof (ClassProp), new PropertyMetadata ("someone@example.com"));

			Assert.AreEqual (false, dp.IsAttached, "C1#1");
			Assert.AreEqual (false, dp.IsEvent, "C1#2");
			Assert.AreEqual ("From", dp.Name, "C1#3");

			Assert.AreEqual (typeof (ClassProp), dp.OwnerType, "C1#4");
			Assert.AreEqual (typeof (string), dp.PropertyType, "C1#5");
			Assert.AreEqual (null, dp.ValidatorType, "C1#6");

			IList <DependencyProperty> list = DependencyProperty.FromType (typeof (DependencyPropertyTest));
			Assert.AreEqual (0, list.Count, "C1#7");
			Assert.AreEqual ("From", dp.ToString (), "C1#8");
		}

		[Test]
		public void RegisterAttachedProperty ()
		{
			DependencyProperty dp = DependencyProperty.RegisterAttached ("From", typeof(string),
				typeof (ClassProp2), new PropertyMetadata ("someone@example.com"));

			Assert.AreEqual (true, dp.IsAttached, "C2#1");
			Assert.AreEqual (false, dp.IsEvent, "C2#2");
			Assert.AreEqual ("From", dp.Name, "C2#3");
			Assert.AreEqual (typeof (ClassProp2), dp.OwnerType, "C2#4");
			Assert.AreEqual (typeof (string), dp.PropertyType, "C2#5");
			Assert.AreEqual (null, dp.ValidatorType, "C2#6");

		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAlreadyExistException ()
		{
			DependencyProperty dp = DependencyProperty.Register ("From", typeof(string),
				typeof (ClassProp3), new PropertyMetadata ("someone@example.com"));

			DependencyProperty dp2 = DependencyProperty.Register ("From", typeof(string),
				typeof (ClassProp3), new PropertyMetadata ("someone@example.com"));
		}

	}
}

