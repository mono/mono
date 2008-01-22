//
// LambdaExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace System.Linq.Expressions {

	internal class EmitContext {
		internal LambdaExpression Owner;
		internal Type [] ParamTypes;
		internal DynamicMethod Method;
		internal ILGenerator ig;

		static object mlock = new object ();
		internal static int method_count;
		static string GenName ()
		{
			lock (mlock){
				return "<LINQ-" + method_count++ + ">";
			}
		}
		
		public EmitContext (LambdaExpression owner)
		{
			Owner = owner;

			ParamTypes = new Type [Owner.parameters.Count];
			for (int i = 0; i < Owner.parameters.Count; i++)
				ParamTypes [i] = Owner.parameters [i].Type;

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
			string name = GenName ();
			Method = new DynamicMethod (name, Owner.Type, ParamTypes, owner_of_code);
			
			ig = Method.GetILGenerator ();
		}

		internal Delegate CreateDelegate ()
		{
			return Method.CreateDelegate (Owner.delegate_type);			
		}

		internal int GetParameterPosition (ParameterExpression p)
		{
			int position = Owner.Parameters.IndexOf (p);
			if (position == -1)
				throw new InvalidOperationException ("Parameter not in scope");

			return position;
		}
	}

	public class LambdaExpression : Expression {

		//
		// LambdaExpression parameters
		//
		Expression body;
		internal ReadOnlyCollection<ParameterExpression> parameters;
		internal Type delegate_type;

		// This is set during compilation
		Delegate lambda_delegate;

		static bool CanAssign (Type target, Type source)
		{
			// This catches object and value type mixage, type compatibility is handled later
			if (target.IsValueType ^ source.IsValueType)
				return false;
			
			return target.IsAssignableFrom (source);
		}
				
		internal LambdaExpression (Type delegateType, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
			: base (ExpressionType.Lambda, body.Type)
		{
			if (!delegateType.IsSubclassOf (typeof (System.Delegate)))
				throw new ArgumentException ("delegateType");

			var invoke = delegateType.GetMethod ("Invoke", BindingFlags.Instance | BindingFlags.Public);
			if (invoke == null)
				throw new ArgumentException ("delegate must contain an Invoke method", "delegateType");

			var invoke_parameters = invoke.GetParameters ();
			if (invoke_parameters.Length != parameters.Count)
				throw new ArgumentException ("Different number of arguments in delegate {0}", "delegateType");

			for (int i = 0; i < invoke_parameters.Length; i++){
				if (!CanAssign (parameters [i].Type, invoke_parameters [i].ParameterType))
					throw new ArgumentException (String.Format ("Can not assign a {0} to a {1}", invoke_parameters [i].ParameterType, parameters [i].Type));
			}

			if (!CanAssign (invoke.ReturnType, body.Type))
				throw new ArgumentException (String.Format ("body type {0} can not be assigned to {1}", body.Type, invoke.ReturnType));
			
			this.body = body;
			this.parameters = parameters;
			delegate_type = delegateType;
		}
		
		public Expression Body {
			get { return body; }
		}

		public ReadOnlyCollection<ParameterExpression> Parameters {
			get { return parameters; }
		}

		internal override void Emit (EmitContext ec)
		{
			body.Emit (ec);
			ec.ig.Emit (OpCodes.Ret);
		}

		public Delegate Compile ()
		{
			if (lambda_delegate == null){
				var ec = new EmitContext (this);

				Emit (ec);

				if (Environment.GetEnvironmentVariable ("LINQ_DBG") != null){
					string fname = "linq" + (EmitContext.method_count-1) + ".dll";
					AssemblyBuilder ab = Thread.GetDomain ().DefineDynamicAssembly (
						new AssemblyName (fname), AssemblyBuilderAccess.RunAndSave, "/tmp");
					
					ModuleBuilder b = ab.DefineDynamicModule (fname, fname);
					TypeBuilder tb = b.DefineType ("LINQ", TypeAttributes.Public);
					MethodBuilder mb = tb.DefineMethod ("GeneratedMethod", MethodAttributes.Static, Type, ec.ParamTypes);
					ec.ig = mb.GetILGenerator ();

					Emit (ec);

					tb.CreateType ();
					ab.Save (fname);
				}
				
				lambda_delegate = ec.CreateDelegate ();
			}
			return lambda_delegate;
		}
	}
}
