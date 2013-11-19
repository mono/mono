//
// BuildEngine4.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Build.Internal
{
	class BuildNodeManager
	{
		public BuildNodeManager (BuildManager buildManager)
		{
			BuildManager = buildManager;
			task_factory.StartNew (RunLoop);
		}
		
		public BuildManager BuildManager { get; private set; }
		
		List<BuildNode> in_proc_nodes = new List<BuildNode> ();
		List<BuildNode> out_proc_nodes = new List<BuildNode> ();
		AutoResetEvent queue_wait_handle = new AutoResetEvent (false);
		Queue<BuildSubmission> queued_builds = new Queue<BuildSubmission> ();
		Dictionary<BuildSubmission,Task> ongoing_builds = new Dictionary<BuildSubmission, Task> ();
		bool run_loop = true;

		readonly TaskFactory task_factory = new TaskFactory ();
		internal TaskFactory ThreadTaskFactory {
			get { return task_factory; }
		}
		
		void RunLoop ()
		{
			while (run_loop) {
				if (queued_builds.Count == 0) {
//Console.Error.WriteLine ("!!!! {0} waiting for build queue...", BuildManager.GetHashCode ());
					queue_wait_handle.WaitOne ();
				}
				if (!run_loop)
					break;
				if (!queued_builds.Any ())
					continue;
				var build = queued_builds.Dequeue ();
				Execute (build);
			}
		}

		public void Stop ()
		{
			run_loop = false;
			queue_wait_handle.Set ();
		}

		public void ResetCaches ()
		{
			in_proc_nodes.Clear ();
			out_proc_nodes.Clear ();
		}
		
		public void Enqueue (BuildSubmission build)
		{
			queued_builds.Enqueue (build);
			queue_wait_handle.Set ();
		}
		
		void Execute (BuildSubmission build)
		{
//Console.Error.WriteLine ("!!!! {0} BuildNodeManager.Execute", BuildManager.GetHashCode ());
			var node = TakeNode (build);
			ongoing_builds [build] = task_factory.StartNew (node.Execute);
		}
		
		// FIXME: take max nodes into account here, and get throttling working.
		BuildNode TakeNode (BuildSubmission build)
		{
			var host = BuildManager.OngoingBuildParameters.HostServices;
			NodeAffinity affinity;
			if (host == null)
				affinity = NodeAffinity.Any;
			else
				affinity = host.GetNodeAffinity (build.BuildRequest.ProjectFullPath);
			BuildNode n = GetReusableNode (affinity);
			if (n != null)
				n.Assign (build);
			else {
				n = new BuildNode (this, affinity == NodeAffinity.Any ? NodeAffinity.InProc : affinity);
				n.Assign (build);
				if (n.Affinity == NodeAffinity.InProc)
					in_proc_nodes.Add (n);
				else
					out_proc_nodes.Add (n);
			}
			return n;
		}
		
		BuildNode GetReusableNode (NodeAffinity affinity)
		{
			if (!BuildManager.OngoingBuildParameters.EnableNodeReuse)
				return null;
			
			if (affinity != NodeAffinity.OutOfProc)
				foreach (var n in in_proc_nodes)
					if (n.IsAvailable && (n.Affinity & affinity) != 0)
						return n;
			if (affinity != NodeAffinity.InProc)
				foreach (var n in out_proc_nodes)
					if (n.IsAvailable && (n.Affinity & affinity) != 0)
						return n;
			return null;
		}
	
		internal class BuildNode
		{
			static Random rnd = new Random ();
			
			public BuildNode (BuildNodeManager manager, NodeAffinity affinity)
			{
				Manager = manager;
				Affinity = affinity;
				Id = rnd.Next ();
			}
			
			public bool IsAvailable { get; private set; }
			public int Id { get; private set; }
			public BuildNodeManager Manager { get; set; }
			public NodeAffinity Affinity { get; private set; }
			public BuildSubmission Build { get; private set; }
			
			public void Assign (BuildSubmission build)
			{
				IsAvailable = false;
				Build = build;
			}
			
			public void Execute ()
			{
				// FIXME: depending on NodeAffinity, build it through another MSBuild process.
				if (Affinity == NodeAffinity.OutOfProc)
					throw new NotImplementedException ();
//Console.Error.WriteLine ("!!!! {0} BuildSubmission.Execute start", Manager.BuildManager.GetHashCode ());
				Build.Execute ();
//Console.Error.WriteLine ("!!!! {0} BuildSubmission.Execute done", Manager.BuildManager.GetHashCode ());
			}
		}
	}
}
