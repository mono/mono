// Toolset.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Evaluation
{
	public class Toolset
	{
		public Toolset (string toolsVersion, string toolsPath,
				ProjectCollection projectCollection, string msbuildOverrideTasksPath)
			: this (toolsVersion, toolsPath, null, projectCollection, msbuildOverrideTasksPath)
		{
		}

		public Toolset (string toolsVersion, string toolsPath,
				IDictionary<string, string> buildProperties, ProjectCollection projectCollection,
				string msbuildOverrideTasksPath)
			: this (toolsVersion, toolsPath, buildProperties, projectCollection, null, msbuildOverrideTasksPath)
		{
		}

		public
		Toolset (string toolsVersion, string toolsPath, IDictionary<string, string> buildProperties,
			ProjectCollection projectCollection, IDictionary<string, SubToolset> subToolsets,
			string msbuildOverrideTasksPath)
		{
			ToolsVersion = toolsVersion;
			ToolsPath = toolsPath;
			Properties = 
				buildProperties == null ?
				new Dictionary<string, ProjectPropertyInstance> () :
				buildProperties.Select (p => new ProjectPropertyInstance (p.Key, true, p.Value)).ToDictionary (e => e.Name);
			SubToolsets = subToolsets ?? new Dictionary<string, SubToolset> ();
		}

		public string DefaultSubToolsetVersion { get; private set; }
		public IDictionary<string, SubToolset> SubToolsets { get; private set; }

		public IDictionary<string, ProjectPropertyInstance> Properties { get; private set; }

		public string ToolsPath { get; private set; }

		public string ToolsVersion { get; private set; }
	}
}

