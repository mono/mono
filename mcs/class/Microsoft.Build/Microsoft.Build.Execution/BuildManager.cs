// BuildManager.cs
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

using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Build.Internal;
using System.Linq;

namespace Microsoft.Build.Execution
{
	public class BuildManager
	{
		static BuildManager default_manager = new BuildManager ();

		public static BuildManager DefaultBuildManager {
			get { return default_manager; }
		}
		
		public BuildManager ()
		{
		}

		public BuildManager (string hostName)
		{
			throw new NotImplementedException ();
		}
		
		public void Dispose ()
		{
			WaitHandle.WaitAll (submissions.Select (s => s.WaitHandle).ToArray ());
			BuildNodeManager.Stop ();
		}

		~BuildManager ()
		{
			// maybe processes created by out-of-process nodes should be signaled.
		}

		readonly List<BuildSubmission> submissions = new List<BuildSubmission> ();
		
		BuildParameters ongoing_build_parameters;
		
		internal BuildParameters OngoingBuildParameters {
			get { return ongoing_build_parameters; }
		}

		public void BeginBuild (BuildParameters parameters)
		{
			if (ongoing_build_parameters != null)
				throw new InvalidOperationException ("There is already ongoing build");
			ongoing_build_parameters = parameters.Clone ();
		}

		public BuildResult Build (BuildParameters parameters, BuildRequestData requestData)
		{
			BeginBuild (parameters);
			var ret = BuildRequest (requestData);
			EndBuild ();
			return ret;
		}

		public BuildResult BuildRequest (BuildRequestData requestData)
		{
			var sub = PendBuildRequest (requestData);
			sub.Execute ();
			return sub.BuildResult;
		}
		
		public void CancelAllSubmissions ()
		{
			foreach (var sub in submissions) {
				try {
					if (!sub.IsCompleted)
						sub.Cancel ();
				} catch (InvalidOperationException) {
					// some submissions could be already done during this iteration. Ignore that.
				}
			}
			submissions.Clear ();
		}

		public void EndBuild ()
		{
			if (ongoing_build_parameters == null)
				throw new InvalidOperationException ("Build has not started");
			if (submissions.Count > 0)
				WaitHandle.WaitAll (submissions.Select (s => s.WaitHandle).ToArray ());
			ongoing_build_parameters = null;
		}
		
		Dictionary<Project,ProjectInstance> instances = new Dictionary<Project, ProjectInstance> ();

		public ProjectInstance GetProjectInstanceForBuild (Project project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");
			if (project.FullPath == null)
				throw new ArgumentNullException ("project", "FullPath parameter in the project cannot be null.");
			if (project.FullPath == string.Empty)
				throw new ArgumentException ("FullPath parameter in the project cannot be empty.", "project");
			// other than that, any invalid path character is accepted...
			
			return GetProjectInstanceForBuildInternal (project);
		}
			
		internal ProjectInstance GetProjectInstanceForBuildInternal (Project project)
		{
			if (!instances.ContainsKey (project))
				instances [project] = project.CreateProjectInstance ();
			return instances [project];
		}

		public BuildSubmission PendBuildRequest (BuildRequestData requestData)
		{
			if (ongoing_build_parameters == null)
				throw new InvalidOperationException ("This method cannot be called before calling BeginBuild method.");
			var sub = new BuildSubmission (this, requestData);
			submissions.Add (sub);
			return sub;
		}

		public void ResetCaches ()
		{
			if (OngoingBuildParameters != null)
				throw new InvalidOperationException ("Cannot reset caches while builds are in progress.");
			
			BuildNodeManager.ResetCaches ();
		}
		
		BuildNodeManager build_node_manager;
		
		internal BuildNodeManager BuildNodeManager {
			get {
				if (build_node_manager == null)
						build_node_manager = new BuildNodeManager (this);
				return build_node_manager;
			}
		}
	}
}
