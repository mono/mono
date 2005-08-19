//
// Binding.cs:
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
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public abstract class Binding : AST {

		protected MemberInfo defaultMember;
		protected bool isAssignmentToDefaultIndexedProperty;
		protected bool isFullyResolved;
		protected bool isNonVirtual;
		protected string name;
		
		protected abstract Object GetObject ();
		protected abstract void HandleNoSuchMemberError ();

		private Binding ()
			: base (null, null)
		{
		}

		protected void ResolveRHValue ()
		{
			throw new NotImplementedException ();
		}

		protected abstract void TranslateToILObject (ILGenerator il, Type obtype, bool noValue);

		protected abstract void TranslateToILWithDupOfThisOb (ILGenerator il);

		public static bool IsMissing (Object value)
		{
			// FIXME
			return false;
		}	    
	}
}		
