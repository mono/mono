// BuildParameters.cs
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

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Microsoft.Build.Execution
{
	public class BuildParameters
	{
		public BuildParameters ()
			: this (ProjectCollection.GlobalProjectCollection)
		{
		}

		public BuildParameters (ProjectCollection projectCollection)
		{
			if (projectCollection == null)
				throw new ArgumentNullException ("projectCollection");
			projects = projectCollection;

			// these properties are copied, while some members (such as Loggers) are not.
			this.DefaultToolsVersion = projectCollection.DefaultToolsVersion;
			this.ToolsetDefinitionLocations = projectCollection.ToolsetLocations;
			this.GlobalProperties = projectCollection.GlobalProperties;
		}

		readonly ProjectCollection projects;

		public BuildParameters Clone ()
		{
			var ret = (BuildParameters) MemberwiseClone ();
			ret.ForwardingLoggers = ret.ForwardingLoggers.ToArray ();
			ret.GlobalProperties = ret.GlobalProperties.ToDictionary (p => p.Key, p => p.Value);
			ret.Loggers = ret.Loggers == null ? null : ret.Loggers.ToArray ();
			return ret;
		}

		public Toolset GetToolset (string toolsVersion)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ThreadPriority BuildThreadPriority { get; set; }

		[MonoTODO]
		public CultureInfo Culture { get; set; }

		[MonoTODO]
		public string DefaultToolsVersion { get; set; }

		[MonoTODO]
		public bool DetailedSummary { get; set; }

		[MonoTODO]
		public bool EnableNodeReuse { get; set; }

		[MonoTODO]
		public IDictionary<string, string> EnvironmentProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IEnumerable<ForwardingLoggerRecord> ForwardingLoggers { get; set; }

		[MonoTODO]
		public IDictionary<string, string> GlobalProperties { get; set; }

		[MonoTODO]
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

