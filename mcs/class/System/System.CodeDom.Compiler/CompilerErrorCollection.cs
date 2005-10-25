//
// System.CodeDom.Compiler.CompilerErrorCollection.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
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

using System.Collections;
using System.Security.Permissions;

namespace System.CodeDom.Compiler {

	[Serializable]
#if ONLY_1_1
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
#endif
	public class CompilerErrorCollection : CollectionBase
	{
		public CompilerErrorCollection ()
		{
		}

		public CompilerErrorCollection (CompilerErrorCollection value)
		{
			InnerList.AddRange(value.InnerList);
		}

		public CompilerErrorCollection (CompilerError[] value)
		{
			InnerList.AddRange(value);
		}

		public int Add (CompilerError value)
		{
			return InnerList.Add(value);
		}

		public void AddRange (CompilerError[] value)
		{
			InnerList.AddRange(value);
		}

		public void AddRange (CompilerErrorCollection value)
		{
			InnerList.AddRange(value.InnerList);
		}

		public bool Contains (CompilerError value)
		{
			return InnerList.Contains(value);
		}

		public void CopyTo (CompilerError[] array, int index)
		{
			InnerList.CopyTo(array,index);
		}

		public int IndexOf (CompilerError value)
		{
			return InnerList.IndexOf(value);
		}

		public void Insert (int index, CompilerError value)
		{
			InnerList.Insert(index,value);
		}

		public void Remove (CompilerError value)
		{
			InnerList.Remove(value);
		}

		public CompilerError this [int index] {
			get { return (CompilerError) InnerList[index]; }
			set { InnerList[index]=value; }
		}

		public bool HasErrors {
			get {
				foreach (CompilerError error in InnerList)
					if (!error.IsWarning) return true;
				return false;
			}
		}

		public bool HasWarnings {
			get {
				foreach (CompilerError error in InnerList)
					if (error.IsWarning) return true;
				return false;
			}
		}
	}
}

