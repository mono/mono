//
// FunctionDeclaration.cs:
//
// Author:
//	 Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.Text;

	public class FunctionDeclaration : AST
	{
		internal string id;
		internal FormalParameterList parameters;
		internal ASTList funcBody;
	
		public static Closure JScriptFunctionDeclaration (RuntimeTypeHandle handle, string name, 
								  string methodName, string [] formalParameters,
								  JSLocalField [] fields, bool mustSaveStackLocals,
								  bool hasArgumentsObjects, string text, 
								  Object declaringObject, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		
		internal FunctionDeclaration ()
		{
			parameters = new FormalParameterList ();
			funcBody = new ASTList ();
		}

		
		public override object Visit (Visitor v, object args)
		{
			return v.VisitFunctionDeclaration (this, args);
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("id: " + id + "\n");
			sb.Append ("parameters: " + parameters.ToString () + "\n");
			sb.Append ("functionBody: " + funcBody.ToString ());
			
			return sb.ToString ();
		}		
	}
}
