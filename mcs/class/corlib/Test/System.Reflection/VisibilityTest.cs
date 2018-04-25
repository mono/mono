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

#if !MONOTOUCH && !FULL_AOT_RUNTIME

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Reflection.VisibilityTypes
{
	[TestFixture]
	public class VisibilityTests
	{
		static void DoesNotContain (IEnumerable collection, object val)
		{
			 Assert.That(collection, Has.No.Member(val));
		}

		static void Contains (IEnumerable collection, object val)
		{
			 Assert.That(collection, Has.Member(val));
		}

		[Test]
		public void TestsExportedTypes ()
		{
			var types = typeof (VisibilityTests).Assembly.GetExportedTypes ();

			// Test visibility means that the class is public by applying and on the 'public' visibility of the nested items.
			DoesNotContain (types, typeof (InternalClass));
			Contains (types, typeof (PublicClass));

			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+InternalNested", true));
			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PrivateNested", true));
			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+ProtectedNested", true));
			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PublicNested", true));

			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+InternalNested", true));
			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PrivateNested", true));
			DoesNotContain (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+ProtectedNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PublicNested", true));
		}

		[Test]
		public void TestsModuleTypes ()
		{
			var types = typeof (VisibilityTests).Module.GetTypes ();

			// Test that all the types defined exist.
			Contains (types, typeof (InternalClass));
			Contains (types, typeof (PublicClass));

			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+InternalNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PrivateNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+ProtectedNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+InternalClass+PublicNested", true));

			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+InternalNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PrivateNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+ProtectedNested", true));
			Contains (types, Type.GetType ("MonoTests.System.Reflection.VisibilityTypes.VisibilityTests+PublicClass+PublicNested", true));
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

