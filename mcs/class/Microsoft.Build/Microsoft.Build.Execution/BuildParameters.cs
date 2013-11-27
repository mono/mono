// BuildParameters.cs
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
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Collections;

namespace Microsoft.Build.Execution
{
	public class BuildParameters
	{
		public BuildParameters ()
			: this (new ProjectCollection ())
		{
		}

		public BuildParameters (ProjectCollection projectCollection)
		{
			if (projectCollection == null)
				throw new ArgumentNullException ("projectCollection");
			projects = projectCollection;
			
			EnableNodeReuse = true;
			Culture = CultureInfo.CurrentCulture;
			UICulture = CultureInfo.CurrentUICulture;
			MaxNodeCount = projectCollection.MaxNodeCount;

			// these properties are copied, while some members (such as Loggers) are not.
			this.DefaultToolsVersion = projectCollection.DefaultToolsVersion;
			this.ToolsetDefinitionLocations = projectCollection.ToolsetLocations;
			this.GlobalProperties = projectCollection.GlobalProperties;
			environment_properties = new Dictionary<string,string> ();
			foreach (DictionaryEntry p in Environment.GetEnvironmentVariables ())
				environment_properties [(string) p.Key] = (string) p.Value;
		}

		readonly ProjectCollection projects;
		Dictionary<string,string> environment_properties;
		
		internal ProjectCollection ProjectCollection {
			get { return projects; }
		}

		public BuildParameters Clone ()
		{
			var ret = (BuildParameters) MemberwiseClone ();
			ret.ForwardingLoggers = ForwardingLoggers == null ? null : ForwardingLoggers.ToArray ();
			ret.GlobalProperties = GlobalProperties == null ? null : GlobalProperties.ToDictionary (p => p.Key, p => p.Value);
			ret.Loggers = Loggers == null ? null : new List<ILogger> (Loggers);
			ret.environment_properties = new Dictionary<string, string> (environment_properties);
			return ret;
		}

		public Toolset GetToolset (string toolsVersion)
		{
			// can return null.
			return projects.Toolsets.FirstOrDefault (t => t.ToolsVersion == toolsVersion);
		}

		[MonoTODO]
		public ThreadPriority BuildThreadPriority { get; set; }

		[MonoTODO]
		public CultureInfo Culture { get; set; }

		public string DefaultToolsVersion { get; set; }

		[MonoTODO]
		public bool DetailedSummary { get; set; }

		public bool EnableNodeReuse { get; set; }

		[MonoTODO]
		public IDictionary<string, string> EnvironmentProperties {
			get { return environment_properties; }
		}

		[MonoTODO]
		public IEnumerable<ForwardingLoggerRecord> ForwardingLoggers { get; set; }

		[MonoTODO]
		public IDictionary<string, string> GlobalProperties { get; set; }

		public HostServices HostServices { get; set; }

		[MonoTODO]
		public bool LegacyThreadingSemantics { get; set; }

		[MonoTODO]
		public IEnumerable<ILogger> Loggers { get; set; }

		[MonoTODO]
		public int MaxNodeCount { get; set; }

		[MonoTODO]
		public int MemoryUseLimit { get; set; }

		[MonoTODO]
		public string NodeExeLocation { get; set; }

		[MonoTODO]
		public bool OnlyLogCriticalEvents { get; set; }

		[MonoTODO]
		public bool ResetCaches { get; set; }

		[MonoTODO]
		public bool SaveOperatingEnvironment { get; set; }

		[MonoTODO]
		public ToolsetDefinitionLocations ToolsetDefinitionLocations { get; set; }

		[MonoTODO]
		public ICollection<Toolset> Toolsets {
			get { return projects.Toolsets; }
		}

		[MonoTODO]
		public CultureInfo UICulture { get; set; }

		[MonoTODO]
		public bool UseSynchronousLogging { get; set; }
	}
}

