//
// System.CodeDom CodeTypeReferenceExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
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

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeReference
		: CodeObject
	{
		private string baseType;
		private CodeTypeReference arrayType;
		private int rank;

		//
		// Constructors
		//
		[MonoTODO ("Missing implementation. Implement array info extraction from the string")]
		public CodeTypeReference( string baseType )
		{
			this.baseType = baseType;
		}
		
		public CodeTypeReference( Type baseType )
		{
			if (baseType.IsArray) {
				this.rank = baseType.GetArrayRank ();
				this.arrayType = new CodeTypeReference (baseType.GetElementType ());
				return;
			}
			this.baseType = baseType.FullName;
		}

		public CodeTypeReference( CodeTypeReference arrayType, int rank )
		{
			this.baseType = null;
			this.rank = rank;
			this.arrayType = arrayType;
		}

		public CodeTypeReference( string baseType, int rank )
			: this (new CodeTypeReference (baseType), rank)
		{
		}
			

		//
		// Properties
		//

		public CodeTypeReference ArrayElementType
		{
			get {
				return arrayType;
			}
			set {
				arrayType = value;
			}
		}
		
		public int ArrayRank {
			get {
				return rank;
			}
			set {
				rank = value;
			}
		}

		public string BaseType {
			get {
				if (baseType == null)
					return String.Empty;

				return baseType;
			}
			set {
				baseType = value;
			}
		}
	}
}
