//
// UsingTask.cs: Represents a single UsingTask element in an MSBuild project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	public class UsingTask {
		XmlElement	usingTaskElement;
		Project		project;
		ImportedProject	importedProject;

		private UsingTask()
		{
		}
		
		internal UsingTask (XmlElement usingTaskElement, Project project, ImportedProject importedProject)
		{
			this.project = project;
			this.importedProject = importedProject;
			this.usingTaskElement = usingTaskElement;
			
			if (project == null)
				throw new ArgumentNullException ("project");
			if (usingTaskElement == null)
				throw new ArgumentNullException ("usingTaskElement");
			if (AssemblyName == String.Empty && AssemblyFile == String.Empty)
				throw new InvalidProjectFileException ("AssemblyName or AssemblyFile attribute must be specified.");
			if (TaskName == String.Empty)
				throw new InvalidProjectFileException ("TaskName attribute must be specified.");
		}

		internal void Evaluate ()
		{
			AssemblyLoadInfo loadInfo;

			if (AssemblyName != String.Empty) {
				loadInfo = new AssemblyLoadInfo (AssemblyName, TaskName);
			} else if (AssemblyFile != String.Empty) {
				string filename = AssemblyFile;
				if (Path.IsPathRooted (filename) == false) {
					string ffn;
					if (importedProject != null) {
						ffn = Path.GetDirectoryName (importedProject.FullFileName);
					} else {
						ffn = Path.GetDirectoryName (project.FullFileName);
					}
					filename = Path.Combine (ffn, filename);
				}
				loadInfo = new AssemblyLoadInfo (LoadInfoType.AssemblyFilename, filename, null, null, null, null, TaskName);
			} else {
				throw new InvalidProjectFileException ("AssemblyName or AssemblyFile attribute must be specified.");
			}
			project.TaskDatabase.RegisterTask (TaskName, loadInfo);
		}

		public bool IsImported {
			get { return importedProject != null; }
		}
		
		public string AssemblyFile {
			get { return usingTaskElement.GetAttribute ("AssemblyFile"); }
		}
		
		public string AssemblyName {
			get { return usingTaskElement.GetAttribute ("AssemblyName"); }
		}
		
		public string Condition {
			get { return usingTaskElement.GetAttribute ("Condition"); }
		}
		
		public string TaskName {
			get { return usingTaskElement.GetAttribute ("TaskName"); }
		}
	}
}

#endif
