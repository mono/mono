//
// System.ComponentModel.ComponentCollection.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
//

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
using System.Runtime.InteropServices;
using System.Reflection;

namespace System.ComponentModel {

#if MOONLIGHT
	public class ComponentCollection
	{
	}
#else

	[ComVisible (true)]
	public class ComponentCollection : ReadOnlyCollectionBase
	{
		#region Constructors

		public ComponentCollection (IComponent[] components)
		{
			InnerList.AddRange (components);
		}

		#endregion // Constructors

		#region Properties

		public virtual IComponent this [int index] {
			get { return (IComponent) InnerList[index]; }
		}

		public virtual IComponent this [string name] {
			get { 
				foreach (IComponent C in InnerList) {
					if (C.Site != null)
					if (C.Site.Name == name)
						return C;
				}
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public void CopyTo (IComponent[] array, int index)
		{
			InnerList.CopyTo (array,index);
		}

		#endregion // Methods
		
	}
#endif
}
