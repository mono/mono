// FileSystemInfoTest.cs - NUnit Test Cases for System.IO.FileSystemInfo class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) 2003 Ville Palo
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Threading;


namespace MonoTests.System.IO
{
	[TestFixture]
        public class FileSystemInfoTest : Assertion
	{
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		[SetUp]
		protected void SetUp() 
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("EN-us");
		}

		[TearDown]
		protected void TearDown() {
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
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
		public void CreationTimeFile ()
		{
			string path = TempFolder + "/FSIT.CreationTime.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileSystemInfo info = new FileInfo (path);
				info.CreationTime = new DateTime (1999, 12, 31, 11, 59, 59);

				DateTime time = info.CreationTime;				
				AssertEquals ("test#01", 1999, time.Year);
				AssertEquals ("test#02", 12, time.Month);
				AssertEquals ("test#03", 31, time.Day);
				AssertEquals ("test#04", 11, time.Hour);
				AssertEquals ("test#05", 59, time.Minute);
				AssertEquals ("test#06", 59, time.Second);
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);	
				AssertEquals ("test#07", 1999, time.Year);
				AssertEquals ("test#08", 12, time.Month);
				AssertEquals ("test#09", 31, time.Day);
				AssertEquals ("test#10", 11, time.Hour);
				AssertEquals ("test#11", 59, time.Minute);
				AssertEquals ("test#12", 59, time.Second);
				
				info.CreationTimeUtc = new DateTime (1999, 12, 31, 11, 59, 59);

				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.CreationTime);				
				AssertEquals ("test#13", 1999, time.Year);
				AssertEquals ("test#14", 12, time.Month);
				AssertEquals ("test#15", 31, time.Day);
				AssertEquals ("test#16", 11, time.Hour);
				AssertEquals ("test#17", 59, time.Minute);
				AssertEquals ("test#18", 59, time.Second);

