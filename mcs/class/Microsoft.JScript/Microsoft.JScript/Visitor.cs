//
// Visitor.cs: A interface for transversing the AST.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public interface Visitor
	{
		object VisitASTList (ASTList prog, object obj);

		object VisitVariableDeclaration (VariableDeclaration decl, object args);

		object VisitFunctionDeclaration (FunctionDeclaration decl, object args);

		object VisitArrayLiteral (ArrayLiteral al, object args);

		object VisitBlock (Block b, object args);

		object VisitEval (Eval e, object args);

		object VisitForIn (ForIn fi, object args);

		object VisitFunctionExpression (FunctionExpression fe, object args);

		object VisitImport (Import imp, object args);

		object VisitPackage (Package imp, object args);

		object VisitScriptBlock (ScriptBlock sblock, object args);

		object VisitThrow (Throw t, object args);

		object VisitTry (Try t, object args);

		object VisitWith (With w, object args);
	}
}
