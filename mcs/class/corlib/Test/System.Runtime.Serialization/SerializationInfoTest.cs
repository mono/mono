//
// System.Runtime.Serialization.SerializationInfoTest.cs.
//
// Author: Carlos Alberto Cortez  <calberto.cortez@gmail.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class SerializationInfoTest
	{
#if NET_4_0
		[Test]
		public void ObjectType ()
		{
			SerializationInfo sinfo = new SerializationInfo (typeof (Point), new FormatterConverter ());
			Assert.AreEqual (typeof (Point), sinfo.ObjectType, "#A0");
		}

		[Test]
		public void IsAssemblyNameSetExplicit ()
		{
			SerializationInfo sinfo = new SerializationInfo (typeof (Point), new FormatterConverter ());
			Assert.AreEqual (false, sinfo.IsAssemblyNameSetExplicit, "#A0");

			string original_name = sinfo.AssemblyName;
			sinfo.AssemblyName = "CustomAssemblyName";
			Assert.AreEqual (true, sinfo.IsAssemblyNameSetExplicit, "#B0");
			Assert.AreEqual ("CustomAssemblyName", sinfo.AssemblyName, "#B1");

			sinfo.AssemblyName = original_name;
			Assert.AreEqual (true, sinfo.IsAssemblyNameSetExplicit, "#C0");
			Assert.AreEqual (original_name, sinfo.AssemblyName, "#C1");
		}

		[Test]
		public void IsFullTypeNameSetExplicit ()
		{
			SerializationInfo sinfo = new SerializationInfo (typeof (Point), new FormatterConverter ());
			Assert.AreEqual (false, sinfo.IsFullTypeNameSetExplicit, "#A0");

			string original_name = sinfo.FullTypeName;
			sinfo.FullTypeName = "CustomTypeName";
			Assert.AreEqual (true, sinfo.IsFullTypeNameSetExplicit, "#B0");
			Assert.AreEqual ("CustomTypeName", sinfo.FullTypeName, "#B1");

			sinfo.FullTypeName = original_name;
			Assert.AreEqual (true, sinfo.IsFullTypeNameSetExplicit, "#C0");
			Assert.AreEqual (original_name, sinfo.FullTypeName, "#C1");
		}
#endif
	}
}

