//
// BinaryOp.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript {

	public abstract class BinaryOp : Exp {

		internal AST left, right;

		//
		// We transform rules of the form:
		//
		// 	E -> E Op R
		//	E -> R
		//
		// to rules of the form:
		//
		//	E -> R E_aux
		//	E_aux -> (Op R E_aux | )
		//
		// so we must keep track of the 
		// two operators (in the case where Op can be
		// more than one operator).
		//

		internal JSToken old_op;
		internal JSToken current_op;
	}
}
