// DirectoryInfoTest.cs - NUnit Test Cases for System.IO.DirectoryInfo class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) 2003 Ville Palo
// 
using NUnit.Framework;
using System;
using System.IO;
namespace MonoTests.System.IO
{
	[TestFixture]
    	public class DirectoryInfoTest : Assertion
    	{
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		static readonly char DSC = Path.DirectorySeparatorChar;

        	[SetUp]
        	protected void SetUp() {
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
        	}
        
        	[TearDown]
        	protected void TearDown() {
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
		}
        
        	[Test]
        	public void Ctr ()
        	{
        	    	string path = TempFolder + DSC + "DIT.Ctr.Test";
        		DeleteDir (path);
            	
        	    	FileInfo info = new FileInfo (path);
        	    	AssertEquals ("test#01", true, info.DirectoryName.EndsWith (".Tests"));
        	    	AssertEquals ("test#02", false, info.Exists);
        	    	AssertEquals ("test#03", ".Test", info.Extension);
        	    	AssertEquals ("test#05", "DIT.Ctr.Test", info.Name);            
        	}

        	[Test]
        	[ExpectedException(typeof(ArgumentNullException))]
        	public void CtorArgumentNullException ()
        	{
        	    	DirectoryInfo info = new DirectoryInfo (null);            
        	}

        	[Test]
        	[ExpectedException(typeof(ArgumentException))]
        	public void CtorArgumentException1 ()
        	{
        	    	DirectoryInfo info = new DirectoryInfo ("");            
        	}

        	[Test]
        	[ExpectedException(typeof(ArgumentException))]
        	public void CtorArgumentException2 ()
        	{
        	    	DirectoryInfo info = new DirectoryInfo ("   ");            
        	}

        	[Test]
        	[ExpectedException(typeof(ArgumentException))]
        	public void CtorArgumentException3 ()
        	{
            	string path = "";
            	foreach (char c in Path.InvalidPathChars) {
                	path += c;
            	}
            	DirectoryInfo info = new DirectoryInfo (path);
        	}
        	        	
        	[Test]
        	public void Exists ()
        	{
            	string path = TempFolder + DSC + "DIT.Exists.Test";
            	DeleteDir (path);
            
            	try {
            	    	DirectoryInfo info = new DirectoryInfo (path);
                	AssertEquals ("test#01", false, info.Exists);
            
                	Directory.CreateDirectory (path);
                	AssertEquals ("test#02", false, info.Exists);
                	info = new DirectoryInfo (path);
                	AssertEquals ("test#03", true, info.Exists);            
            	} finally {
                	DeleteDir (path);
            	}
        	}
        	
        	[Test]
        	public void Name ()
        	{
        		string path = TempFolder + DSC + "DIT.Name.Test";
        		DeleteDir (path);
        		
        		try {
        			DirectoryInfo info = new DirectoryInfo (path);        			
        			AssertEquals ("test#01", "DIT.Name.Test", info.Name);
        			
        			info = Directory.CreateDirectory (path);
        			AssertEquals ("test#02", "DIT.Name.Test", info.Name);
        			
        			
        		} finally {
        			DeleteDir (path);
        		}        		           
        	}
        	
        	[Test]
        	public void Parent ()
        	{
        		string path = TempFolder + DSC + "DIT.Parent.Test";
        		DeleteDir (path);
        		
        		try {
        			DirectoryInfo info = new DirectoryInfo (path);
        			AssertEquals ("test#01", "MonoTests.System.IO.Tests", info.Parent.Name);
        			
        			info = Directory.CreateDirectory (path);
        			AssertEquals ("test#02", "MonoTests.System.IO.Tests", info.Parent.Name);
        			        			
        		} finally {
        			DeleteDir (path);
        		}        		                   	
        	}

        	[Test]
        	public void Create ()
        	{
            	string path = TempFolder + DSC + "DIT.Create.Test";
            	DeleteDir (path);
            
            	try {
                	DirectoryInfo info = new DirectoryInfo (path);
                	AssertEquals ("test#01", false, info.Exists);
                	info.Create ();                
                	AssertEquals ("test#02", false, info.Exists);
                	info = new DirectoryInfo (path);
                	AssertEquals ("test#03", true, info.Exists);
            	} finally {
                	DeleteDir (path);
            	}
        	}

