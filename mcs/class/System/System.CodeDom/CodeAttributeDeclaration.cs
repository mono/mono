//
// System.CodeDom CodeAttributeDeclaration Class implementation
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
	public class CodeAttributeDeclaration 
	{
		private string name;
		private CodeAttributeArgumentCollection arguments;
		
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

		public CodeAttributeDeclaration (string name, params CodeAttributeArgument [] arguments)
		{
			this.name = name;
			Arguments.AddRange (arguments);
		}

		//
		// Properties
		//
		public CodeAttributeArgumentCollection Arguments {
			get {
				if ( arguments == null )
					arguments = new CodeAttributeArgumentCollection();

				return arguments;
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

