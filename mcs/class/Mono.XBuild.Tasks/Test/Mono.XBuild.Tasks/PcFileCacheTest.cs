//
// PcFileCacheTest.cs
//
// Author:
//       Antonius Riha <antoniusriha@gmail.com>
//
// Copyright (c) 2013 Antonius Riha
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.IO;
using Mono.PkgConfig;
using NUnit.Framework;

namespace MonoTests.Mono.PkgConfig
{
	[TestFixture]
	public class PcFileCacheTest
	{
		static readonly string cacheDir = "testcache";
		static readonly string pcCacheFileName = "pkgconfig-cache-2.xml";
		static readonly string pcCacheFilePath = Path.Combine (cacheDir, pcCacheFileName);
		static readonly string pkgConfigDir = "testpkgconfig";

		[SetUp]
		public void Setup ()
		{
			Directory.CreateDirectory (cacheDir);
			Directory.CreateDirectory (pkgConfigDir);
		}

		[TearDown]
		public void Teardown ()
		{
			if (Directory.Exists (cacheDir))
				Directory.Delete (cacheDir, true);
			if (Directory.Exists (pkgConfigDir))
				Directory.Delete (pkgConfigDir, true);
		}

		[Test]
		public void CreatePcFileCache ()
		{
			PcFileCacheStub.Create (cacheDir);

			// cache dir should exist
			Assert.IsTrue (Directory.Exists (cacheDir), "A1");

			// cache file should not exist
			Assert.IsFalse (File.Exists (pcCacheFilePath), "A2");
		}

		[Test]
		public void CreatePcFileCacheWithExistingEmptyCacheFile ()
		{
			// Create pc cache file
			WritePcCacheFileContent ("");
			PcFileCache cache = PcFileCacheStub.Create (cacheDir);

			// cache should be empty
			string[] pkgConfigDirs = { pkgConfigDir };
			CollectionAssert.IsEmpty (cache.GetPackages (pkgConfigDirs), "A1");
		}

		[Test]
		public void CreatePcFileCacheWithCacheFileContaining1EntryForAnExistingPcFile ()
		{
			// Create pc cache file with an entry and corresponding pc file
			string pkgConfigFileName = "gtk-sharp-2.0.pc";
			string pkgConfigFullFilePath = Path.GetFullPath (Path.Combine (pkgConfigDir, pkgConfigFileName));
			string pcCacheFileContent = @"<PcFileCache>
  <File path=""" + pkgConfigFullFilePath + @""" lastWriteTime=""2013-11-23T21:18:31+01:00"" />
</PcFileCache>
";

			string pkgConfigFileContent = @"prefix=${pcfiledir}/../..
exec_prefix=${prefix}
libdir=${exec_prefix}/lib
gapidir=${prefix}/share/gapi-2.0


Name: Gtk#
Description: Gtk# - GNOME .NET Binding
Version: 2.12.10
Cflags: -I:${gapidir}/pango-api.xml -I:${gapidir}/atk-api.xml -I:${gapidir}/gdk-api.xml -I:${gapidir}/gtk-api.xml
Libs: -r:${libdir}/cli/pango-sharp-2.0/pango-sharp.dll -r:${libdir}/cli/atk-sharp-2.0/atk-sharp.dll -r:${libdir}/cli/gdk-sharp-2.0/gdk-sharp.dll -r:${libdir}/cli/gtk-sharp-2.0/gtk-sharp.dll
Requires: glib-sharp-2.0
";

			AddPkgConfigFile (pkgConfigFileName, pkgConfigFileContent);
			WritePcCacheFileContent (pcCacheFileContent);

			PcFileCache cache = PcFileCacheStub.Create (cacheDir);

			// cache should contain entry of pc file
			Assert.IsNotNull (cache.GetPackageInfo (pkgConfigFullFilePath), "A1");
		}

		[Test]
		public void CreatePcFileCacheWithCacheFileContainingOneOrphanedEntry ()
		{
			string pkgConfigFileName = "gtk-sharp-2.0.pc";
			string pkgConfigFullFilePath = Path.GetFullPath (Path.Combine (pkgConfigDir, pkgConfigFileName));
			string pcCacheFileContent = @"<PcFileCache>
  <File path=""" + pkgConfigFullFilePath + @""" lastWriteTime=""2013-11-23T21:18:31+01:00"" />
</PcFileCache>
";
			WritePcCacheFileContent (pcCacheFileContent);

			PcFileCache cache = PcFileCacheStub.Create (cacheDir);

			// cache should contain orphaned entry
			Assert.IsNotNull (cache.GetPackageInfo (pkgConfigFullFilePath), "A1");
		}

		[Test]
		public void CreatePcFileCacheWithoutCacheFileButWithPcFile ()
		{
			string pkgConfigFileName = "gtk-sharp-2.0.pc";
			string pkgConfigFileContent = @"prefix=${pcfiledir}/../..
exec_prefix=${prefix}
libdir=${exec_prefix}/lib
gapidir=${prefix}/share/gapi-2.0


Name: Gtk#
Description: Gtk# - GNOME .NET Binding
Version: 2.12.10
Cflags: -I:${gapidir}/pango-api.xml -I:${gapidir}/atk-api.xml -I:${gapidir}/gdk-api.xml -I:${gapidir}/gtk-api.xml
Libs: -r:${libdir}/cli/pango-sharp-2.0/pango-sharp.dll -r:${libdir}/cli/atk-sharp-2.0/atk-sharp.dll -r:${libdir}/cli/gdk-sharp-2.0/gdk-sharp.dll -r:${libdir}/cli/gtk-sharp-2.0/gtk-sharp.dll
Requires: glib-sharp-2.0
";
			AddPkgConfigFile (pkgConfigFileName, pkgConfigFileContent);

			PcFileCache cache = PcFileCacheStub.Create (cacheDir);

			// cache file should exist
			Assert.IsFalse (File.Exists (pcCacheFilePath), "A1");

			// cache should be empty
			string[] pkgConfigDirs = { pkgConfigDir };
			CollectionAssert.IsEmpty (cache.GetPackages (pkgConfigDirs), "A2");
		}

