//
// RegExpConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class RegExpConstructor : ScriptFunction
	{
		public Object Construct (string pattern, bool ignoreCase, bool global, bool multiLine)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public new RegExpObject CreateInstance(params Object[] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public RegExpObject Invoke(params Object[] args)
		{
			throw new NotImplementedException ();
		}

		public Object index {
			get { throw new NotImplementedException (); }
		}

		public Object input {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Object lastIndex {
			get { throw new NotImplementedException (); }
		}

		public Object lastMatch {
			get { throw new NotImplementedException (); }
		}

		public Object lastParen {
			get { throw new NotImplementedException (); }
		}

		public Object leftContext {
			get { throw new NotImplementedException (); }
		}

		public Object rightContext {
			get { throw new NotImplementedException (); }
		}
	}
}