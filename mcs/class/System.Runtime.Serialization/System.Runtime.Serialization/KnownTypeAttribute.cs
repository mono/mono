//
// KnownTypeAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct,
		Inherited = true, AllowMultiple = true)]
	public sealed class KnownTypeAttribute : Attribute
	{
		string method_name;
		Type type;

		public KnownTypeAttribute (string methodName)
		{
			if (methodName == null)
				throw new ArgumentNullException ("methodName");
			method_name = methodName;
		}

		public KnownTypeAttribute (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			this.type = type;
		}

		public string MethodName {
			get { return method_name; }
		}

		public Type Type {
			get { return type; }
		}

		MethodInfo method_cache;

		internal IEnumerable<Type> GetTypes (Type type)
		{
			if (method_cache != null)
				return (IEnumerable<Type>) method_cache.Invoke (null, new object [0]);

			if (Type != null)
				return new Type [] {Type};
			else {
				var mi = type.GetMethod (MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
				if (mi == null)
					throw new InvalidDataContractException (String.Format ("KnownTypeAttribute on {0} specifies '{1}' method, but that does not exist. The methos must be static.", type, MethodName));
				if (!typeof (IEnumerable<Type>).IsAssignableFrom (mi.ReturnType))
					throw new InvalidDataContractException (String.Format ("KnownTypeAttribute on {0} specifies '{1}' method, but it returns {2} which cannot be assignable from IEnumerable<Type>.", type, MethodName, mi.ReturnType));
				method_cache = mi;
				return GetTypes (type);
			}
		}
	}
}
#endif
