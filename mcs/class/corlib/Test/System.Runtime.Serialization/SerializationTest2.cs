//
// System.Runtime.Serialization.SerializationTest2.cs
//
// Test case for bug #421664.
//
// Author: Robert Jordan  <robertj@gmx.net>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

// We need a different namespace because we pollute it with
// a new ISerializable declaration.
namespace MonoTests.System.Runtime.Serialization2
{
	public interface ISerializable
	{
	}

	[Serializable]
	public class Class : ISerializable
	{
	}

	[TestFixture]
	public class SerializationTest2
	{
		[Test]
		public void TestSerialization ()
		{
			MemoryStream ms = new MemoryStream ();

			new BinaryFormatter ().Serialize (ms, new Class ());
			ms.Position = 0;
			new BinaryFormatter ().Deserialize (ms);
		}
	}
}
