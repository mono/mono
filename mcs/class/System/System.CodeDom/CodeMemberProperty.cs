//
// System.CodeDom CodeMemberProperty Class implementation
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
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
