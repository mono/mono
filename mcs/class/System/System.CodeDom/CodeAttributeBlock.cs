//
// System.CodeDom CodeAttributeBlock Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeAttributeBlock {

		CodeAttributeDeclarationCollection attributes;
		
		//
		// Constructors
		//
		public CodeAttributeBlock ()
		{
		}
		
		public CodeAttributeBlock (CodeAttributeDeclaration [] attributes)
		{
			this.attributes = new CodeAttributeDeclarationCollection ();
			this.attributes.AddRange (attributes);
		}

		//
		// Prpoperties
		//
		public CodeAttributeDeclarationCollection Attributes {
			get {
				return attributes;
			}

			set {
				attributes = value;
			}
		}
	}
}
