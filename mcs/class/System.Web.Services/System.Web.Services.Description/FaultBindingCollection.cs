// 
// System.Web.Services.Description.FaultBindingCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class FaultBindingCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal FaultBindingCollection (OperationBinding operationBinding) 
			: base (operationBinding)
		{
		}

		#endregion // Constructors

		#region Properties

		public FaultBinding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (FaultBinding) List[index]; 
			}
                        set { List [index] = value; }
		}

		public FaultBinding this [string name] {
			get { return this [IndexOf ((FaultBinding) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (FaultBinding bindingOperationFault) 
		{
			Insert (Count, bindingOperationFault);
			return (Count - 1);
		}

		public bool Contains (FaultBinding bindingOperationFault)
		{
			return List.Contains (bindingOperationFault);
		}

		public void CopyTo (FaultBinding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is FaultBinding))
				throw new InvalidCastException ();

			return ((FaultBinding) value).Name;
		}

		public int IndexOf (FaultBinding bindingOperationFault)
		{
			return List.IndexOf (bindingOperationFault);
		}

		public void Insert (int index, FaultBinding bindingOperationFault)
		{
			List.Insert (index, bindingOperationFault);
		}
	
		public void Remove (FaultBinding bindingOperationFault)
		{
			List.Remove (bindingOperationFault);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((FaultBinding) value).SetParent ((OperationBinding) parent);	
		}
			
		#endregion // Methods
	}
}
