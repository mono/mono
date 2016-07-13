//
// CustomReflectionContext.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
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
using System.Collections.Generic;
using System.Reflection;

namespace System.Reflection.Context
{
	public abstract class CustomReflectionContext : ReflectionContext
	{
		[MonoTODO]
		protected CustomReflectionContext ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected CustomReflectionContext (ReflectionContext source)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual IEnumerable<PropertyInfo> AddProperties (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected PropertyInfo CreateProperty (Type propertyType, string name, Func<object, object> getter, Action<object, object> setter)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected PropertyInfo CreateProperty (Type propertyType, string name, Func<object, object> getter, Action<object, object> setter, IEnumerable<Attribute> propertyCustomAttributes, IEnumerable<Attribute> getterCustomAttributes, IEnumerable<Attribute> setterCustomAttributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual IEnumerable<object> GetCustomAttributes (MemberInfo member, IEnumerable<object> declaredAttributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual IEnumerable<object> GetCustomAttributes (ParameterInfo parameter, IEnumerable<object> declaredAttributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Assembly MapAssembly (Assembly assembly)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override TypeInfo MapType (TypeInfo type)
		{
			throw new NotImplementedException ();
		}
	}
}
