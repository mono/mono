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

//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Security.Permissions;

namespace System.Drawing.Design
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class CategoryNameCollection : ReadOnlyCollectionBase
	{
		
		public CategoryNameCollection (CategoryNameCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			InnerList.AddRange (value);
		}

		public CategoryNameCollection (string[] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
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
