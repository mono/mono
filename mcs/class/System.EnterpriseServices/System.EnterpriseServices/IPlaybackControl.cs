// 
// System.EnterpriseServices.IPlaybackControl.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Guid("51372AFD-CAE7-11CF-BE81-00AA00A2FA25")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlaybackControl {

		#region Methods

		void FinalClientRetry ();
		void FinalServerRetry ();

		#endregion
	}
#endif
}
