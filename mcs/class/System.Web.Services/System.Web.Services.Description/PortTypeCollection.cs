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

		#region Constructors

		internal PortTypeCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public PortType this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (PortType) List[index]; 
			}
                        set { List [index] = value; }
		}

		public PortType this [string name] {
			get { return this [IndexOf ((PortType) Table[name])]; }
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
			List.Insert (index, portType);
		}
	
		public void Remove (PortType portType)
		{
			List.Remove (portType);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((PortType) value).SetParent ((ServiceDescription) parent); 
		}
			
		#endregion // Methods
	}
}
