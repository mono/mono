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
    	public class DirectoryInfoTest
    	{
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

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
        	    	string path = TempFolder + "/DIT.Ctr.Test";
        		DeleteDir (path);
            	
        	    	FileInfo info = new FileInfo (path);
        	    	Assertion.AssertEquals ("test#01", true, info.DirectoryName.EndsWith (".Tests"));
        	    	Assertion.AssertEquals ("test#02", false, info.Exists);
        	    	Assertion.AssertEquals ("test#03", ".Test", info.Extension);
        	    	Assertion.AssertEquals ("test#05", "DIT.Ctr.Test", info.Name);            
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
            	string path = TempFolder + "/DIT.Exists.Test";
            	DeleteDir (path);
            
            	try {
            	    	DirectoryInfo info = new DirectoryInfo (path);
                	Assertion.AssertEquals ("test#01", false, info.Exists);
            
                	Directory.CreateDirectory (path);
                	Assertion.AssertEquals ("test#02", false, info.Exists);
                	info = new DirectoryInfo (path);
                	Assertion.AssertEquals ("test#03", true, info.Exists);            
            	} finally {
                	DeleteDir (path);
            	}
        	}
        	
        	[Test]
        	public void Name ()
        	{
        		string path = TempFolder + "/DIT.Name.Test";
        		DeleteDir (path);
        		
        		try {
        			DirectoryInfo info = new DirectoryInfo (path);        			
        			Assertion.AssertEquals ("test#01", "DIT.Name.Test", info.Name);
        			
        			info = Directory.CreateDirectory (path);
        			Assertion.AssertEquals ("test#02", "DIT.Name.Test", info.Name);
        			
        			
        		} finally {
        			DeleteDir (path);
        		}        		           
        	}
        	
        	[Test]
        	public void Parent ()
        	{
        		string path = TempFolder + "/DIT.Parent.Test";
        		DeleteDir (path);
        		
        		try {
        			DirectoryInfo info = new DirectoryInfo (path);
        			Assertion.AssertEquals ("test#01", "MonoTests.System.IO.Tests", info.Parent.Name);
        			
        			info = Directory.CreateDirectory (path);
        			Assertion.AssertEquals ("test#02", "MonoTests.System.IO.Tests", info.Parent.Name);
        			        			
        		} finally {
        			DeleteDir (path);
        		}        		                   	
        	}

        	[Test]
        	public void Create ()
        	{
            	string path = TempFolder + "/DIT.Create.Test";
            	DeleteDir (path);
            
            	try {
                	DirectoryInfo info = new DirectoryInfo (path);
                	Assertion.AssertEquals ("test#01", false, info.Exists);
                	info.Create ();                
                	Assertion.AssertEquals ("test#02", false, info.Exists);
                	info = new DirectoryInfo (path);
                	Assertion.AssertEquals ("test#03", true, info.Exists);
            	} finally {
                	DeleteDir (path);
            	}
        	}

		[Test]
		public void Delete1 ()
		{
            	string path = TempFolder + "/DIT.Delete1.Test";
            	DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				DirectoryInfo info = new DirectoryInfo (path);
				Assertion.AssertEquals ("test#01", true, info.Exists);
				
				info.Delete ();
				Assertion.AssertEquals ("test#02", true, info.Exists);
				
				info = new DirectoryInfo (path);
				Assertion.AssertEquals ("test#03", false, info.Exists);
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void Delete2 ()
		{
            	string path = TempFolder + "/DIT.Delete2.Test";
            	DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + "/test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				Assertion.AssertEquals ("test#01", true, info.Exists);
				
				info.Delete (true);
				Assertion.AssertEquals ("test#02", true, info.Exists);
				
				info = new DirectoryInfo (path);
				Assertion.AssertEquals ("test#03", false, info.Exists);
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (IOException))]
		public void DeleteIOException1 ()
		{
            	string path = TempFolder + "/DIT.DeleteIOException1.Test";
            	DeleteDir (path);			
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + "/test").Close ();
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
            	string path = TempFolder + "/DIT.DeleteIOException2.Test";
            	DeleteDir (path);			
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + "/test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				info.Delete (false);
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetDirectories1 ()
		{
			string path = TempFolder + "/DIT.GetDirectories1.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assertion.AssertEquals ("test#01", 0, info.GetDirectories ().Length);
				
				Directory.CreateDirectory (path + "/" + "1");
				Directory.CreateDirectory (path + "/" + "2");				
				File.Create (path + "/" + "filetest").Close ();
				Assertion.AssertEquals ("test#02", 2, info.GetDirectories ().Length);
				
				Directory.Delete (path + "/" + 2);
				Assertion.AssertEquals ("test#02", 1, info.GetDirectories ().Length);				
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetDirectories2 ()
		{
			string path = TempFolder + "/DIT.GetDirectories2.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assertion.AssertEquals ("test#01", 0, info.GetDirectories ("*").Length);
				
				Directory.CreateDirectory (path + "/" + "test120");
				Directory.CreateDirectory (path + "/" + "test210");
				Directory.CreateDirectory (path + "/" + "atest330");
				Directory.CreateDirectory (path + "/" + "test220");
				File.Create (path + "/" + "filetest").Close ();
				
				Assertion.AssertEquals ("test#02", 4, info.GetDirectories ("*").Length);
				Assertion.AssertEquals ("test#03", 3, info.GetDirectories ("test*").Length);
				Assertion.AssertEquals ("test#04", 2, info.GetDirectories ("test?20").Length);
				Assertion.AssertEquals ("test#05", 0, info.GetDirectories ("test?").Length);
				Assertion.AssertEquals ("test#06", 0, info.GetDirectories ("test[12]*").Length);
				Assertion.AssertEquals ("test#07", 2, info.GetDirectories ("test2*0").Length);
				Assertion.AssertEquals ("test#08", 4, info.GetDirectories ("*test*").Length);
				
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]		
		public void GetDirectoriesDirectoryNotFoundException1 ()
		{
            	string path = TempFolder + "/DIT.GetDirectoriesDirectoryNotFoundException1.Test";
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
            	string path = TempFolder + "/DIT.GetDirectoriesDirectoryNotFoundException2.Test";
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
            	string path = TempFolder + "/DIT.GetDirectoriesArgumentNullException.Test";
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
            	string path = TempFolder + "/DIT.GetFiles1.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assertion.AssertEquals ("test#01", 0, info.GetFiles ().Length);
				File.Create (path + "/" + "file1").Close ();
				File.Create (path + "/" + "file2").Close ();
				Directory.CreateDirectory (path + "/" + "directory1");
				Assertion.AssertEquals ("test#02", 2, info.GetFiles ().Length);
				                        
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void GetFiles2()
		{
            	string path = TempFolder + "/DIT.GetFiles2.Test";
            	DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assertion.AssertEquals ("test#01", 0, info.GetFiles ("*").Length);
				File.Create (path + "/" + "file120file").Close ();
				File.Create (path + "/" + "file220file").Close ();
				File.Create (path + "/" + "afile330file").Close ();
				File.Create (path + "/" + "test.abc").Close ();
				File.Create (path + "/" + "test.abcd").Close ();
				File.Create (path + "/" + "test.abcdef").Close ();				
				Directory.CreateDirectory (path + "/" + "dir");
				
				Assertion.AssertEquals ("test#02", 6, info.GetFiles ("*").Length);
				Assertion.AssertEquals ("test#03", 2, info.GetFiles ("file*file").Length);
				Assertion.AssertEquals ("test#04", 3, info.GetFiles ("*file*").Length);
				Assertion.AssertEquals ("test#05", 2, info.GetFiles ("file?20file").Length);
				Assertion.AssertEquals ("test#07", 1, info.GetFiles ("*.abcd").Length);
				Assertion.AssertEquals ("test#08", 2, info.GetFiles ("*.abcd*").Length);				                        
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void GetFilesDirectoryNotFoundException1 ()
		{
			string path = TempFolder + "/DIT.GetFilesDirectoryNotFoundException1.Test";
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
			string path = TempFolder + "/DIT.GetFilesDirectoryNotFoundException2.Test";
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
			string path = TempFolder + "/DIT.GetFilesArgumentNullException.Test";
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
			string path1 = TempFolder + "/DIT.MoveTo.Soucre.Test";
			string path2 = TempFolder + "/DIT.MoveTo.Dest.Test";
			DeleteDir (path1);
			DeleteDir (path2);
			
			try {
				DirectoryInfo info1 = Directory.CreateDirectory (path1);
				DirectoryInfo info2 = new DirectoryInfo (path2);
				
				Assertion.AssertEquals ("test#01", true, info1.Exists);
				Assertion.AssertEquals ("test#02", false, info2.Exists);
												
				info1.MoveTo (path2);				
				Assertion.AssertEquals ("test#03", true, info1.Exists);
				Assertion.AssertEquals ("test#04", false, info2.Exists);
				
				info1 = new DirectoryInfo (path1);
				info2 = new DirectoryInfo (path2);
				Assertion.AssertEquals ("test#05", false, info1.Exists);
				Assertion.AssertEquals ("test#06", true, info2.Exists);
				
			} finally {
				DeleteDir (path1);
				DeleteDir (path2);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MoveToArgumentNullException ()
		{
			string path = TempFolder + "/DIT.MoveToArgumentNullException.Test";
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
			string path = TempFolder + "/DIT.MoveToIOException1.Test";
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
			string path = TempFolder + "/DIT.MoveToArgumentException1.Test";
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
			string path = TempFolder + "/DIT.MoveToArgumentException2.Test";
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
			string path = TempFolder + "/DIT.MoveToArgumentException3.Test";
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
			string path = TempFolder + "/DIT.MoveToIOException2.Test";
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
