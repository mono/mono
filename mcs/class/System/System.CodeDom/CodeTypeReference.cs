//
// System.CodeDom CodeTypeReferenceExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
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
		public CodeTypeReference( string baseType )
		{
			this.baseType = baseType;
		}
		
		public CodeTypeReference( Type baseType )
		{
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
