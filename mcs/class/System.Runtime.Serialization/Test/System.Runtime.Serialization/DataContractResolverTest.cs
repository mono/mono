//
// XmlObjectSerializerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <JAnkit@novell.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com

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

#if NET_4_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class DataContractResolverTest
	{
		[Test]
		public void UseCase1 ()
		{
			var ds = new DataContractSerializer (typeof (Colors), null, 10000, false, false, null, new MyResolver ());
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw))
				ds.WriteObject (xw, new ResolvedClass ());

			// xml and xml2 are equivalent in infoset, except for prefixes and position of namespace nodes. So the difference should not matter.
			string xml = @"<?xml version='1.0' encoding='utf-16'?><Colors xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns:d1p1='urn:dummy' i:type='d1p1:ResolvedClass' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><Baz xmlns='http://schemas.datacontract.org/2004/07/'>c74376f0-5517-4cb7-8a07-35026423f565</Baz></Colors>".Replace ('\'', '"');
			string xml2 = @"<?xml version='1.0' encoding='utf-16'?><Colors xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns:d1p1='urn:dummy' xmlns:d1p2='http://schemas.datacontract.org/2004/07/' i:type='d1p2:ResolvedClass' xmlns='http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization'><d1p2:Baz>c74376f0-5517-4cb7-8a07-35026423f565</d1p2:Baz></Colors>".Replace ('\'', '"');
			try {
				Assert.AreEqual (xml, sw.ToString ());
			} catch (AssertionException) {
				Assert.AreEqual (xml2, sw.ToString ());
			}
		}
	}

	public class MyResolver : DataContractResolver
	{
		public override bool TryResolveType (Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			//Console.WriteLine ("TryResolveType: {0} {1}", type, declaredType);
			if (knownTypeResolver.TryResolveType (type, declaredType, null, out typeName, out typeNamespace))
				return true;
			return SafeResolveType (type, out typeName, out typeNamespace);
		}

		XmlDictionary dic = new XmlDictionary ();

		bool SafeResolveType (Type type, out XmlDictionaryString name, out XmlDictionaryString ns)
		{
			// Console.WriteLine ("SafeResolveType: {0}", type);
			name = dic.Add (type.Name);
			ns = dic.Add (type.Namespace ?? "urn:dummy");
			return true;
		}

		public override Type ResolveName (string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			//Console.WriteLine ("ResolveName: {0} {1} {2}", typeName, typeNamespace, declaredType);
			return knownTypeResolver.ResolveName (typeName, typeNamespace, declaredType, null);
		}
	}
}

[DataContract]
public class ResolvedClass
{
	[DataMember]
	public Guid Baz = Guid.Parse ("c74376f0-5517-4cb7-8a07-35026423f565");
}

#endif
