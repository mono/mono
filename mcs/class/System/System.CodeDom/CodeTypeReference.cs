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

		// FIXME: probably broken
		[MonoTODO]
		public CodeTypeReference( CodeTypeReference arrayType, int rank )
		{
			this.arrayType = arrayType;
			this.baseType = arrayType.BaseType;
			this.rank = rank;
		}

		// FIXME: probably broken
		[MonoTODO]
		public CodeTypeReference( string baseType, int rank )
		{
			this.baseType = baseType;
			this.rank = rank;
		}
			

		//
		// Properties
		//

		// FIXME: probably broken
		[MonoTODO]
		public CodeTypeReference ArrayElementType
		{
			get {
				return arrayType;
			}
			set {
				arrayType = value;
			}
		}
		
		// FIXME: probably broken
		[MonoTODO]
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
				return baseType;
			}
			set {
				baseType = value;
			}
		}
	}
}
