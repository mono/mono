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
using System.Reflection;
using System.Collections;

namespace Microsoft.JScript {

	public class FunctionObject : ScriptFunction {

		internal MethodAttributes attr;
		internal string name;
		internal string type_annot;
		internal Type return_type;
		internal FormalParameterList parameters;
		internal Block body;

		internal FunctionObject (string name,
					 FormalParameterList p,
					 string ret_type,
					 Block body)
		{
			//
			// FIXME: 
			// 1) Must collect the attributes given.
			// 2) Check if they are semantically correct.
			// 3) Assign those values to 'attr'.
			//
			this.attr = MethodAttributes.Public | MethodAttributes.Static;

			this.name = name;
			this.parameters = p;

			this.type_annot = ret_type;
			//
			// FIXME: Must check that return_type it's a valid type,
			// and assign that to 'return_type' field.
			//
			this.return_type = typeof (Object);

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

		internal Type [] params_types ()
		{
			if (parameters == null)
				return new Type [] {};
			else {
				int i, size;
				ArrayList p = parameters.ids;

				size = p.Count + 2;

				Type [] types = new Type [size];

				types [0] = typeof (Object);
				types [1] = typeof (Microsoft.JScript.Vsa.VsaEngine);

				for (i = 2; i < size; i++)
					types [i] = ((FormalParam) p [i - 2]).type;

				return types;
			}
		}
	}
}
