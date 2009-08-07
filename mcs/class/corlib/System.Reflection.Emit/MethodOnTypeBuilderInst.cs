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

#if NET_2_0 || BOOTSTRAP_NET_2_0

namespace System.Reflection.Emit
{
	/*
	 * This class represents a method of an instantiation of a generic type builder.
	 */
	internal class MethodOnTypeBuilderInst : MethodInfo
	{
		#region Keep in sync with object-internals.h
		MonoGenericClass instantiation;
		internal MethodBuilder mb;
		Type[] method_arguments;
		MethodOnTypeBuilderInst generic_method_definition;
		#endregion

		public MethodOnTypeBuilderInst (MonoGenericClass instantiation, MethodBuilder mb)
		{
			this.instantiation = instantiation;
			this.mb = mb;
		}

		internal MethodOnTypeBuilderInst (MethodOnTypeBuilderInst gmd, Type[] typeArguments)
		{
			this.instantiation = gmd.instantiation;
			this.mb = gmd.mb;
			this.method_arguments = new Type [typeArguments.Length];
			typeArguments.CopyTo (this.method_arguments, 0);
			this.generic_method_definition = gmd;
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
				return mb.Name;
			}
		}

		public override Type ReflectedType {
			get {
				return instantiation;
			}
		}

		public override Type ReturnType {
			get { 
				if (!((ModuleBuilder)mb.Module).assemblyb.IsCompilerContext)
					return mb.ReturnType;
				return instantiation.InflateType (mb.ReturnType, method_arguments);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override string ToString ()
		{
			 //IEnumerable`1 get_Item(TKey)
			 StringBuilder sb = new StringBuilder (ReturnType.ToString ());
			 sb.Append (" ");
			 sb.Append (mb.Name);
			 sb.Append ("(");
			 if (((ModuleBuilder)mb.Module).assemblyb.IsCompilerContext) {
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
			return mb.GetMethodImplementationFlags ();
		}

		public override ParameterInfo [] GetParameters ()
		{
			if (!((ModuleBuilder)mb.Module).assemblyb.IsCompilerContext)
				throw new NotSupportedException ();

			ParameterInfo [] res = new ParameterInfo [mb.parameters.Length];
			for (int i = 0; i < mb.parameters.Length; i++) {
				Type type = instantiation.InflateType (mb.parameters [i], method_arguments);
				res [i] = new ParameterInfo (mb.pinfo == null ? null : mb.pinfo [i + 1], type, this, i + 1);
			}
			return res;
		}

		public override int MetadataToken {
			get {
				if (!((ModuleBuilder)mb.Module).assemblyb.IsCompilerContext)
					return base.MetadataToken;
				return mb.MetadataToken;
			}
		}

		internal override int GetParameterCount ()
		{
			return mb.GetParameterCount ();
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
				return mb.Attributes;
			}
		}

		public override CallingConventions CallingConvention {
			get {
				return mb.CallingConvention;
			}
		}

		public override MethodInfo MakeGenericMethod (params Type [] typeArguments)
		{
			if (mb.generic_params == null || method_arguments != null)
				throw new NotSupportedException (); //FIXME is this the right exception?

			if (typeArguments == null)
				throw new ArgumentNullException ("typeArguments");

			foreach (Type t in typeArguments) {
				if (t == null)
					throw new ArgumentNullException ("typeArguments");
			}

			if (mb.generic_params.Length != typeArguments.Length)
				throw new ArgumentException ("Invalid argument array length");

			return new MethodOnTypeBuilderInst (this, typeArguments);
		}

		public override Type [] GetGenericArguments ()
		{
			if (mb.generic_params == null)
				return null;
			Type[] source = method_arguments ?? mb.generic_params;
			Type[] result = new Type [source.Length];
			source.CopyTo (result, 0);
			return result;
		}

		public override MethodInfo GetGenericMethodDefinition ()
		{
			return (MethodInfo)generic_method_definition ?? mb;
		}

		public override bool ContainsGenericParameters {
			get {
				if (mb.generic_params == null)
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
				return mb.generic_params != null && method_arguments == null;
			}
		}

		public override bool IsGenericMethod {
			get {
				return mb.generic_params != null;
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

#endif
