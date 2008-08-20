//
// ExtensionCollectionTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ExtensionCollectionTest
	{
		class MyExtensibleObject : IExtensibleObject<MyExtensibleObject>
		{
			IExtensionCollection<MyExtensibleObject> _extensions;

			public IExtensionCollection<MyExtensibleObject> Extensions {
				get {
					if (_extensions == null)
						_extensions = new ExtensionCollection<MyExtensibleObject> (this);
					return _extensions;
				}
			}
		}

		abstract class MyExtensionBase : IExtension<MyExtensibleObject>
		{
			public bool IsAttached {
				get;
				private set;
			}

			public void Attach (MyExtensibleObject owner) {
				IsAttached = true;
			}

			public void Detach (MyExtensibleObject owner) {
				IsAttached = false;
			}
		}

		class MyExtension1 : MyExtensionBase
		{
		}

		class MyExtension2 : MyExtensionBase
		{
		}

		[Test]
		public void Add_Remove_Call_Attach () {

			MyExtensibleObject extObj = new MyExtensibleObject ();
			MyExtension1 ext = new MyExtension1 ();
			Assert.AreEqual (false, ext.IsAttached, "IsAttached #1");
			extObj.Extensions.Add (ext);
			Assert.AreEqual (true, ext.IsAttached, "IsAttached #2");
			extObj.Extensions.Remove (ext);
			Assert.AreEqual (false, ext.IsAttached, "IsAttached #3");
		}

		[Test]
		public void Clear_Calls_Attach () {

			MyExtensibleObject extObj = new MyExtensibleObject ();
			MyExtension1 ext1 = new MyExtension1 ();
			MyExtension2 ext2 = new MyExtension2 ();
			extObj.Extensions.Add (ext1);
			extObj.Extensions.Add (ext2);
			Assert.AreEqual (true, ext1.IsAttached, "IsAttached #1");
			Assert.AreEqual (true, ext2.IsAttached, "IsAttached #2");
			extObj.Extensions.Clear ();
			Assert.AreEqual (false, ext1.IsAttached, "IsAttached #3");
			Assert.AreEqual (false, ext2.IsAttached, "IsAttached #4");
		}
	}
}
