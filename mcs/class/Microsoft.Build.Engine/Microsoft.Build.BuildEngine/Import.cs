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

namespace Microsoft.Build.BuildEngine {
	public class Import {
		XmlElement	importElement;
		Project		project;
		ImportedProject originalProject;
		string		evaluatedProjectPath;
	
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
			evaluatedProjectPath = EvaluateProjectPath (ProjectPath);
			evaluatedProjectPath = GetFullPath ();
			if (EvaluatedProjectPath == String.Empty)
				throw new InvalidProjectFileException ("The required attribute \"Project\" is missing from element <Import>.");
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
			Expression exp;

			exp = new Expression ();
			exp.Parse (file, ParseOptions.Split);
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
