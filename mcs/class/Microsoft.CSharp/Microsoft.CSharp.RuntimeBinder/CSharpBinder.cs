//
// CSharpBinder.cs
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
using System.Linq.Expressions;
using Compiler = Mono.CSharp;
using System.Reflection;

namespace Microsoft.CSharp.RuntimeBinder
{
	class CSharpBinder
	{
		static ConstructorInfo binder_exception_ctor;
		static bool compiler_initialized;
		static object compiler_initializer = new object ();
		static object resolver = new object ();

		public static DynamicMetaObject Bind (DynamicMetaObject target, Compiler.Expression expr, BindingRestrictions restrictions, DynamicMetaObject errorSuggestion)
		{
			var report = new Compiler.Report (ErrorPrinter.Instance) { WarningLevel = 0 };
			var ctx = new Compiler.CompilerContext (report);
			Compiler.RootContext.ToplevelTypes = new Compiler.ModuleContainer (ctx, true);

			InitializeCompiler (ctx);

			Expression res;
			try {
				// TODO: ResolveOptions
				Compiler.ResolveContext rc = new Compiler.ResolveContext (new RuntimeBinderContext (ctx));

				// Static typemanager and internal caches are not thread-safe
				lock (resolver) {
					expr = expr.Resolve (rc);
				}

				if (expr == null)
					throw new RuntimeBinderInternalCompilerException ("Expression resolved to null");

				res = expr.MakeExpression (new Compiler.BuilderContext ());
			} catch (RuntimeBinderException e) {
				if (errorSuggestion != null)
					return errorSuggestion;

				if (binder_exception_ctor == null)
					binder_exception_ctor = typeof (RuntimeBinderException).GetConstructor (new[] { typeof (string) });

				//
				// Uses target type to keep expressions composition working
				//
				res = Expression.Throw (Expression.New (binder_exception_ctor, Expression.Constant (e.Message)), target.LimitType);
			} catch (Exception) {
				if (errorSuggestion != null)
					return errorSuggestion;

				throw;
			}

			return new DynamicMetaObject (res, restrictions);
		}

		//
		// Creates mcs expression from dynamic method object
		//
		public static Compiler.Expression CreateCompilerExpression (CSharpArgumentInfo info, DynamicMetaObject value, bool typed)
		{
			if (info.IsNamed)
				throw new NotImplementedException ("IsNamed");

			if (value.Value == null)
				return new Compiler.NullLiteral (value.LimitType, Compiler.Location.Null);

			if ((info.Flags & CSharpArgumentInfoFlags.LiteralConstant) != 0) {
				if (!typed)
					throw new NotImplementedException ("weakly typed constant");

				return Compiler.Constant.CreateConstant (value.RuntimeType ?? value.LimitType, value.Value, Compiler.Location.Null);
			}

			return new Compiler.RuntimeValueExpression (value, typed);
		}

		static void InitializeCompiler (Compiler.CompilerContext ctx)
		{
			if (compiler_initialized)
				return;

			lock (compiler_initializer) {
				if (compiler_initialized)
					return;

				// TODO: This smells like pretty big issue
				// AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { throw new NotImplementedException (); };

				// Add all currently loaded assemblies
				foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies ())
					Compiler.GlobalRootNamespace.Instance.AddAssemblyReference (a);

				Compiler.TypeManager.InitCoreTypes (ctx);
				Compiler.TypeManager.InitOptionalCoreTypes (ctx);
				compiler_initialized = true;
			}
		}

		public static DynamicMetaObject Bind (DynamicMetaObject target, DynamicMetaObject errorSuggestion, DynamicMetaObject[] args)
		{
			return Bind (target, errorSuggestion);
		}

		public static DynamicMetaObject Bind (DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			return errorSuggestion ??
				   new DynamicMetaObject (
						   Expression.Constant (new object ()),
						   target.Restrictions.Merge (
							   BindingRestrictions.GetTypeRestriction (
								   target.Expression, target.LimitType)));
		}
	}
}
