//
// System.Security.Permissions.IBuiltInPermission.cs
//
// Author:
//	Sebastien Pouliot <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

namespace System.Security.Permissions {

	// LAMESPEC: Undocumented interface
	internal interface IBuiltInPermission {
		int GetTokenIndex ();
	}
}
