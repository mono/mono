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

using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

using System;
using System.Collections.Generic;

namespace Microsoft.Build.Evaluation
{
        public class ProjectCollection : IDisposable
        {
                public ProjectCollection ()
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
                        : this (globalProperties, loggers, null, toolsetDefinitionLocations, 1, false)
                {
                }

                public ProjectCollection (IDictionary<string, string> globalProperties,
                                IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers,
                                ToolsetDefinitionLocations toolsetDefinitionLocations,
                                int maxNodeCount, bool onlyLogCriticalEvents)
                {
                        throw new NotImplementedException ();
                }

                public static string Escape (string unescapedString)
                {
                        return unescapedString;
                }

                public static ProjectCollection GlobalProjectCollection {
                        get { return globalProjectCollection; }
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

                static ProjectCollection globalProjectCollection = new ProjectCollection ();

                public ICollection<Project> GetLoadedProjects (string fullPath)
                {
                        throw new NotImplementedException ();
                }

                public ToolsetDefinitionLocations ToolsetLocations {
                        get { throw new NotImplementedException (); }
                }
        }
}
