// BuildTaskFactory.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc.
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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Internal;
using System.Collections.Generic;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Internal
{
	class BuildTaskFactory
	{
		public BuildTaskFactory (BuildTaskDatabase builtInDatabase, BuildTaskDatabase perProjectDatabase)
		{
			this.built_in_database = builtInDatabase;
			this.per_project_database = perProjectDatabase;
		}
		
		readonly BuildTaskDatabase built_in_database, per_project_database;
		readonly List<ITaskFactory> task_factories = new List<ITaskFactory> ();
		
		public void ResetCaches ()
		{
			task_factories.Clear ();
		}
		
		public ITask CreateTask (string name, IDictionary<string,string> factoryIdentityParameters, IBuildEngine engine)
		{
			Func<BuildTaskDatabase.TaskDescription,bool> fn = t => t.IsMatch (name);
			var td = per_project_database.Tasks.FirstOrDefault (fn) ?? built_in_database.Tasks.FirstOrDefault (fn);
			if (td == null)
				throw new InvalidOperationException (string.Format ("Task '{0}' could not be found", name));
			if (td.TaskFactoryType != null) {
				var tf = task_factories.FirstOrDefault (f => f.GetType () == td.TaskFactoryType);
				if (tf == null) {
					tf = (ITaskFactory) Activator.CreateInstance (td.TaskFactoryType);
					var tf2 = tf as ITaskFactory2;
					if (tf2 != null)
						tf2.Initialize (name, factoryIdentityParameters, td.TaskFactoryParameters, td.TaskBody, engine);
					else
						tf.Initialize (name, td.TaskFactoryParameters, td.TaskBody, engine);
					task_factories.Add (tf);
				}
				return tf.CreateTask (engine);
			}
			else
				return (ITask) Activator.CreateInstance (td.TaskType);
		}
	}
}

