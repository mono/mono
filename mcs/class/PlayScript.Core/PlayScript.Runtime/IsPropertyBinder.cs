//
// IsPropertyBinder.cs
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
using Microsoft.CSharp.RuntimeBinder;
using Compiler = Mono.CSharp;

namespace PlayScript.Runtime
{
	class IsPropertyBinder : DynamicMetaObjectBinder
	{
		Type callingContext;
		string name;
		bool static_only;
		
		public IsPropertyBinder (string name, Type callingContext, bool staticOnly)
		{
			this.name = name;
			this.callingContext = callingContext;
			this.static_only = staticOnly;
		}
		
		public override DynamicMetaObject Bind (DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var ctx = DynamicContext.Create ();
			var context_type = ctx.ImportType (callingContext);
			var queried_type = ctx.ImportType ((Type) target.Value);
			var rc = new Compiler.ResolveContext (new RuntimeBinderContext (ctx, context_type), 0);

			var expr = Compiler.Expression.MemberLookup (rc, false, queried_type,
				name, 0, Compiler.Expression.MemberLookupRestrictions.ExactArity, Compiler.Location.Null);

			var pe = expr as Compiler.PropertyExpr;			
			var result = pe != null && pe.IsStatic == static_only;

			var binder = new CSharpBinder (
				this, new Compiler.BoolConstant (ctx.CompilerContext.BuiltinTypes, result, Compiler.Location.Null), null);

			binder.AddRestrictions (target);
			return binder.Bind (ctx, callingContext);
		}

		public override Type ReturnType {
			get {
				return typeof (bool);
			}
		}
	}
}
