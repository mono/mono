// 
// System.Web.Services.Description.OperationCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class OperationCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationCollection (PortType portType) 
			: base (portType)
		{
		}

		#endregion // Constructors

		#region Properties

		public Operation this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Operation) List[index]; 
			}
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Operation operation) 
		{
			Insert (Count, operation);
			return (Count - 1);
		}

		public bool Contains (Operation operation)
		{
			return List.Contains (operation);
		}

		public void CopyTo (Operation[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (Operation operation)
		{
			return List.IndexOf (operation);
		}

		public void Insert (int index, Operation operation)
		{
			List.Insert (index, operation);
		}
	
		public void Remove (Operation operation)
		{
			List.Remove (operation);
		}

		protected override void SetParent (object value, object parent)
		{
			((Operation) value).SetParent ((PortType) parent);
		}
			
		#endregion // Methods
	}
}
