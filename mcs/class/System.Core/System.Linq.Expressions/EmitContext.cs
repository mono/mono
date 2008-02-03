//
// EmitContext.cs
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	abstract class EmitContext {

		protected LambdaExpression owner;
		protected Type [] param_types;

		public ILGenerator ig;

		static object mlock = new object ();
		static int method_count;

		protected EmitContext (LambdaExpression lambda)
		{
			this.owner = lambda;

			param_types = owner.Parameters.Select (p => p.Type).ToArray ();
		}

		public static EmitContext Create (LambdaExpression lambda)
		{
			if (Environment.GetEnvironmentVariable ("LINQ_DBG") != null)
				return new DebugEmitContext (lambda);

			return new DynamicEmitContext (lambda);
		}

		public int GetParameterPosition (ParameterExpression p)
		{
			int position = owner.Parameters.IndexOf (p);
			if (position == -1)
				throw new InvalidOperationException ("Parameter not in scope");

			return position;
		}

		public abstract Delegate CreateDelegate ();

		public abstract void Emit ();

		protected static string GenerateName ()
		{
			lock (mlock) {
				return "lambda_method-" + (method_count++);
			}
		}
	}

	class DynamicEmitContext : EmitContext {

		DynamicMethod method;

		public DynamicEmitContext (LambdaExpression lambda)
			: base (lambda)
		{
			//
			// We probably want to use the 3.5 new API calls to associate
			// the method with the "sandboxed" Assembly, instead am currently
			// dumping these types in this class
			//
			Type owner_of_code = typeof (EmitContext);

			//
			// FIXME: Need to force this to be verifiable, see:
			// https://bugzilla.novell.com/show_bug.cgi?id=355005
			//
			method = new DynamicMethod (GenerateName (), lambda.Body.Type, param_types, owner_of_code);
			ig = method.GetILGenerator ();
		}

		public override Delegate CreateDelegate ()
		{
			return method.CreateDelegate (owner.Type);
		}

		public override void Emit ()
		{
			owner.Emit (this);
		}
	}

	class DebugEmitContext : EmitContext {

		AssemblyBuilder assembly;
		TypeBuilder type;
		MethodBuilder method;

		public DebugEmitContext (LambdaExpression lambda)
			: base (lambda)
		{
			var name = GenerateName ();
			var file_name = name + ".dll";

			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				new AssemblyName (file_name), AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

			type = assembly.DefineDynamicModule (file_name, file_name).DefineType ("Linq", TypeAttributes.Public);

			method = type.DefineMethod (name, MethodAttributes.Public | MethodAttributes.Static, owner.Body.Type, param_types);
			ig = method.GetILGenerator ();
		}

		public override Delegate CreateDelegate ()
		{
			return Delegate.CreateDelegate (owner.Type, GetMethod ());
		}

		MethodInfo GetMethod ()
		{
			return type.GetMethod (method.Name, BindingFlags.Static | BindingFlags.Public);
		}

		public override void Emit ()
		{
			owner.Emit (this);

			type.CreateType ();
			assembly.Save (assembly.GetName ().FullName);
		}
	}
}
