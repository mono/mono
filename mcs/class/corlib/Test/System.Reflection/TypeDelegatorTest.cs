//
// TypeDelegatorTest.cs - NUnit Test Cases for the TypeDelegator class
//
// Zoltan Varga (vargaz@freemail.hu)
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

using System;
using System.Threading;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
[TestFixture]
public class TypeDelegatorTest {

	[Test]
	public void IsAssignableFrom ()
	{
		TypeDelegator td = new TypeDelegator (typeof (int));

		Assert.AreEqual (true, typeof (int).IsAssignableFrom (td));
		Assert.AreEqual (false, typeof (string).IsAssignableFrom (td));
		Assert.AreEqual (true, td.IsAssignableFrom (typeof (int)));
		Assert.AreEqual (false, td.IsAssignableFrom (typeof (string)));
    }

	[Test]
	public void CreateInstance ()
	{
		Assert.AreEqual (typeof (int[]), Array.CreateInstance (new TypeDelegator (typeof (int)), 100).GetType ());
    }
		
	[Test]
	public void Properties ()
	{
		Assert.AreEqual (false, new TypeDelegator (typeof (IComparable)).IsClass);
		Assert.AreEqual (false, new TypeDelegator (typeof (IComparable)).IsValueType);
		Assert.AreEqual (false, new TypeDelegator (typeof (IComparable)).IsEnum);
		Assert.AreEqual (true, new TypeDelegator (typeof (IComparable)).IsInterface);

		Assert.AreEqual (true, new TypeDelegator (typeof (TypeDelegatorTest)).IsClass);
		Assert.AreEqual (false, new TypeDelegator (typeof (TypeDelegatorTest)).IsValueType);
		Assert.AreEqual (false, new TypeDelegator (typeof (TypeDelegatorTest)).IsInterface);

		Assert.AreEqual (false, new TypeDelegator (typeof (TypeCode)).IsClass);
		Assert.AreEqual (false, new TypeDelegator (typeof (TypeCode)).IsInterface);
		Assert.AreEqual (true, new TypeDelegator (typeof (TypeCode)).IsValueType);
		Assert.AreEqual (true, new TypeDelegator (typeof (TypeCode)).IsEnum);
	}
}
}
