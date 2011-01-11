//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Xaml;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class AttachablePropertyServicesTest
	{
		[Test]
		public void NullArgument ()
		{
			// null is not rejected
			AttachablePropertyServices.GetAttachedPropertyCount (null);
			AttachablePropertyServices.CopyPropertiesTo (null, null, 0);
		}

		[Test]
		public void NonStoreArgument ()
		{
			// non-store object either.
			AttachablePropertyServices.GetAttachedPropertyCount (new object ());
		}

		[Test]
		public void NonStoreGetSet ()
		{
			var a = new object ();
			AttachablePropertyServices.SetProperty (a, Attachable.FooIdentifier, "x");
			string v;
			Assert.IsTrue (AttachablePropertyServices.TryGetProperty<string> (a, Attachable.FooIdentifier, out v), "#1");
			Assert.AreEqual ("x", v, "#2");
		}

		[Test]
		public void StoreGetSet ()
		{
			var a = new AttachedWrapper2 ();
			AttachedWrapper2.SetFoo (a, "x");
			string v;
			Assert.IsFalse (AttachablePropertyServices.TryGetProperty<string> (a, AttachedWrapper2.FooIdentifier, out v), "#1");
			Assert.AreEqual ("x", AttachedWrapper2.GetFoo (a), "#2");

			Assert.AreEqual (1, AttachedWrapper2.PropertyCount, "#3");
		}
	}
}
