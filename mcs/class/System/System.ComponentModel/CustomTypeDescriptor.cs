//
// System.ComponentModel.CustomTypeDescriptor
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

using System;

namespace System.ComponentModel
{

	public abstract class CustomTypeDescriptor : ICustomTypeDescriptor
	{

		private ICustomTypeDescriptor _parent;
		
		protected CustomTypeDescriptor ()
		{
		}

		
		protected CustomTypeDescriptor (ICustomTypeDescriptor parent)
		{
			_parent = parent;
		}


		public virtual AttributeCollection GetAttributes ()
		{
			if (_parent != null)
				return _parent.GetAttributes ();

			return AttributeCollection.Empty;
		}


		public virtual string GetClassName ()
		{
			if (_parent != null)
				return _parent.GetClassName ();

			return null;
		}


		public virtual string GetComponentName ()
		{
			if (_parent != null)
				return _parent.GetComponentName ();

			return null;
		}


		public virtual TypeConverter GetConverter ()
		{
			if (_parent != null)
				return _parent.GetConverter ();

			return new TypeConverter();
		}


		public virtual EventDescriptor GetDefaultEvent()
		{
			if (_parent != null)
				return _parent.GetDefaultEvent ();
			
			return null;
		}


		public virtual PropertyDescriptor GetDefaultProperty ()
		{
			if (_parent != null)
				return _parent.GetDefaultProperty ();
			
			return null;
		}

		
		public virtual object GetEditor (Type editorBaseType)
		{
			if (_parent != null)
				return _parent.GetEditor (editorBaseType);

			return null;
		}


		public virtual EventDescriptorCollection GetEvents ()
		{
			if (_parent != null)
				return _parent.GetEvents ();
			
			return EventDescriptorCollection.Empty;
		}

 
		public virtual EventDescriptorCollection GetEvents (Attribute[] attributes)
		{
			if (_parent != null)
				return _parent.GetEvents(attributes);
			
			return EventDescriptorCollection.Empty;
		}

 
		public virtual PropertyDescriptorCollection GetProperties ()
		{
			if (_parent != null)
				return _parent.GetProperties ();
			
			return PropertyDescriptorCollection.Empty;
		}

		
		public virtual PropertyDescriptorCollection GetProperties (Attribute[] attributes)
		{
			if (_parent != null)
				return _parent.GetProperties (attributes);
			
			return PropertyDescriptorCollection.Empty;
		}


		public virtual object GetPropertyOwner (PropertyDescriptor pd)
		{
			if (_parent != null)
				return _parent.GetPropertyOwner (pd);
			
			return null;
		}
		
	}
	
}

