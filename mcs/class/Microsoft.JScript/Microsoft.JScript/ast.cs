//
// ast.cs: Base class for the EcmaScript program tree representation.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript {

	public abstract class AST {

		//
		// Here the actual IL code generation happens.
		//
		internal abstract void Emit (EmitContext ec);

		//
		// Perform type checks and associates expressions
		// with their declarations
		//
		internal abstract bool Resolve (IdentificationTable context);
	}
}
