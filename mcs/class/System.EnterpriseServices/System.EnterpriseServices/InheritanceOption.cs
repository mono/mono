// System.EnterpriseServices.InheritanceOption.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Serializable]
	[ComVisible(false)]
	public enum InheritanceOption {

		Inherit,
		Ignore
	}
#endif
}
