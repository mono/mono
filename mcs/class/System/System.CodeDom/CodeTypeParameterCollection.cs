//
// System.CodeDom CodeTypeParameterCollection class
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
	public class CodeTypeParameterCollection: System.Collections.CollectionBase
	{
		public CodeTypeParameterCollection ()
		{
		}

		public CodeTypeParameterCollection (CodeTypeParameter[] value)
		{
			AddRange (value);
		}

		public CodeTypeParameterCollection (CodeTypeParameterCollection value)
		{
			AddRange (value);
		}

		public int Add (CodeTypeParameter value)
		{
			return List.Add (value);
		}

		public void Add (string value)
		{
			List.Add (new CodeTypeParameter (value));
		}

		public void AddRange (CodeTypeParameter[] value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			for (int i = 0; i < value.Length; i++) {
				Add (value[i]);
			}
		}

		public void AddRange (CodeTypeParameterCollection value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			int count = value.Count;
			for (int i = 0; i < count; i++) {
				Add (value[i]);
			}
		}

		public bool Contains (CodeTypeParameter value)
		{
			return List.Contains (value);
		}
		
		public void CopyTo (CodeTypeParameter[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (CodeTypeParameter value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, CodeTypeParameter value)
		{
			List.Insert (index, value);
		}

		public void Remove (CodeTypeParameter value)
		{
			List.Remove (value);
		}

		public CodeTypeParameter this [int index]
		{
			get {
				return ((CodeTypeParameter) List [index]);
			}
			set {
				List[index] = value;
			}
		}
 	}
}

