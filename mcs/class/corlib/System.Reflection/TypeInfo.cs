//
// TypeInfo.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//    Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2011-2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_5
using System.Collections.Generic;

namespace System.Reflection
{
	public abstract class TypeInfo : Type, IReflectableType
	{
		internal TypeInfo ()
		{
		}
		
		TypeInfo IReflectableType.GetTypeInfo ()
		{
			return this;
		}

		const BindingFlags declaredFlags = BindingFlags.DeclaredOnly |
			BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance;
		
		public virtual IEnumerable<ConstructorInfo> DeclaredConstructors {
			get { return GetConstructors (declaredFlags); }
		}
		
		public virtual IEnumerable<EventInfo> DeclaredEvents {
			get { return GetEvents (declaredFlags); }
		}
		
		public virtual IEnumerable<FieldInfo> DeclaredFields {
			get { return GetFields (declaredFlags); }
		}
		
		public virtual IEnumerable<MethodInfo> DeclaredMethods {
			get { return GetMethods (declaredFlags); }
		}
		
		public virtual IEnumerable<PropertyInfo> DeclaredProperties {
			get { return GetProperties (declaredFlags); }
		}
		
		public virtual IEnumerable<MemberInfo> DeclaredMembers {
			get {
				return GetMembers (declaredFlags);
			}
		}
		
		public virtual IEnumerable<TypeInfo> DeclaredNestedTypes {
			get {
				foreach (var nested in GetNestedTypes (declaredFlags))
					yield return new TypeDelegator (nested);
			}
		}
		
		public virtual Type[] GenericTypeParameters {
			get {
				if (!ContainsGenericParameters)
					return EmptyTypes;

				return GetGenericArguments ();
			}
		}

		public virtual IEnumerable<Type> ImplementedInterfaces {
			get {
				return GetInterfaces ();
			}
		}

		public virtual Type AsType ()
		{
			return this;
		}

		public virtual EventInfo GetDeclaredEvent (string name)
		{
			return GetEvent (name, declaredFlags);
		}

		public virtual FieldInfo GetDeclaredField (string name)
		{
			return GetField (name, declaredFlags);
		}

		public virtual MethodInfo GetDeclaredMethod (string name)
		{
			return GetMethod (name, declaredFlags);
		}

		public virtual IEnumerable<MethodInfo> GetDeclaredMethods (string name)
		{
			foreach (var method in GetMethods (declaredFlags))
				if (method.Name.Equals (name))
					yield return method;
		}

		public virtual TypeInfo GetDeclaredNestedType (string name)
		{
			var nested = GetNestedType (name, declaredFlags);
			if (nested != null)
				return new TypeDelegator (nested);
			else
				return null;
		}

		public virtual PropertyInfo GetDeclaredProperty (string name)
		{
			return GetProperty (name, declaredFlags);
		}

		public virtual bool IsAssignableFrom (TypeInfo typeInfo)
		{
			return IsAssignableFrom (typeInfo.AsType ());
		}
	}
}
#endif
