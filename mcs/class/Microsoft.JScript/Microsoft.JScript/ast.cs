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

		internal AST parent;

		//
		// Here the actual IL code generation happens.
		//
		internal abstract void Emit (EmitContext ec);

		//
		// Perform type checks and associates expressions
		// with their declarations
		//
		internal abstract bool Resolve (IdentificationTable context);

		private bool InLoop {
			get {
				if (parent == null)
					return false;
				else if (parent is DoWhile || parent is While || parent is For || parent is ForIn)
					return true;
				else
					return parent.InLoop;
			}
		}
		
		private bool InSwitch {
			get {
				if (parent == null)
					return false;
				else if (parent is Switch)
					return true;
				else
					return parent.InSwitch;
			}
		}
	}
}
