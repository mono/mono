//
// ProjectCollection.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
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

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Microsoft.Build.Evaluation
{
	public class ProjectCollection : IDisposable
	{
		// static members

		static readonly ProjectCollection global_project_collection;

		static ProjectCollection ()
		{
			#if NET_4_5
			global_project_collection = new ProjectCollection (new ReadOnlyDictionary<string, string> (new Dictionary<string, string> ()));
			#else
			global_project_collection = new ProjectCollection (new Dictionary<string, string> ());
			#endif
		}

		public static string Escape (string unescapedString)
		{
			return unescapedString;
		}

		public static ProjectCollection GlobalProjectCollection {
			get { return global_project_collection; }
		}

		// semantic model part

		public ProjectCollection ()
			: this (null)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties)
        	: this (globalProperties, null, ToolsetDefinitionLocations.Registry | ToolsetDefinitionLocations.ConfigurationFile)
		{
		}

		public ProjectCollection (ToolsetDefinitionLocations toolsetDefinitionLocations)
        	: this (null, null, toolsetDefinitionLocations)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties, IEnumerable<ILogger> loggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations)
			: this (globalProperties, loggers, null, toolsetDefinitionLocations, int.MaxValue, false)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties,
				IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations,
				int maxNodeCount, bool onlyLogCriticalEvents)
		{
			global_properties = globalProperties ?? new Dictionary<string, string> ();
			this.loggers = loggers != null ? loggers.ToList () : new List<ILogger> ();
			toolset_locations = toolsetDefinitionLocations;
			max_node_count = maxNodeCount;
			OnlyLogCriticalEvents = onlyLogCriticalEvents;

			LoadDefaultToolsets ();
		}

		readonly int max_node_count;

		[MonoTODO]
		public int Count {
			get { return loaded_projects.Count; }
		}

		[MonoTODO]
		public string DefaultToolsVersion {
			get { throw new NotImplementedException (); }
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
			}
		}

		public ICollection<Project> GetLoadedProjects (string fullPath)
		{
			return LoadedProjects.Where (p => Path.GetFullPath (p.FullPath) == Path.GetFullPath (fullPath)).ToList ();
		}

		readonly IDictionary<string, string> global_properties;

		public IDictionary<string, string> GlobalProperties {
			get { return global_properties; }
		}

		readonly List<Project> loaded_projects = new List<Project> ();

		[MonoTODO]
		public ICollection<Project> LoadedProjects {
			get { return loaded_projects; }
		}

		readonly List<ILogger> loggers = new List<ILogger> ();
		[MonoTODO]
		public ICollection<ILogger> Loggers {
			get { return loggers; }
		}

		[MonoTODO]
		public bool OnlyLogCriticalEvents { get; set; }

		[MonoTODO]
		public bool SkipEvaluation { get; set; }

		readonly ToolsetDefinitionLocations toolset_locations;
		public ToolsetDefinitionLocations ToolsetLocations {
			get { return toolset_locations; }
		}

		readonly List<Toolset> toolsets = new List<Toolset> ();
		[MonoTODO ("unused")]
		// so what should we do without ToolLocationHelper in Microsoft.Build.Utilities.dll? There is no reference to it in this dll.
		public ICollection<Toolset> Toolsets {
			// For ConfigurationFile and None, they cannot be added externally.
			get { return (ToolsetLocations & ToolsetDefinitionLocations.Registry) != 0 ? toolsets : toolsets.ToList (); }
		}

		//FIXME: should also support config file, depending on ToolsetLocations
		void LoadDefaultToolsets ()
		{
			toolsets.Add (new Toolset ("2.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20), this, null));
			toolsets.Add (new Toolset ("3.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version30), this, null));
			toolsets.Add (new Toolset ("3.5",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version35), this, null));
#if NET_4_0
			toolsets.Add (new Toolset ("4.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40), this, null));
#endif
#if NET_4_5
			toolsets.Add (new Toolset ("4.5",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version45), this, null));
#endif
		}

		public void UnloadAllProjects ()
		{
			throw new NotImplementedException ();
		}

		public void UnloadProject (Project project)
		{
			throw new NotImplementedException ();
		}

		public void UnloadProject (ProjectRootElement projectRootElement)
		{
			throw new NotImplementedException ();
		}

		public static Version Version {
			get { throw new NotImplementedException (); }
		}

		// Execution part

		[MonoTODO]
		public bool DisableMarkDirty { get; set; }

		[MonoTODO]
		public HostServices HostServices { get; set; }

		[MonoTODO]
		public bool IsBuildEnabled { get; set; }
	}
}
