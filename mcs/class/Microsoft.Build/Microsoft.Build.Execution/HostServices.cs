//
// HostServices.cs
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
using Microsoft.Build.Framework;

namespace Microsoft.Build.Execution
{
	public class HostServices
	{
		class HostObjectRegistration
		{
			public string ProjectFile { get; set; }
			public string TargetName { get; set; }
			public string TaskName { get; set; }
			public ITaskHost HostObject { get; set; }
		}
		
		readonly List<HostObjectRegistration> hosts = new List<HostObjectRegistration> ();
		readonly Dictionary<string,NodeAffinity> node_affinities = new Dictionary<string, NodeAffinity> ();
		
		HostObjectRegistration GetHostRegistration (string projectFile, string targetName, string taskName)
		{
			if (projectFile == null)
				throw new ArgumentNullException ("projectFile");
			if (targetName == null)
				throw new ArgumentNullException ("targetName");
			if (taskName == null)
				throw new ArgumentNullException ("taskName");
			return hosts.FirstOrDefault (h =>
				string.Equals (projectFile, h.ProjectFile, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (targetName, h.TargetName, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (taskName, h.TaskName, StringComparison.OrdinalIgnoreCase));
		}
		
		public ITaskHost GetHostObject (string projectFile, string targetName, string taskName)
		{
			var reg = GetHostRegistration (projectFile, targetName, taskName);
			return reg != null ? reg.HostObject : null;
		}

		public NodeAffinity GetNodeAffinity (string projectFile)
		{
			if (projectFile == null)
				throw new ArgumentNullException ("projectFile");
			NodeAffinity na;
			return node_affinities.TryGetValue (projectFile, out na) ? na : NodeAffinity.Any;
		}
		
		IEnumerable<HostObjectRegistration> GetRegistrationsByProject (string project)
		{
			return hosts.Where (h => string.Equals (project, h.ProjectFile, StringComparison.OrdinalIgnoreCase));
		}

		public void OnRenameProject (string oldFullPath, string newFullPath)
		{
			if (oldFullPath == null)
				throw new ArgumentNullException ("oldFullPath");
			if (newFullPath == null)
				throw new ArgumentNullException ("newFullPath");
			foreach (var reg in GetRegistrationsByProject (oldFullPath))
				reg.ProjectFile = newFullPath;
		}

		public void RegisterHostObject (string projectFile, string targetName, string taskName, ITaskHost hostObject)
		{
			if (hostObject == null)
				throw new ArgumentNullException ("hostObject");
			var reg = GetHostRegistration (projectFile, targetName, taskName);
			if (reg != null)
				reg.HostObject = hostObject;
			else
				hosts.Add (new HostObjectRegistration () { ProjectFile = projectFile, TargetName = targetName, TaskName = taskName, HostObject = hostObject });
		}

		public void SetNodeAffinity (string projectFile, NodeAffinity nodeAffinity)
		{
			if (projectFile == null)
				throw new ArgumentNullException ("projectFile");
			node_affinities [projectFile] = nodeAffinity;
		}

		public void UnregisterProject (string projectFullPath)
		{
			if (projectFullPath == null)
				throw new ArgumentNullException ("projectFullPath");
			var removed = GetRegistrationsByProject (projectFullPath).ToArray ();
			foreach (var r in removed)
				hosts.Remove (r);
		}
	}
}

