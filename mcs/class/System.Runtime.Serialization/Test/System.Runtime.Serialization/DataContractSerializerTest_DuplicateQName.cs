//
// DataContractSerializerTest_DuplicateQName.cs
//
// Author:
//	David Ferguson <davecferguson@gmail.com>
//
// Copyright (C) 2012 Dell AppAssure http://www.appassure.com
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
// This test code contains tests for the DataContractSerializer
// concerning duplicate Qualified Names for the object graph and known types
//
using System;
using System.IO;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class DataContractSerializerTest_DuplicateQName
	{
		[DataContract (Name="name", Namespace="http://somecompany.com/function/api/2010/05")]
		[Serializable]
		public class DataContractBase
		{
			public DataContractBase ()
			{
			}

			public DataContractBase (string val)
			{
				BaseValue1 = val;
			}

			[DataMember(Name="baseValue1", Order=1)]
			public string BaseValue1 { get; set; }

		}

		[DataContract (Name="name", Namespace="http://somecompany.com/function/api/2010/05")]
		[Serializable]
		public class DataContract1 : DataContractBase
		{
			public DataContract1 ()
			{
			}

			public DataContract1 (string val) : base (val)
			{
			}
		}

		[DataContract(Name = "name", Namespace = "http://somecompany.com/function/api/2010/05")]
		[Serializable]
		public class DataContract2
		{
			[DataMember]
			public DataContract3 DataContract3 { get; set; }
		}

		[DataContract(Name = "name", Namespace = "http://somecompany.com/function/api/2010/05")]
		[Serializable]
		public class DataContract3
		{

		}

		[DataContract(Name = "name", Namespace = "http://somecompany.com/function/api/2010/05")]
		[Serializable]
		public class DataContract4
		{
			[DataMember(Name = "name")]
			public double
				Test1;
		}

		[Test]
		public void TestMultipleDataContractSameDataContractNameAndNamespace ()
		{
			// DataContract1 derives from DataContractBase and they both have
			// the same QName specified in their respective DataContractAttribute.
			var serializer = new DataContractSerializer (typeof(DataContract1));
			var serializerBase = new DataContractSerializer (typeof(DataContractBase));

			Assert.IsNotNull (serializer);
			Assert.IsNotNull (serializerBase);
		}

		[Test]
		public void TestDataContractWithPropertyHavingSameQName ()
		{
			// DataContract2 has a property of DataContract3.  DataContract2 and
			// DataContract3 both have the same QName specified in their
			// respective DataContractAttribute.  This was causing a failure due
			// to the QName being saved in the SerializationMap twice. Bug 4794.
			var serializer2 = new DataContractSerializer (typeof(DataContract2));
			var d = new DataContract2 ();
			var ms = new MemoryStream (2048);

			Assert.IsNotNull (serializer2, "Failed to create the serializer for DataContract2");

			serializer2.WriteObject (ms, d);
			ms.Position = 0;

			var d2 = serializer2.ReadObject (ms) as DataContract2;

			Assert.IsNotNull (d2, "Failed to deserialize the data buffer into a DataContract2");
		}

		[Test]
		public void TestDataContractWithPrimitiveHavingSameQName ()
		{
			// This test verifies that a primitive with the same qname as the
			// DataContract succeeds in serializing and deserializing
			var serializer4 = new DataContractSerializer (typeof(DataContract4));

			var d = new DataContract4 ();
			var ms = new MemoryStream (2048);

			Assert.IsNotNull (serializer4, "Failed to create the serializer for DataContract4");

			d.Test1 = 3.1416;
			serializer4.WriteObject (ms, d);
			ms.Position = 0;

			var d2 = serializer4.ReadObject (ms) as DataContract4;

			Assert.IsNotNull (d2, "Failed to deserialize the data buffer into a DataContract4");
			Assert.AreEqual (d2.Test1, 3.1416, "Rehydrated Test1 property did not match original");
			Assert.AreNotSame (d2, d, "The instances are the same and should not be");
		}

		[Test]
		public void TestKnownTypes ()
		{
			// The .NET behavior is that the KnownTypes collection is not populated unless you
			// do so through the constructor.  It even ignores attributes on the type indicating
			// a known type.
			var serializer = new DataContractSerializer (typeof(DataContract1));
			var serializerWithKnownType = new DataContractSerializer (
				typeof(DataContract2),
				new [] { typeof(DataContract3) }
			);

			Assert.AreEqual (serializer.KnownTypes.Count, 0, "Expected an empty known type collection");
			Assert.AreEqual (serializerWithKnownType.KnownTypes.Count, 1, "Known count type did not match");
		}
	}
}