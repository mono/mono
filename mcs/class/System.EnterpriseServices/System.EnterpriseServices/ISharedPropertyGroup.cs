// 
// System.EnterpriseServices.ISharedPropertyGroup.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	internal interface ISharedPropertyGroup {

		#region Methods 
		
		ISharedProperty CreateProperty (string name, out bool fExists);
		ISharedProperty CreatePropertyByPosition (int position, out bool fExists);
		ISharedProperty Property (string name);
		ISharedProperty PropertyByPosition (int position);

		#endregion // Methods
	}
}
