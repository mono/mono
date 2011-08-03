//
// Import.cs: Represents a single Import element in an MSBuild project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2006 Marek Sieradzki
// Copyright 2011 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class Import {
		XmlElement	importElement;
		Project		project;
		ImportedProject originalProject;
		string		evaluatedProjectPath;

		static string DotConfigExtensionsPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
								Path.Combine ("xbuild", "tasks"));
		const string MacOSXExternalXBuildDir = "/Library/Frameworks/Mono.framework/External/xbuild";
		static string PathSeparatorAsString = Path.PathSeparator.ToString ();
	
		internal Import (XmlElement importElement, Project project, ImportedProject originalProject)
			: this (importElement, null, project, originalProject)
		{}

		// if @alternateProjectPath is available then that it used as the EvaluatedProjectPath!
		internal Import (XmlElement importElement, string alternateProjectPath, Project project, ImportedProject originalProject)
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
				evaluatedProjectPath = String.IsNullOrEmpty (alternateProjectPath) ? EvaluateProjectPath (ProjectPath) : alternateProjectPath;

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
			if (!File.Exists (filename) && Path.GetFileName (filename) == "Microsoft.CSharp.Targets")
				filename = Path.ChangeExtension (filename, ".targets");

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
			return Expression.ParseAs<string> (file, ParseOptions.Split, project);
		}

		string GetFullPath ()
		{
			string file = EvaluatedProjectPath;
			if (!Path.IsPathRooted (file) && !String.IsNullOrEmpty (ContainedInProjectFileName))
				file = Path.Combine (Path.GetDirectoryName (ContainedInProjectFileName), file);

			return MSBuildUtils.FromMSBuildPath (file);
		}

		// For every extension path, in order, finds suitable
		// import filename(s) matching the Import, and calls
		// @func with them
		//
		// func: bool func(importPath, from_source_msg)
		//
		// If for an extension path, atleast one file gets imported,
		// then it stops at that.
		// So, in case imports like "$(MSBuildExtensionsPath)\foo\*",
		// for every extension path, it will try to import the "foo\*",
		// and if atleast one file gets successfully imported, then it
		// stops at that
		internal static void ForEachExtensionPathTillFound (XmlElement xmlElement, Project project, ImportedProject importingProject,
				Func<string, string, bool> func)
		{
			string project_attribute = xmlElement.GetAttribute ("Project");
			string condition_attribute = xmlElement.GetAttribute ("Condition");

			bool has_extn_ref = project_attribute.IndexOf ("$(MSBuildExtensionsPath)") >= 0 ||
						project_attribute.IndexOf ("$(MSBuildExtensionsPath32)") >= 0 ||
						project_attribute.IndexOf ("$(MSBuildExtensionsPath64)") >= 0;

			string importingFile = importingProject != null ? importingProject.FullFileName : project.FullFileName;
			DirectoryInfo base_dir_info = null;
			if (!String.IsNullOrEmpty (importingFile))
				base_dir_info = new DirectoryInfo (Path.GetDirectoryName (importingFile));
			else
				base_dir_info = new DirectoryInfo (Directory.GetCurrentDirectory ());

			IEnumerable<string> extn_paths = has_extn_ref ? GetExtensionPaths (project) : new string [] {null};
			try {
				foreach (string path in extn_paths) {
					string extn_msg = null;
					if (has_extn_ref) {
						project.SetExtensionsPathProperties (path);
						extn_msg = "from extension path " + path;
					}

					// do this after setting new Extension properties, as condition might
					// reference it
					if (!ConditionParser.ParseAndEvaluate (condition_attribute, project))
						continue;

					// We stop if atleast one file got imported.
					// Remaining extension paths are *not* tried
					bool atleast_one = false;
					foreach (string importPath in GetImportPathsFromString (project_attribute, project, base_dir_info)) {
						try {
							if (func (importPath, extn_msg))
								atleast_one = true;
						} catch (Exception e) {
							throw new InvalidProjectFileException (String.Format (
										"{0}: Project file could not be imported, it was being imported by " +
										"{1}: {2}", importPath, importingFile, e.Message), e);
						}
					}

					if (atleast_one)
						return;
				}
			} finally {
				if (has_extn_ref)
					project.SetExtensionsPathProperties (Project.DefaultExtensionsPath);
			}
		}

		// Parses the Project attribute from an Import,
		// and returns the import filenames that match.
		// This handles wildcards also
		static IEnumerable<string> GetImportPathsFromString (string import_string, Project project, DirectoryInfo base_dir_info)
		{
			string parsed_import = Expression.ParseAs<string> (import_string, ParseOptions.AllowItemsNoMetadataAndSplit, project);
			if (parsed_import != null)
				parsed_import = parsed_import.Trim ();

			if (String.IsNullOrEmpty (parsed_import))
				throw new InvalidProjectFileException ("The required attribute \"Project\" in Import is empty");

#if NET_4_0
			if (DirectoryScanner.HasWildcard (parsed_import)) {
				var directoryScanner = new DirectoryScanner () {
					Includes = new ITaskItem [] { new TaskItem (parsed_import) },
					BaseDirectory = base_dir_info
				};
				directoryScanner.Scan ();

				foreach (ITaskItem matchedItem in directoryScanner.MatchedItems)
					yield return matchedItem.ItemSpec;
			} else
#endif
				yield return parsed_import;
		}

		// Gives a list of extensions paths to try for $(MSBuildExtensionsPath),
		// *in-order*
		static IEnumerable<string> GetExtensionPaths (Project project)
		{
			// This is a *HACK* to support multiple paths for
			// MSBuildExtensionsPath property. Normally it would
			// get resolved to a single value, but here we special
			// case it and try various paths, see the code below
			//
			// The property itself will resolve to the default
			// location though, so you get that in any other part of the
			// project.

			string envvar = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath");
			envvar = String.Join (PathSeparatorAsString, new string [] {
						(envvar ?? String.Empty),
						// For mac osx, look in the 'External' dir on macosx,
						// see bug #663180
						MSBuildUtils.RunningOnMac ? MacOSXExternalXBuildDir : String.Empty,
						DotConfigExtensionsPath,
						Project.DefaultExtensionsPath});

			var pathsTable = new Dictionary<string, string> ();
			foreach (string extn_path in envvar.Split (new char [] {Path.PathSeparator}, StringSplitOptions.RemoveEmptyEntries)) {
				if (pathsTable.ContainsKey (extn_path))
					continue;

				if (!Directory.Exists (extn_path)) {
					project.ParentEngine.LogMessage (MessageImportance.Low, "Extension path '{0}' not found, ignoring.", extn_path);
					continue;
				}

				pathsTable [extn_path] = extn_path;
				yield return extn_path;
			}
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

		internal string ContainedInProjectFileName {
			get { return originalProject != null ? originalProject.FullFileName : project.FullFileName; }
		}
	}
}

#endif
