using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Mfconsulting.General.Prj2Make.Schema.Prjx;
using Mfconsulting.General.Prj2Make.Schema.Csproj;

namespace Mfconsulting.General.Prj2Make
{
	public class SlnMaker
	{ 
		public static string slash;
		static Hashtable projNameInfo = new Hashtable();
		static Hashtable projGuidInfo = new Hashtable();
		private string prjxFileName = null;
		private string cmbxFileName = null;
		private string m_strSlnVer;
		private string m_strCsprojVer;
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

		public string SlnVersion
		{
			get { return m_strSlnVer; }
			set { m_strSlnVer = value; }
		}

		public string CsprojVersion
		{
			get { return m_strCsprojVer; }
			set { m_strCsprojVer = value; }
		}

		// Shuld contain the file name 
		// of the most resent prjx generation
		public string PrjxFileName {
			get { return prjxFileName; }
		}

		// Shuld contain the file name 
		// of the most resent cmbx generation
		public string CmbxFileName {
			get { return cmbxFileName; }
		}

		// Default constructor
		public SlnMaker()
		{
			m_bIsUnix = false;
			m_bIsMcs = false;
			m_bIsUsingLib = false;
		}

		// Utility function to determine the sln file version
		protected string GetSlnFileVersion(string strInSlnFile)
		{
			string strVersion = null;
			string strInput = null;
			Match match;
			FileStream fis = new FileStream(strInSlnFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			StreamReader reader = new StreamReader(fis);
			Regex regex = new Regex(@"Microsoft Visual Studio Solution File, Format Version (\d.\d\d)");
			
			strInput = reader.ReadLine();

			match = regex.Match(strInput);
			if (match.Success)
			{
				strVersion = match.Groups[1].Value;
			}
			
			// Close the stream
			reader.Close();

			// Close the File Stream
			fis.Close();
    
			return strVersion;
		}
    	
		// Utility function to determine the csproj file version
		protected string GetCsprojFileVersion(string strInCsprojFile)
		{
			string strRetVal = null;
			XmlDocument xmlDoc = new XmlDocument();

			xmlDoc.Load(strInCsprojFile);
			strRetVal = xmlDoc.SelectSingleNode("/VisualStudioProject/CSHARP/@ProductVersion").Value;

			return strRetVal;
		}

		protected void ParseMsCsProj(string fname)
		{
			string projectName = System.IO.Path.GetFileNameWithoutExtension (fname);
			string csprojPath = System.IO.Path.GetFileName (fname);
			string projectGuid = "";
            
			CsprojInfo pi = new CsprojInfo (m_bIsUnix, m_bIsMcs, projectName, projectGuid, csprojPath);
            
			projNameInfo[projectName] = pi;
			projGuidInfo[projectGuid] = pi;
		}

		protected void ParseSolution(string fname)
		{
			FileStream fis = new FileStream(fname,FileMode.Open, FileAccess.Read, FileShare.Read);
			StreamReader reader = new StreamReader(fis);
			Regex regex = new Regex(@"Project\(""\{(.*)\}""\) = ""(.*)"", ""(.*)"", ""(\{.*\})""");
			
			while (true)
			{
				string s = reader.ReadLine();
				Match match;
    
				match = regex.Match(s);
				if (match.Success)
				{
					string projectName = match.Groups[2].Value;
					string csprojPath = match.Groups[3].Value;
					string projectGuid = match.Groups[4].Value;
    
					if (csprojPath.StartsWith("http://"))
					{
						Console.WriteLine("WARNING: got http:// project, guessing actual path.");
						csprojPath = Path.Combine(projectName, Path.GetFileName(csprojPath));
					}
					if (csprojPath.EndsWith (".csproj"))
 					{
						CsprojInfo pi = new CsprojInfo (m_bIsUnix, m_bIsMcs, projectName, projectGuid, csprojPath);
    
						projNameInfo[projectName] = pi;
						projGuidInfo[projectGuid] = pi;
					}
				}
    
				if (s.StartsWith("Global"))
				{
					break;
				}
			}
		}

		private bool CheckReference(string referenceFilename)
		{
			foreach (CsprojInfo pi in projNameInfo.Values)
			{
				if (referenceFilename.ToLower() == pi.name.ToLower())
				{
					return true;
				}
			}
			return false;
		}
    
		public string MsSlnHelper(bool isUnixMode, bool isMcsMode, bool isSln, string slnFile)
		{
			bool noCommonTargets = false;
			bool noProjectTargets = false;
			bool noFlags = false;
			StringBuilder MakefileBuilder = new StringBuilder();
    
			m_bIsUnix = isUnixMode;
			m_bIsMcs = isMcsMode;
			
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

				if (isSln == true) 
				{
					// Get the sln file version
					m_strSlnVer = GetSlnFileVersion(slnFile);

					// We invoke the ParseSolution 
					// by passing the file obtained
					ParseSolution (slnFile);
				} 
				else 
				{
					// Get the Csproj version
					m_strCsprojVer = GetCsprojFileVersion(slnFile);

					// We invoke the ParseMsCsProj 
					// by passing the file obtained 
					ParseMsCsProj (slnFile);
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
           				
						if (this.m_bIsMcs == false)
						{
							MakefileBuilder.Append("MCS=csc\n");
							MakefileBuilder.Append("RESGEN=resgen\n");
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
							MakefileBuilder.Append("RESGEN=resgen\n");
							MakefileBuilder.Append("ifndef (RELEASE)\n");
							MakefileBuilder.Append("\tMCSFLAGS=-debug \n");
							MakefileBuilder.Append("endif\n");
							// Define and add the information used in the -lib: arguments passed to the
							// compiler to assist in finding non-fullyqualified assembly references.
							if(m_bIsMcs == true)
							{
								string strlibDir = PkgConfigInvoker.GetPkgVariableValue("mono", "libdir");

								if (strlibDir == null)
								{
									strlibDir = "/usr/lib";
								}							
								
								MakefileBuilder.AppendFormat("LIBS=-lib:{0} -lib:{1}\n\n", 
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
							MakefileBuilder.Append("RESGEN=resgen\n");
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
							MakefileBuilder.Append("RESGEN=resgen\n");
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
    
				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					MakefileBuilder.AppendFormat("{0}=$(TARGET){1}{2}\n", pi.makename_ext, slash, pi.assembly_name);
					MakefileBuilder.AppendFormat("{0}_PDB=$(TARGET){1}{2}\n", pi.makename, slash, pi.assembly_name.Replace(".dll",".pdb"));
					MakefileBuilder.AppendFormat("{0}_SRC={1}\n", pi.makename, pi.src);
					MakefileBuilder.AppendFormat("{0}_RESX={1}\n", pi.makename, pi.resgen);
					MakefileBuilder.AppendFormat("{0}_RES={1}\n\n", pi.makename, pi.res);
				}
				
     
				MakefileBuilder.Append("all: ");

				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					MakefileBuilder.AppendFormat("\\\n$({0})", pi.makename_ext);
				}
				
				MakefileBuilder.Append("\n");

				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					string refs = "";
					string deps = "";
					
					Hashtable libdirs = new Hashtable();
    					
					foreach (Mfconsulting.General.Prj2Make.Schema.Csproj.Reference rf in pi.Proyecto.CSHARP.Build.References)
					{
						if(rf.Package == null || rf.Package.CompareTo("") == 0)
						{
							// Add space in between references as
							// it becomes necessary
							if (refs != "")
								refs += " ";

							string assemblyName = rf.AssemblyName;
							if (rf.HintPath != null) 
							{
								string potentialPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(pi.csprojpath), rf.HintPath));
								if (isUnixMode)
									potentialPath = potentialPath.Replace('\\', '/');
								if (System.IO.File.Exists(potentialPath)) 
								{
									// assemblyName = potentialPath;
									pi.libdirs.Add(Path.GetDirectoryName(potentialPath));
								}
							}

							// HACK - under Unix filenames are case sensitive
							// Under Windows there's no agreement on Xml vs XML ;-)    					
							if (0 == String.Compare(assemblyName, "System.Xml", true))
							{
								assemblyName = "System.Xml";
							}

							// Check if this assembly is actually one compiled earlier in the project.
							// If so we must add the TARGET path to this line.
							string thisref;
							if (CheckReference(rf.AssemblyName) == true)
							{
								thisref = "-r:$(TARGET)" + Path.DirectorySeparatorChar + assemblyName;
							}
							else
							{
								thisref = "-r:" + assemblyName;
							}
							
							if (!thisref.EndsWith(".dll"))
								thisref += ".dll";
							refs += thisref;
						}
						else
						{
							try
							{
								CsprojInfo pi2 = (CsprojInfo)projGuidInfo[rf.Project];

								if (refs != "")
									refs += " ";

								if (deps != "")
									deps += " ";

								refs += "-r:$(" + pi2.makename_ext + ")";
								deps += "$(" + pi2.makename_ext + ")";
								
								foreach (string libdir in pi2.libdirs)
								{
									if (!libdirs.ContainsKey(libdir))
										libdirs[libdir] = 1;
								}
								
							}
							catch(System.NullReferenceException)
							{
								refs += String.Format("-r:{0}.dll", rf.Name);
								deps += String.Format("# Missing dependency project {1} ID:{0}?", 
								rf.Project,
								rf.Name
								);
								
								
								Console.WriteLine(String.Format(
									"Warning: The project {0}, ID: {1} may be required and appears missing.",
									rf.Name, rf.Project)
									);
							}
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

					if (pi.resgen != null && pi.resgen != String.Empty)
						MakefileBuilder.AppendFormat ("\t$(RESGEN) /compile {0}\n", pi.resgen);

					MakefileBuilder.Append("\t$(MCS) $(MCSFLAGS)");

					foreach (string libdir in libdirs.Keys)
					{
						MakefileBuilder.AppendFormat(" -lib:{0}", Utils.Escape (libdir));
					}
					
					foreach (string libdir in pi.libdirs) 
					{
						MakefileBuilder.AppendFormat(" -lib:{0}", Utils.Escape (libdir));
					}
					
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
    
					foreach (CsprojInfo pi in projNameInfo.Values)
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
    
					foreach (CsprojInfo pi in projNameInfo.Values)
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
					foreach (CsprojInfo pi in projNameInfo.Values)
					{
						MakefileBuilder.AppendFormat("{0}: $({1})\n", pi.name, pi.makename_ext);
					}
				}
    			
				return MakefileBuilder.ToString();
			}
			catch (Exception e)
			{
				Console.WriteLine("EXCEPTION: {0}\n", e);
				return "";
			}
		}
		
		public void CreatePrjxFromCsproj(string csprojFileName)
		{
			int nCnt = 0;
			FileStream fsIn = null;
			XmlSerializer xmlDeSer = null;
			FileStream fsOut = null;
			XmlSerializer xmlSer = null;
			Mfconsulting.General.Prj2Make.Schema.Csproj.VisualStudioProject csprojObj = null;
			Mfconsulting.General.Prj2Make.Schema.Prjx.Project prjxObj = new Mfconsulting.General.Prj2Make.Schema.Prjx.Project();
			string PrjxFileName = String.Format ("{0}.prjx",
				Path.Combine(Path.GetDirectoryName(csprojFileName),
				Path.GetFileNameWithoutExtension(csprojFileName))
				);
			
			// convert backslashes to slashes    		
			csprojFileName = csprojFileName.Replace("\\", "/");			
			Console.WriteLine(String.Format("Will create project filename:{0}", PrjxFileName));

			// Load the csproj
			fsIn = new FileStream (csprojFileName, FileMode.Open);	    
			xmlDeSer = new XmlSerializer (typeof(VisualStudioProject));
			csprojObj = (VisualStudioProject) xmlDeSer.Deserialize (fsIn);	    
			fsIn.Close();

			// Begin prjxObj population
			prjxObj.name = Path.GetFileNameWithoutExtension(csprojFileName);
			prjxObj.description = "";
			prjxObj.newfilesearch = "None";
			prjxObj.enableviewstate = "True";
			prjxObj.version = (decimal)1.1;
			prjxObj.projecttype = "C#";

			prjxObj.Contents = GetContents (csprojObj.CSHARP.Files.Include);
			prjxObj.References = GetReferences(csprojObj.CSHARP.Build.References);
			prjxObj.DeploymentInformation = new Mfconsulting.General.Prj2Make.Schema.Prjx.DeploymentInformation();
			prjxObj.DeploymentInformation.target = "";
			prjxObj.DeploymentInformation.script = "";
			prjxObj.DeploymentInformation.strategy = "File";

			nCnt = csprojObj.CSHARP.Build.Settings.Config.Length;
			prjxObj.Configurations = new Configurations();
			prjxObj.Configurations.Configuration = new Configuration[nCnt];
			for(int i = 0; i < nCnt; i++)
			{
				prjxObj.Configurations.Configuration[i] = CreateConfigurationBlock(
					csprojObj.CSHARP.Build.Settings.Config[i],
					csprojObj.CSHARP.Build.Settings.AssemblyName,
					csprojObj.CSHARP.Build.Settings.OutputType
					);
			}
			prjxObj.Configurations.active = prjxObj.Configurations.Configuration[0].name;

			prjxObj.Configuration = prjxObj.Configurations.Configuration[0];

			// Serialize
			fsOut = new FileStream (PrjxFileName, FileMode.Create);	    
			xmlSer = new XmlSerializer (typeof(Project));
			xmlSer.Serialize(fsOut, prjxObj);
			fsOut.Close();

			return;
		}

		public void MsSlnToCmbxHelper(string slnFileName)
		{
			int i = 0;
			FileStream fsOut = null;
			XmlSerializer xmlSer = null;
			StringBuilder MakefileBuilder = new StringBuilder();
			Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine cmbxObj = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine();
			string cmbxFileName = String.Format ("{0}.cmbx",
				Path.Combine(Path.GetDirectoryName(slnFileName),
				Path.GetFileNameWithoutExtension(slnFileName))
				);
			
			Console.WriteLine(String.Format("Will create combine filename:{0}", cmbxFileName));

			try
			{
				string d = Path.GetDirectoryName(slnFileName);
				if (d != "")
					Directory.SetCurrentDirectory(d);

				// We invoke the ParseSolution 
				// by passing the file obtained
				ParseSolution (slnFileName);

				// Create all of the prjx files form the csproj files
				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					CreatePrjxFromCsproj(pi.csprojpath);
				}

				// Begin prjxObj population
				cmbxObj.name = Path.GetFileNameWithoutExtension(slnFileName);
				cmbxObj.description = "";
				cmbxObj.fileversion = (decimal)1.0;

				// Create and attach the StartMode element
				Mfconsulting.General.Prj2Make.Schema.Cmbx.StartMode startModeElem = new Mfconsulting.General.Prj2Make.Schema.Cmbx.StartMode();

				// Create the array of Execute objects
				Mfconsulting.General.Prj2Make.Schema.Cmbx.Execute[] executeElem = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Execute[projNameInfo.Count];

				// Populate the Element objects instances
				i = 0;
				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					Mfconsulting.General.Prj2Make.Schema.Cmbx.Execute execElem = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Execute();
					execElem.entry = pi.name;
					execElem.type = "None";

                    executeElem[i++] = execElem;
				}

				startModeElem.startupentry = executeElem[0].entry;
				startModeElem.single = "True";
				startModeElem.Execute = executeElem;

				// Attach the StartMode Object to the
				// Combine object
				cmbxObj.StartMode = startModeElem;

				// Gnerate the entries array
				Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry[] entriesObj = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry[projNameInfo.Count];
				// Populate the Entry objects instances
				i = 0;
				foreach (CsprojInfo pi in projNameInfo.Values)
				{
					Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry entryObj = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry();
					string PrjxFileName = String.Format (".{0}{1}.prjx",
						Path.DirectorySeparatorChar,
						Path.Combine(Path.GetDirectoryName(pi.csprojpath),
						Path.GetFileNameWithoutExtension(pi.csprojpath))
						);

					entryObj.filename = PrjxFileName; 

					entriesObj[i++] = entryObj;
				}

				// Attach the Entries Object to the
				// Combine object
				cmbxObj.Entries = entriesObj;

				Mfconsulting.General.Prj2Make.Schema.Cmbx.Configurations configurationsObj = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Configurations();
				
				// Hack hardcoded configuration value must get the one
				// from analyzing the different configuration entries
				configurationsObj.active = "Debug";

				// Hack hardcoded number of configuration object
				// assuming 2 for Debug and Release
				configurationsObj.Configuration = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Configuration[2];
				Mfconsulting.General.Prj2Make.Schema.Cmbx.Configuration confObj1 = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Configuration();
				configurationsObj.Configuration[0] = confObj1;
				Mfconsulting.General.Prj2Make.Schema.Cmbx.Configuration confObj2 = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Configuration();
				configurationsObj.Configuration[1] = confObj2;

				configurationsObj.Configuration[0].name = "Release";
				configurationsObj.Configuration[0].Entry = CreateArrayOfConfEntries();
				configurationsObj.Configuration[1].name = "Debug";
				configurationsObj.Configuration[1].Entry = CreateArrayOfConfEntries();

				// Attach the Configurations object to the 
				// Combine Object
				cmbxObj.Configurations = configurationsObj;

				// Serialize
				fsOut = new FileStream (cmbxFileName, FileMode.Create);	    
				xmlSer = new XmlSerializer (typeof(Mfconsulting.General.Prj2Make.Schema.Cmbx.Combine));
				xmlSer.Serialize(fsOut, cmbxObj);
				fsOut.Close();

				return;
			}
			catch (Exception e)
			{
				Console.WriteLine("EXCEPTION: {0}\n", e);
				return;
			}
		}

