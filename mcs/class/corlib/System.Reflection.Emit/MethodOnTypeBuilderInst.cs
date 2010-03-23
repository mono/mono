//
// System.Reflection.Emit/MethodOnTypeBuilderInst.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Reflection;
using System.Text;

namespace System.Reflection.Emit
{
	/*
	 * This class represents a method of an instantiation of a generic type builder.
	 */
	internal class MethodOnTypeBuilderInst : MethodInfo
	{
		#region Keep in sync with object-internals.h
		Type instantiation;
		MethodInfo base_method; /*This is the base method definition, it must be non-inflated and belong to a non-inflated type.*/
		Type[] method_arguments;
		#endregion
		MethodInfo generic_method_definition;
		int is_compiler_context = -1;

		public MethodOnTypeBuilderInst (MonoGenericClass instantiation, MethodInfo base_method)
		{
			this.instantiation = instantiation;
			this.base_method = base_method;
		}

		internal MethodOnTypeBuilderInst (MethodOnTypeBuilderInst gmd, Type[] typeArguments)
		{
			this.instantiation = gmd.instantiation;
			this.base_method = gmd.base_method;
			this.method_arguments = new Type [typeArguments.Length];
			typeArguments.CopyTo (this.method_arguments, 0);
			this.generic_method_definition = gmd;
		}

		internal MethodOnTypeBuilderInst (MethodInfo method, Type[] typeArguments)
		{
			this.instantiation = method.DeclaringType;
			this.base_method = ExtractBaseMethod (method);
			this.method_arguments = new Type [typeArguments.Length];
			typeArguments.CopyTo (this.method_arguments, 0);
			if (base_method != method)
				this.generic_method_definition = method;
		}

		static MethodInfo ExtractBaseMethod (MethodInfo info)
		{
			if (info is MethodBuilder)
				return info;
			if (info is MethodOnTypeBuilderInst)
				return ((MethodOnTypeBuilderInst)info).base_method;

			if (info.IsGenericMethod)
			  	info = info.GetGenericMethodDefinition ();

			Type t = info.DeclaringType;
			if (!t.IsGenericType || t.IsGenericTypeDefinition)
				return info;

			return (MethodInfo)t.Module.ResolveMethod (info.MetadataToken);
		}

		internal Type[] GetTypeArgs ()
		{
			if (!instantiation.IsGenericType || instantiation.IsGenericParameter)
				return null;				

			return instantiation.GetGenericArguments ();
		}

		internal bool IsCompilerContext {
			get {
				if (is_compiler_context == -1) {
					bool is_cc = false;
					is_cc |= instantiation.IsCompilerContext;
					if (!is_cc && method_arguments != null) {
						foreach (Type t in method_arguments)
							is_cc |= t.IsCompilerContext;
					}
					is_compiler_context = is_cc ? 1 : 0;
				}
				return is_compiler_context == 1;
			}
		}

		//
		// MemberInfo members
		//
		
		public override Type DeclaringType {
			get {
				return instantiation;
			}
		}

		public override string Name {
			get {
				return base_method.Name;
			}
		}

		public override Type ReflectedType {
			get {
				return instantiation;
			}
		}

		public override Type ReturnType {
			get { 
				if (!IsCompilerContext)
					return base_method.ReturnType;
				return MonoGenericClass.InflateType (base_method.ReturnType, GetTypeArgs (), method_arguments);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return base_method.IsDefined (attributeType, inherit);
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return base_method.GetCustomAttributes (inherit);
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return base_method.GetCustomAttributes (attributeType, inherit);
		}

		public override string ToString ()
		{
			 //IEnumerable`1 get_Item(TKey)
			 StringBuilder sb = new StringBuilder (ReturnType.ToString ());
			 sb.Append (" ");
			 sb.Append (base_method.Name);
			 sb.Append ("(");
			 if (IsCompilerContext) {
				 ParameterInfo [] par = GetParameters ();
				 for (int i = 0; i < par.Length; ++i) {
				 	if (i > 0)
				 		sb.Append (", ");
				 	sb.Append (par [i].ParameterType);
				 }
			}
			 sb.Append (")");
			 return sb.ToString ();
		}
		//
		// MethodBase members
		//

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return base_method.GetMethodImplementationFlags ();
		}

