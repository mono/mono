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
				throw new InvalidProjectFileException ("Project attribute must be specified.");
		}

		internal void Evaluate ()
		{
			string file;
			Expression exp;

			exp = new Expression (project, ProjectPath);
			file = evaluatedProjectPath = (string) exp.ToNonArray (typeof (string));

			if (Path.IsPathRooted (EvaluatedProjectPath) == false) {
				string dir;
				if (originalProject == null)
					dir = Path.GetDirectoryName (project.FullFileName);
				else
					dir = Path.GetDirectoryName (originalProject.FullFileName);
				file = Path.Combine (dir, EvaluatedProjectPath);
			}

			// FIXME: loggers anybody?
			if (!File.Exists (file)) {
				Console.WriteLine ("Imported file {0} doesn't exist.", file);
				return;
			}
			
			ImportedProject importedProject = new ImportedProject ();
			importedProject.Load (file);
			// FIXME: UGLY HACK
			project.ProcessElements (importedProject.XmlDocument.DocumentElement, importedProject);
		}
		
		public string Condition {
			get { return importElement.GetAttribute ("Condition"); }
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
