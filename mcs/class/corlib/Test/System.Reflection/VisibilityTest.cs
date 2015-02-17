// VisibilityTest.cs
//
//
// Author:
//       Marius Ungureanu <marius.ungureanu@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if !MONOTOUCH

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Reflection.VisibilityTypes
{
	[TestFixture]
	public class VisibilityTests
	{
		[Test]
		public void TestsExportedTypes ()
		{
			var types = typeof (VisibilityTests).Assembly.GetExportedTypes ();

			// Test visibility means that the class is public by applying and on the 'public' visibility of the nested items.
			CollectionAssert.DoesNotContain (types, typeof (InternalClass));
			CollectionAssert.Contains (types, typeof (PublicClass));

			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+InternalNested", true));
			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PrivateNested", true));
			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+ProtectedNested", true));
			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PublicNested", true));

			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+InternalNested", true));
			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PrivateNested", true));
			CollectionAssert.DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+ProtectedNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PublicNested", true));
		}

		[Test]
		public void TestsModuleTypes ()
		{
			var types = typeof (VisibilityTests).Module.GetTypes ();

			// Test that all the types defined exist.
			CollectionAssert.Contains (types, typeof (InternalClass));
			CollectionAssert.Contains (types, typeof (PublicClass));

			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+InternalNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PrivateNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+ProtectedNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PublicNested", true));

			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+InternalNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PrivateNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+ProtectedNested", true));
			CollectionAssert.Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PublicNested", true));
		}

		class InternalClass
		{
			internal class InternalNested {}
			private class PrivateNested {}
			protected class ProtectedNested {}
			public class PublicNested {}
		}

		public class PublicClass
		{
			internal class InternalNested {}
			private class PrivateNested {}
			protected class ProtectedNested {}
			public class PublicNested {}
		}
	}
}

#endif

