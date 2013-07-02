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

#if !FULL_AOT_RUNTIME
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	/*
	 * This class represents a ctor of an instantiation of a generic type builder.
	 */
	[StructLayout (LayoutKind.Sequential)]
	internal class ConstructorOnTypeBuilderInst : ConstructorInfo
	{
		#region Keep in sync with object-internals.h
		MonoGenericClass instantiation;
		ConstructorInfo cb;
		#endregion

		public ConstructorOnTypeBuilderInst (MonoGenericClass instantiation, ConstructorInfo cb)
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
			/*FIXME, maybe the right thing to do when the type is creates is to retrieve from the inflated type*/
			if (!instantiation.IsCreated)
				throw new NotSupportedException ();

			return GetParametersInternal ();
		}

		internal override ParameterInfo[] GetParametersInternal ()
		{
			ParameterInfo [] res;
			if (cb is ConstructorBuilder) {
				ConstructorBuilder cbuilder = (ConstructorBuilder)cb;
				res = new ParameterInfo [cbuilder.parameters.Length];
				for (int i = 0; i < cbuilder.parameters.Length; i++) {
					Type type = instantiation.InflateType (cbuilder.parameters [i]);
					res [i] = new ParameterInfo (cbuilder.pinfo == null ? null : cbuilder.pinfo [i], type, this, i + 1);
				}
			} else {
				ParameterInfo[] parms = cb.GetParameters ();
				res = new ParameterInfo [parms.Length];
				for (int i = 0; i < parms.Length; i++) {
					Type type = instantiation.InflateType (parms [i].ParameterType);
					res [i] = new ParameterInfo (parms [i], type, this, i + 1);
				}
			}
			return res;
		}

		public override int MetadataToken {
			get {
				return base.MetadataToken;
			}
		}

		internal override int GetParametersCount ()
		{
			return cb.GetParametersCount ();
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
