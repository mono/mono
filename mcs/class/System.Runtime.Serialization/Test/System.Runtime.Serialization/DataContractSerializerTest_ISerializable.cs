//
// DataContractSerializerTest_ISerializable.cs
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2015 Xamarin Inc. All rights reserved.
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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class DataContractSerializerTest_ISerializable
	{
		[Serializable]
		sealed class TestClassISerializable : ISerializable
		{
			public string Foo { get; }

			public TestClassISerializable (string foo)
			{
				Foo = foo;
			}

			TestClassISerializable (SerializationInfo info, StreamingContext context)
			{
				Foo = info.GetString ("foo");
			}

			void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", Foo);
			}
		}

		// Tests that the ISerializable constructor is properly invoked, which
		// regressed when integrating MS Reference Source DCS, et. al.:
		//   https://bugzilla.xamarin.com/show_bug.cgi?id=37171
		[Test]
		public void TestISerializableCtor ()
		{
			var serializer = new DataContractSerializer (
				typeof(TestClassISerializable),
				new DataContractSerializerSettings {
					DataContractResolver = new Resolver ()
				}
			);

			var stream = new MemoryStream ();

			var expected = new TestClassISerializable ("hello world");
			serializer.WriteObject (stream, expected);

			stream.Flush ();
			stream.Position = 0;

			var actual = (TestClassISerializable)serializer.ReadObject (stream);

			Assert.AreEqual (expected.Foo, actual.Foo, "#DCS_ISer_Ctor");
		}

		// Resolver to force DCS to serialize any type, ensuring the ISerializable
		// path will be taken for objects implementing that interface
		class Resolver : DataContractResolver
		{
			public override Type ResolveName (string typeName, string typeNamespace,
				Type declaredType, DataContractResolver knownTypeResolver)
			{
				return Type.GetType (typeNamespace == null
					? typeName
					: typeNamespace + "." + typeName
				);
			}

			public override bool TryResolveType (Type type, Type declaredType,
				DataContractResolver knownTypeResolver,
				out XmlDictionaryString typeName,
				out XmlDictionaryString typeNamespace)
			{
				var name = type.FullName;
				var namesp = type.Namespace;
				name = name.Substring (type.Namespace.Length + 1);
				typeName = new XmlDictionaryString (XmlDictionary.Empty, name, 0);
				typeNamespace = new XmlDictionaryString (XmlDictionary.Empty, namesp, 0);
				return true;
			}
		}
	}
}