		protected Mfconsulting.General.Prj2Make.Schema.Prjx.Reference[] GetReferences(Mfconsulting.General.Prj2Make.Schema.Csproj.Reference[] References)
		{
			Mfconsulting.General.Prj2Make.Schema.Prjx.Reference[] theReferences = null;
			int i = 0;

			// Get the GAC path
			string strlibDir = PkgConfigInvoker.GetPkgVariableValue("mono", "libdir");

			if (strlibDir == null)
			{
				strlibDir = "/usr/lib";
			}							
								
			string strBasePathMono1_0 = Path.Combine(
					strlibDir.TrimEnd(),
					"mono/1.0");

			string strBasePathMono2_0 = Path.Combine(
				strlibDir.TrimEnd(),
				"mono/2.0");

			string strBasePathGtkSharp = Path.Combine(
				strlibDir.TrimEnd(),
				"mono/gtk-sharp");

			if(References != null && References.Length > 0)
			{
				theReferences = new Mfconsulting.General.Prj2Make.Schema.Prjx.Reference[References.Length];
			}
			else
			{
				return null;
			}

			// Iterate through the reference collection of the csproj file
			foreach(Mfconsulting.General.Prj2Make.Schema.Csproj.Reference rf in References)
			{
				Mfconsulting.General.Prj2Make.Schema.Prjx.Reference rfOut = new Mfconsulting.General.Prj2Make.Schema.Prjx.Reference();
				string strRefFileName;

				if(rf.Package == null || rf.Package.CompareTo("") == 0)
				{
					bool bIsWhereExpected = false;

					// HACK - under Unix filenames are case sensitive
					// Under Windows there's no agreement on Xml vs XML ;-)    					
					if(Path.GetFileName(rf.HintPath).CompareTo("System.XML.dll") == 0)
					{
						strRefFileName = Path.Combine (strBasePathMono1_0, Path.GetFileName("System.Xml.dll"));

						// Test to see if file exist in GAC location
						if(System.IO.File.Exists(strRefFileName) == true) {
							try {
								rfOut.refto = System.Reflection.Assembly.LoadFrom(strRefFileName).FullName;
								rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
								rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
								bIsWhereExpected = true;
							} catch (Exception exc) {
								Console.WriteLine ("Error doing Assembly.LoadFrom with File: {0}\nErr Msg: {1}",
									strRefFileName,
									exc.Message );
							}
						}

						strRefFileName = Path.Combine (strBasePathMono2_0, Path.GetFileName("System.Xml.dll"));

						// Test to see if file exist in GAC location
						if(System.IO.File.Exists(strRefFileName) == true) {
							try {
								rfOut.refto = System.Reflection.Assembly.LoadFrom(strRefFileName).FullName;
								rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
								rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
								bIsWhereExpected = true;
							} catch (Exception exc) {
								Console.WriteLine ("Error doing Assembly.LoadFrom with File: {0}\nErr Msg: {1}",
									strRefFileName,
									exc.Message );
							}
						}
					} else {
						strRefFileName = Path.Combine (strBasePathMono1_0, Path.GetFileName(rf.HintPath));

						// Test to see if file exist in GAC location
						if(System.IO.File.Exists(strRefFileName) == true) {
							try {
								rfOut.refto = System.Reflection.Assembly.LoadFrom(strRefFileName).FullName;
								rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
								rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
								bIsWhereExpected = true;
							} catch (Exception exc) {
								Console.WriteLine ("Error doing Assembly.LoadFrom with File: {0}\nErr Msg: {1}",
									strRefFileName,
									exc.Message );
							}
						}

						strRefFileName = Path.Combine (strBasePathMono2_0, Path.GetFileName(rf.HintPath));

						// Test to see if file exist in GAC location
						if(System.IO.File.Exists(strRefFileName) == true) {
							try {
								rfOut.refto = System.Reflection.Assembly.LoadFrom(strRefFileName).FullName;
								rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
								rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
								bIsWhereExpected = true;
							} catch (Exception exc) {
								Console.WriteLine ("Error doing Assembly.LoadFrom with File: {0}\nErr Msg: {1}",
									strRefFileName,
									exc.Message );
							}
						}

						strRefFileName = Path.Combine (strBasePathGtkSharp, Path.GetFileName(rf.HintPath));

						// Test to see if file exist in GAC location
						if(System.IO.File.Exists(strRefFileName) == true) {
							try {
								rfOut.refto = System.Reflection.Assembly.LoadFrom(strRefFileName).FullName;
								rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
								rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
								bIsWhereExpected = true;
							} catch (Exception exc) {
								Console.WriteLine ("Error doing Assembly.LoadFrom with File: {0}\nErr Msg: {1}",
									strRefFileName,
									exc.Message );
							}
						}
						
						if(bIsWhereExpected == false)
						{
							rfOut.refto = Path.GetFileName(rf.HintPath);
							rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Gac;
							rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
						}
					}

					// increment the iterator value
					theReferences[i++] = rfOut;
				}
				else
				{
					rfOut.type = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceType.Project;
					
					rfOut.refto = Path.GetFileName(rf.Name);
					rfOut.localcopy = Mfconsulting.General.Prj2Make.Schema.Prjx.ReferenceLocalcopy.True;
					// increment the iterator value
					theReferences[i++] = rfOut;
				}
			}

			return theReferences;
		}
		
