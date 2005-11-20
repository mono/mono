//
// System.CodeDom CodeTypeDeclarationCollection Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
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
using System.Collections;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeDeclarationCollection 
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeTypeDeclarationCollection()
		{
		}

		public CodeTypeDeclarationCollection( CodeTypeDeclaration[] value )
		{
			AddRange( value );
		}

		public CodeTypeDeclarationCollection( CodeTypeDeclarationCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeTypeDeclaration this[int index]
		{
			get {
				return (CodeTypeDeclaration)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeTypeDeclaration value)
		{
			return List.Add (value);
		}

		public void AddRange (CodeTypeDeclaration [] value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			for (int i = 0; i < value.Length; i++) {
				Add (value[i]);
			}
		}

		public void AddRange (CodeTypeDeclarationCollection value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			int count = value.Count;
			for (int i = 0; i < count; i++) {
				Add (value[i]);
			}
		}

		public bool Contains( CodeTypeDeclaration value )
		{
			return List.Contains( value );
		}

		public void CopyTo( CodeTypeDeclaration[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeTypeDeclaration value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeTypeDeclaration value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeTypeDeclaration value )
		{
			List.Remove (value);
		}
	}
}
