//
// TargetBatchingImpl.cs: Class that implements Target Batching Algorithm from the wiki.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2005 Marek Sieradzki
// Copyright 2008 Novell, Inc (http://www.novell.com)
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Xml;

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {

	internal class TargetBatchingImpl : BatchingImplBase
	{
		string		inputs;
		string		outputs;

		public TargetBatchingImpl (Project project, XmlElement targetElement)
			: base (project)
		{
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");

			inputs = targetElement.GetAttribute ("Inputs");
			outputs = targetElement.GetAttribute ("Outputs");
		}

		public bool Build (Target target, out bool executeOnErrors)
		{
			executeOnErrors = false;
			try {
				if (!BuildTargetNeeded ()) {
					LogTargetStarted (target);
					LogTargetSkipped (target);
					LogTargetFinished (target, true);
					return true;
				}

				Init ();

				ParseTargetAttributes (target);
				BatchAndPrepareBuckets ();
				return Run (target, out executeOnErrors);
			} finally {
				consumedItemsByName = null;
				consumedMetadataReferences = null;
				consumedQMetadataReferences = null;
				consumedUQMetadataReferences = null;
				batchedItemsByName = null;
				commonItemsByName = null;
			}
		}

		bool Run (Target target, out bool executeOnErrors)
		{
			executeOnErrors = false;
			if (buckets.Count > 0)
				return RunBatched (target, out executeOnErrors);
			else
				return RunUnbatched (target, out executeOnErrors);
		}

		bool RunBatched (Target target, out bool executeOnErrors)
		{
			bool result = true;
			executeOnErrors = false;
			foreach (Dictionary<string, BuildItemGroup> bucket in buckets) {
				LogTargetStarted (target);
				project.PushBatch (bucket, commonItemsByName);
				try {
					if (!BuildTargetNeeded ()) {
						LogTargetSkipped (target);
						continue;
					}

					for (int i = 0; i < target.BuildTasks.Count; i ++) {
						//FIXME: parsing attributes repeatedly
						BuildTask task = target.BuildTasks [i];
						result = new TaskBatchingImpl (project).Build (task, out executeOnErrors);
						if (!result && !task.ContinueOnError) {
							executeOnErrors = true;
							break;
						}
					}
				} finally {
					project.PopBatch ();
					LogTargetFinished (target, result);
				}
			}
			return result;
		}

		bool RunUnbatched (Target target, out bool executeOnErrors)
		{
			bool result = true;
			executeOnErrors = false;
			LogTargetStarted (target);
			try {
				if (!BuildTargetNeeded ()) {
					LogTargetSkipped (target);
					LogTargetFinished (target, true);
					return true;
				}

				foreach (BuildTask bt in target.BuildTasks) {
					TaskBatchingImpl batchingImpl = new TaskBatchingImpl (project);
					result = batchingImpl.Build (bt, out executeOnErrors);

					if (!result && !bt.ContinueOnError) {
						executeOnErrors = true;
						break;
					}
				}
			} finally {
				LogTargetFinished (target, result);
			}

			return result;
		}

		// Parse target's Input and Output attributes to get list of referenced
		// metadata and items to determine batching
		void ParseTargetAttributes (Target target)
		{
			if (!String.IsNullOrEmpty (inputs))
				ParseAttribute (inputs);

			if (!String.IsNullOrEmpty (outputs))
				ParseAttribute (outputs);
		}

		bool BuildTargetNeeded ()
		{
			ITaskItem [] inputFiles;
			ITaskItem [] outputFiles;
			DateTime oldestInput, youngestOutput;

			if (String.IsNullOrEmpty (inputs.Trim ()))
				return true;

			if (String.IsNullOrEmpty (outputs.Trim ()))
				return true;

			Expression e = new Expression ();
			e.Parse (inputs, ParseOptions.AllowItemsMetadataAndSplit);
			inputFiles = (ITaskItem[]) e.ConvertTo (project, typeof (ITaskItem[]), ExpressionOptions.ExpandItemRefs);

			e = new Expression ();
			e.Parse (outputs, ParseOptions.AllowItemsMetadataAndSplit);
			outputFiles = (ITaskItem[]) e.ConvertTo (project, typeof (ITaskItem[]), ExpressionOptions.ExpandItemRefs);

			if (inputFiles == null || inputFiles.Length == 0)
				return false;

			//FIXME: if input specified, then output must also
			//	 be there, add tests and confirm
			if (outputFiles == null || outputFiles.Length == 0)
				return false;

			if (File.Exists (inputFiles [0].ItemSpec))
				oldestInput = File.GetLastWriteTime (inputFiles [0].ItemSpec);
			else
				return true;

			if (File.Exists (outputFiles [0].ItemSpec))
				youngestOutput = File.GetLastWriteTime (outputFiles [0].ItemSpec);
			else
				return true;

			foreach (ITaskItem item in inputFiles) {
				string file = item.ItemSpec;
				if (file.Trim () == String.Empty)
					continue;

				if (File.Exists (file.Trim ())) {
					if (File.GetLastWriteTime (file.Trim ()) > oldestInput)
						oldestInput = File.GetLastWriteTime (file.Trim ());
				} else {
					return true;
				}
			}

			foreach (ITaskItem item in outputFiles) {
				string file = item.ItemSpec;
				if (file.Trim () == String.Empty)
					continue;

				if (File.Exists (file.Trim ())) {
					if (File.GetLastWriteTime (file.Trim ()) < youngestOutput)
						youngestOutput = File.GetLastWriteTime (file.Trim ());
				} else
					return true;
			}

			if (oldestInput > youngestOutput)
				return true;
			else
				return false;
		}

 		void LogTargetSkipped (Target target)
		{
			BuildMessageEventArgs bmea;
			bmea = new BuildMessageEventArgs (String.Format ("Skipping target \"{0}\" because its outputs are up-to-date.",
				target.Name), null, "MSBuild", MessageImportance.Normal);
			target.Engine.EventSource.FireMessageRaised (this, bmea);
		}

		void LogTargetStarted (Target target)
		{
			TargetStartedEventArgs tsea;
			string projectFile = project.FullFileName;
			tsea = new TargetStartedEventArgs (String.Format ("Target {0} started.", target.Name), null,
					target.Name, projectFile, target.TargetFile);
			target.Engine.EventSource.FireTargetStarted (this, tsea);
		}

		void LogTargetFinished (Target target, bool succeeded)
		{
			TargetFinishedEventArgs tfea;
			string projectFile = project.FullFileName;
			tfea = new TargetFinishedEventArgs (String.Format ("Target {0} finished.", target.Name), null,
					target.Name, projectFile, target.TargetFile, succeeded);
			target.Engine.EventSource.FireTargetFinished (this, tfea);
		}

	}
}

#endif
