//
// System.CodeDom CodeMemberMethod Class implementation
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
	public class CodeMemberMethod
		: CodeTypeMember 
	{
		private CodeTypeReferenceCollection implementationTypes;
		private CodeParameterDeclarationExpressionCollection parameters;
		private CodeTypeReference privateImplements;
		private CodeTypeReference returnType;
		private CodeStatementCollection statements;
		private CodeAttributeDeclarationCollection returnAttributes;
		//int populated;

		CodeTypeParameterCollection typeParameters;
		//
		// Constructors
		//
		public CodeMemberMethod()
		{
		}

		//
		// Properties
		// 
		public CodeTypeReferenceCollection ImplementationTypes {
			get {
				if ( implementationTypes == null ) {
					implementationTypes = new CodeTypeReferenceCollection();
					if ( PopulateImplementationTypes != null )
						PopulateImplementationTypes( this, EventArgs.Empty );
				}
				return implementationTypes;
			}
		}

		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				if ( parameters == null ) {
					parameters = new CodeParameterDeclarationExpressionCollection();
					if ( PopulateParameters != null )
						PopulateParameters( this, EventArgs.Empty );
				}
				return parameters;
			}
		}

		public CodeTypeReference PrivateImplementationType {
			get {
				return privateImplements;
			}
			set {
				privateImplements = value;
			}
		}

		public CodeTypeReference ReturnType {
			get {
				if ( returnType == null )
					return new CodeTypeReference (typeof (void));
				return returnType;
			}
			set {
				returnType = value;
			}
		}

		public CodeStatementCollection Statements {
			get {
				if ( statements == null ) {
					statements = new CodeStatementCollection();
					if ( PopulateStatements != null )
						PopulateStatements( this, EventArgs.Empty );
				}
				return statements;
			}
		}

		public CodeAttributeDeclarationCollection ReturnTypeCustomAttributes {
			get {
				if ( returnAttributes == null )
					returnAttributes = new CodeAttributeDeclarationCollection();
				
				return returnAttributes;
			}
		}

		[ComVisible (false)]
		public CodeTypeParameterCollection TypeParameters {
			get {
				if (typeParameters == null)
					typeParameters = new CodeTypeParameterCollection ();
				return typeParameters;
			}
		}

		//
		// Events
		//
		public event EventHandler PopulateImplementationTypes;

		public event EventHandler PopulateParameters;

		public event EventHandler PopulateStatements;

		//
		// ICodeDomVisitor method
		//
		internal override void Accept (ICodeDomVisitor visitor)
		{
			visitor.Visit (this);
		}
	}
}
