//
// FunctionObject.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;

namespace Microsoft.JScript {

	public class FunctionObject : ScriptFunction {

		internal string Name;
		internal string ReturnType;
		internal FormalParameterList Params;
		internal Block Body;

		internal FunctionObject (string name,
					 FormalParameterList p,
					 Block body)
		{
			this.Name = name;
			this.Params = p;
			this.Body = body;
		}
	    
		internal FunctionObject ()
		{
			Params = new FormalParameterList ();
			Body = new Block ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("function ");
			sb.Append (Name + " ");
			sb.Append ("(");

			if (Params != null)
				sb.Append (Params.ToString ());
					
			sb.Append (")");
			sb.Append ("{");

			if (Body != null)
				sb.Append (Body.ToString ());

			sb.Append ("}");

			return sb.ToString ();		
		}
	}
}
