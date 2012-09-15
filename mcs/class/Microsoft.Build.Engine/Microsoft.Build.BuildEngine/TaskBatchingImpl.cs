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

		public bool Build (BuildTask buildTask, out bool executeOnErrors)
		{
			executeOnErrors = false;
			try {
				Init ();

				// populate list of referenced items and metadata
				ParseTaskAttributes (buildTask);
				if (consumedMetadataReferences.Count == 0) {
					// No batching required
					if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project))
						return buildTask.Execute ();
					else // skipped, it should be logged
						return true;
				}

				BatchAndPrepareBuckets ();
				return Run (buildTask, out executeOnErrors);
			} finally {
				consumedItemsByName = null;
				consumedMetadataReferences = null;
				consumedQMetadataReferences = null;
				consumedUQMetadataReferences = null;
				batchedItemsByName = null;
				commonItemsByName = null;
			}
		}

		bool Run (BuildTask buildTask, out bool executeOnErrors)
		{
			executeOnErrors = false;

			// Run the task in batches
			bool retval = true;
			if (buckets.Count == 0) {
				// batched mode, but no values in the corresponding items!
				if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project)) {
					retval = buildTask.Execute ();
					if (!retval && !buildTask.ContinueOnError)
						executeOnErrors = true;
				}

				return retval;
			}

			// batched
			foreach (Dictionary<string, BuildItemGroup> bucket in buckets) {
				project.PushBatch (bucket, commonItemsByName);
				try {
					if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project)) {
						 retval = buildTask.Execute ();
						 if (!retval && !buildTask.ContinueOnError) {
							executeOnErrors = true;
							break;
						 }
					}
				} finally {
					project.PopBatch ();
				}
			}

			return retval;
		}


		// Parse task attributes to get list of referenced metadata and items
		// to determine batching
		//
		void ParseTaskAttributes (BuildTask buildTask)
		{
			foreach (XmlAttribute attrib in buildTask.TaskElement.Attributes)
				ParseAttribute (attrib.Value);

			foreach (XmlNode xn in buildTask.TaskElement.ChildNodes) {
				XmlElement xe = xn as XmlElement;
				if (xe == null)
					continue;

				//FIXME: error on any other child
				if (String.Compare (xe.LocalName, "Output", StringComparison.Ordinal) == 0) {
					foreach (XmlAttribute attrib in xe.Attributes)
						ParseAttribute (attrib.Value);
				}
			}
		}
	}

}
