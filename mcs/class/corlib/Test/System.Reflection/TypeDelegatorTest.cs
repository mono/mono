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
#if !TARGET_JVM
using System.Reflection.Emit;
#endif // TARGET_JVM

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
[TestFixture]
public class TypeDelegatorTest : Assertion {

	[Test]
	public void IsAssignableFrom ()
	{
		TypeDelegator td = new TypeDelegator (typeof (int));

		AssertEquals (true, typeof (int).IsAssignableFrom (td));
		AssertEquals (false, typeof (string).IsAssignableFrom (td));
		AssertEquals (true, td.IsAssignableFrom (typeof (int)));
		AssertEquals (false, td.IsAssignableFrom (typeof (string)));
    }

	[Test]
	public void CreateInstance ()
	{
		AssertEquals (typeof (int[]), Array.CreateInstance (new TypeDelegator (typeof (int)), 100).GetType ());
    }
		
	[Test]
	public void Properties ()
	{
		AssertEquals (false, new TypeDelegator (typeof (IComparable)).IsClass);
		AssertEquals (false, new TypeDelegator (typeof (IComparable)).IsValueType);
		AssertEquals (false, new TypeDelegator (typeof (IComparable)).IsEnum);
		AssertEquals (true, new TypeDelegator (typeof (IComparable)).IsInterface);

		AssertEquals (true, new TypeDelegator (typeof (TypeDelegatorTest)).IsClass);
		AssertEquals (false, new TypeDelegator (typeof (TypeDelegatorTest)).IsValueType);
		AssertEquals (false, new TypeDelegator (typeof (TypeDelegatorTest)).IsInterface);

		AssertEquals (false, new TypeDelegator (typeof (TypeCode)).IsClass);
		AssertEquals (false, new TypeDelegator (typeof (TypeCode)).IsInterface);
		AssertEquals (true, new TypeDelegator (typeof (TypeCode)).IsValueType);
		AssertEquals (true, new TypeDelegator (typeof (TypeCode)).IsEnum);
	}
}
}
