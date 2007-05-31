//
// TypeDescriptionProvider.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc. http://www.novell.com
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

#if NET_2_0

using System;
using System.Collections;

namespace System.ComponentModel
{
	public abstract class TypeDescriptionProvider
	{
		protected TypeDescriptionProvider ()
		{
		}

		[MonoTODO]
		protected TypeDescriptionProvider (TypeDescriptionProvider other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Object CreateInstance (IServiceProvider provider, Type objectType, Type [] argTypes, object [] args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IDictionary GetCache (object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ICustomTypeDescriptor GetExtendedTypeDescriptor (object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetFullComponentName (object component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetReflectionType (Type objectType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetReflectionType (object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Type GetReflectionType (Type objectType, object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ICustomTypeDescriptor GetTypeDescriptor (Type objectType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ICustomTypeDescriptor GetTypeDescriptor (object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
