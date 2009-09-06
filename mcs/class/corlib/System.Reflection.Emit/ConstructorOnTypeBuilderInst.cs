//
// System.Reflection.Emit/ConstructorOnTypeBuilderInst.cs
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

#if NET_2_0 || BOOTSTRAP_NET_2_0

namespace System.Reflection.Emit
{
	/*
	 * This class represents a ctor of an instantiation of a generic type builder.
	 */
	internal class ConstructorOnTypeBuilderInst : ConstructorInfo
	{
		#region Keep in sync with object-internals.h
		MonoGenericClass instantiation;
		ConstructorBuilder cb;
		#endregion

		public ConstructorOnTypeBuilderInst (MonoGenericClass instantiation, ConstructorBuilder cb)
		{
			this.instantiation = instantiation;
			this.cb = cb;
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
				return cb.Name;
			}
		}

		public override Type ReflectedType {
			get {
				return instantiation;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return cb.IsDefined (attributeType, inherit);
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			return cb.GetCustomAttributes (inherit);
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return cb.GetCustomAttributes (attributeType, inherit);
		}

		//
		// MethodBase members
		//

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return cb.GetMethodImplementationFlags ();
		}

		public override ParameterInfo[] GetParameters ()
		{
			if (!((ModuleBuilder)cb.Module).assemblyb.IsCompilerContext && !instantiation.generic_type.is_created)
				throw new NotSupportedException ();

			ParameterInfo [] res = new ParameterInfo [cb.parameters.Length];
			for (int i = 0; i < cb.parameters.Length; i++) {
				Type type = instantiation.InflateType (cb.parameters [i]);
				res [i] = new ParameterInfo (cb.pinfo == null ? null : cb.pinfo [i], type, this, i + 1);
			}
			return res;
		}

		public override int MetadataToken {
			get {
				if (!((ModuleBuilder)cb.Module).assemblyb.IsCompilerContext)
					return base.MetadataToken;
				return cb.MetadataToken;
			}
		}

		internal override int GetParameterCount ()
		{
			return cb.GetParameterCount ();
		}

		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
		{
			return cb.Invoke (obj, invokeAttr, binder, parameters,
				culture);
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				return cb.MethodHandle;
			}
		}

		public override MethodAttributes Attributes {
			get {
				return cb.Attributes;
			}
		}

		public override CallingConventions CallingConvention {
			get {
				return cb.CallingConvention;
			}
		}

		public override Type [] GetGenericArguments ()
		{
			return cb.GetGenericArguments ();
		}

		public override bool ContainsGenericParameters {
			get {
				return false;
			}
		}

		public override bool IsGenericMethodDefinition {
			get {
				return false;
			}
		}

		public override bool IsGenericMethod {
			get {
				return false;
			}
		}

		//
		// MethodBase members
		//

		public override object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters,
									   CultureInfo culture)
		{
			throw new InvalidOperationException ();
		}
	}
}

#endif
