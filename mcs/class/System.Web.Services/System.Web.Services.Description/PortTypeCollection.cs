// 
// System.Web.Services.Description.PortTypeCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class PortTypeCollection : ServiceDescriptionBaseCollection {

		#region Fields

		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors

		internal PortTypeCollection (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion // Constructors

		#region Properties

		public PortType this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (PortType) List[index]; 
			}
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		public PortType this [string name] {
			get { return this[IndexOf ((PortType) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (PortType portType) 
		{
			Insert (Count, portType);	
			return (Count - 1);
		}

		public bool Contains (PortType portType)
		{
			return List.Contains (portType);
		}

		public void CopyTo (PortType[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is PortType))
				throw new InvalidCastException ();
			return ((PortType) value).Name;
		}

		public int IndexOf (PortType portType)
		{
			return List.IndexOf (portType);
		}

		public void Insert (int index, PortType portType)
		{
			SetParent (portType, serviceDescription);
			Table [GetKey (portType)] = portType;
			List.Insert (index, portType);
		}
	
		public void Remove (PortType portType)
		{
			Table.Remove (GetKey (portType));
			List.Remove (portType);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((PortType) value).SetParent ((ServiceDescription) parent); 
		}
			
		#endregion // Methods
	}
}
