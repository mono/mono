//
// Import.cs: Represents a single Import element in an MSBuild project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Import {
		XmlElement	importElement;
		Project		project;
		ImportedProject originalProject;
		string		evaluatedProjectPath;

		static string DotConfigExtensionsPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
								Path.Combine ("xbuild", "tasks"));
	
		internal Import (XmlElement importElement, Project project, ImportedProject originalProject)
		{
			if (importElement == null)
				throw new ArgumentNullException ("importElement");
			if (project == null)
				throw new ArgumentNullException ("project");
		
			this.project = project;
			this.importElement = importElement;
			this.originalProject = originalProject;

			if (ProjectPath == String.Empty)
				throw new InvalidProjectFileException ("The required attribute \"Project\" is missing from element <Import>.");

			if (ConditionParser.ParseAndEvaluate (Condition, project)) {
				evaluatedProjectPath = EvaluateProjectPath (ProjectPath);
				evaluatedProjectPath = GetFullPath ();
				if (EvaluatedProjectPath == String.Empty)
					throw new InvalidProjectFileException ("The required attribute \"Project\" is missing from element <Import>.");
			}
		}

		// FIXME: condition
		internal void Evaluate (bool ignoreMissingImports)
		{
			string filename = evaluatedProjectPath;
			// NOTE: it's a hack to transform Microsoft.CSharp.Targets to Microsoft.CSharp.targets
			if (Path.HasExtension (filename))
				filename = Path.ChangeExtension (filename, Path.GetExtension (filename));
			
			if (!File.Exists (filename)) {
				if (ignoreMissingImports) {
					project.LogWarning (project.FullFileName, "Could not find project file {0}, to import. Ignoring.", filename);
					return;
				} else {
					throw new InvalidProjectFileException (String.Format ("Imported project: \"{0}\" does not exist.", filename));
				}
			}
			
			ImportedProject importedProject = new ImportedProject ();
			importedProject.Load (filename);

			project.ProcessElements (importedProject.XmlDocument.DocumentElement, importedProject);
		}

		string EvaluateProjectPath (string file)
		{
			string ret;
			if (EvaluateAsMSBuildExtensionsPath (file, "MSBuildExtensionsPath", out ret) ||
				EvaluateAsMSBuildExtensionsPath (file, "MSBuildExtensionsPath32", out ret) ||
				EvaluateAsMSBuildExtensionsPath (file, "MSBuildExtensionsPath64", out ret))
				return ret;

			return EvaluatePath (file);
		}

		bool EvaluateAsMSBuildExtensionsPath (string file, string property_name, out string epath)
		{
			epath = null;
			string property_ref = String.Format ("$({0})", property_name);
			if (file.IndexOf (property_ref) < 0)
				return false;

			// This is a *HACK* to support multiple paths for
			// MSBuildExtensionsPath property. Normally it would
			// get resolved to a single value, but here we special
			// case it and try ~/.config/xbuild/tasks and any
			// paths specified in the env var $MSBuildExtensionsPath .
			//
			// The property itself will resolve to the default
			// location though, so you get in any other part of the
			// project.

			string envvar = Environment.GetEnvironmentVariable (property_name);
			envvar = (envvar ?? String.Empty) + ":" + DotConfigExtensionsPath;

			string [] paths = envvar.Split (new char [] {':'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string path in paths) {
				if (!Directory.Exists (path)) {
					project.ParentEngine.LogMessage (MessageImportance.Low, "Extension path '{0}' not found, ignoring.", path);
					continue;
				}

				string pfile = Path.GetFullPath (file.Replace ("\\", "/").Replace (
							property_ref, path + Path.DirectorySeparatorChar));

				var evaluated_path = EvaluatePath (pfile);
				if (File.Exists (evaluated_path)) {
					project.ParentEngine.LogMessage (MessageImportance.Low,
						"{0}: Importing project {1} from extension path {2}", project.FullFileName, evaluated_path, path);
					epath = pfile;
					return true;
				}
				project.ParentEngine.LogMessage (MessageImportance.Low,
						"{0}: Couldn't find project {1} for extension path {2}", project.FullFileName, evaluated_path, path);
			}

			return false;
		}

		string EvaluatePath (string path)
		{
			var exp = new Expression ();
			exp.Parse (path, ParseOptions.Split);
			return (string) exp.ConvertTo (project, typeof (string));
		}

		string GetFullPath ()
		{
			string file = EvaluatedProjectPath;

			if (!Path.IsPathRooted (EvaluatedProjectPath)) {
				string dir = null;
				if (originalProject == null) {
					if (project.FullFileName != String.Empty) // Path.GetDirectoryName throws exception on String.Empty
						dir = Path.GetDirectoryName (project.FullFileName);
				} else {
					if (originalProject.FullFileName != String.Empty)
						dir = Path.GetDirectoryName (originalProject.FullFileName);
				}
				if (dir != null)
					file = Path.Combine (dir, EvaluatedProjectPath);
			}
			
			return Utilities.FromMSBuildPath (file);
		}
		
		public string Condition {
			get {
				string s = importElement.GetAttribute ("Condition");
				return s == String.Empty ? null : s;
			}
		}
		
		public string EvaluatedProjectPath {
			get { return evaluatedProjectPath; }
		}
		
		public bool IsImported {
			get { return originalProject != null; }
		}
		
		public string ProjectPath {
			get { return importElement.GetAttribute ("Project"); }
		}
	}
}

#endif
