//
// AttributesTest.cs
//
// Author:
//	Eyal Alaluf <mainsoft.com>
//
// Copyright (C) 2008 Mainsoft.co http://www.mainsoft.com

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

//
// This test code contains tests for attributes in System.Runtime.Serialization
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class AttributesTest
	{
		[Test]
		public void TestContractNamespaceAttribute ()
		{
			ContractNamespaceAttribute x = new ContractNamespaceAttribute ("test");
			Assert.AreEqual (null, x.ClrNamespace, "#01");
			Assert.AreEqual ("test", x.ContractNamespace, "#02");
		}

		[Test]
		public void TestDataContractAttribute ()
		{
			DataContractAttribute x = new DataContractAttribute ();
			Assert.AreEqual (null, x.Name, "#01");
			Assert.AreEqual (null, x.Namespace, "#02");
		}

		[Test]
		public void TestDataMemberAttribute ()
		{
			DataMemberAttribute x = new DataMemberAttribute ();
			Assert.AreEqual (null, x.Name, "#01");
			Assert.AreEqual (-1, x.Order, "#02");
			Assert.AreEqual (true, x.EmitDefaultValue, "#02");
			Assert.AreEqual (false, x.IsRequired, "#02");
		}
	}
}
