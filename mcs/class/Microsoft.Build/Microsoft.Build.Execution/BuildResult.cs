// BuildResult.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Execution
{
	public class BuildResult
	{
		public BuildResult ()
		{
			ResultsByTarget = new Dictionary<string, TargetResult> ();
		}
		
		public void AddResultsForTarget (string target, TargetResult result)
		{
			ResultsByTarget.Add (target, result);
		}

		public bool HasResultsForTarget (string target)
		{
			return ResultsByTarget.ContainsKey (target);
		}

		public void MergeResults (BuildResult results)
		{
			if (ConfigurationId != results.ConfigurationId)
				throw new InvalidOperationException ("Argument BuildResults have inconsistent ConfigurationId.");
			if (GlobalRequestId != results.GlobalRequestId)
				throw new InvalidOperationException ("Argument BuildResults have inconsistent GlobalRequestId.");
			if (NodeRequestId != results.NodeRequestId)
				throw new InvalidOperationException ("Argument BuildResults have inconsistent NodeRequestId.");
			if (ParentGlobalRequestId != results.ParentGlobalRequestId)
				throw new InvalidOperationException ("Argument BuildResults have inconsistent ParentGlobalRequestId.");
			if (SubmissionId != results.SubmissionId)
				throw new InvalidOperationException ("Argument BuildResults have inconsistent SubmissionId.");
			
			CircularDependency |= results.CircularDependency;
			Exception = Exception ?? results.Exception;
			foreach (var p in results.ResultsByTarget)
				ResultsByTarget.Add (p.Key, p.Value);
		}

		public bool CircularDependency { get; internal set; }

		public int ConfigurationId { get; internal set; }

		public Exception Exception { get; set; }

		public int GlobalRequestId { get; internal set; }

		public ITargetResult this [string target] {
			get { return ResultsByTarget [target]; }
		}

		public int NodeRequestId { get; internal set; }

		BuildResultCode? overall_result;
		public BuildResultCode OverallResult {
			get {
				if (overall_result == null)
					throw new InvalidOperationException ("Build has not finished");
				return overall_result.Value;
			}
			internal set { overall_result = value; }
		}

		public int ParentGlobalRequestId { get; internal set; }

		public IDictionary<string, TargetResult> ResultsByTarget { get; private set; }

		public int SubmissionId { get; internal set; }
	}
}

