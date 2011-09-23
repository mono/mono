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
using System.Threading;

namespace Microsoft.Build.Execution
{
        public class BuildParameters
        {
                public BuildParameters ()
                {
                        throw new NotImplementedException ();
                }

                public BuildParameters (ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                public BuildParameters Clone ()
                {
                        throw new NotImplementedException ();
                }

                public Toolset GetToolset (string toolsVersion)
                {
                        throw new NotImplementedException ();
                }

                public ThreadPriority BuildThreadPriority {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public CultureInfo Culture {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public string DefaultToolsVersion {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool DetailedSummary {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool EnableNodeReuse {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public IDictionary<string, string> EnvironmentProperties {
                        get { throw new NotImplementedException (); }
                }

                public IEnumerable<ForwardingLoggerRecord> ForwardingLoggers {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public IDictionary<string, string> GlobalProperties {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public HostServices HostServices {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool LegacyThreadingSemantics { get; set; }

                public IEnumerable<ILogger> Loggers {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public int MaxNodeCount {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public int MemoryUseLimit {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public string NodeExeLocation {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool OnlyLogCriticalEvents {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool ResetCaches { get; set; }

                public bool SaveOperatingEnvironment {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public ToolsetDefinitionLocations ToolsetDefinitionLocations {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public ICollection<Toolset> Toolsets {
                        get { throw new NotImplementedException (); }
                }

                public CultureInfo UICulture {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public bool UseSynchronousLogging {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }
        }
}

