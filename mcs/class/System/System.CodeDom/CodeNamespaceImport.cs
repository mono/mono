//
// System.CodeDom CodeNamespaceImport Class implementation
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
	public class CodeNamespaceImport
		: CodeObject 
	{
		private CodeLinePragma linePragma;
		private string nameSpace;
		
		//
		// Constructors
		//
		public CodeNamespaceImport ()
		{
		}

		public CodeNamespaceImport (string nameSpace)
		{
			this.nameSpace = nameSpace;
		}

		//
		// Properties
		//
		public CodeLinePragma LinePragma {
			get {
				return linePragma;
			}
			set {
				linePragma = value;
			}
		}

		public string Namespace {
			get {
				return nameSpace;
			}
			set {
				nameSpace = value;
			}
		}
	}
}
