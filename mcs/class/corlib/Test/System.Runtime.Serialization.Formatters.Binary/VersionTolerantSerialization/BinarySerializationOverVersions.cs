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
	[TestFixture]
	public class BinarySerializationOverVersions {

		static readonly string dirName = typeof(VersionTolerantSerializationTestLib.Address).Namespace;
		static readonly string assemblyName = typeof(VersionTolerantSerializationTestLib.Address).Name;
		static readonly string assemblyFileName = assemblyName + ".dll";
		static readonly string binName = Path.GetFileName (Assembly.GetExecutingAssembly ().Location);
		const bool cleanup = true;
		enum OopOperation { Serialize, Deserialize }
		static IFormatter formatter = new BinaryFormatter ();


		//TODO: add tests that use SoapFormatter (if you go this
		//	  path, it would be interesting also to test a
		//	  custom formatter, like the XML-RPC.net one!)
		public virtual IFormatter Formatter { get { return formatter; } }

		static void Main (string [] args)
		{
			var p = new BinarySerializationOverVersions ();

			/* this block is useful for testing this without NUnit:
			if (args.Length == 0)
			{
				Console.WriteLine ("Starting...");
				p.TestDroppedField ();
				Console.Write ("x");
				p.TestAddedField ();
				Console.Write ("x");

				p.TestAddedFieldWithData ();
				Console.Write ("x");
				p.TestDroppedFieldWithData ();
				Console.Write ("x");

				p.TestAddedFieldWithOptionalAttrib ();
				Console.Write ("x");
				p.TestDroppedFieldWithOptionalAttrib ();
				Console.Write ("x");

				p.TestAddedFieldWithOptionalAttribAndData ();
				Console.Write ("x");
				p.TestDroppedFieldWithOptionalAttribAndData ();
				Console.Write ("x");
				
				Console.WriteLine ();
				Environment.Exit (0);
			}*/

			if (args.Length < 2)
				throw new Exception ("Please specify arguments");

			if (args [0] == OopOperation.Serialize.ToString ())
			{
				p.SerializeToFile (args [1]);
			}
			else if (args [0] == OopOperation.Deserialize.ToString ())
			{
				p.DeserializeFromFile (args [1]);
			}
			else
			{
				throw new Exception(String.Format ("{0} operation not recognized. Only {Serialize|Deserialize} operations are supported.", args [0]));
			}
		}

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

		private static string Serialize (string assemblyVersion)
		{
			return SerializeOOP (SetEnvironment (assemblyVersion)); ;
		}

		private static void Deserialize (string assemblyVersion, string filename)
		{
			DeserializeOOP (SetEnvironment (assemblyVersion), filename);
		}

		private static string SerializeOOP (string executionDir)
		{
			string filename = Path.GetTempFileName ();
			var p = new Process ();
			p.StartInfo.WorkingDirectory = executionDir;
			p.StartInfo.FileName = Path.Combine (executionDir, binName);
			p.StartInfo.Arguments = OopOperation.Serialize.ToString () + " \"" + filename + "\"";
			p.StartInfo.UseShellExecute = false;
			p.Start();
			p.WaitForExit();
			if (p.ExitCode != 0)
				throw new Exception ("Problem in serialization operation");

			if (cleanup)
				Directory.Delete (executionDir, true);

			return filename;
		}

		private static void DeserializeOOP (string executionDir, string filename)
		{
			var p = new Process ();
			p.StartInfo.WorkingDirectory = executionDir;
			p.StartInfo.FileName = Path.Combine (executionDir, binName);
			p.StartInfo.Arguments = OopOperation.Deserialize.ToString () + " \"" + filename + "\"";
			p.StartInfo.UseShellExecute = false;
			p.Start ();
			p.WaitForExit ();
			if (p.ExitCode != 0)
				throw new Exception("Problem in deserialization operation");

			if (cleanup)
			{
				Directory.Delete (executionDir, true);
				File.Delete (filename);
			}
		}

		private static string SetEnvironment (string assemblyVersion)
		{
			if (!assemblyVersion.Contains ("."))
				throw new NotSupportedException ("The version number should contain a dot, i.e.: 2.0");

			string tmpDir = Path.Combine (Path.GetTempPath (), "deleteme" + Guid.NewGuid ());
			Directory.CreateDirectory (tmpDir);
			string currentBin = Assembly.GetExecutingAssembly ().Location;
			File.Copy (currentBin, Path.Combine (tmpDir, Path.GetFileName (currentBin)));

			string ass = Find (assemblyVersion);
			File.Copy (ass, Path.Combine (tmpDir, Path.GetFileName (ass)));
			return tmpDir;
		}

		private static string Find (string assemblyVersion)
		{
			string initDir = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			return Find (assemblyVersion, initDir);
		}

		private static string Find (string assemblyVersion, string path)
		{
			//Console.WriteLine ("Looking in " + path);
			string test;

			if (!path.Contains(assemblyVersion))
			{
				//outside or here

				if (Path.GetFileName(path) == dirName)
				{
					test = Path.Combine (path, assemblyVersion);
					if (Directory.Exists (test))
						return Find (assemblyVersion, test);
					else
						throw new DirectoryNotFoundException (String.Format ("{0} was not found", test));
				}

				test = Path.Combine (path, dirName);
				if (Directory.Exists (test))
					return Find (assemblyVersion, test);

				return Find (assemblyVersion, Path.Combine (path, ".."));
			}
			else
			{
				//inside
				test = Path.Combine (path, assemblyFileName);
				if (File.Exists (test))
					return test;

				test = Path.Combine (path, "bin");
				if (Directory.Exists (test))
					return Find (assemblyVersion, test);

				test = Path.Combine (path, "Debug");
				if (Directory.Exists (test))
					return Find (assemblyVersion, test);

				test = Path.Combine (path, "Release");
				if (Directory.Exists (test))
					return Find (assemblyVersion, test);

				test = Path.Combine (path.Replace ("Debug", "Release"), assemblyFileName);
				if (File.Exists (test))
					return test;

				throw new NotSupportedException(
					String.Format(
						"The tree is not predictible according to the philosophy of the test. (Stuck in {0})",
						path));
			}
		}

		private void SerializeToFile (string filename)
		{
			var type = typeof (VersionTolerantSerializationTestLib.Address);
			object obj = Activator.CreateInstance (type);
			Stream stream = new FileStream (filename,
										   FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			Formatter.Serialize (stream, obj);
			stream.Dispose ();
		}

		private void DeserializeFromFile (string filename)
		{
			//Console.WriteLine("Trying to deserialize {0}...", filename);
			FileStream readStream = new FileStream (filename, FileMode.Open);
			//var readData = 
			Formatter.Deserialize (readStream);
		}
	}
}
