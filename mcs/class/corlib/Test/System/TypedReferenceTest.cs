//
// TypedReferenceTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System
{
#if !MONODROID // Type load segfaults the runtime on ARM64 (https://gist.github.com/grendello/334d06c45376602a9afc)
	[TestFixture]
	// Currently causes the WASM runtime to abort
	[Category("NotWasm")]
	public class TypedReferenceTest
	{
		struct TestFields
		{
			public int MaxValue;
		}

		[Test]
		public void GetTargetType ()
		{
			TestFields fields = new TestFields { MaxValue = 1234 };

			TypedReference ti = __makeref(fields);
			Assert.AreEqual (typeof (TestFields), TypedReference.GetTargetType (ti));
		}

		struct AStruct {
			public string b;
		}

		class CClass {
			public AStruct a;
		}

		[Test]
		public void MakeTypedReference ()
		{
			var o = new CClass () { a = new AStruct () { b = "5" }};
			TypedReference r = TypedReference.MakeTypedReference (o, new FieldInfo[] { typeof (CClass).GetField ("a"), typeof (AStruct).GetField ("b") });
			Assert.AreEqual ("5", TypedReference.ToObject (r));
		}
	}
#endif
}
