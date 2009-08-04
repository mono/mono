//
// System.Reflection.Emit/EventOnTypeBuilderInst.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
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
using System.Collections;
using System.Globalization;
using System.Reflection;

#if NET_2_0 || BOOTSTRAP_NET_2_0

namespace System.Reflection.Emit
{
	/*
	 * This class represents an event of an instantiation of a generic type builder.
	 */
	internal class EventOnTypeBuilderInst : EventInfo
	{
		MonoGenericClass instantiation;
		EventBuilder evt;

		internal EventOnTypeBuilderInst (MonoGenericClass instantiation, EventBuilder evt)
		{
			this.instantiation = instantiation;
			this.evt = evt;
		}

		public override EventAttributes Attributes {
			get { return evt.attrs; }
		}

		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			if (evt.add_method == null || (!nonPublic && !evt.add_method.IsPublic))
				return null;
			return TypeBuilder.GetMethod (instantiation, evt.add_method);
		}

		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			if (evt.raise_method == null || (!nonPublic && !evt.raise_method.IsPublic))
				return null;
			return TypeBuilder.GetMethod (instantiation, evt.raise_method);
		}

		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			if (evt.remove_method == null || (!nonPublic && !evt.remove_method.IsPublic))
				return null;
			return TypeBuilder.GetMethod (instantiation, evt.remove_method);
		}

#if NET_2_0
		public override MethodInfo[] GetOtherMethods (bool nonPublic)
		{
			if (evt.other_methods == null)
				return new MethodInfo [0];

			ArrayList ar = new ArrayList ();
			foreach (MethodInfo method in evt.other_methods) {
				if (nonPublic || method.IsPublic)
					ar.Add (TypeBuilder.GetMethod (instantiation, method));
			}
			MethodInfo[] res = new MethodInfo [ar.Count];
			ar.CopyTo (res, 0);
			return res;
		}
#endif

		public override Type DeclaringType {
			get { return instantiation; }
		}

		public override string Name {
			get { return evt.name; }
		}

		public override Type ReflectedType {
			get { return instantiation; }
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
	}
}

#endif