//
// ValidationContextTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[TestFixture]
	public class ValidationContextTest
	{
		[Test]
		public void Constructor ()
		{
			ValidationContext vc;
			try {
				vc = new ValidationContext (null, new FakeServiceProvider (), new Dictionary <object, object> ());
				Assert.Fail ("#A1-1");
			} catch (ArgumentNullException) {
				// success
			}

			try {
				vc = new ValidationContext ("stuff", null, new Dictionary<object, object> ());
			} catch {
				Assert.Fail ("#A1-2");
			}

			try {
				vc = new ValidationContext ("stuff", new FakeServiceProvider (), null);
			} catch {
				Assert.Fail ("#A1-3");
			}

			object o = "stuff";
			vc = new ValidationContext (o, null, null);
			Assert.AreSame (o, vc.ObjectInstance, "#A2-1");
			Assert.AreEqual (o.GetType ().Name, vc.DisplayName, "#A2-2");
			Assert.AreEqual (o.GetType (), vc.ObjectType, "#A2-3");

			o = 12;
			var dict = new Dictionary<object, object> () {
				{"stuff", 1}
			};
			vc = new ValidationContext (o, null, dict);
			Assert.IsNotNull (vc.Items, "#A3-1");
			Assert.AreEqual (1, vc.Items.Count, "#A3-2");
			Assert.AreNotSame (dict, vc.Items, "#A3-3");
			Assert.AreEqual (typeof (Dictionary <object, object>), vc.Items.GetType (), "#A3-4");
			Assert.AreEqual (1, vc.Items ["stuff"], "#A3-5");
			Assert.AreEqual (o.GetType ().Name, vc.DisplayName, "#A3-6");
			Assert.AreEqual (o.GetType (), vc.ObjectType, "#A3-7");
			Assert.AreEqual (null, vc.MemberName, "#A3-8");
			Assert.IsNotNull (vc.ServiceContainer, "#A3-9");
			Assert.AreEqual ("System.ComponentModel.DataAnnotations.ValidationContext+ValidationContextServiceContainer", vc.ServiceContainer.GetType ().FullName, "#A3-10");
		}

		[Test]
		public void ServiceContainer ()
		{
			var vc = new ValidationContext ("stuff", null, null);

			// Test the default container
			IServiceContainer container = vc.ServiceContainer;
			Assert.AreEqual ("System.ComponentModel.DataAnnotations.ValidationContext+ValidationContextServiceContainer", container.GetType ().FullName, "#A1");

			var fs1 = new FakeService1 ();
			container.AddService (typeof (FakeService1), fs1);
			container.AddService (typeof (FakeService2), CreateFakeService);

			var fs3 = new FakeService3 ();
			container.AddService (typeof (FakeService3), fs3, false);
			container.AddService (typeof (FakeService4), CreateFakeService, false);

			try {
				container.AddService (null, CreateFakeService);
				Assert.Fail ("#A2-1");
			} catch (ArgumentNullException) {
				// success
			}

			try {
				container.AddService (typeof (string), null);
			} catch {
				Assert.Fail ("#A2-2");
			}

			try {
				container.AddService (typeof (FakeService2), CreateFakeService);
				Assert.Fail ("#A2-3");
			} catch (ArgumentException) {
				// success
			}

			try {
				container.RemoveService (GetType ());
			} catch {
				Assert.Fail ("#A2-4");
			}

			try {
				container.RemoveService (null);
				Assert.Fail ("#A2-5");
			} catch (ArgumentNullException) {
				// success
			}

			try {
				container.GetService (null);
			} catch (ArgumentNullException) {
				// success
			}

			object o = container.GetService (typeof (FakeService1));
			Assert.IsNotNull (o, "#B1-1");
			Assert.AreSame (fs1, o, "#B1-2");

			o = container.GetService (typeof (FakeService2));
			Assert.IsNotNull (o, "#B2-1");
			Assert.AreEqual (typeof (FakeService2), o.GetType (), "#B2-2");

			o = container.GetService (typeof (FakeService3));
			Assert.IsNotNull (o, "#B3-1");
			Assert.AreSame (fs3, o, "#B3-2");

			o = container.GetService (typeof (FakeService4));
			Assert.IsNotNull (o, "#B4-1");
			Assert.AreEqual (typeof (FakeService4), o.GetType (), "#B4-2");

			o = container.GetService (GetType ());
			Assert.IsNull (o, "#B5");

			// Test custom container
			var fsc = new FakeServiceContainer ();
			vc = new ValidationContext ("stuff", fsc, null);
			container = vc.ServiceContainer;
			Assert.IsNotNull (container, "#B6-1");

			// LAMESPEC: MSDN says vc.ServiceContainer should be initialized with the passed container if it implements 
			// the IServiceContainer interface - not the case, though.
			Assert.AreNotSame (fsc, container, "#B6-2");
		}

		object CreateFakeService (IServiceContainer container, Type serviceType)
		{
			if (serviceType == typeof (FakeService2))
				return Activator.CreateInstance (serviceType);

			if (serviceType == typeof (FakeService4))
				return Activator.CreateInstance (serviceType);

			return null;
		}
	}

	class FakeService1
	{ }

	class FakeService2
	{ }

	class FakeService3
	{ }

	class FakeService4
	{ }

	class FakeServiceProvider : IServiceProvider
	{
		#region IServiceProvider Members

		public object GetService (Type serviceType)
		{
			return Activator.CreateInstance (serviceType);
		}

		#endregion
	}

	class FakeServiceContainer : IServiceContainer
	{
		public void AddService (Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
		}

		public void AddService (Type serviceType, ServiceCreatorCallback callback)
		{
		}

		public void AddService (Type serviceType, object serviceInstance, bool promote)
		{
		}

		public void AddService (Type serviceType, object serviceInstance)
		{
		}

		public void RemoveService (Type serviceType, bool promote)
		{
		}

		public void RemoveService (Type serviceType)
		{
		}

		public object GetService (Type serviceType)
		{
			return null;
		}
	}
#endif
}