		[Test]
		public void CreateSubdirectory ()
		{
			string sub_path = Path.Combine ("test01", "test02");
			try {
				DirectoryInfo info = new DirectoryInfo (TempFolder);
				info.CreateSubdirectory (sub_path);
				Assert ("test#01", Directory.Exists (Path.Combine (TempFolder, sub_path)));
			} finally {
				DeleteDir (Path.Combine (TempFolder, sub_path));
			}
				
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CreateSubdirectoryEmptyString ()
		{
			new DirectoryInfo (".").CreateSubdirectory ("");
		}

		[Test]
		public void Delete1 ()
		{
            	string path = TempFolder + DSC + "DIT.Delete1.Test";
            	DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				DirectoryInfo info = new DirectoryInfo (path);
				AssertEquals ("test#01", true, info.Exists);
				
				info.Delete ();
				AssertEquals ("test#02", true, info.Exists);
				
				info = new DirectoryInfo (path);
				AssertEquals ("test#03", false, info.Exists);
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void Delete2 ()
		{
            	string path = TempFolder + DSC + "DIT.Delete2.Test";
            	DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				AssertEquals ("test#01", true, info.Exists);
				
				info.Delete (true);
				AssertEquals ("test#02", true, info.Exists);
				
				info = new DirectoryInfo (path);
				AssertEquals ("test#03", false, info.Exists);
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (IOException))]
		public void DeleteIOException1 ()
		{
            	string path = TempFolder + DSC + "DIT.DeleteIOException1.Test";
            	DeleteDir (path);			
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				info.Delete ();
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void DeleteIOException2 ()
		{
            	string path = TempFolder + DSC + "DIT.DeleteIOException2.Test";
            	DeleteDir (path);			
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				info.Delete (false);
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetDirectories1 ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectories1.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				AssertEquals ("test#01", 0, info.GetDirectories ().Length);
				
				Directory.CreateDirectory (path + DSC + "1");
				Directory.CreateDirectory (path + DSC + "2");				
				File.Create (path + DSC + "filetest").Close ();
				AssertEquals ("test#02", 2, info.GetDirectories ().Length);
				
				Directory.Delete (path + DSC + 2);
				AssertEquals ("test#02", 1, info.GetDirectories ().Length);				
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetDirectories2 ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectories2.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				AssertEquals ("test#01", 0, info.GetDirectories ("*").Length);
				
				Directory.CreateDirectory (path + DSC + "test120");
				Directory.CreateDirectory (path + DSC + "test210");
				Directory.CreateDirectory (path + DSC + "atest330");
				Directory.CreateDirectory (path + DSC + "test220");
				File.Create (path + DSC + "filetest").Close ();
				
				AssertEquals ("test#02", 4, info.GetDirectories ("*").Length);
				AssertEquals ("test#03", 3, info.GetDirectories ("test*").Length);
				AssertEquals ("test#04", 2, info.GetDirectories ("test?20").Length);
				AssertEquals ("test#05", 0, info.GetDirectories ("test?").Length);
				AssertEquals ("test#06", 0, info.GetDirectories ("test[12]*").Length);
				AssertEquals ("test#07", 2, info.GetDirectories ("test2*0").Length);
				AssertEquals ("test#08", 4, info.GetDirectories ("*test*").Length);
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]		
		public void GetDirectoriesDirectoryNotFoundException1 ()
		{
            	string path = TempFolder + DSC + "DIT.GetDirectoriesDirectoryNotFoundException1.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetDirectories ();
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]		
		public void GetDirectoriesDirectoryNotFoundException2 ()
		{
            	string path = TempFolder + DSC + "DIT.GetDirectoriesDirectoryNotFoundException2.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetDirectories ("*");
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetDirectoriesArgumentNullException ()
		{
            	string path = TempFolder + DSC + "DIT.GetDirectoriesArgumentNullException.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetDirectories (null);
			} finally {
				DeleteDir (path);
			}			
		}

		[Test]
		public void GetFiles1 ()
		{
            	string path = TempFolder + DSC + "DIT.GetFiles1.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				AssertEquals ("test#01", 0, info.GetFiles ().Length);
				File.Create (path + DSC + "file1").Close ();
				File.Create (path + DSC + "file2").Close ();
				Directory.CreateDirectory (path + DSC + "directory1");
				AssertEquals ("test#02", 2, info.GetFiles ().Length);
				                        
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetFiles2()
		{
            	string path = TempFolder + DSC + "DIT.GetFiles2.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				AssertEquals ("test#01", 0, info.GetFiles ("*").Length);
				File.Create (path + DSC + "file120file").Close ();
				File.Create (path + DSC + "file220file").Close ();
				File.Create (path + DSC + "afile330file").Close ();
				File.Create (path + DSC + "test.abc").Close ();
				File.Create (path + DSC + "test.abcd").Close ();
				File.Create (path + DSC + "test.abcdef").Close ();				
				Directory.CreateDirectory (path + DSC + "dir");
				
				AssertEquals ("test#02", 6, info.GetFiles ("*").Length);
				AssertEquals ("test#03", 2, info.GetFiles ("file*file").Length);
				AssertEquals ("test#04", 3, info.GetFiles ("*file*").Length);
				AssertEquals ("test#05", 2, info.GetFiles ("file?20file").Length);
				AssertEquals ("test#07", 1, info.GetFiles ("*.abcd").Length);
				AssertEquals ("test#08", 2, info.GetFiles ("*.abcd*").Length);				                        
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void GetFilesDirectoryNotFoundException1 ()
		{
			string path = TempFolder + DSC + "DIT.GetFilesDirectoryNotFoundException1.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetFiles ();
				
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void GetFilesDirectoryNotFoundException2 ()
		{
			string path = TempFolder + DSC + "DIT.GetFilesDirectoryNotFoundException2.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetFiles ("*");
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFilesArgumentNullException ()
		{
			string path = TempFolder + DSC + "DIT.GetFilesArgumentNullException.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetFiles (null);				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void MoveTo ()
		{
			string path1 = TempFolder + DSC + "DIT.MoveTo.Soucre.Test";
			string path2 = TempFolder + DSC + "DIT.MoveTo.Dest.Test";
			DeleteDir (path1);
			DeleteDir (path2);
			
			try {
				DirectoryInfo info1 = Directory.CreateDirectory (path1);
				DirectoryInfo info2 = new DirectoryInfo (path2);
				
				AssertEquals ("test#01", true, info1.Exists);
				AssertEquals ("test#02", false, info2.Exists);
												
				info1.MoveTo (path2);				
				AssertEquals ("test#03", true, info1.Exists);
				AssertEquals ("test#04", false, info2.Exists);
				
				info1 = new DirectoryInfo (path1);
				info2 = new DirectoryInfo (path2);
				AssertEquals ("test#05", false, info1.Exists);
				AssertEquals ("test#06", true, info2.Exists);
				
			} finally {
				DeleteDir (path1);
				DeleteDir (path2);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MoveToArgumentNullException ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentNullException.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				info.MoveTo (null);
			} finally {
				DeleteDir (path);
			}
			
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void MoveToIOException1 ()
		{
			string path = TempFolder + DSC + "DIT.MoveToIOException1.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				info.MoveTo (path);
			} finally {
				DeleteDir (path);
			}			
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MoveToArgumentException1 ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException1.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				info.MoveTo ("");
			} finally {
				DeleteDir (path);
			}			
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MoveToArgumentException2 ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException2.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				info.MoveTo ("    ");
			} finally {
				DeleteDir (path);
			}			
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MoveToArgumentException3 ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException3.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				info.MoveTo (Path.InvalidPathChars [0].ToString ());
			} finally {
				DeleteDir (path);
			}			
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void MoveToIOException2 ()
		{
			string path = TempFolder + DSC + "DIT.MoveToIOException2.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.MoveTo (path);
			} finally {
				DeleteDir (path);
			}			
		}

        	private void DeleteDir (string path)
        	{
        		if (Directory.Exists (path))
        			Directory.Delete (path, true);
        	}
        			
    	}
}
