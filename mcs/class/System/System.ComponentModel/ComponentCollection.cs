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

using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;

namespace System.ComponentModel {
	[ComVisible (true)]
	public class ComponentCollection : ReadOnlyCollectionBase {

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
}
			
