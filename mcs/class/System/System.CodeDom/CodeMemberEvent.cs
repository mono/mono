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
		private CodeTypeReferenceCollection implementationTypes;
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
		public CodeTypeReferenceCollection ImplementationTypes
		{
			get {
				if (implementationTypes == null)
					implementationTypes = new CodeTypeReferenceCollection ();

				return implementationTypes;
			}
		}

		public CodeTypeReference PrivateImplementationType
		{
			get {
				return privateImplementationType;
			}
			set {
				privateImplementationType = value;
			}
		}

		public CodeTypeReference Type
		{
			get {
				if (type == null)
					type = new CodeTypeReference (String.Empty);

				return type;
			}
			set {
				type = value;
			}
		}
	}
}
