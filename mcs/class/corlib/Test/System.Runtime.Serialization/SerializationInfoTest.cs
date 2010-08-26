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
        
		[Test]
		public void SetType ()
		{
			SerializationInfo sinfo = new SerializationInfo (typeof (DateTime), new FormatterConverter ());

			Type point_type = typeof (Point);
			sinfo.SetType (point_type);
			Assert.AreEqual (point_type.FullName, sinfo.FullTypeName, "#A0");
			Assert.AreEqual (point_type.Assembly.FullName, sinfo.AssemblyName, "#A1");
#if NET_4_0
			Assert.AreEqual (point_type, sinfo.ObjectType, "#A2");
			Assert.AreEqual (false, sinfo.IsAssemblyNameSetExplicit, "#A3");
			Assert.AreEqual (false, sinfo.IsFullTypeNameSetExplicit, "#A4");

			sinfo.FullTypeName = "Point2";
			sinfo.AssemblyName = "NewAssembly";
			Type datetime_type = typeof (DateTime);
			sinfo.SetType (datetime_type);

			Assert.AreEqual (datetime_type.FullName, sinfo.FullTypeName, "#B0");
			Assert.AreEqual (datetime_type.Assembly.FullName, sinfo.AssemblyName, "#B1");
			Assert.AreEqual (datetime_type, sinfo.ObjectType, "#B2");
			Assert.AreEqual (false, sinfo.IsAssemblyNameSetExplicit, "#B3");
			Assert.AreEqual (false, sinfo.IsFullTypeNameSetExplicit, "#B4");
#endif
		}

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

