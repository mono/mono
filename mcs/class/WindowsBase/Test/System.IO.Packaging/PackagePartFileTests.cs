using System;
using System.IO;
using System.IO.Packaging;
using NUnit.Framework;

namespace System.IO.Packaging.Tests
{
	[TestFixture]
	public class PackagePartFileTests
	{
		string path;

		[SetUp]
		public void Setup()
		{
			path = Path.GetTempFileName();
			Package.Open(path, FileMode.Create).Close();
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(path);
		}

		[Test]
		public void TestFileMode ()
		{
			Uri uri =new Uri("/somepart.xml", UriKind.Relative);
			FileMode[] modes =  { FileMode.Open, FileMode.OpenOrCreate, FileMode.Create };
			using (Package package = Package.Open(path))
			{
				PackagePart part;
				foreach (FileMode mode in modes)
				{
					part = package.CreatePart(uri, "application/xml");
					part.GetStream(mode, FileAccess.Write);
					package.DeletePart(uri);
				}

				part = package.CreatePart(uri, "application/xml");
				foreach (FileMode mode in modes)
					part.GetStream(mode, FileAccess.Write);
			}
		}

		[Test]
		public void TestFileMode2()
		{
			Uri uri = new Uri("/somepart.xml", UriKind.Relative);
			FileMode[] modes = { FileMode.Create, FileMode.CreateNew, FileMode.Truncate, FileMode.Append };
			FileMode[] otherModes = { FileMode.Open, FileMode.OpenOrCreate };

			using (Package package = Package.Open(path))
			{
				PackagePart part = package.CreatePart(uri, "application/xml");
				foreach (FileMode mode in modes)
				{
					try
					{
						part.GetStream(mode, FileAccess.Read);
						throw new Exception (string.Format ("Should not be able to open with: {0}", mode));
					}
					catch (IOException)
					{
						// This should be thrown
					}
				}

				foreach (FileMode mode in otherModes)
				{
					Stream s = part.GetStream(mode, FileAccess.Read);
					Assert.IsTrue(s.CanRead);
					Assert.IsFalse(s.CanWrite);
				}
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CreateNewPackagePart()
		{
			using (Package package = Package.Open(path))
			{
				PackagePart part = package.CreatePart(new Uri("/somepart.xml", UriKind.Relative), "application/xml");
				// CreateNew is not supported
				part.GetStream(FileMode.CreateNew, FileAccess.Write);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TruncatePackagePart()
		{
			using (Package package = Package.Open(path))
			{
				PackagePart part = package.CreatePart(new Uri("/somepart.xml", UriKind.Relative), "application/xml");
				// CreateNew is not supported
				part.GetStream(FileMode.Truncate, FileAccess.Write);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AppendPackagePart()
		{
			using (Package package = Package.Open(path))
			{
				PackagePart part = package.CreatePart(new Uri("/somepart.xml", UriKind.Relative), "application/xml");
				// CreateNew is not supported
				part.GetStream(FileMode.Append, FileAccess.Write);
			}
		}

		[Test]
		public void TestOverwrite()
		{
			using (Package package = Package.Open(path))
			{
				PackagePart part = package.CreatePart(new Uri("/Uri.xml", UriKind.Relative), "content/type");
				Stream s = part.GetStream(FileMode.OpenOrCreate, FileAccess.Write);
				StreamWriter sw = new StreamWriter(s);
				sw.Write("<test>aaaaaaa</test>");
				sw.Flush();

				Stream s5 = part.GetStream(FileMode.Create, FileAccess.ReadWrite);
				StreamWriter sw2 = new StreamWriter(s5);
				sw2.Write("<test>bbb</test>");
				sw2.Flush();

				// Verify that the part got overwritten correctly.
				Stream s6 = part.GetStream();
				StreamReader sr = new StreamReader(s6);
				Assert.AreEqual("<test>bbb</test>", sr.ReadToEnd());
			}
		}
	}
}