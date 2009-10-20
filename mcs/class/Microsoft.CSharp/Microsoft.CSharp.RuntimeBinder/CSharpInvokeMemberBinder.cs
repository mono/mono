//
// CSharpInvokeMemberBinder.cs
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
	class CSharpInvokeMemberBinder : InvokeMemberBinder
	{
		readonly CSharpBinderFlags flags;
		IList<CSharpArgumentInfo> argumentInfo;
		IList<Type> typeArguments;
		Type callingContext;
		
		public CSharpInvokeMemberBinder (CSharpBinderFlags flags, string name, Type callingContext, IEnumerable<Type> typeArguments, IEnumerable<CSharpArgumentInfo> argumentInfo)
			: base (name, false, CSharpArgumentInfo.CreateCallInfo (argumentInfo, 1))
		{
			this.flags = flags;
			this.callingContext = callingContext;
			this.argumentInfo = argumentInfo.ToReadOnly ();
			this.typeArguments = typeArguments.ToReadOnly ();
		}
		
		public IList<CSharpArgumentInfo> ArgumentInfo {
			get {
				return argumentInfo;
			}
		}
		
		public override bool Equals (object obj)
		{
			var other = obj as CSharpInvokeMemberBinder;
			return other != null && other.Name == Name && other.flags == flags && other.callingContext == callingContext && 
				other.argumentInfo.SequenceEqual (argumentInfo) && other.typeArguments.SequenceEqual (typeArguments);
		}

		public override int GetHashCode ()
		{
			return Extensions.HashCode (
				flags.GetHashCode (),
				callingContext.GetHashCode (),
				argumentInfo.GetHashCode (),
				typeArguments.GetHashCode (),
				Name.GetHashCode ());
		}
		
		public override DynamicMetaObject FallbackInvoke (DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			var b = new CSharpInvokeBinder (flags, callingContext, argumentInfo);
			
			// TODO: Is errorSuggestion ever used?
			return b.Defer (target, args);
		}
		
		public override DynamicMetaObject FallbackInvokeMember (DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			return CSharpBinder.Bind (target, errorSuggestion, args);
/*			
			var expr = CSharpBinder.CreateCompilerExpression (argumentInfo[0], target);
			var c_args = CSharpBinder.CreateCompilerArguments (argumentInfo.Skip (1), args);

			expr = new Compiler.MemberAccess (expr, Name, null, Compiler.Location.Null);
			expr = new Compiler.Invocation (expr, c_args);

			var restrictions = CSharpBinder.CreateRestrictionsOnTarget (target).Merge (
				CSharpBinder.CreateRestrictionsOnTarget (args));

			return CSharpBinder.Bind (target, expr, callingContext, restrictions, errorSuggestion);
*/
		}
		
		public IList<Type> TypeArguments {
			get {
				return typeArguments;
			}
		}
	}
}
