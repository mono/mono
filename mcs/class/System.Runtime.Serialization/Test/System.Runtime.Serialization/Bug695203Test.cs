using System;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.CodeDom.Compiler;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class Bug695203Test
	{
		[Test]
		public void DoTest ()
		{
			using (var mem = new MemoryStream ()) {
				BaseClass data = new DerivedA1 { Code = "1", CodeA = "A", CodeA1 = "A1" };
				Serialize (data, mem);
				mem.Position = 0;
				var docResult = Deserialize<BaseClass> (mem);
			}

			using (var mem = new MemoryStream ()) {
				BaseClass data = new DerivedA2 { Code = "1", CodeA = "A", CodeA2 = "A1" };

				Serialize (data, mem);

				mem.Position = 0;
				var docResult = Deserialize<BaseClass> (mem);
			}
		}

		void Serialize<T> (T instance, Stream destinationStream)
		{
			var serializer = new DataContractSerializer (typeof (T), null, int.MaxValue, false, true, null);

			using (var writer = XmlDictionaryWriter.CreateBinaryWriter (destinationStream, null, null, false))
				serializer.WriteObject (writer, instance);
		}


		public static T Deserialize<T> (Stream sourceStream)
		{
			var serializer = new DataContractSerializer (typeof (T), null, int.MaxValue, false, true, null);

			using (var reader = XmlDictionaryReader.CreateBinaryReader(sourceStream, XmlDictionaryReaderQuotas.Max))
				return (T) serializer.ReadObject (reader);
		}

		[DataContract]
		[KnownType (typeof (DerivedA1))]
		[KnownType (typeof (DerivedA))]
		public abstract class BaseClass
		{
			[DataMember]
			public string Code { get; set; }
		}


		[DataContract]
		[KnownType (typeof (DerivedA1))]
		[KnownType (typeof (DerivedA2))]
		public abstract class DerivedA : BaseClass
		{
			public string CodeA { get; set; }
		}

		[DataContract]
		public class DerivedA1 : DerivedA
		{
			[DataMember]
			public string CodeA1 { get; set; }
		}

		[DataContract]
		public class DerivedA2 : DerivedA
		{
			[DataMember]
			public string CodeA2 { get; set; }
		}
	}
}

