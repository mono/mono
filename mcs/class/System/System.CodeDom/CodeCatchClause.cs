//
// System.CodeDom CodeCatchClaus Class implementation
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
	public class CodeCatchClause
	{
		private CodeTypeReference catchExceptionType;
		private string localName;
		private CodeStatementCollection statements;

		//
		// Constructors
		//
		public CodeCatchClause ()
		{
		}

		public CodeCatchClause ( string localName )
		{
			this.localName = localName;
		}

		public CodeCatchClause ( string localName,
					 CodeTypeReference catchExceptionType )
		{
			this.localName = localName;
			this.catchExceptionType = catchExceptionType;
		}

		public CodeCatchClause ( string localName,
					 CodeTypeReference catchExceptionType,
					 CodeStatement[] statements )
		{
			this.localName = localName;
			this.catchExceptionType = catchExceptionType;
			this.Statements.AddRange( statements );
		}

		//
		// Properties
		//
		public CodeTypeReference CatchExceptionType {
			get {
				return catchExceptionType;
			}
			set {
				catchExceptionType = value;
			}
		}

		public string LocalName {
			get {
				return localName;
			}
			set {
				localName = value;
			}
		}

		public CodeStatementCollection Statements {
			get {
				if ( statements == null )
					statements = new CodeStatementCollection();
				return statements;
			}
		}
	}
}
