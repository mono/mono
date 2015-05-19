//
// AssemblyReferenceCollection.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public sealed class AssemblyReferenceCollection : IEnumerable {
	
		List <AssemblyReference> list;

		AssemblyReferenceCollection ()
		{
			list = new List <AssemblyReference> ();
		}
	
		public AssemblyReference Add (AssemblyReference assembly)
		{
			list.Add (assembly);
			return assembly;
		}
		
		public AssemblyReference Add (string path)
		{
			AssemblyReference ar = new AssemblyReference (path);
			list.Add (ar);
			return ar;
		}
		
		public void Clear ()
		{
			list.Clear ();
		}
		
		public AssemblyReference Find (AssemblyIdentity identity)
		{
			throw new NotImplementedException ();
		}
		
		public AssemblyReference Find (string name)
		{
			throw new NotImplementedException ();
		}
		
		public AssemblyReference FindTargetPath (string targetPath)
		{
			throw new NotImplementedException ();
		}
		
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		public void Remove (AssemblyReference assemblyReference)
		{
			list.Remove (assemblyReference);
		}
		
		public int Count {
			get { return list.Count; }
		}
		
		public AssemblyReference this [int index] {
			get { return list [index]; }
		}
	}
}

