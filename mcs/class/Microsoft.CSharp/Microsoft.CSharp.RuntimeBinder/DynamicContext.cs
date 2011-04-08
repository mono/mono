//
// DynamicContext.cs
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
using Compiler = Mono.CSharp;
using System.Reflection;
using System.Collections.Generic;

namespace Microsoft.CSharp.RuntimeBinder
{
	class DynamicContext
	{
		static DynamicContext dc;
		static object compiler_initializer = new object ();
		static object lock_object = new object ();

		readonly Compiler.ModuleContainer module;
		readonly Compiler.ReflectionImporter importer;

		private DynamicContext (Compiler.ModuleContainer module, Compiler.ReflectionImporter importer)
		{
			this.module = module;
			this.importer = importer;
		}

		public Compiler.CompilerContext CompilerContext {
			get {
				return module.Compiler;
			}
		}

		public Compiler.ModuleContainer Module {
			get {
				return module;
			}
		}

		public static DynamicContext Create ()
		{
			if (dc != null)
				return dc;

			lock (compiler_initializer) {
				if (dc != null)
					return dc;

				var reporter = new Compiler.Report (ErrorPrinter.Instance) {
					WarningLevel = 0
				};

				var settings = new Compiler.CompilerSettings ();

				var cc = new Compiler.CompilerContext (settings, reporter) {
					IsRuntimeBinder = true
				};

				//IList<Compiler.PredefinedTypeSpec> core_types = null;
				//// HACK: To avoid re-initializing static TypeManager types, like string_type
				//if (!Compiler.RootContext.EvalMode) {
				//    core_types = Compiler.TypeManager.InitCoreTypes ();
				//}

				//
				// Any later loaded assemblies are handled internally by GetAssemblyDefinition
				// domain.AssemblyLoad cannot be used as that would be too destructive as we
				// would hold all loaded assemblies even if they can be never visited
				//
				// TODO: Remove this code and rely on GetAssemblyDefinition only
				//
				var module = new Compiler.ModuleContainer (cc);
				module.HasTypesFullyDefined = true;
				var temp = new Compiler.AssemblyDefinitionDynamic (module, "dynamic");
				module.SetDeclaringAssembly (temp);

				// Import all currently loaded assemblies
				var domain = AppDomain.CurrentDomain;

				temp.Create (domain, System.Reflection.Emit.AssemblyBuilderAccess.Run);
				var importer = new Compiler.ReflectionImporter (module, cc.BuiltinTypes) {
					IgnorePrivateMembers = false
				};

				foreach (var a in AppDomain.CurrentDomain.GetAssemblies ()) {
					importer.ImportAssembly (a, module.GlobalRootNamespace);
				}

				cc.BuiltinTypes.CheckDefinitions (module);
				module.InitializePredefinedTypes ();

				dc = new DynamicContext (module, importer);
			}

			return dc;
		}

		//
		// Creates mcs expression from dynamic object
		//
		public Compiler.Expression CreateCompilerExpression (CSharpArgumentInfo info, DynamicMetaObject value)
		{
			//
			// No type details provider, go with runtime type
			//
			if (info == null) {
				if (value.LimitType == typeof (object))
					return new Compiler.NullLiteral (Compiler.Location.Null);

				return new Compiler.RuntimeValueExpression (value, ImportType (value.RuntimeType));
			}

			//
			// Value is known to be a type
			//
			if ((info.Flags & CSharpArgumentInfoFlags.IsStaticType) != 0)
				return new Compiler.TypeExpression (ImportType ((Type) value.Value), Compiler.Location.Null);

			if (value.Value == null &&
				(info.Flags & (CSharpArgumentInfoFlags.IsOut | CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.UseCompileTimeType)) == 0 &&
				value.LimitType == typeof (object)) {
				return new Compiler.NullLiteral (Compiler.Location.Null);
			}

			//
			// Use compilation time type when type was known not to be dynamic during compilation
			//
			Type value_type = (info.Flags & CSharpArgumentInfoFlags.UseCompileTimeType) != 0 ? value.Expression.Type : value.LimitType;
			var type = ImportType (value_type);

			if ((info.Flags & CSharpArgumentInfoFlags.Constant) != 0)
				return Compiler.Constant.CreateConstantFromValue (type, value.Value, Compiler.Location.Null);

			return new Compiler.RuntimeValueExpression (value, type);
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
					res.Add (new Compiler.NamedArgument (item.Name, Compiler.Location.Null, expr, item.ArgumentModifier));
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
				return importer.ImportType (type);
			}
		}
	}
}