		[Test]
		public void GetPackagesOrderedByFolder ()
		{
			string pkgConfigDir1 = "testpkgconfigdir1";
			string pkgConfigDir2 = "testpkgconfigdir2";
			Directory.CreateDirectory (pkgConfigDir1);
			Directory.CreateDirectory (pkgConfigDir2);

			string pkgConfigFile11NameAttr = "gtk-sharp-2.0";
			string pkgConfigFile11FullPath = Path.GetFullPath (Path.Combine (pkgConfigDir1, "gtk-sharp-2.0.pc"));

			string pkgConfigFile21NameAttr = "art-sharp-2.0";
			string pkgConfigFile21FullPath = Path.GetFullPath (Path.Combine (pkgConfigDir2, "art-sharp-2.0.pc"));

			string pkgConfigFile12NameAttr = "cecil";
			string pkgConfigFile12FullPath = Path.GetFullPath (Path.Combine (pkgConfigDir1, "cecil.pc"));

			string pcCacheFileContent = @"<PcFileCache>
  <File path=""" + pkgConfigFile11FullPath + @""" lastWriteTime=""2013-11-23T21:18:31+01:00"" name=""" + pkgConfigFile11NameAttr + @""" />
  <File path=""" + pkgConfigFile21FullPath + @""" lastWriteTime=""2011-07-12T12:04:53+02:00"" name=""" + pkgConfigFile21NameAttr + @""" />
  <File path=""" + pkgConfigFile12FullPath + @""" lastWriteTime=""2012-07-24T22:28:30+02:00"" name=""" + pkgConfigFile12NameAttr + @""" />
</PcFileCache>
";

			WritePcCacheFileContent (pcCacheFileContent);

			PcFileCache cache = PcFileCacheStub.Create (cacheDir);
			string[] pkgConfigDirs = { pkgConfigDir1, pkgConfigDir2 };
			IEnumerable<PackageInfo> packages = cache.GetPackages (pkgConfigDirs);

			PackageInfo[] packageArray = new PackageInfo [3];
			int i = 0;
			foreach (PackageInfo package in packages)
				packageArray [i++] = package;

			Assert.AreEqual (pkgConfigFile11NameAttr, packageArray [0].Name, "A1");
			Assert.AreEqual (pkgConfigFile12NameAttr, packageArray [1].Name, "A2");
			Assert.AreEqual (pkgConfigFile21NameAttr, packageArray [2].Name, "A3");

			Directory.Delete (pkgConfigDir1, true);
			Directory.Delete (pkgConfigDir2, true);
		}

		[Test]
		public void UpdatePcFileCacheWithOrphanedEntry ()
		{
			string pkgConfigFileNameAttr = "gtk-sharp-2.0";
			string pkgConfigFileName = "gtk-sharp-2.0.pc";
			string pkgConfigFullFilePath = Path.GetFullPath (Path.Combine (pkgConfigDir, pkgConfigFileName));
			string pcCacheFileContent = @"<PcFileCache>
  <File path=""" + pkgConfigFullFilePath + @""" lastWriteTime=""2013-11-23T21:18:31+01:00"" name=""" + pkgConfigFileNameAttr + @""" />
</PcFileCache>
";

			WritePcCacheFileContent (pcCacheFileContent);

			PcFileCache cache = PcFileCacheStub.Create (cacheDir);

			// precondition
			string[] pkgConfigDirs = { pkgConfigDir };
			Assert.IsNotNull (cache.GetPackageInfoByName (pkgConfigFileNameAttr, pkgConfigDirs), "A1");

			cache.Update (pkgConfigDirs);
			Assert.IsNull (cache.GetPackageInfoByName (pkgConfigFileNameAttr, pkgConfigDirs), "A2");
		}

		static void WritePcCacheFileContent (string content)
		{
			File.WriteAllText (pcCacheFilePath, content);
		}

		static void AddPkgConfigFile (string fileName, string content)
		{
			AddPkgConfigFile (fileName, content, pkgConfigDir);
		}

		static void AddPkgConfigFile (string fileName, string content, string pkgConfigDir)
		{
			string path = Path.Combine (pkgConfigDir, fileName);
			File.WriteAllText (path, content);
		}

		class PcFileCacheContextStub : IPcFileCacheContext
		{
			public void StoreCustomData (PcFile pcfile, PackageInfo pkg)
			{
			}

			public bool IsCustomDataComplete (string pcfile, PackageInfo pkg)
			{
				return false;
			}

			public void ReportError (string message, Exception ex)
			{
			}
		}

		class PcFileCacheStub : PcFileCache
		{
			static string initCacheDirectory;
			readonly string cacheDirectory;

			PcFileCacheStub (string cacheDirectory) : base (new PcFileCacheContextStub ())
			{
				if (cacheDirectory == null)
					throw new ArgumentNullException ("cacheDirectory");
				this.cacheDirectory = cacheDirectory;
			}

			protected override string CacheDirectory {
				get { return initCacheDirectory == null ? cacheDirectory : initCacheDirectory; }
			}

			public static PcFileCache Create (string cacheDirectory)
			{
				initCacheDirectory = cacheDirectory;
				PcFileCache cache = new PcFileCacheStub (cacheDirectory);
				initCacheDirectory = null;
				return cache;
			}
		}
	}
}
