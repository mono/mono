// 
// System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class ServiceDescriptionFormatExtensionCollection : ServiceDescriptionBaseCollection {
		
		#region Fields

		object parent;

		#endregion // Fields

		#region Constructors
	
		internal ServiceDescriptionFormatExtensionCollection (object parent) 
		{
			this.parent = parent;
		}

		#endregion // Constructors

		#region Properties

		public object this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return List[index]; 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object extension) 
		{
			Insert (Count, extension);
			return (Count - 1);
		}

		public bool Contains (object extension)
		{
			return List.Contains (extension);
		}

		public void CopyTo (object[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		[MonoTODO]
		public object Find (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlElement Find (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object[] FindAll (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlElement[] FindAll (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (object extension)
		{
			return List.IndexOf (extension);
		}

		public void Insert (int index, object extension)
		{
			SetParent (extension, parent);
			List.Insert (index, extension);
		}

		[MonoTODO]
		public bool IsHandled (object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsRequired (object item)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected override void OnValidate (object value)
		{
			throw new NotImplementedException ();
		}
	
		public void Remove (object extension)
		{
			List.Remove (extension);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((ServiceDescriptionFormatExtension) value).SetParent (parent);
		}
			
		#endregion // Methods
	}
}
