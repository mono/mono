//
// BatchingImpl.cs: Class that implements BatchingAlgorithm from the wiki.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2005 Marek Sieradzki
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Xml;

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class BatchingImpl {
	
		string		inputs;
		string		outputs;
		Project		project;

		List<BuildItemGroup> consumedItemNames;
		List<MetadataReference> consumedMetadataReferences;
		List<MetadataReference> consumedQMetadataReferences;
		List<MetadataReference> consumedUQMetadataReferences;
		Dictionary<string, BuildItemGroup> batchedItemsByName;
		Dictionary<string, BuildItemGroup> commonItemsByName;

		public BatchingImpl (Project project, XmlElement targetElement)
		{
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");
			if (project == null)
				throw new ArgumentNullException ("project");
		
			this.project = project;
			
			inputs = targetElement.GetAttribute ("Inputs");
			outputs = targetElement.GetAttribute ("Outputs");
		}
		
		public bool BuildNeeded ()
		{
			// FIXME: change this to ITaskItem instead of string
		
			Expression inputExpr, outputExpr;
			string[] inputFiles, outputFiles;
			DateTime oldestInput, youngestOutput;
		
			if (inputs == String.Empty)
				return true;
			
			if (outputs == String.Empty)
				return true;
			
			
			inputExpr = new Expression ();
			inputExpr.Parse (inputs, true);
			outputExpr = new Expression ();
			outputExpr.Parse (outputs, true);
			
			inputFiles = (string[]) inputExpr.ConvertTo (project, typeof (string[]));
			outputFiles = (string[]) outputExpr.ConvertTo (project, typeof (string[]));
			
			if (inputFiles == null)
				return true;
			
			if (outputFiles == null)
				return true;
			
			if (inputFiles.Length == 0)
				return true;
			
			if (outputFiles.Length == 0)
				return true;
			
			
			if (File.Exists (inputFiles [0])) 
				oldestInput = File.GetLastWriteTime (inputFiles [0]);
			else 
				return true;
			
			if (File.Exists (outputFiles [0]))
				youngestOutput = File.GetLastWriteTime (outputFiles [0]);
			else
				return true;
			
				
			foreach (string file in inputFiles) {
				if (file.Trim () == String.Empty)
					continue;
			
				if (File.Exists (file.Trim ())) {
					if (File.GetLastWriteTime (file.Trim ()) > oldestInput)
						oldestInput = File.GetLastWriteTime (file.Trim ());
				} else {
					return true;
				}
			}
			foreach (string file in outputFiles) {
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

		public bool BatchBuildTask (BuildTask buildTask)
		{
			try {
				return BatchBuildTaskInternal (buildTask);
			} finally {
				consumedItemNames = null;
				consumedMetadataReferences = null;
				consumedQMetadataReferences = null;
				consumedUQMetadataReferences = null;
				batchedItemsByName = null;
				commonItemsByName = null;
			}
		}

		//FIXME: Target batching
		bool BatchBuildTaskInternal (BuildTask buildTask)
		{
			// all referenced item lists
			consumedItemNames = new List<BuildItemGroup> ();

			// all referenced metadata
			consumedMetadataReferences = new List<MetadataReference> ();
			consumedQMetadataReferences = new List<MetadataReference> ();
			consumedUQMetadataReferences = new List<MetadataReference> ();

			// populate list of referenced items and metadata
			ParseAttributesForBatching (buildTask);

			if (consumedMetadataReferences.Count == 0) {
				// No batching required
				if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project))
					return buildTask.Execute ();
				else // skipped, it should be logged
					return true;
			}

			batchedItemsByName = new Dictionary<string, BuildItemGroup> ();

			// These will passed as is for every batch
			commonItemsByName = new Dictionary<string, BuildItemGroup> ();

			ValidateUnqualifiedMetadataReferences ();

			if (consumedUQMetadataReferences.Count > 0) {
				// Atleast one unqualified metadata ref is found, so
				// batching will be done for all referenced item lists
				foreach (BuildItemGroup group in consumedItemNames)
					batchedItemsByName [group [0].Name] = group;
			}

			// All items referred via qualified metadata refs will be batched
			foreach (MetadataReference mr in consumedQMetadataReferences) {
				BuildItemGroup group;
				if (project.EvaluatedItemsByName.TryGetValue (mr.ItemName, out group))
					batchedItemsByName [mr.ItemName] = group;
			}

			// CommonItemNames = ConsumedItemNames - BatchedItemNames
			foreach (BuildItemGroup group in consumedItemNames) {
				if (!batchedItemsByName.ContainsKey (group [0].Name))
					commonItemsByName [group [0].Name] = group;
			}

			// Bucketizing
			IEnumerable<Dictionary<string, BuildItemGroup>> buckets = Bucketize ();

			// Run the task in batches
			bool retval = true;
			foreach (Dictionary<string, BuildItemGroup> bucket in buckets) {
				project.SetBatchedItems (bucket, commonItemsByName);
				if (ConditionParser.ParseAndEvaluate (buildTask.Condition, project)) {
					 if (! (retval = buildTask.Execute ()))
						 break;
				}
			}
			project.SetBatchedItems (null, null);

			return retval;
		}

		// Parse task attributes to get list of referenced metadata and items
		// to determine batching
		//
		void ParseAttributesForBatching (BuildTask buildTask)
		{
			foreach (XmlAttribute attrib in buildTask.TaskElement.Attributes) {
				Expression expr = new Expression ();
				expr.Parse (attrib.Value, true);

				foreach (object o in expr.Collection) {
					MetadataReference mr = o as MetadataReference;
					if (mr != null) {
						consumedMetadataReferences.Add (mr);
						if (mr.IsQualified)
							consumedQMetadataReferences.Add (mr);
						else
							consumedUQMetadataReferences.Add (mr);
						continue;
					}

					ItemReference ir = o as ItemReference;
					if (ir != null) {
						BuildItemGroup group;
						if (project.EvaluatedItemsByName.TryGetValue (ir.ItemName, out group))
							consumedItemNames.Add (group);
					}
				}
			}
		}
			
		//Ensure that for every metadataReference in consumedUQMetadataReferences,
		//every item in every itemlist in consumedItemNames has a non-null value
		//for that metadata
		void ValidateUnqualifiedMetadataReferences ()
		{
			if (consumedUQMetadataReferences.Count > 0 &&
				consumedItemNames.Count == 0 &&
				consumedQMetadataReferences.Count == 0) {
				throw new Exception (String.Format (
							"Item metadata should be referenced with the item name %(ItemName.{0})",
							consumedQMetadataReferences [0].MetadataName));
			}

			foreach (MetadataReference mr in consumedUQMetadataReferences) {
				foreach (BuildItemGroup group in consumedItemNames) {
					foreach (BuildItem item in group) {
						if (item.HasMetadata (mr.MetadataName))
							continue;

						throw new Exception (String.Format (
							"Metadata named '{0}' not found in item named {1} in item list named {2}",
							mr.MetadataName, item.FinalItemSpec, group [0].Name));
					}
				}
			}
		}


		IEnumerable<Dictionary<string, BuildItemGroup>> Bucketize ()
		{
			Dictionary<string, Dictionary<string, BuildItemGroup>> buckets = new Dictionary<string, Dictionary<string, BuildItemGroup>> ();

			// For each item list represented in "BatchedItemNames", and then for each item
			// within that list, get the values for that item for each of the metadata in
			// "ConsumedMetadataReferences". In the table of metadata values, "%(MyItem.MyMetadata)"
			// would get a separate entry than "%(MyMetadata)", even though the metadata name is the same.

			foreach (BuildItemGroup group in batchedItemsByName.Values) {
				string itemName = group [0].Name;
				foreach (BuildItem item in group) {
					StringBuilder key_sb = new StringBuilder ();
					string value = String.Empty;

					// build the bucket key, unique set of metadata values
					foreach (MetadataReference mr in consumedMetadataReferences) {
						value = String.Empty;
						if (mr.IsQualified) {
							if (String.Compare (mr.ItemName, itemName) == 0)
								value = item.GetEvaluatedMetadata (mr.MetadataName);
						} else {
							if (item.HasMetadata (mr.MetadataName))
								value = item.GetEvaluatedMetadata (mr.MetadataName);
						}

						key_sb.AppendFormat ("{0}.{1}:{2},",
								mr.IsQualified ? mr.ItemName : "",
								mr.MetadataName,
								value);
					}

					// Every bucket corresponds to a unique _set_ of metadata values
					// So, every bucket would have itemGroups with same set of metadata
					// values

					string bucket_key = key_sb.ToString ();
					Dictionary<string, BuildItemGroup> bucket;
					if (!buckets.TryGetValue (bucket_key, out bucket))
						// new bucket
						buckets [bucket_key] = bucket = new Dictionary<string, BuildItemGroup> ();

					string itemGroup_key = item.Name;
					BuildItemGroup itemGroup;
					if (!bucket.TryGetValue (itemGroup_key, out itemGroup))
						bucket [itemGroup_key] = itemGroup = new BuildItemGroup ();

					itemGroup.AddItem (item);
				}
			}

			return buckets.Values;
		}

	}
}

#endif
