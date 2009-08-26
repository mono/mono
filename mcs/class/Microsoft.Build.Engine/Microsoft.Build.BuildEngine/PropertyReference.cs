//
// PropertyReference.cs
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
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class PropertyReference : IReference {
		
		string	name;
		int	start;
		int	length;
		
		public PropertyReference (string name, int start, int length)
		{
			this.name = name;
			this.start = start;
			this.length = length;
		}
		
		public string ConvertToString (Project project)
		{
			BuildProperty bp = project.EvaluatedProperties [name];
			return bp != null ? bp.ConvertToString (project) : String.Empty;
		}

		public ITaskItem[] ConvertToITaskItemArray (Project project)
		{
			BuildProperty bp = project.EvaluatedProperties [name];
			return bp != null ? bp.ConvertToITaskItemArray (project) : null;
		}
		
		public string Name {
			get { return name; }
		}

		public string GetValue (Project project)
		{
			BuildProperty bp = project.EvaluatedProperties [name];
			return bp == null ? String.Empty : bp.Value;
		}

		public int Start {
			get { return start; }
		}

		public int End {
			get { return start + length - 1; }
		}

		public override string ToString ()
		{
			return String.Format ("$({0})", name);
		}
	}
}

#endif
