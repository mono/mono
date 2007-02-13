//
// FileTest.cs: Test cases for System.IO.File
//
// Author: 
//     Duncan Mak (duncan@ximian.com)
//     Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//
// TODO: Find out why ArgumentOutOfRangeExceptions does not manage to close streams properly
//
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileTest : Assertion
	{
		static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		[SetUp]
		public void SetUp ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
		
                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("EN-us");
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
		}

		[Test]
		public void TestExists ()
		{
			int i = 0;
			FileStream s = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			try {
				Assert ("null filename should not exist", !File.Exists (null));
				i++;
				Assert ("empty filename should not exist", !File.Exists (""));
				i++;
				Assert ("whitespace filename should not exist", !File.Exists ("  \t\t  \t \n\t\n \n"));
				i++;				
				DeleteFile (path);
				s = File.Create (path);
				s.Close ();
				Assert ("File " + path + " should exists", File.Exists (path));
				i++;
				Assert ("File resources" + Path.DirectorySeparatorChar + "doesnotexist should not exist", !File.Exists (TempFolder + Path.DirectorySeparatorChar + "doesnotexist"));
			} catch (Exception e) {
				Fail ("Unexpected exception at i = " + i + ". e=" + e);
			} finally {
				if (s != null)
					s.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void Exists_InvalidFileName () 
		{
			Assert ("><|", !File.Exists ("><|"));
			Assert ("?*", !File.Exists ("?*"));
		}

		[Test]
		public void Exists_InvalidDirectory () 
		{
			Assert ("InvalidDirectory", !File.Exists (Path.Combine ("does not exist", "file.txt")));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void CtorArgumentNullException1 ()
		{	
			FileStream stream = File.Create (null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorArgumentException1 ()
		{	
			FileStream stream = File.Create ("");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorArgumentException2 ()
		{	
			FileStream stream = File.Create (" ");
		}

		[Test]
		[ExpectedException(typeof (DirectoryNotFoundException))]
		public void CtorDirectoryNotFoundException ()
		{	
			FileStream stream = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "directory_does_not_exist" + Path.DirectorySeparatorChar + "foo";
			
			try {
				stream = File.Create (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void TestCreate ()
		{
			FileStream stream = null;
			string path = "";
			/* positive test: create resources/foo */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "foo";
				stream = File.Create (path);
				Assert ("File should exist", File.Exists (path));
				stream.Close ();
			} catch (Exception e) {
				Fail ("File.Create(resources/foo) unexpected exception caught: e=" + e.ToString());
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
			
			path = "";
			stream = null;

			/* positive test: repeat test above again to test for overwriting file */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "foo";
				stream = File.Create (path);
				Assert ("File should exist", File.Exists (path));
				stream.Close ();
			} catch (Exception e) {
				Fail ("File.Create(resources/foo) unexpected exception caught: e=" + e.ToString()); 
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyArgumentNullException1 ()
		{
			File.Copy (null, "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyArgumentNullException2 ()
		{
			File.Copy ("a", null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException1 ()
		{
			File.Copy ("", "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException2 ()
		{
			File.Copy ("a", "");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException3 ()
		{
			File.Copy (" ", "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException4 ()
		{
			File.Copy ("a", " ");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyArgumentException5 ()
		{
			File.Copy ("a*", "b");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyArgumentException6 ()
		{
			File.Copy ("a?", "b");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyArgumentException7 ()
		{
			File.Copy ("a", "b*");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyArgumentException8 ()
		{
			File.Copy ("a", "b?");
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CopyIOException2 ()
		{
			string dirpath = TempFolder + Path.DirectorySeparatorChar + "dir";
			string filepath = TempFolder + Path.DirectorySeparatorChar + "file.txt";

			try {
				Directory.CreateDirectory (dirpath);
				File.WriteAllText (filepath, "xxx");

				File.Copy (filepath, dirpath);
			}
			finally {
				DeleteFile (filepath);
				DeleteDirectory (dirpath);
			}
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CopyIOException3 ()
		{
			string filepath1 = TempFolder + Path.DirectorySeparatorChar + "file1.txt";
			string filepath2 = TempFolder + Path.DirectorySeparatorChar + "file2.txt";

			try {
				File.WriteAllText (filepath1, "xxx");
				File.WriteAllText (filepath2, "yyy");

				File.Copy (filepath1, filepath2);
			}
			finally {
				DeleteFile (filepath1);
				DeleteDirectory (filepath2);
			}
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void CopyFileNotFoundException ()
		{
			File.Copy ("doesnotexist", "b");
		}

		[ExpectedException(typeof(IOException))]
		public void CopyIOException ()
		{
			DeleteFile (TempFolder + Path.DirectorySeparatorChar + "bar");
			DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");
			try {
				File.Create (TempFolder + Path.DirectorySeparatorChar + "AFile.txt").Close ();
				File.Copy (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", TempFolder + Path.DirectorySeparatorChar + "bar");
				File.Copy (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", TempFolder + Path.DirectorySeparatorChar + "bar");
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "bar");
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");				
			}
		}

		[Test]
		public void TestCopy ()
		{
			string path1 = TempFolder + Path.DirectorySeparatorChar + "bar";
			string path2 = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			/* positive test: copy resources/AFile.txt to resources/bar */
			try {
				try {
					DeleteFile (path1);
					DeleteFile (path2);

					File.Create (path2).Close ();
					File.Copy (path2, path1);
					Assert ("File AFile.txt should still exist", File.Exists (path2));
					Assert ("File bar should exist after File.Copy", File.Exists (path1));
				} catch (Exception e) {
					Fail ("#1 File.Copy('resources/AFile.txt', 'resources/bar') unexpected exception caught: e=" + e.ToString());
				}

				/* positive test: copy resources/AFile.txt to resources/bar, overwrite */
				try {
					Assert ("File bar should exist before File.Copy", File.Exists (path1));
					File.Copy (path2, path1, true);
					Assert ("File AFile.txt should still exist", File.Exists (path2));
					Assert ("File bar should exist after File.Copy", File.Exists (path1));
				} catch (Exception e) {
					Fail ("File.Copy('resources/AFile.txt', 'resources/bar', true) unexpected exception caught: e=" + e.ToString());
				}
			}finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}			
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeleteArgumentNullException ()
		{
			File.Delete (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DeleteArgumentException1 ()
		{
			File.Delete ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DeleteArgumentException2 ()
		{
			File.Delete (" ");
		}

		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void DeleteDirectoryNotFoundException ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "directory_does_not_exist" + Path.DirectorySeparatorChar + "foo";
			if (Directory.Exists (path))
				Directory.Delete (path, true);
			File.Delete (path);			
		}


		[Test]
		public void TestDelete ()
		{
			string foopath = TempFolder + Path.DirectorySeparatorChar + "foo";
			DeleteFile (foopath);
			try {
				File.Create (foopath).Close ();

                        	try {
                                	File.Delete (foopath);
	                       	} catch (Exception e) {
        	                        Fail ("Unable to delete " + foopath + " e=" + e.ToString());
                        	} 
				Assert ("File " + foopath + " should not exist after File.Delete", !File.Exists (foopath));
			} finally {
			        DeleteFile (foopath);
			}
		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("NotWorking")]
		public void DeleteOpenStreamException ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "DeleteOpenStreamException";
			DeleteFile (path);			
			FileStream stream = null;
			try {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				File.Delete (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void MoveException1 ()
		{
	                File.Move (null, "b");
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void MoveException2 ()
		{
			File.Move ("a", null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException3 ()
		{
                	File.Move ("", "b");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException4 ()
		{
                	File.Move ("a", "");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException5 ()
		{
                        File.Move (" ", "b");			
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException6 ()
		{
                	File.Move ("a", " ");
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void MoveException7 ()
		{
                       	DeleteFile (TempFolder + Path.DirectorySeparatorChar + "doesnotexist");			
                        File.Move (TempFolder + Path.DirectorySeparatorChar + "doesnotexist", "b");
		}

		[Test]
		[ExpectedException(typeof (DirectoryNotFoundException))]
		public void MoveException8 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "foo";
                        DeleteFile (path);
			try {
				File.Create (TempFolder + Path.DirectorySeparatorChar + "AFile.txt").Close ();
                        	File.Copy(TempFolder + Path.DirectorySeparatorChar + "AFile.txt", path, true);
                        	DeleteFile (TempFolder + Path.DirectorySeparatorChar + "doesnotexist" + Path.DirectorySeparatorChar + "b");
                        	File.Move (TempFolder + Path.DirectorySeparatorChar + "foo", TempFolder + Path.DirectorySeparatorChar + "doesnotexist" + Path.DirectorySeparatorChar + "b");
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (IOException))]
		public void MoveException9 ()
		{
			File.Create (TempFolder + Path.DirectorySeparatorChar + "foo").Close ();
			try {
                		File.Move (TempFolder + Path.DirectorySeparatorChar + "foo", TempFolder);		
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "foo");
			}
		}

		[Test]
		public void TestMove ()
		{
			string bar = TempFolder + Path.DirectorySeparatorChar + "bar";
			string baz = TempFolder + Path.DirectorySeparatorChar + "baz";
			if (!File.Exists (bar)) {
				FileStream f = File.Create(bar);
				f.Close();
			}
			
			Assert ("File " + TempFolder + Path.DirectorySeparatorChar + "bar should exist", File.Exists (bar));
			File.Move (bar, baz);
			Assert ("File " + TempFolder + Path.DirectorySeparatorChar + "bar should not exist", !File.Exists (bar));
			Assert ("File " + TempFolder + Path.DirectorySeparatorChar + "baz should exist", File.Exists (baz));

			// Test moving of directories
			string dir = Path.Combine (TempFolder, "dir");
			string dir2 = Path.Combine (TempFolder, "dir2");
			string dir_foo = Path.Combine (dir, "foo");
			string dir2_foo = Path.Combine (dir2, "foo");

			if (Directory.Exists (dir))
				Directory.Delete (dir, true);
						
			Directory.CreateDirectory (dir);
			Directory.CreateDirectory (dir2);
			File.Create (dir_foo).Close ();
			File.Move (dir_foo, dir2_foo);
			Assert (File.Exists (dir2_foo));
			
			Directory.Delete (dir, true);
			Directory.Delete (dir2, true);
			DeleteFile (dir_foo);
			DeleteFile (dir2_foo);
		}

		[Test]
		public void TestOpen ()
		{
			string path = "";
			FileStream stream = null;
                        try {
                        	path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
				if (!File.Exists (path))
					stream = File.Create (path);
                        	stream.Close ();
                                stream = File.Open (path, FileMode.Open);
				stream.Close ();
                        } catch (Exception e) {
                                Fail ("Unable to open " + TempFolder + Path.DirectorySeparatorChar + "AFile.txt: e=" + e.ToString());
                        } finally {
                        	if (stream != null)
                        		stream.Close ();
                        	DeleteFile (path);
                        }
			
			path = "";
			stream = null;
                        /* Exception tests */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "filedoesnotexist";
				stream = File.Open (path, FileMode.Open);
				Fail ("File 'filedoesnotexist' should not exist");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("Unexpect exception caught: e=" + e.ToString());
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

                [Test]
                public void Open () 
                {
                	string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
			FileStream stream = null;
                	try {
			
				stream = File.Open (path, FileMode.Open);
                	
                		Assertion.AssertEquals ("test#01", true, stream.CanRead);
                		Assertion.AssertEquals ("test#02", true, stream.CanSeek);
                		Assertion.AssertEquals ("test#03", true, stream.CanWrite);
                		stream.Close ();
                	
                		stream = File.Open (path, FileMode.Open, FileAccess.Write);
                		Assertion.AssertEquals ("test#04", false, stream.CanRead);
                		Assertion.AssertEquals ("test#05", true, stream.CanSeek);
                		Assertion.AssertEquals ("test#06", true, stream.CanWrite);
                		stream.Close ();
		                	
                		stream = File.Open (path, FileMode.Open, FileAccess.Read);
                		Assertion.AssertEquals ("test#04", true, stream.CanRead);
                		Assertion.AssertEquals ("test#05", true, stream.CanSeek);
                		Assertion.AssertEquals ("test#06", false, stream.CanWrite);
                		stream.Close ();
                		
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);
                	}
                }
                
                [Test]
                [ExpectedException(typeof(ArgumentException))]
                public void OpenException1 ()
                {
                	string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
                	FileStream stream = null;
                	// CreateNew + Read throws an exceptoin
                	try {
                		stream = File.Open (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", FileMode.CreateNew, FileAccess.Read);
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);
                	}
                }

                [Test]
                [ExpectedException(typeof(ArgumentException))]
                public void OpenException2 ()
                {
                	string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
                	FileStream s = null;
                	// Append + Read throws an exceptoin
			if (!File.Exists (path))
				File.Create (path).Close ();
                	try {
                		s = File.Open (path, FileMode.Append, FileAccess.Read);
                	} finally {
                		if (s != null)
                			s.Close ();
                		DeleteFile (path);
                	}
                }
                
                [Test]
                public void OpenRead ()
                {
                	string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
                	FileStream stream = null;
                	
                	try {
				stream = File.OpenRead (path);
                	
                		Assertion.AssertEquals ("test#01", true, stream.CanRead);
                		Assertion.AssertEquals ("test#02", true, stream.CanSeek);
                		Assertion.AssertEquals ("test#03", false, stream.CanWrite);
                		
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);
                	}
                }

                [Test]
                public void OpenWrite ()
                {
                	string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
                	FileStream stream = null;
                	
                	try {
				stream = File.OpenWrite (path);
                		Assertion.AssertEquals ("test#01", false, stream.CanRead);
                		Assertion.AssertEquals ("test#02", true, stream.CanSeek);
                		Assertion.AssertEquals ("test#03", true, stream.CanWrite);
                		stream.Close ();                				                	
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);
                	}
                }

		[Test]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void TestGetCreationTime ()
		{
                        string path = TempFolder + Path.DirectorySeparatorChar + "baz";
                	DeleteFile (path);
                	
                	try {
				File.Create (path).Close();
                		DateTime time = File.GetCreationTime (path);
                        	Assert ("GetCreationTime incorrect", (DateTime.Now - time).TotalSeconds < 10);
                	} finally {
                		DeleteFile (path);
                	}
		}

		// Setting the creation time on Unix is not possible
                [Test]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
                public void CreationTime ()
                {
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128))
				return;

                        string path = Path.GetTempFileName ();	
                       	
                       	try {
                		File.SetCreationTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		DateTime time = File.GetCreationTime (path);
                		Assertion.AssertEquals ("test#01", 2002, time.Year);
                		Assertion.AssertEquals ("test#02", 4, time.Month);
                		Assertion.AssertEquals ("test#03", 6, time.Day);
                		Assertion.AssertEquals ("test#04", 4, time.Hour);
                		Assertion.AssertEquals ("test#05", 4, time.Second);
                	
                		time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetCreationTimeUtc (path));
                		Assertion.AssertEquals ("test#06", 2002, time.Year);
                		Assertion.AssertEquals ("test#07", 4, time.Month);
                		Assertion.AssertEquals ("test#08", 6, time.Day);
                		Assertion.AssertEquals ("test#09", 4, time.Hour);
                		Assertion.AssertEquals ("test#10", 4, time.Second);                	

                		File.SetCreationTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		time = File.GetCreationTimeUtc (path);
                		Assertion.AssertEquals ("test#11", 2002, time.Year);
                		Assertion.AssertEquals ("test#12", 4, time.Month);
                		Assertion.AssertEquals ("test#13", 6, time.Day);
                		Assertion.AssertEquals ("test#14", 4, time.Hour);
                		Assertion.AssertEquals ("test#15", 4, time.Second);
                	
                		time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetCreationTime (path));
                		Assertion.AssertEquals ("test#16", 2002, time.Year);
                		Assertion.AssertEquals ("test#17", 4, time.Month);
                		Assertion.AssertEquals ("test#18", 6, time.Day);
                		Assertion.AssertEquals ("test#19", 4, time.Hour);
                		Assertion.AssertEquals ("test#20", 4, time.Second);
                       	} finally {
                       		DeleteFile (path);
                       	}
                }

                [Test]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
                public void LastAccessTime ()
                {
                        string path = TempFolder + Path.DirectorySeparatorChar + "lastAccessTime";                	
                        if (File.Exists (path))
                        	File.Delete (path);
			FileStream stream = null;
                	try {
                       		stream = File.Create (path);
                		stream.Close ();                	
                	
                		File.SetLastAccessTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		DateTime time = File.GetLastAccessTime (path);
                		Assertion.AssertEquals ("test#01", 2002, time.Year);
                		Assertion.AssertEquals ("test#02", 4, time.Month);
                		Assertion.AssertEquals ("test#03", 6, time.Day);
                		Assertion.AssertEquals ("test#04", 4, time.Hour);
                		Assertion.AssertEquals ("test#05", 4, time.Second);
                	
                		time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetLastAccessTimeUtc (path));
                		Assertion.AssertEquals ("test#06", 2002, time.Year);
                		Assertion.AssertEquals ("test#07", 4, time.Month);
                		Assertion.AssertEquals ("test#08", 6, time.Day);
                		Assertion.AssertEquals ("test#09", 4, time.Hour);
                		Assertion.AssertEquals ("test#10", 4, time.Second);                	
	
        	        	File.SetLastAccessTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		time = File.GetLastAccessTimeUtc (path);
                		Assertion.AssertEquals ("test#11", 2002, time.Year);
                		Assertion.AssertEquals ("test#12", 4, time.Month);
                		Assertion.AssertEquals ("test#13", 6, time.Day);
                		Assertion.AssertEquals ("test#14", 4, time.Hour);
                		Assertion.AssertEquals ("test#15", 4, time.Second);
                	
	                	time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetLastAccessTime (path));
                		Assertion.AssertEquals ("test#16", 2002, time.Year);
                		Assertion.AssertEquals ("test#17", 4, time.Month);
                		Assertion.AssertEquals ("test#18", 6, time.Day);
                		Assertion.AssertEquals ("test#19", 4, time.Hour);
                		Assertion.AssertEquals ("test#20", 4, time.Second);
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);                			
                	}
                }
                
                [Test]
                public void LastWriteTime ()
                {
                        string path = TempFolder + Path.DirectorySeparatorChar + "lastWriteTime";                	
                        if (File.Exists (path))
                        	File.Delete (path);
                    	FileStream stream = null;
                	try {
                       		stream = File.Create (path);
                		stream.Close ();                	
                	
                		File.SetLastWriteTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		DateTime time = File.GetLastWriteTime (path);
                		Assertion.AssertEquals ("test#01", 2002, time.Year);
                		Assertion.AssertEquals ("test#02", 4, time.Month);
                		Assertion.AssertEquals ("test#03", 6, time.Day);
                		Assertion.AssertEquals ("test#04", 4, time.Hour);
                		Assertion.AssertEquals ("test#05", 4, time.Second);
	                	
                		time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetLastWriteTimeUtc (path));
                		Assertion.AssertEquals ("test#06", 2002, time.Year);
                		Assertion.AssertEquals ("test#07", 4, time.Month);
                		Assertion.AssertEquals ("test#08", 6, time.Day);
                		Assertion.AssertEquals ("test#09", 4, time.Hour);
                		Assertion.AssertEquals ("test#10", 4, time.Second);                	
	
                		File.SetLastWriteTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                		time = File.GetLastWriteTimeUtc (path);
                		Assertion.AssertEquals ("test#11", 2002, time.Year);
                		Assertion.AssertEquals ("test#12", 4, time.Month);
                		Assertion.AssertEquals ("test#13", 6, time.Day);
                		Assertion.AssertEquals ("test#14", 4, time.Hour);
                		Assertion.AssertEquals ("test#15", 4, time.Second);
	                	
                		time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetLastWriteTime (path));
                		Assertion.AssertEquals ("test#16", 2002, time.Year);
                		Assertion.AssertEquals ("test#17", 4, time.Month);
                		Assertion.AssertEquals ("test#18", 6, time.Day);
                		Assertion.AssertEquals ("test#19", 4, time.Hour);
                		Assertion.AssertEquals ("test#20", 4, time.Second);
                	} finally {
                		if (stream != null)
                			stream.Close ();
                		DeleteFile (path);
                	}
                }

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeException1 ()
		{
			File.GetCreationTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeException2 ()
		{
			File.GetCreationTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException(typeof(IOException))]
#endif
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetCreationTimeException3";
			DeleteFile (path);
			DateTime time = File.GetCreationTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeException4 ()
		{
			File.GetCreationTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeUtcException1 ()
		{
			File.GetCreationTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeUtcException2 ()
		{
			File.GetCreationTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetCreationTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetCreationTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeUtcException4 ()
		{
			File.GetCreationTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetCreationTimeUtcException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeException1 ()
		{
			File.GetLastAccessTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeException2 ()
		{
			File.GetLastAccessTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeException3";
			DeleteFile (path);
			DateTime time = File.GetLastAccessTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeException4 ()
		{
			File.GetLastAccessTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeException5 ()
		{
			File.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeUtcException1 ()
		{
			File.GetLastAccessTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeUtcException2 ()
		{
			File.GetLastAccessTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastAccessTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeUtcException4 ()
		{
			File.GetLastAccessTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void GetLastAccessTimeUtcException5 ()
		{
			File.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetLastWriteTimeException1 ()
		{
			File.GetLastWriteTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException2 ()
		{
			File.GetLastWriteTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		public void GetLastWriteTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastWriteTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException4 ()
		{
			File.GetLastWriteTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException5 ()
		{
			File.GetLastWriteTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetLastWriteTimeUtcException1 ()
		{
			File.GetLastWriteTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException2 ()
		{
			File.GetLastWriteTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		public void GetLastWriteTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastWriteTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastWriteTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException4 ()
		{
			File.GetLastWriteTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException5 ()
		{
			File.GetLastWriteTimeUtc (Path.InvalidPathChars [0].ToString ());
		}		

		[Test]
		public void FileStreamClose ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "FileStreamClose";
			FileStream stream = null;
			try {
				stream = File.Create (path);
				stream.Close ();
				File.Delete (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		// SetCreationTime and SetCreationTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeArgumentNullException1 ()
		{
			File.SetCreationTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeArgumenException1 ()
		{
			File.SetCreationTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeArgumenException2 ()
		{
			File.SetCreationTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeArgumenException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetCreationTime (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetCreationTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetCreationTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeArgumentOutOfRangeException1";
//			FileStream stream = null;
//			DeleteFile (path);
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetCreationTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "CreationTimeIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetCreationTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcArgumentNullException1 ()
		{ 
			File.SetCreationTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcArgumenException1 ()
		{
			File.SetCreationTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcArgumenException2 ()
		{
			File.SetCreationTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcArgumentException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetCreationTimeUtc (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetCreationTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetCreationTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {				
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetCreationTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCreationTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetCreationTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		// SetLastAccessTime and SetLastAccessTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeArgumentNullException1 ()
		{
			File.SetLastAccessTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeArgumenException1 ()
		{
			File.SetLastAccessTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeArgumenException2 ()
		{
			File.SetLastAccessTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeArgumenException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetLastAccessTime (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastAccessTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastAccessTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastTimeArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastAccessTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "LastAccessIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastAccessTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeUtcArgumentNullException1 ()
		{
			File.SetLastAccessTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetCLastAccessTimeUtcArgumenException1 ()
		{
			File.SetLastAccessTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeUtcArgumenException2 ()
		{
			File.SetLastAccessTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeUtcArgumenException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetLastAccessTimeUtc (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastAccessTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastAccessTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastAccessTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastAccessTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastAccessTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		// SetLastWriteTime and SetLastWriteTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetLastWriteTimeArgumentNullException1 ()
		{
			File.SetLastWriteTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeArgumenException1 ()
		{
			File.SetLastWriteTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeArgumenException2 ()
		{
			File.SetLastWriteTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
		public void SetLastWriteTimeArgumenException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetLastWriteTime (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void SetLastWriteTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastWriteTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastWriteTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastWriteTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastWriteTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "LastWriteTimeIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastWriteTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetLastWriteTimeUtcArgumentNullException1 ()
		{
			File.SetLastWriteTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetCLastWriteTimeUtcArgumenException1 ()
		{
			File.SetLastWriteTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeUtcArgumenException2 ()
		{
			File.SetLastWriteTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		// On Unix there are no invalid path chars.
		public void SetLastWriteTimeUtcArgumenException3 ()
		{
			if (Path.InvalidPathChars.Length > 1) {
				bool pass = false;
				try {
					File.SetLastWriteTimeUtc (Path.InvalidPathChars [1].ToString (), new DateTime (2000, 12, 12, 11, 59, 59));
				} catch (ArgumentException) {
					pass = true;
				}

				Assertion.Assert ("#01", pass);
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void SetLastWriteTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastWriteTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastWriteTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastWriteTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}
//
		[Test]
		[ExpectedException(typeof (IOException))]
#if TARGET_JVM
        [Category("NotWorking")]
#endif
		public void SetLastWriteTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastAccessTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void OpenAppend ()
		{
			string fn = Path.GetTempFileName ();
			using (FileStream s = File.Open (fn, FileMode.Append))
				;
			
			DeleteFile (fn);
		}

#if NET_2_0
		void TestRWAT (string s)
		{
			string f = Path.GetTempFileName ();
			try {
				File.WriteAllText (f, s);
				string r = File.ReadAllText (f);
				AssertEquals (r, s);
			} finally {
				DeleteFile (f);
			}
		}
		[Test]
		public void ReadWriteAllText ()
		{
			// The MSDN docs said something about
			// not including a final new line. it looks
			// like that was not true. I'm not sure what
			// that was talking about
			TestRWAT ("");
			TestRWAT ("\r");
			TestRWAT ("\n");
			TestRWAT ("\r\n");
			TestRWAT ("a\r");
			TestRWAT ("a\n");
			TestRWAT ("a\r\n");	
			TestRWAT ("a\ra");
			TestRWAT ("a\na");
			TestRWAT ("a\r\na");
			TestRWAT ("a");
			TestRWAT ("\r\r");
			TestRWAT ("\n\n");
			TestRWAT ("\r\n\r\n");
		}
#endif

		private void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}

		private void DeleteDirectory (string path)
		{
			if (Directory.Exists (path))
				Directory.Delete (path);
		}
	}
}
