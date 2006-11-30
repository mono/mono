// 
// System.Web.Services.Description.BasicProfileViolationCollection.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
//

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

#if NET_2_0

using System.Collections;
using System.Collections.Generic;

namespace System.Web.Services.Description
{
	public class BasicProfileViolationCollection
		: CollectionBase, IEnumerable<BasicProfileViolation>
	{
		int generation;

		public BasicProfileViolationCollection ()
		{
		}

		IEnumerator<BasicProfileViolation> IEnumerable<BasicProfileViolation>.GetEnumerator ()
		{
			return new BasicProfileViolationEnumerator (this);
		}

		internal int Generation {
			get { return generation; }
		}

		public BasicProfileViolation this [int index] {
			get { return (BasicProfileViolation) List [index]; }
			set { List [index] = value; }
		}

		internal int Add (BasicProfileViolation violation)
		{
			return List.Add (violation);
		}

		public bool Contains (BasicProfileViolation violation)
		{
			return List.Contains (violation);
		}
		
		public void CopyTo (BasicProfileViolation[] array, int index)
		{
			List.CopyTo (array, index);
		}
		
		public int IndexOf (BasicProfileViolation violation)
		{
			return List.IndexOf (violation);
		}

		public void Insert (int index, BasicProfileViolation violation)
		{
			generation++;
			List.Insert (index, violation);
		}
		
		public void Remove (BasicProfileViolation violation)
		{
			List.Remove (violation);
		}
		
		public override string ToString()
		{
			string s = "";
			foreach (BasicProfileViolation violation in List)
				s += violation.ToString () + "\n";
			return s;
		}
	}
}

#endif
