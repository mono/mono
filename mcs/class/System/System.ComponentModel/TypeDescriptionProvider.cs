//
// System.ComponentModel.TypeDescriptionProvider
//
// Authors:		
//		Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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

		private sealed class EmptyCustomTypeDescriptor : CustomTypeDescriptor 
		{
		}

		private EmptyCustomTypeDescriptor _emptyCustomTypeDescriptor;
		private TypeDescriptionProvider _parent;
		
		protected TypeDescriptionProvider ()
		{
		}
		
		protected TypeDescriptionProvider (TypeDescriptionProvider parent)
		{
			_parent = parent;
		}

		public virtual object CreateInstance (IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			if (_parent != null)
				return _parent.CreateInstance (provider, objectType, argTypes, args);
			
			return System.Activator.CreateInstance (objectType, args);
		}


		public virtual IDictionary GetCache (object instance)
		{
			if (_parent != null)
				return _parent.GetCache (instance);

			return null;
		}

		
		public virtual ICustomTypeDescriptor GetExtendedTypeDescriptor (object instance)
		{
			if (_parent != null)
				return _parent.GetExtendedTypeDescriptor (instance);

			if (_emptyCustomTypeDescriptor == null)
				_emptyCustomTypeDescriptor = new EmptyCustomTypeDescriptor ();
						
			return _emptyCustomTypeDescriptor;
		}

		
		public virtual string GetFullComponentName (object component)
		{
			if (_parent != null)
				return _parent.GetFullComponentName (component);

			return GetTypeDescriptor (component).GetComponentName ();
		}

		
		public Type GetReflectionType (object instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");

			return GetReflectionType (instance.GetType (), instance);
		}

		
		public Type GetReflectionType (Type objectType)
		{
			return GetReflectionType (objectType, null);
		}


		public virtual Type GetReflectionType (Type objectType, object instance)
		{
			if (_parent != null)
				return _parent.GetReflectionType (objectType, instance);

			return objectType;
		}

		
		public ICustomTypeDescriptor GetTypeDescriptor (object instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");

			return GetTypeDescriptor (instance.GetType (), instance);
		}

		
		public ICustomTypeDescriptor GetTypeDescriptor (Type objectType)
		{
			return GetTypeDescriptor (objectType, null);
		}

		
		public virtual ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			if (_parent != null)
				return _parent.GetTypeDescriptor (objectType, instance);

			if (_emptyCustomTypeDescriptor == null)
				_emptyCustomTypeDescriptor = new EmptyCustomTypeDescriptor ();
			
			return _emptyCustomTypeDescriptor;
		}

	}

}

#endif
