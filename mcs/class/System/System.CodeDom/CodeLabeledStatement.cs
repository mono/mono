//
// System.CodeDom CodeLabeledStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeLabeledStatement
		: CodeStatement
	{
		private string label;
		private CodeStatement statement;

		//
		// Constructors
		//
		public CodeLabeledStatement()
		{
		}

		public CodeLabeledStatement( string label )
		{
			this.label = label;
		}

		public CodeLabeledStatement( string label, CodeStatement statement )
		{
			this.label = label;
			this.statement = statement;
		}

		//
		// Properties
		//
		public string Label {
			get {
				return label;
			}
			set {
				label = value;
			}
		}

		public CodeStatement Statement {
			get {
				return statement;
			}
			set {
				statement = value;
			}
		}
	}
}
