//
// CSharpConvertBinder.cs
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
	class CSharpConvertBinder : ConvertBinder
	{
		readonly CSharpBinderFlags flags;
		readonly Type context;

		public CSharpConvertBinder (Type type, Type context, CSharpBinderFlags flags)
			: base (type, (flags & CSharpBinderFlags.ConvertExplicit) != 0)
		{
			this.flags = flags;
			this.context = context;
		}

		public override DynamicMetaObject FallbackConvert (DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var expr = CSharpBinder.CreateCompilerExpression (null, target);

			if (Explicit)
				expr = new Compiler.Cast (new Compiler.TypeExpression (TypeImporter.Import (Type), Compiler.Location.Null), expr, Compiler.Location.Null);
			else
				expr = new Compiler.ImplicitCast (expr, TypeImporter.Import (Type), (flags & CSharpBinderFlags.ConvertArrayIndex) != 0);

			if ((flags & CSharpBinderFlags.CheckedContext) != 0)
				expr = new Compiler.CheckedExpr (expr, Compiler.Location.Null);

			var binder = new CSharpBinder (this, expr, errorSuggestion);
			binder.AddRestrictions (target);

			return binder.Bind (context);
		}
	}
}
