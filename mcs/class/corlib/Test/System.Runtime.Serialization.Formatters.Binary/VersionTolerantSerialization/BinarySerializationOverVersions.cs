using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Formatters.Binary
{
		//TODO: add tests that use SoapFormatter (if you go this
		//	  path, it would be interesting also to test a
		//	  custom formatter, like the XML-RPC.net one!)
	[TestFixture]
	public class BinarySerializationOverVersions {

		[Test]
		public void TestDroppedField () //eliminate CountryCode
		{
			Deserialize ("2.0", Serialize ("3.0"));
		}

		[Test]
		public void TestAddedField () //add CountryCode
		{
			Deserialize ("3.0", Serialize ("2.0"));
		}

		[Test]
		public void TestAddedFieldWithData () //add Country
		{
			Deserialize( "1.0", Serialize ("2.0"));
		}

		[Test]
		public void TestDroppedFieldWithData () //eliminate Country
		{
			Deserialize ("2.0", Serialize ("3.0"));
		}

		[Test]
		public void TestAddedFieldWithOptionalAttrib () //add PostCode
		{
			Deserialize ("4.0", Serialize ("3.0"));
		}

		[Test]
		public void TestDroppedFieldWithOptionalAttrib () //eliminate PostCode
		{
			Deserialize ("3.0", Serialize ("4.0"));
		}

		[Test]
		public void TestAddedFieldWithOptionalAttribAndData () //add AreaCode
		{
			Deserialize ("5.0", Serialize ("4.0"));
		}

		[Test]
		public void TestDroppedFieldWithOptionalAttribAndData () //eliminate AreaCode
		{
			Deserialize ("4.0", Serialize ("5.0"));
		}

		[Test]
		public void TestDroppedPrimitiveTypeField() //eliminate Id (int)
		{
			Deserialize ("5.0", Serialize ("6.0"));
		}

		[Test]
		public void TestAddedPrimitiveTypeField () //add Id (int)
		{
			Deserialize ("6.0", Serialize ("5.0"));
		}

		const string AssemblyName = "Address.dll";
		const string TypeName = "VersionTolerantSerializationTestLib.Address";

		class Serializer : MarshalByRefObject {
			static IFormatter formatter = new BinaryFormatter ();
			public static IFormatter Formatter { get { return formatter; } }

			public byte[] Serialize (string version) {
				var assembly = Assembly.LoadFrom (Find (version));
				var type = assembly.GetType (TypeName);
				var obj = Activator.CreateInstance (type);
				var stream = new MemoryStream ();
				
				Formatter.Serialize (stream, obj);
				return stream.ToArray ();
			}

			public void Deserialize (string version, byte[] payload) {
				var assembly = Assembly.LoadFrom (Find (version));
				var stream = new MemoryStream (payload);
				var obj = Formatter.Deserialize (stream);
				//Console.WriteLine ("obj version {0} -> {1}", version, obj);
			}
		}

		private static byte[] Serialize (string assemblyVersion)
		{
			var setup = new AppDomainSetup ();
			setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			AppDomain ad = AppDomain.CreateDomain (assemblyVersion, null, setup);
			Serializer ser = (Serializer) ad.CreateInstanceAndUnwrap (typeof(Serializer).Assembly.FullName, typeof(Serializer).FullName);
			byte[] stuff = ser.Serialize (assemblyVersion);
			AppDomain.Unload (ad);
			return stuff;
		}

		private static void Deserialize (string assemblyVersion, byte[] payload)
		{
			var setup = new AppDomainSetup ();
			setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			AppDomain ad = AppDomain.CreateDomain (assemblyVersion, null, setup);
			Serializer ser = (Serializer) ad.CreateInstanceAndUnwrap (typeof(Serializer).Assembly.FullName, typeof(Serializer).FullName);
			ser.Deserialize (assemblyVersion, payload);
			AppDomain.Unload (ad);
		}

		static void Main () {
			throw new Exception ();
		}


		private static string Find (string assemblyVersion)
		{
			string initDir = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			return Find (assemblyVersion, initDir);
		}

		private static string Find (string assemblyVersion, string path)
		{
			return Path.Combine (Path.Combine (path, assemblyVersion), AssemblyName);
		}
	}
}
