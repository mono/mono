//
// ServiceKnownTypeAttribute.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Reflection;

namespace System.ServiceModel {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)] 
	public sealed class ServiceKnownTypeAttribute : Attribute
	{
		public ServiceKnownTypeAttribute (string methodName)
			: this (methodName, null)
		{
		}

		public ServiceKnownTypeAttribute (Type type)
		{
			this.type = type;
		}

		public ServiceKnownTypeAttribute (string methodName, Type declaringType)
		{
			this.declaring_type = declaringType;
			this.method = methodName;
		}

		string method;
		Type declaring_type, type;
		MethodInfo method_cached;

		public string MethodName {
			get { return method; }
		}

		public Type DeclaringType {
			get { return declaring_type; }
		}

		public Type Type {
			get { return type; }
		}
		
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		static readonly Type [] get_types = new Type [] { typeof (ICustomAttributeProvider) };

		internal IEnumerable<Type> GetTypes (ICustomAttributeProvider provider)
		{
			if (method_cached != null)
				return (IEnumerable<Type>) method_cached.Invoke (null, new object [] {provider});
			if (type != null)
				return new Type [] {type};
			else if (declaring_type == null || method == null)
				return Type.EmptyTypes;

			var mi = declaring_type.GetMethod (method, flags, null, get_types, null);
			if (mi == null)
				throw new InvalidOperationException (String.Format ("ServiceKnownTypeAttribute specifies method {0} in type {1} which does not exist. The method must be static and takes one parameter of type ICustomAttributeProvider", method, declaring_type));
			if (!typeof (IEnumerable<Type>).IsAssignableFrom (mi.ReturnType))
				throw new InvalidOperationException (String.Format ("ServiceKnownTypeAttribute specifies method {0} in type {1} which returns {2} object. This attribute expects the method to have IEnumerable<Type> as its return type", method, declaring_type, mi.ReturnType));
			method_cached = mi;
			return GetTypes (provider); // goto top
		}
	}
}
