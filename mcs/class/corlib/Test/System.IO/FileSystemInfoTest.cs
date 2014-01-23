// FileSystemInfoTest.cs - NUnit Test Cases for System.IO.FileSystemInfo class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) 2003 Ville Palo
// 

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileSystemInfoTest
	{
		CultureInfo old_culture;
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
		static readonly char DSC = Path.DirectorySeparatorChar;

		[SetUp]
		public void SetUp()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
			old_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Thread.CurrentThread.CurrentCulture = old_culture;
		}
		
		bool Windows {
			get {
				return Path.DirectorySeparatorChar == '\\';
			}
		}

		bool Unix {
			get {
				return Path.DirectorySeparatorChar == '/';
			}
		}

		bool Mac {
			get {
				return Path.DirectorySeparatorChar == ':';
			}
		}

		private void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}

		private void DeleteDir (string path)
		{
			if (Directory.Exists (path))
				Directory.Delete (path, true);
		}

		[Test]
		[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
		public void CreationTimeFile ()
		{
			if (Unix)
				Assert.Ignore ("Unix doesn't support CreationTimes");

			string path = TempFolder + DSC + "FSIT.CreationTime.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileSystemInfo info = new FileInfo (path);
				info.CreationTime = new DateTime (1999, 12, 31, 11, 59, 59);

				DateTime time = info.CreationTime;
				Assert.AreEqual (1999, time.Year, "#A1");
				Assert.AreEqual (12, time.Month, "#A2");
				Assert.AreEqual (31, time.Day, "#A3");
				Assert.AreEqual (11, time.Hour, "#A4");
				Assert.AreEqual (59, time.Minute, "#A5");
				Assert.AreEqual (59, time.Second, "#A6");
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);
				Assert.AreEqual (1999, time.Year, "#B1");
				Assert.AreEqual (12, time.Month, "#B2");
				Assert.AreEqual (31, time.Day, "#B3");
				Assert.AreEqual (11, time.Hour, "#B4");
				Assert.AreEqual (59, time.Minute, "#B5");
				Assert.AreEqual (59, time.Second, "#B6");
				
				info.CreationTimeUtc = new DateTime (1999, 12, 31, 11, 59, 59);

				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.CreationTime);
				Assert.AreEqual (1999, time.Year, "#C1");
				Assert.AreEqual (12, time.Month, "#C2");
				Assert.AreEqual (31, time.Day, "#C3");
				Assert.AreEqual (11, time.Hour, "#C4");
				Assert.AreEqual (59, time.Minute, "#C5");
				Assert.AreEqual (59, time.Second, "#C6");

				time = info.CreationTimeUtc;
				Assert.AreEqual (1999, time.Year, "#D1");
				Assert.AreEqual (12, time.Month, "#D2");
				Assert.AreEqual (31, time.Day, "#D3");
				Assert.AreEqual (11, time.Hour, "#D4");
				Assert.AreEqual (59, time.Minute, "#D5");
				Assert.AreEqual (59, time.Second, "#D6");
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
		public void CreationTimeDirectory ()
		{
			if (Unix)
				Assert.Ignore ("Unix doesn't support CreationTimes");

			string path = TempFolder + DSC + "FSIT.CreationTimeDirectory.Test";
			DeleteDir (path);

			try {
				FileSystemInfo info = Directory.CreateDirectory (path);
				info.CreationTime = new DateTime (1999, 12, 31, 11, 59, 59);
				DateTime time = info.CreationTime;

				Assert.AreEqual (1999, time.Year, "#A1");
				Assert.AreEqual (12, time.Month, "#A2");
				Assert.AreEqual (31, time.Day, "#A3");
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);
				Assert.AreEqual (1999, time.Year, "#B1");
				Assert.AreEqual (12, time.Month, "#B2");
				Assert.AreEqual (31, time.Day, "#B3");
				Assert.AreEqual (11, time.Hour, "#B4");
				
				info.CreationTimeUtc = new DateTime (1999, 12, 31, 11, 59, 59);
				
				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.CreationTime);
				Assert.AreEqual (1999, time.Year, "#C1");
				Assert.AreEqual (12, time.Month, "#C2");
				Assert.AreEqual (31, time.Day, "#C3");
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);
				Assert.AreEqual (1999, time.Year, "#D1");
				Assert.AreEqual (12, time.Month, "#D2");
				Assert.AreEqual (31, time.Day, "#D3");
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
		public void CreationTimeNoFileOrDirectory ()
		{
			string path = TempFolder + DSC + "FSIT.CreationTimeNoFile.Test";
			DeleteFile (path);
			DeleteDir (path);
			
			try {
				FileSystemInfo info = new FileInfo (path);
				
				DateTime time = TimeZone.CurrentTimeZone.ToUniversalTime(info.CreationTime);
				Assert.AreEqual (1601, time.Year, "#A1");
				Assert.AreEqual (1, time.Month, "#A2");
				Assert.AreEqual (1, time.Day, "#A3");
				Assert.AreEqual (0, time.Hour, "#A4");
				Assert.AreEqual (0, time.Minute, "#A5");
				Assert.AreEqual (0, time.Second, "#A6");
				Assert.AreEqual (0, time.Millisecond, "#A7");
				
				info = new DirectoryInfo (path);
				
				time = TimeZone.CurrentTimeZone.ToUniversalTime(info.CreationTime);
				Assert.AreEqual (1601, time.Year, "#B1");
				Assert.AreEqual (1, time.Month, "#B2");
				Assert.AreEqual (1, time.Day, "#B3");
				Assert.AreEqual (0, time.Hour, "#B4");
				Assert.AreEqual (0, time.Minute, "#B5");
				Assert.AreEqual (0, time.Second, "#B6");
				Assert.AreEqual (0, time.Millisecond, "#B7");
			} finally {
				DeleteFile (path);
				DeleteDir (path);
			}
		}
		
		[Test]
		public void Extenssion ()
		{
			string path = TempFolder + DSC + "FSIT.Extenssion.Test";
			DeleteFile (path);
			
			try {
				FileSystemInfo info = new FileInfo (path);
				Assert.AreEqual (".Test", info.Extension);
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
		public void DefaultLastAccessTime ()
		{
			string path = TempFolder + DSC + "FSIT.DefaultLastAccessTime.Test";
			DeleteFile (path);
			
			try {
				FileSystemInfo info = new FileInfo (path);
				DateTime time = TimeZone.CurrentTimeZone.ToUniversalTime(info.LastAccessTime);

				Assert.AreEqual (1601, time.Year, "#1");
				Assert.AreEqual (1, time.Month, "#2");
				Assert.AreEqual (1, time.Day, "#3");
				Assert.AreEqual (0, time.Hour, "#4");
				Assert.AreEqual (0, time.Minute, "#5");
				Assert.AreEqual (0, time.Second, "#6");
				Assert.AreEqual (0, time.Millisecond, "#7");
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
		public void LastAccessTime ()
		{
			string path = TempFolder + DSC + "FSIT.LastAccessTime.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileSystemInfo info = new FileInfo (path);
				DateTime time;
				info = new FileInfo (path);
				
				info.LastAccessTime = new DateTime (2000, 1, 1, 1, 1, 1);
				time = info.LastAccessTime;
				Assert.AreEqual (2000, time.Year, "#A1");
				Assert.AreEqual (1, time.Month, "#A2");
				Assert.AreEqual (1, time.Day, "#A3");
				Assert.AreEqual (1, time.Hour, "#A4");
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.LastAccessTimeUtc);
				Assert.AreEqual (2000, time.Year, "#B1");
				Assert.AreEqual (1, time.Month, "#B2");
				Assert.AreEqual (1, time.Day, "#B3");
				Assert.AreEqual (1, time.Hour, "#B4");
				
				info.LastAccessTimeUtc = new DateTime (2000, 1, 1, 1, 1, 1);
				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.LastAccessTime);
				Assert.AreEqual (2000, time.Year, "#C1");
				Assert.AreEqual (1, time.Month, "#C2");
				Assert.AreEqual (1, time.Day, "#C3");
				Assert.AreEqual (1, time.Hour, "#C4");

				time = info.LastAccessTimeUtc;
				Assert.AreEqual (2000, time.Year, "#D1");
				Assert.AreEqual (1, time.Month, "#D2");
				Assert.AreEqual (1, time.Day, "#D3");
				Assert.AreEqual (1, time.Hour, "#D4");
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
		public void DefaultLastWriteTime ()
		{
			string path = TempFolder + DSC + "FSIT.DefaultLastWriteTime.Test";
			DeleteDir (path);

			try {

				FileSystemInfo info = new DirectoryInfo (path);
				DateTime time = TimeZone.CurrentTimeZone.ToUniversalTime(info.LastWriteTime);

				Assert.AreEqual (1601, time.Year, "#1");
				Assert.AreEqual (1, time.Month, "#2");
				Assert.AreEqual (1, time.Day, "#3");
				Assert.AreEqual (0, time.Hour, "#4");
				Assert.AreEqual (0, time.Minute, "#5");
				Assert.AreEqual (0, time.Second, "#6");
				Assert.AreEqual (0, time.Millisecond, "#7");
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void LastWriteTime ()
		{
			string path = TempFolder + DSC + "FSIT.LastWriteTime.Test";
			DeleteDir (path);
			
			try {
				FileSystemInfo info = Directory.CreateDirectory (path);
				
				info.LastWriteTime = new DateTime (2000, 1, 1, 1, 1, 1);
				DateTime time = info.LastWriteTime;

				Assert.AreEqual (2000, time.Year, "#A1");
				Assert.AreEqual (1, time.Month, "#A2");
				Assert.AreEqual (1, time.Day, "#A3");
				Assert.AreEqual (1, time.Hour, "#A4");
				
				time = info.LastWriteTimeUtc.ToLocalTime ();
				Assert.AreEqual (2000, time.Year, "#B1");
				Assert.AreEqual (1, time.Month, "#B2");
				Assert.AreEqual (1, time.Day, "#B3");
				Assert.AreEqual (1, time.Hour, "#B4");

				info.LastWriteTimeUtc = new DateTime (2000, 1, 1, 1, 1, 1);
				time = info.LastWriteTimeUtc;
				Assert.AreEqual (2000, time.Year, "#C1");
				Assert.AreEqual (1, time.Month, "#C2");
				Assert.AreEqual (1, time.Day, "#C3");
				Assert.AreEqual (1, time.Hour, "#C4");

				time = info.LastWriteTime.ToUniversalTime ();
				Assert.AreEqual (2000, time.Year, "#D1");
				Assert.AreEqual (1, time.Month, "#D2");
				Assert.AreEqual (1, time.Day, "#D3");
				Assert.AreEqual (1, time.Hour, "#D4");
			} finally {
				DeleteDir (path);
			}
		}
#if !MOBILE
		[Test]
		public void Serialization ()
		{
			string path = TempFolder + DSC + "FSIT.Serialization.Test";
			DeleteDir (path);

			try {
				FileSystemInfo info = Directory.CreateDirectory (path);

				SerializationInfo si = new SerializationInfo (
					typeof (FileSystemInfo), new FormatterConverter ());
				info.GetObjectData (si, new StreamingContext ());

				Assert.AreEqual (2, si.MemberCount, "#1");
				Assert.AreEqual ("FSIT.Serialization.Test", si.GetString ("OriginalPath"), "#2");
				Assert.AreEqual (path, si.GetString ("FullPath"), "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void Deserialization ()
		{
			string path = TempFolder + DSC + "FSIT.Deserialization.Test";
			DeleteDir (path);

			try {
				FileSystemInfo info = Directory.CreateDirectory (path);

				MemoryStream ms = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, info);
				ms.Position = 0;

				FileSystemInfo clone = (FileSystemInfo) bf.Deserialize (ms);
				Assert.AreEqual (clone.FullName, info.FullName);
			} finally {
				DeleteDir (path);
			}
		}
#endif
	}
}
