// 
// System.Web.Services.Description.MimeTextMatchCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace System.Web.Services.Description {
	public sealed class MimeTextMatchCollection : CollectionBase {

		#region Properties

		public MimeTextMatch this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (MimeTextMatch) List [index]; 
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (MimeTextMatch match) 
		{
			Insert (Count, match);
			return (Count - 1);
		}

		public bool Contains (MimeTextMatch match)
		{
			return List.Contains (match);
		}

		public void CopyTo (MimeTextMatch[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (MimeTextMatch match)
		{
			return List.IndexOf (match);
		}

		public void Insert (int index, MimeTextMatch match)
		{
			SetParent (match, this);
			List.Insert (index, match);
		}
	
		public void Remove (MimeTextMatch match)
		{
			List.Remove (match);
		}

		private void SetParent (object value, object parent)
		{
			((MimeTextMatch) value).SetParent ((MimeTextMatchCollection) parent);
		}
			
		#endregion // Methods
	}
}
