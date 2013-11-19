//
// BuildTaskFactory.cs
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
using Microsoft.Build.Framework;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using System.IO;

namespace Microsoft.Build.Internal
{
	class BuildTaskDatabase
	{
		const string default_tasks_file = "Microsoft.Common.tasks";
		static readonly Dictionary<string,BuildTaskDatabase> default_factory = new Dictionary<string, BuildTaskDatabase> ();

		public static BuildTaskDatabase GetDefaultTaskDatabase (Toolset toolset)
		{
			BuildTaskDatabase defaults;
			if (!default_factory.TryGetValue (toolset.ToolsVersion, out defaults)) {
				defaults = new BuildTaskDatabase (toolset);
			}
			return defaults;
		}
		
		// for 'default' tasks.
		BuildTaskDatabase (Toolset toolset)
		{
			var root = ProjectRootElement.Create (Path.Combine (toolset.ToolsPath, default_tasks_file));
			LoadUsingTasks (null, root);
		}
		
		public BuildTaskDatabase (ProjectInstance projectInstance, ProjectRootElement projectRootElement)
		{
			LoadUsingTasks (projectInstance, projectRootElement);
		}
		
		internal class TaskDescription
		{
			public TaskAssembly TaskAssembly { get; set; }
			public string Name { get; set; }
			public Type TaskFactoryType { get; set; }
			public Type TaskType { get; set; }
			public IDictionary<string, TaskPropertyInfo> TaskFactoryParameters { get; set; }
			public string TaskBody { get; set; }
		}
		
		internal class TaskAssembly
		{
			public string AssemblyName { get; set; }
			public string AssemblyFile { get; set; }
			public Assembly LoadedAssembly { get; set; }
		}
		
		readonly List<TaskAssembly> assemblies = new List<TaskAssembly> ();
		readonly List<TaskDescription> task_descs = new List<TaskDescription> ();

		public List<TaskDescription> Tasks {
			get { return task_descs; }
		}
		
		void LoadUsingTasks (ProjectInstance projectInstance, ProjectRootElement project)
		{
			Func<string,bool> cond = s => projectInstance != null ? projectInstance.EvaluateCondition (s) : Convert.ToBoolean (s);
			foreach (var ut in project.UsingTasks) {
				var ta = assemblies.FirstOrDefault (a => a.AssemblyFile.Equals (ut.AssemblyFile, StringComparison.OrdinalIgnoreCase) || a.AssemblyName.Equals (ut.AssemblyName, StringComparison.OrdinalIgnoreCase));
				if (ta == null) {
					ta = new TaskAssembly () { AssemblyName = ut.AssemblyName, AssemblyFile = ut.AssemblyFile };
					ta.LoadedAssembly = ta.AssemblyName != null ? Assembly.Load (ta.AssemblyName) : Assembly.LoadFile (ta.AssemblyFile);
					assemblies.Add (ta);
				}
				var pg = ut.ParameterGroup.Count == 0 ? null : ut.ParameterGroup.Parameters.Select (p => new TaskPropertyInfo (p.Name, Type.GetType (p.ParameterType), cond (p.Output), cond (p.Required)))
					.ToDictionary (p => p.Name);
				var task = new TaskDescription () {
					TaskAssembly = ta,
					Name = ut.TaskName,
					TaskFactoryType = ut.TaskFactory != null ? LoadTypeFrom (ta.LoadedAssembly, ut.TaskFactory) : null,
					TaskType = ut.TaskFactory == null ? LoadTypeFrom (ta.LoadedAssembly, ut.TaskName) : null,
					TaskFactoryParameters = pg,
					TaskBody = cond (ut.TaskBody.Condition) ? ut.TaskBody.Evaluate : null,
					};
				task_descs.Add (task);
			}
		}
		
		Type LoadTypeFrom (Assembly a, string possiblyShortTypeName)
		{
			if (possiblyShortTypeName.IndexOf ('.') > 0)
				return a.GetType (possiblyShortTypeName);
			else
				return a.GetTypes ().FirstOrDefault (t => t.Name == possiblyShortTypeName);
		}
	}
}

