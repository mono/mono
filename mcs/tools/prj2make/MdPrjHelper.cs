using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Mfconsulting.General.Prj2Make
{
    public class CmbxMaker
    {
		public static string slash;
		static Hashtable projNameInfo = new Hashtable();
		static Hashtable projGuidInfo = new Hashtable();
		private Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine m_cmbObject;
		private bool m_bIsUnix;
		private bool m_bIsMcs;
		private bool m_bIsUsingLib;
 
		// Flag use to determine if the LIB variable will be used in
		// the Makefile that prj2make generates
		public bool IsUsingLib
		{
			get{ return m_bIsUsingLib; }
			set{ m_bIsUsingLib = value; }
		}

		// Determines if the makefile is intended for nmake or gmake for urposes of path separator character
		public bool IsUnix
		{
			get{ return m_bIsUnix; }
			set{ m_bIsUnix = value; }
		}

		// Determines if using MCS or CSC
		public bool IsMcs
		{
			get{ return m_bIsMcs; }
			set{ m_bIsMcs = value; }
		}

	    public Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine Solucion {
		    get { return m_cmbObject; }
	    }

		public CmbxMaker()
		{
			m_bIsUnix = false;
			m_bIsMcs = false;
			m_bIsUsingLib = false;
		}
    
		// Combine desirialization 
		protected Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine LoadCmbFromFile (string strIn)
		{
			FileStream fs = new FileStream (strIn, FileMode.Open);
	    
			XmlSerializer xmlSer = new XmlSerializer (typeof(Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine));
			Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine cmbObj = (Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine) xmlSer.Deserialize (fs);
	    
			fs.Close();
	    
			return (cmbObj);
		}

   		protected void ParseMdCsProj(string fname)
    	{
    		string projectName;
    		
            PrjxInfo pi = new PrjxInfo(m_bIsUnix, m_bIsMcs, fname);
            projectName = pi.name;            
			projNameInfo[projectName] = pi;            
    	}
    	
    	protected void ParseCombine(string fname)
    	{
    		string CombineFilePath = fname;
    		
    		// convert backslashes to slashes    		
    		CombineFilePath = CombineFilePath.Replace("\\", "/");
    
    		// loads the file in order to deserialize and
    		// build the object graph
    		try {
    			m_cmbObject = LoadCmbFromFile (CombineFilePath);
			} catch (Exception exc) {
			
				Console.WriteLine (
					String.Format ("Could not load the file {0}\nException: {1}",
						CombineFilePath,
						exc.Message)
					);
				return;			
			}

    		foreach(Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry ent in m_cmbObject.Entries)
    		{
    				string projectName = System.IO.Path.GetFileNameWithoutExtension(ent.filename);
    				string csprojPath = ent.filename;
    
    				if (csprojPath.EndsWith(".prjx"))
    				{
    					PrjxInfo pi = new PrjxInfo(m_bIsUnix, m_bIsMcs, csprojPath);
    
    					projNameInfo[projectName] = pi;
    				}    
    		}
    	}
    
    	public string MdCmbxHelper(bool isUnixMode, bool isMcsMode, bool isSln, string slnFile)
    	{
    		bool noCommonTargets = false;
    		bool noProjectTargets = false;
    		bool noFlags = false;
			int nPos = -1;
    		StringBuilder MakefileBuilder = new StringBuilder();

			m_bIsUnix = isUnixMode;
			m_bIsMcs =  isMcsMode;

			if(m_bIsUnix == true && m_bIsMcs == true)
			{
				m_bIsUsingLib = true;
			}
    
    		if (m_bIsUnix)
    		{
    			slash = "/";
    		}
    		else
    		{
    			slash = "\\";
    		}
    
    		try
    		{
    			string d = Path.GetDirectoryName(slnFile);
    			
    			if (d != "")
    				Directory.SetCurrentDirectory(d);

    			if (isSln == true) {
        				// We invoke the ParseSolution 
        				// by passing the file obtained
        				ParseCombine (slnFile);
    			} else {
    
        				// We invoke the ParseMsCsProj 
        				// by passing the file obtained 
    					ParseMdCsProj (slnFile);
    			}
        
    			if (!noFlags)
    			{
            		if (m_bIsUnix) // gmake
            		{
           				MakefileBuilder.Append("ifndef TARGET\n");
           				MakefileBuilder.Append("\tTARGET=./bin/Debug\n");        				
           				MakefileBuilder.Append("else\n");
           				MakefileBuilder.Append("\tTARGET=./bin/$(TARGET)\n");
           				MakefileBuilder.Append("endif\n\n");
           				
        				if (m_bIsMcs == false)
        				{
        					MakefileBuilder.Append("MCS=csc\n");
        					MakefileBuilder.Append("MCSFLAGS=-nologo\n\n");
        					MakefileBuilder.Append("ifdef (RELEASE)\n");
        					MakefileBuilder.Append("\tMCSFLAGS=$(MCSFLAGS) -optimize+ -d:TRACE\n");
        					MakefileBuilder.Append("else\n");
        					MakefileBuilder.Append("\tMCSFLAGS=$(MCSFLAGS) -debug+ -d:TRACE,DEBUG\n");
        					MakefileBuilder.Append("endif\n");
        				}
        				else
        				{
        					MakefileBuilder.Append("MCS=mcs\n");
        					MakefileBuilder.Append("ifndef (RELEASE)\n");
        					MakefileBuilder.Append("\tMCSFLAGS=-debug \n");
        					MakefileBuilder.Append("endif\n");
							// Define and add the information used in the -lib: arguments passed to the
							// compiler to assist in finding non-fullyqualified assembly references.
							if(m_bIsUsingLib == true)
							{
								string strlibDir = PkgConfigInvoker.GetPkgVariableValue("mono", "libdir");

								if (strlibDir == null)
								{
									strlibDir = "/usr/lib";
								}

								MakefileBuilder.AppendFormat("\nLIBS=-lib:{0} -lib:{1}\n\n", 
									Utils.Escape (Path.Combine(strlibDir.TrimEnd(), "mono/1.0")),
									Utils.Escape (Path.Combine(strlibDir.TrimEnd(), "mono/gtk-sharp"))
									);
							}
        				}        		
            		}
            		else // nmake
            		{
           				MakefileBuilder.Append("!if !defined (TARGET)\n");
           				MakefileBuilder.Append("TARGET=.\\bin\\Debug\n");        				
           				MakefileBuilder.Append("!else\n");
           				MakefileBuilder.Append("TARGET=.\\bin\\$(TARGET)\n");
           				MakefileBuilder.Append("!endif\n\n");
           				
        				if (m_bIsMcs == false)
        				{
        					MakefileBuilder.Append("MCS=csc\n");
        					MakefileBuilder.Append("MCSFLAGS=-nologo\n\n");
        					MakefileBuilder.Append("!if !defined(RELEASE)\n");
        					MakefileBuilder.Append("MCSFLAGS=$(MCSFLAGS) -optimize+ -d:TRACE\n");
        					MakefileBuilder.Append("!else\n");
        					MakefileBuilder.Append("MCSFLAGS=$(MCSFLAGS) -debug+ -d:TRACE,DEBUG\n");
        					MakefileBuilder.Append("!endif\n");
        				}
        				else
        				{
        					MakefileBuilder.Append("MCS=mcs\n");
        					MakefileBuilder.Append("!if !defined(RELEASE)\n");
        					MakefileBuilder.Append("MCSFLAGS=-debug \n");
        					MakefileBuilder.Append("!endif\n");
        				}    				
            		}
    
    				MakefileBuilder.Append("\n");
    			}
    			else
    			{
    				MakefileBuilder.Append("!if !defined(MCS)\n");
    				MakefileBuilder.Append("!error You must provide MCS when making\n");
    				MakefileBuilder.Append("!endif\n\n");
    			}
    
    			foreach (PrjxInfo prjI in projNameInfo.Values)
    			{
    				MakefileBuilder.AppendFormat("{0}=$(TARGET){1}{2}\n", prjI.makename_ext, slash, prjI.assembly_name);
    				MakefileBuilder.AppendFormat("{0}_PDB=$(TARGET){1}{2}\n", prjI.makename, slash, prjI.assembly_name.Replace(".dll",".pdb"));
    				MakefileBuilder.AppendFormat("{0}_SRC={1}\n", prjI.makename, prjI.src);
    				MakefileBuilder.AppendFormat("{0}_RES={1}\n\n", prjI.makename, prjI.res);
    			}

    
    			foreach (PrjxInfo pi in projNameInfo.Values)
    			{
    				string refs = "";
    				string deps = "";
    				
    				foreach (Mfconsulting.General.Prj2Make.Schema.Prjx.Reference rf in pi.Proyecto.References)
    				{
   						if (refs != "")
   							refs += " ";
   
   						string assemblyName = rf.refto;
   
   						// HACK - under Unix filenames are case sensitive
   						// Under Windows there's no agreement on Xml vs XML ;-)   						
   						if (0 == String.Compare(assemblyName, "System.Xml", true))
   						{
   							assemblyName = "System.Xml";
   						}

						// Check to see if there is a coma in the
						// reference. This could indicate a GAC
						// style reference
						nPos = assemblyName.IndexOf(',');
						if(nPos == -1)
						{
							if (System.IO.Path.GetExtension(assemblyName).ToUpper().CompareTo(".DLL") == 0) 
							{
								refs += "-r:" + assemblyName;
							} 
							else 
							{
								refs += "-r:" + assemblyName + ".dll";
							}
						}
						else
						{
							refs += "-r:" + assemblyName.Substring(0, nPos) + ".dll";
						}
    				}
    
    				MakefileBuilder.AppendFormat("$({0}): $({1}_SRC) {2}\n", pi.makename_ext, pi.makename, deps);
    		
            		if (isUnixMode)
            		{
    					MakefileBuilder.Append("\t-mkdir -p $(TARGET)\n");
            		}
            		else
            		{
    					MakefileBuilder.Append("\t-md $(TARGET)\n");
            		}
            		
					MakefileBuilder.Append("\t$(MCS) $(MCSFLAGS)");

					// Test to see if any configuratino has the Allow unsafe blocks on
					if(pi.AllowUnsafeCode == true ) {
						MakefileBuilder.Append(" -unsafe");
					}

					// Test for LIBS usage
					if(m_bIsUsingLib == true) {
	    				MakefileBuilder.Append(" $(LIBS)");
					}

					MakefileBuilder.AppendFormat(" {2}{3} -out:$({0}) $({1}_RES) $({1}_SRC)\n", 
							pi.makename_ext, pi.makename, refs, pi.switches);
            								
					MakefileBuilder.Append("\n");
    			}
    
    			if (!noCommonTargets)
    			{
    				MakefileBuilder.Append("\n");
    				MakefileBuilder.Append("# common targets\n\n");
    				MakefileBuilder.Append("all:\t");
    
    				bool first = true;
    
    				foreach (PrjxInfo pi in projNameInfo.Values)
    				{
    					if (!first)
    					{
    						MakefileBuilder.Append(" \\\n\t");
    					}
    					MakefileBuilder.AppendFormat("$({0})", pi.makename_ext);
    					first = false;
    				}
    				MakefileBuilder.Append("\n\n");
    
    				MakefileBuilder.Append("clean:\n");
    
    				foreach (PrjxInfo pi in projNameInfo.Values)
    				{
    					if (isUnixMode)
    					{
    						MakefileBuilder.AppendFormat("\t-rm -f \"$({0})\" 2> /dev/null\n", pi.makename_ext);
    						MakefileBuilder.AppendFormat("\t-rm -f \"$({0}_PDB)\" 2> /dev/null\n", pi.makename);
    					}
    					else
    					{
    						MakefileBuilder.AppendFormat("\t-del \"$({0})\" 2> nul\n", pi.makename_ext);
    						MakefileBuilder.AppendFormat("\t-del \"$({0}_PDB)\" 2> nul\n", pi.makename);
    					}
    				}
    				MakefileBuilder.Append("\n");
    			}
    
    			if (!noProjectTargets)
    			{
    				MakefileBuilder.Append("\n");
    				MakefileBuilder.Append("# project names as targets\n\n");
    				foreach (PrjxInfo pi in projNameInfo.Values)
    				{
    					MakefileBuilder.AppendFormat("{0}: $({1})\n", pi.name, pi.makename_ext);
    				}
    			}
    		}
    		catch (Exception e)
    		{
    			Console.WriteLine("EXCEPTION: {0}\n", e);
    			return "";
    		}
    		
   			return MakefileBuilder.ToString();
    	}
    }    
}
