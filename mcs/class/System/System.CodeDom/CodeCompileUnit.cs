//
// System.CodeDom CodeCompileUnit Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections.Specialized;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeCompileUnit
		: CodeObject
	{
		private CodeAttributeDeclarationCollection assemblyCustomAttributes;
		private CodeNamespaceCollection namespaces;
		private StringCollection referencedAssemblies;

		//
		// Constructors
		//
		public CodeCompileUnit()
		{
		}

		//
		// Properties
		//
		public CodeAttributeDeclarationCollection AssemblyCustomAttributes {
			get {
				if ( assemblyCustomAttributes == null )
					assemblyCustomAttributes = 
						new CodeAttributeDeclarationCollection();
				return assemblyCustomAttributes;
			}
		}

		public CodeNamespaceCollection Namespaces {
			get {
				if ( namespaces == null )
					namespaces = new CodeNamespaceCollection();
				return namespaces;
			}
		}

		public StringCollection ReferencedAssemblies {
			get {
				if ( referencedAssemblies == null )
					referencedAssemblies = new StringCollection();
				return referencedAssemblies;
			}
		}
	}
}
