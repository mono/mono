//
// RemoveDuplicates.cs
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

#if NET_2_0

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {
	public class RemoveDuplicates : TaskExtension {

		ITaskItem [] filtered;
		ITaskItem [] inputs;
	
		public RemoveDuplicates ()
		{
		}

		[MonoTODO]
		public override bool Execute ()
		{
			if (inputs == null || inputs.Length == 0)
				return true;

			Dictionary <string, ITaskItem> items = new Dictionary <string, ITaskItem> ();
			List <ITaskItem> list = new List <ITaskItem> ();

			foreach (ITaskItem item in inputs) {
				if (!items.ContainsKey (item.ItemSpec)) {
					items.Add (item.ItemSpec, item);
					list.Add (item);
				}
			}

			filtered = list.ToArray ();

			return true;
		}
		
		[Output]
		public ITaskItem [] Filtered {
			get { return filtered; }
			set { filtered = value; }
		}

		public ITaskItem [] Inputs {
			get { return inputs; }
			set { inputs = value; }
		}
	}
}

#endif
