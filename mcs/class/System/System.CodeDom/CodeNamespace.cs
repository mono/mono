//
// System.CodeDom CodeNamespace Class implementation
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
	public class CodeNamespace
		: CodeObject
	{
		private CodeCommentStatementCollection comments;
		private CodeNamespaceImportCollection imports;
		private CodeTypeDeclarationCollection types;
		private string name;

		//
		// Constructors
		//
		public CodeNamespace()
		{
		}

		public CodeNamespace(string name)
		{
			this.name = name;
		}

		//
		// Properties
		//
		public CodeCommentStatementCollection Comments {
			get {
				if ( comments == null ) {
					comments = new CodeCommentStatementCollection();
					PopulateComments( this, EventArgs.Empty );
				}
				return comments;
			}
		}

		public CodeNamespaceImportCollection Imports {
			get {
				if ( imports == null ) {
					imports = new CodeNamespaceImportCollection();
					PopulateImports( this, EventArgs.Empty );
				}
				return imports;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public CodeTypeDeclarationCollection Types {
			get {
				if ( types == null ) {
					types = new CodeTypeDeclarationCollection();
					PopulateTypes( this, EventArgs.Empty );
				}
				return types;
			}
		}

		//
		// Events
		//
		public event EventHandler PopulateComments;
		
		public event EventHandler PopulateImports;
		
		public event EventHandler PopulateTypes;
	}
}
