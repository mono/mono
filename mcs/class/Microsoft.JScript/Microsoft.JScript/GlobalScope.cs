//
// GlobalScope.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Reflection;
using System;
using System.Runtime.InteropServices.Expando;
using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript {

	public class GlobalScope : ActivationObject, IExpando {

		public GlobalScope (GlobalScope parent, VsaEngine engine)
			: this (parent, engine, engine != null)
		{
			this.elems = new Hashtable ();
			this.property_cache = new Hashtable ();
		}

		internal GlobalScope (GlobalScope parent, VsaEngine engine, bool is_comp_scope)
		{
			this.elems = new Hashtable ();
			this.parent = parent;
			this.engine = engine;
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			throw new NotImplementedException ();
		}

		MethodInfo IExpando.AddMethod (string name, Delegate method)
		{
			throw new NotImplementedException ();
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			throw new NotImplementedException ();
		}

		public override Object GetDefaultThisObject ()
		{
			return this;
		}

		public override FieldInfo GetField (string name, int lexLevel)
		{
			return GetField (name);
		}

		public override FieldInfo [] GetFields (BindingFlags bidFlags)
		{
			throw new NotImplementedException ();
		}

		public override GlobalScope GetGlobalScope ()
		{
			return this;
		}

		public override FieldInfo GetLocalField (string name)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMember (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMembers (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MethodInfo [] GetMethods (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override PropertyInfo [] GetProperties (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}
	}
}
