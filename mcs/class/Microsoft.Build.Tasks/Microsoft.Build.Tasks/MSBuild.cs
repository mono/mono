//
// MSBuild.cs: Task that can run .*proj files
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
// Copyright 2011 Xamarin Inc
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {

	[MonoTODO]
	public class MSBuild : TaskExtension {
	
		ITaskItem []	projects;
		string []	properties;
		bool		rebaseOutputs;
		bool		runEachTargetSeparately;
		bool		stopOnFirstFailure;
		bool		buildInParallel;
		ITaskItem []	targetOutputs;
		string []	targets;
	
		public MSBuild ()
		{
		}

		public override bool Execute ()
		{
			if (projects.Length == 0)
				return true;

			string filename;
			bool result = true;
			bool all_result = true;
			stopOnFirstFailure = false;
			List <ITaskItem > outputItems = new List <ITaskItem> ();
			string currentDirectory = Environment.CurrentDirectory;
			Hashtable outputs;
		
			var global_properties = SplitPropertiesToDictionary ();

			Log.LogMessage (MessageImportance.Low, "Global Properties:");
			if (global_properties != null)
				foreach (KeyValuePair<string, string> pair in global_properties)
					Log.LogMessage (MessageImportance.Low, "\t{0} = {1}", pair.Key, pair.Value);

			foreach (ITaskItem project in projects) {
				filename = project.GetMetadata ("FullPath");
				if (!File.Exists (filename)) {
					Log.LogError ("Could not find the project file '{0}'", filename);
					if (stopOnFirstFailure)
						break;

					continue;
				}

				Directory.SetCurrentDirectory (Path.GetDirectoryName (filename));
				outputs = new Hashtable ();

				try {
					// Order of precedence:
					// ToolsVersion property, %(Project.ToolsVersion)
					string tv = ToolsVersion;
					if (String.IsNullOrEmpty (tv))
						// metadata on the Project item
						tv = project.GetMetadata ("ToolsVersion");

					if (!String.IsNullOrEmpty (tv) && Engine.GlobalEngine.Toolsets [tv] == null)
						throw new UnknownToolsVersionException (tv);

					result = BuildEngine2.BuildProjectFile (filename, targets, global_properties, outputs, tv);
				} catch (InvalidProjectFileException e) {
					Log.LogError ("Error building project {0}: {1}", filename, e.Message);
					result = false;
				}

				if (!result)
					all_result = false;

				if (result) {
					foreach (DictionaryEntry de in outputs) {
						ITaskItem [] array = (ITaskItem []) de.Value;
						foreach (ITaskItem item in array) {
							// DONT share items!
							ITaskItem new_item = new TaskItem (item);

							// copy the metadata from original @project to here
							// CopyMetadataTo does _not_ overwrite
							project.CopyMetadataTo (new_item);

							outputItems.Add (new_item);

							//FIXME: Correctly rebase output paths to be relative to the
							//	 calling project
							//if (rebaseOutputs)
							//	File.Copy (item.ItemSpec, Path.Combine (currentDirectory, item.ItemSpec), true);
						}
					}
				} else {
					if (stopOnFirstFailure)
						break;
				}

				Directory.SetCurrentDirectory (currentDirectory);
			}

			if (all_result)
				targetOutputs = outputItems.ToArray ();

			Directory.SetCurrentDirectory (currentDirectory);
			return all_result;
		}

		void ThrowIfInvalidToolsVersion (string toolsVersion)
		{
			if (!String.IsNullOrEmpty (toolsVersion) && Engine.GlobalEngine.Toolsets [toolsVersion] == null)
				throw new UnknownToolsVersionException (toolsVersion);
		}

		[Required]
		public ITaskItem [] Projects {
			get { return projects; }
			set { projects = value; }
		}

		[MonoTODO]
		public string [] Properties {
			get { return properties; }
			set { properties = value; }
		}

		public bool RebaseOutputs {
			get { return rebaseOutputs; }
			set { rebaseOutputs = value; }
		}

		[MonoTODO]
		public bool RunEachTargetSeparately {
			get { return runEachTargetSeparately; }
			set { runEachTargetSeparately = value; }
		}

		public bool StopOnFirstFailure {
			get { return stopOnFirstFailure; }
			set { stopOnFirstFailure = value; }
		}

		[Output]
		public ITaskItem [] TargetOutputs {
			get { return targetOutputs; }
		}

		public string [] Targets {
			get { return targets; }
			set { targets = value; }
		}

		public bool BuildInParallel {
			get { return buildInParallel; }
			set { buildInParallel = value; }
		}

		public string ToolsVersion {
			get; set;
		}

		SortedDictionary<string, string> SplitPropertiesToDictionary ()
		{
			if (properties == null)
				return null;

			var global_properties = new SortedDictionary<string, string> ();
			foreach (string kvpair in properties) {
				if (String.IsNullOrEmpty (kvpair))
					continue;

				string [] parts = kvpair.Trim ().Split (new char [] {'='}, 2);
				if (parts.Length != 2) {
					Log.LogWarning ("Invalid key/value pairs ({0}) in Properties, ignoring.", kvpair);
					continue;
				}

				global_properties.Add (parts [0], parts [1]);
			}

			return global_properties;
		}

	}
}

#endif
