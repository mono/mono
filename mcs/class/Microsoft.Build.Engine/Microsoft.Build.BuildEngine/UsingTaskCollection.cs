//
// UsingTaskCollection.cs: Represents a collection of all UsingTask elements in
// a project.
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
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Build.BuildEngine {
	public class UsingTaskCollection : ICollection, IEnumerable {
	
		//Project		parentProject;
		object		syncRoot;
		List <UsingTask>	usingTasks;
		
		internal UsingTaskCollection (Project parentProject)
		{
			//this.parentProject = parentProject;
			this.syncRoot = new Object ();
			this.usingTasks = new List <UsingTask> ();
		}
		
		internal void Add (UsingTask usingTask)
		{
			if (usingTask == null)
				throw new ArgumentNullException ("usingTask");
			
			if (usingTasks.Contains (usingTask))
				throw new InvalidOperationException ("Task already registered.");
			
			usingTasks.Add (usingTask);
		}
		
		public void CopyTo (Array array, int index)
		{
			usingTasks.CopyTo ((UsingTask[]) array, index);
		}
		
		public void CopyTo (UsingTask[] array, int index)
		{
			usingTasks.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			foreach (UsingTask ut in usingTasks)
				yield return ut;
		}
		
		public int Count {
			get { return usingTasks.Count; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return syncRoot; }
		}
	}
}
