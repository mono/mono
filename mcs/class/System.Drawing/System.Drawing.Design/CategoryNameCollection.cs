//
// System.Drawing.Design.CategoryNameCollection.cs
//
// Authors:
// 	Alejandro Sánchez Acosta
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System.Collections;

namespace System.Drawing.Design
{
	public sealed class CategoryNameCollection : ReadOnlyCollectionBase
	{
		
		public CategoryNameCollection (CategoryNameCollection value) {
			InnerList.AddRange (value);
		}

		public CategoryNameCollection(string[] value) {
			InnerList.AddRange (value);
		}

		public string this[int index] {
			get {
				return (string) InnerList[index];
			}
		}

		public bool Contains (string value)
		{
			return InnerList.Contains (value);
		}
		
		public void CopyTo (string[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}
		
		public int IndexOf (string value)
		{
			return InnerList.IndexOf (value);
		}
	}
}
