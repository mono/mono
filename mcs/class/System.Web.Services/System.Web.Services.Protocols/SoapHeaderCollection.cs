// 
// System.Web.Services.Protocols.SoapHeaderCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace System.Web.Services.Protocols {
	public class SoapHeaderCollection : CollectionBase {

		#region Constructors

		public SoapHeaderCollection ()
		{
		}

		#endregion

		#region Properties

		public SoapHeader this [int index] {
			get { return (SoapHeader) List[index]; }
			set { List[index] = value; }
		}

		#endregion // Properties

		#region Methods

		public int Add (SoapHeader header)
		{
			Insert (Count, header);
			return (Count - 1);
		}

		public bool Contains (SoapHeader header)
		{
			return List.Contains (header);
		}

		public void CopyTo (SoapHeader[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (SoapHeader header)
		{
			return List.IndexOf (header);
		}

		public void Insert (int index, SoapHeader header)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ();
			List.Insert (index, header);
		}

		public void Remove (SoapHeader header)
		{
			List.Remove (header);
		}

		#endregion // Methods
	}
}
