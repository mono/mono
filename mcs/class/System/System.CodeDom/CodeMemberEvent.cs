//
// System.CodeDom CodeMemberEvent Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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
	public class CodeMemberEvent
		: CodeTypeMember
	{
		private CodeTypeReference implementationType;
		private CodeTypeReference privateImplementationType;
		private CodeTypeReference type;
		
		//
		// Constructors
		//
		public CodeMemberEvent ()
		{
		}

		//
		// Properties
		//
		public CodeTypeReference ImplementationTypes {
			get {
				return implementationType;
			}
			set {
				implementationType = value;
			}
		}

		public CodeTypeReference PrivateImplementationType {
			get {
				return privateImplementationType;
			}
			set {
				privateImplementationType = value;
			}
		}

		public CodeTypeReference Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
