// 
// System.Web.Services.Description.ServiceDescriptionFormatExtensionCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class ServiceDescriptionFormatExtensionCollection : ServiceDescriptionBaseCollection {
		
		#region Constructors
	
		public ServiceDescriptionFormatExtensionCollection (object parent) 
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
			set { List[index] = value; }
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

		public object Find (Type type)
		{
			foreach (object value in List)
				if (value.GetType () == type)
					return value;
			return null;
		}

		public XmlElement Find (string name, string ns)
		{
			XmlElement xmlElement;
			foreach (object value in List) 
				if (value is XmlElement) {
					xmlElement = (value as XmlElement);
					if (xmlElement.Name == name && xmlElement.NamespaceURI == ns)
						return xmlElement;
				}
			return null;
		}

		public object[] FindAll (Type type)
		{
			ArrayList searchResults = new ArrayList ();
			foreach (object value in List)
				if (value.GetType () == type)
					searchResults.Add (value);
			object[] returnValue = new object [searchResults.Count];

			if (searchResults.Count > 0)
				searchResults.CopyTo (returnValue);

			return returnValue;
		}

		public XmlElement[] FindAll (string name, string ns)
		{
			ArrayList searchResults = new ArrayList ();
			XmlElement xmlElement;

			foreach (object value in List)
				if (value is XmlElement) {
					xmlElement = (value as XmlElement);
					if (xmlElement.Name == name && xmlElement.NamespaceURI == ns)
						searchResults.Add (xmlElement);
				}

			XmlElement[] returnValue = new XmlElement [searchResults.Count];

			if (searchResults.Count > 0)
				searchResults.CopyTo (returnValue);

			return returnValue;
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
	
		protected override void OnValidate (object value)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (!(value is XmlElement || value is ServiceDescriptionFormatExtension))
				throw new ArgumentException ();
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
