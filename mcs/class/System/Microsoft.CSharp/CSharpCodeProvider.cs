//
// Microsoft.CSharp CSharpCodeProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

namespace Microsoft.CSharp
{
	using System;
	using System.CodeDom.Compiler;
	using System.ComponentModel;

	public class CSharpCodeProvider
		: CodeDomProvider
	{
		//
		// Constructors
		//
		public CSharpCodeProvider()
		{
		}

		//
		// Properties
		//
		public override string FileExtension {
			get {
				return "cs";
			}
		}

		//
		// Methods
		//
		public override ICodeCompiler CreateCompiler()
		{
			return new Mono.CSharp.CSharpCodeCompiler();
		}

		public override ICodeGenerator CreateGenerator()
		{
			return new Mono.CSharp.CSharpCodeGenerator();
		}
		
		[MonoTODO]
		public override TypeConverter GetConverter( Type Type )
		{
			throw new NotImplementedException();
		}
	}
}
