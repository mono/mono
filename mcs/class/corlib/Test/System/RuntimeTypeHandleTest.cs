//
// RuntimeTypeHandleTest.cs - Unit tests for System.RuntimeTypeHandle
//
// Author:
//	Robert Jordan  <robertj@gmx.net>
//
// Copyright (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class RuntimeTypeHandleTest
	{
		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void Serialization_Of_Empty_Handle ()
		{
			RuntimeTypeHandle handle = new RuntimeTypeHandle ();
			new BinaryFormatter ().Serialize (Stream.Null, handle);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotDotNet")] // it crashes the runtime on MS.NET
		public void GetModuleHandle_Of_Empty_Handle ()
		{
			RuntimeTypeHandle handle = new RuntimeTypeHandle ();
			handle.GetModuleHandle ();
		}
	}
}
