//
// JSObject.cs:
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

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices.Expando;

namespace Microsoft.JScript {

	public class JSObject : ScriptObject, IEnumerable, IExpando {

		public JSObject ()
		{}

		public FieldInfo AddField (string name)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			throw new NotImplementedException ();
		}
		
		MethodInfo IExpando.AddMethod (String name, Delegate method)
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

		public void SetMemberValue2 (string name, Object value)
		{
			throw new NotImplementedException ();
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
