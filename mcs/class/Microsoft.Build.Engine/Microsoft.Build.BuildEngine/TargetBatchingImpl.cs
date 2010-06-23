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
		string		name;

		public TargetBatchingImpl (Project project, XmlElement targetElement)
			: base (project)
		{
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");

			inputs = targetElement.GetAttribute ("Inputs");
			outputs = targetElement.GetAttribute ("Outputs");
			name = targetElement.GetAttribute ("Name");
		}

		public bool Build (Target target, out bool executeOnErrors)
		{
			executeOnErrors = false;
			try {
				string reason;
				if (!BuildTargetNeeded (out reason)) {
					LogTargetStarted (target);
					LogTargetSkipped (target, reason);
					LogTargetFinished (target, true);
					return true;
				}

				if (!String.IsNullOrEmpty (reason))
					target.Engine.LogMessage (MessageImportance.Low, reason);

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
			if (buckets.Count > 0) {
				foreach (Dictionary<string, BuildItemGroup> bucket in buckets)
					if (!RunTargetWithBucket (bucket, target, out executeOnErrors))
						return false;

				return true;
			} else {
				return RunTargetWithBucket (null, target, out executeOnErrors);
			}
		}

		bool RunTargetWithBucket (Dictionary<string, BuildItemGroup> bucket, Target target, out bool executeOnErrors)
		{
			bool target_result = true;
			executeOnErrors = false;

			LogTargetStarted (target);
			if (bucket != null)
				project.PushBatch (bucket, commonItemsByName);
			try {
				string reason;
				if (!BuildTargetNeeded (out reason)) {
					LogTargetSkipped (target, reason);
					return true;
				}

				if (!String.IsNullOrEmpty (reason))
					target.Engine.LogMessage (MessageImportance.Low, reason);

				for (int i = 0; i < target.BuildTasks.Count; i ++) {
					//FIXME: parsing attributes repeatedly
					BuildTask bt = target.BuildTasks [i];

					TaskBatchingImpl batchingImpl = new TaskBatchingImpl (project);
					bool task_result = batchingImpl.Build (bt, out executeOnErrors);
					if (task_result)
						continue;

					// task failed, if ContinueOnError,
					// ignore failed state for target
					target_result = bt.ContinueOnError;

					if (!bt.ContinueOnError) {
						executeOnErrors = true;
						return false;
					}

				}
			} finally {
				if (bucket != null)
					project.PopBatch ();
				LogTargetFinished (target, target_result);
			}

			return target_result;
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

		bool BuildTargetNeeded (out string reason)
		{
			reason = String.Empty;
			ITaskItem [] inputFiles;
			ITaskItem [] outputFiles;
			DateTime youngestInput, oldestOutput;

			if (String.IsNullOrEmpty (inputs.Trim ()))
				return true;

			if (String.IsNullOrEmpty (outputs.Trim ())) {
				project.ParentEngine.LogError ("Target {0} has inputs but no outputs specified.", name);
				return true;
			}

			Expression e = new Expression ();
			e.Parse (inputs, ParseOptions.AllowItemsMetadataAndSplit);
			inputFiles = (ITaskItem[]) e.ConvertTo (project, typeof (ITaskItem[]), ExpressionOptions.ExpandItemRefs);

			e = new Expression ();
			e.Parse (outputs, ParseOptions.AllowItemsMetadataAndSplit);
			outputFiles = (ITaskItem[]) e.ConvertTo (project, typeof (ITaskItem[]), ExpressionOptions.ExpandItemRefs);

			if (outputFiles == null || outputFiles.Length == 0) {
				reason = String.Format ("No output files were specified for target {0}, skipping.", name);
				return false;
			}

			if (inputFiles == null || inputFiles.Length == 0) {
				reason = String.Format ("No input files were specified for target {0}, skipping.", name);
				return false;
			}

			youngestInput = DateTime.MinValue;
			oldestOutput = DateTime.MaxValue;

			string youngestInputFile, oldestOutputFile;
			youngestInputFile = oldestOutputFile = String.Empty;
			foreach (ITaskItem item in inputFiles) {
				string file = item.ItemSpec.Trim ();
				if (file.Length == 0)
					continue;

				if (!File.Exists (file)) {
					reason = String.Format ("Target {0} needs to be built as input file '{1}' does not exist.", name, file);
					return true;
				}

				DateTime lastWriteTime = File.GetLastWriteTime (file);
				if (lastWriteTime > youngestInput) {
					youngestInput = lastWriteTime;
					youngestInputFile = file;
				}
			}

			foreach (ITaskItem item in outputFiles) {
				string file = item.ItemSpec.Trim ();
				if (file.Length == 0)
					continue;

				if (!File.Exists (file)) {
					reason = String.Format ("Target {0} needs to be built as output file '{1}' does not exist.", name, file);
					return true;
				}

				DateTime lastWriteTime = File.GetLastWriteTime (file);
				if (lastWriteTime < oldestOutput) {
					oldestOutput = lastWriteTime;
					oldestOutputFile = file;
				}
			}

			if (youngestInput > oldestOutput) {
				reason = String.Format ("Target {0} needs to be built as input file '{1}' is newer than output file '{2}'",
						name, youngestInputFile, oldestOutputFile);
				return true;
			}

			return false;
		}

		void LogTargetSkipped (Target target, string reason)
		{
			BuildMessageEventArgs bmea;
			bmea = new BuildMessageEventArgs (reason ?? String.Format ("Skipping target \"{0}\" because its outputs are up-to-date.", target.Name),
				null, "MSBuild", MessageImportance.Normal);
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
