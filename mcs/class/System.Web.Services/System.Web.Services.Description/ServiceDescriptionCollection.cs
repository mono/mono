// 
// System.Web.Services.Description.ServiceDescriptionCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class ServiceDescriptionCollection : ServiceDescriptionBaseCollection {
		
		#region Constructors
	
		public ServiceDescriptionCollection () 
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
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		public ServiceDescription this [string ns] {
			get { return this[IndexOf ((ServiceDescription) Table[ns])]; }
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
			foreach (object value in List) 
				foreach (Binding binding in ((ServiceDescription) value).Bindings) 
					if (binding.Name == name.Name)
						return binding;
			throw new Exception ();
		}

		protected override string GetKey (object value) 
		{
			if (!(value is ServiceDescription))
				throw new InvalidCastException ();
			return ((ServiceDescription) value).TargetNamespace;
		}

		public Message GetMessage (XmlQualifiedName name)
		{
			foreach (object value in List) 
				foreach (Message message in ((ServiceDescription) value).Messages) 
					if (message.Name == name.Name)
						return message;
			throw new Exception ();
		}

		public PortType GetPortType (XmlQualifiedName name)
		{
			foreach (object value in List) 
				foreach (PortType portType in ((ServiceDescription) value).PortTypes) 
					if (portType.Name == name.Name)
						return portType;
			throw new Exception ();
		}

		public Service GetService (XmlQualifiedName name)
		{
			foreach (object value in List) 
				foreach (Service service in ((ServiceDescription) value).Services) 
					if (service.Name == name.Name)
						return service;
			throw new Exception ();
		}

		public int IndexOf (ServiceDescription serviceDescription)
		{
			return List.IndexOf (serviceDescription);
		}

		public void Insert (int index, ServiceDescription serviceDescription)
		{
			Table[GetKey (serviceDescription)] = serviceDescription;
			List.Insert (index, serviceDescription);
		}
	
		public void Remove (ServiceDescription serviceDescription)
		{
			Table.Remove (GetKey (serviceDescription));
			List.Remove (serviceDescription);
		}

		//protected override void SetParent (object value, object parent)
		//{
			//((ServiceDescription) value).SetParent ((ServiceDescriptionCollection) parent);
		//}
			
		#endregion // Methods
	}
}
