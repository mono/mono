// 
// System.Web.Services.Description.ServiceCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class ServiceCollection : ServiceDescriptionBaseCollection {
		
		#region Constructors
	
		internal ServiceCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public Service this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (Service) List[index]; 
			}
			set { List [index] = value; }
		}

		public Service this [string name] {
			get { return this [IndexOf ((Service) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Service service) 
		{
			Insert (Count, service);
			return (Count - 1);
		}

		public bool Contains (Service service)
		{
			return List.Contains (service);
		}

		public void CopyTo (Service[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Service))
				throw new InvalidCastException ();

			return ((Service) value).Name;
		}

		public int IndexOf (Service service)
		{
			return List.IndexOf (service);
		}

		public void Insert (int index, Service service)
		{
			List.Insert (index, service);
		}
	
		public void Remove (Service service)
		{
			List.Remove (service);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Service) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
