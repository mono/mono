//
// ProjectInstance.cs
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

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Execution
{
        public class ProjectInstance
        {
                public ProjectInstance (ProjectRootElement xml)
                        : this (xml, null, null, ProjectCollection.GlobalProjectCollection)
                {
                }

                public ProjectInstance (string projectFile)
                        : this (projectFile, null, null, ProjectCollection.GlobalProjectCollection)
                {
                }

                public ProjectInstance (string projectFile, IDictionary<string, string> globalProperties,
                                string toolsVersion)
                        : this (projectFile, globalProperties, toolsVersion, ProjectCollection.GlobalProjectCollection)
                {
                }

                public ProjectInstance (ProjectRootElement xml, IDictionary<string, string> globalProperties,
                                string toolsVersion, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                public ProjectInstance (string projectFile, IDictionary<string, string> globalProperties,
                                string toolsVersion, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }
        }
}