		protected Mfconsulting.General.Prj2Make.Schema.Prjx.File[] GetContents(Mfconsulting.General.Prj2Make.Schema.Csproj.File[] Include)
		{
			Mfconsulting.General.Prj2Make.Schema.Prjx.File[] theFiles = null;
			int i = 0;

			if(Include != null && Include.Length > 0)
			{
				theFiles = new Mfconsulting.General.Prj2Make.Schema.Prjx.File[Include.Length];
			}
			else
			{
				return null;
			}

			// Iterate through the file collection of the csproj file
			foreach(Mfconsulting.General.Prj2Make.Schema.Csproj.File fl in Include)
			{
				Mfconsulting.General.Prj2Make.Schema.Prjx.File flOut = new Mfconsulting.General.Prj2Make.Schema.Prjx.File();
				flOut.name = String.Format(".{0}{1}", Path.DirectorySeparatorChar, fl.RelPath);
				
				switch(fl.SubType)
				{
					case "Code":
						flOut.subtype = Mfconsulting.General.Prj2Make.Schema.Prjx.FileSubtype.Code;
						break;
				}

				switch(fl.BuildAction)
				{
					case Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.Compile:
						flOut.buildaction = Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.Compile;
						break;
					case Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.Content:
						flOut.buildaction = Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.Exclude;
						break;
					case Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.EmbeddedResource:
						flOut.buildaction = Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.EmbedAsResource;
						break;
					case Mfconsulting.General.Prj2Make.Schema.Csproj.FileBuildAction.None:
						flOut.buildaction = Mfconsulting.General.Prj2Make.Schema.Prjx.FileBuildaction.Exclude;
						break;				
				}
				flOut.dependson = fl.DependentUpon;
				flOut.data = "";

				// increment the iterator value
				theFiles[i++ ] = flOut;
			}

			return theFiles;
		}

