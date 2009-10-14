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
using System.Collections.Generic;

namespace Microsoft.CSharp.RuntimeBinder
{
	static class CSharpBinder
	{
		static ConstructorInfo binder_exception_ctor;
		static object compiler_initializer = new object ();
		static object resolver = new object ();

		public static DynamicMetaObject Bind (DynamicMetaObject target, Compiler.Expression expr, BindingRestrictions restrictions, DynamicMetaObject errorSuggestion)
		{
			return Bind (target, expr, null, restrictions, errorSuggestion);
		}

		public static DynamicMetaObject Bind (DynamicMetaObject target, Compiler.Expression expr, Type callingType, BindingRestrictions restrictions, DynamicMetaObject errorSuggestion)
		{
			var ctx = CreateDefaultCompilerContext ();

			InitializeCompiler (ctx);

			Expression res;
			try {
				// TODO: ResolveOptions
				Compiler.ResolveContext rc = new Compiler.ResolveContext (new RuntimeBinderContext (ctx, callingType));

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

				res = CreateBinderException (target, e.Message);
			} catch (Exception) {
				if (errorSuggestion != null)
					return errorSuggestion;

				throw;
			}

			return new DynamicMetaObject (res, restrictions);
		}

		public static Expression CreateBinderException (DynamicMetaObject target, string message)
		{
			if (binder_exception_ctor == null)
				binder_exception_ctor = typeof (RuntimeBinderException).GetConstructor (new[] { typeof (string) });

			//
			// Uses target type to keep expressions composition working
			//
			return Expression.Throw (Expression.New (binder_exception_ctor, Expression.Constant (message)), target.LimitType);
		}

		//
		// Creates mcs expression from dynamic method object
		//
		public static Compiler.Expression CreateCompilerExpression (CSharpArgumentInfo info, DynamicMetaObject value)
		{
			if (value.Value == null)
				return new Compiler.NullLiteral (value.LimitType, Compiler.Location.Null);

			if (info != null && (info.Flags & CSharpArgumentInfoFlags.LiteralConstant) != 0) {
				InitializeCompiler (null);
				return Compiler.Constant.CreateConstant (value.RuntimeType ?? value.LimitType, value.Value, Compiler.Location.Null);
			}

			return new Compiler.RuntimeValueExpression (value);
		}

		public static Compiler.Arguments CreateCompilerArguments (IEnumerable<CSharpArgumentInfo> info, DynamicMetaObject[] args)
		{
			var res = new Compiler.Arguments (args.Length);
			int pos = 0;
			foreach (var item in info) {
				var expr = CreateCompilerExpression (item, args [pos]);
				if (item.IsNamed) {
					res.Add (new Compiler.NamedArgument (new Compiler.LocatedToken (Compiler.Location.Null, item.Name), expr));
				} else {
					res.Add (new Compiler.Argument (expr, item.ArgumentModifier));
				}
			}

			return res;
		}

		static Compiler.CompilerContext CreateDefaultCompilerContext ()
		{
			return new Compiler.CompilerContext (
				new Compiler.Report (ErrorPrinter.Instance) {
					WarningLevel = 0 
				}
			);
		}

		public static BindingRestrictions CreateRestrictionsOnTarget (DynamicMetaObject arg)
		{
			return arg.HasValue && arg.Value == null ?
				BindingRestrictions.GetInstanceRestriction (arg.Expression, null) :
				BindingRestrictions.GetTypeRestriction (arg.Expression, arg.LimitType);
		}

		public static BindingRestrictions CreateRestrictionsOnTarget (DynamicMetaObject[] args)
		{
			var res = CreateRestrictionsOnTarget (args[0]);
			for (int i = 1; i < args.Length; ++i)
				res = res.Merge (CreateRestrictionsOnTarget (args[i]));

			return res;
		}

		static void InitializeCompiler (Compiler.CompilerContext ctx)
		{
			if (Compiler.TypeManager.object_type != null)
				return;

			lock (compiler_initializer) {
				if (Compiler.TypeManager.object_type != null)
					return;

				// TODO: This smells like pretty big issue
				// AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { throw new NotImplementedException (); };

				// Add all currently loaded assemblies
				foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies ())
					Compiler.GlobalRootNamespace.Instance.AddAssemblyReference (a);

				if (ctx == null)
					ctx = CreateDefaultCompilerContext ();

				// FIXME: this is wrong
				Compiler.RootContext.ToplevelTypes = new Compiler.ModuleContainer (ctx, true);

				Compiler.TypeManager.InitCoreTypes (ctx);
				Compiler.TypeManager.InitOptionalCoreTypes (ctx);
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
