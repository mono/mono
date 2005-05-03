//
// TypedArray.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
using System.Globalization;

namespace Microsoft.JScript {

	public sealed class TypedArray : IReflect
	{
		public TypedArray (IReflect elementType, int rank)
		{
			throw new NotImplementedException ();
		}


		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}


		public FieldInfo GetField (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public FieldInfo [] GetFields (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}


		public MemberInfo [] GetMember (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MemberInfo [] GetMembers (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo GetMethod (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo GetMethod (string name, BindingFlags bindAttr, System.Reflection.Binder binder,
					     Type [] types, ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo [] GetMethods (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo GetProperty (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo GetProperty (string name, BindingFlags bindAttr, System.Reflection.Binder binder,
						 Type returnType, Type [] types, 
						 ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo [] GetProperties (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public object InvokeMember (string name, BindingFlags flags, System.Reflection.Binder binder,
					    object target, object [] args, 
					    ParameterModifier [] modifiers, CultureInfo locale,
					    string [] namedParameters)
		{
			throw new NotImplementedException ();
		}


		public override string ToString ()
		{
			throw new NotImplementedException ();
		}


		public Type UnderlyingSystemType {
			get { throw new NotImplementedException (); }
		}
	}
}
