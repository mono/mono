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

		public string MethodName {
			get { return method; }
		}

		public Type DeclaringType {
			get { return declaring_type; }
		}

		public Type Type {
			get { return type; }
		}

		public IEnumerable<Type> GetTypes ()
		{
			if (type != null)
				return new Type [] {type};
			else if (declaring_type == null || method == null)
				return Type.EmptyTypes;

			var mi = declaring_type.GetMethod (method);
			if (mi == null || mi.ReturnType != typeof (Type []) || !mi.IsStatic)
				// actuall nonstatic method raises anerror on .NET, but it does not make sense
				// because it ignores other erroneous patterns.
				return Type.EmptyTypes;
			return (Type []) mi.Invoke (null, new object [0]);
		}
	}
}