				time = info.CreationTimeUtc;	
				AssertEquals ("test#19", 1999, time.Year);
				AssertEquals ("test#20", 12, time.Month);
				AssertEquals ("test#21", 31, time.Day);
				AssertEquals ("test#22", 11, time.Hour);
				AssertEquals ("test#23", 59, time.Minute);
				AssertEquals ("test#24", 59, time.Second);

			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		public void CreationTimeDirectory ()
		{
			string path = TempFolder + "/FSIT.CreationTimeDirectory.Test";
			DeleteDir (path);
			
			try {				
				FileSystemInfo info = Directory.CreateDirectory (path);
				info.CreationTime = new DateTime (1999, 12, 31, 11, 59, 59);
				DateTime time = info.CreationTime;	
				
				AssertEquals ("test#01", 1999, time.Year);
				AssertEquals ("test#02", 12, time.Month);
				AssertEquals ("test#03", 31, time.Day);
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);	
				AssertEquals ("test#07", 1999, time.Year);
				AssertEquals ("test#08", 12, time.Month);
				AssertEquals ("test#09", 31, time.Day);
				AssertEquals ("test#10", 11, time.Hour);
				
				info.CreationTimeUtc = new DateTime (1999, 12, 31, 11, 59, 59);
				
				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.CreationTime);				
				AssertEquals ("test#13", 1999, time.Year);
				AssertEquals ("test#14", 12, time.Month);
				AssertEquals ("test#15", 31, time.Day);
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.CreationTimeUtc);	
				AssertEquals ("test#19", 1999, time.Year);
				AssertEquals ("test#20", 12, time.Month);
				AssertEquals ("test#21", 31, time.Day);
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void CreationTimeNoFileOrDirectory ()
		{
			string path = TempFolder + "/FSIT.CreationTimeNoFile.Test";
			DeleteFile (path);
			DeleteDir (path);
			
			try {
				FileSystemInfo info = new FileInfo (path);
				
				DateTime time = info.CreationTime;
				AssertEquals ("test#01", 1601, time.Year);
				AssertEquals ("test#02", 1, time.Month);
				AssertEquals ("test#03", 1, time.Day);
				AssertEquals ("test#04", 2, time.Hour);
				AssertEquals ("test#05", 0, time.Minute);
				AssertEquals ("test#06", 0, time.Second);
				AssertEquals ("test#07", 0, time.Millisecond);
				
				info = new DirectoryInfo (path);
				
				time = info.CreationTime;
				AssertEquals ("test#08", 1601, time.Year);
				AssertEquals ("test#09", 1, time.Month);
				AssertEquals ("test#10", 1, time.Day);
				AssertEquals ("test#11", 2, time.Hour);
				AssertEquals ("test#12", 0, time.Minute);
				AssertEquals ("test#13", 0, time.Second);
				AssertEquals ("test#14", 0, time.Millisecond);
			} finally {
				DeleteFile (path);
				DeleteDir (path);
			}
		}
		
		[Test]
		public void Extenssion ()
		{
			string path = TempFolder + "/FSIT.Extenssion.Test";
			DeleteFile (path);
			
			try {
				FileSystemInfo info = new FileInfo (path);
				AssertEquals ("test#01", ".Test", info.Extension);
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void DefaultLastAccessTime ()
		{
			string path = TempFolder + "/FSIT.DefaultLastAccessTime.Test";
			DeleteFile (path);
			
			try {
				
				FileSystemInfo info = new FileInfo (path);
				DateTime time = info.LastAccessTime;

				AssertEquals ("test#01", 1601, time.Year);
				AssertEquals ("test#02", 1, time.Month);
				AssertEquals ("test#03", 1, time.Day);
				AssertEquals ("test#04", 2, time.Hour);
				AssertEquals ("test#05", 0, time.Minute);
				AssertEquals ("test#06", 0, time.Second);
				AssertEquals ("test#07", 0, time.Millisecond);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		public void LastAccessTime ()
		{
			string path = TempFolder + "/FSIT.LastAccessTime.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileSystemInfo info = new FileInfo (path);
				DateTime time;
				info = new FileInfo (path);
				
				info.LastAccessTime = new DateTime (2000, 1, 1, 1, 1, 1);
				time = info.LastAccessTime;
				AssertEquals ("test#01", 2000, time.Year);
				AssertEquals ("test#02", 1, time.Month);
				AssertEquals ("test#03", 1, time.Day);
				AssertEquals ("test#04", 1, time.Hour);
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.LastAccessTimeUtc);
				AssertEquals ("test#05", 2000, time.Year);
				AssertEquals ("test#06", 1, time.Month);
				AssertEquals ("test#07", 1, time.Day);
				AssertEquals ("test#08", 1, time.Hour);
				
				info.LastAccessTimeUtc = new DateTime (2000, 1, 1, 1, 1, 1);
				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.LastAccessTime);
				AssertEquals ("test#09", 2000, time.Year);
				AssertEquals ("test#10", 1, time.Month);
				AssertEquals ("test#11", 1, time.Day);
				AssertEquals ("test#12", 1, time.Hour);

				time = info.LastAccessTimeUtc;
				AssertEquals ("test#13", 2000, time.Year);
				AssertEquals ("test#14", 1, time.Month);
				AssertEquals ("test#15", 1, time.Day);
				AssertEquals ("test#16", 1, time.Hour);				
				
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void DefaultLastWriteTime ()
		{
			string path = TempFolder + "/FSIT.DefaultLastWriteTime.Test";
			DeleteDir (path);

			try {

				FileSystemInfo info = new DirectoryInfo (path);
				DateTime time = info.LastAccessTime;
				
				AssertEquals ("test#01", 1601, time.Year);
				AssertEquals ("test#02", 1, time.Month);
				AssertEquals ("test#03", 1, time.Day);
				AssertEquals ("test#04", 2, time.Hour);
				AssertEquals ("test#05", 0, time.Minute);
				AssertEquals ("test#06", 0, time.Second);
				AssertEquals ("test#07", 0, time.Millisecond);
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void LastWriteTime ()
		{
			string path = TempFolder + "/FSIT.LastWriteTime.Test";
			DeleteDir (path);
			
			try {
				FileSystemInfo info = Directory.CreateDirectory (path);
				
				info.LastWriteTime = new DateTime (2000, 1, 1, 1, 1, 1);
				DateTime time = info.LastWriteTime;
				
				AssertEquals ("test#01", 2000, time.Year);
				AssertEquals ("test#02", 1, time.Month);
				AssertEquals ("test#03", 1, time.Day);
				AssertEquals ("test#04", 1, time.Hour);
				
				time = TimeZone.CurrentTimeZone.ToLocalTime (info.LastWriteTimeUtc);
				AssertEquals ("test#05", 2000, time.Year);
				AssertEquals ("test#06", 1, time.Month);
				AssertEquals ("test#07", 1, time.Day);
				AssertEquals ("test#08", 1, time.Hour);
				
				info.LastWriteTimeUtc = new DateTime (2000, 1, 1, 1, 1, 1);
				time = TimeZone.CurrentTimeZone.ToUniversalTime (info.LastWriteTime);
				AssertEquals ("test#09", 2000, time.Year);
				AssertEquals ("test#10", 1, time.Month);
				AssertEquals ("test#11", 1, time.Day);
				AssertEquals ("test#12", 1, time.Hour);

				time = info.LastWriteTimeUtc;
				AssertEquals ("test#13", 2000, time.Year);
				AssertEquals ("test#14", 1, time.Month);
				AssertEquals ("test#15", 1, time.Day);
				AssertEquals ("test#16", 1, time.Hour);

			} finally {
				DeleteDir (path);
			}
		}
	}
}
