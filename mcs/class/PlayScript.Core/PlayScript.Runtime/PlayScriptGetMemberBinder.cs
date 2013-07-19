//
// PlayScriptGetMemberBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using PSCompiler = Mono.PlayScript;

namespace PlayScript.Runtime
{
	class PlayScriptGetMemberBinder : GetMemberBinder
	{
		Type callingContext;
		DefaultObjectContext objectContext;
		
		public PlayScriptGetMemberBinder (string name, Type callingContext, DefaultObjectContext objectContext)
			: base (name, false)
		{
			this.callingContext = callingContext;
			this.objectContext = objectContext;
		}
		
		public override DynamicMetaObject FallbackGetMember (DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var ctx = DynamicContext.Create ();

			Compiler.Expression expr = new PSCompiler.SimpleName (Name, Compiler.Location.Null);
			expr = new Compiler.Cast (new Compiler.TypeExpression (ctx.ImportType (ReturnType), Compiler.Location.Null), expr, Compiler.Location.Null);

			var binder = new CSharpBinder (this, expr, errorSuggestion);
			binder.AddRestrictions (target);

			// TODO: RuntimeBinderContext is not enough, it won't search global extension methods for builtin types
			var rc = new Compiler.ResolveContext (new RuntimeBinderContext (ctx, callingContext), binder.ResolveOptions);
			rc.DefaultObjectContext = new PSCompiler.WithContext (objectContext.Context);	

			return binder.Bind (rc);
		}
	}
}
