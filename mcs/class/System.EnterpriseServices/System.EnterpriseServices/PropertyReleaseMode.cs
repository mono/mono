// 
// System.EnterpriseServices.PropertyReleaseMode.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Serializable]
	[ComVisible (false)]
	public enum PropertyReleaseMode {
		Process,
		Standard
	}
}
