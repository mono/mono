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
		private CodeTypeReference elementType;
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

		// FIXME: probably broken
		public CodeTypeReference( CodeTypeReference baseType, int rank )
		{
			this.baseType = typeof(System.Array).Name;
			this.rank = rank;
		}

		// FIXME: probably broken
		public CodeTypeReference( string baseType, int rank )
		{
			this.baseType = baseType;
			this.rank = rank;
		}
			

		//
		// Properties
		//
		// FIXME: probably broken
		public CodeTypeReference ArrayElementType
		{
			get {
				return elementType;;
			}
			set {
				elementType = value;
			}
		}
		
		// FIXME: probably broken
		public int ArrayRank {
			get {
				return rank;
			}
			set {
				rank = value;
			}
		}
	}
}
