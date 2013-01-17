//
// BuildRequestData.cs
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

namespace Microsoft.Build.Execution
{
        public class BuildRequestData
        {
                public BuildRequestData (ProjectInstance projectInstance, string[] targetsToBuild)
                        : this (projectInstance, targetsToBuild, null, BuildRequestDataFlags.None)
                {
                }

                public BuildRequestData (ProjectInstance projectInstance, string[] targetsToBuild, HostServices hostServices)
                        : this (projectInstance, targetsToBuild, hostServices, BuildRequestDataFlags.None)
                {
                }

                public BuildRequestData (ProjectInstance projectInstance, string[] targetsToBuild, HostServices hostServices,
                                BuildRequestDataFlags flags)
                {
                        throw new NotImplementedException ();
                }

                public BuildRequestData (string projectFullPath, IDictionary<string, string> globalProperties,
                                string toolsVersion, string[] targetsToBuild, HostServices hostServices)
                        : this (projectFullPath, globalProperties, toolsVersion, targetsToBuild, hostServices, BuildRequestDataFlags.None)
                {
                }

                public BuildRequestData (string projectFullPath, IDictionary<string, string> globalProperties,
                                string toolsVersion, string[] targetsToBuild, HostServices hostServices, BuildRequestDataFlags flags)
                {
                        throw new NotImplementedException ();
                }

                public string ExplicitlySpecifiedToolsVersion { get; private set; }
                public BuildRequestDataFlags Flags { get; private set; }
                public HostServices HostServices { get; private set; }
                public string ProjectFullPath { get; private set; }
                public ProjectInstance ProjectInstance { get; private set; }
                public ICollection<string> TargetNames { get; private set; }

                ICollection<ProjectPropertyInstance> GlobalProperties {
                        get { throw new NotImplementedException (); }
                }
        }
}

