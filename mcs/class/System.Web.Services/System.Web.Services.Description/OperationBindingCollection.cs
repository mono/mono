// 
// System.Web.Services.Description.OperationBindingCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class OperationBindingCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationBindingCollection (Binding binding)
		{
			parent = binding;
		}

		#endregion // Constructors

		#region Properties

		public OperationBinding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (OperationBinding) List[index]; 
			}
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationBinding bindingOperation) 
		{
			Insert (Count, bindingOperation);
			return (Count - 1);
		}

		public bool Contains (OperationBinding bindingOperation)
		{
			return List.Contains (bindingOperation);
		}

		public void CopyTo (OperationBinding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (OperationBinding bindingOperation)
		{
			return List.IndexOf (bindingOperation);
		}

		public void Insert (int index, OperationBinding bindingOperation)
		{
			List.Insert (index, bindingOperation);
		}
	
		public void Remove (OperationBinding bindingOperation)
		{
			List.Remove (bindingOperation);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationBinding) value).SetParent ((Binding) parent);
		}
			
		#endregion // Methods
	}
}
