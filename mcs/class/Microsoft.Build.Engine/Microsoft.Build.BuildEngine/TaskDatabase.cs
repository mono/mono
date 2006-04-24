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

#if NET_2_0

using System;
using System.Collections;
using System.Reflection;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class TaskDatabase {
		
		// full name -> AssemblyLoadInfo
		IDictionary	assemblyInformation;
		// full name -> Type
		IDictionary	typesByFullName;
		// short name -> Type
		IDictionary	typesByShortName;
	
		public TaskDatabase ()
		{
			assemblyInformation = new Hashtable ();
			typesByFullName = new Hashtable ();
			typesByShortName = new Hashtable ();
		}
		
		public void RegisterTask (string classname, AssemblyLoadInfo assemblyLoadInfo)
		{
			assemblyInformation.Add (classname, assemblyLoadInfo);
			Assembly assembly;
			if (assemblyLoadInfo.InfoType == LoadInfoType.AssemblyFilename) {
				assembly = Assembly.LoadFrom (assemblyLoadInfo.Filename);
			} else if (assemblyLoadInfo.InfoType == LoadInfoType.AssemblyName) {
				assembly = Assembly.Load (assemblyLoadInfo.AssemblyName);
			} else {
				assembly = Assembly.Load (assemblyLoadInfo.AssemblyNameString);
			}
			Type type = assembly.GetType (classname);
			typesByFullName.Add (classname, type);
			typesByShortName.Add (GetShortName (classname), type);
		}
		
		public Type GetTypeFromClassName (string classname)
		{
			if (typesByFullName.Contains (classname) == false) {
				if (typesByShortName.Contains (classname) == false)
					throw new Exception ("Not registered task.");
				else {
					return (Type) typesByShortName [classname];
				}
			} else
				return (Type) typesByFullName [classname];
		}
		
		public void CopyTasks (TaskDatabase taskDatabase)
		{
			foreach (DictionaryEntry de in taskDatabase.assemblyInformation)
				assemblyInformation.Add (de.Key, de.Value);
			foreach (DictionaryEntry de in taskDatabase.typesByFullName)
				typesByFullName.Add (de.Key, de.Value);
			foreach (DictionaryEntry de in taskDatabase.typesByShortName)
				typesByShortName.Add (de.Key, de.Value);
		}
		
		private string GetShortName (string fullname)
		{
			string[] parts = fullname.Split ('.');
			return parts [parts.Length - 1];
		}
	}
}

#endif