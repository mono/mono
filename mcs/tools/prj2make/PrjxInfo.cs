// Copyright (c) 2004 Francisco T. Martinez <paco@mfcon.com>
// All rights reserved.
//
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
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;


namespace Mfconsulting.General.Prj2Make
{
    class PrjxInfo
    {
    	public readonly string name;
    	public readonly string csprojpath;
    	public string makename;
    	public string makename_ext;
    	public string assembly_name;
    	public string res;
    	public string src;
		private bool m_bAllowUnsafeCode;
    	private Mfconsulting.General.Prj2Make.Schema.Prjx.Project m_projObject;
    
    	public string ext_refs = "";
    	public string switches = "";

		public bool AllowUnsafeCode
		{
			get { return m_bAllowUnsafeCode; }
		}
    	
    	public Mfconsulting.General.Prj2Make.Schema.Prjx.Project Proyecto {
    		get { return m_projObject; }
    	}
    
    	// Project desirialization
		protected Mfconsulting.General.Prj2Make.Schema.Prjx.Project LoadPrjFromFile (string strIn)
		{
			FileStream fs = new FileStream (strIn, FileMode.Open);
	    
			XmlSerializer xmlSer = new XmlSerializer (typeof(Mfconsulting.General.Prj2Make.Schema.Prjx.Project));
			Mfconsulting.General.Prj2Make.Schema.Prjx.Project prjObj = (Mfconsulting.General.Prj2Make.Schema.Prjx.Project) xmlSer.Deserialize (fs);
	    
			fs.Close();
	    
			return (prjObj);
		}

		public PrjxInfo(bool isUnixMode, bool isMcsMode, string csprojpath)
	   	{
			Mfconsulting.General.Prj2Make.Schema.Prjx.Configuration activeConf = null;
    		this.csprojpath = csprojpath;

    		// convert backslashes to slashes    		
    		csprojpath = csprojpath.Replace("\\", "/");
    
		m_bAllowUnsafeCode = false;
		
    		// loads the file in order to deserialize and
    		// build the object graph
    		try {
    			m_projObject = LoadPrjFromFile (csprojpath);
			} catch (Exception exc) {
			
				Console.WriteLine ("Could not load the file {0}\n{1}: {2}",
						csprojpath,
						exc.GetType().Name,
						exc.Message
					);
				return;			
			}

    		this.name = m_projObject.name;

    		makename = name.Replace('.','_').ToUpper();
    		makename_ext = makename + "_EXT";

			// Get the configuration to be used and
			// copy it to a local configuration object
			foreach(Mfconsulting.General.Prj2Make.Schema.Prjx.Configuration cnfObj in m_projObject.Configurations.Configuration)
			{
				if(cnfObj.name.CompareTo(m_projObject.Configurations.active) == 0)
				{
					// Assign the active configuration
					activeConf = cnfObj;
					break;
				}
			}

			// Establish if the allow unsafe code flag is true
			if(activeConf.CodeGeneration.unsafecodeallowed == Mfconsulting.General.Prj2Make.Schema.Prjx.CodeGenerationUnsafecodeallowed.True)
			{
				m_bAllowUnsafeCode = true;
			}
			
    		switch (activeConf.CodeGeneration.target)
    		{
    			case "Library":
    				makename_ext = makename + "_DLL";
    				assembly_name = activeConf.Output.assembly + ".dll";
    				switches += " -target:library";
    				break;
    
    			case "Exe":
    				makename_ext = makename + "_EXE";
    				assembly_name = activeConf.Output.assembly + ".exe";
    				switches += " -target:exe";
    				break;
    
    			case "WinExe":
    				makename_ext = makename + "_EXE";
    				assembly_name = activeConf.Output.assembly + ".exe";
    				switches += " -target:winexe";
    				break;
    
    			default:
    				throw new NotSupportedException("Unsupported OutputType: " + activeConf.CodeGeneration.target);
    			
    		}
    
    		src = "";    
    		string basePath = Path.GetDirectoryName(csprojpath);
    		string s;
    		
			// Process Source code files for compiling
    		foreach (Mfconsulting.General.Prj2Make.Schema.Prjx.File f in m_projObject.Contents)
    		{
    			if(f.buildaction == Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.Compile)
    			{
    				if (src != "") {
	    				src += " \\\n\t";
    				}
    			
    				s = System.IO.Path.Combine(basePath, f.name);
    				s = s.Replace("\\", "/");
    				if (CmbxMaker.slash != "/")
	    				s = s.Replace("/", CmbxMaker.slash);

					// Test for spaces
					if (isUnixMode == false)
					{
						// We are in win32 using a cmd.exe or other
						// DOS shell
						if(s.IndexOf(' ') > -1) {
							src += String.Format("\"{0}\"", s);
						} else {
							src += s;
						}
					} else {
						// We are in *NIX or some other
						// GNU like shell
						src += CsprojInfo.Quote (s);
					}    			
    			}
    		}
    		
    		// Process resources for embedding
    		res = "";
    		string rootNS = this.name; 
    		string relPath;
    		foreach (Mfconsulting.General.Prj2Make.Schema.Prjx.File f in m_projObject.Contents)
    		{
    			if(f.buildaction == Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.EmbedAsResource)
    			{
    				if (src != "") {
	    				src += " \\\n\t";
    				}
    			
	    			relPath = f.name.Replace("\\", "/");
	    			s = System.IO.Path.Combine(basePath, relPath);
	    			s = String.Format(" -resource:{0},{1}", s, System.IO.Path.GetFileName(relPath));
	    			s = s.Replace("\\", "/");
	    			if (CmbxMaker.slash != "/")
	    				s = s.Replace("/", CmbxMaker.slash);
	    			res += s;
    			}
    		}
    	}
    }    
}
