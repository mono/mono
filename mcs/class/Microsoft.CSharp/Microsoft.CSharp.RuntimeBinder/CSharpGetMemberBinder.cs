//
// CSharpGetMemberBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using Compiler = Mono.CSharp;

namespace Microsoft.CSharp.RuntimeBinder
{
	public class CSharpGetMemberBinder : GetMemberBinder
	{
		IList<CSharpArgumentInfo> argumentInfo;
		Type callingContext;
		
		public CSharpGetMemberBinder (string name, Type callingContext, IEnumerable<CSharpArgumentInfo> argumentInfo)
			: base (name, false)
		{
			this.callingContext = callingContext;
			this.argumentInfo = argumentInfo.ToReadOnly ();
		}
		
		public IList<CSharpArgumentInfo> ArgumentInfo {
			get {
				return argumentInfo;
			}
		}

		public Type CallingContext {
			get {
				return callingContext;
			}
		}
		
		public override bool Equals (object obj)
		{
			var other = obj as CSharpGetMemberBinder;
			return other != null && other.Name == Name && other.callingContext == callingContext && 
				other.argumentInfo.SequenceEqual (argumentInfo);
		}

		public override int GetHashCode ()
		{
			return Extensions.HashCode (
				Name.GetHashCode (),
				callingContext.GetHashCode (),
				argumentInfo.GetHashCode ());
		}

		public override DynamicMetaObject FallbackGetMember (DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var expr = CSharpBinder.CreateCompilerExpression (argumentInfo [0], target, true);
			expr = new Compiler.MemberAccess (expr, Name);
			expr = new Compiler.Cast (new Compiler.TypeExpression (typeof (object), Compiler.Location.Null), expr);

			var restrictions = CSharpBinder.CreateRestrictionsOnTarget (target);

			if (target.Value == null) {
				if (errorSuggestion != null)
					return errorSuggestion;

				var ex = CSharpBinder.CreateBinderException (target, "GetMember cannot perform binding on `null' value");
				return new DynamicMetaObject (ex, restrictions);
			}

			return CSharpBinder.Bind (target, expr, callingContext, restrictions, errorSuggestion);
		}
	}
}
