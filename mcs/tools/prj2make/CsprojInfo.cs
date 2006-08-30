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
	class CsprojInfo
	{
		public readonly string name;
		public readonly string guid;
		public readonly string csprojpath;
		public string makename;
		public string makename_ext;
		public string assembly_name;
		public string res;
		public string resgen;
		public string src;
		private bool m_bAllowUnsafeCode;
		private Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject m_projObject;
    
		public string ext_refs = "";
		public string switches = "";

		public bool AllowUnsafeCode
		{
			get { return m_bAllowUnsafeCode; }
		}
    
		public ArrayList libdirs = new ArrayList();
		
		public Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject Proyecto 
		{
			get { return m_projObject; }
		}
    
		// Project desirialization
		protected Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject LoadPrjFromFile (string strIn)
		{
			FileStream fs = new FileStream (strIn, FileMode.Open, FileAccess.Read);
	    
			XmlSerializer xmlSer = new XmlSerializer (typeof(Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject));
			Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject prjObj = (Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject) xmlSer.Deserialize (fs);
	    
			fs.Close();
	    
			return (prjObj);
		}

		// Character to quote
		
		static char [] quotable = new char [] { ' ', '(', ')' };

		static public string Quote (string s)
		{
			if (s.IndexOfAny (quotable) == -1)
				return s;
			else {
				StringBuilder sb = new StringBuilder ();
				foreach (char c in s){
					switch (c){
					case ' ': case '(': case ')':
						sb.Append ('\\');
						break;
					}
					sb.Append (c);
				}
				return sb.ToString ();
			}
		}
		
		public CsprojInfo(bool isUnixMode, bool isMcsMode, string name, string guid, string csprojpath)
		{
			this.name = name;
			this.guid = guid;
			this.csprojpath = csprojpath;
    
			makename = name.Replace('.','_').ToUpper();
			makename_ext = makename + "_EXT";
			m_bAllowUnsafeCode = false;
    
			// convert backslashes to slashes    		
			csprojpath = csprojpath.Replace("\\", "/");

			// loads the file in order to deserialize and
			// build the object graph
			try 
			{
				m_projObject = LoadPrjFromFile (csprojpath);
			} 
			catch (Exception exc) 
			{
				Console.WriteLine("CurrentDirectory={0}", Directory.GetCurrentDirectory());
				Console.WriteLine (
					"Could not load the file {0}\n{1}: {2}\n{3}",
					csprojpath,
					exc.GetType().Name,
					exc.Message,
					exc.StackTrace
					);
				return;			
			}

			// Establish if the allow unsafe code flag is true
			foreach (Mfconsulting.General.Prj2Make.Schema.Csproj.Config cf in m_projObject.CSHARP.Build.Settings.Config)
			{
				if(cf.AllowUnsafeBlocks == true)
					m_bAllowUnsafeCode = true;
			}
    		
			switch (m_projObject.CSHARP.Build.Settings.OutputType)
			{
				case "Library":
					makename_ext = makename + "_DLL";
					assembly_name = m_projObject.CSHARP.Build.Settings.AssemblyName + ".dll";
					switches += " -target:library";
					break;
    
				case "Exe":
					makename_ext = makename + "_EXE";
					assembly_name = m_projObject.CSHARP.Build.Settings.AssemblyName + ".exe";
					switches += " -target:exe";
					break;
    
				case "WinExe":
					makename_ext = makename + "_EXE";
					assembly_name = m_projObject.CSHARP.Build.Settings.AssemblyName + ".exe";
					switches += " -target:winexe";
					break;
    
				default:
					throw new NotSupportedException("Unsupported OutputType: " + m_projObject.CSHARP.Build.Settings.OutputType);
    			
			}
    
			src = "";    
			string basePath = Path.GetDirectoryName(csprojpath);
			string s;
    
			foreach (Mfconsulting.General.Prj2Make.Schema.Csproj.File fl in m_projObject.CSHARP.Files.Include)
			{
				if(fl.BuildAction == Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.Compile)
				{
					if (src != "")
					{
						src += " \\\n\t";
					}
	    			
					string filePath = fl.Link;
					if (filePath == null)
						filePath = fl.RelPath;
	    			
					s = System.IO.Path.Combine(basePath, filePath);
					s = s.Replace("\\", "/");
					if (SlnMaker.slash != "/")
						s = s.Replace("/", SlnMaker.slash);
	
					// Test for spaces
					if (isUnixMode == false) {
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
						src += s.Replace(" ", "\\ ");
					}
				}    			
			}
    		
			res = "";
			resgen = "";
			string rootNS = m_projObject.CSHARP.Build.Settings.RootNamespace;
			string relPath;
			foreach (Mfconsulting.General.Prj2Make.Schema.Csproj.File fl in m_projObject.CSHARP.Files.Include)
			{
				if(fl.BuildAction == Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.EmbeddedResource)
				{
					if (res != "") {
						res += " \\\n\t";
					}
    			
					relPath = fl.RelPath.Replace("\\", "/");
					s = System.IO.Path.Combine(basePath, relPath);
					if (Path.GetExtension (s) == ".resx") {
						string path = s;
						path = path.Replace (@"\", "/");
						if (SlnMaker.slash != "/")
							path = path.Replace("/", SlnMaker.slash);
						resgen += String.Format ("{0} ", Quote (path));
						s = Path.ChangeExtension (s, ".resources");
						relPath = Path.ChangeExtension (relPath, ".resources");
					}
					s = s.Replace("\\", "/");
	                                string rp = relPath.Replace ("/", ".").Replace ("\\", "/");
					s = String.Format("-resource:{0},{1}", Quote (s), rootNS + "." + rp);
					if (SlnMaker.slash != "/")
						s = s.Replace("/", SlnMaker.slash);
					res += s;
				}
			} 		
		}
	}    
}
