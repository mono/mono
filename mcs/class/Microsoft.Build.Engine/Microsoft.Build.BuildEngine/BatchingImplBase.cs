//
// BatchingImplBase.cs: Base class that implements BatchingAlgorithm from the wiki.
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class BatchingImplBase {

		protected Dictionary<string, BuildItemGroup> consumedItemsByName;
		protected List<MetadataReference> consumedMetadataReferences;
		protected List<MetadataReference> consumedQMetadataReferences;
		protected List<MetadataReference> consumedUQMetadataReferences;
		protected Dictionary<string, BuildItemGroup> batchedItemsByName;
		protected Dictionary<string, BuildItemGroup> commonItemsByName;

		protected Project project;
		protected ICollection<Dictionary<string, BuildItemGroup>> buckets;

		protected BatchingImplBase (Project project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");

			this.project = project;
		}

		protected void Init ()
		{
			// all referenced item lists
			consumedItemsByName = new Dictionary<string, BuildItemGroup> (StringComparer.OrdinalIgnoreCase);

			// all referenced metadata
			consumedMetadataReferences = new List<MetadataReference> ();
			consumedQMetadataReferences = new List<MetadataReference> ();
			consumedUQMetadataReferences = new List<MetadataReference> ();
		}

		protected void BatchAndPrepareBuckets ()
		{
			batchedItemsByName = new Dictionary<string, BuildItemGroup> (StringComparer.OrdinalIgnoreCase);

			// These will passed as is for every batch
			commonItemsByName = new Dictionary<string, BuildItemGroup> (StringComparer.OrdinalIgnoreCase);

			ValidateUnqualifiedMetadataReferences ();

			if (consumedUQMetadataReferences.Count > 0) {
				// Atleast one unqualified metadata ref is found, so
				// batching will be done for all referenced item lists
				foreach (KeyValuePair<string, BuildItemGroup> pair in consumedItemsByName)
					batchedItemsByName [pair.Key] = pair.Value;
			}

			// All items referred via qualified metadata refs will be batched
			foreach (MetadataReference mr in consumedQMetadataReferences) {
				BuildItemGroup group;
				if (project.TryGetEvaluatedItemByNameBatched (mr.ItemName, out group))
					batchedItemsByName [mr.ItemName] = group;
			}

			// CommonItemNames = ConsumedItemNames - BatchedItemNames
			foreach (KeyValuePair<string, BuildItemGroup> pair in consumedItemsByName) {
				if (!batchedItemsByName.ContainsKey (pair.Key))
					commonItemsByName [pair.Key] = pair.Value;
			}

			// Bucketizing
			buckets = Bucketize ();
		}

		protected void ParseAttribute (string value)
		{
			Expression expr = new Expression ();
			expr.Parse (value, ParseOptions.AllowItemsMetadataAndSplit);

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
					if (!project.TryGetEvaluatedItemByNameBatched (ir.ItemName, out group))
						if (!project.EvaluatedItemsByName.TryGetValue (ir.ItemName, out group))
							group = new BuildItemGroup ();

					consumedItemsByName [ir.ItemName] = group;
				}
			}
		}

		//Ensure that for every metadataReference in consumedUQMetadataReferences,
		//every item in every itemlist in consumedItemsByName has a non-null value
		//for that metadata
		void ValidateUnqualifiedMetadataReferences ()
		{
			if (consumedUQMetadataReferences.Count > 0 &&
				consumedItemsByName.Count == 0 &&
				consumedQMetadataReferences.Count == 0) {
				throw new Exception ("Item metadata should be referenced with the item name %(ItemName.MetadataName)");
			}

			foreach (MetadataReference mr in consumedUQMetadataReferences) {
				foreach (KeyValuePair<string, BuildItemGroup> pair in consumedItemsByName) {
					foreach (BuildItem item in pair.Value) {
						if (item.HasMetadata (mr.MetadataName))
							continue;

						throw new Exception (String.Format (
							"Metadata named '{0}' not found in item named {1} in item list named {2}",
							mr.MetadataName, item.FinalItemSpec, pair.Key));
					}
				}
			}
		}

		ICollection<Dictionary<string, BuildItemGroup>> Bucketize ()
		{
			var buckets = new Dictionary<string, Dictionary<string, BuildItemGroup>> (
					StringComparer.OrdinalIgnoreCase);

			// For each item list represented in "BatchedItemNames", and then for each item
			// within that list, get the values for that item for each of the metadata in
			// "ConsumedMetadataReferences". In the table of metadata values, "%(MyItem.MyMetadata)"
			// would get a separate entry than "%(MyMetadata)", even though the metadata name is the same.

			foreach (KeyValuePair<string, BuildItemGroup> pair in batchedItemsByName) {
				string itemName = pair.Key;
				BuildItemGroup group = pair.Value;
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
						buckets [bucket_key] = bucket = new Dictionary<string, BuildItemGroup> (
								StringComparer.OrdinalIgnoreCase);

					string itemGroup_key = item.Name;
					BuildItemGroup itemGroup;
					if (!bucket.TryGetValue (itemGroup_key, out itemGroup))
						bucket [itemGroup_key] = itemGroup = new BuildItemGroup ();

					itemGroup.AddItem (item);
				}
			}

			if (buckets.Values.Count == 0) {
				// no buckets
				buckets.Add ("none", new Dictionary<string, BuildItemGroup> ());
				AddEmptyGroups (buckets);
				if (buckets ["none"].Values.Count == 0)
					buckets.Remove ("none");
			} else {
				AddEmptyGroups (buckets);
			}

			return buckets.Values;
		}

		void AddEmptyGroups (Dictionary<string, Dictionary<string, BuildItemGroup>> buckets)
		{
			foreach (Dictionary<string, BuildItemGroup> bucket in buckets.Values) {
				foreach (string name in batchedItemsByName.Keys) {
					BuildItemGroup group;
					if (!bucket.TryGetValue (name, out group))
						bucket [name] = new BuildItemGroup ();
				}
			}
		}

               public void DumpBuckets (Dictionary<string, Dictionary<string, BuildItemGroup>> buckets)
               {
                       foreach (KeyValuePair<string, Dictionary<string, BuildItemGroup>> pair in buckets) {
                               Console.WriteLine ("Bucket> {0} {", pair.Key);
			       DumpBucket (pair.Value);
			       Console.WriteLine ("}");
                       }
               }

		public static void DumpBucket (Dictionary<string, BuildItemGroup> bucket)
		{
		       foreach (KeyValuePair<string, BuildItemGroup> bpair in bucket) {
			       Console.WriteLine ("\t{0} [", bpair.Key);
			       foreach (BuildItem item in bpair.Value)
				       Console.WriteLine ("\t\t{0} - {1}", item.Name, item.FinalItemSpec);
			       Console.WriteLine ("\t]");
		       }
		}


	}
}
