//
// System.Reflection.MethodInfo Test Cases
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class MethodInfoTest : Assertion
	{
		[DllImport ("libfoo", EntryPoint="foo", CharSet=CharSet.Unicode, ExactSpelling=false, PreserveSig=true, SetLastError=true, BestFitMapping=true, ThrowOnUnmappableChar=true)]
		public static extern void dllImportMethod ();

		[MethodImplAttribute(MethodImplOptions.PreserveSig)]
		public void preserveSigMethod () {
		}

		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void synchronizedMethod () {
		}

#if NET_2_0
		[Test]
		public void PseudoCustomAttributes ()
		{
			Type t = typeof (MethodInfoTest);

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod ("dllImportMethod").GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			AssertEquals (CallingConvention.Winapi, attr.CallingConvention);
			AssertEquals ("foo", attr.EntryPoint);
			AssertEquals ("libfoo", attr.Value);
			AssertEquals (CharSet.Unicode, attr.CharSet);
			AssertEquals (false, attr.ExactSpelling);
			AssertEquals (true, attr.PreserveSig);
			AssertEquals (true, attr.SetLastError);
			AssertEquals (true, attr.BestFitMapping);
			AssertEquals (true, attr.ThrowOnUnmappableChar);

			PreserveSigAttribute attr2 = (PreserveSigAttribute)((t.GetMethod ("preserveSigMethod").GetCustomAttributes (true)) [0]);

			// This doesn't work under MS.NET
			/*
			  MethodImplAttribute attr3 = (MethodImplAttribute)((t.GetMethod ("synchronizedMethod").GetCustomAttributes (true)) [0]);
			*/
		}
#endif
	}
}

