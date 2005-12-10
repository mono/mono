//
// JSObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) 2005, Novell Inc, (http://novell.com)
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

	internal class JSObjectEnumerator : IEnumerator {
		private IDictionaryEnumerator enumerator;

		internal JSObjectEnumerator (IDictionaryEnumerator enumerator)
		{
			this.enumerator = enumerator;
		}

		object IEnumerator.Current {
			get { return this.enumerator.Key; }
		}

		bool IEnumerator.MoveNext ()
		{
			return this.enumerator.MoveNext ();
		}

		void IEnumerator.Reset ()
		{
			this.enumerator.Reset ();
		}
	}

	public class JSObject : ScriptObject, IEnumerable, IExpando {

		public JSObject ()
		{
			elems = new Hashtable ();
			property_cache = new Hashtable ();
			
			/* Initialize cache for common properties if this is a Prototype */
			Type type = this.GetType ();
			if (type.GetField ("Proto", BindingFlags.NonPublic | BindingFlags.Static) != null)
				foreach (MemberInfo field in type.GetMembers ()) {
					if (field.DeclaringType != type)
						continue;

					string name = field.Name;
					if (name.StartsWith ("get_"))
						name = name.Substring (4);
					if (name == "Item" || name.StartsWith ("set_") || name.StartsWith ("."))
						continue;

					property_cache [LateBinding.MapToExternalName (name)] = this;
				}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new JSObjectEnumerator (elems.GetEnumerator ());
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			JSPropertyInfo pi = new JSPropertyInfo (name);
			elems.Add (name, pi);
			return pi;
		}
		
		MethodInfo IExpando.AddMethod (String name, Delegate method)
		{
			JSMethodInfo minfo = new JSMethodInfo (name, method.Method);
			elems.Add (name, minfo);
			return minfo;
		}
		
		public override MemberInfo [] GetMember (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMembers (BindingFlags bindFlags)
		{
			MemberInfo [] members;
			IEnumerator enumerator = elems.GetEnumerator ();
			ArrayList tmp = new ArrayList ();

			while (enumerator.MoveNext ())
				tmp.Add (((Node) enumerator.Current).Element);
			
			members = new MemberInfo [tmp.Count];
			tmp.CopyTo (members);
			return members;
		}

		public void SetMemberValue2 (string name, Object value)
		{
			AddField (name, value);
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			elems.Remove (m);
		}

		public override string ToString ()
		{
			return String.Format ("[{0}]", ClassName);
		}

		internal virtual object GetDefaultValue (Type hint, bool avoid_toString)
		{
			if (avoid_toString)
				return this;
			else
				return ObjectPrototype.smartToString (this);
		}

		internal object GetDefaultValue (Type hint)
		{
			return GetDefaultValue (hint, false);
		}
	}
}
