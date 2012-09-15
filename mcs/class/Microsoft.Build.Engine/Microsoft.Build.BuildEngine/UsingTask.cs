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

using System;
using System.IO;
using System.Xml;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	public class UsingTask {
	
		ImportedProject	importedProject;
		Project		project;
		XmlElement	usingTaskElement;

		internal UsingTask (XmlElement usingTaskElement, Project project, ImportedProject importedProject)
		{
			this.project = project;
			this.importedProject = importedProject;
			this.usingTaskElement = usingTaskElement;
			
			if (project == null)
				throw new ArgumentNullException ("project");
			if (usingTaskElement == null)
				throw new ArgumentNullException ("usingTaskElement");
			if (AssemblyName != null && AssemblyFile != null)
				throw new InvalidProjectFileException ("A <UsingTask> element must contain either the \"AssemblyName\" attribute or the \"AssemblyFile\" attribute (but not both).  ");
			if (TaskName == String.Empty)
				throw new InvalidProjectFileException ("The required attribute \"TaskName\" is missing from element <UsingTask>.  ");
		}

		internal void Evaluate ()
		{
			if (AssemblyName == null && AssemblyFile == null)
				throw new InvalidProjectFileException ("A <UsingTask> element must contain either the \"AssemblyName\" attribute or the \"AssemblyFile\" attribute (but not both).  ");

			if (ConditionParser.ParseAndEvaluate (Condition, project))
				project.TaskDatabase.RegisterUsingTask (this);
		}

		internal void Load (TaskDatabase db)
		{
			AssemblyLoadInfo loadInfo = null;

			if (AssemblyName != null) {
				loadInfo = new AssemblyLoadInfo (AssemblyName, TaskName);
			} else if (AssemblyFile != null) {
				Expression exp = new Expression ();
				// FIXME: test it
				exp.Parse (AssemblyFile, ParseOptions.Split);
				string filename = (string) exp.ConvertTo (project, typeof (string));

				if (Path.IsPathRooted (filename) == false) {
					string ffn;
					if (importedProject != null) {
						ffn = Path.GetDirectoryName (importedProject.FullFileName);
					} else if (project.FullFileName != String.Empty) {
						ffn = Path.GetDirectoryName (project.FullFileName);
					} else {
						ffn = Environment.CurrentDirectory;
					}
					filename = Path.Combine (ffn, filename);
				}
				loadInfo = new AssemblyLoadInfo (LoadInfoType.AssemblyFilename, filename, null, null, null, null, TaskName);
			}

			db.RegisterTask (TaskName, loadInfo);
		}

		public bool IsImported {
			get { return importedProject != null; }
		}
		
		public string AssemblyFile {
			get {
				string assemblyFile = usingTaskElement.GetAttribute ("AssemblyFile");
				return (assemblyFile == String.Empty) ? null : assemblyFile;
			}
		}
		
		public string AssemblyName {
			get {
				string assemblyName = usingTaskElement.GetAttribute ("AssemblyName");
				return (assemblyName == String.Empty) ? null : assemblyName;
			}
		}
		
		public string Condition {
			get {
				string condition = usingTaskElement.GetAttribute ("Condition");
				return (condition == String.Empty) ? null : condition;
			}
		}
		
		public string TaskName {
			get { return usingTaskElement.GetAttribute ("TaskName"); }
		}
	}
}
