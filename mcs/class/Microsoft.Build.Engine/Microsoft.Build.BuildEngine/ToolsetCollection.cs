//
// ToolsetCollection.cs
//
// Author:
//	Ankit Jain (jankit@novell.com)
//
// Copyright 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;

namespace Microsoft.Build.BuildEngine
{

	public class ToolsetCollection : ICollection<Toolset>, IEnumerable<Toolset>, IEnumerable
	{
		List<Toolset> toolsets;
		
		internal ToolsetCollection ()
		{
			toolsets = new List<Toolset> ();
		}
		
		public int Count
		{
			get { return toolsets.Count; }
		}
		
		public bool IsReadOnly { get { return false; } }
			
		public Toolset this [string toolsVersion]
		{
			get { return toolsets.Find (item => item.ToolsVersion == toolsVersion); }
		}
		
		public void Add (Toolset item)
		{
			toolsets.Add (item);
		}
		
		public void Clear ()
		{
			toolsets.Clear ();
		}
		
		public bool Contains (string toolsVersion)
		{
			return toolsets.Exists (item => item.ToolsVersion == toolsVersion);
		}
		
		public bool Contains (Toolset item)
		{
			return toolsets.Contains (item);
		}

		public void CopyTo (Toolset[] array, int arrayIndex)
		{
			toolsets.CopyTo (array, arrayIndex);
		}
		
		public IEnumerator<Toolset> GetEnumerator ()
		{
			return toolsets.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return toolsets.GetEnumerator ();
		}
		
		public bool Remove (Toolset item)
		{
			return toolsets.Remove (item);
		}
	}
}
