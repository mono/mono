//
// System.CodeDom CodeParameterDeclarationExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeParameterDeclarationExpression
		: CodeExpression 
	{
		private CodeAttributeDeclarationCollection customAttributes;
		private FieldDirection direction;
		private string name;
		private CodeTypeReference type;

		//
		// Constructors
		//
		public CodeParameterDeclarationExpression ()
		{
		}

		public CodeParameterDeclarationExpression( CodeTypeReference type, string name )
		{
			this.type = type;
			this.name = name;
		}

		public CodeParameterDeclarationExpression (string type, string name)
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
		}

		public CodeParameterDeclarationExpression (Type type, string name)
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
		}

		//
		// Properties
		//
		public CodeAttributeDeclarationCollection CustomAttributes {
			get {
				if ( customAttributes == null )
					customAttributes = new CodeAttributeDeclarationCollection();
				return customAttributes;
			}
			set {
				customAttributes = value;
			}
		}

		public FieldDirection Direction {
			get {
				return direction;
			}
			set {
				direction = value;
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
