//
// SemanticAnaliser.cs: A implementation of the Visitor interface.
//			It performs the Semantic Analysis for EcmaScript
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript
{
	internal class SemanticAnaliser : Visitor
	{
		private IdentificationTable symTab;

		public object VisitASTList (ASTList prog, object obj)
		{
			throw new NotImplementedException ();
		}


		public object VisitVariableDeclaration (VariableDeclaration decl, 
							object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitFunctionDeclaration (FunctionDeclaration decl,
							object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitArrayLiteral (ArrayLiteral al, object args)
		{
			throw new NotImplementedException ();
		}

		
		public object VisitBlock (Block b, object args)
		{
			throw new NotImplementedException ();
		}

		
		public object VisitEval (Eval e, object args)
		{	
			throw new NotImplementedException ();
		}

		
		public object VisitForIn (ForIn forIn, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitFunctionExpression (FunctionExpression fexp,	
						       object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitImport (Import imp, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitPackage (Package pkg, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitScriptBlock (ScriptBlock sblock, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitThrow (Throw t, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitTry (Try t, object args)
		{
			throw new NotImplementedException ();
		}


		public object VisitWith (With w, object args)
		{
			throw new NotImplementedException ();
		}
	}
}
