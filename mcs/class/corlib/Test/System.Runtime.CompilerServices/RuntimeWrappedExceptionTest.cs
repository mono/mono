//
// RuntimeWrappedExceptionTest.cs - NUnit Test Cases for 
//	System.Runtime.CompilerServices.RuntimeWrappedException
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace MonoTests.System.Runtime.CompilerServices {

	[TestFixture]
	public class RuntimeWrappedExceptionTest {

		internal RuntimeWrappedException rwe;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Type rwet = typeof (RuntimeWrappedException);
			ConstructorInfo[] ctors = rwet.GetConstructors (BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (ConstructorInfo ctor in ctors) {
				switch (ctor.GetParameters ().Length) {
				case 0:
					// mono
					rwe = (RuntimeWrappedException) ctor.Invoke (null);
					return;
				case 1:
					// ms
					rwe = (RuntimeWrappedException) ctor.Invoke (new object[1] { null });
					return;
				}
			}
			Assert.Ignore ("uho, couldn't figure out RuntimeWrappedException ctor");
		}

		[Test]
		public void WrappedException ()
		{
			Assert.IsNull (rwe.WrappedException, "WrappedException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_Null ()
		{
			rwe.GetObjectData (null, new StreamingContext (StreamingContextStates.All));
		}

		[Test]
		public void GetObjectData ()
		{
			SerializationInfo info = new SerializationInfo (typeof (RuntimeWrappedException), new FormatterConverter ());
			rwe.GetObjectData (info, new StreamingContext (StreamingContextStates.All));
			Assert.IsNull (info.GetValue ("WrappedException", typeof (object)), "WrappedException");
			// a SerializationException would occur if this was a bad name
		}
	}
}

#endif
