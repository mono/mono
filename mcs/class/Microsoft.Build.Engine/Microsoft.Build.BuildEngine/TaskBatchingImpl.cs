//
// TaskBatchingImpl.cs: Class that implements Task Batching Algorithm from the wiki.
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

using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal class TaskBatchingImpl : BatchingImplBase
	{
		public TaskBatchingImpl (Project project)
			: base (project)
		{
		}

		public bool Build (IBuildTask buildTask, TaskExecutionMode taskExecutionMode, out bool executeOnErrors)
		{
			executeOnErrors = false;
			try {
				Init ();

				// populate list of referenced items and metadata
				ParseTaskAttributes (buildTask);
				if (consumedMetadataReferences.Count == 0) {
					return Execute (buildTask, taskExecutionMode);
				}

				BatchAndPrepareBuckets ();
				return Run (buildTask, taskExecutionMode, out executeOnErrors);
			} finally {
				consumedItemsByName = null;
				consumedMetadataReferences = null;
				consumedQMetadataReferences = null;
				consumedUQMetadataReferences = null;
				batchedItemsByName = null;
				commonItemsByName = null;
			}
		}

		bool Run (IBuildTask buildTask, TaskExecutionMode taskExecutionMode, out bool executeOnErrors)
		{
			executeOnErrors = false;

			// Run the task in batches
			bool retval = true;
			if (buckets.Count == 0) {
				// batched mode, but no values in the corresponding items!
				retval = Execute (buildTask, taskExecutionMode);
				if (!retval && !buildTask.ContinueOnError)
					executeOnErrors = true;

				return retval;
			}

			// batched
			foreach (Dictionary<string, BuildItemGroup> bucket in buckets) {
				project.PushBatch (bucket, commonItemsByName);
				try {
					retval = Execute (buildTask, taskExecutionMode);
					 if (!retval && !buildTask.ContinueOnError) {
						executeOnErrors = true;
						break;
					}
				} finally {
					project.PopBatch ();
				}
			}

			return retval;
		}

		bool Execute (IBuildTask buildTask, TaskExecutionMode taskExecutionMode)
		{
			if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project)) {
				switch (taskExecutionMode) {
				case TaskExecutionMode.Complete:
					return buildTask.Execute ();
				case TaskExecutionMode.SkipAndSetOutput:
					return buildTask.ResolveOutputItems ();
				default:
					throw new NotImplementedException ();
				}
			}

			return true;
		}


		// Parse task attributes to get list of referenced metadata and items
		// to determine batching
		//
		void ParseTaskAttributes (IBuildTask buildTask)
		{
			foreach (var attr in buildTask.GetAttributes ()) {
				ParseAttribute (attr);
			}
		}
	}

}
