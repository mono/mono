// 
// System.Web.Services.Description.OperationFaultCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class OperationFaultCollection : ServiceDescriptionBaseCollection {

		#region Fields

		Operation operation;

		#endregion // Fields

		#region Constructors

		internal OperationFaultCollection (Operation operation) 
		{
			this.operation = operation;
		}

		#endregion // Constructors

		#region Properties

		public OperationFault this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (OperationFault) List[index]; 
			}
		}

		public OperationFault this [string name] {
			get { return this[IndexOf ((OperationFault) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationFault operationFaultMessage) 
		{
			Insert (Count, operationFaultMessage);
			return (Count - 1);
		}

		public bool Contains (OperationFault operationFaultMessage)
		{
			return List.Contains (operationFaultMessage);
		}

		public void CopyTo (OperationFault[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value)
		{
			if (!(value is OperationFault))
				throw new InvalidCastException ();

			return ((OperationFault) value).Name;
		}

		public int IndexOf (OperationFault operationFaultMessage)
		{
			return List.IndexOf (operationFaultMessage);
		}

		public void Insert (int index, OperationFault operationFaultMessage)
		{
			SetParent (operationFaultMessage, operation);
			Table [GetKey (operationFaultMessage)] = operationFaultMessage;
			List.Insert (index, operation);
		}
	
		public void Remove (OperationFault operationFaultMessage)
		{
			Table.Remove (GetKey (operationFaultMessage));
			List.Remove (operationFaultMessage);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationFault) value).SetParent ((Operation) parent);
		}
			
		#endregion // Methods
	}
}
