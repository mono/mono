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

		internal string name;
		internal string return_type;
		internal FormalParameterList parameters;
		internal Block body;

		internal FunctionObject (string name,
					 FormalParameterList p,
					 string return_type,
					 Block body)
		{
			this.name = name;
			this.parameters = p;
			this.return_type = return_type;
			this.body = body;
		}
	    
		internal FunctionObject ()
		{
			this.parameters = new FormalParameterList ();
			this.body = new Block ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("function ");
			sb.Append (name + " ");
			sb.Append ("(");

			if (parameters != null)
				sb.Append (this.parameters.ToString ());
					
			sb.Append (")");
			sb.Append (" : " + return_type);
			sb.Append ("{");

			if (body != null)
				sb.Append (body.ToString ());

			sb.Append ("}");

			return sb.ToString ();		
		}
	}
}
