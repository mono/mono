// 
// System.Web.Services.Description.ServiceDescriptionCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class ServiceDescriptionCollection : ServiceDescriptionBaseCollection {

		#region Constructors
	
		public ServiceDescriptionCollection () 
			: base (null)
		{
		}

		#endregion // Constructors

		#region Properties

		public ServiceDescription this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (ServiceDescription) List[index]; 
			}
			set { List [index] = value; }
		}

		public ServiceDescription this [string ns] {
			get { 
				return (ServiceDescription) Table[ns];
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (ServiceDescription serviceDescription) 
		{
			Insert (Count, serviceDescription);
			return (Count - 1);
		}

		public bool Contains (ServiceDescription serviceDescription)
		{
			return List.Contains (serviceDescription);
		}

		public void CopyTo (ServiceDescription[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public Binding GetBinding (XmlQualifiedName name)
		{
			ServiceDescription desc = (ServiceDescription) Table[name.Namespace];
			if (desc != null) {
				foreach (Binding binding in desc.Bindings) 
					if (binding.Name == name.Name)
						return binding;
			}
			throw new InvalidOperationException ("Binding '" + name + "' not found");
		}

		protected override string GetKey (object value) 
		{
			return ((ServiceDescription) value).TargetNamespace;
		}

		public Message GetMessage (XmlQualifiedName name)
		{
			ServiceDescription desc = (ServiceDescription) Table[name.Namespace];
			if (desc != null) {
				foreach (Message message in desc.Messages) 
					if (message.Name == name.Name)
						return message;
			}
			throw new InvalidOperationException ("Message '" + name + "' not found");
		}

		public PortType GetPortType (XmlQualifiedName name)
		{
			ServiceDescription desc = (ServiceDescription) Table[name.Namespace];
			if (desc != null) {
				foreach (PortType portType in desc.PortTypes) 
					if (portType.Name == name.Name)
						return portType;
			}
			throw new InvalidOperationException ("Port type '" + name + "' not found");
		}

		public Service GetService (XmlQualifiedName name)
		{
			ServiceDescription desc = (ServiceDescription) Table[name.Namespace];
			if (desc != null) {
				foreach (Service service in desc.Services) 
					if (service.Name == name.Name)
						return service;
			}
			throw new InvalidOperationException ("Service '" + name + "' not found");
		}

		public int IndexOf (ServiceDescription serviceDescription)
		{
			return List.IndexOf (serviceDescription);
		}

		public void Insert (int index, ServiceDescription serviceDescription)
		{
			List.Insert (index, serviceDescription);
		}
	
		public void Remove (ServiceDescription serviceDescription)
		{
			List.Remove (serviceDescription);
		}

		#endregion // Methods
	}
}