		protected Configuration CreateConfigurationBlock(Config ConfigBlock, string AssemblyName, string OuputType)
		{
			Configuration ConfObj = new Configuration();
			CodeGeneration CodeGenObj = new CodeGeneration();
			Execution ExecutionObj = new Execution();
			Output OutputObj = new Output();

			ConfObj.runwithwarnings = "False";
			ConfObj.name = ConfigBlock.Name;

			// CodeGenObj member population
			CodeGenObj.runtime = "MsNet";
			CodeGenObj.compiler = "Csc";
			CodeGenObj.warninglevel = ConfigBlock.WarningLevel;
			CodeGenObj.nowarn = "";
			CodeGenObj.includedebuginformation = (ConfigBlock.DebugSymbols == true) ? 
				CodeGenerationIncludedebuginformation.True : 
				CodeGenerationIncludedebuginformation.False;
			
			CodeGenObj.optimize = (ConfigBlock.Optimize == true) ? "True" : "False";

			if (ConfigBlock.AllowUnsafeBlocks == true)
			{
				CodeGenObj.unsafecodeallowed = CodeGenerationUnsafecodeallowed.True;
			}
			else
			{
				CodeGenObj.unsafecodeallowed = CodeGenerationUnsafecodeallowed.False;
			}
			if (ConfigBlock.CheckForOverflowUnderflow == true)
			{
				CodeGenObj.generateoverflowchecks = "True";
			}
			else
			{
				CodeGenObj.generateoverflowchecks = "False";
			}
			
			CodeGenObj.mainclass = "";
			CodeGenObj.target = OuputType;
			CodeGenObj.generatexmldocumentation = "False";
			CodeGenObj.win32Icon = "";

			// ExecutionObj member population
			ExecutionObj.commandlineparameters = "";
			ExecutionObj.consolepause = "True";

			// OutputObj member population
			OutputObj.directory = ConfigBlock.OutputPath;
			OutputObj.assembly = AssemblyName;
			OutputObj.executeScript = "";
			OutputObj.executeBeforeBuild = "";
			OutputObj.executeAfterBuild = "";

			ConfObj.CodeGeneration = CodeGenObj;
			ConfObj.Execution = ExecutionObj;
			ConfObj.Output = OutputObj;

			return ConfObj;
		}
		
		protected Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry[] CreateArrayOfConfEntries()
		{
			Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry[] confEntry = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry[projNameInfo.Count];
			// Populate the Entry objects instances
			int i = 0;
			foreach (CsprojInfo pi in projNameInfo.Values)
			{
				Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry entryObj = new Mfconsulting.General.Prj2Make.Schema.Cmbx.Entry();
				entryObj.name = pi.name;
				entryObj.configurationname = "Debug";
				entryObj.build = "False";

				confEntry[i++] = entryObj;
			}

			return confEntry;
		}
	}   
}
