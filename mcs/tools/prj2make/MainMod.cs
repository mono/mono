// created on 4/3/04 at 6:27 a

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Library General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

using System;
using System.IO;
using Mfconsulting.General.Prj2Make;

namespace Mfconsulting.General.Prj2Make.Cui
{
    public class MainMod 
    {
    	public string m_AppName = "prj2make-sharp";
    	public string m_WorkspaceFileName;
    	public string m_FileNamePath;
    	public string m_OutputMakefile;
    	
    	private bool m_IsUnix = false;
    	private bool m_IsMcs = true;
    	
    	// Determines the make file type/style
    	public bool IsUnix
    	{
    		get { return m_IsUnix; }
    		set { m_IsUnix = value; }
    	}
    	
    	// Determines what compiler to use MCS or CSC
    	public bool IsMcs
    	{
    		get { return m_IsMcs; }
    		set { m_IsMcs = value; }
    	}
    	
    	protected void MyInitializeComponents()
    	{
    	}
    	
		public MainMod (string inputFileName)
		{
			Mfconsulting.General.Prj2Make.Maker mkObj = null;

			MyInitializeComponents();
    		
			if (inputFileName == null || inputFileName.Length < 1) 
			{
				Console.WriteLine ("No input file has been specified.");
				return;            
			}

			if (Path.GetExtension(inputFileName).ToUpper().CompareTo(".SLN") == 0)
			{
				mkObj = new Mfconsulting.General.Prj2Make.Maker();
				mkObj.CreateCombineFromSln(inputFileName);
				return;
			}

			if (Path.GetExtension(inputFileName).ToUpper().CompareTo(".CSPROJ") == 0)
			{
				mkObj = new Mfconsulting.General.Prj2Make.Maker();
				mkObj.CreatePrjxFromCsproj(inputFileName);
				return;
			}
   		}
		
		// For command line handling
    	public MainMod (bool isNmake, bool isCsc, string WorkspaceFile)
    	{
    		MyInitializeComponents();
    		this.IsMcs = (isCsc == true) ? false : true;
    		this.IsUnix = (isNmake == true) ? false : true;
    		IniFromCommandLine(WorkspaceFile);
    	}
   	
    	void IniFromCommandLine(string WorkspaceFilename)
    	{
       		m_WorkspaceFileName = WorkspaceFilename;
       		m_FileNamePath = System.IO.Path.GetDirectoryName(m_WorkspaceFileName);
       		m_OutputMakefile = System.IO.Path.Combine(m_FileNamePath, "Makefile");
       
       		if (this.IsUnix)
       			m_OutputMakefile = System.IO.Path.Combine(m_FileNamePath, "Makefile");
       		else
       			m_OutputMakefile = System.IO.Path.Combine(m_FileNamePath, "Makefile.Win32");
     
       		// Actually follow through and genrate the contents of the Makefile
       		// to be placed on the textview
       		CreateMakefile();
    	}


    	protected void CreateMakefile()
    	{
    		Mfconsulting.General.Prj2Make.Maker mkObj = null;
    		string slnFile = null;
    		
    		if (this.m_WorkspaceFileName.Length < 1) {
    			Console.WriteLine ("No input file has been specified.");
    			return;            
    		}
    		else
    			slnFile = m_WorkspaceFileName;
    		
    		mkObj = new Mfconsulting.General.Prj2Make.Maker();		
    		SaveMakefileToDisk(mkObj.MakerMain(IsUnix, IsMcs, slnFile));
    	}
    
    	protected void SaveMakefileToDisk (string MakeFileContents)
    	{
    		FileStream fs = null;
    		StreamWriter w = null;
    		
    		if (MakeFileContents.StartsWith ("Error") || MakeFileContents.StartsWith ("Notice")) {
    			Console.WriteLine(MakeFileContents);
    			return;    		
			} 
    		
    		if (m_OutputMakefile != null && m_OutputMakefile.Length > 1) {
    			fs = new FileStream(m_OutputMakefile, FileMode.Create, FileAccess.Write);
    			w = new StreamWriter(fs);
    		}
    		
    		if (w != null) {
    			w.WriteLine (MakeFileContents);
    			w.Close();
    		}
    	}
    }    
}