//
// ast.cs: Base class for the EcmaScript program tree representation.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

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
using System.Collections;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public abstract class AST {

		internal AST parent;
		protected int line_number;

		//
		// Here the actual IL code generation happens.
		//
		internal abstract void Emit (EmitContext ec);

		//
		// Perform type checks and associates expressions
		// with their declarations
		//
		internal abstract bool Resolve (IdentificationTable context);

		private bool InLoop {
			get {
				if (parent == null || parent is ScriptBlock)
					return false;
				else if (parent is DoWhile || parent is While || parent is For || parent is ForIn)
					return true;
				else
					return parent.InLoop;
			}
		}
		
		private bool InSwitch {
			get {
				if (parent == null)
					return false;
				else if (parent is Switch)
					return true;
				else
					return parent.InSwitch;
			}
		}

		private bool InFunction {
			get {
				if (parent == null)
					return false;
				else if (parent is FunctionDeclaration || parent is FunctionExpression)
					return true;
				else
					return parent.InFunction;
			}
		}
		
		private Function GetContainerFunction {
			get {
				if (parent == null)
					return null;
				if (parent is Function)
					return parent as Function;
				return parent.GetContainerFunction;
				
			}
		}
	}

	public abstract class Function : AST 
	{
		bool check_this;
		bool ignore_dynamic_scope;
		bool requires_activation;

		internal string prefix;
		internal FunctionObject func_obj;
		internal JSFunctionAttributeEnum func_type;

		internal DictionaryEntry [] locals;
		internal LocalBuilder local_func;

		protected bool not_void_return = false;
		
		internal bool CheckThis {
			get { return check_this; }
			set { check_this = value; }
		}

		internal bool IgnoreDynamicScope {
			get { return ignore_dynamic_scope; }
			set { ignore_dynamic_scope = value; }
		}

		internal bool RequiresActivation {
			get { return requires_activation; }
			set { requires_activation = value; }
		}

		public  void Init (Block body, FormalParameterList p)
		{
			func_obj.body = body;
			func_obj.parameters = p;
		}

		internal void set_prefix ()
		{
			if (parent != null && parent is Function) {
				Function tmp;
				tmp = (Function) parent;
				if (tmp.prefix != String.Empty)
					prefix = tmp.prefix + "." + tmp.func_obj.name;
				else
					prefix = tmp.func_obj.name;
			} else
				prefix = String.Empty;			
		}

		internal void set_function_type ()
		{
			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				func_type = JSFunctionAttributeEnum.ClassicFunction;
			else if (parent is FunctionDeclaration)
				func_type = JSFunctionAttributeEnum.NestedFunction;
		}

		internal void set_custom_attr (MethodBuilder mb)
		{
			CustomAttributeBuilder attr_builder;
			Type func_attr = typeof (JSFunctionAttribute);
			Type [] func_attr_enum = new Type [] {typeof (JSFunctionAttributeEnum)};
			attr_builder = new CustomAttributeBuilder (func_attr.GetConstructor (func_attr_enum), 
								   new object [] {func_type});
			mb.SetCustomAttribute (attr_builder);			
		}

		internal void NotVoidReturnHappened (object sender, NotVoidReturnEventArgs args)
		{			
			not_void_return = true;
		}

		protected Type HandleReturnType {
			get {
				Type ret_type;
				if (not_void_return)
					ret_type = typeof (object);
				else
					ret_type = typeof (void);
				return ret_type;
			}
		}
	}
}