		public override ParameterInfo [] GetParameters ()
		{
			ParameterInfo [] res = null;
			if (!IsCompilerContext)
				throw new NotSupportedException ();

			if (base_method is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)base_method;
				res = new ParameterInfo [mb.parameters.Length];
				for (int i = 0; i < mb.parameters.Length; i++) {
					Type type = MonoGenericClass.InflateType (mb.parameters [i], GetTypeArgs (), method_arguments);
					res [i] = new ParameterInfo (mb.pinfo == null ? null : mb.pinfo [i + 1], type, this, i + 1);
				}
			} else {
				ParameterInfo[] base_params = base_method.GetParameters ();
				res = new ParameterInfo [base_params.Length];
				for (int i = 0; i < base_params.Length; i++) {
					Type type = MonoGenericClass.InflateType (base_params [i].ParameterType, GetTypeArgs (), method_arguments);
					res [i] = new ParameterInfo (base_params [i], type, this, i + 1);
				}
			}
			return res;
		}

		public override int MetadataToken {
			get {
				if (!IsCompilerContext)
					return base.MetadataToken;
				return base_method.MetadataToken;
			}
		}

		internal override int GetParameterCount ()
		{
			return base_method.GetParameterCount ();
		}

		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
		{
			throw new NotSupportedException ();
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw new NotSupportedException ();
			}
		}

		public override MethodAttributes Attributes {
			get {
				return base_method.Attributes;
			}
		}

		public override CallingConventions CallingConvention {
			get {
				return base_method.CallingConvention;
			}
		}

		public override MethodInfo MakeGenericMethod (params Type [] methodInstantiation)
		{
			if (!base_method.IsGenericMethodDefinition || (method_arguments != null && !IsCompilerContext))
				throw new InvalidOperationException ("Method is not a generic method definition");

			if (methodInstantiation == null)
				throw new ArgumentNullException ("methodInstantiation");

			if (base_method.GetGenericArguments ().Length != methodInstantiation.Length)
				throw new ArgumentException ("Incorrect length", "methodInstantiation");

			foreach (Type type in methodInstantiation) {
				if (type == null)
					throw new ArgumentNullException ("methodInstantiation");
			}

			return new MethodOnTypeBuilderInst (this, methodInstantiation);
		}

		public override Type [] GetGenericArguments ()
		{
			if (!base_method.IsGenericMethodDefinition)
				return null;
			Type[] source = method_arguments ?? base_method.GetGenericArguments ();
			Type[] result = new Type [source.Length];
			source.CopyTo (result, 0);
			return result;
		}

		public override MethodInfo GetGenericMethodDefinition ()
		{
			return generic_method_definition ?? base_method;
		}

		public override bool ContainsGenericParameters {
			get {
				if (base_method.ContainsGenericParameters)
					return true;
				if (!base_method.IsGenericMethodDefinition)
					throw new NotSupportedException ();
				if (method_arguments == null)
					return true;
				foreach (Type t in method_arguments) {
					if (t.ContainsGenericParameters)
						return true;
				}
				return false;
			}
		}

		public override bool IsGenericMethodDefinition {
			get {
				return base_method.IsGenericMethodDefinition && method_arguments == null;
			}
		}

		public override bool IsGenericMethod {
			get {
				return base_method.IsGenericMethodDefinition;
			}
		}

		//
		// MethodInfo members
		//

		public override MethodInfo GetBaseDefinition ()
		{
			throw new NotSupportedException ();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {
				throw new NotSupportedException ();
			}
		}
	}
}

