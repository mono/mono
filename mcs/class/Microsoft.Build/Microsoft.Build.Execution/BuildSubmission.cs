// BuildSubmission.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2011,2013 Xamarin Inc.
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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Internal;
using System.Collections.Generic;

namespace Microsoft.Build.Execution
{
	public class BuildSubmission
	{
		static Random rnd = new Random ();

		internal BuildSubmission (BuildManager build, BuildRequestData requestData)
		{
			BuildManager = build;
			this.request = requestData;
			SubmissionId = rnd.Next ();
		}

		BuildRequestData request;
		BuildSubmissionCompleteCallback callback;
		bool is_started, is_completed, is_canceled;
		ManualResetEvent wait_handle = new ManualResetEvent (true);

		public object AsyncContext { get; private set; }
		public BuildManager BuildManager { get; private set; }
		public BuildResult BuildResult { get; set; }
		public bool IsCompleted {
			get { return is_completed; }
		}
		public int SubmissionId { get; private set; }
		public WaitHandle WaitHandle {
			get { return wait_handle; }
		}
		
		internal BuildRequestData BuildRequest {
			get { return this.request; }
		}

		internal void Cancel ()
		{
			if (is_canceled)
				throw new InvalidOperationException ("Build has already canceled");
			is_canceled = true;
		}

		public BuildResult Execute ()
		{
			ExecuteAsync (null, null);
			WaitHandle.WaitOne ();
			return BuildResult;
		}
		
		internal BuildResult InternalExecute ()
		{
			BuildResult = new BuildResult () { SubmissionId = SubmissionId };
			try {
				var engine = new BuildEngine4 (this);
				string toolsVersion = request.ExplicitlySpecifiedToolsVersion ?? request.ProjectInstance.ToolsVersion ?? BuildManager.OngoingBuildParameters.DefaultToolsVersion;
				var outputs = new Dictionary<string,string> ();
				engine.BuildProject (() => is_canceled, BuildResult, request.ProjectInstance, request.TargetNames, BuildManager.OngoingBuildParameters.GlobalProperties, outputs, toolsVersion);
			} catch (Exception ex) {
				BuildResult.Exception = ex;
				BuildResult.OverallResult = BuildResultCode.Failure;
			}
			is_completed = true;
			if (callback != null)
				callback (this);
			wait_handle.Set ();
			return BuildResult;
		}

		public void ExecuteAsync (BuildSubmissionCompleteCallback callback, object context)
		{
			if (is_completed)
				throw new InvalidOperationException ("Build has already completed");
			if (is_canceled)
				throw new InvalidOperationException ("Build has already canceled");
			if (is_started)
				throw new InvalidOperationException ("Build has already started");
			is_started = true;
			this.AsyncContext = context;
			this.callback = callback;
			wait_handle.Reset ();
			
			BuildManager.BuildNodeManager.Enqueue (this);
		}
	}
}

