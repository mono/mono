//
// System.CodeDOM CodeAttributeDeclaration Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeAttributeDeclaration {

		string name;
		CodeAttributeArgumentCollection arguments;
		
		//
		// Constructors
		//
		public CodeAttributeDeclaration ()
		{
		}

		public CodeAttributeDeclaration (string name)
		{
			this.name = name;
		}

		public CodeAttributeDeclaration (string name, CodeAttributeArgument [] arguments)
		{
			this.name = name;
			this.arguments = new CodeAttributeArgumentCollection ();
			this.arguments.AddRange (arguments);
		}

		//
		// Properties
		//
		public CodeAttributeArgumentCollection Arguments {
			get {
				return arguments;
			}

			set {
				arguments = value;
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
	}
}

