//
// ExecutionScope.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//	Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2007 - 2008, Novell, Inc (http://www.novell.com)
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
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices {

#if MOONLIGHT
	[Obsolete ("do not use this type", true)]
#endif
	public class ExecutionScope {

		public object [] Globals;
		public object [] Locals;
		public ExecutionScope Parent;

#if !MOONLIGHT
		internal CompilationContext context;
#endif
		internal int compilation_unit;

#if !MOONLIGHT
		ExecutionScope (CompilationContext context, int compilation_unit)
		{
			this.context = context;
			this.compilation_unit = compilation_unit;
			this.Globals = context.GetGlobals ();
		}

		internal ExecutionScope (CompilationContext context)
			: this (context, 0)
		{
		}

		internal ExecutionScope (CompilationContext context, int compilation_unit, ExecutionScope parent, object [] locals)
			: this (context, compilation_unit)
		{
			this.Parent = parent;
			this.Locals = locals;
		}
#endif
		public Delegate CreateDelegate (int indexLambda, object [] locals)
		{
#if MOONLIGHT
			throw new NotSupportedException ();
#else
			return context.CreateDelegate (
				indexLambda,
				new ExecutionScope (context, indexLambda, this, locals));
#endif
		}

		public object [] CreateHoistedLocals ()
		{
#if MOONLIGHT
			throw new NotSupportedException ();
#else
			return context.CreateHoistedLocals (compilation_unit);
#endif
		}

		public Expression IsolateExpression (Expression expression, object [] locals)
		{
#if MOONLIGHT
			throw new NotSupportedException ();
#else
			return context.IsolateExpression (this, locals, expression);
#endif
		}
	}
}
