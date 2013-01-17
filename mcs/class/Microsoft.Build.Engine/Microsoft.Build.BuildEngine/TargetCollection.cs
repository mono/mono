//
// TargetCollection.cs: Collection of targets.
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
using System.Reflection;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class TargetCollection : ICollection, IEnumerable {
		
		Dictionary <string, Target>	targetsByName;
		Project				parentProject;
	
		internal TargetCollection (Project project)
		{
			this.targetsByName = new Dictionary <string, Target> (StringComparer.OrdinalIgnoreCase);
			this.parentProject = project;
		}

		[MonoTODO]
		public Target AddNewTarget (string targetName)
		{
			if (targetName == null)
				throw new InvalidProjectFileException (
					"The required attribute \"Name\" is missing from element <Target>.");
		
			XmlElement targetElement = parentProject.XmlDocument.CreateElement ("Target", Project.XmlNamespace);
			parentProject.XmlDocument.DocumentElement.AppendChild (targetElement);
			targetElement.SetAttribute ("Name", targetName);
			
			Target t = new Target (targetElement, parentProject, null);
			
			AddTarget (t);
			
			return t;
		}

		internal void AddTarget (Target target)
		{
			if (targetsByName.ContainsKey (target.Name))
				targetsByName.Remove (target.Name);
			targetsByName.Add (target.Name, target);
		}

		public void CopyTo (Array array, int index)
		{
			targetsByName.Values.CopyTo ((Target[]) array, index);
		}

		public bool Exists (string targetName)
		{
			return targetsByName.ContainsKey (targetName);
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (KeyValuePair <string, Target> kvp in targetsByName)
				yield return kvp.Value;
		}

		internal IEnumerable<Target> AsIEnumerable ()
		{
			foreach (KeyValuePair <string, Target> kvp in targetsByName)
				yield return kvp.Value;
		}

		public void RemoveTarget (Target targetToRemove)
		{
			if (targetToRemove == null)
				throw new ArgumentNullException ();
			
			targetsByName.Remove (targetToRemove.Name);
		}

		public int Count {
			get {
				return targetsByName.Count;
			}
		}

		public bool IsSynchronized {
			get {
				return false;
			}
		}

		public object SyncRoot {
			get {
				return this;
			}
		}

		public Target this [string index] {
			get {
				if (targetsByName.ContainsKey (index))
					return targetsByName [index];
				else
					return null;
			}
		}
	}
}
