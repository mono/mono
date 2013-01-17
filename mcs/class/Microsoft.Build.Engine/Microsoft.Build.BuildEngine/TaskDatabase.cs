//
// TaskDatabase.cs: Provides information about tasks.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class TaskDatabase {
		
		Dictionary<string, UsingTaskInfo> usingTasksByFullName;
		// full name -> AssemblyLoadInfo
		Dictionary <string, AssemblyLoadInfo>	assemblyInformation;
		// full name -> Type
		Dictionary <string, Type>		typesByFullName;
		// short name -> Type
		Dictionary <string, Type>		typesByShortName;
	
		public TaskDatabase ()
		{
			assemblyInformation = new Dictionary <string, AssemblyLoadInfo> ();
			typesByFullName = new Dictionary <string, Type> (StringComparer.OrdinalIgnoreCase);
			typesByShortName = new Dictionary <string, Type> (StringComparer.OrdinalIgnoreCase);
			usingTasksByFullName = new Dictionary <string, UsingTaskInfo> (StringComparer.OrdinalIgnoreCase);
		}
		
		public void RegisterTask (string classname, AssemblyLoadInfo assemblyLoadInfo)
		{
			assemblyInformation.Add (classname, assemblyLoadInfo);
			Assembly assembly;

			if (assemblyLoadInfo.InfoType == LoadInfoType.AssemblyFilename)
				assembly = Assembly.LoadFrom (assemblyLoadInfo.Filename);
			else if (assemblyLoadInfo.InfoType == LoadInfoType.AssemblyName)
				assembly = Assembly.Load (assemblyLoadInfo.AssemblyName);
			else
				assembly = Assembly.Load (assemblyLoadInfo.AssemblyNameString);
			
			Type type = assembly.GetType (classname);
			if (type == null) {
				// search for matching class in case namespace was not used
				foreach (Type exportedType in assembly.GetExportedTypes()) {
					if (exportedType.Name == classname) {
						type = exportedType;
						break;
					}
				}
			}
			typesByFullName.Add (classname, type);
			typesByShortName [GetShortName (classname)] = type;
		}

		public void RegisterUsingTask (UsingTask ut)
		{
			usingTasksByFullName [ut.TaskName] = new UsingTaskInfo (GetShortName (ut.TaskName), ut);
		}
		
		public Type GetTypeFromClassName (string classname)
		{
			Type ret = GetTypeFromClassNameInternal (classname);
			if (ret == null) {
				// Task not already loaded,
				// Check list of pending UsingTasks
				bool is_shortname = classname.IndexOf ('.') < 0;
				UsingTaskInfo info = new UsingTaskInfo (String.Empty, null);
				if (is_shortname) {
					// Linear search UsingTaskInfo objects for short name match
					foreach (UsingTaskInfo ut_info in usingTasksByFullName.Values) {
						if (String.Compare (ut_info.ShortName, classname, true) == 0) {
							info = ut_info;
							break;
						}
					}

					if (info.Task == null)
						ThrowTaskNotRegistered (classname);
				} else {
					// Look for full name match
					if (!usingTasksByFullName.TryGetValue (classname, out info))
						ThrowTaskNotRegistered (classname);
				}

				usingTasksByFullName.Remove (info.Task.TaskName);
				info.Task.Load (this);
				ret = GetTypeFromClassNameInternal (classname);
			}

			if (ret == null)
				ThrowTaskNotRegistered (classname);
			return ret;
		}

		Type GetTypeFromClassNameInternal (string classname)
		{
			if (!typesByFullName.ContainsKey (classname)) {
				if (!typesByShortName.ContainsKey (classname))
					return null;
				else
					return typesByShortName [classname];
			} else
				return typesByFullName [classname];
		}
		
		public void CopyTasks (TaskDatabase taskDatabase)
		{
			foreach (KeyValuePair <string, AssemblyLoadInfo> kvp in taskDatabase.assemblyInformation)
				assemblyInformation.Add (kvp.Key, kvp.Value);
			foreach (KeyValuePair <string, Type> kvp in taskDatabase.typesByFullName)
				typesByFullName.Add (kvp.Key, kvp.Value);
			foreach (KeyValuePair <string, Type> kvp in taskDatabase.typesByShortName)
				typesByShortName.Add (kvp.Key, kvp.Value);
			foreach (KeyValuePair <string, UsingTaskInfo> kvp in taskDatabase.usingTasksByFullName)
				usingTasksByFullName.Add (kvp.Key, kvp.Value);
		}
		
		private string GetShortName (string fullname)
		{
			string[] parts = fullname.Split ('.');
			return parts [parts.Length - 1];
		}

		void ThrowTaskNotRegistered (string classname)
		{
			throw new Exception (String.Format ("Not registered task {0}.", classname));
		}
	}

	struct UsingTaskInfo {
		public string ShortName;
		public UsingTask Task;

		public UsingTaskInfo (string shortname, UsingTask task)
		{
			ShortName = shortname;
			Task = task;
		}
	}
}
