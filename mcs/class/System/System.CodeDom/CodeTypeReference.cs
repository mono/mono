//
// System.CodeDom CodeTypeReferenceExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
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

#if NET_2_0
		CodeTypeReferenceCollection typeArguments;
		CodeTypeReferenceOptions codeTypeReferenceOption;
#endif

		//
		// Constructors
		//
		public CodeTypeReference( string baseType )
		{
			if (baseType.Length == 0) {
				this.baseType = typeof (void).FullName;
				return;
			}

			int array_start = baseType.LastIndexOf ('[');
			if (array_start == -1) {
				this.baseType = baseType;
				return;
			}
			string[] args = baseType.Split (',');

#if NET_2_0
			int array_end = baseType.LastIndexOf (']');

			if ((array_end - array_start) != args.Length) {
				arrayType = new CodeTypeReference (baseType.Substring (0, array_start));
				array_start++;
				TypeArguments.Add (new CodeTypeReference (baseType.Substring (array_start, array_end - array_start)));
			} else
#endif
				arrayType = new CodeTypeReference (baseType.Substring (0, array_start), args.Length);
		}
		
		public CodeTypeReference( Type baseType )
		{
			if (baseType.IsArray) {
				this.rank = baseType.GetArrayRank ();
				this.arrayType = new CodeTypeReference (baseType.GetElementType ());
				this.baseType = arrayType.BaseType;
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
			
#if NET_2_0
		public CodeTypeReference( CodeTypeParameter typeParameter ) :
			this (typeParameter.Name)
		{
		}

		public CodeTypeReference( string typeName, CodeTypeReferenceOptions codeTypeReferenceOption ) :
			this (typeName)
		{
			this.codeTypeReferenceOption = codeTypeReferenceOption;
		}

		public CodeTypeReference( Type type, CodeTypeReferenceOptions codeTypeReferenceOption ) :
			this (type)
		{
			this.codeTypeReferenceOption = codeTypeReferenceOption;
		}

		public CodeTypeReference( string typeName, params CodeTypeReference[] typeArguments ) :
			this (typeName)
		{
			TypeArguments.AddRange (typeArguments);
		}
#endif

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

#if NET_2_0
		[ComVisible (false)]
		public CodeTypeReferenceOptions Options {
			get {
				return codeTypeReferenceOption;
			}
			set {
				codeTypeReferenceOption = value;
			}
		}

		[ComVisible (false)]
		public CodeTypeReferenceCollection TypeArguments {
			get {
				if (typeArguments == null)
					typeArguments = new CodeTypeReferenceCollection ();
				return typeArguments;
			}
		}
#endif
	}
}
