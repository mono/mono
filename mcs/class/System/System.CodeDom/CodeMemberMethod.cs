//
// System.CodeDom CodeMemberMethod Class implementation
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
	public class CodeMemberMethod
		: CodeTypeMember 
	{
		private CodeTypeReferenceCollection implementationTypes;
		private CodeParameterDeclarationExpressionCollection parameters;
		private CodeTypeReference privateImplementationType;
		private CodeTypeReference returnType;
		private CodeStatementCollection statements;
		private CodeAttributeDeclarationCollection returnTypeCustomAttributes;

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
					PopulateImplementationTypes( this, EventArgs.Empty );
				}
				return implementationTypes;
			}
		}

		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				if ( parameters == null ) {
					parameters = new CodeParameterDeclarationExpressionCollection();
					PopulateParameters( this, EventArgs.Empty );
				}
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

		public CodeTypeReference ReturnType {
			get {
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
					PopulateStatements( this, EventArgs.Empty );
				}
				return statements;
			}
		}

		public CodeAttributeDeclarationCollection ReturnTypeCustomAttributes {
			get {
				if ( returnTypeCustomAttributes == null )
					returnTypeCustomAttributes = new CodeAttributeDeclarationCollection();
				
				return returnTypeCustomAttributes;
			}
		}

		//
		// Events
		//
		public event EventHandler PopulateImplementationTypes;

		public event EventHandler PopulateParameters;

		public event EventHandler PopulateStatements;
	}
}
