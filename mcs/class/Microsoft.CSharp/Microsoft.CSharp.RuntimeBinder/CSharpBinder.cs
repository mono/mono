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
	class CSharpBinder
	{
		static ConstructorInfo binder_exception_ctor;
		static object resolver = new object ();

		DynamicMetaObjectBinder binder;
		Compiler.Expression expr;
		BindingRestrictions restrictions;
		DynamicMetaObject errorSuggestion;

		public CSharpBinder (DynamicMetaObjectBinder binder, Compiler.Expression expr, DynamicMetaObject errorSuggestion)
		{
			this.binder = binder;
			this.expr = expr;
			this.restrictions = BindingRestrictions.Empty;
			this.errorSuggestion = errorSuggestion;
		}

		public Compiler.ResolveContext.Options ResolveOptions { get; set; }

		public void AddRestrictions (DynamicMetaObject arg)
		{
			restrictions = restrictions.Merge (CreateRestrictionsOnTarget (arg));
		}

		public void AddRestrictions (DynamicMetaObject[] args)
		{
			restrictions = restrictions.Merge (CreateRestrictionsOnTarget (args));
		}

		public DynamicMetaObject Bind (DynamicContext ctx, Type callingType)
		{
			Expression res;
			try {
				var rc = new Compiler.ResolveContext (new RuntimeBinderContext (ctx, ctx.ImportType (callingType)), ResolveOptions);

				// Static typemanager and internal caches are not thread-safe
				lock (resolver) {
					expr = expr.Resolve (rc, Compiler.ResolveFlags.VariableOrValue);
				}

				if (expr == null)
					throw new RuntimeBinderInternalCompilerException ("Expression resolved to null");

				res = expr.MakeExpression (new Compiler.BuilderContext ());
			} catch (RuntimeBinderException e) {
				if (errorSuggestion != null)
					return errorSuggestion;

				res = CreateBinderException (e.Message);
			} catch (Exception) {
				if (errorSuggestion != null)
					return errorSuggestion;

				throw;
			}

			return new DynamicMetaObject (res, restrictions);
		}

		Expression CreateBinderException (string message)
		{
			if (binder_exception_ctor == null)
				binder_exception_ctor = typeof (RuntimeBinderException).GetConstructor (new[] { typeof (string) });

			//
			// Uses target type to keep expressions composition working
			//
			return Expression.Throw (Expression.New (binder_exception_ctor, Expression.Constant (message)), binder.ReturnType);
		}

		static BindingRestrictions CreateRestrictionsOnTarget (DynamicMetaObject arg)
		{
			return arg.HasValue && arg.Value == null ?
				BindingRestrictions.GetInstanceRestriction (arg.Expression, null) :
				BindingRestrictions.GetTypeRestriction (arg.Expression, arg.LimitType);
		}

		public static BindingRestrictions CreateRestrictionsOnTarget (DynamicMetaObject[] args)
		{
			if (args.Length == 0)
				return BindingRestrictions.Empty;

			var res = CreateRestrictionsOnTarget (args[0]);
			for (int i = 1; i < args.Length; ++i)
				res = res.Merge (CreateRestrictionsOnTarget (args[i]));

			return res;
		}
	}

	class DynamicContext
	{
		static DynamicContext dc;
		static object compiler_initializer = new object ();
		static object lock_object = new object ();

		readonly Compiler.CompilerContext cc;

		private DynamicContext (Compiler.CompilerContext cc)
		{
			this.cc = cc;
		}

		public Compiler.CompilerContext CompilerContext {
			get {
				return cc;
			}
		}

		public static DynamicContext Create ()
		{
			if (dc != null)
				return dc;

			lock (compiler_initializer) {
				if (dc != null)
					return dc;

				var importer = new Compiler.ReflectionMetaImporter () {
					IgnorePrivateMembers = false
				};

				var core_types = Compiler.TypeManager.InitCoreTypes ();
				importer.Initialize ();

				// I don't think dynamically loaded assemblies can be used as dynamic
				// expression without static type to be loaded first
				// AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { throw new NotImplementedException (); };

				// Import all currently loaded assemblies
				var ns = Compiler.GlobalRootNamespace.Instance;
				foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies ()) {
					ns.AddAssemblyReference (a);
					importer.ImportAssembly (a, ns);
				}

				var reporter = new Compiler.Report (ErrorPrinter.Instance) {
					WarningLevel = 0
				};

				var cc = new Compiler.CompilerContext (importer, reporter) {
					IsRuntimeBinder = true
				};

				Compiler.TypeManager.InitCoreTypes (cc, core_types);
				Compiler.TypeManager.InitOptionalCoreTypes (cc);

				dc = new DynamicContext (cc);
			}

			return dc;
		}

		//
		// Creates mcs expression from dynamic method object
		//
		public Compiler.Expression CreateCompilerExpression (CSharpArgumentInfo info, DynamicMetaObject value)
		{
			if (value.Value == null) {
				if (value.LimitType == typeof (object))
					return new Compiler.NullLiteral (Compiler.Location.Null);

				return Compiler.Constant.CreateConstantFromValue (ImportType (value.LimitType), null, Compiler.Location.Null);
			}

			bool is_compile_time;

			if (info != null) {
				if ((info.Flags & CSharpArgumentInfoFlags.Constant) != 0) {
					return Compiler.Constant.CreateConstantFromValue (ImportType (value.LimitType), value.Value, Compiler.Location.Null);
				}

				if ((info.Flags & CSharpArgumentInfoFlags.IsStaticType) != 0)
					return new Compiler.TypeExpression (ImportType ((Type) value.Value), Compiler.Location.Null);

				is_compile_time = (info.Flags & CSharpArgumentInfoFlags.UseCompileTimeType) != 0;
			} else {
				is_compile_time = false;
			}

			return new Compiler.RuntimeValueExpression (value, ImportType (is_compile_time ? value.LimitType : value.RuntimeType));
		}

		//
		// Creates mcs arguments from dynamic argument info
		//
		public Compiler.Arguments CreateCompilerArguments (IEnumerable<CSharpArgumentInfo> info, DynamicMetaObject[] args)
		{
			var res = new Compiler.Arguments (args.Length);
			int pos = 0;

			// enumerates over args
			foreach (var item in info) {
				var expr = CreateCompilerExpression (item, args[pos++]);
				if (item.IsNamed) {
					res.Add (new Compiler.NamedArgument (item.Name, Compiler.Location.Null, expr));
				} else {
					res.Add (new Compiler.Argument (expr, item.ArgumentModifier));
				}

				if (pos == args.Length)
					break;
			}

			return res;
		}

		public Compiler.TypeSpec ImportType (Type type)
		{
			lock (lock_object) {
				return cc.MetaImporter.ImportType (type);
			}
		}
	}
}
