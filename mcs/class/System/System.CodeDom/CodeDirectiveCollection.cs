//
// System.CodeDom CodeDirectiveCollection class
//
// Authors:
//	Marek Safar (marek.safar@seznam.cz)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible (true), ClassInterface (ClassInterfaceType.AutoDispatch)]
	public class CodeDirectiveCollection: System.Collections.CollectionBase {

		public CodeDirectiveCollection ()
		{
		}

		public CodeDirectiveCollection (CodeDirective[] value)
		{
			AddRange (value);
		}

		public CodeDirectiveCollection (CodeDirectiveCollection value)
		{
			AddRange (value);
		}


		public CodeDirective this [int index] {
			get { return (CodeDirective) List [index]; }
			set { List [index] = value; }
		}


		public int Add (CodeDirective value)
		{
			return List.Add (value);
		}

		public void AddRange (CodeDirective[] value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			for (int i = 0; i < value.Length; i++) {
				Add (value[i]);
			}
		}
		
		public void AddRange (CodeDirectiveCollection value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			int count = value.Count;
			for (int i = 0; i < count; i++) {
				Add (value[i]);
			}
		}

		public bool Contains (CodeDirective value)
		{
			return List.Contains (value);
		}
		
		public void CopyTo (CodeDirective[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (CodeDirective value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, CodeDirective value)
		{
			List.Insert (index, value);
		}

		public void Remove (CodeDirective value)
		{
			List.Remove (value);
		}
	}
}
