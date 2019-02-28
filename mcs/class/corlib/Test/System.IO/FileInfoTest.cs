// FileInfoTest.cs - NUnit Test Cases for System.IO.FileInfo class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) 2003 Ville Palo
// 

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileInfoTest
	{
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
		static readonly char DSC = Path.DirectorySeparatorChar;

		[SetUp]
		public void SetUp ()
		{
			DeleteDirectory (TempFolder);
			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			DeleteDirectory (TempFolder);
		}

		[Test] // ctor (String)
		public void Constructor1 ()
		{
			string path = TempFolder + DSC + "FIT.Ctr.Test";
			DeleteFile (path);

			FileInfo info = new FileInfo (path);
			Assert.IsTrue (info.DirectoryName.EndsWith (".Tests"), "#1");
			Assert.IsFalse (info.Exists, "#2");
			Assert.AreEqual (".Test", info.Extension, "#3");
			Assert.AreEqual ("FIT.Ctr.Test", info.Name, "#4");
		}

		[Test] // ctor (String)
		public void Constructor1_FileName_Null ()
		{
			try {
				new FileInfo (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("fileName", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_FileName_Empty ()
		{
			try {
				new FileInfo (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty file name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_FileName_InvalidPathChars ()
		{
			string path = string.Empty;
			foreach (char c in Path.InvalidPathChars)
				path += c;
			try {
				new FileInfo (path);
			} catch (ArgumentException ex) {
				// The path contains illegal characters
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_FileName_Whitespace ()
		{
			try {
				new FileInfo ("      ");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The path is not of a legal form
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void DirectoryTest ()
		{
			string path = TempFolder + DSC + "FIT.Directory.Test";
			DeleteFile (path);
			
			FileInfo info = new FileInfo (path);
			DirectoryInfo dir = info.Directory;
			Assert.AreEqual ("MonoTests.System.IO.Tests", dir.Name);
		}
		
		[Test]
		public void Exists ()
		{
			string path = TempFolder + DSC + "FIT.Exists.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#1");
			
				File.Create (path).Close ();
				Assert.IsFalse (info.Exists, "#2");
				info = new FileInfo (path);
				Assert.IsTrue (info.Exists, "#3");
				info = new FileInfo (TempFolder);
				Assert.IsFalse (info.Exists, "#4");
			} finally {
				DeleteFile (path);
			}
		}

#if !MOBILE
		[Test]
		public void IsReadOnly ()
		{
			string path = TempFolder + DSC + "FIT.IsReadOnly.Test";
			DeleteFile (path);
			
			try {
				using (FileStream stream = File.Create (path)) {
					stream.WriteByte (12);
					stream.Close ();
				}

				FileInfo info1 = new FileInfo (path);
				Assert.IsFalse (info1.IsReadOnly, "#1");

				FileInfo info2 = new FileInfo (path);
				info2.IsReadOnly = true;
				Assert.IsFalse (info1.IsReadOnly, "#2");
				Assert.IsTrue (info2.IsReadOnly, "#3");

				FileInfo info3 = new FileInfo (path);
				Assert.IsTrue (info3.IsReadOnly, "#4");
				info3.IsReadOnly = false;
				Assert.IsFalse (info1.IsReadOnly, "#4");
				Assert.IsTrue (info2.IsReadOnly, "#5");
				Assert.IsFalse (info3.IsReadOnly, "#6");
			} finally {
				File.SetAttributes (path, FileAttributes.Normal);
				DeleteFile (path);
			}
		}
#endif

		[Test]
		public void Length ()
		{
			string path = TempFolder + DSC + "FIT.Length.Test";
			DeleteFile (path);
			
			try {
				FileStream stream = File.Create (path);
				FileInfo info = new FileInfo (path);
				Assert.AreEqual (0, info.Length, "#1");
				stream.WriteByte (12);
				stream.Flush ();
				Assert.AreEqual (0, info.Length, "#2");
				info = new FileInfo (path);
				Assert.AreEqual (1, info.Length, "#3");
				stream.Close ();
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Length_FileDoesNotExist ()
		{
			string path = TempFolder + DSC + "FIT.LengthException.Test";
			DeleteFile (path);
			FileInfo info = new FileInfo (path);
			try {
				long l = info.Length;
				Assert.Fail ("#1:" + l);
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
				Assert.AreEqual (path, ex.FileName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
		}
		
		[Test]
		public void AppendText ()
		{
			string path = TempFolder + DSC + "FIT.AppendText.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#1");
			
				StreamWriter writer = info.AppendText ();
				info = new FileInfo (path);
				Assert.IsTrue (info.Exists, "#2");
				
				writer.Write ("aaa");
				writer.Flush ();
				writer.Close ();
			
				Assert.AreEqual (0, info.Length, "#3");
				info = new FileInfo (path);
				Assert.AreEqual (3, info.Length, "#4");
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1 ()
		{
			string path1 = TempFolder + DSC + "FIT.CopyTo.Source.Test";
			string path2 = TempFolder + DSC + "FIT.CopyTo.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			try {
				File.Create (path1).Close ();
			
				FileInfo info = new FileInfo (path1);
				Assert.IsTrue (info.Exists, "#1");

				FileInfo info2 = info.CopyTo (path2);
				info = new FileInfo (path1);
				Assert.IsTrue (info2.Exists, "#2");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1_DestFileName_AlreadyExists ()
		{
			string path1 = TempFolder + DSC + "FIT.CopyToException.Source.Test";
			string path2 = TempFolder + DSC + "FIT.CopyToException.Dest.Test";

			try {
				DeleteFile (path1);
				DeleteFile (path2);
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				FileInfo info = new FileInfo (path1);
				try {
					info.CopyTo (path2);
					Assert.Fail ("#1");
				} catch (IOException ex) {
					// The file '...' already exists.
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf (path2) != -1, "#5");
				}
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1_DestFileName_Null ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgumentNullException.Test";
			DeleteFile (path);
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1_DestFileName_Empty ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument1Exception.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo (string.Empty);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Empty file name is not legal
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1_DestFileName_Whitespace ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument2Exception.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo ("    ");
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The path is not of a legal form
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String)
		public void CopyTo1_DestFileName_InvalidPathChars ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument4Exception.Test";
			string path2 = string.Empty;
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				foreach (char c in Path.InvalidPathChars)
					path2 += c;
				try {
					info.CopyTo (path2);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Illegal characters in path
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2 ()
		{
			string path1 = TempFolder + DSC + "FIT.CopyTo2.Source.Test";
			string path2 = TempFolder + DSC + "FIT.CopyTo2.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			try {
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				FileInfo info = new FileInfo (path1);

				FileInfo info2 = info.CopyTo (path2, true);
				info = new FileInfo (path1);
				Assert.IsTrue (info.Exists, "#1");
				Assert.IsTrue (info2.Exists, "#2");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2_DestFileName_AlreadyExists ()
		{
			string path1 = TempFolder + DSC + "FIT.CopyToException.Source.Test";
			string path2 = TempFolder + DSC + "FIT.CopyToException.Dest.Test";

			try {
				DeleteFile (path1);
				DeleteFile (path2);
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				FileInfo info = new FileInfo (path1);
				try {
					info.CopyTo (path2, false);
					Assert.Fail ("#1");
				} catch (IOException ex) {
					// The file '...' already exists.
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsTrue (ex.Message.IndexOf (path2) != -1, "#5");
				}
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2_DestFileName_Null ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgumentNullException.Test";
			DeleteFile (path);
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo (null, false);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2_DestFileName_Empty ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument1Exception.Test";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo (string.Empty, false);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Empty file name is not legal
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2_DestFileName_Whitespace ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument2Exception.Test";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.CopyTo ("    ", false);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The path is not of a legal form
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test] // CopyTo (String, Boolean)
		public void CopyTo2_DestFileName_InvalidPathChars ()
		{
			string path = TempFolder + DSC + "FIT.CopyToArgument4Exception.Test";
			string path2 = string.Empty;
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				foreach (char c in Path.InvalidPathChars)
					path2 += c;
				try {
					info.CopyTo (path2, false);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Illegal characters in path
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Create ()
		{
			string path = TempFolder + DSC + "FIT.Create.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#1");
				FileStream stream = info.Create ();
				Assert.IsFalse (info.Exists, "#2");
				info = new FileInfo (path);
				Assert.IsTrue (info.Exists, "#3");
				Assert.IsTrue (stream.CanRead, "#4");
				Assert.IsTrue (stream.CanWrite, "#5");
				Assert.IsTrue (stream.CanSeek, "#6");
				stream.Close ();
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void CreateText ()
		{
			string path = TempFolder + DSC + "FIT.CreateText.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#1");
				StreamWriter writer = info.CreateText ();
				writer.WriteLine ("test");
				writer.Close ();
				info = new FileInfo (path);
				Assert.IsTrue (info.Exists, "#2");
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void CreateText_Directory ()
		{
			FileInfo info = new FileInfo (TempFolder);
			try {
				info.CreateText ();
				Assert.Fail ("#1");
			} catch (UnauthorizedAccessException ex) {
				Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
		
		[Test]
		public void Delete ()
		{
			string path = TempFolder + DSC + "FIT.Delete.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#1");
				info.Delete ();
				Assert.IsFalse (info.Exists, "#1a");
				info.Create ().Close ();
				info = new FileInfo (path);
				Assert.IsTrue (info.Exists, "#2");
				info.Delete ();
				Assert.IsTrue (info.Exists, "#3");
				info = new FileInfo (path);
				Assert.IsFalse (info.Exists, "#4");
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Delete_Directory ()
		{
			FileInfo info = new FileInfo (TempFolder);
			try {
				info.Delete ();
				Assert.Fail ("#1");
			} catch (UnauthorizedAccessException ex) {
				Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
		
		[Test]
		[Category("AndroidSdksNotWorking")]
		public void MoveTo ()
		{
			string path1 = TempFolder + DSC + "FIT.MoveTo.Source.Test";
			string path2 = TempFolder + DSC + "FIT.MoveTo.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				File.Create (path1).Close ();
				FileInfo info1 = new FileInfo (path1);
				FileInfo info2 = new FileInfo (path2);
				Assert.IsTrue (info1.Exists, "#A1");
				Assert.AreEqual (path1, info1.FullName, "#A2");
				Assert.IsFalse (info2.Exists, "#A3");
				Assert.AreEqual (path2, info2.FullName, "#A4");

				info1.MoveTo (path2);
				info2 = new FileInfo (path2);
				Assert.IsTrue (info1.Exists, "#B1");
				Assert.AreEqual (path2, info1.FullName, "#B2");
				Assert.IsTrue (info2.Exists, "#B3");
				Assert.AreEqual (path2, info2.FullName, "#B4");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test] //Covers #18361
		[Category("AndroidSdksNotWorking")]
		public void MoveTo_SameName ()
		{
			string name = "FIT.MoveTo.SameName.Test";
			string path1 = TempFolder + DSC + name;
			string path2 = TempFolder + DSC + "same";
			Directory.CreateDirectory (path2);
			path2 += DSC + name;
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				File.Create (path1).Close ();
				FileInfo info1 = new FileInfo (path1);
				FileInfo info2 = new FileInfo (path2);
				Assert.IsTrue (info1.Exists, "#A1");
				Assert.IsFalse (info2.Exists, "#A2");

				info1.MoveTo (path2);
				info1 = new FileInfo (path1);
				info2 = new FileInfo (path2);
				Assert.IsFalse (info1.Exists, "#B1");
				Assert.IsTrue (info2.Exists, "#B2");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test]
		[Category ("NotWasm")]
		public void MoveTo_DestFileName_AlreadyExists ()
		{
			string sourceFile = TempFolder + DSC + "FIT.MoveTo.Source.Test";
			string destFile;
			FileInfo info;

			// move to same directory
			File.Create (sourceFile).Close ();
			info = new FileInfo (sourceFile);
			try {
				info.MoveTo (TempFolder);
				Assert.Fail ("#A1");
			} catch (IOException ex) {
				// Cannot create a file when that file already exists
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsFalse (ex.Message.IndexOf (sourceFile) != -1, "#A5");
			} finally {
				DeleteFile (sourceFile);
			}

			// move to exist file
			File.Create (sourceFile).Close ();
			destFile = TempFolder + DSC + "FIT.MoveTo.Dest.Test";
			File.Create (destFile).Close ();
			info = new FileInfo (sourceFile);
			try {
				info.MoveTo (destFile);
				Assert.Fail ("#B1");
			} catch (IOException ex) {
				// Cannot create a file when that file already exists
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsFalse (ex.Message.IndexOf (sourceFile) != -1, "#B5");
			} finally {
				DeleteFile (sourceFile);
				DeleteFile (destFile);
			}

			// move to existing directory
			File.Create (sourceFile).Close ();
			destFile = TempFolder + Path.DirectorySeparatorChar + "bar";
			Directory.CreateDirectory (destFile);
			info = new FileInfo (sourceFile);
			try {
				info.MoveTo (destFile);
				Assert.Fail ("#C1");
			} catch (IOException ex) {
				// Cannot create a file when that file already exists
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsFalse (ex.Message.IndexOf (sourceFile) != -1, "#C5");
			} finally {
				DeleteFile (sourceFile);
				DeleteDirectory (destFile);
			}
		}

		[Test]
		[Category ("NotWasm")]
		public void MoveTo_DestFileName_DirectoryDoesNotExist ()
		{
			string sourceFile = TempFolder + Path.DirectorySeparatorChar + "foo";
			string destFile = Path.Combine (Path.Combine (TempFolder, "doesnotexist"), "b");
			DeleteFile (sourceFile);
			try {
				File.Create (sourceFile).Close ();
				FileInfo info = new FileInfo (sourceFile);
				try {
					info.MoveTo (destFile);
					Assert.Fail ("#1");
				} catch (DirectoryNotFoundException ex) {
					// Could not find a part of the path
					Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (sourceFile);
			}
		}

		[Test]
		public void MoveTo_DestFileName_Null ()
		{
			string path = TempFolder + DSC + "FIT.MoveToArgumentNullException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.MoveTo (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		public void MoveTo_DestFileName_Empty ()
		{
			string path = TempFolder + DSC + "FIT.MoveToArgumentException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.MoveTo (string.Empty);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Empty file name is not legal
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destFileName", ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		public void MoveTo_DestFileName_Whitespace ()
		{
			string path = TempFolder + DSC + "FIT.MoveToArgumentException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				try {
					info.MoveTo ("   ");
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The path is not of a legal form
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		public void MoveTo_FileDoesNotExist ()
		{
			string path1 = TempFolder + DSC + "FIT.MoveToFileNotFoundException.Src";
			string path2 = TempFolder + DSC + "FIT.MoveToFileNotFoundException.Dst";
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				FileInfo info = new FileInfo (path1);
				try {
					info.MoveTo (path2);
					Assert.Fail ("#1");
				} catch (FileNotFoundException ex) {
					// Unable to find the specified file
					Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test]
		public void MoveTo_Same ()
		{
			string path = TempFolder + DSC + "FIT.MoveToSame.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.MoveTo (path);
				Assert.IsTrue (info.Exists, "#1");
				Assert.IsTrue (File.Exists (path), "#2");
			} finally {
				DeleteFile (path);
			}
		}

		[Test] //Covers #38796
		[Category("AndroidSdksNotWorking")]
		public void ToStringAfterMoveTo ()
		{
			string name1 = "FIT.ToStringAfterMoveTo.Test";
			string name2 = "FIT.ToStringAfterMoveTo.Test.Alt";
			string path1 = TempFolder + DSC + name1;
			string path2 = TempFolder + DSC + name2;
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				File.Create (path1).Close ();
				FileInfo info = new FileInfo (path1);
				Assert.AreEqual (path1, info.ToString (), "#A");

				info.MoveTo (path2);
				Assert.AreEqual (path2, info.ToString (), "#B");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

#if !MOBILE
		[Test]
		public void Replace1 ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			string path2 = TempFolder + DSC + "FIT.Replace.Dest.Test";
			string path3 = TempFolder + DSC + "FIT.Replace.Back.Test";
			
			DeleteFile (path1);
			DeleteFile (path2);
			DeleteFile (path3);
			try {
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				File.Create (path3).Close ();				
				FileInfo info = new FileInfo (path1);
				Assert.IsTrue (info.Exists, "#1");
				FileInfo info2 = info.Replace (path2, path3);
				Assert.IsTrue (info2.Exists, "#2");				
				FileInfo info3 = new FileInfo (path3);
				Assert.IsTrue (info3.Exists, "#3");							
			} finally {
				DeleteFile (path2);
				DeleteFile (path3);
			}
		}

		[Test]
		public void Replace1_Backup_Null ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			string path2 = TempFolder + DSC + "FIT.Replace.Dest.Test";
			
			DeleteFile (path1);
			DeleteFile (path2);
			try {
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				FileInfo info = new FileInfo (path1);
				Assert.IsTrue (info.Exists, "#1");
				FileInfo info2 = info.Replace (path2, null);
				Assert.IsTrue (info2.Exists, "#2");
				info = new FileInfo (path1);
				Assert.IsFalse (info.Exists, "#3");				
			} finally {
				DeleteFile (path2);
			} 
		}

		[Test]
		public void Replace1_DestFileName_Null ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			DeleteFile (path1);
			try {
				try {
					File.Create (path1).Close ();
					FileInfo info = new FileInfo (path1);
					info.Replace (null, null);
					Assert.Fail ("#1");						
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");				
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");	
				}
			} finally {
				DeleteFile (path1);
			}
		}

		[Test]
		public void Replace1_DestFileName_Empty ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			DeleteFile (path1);
			try {
				try {
					File.Create (path1).Close ();
					FileInfo info = new FileInfo (path1);
					info.Replace (string.Empty, null);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (path1);
			}
		}
		
		[Test]
		public void Replace1_DestFileName_WhiteSpace ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			DeleteFile (path1);
			try {
				try {
					File.Create (path1).Close ();
					FileInfo info = new FileInfo (path1);
					info.Replace ("     ", null);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (path1);
			}
		}

		[Test] 
		public void Replace1_DestFileName_InvalidPathChars ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			string path2 = string.Empty;
			DeleteFile (path1);

			try {
				File.Create (path1).Close ();
				FileInfo info = new FileInfo (path1);
				foreach (char c in Path.InvalidPathChars)
					path2 += c;
				try {
					info.Replace (path2, null);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Illegal characters in path
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteFile (path1);
			}
		}

		[Test]
		public void Replace1_DestFileName_FileNotFound ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			string path2 = TempFolder + DSC + "FIT.Replace.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);

			try {
				try {
					File.Create (path1).Close ();
					FileInfo info = new FileInfo (path1);
					info.Replace (path2, null);
					Assert.Fail ("#1");
				} catch (FileNotFoundException ex) {
					Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (path1);			
			}
		}

		[Test]
		public void Replace1_Source_FileNotFound ()
		{
			string path1 = TempFolder + DSC + "FIT.Replace.Source.Test";
			string path2 = TempFolder + DSC + "FIT.Replace.Dest.Test";
			DeleteFile (path2);

			try {
				try {
					File.Create (path2).Close ();
					FileInfo info = new FileInfo (path1);
					info.Replace (path2, null);
					Assert.Fail ("#1");
				} catch (FileNotFoundException ex) {
					Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteFile (path2);			
			}		
		}
#endif

		[Test]
		public void Open ()
		{
			string path = TempFolder + DSC + "FIT.Open.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				FileInfo info = new FileInfo (path);
				stream = info.Open (FileMode.CreateNew);
				Assert.IsTrue (stream.CanRead, "#A1");
				Assert.IsTrue (stream.CanSeek, "#A2");
				Assert.IsTrue (stream.CanWrite, "#A3");
				stream.Close ();
				
				stream = info.Open (FileMode.Open);
				Assert.IsTrue (stream.CanRead, "#B1");
				Assert.IsTrue (stream.CanSeek, "#B2");
				Assert.IsTrue (stream.CanWrite, "#B3");
				stream.Close ();
				
				stream = info.Open (FileMode.Append, FileAccess.Write);
				Assert.IsFalse (stream.CanRead, "#C1");
				Assert.IsTrue (stream.CanSeek, "#C2");
				Assert.IsTrue (stream.CanWrite, "#C3");
				stream.Close ();

				stream = info.Open (FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				Assert.IsTrue (stream.CanRead, "#D1");
				Assert.IsTrue (stream.CanSeek, "#D2");
				Assert.IsTrue (stream.CanWrite, "#D3");
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Open_FileDoesNotExist ()
		{
			string path = TempFolder + DSC + "FIT.OpenFileNotFoundException.Test";
			DeleteFile (path);
			
			FileInfo info = new FileInfo (path);
			try {
				info.Open (FileMode.Open);
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
				Assert.AreEqual (path, ex.FileName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
		}
		
		[Test]
		public void OpenRead ()
		{
			string path = TempFolder + DSC + "FIT.OpenRead.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				stream = info.OpenRead ();
				Assert.IsTrue (stream.CanRead, "#1");
				Assert.IsTrue (stream.CanSeek, "#2");
				Assert.IsFalse (stream.CanWrite, "#3");
				stream.Close ();
				
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void OpenRead_FileLock ()
		{
			string path = TempFolder + DSC + "FIT.OpenReadIOException.Test";
			DeleteFile (path);
			FileStream stream = null;
			
			try {
				stream = File.Create (path);
				FileInfo info = new FileInfo (path);
				try {
					info.OpenRead ();
					Assert.Fail ("#1");
				} catch (IOException ex) {
					// The process cannot access the file because
					// it is being used by another process
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void OpenRead_Directory ()
		{
			FileInfo info = new FileInfo (TempFolder);
			try {
				info.OpenRead ();
				Assert.Fail ("#1");
			} catch (UnauthorizedAccessException ex) {
				Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
		
		[Test]
		public void OpenText ()
		{
			string path = TempFolder + DSC + "FIT.OpenText.Test";
			DeleteFile (path);
			StreamReader reader = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				reader = info.OpenText ();
				Assert.AreEqual (-1, reader.Peek ());
			} finally {
				if (reader != null)
					reader.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void OpenText_FileDoesNotExist ()
		{
			string path = TempFolder + DSC + "FIT.OpenTextFileNotFoundException.Test";
			DeleteFile (path);
			FileInfo info = new FileInfo (path);
			try {
				info.OpenText ();
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
				Assert.AreEqual (path, ex.FileName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
		}

		[Test]
		public void OpenText_Directory ()
		{
			FileInfo info = new FileInfo (TempFolder);
			try {
				info.OpenText ();
				Assert.Fail ("#1");
			} catch (UnauthorizedAccessException ex) {
				Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void OpenWrite ()
		{
			string path = TempFolder + DSC + "FIT.OpenWrite.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				stream = info.OpenWrite ();
				Assert.IsFalse (stream.CanRead, "#1");
				Assert.IsTrue (stream.CanSeek, "#2");
				Assert.IsTrue (stream.CanWrite, "#3");
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void OpenWrite_Directory ()
		{
			FileInfo info = new FileInfo (TempFolder);
			try {
				info.OpenWrite ();
				Assert.Fail ("#1");
			} catch (UnauthorizedAccessException ex) {
				Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
#if !MOBILE			
		[Test]
		public void Serialization ()
		{
			FileInfo info;
			SerializationInfo si;
			
			info = new FileInfo ("Test");
			si = new SerializationInfo (typeof (FileInfo), new FormatterConverter ());
			info.GetObjectData (si, new StreamingContext ());

			Assert.AreEqual (3, si.MemberCount, "#A1");
			Assert.AreEqual ("Test", si.GetString ("OriginalPath"), "#A2");
			Assert.AreEqual (Path.Combine (Directory.GetCurrentDirectory (), "Test"), si.GetString ("FullPath"), "#A3");

			info = new FileInfo (TempFolder);
			si = new SerializationInfo (typeof (FileInfo), new FormatterConverter ());
			info.GetObjectData (si, new StreamingContext ());

			Assert.AreEqual (3, si.MemberCount, "#B1");
			Assert.AreEqual (TempFolder, si.GetString ("OriginalPath"), "#B2");
			Assert.AreEqual (TempFolder, si.GetString ("FullPath"), "#B3");
		}

		[Test]
		public void Deserialization ()
		{
			FileInfo info = new FileInfo ("Test");

			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, info);
			ms.Position = 0;

			FileInfo clone = (FileInfo) bf.Deserialize (ms);
			Assert.AreEqual (info.Name, clone.Name, "#1");
			Assert.AreEqual (info.FullName, clone.FullName, "#2");
		}
#endif

		[Test]
		[Category ("MobileNotWorking")]
		public void ToStringVariety ()
		{
			Assert.AreEqual ("foo", new FileInfo ("foo").ToString ());
			Assert.AreEqual ("c:/foo", new FileInfo ("c:/foo").ToString ());
			Assert.AreEqual ("/usr/local/foo", new FileInfo ("/usr/local/foo").ToString ());
			Assert.AreEqual ("c:\\foo", new FileInfo ("c:\\foo").ToString ());
			Assert.AreEqual ("/usr/local\\foo", new FileInfo ("/usr/local\\foo").ToString ());
			Assert.AreEqual ("foo/BAR/baz", new FileInfo ("foo/BAR/baz").ToString ());
			Assert.AreEqual ("c:/documents and settings", new FileInfo ("c:/documents and settings").ToString ());
			Assert.AreEqual ("c:/docUme~1", new FileInfo ("c:/docUme~1").ToString ());
		}

		void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}

		void DeleteDirectory (string path)
		{
			if (Directory.Exists (path))
				Directory.Delete (path, true);
		}
	}
}
