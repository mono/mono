// 
// System.EnterpriseServices.SharedPropertyGroup.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[ComVisible (false)]
	public sealed class SharedPropertyGroup {

		#region Fields

		ISharedPropertyGroup propertyGroup;

		#endregion

		#region Constructors

		internal SharedPropertyGroup (ISharedPropertyGroup propertyGroup)
		{
			this.propertyGroup = propertyGroup;
		}

		#endregion // Constructors

		#region Methods

		public SharedProperty CreateProperty (string name, out bool fExists)
		{
			return new SharedProperty (propertyGroup.CreateProperty (name, out fExists));
		}

		public SharedProperty CreatePropertyByPosition (int position, out bool fExists)
		{
			return new SharedProperty (propertyGroup.CreatePropertyByPosition (position, out fExists));
		}

		public SharedProperty Property (string name)
		{
			return new SharedProperty (propertyGroup.Property (name));
		}

		public SharedProperty PropertyByPosition (int position)
		{
			return new SharedProperty (propertyGroup.PropertyByPosition (position));
		}

		#endregion // Methods
	}
}
