// 
// System.Web.Services.Description.MimePartCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace System.Web.Services.Description {
	public sealed class MimePartCollection : CollectionBase {

		#region Properties

		public MimePart this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (MimePart) List[index]; 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (MimePart mimePart) 
		{
			Insert (Count, mimePart);	
			return (Count - 1);
		}

		public bool Contains (MimePart mimePart)
		{
			return List.Contains (mimePart);
		}

		public void CopyTo (MimePart[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (MimePart mimePart)
		{
			return List.IndexOf (mimePart);
		}

		public void Insert (int index, MimePart mimePart)
		{
			List.Insert (index, mimePart);
		}
	
		public void Remove (MimePart mimePart)
		{
			List.Remove (mimePart);
		}
			
		#endregion // Methods
	}
}
