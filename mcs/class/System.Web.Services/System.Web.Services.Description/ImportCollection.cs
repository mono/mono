// 
// System.Web.Services.Description.ImportCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class ImportCollection : ServiceDescriptionBaseCollection {

		#region Constructors
		
		internal ImportCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion

		#region Properties

		public Import this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Import) List[index]; 
			}
			set { List [index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Import import) 
		{
			Insert (Count, import);
			return (Count - 1);
		}

		public bool Contains (Import import)
		{
			return List.Contains (import);
		}

		public void CopyTo (Import[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (Import import)
		{
			return List.IndexOf (import);
		}

		public void Insert (int index, Import import)
		{
			List.Insert (index, import);
		}
	
		public void Remove (Import import)
		{
			List.Remove (import);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Import) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
