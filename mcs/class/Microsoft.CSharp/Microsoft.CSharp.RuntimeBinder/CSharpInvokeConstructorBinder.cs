//
// CSharpInvokeConstructorBinder.cs
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
	class CSharpInvokeConstructorBinder : DynamicMetaObjectBinder
	{
		IList<CSharpArgumentInfo> argumentInfo;
		Type callingContext;
		Type target_return_type;

		public CSharpInvokeConstructorBinder (Type callingContext, IEnumerable<CSharpArgumentInfo> argumentInfo)
		{
			this.callingContext = callingContext;
			this.argumentInfo = argumentInfo.ToReadOnly ();
		}

		public override DynamicMetaObject Bind (DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var ctx = DynamicContext.Create ();

			var type = ctx.CreateCompilerExpression (argumentInfo [0], target);
			target_return_type = type.Type.GetMetaInfo ();

			var c_args = ctx.CreateCompilerArguments (argumentInfo.Skip (1), args);

			var binder = new CSharpBinder (
				this, new Compiler.New (type, c_args, Compiler.Location.Null), null);

			binder.AddRestrictions (target);
			binder.AddRestrictions (args);

			return binder.Bind (ctx, callingContext);
		}

		public override Type ReturnType {
			get {
				return target_return_type;
			}
		}
	}
}
