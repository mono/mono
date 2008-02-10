//
// System.CodeDom CodeMemberProperty Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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
	public class CodeMemberProperty
		: CodeTypeMember
	{
		private CodeStatementCollection getStatements;
		private bool hasGet;
		private bool hasSet;
		private CodeTypeReferenceCollection implementationTypes;
		private CodeParameterDeclarationExpressionCollection parameters;
		private CodeTypeReference privateImplementationType;
		private CodeStatementCollection setStatements;
		private CodeTypeReference type;
		
		//
		// Constructors
		//
		public CodeMemberProperty ()
		{
		}

		//
		// Properties
		//
		public CodeStatementCollection GetStatements {
			get {
				if ( getStatements == null )
					getStatements = new CodeStatementCollection();
				return getStatements;
			}
		}

		public bool HasGet {
			get {
				return (hasGet || (getStatements != null && getStatements.Count > 0));
			}
			set {
				hasGet = value;
				if (!hasGet && getStatements != null)
					getStatements.Clear ();
					
			}
		}
		
		public bool HasSet {
			get {
				return (hasSet || (setStatements != null && setStatements.Count > 0));
			}
			set {
				hasSet = value;
				if (!hasSet && setStatements != null)
					setStatements.Clear ();
			}
		}

		public CodeTypeReferenceCollection ImplementationTypes {
			get {
				if ( implementationTypes == null )
					implementationTypes = new CodeTypeReferenceCollection();
				return implementationTypes;
			}
		}

		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				if ( parameters == null )
					parameters = new CodeParameterDeclarationExpressionCollection();
				return parameters;
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

		public CodeStatementCollection SetStatements {
			get {
				if ( setStatements == null )
					setStatements = new CodeStatementCollection();
				return setStatements;
			}
		}

		public CodeTypeReference Type {
			get {
				if (type == null) {
					type = new CodeTypeReference(string.Empty);
				}
				return type;
			}
			set {
				type = value;
			}
		}

		//
		// ICodeDomVisitor method
		//
		internal override void Accept (ICodeDomVisitor visitor)
		{
			visitor.Visit (this);
		}
	}
}
