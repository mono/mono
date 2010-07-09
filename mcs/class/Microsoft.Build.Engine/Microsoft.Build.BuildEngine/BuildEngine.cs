//
// BuildEngine.cs: Class that can be accessed by task.
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
using System.Collections;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class BuildEngine : IBuildEngine2 {
	
		Engine	engine;
		int	columnNumberOfTaskNode;
		bool	continueOnError;
		int	lineNumberOfTaskNode;
		Project project;
		string taskfile;
		
		public BuildEngine (Engine engine, Project project, string taskfile, int column, int line,
				    bool continueOnError)
		{
			this.engine = engine;
			this.project = project;
			this.columnNumberOfTaskNode = column;
			this.continueOnError = continueOnError;
			this.lineNumberOfTaskNode = line;
			this.taskfile = taskfile;
		}
	
		// Initiates a build of a project file. If the build is
		// successful, the outputs (if any) of the specified targets
		// are returned.
		public bool BuildProjectFile (string projectFileName,
				       string[] targetNames,
				       IDictionary globalProperties,
				       IDictionary targetOutputs)
		{
			return BuildProjectFile (projectFileName, targetNames, globalProperties, targetOutputs, null);
		}

		public bool BuildProjectFile (string projectFileName,
				       string[] targetNames,
				       IDictionary globalProperties,
				       IDictionary targetOutputs, string toolsVersion)
		{
			if (String.IsNullOrEmpty (projectFileName)) {
				string oldProjectToolsVersion = project.ToolsVersion;
				project.ToolsVersion = toolsVersion;
				try {
					return engine.BuildProject (project, targetNames, targetOutputs,
						BuildSettings.DoNotResetPreviouslyBuiltTargets);
				} finally {
					project.ToolsVersion = oldProjectToolsVersion;
				}
			} else {
				BuildPropertyGroup bpg = new BuildPropertyGroup ();
				if (globalProperties != null)
					foreach (DictionaryEntry de in globalProperties)
						bpg.AddProperty (new BuildProperty (
							(string) de.Key, (string) de.Value,
							PropertyType.Global));
				return engine.BuildProjectFile (projectFileName,
					targetNames, bpg, targetOutputs, BuildSettings.DoNotResetPreviouslyBuiltTargets, toolsVersion);
			}
		}

		public bool BuildProjectFilesInParallel (string[] projectFileNames,
					string [] targetNames,
					IDictionary[] globalProperties,
					IDictionary[] targetOutputsPerProject,
					string[] toolsVersion,
					bool useResultsCache,
					bool unloadProjectsOnCompletion)
		{
			throw new NotImplementedException ();
		}

		// Raises a custom event to all registered loggers.
		public void LogCustomEvent (CustomBuildEventArgs e)
		{
			engine.EventSource.FireCustomEventRaised (this, e);
		}

		// Raises an error to all registered loggers.
		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			if (ContinueOnError) {
				// log the error as a warning
				LogWarningEvent (new BuildWarningEventArgs (
					e.Subcategory, e.Code, e.File, e.LineNumber, e.ColumnNumber,
					e.EndLineNumber, e.EndColumnNumber, e.Message,
					e.HelpKeyword, e.SenderName));

				LogMessageEvent (new BuildMessageEventArgs (
							"Previous error was converted to a warning as the " +
							"task was called with ContinueOnError=true.",
							null, null, MessageImportance.Normal));

			} else {
				engine.EventSource.FireErrorRaised (this, e);
			}
		}

		// Raises a message event to all registered loggers.
		public void LogMessageEvent (BuildMessageEventArgs e)
		{
			engine.EventSource.FireMessageRaised (this, e);
		}

		// Raises a warning to all registered loggers.
		public void LogWarningEvent (BuildWarningEventArgs e)
		{
			engine.EventSource.FireWarningRaised (this, e);
		}

		public int ColumnNumberOfTaskNode {
			get { return columnNumberOfTaskNode; }
		}

		public bool ContinueOnError {
			get { return continueOnError; }
		}

		public int LineNumberOfTaskNode {
			get { return lineNumberOfTaskNode; }
		}

		public string ProjectFileOfTaskNode {
			get { return taskfile; }
		}

		public bool IsRunningMultipleNodes {
			get { return false; }
		}
		
	}
}

#endif
