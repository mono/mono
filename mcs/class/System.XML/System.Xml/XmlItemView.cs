//
// XmlItemView.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
#if NET_2_0

using System;
using System.ComponentModel;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlItemView : ICustomTypeDescriptor, IXPathNavigable
	{
		[MonoTODO]
		public XPathNavigator CreateNavigator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual AttributeCollection GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual TypeConverter GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptor GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptor GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection GetEvents (Attribute [] attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection GetProperties ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection GetProperties (Attribute [] attrs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object this [string fieldName] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual XmlItemViewCollection XmlItemViewCollection {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif
