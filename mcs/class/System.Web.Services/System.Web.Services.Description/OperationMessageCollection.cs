// 
// System.Web.Services.Description.OperationMessageCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class OperationMessageCollection : ServiceDescriptionBaseCollection {

		#region Fields

		Operation operation;

		#endregion // Fields

		#region Constructors

		internal OperationMessageCollection (Operation operation)
		{
			this.operation = operation; 
		}

		#endregion // Constructors

		#region Properties

		public OperationFlow Flow {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public OperationInput Input {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
	
		public OperationMessage this [int index] {
			get { return (OperationMessage) List[index]; }
			set { List[index] = value; }
		}

		public OperationOutput Output {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationMessage operationMessage) 
		{
			Insert (Count, operationMessage);
			return (Count - 1);
		}

		public bool Contains (OperationMessage operationMessage)
		{
			return List.Contains (operationMessage);
		}

		public void CopyTo (OperationMessage[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (OperationMessage operationMessage)
		{
			return List.IndexOf (operationMessage);
		}

		public void Insert (int index, OperationMessage operationMessage)
		{
			SetParent (operationMessage, operation);
			List.Insert (index, operationMessage);
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSet (int index, object oldValue, object newValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnValidate (object value)
		{
			throw new NotImplementedException ();
		}
	
		public void Remove (OperationMessage operationMessage)
		{
			List.Remove (operationMessage);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationMessage) value).SetParent ((Operation) parent);
		}
			
		#endregion // Methods
	}
}
