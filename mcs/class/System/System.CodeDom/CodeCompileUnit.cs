//
// System.CodeDom CodeCompileUnit Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2002 Ximian, Inc.
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

#if NET_2_0

		CodeDirectiveCollection startDirectives;
		CodeDirectiveCollection endDirectives;

		public CodeDirectiveCollection StartDirectives {
			get {
				if (startDirectives == null)
					startDirectives = new CodeDirectiveCollection ();
				return startDirectives;
			}
		}

		public CodeDirectiveCollection EndDirectives {
			get {
				if (endDirectives == null)
					endDirectives = new CodeDirectiveCollection ();
				return endDirectives;
			}
		}

#endif

	}
}
