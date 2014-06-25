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
using System.Xml;

namespace Microsoft.Build.Internal
{
	class BuildTaskDatabase
	{
		const string default_tasks_file = "Microsoft.Common.tasks";
		static readonly Dictionary<string,BuildTaskDatabase> default_factory = new Dictionary<string, BuildTaskDatabase> ();

		public static BuildTaskDatabase GetDefaultTaskDatabase (Toolset toolset)
		{
			if (toolset == null)
				throw new ArgumentNullException ("toolset");
			BuildTaskDatabase defaults;
			if (!default_factory.TryGetValue (toolset.ToolsVersion, out defaults)) {
				defaults = new BuildTaskDatabase (toolset);
			}
			return defaults;
		}
		
		// for 'default' tasks.
		BuildTaskDatabase (Toolset toolset)
		{
			ProjectRootElement root;
			using (var xml = XmlReader.Create (Path.Combine (toolset.ToolsPath, default_tasks_file)))
				root = ProjectRootElement.Create (xml);
			LoadUsingTasks (null, root.UsingTasks);
		}
		
		public BuildTaskDatabase (IBuildEngine engine, ProjectInstance projectInstance)
		{
			this.engine = engine;
			LoadUsingTasks (projectInstance, projectInstance.UsingTasks);
		}
		
		internal class TaskDescription
		{
			public TaskAssembly TaskAssembly { get; set; }
			public string Name { get; set; }
			public Type TaskFactoryType { get; set; }
			public Type TaskType { get; set; }
			public IDictionary<string, TaskPropertyInfo> TaskFactoryParameters { get; set; }
			public string TaskBody { get; set; }
			
			public bool IsMatch (string name)
			{
				int ridx = Name.LastIndexOf ('.');
				int tidx = name.IndexOf ('.');
				return string.Equals (Name, name, StringComparison.OrdinalIgnoreCase) ||
					tidx < 0 && ridx > 0 && string.Equals (Name.Substring (ridx + 1), name, StringComparison.OrdinalIgnoreCase);
			}
		}
		
		internal class TaskAssembly
		{
			public string AssemblyName { get; set; }
			public string AssemblyFile { get; set; }
			public Assembly LoadedAssembly { get; set; }
		}

		readonly IBuildEngine engine;
		readonly List<TaskAssembly> assemblies = new List<TaskAssembly> ();
		readonly List<TaskDescription> task_descs = new List<TaskDescription> ();

		public List<TaskDescription> Tasks {
			get { return task_descs; }
		}

		// FIXME: my guess is the tasks does not have to be loaded entirely but only requested tasks must be loaded at invocation time.
		void LoadUsingTasks (ProjectInstance projectInstance, IEnumerable<ProjectUsingTaskElement> usingTasks)
		{
			Func<string,bool> cond = s => projectInstance != null ? projectInstance.EvaluateCondition (s) : Convert.ToBoolean (s);
			Func<string,string> expand = s => projectInstance != null ? projectInstance.ExpandString (s) : s;
			foreach (var ut in usingTasks) {
				var aName = expand (ut.AssemblyName);
				var aFile = expand (ut.AssemblyFile);
				if (string.IsNullOrEmpty (aName) && string.IsNullOrEmpty (aFile)) {
					var errorNoAssembly = string.Format ("Task '{0}' does not specify either of AssemblyName or AssemblyFile.", ut.TaskName);
					engine.LogWarningEvent (new BuildWarningEventArgs (null, null, projectInstance.FullPath, ut.Location.Line, ut.Location.Column, 0, 0, errorNoAssembly, null, null));
					continue;
				}
				var ta = assemblies.FirstOrDefault (a => a.AssemblyFile.Equals (aFile, StringComparison.OrdinalIgnoreCase) || a.AssemblyName.Equals (aName, StringComparison.OrdinalIgnoreCase));
				if (ta == null) {
					var path = Path.GetDirectoryName (string.IsNullOrEmpty (ut.Location.File) ? projectInstance.FullPath : ut.Location.File);
					ta = new TaskAssembly () { AssemblyName = aName, AssemblyFile = aFile };
					try {
						ta.LoadedAssembly = !string.IsNullOrEmpty (ta.AssemblyName) ? Assembly.Load (ta.AssemblyName) : Assembly.LoadFile (Path.Combine (path, ta.AssemblyFile));
					} catch {
						var errorNotLoaded = string.Format ("For task '{0}' Specified assembly '{1}' was not found", ut.TaskName, string.IsNullOrEmpty (ta.AssemblyName) ? Path.Combine (path, ta.AssemblyFile) : ta.AssemblyName);
						engine.LogWarningEvent (new BuildWarningEventArgs (null, null, projectInstance.FullPath, ut.Location.Line, ut.Location.Column, 0, 0, errorNotLoaded, null, null));
						continue;
					}
					assemblies.Add (ta);
				}
				var pg = ut.ParameterGroup == null ? null : ut.ParameterGroup.Parameters.Select (p => new TaskPropertyInfo (p.Name, Type.GetType (p.ParameterType), cond (p.Output), cond (p.Required)))
					.ToDictionary (p => p.Name);
				

				Type type = null;
				string error = null;
				TaskDescription task = new TaskDescription () {
					TaskAssembly = ta,
					Name = ut.TaskName,
					TaskFactoryParameters = pg,
					TaskBody = ut.TaskBody != null && cond (ut.TaskBody.Condition) ? ut.TaskBody.TaskBody : null,
					};
				if (string.IsNullOrEmpty (ut.TaskFactory)) {
					type = LoadTypeFrom (ta.LoadedAssembly, ut.TaskName, ut.TaskName);
					if (type == null)
						error = string.Format ("For task '{0}' Specified type '{1}' was not found in assembly '{2}'", ut.TaskName, ut.TaskName, ta.LoadedAssembly.FullName);
					else
						task.TaskType = type;
				} else {
					type = LoadTypeFrom (ta.LoadedAssembly, ut.TaskName, ut.TaskFactory);
					if (type == null)
						error = string.Format ("For task '{0}' Specified factory type '{1}' was not found in assembly '{2}'", ut.TaskName, ut.TaskFactory, ta.LoadedAssembly.FullName);
					else
						task.TaskFactoryType = type;
				}
				if (error != null)
					engine.LogWarningEvent (new BuildWarningEventArgs (null, null, projectInstance.FullPath, ut.Location.Line, ut.Location.Column, 0, 0, error, null, null));
				else
					task_descs.Add (task);
			}
		}
		
		Type LoadTypeFrom (Assembly a, string taskName, string possiblyShortTypeName)
		{
			Type type = a.GetType (possiblyShortTypeName, false, true);
			if (possiblyShortTypeName.IndexOf ('.') < 0)
				type = a.GetTypes ().FirstOrDefault (t => t.Name == possiblyShortTypeName);
			return type;
		}
	}
}

