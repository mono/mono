//
// System.CodeDom CodeMemberField Class implementation
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
	public class CodeMemberField
		: CodeTypeMember
	{
		private CodeExpression initExpression;
		private CodeTypeReference type;
		
		public CodeMemberField ()
		{
		}

		public CodeMemberField (CodeTypeReference type, string name)
		{
			this.type = type;
			this.Name = name;
		}
		
		public CodeMemberField (string type, string name)
		{
			this.type = new CodeTypeReference( type );
			this.Name = name;
		}
			
		public CodeMemberField (Type type, string name)
		{
			this.type = new CodeTypeReference( type );
			this.Name = name;
		}

		//
		// Properties
		//
		public CodeExpression InitExpression {
			get {
				return initExpression;
			}
			set {
				initExpression = value;
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
